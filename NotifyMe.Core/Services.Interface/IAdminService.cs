using NotifyMe.Core.Models;

namespace NotifyMe.Core.Services.Interface
{
    public interface IAdminService
    {
        Task<AdminStatisticsViewModel> GetAdminStatisticsAsync();
    }
}
