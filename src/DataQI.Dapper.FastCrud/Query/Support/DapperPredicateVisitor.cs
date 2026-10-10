using System;
using System.Collections.Generic;
using System.Dynamic;
using System.Runtime.CompilerServices;

using Dapper.FastCrud;

using DataQI.Commons.Query;
using DataQI.Commons.Query.Ast;

namespace DataQI.Dapper.FastCrud.Query.Support
{
    internal sealed class DapperPredicateVisitor : ICriterionVisitor<FormattableString>
    {
        private readonly IDictionary<string, object> parameters = new ExpandoObject();

        private DapperPredicateVisitor() { }

        public FormattableString Visit(Comparison comparison)
        {
            var column = Sql.Column(comparison.PropertyName);
            if (comparison.Kind == ComparisonKind.Equal && comparison.Value == null)
                return FormattableStringFactory.Create("{0} Is Null", column);

            var parameterName = Bind(comparison.Value);
            return FormattableStringFactory.Create(
                $"{{0}} {Symbol(comparison.Kind)} @{{1}}", column, parameterName);
        }

        public FormattableString Visit(Between between)
        {
            var column = Sql.Column(between.PropertyName);
            var startsParam = Bind(between.Starts);
            var endsParam = Bind(between.Ends);
            return FormattableStringFactory.Create(
                "{0} Between @{1} And @{2}", column, startsParam, endsParam);
        }

        public FormattableString Visit(In inCriterion)
        {
            var column = Sql.Column(inCriterion.PropertyName);
            var parameterName = Bind(inCriterion.Values);
            return FormattableStringFactory.Create("{0} In @{1}", column, parameterName);
        }

        public FormattableString Visit(IsNull isNull)
        {
            var column = Sql.Column(isNull.PropertyName);
            return FormattableStringFactory.Create("{0} Is Null", column);
        }

        public FormattableString Visit(TextMatch textMatch)
        {
            var column = Sql.Column(textMatch.PropertyName);
            var parameterName = Bind(textMatch.Value);
            return FormattableStringFactory.Create("{0} Like @{1}", column, parameterName);
        }

        public FormattableString Visit(Not not)
        {
            var inner = not.Inner.Accept(this);
            return FormattableStringFactory.Create("Not ({0})", inner);
        }

        public FormattableString Visit(Junction junction)
        {
            if (junction.Members.Count == 0)
                throw new InvalidOperationException($"Junction '{junction.Kind}' must contain at least one criterion.");

            var members = new List<FormattableString>();
            foreach (var member in junction.Members)
                members.Add(member.Accept(this));

            var separator = junction.Kind == LogicalKind.And ? " AND " : " OR ";
            var format = $"({JoinPlaceholders(members.Count, separator)})";

            return FormattableStringFactory.Create(format, members.ToArray());
        }

        private static string JoinPlaceholders(int count, string separator)
        {
            var placeholders = new string[count];
            for (int i = 0; i < count; i++)
                placeholders[i] = $"{{{i}}}";
            return string.Join(separator, placeholders);
        }

        private static string Symbol(ComparisonKind kind)
        {
            switch (kind)
            {
                case ComparisonKind.GreaterThan: return ">";
                case ComparisonKind.GreaterThanEqual: return ">=";
                case ComparisonKind.LessThan: return "<";
                case ComparisonKind.LessThanEqual: return "<=";
                case ComparisonKind.Equal:
                default: return "=";
            }
        }

        private string Bind(object value)
        {
            var parameterName = $"p{parameters.Count}";
            parameters.Add(parameterName, value);
            return parameterName;
        }

        public static DapperPredicate BuildPredicate(ICriteria criteria)
        {
            var visitor = new DapperPredicateVisitor();
            var expressions = new List<FormattableString>();
            foreach (var criterion in criteria.Criterions)
                expressions.Add(criterion.Accept(visitor));

            var placeholders = new string[expressions.Count];
            for (int i = 0; i < expressions.Count; i++)
                placeholders[i] = $"{{{i}}}";
            var format = string.Join(" AND ", placeholders);

            var command = FormattableStringFactory.Create(format, expressions.ToArray());

            return new DapperPredicate(command, visitor.parameters);
        }
    }
}
