using System;
using System.Data;
using System.Linq;
using Microsoft.Data.Sqlite;
using Microsoft.Extensions.DependencyInjection;
using Xunit;

using DataQI.Dapper.FastCrud.Repository;
using DataQI.Dapper.FastCrud.Repository.Support;

namespace DataQI.Dapper.FastCrud.Test.DependencyInjection
{
    public class ServiceCollectionExtensionsTest
    {        
        private readonly IServiceCollection services =
            new ServiceCollection().AddSingleton<IDbConnection, SqliteConnection>();
        
        [Fact]
        public void TestRejectsInvalidRepositoryType()
        {
            var exception = Assert.Throws<ArgumentException>(() => 
                services.AddDapperRepository<EntityRepository, IDbConnection>());
            Assert.Equal("TRepository must be a repository interface.", exception.Message);
        }
        
        [Fact]
        public void TestRejectsInvalidRepositoryImplementationType()
        {
            var exception = Assert.Throws<ArgumentException>(() => 
                services.AddDapperRepository<IEntityRepository, IEntityRepository, IDbConnection>());
            Assert.Equal("TRepositoryImplementation must be a repository concrete class", exception.Message);
        }
            
        [Fact]
        public void TestAddDefaultDapperRepository()
        {
            services.AddDefaultDapperRepository<Entity, IDbConnection>();

            var serviceProvider = services.BuildServiceProvider();
            var repository = serviceProvider.GetService<IDapperRepository<Entity>>();
            
            Assert.NotNull(repository);
        }
        
        [Fact]
        public void TestAddDapperRepository()
        {
            services.AddDapperRepository<IEntityRepository, IDbConnection>();

            var serviceProvider = services.BuildServiceProvider();
            var repository = serviceProvider.GetService<IEntityRepository>();

            Assert.NotNull(repository);
        }
        
        [Fact]
        public void TestAddDapperRepositoryImplementation()
        {
            services.AddDapperRepository<IEntityRepository, EntityRepository, IDbConnection>();

            var serviceProvider = services.BuildServiceProvider();
            var repository = serviceProvider.GetService<IEntityRepository>();
            
            Assert.NotNull(repository);
        }
        
        [Fact]
        public void TestAddSimpleDapperRepositoryMultipleTimes()
        {
            services.AddDefaultDapperRepository<Entity, IDbConnection>();
            services.AddDefaultDapperRepository<Entity, IDbConnection>();

            var serviceProvider = services.BuildServiceProvider();
            var repository = serviceProvider.GetService<IDapperRepository<Entity>>();

            Assert.Equal(1, services.Count(sd => sd.ServiceType == typeof(IDapperRepository<Entity>)));
            Assert.Equal(1, services.Count(sd => sd.ServiceType == typeof(DapperRepositoryFactory)));
            Assert.NotNull(repository);
        }
        
        [Fact]
        public void TestAddDapperRepositoryMultipleTimes()
        {
            services.AddDapperRepository<IEntityRepository, IDbConnection>();
            services.AddDapperRepository<IEntityRepository, IDbConnection>();

            var serviceProvider = services.BuildServiceProvider();
            var repository = serviceProvider.GetService<IEntityRepository>();

            Assert.Equal(1, services.Count(sd => sd.ServiceType == typeof(IEntityRepository)));
            Assert.Equal(1, services.Count(sd => sd.ServiceType == typeof(DapperRepositoryFactory)));
            Assert.NotNull(repository);
        }
        
        [Fact]
        public void TestAddDapperRepositoryImplementationMultipleTimes()
        {
            services.AddDapperRepository<IEntityRepository, EntityRepository, IDbConnection>();
            services.AddDapperRepository<IEntityRepository, EntityRepository, IDbConnection>();

            var serviceProvider = services.BuildServiceProvider();
            var repository = serviceProvider.GetService<IEntityRepository>();

            Assert.Equal(1, services.Count(sd => sd.ServiceType == typeof(IEntityRepository)));
            Assert.Equal(1, services.Count(sd => sd.ServiceType == typeof(DapperRepositoryFactory)));
            Assert.NotNull(repository);
        }
        
        private interface IEntityRepository : IDapperRepository<Entity> {  }
        
        private class EntityRepository : DapperRepository<Entity>, IEntityRepository
        {
            public EntityRepository(IDbConnection connection) : base(connection) { }
        }
        
        public sealed class Entity
        {
            public int Id { get; set; }
        }
    }
}

