using System;
using System.ComponentModel.Design;
using System.Collections.Generic;
using System.Data;
using System.Threading.Tasks;

using Dapper.FastCrud;

using Net.Data.Commons.Repository;
using Net.Data.Commons.Criterions;
using Net.Data.Commons.Criterions.Support;

namespace Net.Data.Dapper.FastCrud.Repository.Support
{
    public class DapperRepository<TEntity> : IDapperRepository<TEntity> 
        where TEntity : class, new()
    {
        protected IDbConnection connection;

        public DapperRepository(IDbConnection connection)
        {
            // 1. O Dapper.FastCrud não detecta qual o driver (SqlDialect) está sendo utilizado.
            //    Isso é necessário para que os statements sejam preparados de acordo com as
            //    as especificações de cada banco, os suportados (MsSql, MySql, SqLite, PostgreSql)
            //
            // 2. Além disso, manter a conexão aberta com o banco aguardando o Garbage Collector se torna
            //    custoso. Executar um try finally para abrir (connection.Open()) e fechar (connection.Close())
            //    também não é solução ideal, o uso da conection compartilhada entre métodos sincronos e assincronos pode 
            //    causar problemas com estados inadequados. A solução seria seria criar a conexão sempre que necessário utilizar 
            //    fazendo uso do using: using (var connection = new SqlConnection) { ... }, mas só isso não seria uma solução 
            //    elegante, pois estariamos nos acoplando com a classe concreta provedora da conexão.
            //   
            //    Solução: Substituir IDbConnection por um Factory Method IDbConnectionFactory responsável por saber os 
            //             detalhes da instância e de qual dialeto se trata, com isso, basta atribuir na classe base e 
            //             isso ficaria transparente para todos.
            //             ...
            //             OrmConfiguration.DefaultDialect = connectionFactory.Dialect;

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