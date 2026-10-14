using Microsoft.Extensions.DependencyInjection;
using Microsoft.Extensions.Hosting;
using Microsoft.Extensions.Logging;
using NotifyMe.Core.Entities;
using NotifyMe.Core.Models;
using NotifyMe.Core.Services.Interface;
using System.Security.Cryptography;
using System.Text;
using NotifyMe.Core.Enums;
using Microsoft.IdentityModel.Tokens;
using System.Text.RegularExpressions;
using Fizzler.Systems.HtmlAgilityPack;
using Microsoft.Playwright;
using System.Collections.Concurrent;

namespace NotifyMe.Services.BackgroundServices
{
    public class MonitorCheckerService : BackgroundService
    {
        private readonly IServiceScopeFactory _scopeFactory;

        private readonly ILogger<MonitorCheckerService> _logger;

        private readonly List<int> _freeIntervalList = new List<int> { 45, 60 };

        private readonly List<int> _proIntervalList = new List<int> { 10, 15, 20, 25, 30 };

        private readonly List<int> _businessIntervalList = new List<int> { 1, 2, 3, 4, 5, 6, 7, 8, 9, 10 };

        private static readonly SemaphoreSlim _globalBrowserSemaphore = new(20);

        private static readonly ConcurrentDictionary<string, SemaphoreSlim> _userSemaphores = new();

        private static readonly ConcurrentDictionary<string, IBrowserContext> _userContexts = new();

        public MonitorCheckerService(IServiceScopeFactory scopeFactory, ILogger<MonitorCheckerService> logger)
        {
            _scopeFactory = scopeFactory;
            _logger = logger;
        }

        protected override async Task ExecuteAsync(CancellationToken stoppingToken)
        {
            Microsoft.Playwright.Program.Main(new[] { "install", "chromium" });

            using var playwright = await Playwright.CreateAsync();

            await using var browser = await playwright.Chromium.LaunchAsync(new BrowserTypeLaunchOptions
            {
                Headless = true,
                Args = new[] { "--disable-gpu", "--no-sandbox", "--disable-dev-shm-usage" }
            });

            while (!stoppingToken.IsCancellationRequested)
            {
                try
                {
                    await CheckMonitorsAsync(stoppingToken, browser);
                }
                catch (Exception ex)
                {
                    _logger.LogError(ex, "Error while checking monitors");
                }

                await Task.Delay(TimeSpan.FromMinutes(1), stoppingToken);
            }
        }

        private async Task CheckMonitorsAsync(CancellationToken stoppingToken, IBrowser browser)
        {
            using var scopeAttribute = _scopeFactory.CreateScope();

            var subscriptionService = scopeAttribute.ServiceProvider.GetRequiredService<ISubscriptionService>();

            var monitors = await subscriptionService.GetActivateSubscriptions();

            var monitorsToProcess = monitors.Where(m => IsMonitorAllowedByPlan(m)).ToList();

            await Parallel.ForEachAsync(monitorsToProcess, new ParallelOptions
            {
                MaxDegreeOfParallelism = 50,
                CancellationToken = stoppingToken
            }, async (monitor, ct) =>
            {
                using var scope = _scopeFactory.CreateScope();

                var monitorService = scope.ServiceProvider.GetRequiredService<IMonitorService>();

                var alertService = scope.ServiceProvider.GetRequiredService<IAlertService>();

                var sendAlertService = scope.ServiceProvider.GetRequiredService<ISendAlertToThirdParty>();

                var monitorErrorService = scope.ServiceProvider.GetRequiredService<IMonitorErrorService>();

                await ProcessMonitorWithLockAsync(monitor, monitorService, alertService, sendAlertService, ct, browser, monitorErrorService);
            });
        }

        private async Task ProcessMonitorWithLockAsync(
            MonitorViewModel monitor,
            IMonitorService monitorService,
            IAlertService alertService,
            ISendAlertToThirdParty sendAlertService,
            CancellationToken stoppingToken,
            IBrowser browser,
            IMonitorErrorService monitorErrorService)
        {
            var userLock = _userSemaphores.GetOrAdd(monitor.Customer, _ => new SemaphoreSlim(3));

            await _globalBrowserSemaphore.WaitAsync(stoppingToken);

            await userLock.WaitAsync(stoppingToken);

            try
            {
                await ProcessMonitorAsync(
                    monitor,
                    monitorService,
                    alertService,
                    sendAlertService,
                    stoppingToken,
                    browser,
                    monitorErrorService);
            }
            finally
            {
                userLock.Release();
                _globalBrowserSemaphore.Release();
            }
        }

        private bool IsMonitorAllowedByPlan(MonitorViewModel m)
        {
            if (m.PackageType == PackageType.Free) return _freeIntervalList.Contains(m.CheckIntervalMinutes) && m.NotificationChannel == NotificationChannel.Email;
            if (m.PackageType == PackageType.Pro) return _proIntervalList.Contains(m.CheckIntervalMinutes) && (m.NotificationChannel == NotificationChannel.Email || m.NotificationChannel == NotificationChannel.Telegram);
            if (m.PackageType == PackageType.Business) return _businessIntervalList.Contains(m.CheckIntervalMinutes);
            return false;
        }

        private async Task<IBrowserContext> GetOrCreateUserContextAsync(
            IBrowser browser,
            string customerId)
        {
            if (_userContexts.TryGetValue(customerId, out var existing))
                return existing;

            var context = await browser.NewContextAsync(new BrowserNewContextOptions
            {
                UserAgent = "Mozilla/5.0 (Windows NT 10.0; Win64; x64) AppleWebKit/537.36 (KHTML, like Gecko) Chrome/120.0.0.0 Safari/537.36"
            });

            _userContexts[customerId] = context;

            return context;
        }

        private async Task ProcessMonitorAsync(
                    MonitorViewModel monitor,
                    IMonitorService monitorService,
                    IAlertService alertService,
                    ISendAlertToThirdParty sendAlertService,
                    CancellationToken stoppingToken,
                    IBrowser browser,
                    IMonitorErrorService monitorErrorService)
        {
            var context = await GetOrCreateUserContextAsync(browser, monitor.Customer);

            try
            {
                var page = await context.NewPageAsync();

                await page.RouteAsync("**/*.{png,jpg,jpeg,gif,svg,css,pdf,woff,woff2,ttf,otf,ico,mp4,webm,mp3,wav}",
                    route => route.AbortAsync()); // to prevent load image and any files

                await page.RouteAsync("**/*", async route =>
                {
                    var type = route.Request.ResourceType;
                    if (type == "image" || type == "stylesheet" || type == "font" || type == "media")
                    {
                        await route.AbortAsync();
                    }
                    else
                    {
                        await route.ContinueAsync();
                    }
                });

                var response = await page.GotoAsync(monitor.URL, new()
                {
                    WaitUntil = WaitUntilState.NetworkIdle,
                    Timeout = 60000
                });

                if (response == null || !response.Ok)
                {
                    _logger.LogWarning($"Failed to load {monitor.URL}");
                    return;
                }

                if (!string.IsNullOrEmpty(monitor.CssSelector))
                {
                    try
                    {
                        await page.WaitForSelectorAsync(monitor.CssSelector, new PageWaitForSelectorOptions { Timeout = 15000 });
                    }
                    catch
                    {
                        _logger.LogWarning($"Selector {monitor.CssSelector} not found on {monitor.URL}");
                    }
                }

                var html = await page.ContentAsync();

                await page.CloseAsync();

                var extractedContent = ExtractContent(html, monitor.CssSelector);

                var normalized = Normalize(extractedContent);

                if (string.IsNullOrWhiteSpace(normalized) || normalized == "__EMPTY__")
                {
                    _logger.LogWarning($"Skipping hash update for {monitor.Id}: No content extracted.");
                    return;
                }

                var newHash = ComputeHash(normalized);

                _logger.LogInformation($"New Hash computed: {newHash}");

                if (monitor.LastContentHash != newHash)
                {
                    if (!string.IsNullOrEmpty(monitor.LastContentHash))
                    {
                        await SendAlertToThirdParty(sendAlertService, monitor, alertService, newHash);
                    }
                    await monitorService.UpdateMonitorFromBackground(monitor.Id, newHash);
                    _logger.LogInformation($"Compare hashes | Old: {monitor.LastContentHash} | New: {newHash}");
                }
            }
            catch (Exception ex)
            {
                await HandleMonitorException(ex, monitor.Id, monitorErrorService);
            }
        }

        private async Task HandleMonitorException(
            Exception ex,
            string monitorId,
            IMonitorErrorService monitorErrorService)
        {
            var errorType = ResolveErrorType(ex);

            var errorTypeString = ErrorTypeToString(errorType);

            _logger.LogWarning(ex, $"Monitor {monitorId} failed | Type: {errorType}");

            await monitorErrorService.AddMonitorError(monitorId, errorTypeString, ex.Message, ex.StackTrace);
        }

        private static string ErrorTypeToString(MonitorErrorType type)
        {
            return type switch
            {
                MonitorErrorType.Timeout => "Timeout",
                MonitorErrorType.Network => "NetworkError",
                MonitorErrorType.Dns => "DnsError",
                MonitorErrorType.Ssl => "SslError",
                MonitorErrorType.HttpError => "HttpError",
                MonitorErrorType.SelectorNotFound => "SelectorNotFound",
                MonitorErrorType.EmptyContent => "EmptyContent",
                MonitorErrorType.Playwright => "PlaywrightError",
                MonitorErrorType.Cancelled => "RequestCancelled",
                MonitorErrorType.RateLimited => "RateLimited",
                MonitorErrorType.BusinessRule => "BusinessRule",
                _ => "Unknown"
            };
        }

        private static MonitorErrorType ResolveErrorType(Exception ex)
        {
            var msg = ex.Message.ToLower();

            if (ex is TaskCanceledException)
                return MonitorErrorType.Cancelled;

            if (ex is TimeoutException)
                return MonitorErrorType.Timeout;

            if (ex is PlaywrightException)
            {
                if (msg.Contains("timeout"))
                    return MonitorErrorType.Timeout;

                if (msg.Contains("target closed") || msg.Contains("browser"))
                    return MonitorErrorType.Playwright;
            }

            if (msg.Contains("net::err_name_not_resolved"))
                return MonitorErrorType.Dns;

            if (msg.Contains("net::err_connection_refused"))
                return MonitorErrorType.Network;

            if (msg.Contains("ssl") || msg.Contains("certificate"))
                return MonitorErrorType.Ssl;

            if (msg.Contains("selector"))
                return MonitorErrorType.SelectorNotFound;

            if (msg.Contains("empty"))
                return MonitorErrorType.EmptyContent;

            if (msg.Contains("429"))
                return MonitorErrorType.RateLimited;

            if (msg.Contains("401") || msg.Contains("403") || msg.Contains("404"))
                return MonitorErrorType.HttpError;

            return MonitorErrorType.Unknown;
        }

        private string ComputeHash(string content)
        {
            using var sha256 = SHA256.Create();
            var bytes = Encoding.UTF8.GetBytes(content);
            var hash = sha256.ComputeHash(bytes);
            return Convert.ToBase64String(hash);
        }

        private string Normalize(string content)
        {
            return Regex.Replace(content, @"\s+", " ").Trim();
        }

        private string ExtractContent(string html, string? cssSelector)
        {
            if (string.IsNullOrWhiteSpace(cssSelector)) return html;

            var doc = new HtmlAgilityPack.HtmlDocument();

            doc.LoadHtml(html);

            var nodes = doc.DocumentNode.QuerySelectorAll(cssSelector);

            if (!nodes.Any())
            {
                _logger.LogWarning($"Selector returned empty for {cssSelector}");

                return "__EMPTY__";
            }

            var sb = new StringBuilder();

            foreach (var node in nodes) sb.AppendLine(node.InnerText);

            return sb.ToString();
        }

        private async Task SendAlertToThirdParty(ISendAlertToThirdParty sendAlertService, MonitorViewModel monitor, IAlertService alertService, string newHash)
        {
            try
            {
                switch (monitor.NotificationChannel)
                {
                    case NotificationChannel.Email:
                        if (!monitor.Email.IsNullOrEmpty())
                        {
                            await sendAlertService.SendEmailAsync(monitor.Email, "Monitor Alert", monitor.URL);

                            await AddAlertToTable(alertService, monitor, newHash);
                        }
                        break;
                    case NotificationChannel.Telegram:
                        if (!monitor.Telegram.IsNullOrEmpty() && (monitor.PackageType == PackageType.Pro || monitor.PackageType == PackageType.Business))
                        {
                            await sendAlertService.SendTelegramAsync(monitor.Telegram, $"The monitor at {monitor.URL} has changed.");

                            await AddAlertToTable(alertService, monitor, newHash);
                        }
                        break;
                    case NotificationChannel.Webhook:
                        if (!monitor.Webhook.IsNullOrEmpty() && monitor.PackageType == PackageType.Business)
                        {
                            await sendAlertService.SendWebhookAsync(monitor.Webhook, new { url = monitor.URL, message = "The monitor has changed." });

                            await AddAlertToTable(alertService, monitor, newHash);
                        }
                        break;
                    default:
                        break;
                }
            }
            catch (Exception e)
            {
                _logger.LogWarning(e, $"Failed to send alert for monitor {monitor.URL}");
            }
        }

        private async Task AddAlertToTable(IAlertService alertService, MonitorViewModel monitor, string newHash)
        {
            await alertService.AddAlert(new Alert
            {
                MonitorID = monitor.Id,
                OldHash = monitor.LastContentHash,
                NewHash = newHash,
                ChangeSummary = "Content changed",
                SentVia = monitor.NotificationChannel,
            });
        }
    }
}
