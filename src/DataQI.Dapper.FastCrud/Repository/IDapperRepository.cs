using System;
using System.Collections.Generic;
using System.Threading;
using System.Threading.Tasks;

using Dapper.FastCrud.Configuration.StatementOptions.Builders;

using DataQI.Commons.Repository;

namespace DataQI.Dapper.FastCrud.Repository
{
    /// <summary>Defines the Dapper.FastCrud repository of an entity type, which is also its own identifier.</summary>
    /// <typeparam name="TEntity">The entity type.</typeparam>
    /// <remarks>
    /// In addition to the members of <see cref="ICrudRepository{TEntity, TId}"/>, it accepts a Dapper.FastCrud
    /// statement builder. Where the base contract takes an identifier, pass an entity instance whose key
    /// properties are filled in.
    /// </remarks>
    public interface IDapperRepository<TEntity> : ICrudRepository<TEntity, TEntity>
        where TEntity : class
    {
        /// <summary>Finds the entities selected by a Dapper.FastCrud statement.</summary>
        /// <param name="statementBuilder">A function that configures the select statement options (where clause, order, paging) and returns them.</param>
        /// <returns>The matching entities; an empty sequence when none match.</returns>
        /// <exception cref="System.ArgumentException"><paramref name="statementBuilder"/> is <c>null</c>.</exception>
        IEnumerable<TEntity> Find(
            Func<
                IRangedBatchSelectSqlSqlStatementOptionsOptionsBuilder<TEntity>,
                IRangedBatchSelectSqlSqlStatementOptionsOptionsBuilder<TEntity>
            > statementBuilder);
        
        /// <summary>Finds the entities selected by a Dapper.FastCrud statement asynchronously.</summary>
        /// <param name="statementBuilder">A function that configures the select statement options (where clause, order, paging) and returns them.</param>
        /// <param name="cancellationToken">Accepted for interface compatibility; this provider does not observe it.</param>
        /// <returns>A task that yields the matching entities; an empty sequence when none match.</returns>
        /// <exception cref="System.ArgumentException"><paramref name="statementBuilder"/> is <c>null</c>.</exception>
        Task<IEnumerable<TEntity>> FindAsync(
            Func<
                IRangedBatchSelectSqlSqlStatementOptionsOptionsBuilder<TEntity>,
                IRangedBatchSelectSqlSqlStatementOptionsOptionsBuilder<TEntity>
            > statementBuilder, CancellationToken cancellationToken = default);
    }
}