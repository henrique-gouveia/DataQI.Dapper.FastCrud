namespace SimpleApi;

public static class DbConnectionExtensions
{
    public static void EnsureDatabaseCreated(this IDbConnection connection)
    {
        using var command = connection.CreateCommand();
        command.CommandText = @"
CREATE TABLE IF NOT EXISTS [entities] (
    [id] INTEGER PRIMARY KEY AUTOINCREMENT,
    [name] VARCHAR(60),
    [created_at] DATE
);
";
        connection.Open();
        command.ExecuteNonQuery();
    }
}