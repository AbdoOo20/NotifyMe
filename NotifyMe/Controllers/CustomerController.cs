using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Mvc;
using NotifyMe.Core.Services.Interface;

namespace NotifyMe.App.Controllers
{
    public class CustomerController : Controller
    {
        private readonly ICustomerService _customerService;

        private readonly IConfiguration _configuration;

        public CustomerController(IConfiguration configuration, ICustomerService customerService)
        {
            _configuration = configuration;
            _customerService = customerService;
        }

        [Authorize("Customer")]
        public async Task<IActionResult> Index()
        {
            var model = await _customerService.GetCustomerStatisticsAsync();

            return View(model);
        }

        public IActionResult Support()
        {
            string phoneNumber = _configuration["SupportPhoneNumber"]!;

            string whatsappUrl = $"https://wa.me/{phoneNumber}";

            return Redirect(whatsappUrl);
        }

        public IActionResult Email()
        {
            string email = "mailto:" + _configuration["SupportEmail"]!;

            return Redirect(email);
        }
    }
}
