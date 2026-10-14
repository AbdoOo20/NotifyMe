using System.ComponentModel.DataAnnotations.Schema;

namespace NotifyMe.Core.Entities
{
    public class Notification : BaseEntity<string>
    {
        public string Title { get; set; } = string.Empty;

        public string Body { get; set; } = string.Empty;

        public bool IsRead { get; set; } = false;

        [ForeignKey("ApplicationUser")]
        public string ApplicationUserID { get; set; } = string.Empty;

        public virtual ApplicationUser? ApplicationUser { get; set; }
    }
}
