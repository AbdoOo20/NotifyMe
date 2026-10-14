using System.ComponentModel.DataAnnotations;

namespace NotifyMe.Core.Enums
{
    public enum NotificationChannel
    {
        [Display(Name = "Email")]
        Email = 1,

        [Display(Name = "Telegram")]
        Telegram = 2,

        [Display(Name = "Webhook")]
        Webhook = 3
    }
}
