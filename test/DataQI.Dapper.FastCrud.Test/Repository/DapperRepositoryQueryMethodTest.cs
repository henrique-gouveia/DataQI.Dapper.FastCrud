using System;
using System.Collections.Generic;
using System.Data;
using System.Linq;

using ExpectedObjects;
using Xunit;

using Dapper.FastCrud;

using DataQI.Dapper.FastCrud.Test.Fixtures;
using DataQI.Dapper.FastCrud.Test.Repository.Products;

namespace DataQI.Dapper.FastCrud.Test.Repository
{
    public sealed class DapperRepositoryQueryMethodTest : IClassFixture<DbFixture>, IDisposable
    {
        private readonly IDbConnection connection;
        private readonly IProductRepository productRepository;

        public DapperRepositoryQueryMethodTest(DbFixture fixture)
        {
            connection = fixture.Connection;
            productRepository = fixture.ProductRepository;
        }

        [Fact]
        public void TestFindByEanLike()
        {
            var productsExpected = InsertTestProducts();

            while (productsExpected.MoveNext())
            {
                var productExpected = productsExpected.Current;
                var products = productRepository.FindByEanLike($"{productExpected.Ean}%");

                productExpected.ToExpectedObject().ShouldMatch(products.FirstOrDefault());
            }
        }

        [Fact]
        public void TestFindByIdOrEanOrReference()
        {
            var productsExpected = InsertTestProducts();

            while (productsExpected.MoveNext())
            {
                var productExpected = productsExpected.Current;

                var productById = productRepository.FindByIdOrEanOrReference(productExpected.Id, "", "");
                var productByEan = productRepository.FindByIdOrEanOrReference(0, productExpected.Ean, "");
                var productByReference = productRepository.FindByIdOrEanOrReference(0, "", productExpected.Reference);

                productExpected.ToExpectedObject().ShouldMatch(productById.FirstOrDefault());
                productExpected.ToExpectedObject().ShouldMatch(productByEan.FirstOrDefault());
                productExpected.ToExpectedObject().ShouldMatch(productByReference.FirstOrDefault());
            }
        }

        [Fact]
        public void TestFindByNameLikeAndStockGreaterThan()
        {
            var productList = InsertTestProductsList();
            using var productEnumerator = productList.GetEnumerator();

            while (productEnumerator.MoveNext())
            {
                var product = productEnumerator.Current;
                var productNameStartsWith = product.Name.Substring(0, 5);

                var productsExpected = productList.Where(p => p.Name.StartsWith(productNameStartsWith) && p.Stock > 0);
                var products = productRepository.FindByNameLikeAndStockGreaterThan($"{productNameStartsWith}%", 0);

                productsExpected.ToExpectedObject().ShouldMatch(products);
            }
        }

        [Fact]
        public void TestFindByDepartamentInAndNameLike()
        {
            var productList = InsertTestProductsList();

            var departments = productList.Select(p => p.Department);
            using var productEnumerator = productList.GetEnumerator();

            while (productEnumerator.MoveNext())
            {
                var product = productEnumerator.Current;
                var productNameStartsWith = product.Name.Substring(0, 5);

                var productsExpected = productList.Where(p =>
                    departments.Any(d => d == p.Department) 
                    && p.Name.StartsWith(productNameStartsWith));
                var products = productRepository.FindByDepartmentInAndNameLike(departments.ToArray(), $"{productNameStartsWith}%");

                productsExpected.ToExpectedObject().ShouldMatch(products);
            }
        }

        [Fact]
        public void TestFindByKeywordsLikeAndActive()
        {
            var productList = InsertTestProductsList();
            using var productEnumerator = productList.GetEnumerator();

            while (productEnumerator.MoveNext())
            {
                var product = productEnumerator.Current;
                var productsExpected = productList.Where(p =>
                    p.Active == product.Active && p.Keywords.Contains(product.Keywords));
                var products = productRepository.FindByKeywordsLikeAndActive($"%{product.Keywords}%", product.Active);

                productsExpected.ToExpectedObject().ShouldMatch(products);
            }
        }

        private IEnumerator<Product> InsertTestProducts()
        {
            var Products = InsertTestProductsList();
            return Products.GetEnumerator();
        }

        private IList<Product> InsertTestProductsList()
        {
            var Products = new List<Product>()
            {
                ProductBuilder.NewInstance().Build(),
                ProductBuilder.NewInstance().Build(),
                ProductBuilder.NewInstance().Build(),
                ProductBuilder.NewInstance().Build(),
                ProductBuilder.NewInstance().Build(),
            };

            Products.ForEach(p =>
            {
                productRepository.Save(p);
                Assert.True(productRepository.Exists(p));
            });

            return Products;
        }

        #region IDisposable Support
        private bool disposedValue = false;

        private void Dispose(bool disposing)
        {
            if (!disposedValue)
            {
                connection.BulkDelete<Product>();
                disposedValue = true;
            }
        }

        public void Dispose()
        {
            Dispose(true);
        }
        #endregion

    }
}