using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Mvc;
using NotifyMe.Core.Models;
using NotifyMe.Core.Services.Interface;
using System.Security.Claims;

namespace NotifyMe.App.Controllers
{
    [Authorize("Customer")]
    public class ProfileController : Controller
    {
        private readonly ICustomerService _customerService;

        public ProfileController(ICustomerService customerService)
        {
            _customerService = customerService;
        }

        public async Task<IActionResult> Index()
        {
            var userId = User.FindFirstValue(ClaimTypes.NameIdentifier);

            var customer = await _customerService.GetCustomer(userId!);

            return View(customer);
        }

        public async Task<IActionResult> Edit(CustomerViewModel model)
        {
            try 
            {
                await _customerService.UpdateCustomer(model);

                TempData["SuccessMessage"] = "Profile Updated Successfully";
            }
            catch(Exception e)
            {
                TempData["ErrorMessage"] = e.Message.ToString();
            }

            return RedirectToAction(nameof(Index));
        }
    }
}
