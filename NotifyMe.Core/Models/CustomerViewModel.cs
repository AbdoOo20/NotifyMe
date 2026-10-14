using NotifyMe.Core.Enums;
using System.ComponentModel.DataAnnotations;

namespace NotifyMe.Core.Models
{
    public class CustomerViewModel
    {
        public string Id { get; set; } = string.Empty;

        public string Email { get; set; } = string.Empty;

        public PackageType Plan { get; set; }

        public string Phone { get; set; } = string.Empty;

        public bool IsActive { get; set; }

        public bool IsLogin { get; set; }

        [EmailAddress(ErrorMessage = "Invalid Email Address")]
        public string NotificationEmail { get; set; } = string.Empty;

        [RegularExpression(@"^-?\d+$", ErrorMessage = "Telegram Chat ID must be a numeric value")]
        public string TelegramChatId { get; set; } = string.Empty;

        [Url(ErrorMessage = "Invalid Webhook URL. Must start with http:// or https://")]
        public string Webhook { get; set; } = string.Empty;
    }
}