using NotifyMe.Core.Entities;
using System.Linq.Expressions;

namespace NotifyMe.Core.Specifications
{
    public class BaseSpecification<TEntity, TKey> : ISpecifications<TEntity, TKey> where TEntity : BaseEntity<TKey>
    {
        public Expression<Func<TEntity, bool>> Criteria { get; set; } = null;

        public List<Expression<Func<TEntity, object>>> Includes { get; set; } = new List<Expression<Func<TEntity, object>>> ();

        public Expression<Func<TEntity, object>> OrderBy { get; set; }

        public Expression<Func<TEntity, object>> OrderByDescending { get; set; }

        public List<string> IncludeStrings { get; } = new();

        public int? Take { get; set; }

        public int? Skip { get; set; }

        public bool IsPagingEnabled { get; set; }

        public BaseSpecification(Expression<Func<TEntity, bool>> expression) 
        {
            Criteria = expression;
        }

        public BaseSpecification()
        {
        }

        public void AddOrderBy(Expression<Func<TEntity, object>> expression) 
        {
            OrderBy = expression;
        }

        public void AddOrderByDescending(Expression<Func<TEntity, object>> expression)
        {
            OrderByDescending = expression;
        }

        public void ApplyTake(int count)
        {
            Take = count;
        }

        protected void ApplyPaging(int skip, int take)
        {
            Skip = skip;
            Take = take;
            IsPagingEnabled = true;
        }
    }
}