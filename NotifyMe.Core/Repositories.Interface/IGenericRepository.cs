using NotifyMe.Core.Entities;
using NotifyMe.Core.Specifications;

namespace NotifyMe.Core.Repositories.Interface
{
    public interface IGenericRepository<TEntity, TKey> where TEntity : BaseEntity<TKey>
    {
        Task<IEnumerable<TEntity>> GetAllAsync(ISpecifications<TEntity, TKey> spec);

        Task<IEnumerable<TEntity>> GetAll(ISpecifications<TEntity, TKey> spec);

        Task<TEntity?> GetAsync(ISpecifications<TEntity, TKey> spec);

        Task<int> GetCount(ISpecifications<TEntity, TKey> spec);

        Task AddAsync(TEntity entity);

        void UpdateAsync(TEntity entity);

        void DeleteAsync(TEntity entity);
    }
}