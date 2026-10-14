using NotifyMe.Core.Enums;
using System.ComponentModel.DataAnnotations;

namespace NotifyMe.Core.Models
{
    public class AddMonitorViewModel
    {
        [Required(ErrorMessage = "URL Required")]
        [Url(ErrorMessage = "URL Not Valid, Enter Valid URL")]
        [MaxLength(2048)]
        [Display(Name = "Website URL")]
        public string Url { get; set; } = string.Empty;

        [Display(Name = "CSS Selector")]
        public string? CssSelector { get; set; }

        [Required(ErrorMessage = "Interval Minutes Required")]
        [Range(1, 10080, ErrorMessage = "Interval Minutes Between 1 : 10080")]
        [Display(Name = "Check Interval (minutes)")]
        public int CheckIntervalMinutes { get; set; }

        [Required(ErrorMessage = "Notification Channel Required")]
        [Display(Name = "Notification Channel")]
        public NotificationChannel NotificationChannel { get; set; }

        public bool IsActive { get; set; } = true;
    }
}
