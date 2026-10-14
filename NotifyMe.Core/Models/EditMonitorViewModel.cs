using NotifyMe.Core.Enums;
using System.ComponentModel.DataAnnotations;

namespace NotifyMe.Core.Models
{
    public class EditMonitorViewModel
    {
        [Required(ErrorMessage = "ID Required")]
        public string Id { get; set; } = string.Empty;

        [Display(Name = "CSS Selector")]
        public string? CssSelector { get; set; }

        [Required(ErrorMessage = "Interval Minutes Required")]
        [Range(1, 10080, ErrorMessage = "Interval Minutes Between 1 : 10080")]
        [Display(Name = "Check Interval (minutes)")]
        public int CheckIntervalMinutes { get; set; }

        [Required(ErrorMessage = "Notification Channel Required")]
        [Display(Name = "Notification Channel")]
        public NotificationChannel NotificationChannel { get; set; }

        [Display(Name = "Activation")]
        public bool IsActive { get; set; } = true;
    }
}
