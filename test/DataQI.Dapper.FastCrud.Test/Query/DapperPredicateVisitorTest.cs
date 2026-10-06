using System;
using System.Collections.Generic;
using System.Linq;
using System.Runtime.CompilerServices;

using Dapper.FastCrud;

using DataQI.Commons.Query;
using DataQI.Commons.Query.Ast;
using DataQI.Commons.Query.Support;
using DataQI.Dapper.FastCrud.Query;
using DataQI.Dapper.FastCrud.Query.Support;

using Xunit;

namespace DataQI.Dapper.FastCrud.Test.Query
{
    public class DapperPredicateVisitorTest : DapperExpressionTestBase
    {
        [Fact]
        public void TestBuildComparisonCorrectly()
        {
            var criteria = new Criteria().Add(Restrictions.Equal("FirstName", "Adams"));

            var predicate = DapperPredicateVisitor.BuildPredicate(criteria);

            FormattableString expected = $"{Sql.Column("FirstName")} = @{"0"}";
            AssertWhole(expected, predicate.Command);
            AssertObject("Adams", ((IDictionary<string, object>)predicate.Values)["0"]);
        }

        [Fact]
        public void TestBuildBetweenCorrectly()
        {
            var start = DateTime.Now.AddYears(-1);
            var end = DateTime.Now.AddYears(1);
            var criteria = new Criteria().Add(Restrictions.Between("BirthDate", start, end));

            var predicate = DapperPredicateVisitor.BuildPredicate(criteria);

            FormattableString expected = $"{Sql.Column("BirthDate")} Between @{"0"} And @{"1"}";
            AssertWhole(expected, predicate.Command);
        }

        [Fact]
        public void TestBuildInCorrectly()
        {
            var cities = new object[] { "Fortaleza", "Barcelona" };
            var criteria = new Criteria().Add(Restrictions.In("City", cities));

            var predicate = DapperPredicateVisitor.BuildPredicate(criteria);

            FormattableString expected = $"{Sql.Column("City")} In @{"0"}";
            AssertWhole(expected, predicate.Command);
        }

        [Fact]
        public void TestBuildIsNullCorrectly()
        {
            var criteria = new Criteria().Add(Restrictions.Null("Email"));

            var predicate = DapperPredicateVisitor.BuildPredicate(criteria);

            FormattableString expected = $"{Sql.Column("Email")} Is Null";
            AssertWhole(expected, predicate.Command);
        }

        [Fact]
        public void TestBuildTextMatchCorrectly()
        {
            var criteria = new Criteria().Add(Restrictions.Like("Title", "%General%"));

            var predicate = DapperPredicateVisitor.BuildPredicate(criteria);

            FormattableString expected = $"{Sql.Column("Title")} Like @{"0"}";
            AssertWhole(expected, predicate.Command);
            AssertObject("%General%", ((IDictionary<string, object>)predicate.Values)["0"]);
        }

        [Fact]
        public void TestBuildNotCorrectly()
        {
            var criteria = new Criteria().Add(Restrictions.Not(Restrictions.Equal("FirstName", "Adams")));

            var predicate = DapperPredicateVisitor.BuildPredicate(criteria);

            FormattableString inner = $"{Sql.Column("FirstName")} = @{"0"}";
            FormattableString expected = $"Not ({inner})";
            AssertWhole(expected, predicate.Command);
        }

        [Fact]
        public void TestBuildJunctionCorrectly()
        {
            var criteria = new Criteria().Add(Restrictions
                .Conjunction()
                .Add(Restrictions.Equal("FirstName", "Adams"))
                .Add(Restrictions.Equal("LastName", "Barnes")));

            var predicate = DapperPredicateVisitor.BuildPredicate(criteria);

            FormattableString first = $"{Sql.Column("FirstName")} = @{"0"}";
            FormattableString second = $"{Sql.Column("LastName")} = @{"1"}";
            FormattableString expected = $"({first} AND {second})";
            AssertWhole(expected, predicate.Command);
        }

        [Fact]
        public void TestBuildMultipleTopLevelCriterionsJoinedWithAnd()
        {
            var criteria = new Criteria()
                .Add(Restrictions.Equal("FirstName", "Adams"))
                .Add(Restrictions.Equal("LastName", "Barnes"));

            var predicate = DapperPredicateVisitor.BuildPredicate(criteria);

            FormattableString first = $"{Sql.Column("FirstName")} = @{"0"}";
            FormattableString second = $"{Sql.Column("LastName")} = @{"1"}";
            FormattableString expected = $"{first} AND {second}";
            AssertExpression(expected, predicate.Command);
        }

        [Fact]
        public void TestBuildWithoutCriterionsLeavesCommandEmpty()
        {
            var predicate = DapperPredicateVisitor.BuildPredicate(new Criteria());

            Assert.Equal(string.Empty, predicate.Command.Format);
        }

        [Fact]
        public void TestBuildIgnoresOrdering()
        {
            var criteria = new Criteria()
                .Add(Restrictions.Equal("FirstName", "Adams"))
                .AddOrder(Order.Asc("FirstName"));

            var predicate = DapperPredicateVisitor.BuildPredicate(criteria);

            AssertWhole($"{Sql.Column("FirstName")} = @{"0"}", predicate.Command);
        }

        [Theory]
        [InlineData(ComparisonKind.Equal, "=")]
        [InlineData(ComparisonKind.GreaterThan, ">")]
        [InlineData(ComparisonKind.GreaterThanEqual, ">=")]
        [InlineData(ComparisonKind.LessThan, "<")]
        [InlineData(ComparisonKind.LessThanEqual, "<=")]
        public void TestBuildEveryComparisonKindCorrectly(ComparisonKind kind, string symbol)
        {
            var predicate = DapperPredicateVisitor.BuildPredicate(new Criteria().Add(new Comparison("Age", kind, 30)));

            AssertWhole(ComparisonSql("Age", symbol, "0"), predicate.Command);
            AssertObject(30, ((IDictionary<string, object>)predicate.Values)["0"]);
        }

        [Theory]
        [InlineData(TextMatchKind.Like)]
        [InlineData(TextMatchKind.Containing)]
        [InlineData(TextMatchKind.StartingWith)]
        [InlineData(TextMatchKind.EndingWith)]
        public void TestBuildEveryTextMatchKindAsLike(TextMatchKind kind)
        {
            var predicate = DapperPredicateVisitor.BuildPredicate(new Criteria().Add(new TextMatch("Title", kind, "Ad%")));

            AssertWhole($"{Sql.Column("Title")} Like @{"0"}", predicate.Command);
            AssertObject("Ad%", ((IDictionary<string, object>)predicate.Values)["0"]);
        }

        [Fact]
        public void TestBuildNotOverEveryCriterionCorrectly()
        {
            AssertNot(Restrictions.Between("Age", 18, 65), $"{Sql.Column("Age")} Between @{"0"} And @{"1"}");
            AssertNot(Restrictions.StartingWith("Name", "Ad"), $"{Sql.Column("Name")} Like @{"0"}");
            AssertNot(Restrictions.EndingWith("Name", "ms"), $"{Sql.Column("Name")} Like @{"0"}");
            AssertNot(Restrictions.Containing("Name", "da"), $"{Sql.Column("Name")} Like @{"0"}");
            AssertNot(Restrictions.Like("Name", "Ad%"), $"{Sql.Column("Name")} Like @{"0"}");
            AssertNot(Restrictions.Equal("Name", "Adams"), $"{Sql.Column("Name")} = @{"0"}");
            AssertNot(Restrictions.In("City", new object[] { "A", "B" }), $"{Sql.Column("City")} In @{"0"}");
            AssertNot(Restrictions.Null("Email"), $"{Sql.Column("Email")} Is Null");
        }

        [Theory]
        [InlineData(LogicalKind.And, " AND ")]
        [InlineData(LogicalKind.Or, " OR ")]
        public void TestBuildJunctionOfEveryKindAndSizeCorrectly(LogicalKind kind, string separator)
        {
            FormattableString first = $"{Sql.Column("FirstName")} = @{"0"}";
            FormattableString second = $"{Sql.Column("LastName")} = @{"1"}";

            var single = DapperPredicateVisitor.BuildPredicate(new Criteria().Add(
                new Junction(kind).Add(Restrictions.Equal("FirstName", "A"))));
            var pair = DapperPredicateVisitor.BuildPredicate(new Criteria().Add(
                new Junction(kind).Add(Restrictions.Equal("FirstName", "A")).Add(Restrictions.Equal("LastName", "B"))));

            AssertWhole($"({first})", single.Command);
            AssertWhole(FormattableStringFactory.Create($"({{0}}{separator}{{1}})", first, second), pair.Command);
        }

        [Fact]
        public void TestBuildNestedJunctionsCorrectly()
        {
            var criteria = new Criteria().Add(new Junction(LogicalKind.Or)
                .Add(new Junction(LogicalKind.And)
                    .Add(Restrictions.Equal("FirstName", "A"))
                    .Add(Restrictions.Equal("LastName", "B")))
                .Add(new Junction(LogicalKind.And)
                    .Add(Restrictions.Equal("City", "C"))));

            var predicate = DapperPredicateVisitor.BuildPredicate(criteria);

            FormattableString first = $"{Sql.Column("FirstName")} = @{"0"}";
            FormattableString second = $"{Sql.Column("LastName")} = @{"1"}";
            FormattableString third = $"{Sql.Column("City")} = @{"2"}";
            FormattableString left = $"({first} AND {second})";
            FormattableString right = $"({third})";
            AssertWhole($"({left} OR {right})", predicate.Command);
        }

        private static FormattableString ComparisonSql(string column, string symbol, string parameter)
            => FormattableStringFactory.Create($"{{0}} {symbol} @{{1}}", Sql.Column(column), parameter);

        private void AssertNot(ICriterion inner, FormattableString innerExpected)
        {
            var predicate = DapperPredicateVisitor.BuildPredicate(new Criteria().Add(Restrictions.Not(inner)));

            AssertWhole($"Not ({innerExpected})", predicate.Command);
        }

        private void AssertWhole(FormattableString expression, FormattableString actual)
            => AssertExpression($"{expression}", actual);
    }
}
