using Microsoft.AspNetCore.Http;
using Microsoft.AspNetCore.SignalR;
using NotifyMe.Core;
using NotifyMe.Core.Entities;
using NotifyMe.Core.Models;
using NotifyMe.Core.Repositories.Interface;
using NotifyMe.Core.Services;
using NotifyMe.Core.Services.Interface;
using NotifyMe.Core.Specifications.NotificationSpecification;
using TimeZoneConverter;

namespace NotifyMe.Services.NotificationService
{
    public class NotificationService : INotificationService
    {
        private readonly IUnitOfWork _unitOfWork;

        private readonly IHubContext<NotificationHub> _notificationHub;

        private readonly IHttpContextAccessor _httpContextAccessor;

        public NotificationService(IUnitOfWork unitOfWork, IHubContext<NotificationHub> notificationHub, IHttpContextAccessor httpContextAccessor)
        {
            _unitOfWork = unitOfWork;
            _notificationHub = notificationHub;
            _httpContextAccessor = httpContextAccessor;
        }
        
        public async Task<int> GetUnReadCount()
        {
            var repoNotification = GetRepository();

            string userId = _unitOfWork.GetCurrentUserID();

            var specNotification = new NotificationSpecification(userId, false);

            var notifications = await repoNotification.GetCount(specNotification);

            return notifications;
        }

        public async Task<List<NotificationViewModel>> GetUserNotifications()
        {
            var repoNotification = GetRepository();

            string userId = _unitOfWork.GetCurrentUserID();

            var specNotification = new NotificationSpecification(userId);

            var notifications = await repoNotification.GetAllAsync(specNotification);

            var userTimeZoneId = _httpContextAccessor.HttpContext?.Request.Cookies["UserTimeZone"] ?? "UTC";

            TimeZoneInfo targetZone = TZConvert.GetTimeZoneInfo(userTimeZoneId);

            var notificationModels = notifications.Select(notification => new NotificationViewModel()
            {
                Id = notification.Id,
                Title = notification.Title,
                Body = notification.Body,
                CreateAt = TimeZoneInfo.ConvertTimeFromUtc(notification.CreateAt, targetZone),
                IsRead = notification.IsRead,
            }).ToList();

            return notificationModels;
        }

        public async Task MarkAllNotificationsRead()
        {
            var repoNotification = GetRepository();

            string userId = _unitOfWork.GetCurrentUserID();

            var specNotification = new NotificationSpecification(userId, false);

            var notifications = await repoNotification.GetAllAsync(specNotification);

            foreach (var notification in notifications)
            {
                notification.IsRead = true;

                repoNotification.UpdateAsync(notification);
            }

            try
            {
                await _unitOfWork.CompleteAsync();
            }
            catch
            {
                throw new Exception("Error Update Notification");
            }
        }

        public async Task MarkNotificationRead(string notificationId)
        {
            var repoNotification = GetRepository();

            var specNotification = new NotificationSpecification(notificationId, 's');

            var notification = await repoNotification.GetAsync(specNotification);

            if (notification is null) throw new Exception("Notification Not Found");

            notification.IsRead = true;

            repoNotification.UpdateAsync(notification);

            try
            {
                await _unitOfWork.CompleteAsync();
            }
            catch
            {
                throw new Exception("Error Update Notification");
            }
        }

        public async Task SendNotification(string userId, string title, string message)
        {
            var repoNotification = GetRepository(); 

            var newNotification = new Notification
            {
                Id = Guid.NewGuid().ToString(),
                ApplicationUserID = userId,
                Title = title,
                Body = message,
                IsRead = false,
            };

            await repoNotification.AddAsync(newNotification);

            try 
            {
                await _notificationHub.Clients.User(userId).SendAsync("ReceiveNotification", title, message);

                await _unitOfWork.CompleteAsync();
            }
            catch (Exception e) 
            {
                throw new Exception("Error Save Notification");
            }
        }

        private IGenericRepository<Notification, string> GetRepository()
        {
            var repo = _unitOfWork.Repository<Notification, string>();

            if (repo is null) throw new Exception("Error Create Repository");

            return repo;
        }
    }
}
