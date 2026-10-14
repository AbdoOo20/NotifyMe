using NotifyMe.Core.Enums;

namespace NotifyMe.Core.Models
{
    public class AlertViewModel
    {
        public string Id { get; set; } = string.Empty;

        public string ChangeSummary { get; set; } = string.Empty;

        public NotificationChannel SentVia { get; set; }

        public bool IsRead { get; set; }

        public string URL { get; set; } = string.Empty;

        public DateTime CreateAt { get; set; } = DateTime.UtcNow;
    }
}
