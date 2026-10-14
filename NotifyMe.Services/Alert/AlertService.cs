using Microsoft.AspNetCore.Http;
using NotifyMe.Core;
using NotifyMe.Core.Entities;
using NotifyMe.Core.Enums;
using NotifyMe.Core.Models;
using NotifyMe.Core.Repositories.Interface;
using NotifyMe.Core.Services.Interface;
using NotifyMe.Core.Specifications.AlertSpecification;
using TimeZoneConverter;

namespace NotifyMe.Services.AlertService
{
    public class AlertService : IAlertService
    {
        private readonly IUnitOfWork _unitOfWork;

        private readonly IHttpContextAccessor _httpContextAccessor;

        private readonly ISubscriptionService _subscriptionService;

        public AlertService(IUnitOfWork unitOfWork, IHttpContextAccessor httpContextAccessor, ISubscriptionService subscriptionService)
        {
            _unitOfWork = unitOfWork;
            _httpContextAccessor = httpContextAccessor;
            _subscriptionService = subscriptionService;
        }

        public async Task<List<AlertViewModel>> GetAlerts()
        {
            var repoAlert = GetRepository();

            string userId = _unitOfWork.GetCurrentUserID();

            var subscription = await _subscriptionService.GetUserActiveSubscription(userId);

            var specAlert = new AlertSpecification(userId);

            var alerts = await repoAlert.GetAll(specAlert);

            DateTime now = DateTime.UtcNow;

            DateTime limitDate;

            switch (subscription.CurrentPackage)
            {
                case PackageType.Pro:
                    limitDate = now.AddDays(-30);
                    break;
                case PackageType.Business:
                    limitDate = now.AddDays(-90);
                    break;
                case PackageType.Free:
                default:
                    limitDate = now.AddDays(-7);
                    break;
            }

            var currentAlerts = alerts.Where(a => a.CreateAt >= limitDate).ToList();

            var oldAlerts = alerts.Where(a => a.CreateAt < limitDate).ToList();

            if (oldAlerts.Any())
            {
                await DeleteAlerts(oldAlerts);
            }

            var userTimeZoneId = _httpContextAccessor.HttpContext?.Request.Cookies["UserTimeZone"] ?? "UTC";

            TimeZoneInfo targetZone = TZConvert.GetTimeZoneInfo(userTimeZoneId);

            var alertModels = currentAlerts.Select(alert => new AlertViewModel()
            {
                Id = alert.Id,
                URL = alert.Monitor!.URL,
                ChangeSummary = alert.ChangeSummary,
                SentVia = alert.SentVia,
                CreateAt = TimeZoneInfo.ConvertTimeFromUtc(alert.CreateAt, targetZone),
                IsRead = alert.IsRead,
            }).ToList();

            return alertModels;
        }

        public async Task DeleteAlerts(List<Alert> alerts)
        {
            var repoAlert = GetRepository();

            alerts.ForEach(alert => repoAlert.DeleteAsync(alert));

            try
            {
                await _unitOfWork.CompleteAsync();
            }
            catch
            {
                throw new Exception("Error Delete Alerts");
            }
        }

        public async Task AddAlert(Alert alert)
        {
            var repoAlert = GetRepository();

            var newAlert = new Alert()
            {
                Id = Guid.NewGuid().ToString(),
                ChangeSummary = alert.ChangeSummary,
                MonitorID = alert.MonitorID,
                NewHash = alert.NewHash,
                OldHash = alert.OldHash,
                SentVia = alert.SentVia,
            };

            await repoAlert.AddAsync(newAlert);

            try
            {
                await _unitOfWork.CompleteAsync();
            }
            catch
            {
                throw new Exception("Error Create Alert");
            }
        }

        public async Task MarkAlertRead(string alertId)
        {
            var repoAlert = GetRepository();

            var specAlert = new AlertSpecification(alertId, true);

            var alert = await repoAlert.GetAsync(specAlert);

            if (alert is null) throw new Exception("Alert Not Found");

            alert.IsRead = true;

            repoAlert.UpdateAsync(alert);

            try
            {
                await _unitOfWork.CompleteAsync();
            }
            catch
            {
                throw new Exception("Error Update Alert");
            }
        }

        public async Task MarkAllAlertsRead()
        {
            var repoAlert = GetRepository();

            string userId = _unitOfWork.GetCurrentUserID();

            var specAlert = new AlertSpecification(userId, string.Empty);

            var alerts = await repoAlert.GetAllAsync(specAlert);

            foreach (var alert in alerts)
            {
                alert.IsRead = true;

                repoAlert.UpdateAsync(alert);
            }

            try
            {
                await _unitOfWork.CompleteAsync();
            }
            catch
            {
                throw new Exception("Error Update Alerts");
            }
        }

        public async Task<int> GetUnReadCount()
        {
            var repoAlert = GetRepository();

            string userId = _unitOfWork.GetCurrentUserID();

            var specAlert = new AlertSpecification(userId, string.Empty);

            var alerts = await repoAlert.GetCount(specAlert);

            return alerts;
        }

        private IGenericRepository<Alert, string> GetRepository()
        {
            var repo = _unitOfWork.Repository<Alert, string>();

            if (repo is null) throw new Exception("Error Create Repository");

            return repo;
        }
    }
}
