using System;
using System.Data;
using Microsoft.Extensions.DependencyInjection.Extensions;

using DataQI.Commons.Util;
using DataQI.Dapper.FastCrud.Repository;
using DataQI.Dapper.FastCrud.Repository.Support;

namespace Microsoft.Extensions.DependencyInjection
{
    public static class ServiceCollectionExtensions
    {
        public static IServiceCollection AddDefaultDapperRepository<TEntity, TDbConnection>(
            this IServiceCollection services)
            where TEntity : class
            where TDbConnection : IDbConnection
            => AddDapperRepository<IDapperRepository<TEntity>, TDbConnection>(services);

        public static IServiceCollection AddDapperRepository<TRepository, TDbConnection>(
            this IServiceCollection services)
            where TRepository : class
            where TDbConnection : IDbConnection
            => AddDapperRepository<TRepository, TDbConnection>(services, null);

        public static IServiceCollection AddDapperRepository<TRepository, TRepositoryImplementation, TDbConnection>(
            this IServiceCollection services)
            where TRepository : class
            where TRepositoryImplementation : class
            where TDbConnection : IDbConnection
            => AddDapperRepository<TRepository, TDbConnection>(services, typeof(TRepositoryImplementation));

        private static IServiceCollection AddDapperRepository<TRepository, TDbConnection>(
            this IServiceCollection services, Type repositoryImplementationType)
            where TRepository : class
            where TDbConnection : IDbConnection
        {
            Assert.True(typeof(TRepository).IsInterface, "TRepository must be a repository interface.");
            Assert.True(repositoryImplementationType == null || !repositoryImplementationType.IsAbstract, 
                "TRepositoryImplementation must be a repository concrete class");
            
            services.TryAddScoped<DapperRepositoryFactory>();
            services.TryAddScoped(serviceFactory =>
            {
                var dbConnection = serviceFactory.GetRequiredService<TDbConnection>();
                var repositoryFactory = serviceFactory.GetRequiredService<DapperRepositoryFactory>();
                TRepository repository;
                if (repositoryImplementationType != null)
                {
                    var repositoryImplementationInstance = Activator.CreateInstance(repositoryImplementationType, dbConnection);
                    repository = repositoryFactory.GetRepository<TRepository>(() => repositoryImplementationInstance);
                }
                else
                    repository = repositoryFactory.GetRepository<TRepository>(dbConnection);

                return repository;
            });

            return services;
        }
    }
}