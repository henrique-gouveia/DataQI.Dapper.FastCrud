using System;
using System.Collections.Generic;
using System.Threading;
using System.Threading.Tasks;

using Dapper.FastCrud.Configuration.StatementOptions.Builders;
using DataQI.Commons.Repository;

namespace DataQI.Dapper.FastCrud.Repository
{
    public interface IDapperRepository<TEntity> : ICrudRepository<TEntity, TEntity>
        where TEntity : class
    {
        IEnumerable<TEntity> Find(
            Func<
                IRangedBatchSelectSqlSqlStatementOptionsOptionsBuilder<TEntity>,
                IRangedBatchSelectSqlSqlStatementOptionsOptionsBuilder<TEntity>
            > statementBuilder);
        
        Task<IEnumerable<TEntity>> FindAsync(
            Func<
                IRangedBatchSelectSqlSqlStatementOptionsOptionsBuilder<TEntity>,
                IRangedBatchSelectSqlSqlStatementOptionsOptionsBuilder<TEntity>
            > statementBuilder, CancellationToken cancellationToken = default);
    }
}