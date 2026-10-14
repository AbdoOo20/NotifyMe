using Microsoft.AspNetCore.Identity;
using Microsoft.IdentityModel.Tokens;
using NotifyMe.Core;
using NotifyMe.Core.Entities;
using NotifyMe.Core.Models;
using NotifyMe.Core.Repositories.Interface;
using NotifyMe.Core.Services.Interface;
using NotifyMe.Core.Specifications.AlertSpecification;
using NotifyMe.Core.Specifications.CustomerSpecification;
using NotifyMe.Core.Specifications.MonitorSpecification;
using NotifyMe.Core.Specifications.NotificationSpecification;

namespace NotifyMe.Services.CustomerService
{
    public class CustomerService : ICustomerService
    {
        private readonly IUnitOfWork _unitOfWork;

        private readonly ISubscriptionService _subscriptionService;

        private readonly INotificationService _notificationService;

        private readonly UserManager<ApplicationUser> _userManager;

        public CustomerService(IUnitOfWork unitOfWork, ISubscriptionService subscriptionService, INotificationService notificationService, UserManager<ApplicationUser> userManager)
        {
            _unitOfWork = unitOfWork;
            _subscriptionService = subscriptionService;
            _notificationService = notificationService;
            _userManager = userManager;
        }

        public async Task<CustomerStatisticsViewModel> GetCustomerStatisticsAsync()
        {
            var repoCustomer = GetRepository<Customer, string>();

            var repoMonitor = GetRepository<Core.Entities.Monitor, string>();

            var repoSubscription = GetRepository<Subscription, string>();

            var repoNotification = GetRepository<Notification, string>();

            var repoAlert = GetRepository<Alert, string>();

            var userId = _unitOfWork.GetCurrentUserID();

            var monitors = await repoMonitor.GetAllAsync(new MonitorSpecification(userId, true));

            var subscription = await _subscriptionService.GetUserActiveSubscription(userId);

            var notification = await repoNotification.GetCount(new NotificationSpecification(userId));

            var alert = await repoAlert.GetCount(new AlertSpecification(userId, 'x'));

            return new CustomerStatisticsViewModel()
            {
                ActiveMonitorCount = monitors.Where(m => m.IsActive).Count(),
                PackageType = subscription.CurrentPackage,
                InActiveMonitorCount = monitors.Where(m => !m.IsActive).Count(),
                SubscriptionDuration = subscription.Type,
                TotalAlerts = alert,
                TotalNotifications = notification,
                StartDate = subscription.StartDate,
                EndDate = subscription.EndDate,
            };
        }

        public async Task<List<CustomerViewModel>> GetCustomers()
        {
            var repoCustomer = GetRepository<Customer, string>();

            var customers = (await repoCustomer.GetAllAsync(new CustomerSpecification(true))).OrderByDescending(c => c.CreateAt).ToList();

            var users = new List<CustomerViewModel>();

            foreach (var item in customers)
            {
                var subscription = await _subscriptionService.UserHasSubscription(item.Id);

                var newUser = new CustomerViewModel() 
                {
                    Id = item.Id,
                    Email = item.IdentityUser!.Email!,
                    Phone = item.IdentityUser.PhoneNumber!,
                    IsActive = item.IsActive,
                    Plan = subscription.PackageType,
                    IsLogin = item.IdentityUser.IsLogin,
                };

                users.Add(newUser);
            }

            return users;
        }

        public async Task CreateCustomer(string UserId, string Email, string Phone)
        {
            var repoCustomer = GetRepository<Customer, string>();

            var newCustomer = new Customer()
            {
                Id = UserId,
                IsActive = true,
            };

            await repoCustomer.AddAsync(newCustomer);

            bool success = false;

            try
            {
                await _unitOfWork.CompleteAsync();

                success = true;
            }
            catch
            {
                success = false;

                throw new Exception("Error Create Customer");
            }
            finally 
            {
                if (success) 
                {
                    var admin = _userManager.GetUsersInRoleAsync("Admin").Result.FirstOrDefault();

                    await _notificationService.SendNotification(
                        admin!.Id,
                        "New User Registered",
                        $"A new customer ({Email}) has joined. Phone: {Phone}. Account is currently on Free Trial."
                    );
                }
            }
        }

        public async Task<CustomerViewModel> GetCustomer(string CustomerId)
        {
            var repoCustomer = GetRepository<Customer, string>();

            var customer = await repoCustomer.GetAsync(new CustomerSpecification(CustomerId));

            if (customer is null) throw new Exception("Customer Not Found");

            var subscription = await _subscriptionService.UserHasSubscription(CustomerId);

            var GetCustomer = new CustomerViewModel()
            {
                Id = customer.Id,
                Email = customer.IdentityUser!.Email!,
                Phone = customer.IdentityUser.PhoneNumber!,
                Plan = subscription.PackageType,
                IsActive = customer.IsActive,
                NotificationEmail = customer.IdentityUser.NotificationEmail!,
                TelegramChatId = customer.IdentityUser.TelegramChatId!,
                Webhook = customer.IdentityUser.Webhook!,
            };

            return GetCustomer;
        }

        public async Task ActivateCustomer(string CustomerId)
        {
            var repoCustomer = GetRepository<Customer, string>();

            var customer = await repoCustomer.GetAsync(new CustomerSpecification(CustomerId));

            if (customer is null) throw new Exception("Customer Not Found");

            customer.IsActive = !customer.IsActive;

            repoCustomer.UpdateAsync(customer);

            try
            {
                await _unitOfWork.CompleteAsync();
            }
            catch
            {
                throw new Exception("Error Update Customer");
            }           
        }

        public async Task UpdateCustomer(CustomerViewModel model)
        {
            var repoCustomer = GetRepository<Customer, string>();

            var customer = await repoCustomer.GetAsync(new CustomerSpecification(model.Id));

            if (customer is null) throw new Exception("Customer Not Found");

            if (!model.NotificationEmail.IsNullOrEmpty()) 
            {
                customer.IdentityUser!.NotificationEmail = model.NotificationEmail;
            }

            if (!model.TelegramChatId.IsNullOrEmpty())
            {
                customer.IdentityUser!.TelegramChatId = model.TelegramChatId;
            }

            if (!model.Webhook.IsNullOrEmpty())
            {
                customer.IdentityUser!.Webhook = model.Webhook;
            }

            repoCustomer.UpdateAsync(customer);

            try
            {
                await _unitOfWork.CompleteAsync();
            }
            catch
            {
                throw new Exception("Error Update Customer");
            }
        }

        private IGenericRepository<TEntity, TKey> GetRepository<TEntity, TKey>() where TEntity : BaseEntity<TKey>
        {
            var repo = _unitOfWork.Repository<TEntity, TKey>();

            if (repo is null) throw new Exception("Error Create Repository");

            return repo;
        }
    }
}
