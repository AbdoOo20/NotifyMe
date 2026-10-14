using NotifyMe.Core.Enums;

namespace NotifyMe.Core.Models
{
    public class ShowSubscriptionViewModel
    {
        public SubscriptionDuration Type { get; set; }

        public PackageType CurrentPackage { get; set; }

        public DateTime StartDate { get; set; }

        public DateTime EndDate { get; set; }
    }
}
