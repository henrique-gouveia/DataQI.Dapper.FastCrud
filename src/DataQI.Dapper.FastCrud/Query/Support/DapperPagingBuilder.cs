using System;
using System.Collections.Generic;
using System.Linq;

using Dapper.FastCrud;
using Dapper.FastCrud.Configuration.StatementOptions.Builders;

using DataQI.Commons.Query;

namespace DataQI.Dapper.FastCrud.Query.Support
{
    internal static class DapperPagingBuilder<TEntity> where TEntity : class
    {
        public static Action<IRangedBatchSelectSqlSqlStatementOptionsOptionsBuilder<TEntity>> PageStatement(
            DapperPredicate predicate, bool hasCriterions, IReadOnlyList<IOrderCriterion> orders, long skip, long top)
        {
            var mappedProperties = new HashSet<string>(OrmConfiguration.GetDefaultEntityMapping<TEntity>()
                .GetProperties()
                .Select(p => p.PropertyName),
                StringComparer.Ordinal);
            foreach (var order in orders)
                if (!mappedProperties.Contains(order.PropertyName))
                    throw new InvalidOperationException(
                        $"Cannot order {typeof(TEntity).Name} by unknown property '{order.PropertyName}'.");
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

    }
}
