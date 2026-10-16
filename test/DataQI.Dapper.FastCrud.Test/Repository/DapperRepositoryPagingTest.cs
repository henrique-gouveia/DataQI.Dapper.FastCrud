using System;
using System.Collections.Generic;
using System.Data;
using System.Linq;
using System.Reflection;
using System.Threading;
using System.Threading.Tasks;
using Dapper.FastCrud;
using DataQI.Commons.Query;
using DataQI.Commons.Query.Support;
using DataQI.Commons.Repository.Paging;
using DataQI.Commons.Repository.Paging.Support;
using DataQI.Dapper.FastCrud.Repository.Support;
using DataQI.Dapper.FastCrud.Test.Fixtures;
using DataQI.Dapper.FastCrud.Test.Query;
using DataQI.Dapper.FastCrud.Test.Repository.Products;
using Xunit;

namespace DataQI.Dapper.FastCrud.Test.Repository
{
    public sealed class DapperRepositoryPagingTest : IClassFixture<DbFixture>, IDisposable
    {
        private readonly IDbConnection connection;
        private readonly IProductRepository repository;
        private readonly DapperRepository<Product> direct;

        public DapperRepositoryPagingTest(DbFixture fixture)
        {
            connection = fixture.Connection;
            repository = fixture.ProductRepository;
            direct = new DapperRepository<Product>(connection);
            Seed(25);
        }

        private void Seed(int count, string department = "D1")
        {
            for (var i = 0; i < count; i++)
            {
                var product = ProductBuilder.NewInstance().Build();
                product.Department = department;
                product.Name = "Name" + i % 5;
                product.Ean = department + i.ToString("D4");
                connection.Insert(product);
            }
        }

        [Fact]
        public void TestFirstPage()
        {
            var slice = repository.FindSlice(c => c, PageRequest.Of(0, 10));
            Assert.Equal(10, slice.Items.Count);
            Assert.True(slice.HasNext);
            Assert.False(slice.HasPrevious);
            var page = repository.FindPage(c => c, PageRequest.Of(0, 10));
            Assert.Equal(25, page.TotalCount);
            Assert.Equal(3, page.TotalPages);
        }

        [Fact]
        public async Task TestLastPartialPage()
        {
            var slice = repository.FindSlice(c => c, PageRequest.Of(2, 10));
            Assert.Equal(5, slice.Items.Count);
            Assert.False(slice.HasNext);
            var page = repository.FindPage(c => c, PageRequest.Of(2, 10));
            Assert.Equal(5, page.Items.Count);
            Assert.False(page.HasNext);
            Assert.Equal(25, page.TotalCount);
            var asyncPage = await repository.FindPageAsync(c => c, PageRequest.Of(2, 10));
            Assert.Equal(5, asyncPage.Items.Count);
            Assert.False(asyncPage.HasNext);
            Assert.Equal(25, asyncPage.TotalCount);
        }

        [Fact]
        public void TestExactMultipleLastPage()
        {
            connection.BulkDelete<Product>();
            Seed(20);
            var slice = repository.FindSlice(c => c, PageRequest.Of(1, 10));
            Assert.Equal(10, slice.Items.Count);
            Assert.False(slice.HasNext);
            var page = repository.FindPage(c => c, PageRequest.Of(1, 10));
            Assert.Equal(20, page.TotalCount);
            Assert.False(page.HasNext);
        }

        [Fact]
        public async Task TestSliceAsyncFirstAndLastPage()
        {
            var first = await repository.FindSliceAsync(c => c, PageRequest.Of(0, 10));
            Assert.Equal(10, first.Items.Count);
            Assert.True(first.HasNext);
            Assert.False(first.HasPrevious);
            var last = await repository.FindSliceAsync(c => c, PageRequest.Of(2, 10));
            Assert.Equal(5, last.Items.Count);
            Assert.False(last.HasNext);
            Assert.True(last.HasPrevious);
            var beyond = await repository.FindSliceAsync(c => c, PageRequest.Of(5, 10));
            Assert.Empty(beyond.Items);
            Assert.False(beyond.HasNext);
            connection.BulkDelete<Product>();
            Seed(20);
            var exact = await repository.FindSliceAsync(c => c, PageRequest.Of(1, 10));
            Assert.Equal(10, exact.Items.Count);
            Assert.False(exact.HasNext);
        }

        [Fact]
        public async Task TestPageBeyondTheEnd()
        {
            var page = repository.FindPage(c => c, PageRequest.Of(5, 10));
            Assert.Empty(page.Items);
            Assert.Equal(25, page.TotalCount);
            var asyncPage = await repository.FindPageAsync(c => c, PageRequest.Of(5, 10));
            Assert.Empty(asyncPage.Items);
            Assert.Equal(25, asyncPage.TotalCount);
        }

        [Fact]
        public async Task TestPageTotalRespectsTheCriteria()
        {
            Seed(12, "D2");
            foreach (var index in new[] { 0, 5 })
            {
                Func<ICriteria, ICriteria> criteria = c => c.Add(Restrictions.Equal("Department", "D1"));
                Assert.Equal(25, repository.FindPage(criteria, PageRequest.Of(index, 10)).TotalCount);
                Assert.Equal(25, (await repository.FindPageAsync(criteria, PageRequest.Of(index, 10))).TotalCount);
            }
        }

        [Fact]
        public void TestFilterWithoutResults()
        {
            var page = repository.FindPage(c => c.Add(Restrictions.Equal("Department", "missing")), PageRequest.Of(0, 10));
            Assert.Empty(page.Items);
            Assert.Equal(0, page.TotalCount);
            Assert.Equal(0, page.TotalPages);
        }

        [Fact]
        public void TestPageSizeOne()
        {
            List<int> ids = [];
            for (var i = 0; i < 25; i++)
            {
                var page = repository.FindPage(c => c, PageRequest.Of(i, 1));
                ids.Add(Assert.Single(page.Items).Id);
                Assert.Equal(25, page.TotalPages);
            }
            Assert.Equal(25, ids.Distinct().Count());
        }

        [Fact]
        public void TestRepeatedNamesWithOrderByNameCoverAllRowsOnce()
        {
            var ids = Enumerable.Range(0, 3).SelectMany(i => repository.FindSlice(c => c,
                PageRequest.Of(i, 10, Order.Asc("Name"))).Items.Select(p => p.Id)).ToList();
            Assert.Equal(25, ids.Count);
            Assert.Equal(25, ids.Distinct().Count());
        }

        [Fact]
        public void TestMethodNameOrderThenPageOrders()
        {
            var result = repository.FindByDepartmentOrderByNameAsc("D1",
                PageRequest.Of(0, 25, Order.Desc("Name"), Order.Desc("Ean")));
            var expected = connection.Find<Product>().OrderBy(p => p.Name).ThenByDescending(p => p.Ean).Select(p => p.Id);
            Assert.Equal(expected, result.Items.Select(p => p.Id));
        }

        [Fact]
        public void TestEmptyCriteriaPagesWholeTable()
        {
            Assert.Equal(25, repository.FindPage(c => c, PageRequest.Of(0, 30)).TotalCount);
        }

        [Fact]
        public async Task TestNullPageRequestFailsOnTheRepository()
        {
            Assert.Throws<ArgumentException>(() => direct.FindSlice(c => c, null));
            Assert.Throws<ArgumentException>(() => direct.FindPage(c => c, null));
            await Assert.ThrowsAsync<ArgumentException>(() => direct.FindSliceAsync(c => c, null));
            await Assert.ThrowsAsync<ArgumentException>(() => direct.FindPageAsync(c => c, null));
        }

        [Fact]
        public void TestNullPageRequestFailsThroughTheProxy()
        {
            var exception = Assert.Throws<TargetInvocationException>(() => repository.FindByDepartmentOrderByNameAsc("D1", null));
            Assert.IsType<ArgumentException>(exception.GetBaseException());
        }

        [Fact]
        public async Task TestOffsetBeyondIntMaxIsRejectedOnTheRepository()
        {
            var page = PageRequest.Of(int.MaxValue, 10);
            Assert.Throws<ArgumentException>(() => direct.FindSlice(c => c, page));
            Assert.Throws<ArgumentException>(() => direct.FindPage(c => c, page));
            await Assert.ThrowsAsync<ArgumentException>(() => direct.FindSliceAsync(c => c, page));
            await Assert.ThrowsAsync<ArgumentException>(() => direct.FindPageAsync(c => c, page));
        }

        [Fact]
        public void TestOffsetBeyondIntMaxIsRejectedThroughTheProxy()
        {
            var exception = Assert.Throws<TargetInvocationException>(() =>
                repository.FindByDepartmentOrderByNameAsc("D1", PageRequest.Of(int.MaxValue, 10)));
            Assert.IsType<ArgumentException>(exception.GetBaseException());
        }

        [Fact]
        public void TestCustomPageRequestWithInvalidValuesIsRejected()
        {
            Assert.Throws<ArgumentException>(() => direct.FindSlice(c => c,
                new CustomPageRequest(0, 0, 0, [])));
            Assert.Throws<ArgumentException>(() => direct.FindPage(c => c,
                new CustomPageRequest(2, 10, 5, [])));
        }

        [Theory]
        [InlineData("Nope")]
        [InlineData("Name; DROP TABLE Products")]
        [InlineData("Name DESC, (SELECT 1)")]
        public async Task TestUnknownOrderPropertyFails(string propertyName)
        {
            var page = PageRequest.Of(0, 10, Order.Asc(propertyName));
            var message = $"Cannot order Product by unknown property '{propertyName}'.";
            Assert.Equal(message, Assert.Throws<InvalidOperationException>(() => direct.FindSlice(c => c, page)).Message);
            Assert.Equal(message, Assert.Throws<InvalidOperationException>(() => direct.FindPage(c => c, page)).Message);
            Assert.Equal(message, (await Assert.ThrowsAsync<InvalidOperationException>(() => direct.FindSliceAsync(c => c, page))).Message);
            Assert.Equal(message, (await Assert.ThrowsAsync<InvalidOperationException>(() => direct.FindPageAsync(c => c, page))).Message);
            Assert.Equal(25, direct.FindPage(c => c, PageRequest.Of(0, 10)).TotalCount);
        }

        [Fact]
        public async Task TestQueryMethodWithPageRequestAndTokenMatchesFindSlice()
        {
            using var source = new CancellationTokenSource();
            var request = PageRequest.Of(1, 10, Order.Desc("Ean"));
            var named = await repository.FindByDepartmentAsync("D1", request, source.Token);
            var criteria = await repository.FindPageAsync(c => c.Add(Restrictions.Equal("Department", "D1")), request, source.Token);
            Assert.Equal(criteria.Items.Select(p => p.Id), named.Items.Select(p => p.Id));
        }

        [Fact]
        public async Task TestCancelledTokenIsNotObserved()
        {
            using var source = new CancellationTokenSource();
            source.Cancel();
            var slice = await repository.FindSliceAsync(c => c, PageRequest.Of(0, 10), source.Token);
            Assert.Equal(10, slice.Items.Count);
            var page = await repository.FindByDepartmentAsync("D1", PageRequest.Of(0, 10), source.Token);
            Assert.Equal(25, page.TotalCount);
        }

        [Fact]
        public void TestSliceRunsOneCommandAndPageDeducesTheTotal()
        {
            Assert.Equal(25, repository.FindPage(c => c, PageRequest.Of(2, 10)).TotalCount);
            Assert.Equal(25, repository.FindPage(c => c, PageRequest.Of(0, 10)).TotalCount);
        }

        public void Dispose() => connection.BulkDelete<Product>();
    }
}
