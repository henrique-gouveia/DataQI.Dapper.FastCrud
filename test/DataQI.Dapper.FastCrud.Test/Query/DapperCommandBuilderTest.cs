using System.Collections.Generic;

using DataQI.Commons.Query.Support;

using DataQI.Dapper.FastCrud.Query.Extensions;
using DataQI.Dapper.FastCrud.Query.Support;

using Xunit;

namespace DataQI.Dapper.FastCrud.Test.Query
{
    public class DapperCommandBuilderTest : DapperCommandBaseTest
    {
        private readonly DapperCommandBuilder commandBuilder;

        public DapperCommandBuilderTest()
        {
            this.commandBuilder = new DapperCommandBuilder();
        }

        [Fact]
        public void TestBuildSimpleExpressionCorrectly()
        {
            var findByParameters = Parameters(KeyValuePair.Create<string, object>("0", "fake name"));
            var findByParametersExpected = Parameters(findByParameters);

            var firstNameCriterion = Restrictions.Equal("FirstName", findByParameters["0"]);

            commandBuilder.AddExpression(firstNameCriterion.GetExpressionBuilder());

            AssertCommand(commandBuilder.Build(), "FirstName = @0", findByParametersExpected);
        }

        [Fact]
        public void TestBuildComposedExpressionCorrectly()
        {
            var findByParameters = Parameters(
                KeyValuePair.Create<string, object>("0", "Fake First Name"),
                KeyValuePair.Create<string, object>("1", "Fake Last Name")
            );
            var findByParametersExpected = Parameters(findByParameters);

            var firstNameCriterion = Restrictions.Equal("FirstName", findByParameters["0"]);
            var lastNameCriterion = Restrictions.Equal("LastName", findByParameters["1"]);

            commandBuilder
                .AddExpression(firstNameCriterion.GetExpressionBuilder())
                .AddExpression(lastNameCriterion.GetExpressionBuilder());

            AssertCommand(commandBuilder.Build(), "FirstName = @0 AND LastName = @1", findByParametersExpected);
        }

        [Fact]
        public void TestBuildSimpleConjunctionWithSimpleExpressionCorrectly()
        {
            var findByParameters = Parameters(KeyValuePair.Create<string, object>("0", "fake name"));
            var findByParametersExpected = Parameters(findByParameters);

            var junction = Restrictions
                .Conjunction()
                .Add(Restrictions.Equal("FirstName", findByParameters["0"]));

            commandBuilder.AddExpression(junction.GetExpressionBuilder());

            AssertCommand(commandBuilder.Build(), "(FirstName = @0)", findByParametersExpected);
        }

        [Fact]
        public void TestBuildSimpleDisjunctionWithSimpleExpressionCorrectly()
        {
            var findByParameters = Parameters(KeyValuePair.Create<string, object>("0", "fake name"));
            var findByParametersExpected = Parameters(findByParameters);

            var junction = Restrictions
                .Disjunction()
                .Add(Restrictions.Equal("FirstName", findByParameters["0"]));

            commandBuilder.AddExpression(junction.GetExpressionBuilder());

            AssertCommand(commandBuilder.Build(), "(FirstName = @0)", findByParametersExpected);
        }

        [Fact]
        public void TestBuildComposedConjunctionWithSimpleExpressionCorrectly()
        {
            var findByParameters = Parameters(
                KeyValuePair.Create<string, object>("0", "Fake First Name"),
                KeyValuePair.Create<string, object>("1", "Fake Last Name")
            );
            var findByParametersExpected = Parameters(findByParameters);

            var firstNameCriterion = Restrictions.Equal("FirstName", findByParameters["0"]);
            var lastNameCriterion = Restrictions.Equal("LastName", findByParameters["1"]);    

            var junction1 = Restrictions
                .Conjunction()
                .Add(firstNameCriterion);

            var junction2 = Restrictions
                .Conjunction()
                .Add(lastNameCriterion);

            commandBuilder
                .AddExpression(junction1.GetExpressionBuilder())
                .AddExpression(junction2.GetExpressionBuilder());

            AssertCommand(commandBuilder.Build(), "(FirstName = @0) AND (LastName = @1)", findByParametersExpected);
        }

        [Fact]
        public void TestBuildComposedDisjunctionWithSimpleExpressionCorrectly()
        {
            var findByParameters = Parameters(
                KeyValuePair.Create<string, object>("0", "Fake First Name"),
                KeyValuePair.Create<string, object>("1", "Fake Last Name")
            );
            var findByParametersExpected = Parameters(findByParameters);

            var firstNameCriterion = Restrictions.Equal("FirstName", findByParameters["0"]);
            var lastNameCriterion = Restrictions.Equal("LastName", findByParameters["1"]);    

            var junction1 = Restrictions
                .Disjunction()
                .Add(firstNameCriterion);

            var junction2 = Restrictions
                .Disjunction()
                .Add(lastNameCriterion);

            commandBuilder
                .AddExpression(junction1.GetExpressionBuilder())
                .AddExpression(junction2.GetExpressionBuilder());

            AssertCommand(commandBuilder.Build(), "(FirstName = @0) AND (LastName = @1)", findByParametersExpected);
        }
    }
}
