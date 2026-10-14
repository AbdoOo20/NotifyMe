using System.ComponentModel.DataAnnotations.Schema;
using System.ComponentModel.DataAnnotations;

namespace NotifyMe.Core.Entities
{
    public class Customer : BaseEntity<string>
    {
        [Key]
        [ForeignKey("IdentityUser")]
        public override string Id { get; set; } = string.Empty;

        public bool IsActive { get; set; }

        public virtual ApplicationUser? IdentityUser { get; set; }

        public virtual ICollection<Monitor> Monitors { get; set; } = [];
        
        public virtual ICollection<Subscription> Subscriptions { get; set; } = [];
    }
}