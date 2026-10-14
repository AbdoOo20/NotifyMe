using NotifyMe.Core.Entities;
using NotifyMe.Core.Enums;

namespace NotifyMe.Core.Specifications.SubscriptionSpecification
{
    public class SubscriptionSpecification : BaseSpecification<Subscription, string>
    {
        public SubscriptionSpecification()
        {
            Includes.Add(s => s.Customer!);
            Includes.Add(s => s.Customer!.IdentityUser!);
            OrderByDescending = s => s.CreateAt;
        }

        public SubscriptionSpecification(string Id, bool getById) : base (s => s.Id == Id)
        {
            Includes.Add(s => s.Customer!.IdentityUser!);
        }

        public SubscriptionSpecification(string userId, DateTime now)
            : base(s => s.CustomerID == userId && s.EndDate >= now && s.SubscriptionStatus == SubscriptionStatus.Active && s.PackageType != PackageType.Free)
        {
        }

        public SubscriptionSpecification(char getFreeSubscribe)
            : base(s => s.SubscriptionStatus != SubscriptionStatus.Cancelled && s.SubscriptionStatus != SubscriptionStatus.Pending && s.PackageType != PackageType.Free)
        {
        }

        public SubscriptionSpecification(DateTime now)
            : base(s => s.EndDate >= now && s.SubscriptionStatus == SubscriptionStatus.Active && s.PackageType != PackageType.Free)
        {
        }

        public SubscriptionSpecification(SubscriptionStatus status): base(s => s.SubscriptionStatus == status)
        {
        }

        public SubscriptionSpecification(PackageType package) : base(s => s.PackageType == package && s.SubscriptionStatus == SubscriptionStatus.Active)
        {
        }

        public SubscriptionSpecification(string userId)
            : base(s => s.CustomerID == userId && s.SubscriptionStatus == SubscriptionStatus.Pending)
        {
            Includes.Add(s => s.Customer!.IdentityUser!);
        }

        public SubscriptionSpecification(string userId, char getFreeSubscribe)
            : base(s => s.CustomerID == userId && s.PackageType == PackageType.Free)
        {
        }

        public SubscriptionSpecification(string userId, DateTime now, bool includeData)
            : base(s => s.CustomerID == userId && s.EndDate >= now && s.SubscriptionStatus == SubscriptionStatus.Active && s.PackageType != PackageType.Free)
        {
            Includes.Add(s => s.Customer!);
            Includes.Add(s => s.Customer!.IdentityUser!);
        }

        // To get all active subscriptions with their due monitors
        public SubscriptionSpecification(DateTime now, PackageType packageType)
            : base(s => s.EndDate >= now && s.SubscriptionStatus == SubscriptionStatus.Active && s.PackageType == packageType)
        {
            int limit = packageType switch
            {
                PackageType.Business => 200,
                PackageType.Pro => 30,
                _ => 3
            };
            Includes.Add(s => s.Customer!.Monitors.Where(m => m.IsActive && (m.LastCheckedAt == null || now >= m.LastCheckedAt.Value.AddMinutes(m.CheckIntervalMinutes))).Take(limit));
            Includes.Add(s => s.Customer!.IdentityUser!);
            OrderByDescending = s => s.PackageType;
        }

        public SubscriptionSpecification(DateTime now, char x)
            : base(s => s.EndDate < now && s.SubscriptionStatus == SubscriptionStatus.Active)
        {
        }
    }
}
