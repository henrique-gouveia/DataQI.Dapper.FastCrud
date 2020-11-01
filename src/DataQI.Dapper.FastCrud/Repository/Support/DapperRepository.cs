using System;
using System.Collections.Generic;
using System.Data;
using System.Threading.Tasks;

using Dapper.FastCrud;

using DataQI.Commons.Query;
using DataQI.Commons.Util;

using DataQI.Dapper.FastCrud.Query.Support;

namespace DataQI.Dapper.FastCrud.Repository.Support
{
    public class DapperRepository<TEntity> : IDapperRepository<TEntity>
        where TEntity : class, new()
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

        public async Task DeleteAsync(TEntity entity)
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

        public async Task<bool> ExistsAsync(TEntity id)
        {
            Assert.NotNull(id, "Id must not be null");

            var entity = await FindOneAsync(id);
            return entity != null;
        }

        public IEnumerable<TEntity> Find(Func<ICriteria, ICriteria> criteriaBuilder)
        {
            Assert.NotNull(criteriaBuilder, "CriteriaBuilder must not be null");

            var criteria = new DapperCriteria();
            criteriaBuilder(criteria);

            var dapperCommand = criteria.BuildCommand();

            var entities = connection.Find<TEntity>(statement => statement
                .Where(dapperCommand.Command)
                .WithParameters(dapperCommand.Values));

            return entities;
        }

        public async Task<IEnumerable<TEntity>> FindAsync(Func<ICriteria, ICriteria> criteriaBuilder)
        {
            Assert.NotNull(criteriaBuilder, "CriteriaBuilder must not be null");

            var criteria = new DapperCriteria();
            criteriaBuilder(criteria);

            var dapperCommand = criteria.BuildCommand();

            var entities = await connection.FindAsync<TEntity>(statement => statement
                .Where(dapperCommand.Command)
                .WithParameters(dapperCommand.Values));

            return entities;
        }

        public IEnumerable<TEntity> FindAll()
        {
            var entities = connection.Find<TEntity>();
            return entities;
        }

        public async Task<IEnumerable<TEntity>> FindAllAsync()
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
        public async Task<TEntity> FindOneAsync(TEntity id)
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

        public async Task InsertAsync(TEntity entity)
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

        public async Task SaveAsync(TEntity entity)
        {
            Assert.NotNull(entity, "Entity must not be null");

            if (await ExistsAsync(entity))
                await connection.UpdateAsync(entity);
            else
                await InsertAsync(entity);
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