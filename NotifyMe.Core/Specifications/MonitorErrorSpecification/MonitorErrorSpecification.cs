using NotifyMe.Core.Entities;

namespace NotifyMe.Core.Specifications.MonitorErrorSpecification
{
    public class MonitorErrorSpecification : BaseSpecification<MonitorErrors, string>
    {
        public MonitorErrorSpecification(string monitorId) : base(me => me.MonitorId == monitorId)
        {
        }
    }
}
