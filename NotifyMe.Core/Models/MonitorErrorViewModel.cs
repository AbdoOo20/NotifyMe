namespace NotifyMe.Core.Models
{
    public class MonitorErrorViewModel
    {
        public string Id { get; set; } = string.Empty;

        public DateTime CreateAt { get; set; }

        public string Message { get; set; } = string.Empty;

        public string Type { get; set; } = string.Empty;

        public string StackTrace { get; set; } = string.Empty;
    }
}
