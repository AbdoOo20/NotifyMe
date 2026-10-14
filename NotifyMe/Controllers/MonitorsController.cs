using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Mvc;
using Microsoft.AspNetCore.Mvc.Rendering;
using NotifyMe.Core.Models;
using NotifyMe.Core.Services.Interface;
using System.Security.Claims;

namespace NotifyMe.App.Controllers
{
    public class MonitorsController : Controller
    {
        private readonly IMonitorService _monitorService;

        private readonly ISubscriptionService _subscriptionService;

        private readonly IMonitorErrorService _monitorErrorService;

        public MonitorsController(IMonitorService monitorService, ISubscriptionService subscriptionService, IMonitorErrorService monitorErrorService)
        {
            _monitorService = monitorService;
            _subscriptionService = subscriptionService;
            _monitorErrorService = monitorErrorService;
        }

        private void GetData()
        {
            var userId = User.FindFirstValue(ClaimTypes.NameIdentifier);

            ViewData["NotificationChannel"] = new SelectList(_subscriptionService.GetAvailableNotificationChannel(userId!).Result);

            ViewData["IntervalMinutes"] = new SelectList(_subscriptionService.GetIntervalDependOnUserSubscription(userId!).Result);
        }

        [Authorize("Customer")]
        public async Task<IActionResult> Index()
        {
            return View(await _monitorService.GetMonitors());
        }

        [Authorize("Admin")]
        public async Task<IActionResult> Details(string id)
        {
            var monitor = await _monitorErrorService.GetMonitorErrors(id);

            return View(monitor);
        }

        [Authorize("Admin")]
        public async Task<IActionResult> All()
        {
            return View(await _monitorService.GetAllMonitorsForAdmin());
        }

        [Authorize("Customer")]
        public IActionResult Create()
        {
            GetData();

            return View();
        }

        [HttpPost]
        [ValidateAntiForgeryToken]
        [Authorize("Customer")]
        public async Task<IActionResult> Create(AddMonitorViewModel monitor)
        {
            if (ModelState.IsValid)
            {
                try
                {
                    await _monitorService.AddMonitor(monitor);

                    TempData["SuccessMessage"] = "Monitor Added Successfully";

                    return RedirectToAction(nameof(Index));
                }
                catch (Exception e)
                {
                    TempData["ErrorMessage"] = e.Message.ToString();

                    GetData();

                    return View(monitor);
                }
            }
            GetData();

            return View(monitor);
        }

        [Authorize("Customer")]
        public async Task<IActionResult> Edit(string id)
        {
            if (id == null)
            {
                TempData["ErrorMessage"] = "Monitor Id Required";

                return RedirectToAction(nameof(Index));
            }

            var monitor = await _monitorService.GetMonitorById(id);

            if (monitor == null)
            {
                TempData["ErrorMessage"] = "Monitor Not Found";

                return RedirectToAction(nameof(Index));
            }

            GetData();

            return View(monitor);
        }

        [HttpPost]
        [ValidateAntiForgeryToken]
        [Authorize("Customer")]
        public async Task<IActionResult> Edit(string id, EditMonitorViewModel monitor)
        {
            if (id != monitor.Id)
            {
                TempData["ErrorMessage"] = "Monitor Id Not Match";

                return RedirectToAction(nameof(Index));
            }

            if (ModelState.IsValid)
            {
                try
                {
                    await _monitorService.UpdateMonitor(monitor);

                    TempData["SuccessMessage"] = "Monitor Updated Successfully";
                }
                catch
                {
                    TempData["ErrorMessage"] = "Error Update Monitor";
                }

                return RedirectToAction(nameof(Index));
            }

            GetData();

            return View(monitor);
        }

        [Authorize("Customer")]
        public async Task<IActionResult> Delete(string id)
        {
            if (id == null)
            {
                TempData["ErrorMessage"] = "Monitor Id Required";

                return RedirectToAction(nameof(Index));
            }

            var monitor = await _monitorService.GetMonitor(id);

            if (monitor == null)
            {
                TempData["ErrorMessage"] = "Monitor Not Found";

                return RedirectToAction(nameof(Index));
            }

            return View(monitor);
        }

        [HttpPost, ActionName("Delete")]
        [ValidateAntiForgeryToken]
        [Authorize("Customer")]
        public async Task<IActionResult> DeleteConfirmed(string id)
        {
            try
            {
                await _monitorService.DeleteMonitorById(id);

                TempData["SuccessMessage"] = "Monitor Deleted Successfully";
            }
            catch (Exception e)
            {
                TempData["ErrorMessage"] = "Error Delete Monitor";
            }

            return RedirectToAction(nameof(Index));
        }
    }
}
