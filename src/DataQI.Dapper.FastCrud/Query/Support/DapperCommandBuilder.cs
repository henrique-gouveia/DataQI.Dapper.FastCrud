using System.Dynamic;
using System.Text;
using System.Linq;
using System.Collections.Generic;

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

            var nextKey = 0;
            if (int.TryParse(lastKey, out nextKey))
                nextKey++;            

            valuesDictionary.Add(nextKey.ToString(), value);
            return nextKey.ToString();
        }

        public DapperCommand Build()
        {
            var expressionBuilder = new StringBuilder();
            var expressionEnumerator = expressions.GetEnumerator();

            while (expressionEnumerator.MoveNext())
            {
                var expression = expressionEnumerator.Current;

                if (expressionBuilder.Length > 0)
                    expressionBuilder.Append(" AND ");

                expressionBuilder.Append(expression.Build(this));
            }

            return new DapperCommand(expressionBuilder.ToString(), values);
        }
    }
}