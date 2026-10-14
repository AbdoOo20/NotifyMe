using Microsoft.AspNetCore.Identity;

namespace NotifyMe.Core.Entities
{
    public class ApplicationUser : IdentityUser
    {
        public string NotificationEmail { get; set; } = string.Empty;

        public string TelegramChatId { get; set; } = string.Empty;

        public string Webhook { get; set; } = string.Empty;

        public bool IsLogin { get; set; } = false;

        public virtual ICollection<Notification> Notifications { get; set; } = [];
    }
}
