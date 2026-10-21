using System;
using System.Collections.Generic;
using System.Linq;
using System.Runtime.CompilerServices;

using Dapper.FastCrud;
using Dapper.FastCrud.Configuration.StatementOptions.Builders;
using Dapper.FastCrud.Mappings;

using DataQI.Commons.Query;
using DataQI.Commons.Query.Support;
using DataQI.Commons.Util;

namespace DataQI.Dapper.FastCrud.Query.Support
{
    internal static class DapperStatementBuilder<TEntity> where TEntity : class
    {
        public static Action<IRangedBatchSelectSqlSqlStatementOptionsOptionsBuilder<TEntity>> BuildFind(
            DapperPredicate filter, IReadOnlyList<IOrderCriterion> orders, long? top)
        {
            var orderBy = BuildOrderBy(orders);
            return statement =>
            {
                if (top.HasValue)
                    statement.Top(top);

                if (!filter.IsEmpty)
                    statement.Where(filter.Command).OrderBy(orderBy).WithParameters(filter.Values);
                else if (orderBy != null)
                    statement.OrderBy(orderBy);
            };
        }

        public static Action<IRangedBatchSelectSqlSqlStatementOptionsOptionsBuilder<TEntity>> BuildPage(
            DapperPredicate filter, IReadOnlyList<IOrderCriterion> orders, EntityMapping<TEntity> mapping,
            long skip, long take)
        {
            var mappedProperties = new HashSet<string>(mapping.GetProperties().Select(p => p.PropertyName),
                StringComparer.Ordinal);
            foreach (var order in orders)
                Assert.True<InvalidOperationException>(mappedProperties.Contains(order.PropertyName),
                    () => $"Cannot order {typeof(TEntity).Name} by unknown property '{order.PropertyName}'.");

            var orderBy = BuildOrderBy(orders);
            return statement =>
            {
                if (!filter.IsEmpty)
                    statement.Where(filter.Command).WithParameters(filter.Values);
                statement.OrderBy(orderBy).Skip(skip).Top(take);
            };
        }

        public static Action<IConditionalSqlStatementOptionsBuilder<TEntity>> BuildWhere(DapperPredicate filter)
            => statement =>
            {
                if (!filter.IsEmpty)
                    statement.Where(filter.Command).WithParameters(filter.Values);
            };

        private static FormattableString BuildOrderBy(IReadOnlyList<IOrderCriterion> orders)
        {
            if (orders.Count == 0)
                return null;

            var columns = orders.Select(order => (object)Sql.Column(order.PropertyName)).ToArray();
            var fragments = orders.Select((order, index) =>
                $"{{{index}}} {(order.Direction == OrderDirection.Asc ? "ASC" : "DESC")}");

            return FormattableStringFactory.Create(string.Join(", ", fragments), columns);
        }
    }
}
