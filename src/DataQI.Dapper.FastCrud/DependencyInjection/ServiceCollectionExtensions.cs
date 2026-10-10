using System;
using System.Data;
using Microsoft.Extensions.DependencyInjection.Extensions;

using DataQI.Commons.Util;
using DataQI.Dapper.FastCrud.Repository;
using DataQI.Dapper.FastCrud.Repository.Support;

namespace Microsoft.Extensions.DependencyInjection
{
    /// <summary>Registers DataQI Dapper.FastCrud repositories in an <see cref="IServiceCollection"/>.</summary>
    /// <remarks>
    /// Registrations are scoped and use <c>TryAdd</c> semantics: existing registrations for <c>TRepository</c>
    /// and the factory are preserved, and missing registrations are added. The <c>TDbConnection</c> type must be registered by the caller; the
    /// repository receives the instance resolved from the container.
    /// </remarks>
    public static class ServiceCollectionExtensions
    {
        /// <summary>Registers <see cref="IDapperRepository{TEntity}"/> for an entity type.</summary>
        /// <typeparam name="TEntity">The entity type.</typeparam>
        /// <typeparam name="TDbConnection">The connection type resolved from the container.</typeparam>
        /// <param name="services">The service collection to add to.</param>
        /// <returns>The same <paramref name="services"/>, to allow chaining.</returns>
        public static IServiceCollection AddDefaultDapperRepository<TEntity, TDbConnection>(
            this IServiceCollection services)
            where TEntity : class
            where TDbConnection : IDbConnection
            => AddDapperRepository<IDapperRepository<TEntity>, TDbConnection>(services);

        /// <summary>Registers a custom repository interface that the framework implements from method names.</summary>
        /// <typeparam name="TRepository">The repository interface; query methods declared on it are parsed from their names.</typeparam>
        /// <typeparam name="TDbConnection">The connection type resolved from the container.</typeparam>
        /// <param name="services">The service collection to add to.</param>
        /// <returns>The same <paramref name="services"/>, to allow chaining.</returns>
        /// <exception cref="System.ArgumentException"><typeparamref name="TRepository"/> is not an interface.</exception>
        public static IServiceCollection AddDapperRepository<TRepository, TDbConnection>(
            this IServiceCollection services)
            where TRepository : class
            where TDbConnection : IDbConnection
            => AddDapperRepository<TRepository, TDbConnection>(services, null);

        /// <summary>Registers a custom repository interface served by a custom implementation.</summary>
        /// <typeparam name="TRepository">The repository interface.</typeparam>
        /// <typeparam name="TRepositoryImplementation">The concrete class the proxy forwards to; it needs a public constructor taking the connection as its only argument.</typeparam>
        /// <typeparam name="TDbConnection">The connection type resolved from the container.</typeparam>
        /// <param name="services">The service collection to add to.</param>
        /// <returns>The same <paramref name="services"/>, to allow chaining.</returns>
        /// <exception cref="System.ArgumentException"><typeparamref name="TRepository"/> is not an interface, or <typeparamref name="TRepositoryImplementation"/> is abstract.</exception>
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