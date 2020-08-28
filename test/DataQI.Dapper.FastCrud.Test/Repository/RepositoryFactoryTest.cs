using System;
using System.Data;

using DataQI.Dapper.FastCrud.Repository;
using DataQI.Dapper.FastCrud.Repository.Support;

using DataQI.Dapper.FastCrud.Test.Fixtures;

using Xunit;

namespace DataQI.Dapper.FastCrud.Test.Repository
{
    public class RepositoryFactoryTest : IClassFixture<DbFixture>
    {
        private readonly IDbConnection connection;

        public RepositoryFactoryTest(DbFixture fixture)
        {
            connection = fixture.Connection;
        }

        [Fact]
        public void TestRejectsNullConnection()
        {
            var exception = Assert.Throws<ArgumentException>(() => 
                new DapperRepositoryFactory(null));
            var exceptionMessage = exception.GetBaseException().Message;

            Assert.IsType<ArgumentException>(exception.GetBaseException());
            Assert.Equal("Connection must not be null", exceptionMessage);
        }

        [Fact]
        public void TestGetRepository()
        {
            var connectionFactory = new DapperRepositoryFactory(connection);
            var entityRepository = connectionFactory.GetRepository<IEntityRepository>();

            Assert.NotNull(entityRepository);
        }

        private interface IEntityRepository : IDapperRepository<object>
        {
            
        }
    }
}