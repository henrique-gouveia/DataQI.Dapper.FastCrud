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

    }
}
