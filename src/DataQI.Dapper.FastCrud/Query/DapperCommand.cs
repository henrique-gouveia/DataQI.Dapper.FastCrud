using System;

namespace DataQI.Dapper.FastCrud.Query
{
    public struct DapperCommand
    {
        public DapperCommand(FormattableString command, object values, FormattableString orderBy = null)
        {
            Command = command;
            Values = values;
            OrderBy = orderBy;
        }

        public FormattableString Command { get; private set; }

        public object Values { get; private set; }

        public FormattableString OrderBy { get; private set; }
    }
}
