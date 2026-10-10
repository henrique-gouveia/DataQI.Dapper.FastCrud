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
    public class DapperRepository<TEntity> : IDapperRepository<TEntity>
        where TEntity : class
    {
        protected IDbConnection connection;

        public DapperRepository(IDbConnection connection)
        {
            Assert.NotNull(connection, "Connection must not be null");
            this.connection = connection;
        }

        public void Delete(TEntity entity)
        {
            Assert.NotNull(entity, "Entity must not be null");
            connection.Delete(entity);
        }

        public async Task DeleteAsync(TEntity entity, CancellationToken cancellationToken = default)
        {
            Assert.NotNull(entity, "Entity must not be null");
            await connection.DeleteAsync(entity);
        }

        public bool Exists(TEntity id)
        {
            Assert.NotNull(id, "Id must not be null");
            var entity = FindOne(id);
            return entity != null;
        }

        public async Task<bool> ExistsAsync(TEntity id, CancellationToken cancellationToken = default)
        {
            Assert.NotNull(id, "Id must not be null");
            var entity = await FindOneAsync(id, cancellationToken);
            return entity != null;
        }

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

        public IEnumerable<TEntity> Find(Func<ICriteria, ICriteria> criteriaBuilder)
            => connection.Find<TEntity>(CriteriaStatement(criteriaBuilder));

        public async Task<IEnumerable<TEntity>> FindAsync(Func<ICriteria, ICriteria> criteriaBuilder,
            CancellationToken cancellationToken = default)
            => await connection.FindAsync<TEntity>(CriteriaStatement(criteriaBuilder));

        public TEntity FindOne(Func<ICriteria, ICriteria> criteriaBuilder)
            => connection.Find<TEntity>(CriteriaStatement(criteriaBuilder, 2)).SingleOrDefault();

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

        public IEnumerable<TEntity> FindAll()
        {
            var entities = connection.Find<TEntity>();
            return entities;
        }

        public async Task<IEnumerable<TEntity>> FindAllAsync(CancellationToken cancellationToken = default)
        {
            var entities = await connection.FindAsync<TEntity>();
            return entities;
        }

        public TEntity FindOne(TEntity id)
        {
            Assert.NotNull(id, "Id must not be null");
            var entity = connection.Get(id);
            return entity;
        }
        public async Task<TEntity> FindOneAsync(TEntity id, CancellationToken cancellationToken = default)
        {
            Assert.NotNull(id, "Id must not be null");
            var entity = await connection.GetAsync(id);
            return entity;
        }

        public void Insert(TEntity entity)
        {
            Assert.NotNull(entity, "Entity must not be null");
            connection.Insert(entity);
        }

        public async Task InsertAsync(TEntity entity, CancellationToken cancellationToken = default)
        {
            Assert.NotNull(entity, "Entity must not be null");
            await connection.InsertAsync(entity);
        }

        public void Save(TEntity entity)
        {
            Assert.NotNull(entity, "Entity must not be null");
            if (Exists(entity))
                connection.Update(entity);
            else
                Insert(entity);
        }

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

        public void Dispose()
        {
            Dispose(true);
        }
        #endregion
    }
}
