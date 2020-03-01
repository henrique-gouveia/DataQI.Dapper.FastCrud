using System.Collections.Generic;
using System.Data;

using Dapper.FastCrud;
using DataQI.Dapper.FastCrud.Repository.Support;

namespace DataQI.Dapper.FastCrud.Test.Repository.Products
{
    public class ProductRepository : DapperRepository<Product>, IProductRepository
    {
        public ProductRepository(IDbConnection connection) : base(connection)
        {
        }

        public IEnumerable<Product> FindByEanLike(string ean)
        {
            var products = connection.Find<Product>(statement => statement
                .Where($"{nameof(Product.Ean):C} LIKE @ean")
                .WithParameters(new { ean }));

            return products;
        }

        public IEnumerable<Product> FindByIdOrEanOrReference(int id, string ean, string reference)
        {
            var products = connection.Find<Product>(statement => statement
                .Where($"{nameof(Product.Id):C} = @id OR {nameof(Product.Ean):C} = @ean OR {nameof(Product.Reference):C} = @reference")
                .WithParameters(new { id, ean, reference }));

            return products;
        }

        public IEnumerable<Product> FindByNameLikeAndStockGreaterThan(string name, decimal stock = 0)
        {
            var products = connection.Find<Product>(statement => statement
                .Where($"{nameof(Product.Name):C} LIKE @name AND {nameof(Product.Stock):C} > @stock")
                .WithParameters(new { name, stock }));

            return products;
        }

        public IEnumerable<Product> FindByDepartmentInAndNameLike(string[] departments, string name)
        {
            var products = connection.Find<Product>(statement => statement
                .Where($"{nameof(Product.Department):C} IN @departments AND {nameof(Product.Name):C} LIKE @name")
                .WithParameters(new { name, departments }));

            return products;
        }

        public IEnumerable<Product> FindByKeywordsLikeAndActive(string keywords, bool active = true)
        {
            var products = connection.Find<Product>(statement => statement
                .Where($"{nameof(Product.Keywords):C} LIKE @keywords AND {nameof(Product.Active):C} == @active")
                .WithParameters(new { keywords, active }));

            return products;
        }
    }
}