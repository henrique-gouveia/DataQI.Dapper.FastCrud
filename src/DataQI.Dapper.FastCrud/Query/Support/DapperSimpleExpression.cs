using DataQI.Commons.Query.Support;
using DataQI.Commons.Util;
using DataQI.Dapper.FastCrud.Query.Extensions;

namespace DataQI.Dapper.FastCrud.Query.Support
{
    public class DapperSimpleExpression : IDapperExpressionBuilder
    {
        private readonly SimpleExpression simpleExpression;

        public DapperSimpleExpression(SimpleExpression simpleExpression)
        {
            Assert.NotNull(simpleExpression, "SimpleExpression must not be null");
            this.simpleExpression = simpleExpression;
        }

        public string Build(IDapperCommandBuilder commandBuilder)
        {
            var parameterName = commandBuilder.AddExpressionValue(simpleExpression.Value);

            return string.Format(
                simpleExpression.GetCommandTemplate(), 
                simpleExpression.GetPropertyName(), 
                parameterName);
        }
   }
}
