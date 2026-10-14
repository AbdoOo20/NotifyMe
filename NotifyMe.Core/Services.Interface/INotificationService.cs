using NotifyMe.Core.Models;

namespace NotifyMe.Core.Services.Interface
{
    public interface INotificationService
    {
        public Task SendNotification(string userId, string title, string message);

        public Task<List<NotificationViewModel>> GetUserNotifications();

        Task MarkNotificationRead(string notificationId);

        Task<int> GetUnReadCount();

        Task MarkAllNotificationsRead();
    }
}
