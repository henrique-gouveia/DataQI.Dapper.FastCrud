using DataQI.Commons.Repository;

namespace DataQI.Dapper.FastCrud.Repository
{
    public interface IDapperRepository<TEntity> : ICrudRepository<TEntity, TEntity>
        where TEntity : class
    {

    }
}