using System.Collections.Generic;
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

        public string Build(IDapperCommandBuilder commandBuilder)
        {
            var sqlWhereBuilder = new StringBuilder();
            var expressionsNumerator = BuildExpressionsBuilder().GetEnumerator();

            while (expressionsNumerator.MoveNext())
            {
                if (sqlWhereBuilder.Length > 0)
                    sqlWhereBuilder.Append(junction.GetLogicalOperator());

                var expressionBuilder = expressionsNumerator.Current;
                sqlWhereBuilder.Append(expressionBuilder.Build(commandBuilder));
            }

            return string.Format(
                junction.GetCommandTemplate(), 
                sqlWhereBuilder.ToString());
        }

        private IReadOnlyCollection<IDapperExpressionBuilder> BuildExpressionsBuilder()
        {
            var criterionsEnumerator = junction.Criterions.GetEnumerator();
            var expressions = new List<IDapperExpressionBuilder>();

            while (criterionsEnumerator.MoveNext()) 
            {
                var criterion = criterionsEnumerator.Current;
               
                IDapperExpressionBuilder builder = criterion.GetExpressionBuilder();
                expressions.Add(builder);
            }

            return expressions;       
        }
    }
}
