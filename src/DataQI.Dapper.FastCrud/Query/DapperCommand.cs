namespace DataQI.Dapper.FastCrud.Query
{
    public struct DapperCommand
    {
        public DapperCommand(string command, object values)
        {
            Command = command;
            Values = values;   
        }

        public string Command { get; private set; }

        public object Values { get; private set; }
    }
}