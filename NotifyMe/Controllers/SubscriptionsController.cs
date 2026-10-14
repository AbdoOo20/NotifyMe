using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Mvc;
using NotifyMe.Core.Enums;
using NotifyMe.Core.Services.Interface;

namespace NotifyMe.App.Controllers
{
    [Authorize("Admin")]
    public class SubscriptionsController : Controller
    {
        private readonly ISubscriptionService _subscriptionService;

        public SubscriptionsController(ISubscriptionService subscriptionService)
        {
            _subscriptionService = subscriptionService;
        }

        public async Task<IActionResult> Index()
        {
            return View(await _subscriptionService.GetAllSubscriptions());
        }

        [HttpPost]
        [ValidateAntiForgeryToken]
        public async Task<IActionResult> Approve(string id)
        {
            try
            {
                await _subscriptionService.UpdateSubscriptionStatus(id, SubscriptionStatus.Active);

                TempData["SuccessMessage"] = "Subscription has been accepted";
            }
            catch (Exception ex)
            {
                TempData["ErrorMessage"] = ex.Message.ToString();
            }

            return RedirectToAction(nameof(Index));
        }

        [HttpPost]
        [ValidateAntiForgeryToken]
        public async Task<IActionResult> Reject(string id)
        {
            try
            {
                await _subscriptionService.UpdateSubscriptionStatus(id, SubscriptionStatus.Cancelled);

                TempData["SuccessMessage"] = "Subscription has been rejected";
            }
            catch (Exception ex)
            {
                TempData["ErrorMessage"] = ex.Message.ToString();
            }

            return RedirectToAction(nameof(Index));
        }
    }
}
