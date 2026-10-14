using System.ComponentModel.DataAnnotations;

namespace NotifyMe.Core.Enums
{
    public enum PackageType
    {
        [Display(Name = "Free")]
        Free = 1,

        [Display(Name = "Pro")]
        Pro = 2,

        [Display(Name = "Business")]
        Business = 3
    }
}
