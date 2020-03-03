using System;
using System.Data;

using DataQI.Commons.Repository.Core;
using DataQI.Commons.Util;

namespace DataQI.Dapper.FastCrud.Repository.Support
{
    public class DapperRepositoryFactory : RepositoryFactory
    {
        private readonly IDbConnection connection;

        public DapperRepositoryFactory(IDbConnection connection)
        {
            Assert.NotNull(connection, "Connection must not be null");

            this.connection = connection;
        }

        protected override object GetCustomImplementation(Type repositoryInterface)
        {
            Assert.NotNull(connection, "RepositoryInterface must not be null");

            var repositoryMetadata = GetRepositoryMetadata(repositoryInterface);

            var dapperImplementationType = typeof(DapperRepository<>);
            var customImplementationType = dapperImplementationType.MakeGenericType(repositoryMetadata.EntityType);
            var customImplementation = Activator.CreateInstance(customImplementationType, new[] { connection });

            return customImplementation;
        }
    }
}