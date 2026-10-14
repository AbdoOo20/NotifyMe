using NotifyMe.Core.Entities;
using NotifyMe.Core.Enums;
using NotifyMe.Core.Models;

namespace NotifyMe.Core.Services.Interface
{
    public interface ISubscriptionService
    {
        public Task<List<SubscriptionViewModel>> GetAllSubscriptions();

        public Task<ShowSubscriptionViewModel> GetUserActiveSubscription(string userId);

        public Task<SubscriptionViewModel> UserHasSubscription(string userId);

        public Task CreateSubscription(string userId, PackageType package, SubscriptionDuration duration);

        public Task UpdateSubscriptionStatus(string subscriptionId, SubscriptionStatus status);

        public Task<List<int>> GetIntervalDependOnUserSubscription(string userId);

        public Task SetUserSubscriptionFree(string userId);

        Task<List<MonitorViewModel>> GetActivateSubscriptions();

        Task ExpiredSubscriptions();

        Task<List<NotificationChannel>> GetAvailableNotificationChannel(string userId);
    }
}
