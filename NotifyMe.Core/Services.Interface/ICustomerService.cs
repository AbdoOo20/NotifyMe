using NotifyMe.Core.Models;

namespace NotifyMe.Core.Services.Interface
{
    public interface ICustomerService
    {
        Task<List<CustomerViewModel>> GetCustomers();

        Task CreateCustomer(string UserId, string Email, string Phone);

        Task<CustomerViewModel> GetCustomer(string CustomerId);

        Task ActivateCustomer(string CustomerId);

        Task UpdateCustomer(CustomerViewModel model);

        Task<CustomerStatisticsViewModel> GetCustomerStatisticsAsync();
    }
}
