using System;
using System.Data;

using DataQI.Dapper.FastCrud.Repository;
using DataQI.Dapper.FastCrud.Repository.Support;

using DataQI.Dapper.FastCrud.Test.Fixtures;

using Xunit;

namespace DataQI.Dapper.FastCrud.Test.Repository
{
    public sealed class DapperRepositoryFactoryTest : IClassFixture<DbFixture>
    {
        private readonly IDbConnection connection;
        private readonly DapperRepositoryFactory repositoryFactory;

        public DapperRepositoryFactoryTest(DbFixture fixture)
        {
            connection = fixture.Connection;
            repositoryFactory = new DapperRepositoryFactory();
        }

        [Fact]
        public void TestRejectsInvalidArgs()
            => Assert.Throws<MissingMethodException>(() =>
                repositoryFactory.GetRepository<IEntityRepository>());

        [Fact]
        public void TestGetRepositoryWithArgsCorrectly()
        {
            var entityRepository = repositoryFactory.GetRepository<IEntityRepository>(connection);
            Assert.NotNull(entityRepository);
        }

        [Fact]
        public void TestGetRepositoryWithRepositoryFactoryCorrectly()
        {
            var entityRepository = repositoryFactory.GetRepository<IEntityRepository>(() => 
                new DapperRepository<object>(connection));

            Assert.NotNull(entityRepository);
        }

        private interface IEntityRepository : IDapperRepository<object>
        {
            
        }
    }
}