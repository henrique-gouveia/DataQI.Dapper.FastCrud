using System;

using DataQI.Commons.Repository.Core;
using DataQI.Commons.Util;

namespace DataQI.Dapper.FastCrud.Repository.Support
{
    /// <summary>Creates repositories backed by <see cref="DapperRepository{TEntity}"/>.</summary>
    /// <remarks>
    /// The arguments passed to <c>GetRepository</c> become the constructor arguments of the repository, so the first
    /// one must be an <see cref="System.Data.IDbConnection"/>.
    /// </remarks>
    public class DapperRepositoryFactory : RepositoryFactory
    {
        /// <inheritdoc />
        /// <exception cref="System.ArgumentException"><paramref name="repositoryType"/> is <c>null</c>.</exception>
        protected override object GetRepositoryInstance(Type repositoryType, params object[] args)
        {
            Assert.NotNull(repositoryType, "Repository Type must not be null");

            var repositoryMetadata = GetRepositoryMetadata(repositoryType);

            var dapperRepositoryType = typeof(DapperRepository<>);
            var repositoryInstanceType = dapperRepositoryType.MakeGenericType(repositoryMetadata.EntityType);
            
            var repositoryInstance = Activator.CreateInstance(repositoryInstanceType, args);
            return repositoryInstance;
        }
    }
}