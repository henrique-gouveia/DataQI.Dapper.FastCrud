using System;

namespace DataQI.Dapper.FastCrud.Query.Support
{
    internal sealed class DapperPredicate
    {
        public DapperPredicate(FormattableString command, object values, bool isEmpty)
        {
            Command = command;
            Values = values;
            IsEmpty = isEmpty;
        }

        public FormattableString Command { get; }

        public object Values { get; }

        public bool IsEmpty { get; }
    }
}
