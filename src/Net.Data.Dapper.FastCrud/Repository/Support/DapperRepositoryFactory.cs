using System.Data;
using System;
using Net.Data.Commons.Repository.Core;

namespace Net.Data.Dapper.FastCrud.Repository.Support
{
    public class DapperRepositoryFactory : RepositoryFactory
    {
        private readonly IDbConnection connection;

        public DapperRepositoryFactory(IDbConnection connection)
        {
            this.connection = connection;
        }

        protected override object GetCustomImplementation(Type repositoryInterface)
        {
            var repositoryMetadata = GetRepositoryMetadata(repositoryInterface);

            var dapperImplementationType = typeof(DapperRepository<>);
            var customImplementationType = dapperImplementationType.MakeGenericType(repositoryMetadata.EntityType);
            var customImplementation = Activator.CreateInstance(customImplementationType, new[] { connection });

            return customImplementation;
        }
    }
}