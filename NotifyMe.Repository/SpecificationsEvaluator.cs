using Microsoft.EntityFrameworkCore;
using NotifyMe.Core.Entities;
using NotifyMe.Core.Specifications;

namespace NotifyMe.Repository.Specification
{
    public class SpecificationsEvaluator<TEntity, TKey> where TEntity : BaseEntity<TKey>
    {
        // Create A Return Query
        public static IQueryable<TEntity> GetQuery(IQueryable<TEntity> inputQuery, ISpecifications<TEntity, TKey> spec)
        {
            var query = inputQuery;

            if (spec.Criteria is not null)
            {
               query = query.Where(spec.Criteria);
            }

            if (spec.OrderBy is not null)
            {
                query = query.OrderBy(spec.OrderBy);
            }

            if (spec.OrderByDescending is not null)
            {
                query = query.OrderByDescending(spec.OrderByDescending);
            }

            if (spec.IsPagingEnabled)
            {
                query = query.Skip(spec.Skip!.Value).Take(spec.Take!.Value);
            }
            else if (spec.Take.HasValue)
            {
                query = query.Take(spec.Take.Value);
            }

            // Includes
            query = spec.Includes.Aggregate(query, (currentQuery, includeExpression) => currentQuery.Include(includeExpression));

            foreach (var include in spec.IncludeStrings)
            {
                query = query.Include(include);
            }

            // 🧩 DEBUG: Show generated SQL
            //var sql = query.ToQueryString();
            //Console.WriteLine("=====================================");
            //Console.WriteLine($"[DEBUG SQL for {typeof(TEntity).Name}]");
            //Console.WriteLine(sql);
            //Console.WriteLine("=====================================");

            return query;
        }
    }
}