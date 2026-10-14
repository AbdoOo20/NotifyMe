using NotifyMe.Core.Entities;
using NotifyMe.Core.Models;

namespace NotifyMe.Core.Services.Interface
{
    public interface IAlertService
    {
        Task<List<AlertViewModel>> GetAlerts();

        Task AddAlert(Alert alert);

        Task MarkAlertRead(string alertId);

        Task<int> GetUnReadCount();

        Task MarkAllAlertsRead();

        Task DeleteAlerts(List<Alert> alerts);
    }
}
