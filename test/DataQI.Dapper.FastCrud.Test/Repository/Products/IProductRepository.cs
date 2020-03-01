using System.Collections.Generic;
using DataQI.Dapper.FastCrud.Repository;

namespace DataQI.Dapper.FastCrud.Test.Repository.Products
{
    public interface IProductRepository : IDapperRepository<Product>
    {
        IEnumerable<Product> FindByEanLike(string ean);

        IEnumerable<Product> FindByIdOrEanOrReference(int id, string ean, string reference);

        IEnumerable<Product> FindByNameLikeAndStockGreaterThan(string name, decimal stock = 0);

        IEnumerable<Product> FindByDepartmentInAndNameLike(string[] departments, string name);
        
        IEnumerable<Product> FindByKeywordsLikeAndActive(string keywords, bool active = true);
    }
}