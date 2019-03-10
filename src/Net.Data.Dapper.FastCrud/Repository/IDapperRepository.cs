using Net.Data.Commons.Repository;

namespace Net.Data.Dapper.FastCrud.Repository
{
    public interface IDapperRepository<TEntity> : ICrudRepository<TEntity, TEntity>
        where TEntity : class, new()
    {

    }
}