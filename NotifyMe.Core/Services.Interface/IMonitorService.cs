using NotifyMe.Core.Models;

namespace NotifyMe.Core.Services.Interface
{
    public interface IMonitorService
    {
        Task<List<MonitorViewModel>> GetAllMonitorsForAdmin();

        Task<List<MonitorViewModel>> GetMonitors();

        Task<EditMonitorViewModel> GetMonitorById(string monitorId);

        Task<MonitorViewModel> GetMonitor(string monitorId);

        Task AddMonitor(AddMonitorViewModel monitor);

        Task UpdateMonitor(EditMonitorViewModel monitor);

        Task UpdateMonitorFromBackground(string monitorId, string? newHash);

        Task DeleteMonitorById(string monitorId);

        Task<string> IsValidForCreateMonitor(string userId);
    }
}
