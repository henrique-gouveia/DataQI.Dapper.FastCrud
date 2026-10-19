using Xunit;

using Dapper.FastCrud;

using DataQI.Dapper.FastCrud.Extensions;
using DataQI.Dapper.FastCrud.Test.Repository.Products;

namespace DataQI.Dapper.FastCrud.Test.Extensions
{
    public sealed class EntityMappingExtensionsTest
    {
        [Fact]
        public void TestKeyPropertiesComeFromTheMapping()
        {
            Assert.Equal(new[] { nameof(Product.Id) }, OrmConfiguration.GetDefaultEntityMapping<Product>().KeyProperties());
        }

        [Fact]
        public void TestKeyPropertiesReflectMappingChangesAfterFirstRead()
        {
            var original = OrmConfiguration.GetDefaultEntityMapping<ReconfigurableEntity>();
            try
            {
                var initialKeys = OrmConfiguration.GetDefaultEntityMapping<ReconfigurableEntity>().KeyProperties();
                var replacement = original.Clone()
                    .SetProperty(e => e.Id, p => p.SetPrimaryKey(false))
                    .SetProperty(e => e.AlternateId, p => p.SetPrimaryKey(true));
                OrmConfiguration.SetDefaultEntityMapping(replacement);

                var updatedKeys = OrmConfiguration.GetDefaultEntityMapping<ReconfigurableEntity>().KeyProperties();

                Assert.Equal(new[] { nameof(ReconfigurableEntity.Id) }, initialKeys);
                Assert.Equal(new[] { nameof(ReconfigurableEntity.AlternateId) }, updatedKeys);
            }
            finally
            {
                OrmConfiguration.SetDefaultEntityMapping(original);
            }
        }

        private sealed class ReconfigurableEntity
        {
            [System.ComponentModel.DataAnnotations.Key]
            public int Id { get; set; }
            public int AlternateId { get; set; }
        }
    }
}
