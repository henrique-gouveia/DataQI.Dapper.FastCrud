using System;

using DataQI.Commons.Query.Support;

using DataQI.Dapper.FastCrud.Query;
using DataQI.Dapper.FastCrud.Query.Extensions;
using DataQI.Dapper.FastCrud.Query.Support;
using DataQI.Dapper.FastCrud.Test.Fixtures;

using Xunit;

namespace DataQI.Dapper.FastCrud.Test.Query
{
    public class DapperNotExpressionTest : IClassFixture<QueryFixture>
    {
        private readonly IDapperCommandBuilder commandBuilder;

        public DapperNotExpressionTest(QueryFixture fixture)
        {
            commandBuilder = fixture.GetCommandBuilder();
        }
        
        [Fact]
        public void TestRejectsNullCriterion()
        {
            var exception = Assert.Throws<ArgumentException>(() => 
                new DapperNotExpression(null));
            var exceptionMessage = exception.GetBaseException().Message;

            Assert.IsType<ArgumentException>(exception.GetBaseException());
            Assert.Equal("NotExpression must not be null", exceptionMessage);
        }

        [Fact]
        public void TestBuildNotBetweenExpressionCorrectly()
        {
            var between = Restrictions.Between("DateOfBirth", DateTime.Now.AddYears(-1), DateTime.Now.AddYears(1));
            var notBetween = Restrictions.Not(between);

            Assert.Equal("DateOfBirth Not Between @0 And @1", notBetween.GetExpressionBuilder().Build(commandBuilder));
        }        

        [Fact]
        public void TestBuildNotContainingExpressionCorrectly()
        {
            var containing = Restrictions.Containing("FirstName", "Fake Name");
            var notContaining = Restrictions.Not(containing);

            Assert.Equal("FirstName Not Like @0", notContaining.GetExpressionBuilder().Build(commandBuilder));
        }

        [Fact]
        public void TestBuildNotEndingWithExpressionCorrectly()
        {
            var endingWith = Restrictions.EndingWith("LastName", "Fake Name");
            var notEndingWith = Restrictions.Not(endingWith);

            Assert.Equal("LastName Not Like @0", notEndingWith.GetExpressionBuilder().Build(commandBuilder));
        }

        [Fact]
        public void TestBuildNotStartingWithExpressionCorrectly()
        {
            var startingWith = Restrictions.StartingWith("LastName", "Fake Name");
            var notStartingWith = Restrictions.Not(startingWith);

            Assert.Equal("LastName Not Like @0", notStartingWith.GetExpressionBuilder().Build(commandBuilder));
        }

        [Fact]
        public void TestBuildNotEqualExpressionCorrectly()
        {
            var equal = Restrictions.Equal("FirstName", "Fake Name");
            var notEqual = Restrictions.Not(equal);

            Assert.Equal("FirstName != @0", notEqual.GetExpressionBuilder().Build(commandBuilder));
        }        

        [Fact]
        public void TestBuildNotInExpressionCorrectly()
        {
            var In = Restrictions.In("FirstName", new string[] { "Fake Name A", "Fake Name B", "Fake Name C" });
            var notIn = Restrictions.Not(In);

            Assert.Equal("FirstName Not In @0", notIn.GetExpressionBuilder().Build(commandBuilder));
        }        
    }
}