namespace DataQI.Dapper.FastCrud.Query
{
    public interface IDapperCommandBuilder
    {
        IDapperCommandBuilder AddExpression(IDapperExpressionBuilder expression);

        string AddExpressionValue(object value);

        DapperCommand Build();
    }
}