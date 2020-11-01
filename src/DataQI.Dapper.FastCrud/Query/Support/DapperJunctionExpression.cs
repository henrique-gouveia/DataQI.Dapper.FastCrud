using System;
using System.Collections.Generic;
using System.Linq;
using System.Runtime.CompilerServices;

using DataQI.Commons.Query.Support;
using DataQI.Commons.Util;

using DataQI.Dapper.FastCrud.Query.Extensions;

namespace DataQI.Dapper.FastCrud.Query.Support
{
    public class DapperJunctionExpression : DapperExpressionBuilder, IDapperExpressionBuilder
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
            var junctionFormat = $"({BuildFormat(expressions)})";

            return FormattableStringFactory.Create(
                junctionFormat, 
                expressions.ToArray());
        }

        protected override IEnumerable<IDapperExpressionBuilder> GetExpressionBuilders()
            => junction.Criterions.Select(c => c.GetExpressionBuilder());

        protected override string GetLogicalOperator()
            => junction.GetLogicalOperator();
    }
}
