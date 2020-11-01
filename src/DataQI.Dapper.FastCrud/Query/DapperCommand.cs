using System;

namespace DataQI.Dapper.FastCrud.Query
{
    public struct DapperCommand
    {
        public DapperCommand(FormattableString command, object values)
        {
            Command = command;
            Values = values;
        }

        public FormattableString Command { get; private set; }

        public object Values { get; private set; }
    }
}