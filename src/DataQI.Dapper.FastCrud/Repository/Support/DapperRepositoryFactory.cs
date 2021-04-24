using System;

using DataQI.Commons.Repository.Core;
using DataQI.Commons.Util;

namespace DataQI.Dapper.FastCrud.Repository.Support
{
    public class DapperRepositoryFactory : RepositoryFactory
    {
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