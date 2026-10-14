using System.ComponentModel.DataAnnotations;

namespace NotifyMe.Core.Enums
{
    public enum SubscriptionDuration
    {
        [Display(Name = "Month")]
        Month = 1,

        [Display(Name = "Year")]
        Year = 2,
    }
}
