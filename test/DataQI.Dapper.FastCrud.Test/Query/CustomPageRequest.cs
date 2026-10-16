using System.Collections.Generic;
using DataQI.Commons.Query;
using DataQI.Commons.Repository.Paging;

namespace DataQI.Dapper.FastCrud.Test.Query
{
    public sealed class CustomPageRequest : IPageRequest
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
