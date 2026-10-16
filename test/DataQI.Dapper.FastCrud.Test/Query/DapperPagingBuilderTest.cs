using System;
using System.Linq;
using Dapper.FastCrud;
using Xunit;
using DataQI.Commons.Query;
using DataQI.Commons.Query.Support;
using DataQI.Dapper.FastCrud.Query.Support;
using DataQI.Dapper.FastCrud.Test.Fixtures;
using DataQI.Dapper.FastCrud.Test.Repository.Products;

namespace DataQI.Dapper.FastCrud.Test.Query
{
    public sealed class DapperPagingBuilderTest : IClassFixture<DbFixture>, IDisposable
    {
        private readonly System.Data.IDbConnection connection;

        public DapperPagingBuilderTest(DbFixture fixture)
        {
            connection = fixture.Connection;
            for (var i = 0; i < 25; i++)
                connection.Insert(ProductBuilder.NewInstance().Build());
        }

        public void Dispose() => connection.BulkDelete<Product>();

        [Fact]
        public void TestKeyPropertiesComeFromTheMapping()
        {
            Assert.Equal(new[] { nameof(Product.Id) }, DapperPagingBuilder<Product>.KeyProperties());
        }

        [Fact]
        public void TestPageStatementSkipsAndLimitsTheRows()
        {
            var predicate = DapperPredicateVisitor.BuildPredicate(new Criteria());
            var statement = DapperPagingBuilder<Product>.PageStatement(
                predicate, false, new[] { Order.Asc("Id") }, 10, 10);
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
            Assert.Equal(25, connection.Count<Product>(DapperPagingBuilder<Product>.CountStatement(predicate, false)));
        }

    }
}
