using Microsoft.AspNetCore.Mvc;

namespace NotifyMe.App.Controllers
{
    public class HomeController : Controller
    {
        public IActionResult Index()
        {
            return View();
        }
    }
}
