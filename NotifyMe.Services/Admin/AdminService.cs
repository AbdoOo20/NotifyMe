using NotifyMe.Core;
using NotifyMe.Core.Entities;
using NotifyMe.Core.Enums;
using NotifyMe.Core.Models;
using NotifyMe.Core.Repositories.Interface;
using NotifyMe.Core.Services.Interface;
using NotifyMe.Core.Specifications.CustomerSpecification;
using NotifyMe.Core.Specifications.MonitorSpecification;
using NotifyMe.Core.Specifications.SubscriptionSpecification;

namespace NotifyMe.Services.AdminService
{
    public class AdminService : IAdminService
    {
        private readonly IUnitOfWork _unitOfWork;

        public AdminService(IUnitOfWork unitOfWork)
        {
            _unitOfWork = unitOfWork;
        }

        public async Task<AdminStatisticsViewModel> GetAdminStatisticsAsync()
        {
            DateTime now = DateTime.UtcNow;

            var repoCustomer = GetRepository<Customer, string>();

            var repoMonitor = GetRepository<Core.Entities.Monitor, string>();

            var repoSubscriptions = GetRepository<Subscription, string>();

            var customers = await repoCustomer.GetCount(new CustomerSpecification());

            var activeMonitors = await repoMonitor.GetCount(new MonitorSpecification(true));

            var inActiveMonitors = await repoMonitor.GetCount(new MonitorSpecification(false));

            var activeRevenueSubscriptions = await repoSubscriptions.GetAllAsync(new SubscriptionSpecification('c'));

            var activeSubscriptions = await repoSubscriptions.GetCount(new SubscriptionSpecification(now));

            var cancelledSubscriptions = await repoSubscriptions.GetCount(new SubscriptionSpecification(SubscriptionStatus.Cancelled));

            var expiredSubscriptions = await repoSubscriptions.GetCount(new SubscriptionSpecification(SubscriptionStatus.Expired));

            var pendingSubscriptions = await repoSubscriptions.GetCount(new SubscriptionSpecification(SubscriptionStatus.Pending));

            var freeSubscriptions = await repoSubscriptions.GetCount(new SubscriptionSpecification(PackageType.Free));

            int totalRevenue = 0;

            foreach (var sub in activeRevenueSubscriptions)
            {
                var totalDays = (sub.EndDate - sub.StartDate).TotalDays;

                if (totalDays >= 360)
                {
                    totalRevenue += sub.PackageType == PackageType.Business ? 250 : 80;  
                }
                else
                {
                    totalRevenue += sub.PackageType == PackageType.Business ? 25 : 8;
                }
            }

            return new AdminStatisticsViewModel
            {
                CustomerCount = customers,
                ActiveMonitorCount = activeMonitors,
                ActiveSubs = activeSubscriptions,
                CancelledSubs = cancelledSubscriptions,
                ExpiredSubs = expiredSubscriptions,
                InActiveMonitorCount = inActiveMonitors,
                PendingSubs = pendingSubscriptions,
                TotalRevenue = totalRevenue,
                ActiveFreePackage = freeSubscriptions,
            };
        }

        private IGenericRepository<TEntity, TKey> GetRepository<TEntity, TKey>() where TEntity : BaseEntity<TKey>
        {
            var repo = _unitOfWork.Repository<TEntity, TKey>();

            if (repo is null) throw new Exception("Error Create Repository");

            return repo;
        }
    }
}
