using System;
using System.Runtime.CompilerServices;

using Dapper.FastCrud;

using DataQI.Commons.Query.Support;
using DataQI.Commons.Util;
using DataQI.Dapper.FastCrud.Query.Extensions;

namespace DataQI.Dapper.FastCrud.Query.Support
{
    public class DapperNullExpression : IDapperExpressionBuilder
    {
        private NullExpression nullExpression;

        public DapperNullExpression(NullExpression nullExpression)
        {
            Assert.NotNull(nullExpression, "NullExpression must not be null");
            this.nullExpression = nullExpression;
        }

        public FormattableString Build(IDapperCommandBuilder commandBuilder)
        {
            string commandTemplate = nullExpression.GetCommandTemplate();
            IFormattable columnName = Sql.Column(nullExpression.GetPropertyName());

            return FormattableStringFactory.Create(
                commandTemplate,
                columnName);
        }
    }
}