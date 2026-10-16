using System;
using System.Collections.Generic;
using System.Linq;

using Dapper.FastCrud;
using Dapper.FastCrud.Configuration.StatementOptions.Builders;

using DataQI.Commons.Query;
using DataQI.Commons.Query.Support;
using DataQI.Commons.Repository.Paging;

namespace DataQI.Dapper.FastCrud.Query.Support
{
    internal static class DapperPagingBuilder<TEntity> where TEntity : class
    {
        public static void EnsurePage(IPageRequest page)
        {
            if (page == null)
                throw new ArgumentException("Page request must not be null", nameof(page));
            if (page.PageIndex < 0 || page.PageSize < 1 || page.PageSize == int.MaxValue)
                throw new ArgumentException("Page index must not be negative and page size must be between 1 and int.MaxValue - 1", nameof(page));
            if (page.Offset != (long)page.PageIndex * page.PageSize || page.Offset > int.MaxValue)
                throw new ArgumentException("Page offset must equal PageIndex * PageSize and must not exceed int.MaxValue", nameof(page));
            if (page.Orders == null || page.Orders.Any(o => o == null || string.IsNullOrEmpty(o.PropertyName)))
                throw new ArgumentException("Page orders must not be null or contain null orders or empty property names", nameof(page));
        }

        public static IReadOnlyList<IOrderCriterion> MergeOrders(
            IEnumerable<IOrderCriterion> criteriaOrders, IPageRequest page, IEnumerable<string> keyProperties)
        {
            var merged = new List<IOrderCriterion>();
            var seen = new HashSet<string>(StringComparer.Ordinal);
            foreach (var order in criteriaOrders.Concat(page.Orders))
                if (seen.Add(order.PropertyName))
                    merged.Add(order);
            foreach (var key in keyProperties)
                if (seen.Add(key))
                    merged.Add(Order.Asc(key));
            if (merged.Count == 0)
                throw new InvalidOperationException(
                    $"Cannot page {typeof(TEntity).Name}: it has no primary key and no explicit ordering");
            return merged;
        }

        public static IReadOnlyList<string> KeyProperties()
            => OrmConfiguration.GetDefaultEntityMapping<TEntity>().GetProperties()
                .Where(p => p.IsPrimaryKey).Select(p => p.PropertyName).ToArray();

        public static Action<IRangedBatchSelectSqlSqlStatementOptionsOptionsBuilder<TEntity>> PageStatement(
            DapperPredicate predicate, bool hasCriterions, IReadOnlyList<IOrderCriterion> orders, long skip, long top)
        {
            var orderBy = DapperOrderByBuilder.BuildOrderBy(orders.ToArray());
            return statement =>
            {
                if (hasCriterions)
                    statement.Where(predicate.Command).WithParameters(predicate.Values);
                statement.OrderBy(orderBy).Skip(skip).Top(top);
            };
        }

        public static Action<IConditionalSqlStatementOptionsBuilder<TEntity>> CountStatement(
            DapperPredicate predicate, bool hasCriterions)
            => statement =>
            {
                if (hasCriterions)
                    statement.Where(predicate.Command).WithParameters(predicate.Values);
            };

        public static long? DeduceTotal(int itemCount, IPageRequest page)
            => itemCount < page.PageSize && (itemCount > 0 || page.PageIndex == 0)
                ? page.Offset + itemCount
                : (long?)null;
    }
}
