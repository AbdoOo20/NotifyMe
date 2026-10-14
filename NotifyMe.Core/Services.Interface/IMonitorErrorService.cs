using NotifyMe.Core.Models;

namespace NotifyMe.Core.Services.Interface
{
    public interface IMonitorErrorService
    {
        public Task AddMonitorError(string monitorId, string errorType, string error, string? stackTrace);

        public Task<List<MonitorErrorViewModel>> GetMonitorErrors(string monitorId);

        public Task<int> GetMonitorErrorsCount(string monitorId);
    }
}
