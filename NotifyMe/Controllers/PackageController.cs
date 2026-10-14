using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Mvc;
using NotifyMe.Core.Enums;
using NotifyMe.Core.Services.Interface;
using System.Security.Claims;

namespace NotifyMe.App.Controllers
{
    [Authorize("Customer")]
    public class PackageController : Controller
    {
        private readonly ISubscriptionService _subscriptionService;

        public PackageController(ISubscriptionService subscriptionService)
        {
            _subscriptionService = subscriptionService;
        }

        public async Task<IActionResult> Index()
        {
            var userId = User.FindFirstValue(ClaimTypes.NameIdentifier);

            var model = await _subscriptionService.GetUserActiveSubscription(userId!);

            return View(model);
        }

        [HttpPost]
        [ValidateAntiForgeryToken]
        public async Task<IActionResult> Upgrade(PackageType packageType, SubscriptionDuration duration)
        {
            var userId = User.FindFirstValue(ClaimTypes.NameIdentifier);

            try
            {
                await _subscriptionService.CreateSubscription(userId!, packageType, duration);

                TempData["SuccessMessage"] = "Subscription created successfully, Wait admin Approval";

                return Ok();
            }
            catch (Exception e)
            {
                TempData["ErrorMessage"] = e.Message.ToString();

                return BadRequest();
            }
        }
    }
}
