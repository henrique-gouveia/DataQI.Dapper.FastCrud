using System;
using System.Linq;
using Dapper.FastCrud;
using Xunit;
using DataQI.Commons.Query;
using DataQI.Commons.Query.Support;
using DataQI.Commons.Repository.Paging;
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

        [Fact]
        public void TestCountStatementIgnoresOrderAndWindow()
        {
            var predicate = DapperPredicateVisitor.BuildPredicate(new Criteria());
            Assert.Equal(25, connection.Count<Product>(DapperPagingBuilder<Product>.CountStatement(predicate, false)));
        }

        [Fact]
        public void TestMergeKeepsMethodThenCriteriaThenPageThenKeyOrderAndDropsRepeats()
        {
            var orders = DapperPagingBuilder<Product>.MergeOrders(
                new[] { Order.Asc("Name") },
                PageRequest.Of(0, 10, Order.Desc("Name"), Order.Desc("Price")),
                new[] { "Id" });

            Assert.Equal(new[] { "Name", "Price", "Id" }, orders.Select(o => o.PropertyName));
            Assert.Equal(OrderDirection.Asc, orders[0].Direction);
            Assert.Equal(OrderDirection.Asc, orders[2].Direction);
        }

        [Fact]
        public void TestMergeDoesNotRepeatAKeyAlreadyInTheOrdering()
        {
            var orders = DapperPagingBuilder<Product>.MergeOrders(
                new[] { Order.Desc("Id") }, PageRequest.Of(0, 10), new[] { "Id" });

            Assert.Single(orders);
            Assert.Equal(OrderDirection.Desc, orders[0].Direction);
        }

        [Fact]
        public void TestMergeAppendsEveryPartOfACompositeKey()
        {
            var orders = DapperPagingBuilder<Product>.MergeOrders(
                new IOrderCriterion[0], PageRequest.Of(0, 10), new[] { "A", "B" });

            Assert.Equal(new[] { "A", "B" }, orders.Select(o => o.PropertyName));
        }

        [Fact]
        public void TestMergeFailsWithoutKeyAndWithoutExplicitOrder()
        {
            var exception = Assert.Throws<InvalidOperationException>(() =>
                DapperPagingBuilder<Product>.MergeOrders(new IOrderCriterion[0], PageRequest.Of(0, 10), new string[0]));

            Assert.Contains(nameof(Product), exception.Message);
        }

        [Fact]
        public void TestMergeAcceptsAnEntityWithoutKeyWhenAnOrderIsGiven()
        {
            var orders = DapperPagingBuilder<Product>.MergeOrders(
                new[] { Order.Asc("Name") }, PageRequest.Of(0, 10), new string[0]);

            Assert.Single(orders);
        }

        [Fact]
        public void TestEnsurePageRejectsNull()
        {
            Assert.Throws<ArgumentException>(() => DapperPagingBuilder<Product>.EnsurePage(null));
        }

        [Fact]
        public void TestEnsurePageRejectsOffsetBeyondIntMax()
        {
            var page = PageRequest.Of(int.MaxValue, 10);

            Assert.Throws<ArgumentException>(() => DapperPagingBuilder<Product>.EnsurePage(page));
        }

        [Fact]
        public void TestEnsurePageAcceptsAValidCustomImplementation()
        {
            DapperPagingBuilder<Product>.EnsurePage(new CustomPageRequest(2, 10, 20, new IOrderCriterion[0]));
        }

        [Theory]
        [InlineData(-1, 10, -10L)]
        [InlineData(0, 0, 0L)]
        [InlineData(0, -3, 0L)]
        [InlineData(0, int.MaxValue, 0L)]
        [InlineData(2, 10, 5L)]
        [InlineData(2, 10, -20L)]
        public void TestEnsurePageRejectsInconsistentCustomImplementations(int index, int size, long offset)
        {
            var page = new CustomPageRequest(index, size, offset, new IOrderCriterion[0]);

            Assert.Throws<ArgumentException>(() => DapperPagingBuilder<Product>.EnsurePage(page));
        }

        [Fact]
        public void TestEnsurePageRejectsNullOrders()
        {
            var page = new CustomPageRequest(0, 10, 0, null);

            Assert.Throws<ArgumentException>(() => DapperPagingBuilder<Product>.EnsurePage(page));
        }

        [Fact]
        public void TestEnsurePageRejectsNullOrderOrEmptyPropertyName()
        {
            Assert.Throws<ArgumentException>(() => DapperPagingBuilder<Product>.EnsurePage(
                new CustomPageRequest(0, 10, 0, new IOrderCriterion[] { null })));
            Assert.Throws<ArgumentException>(() => DapperPagingBuilder<Product>.EnsurePage(
                new CustomPageRequest(0, 10, 0, new[] { Order.Asc("") })));
        }









        [Theory]
        [InlineData(5, 10, 2, 25L)]
        [InlineData(0, 10, 0, 0L)]
        public void TestDeducesTheTotalWhenThePageRevealsIt(int items, int size, int index, long expected)
        {
            Assert.Equal(expected, DapperPagingBuilder<Product>.DeduceTotal(items, PageRequest.Of(index, size)));
        }

        [Theory]
        [InlineData(10, 10, 0)]
        [InlineData(0, 10, 5)]
        public void TestDoesNotDeduceTheTotalOtherwise(int items, int size, int index)
        {
            Assert.Null(DapperPagingBuilder<Product>.DeduceTotal(items, PageRequest.Of(index, size)));
        }


    }
}
