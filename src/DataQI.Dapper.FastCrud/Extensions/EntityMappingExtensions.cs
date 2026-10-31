using System.Collections.Generic;
using System.Linq;

using Dapper.FastCrud.Mappings;

namespace DataQI.Dapper.FastCrud.Extensions
{
    /// <summary>Provides extension methods for <see cref="EntityMapping{TEntity}"/>.</summary>
    internal static class EntityMappingExtensions
    {
        /// <summary>Gets the names of the primary key properties of a mapped entity.</summary>
        /// <typeparam name="TEntity">The entity type.</typeparam>
        /// <param name="mapping">The mapping of <typeparamref name="TEntity"/>.</param>
        /// <returns>The key property names, or an empty list when the mapping has no primary key.</returns>
        internal static IReadOnlyList<string> KeyProperties<TEntity>(this EntityMapping<TEntity> mapping)
            => mapping.GetProperties()
                .Where(p => p.IsPrimaryKey)
                .Select(p => p.PropertyName)
                .ToArray();
    }
}
