using System.Collections.Generic;
using System.Dynamic;
using DataQI.Dapper.FastCrud.Query;
using ExpectedObjects;
using Xunit;

namespace DataQI.Dapper.FastCrud.Test.Query
{
    public abstract class DapperCommandBaseTest
    {
        protected void AssertCommand(DapperCommand command, string commandExpected, object parametersExpected)
        {
            Assert.NotNull(command);
            Assert.Equal(commandExpected, command.Command);

            parametersExpected.ToExpectedObject().ShouldMatch(command.Values);
        }

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
            var parametersDictionary = (IDictionary<string, object>) parametersDynamic;

            foreach (var parameter in parameters)
                parametersDictionary.Add(parameter.Key, parameter.Value);

            return parametersDynamic;
        }        
    }
}