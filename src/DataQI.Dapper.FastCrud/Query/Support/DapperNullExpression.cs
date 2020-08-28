using DataQI.Commons.Query.Support;
using DataQI.Commons.Util;
using DataQI.Dapper.FastCrud.Query.Extensions;

namespace DataQI.Dapper.FastCrud.Query.Support
{
    public class DapperNullExpression : IDapperExpressionBuilder
    {
        private NullExpression nullExpression;

        public DapperNullExpression(NullExpression nullExpression)
        {
            Assert.NotNull(nullExpression, "NullExpression must not be null");
            this.nullExpression = nullExpression;
        }

        public string Build(IDapperCommandBuilder commandBuilder)
        {
            return string.Format(
                nullExpression.GetCommandTemplate(), 
                nullExpression.GetPropertyName());
        }
    }
}