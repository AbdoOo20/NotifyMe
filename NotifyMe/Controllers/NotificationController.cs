using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Mvc;
using NotifyMe.Core.Services.Interface;

namespace NotifyMe.App.Controllers
{
    [Authorize(Roles = "Admin,Customer")]
    public class NotificationController : Controller
    {
        private readonly INotificationService _notificationService;

        public NotificationController(INotificationService notificationService)
        {
            _notificationService = notificationService;
        }

        public async Task<IActionResult> Index()
        {
            return View(await _notificationService.GetUserNotifications());
        }

        [HttpPost]
        public async Task<IActionResult> MarkAllAlertRead()
        {
            try
            {
                await _notificationService.MarkAllNotificationsRead();

                TempData["SuccessMessage"] = "All Notifications Marked as Read";
            }
            catch (Exception e)
            {
                TempData["ErrorMessage"] = e.Message.ToString();
            }

            return RedirectToAction(nameof(Index));
        }

        [HttpPost]
        public async Task<IActionResult> MarkAlertRead(string id)
        {
            try
            {
                await _notificationService.MarkNotificationRead(id);

                TempData["SuccessMessage"] = "Notification Marked as Read";
            }
            catch (Exception e)
            {
                TempData["ErrorMessage"] = e.Message.ToString();
            }

            return RedirectToAction(nameof(Index));
        }
    }
}
