using System;

namespace DataQI.Dapper.FastCrud.Query
{
    public interface IDapperExpressionBuilder
    {
        FormattableString Build(IDapperCommandBuilder commandBuilder);
    }
}