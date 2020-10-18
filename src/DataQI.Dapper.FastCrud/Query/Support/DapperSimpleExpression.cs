using System;
using System.Runtime.CompilerServices;

using Dapper.FastCrud;

using DataQI.Commons.Query.Support;
using DataQI.Commons.Util;
using DataQI.Dapper.FastCrud.Query.Extensions;

namespace DataQI.Dapper.FastCrud.Query.Support
{
    public class DapperSimpleExpression : IDapperExpressionBuilder
    {
        private readonly SimpleExpression simpleExpression;

        public DapperSimpleExpression(SimpleExpression simpleExpression)
        {
            Assert.NotNull(simpleExpression, "SimpleExpression must not be null");
            this.simpleExpression = simpleExpression;
        }

        public FormattableString Build(IDapperCommandBuilder commandBuilder)
        {
            string commandTemplate = simpleExpression.GetCommandTemplate();
            IFormattable columnName = Sql.Column(simpleExpression.GetPropertyName());
            string parameterName = commandBuilder.AddExpressionValue(simpleExpression.Value);

            return FormattableStringFactory.Create(
                commandTemplate,
                columnName,
                parameterName);
        }
    }
}
