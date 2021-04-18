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
    public class DapperNotExpressionTest : DapperExpressionTestBase, IClassFixture<QueryFixture>
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

            AssertExpression(
                $"{Sql.Column("DateOfBirth")} Not Between @{"0"} And @{"1"}",
                notBetween.GetExpressionBuilder().Build(commandBuilder));
        }

        [Fact]
        public void TestBuildNotStartingWithExpressionCorrectly()
        {
            var startingWith = Restrictions.StartingWith("LastName", "Fake Name");
            var notStartingWith = Restrictions.Not(startingWith);

            AssertExpression(
                $"{Sql.Column("LastName")} Not Like @{"0"}",
                notStartingWith.GetExpressionBuilder().Build(commandBuilder));
        }

        [Fact]
        public void TestBuildNotEndingWithExpressionCorrectly()
        {
            var endingWith = Restrictions.EndingWith("LastName", "Fake Name");
            var notEndingWith = Restrictions.Not(endingWith);

            AssertExpression(
                $"{Sql.Column("LastName")} Not Like @{"0"}",
                notEndingWith.GetExpressionBuilder().Build(commandBuilder));
        }

        [Fact]
        public void TestBuildNotContainingExpressionCorrectly()
        {
            var containing = Restrictions.Containing("FirstName", "Fake Name");
            var notContaining = Restrictions.Not(containing);

            AssertExpression(
                $"{Sql.Column("FirstName")} Not Like @{"0"}",
                notContaining.GetExpressionBuilder().Build(commandBuilder));
        }

        [Fact]
        public void TestBuildNotLikeExpressionCorrectly()
        {
            var like = Restrictions.Like("FirstName", "Fake Name");
            var notLike = Restrictions.Not(like);

            AssertExpression(
                $"{Sql.Column("FirstName")} Not Like @{"0"}",
                notLike.GetExpressionBuilder().Build(commandBuilder));
        }

        [Fact]
        public void TestBuildNotEqualExpressionCorrectly()
        {
            var equal = Restrictions.Equal("FirstName", "Fake Name");
            var notEqual = Restrictions.Not(equal);

            AssertExpression(
                $"{Sql.Column("FirstName")} != @{"0"}",
                notEqual.GetExpressionBuilder().Build(commandBuilder));
        }

        [Fact]
        public void TestBuildNotInExpressionCorrectly()
        {
            var In = Restrictions.In("FirstName", new string[] { "Fake Name A", "Fake Name B", "Fake Name C" });
            var notIn = Restrictions.Not(In);

            AssertExpression(
                $"{Sql.Column("FirstName")} Not In @{"0"}",
                notIn.GetExpressionBuilder().Build(commandBuilder));
        }

        [Fact]
        public void TestBuildNotNullExpressionCorrectly()
        {
            var Null = Restrictions.Null("Email");
            var notNull = Restrictions.Not(Null);

            AssertExpression(
                $"{Sql.Column("Email")} Is Not Null",
                notNull.GetExpressionBuilder().Build(commandBuilder));
        }
    }
}