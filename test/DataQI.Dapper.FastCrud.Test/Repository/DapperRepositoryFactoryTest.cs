using System;
using System.Data;

using DataQI.Commons.Repository;
using DataQI.Commons.Repository.Paging;
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
        public void TestRepositoryCombinesIndependentCapabilities()
        {
            Assert.True(typeof(ICrudRepository<object, object>).IsAssignableFrom(typeof(IDapperRepository<object>)));
            Assert.True(typeof(IPagedRepository<object, object>).IsAssignableFrom(typeof(IDapperRepository<object>)));
            Assert.False(typeof(ICrudRepository<object, object>).IsAssignableFrom(typeof(IPagedRepository<object, object>)));
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