namespace NotifyMe.Core.Specifications.MonitorSpecification
{
    public class MonitorSpecification : BaseSpecification<Entities.Monitor, string>
    {
        public MonitorSpecification() : base()
        {
            Includes.Add(m => m.Customer!.IdentityUser!);
            Includes.Add(m => m.MonitorErrors);
        }

        public MonitorSpecification(bool isActive) : base(m => m.IsActive == isActive)
        {
        } 

        public MonitorSpecification(string Id) : base(m  => m.Id == Id)
        {
        }

        public MonitorSpecification(string CustomerId, bool getByUser) : base(m => m.CustomerID == CustomerId)
        {
            OrderByDescending = m => m.CreateAt;
        }

        public MonitorSpecification(DateTime now) : base(m => m.IsActive && (m.LastCheckedAt == null || now >= m.LastCheckedAt.Value.AddMinutes(m.CheckIntervalMinutes)))
        {
            Includes.Add(m => m.Customer!);
            Includes.Add(m => m.Customer!.IdentityUser!);
            OrderByDescending = m => m.CreateAt;
        }
    }
}
