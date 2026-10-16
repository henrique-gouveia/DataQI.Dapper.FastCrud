using System.Collections.Generic;
using System.Threading;
using System.Threading.Tasks;

using DataQI.Dapper.FastCrud.Repository;
using DataQI.Commons.Repository.Paging;

namespace DataQI.Dapper.FastCrud.Test.Repository.Products
{
    public interface IProductRepository : IDapperRepository<Product>
    {
        ISlice<Product> FindByDepartmentOrderByNameAsc(string department, IPageRequest page);
        Task<IPage<Product>> FindByDepartmentAsync(string department, IPageRequest page, CancellationToken cancellationToken = default);

        IEnumerable<Product> FindByEanLike(string ean);

        Product FindByEan(string ean);

        Task<Product> FindByEanAsync(string ean);

        IEnumerable<Product> FindByIdOrEanOrReference(int id, string ean, string reference);

        IEnumerable<Product> FindByNameLikeAndStockGreaterThan(string name, decimal stock = 0);

        IEnumerable<Product> FindByDepartmentInAndNameLike(string[] departments, string name);
        
        IEnumerable<Product> FindByKeywordsLikeAndActive(string keywords, bool active = true);

        Task<IEnumerable<Product>> FindByEanLikeAsync(string ean);

        Task<IEnumerable<Product>> FindByEanLikeAsync(string ean, CancellationToken cancellationToken);
    }
}
