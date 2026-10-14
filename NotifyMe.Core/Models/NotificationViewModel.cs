namespace NotifyMe.Core.Models
{
    public class NotificationViewModel
    {
        public string Id { get; set; } = string.Empty;

        public string Title { get; set; } = string.Empty;

        public string Body { get; set; } = string.Empty;

        public bool IsRead { get; set; } = false;

        public DateTime CreateAt { get; set; }
    }
}
