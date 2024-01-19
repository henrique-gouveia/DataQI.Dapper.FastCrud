namespace DataQI.Dapper.FastCrud.Benchmarks.Support;

public static class ConnectionFactory
{
    public static IDbConnection NewConnection()
    {
        var connectionString = Environment.GetEnvironmentVariable("ConnectionString");
        connectionString.Should().NotBeNull().And.NotBeEmpty();
        
        var connection = new SqlConnection(connectionString);
        connection.Open();
        
        return connection;
    }
}