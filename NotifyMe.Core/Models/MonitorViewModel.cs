using NotifyMe.Core.Enums;
using System.ComponentModel.DataAnnotations;

namespace NotifyMe.Core.Models
{
    public class MonitorViewModel
    {
        public string Id { get; set; } = string.Empty;

        [Display(Name = "Website")]
        public string URL { get; set; } = string.Empty;

        public string? CssSelector { get; set; }

        public string LastContentHash { get; set; } = string.Empty;

        [Display(Name = "Interval Minutes")]
        public int CheckIntervalMinutes { get; set; }

        [Display(Name = "Activation")]
        public bool IsActive { get; set; } = true;

        [Display(Name = "Notification")]
        public NotificationChannel NotificationChannel { get; set; }

        public PackageType PackageType { get; set; }

        [Display(Name = "Last Checked")]
        public DateTime? LastCheckedAt { get; set; }

        [Display(Name = "Create At")]
        public DateTime CreateAt { get; set; } = DateTime.UtcNow;

        public string Customer { get; set; } = string.Empty;

        public string Email { get; set; } = string.Empty;

        public string Telegram { get; set; } = string.Empty;

        public string Webhook { get; set; } = string.Empty;

        public int ErrorCount { get; set; } = 0;
    }
}
