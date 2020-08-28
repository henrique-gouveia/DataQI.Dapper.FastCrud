using DataQI.Commons.Query.Support;
using DataQI.Dapper.FastCrud.Query.Extensions;

namespace DataQI.Dapper.FastCrud.Query.Support
{
    public class DapperCriteria : Criteria
    {
        public DapperCommand BuildCommand()
        {
            var commandBuilder = new DapperCommandBuilder();

            var criterionsEnumerator = criterions.GetEnumerator();
            while (criterionsEnumerator.MoveNext()) 
            {
                var criterion = criterionsEnumerator.Current;
                commandBuilder.AddExpression(criterion.GetExpressionBuilder());
            }

            return commandBuilder.Build();
        }
    }
}