using DataQI.Commons.Query;
using DataQI.Commons.Query.Support;
using DataQI.Dapper.FastCrud.Query.Extensions;

namespace DataQI.Dapper.FastCrud.Query.Support
{
    public class DapperCriteria : Criteria
    {
        public DapperCommand BuildCommand()
        {
            var commandBuilder = new DapperCommandBuilder();

            foreach (ICriterion criterion in criterions)
                commandBuilder.AddExpression(criterion.GetExpressionBuilder());

            return commandBuilder.Build();
        }
    }
}