using System.Collections.Generic;
using System.Linq;

using DataQI.Dapper.FastCrud.Query;
using Moq;

namespace DataQI.Dapper.FastCrud.Test.Fixtures
{
    public class QueryFixture
    {
        private readonly Mock<IDapperCommandBuilder> commandBuilderMoq;
        private readonly ICollection<int> parameters;
        
        public QueryFixture()
        {
            commandBuilderMoq = new Mock<IDapperCommandBuilder>();
            parameters = new List<int>();

            commandBuilderMoq
                .Setup(cb => cb.AddExpressionValue(It.IsAny<object>()))
                .Returns(() => 
                {
                    var parameter = 0;
                    if (parameters.Count > 0)
                    {
                        parameter = parameters.LastOrDefault();
                        parameter++;
                    }
                    parameters.Add(parameter);

                    return parameter.ToString();
                });
        }

        public IDapperCommandBuilder GetCommandBuilder()
        {
            parameters.Clear();
            return commandBuilderMoq.Object;
        }
    }
}