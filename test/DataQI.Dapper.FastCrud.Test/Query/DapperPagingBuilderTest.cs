using System;
using System.Data;
using System.Linq;

using Dapper.FastCrud;
using Xunit;

using DataQI.Commons.Query.Support;

using DataQI.Dapper.FastCrud.Query.Support;
using DataQI.Dapper.FastCrud.Test.Fixtures;
using DataQI.Dapper.FastCrud.Test.Repository.Products;

namespace DataQI.Dapper.FastCrud.Test.Query
{
    public sealed class DapperPagingBuilderTest : IClassFixture<DbFixture>, IDisposable
    {
        private readonly IDbConnection connection;

        public DapperPagingBuilderTest(DbFixture fixture)
        {
            connection = fixture.Connection;
            for (var i = 0; i < 25; i++)
                connection.Insert(ProductBuilder.NewInstance().Build());
        }

        [Fact]
        public void TestKeyPropertiesComeFromTheMapping()
        {
            Assert.Equal(new[] { nameof(Product.Id) }, DapperPagingBuilder<Product>.KeyProperties());
        }

        [Fact]
        public void TestKeyPropertiesReflectMappingChangesAfterFirstRead()
        {
            var original = OrmConfiguration.GetDefaultEntityMapping<ReconfigurableEntity>();
            try
            {
                var initialKeys = DapperPagingBuilder<ReconfigurableEntity>.KeyProperties();
                var replacement = original.Clone()
                    .SetProperty(e => e.Id, p => p.SetPrimaryKey(false))
                    .SetProperty(e => e.AlternateId, p => p.SetPrimaryKey(true));
                OrmConfiguration.SetDefaultEntityMapping(replacement);

                var updatedKeys = DapperPagingBuilder<ReconfigurableEntity>.KeyProperties();

                Assert.Equal(new[] { nameof(ReconfigurableEntity.Id) }, initialKeys);
                Assert.Equal(new[] { nameof(ReconfigurableEntity.AlternateId) }, updatedKeys);
            }
            finally
            {
                OrmConfiguration.SetDefaultEntityMapping(original);
            }
        }

        [Fact]
        public void TestPageStatementSkipsAndLimitsTheRows()
        {
            var predicate = DapperPredicateVisitor.BuildPredicate(new Criteria());

            var statement = DapperPagingBuilder<Product>.PageStatement(
                predicate, false, new[] { Order.Asc(nameof(Product.Id)) }, 10, 10);

            Assert.Equal(10, connection.Find<Product>(statement).Count());
        }

        [Theory]
        [InlineData("Nope")]
        [InlineData("Name; DROP TABLE Products")]
        [InlineData("Name DESC, (SELECT 1)")]
        public void TestUnknownOrderIsRejectedBeforeCreatingTheSqlStatement(string propertyName)
        {
            var predicate = DapperPredicateVisitor.BuildPredicate(new Criteria());

            var exception = Assert.Throws<InvalidOperationException>(() =>
                DapperPagingBuilder<Product>.PageStatement(predicate, false,
                    new[] { Order.Asc(propertyName) }, 0, 10));

            Assert.Equal($"Cannot order Product by unknown property '{propertyName}'.", exception.Message);
        }

        [Fact]
        public void TestCountStatementIgnoresOrderAndPaging()
        {
            var predicate = DapperPredicateVisitor.BuildPredicate(new Criteria());

            var countStatement = DapperPagingBuilder<Product>.CountStatement(predicate, false);

            Assert.Equal(25, connection.Count<Product>(countStatement));
        }

        public void Dispose() => connection.BulkDelete<Product>();

        private sealed class ReconfigurableEntity
        {
            [System.ComponentModel.DataAnnotations.Key]
            public int Id { get; set; }
            public int AlternateId { get; set; }
        }
    }
}
