using System;
using System.Collections.Generic;
using System.Linq;
using System.Runtime.CompilerServices;
using System.Text;
using DataQI.Commons.Query.Support;
using DataQI.Commons.Util;

using DataQI.Dapper.FastCrud.Query.Extensions;

namespace DataQI.Dapper.FastCrud.Query.Support
{
    public class DapperJunctionExpression : IDapperExpressionBuilder
    {
        private readonly Junction junction;

        public DapperJunctionExpression(Junction junction)
        {
            Assert.NotNull(junction, "Junction must not be null");
            this.junction = junction;
        }

        public FormattableString Build(IDapperCommandBuilder commandBuilder)
        {
            var expressions = BuildExpressions(commandBuilder);
            var junctionFormat = BuildFormat(expressions.GetEnumerator());

            return FormattableStringFactory.Create(
                junctionFormat, 
                expressions.ToArray());
        }

        // TODO: Duplicated on DapperCommandBuilder
        private IReadOnlyCollection<FormattableString> BuildExpressions(IDapperCommandBuilder commandBuilder)
        {
            var criterionsEnumerator = junction.Criterions.GetEnumerator();
            var expressions = new List<FormattableString>();

            while (criterionsEnumerator.MoveNext())
            {
                var criterion = criterionsEnumerator.Current;

                IDapperExpressionBuilder builder = criterion.GetExpressionBuilder();
                expressions.Add(builder.Build(commandBuilder));
            }

            return expressions;
        }

        // TODO: Duplicated on DapperCommandBuilder
        private string BuildFormat(IEnumerator<FormattableString> expressions)
        {
            var expressionIndex = 0;
            var formatBuilder = new StringBuilder();

            formatBuilder.Append("(");
            while (expressions.MoveNext())
            {
                if (expressionIndex > 0)
                    formatBuilder.Append(junction.GetLogicalOperator());

                formatBuilder.AppendFormat("{{{0}}}", expressionIndex);
                expressionIndex++;
            }
            formatBuilder.Append(")");

            return formatBuilder.ToString();
        }
    }
}
