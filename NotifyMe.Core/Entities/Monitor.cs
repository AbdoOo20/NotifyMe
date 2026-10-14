using NotifyMe.Core.Enums;
using System.ComponentModel.DataAnnotations;
using System.ComponentModel.DataAnnotations.Schema;

namespace NotifyMe.Core.Entities
{
    public class Monitor : BaseEntity<string>
    {
        [Required(ErrorMessage = "URL Required")]
        [Url(ErrorMessage = "URL Not Valid, Enter Valid URL")]
        [MaxLength(2048)]
        public string URL { get; set; } = string.Empty;

        [Display(Name = "Css Selector")]
        public string? CssSelector { get; set; }

        public string LastContentHash { get; set; } = string.Empty;

        [Required(ErrorMessage = "Interval Minutes Required")]
        [Range(1, 10080, ErrorMessage = "Interval Minutes Between 1 : 10080")]
        public int CheckIntervalMinutes { get; set; }

        public bool IsActive { get; set; } = true;
        
        public NotificationChannel NotificationChannel { get; set; }

        public DateTime? LastCheckedAt { get; set; }

        [ForeignKey("Customer")]
        public string CustomerID { get; set; } = string.Empty;

        public virtual Customer? Customer { get; set; }

        public virtual ICollection<Alert> Alerts { get; set; } = [];

        public virtual ICollection<MonitorErrors> MonitorErrors { get; set; } = [];
    }
}