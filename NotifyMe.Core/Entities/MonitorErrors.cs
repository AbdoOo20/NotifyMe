using System.ComponentModel.DataAnnotations.Schema;

namespace NotifyMe.Core.Entities
{
    public class MonitorErrors : BaseEntity<string>
    {
        [ForeignKey("Monitor")]
        public string MonitorId { get; set; } = string.Empty;

        public string ErrorType { get; set; } = string.Empty;

        public string ErrorMessage { get; set; } = string.Empty;

        public string? StackTrace { get; set; }

        public bool IsResolved { get; set; } = false;

        public virtual Monitor? Monitor { get; set; }
    }
}
