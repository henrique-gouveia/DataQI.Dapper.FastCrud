using System;
using Dapper.FastCrud;
using DataQI.Commons.Query.Support;

using DataQI.Dapper.FastCrud.Query;
using DataQI.Dapper.FastCrud.Query.Extensions;
using DataQI.Dapper.FastCrud.Query.Support;
using DataQI.Dapper.FastCrud.Test.Fixtures;

using Xunit;

namespace DataQI.Dapper.FastCrud.Test.Query
{
    public class DapperBetweenExpressionTest : DapperExpressionTestBase, IClassFixture<QueryFixture>
    {
        private readonly IDapperCommandBuilder commandBuilder;

        public DapperBetweenExpressionTest(QueryFixture fixture)
        {
            commandBuilder = fixture.GetCommandBuilder();
        }

        [Fact]
        public void TestRejectsNullCriterion()
        {
            var exception = Assert.Throws<ArgumentException>(() => 
                new DapperBetweenExpression(null));
            var exceptionMessage = exception.GetBaseException().Message;

            Assert.IsType<ArgumentException>(exception.GetBaseException());
            Assert.Equal("BetweenExpression must not be null", exceptionMessage);
        }

        [Fact]
        public void TestBuildBetweenExpressionCorrectly()
        {
            var criterion = Restrictions.Between("DateOfBirth", DateTime.Now.AddYears(-1), DateTime.Now.AddYears(1));

            AssertExpression(
                $"{Sql.Column("DateOfBirth")} Between @{"0"} And @{"1"}",
                criterion.GetExpressionBuilder().Build(commandBuilder));
        }
    }
}