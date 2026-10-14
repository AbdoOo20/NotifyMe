using NotifyMe.Core.Entities;
using System.Linq.Expressions;

namespace NotifyMe.Core.Specifications
{
    public interface ISpecifications<TEntity, TKey> where TEntity : BaseEntity<TKey>
    {
        public Expression<Func<TEntity, bool>> Criteria { get; set; }

        public List<Expression<Func<TEntity, object>>> Includes { get; set; }

        public List<string> IncludeStrings { get; }

        public Expression<Func<TEntity, object>> OrderBy { get; set; }

        public Expression<Func<TEntity, object>> OrderByDescending { get; set; }

        public int? Take { get; set; }

        public int? Skip { get; set; }
        
        public bool IsPagingEnabled { get; set; }
    }
}