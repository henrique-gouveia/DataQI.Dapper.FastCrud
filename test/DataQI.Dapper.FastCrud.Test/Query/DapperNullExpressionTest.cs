using System;

using DataQI.Commons.Query.Support;

using DataQI.Dapper.FastCrud.Query;
using DataQI.Dapper.FastCrud.Query.Extensions;
using DataQI.Dapper.FastCrud.Query.Support;
using DataQI.Dapper.FastCrud.Test.Fixtures;

using Xunit;

namespace DataQI.Dapper.FastCrud.Test.Query
{
    public class DapperNullExpressionTest: IClassFixture<QueryFixture>
    {
        private readonly IDapperCommandBuilder commandBuilder;

        public DapperNullExpressionTest(QueryFixture fixture)
        {
            commandBuilder = fixture.GetCommandBuilder();
        }

        [Fact]
        public void TestRejectsNullCriterion()
        {
            var exception = Assert.Throws<ArgumentException>(() => 
                new DapperNullExpression(null));
            var exceptionMessage = exception.GetBaseException().Message;

            Assert.IsType<ArgumentException>(exception.GetBaseException());
            Assert.Equal("NullExpression must not be null", exceptionMessage);
        }

        [Fact]
        public void TestBuildNullExpressionCorrectly()
        {
            var criterion = Restrictions.Null("Email");
            Assert.Equal("Email Is Null", criterion.GetExpressionBuilder().Build(commandBuilder));
        }          
    }
}