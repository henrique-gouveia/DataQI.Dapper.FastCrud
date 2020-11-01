using System;
using System.Collections.Generic;
using System.Text;

namespace DataQI.Dapper.FastCrud.Query.Support
{
    public abstract class DapperExpressionBuilder
    {
        protected IReadOnlyCollection<FormattableString> BuildExpressions(IDapperCommandBuilder commandBuilder)
        {
            var formattableExpression = new List<FormattableString>();

            foreach (IDapperExpressionBuilder builder in GetExpressionBuilders())
                formattableExpression.Add(builder.Build(commandBuilder));

            return formattableExpression;
        }

        protected abstract IEnumerable<IDapperExpressionBuilder> GetExpressionBuilders();

        protected string BuildFormat(IReadOnlyCollection<FormattableString> expressions)
        {
            var formatBuilder = new StringBuilder();
            var expressionIndex = 0;

            foreach (var expression in expressions)
            {
                if (expressionIndex > 0)
                    formatBuilder.Append(GetLogicalOperator());

                formatBuilder.AppendFormat("{{{0}}}", expressionIndex);
                expressionIndex++;
            }

            return formatBuilder.ToString();
        }

        protected abstract string GetLogicalOperator();
    }
}
