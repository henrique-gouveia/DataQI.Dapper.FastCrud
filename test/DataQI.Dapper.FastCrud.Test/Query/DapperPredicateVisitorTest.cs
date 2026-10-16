using System;
using System.Collections.Generic;
using System.Linq;
using System.Reflection;
using System.Runtime.CompilerServices;

using Dapper.FastCrud;

using DataQI.Commons.Query;
using DataQI.Commons.Query.Ast;
using DataQI.Commons.Query.Support;
using DataQI.Dapper.FastCrud.Query;
using DataQI.Dapper.FastCrud.Query.Support;

using Moq;
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

            FormattableString expected = $"{Sql.Column("FirstName")} = @{"p0"}";
            AssertWhole(expected, predicate.Command);
            AssertObject("Adams", ((IDictionary<string, object>)predicate.Values)["p0"]);
        }

        [Fact]
        public void TestBuildRestartsParameterKeysForEachPredicate()
        {
            var first = DapperPredicateVisitor.BuildPredicate(new Criteria()
                .Add(Restrictions.Equal("FirstName", "Adams"))
                .Add(Restrictions.Equal("LastName", "Barnes")));
            var second = DapperPredicateVisitor.BuildPredicate(new Criteria()
                .Add(Restrictions.Equal("FirstName", "Miller")));
            var firstParameters = (IDictionary<string, object>)first.Values;
            var secondParameters = (IDictionary<string, object>)second.Values;

            Assert.Equal(2, firstParameters.Count);
            Assert.Equal("Adams", firstParameters["p0"]);
            Assert.Equal("Barnes", firstParameters["p1"]);
            Assert.Single(secondParameters);
            Assert.Equal("Miller", secondParameters["p0"]);
            AssertWhole($"{Sql.Column("FirstName")} = @{"p0"}", second.Command);
        }

        [Theory]
        [InlineData(false)]
        [InlineData(true)]
        public void TestBuildEqualNullAsIsNull(bool negated)
        {
            var criterion = Restrictions.Equal("Email", null);
            if (negated)
                criterion = Restrictions.Not(criterion);

            var predicate = DapperPredicateVisitor.BuildPredicate(new Criteria().Add(criterion));
            FormattableString inner = $"{Sql.Column("Email")} Is Null";
            FormattableString expected = negated ? $"Not ({inner})" : inner;

            AssertWhole(expected, predicate.Command);
            Assert.Empty((IDictionary<string, object>)predicate.Values);
        }

        [Fact]
        public void TestBuildEqualNullDoesNotConsumeParameterKey()
        {
            var criteria = new Criteria()
                .Add(Restrictions.Equal("Email", null))
                .Add(Restrictions.Equal("FirstName", "Adams"));
            var predicate = DapperPredicateVisitor.BuildPredicate(criteria);
            FormattableString first = $"{Sql.Column("Email")} Is Null";
            FormattableString second = $"{Sql.Column("FirstName")} = @{"p0"}";
            var parameters = (IDictionary<string, object>)predicate.Values;

            AssertExpression($"{first} AND {second}", predicate.Command);
            Assert.Single(parameters);
            Assert.Equal("Adams", parameters["p0"]);
        }

        [Theory]
        [InlineData(10)]
        [InlineData(100)]
        public void TestBindDoesNotEnumeratePreviouslyBoundKeys(int parameterCount)
        {
            var values = new Dictionary<string, object>();
            var visitedKeyCount = 0;
            var keys = new Mock<ICollection<string>>();
            keys.Setup(collection => collection.GetEnumerator()).Returns(() =>
                values.Keys.Select(key =>
                {
                    visitedKeyCount++;
                    return key;
                }).GetEnumerator());
            var parameters = new Mock<IDictionary<string, object>>();
            parameters.SetupGet(dictionary => dictionary.Keys).Returns(keys.Object);
            parameters.SetupGet(dictionary => dictionary.Count).Returns(() => values.Count);
            parameters.Setup(dictionary => dictionary.Add(It.IsAny<string>(), It.IsAny<object>()))
                .Callback<string, object>((key, value) => values.Add(key, value));
            var visitor = (DapperPredicateVisitor)Activator.CreateInstance(typeof(DapperPredicateVisitor), true);
            var parametersField = typeof(DapperPredicateVisitor)
                .GetField("parameters", BindingFlags.Instance | BindingFlags.NonPublic);
            parametersField.SetValue(visitor, parameters.Object);

            for (var i = 0; i < parameterCount; i++)
                visitor.Visit(new Comparison("Age", ComparisonKind.Equal, i));

            Assert.Equal(parameterCount, values.Count);
            for (var i = 0; i < parameterCount; i++)
                Assert.Equal(i, values[$"p{i}"]);
            Assert.Equal(0, visitedKeyCount);
        }

        [Fact]
        public void TestBuildBetweenCorrectly()
        {
            var start = DateTime.Now.AddYears(-1);
            var end = DateTime.Now.AddYears(1);
            var criteria = new Criteria().Add(Restrictions.Between("BirthDate", start, end));

            var predicate = DapperPredicateVisitor.BuildPredicate(criteria);

            FormattableString expected = $"{Sql.Column("BirthDate")} Between @{"p0"} And @{"p1"}";
            AssertWhole(expected, predicate.Command);
        }

        [Fact]
        public void TestBuildInCorrectly()
        {
            var cities = new object[] { "Fortaleza", "Barcelona" };
            var criteria = new Criteria().Add(Restrictions.In("City", cities));

            var predicate = DapperPredicateVisitor.BuildPredicate(criteria);

            FormattableString expected = $"{Sql.Column("City")} In @{"p0"}";
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

            FormattableString expected = $"{Sql.Column("Title")} Like @{"p0"}";
            AssertWhole(expected, predicate.Command);
            AssertObject("%General%", ((IDictionary<string, object>)predicate.Values)["p0"]);
        }

        [Fact]
        public void TestBuildNotCorrectly()
        {
            var criteria = new Criteria().Add(Restrictions.Not(Restrictions.Equal("FirstName", "Adams")));

            var predicate = DapperPredicateVisitor.BuildPredicate(criteria);

            FormattableString inner = $"{Sql.Column("FirstName")} = @{"p0"}";
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

            FormattableString first = $"{Sql.Column("FirstName")} = @{"p0"}";
            FormattableString second = $"{Sql.Column("LastName")} = @{"p1"}";
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

            FormattableString first = $"{Sql.Column("FirstName")} = @{"p0"}";
            FormattableString second = $"{Sql.Column("LastName")} = @{"p1"}";
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

            AssertWhole($"{Sql.Column("FirstName")} = @{"p0"}", predicate.Command);
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

            AssertWhole(ComparisonSql("Age", symbol, "p0"), predicate.Command);
            AssertObject(30, ((IDictionary<string, object>)predicate.Values)["p0"]);
        }

        [Theory]
        [InlineData(TextMatchKind.Like)]
        [InlineData(TextMatchKind.Containing)]
        [InlineData(TextMatchKind.StartingWith)]
        [InlineData(TextMatchKind.EndingWith)]
        public void TestBuildEveryTextMatchKindAsLike(TextMatchKind kind)
        {
            var predicate = DapperPredicateVisitor.BuildPredicate(new Criteria().Add(new TextMatch("Title", kind, "Ad%")));

            AssertWhole($"{Sql.Column("Title")} Like @{"p0"}", predicate.Command);
            AssertObject("Ad%", ((IDictionary<string, object>)predicate.Values)["p0"]);
        }

        [Fact]
        public void TestBuildNotOverEveryCriterionCorrectly()
        {
            AssertNot(Restrictions.Between("Age", 18, 65), $"{Sql.Column("Age")} Between @{"p0"} And @{"p1"}");
            AssertNot(Restrictions.StartingWith("Name", "Ad"), $"{Sql.Column("Name")} Like @{"p0"}");
            AssertNot(Restrictions.EndingWith("Name", "ms"), $"{Sql.Column("Name")} Like @{"p0"}");
            AssertNot(Restrictions.Containing("Name", "da"), $"{Sql.Column("Name")} Like @{"p0"}");
            AssertNot(Restrictions.Like("Name", "Ad%"), $"{Sql.Column("Name")} Like @{"p0"}");
            AssertNot(Restrictions.Equal("Name", "Adams"), $"{Sql.Column("Name")} = @{"p0"}");
            AssertNot(Restrictions.In("City", new object[] { "A", "B" }), $"{Sql.Column("City")} In @{"p0"}");
            AssertNot(Restrictions.Null("Email"), $"{Sql.Column("Email")} Is Null");
        }

        [Theory]
        [InlineData(LogicalKind.And, " AND ")]
        [InlineData(LogicalKind.Or, " OR ")]
        public void TestBuildJunctionOfEveryKindAndSizeCorrectly(LogicalKind kind, string separator)
        {
            FormattableString first = $"{Sql.Column("FirstName")} = @{"p0"}";
            FormattableString second = $"{Sql.Column("LastName")} = @{"p1"}";

            var single = DapperPredicateVisitor.BuildPredicate(new Criteria().Add(
                new Junction(kind).Add(Restrictions.Equal("FirstName", "A"))));
            var pair = DapperPredicateVisitor.BuildPredicate(new Criteria().Add(
                new Junction(kind).Add(Restrictions.Equal("FirstName", "A")).Add(Restrictions.Equal("LastName", "B"))));

            AssertWhole($"({first})", single.Command);
            AssertWhole(FormattableStringFactory.Create($"({{0}}{separator}{{1}})", first, second), pair.Command);
        }

        [Theory]
        [InlineData(LogicalKind.And)]
        [InlineData(LogicalKind.Or)]
        public void TestEmptyJunctionIsRejected(LogicalKind kind)
        {
            var criteria = new Criteria().Add(new Junction(kind));

            AssertEmptyJunctionRejected(criteria, kind);
        }

        [Theory]
        [InlineData(LogicalKind.And)]
        [InlineData(LogicalKind.Or)]
        public void TestEmptyJunctionBeforeComparisonIsRejected(LogicalKind kind)
        {
            var criteria = new Criteria()
                .Add(new Junction(kind))
                .Add(Restrictions.Equal("Name", "Adams"));

            AssertEmptyJunctionRejected(criteria, kind);
        }

        [Theory]
        [InlineData(LogicalKind.And)]
        [InlineData(LogicalKind.Or)]
        public void TestEmptyJunctionAfterComparisonIsRejected(LogicalKind kind)
        {
            var criteria = new Criteria()
                .Add(Restrictions.Equal("Name", "Adams"))
                .Add(new Junction(kind));

            AssertEmptyJunctionRejected(criteria, kind);
        }

        [Theory]
        [InlineData(LogicalKind.And, LogicalKind.And, true)]
        [InlineData(LogicalKind.And, LogicalKind.And, false)]
        [InlineData(LogicalKind.And, LogicalKind.Or, true)]
        [InlineData(LogicalKind.And, LogicalKind.Or, false)]
        [InlineData(LogicalKind.Or, LogicalKind.And, true)]
        [InlineData(LogicalKind.Or, LogicalKind.And, false)]
        [InlineData(LogicalKind.Or, LogicalKind.Or, true)]
        [InlineData(LogicalKind.Or, LogicalKind.Or, false)]
        public void TestNestedEmptyJunctionIsRejected(LogicalKind outerKind, LogicalKind innerKind, bool emptyFirst)
        {
            var outer = new Junction(outerKind);
            var empty = new Junction(innerKind);
            var comparison = Restrictions.Equal("Name", "Adams");
            if (emptyFirst)
                outer.Add(empty).Add(comparison);
            else
                outer.Add(comparison).Add(empty);

            AssertEmptyJunctionRejected(new Criteria().Add(outer), innerKind);
        }

        [Theory]
        [InlineData(LogicalKind.And, LogicalKind.And)]
        [InlineData(LogicalKind.And, LogicalKind.Or)]
        [InlineData(LogicalKind.Or, LogicalKind.And)]
        [InlineData(LogicalKind.Or, LogicalKind.Or)]
        public void TestOnlyNestedEmptyJunctionIsRejected(LogicalKind outerKind, LogicalKind innerKind)
        {
            var criteria = new Criteria().Add(new Junction(outerKind)
                .Add(new Junction(innerKind)));

            AssertEmptyJunctionRejected(criteria, innerKind);
        }

        [Theory]
        [InlineData(LogicalKind.And)]
        [InlineData(LogicalKind.Or)]
        public void TestNotOverEmptyJunctionIsRejected(LogicalKind kind)
        {
            var criteria = new Criteria().Add(Restrictions.Not(new Junction(kind)));

            AssertEmptyJunctionRejected(criteria, kind);
        }

        [Theory]
        [InlineData(LogicalKind.And)]
        [InlineData(LogicalKind.Or)]
        public void TestJunctionCanBePopulatedBeforeBuildingPredicate(LogicalKind kind)
        {
            var junction = new Junction(kind);
            var criteria = new Criteria().Add(junction);

            junction.Add(Restrictions.Equal("Name", "Adams"));
            var predicate = DapperPredicateVisitor.BuildPredicate(criteria);
            FormattableString inner = $"{Sql.Column("Name")} = @{"p0"}";

            AssertWhole($"({inner})", predicate.Command);
            AssertObject("Adams", ((IDictionary<string, object>)predicate.Values)["p0"]);
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

            FormattableString first = $"{Sql.Column("FirstName")} = @{"p0"}";
            FormattableString second = $"{Sql.Column("LastName")} = @{"p1"}";
            FormattableString third = $"{Sql.Column("City")} = @{"p2"}";
            FormattableString left = $"({first} AND {second})";
            FormattableString right = $"({third})";
            AssertWhole($"({left} OR {right})", predicate.Command);
        }

        private static void AssertEmptyJunctionRejected(ICriteria criteria, LogicalKind kind)
        {
            var exception = Assert.Throws<InvalidOperationException>(() =>
                DapperPredicateVisitor.BuildPredicate(criteria));

            Assert.Equal($"Junction '{kind}' must contain at least one criterion.", exception.Message);
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
