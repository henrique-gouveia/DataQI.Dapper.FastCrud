using System;

using DataQI.Commons.Query.Support;

using DataQI.Dapper.FastCrud.Query;
using DataQI.Dapper.FastCrud.Query.Extensions;
using DataQI.Dapper.FastCrud.Query.Support;
using DataQI.Dapper.FastCrud.Test.Fixtures;

using Xunit;

namespace DataQI.Dapper.FastCrud.Test.Query
{
    public class DapperInExpressionTest : IClassFixture<QueryFixture>
    {
        private readonly IDapperCommandBuilder commandBuilder;

        public DapperInExpressionTest(QueryFixture fixture)
        {
            commandBuilder = fixture.GetCommandBuilder();
        }

        [Fact]
        public void TestRejectsNullCriterion()
        {
            var exception = Assert.Throws<ArgumentException>(() => 
                new DapperInExpression(null));
            var exceptionMessage = exception.GetBaseException().Message;

            Assert.IsType<ArgumentException>(exception.GetBaseException());
            Assert.Equal("InExpression must not be null", exceptionMessage);
        }

        [Fact]
        public void TestBuildInExpressionCorrectly()
        {
            var criterion = Restrictions.In("FirstName", new string[] { "Fake Name A", "Fake Name B", "Fake Name C" });
            Assert.Equal("FirstName In @0", criterion.GetExpressionBuilder().Build(commandBuilder));
        }  
    }
}