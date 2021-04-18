using System;
using System.Collections.Generic;
using System.Dynamic;
using DataQI.Dapper.FastCrud.Query;
using ExpectedObjects;
using Xunit;

namespace DataQI.Dapper.FastCrud.Test.Query
{
    public abstract class DapperExpressionTestBase
    {
        protected DapperCommand Command(FormattableString command, object values)
            => new DapperCommand(command, values);

        protected IDictionary<string, object> Parameters(params KeyValuePair<string, object>[] parametersKeyValue)
        {
            var parameters = new Dictionary<string, object>();

            foreach (var parameter in parametersKeyValue)
                parameters.Add(parameter.Key, parameter.Value);

            return parameters;
        }

        protected dynamic Parameters(IDictionary<string, object> parameters)
        {
            dynamic parametersDynamic = new ExpandoObject();
            var parametersDictionary = (IDictionary<string, object>)parametersDynamic;

            foreach (var parameter in parameters)
                parametersDictionary.Add(parameter.Key, parameter.Value);

            return parametersDynamic;
        }

        protected void AssertCommand(DapperCommand expected, DapperCommand actual)
        {
            AssertExpression(expected.Command, actual.Command);
            AssertObject(expected.Values, actual.Values);
        }

        protected void AssertExpression(FormattableString expected, FormattableString actual)
        {
            expected.ToExpectedObject().ShouldMatch(actual);
            AssertExpressionArgs(expected.GetArguments(), actual.GetArguments());
        }

        protected void AssertExpressionArgs(object[] expected, object[] actual)
        {
            Assert.Equal(expected.Length, actual.Length);
            
            for (int i = 0; i < expected.Length; i++)
            {
                var expectedArg = expected[i];
                var actualArg = actual[i];

                AssertObject(expectedArg, actualArg);

                if (expectedArg is FormattableString)
                    AssertExpressionArgs(
                        ((FormattableString)expectedArg).GetArguments(),
                        ((FormattableString)actualArg).GetArguments());
            }
        }

        protected void AssertObject(object expected, object actual)
            => expected.ToExpectedObject().ShouldEqual(actual);
    }
}
