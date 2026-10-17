using System;
using System.Collections.Generic;
using System.Data;
using System.Linq;
using System.Reflection;
using System.Threading;
using System.Threading.Tasks;

using Dapper.FastCrud;
using Xunit;

using DataQI.Commons.Query;
using DataQI.Commons.Query.Support;
using DataQI.Commons.Repository.Paging;
using DataQI.Commons.Repository.Paging.Support;

using DataQI.Dapper.FastCrud.Repository.Support;
using DataQI.Dapper.FastCrud.Test.Fixtures;
using DataQI.Dapper.FastCrud.Test.Repository.Products;

namespace DataQI.Dapper.FastCrud.Test.Repository
{
    public sealed class DapperRepositoryPagingTest : IClassFixture<DbFixture>, IDisposable
    {
        private readonly IDbConnection connection;
        private readonly IProductRepository productRepository;
        private readonly DapperRepository<Product> directRepository;

        public DapperRepositoryPagingTest(DbFixture fixture)
        {
            connection = fixture.Connection;
            productRepository = fixture.ProductRepository;
            directRepository = new DapperRepository<Product>(connection);
            InsertProducts(25);
        }

        [Theory]
        [InlineData(0, 10, true, false)]
        [InlineData(2, 5, false, true)]
        [InlineData(5, 0, false, true)]
        public void TestFindSliceReturnsRequestedPage(int pageIndex, int itemCount, bool hasNext, bool hasPrevious)
        {
            var request = PageRequest.Of(pageIndex, 10);

            var slice = productRepository.FindSlice(c => c, request);

            Assert.Equal(itemCount, slice.Items.Count);
            Assert.Equal(hasNext, slice.HasNext);
            Assert.Equal(hasPrevious, slice.HasPrevious);
        }

        [Theory]
        [InlineData(0, 10, true, false)]
        [InlineData(2, 5, false, true)]
        [InlineData(5, 0, false, true)]
        public async Task TestFindSliceAsyncReturnsRequestedPage(int pageIndex, int itemCount, bool hasNext, bool hasPrevious)
        {
            var request = PageRequest.Of(pageIndex, 10);

            var slice = await productRepository.FindSliceAsync(c => c, request);

            Assert.Equal(itemCount, slice.Items.Count);
            Assert.Equal(hasNext, slice.HasNext);
            Assert.Equal(hasPrevious, slice.HasPrevious);
        }

        [Theory]
        [InlineData(0, 10, true)]
        [InlineData(2, 5, false)]
        [InlineData(5, 0, false)]
        public void TestFindPageReturnsRequestedPage(int pageIndex, int itemCount, bool hasNext)
        {
            var request = PageRequest.Of(pageIndex, 10);

            var page = productRepository.FindPage(c => c, request);

            Assert.Equal(itemCount, page.Items.Count);
            Assert.Equal(hasNext, page.HasNext);
        }

        [Theory]
        [InlineData(0, 10, true)]
        [InlineData(2, 5, false)]
        [InlineData(5, 0, false)]
        public async Task TestFindPageAsyncReturnsRequestedPage(int pageIndex, int itemCount, bool hasNext)
        {
            var request = PageRequest.Of(pageIndex, 10);

            var page = await productRepository.FindPageAsync(c => c, request);

            Assert.Equal(itemCount, page.Items.Count);
            Assert.Equal(hasNext, page.HasNext);
        }

        [Fact]
        public void TestFindSliceHasNoNextPageWhenLastPageIsFull()
        {
            ReplaceProducts(20);

            var slice = productRepository.FindSlice(c => c, PageRequest.Of(1, 10));

            Assert.Equal(10, slice.Items.Count);
            Assert.False(slice.HasNext);
        }

        [Fact]
        public async Task TestFindSliceAsyncHasNoNextPageWhenLastPageIsFull()
        {
            ReplaceProducts(20);

            var slice = await productRepository.FindSliceAsync(c => c, PageRequest.Of(1, 10));

            Assert.Equal(10, slice.Items.Count);
            Assert.False(slice.HasNext);
        }

        [Fact]
        public void TestFindPageHasNoNextPageWhenLastPageIsFull()
        {
            ReplaceProducts(20);

            var page = productRepository.FindPage(c => c, PageRequest.Of(1, 10));

            Assert.Equal(10, page.Items.Count);
            Assert.False(page.HasNext);
            Assert.Equal(20, page.TotalCount);
        }

        [Fact]
        public async Task TestFindPageAsyncHasNoNextPageWhenLastPageIsFull()
        {
            ReplaceProducts(20);

            var page = await productRepository.FindPageAsync(c => c, PageRequest.Of(1, 10));

            Assert.Equal(10, page.Items.Count);
            Assert.False(page.HasNext);
            Assert.Equal(20, page.TotalCount);
        }

        [Theory]
        [InlineData(0)]
        [InlineData(2)]
        [InlineData(5)]
        public void TestFindPageReportsTotalRegardlessOfPageIndex(int pageIndex)
        {
            var page = productRepository.FindPage(c => c, PageRequest.Of(pageIndex, 10));

            Assert.Equal(25, page.TotalCount);
            Assert.Equal(3, page.TotalPages);
        }

        [Theory]
        [InlineData(0)]
        [InlineData(2)]
        [InlineData(5)]
        public async Task TestFindPageAsyncReportsTotalRegardlessOfPageIndex(int pageIndex)
        {
            var page = await productRepository.FindPageAsync(c => c, PageRequest.Of(pageIndex, 10));

            Assert.Equal(25, page.TotalCount);
            Assert.Equal(3, page.TotalPages);
        }

        [Theory]
        [InlineData(0)]
        [InlineData(2)]
        [InlineData(5)]
        public void TestFindPageTotalCountsOnlyMatchingProducts(int pageIndex)
        {
            InsertProducts(12, "D2");
            var request = PageRequest.Of(pageIndex, 10);

            var page = productRepository.FindPage(c => c.Add(Restrictions.Equal(nameof(Product.Department), "D1")), request);

            Assert.Equal(25, page.TotalCount);
        }

        [Theory]
        [InlineData(0)]
        [InlineData(2)]
        [InlineData(5)]
        public async Task TestFindPageAsyncTotalCountsOnlyMatchingProducts(int pageIndex)
        {
            InsertProducts(12, "D2");
            var request = PageRequest.Of(pageIndex, 10);

            var page = await productRepository.FindPageAsync(c => c.Add(Restrictions.Equal(nameof(Product.Department), "D1")), request);

            Assert.Equal(25, page.TotalCount);
        }

        [Fact]
        public void TestFindPageReturnsEmptyResultWhenNoProductsMatch()
        {
            var request = PageRequest.Of(0, 10);

            var page = productRepository.FindPage(c => c.Add(Restrictions.Equal(nameof(Product.Department), "Missing")), request);

            Assert.Empty(page.Items);
            Assert.Equal(0, page.TotalCount);
            Assert.Equal(0, page.TotalPages);
        }

        [Fact]
        public async Task TestFindPageAsyncReturnsEmptyResultWhenNoProductsMatch()
        {
            var request = PageRequest.Of(0, 10);

            var page = await productRepository.FindPageAsync(c => c.Add(Restrictions.Equal(nameof(Product.Department), "Missing")), request);

            Assert.Empty(page.Items);
            Assert.Equal(0, page.TotalCount);
            Assert.Equal(0, page.TotalPages);
        }

        [Fact]
        public void TestFindPageWithSizeOneReturnsEveryProductOnce()
        {
            var expectedIds = connection.Find<Product>().OrderBy(p => p.Id).Select(p => p.Id).ToArray();

            var pages = Enumerable.Range(0, 25)
                .Select(index => productRepository.FindPage(c => c, PageRequest.Of(index, 1)))
                .ToArray();

            Assert.Equal(expectedIds, pages.Select(page => Assert.Single(page.Items).Id));
            Assert.All(pages, page => Assert.Equal(25, page.TotalPages));
        }

        [Fact]
        public async Task TestFindPageAsyncWithSizeOneReturnsEveryProductOnce()
        {
            var expectedIds = connection.Find<Product>().OrderBy(p => p.Id).Select(p => p.Id).ToArray();
            List<IPage<Product>> pages = [];

            for (var index = 0; index < 25; index++)
                pages.Add(await productRepository.FindPageAsync(c => c, PageRequest.Of(index, 1)));

            Assert.Equal(expectedIds, pages.Select(page => Assert.Single(page.Items).Id));
            Assert.All(pages, page => Assert.Equal(25, page.TotalPages));
        }

        [Fact]
        public void TestFindSliceUsesPrimaryKeyToBreakOrderingTies()
        {
            var expectedIds = connection.Find<Product>().OrderBy(p => p.Name).ThenBy(p => p.Id).Select(p => p.Id).ToArray();

            var actualIds = Enumerable.Range(0, 3)
                .SelectMany(index => productRepository.FindSlice(c => c,
                    PageRequest.Of(index, 10, Order.Asc(nameof(Product.Name)))).Items)
                .Select(p => p.Id);

            Assert.Equal(expectedIds, actualIds);
        }

        [Fact]
        public void TestQueryMethodOrderingTakesPrecedenceOverRequestOrdering()
        {
            var request = PageRequest.Of(0, 25, Order.Desc(nameof(Product.Name)), Order.Desc(nameof(Product.Ean)));
            var expectedIds = connection.Find<Product>().OrderBy(p => p.Name).ThenByDescending(p => p.Ean).Select(p => p.Id).ToArray();

            var slice = productRepository.FindByDepartmentOrderByNameAsc("D1", request);

            Assert.Equal(expectedIds, slice.Items.Select(p => p.Id));
        }

        [Fact]
        public void TestFindPageWithEmptyCriteriaReturnsAllProducts()
        {
            var page = productRepository.FindPage(c => c, PageRequest.Of(0, 30));

            Assert.Equal(25, page.Items.Count);
            Assert.Equal(25, page.TotalCount);
        }

        [Theory]
        [MemberData(nameof(InvalidPageRequests))]
        public void TestFindSliceRejectsInvalidPageRequest(IPageRequest request)
        {
            Assert.Throws<ArgumentException>(() => directRepository.FindSlice(c => c, request));
        }

        [Theory]
        [MemberData(nameof(InvalidPageRequests))]
        public async Task TestFindSliceAsyncRejectsInvalidPageRequest(IPageRequest request)
        {
            await Assert.ThrowsAsync<ArgumentException>(() => directRepository.FindSliceAsync(c => c, request));
        }

        [Theory]
        [MemberData(nameof(InvalidPageRequests))]
        public void TestFindPageRejectsInvalidPageRequest(IPageRequest request)
        {
            Assert.Throws<ArgumentException>(() => directRepository.FindPage(c => c, request));
        }

        [Theory]
        [MemberData(nameof(InvalidPageRequests))]
        public async Task TestFindPageAsyncRejectsInvalidPageRequest(IPageRequest request)
        {
            await Assert.ThrowsAsync<ArgumentException>(() => directRepository.FindPageAsync(c => c, request));
        }

        [Fact]
        public void TestQueryMethodRejectsNullPageRequest()
        {
            var exception = Assert.Throws<TargetInvocationException>(() =>
                productRepository.FindByDepartmentOrderByNameAsc("D1", null));

            Assert.IsType<ArgumentException>(exception.GetBaseException());
        }

        [Fact]
        public void TestQueryMethodRejectsOffsetBeyondIntMax()
        {
            var request = PageRequest.Of(int.MaxValue, 10);

            var exception = Assert.Throws<TargetInvocationException>(() =>
                productRepository.FindByDepartmentOrderByNameAsc("D1", request));

            Assert.IsType<ArgumentException>(exception.GetBaseException());
        }

        [Theory]
        [InlineData("Nope")]
        [InlineData("Name; DROP TABLE Products")]
        [InlineData("Name DESC, (SELECT 1)")]
        public void TestFindSliceRejectsUnknownOrderProperty(string propertyName)
        {
            var request = PageRequest.Of(0, 10, Order.Asc(propertyName));

            var exception = Assert.Throws<InvalidOperationException>(() => directRepository.FindSlice(c => c, request));

            Assert.Equal($"Cannot order Product by unknown property '{propertyName}'.", exception.Message);
        }

        [Theory]
        [InlineData("Nope")]
        [InlineData("Name; DROP TABLE Products")]
        [InlineData("Name DESC, (SELECT 1)")]
        public async Task TestFindSliceAsyncRejectsUnknownOrderProperty(string propertyName)
        {
            var request = PageRequest.Of(0, 10, Order.Asc(propertyName));

            var exception = await Assert.ThrowsAsync<InvalidOperationException>(() => directRepository.FindSliceAsync(c => c, request));

            Assert.Equal($"Cannot order Product by unknown property '{propertyName}'.", exception.Message);
        }

        [Theory]
        [InlineData("Nope")]
        [InlineData("Name; DROP TABLE Products")]
        [InlineData("Name DESC, (SELECT 1)")]
        public void TestFindPageRejectsUnknownOrderProperty(string propertyName)
        {
            var request = PageRequest.Of(0, 10, Order.Asc(propertyName));

            var exception = Assert.Throws<InvalidOperationException>(() => directRepository.FindPage(c => c, request));

            Assert.Equal($"Cannot order Product by unknown property '{propertyName}'.", exception.Message);
        }

        [Theory]
        [InlineData("Nope")]
        [InlineData("Name; DROP TABLE Products")]
        [InlineData("Name DESC, (SELECT 1)")]
        public async Task TestFindPageAsyncRejectsUnknownOrderProperty(string propertyName)
        {
            var request = PageRequest.Of(0, 10, Order.Asc(propertyName));

            var exception = await Assert.ThrowsAsync<InvalidOperationException>(() => directRepository.FindPageAsync(c => c, request));

            Assert.Equal($"Cannot order Product by unknown property '{propertyName}'.", exception.Message);
        }

        [Fact]
        public async Task TestQueryMethodWithTokenReturnsTheSamePageAsCriteria()
        {
            var request = PageRequest.Of(1, 10, Order.Desc(nameof(Product.Ean)));
            var expected = await productRepository.FindPageAsync(
                c => c.Add(Restrictions.Equal(nameof(Product.Department), "D1")), request);

            var actual = await productRepository.FindByDepartmentAsync("D1", request, CancellationToken.None);

            Assert.Equal(expected.Items.Select(p => p.Id), actual.Items.Select(p => p.Id));
        }

        [Fact]
        public async Task TestFindSliceAsyncIgnoresCancelledToken()
        {
            using var source = new CancellationTokenSource();
            source.Cancel();
            var request = PageRequest.Of(0, 10);

            var result = await productRepository.FindSliceAsync(c => c, request, source.Token);

            Assert.Equal(10, result.Items.Count);
        }

        [Fact]
        public async Task TestFindPageAsyncIgnoresCancelledToken()
        {
            using var source = new CancellationTokenSource();
            source.Cancel();
            var request = PageRequest.Of(0, 10);

            var result = await productRepository.FindPageAsync(c => c, request, source.Token);

            Assert.Equal(10, result.Items.Count);
        }

        [Fact]
        public async Task TestQueryMethodIgnoresCancelledToken()
        {
            using var source = new CancellationTokenSource();
            source.Cancel();

            var page = await productRepository.FindByDepartmentAsync("D1", PageRequest.Of(0, 10), source.Token);

            Assert.Equal(25, page.TotalCount);
        }

        [Theory]
        [InlineData("Name; DROP TABLE Products")]
        [InlineData("Name DESC, (SELECT 1)")]
        public void TestRejectedOrderLeavesProductsUnchanged(string propertyName)
        {
            var expectedIds = connection.Find<Product>().OrderBy(p => p.Id).Select(p => p.Id).ToArray();
            var request = PageRequest.Of(0, 10, Order.Asc(propertyName));

            Assert.Throws<InvalidOperationException>(() => directRepository.FindSlice(c => c, request));

            var actualIds = connection.Find<Product>().OrderBy(p => p.Id).Select(p => p.Id);
            Assert.Equal(expectedIds, actualIds);
        }

        public static IEnumerable<object[]> InvalidPageRequests()
        {
            yield return [null];
            yield return [PageRequest.Of(int.MaxValue, 10)];
            yield return [new CustomPageRequest(0, 0, 0, [])];
            yield return [new CustomPageRequest(2, 10, 5, [])];
        }

        public void Dispose() => connection.BulkDelete<Product>();

        private void ReplaceProducts(int count)
        {
            connection.BulkDelete<Product>();
            InsertProducts(count);
        }

        private void InsertProducts(int count, string department = "D1")
        {
            for (var index = 0; index < count; index++)
            {
                var product = ProductBuilder.NewInstance().Build();
                product.Department = department;
                product.Name = "Name" + index % 5;
                product.Ean = department + index.ToString("D4");
                connection.Insert(product);
            }
        }

        private sealed class CustomPageRequest : IPageRequest
        {
            public CustomPageRequest(int pageIndex, int pageSize, long offset, IReadOnlyList<IOrderCriterion> orders)
            {
                PageIndex = pageIndex;
                PageSize = pageSize;
                Offset = offset;
                Orders = orders;
            }

            public int PageIndex { get; }
            public int PageSize { get; }
            public long Offset { get; }
            public IReadOnlyList<IOrderCriterion> Orders { get; }
        }
    }
}
