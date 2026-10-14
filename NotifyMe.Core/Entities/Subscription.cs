using NotifyMe.Core.Enums;
using System.ComponentModel.DataAnnotations.Schema;

namespace NotifyMe.Core.Entities
{
    public class Subscription : BaseEntity<string>
    {
        [ForeignKey("Customer")]
        public string CustomerID { get; set; } = string.Empty;

        public DateTime StartDate { get; set; }

        public DateTime EndDate { get; set; }

        public PackageType PackageType { get; set; }

        public SubscriptionStatus SubscriptionStatus { get; set; }

        [NotMapped]
        public bool IsActive =>
        SubscriptionStatus == SubscriptionStatus.Active &&
        EndDate >= DateTime.UtcNow;

        public virtual Customer? Customer { get; set; }
    }
}
