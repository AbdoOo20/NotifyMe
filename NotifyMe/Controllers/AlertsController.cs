using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Mvc;
using NotifyMe.Core.Services.Interface;

namespace NotifyMe.App.Controllers
{
    [Authorize("Customer")]
    public class AlertsController : Controller
    {
        private readonly IAlertService _alertService;

        public AlertsController(IAlertService alertService)
        {
            _alertService = alertService;
        }

        public async Task<IActionResult> Index()
        {
            return View(await _alertService.GetAlerts());
        }

        [HttpPost]
        public async Task<IActionResult> MarkAllAlertRead()
        {
            try
            {
                await _alertService.MarkAllAlertsRead();

                TempData["SuccessMessage"] = "All Alerts Marked as Read";
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
                await _alertService.MarkAlertRead(id);

                TempData["SuccessMessage"] = "Alert Marked as Read";
            }
            catch (Exception e)
            {
                TempData["ErrorMessage"] = e.Message.ToString();
            }

            return RedirectToAction(nameof(Index));
        }
    }
}