using Microsoft.AspNetCore.Http;
using NotifyMe.Core;
using NotifyMe.Core.Enums;
using NotifyMe.Core.Models;
using NotifyMe.Core.Repositories.Interface;
using NotifyMe.Core.Services.Interface;
using NotifyMe.Core.Specifications.MonitorSpecification;
using TimeZoneConverter;
using Monitor = NotifyMe.Core.Entities.Monitor;

namespace NotifyMe.Services.MonitorService
{
    public class MonitorService : IMonitorService
    {
        private readonly IUnitOfWork _unitOfWork;

        private readonly IHttpContextAccessor _httpContextAccessor;

        private readonly ISubscriptionService _subscriptionService;

        private readonly IMonitorErrorService _monitorErrorService;

        public MonitorService(IUnitOfWork unitOfWork, IHttpContextAccessor httpContextAccessor, ISubscriptionService subscriptionService, IMonitorErrorService monitorErrorService)
        {
            _unitOfWork = unitOfWork;
            _httpContextAccessor = httpContextAccessor;
            _subscriptionService = subscriptionService;
            _monitorErrorService = monitorErrorService;
        }

        public async Task<List<MonitorViewModel>> GetAllMonitorsForAdmin()
        {
            var repoMonitor = GetRepository();

            var specMonitor = new MonitorSpecification();

            var monitors = await repoMonitor.GetAllAsync(specMonitor);

            var userTimeZoneId = _httpContextAccessor.HttpContext?.Request.Cookies["UserTimeZone"] ?? "UTC";

            TimeZoneInfo targetZone = TZConvert.GetTimeZoneInfo(userTimeZoneId);

            var monitorModels = monitors.Select(monitor => new MonitorViewModel()
            {
                Id = monitor.Id,
                CheckIntervalMinutes = monitor.CheckIntervalMinutes,
                NotificationChannel = monitor.NotificationChannel,
                IsActive = monitor.IsActive,
                CssSelector = monitor.CssSelector,
                URL = monitor.URL,
                LastCheckedAt = monitor.LastCheckedAt.HasValue
                ? TimeZoneInfo.ConvertTimeFromUtc(monitor.LastCheckedAt.Value, targetZone)
                : null,
                LastContentHash = monitor.LastContentHash,
                CreateAt = TimeZoneInfo.ConvertTimeFromUtc(monitor.CreateAt, targetZone),
                Customer = monitor.Customer!.IdentityUser!.UserName!,
                ErrorCount = _monitorErrorService.GetMonitorErrorsCount(monitor.Id).Result
            }).ToList();

            return monitorModels;
        }

        public async Task<List<MonitorViewModel>> GetMonitors()
        {
            var repoMonitor = GetRepository();

            string userId = _unitOfWork.GetCurrentUserID();

            var specMonitor = new MonitorSpecification(userId, true);

            var monitors = await repoMonitor.GetAllAsync(specMonitor);

            var userTimeZoneId = _httpContextAccessor.HttpContext?.Request.Cookies["UserTimeZone"] ?? "UTC";

            TimeZoneInfo targetZone = TZConvert.GetTimeZoneInfo(userTimeZoneId);

            var monitorModels = monitors.Select(monitor => new MonitorViewModel()
            {
                Id = monitor.Id,
                CheckIntervalMinutes = monitor.CheckIntervalMinutes,
                NotificationChannel = monitor.NotificationChannel,
                IsActive = monitor.IsActive,
                CssSelector = monitor.CssSelector,
                URL = monitor.URL,
                LastCheckedAt = monitor.LastCheckedAt.HasValue
                ? TimeZoneInfo.ConvertTimeFromUtc(monitor.LastCheckedAt.Value, targetZone)
                : null,
                LastContentHash = monitor.LastContentHash,
                CreateAt = TimeZoneInfo.ConvertTimeFromUtc(monitor.CreateAt, targetZone),
            }).ToList();

            return monitorModels;
        }

        public async Task<MonitorViewModel> GetMonitor(string monitorId)
        {
            var repoMonitor = GetRepository();

            var specMonitor = new MonitorSpecification(monitorId);

            var monitor = await repoMonitor.GetAsync(specMonitor);

            if (monitor is null) throw new Exception("Monitor Not Found");

            var monitorModel = new MonitorViewModel()
            {
                Id = monitor.Id,
                CheckIntervalMinutes = monitor.CheckIntervalMinutes,
                NotificationChannel = monitor.NotificationChannel,
                IsActive = monitor.IsActive,
                CssSelector = monitor.CssSelector,
                URL = monitor.URL,
                LastCheckedAt = monitor.LastCheckedAt,
                LastContentHash = monitor.LastContentHash,
            };

            return monitorModel;
        }

        public async Task<EditMonitorViewModel> GetMonitorById(string monitorId)
        {
            var repoMonitor = GetRepository();

            var specMonitor = new MonitorSpecification(monitorId);

            var monitor = await repoMonitor.GetAsync(specMonitor);

            if (monitor is null) throw new Exception("Monitor Not Found");

            var monitorModel = new EditMonitorViewModel()
            {
                Id = monitor.Id,
                CheckIntervalMinutes = monitor.CheckIntervalMinutes,
                NotificationChannel = monitor.NotificationChannel,
                IsActive = monitor.IsActive,
                CssSelector = monitor.CssSelector,
            };

            return monitorModel;
        }

        public async Task AddMonitor(AddMonitorViewModel monitor)
        {
            var repoMonitor = GetRepository();

            string userId = _unitOfWork.GetCurrentUserID();

            string message = await IsValidForCreateMonitor(userId);

            if (message != "Success") throw new Exception(message);

            var newMonitor = new Monitor()
            {
                Id = Guid.NewGuid().ToString(),
                URL = monitor.Url,
                CheckIntervalMinutes = monitor.CheckIntervalMinutes,
                NotificationChannel = monitor.NotificationChannel,
                IsActive = monitor.IsActive,
                CustomerID = userId,
                CssSelector = monitor.CssSelector ?? "",
            };

            await repoMonitor.AddAsync(newMonitor);

            try
            {
                await _unitOfWork.CompleteAsync();
            }
            catch
            {
                throw new Exception("Error Create Monitor");
            }
        }

        public async Task<string> IsValidForCreateMonitor(string userId)
        {
            var repoMonitor = GetRepository();

            var specMonitor = new MonitorSpecification(userId, true);

            var monitorsCount = await repoMonitor.GetCount(specMonitor);

            var subscription = await _subscriptionService.GetUserActiveSubscription(userId);

            return subscription.CurrentPackage switch
            {
                PackageType.Free when monitorsCount >= 3 =>
                    "🚫 Monitor limit reached, " +
                    "You’ve reached the maximum of 3 monitors allowed on the Free plan, " +
                    "Upgrade to Pro to monitor up to 30 websites with faster checks and advanced alerts.",

                PackageType.Pro when monitorsCount >= 30 =>
                    "🚫 Monitor limit reached, " +
                    "Your Pro plan allows up to 30 monitors, and you’ve reached this limit, " +
                    "Upgrade to Business to monitor up to 200 websites with priority processing.",

                PackageType.Business when monitorsCount >= 200 =>
                    "🚫 Monitor limit reached, " +
                    "You’ve reached the maximum of 200 monitors for the Business plan, " +
                    "Please contact support if you need a custom or enterprise solution.",

                _ => "Success"
            };
        }

        public async Task DeleteMonitorById(string monitorId)
        {
            var repoMonitor = GetRepository();

            var specMonitor = new MonitorSpecification(monitorId);

            var monitor = await repoMonitor.GetAsync(specMonitor);

            if (monitor is null) throw new Exception("Monitor Not Found");

            repoMonitor.DeleteAsync(monitor);

            try
            {
                await _unitOfWork.CompleteAsync();
            }
            catch
            {
                throw new Exception("Error Delete Monitor");
            }
        }

        public async Task UpdateMonitor(EditMonitorViewModel monitor)
        {
            var repoMonitor = GetRepository();

            var specMonitor = new MonitorSpecification(monitor.Id);

            var monitorDB = await repoMonitor.GetAsync(specMonitor);

            if (monitorDB is null) throw new Exception("Monitor Not Found");

            monitorDB.NotificationChannel = monitor.NotificationChannel;

            monitorDB.CheckIntervalMinutes = monitor.CheckIntervalMinutes;

            monitorDB.IsActive = monitor.IsActive;

            if (monitorDB.CssSelector != monitor.CssSelector) 
            {
                monitorDB.CssSelector = monitor.CssSelector ?? "";

                monitorDB.LastContentHash = "";

                monitorDB.LastCheckedAt = null;
            }

            repoMonitor.UpdateAsync(monitorDB);

            try
            {
                await _unitOfWork.CompleteAsync();
            }
            catch
            {
                throw new Exception("Error Update Monitor");
            }
        }

        public async Task UpdateMonitorFromBackground(string monitorId, string? newHash)
        {
            var repoMonitor = GetRepository();

            var specMonitor = new MonitorSpecification(monitorId);

            var monitorDB = await repoMonitor.GetAsync(specMonitor);

            if (monitorDB is null) throw new Exception("Monitor Not Found");

            monitorDB.LastContentHash = newHash ?? "";

            monitorDB.LastCheckedAt = DateTime.UtcNow;

            repoMonitor.UpdateAsync(monitorDB);

            try
            {
                await _unitOfWork.CompleteAsync();
            }
            catch
            {
                throw new Exception("Error Update Monitor");
            }
        }

        private IGenericRepository<Monitor, string> GetRepository()
        {
            var repo = _unitOfWork.Repository<Monitor, string>();

            if (repo is null) throw new Exception("Error Create Repository");

            return repo;
        }
    }
}
