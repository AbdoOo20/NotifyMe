using Microsoft.AspNetCore.Http;
using Microsoft.AspNetCore.Identity;
using NotifyMe.Core;
using NotifyMe.Core.Entities;
using NotifyMe.Core.Repositories.Interface;
using NotifyMe.Repository.Data.Contexts;
using NotifyMe.Repository.Repositories;
using System.Collections;
using System.Security.Claims;

namespace NotifyMe.Repository.UnitOfWork
{
    public class UnitOfWork : IUnitOfWork
    {
        private readonly AppDbContext _context;

        private Hashtable _repositories;

        private readonly UserManager<ApplicationUser> _userManager;

        private readonly IHttpContextAccessor _httpContextAccessor;

        public UnitOfWork(AppDbContext context, UserManager<ApplicationUser> userManager, IHttpContextAccessor httpContextAccessor)
        {
            _context = context;
            _repositories = new Hashtable();
            _userManager = userManager;
            _httpContextAccessor = httpContextAccessor;
        }

        // Database Functions
        public async Task<int> CompleteAsync()
        {
            return await _context.SaveChangesAsync();
        }

        public async Task<string> GetCurrentUserRole(ApplicationUser user)
        {
            var isAdmin = await _userManager.IsInRoleAsync(user!, "Admin");

            var isCustomer = await _userManager.IsInRoleAsync(user!, "Customer");

            return isAdmin == true ? "Admin" : "Customer";
        }

        public string GetCurrentUserID()
        {
            return _httpContextAccessor.HttpContext?.User?.FindFirstValue(ClaimTypes.NameIdentifier) ?? "";
        }

        public IGenericRepository<TEntity, TKey>? Repository<TEntity, TKey>() where TEntity : BaseEntity<TKey>
        {
            var type = typeof(TEntity).Name;

            if (!_repositories.ContainsKey(type))
            {
                var repository = new GenericRepository<TEntity, TKey>(_context);

                _repositories.Add(type, repository);

                return repository;
            }

            return _repositories[type] as IGenericRepository<TEntity, TKey>;
        }
    }
}