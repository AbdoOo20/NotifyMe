using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Mvc;
using NotifyMe.Core.Services.Interface;

namespace NotifyMe.App.Controllers
{
    [Authorize("Admin")]
    public class CustomersController : Controller
    {
        private readonly ICustomerService _customerService;

        public CustomersController(ICustomerService customerService)
        {
            _customerService = customerService;
        }

        public async Task<IActionResult> Index()
        {
            return View(await _customerService.GetCustomers());
        }

        public async Task<ActionResult> Activate(string id)
        {
            try
            {
                await _customerService.ActivateCustomer(id);

                return RedirectToAction(nameof(Index));
            }
            catch (Exception e) 
            {
                TempData["ErrorMessage"] = e.Message.ToString();

                return RedirectToAction(nameof(Index));
            }
        }
    }
}
