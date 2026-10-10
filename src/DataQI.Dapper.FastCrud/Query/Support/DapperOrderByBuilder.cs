using System;
using System.Collections.Generic;
using System.Linq;
using System.Runtime.CompilerServices;

using Dapper.FastCrud;

using DataQI.Commons.Query;
using DataQI.Commons.Query.Support;

namespace DataQI.Dapper.FastCrud.Query.Support
{
    internal static class DapperOrderByBuilder
    {
        public static FormattableString BuildOrderBy(IReadOnlyCollection<IOrderCriterion> orders)
        {
            if (orders.Count == 0)
                return null;

            var columns = orders.Select(order => (object)Sql.Column(order.PropertyName)).ToArray();
            var fragments = orders.Select((order, index) =>
                $"{{{index}}} {(order.Direction == OrderDirection.Asc ? "ASC" : "DESC")}");

            return FormattableStringFactory.Create(string.Join(", ", fragments), columns);
        }
    }
}
