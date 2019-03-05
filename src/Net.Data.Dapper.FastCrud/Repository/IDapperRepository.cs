using System.Collections.Generic;
using System.Threading.Tasks;

namespace Net.Data.Dapper.FastCrud.Repository
{
    public interface IDapperRepository<TEntity> where TEntity : class, new()
    {
        IEnumerable<TEntity> FindAll();

        Task<IEnumerable<TEntity>> FindAllAsync();

        TEntity FindOne(TEntity id);

        Task<TEntity> FindOneAsync(TEntity id);

        bool Exists(TEntity id);

        Task<bool> ExistsAsync(TEntity id);

        void Insert(TEntity entity);

        Task InsertAsync(TEntity entity);

        void Save(TEntity entity);

        Task SaveAsync(TEntity entity);

        void Delete(TEntity entity);

        Task DeleteAsync(TEntity entity);
    }
}