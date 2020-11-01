using System;
using System.Runtime.CompilerServices;
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

        public FormattableString Build(IDapperCommandBuilder commandBuilder)
        {
            FormattableString expression = notExpression
                .Criterion
                .GetExpressionBuilder()
                .Build(commandBuilder);

            string commandTemplate = expression.Format;
            string notOperator = notExpression.Criterion.GetNotOperator();

            var match = Regex.Match(commandTemplate, "(Between|Like|In|Null|=)");
            if (match.Success)
            {
                commandTemplate = Regex.Replace(
                    input: commandTemplate,
                    pattern: match.Value,
                    replacement: $"{notOperator}{match.Value}",
                    options: RegexOptions.IgnoreCase);
            }

            return FormattableStringFactory.Create(
                commandTemplate,
                expression.GetArguments());
        }
    }
}