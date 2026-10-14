using NotifyMe.Core.Entities;

namespace NotifyMe.Core.Specifications.AlertSpecification
{
    public class AlertSpecification : BaseSpecification<Alert, string>
    {
        public AlertSpecification() : base()
        {
        }

        public AlertSpecification(string AlertId, bool GetById) : base(a => a.Id == AlertId)
        {
        }

        public AlertSpecification(string CustomerId) : base(a => a.Monitor != null && a.Monitor.CustomerID == CustomerId)
        {
            Includes.Add(a => a.Monitor!);
            OrderByDescending = a => a.CreateAt;
        }

        public AlertSpecification(string CustomerId, char getAllAlerts) : base(a => a.Monitor != null && a.Monitor.CustomerID == CustomerId)
        {
            OrderByDescending = a => a.CreateAt;
        }

        public AlertSpecification(string CustomerId, string GetUnRead) : base(a => a.Monitor != null && a.Monitor.CustomerID == CustomerId && a.IsRead == false)
        {
            Includes.Add(a => a.Monitor!);
            OrderByDescending = a => a.CreateAt;
        }
    }
}
