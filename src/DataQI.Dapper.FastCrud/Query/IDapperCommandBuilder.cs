using DataQI.Commons.Query;

namespace DataQI.Dapper.FastCrud.Query
{
    public interface IDapperCommandBuilder
    {
        IDapperCommandBuilder AddExpression(IDapperExpressionBuilder expression);

        IDapperCommandBuilder AddOrder(IOrderCriterion orderCriterion);

        string AddExpressionValue(object value);

        DapperCommand Build();
    }
}
