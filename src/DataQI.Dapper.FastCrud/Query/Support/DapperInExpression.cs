using DataQI.Commons.Query.Support;
using DataQI.Commons.Util;
using DataQI.Dapper.FastCrud.Query.Extensions;

namespace DataQI.Dapper.FastCrud.Query.Support
{
    public class DapperInExpression : IDapperExpressionBuilder
    {
        private readonly InExpression inExpression;

        public DapperInExpression(InExpression inExpression)
        {
            Assert.NotNull(inExpression, "InExpression must not be null");
            this.inExpression = inExpression;
        }

        public string Build(IDapperCommandBuilder commandBuilder)
        {
            var parameterName = commandBuilder.AddExpressionValue(inExpression.Values);

            return string.Format(
                inExpression.GetCommandTemplate(), 
                inExpression.GetPropertyName(), 
                parameterName);
        }
    }
}