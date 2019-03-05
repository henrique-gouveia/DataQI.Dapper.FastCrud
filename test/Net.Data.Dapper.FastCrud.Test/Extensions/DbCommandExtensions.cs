using System.Data;

namespace Net.Data.Dapper.FastCrud.Test.Extensions
{
    public static class DbCommandExtensions
    {
        public static IDbCommand AddCommandText(this IDbCommand command, string commandText)
        {
            if (!string.IsNullOrEmpty(commandText))
                command.CommandText = commandText;

            return command;
        }

        public static IDbCommand AddParameter(this IDbCommand command, string name, object value)
        {
            var parameter = command.CreateParameter();
            parameter.ParameterName = name;
            parameter.Value = value;

            command.Parameters.Add(parameter);
            return command;
        }

        public static int PrepareAndExecuteNonQuery(this IDbCommand command)
        {
            command.Prepare();
            return command.ExecuteNonQuery();
        }

        public static IDataReader PreparaAndExecuteQuery(this IDbCommand command, CommandBehavior? behavior = null)
        {
            command.Prepare();

            if (behavior.HasValue)
                return command.ExecuteReader(behavior.Value);
            else
                return command.ExecuteReader();
        }

        public static object PrepareAndExecuteScalar(this IDbCommand command)
        {
            command.Prepare();
            return command.ExecuteScalar();
        }
    }
}