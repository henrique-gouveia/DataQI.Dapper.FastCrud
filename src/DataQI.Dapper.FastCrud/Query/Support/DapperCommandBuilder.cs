using System;
using System.Collections.Generic;
using System.Dynamic;
using System.Linq;
using System.Runtime.CompilerServices;

namespace DataQI.Dapper.FastCrud.Query.Support
{
    public class DapperCommandBuilder : DapperExpressionBuilder, IDapperCommandBuilder
    {
        private readonly ICollection<IDapperExpressionBuilder> expressionBuilders;
        private readonly dynamic values;

        public DapperCommandBuilder()
        {
            this.expressionBuilders = new List<IDapperExpressionBuilder>();
            this.values = new ExpandoObject();
        }

        public IDapperCommandBuilder AddExpression(IDapperExpressionBuilder expression)
        {
            expressionBuilders.Add(expression);
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
            var junctionFormat = BuildFormat(expressions);

            FormattableString command = FormattableStringFactory.Create(
                junctionFormat,
                expressions.ToArray());

            return new DapperCommand(command, values);
        }

        protected override IEnumerable<IDapperExpressionBuilder> GetExpressionBuilders()
            => expressionBuilders;

        protected override string GetLogicalOperator() 
            => " AND ";
    }
}