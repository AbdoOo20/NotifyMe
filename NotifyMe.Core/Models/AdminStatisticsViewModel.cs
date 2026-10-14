namespace NotifyMe.Core.Models
{
    public class AdminStatisticsViewModel
    {
        public int CustomerCount { get; set; }

        public int ActiveSubs { get; set; }

        public int ExpiredSubs { get; set; }

        public int PendingSubs { get; set; }

        public int CancelledSubs { get; set; }

        public int TotalRevenue { get; set; }

        public int ActiveMonitorCount { get; set; }

        public int InActiveMonitorCount { get; set; }

        public int ActiveFreePackage { get; set; }
    }
}
