using System;
using System.Runtime.CompilerServices;

using Dapper.FastCrud;

using DataQI.Commons.Query.Support;
using DataQI.Commons.Util;
using DataQI.Dapper.FastCrud.Query.Extensions;

namespace DataQI.Dapper.FastCrud.Query.Support
{
    public class DapperInExpression : IDapperExpressionBuilder
    {
        private readonly InExpression inExpression;

        public DapperInExpression(InExpression inExpression)
        {
            Assert.NotNull(inExpression, "InExpression must not be null");
            this.inExpression = inExpression;
        }

        public FormattableString Build(IDapperCommandBuilder commandBuilder)
        {
            string commandTemplate = inExpression.GetCommandTemplate();
            IFormattable columnName = Sql.Column(inExpression.GetPropertyName());
            string parameterName = commandBuilder.AddExpressionValue(inExpression.Values);

            return FormattableStringFactory.Create(
                commandTemplate,
                columnName,
                parameterName);
        }
    }
}