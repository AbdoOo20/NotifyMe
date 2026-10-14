using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Mvc;
using NotifyMe.Core.Services.Interface;

namespace NotifyMe.App.Controllers
{
    [Authorize(Roles = "Admin,Customer")]
    [Route("api/[controller]")]
    [ApiController]
    public class NotificationAPIController : ControllerBase
    {
        private readonly INotificationService _notificationService;

        private readonly IAlertService _alertService;

        public NotificationAPIController(INotificationService notificationService, IAlertService alertService)
        {
            _notificationService = notificationService;
            _alertService = alertService;
        }

        [HttpGet("counts")]
        public async Task<IActionResult> GetCounts()
        {
            var notifications = await _notificationService.GetUnReadCount();

            var alerts = await _alertService.GetUnReadCount();

            return Ok(new { notifications, alerts });
        }
    }
}