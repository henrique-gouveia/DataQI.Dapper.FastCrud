using System;
using System.Runtime.CompilerServices;

using Dapper.FastCrud;

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

        public FormattableString Build(IDapperCommandBuilder commandBuilder)
        {
            string commandTemplate = betweenExpression.GetCommandTemplate();
            IFormattable columnName = Sql.Column(betweenExpression.GetPropertyName());
            string parameterNameStarts = commandBuilder.AddExpressionValue(betweenExpression.Starts);
            string parameterNameEnds = commandBuilder.AddExpressionValue(betweenExpression.Ends);

            return FormattableStringFactory.Create(
                commandTemplate, 
                columnName, 
                parameterNameStarts, 
                parameterNameEnds);
        }
    }
}