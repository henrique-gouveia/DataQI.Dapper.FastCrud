namespace DataQI.Dapper.FastCrud.Query
{
    public interface IDapperExpressionBuilder
    {
        string Build(IDapperCommandBuilder commandBuilder);
    }
}