using Microsoft.AspNetCore.Identity;
using NotifyMe.Core;
using NotifyMe.Core.Entities;
using NotifyMe.Core.Enums;
using NotifyMe.Core.Models;
using NotifyMe.Core.Repositories.Interface;
using NotifyMe.Core.Services.Interface;
using NotifyMe.Core.Specifications.CustomerSpecification;
using NotifyMe.Core.Specifications.SubscriptionSpecification;

namespace NotifyMe.Services.SubscriptionService
{
    public class SubscriptionService : ISubscriptionService
    {
        private readonly IUnitOfWork _unitOfWork;

        private readonly INotificationService _notificationService;

        private readonly UserManager<ApplicationUser> _userManager;

        public SubscriptionService(IUnitOfWork unitOfWork, INotificationService notificationService, UserManager<ApplicationUser> userManager)
        {
            _unitOfWork = unitOfWork;
            _notificationService = notificationService;
            _userManager = userManager;
        }

        public async Task CreateSubscription(string userId, PackageType package, SubscriptionDuration duration)
        {
            var repoSubscription = GetRepository<Subscription, string>();

            DateTime startDate = DateTime.UtcNow;

            var subscription = await repoSubscription.GetAsync(new SubscriptionSpecification(userId));

            if (subscription is not null)
            {
                throw new Exception("User already has a pending subscription, Wait admin approval");
            }

            var repoUser = GetRepository<Customer, string>();

            var user = await repoUser.GetAsync(new CustomerSpecification(userId));

            if (user is null) throw new Exception("User not found");

            if (package != PackageType.Free)
            {
                var endDate = duration == SubscriptionDuration.Month ? startDate.AddMonths(1) : startDate.AddYears(1);

                var newSubscription = new Subscription()
                {
                    Id = Guid.NewGuid().ToString(),
                    PackageType = package,
                    CustomerID = userId,
                    StartDate = startDate,
                    EndDate = endDate,
                    SubscriptionStatus = SubscriptionStatus.Pending,
                };

                await repoSubscription.AddAsync(newSubscription);
            }
            else 
            {
                var freeSubscription = await repoSubscription.GetAsync(new SubscriptionSpecification(userId, 'c'));

                if (freeSubscription is null)
                {
                    throw new Exception("User has not a Free subscription");
                }

                freeSubscription.SubscriptionStatus = SubscriptionStatus.Pending;

                freeSubscription.CreateAt = DateTime.UtcNow;

                repoSubscription.UpdateAsync(freeSubscription);
            }

            var admin = _userManager.GetUsersInRoleAsync("Admin").Result.FirstOrDefault();

            try
            {
                if (admin != null) 
                {
                    await _notificationService.SendNotification(
                        admin.Id,
                        $"New Subscription Activated",
                        $"Customer ({user.IdentityUser!.Email}) has successfully started a new subscription with plan ({package})."
                    );
                }

                await _unitOfWork.CompleteAsync();
            }
            catch
            {
                throw new Exception("Error Create Subscription");
            }
        }

        public async Task<List<SubscriptionViewModel>> GetAllSubscriptions()
        {
            var repoSubscription = GetRepository<Subscription, string>();

            var subscription = await repoSubscription.GetAllAsync(new SubscriptionSpecification());

            var subscriptionViewModel = new List<SubscriptionViewModel>();

            foreach (var sub in subscription)
            {
                subscriptionViewModel.Add(new SubscriptionViewModel()
                {
                    Id = sub.Id,
                    CustomerEmail = sub.Customer!.IdentityUser!.Email!,
                    CustomerPhone = sub.Customer!.IdentityUser!.PhoneNumber!,
                    CustomerID = sub.CustomerID,
                    EndDate = sub.EndDate,
                    StartDate = sub.StartDate,
                    PackageType = sub.PackageType,
                    SubscriptionStatus = sub.SubscriptionStatus,
                });
            }

            return subscriptionViewModel;
        }

        public async Task<List<int>> GetIntervalDependOnUserSubscription(string userId)
        {
            var repoSubscription = GetRepository<Subscription, string>();

            DateTime now = DateTime.UtcNow;

            var subscription = await repoSubscription.GetAsync(new SubscriptionSpecification(userId, now));

            var IntervalList = new List<int>();

            if (subscription is not null)
            {
                if (subscription.PackageType == PackageType.Pro)
                {
                    IntervalList = new List<int> { 10, 15, 20, 25, 30 };
                }
                else if (subscription.PackageType == PackageType.Business)
                {
                    IntervalList = new List<int>() { 1, 2, 3, 4, 5, 6, 7, 8, 9, 10 };
                }
            }
            else 
            {
                IntervalList = new List<int>() { 45, 60 };
            }

            return IntervalList;
        }

        public async Task<List<NotificationChannel>> GetAvailableNotificationChannel(string userId)
        {
            var repoSubscription = GetRepository<Subscription, string>();

            DateTime now = DateTime.UtcNow;

            var subscription = await repoSubscription.GetAsync(new SubscriptionSpecification(userId, now));

            var NotificationChannelList = new List<NotificationChannel>();

            if (subscription is not null)
            {
                if (subscription.PackageType == PackageType.Pro)
                {
                    NotificationChannelList = new List<NotificationChannel> { NotificationChannel.Email, NotificationChannel.Telegram };
                }
                else if (subscription.PackageType == PackageType.Business)
                {
                    NotificationChannelList = new List<NotificationChannel>() { NotificationChannel.Email, NotificationChannel.Telegram, NotificationChannel.Webhook };
                }
            }
            else
            {
                NotificationChannelList = new List<NotificationChannel>() { NotificationChannel.Email };
            }

            return NotificationChannelList;
        }

        public async Task<ShowSubscriptionViewModel> GetUserActiveSubscription(string userId)
        {
            var repoSubscription = GetRepository<Subscription, string>();

            DateTime now = DateTime.UtcNow;

            var subscription = await repoSubscription.GetAsync(new SubscriptionSpecification(userId, now, true));

            var subscriptionViewModel = new ShowSubscriptionViewModel();

            if (subscription is null)
            {
                subscriptionViewModel = new ShowSubscriptionViewModel()
                {
                    Type = SubscriptionDuration.Year,
                    CurrentPackage = PackageType.Free,
                    StartDate = DateTime.UtcNow,
                    EndDate = DateTime.UtcNow,
                };
            }
            else
            {
                subscription.StartDate = subscription.StartDate.ToUniversalTime();

                subscription.EndDate = subscription.EndDate.ToUniversalTime();

                var duration = subscription.EndDate - subscription.StartDate;

                double totalDays = duration.TotalDays;

                subscriptionViewModel = new ShowSubscriptionViewModel()
                {
                    Type = totalDays >= 365 ? SubscriptionDuration.Year : SubscriptionDuration.Month,
                    CurrentPackage = subscription.PackageType,
                    StartDate = subscription.StartDate,
                    EndDate = subscription.EndDate,
                };
            }

            return subscriptionViewModel;
        }

        public async Task SetUserSubscriptionFree(string userId)
        {
            var repoSubscription = GetRepository<Subscription, string>();

            DateTime startDate = DateTime.UtcNow;

            var endDate = startDate.AddYears(30);

            var newSubscription = new Subscription()
            {
                Id = Guid.NewGuid().ToString(),
                PackageType = PackageType.Free,
                CustomerID = userId,
                StartDate = startDate,
                EndDate = endDate,
                SubscriptionStatus = SubscriptionStatus.Active,
            };

            await repoSubscription.AddAsync(newSubscription);

            try
            {
                await _unitOfWork.CompleteAsync();
            }
            catch
            {
                throw new Exception("Error Create Subscription");
            }
        }

        public async Task<List<MonitorViewModel>> GetActivateSubscriptions()
        {
            var repoSubscription = GetRepository<Subscription, string>();

            var now = DateTime.UtcNow;

            var allSubscriptions = new List<Subscription>();

            var specSubscriptionBusiness = new SubscriptionSpecification(now, PackageType.Business);

            var subscriptionsBusiness = await repoSubscription.GetAllAsync(specSubscriptionBusiness);

            allSubscriptions.AddRange(subscriptionsBusiness);

            var specSubscriptionPro = new SubscriptionSpecification(now, PackageType.Pro);

            var subscriptionsPro = await repoSubscription.GetAllAsync(specSubscriptionPro);

            allSubscriptions.AddRange(subscriptionsPro);

            var specSubscriptionFree = new SubscriptionSpecification(now, PackageType.Free);

            var subscriptionsFree = await repoSubscription.GetAllAsync(specSubscriptionFree);

            allSubscriptions.AddRange(subscriptionsFree);

            var monitorModels = new List<MonitorViewModel>();

            foreach (var sub in allSubscriptions)
            {
                foreach (var item in sub.Customer!.Monitors)
                {
                    monitorModels.Add(new MonitorViewModel()
                    {
                        Id = item.Id,
                        CheckIntervalMinutes = item.CheckIntervalMinutes,
                        NotificationChannel = item.NotificationChannel,
                        IsActive = item.IsActive,
                        CssSelector = item.CssSelector,
                        URL = item.URL,
                        LastCheckedAt = item.LastCheckedAt,
                        LastContentHash = item.LastContentHash,
                        CreateAt = item.CreateAt,
                        Email = item.Customer!.IdentityUser!.NotificationEmail,
                        Telegram = item.Customer!.IdentityUser!.TelegramChatId,
                        Webhook = item.Customer!.IdentityUser!.Webhook,
                        PackageType = sub.PackageType,
                    });
                }
            }

            return monitorModels;
        }

        public async Task UpdateSubscriptionStatus(string subscriptionId, SubscriptionStatus status)
        {
            var repoSubscription = GetRepository<Subscription, string>();

            var subscription = await repoSubscription.GetAsync(new SubscriptionSpecification(subscriptionId, true));

            if (subscription is null)
            {
                throw new Exception("Subscription not found");
            }

            subscription.SubscriptionStatus = status;

            repoSubscription.UpdateAsync(subscription);

            bool savedSuccess = false;

            try
            {
                if (status == SubscriptionStatus.Active)
                {
                    await _notificationService.SendNotification(
                        subscription.CustomerID,
                        "Subscription Approved",
                        $"Your subscription to the {subscription.PackageType} plan has been approved. Enjoy your new features"
                    );
                }
                else 
                {
                    await _notificationService.SendNotification(
                        subscription.CustomerID,
                        "Subscription Rejected",
                        $"Your request for the {subscription.PackageType} plan was not approved"
                    );
                }

                await _unitOfWork.CompleteAsync();

                savedSuccess = true;
            }
            catch
            {
                savedSuccess = false;

                throw new Exception("Error Update Subscription Status");
            }
            finally 
            {
                if (savedSuccess && status == SubscriptionStatus.Active)
                {
                    DateTime now = DateTime.UtcNow;

                    var activeSubscriptions = await repoSubscription.GetAllAsync(new SubscriptionSpecification(subscription.CustomerID, now));

                    foreach (var item in activeSubscriptions)
                    {
                        if (subscriptionId != item.Id)
                        {
                            item.SubscriptionStatus = SubscriptionStatus.Expired;

                            repoSubscription.UpdateAsync(item);
                        }
                    }

                    await _unitOfWork.CompleteAsync();
                }
            }
        }

        public async Task<SubscriptionViewModel> UserHasSubscription(string userId)
        {
            var repoSubscription = GetRepository<Subscription, string>();

            DateTime now = DateTime.UtcNow;

            var subscription = await repoSubscription.GetAsync(new SubscriptionSpecification(userId, now));

            var subscriptionViewModel = new SubscriptionViewModel();

            if (subscription is null)
            {
                subscriptionViewModel = new SubscriptionViewModel()
                {
                    Id = string.Empty,
                    CustomerEmail = string.Empty,
                    CustomerPhone = string.Empty,
                    CustomerID = string.Empty,
                    EndDate = DateTime.MinValue,
                    StartDate = DateTime.MinValue,
                    PackageType = PackageType.Free,
                    SubscriptionStatus = SubscriptionStatus.Active,
                };
            }
            else 
            {
                subscriptionViewModel = new SubscriptionViewModel()
                {
                    Id = subscription.Id,
                    CustomerEmail = string.Empty,
                    CustomerPhone = string.Empty,
                    CustomerID = subscription.CustomerID,
                    EndDate = subscription.EndDate,
                    StartDate = subscription.StartDate,
                    PackageType = subscription.PackageType,
                    SubscriptionStatus = subscription.SubscriptionStatus,
                };
            }

            return subscriptionViewModel;
        }

        public async Task ExpiredSubscriptions()
        {
            var repoSubscription = GetRepository<Subscription, string>();

            var now = DateTime.UtcNow;

            var specSubscription = new SubscriptionSpecification(now, 's');

            var subscriptions = await repoSubscription.GetAllAsync(specSubscription);

            foreach (var subscription in subscriptions)
            {
                subscription.SubscriptionStatus = SubscriptionStatus.Expired;

                repoSubscription.UpdateAsync(subscription);
            }

            try 
            {
                if (subscriptions.Any()) 
                {
                    await _unitOfWork.CompleteAsync();
                }
            }
            catch
            {
                throw new Exception("Error While Expire Subscriptions");
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
