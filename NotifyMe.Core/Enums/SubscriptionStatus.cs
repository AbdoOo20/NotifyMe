using System.ComponentModel.DataAnnotations;

namespace NotifyMe.Core.Enums
{
    public enum SubscriptionStatus
    {
        [Display(Name = "Active")]
        Active = 1,

        [Display(Name = "Expired")]
        Expired = 2,

        [Display(Name = "Cancelled")]
        Cancelled = 3,

        [Display(Name = "Pending")]
        Pending = 4
    }
}