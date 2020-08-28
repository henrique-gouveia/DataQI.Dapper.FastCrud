using System;

using DataQI.Commons.Query.Support;

using DataQI.Dapper.FastCrud.Query;
using DataQI.Dapper.FastCrud.Query.Extensions;
using DataQI.Dapper.FastCrud.Query.Support;
using DataQI.Dapper.FastCrud.Test.Fixtures;

using Xunit;

namespace DataQI.Dapper.FastCrud.Test.Query
{
    public class DapperJunctionExpressionTest : IClassFixture<QueryFixture>
    {
        private readonly IDapperCommandBuilder commandBuilder;

        public DapperJunctionExpressionTest(QueryFixture fixture)
        {
            commandBuilder = fixture.GetCommandBuilder();
        }

        [Fact]
        public void TestRejectsNullJunction()
        {
            var exception = Assert.Throws<ArgumentException>(() => 
                new DapperJunctionExpression(null));
            var exceptionMessage = exception.GetBaseException().Message;

            Assert.IsType<ArgumentException>(exception.GetBaseException());
            Assert.Equal("Junction must not be null", exceptionMessage);
        }

        [Fact]
        public void TestBuildConjunctionSimpleExpressionCorrectly()
        {
            var junction = Restrictions.Conjunction();
            junction.Add(Restrictions.Equal("FirstName", "Fake Name"));

            Assert.Equal("(FirstName = @0)", junction.GetExpressionBuilder().Build(commandBuilder));
        }        

        [Fact]
        public void TestBuildConjunctionComposedExpressionsCorrectly()
        {
            var junction = Restrictions.Conjunction();
            junction
                .Add(Restrictions.Equal("FirstName", "Fake Name"))
                .Add(Restrictions.Equal("LastName", "Fake Name"));

            Assert.Equal("(FirstName = @0 AND LastName = @1)", junction.GetExpressionBuilder().Build(commandBuilder));
        }
 
        [Fact]
        public void TestBuildConjunctionsSimpleExpressionCorrectly()
        {
            var junction = Restrictions.Conjunction();
            junction
                .Add(Restrictions
                    .Conjunction()
                    .Add(Restrictions.Equal("FirstName", "Fake Name")))
                .Add(Restrictions
                    .Conjunction()
                    .Add(Restrictions.Equal("LastName", "Fake Name")));

            Assert.Equal("((FirstName = @0) AND (LastName = @1))", junction.GetExpressionBuilder().Build(commandBuilder));
        }

        [Fact]
        public void TestBuildDisjunctionSimpleExpressionCorrectly()
        {
            var junction = Restrictions.Disjunction();
            junction.Add(Restrictions.Equal("FirstName", "Fake Name"));

            Assert.Equal("(FirstName = @0)", junction.GetExpressionBuilder().Build(commandBuilder));
        }

        [Fact]
        public void TestBuildDisjunctionComposedExpressionsCorrectly()
        {
            var junction = Restrictions.Disjunction();
            junction
                .Add(Restrictions.Equal("FirstName", "Fake Name"))
                .Add(Restrictions.Equal("LastName", "Fake Name"));

            Assert.Equal("(FirstName = @0 OR LastName = @1)", junction.GetExpressionBuilder().Build(commandBuilder));
        }        
 
        [Fact]
        public void TestBuildDisjunctionsSimpleExpressionCorrectly()
        {
            var junction = Restrictions.Disjunction();
            junction
                .Add(Restrictions
                    .Disjunction()
                    .Add(Restrictions.Equal("FirstName", "Fake Name")))
                .Add(Restrictions
                    .Disjunction()
                    .Add(Restrictions.Equal("LastName", "Fake Name")));

            Assert.Equal("((FirstName = @0) OR (LastName = @1))", junction.GetExpressionBuilder().Build(commandBuilder));
        }
    }
}
