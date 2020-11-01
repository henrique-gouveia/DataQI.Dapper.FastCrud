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
    public class DapperSimpleExpressionTest : DapperExpressionTestBase, IClassFixture<QueryFixture>
    {
        private readonly IDapperCommandBuilder commandBuilder;

        public DapperSimpleExpressionTest(QueryFixture fixture)
        {
            commandBuilder = fixture.GetCommandBuilder();
        }

        [Fact]
        public void TestRejectsNullCriterion()
        {
            var exception = Assert.Throws<ArgumentException>(() =>
                new DapperSimpleExpression(null));
            var exceptionMessage = exception.GetBaseException().Message;

            Assert.IsType<ArgumentException>(exception.GetBaseException());
            Assert.Equal("SimpleExpression must not be null", exceptionMessage);
        }

        [Fact]
        public void TestBuildEqualExpressionCorrectly()
        {
            var criterion = Restrictions.Equal("FirstName", "Fake Name");

            AssertExpression(
                $"{Sql.Column("FirstName")} = @{"0"}", 
                criterion.GetExpressionBuilder().Build(commandBuilder));
        }

        [Fact]
        public void TestBuildLikeExpressionCorrectly()
        {
            var criterion = Restrictions.Like("FirstName", "Fake Name");

            AssertExpression(
                $"{Sql.Column("FirstName")} Like @{"0"}", 
                criterion.GetExpressionBuilder().Build(commandBuilder));
        }

        [Fact]
        public void TestBuildContainingExpressionCorrectly()
        {
            var criterion = Restrictions.Containing("FirstName", "Fake Name");

            AssertExpression(
                $"{Sql.Column("FirstName")} Like @{"0"}",
                criterion.GetExpressionBuilder().Build(commandBuilder));
        }

        [Fact]
        public void TestBuildStartingWithExpressionCorrectly()
        {
            var criterion = Restrictions.StartingWith("LastName", "Fake Name");

            AssertExpression(
                $"{Sql.Column("LastName")} Like @{"0"}",
                criterion.GetExpressionBuilder().Build(commandBuilder));
        }

        [Fact]
        public void TestBuildEndingWithExpressionCorrectly()
        {
            var criterion = Restrictions.EndingWith("LastName", "Fake Name");

            AssertExpression(
                $"{Sql.Column("LastName")} Like @{"0"}",
                criterion.GetExpressionBuilder().Build(commandBuilder));
        }

        [Fact]
        public void TestBuildGreaterThanExpressionCorrectly()
        {
            var criterion = Restrictions.GreaterThan("Age", 20);

            AssertExpression(
                $"{Sql.Column("Age")} > @{"0"}",
                criterion.GetExpressionBuilder().Build(commandBuilder));
        }

        [Fact]
        public void TestBuildGreaterThanEqualExpressionCorrectly()
        {
            var criterion = Restrictions.GreaterThanEqual("Age", 20);

            AssertExpression(
                $"{Sql.Column("Age")} >= @{"0"}",
                criterion.GetExpressionBuilder().Build(commandBuilder));
        }

        [Fact]
        public void TestBuildLessThanExpressionCorrectly()
        {
            var criterion = Restrictions.LessThan("Age", 20);

            AssertExpression(
                $"{Sql.Column("Age")} < @{"0"}",
                criterion.GetExpressionBuilder().Build(commandBuilder));
        }

        [Fact]
        public void TestBuildLessThanEqualExpressionCorrectly()
        {
            var criterion = Restrictions.LessThanEqual("Age", 20);

            AssertExpression(
                $"{Sql.Column("Age")} <= @{"0"}",
                criterion.GetExpressionBuilder().Build(commandBuilder));
        }
    }
}
