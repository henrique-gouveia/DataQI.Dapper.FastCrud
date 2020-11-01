using DataQI.Commons.Query;
using DataQI.Commons.Query.Support;

using DataQI.Dapper.FastCrud.Query.Support;

namespace DataQI.Dapper.FastCrud.Query.Extensions
{
    public static class DapperCriterionExtensions
    {
        public static string GetCommandTemplate(this ICriterion criterion)
        {
            switch (criterion.GetWhereOperator())
            {
                case WhereOperator.Equal:
                    return "{0} = @{1}";
                case WhereOperator.GreaterThan:
                    return "{0} > @{1}";
                case WhereOperator.GreaterThanEqual:
                    return "{0} >= @{1}";
                case WhereOperator.LessThan:
                    return "{0} < @{1}";
                case WhereOperator.LessThanEqual:
                    return "{0} <= @{1}";
                case WhereOperator.Between:
                    return "{0} Between @{1} And @{2}";
                case WhereOperator.Containing:
                case WhereOperator.EndingWith:
                case WhereOperator.Like:
                case WhereOperator.StartingWith:
                    return "{0} Like @{1}";
                case WhereOperator.In:
                    return "{0} In @{1}";
                case WhereOperator.Null:
                    return "{0} Is Null";
                default:
                    return "";
            }
        }

        public static string GetLogicalOperator(this ICriterion criterion)
        {
            switch (criterion.GetWhereOperator())
            {
                case WhereOperator.And:
                    return " AND ";
                case WhereOperator.Or:
                    return " OR ";
                default:
                    return "";
            }
        }

        public static string GetNotOperator(this ICriterion criterion)
        {
            switch (criterion.GetWhereOperator())
            {
                case WhereOperator.Between:
                case WhereOperator.Containing:
                case WhereOperator.EndingWith:
                case WhereOperator.Like:
                case WhereOperator.StartingWith:
                case WhereOperator.In:
                case WhereOperator.Null:
                    return "Not ";
                case WhereOperator.Equal:
                    return "!";
                default:
                    return "";
            }            
        }

        public static IDapperExpressionBuilder GetExpressionBuilder(this ICriterion criterion)
        {
            switch (criterion.GetWhereOperator())
            {
                case WhereOperator.Between:
                    return new DapperBetweenExpression(criterion as BetweenExpression);
                case WhereOperator.In:
                    return new DapperInExpression(criterion as InExpression);
                case WhereOperator.Not:
                    return new DapperNotExpression(criterion as NotExpression);
                case WhereOperator.Null:
                    return new DapperNullExpression(criterion as NullExpression);
                case WhereOperator.And:
                case WhereOperator.Or:
                    return new DapperJunctionExpression(criterion as Junction);
                default: 
                    return new DapperSimpleExpression(criterion as SimpleExpression);
            }
        }
    }
}
