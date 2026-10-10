using System;
using System.Collections.Generic;
using System.Data;
using System.Linq;
using System.Threading;
using System.Threading.Tasks;

using Dapper.FastCrud;
using Dapper.FastCrud.Configuration.StatementOptions.Builders;

using DataQI.Commons.Query;
using DataQI.Commons.Query.Support;
using DataQI.Commons.Util;

using DataQI.Dapper.FastCrud.Query.Support;

namespace DataQI.Dapper.FastCrud.Repository.Support
{
    /// <summary>Implements <see cref="IDapperRepository{TEntity}"/> on top of Dapper.FastCrud and an <see cref="IDbConnection"/>.</summary>
    /// <typeparam name="TEntity">The entity type; it also serves as its own identifier.</typeparam>
    /// <remarks>
    /// <para>Each call runs immediately against the connection; there is no unit of work to commit.</para>
    /// <para>The <see cref="System.Threading.CancellationToken"/> parameters are accepted for interface compatibility but are not observed.</para>
    /// <para>Disposing the repository disposes the connection it was given.</para>
    /// </remarks>
    public class DapperRepository<TEntity> : IDapperRepository<TEntity>
        where TEntity : class
    {
        /// <summary>Holds the connection used by every operation, which is disposed and set to <c>null</c> when the repository is disposed.</summary>
        protected IDbConnection connection;

        /// <summary>Initializes a new instance of the <see cref="DapperRepository{TEntity}"/> class.</summary>
        /// <param name="connection">The connection to run statements on; must not be <c>null</c>.</param>
        /// <exception cref="System.ArgumentException"><paramref name="connection"/> is <c>null</c>.</exception>
        public DapperRepository(IDbConnection connection)
        {
            Assert.NotNull(connection, "Connection must not be null");
            this.connection = connection;
        }

        /// <summary>Deletes the given entity, located by its key.</summary>
        /// <param name="entity">The entity to delete; must not be <c>null</c>.</param>
        /// <exception cref="System.ArgumentException"><paramref name="entity"/> is <c>null</c>.</exception>
        public void Delete(TEntity entity)
        {
            Assert.NotNull(entity, "Entity must not be null");
            connection.Delete(entity);
        }

        /// <summary>Deletes the given entity asynchronously, located by its key.</summary>
        /// <param name="entity">The entity to delete; must not be <c>null</c>.</param>
        /// <param name="cancellationToken">Accepted for interface compatibility; not observed.</param>
        /// <returns>A task that represents the asynchronous operation.</returns>
        /// <exception cref="System.ArgumentException"><paramref name="entity"/> is <c>null</c>.</exception>
        public async Task DeleteAsync(TEntity entity, CancellationToken cancellationToken = default)
        {
            Assert.NotNull(entity, "Entity must not be null");
            await connection.DeleteAsync(entity);
        }

        /// <summary>Determines whether a row exists for the key properties of the given entity.</summary>
        /// <param name="id">An entity whose key properties are filled in; must not be <c>null</c>.</param>
        /// <returns><c>true</c> if a row exists; otherwise, <c>false</c>.</returns>
        /// <exception cref="System.ArgumentException"><paramref name="id"/> is <c>null</c>.</exception>
        public bool Exists(TEntity id)
        {
            Assert.NotNull(id, "Id must not be null");
            var entity = FindOne(id);
            return entity != null;
        }

        /// <summary>Determines asynchronously whether a row exists for the key properties of the given entity.</summary>
        /// <param name="id">An entity whose key properties are filled in; must not be <c>null</c>.</param>
        /// <param name="cancellationToken">Accepted for interface compatibility; not observed.</param>
        /// <returns>A task that yields <c>true</c> if a row exists; otherwise, <c>false</c>.</returns>
        /// <exception cref="System.ArgumentException"><paramref name="id"/> is <c>null</c>.</exception>
        public async Task<bool> ExistsAsync(TEntity id, CancellationToken cancellationToken = default)
        {
            Assert.NotNull(id, "Id must not be null");
            var entity = await FindOneAsync(id, cancellationToken);
            return entity != null;
        }

        /// <inheritdoc cref="IDapperRepository{TEntity}.Find(Func{IRangedBatchSelectSqlSqlStatementOptionsOptionsBuilder{TEntity}, IRangedBatchSelectSqlSqlStatementOptionsOptionsBuilder{TEntity}})" />
        public IEnumerable<TEntity> Find(
            Func<
                IRangedBatchSelectSqlSqlStatementOptionsOptionsBuilder<TEntity>,
                IRangedBatchSelectSqlSqlStatementOptionsOptionsBuilder<TEntity>
            > statementBuilder)
        {
            Assert.NotNull(statementBuilder, "StatementBuilder must not be null");
            var entities = connection.Find<TEntity>(statement =>
                statementBuilder(statement));
            return entities;
        }

        /// <inheritdoc cref="IDapperRepository{TEntity}.FindAsync(Func{IRangedBatchSelectSqlSqlStatementOptionsOptionsBuilder{TEntity}, IRangedBatchSelectSqlSqlStatementOptionsOptionsBuilder{TEntity}}, CancellationToken)" />
        public async Task<IEnumerable<TEntity>> FindAsync(
            Func<
                IRangedBatchSelectSqlSqlStatementOptionsOptionsBuilder<TEntity>,
                IRangedBatchSelectSqlSqlStatementOptionsOptionsBuilder<TEntity>
            > statementBuilder, CancellationToken cancellationToken = default)
        {
            Assert.NotNull(statementBuilder, "StatementBuilder must not be null");
            var entities = await connection.FindAsync<TEntity>(statement =>
                statementBuilder(statement));
            return entities;
        }

        /// <inheritdoc />
        public IEnumerable<TEntity> Find(Func<ICriteria, ICriteria> criteriaBuilder)
            => connection.Find<TEntity>(CriteriaStatement(criteriaBuilder));

        /// <inheritdoc />
        /// <remarks>The cancellation token is accepted for interface compatibility but is not observed.</remarks>
        public async Task<IEnumerable<TEntity>> FindAsync(Func<ICriteria, ICriteria> criteriaBuilder,
            CancellationToken cancellationToken = default)
            => await connection.FindAsync<TEntity>(CriteriaStatement(criteriaBuilder));

        /// <inheritdoc />
        public TEntity FindOne(Func<ICriteria, ICriteria> criteriaBuilder)
            => connection.Find<TEntity>(CriteriaStatement(criteriaBuilder, 2)).SingleOrDefault();

        /// <inheritdoc />
        /// <remarks>The cancellation token is accepted for interface compatibility but is not observed.</remarks>
        public async Task<TEntity> FindOneAsync(Func<ICriteria, ICriteria> criteriaBuilder,
            CancellationToken cancellationToken = default)
            => (await connection.FindAsync<TEntity>(CriteriaStatement(criteriaBuilder, 2))).SingleOrDefault();


        private static Action<IRangedBatchSelectSqlSqlStatementOptionsOptionsBuilder<TEntity>> CriteriaStatement(
            Func<ICriteria, ICriteria> criteriaBuilder, long? maxResults = null)
        {
            Assert.NotNull(criteriaBuilder, "CriteriaBuilder must not be null");
            var criteria = criteriaBuilder(new Criteria());

            var predicate = DapperPredicateVisitor.BuildPredicate(criteria);
            var orderBy = DapperOrderByBuilder.BuildOrderBy(criteria.Orders);
            var hasCriterions = criteria.Criterions.Count > 0;

            return statement =>
            {
                if (maxResults.HasValue)
                    statement.Top(maxResults);

                if (hasCriterions)
                    statement
                        .Where(predicate.Command)
                        .OrderBy(orderBy)
                        .WithParameters(predicate.Values);
                else if (orderBy != null)
                    statement.OrderBy(orderBy);
            };
        }

        /// <inheritdoc />
        public IEnumerable<TEntity> FindAll()
        {
            var entities = connection.Find<TEntity>();
            return entities;
        }

        /// <inheritdoc />
        /// <remarks>The cancellation token is accepted for interface compatibility but is not observed.</remarks>
        public async Task<IEnumerable<TEntity>> FindAllAsync(CancellationToken cancellationToken = default)
        {
            var entities = await connection.FindAsync<TEntity>();
            return entities;
        }

        /// <summary>Finds the entity whose key properties match those of the given entity.</summary>
        /// <param name="id">An entity whose key properties are filled in; must not be <c>null</c>.</param>
        /// <returns>The entity, or <c>null</c> when no row matches.</returns>
        /// <exception cref="System.ArgumentException"><paramref name="id"/> is <c>null</c>.</exception>
        public TEntity FindOne(TEntity id)
        {
            Assert.NotNull(id, "Id must not be null");
            var entity = connection.Get(id);
            return entity;
        }
        /// <summary>Finds the entity asynchronously by the key properties of the given entity.</summary>
        /// <param name="id">An entity whose key properties are filled in; must not be <c>null</c>.</param>
        /// <param name="cancellationToken">Accepted for interface compatibility; not observed.</param>
        /// <returns>A task that yields the entity, or <c>null</c> when no row matches.</returns>
        /// <exception cref="System.ArgumentException"><paramref name="id"/> is <c>null</c>.</exception>
        public async Task<TEntity> FindOneAsync(TEntity id, CancellationToken cancellationToken = default)
        {
            Assert.NotNull(id, "Id must not be null");
            var entity = await connection.GetAsync(id);
            return entity;
        }

        /// <summary>Inserts a new row for the entity.</summary>
        /// <param name="entity">The entity to insert; must not be <c>null</c>.</param>
        /// <exception cref="System.ArgumentException"><paramref name="entity"/> is <c>null</c>.</exception>
        public void Insert(TEntity entity)
        {
            Assert.NotNull(entity, "Entity must not be null");
            connection.Insert(entity);
        }

        /// <summary>Inserts a new row for the entity asynchronously.</summary>
        /// <param name="entity">The entity to insert; must not be <c>null</c>.</param>
        /// <param name="cancellationToken">Accepted for interface compatibility; not observed.</param>
        /// <returns>A task that represents the asynchronous operation.</returns>
        /// <exception cref="System.ArgumentException"><paramref name="entity"/> is <c>null</c>.</exception>
        public async Task InsertAsync(TEntity entity, CancellationToken cancellationToken = default)
        {
            Assert.NotNull(entity, "Entity must not be null");
            await connection.InsertAsync(entity);
        }

        /// <summary>Updates the row of the entity when one exists for its key; otherwise inserts it.</summary>
        /// <param name="entity">The entity to save; must not be <c>null</c>.</param>
        /// <exception cref="System.ArgumentException"><paramref name="entity"/> is <c>null</c>.</exception>
        public void Save(TEntity entity)
        {
            Assert.NotNull(entity, "Entity must not be null");
            if (Exists(entity))
                connection.Update(entity);
            else
                Insert(entity);
        }

        /// <summary>Updates the row of the entity asynchronously when one exists for its key; otherwise inserts it.</summary>
        /// <param name="entity">The entity to save; must not be <c>null</c>.</param>
        /// <param name="cancellationToken">Accepted for interface compatibility; not observed.</param>
        /// <returns>A task that represents the asynchronous operation.</returns>
        /// <exception cref="System.ArgumentException"><paramref name="entity"/> is <c>null</c>.</exception>
        public async Task SaveAsync(TEntity entity, CancellationToken cancellationToken = default)
        {
            Assert.NotNull(entity, "Entity must not be null");
            if (await ExistsAsync(entity, cancellationToken))
                await connection.UpdateAsync(entity);
            else
                await InsertAsync(entity, cancellationToken);
        }

        #region IDisposable Support
        private bool disposedValue = false;

        /// <summary>Releases the connection when <paramref name="disposing"/> is <c>true</c>; later calls do nothing.</summary>
        /// <param name="disposing"><c>true</c> when called from <see cref="Dispose()"/>.</param>
        protected virtual void Dispose(bool disposing)
        {
            if (!disposedValue)
            {
                if (disposing)
                {
                    connection?.Dispose();
                    connection = null;
                }
                disposedValue = true;
            }
        }

        /// <summary>Releases the connection used by this repository.</summary>
        public void Dispose()
        {
            Dispose(true);
        }
        #endregion
    }
}
