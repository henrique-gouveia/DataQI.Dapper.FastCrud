using System;
using System.Collections.Generic;
using System.Dynamic;
using System.Linq;
using System.Runtime.CompilerServices;
using System.Text;

namespace DataQI.Dapper.FastCrud.Query.Support
{
    public class DapperCommandBuilder : IDapperCommandBuilder
    {
        private readonly ICollection<IDapperExpressionBuilder> expressions;
        private readonly dynamic values;

        public DapperCommandBuilder()
        {
            this.expressions = new List<IDapperExpressionBuilder>();
            this.values = new ExpandoObject();
        }

        public IDapperCommandBuilder AddExpression(IDapperExpressionBuilder expression)
        {
            expressions.Add(expression);
            return this;
        }

        public string AddExpressionValue(object value)
        {
            var valuesDictionary = (IDictionary<string, object>) values;
            var lastKey = valuesDictionary.Keys.LastOrDefault();

            if (int.TryParse(lastKey, out int nextKey))
                nextKey++;

            valuesDictionary.Add(nextKey.ToString(), value);
            return nextKey.ToString();
        }

        public DapperCommand Build()
        {
            var expressions = BuildExpressions(this);
            var junctionFormat = BuildFormat(expressions.GetEnumerator());

            FormattableString command = FormattableStringFactory.Create(
                junctionFormat,
                expressions.ToArray());

            return new DapperCommand(command, values);
        }

        // TODO: Duplicated on DapperJunctionExpression
        private IReadOnlyCollection<FormattableString> BuildExpressions(IDapperCommandBuilder commandBuilder)
        {
            var expressionEnumerator = expressions.GetEnumerator();
            var formattableExpression = new List<FormattableString>();

            while (expressionEnumerator.MoveNext())
            {
                var builder = expressionEnumerator.Current;
                formattableExpression.Add(builder.Build(commandBuilder));
            }

            return formattableExpression;
        }

        // TODO: Duplicated on DapperJunctionExpression
        private string BuildFormat(IEnumerator<FormattableString> expressions)
        {
            var expressionIndex = 0;
            var formatBuilder = new StringBuilder();

            while (expressions.MoveNext())
            {
                if (expressionIndex > 0)
                    formatBuilder.Append(" AND ");

                formatBuilder.AppendFormat("{{{0}}}", expressionIndex);
                expressionIndex++;
            }

            return formatBuilder.ToString();
        }
    }
}