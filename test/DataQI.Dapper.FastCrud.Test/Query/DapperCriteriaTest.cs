using System;
using System.Collections.Generic;
using System.Runtime.CompilerServices;

using Dapper.FastCrud;

using DataQI.Commons.Query.Support;
using DataQI.Dapper.FastCrud.Query;
using DataQI.Dapper.FastCrud.Query.Support;

using Xunit;

namespace DataQI.Dapper.FastCrud.Test.Query
{
    public class DapperCriteriaTest : DapperExpressionTestBase
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
        public void TestBuildCommandCorrectly()
        {
            var findByParameters = Parameters(KeyValuePair.Create<string, object>("0", "fake name"));
            var findByParametersExpected = Parameters(findByParameters);

            FormattableString expression = $"{Sql.Column("FirstName")} = @{"0"}";
            FormattableString expressionCommand = $"{expression}";
            DapperCommand expectedCommand = Command(expressionCommand, findByParametersExpected);

            var firstNameCriterion = Restrictions.Equal("FirstName", findByParameters["0"]);

            criteria.Add(firstNameCriterion);
            var command = criteria.BuildCommand();

            AssertCommand(expectedCommand, command);
        }

        [Fact]
        public void TestBuildCommandIncludesOrderBy()
        {
            var expected = FormattableStringFactory.Create("FirstName ASC, LastName DESC");

            criteria
                .AddOrder(Order.Asc("FirstName"))
                .AddOrder(Order.Desc("LastName"));

            var command = criteria.BuildCommand();

            AssertExpression(expected, command.OrderBy);
        }
    }
}