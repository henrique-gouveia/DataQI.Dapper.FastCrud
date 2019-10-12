using System;
using System.ComponentModel.Design;
using System.Collections.Generic;
using System.Data;
using System.Threading.Tasks;

using Dapper.FastCrud;

using DataQI.Commons.Repository;
using DataQI.Commons.Criterions;
using DataQI.Commons.Criterions.Support;

namespace DataQI.Dapper.FastCrud.Repository.Support
{
    public class DapperRepository<TEntity> : IDapperRepository<TEntity> 
        where TEntity : class, new()
    {
        protected IDbConnection connection;

        public DapperRepository(IDbConnection connection)
        {
            this.connection = connection;
        }

        public void Delete(TEntity entity)
        {
            connection.Delete(entity);
        }

        public async Task DeleteAsync(TEntity entity)
        {
            await connection.DeleteAsync(entity);
        }

        public bool Exists(TEntity id)
        {
            var entity = FindOne(id);
            return entity != null;
        }

        public async Task<bool> ExistsAsync(TEntity id)
        {
            var entity = await FindOneAsync(id);
            return entity != null;
        }

        public IEnumerable<TEntity> Find(Func<ICriteria, ICriteria> criteriaBuilder)
        {
            var criteria = criteriaBuilder(new Criteria());

            var entities = connection.Find<TEntity>(statement => statement
                .Where($"{criteria.ToSqlString()}")
                .WithParameters(criteria.Parameters));

            return entities;
        }

        public async Task<IEnumerable<TEntity>> FindAsync(Func<ICriteria, ICriteria> criteriaBuilder)
        {
            var criteria = criteriaBuilder(new Criteria());

            var entities = await connection.FindAsync<TEntity>(statement => statement
                .Where($"{criteria.ToSqlString()}")
                .WithParameters(criteria.Parameters));
            
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
            var entity = connection.Get(id);
            return entity;
        }
        public async Task<TEntity> FindOneAsync(TEntity id)
        {
            var entity = await connection.GetAsync(id);
            return entity;
        }

        public void Insert(TEntity entity)
        {
            connection.Insert(entity);
        }

        public async Task InsertAsync(TEntity entity)
        {
            await connection.InsertAsync(entity);
        }

        public void Save(TEntity entity)
        {
            if (Exists(entity))
                connection.Update(entity);
            else
                Insert(entity);
        }

        public async Task SaveAsync(TEntity entity)
        {
            if (await ExistsAsync(entity))
                await connection.UpdateAsync(entity);
            else
                await InsertAsync(entity);
        }
    }
}