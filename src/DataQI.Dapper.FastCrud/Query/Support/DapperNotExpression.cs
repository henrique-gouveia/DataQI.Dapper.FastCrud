using System.Text.RegularExpressions;

using DataQI.Commons.Query.Support;
using DataQI.Commons.Util;
using DataQI.Dapper.FastCrud.Query.Extensions;

namespace DataQI.Dapper.FastCrud.Query.Support
{
    public class DapperNotExpression : IDapperExpressionBuilder
    {
        private readonly NotExpression notExpression;

        public DapperNotExpression(NotExpression notExpression)
        {
            Assert.NotNull(notExpression, "NotExpression must not be null");
            this.notExpression = notExpression;
        }

        public string Build(IDapperCommandBuilder commandBuilder)
        {
            var notOperator = notExpression.Criterion.GetNotOperator();
            var expression = notExpression
                .Criterion
                .GetExpressionBuilder()
                .Build(commandBuilder);

            var match = Regex.Match(expression, "(Between|Like|In|Null|=)");
            if (match.Success) 
            {
                expression = Regex.Replace(
                    input: expression, 
                    pattern: match.Value, 
                    replacement: $"{notOperator}{match.Value}", 
                    options: RegexOptions.IgnoreCase);
            }
 
            return expression;
        }
    }
}