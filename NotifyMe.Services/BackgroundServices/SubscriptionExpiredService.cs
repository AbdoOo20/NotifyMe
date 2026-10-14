using Microsoft.Extensions.DependencyInjection;
using Microsoft.Extensions.Hosting;
using Microsoft.Extensions.Logging;
using NotifyMe.Core.Services.Interface;

namespace NotifyMe.Services.BackgroundServices
{
    public class SubscriptionExpiredService : BackgroundService
    {
        private readonly IServiceScopeFactory _scopeFactory;

        private readonly ILogger<SubscriptionExpiredService> _logger;

        public SubscriptionExpiredService(IServiceScopeFactory scopeFactory, ILogger<SubscriptionExpiredService> logger)
        {
            _scopeFactory = scopeFactory;
            _logger = logger;
        }

        protected override async Task ExecuteAsync(CancellationToken stoppingToken)
        {
            while (!stoppingToken.IsCancellationRequested)
            {
                try
                {
                    await CheckSubscriptionExpiredAsync(stoppingToken);
                }
                catch (Exception ex)
                {
                    _logger.LogError(ex, "Error while checking Expired Subscription");
                }

                await Task.Delay(TimeSpan.FromDays(1), stoppingToken);
            }
        }

        private async Task CheckSubscriptionExpiredAsync(CancellationToken stoppingToken) 
        {
            using var scope = _scopeFactory.CreateScope();

            var subscriptionService = scope.ServiceProvider.GetRequiredService<ISubscriptionService>();

            await subscriptionService.ExpiredSubscriptions();
        }
    }
}
