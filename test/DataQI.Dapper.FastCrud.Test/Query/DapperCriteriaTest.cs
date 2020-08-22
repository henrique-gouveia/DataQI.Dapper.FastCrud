using System;
using System.Collections.Generic;

using DataQI.Commons.Query.Support;
using DataQI.Dapper.FastCrud.Query;
using DataQI.Dapper.FastCrud.Query.Support;

using Xunit;

namespace DataQI.Dapper.FastCrud.Test.Query
{
    public class DapperCriteriaTest : DapperCommandBaseTest
    {
        private readonly DapperCriteria criteria;

        public DapperCriteriaTest()
        {
            criteria = new DapperCriteria();
        }

        [Fact]
        public void TestRejectsAddNullCriterion()
        {
            var exception = Assert.Throws<ArgumentException>(() =>
                new DapperCriteria().Add(null));
            var exceptionMessage = exception.GetBaseException().Message;

            Assert.IsType<ArgumentException>(exception.GetBaseException());
            Assert.Equal("Criterion must not be null", exceptionMessage);
        }

        [Fact]
        public void TestBuildCommandSimpleExpression()
        {
            var findByParameters = Parameters(KeyValuePair.Create<string, object>("0", "fake name"));
            var findByParametersExpected = Parameters(findByParameters);

            var firstNameCriterion = Restrictions.Equal("FirstName", findByParameters["0"]);

            criteria.Add(firstNameCriterion);
            var command = criteria.BuildCommand();

            AssertCommand(command, "FirstName = @0", findByParametersExpected);
        }

        [Fact]
        public void TestBuildCommandComposedExpressionCorrectly()
        {
            var findByParameters = Parameters(
                KeyValuePair.Create<string, object>("0", "Fake First Name"),
                KeyValuePair.Create<string, object>("1", "Fake Last Name")
            );
            var findByParametersExpected = Parameters(findByParameters);

            var firstNameCriterion = Restrictions.Equal("FirstName", findByParameters["0"]);
            var lastNameCriterion = Restrictions.Equal("LastName", findByParameters["1"]);

            criteria
                .Add(firstNameCriterion)
                .Add(lastNameCriterion);
            var command = criteria.BuildCommand();

            AssertCommand(command, "FirstName = @0 AND LastName = @1", findByParametersExpected);
        }

        [Fact]
        public void TestBuildSimpleConjunctionWithSimpleExpressionCorrectly()
        {
            var findByParameters = Parameters(KeyValuePair.Create<string, object>("0", "fake name"));
            var findByParametersExpected = Parameters(findByParameters);

            var junction = Restrictions
                .Conjunction()
                .Add(Restrictions.Equal("FirstName", findByParameters["0"]));

            criteria.Add(junction);
            var command = criteria.BuildCommand();

            AssertCommand(command, "(FirstName = @0)", findByParametersExpected);
        }

        [Fact]
        public void TestBuildSimpleDisjunctionWithSimpleExpressionCorrectly()
        {
            var findByParameters = Parameters(KeyValuePair.Create<string, object>("0", "fake name"));
            var findByParametersExpected = Parameters(findByParameters);

            var junction = Restrictions
                .Disjunction()
                .Add(Restrictions.Equal("FirstName", findByParameters["0"]));

            criteria.Add(junction);
            var command = criteria.BuildCommand();

            AssertCommand(command, "(FirstName = @0)", findByParametersExpected);
        }

        [Fact]
        public void TestBuildComposedConjunctionWithSimpleExpressionCorrectly()
        {
            var findByParameters = Parameters(
                KeyValuePair.Create<string, object>("0", "Fake First Name"),
                KeyValuePair.Create<string, object>("1", "Fake Last Name")
            );
            var findByParametersExpected = Parameters(findByParameters);

            var firtNameCriterion = Restrictions.Equal("FirstName", findByParameters["0"]);
            var lastNameCriterion = Restrictions.Equal("LastName", findByParameters["1"]);    

            var junction1 = Restrictions
                .Conjunction()
                .Add(firtNameCriterion);

            var junction2 = Restrictions
                .Conjunction()
                .Add(lastNameCriterion);

            criteria.Add(junction1).Add(junction2);
            var command = criteria.BuildCommand();

            AssertCommand(command, "(FirstName = @0) AND (LastName = @1)", findByParametersExpected);
        }

        [Fact]
        public void TestBuildComposedDisjunctionWithSimpleExpressionCorrectly()
        {
            var findByParameters = Parameters(
                KeyValuePair.Create<string, object>("0", "Fake First Name"),
                KeyValuePair.Create<string, object>("1", "Fake Last Name")
            );
            var findByParametersExpected = Parameters(findByParameters);

            var firtNameCriterion = Restrictions.Equal("FirstName", findByParameters["0"]);
            var lastNameCriterion = Restrictions.Equal("LastName", findByParameters["1"]);    

            var junction1 = Restrictions
                .Disjunction()
                .Add(firtNameCriterion);

            var junction2 = Restrictions
                .Disjunction()
                .Add(lastNameCriterion);

            criteria.Add(junction1).Add(junction2);
            var command = criteria.BuildCommand();

            AssertCommand(command, "(FirstName = @0) AND (LastName = @1)", findByParametersExpected);
        }        
    }
}