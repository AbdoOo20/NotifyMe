using NotifyMe.Core.Enums;

namespace NotifyMe.Core.Models
{
    public class SubscriptionViewModel
    {
        public string Id { get; set; } = string.Empty;

        public string CustomerID { get; set; } = string.Empty;

        public DateTime StartDate { get; set; }

        public DateTime EndDate { get; set; }

        public PackageType PackageType { get; set; }

        public SubscriptionStatus SubscriptionStatus { get; set; }

        public bool IsActive =>
        SubscriptionStatus == SubscriptionStatus.Active &&
        EndDate >= DateTime.UtcNow;

        public string CustomerEmail { get; set; } = string.Empty;

        public string CustomerPhone { get; set; } = string.Empty;
    }
}
