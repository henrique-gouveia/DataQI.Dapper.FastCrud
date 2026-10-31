using System;
using System.Data;
using System.Linq;

using Dapper.FastCrud;
using Dapper.FastCrud.Configuration.StatementOptions.Builders;
using Moq;
using Xunit;

using DataQI.Commons.Query;
using DataQI.Commons.Query.Support;

using DataQI.Dapper.FastCrud.Query.Support;
using DataQI.Dapper.FastCrud.Test.Fixtures;
using DataQI.Dapper.FastCrud.Test.Repository.Products;

namespace DataQI.Dapper.FastCrud.Test.Query
{
    public sealed class DapperStatementBuilderTest : IClassFixture<DbFixture>, IDisposable
    {
        private readonly IDbConnection connection;

        public DapperStatementBuilderTest(DbFixture fixture)
        {
            connection = fixture.Connection;
            for (var i = 0; i < 25; i++)
                connection.Insert(ProductBuilder.NewInstance().Build());
        }

        [Fact]
        public void TestBuildPageSkipsAndLimitsTheRows()
        {
            var predicate = DapperPredicateVisitor.BuildPredicate(new Criteria());

            var statement = DapperStatementBuilder<Product>.BuildPage(
                predicate, new[] { Order.Asc(nameof(Product.Id)) }, OrmConfiguration.GetDefaultEntityMapping<Product>(), 10, 10);

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
                DapperStatementBuilder<Product>.BuildPage(predicate,
                    new[] { Order.Asc(propertyName) }, OrmConfiguration.GetDefaultEntityMapping<Product>(), 0, 10));

            Assert.Equal($"Cannot order Product by unknown property '{propertyName}'.", exception.Message);
        }

        [Fact]
        public void TestBuildWhereIgnoresOrderAndPaging()
        {
            var predicate = DapperPredicateVisitor.BuildPredicate(new Criteria());

            var whereStatement = DapperStatementBuilder<Product>.BuildWhere(predicate);

            Assert.Equal(25, connection.Count<Product>(whereStatement));
        }

        [Fact]
        public void TestBuildWhereWithEmptyFilterDoesNotCallWhere()
        {
            var filter = DapperPredicateVisitor.BuildPredicate(new Criteria());
            var statement = new Mock<IConditionalSqlStatementOptionsBuilder<Product>>(MockBehavior.Strict);

            DapperStatementBuilder<Product>.BuildWhere(filter)(statement.Object);

            statement.VerifyNoOtherCalls();
        }

        [Fact]
        public void TestFindAppliesAscendingAndDescendingOrdersInSequence()
        {
            var filter = DapperPredicateVisitor.BuildPredicate(new Criteria());
            var orders = new[] { Order.Asc(nameof(Product.Name)), Order.Desc(nameof(Product.Stock)),
                Order.Asc(nameof(Product.Id)) };

            var actual = connection.Find<Product>(DapperStatementBuilder<Product>.BuildFind(filter, orders, null))
                .Select(p => p.Id);
            var expected = connection.Find<Product>().OrderBy(p => p.Name)
                .ThenByDescending(p => p.Stock).ThenBy(p => p.Id).Select(p => p.Id);

            Assert.Equal(expected, actual);
        }

        [Fact]
        public void TestFindAppliesASingleDescendingOrder()
        {
            var filter = DapperPredicateVisitor.BuildPredicate(new Criteria());
            var orders = new[] { Order.Desc(nameof(Product.Id)) };

            var actual = connection.Find<Product>(DapperStatementBuilder<Product>.BuildFind(filter, orders, null))
                .Select(p => p.Id);

            Assert.Equal(connection.Find<Product>().OrderByDescending(p => p.Id).Select(p => p.Id), actual);
        }

        [Fact]
        public void TestFindWithEmptyFilterAndNoOrdersDoesNotAddWhere()
        {
            var filter = DapperPredicateVisitor.BuildPredicate(new Criteria());

            var rows = connection.Find<Product>(DapperStatementBuilder<Product>.BuildFind(
                filter, Array.Empty<IOrderCriterion>(), null));

            Assert.Equal(25, rows.Count());
        }

        [Fact]
        public void TestFindWithFilterAndNoOrdersKeepsThePredicate()
        {
            var name = connection.Find<Product>().First().Name;
            var filter = DapperPredicateVisitor.BuildPredicate(new Criteria()
                .Add(Restrictions.Equal(nameof(Product.Name), name)));

            var rows = connection.Find<Product>(DapperStatementBuilder<Product>.BuildFind(
                filter, Array.Empty<IOrderCriterion>(), null));

            Assert.NotEmpty(rows);
            Assert.All(rows, row => Assert.Equal(name, row.Name));
        }

        public void Dispose() => connection.BulkDelete<Product>();
    }
}
