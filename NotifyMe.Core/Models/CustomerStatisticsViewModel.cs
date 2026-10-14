using NotifyMe.Core.Enums;

namespace NotifyMe.Core.Models
{
    public class CustomerStatisticsViewModel
    {
        public int ActiveMonitorCount { get; set; }

        public int InActiveMonitorCount { get; set; }

        public int TotalAlerts { get; set; }

        public int TotalNotifications { get; set; }

        public DateTime StartDate { get; set; }

        public DateTime EndDate { get; set; }

        public PackageType PackageType { get; set; }

        public SubscriptionDuration SubscriptionDuration { get; set; }
    }
}
