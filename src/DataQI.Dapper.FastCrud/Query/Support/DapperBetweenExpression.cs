using DataQI.Commons.Query.Support;
using DataQI.Commons.Util;
using DataQI.Dapper.FastCrud.Query.Extensions;

namespace DataQI.Dapper.FastCrud.Query.Support
{
    public class DapperBetweenExpression : IDapperExpressionBuilder
    {
        private readonly BetweenExpression betweenExpression;

        public DapperBetweenExpression(BetweenExpression betweenExpression)
        {
            Assert.NotNull(betweenExpression, "BetweenExpression must not be null");
            this.betweenExpression = betweenExpression;
        }

        public string Build(IDapperCommandBuilder commandBuilder)
        {
            var parameterNameStarts = commandBuilder.AddExpressionValue(betweenExpression.Starts);
            var parameterNameEnds = commandBuilder.AddExpressionValue(betweenExpression.Ends);

            return string.Format(
                betweenExpression.GetCommandTemplate(), 
                betweenExpression.GetPropertyName(), 
                parameterNameStarts,
                parameterNameEnds);
        }
    }
}