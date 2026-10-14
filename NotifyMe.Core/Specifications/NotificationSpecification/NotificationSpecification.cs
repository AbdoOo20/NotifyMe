using NotifyMe.Core.Entities;

namespace NotifyMe.Core.Specifications.NotificationSpecification
{
    public class NotificationSpecification : BaseSpecification<Notification, string>
    {
        public NotificationSpecification(string userId) : base(n => n.ApplicationUserID == userId)
        {
            OrderByDescending = n => n.CreateAt;
        }

        public NotificationSpecification(string userId, bool isUnRead) : base(n => n.ApplicationUserID == userId && n.IsRead == false)
        {
        }

        public NotificationSpecification(string notificationId, char getById) : base(a => a.Id == notificationId)
        {
        }
    }
}
