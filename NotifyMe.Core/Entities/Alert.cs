using NotifyMe.Core.Enums;
using System.ComponentModel.DataAnnotations.Schema;

namespace NotifyMe.Core.Entities
{
    public class Alert : BaseEntity<string>
    {
        public string OldHash { get; set; } = string.Empty;

        public string NewHash { get; set; } = string.Empty;

        public string ChangeSummary { get; set; } = string.Empty;

        public NotificationChannel SentVia { get; set; }

        public bool IsRead { get; set; } = false;

        [ForeignKey("Monitor")]
        public string MonitorID { get; set; } = string.Empty;

        public virtual Monitor? Monitor { get; set; }
    }
}