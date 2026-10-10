using System.Collections.Generic;
using System.Runtime.CompilerServices;

using Dapper.FastCrud;

using DataQI.Commons.Query;
using DataQI.Commons.Query.Support;
using DataQI.Dapper.FastCrud.Query.Support;

using Xunit;

namespace DataQI.Dapper.FastCrud.Test.Query
{
    public class DapperOrderByBuilderTest : DapperExpressionTestBase
    {
        [Fact]
        public void TestBuildsAscendingAndDescendingInCallSequence()
        {
            var orders = new List<IOrderCriterion> { Order.Asc("FirstName"), Order.Desc("LastName") };

            var orderBy = DapperOrderByBuilder.BuildOrderBy(orders);

            AssertExpression(
                FormattableStringFactory.Create("{0} ASC, {1} DESC", Sql.Column("FirstName"), Sql.Column("LastName")),
                orderBy);
        }

        [Fact]
        public void TestBuildsASingleOrder()
        {
            var orderBy = DapperOrderByBuilder.BuildOrderBy(new List<IOrderCriterion> { Order.Desc("Stock") });

            AssertExpression(FormattableStringFactory.Create("{0} DESC", Sql.Column("Stock")), orderBy);
        }

        [Fact]
        public void TestReturnsNullWhenThereAreNoOrders()
        {
            Assert.Null(DapperOrderByBuilder.BuildOrderBy(new List<IOrderCriterion>()));
        }
    }
}
