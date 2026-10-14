using Microsoft.AspNetCore.Http;
using NotifyMe.Core;
using NotifyMe.Core.Entities;
using NotifyMe.Core.Models;
using NotifyMe.Core.Repositories.Interface;
using NotifyMe.Core.Services.Interface;
using NotifyMe.Core.Specifications.MonitorErrorSpecification;
using TimeZoneConverter;

namespace NotifyMe.Services.MonitorErrorService
{
    public class MonitorErrorService : IMonitorErrorService
    {
        private readonly IUnitOfWork _unitOfWork;

        private readonly IHttpContextAccessor _httpContextAccessor;

        public MonitorErrorService(IUnitOfWork unitOfWork, IHttpContextAccessor httpContextAccessor)
        {
            _unitOfWork = unitOfWork;
            _httpContextAccessor = httpContextAccessor;
        }

        public async Task AddMonitorError(string monitorId, string errorType, string error, string? stackTrace)
        {
            var repoMonitorError = GetRepository<MonitorErrors, string>();

            var newMonitorError = new MonitorErrors()
            {
                Id = Guid.NewGuid().ToString(),
                ErrorMessage = error,
                ErrorType = errorType,
                StackTrace = stackTrace,
                MonitorId = monitorId,
            };

            await repoMonitorError.AddAsync(newMonitorError);

            try
            {
                await _unitOfWork.CompleteAsync();
            }
            catch
            {
                throw new Exception("Error Create Monitor-Error");
            }
        }

        public async Task<List<MonitorErrorViewModel>> GetMonitorErrors(string monitorId)
        {
            var repoMonitorError = GetRepository<MonitorErrors, string>();

            var specification = new MonitorErrorSpecification(monitorId);

            var errors = await repoMonitorError.GetAllAsync(specification);

            var userTimeZoneId = _httpContextAccessor.HttpContext?.Request.Cookies["UserTimeZone"] ?? "UTC";

            TimeZoneInfo targetZone = TZConvert.GetTimeZoneInfo(userTimeZoneId);

            var errorViewModels = errors.Select(e => new MonitorErrorViewModel
            {
                Id = e.Id,
                Message = e.ErrorMessage,
                Type = e.ErrorType,
                CreateAt = TimeZoneInfo.ConvertTimeFromUtc(e.CreateAt, targetZone),
                StackTrace = e.StackTrace ?? "",

            }).ToList();

            return errorViewModels;
        }

        public async Task<int> GetMonitorErrorsCount(string monitorId)
        {
            var repoMonitorError = GetRepository<MonitorErrors, string>();

            var specification = new MonitorErrorSpecification(monitorId);

            var errors = await repoMonitorError.GetCount(specification);

            return errors;
        }

        private IGenericRepository<TEntity, TKey> GetRepository<TEntity, TKey>() where TEntity : BaseEntity<TKey>
        {
            var repo = _unitOfWork.Repository<TEntity, TKey>();

            if (repo is null) throw new Exception("Error Create Repository");

            return repo;
        }
    }
}
