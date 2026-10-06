using System;

namespace DataQI.Dapper.FastCrud.Query.Support
{
    internal sealed class DapperPredicate
    {
        public DapperPredicate(FormattableString command, object values)
        {
            Command = command;
            Values = values;
        }

        public FormattableString Command { get; }

        public object Values { get; }
    }
}
