using NotifyMe.Core.Entities;
using NotifyMe.Core.Repositories.Interface;

namespace NotifyMe.Core
{
    public interface IUnitOfWork
    {
        Task<int> CompleteAsync();

        string GetCurrentUserID();

        IGenericRepository<TEntity, TKey>? Repository<TEntity, TKey>() where TEntity : BaseEntity<TKey>;
    }
}