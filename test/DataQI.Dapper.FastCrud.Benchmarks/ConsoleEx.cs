namespace DataQI.Dapper.FastCrud.Benchmarks;

public static class ConsoleEx
{
    public static void WriteLine()
        => Console.WriteLine();
    
    public static void WriteLine(string? value)
        => Console.WriteLine(value);
    
    public static void WriteInfoLine(string? value)
        => WriteColorLine(value, ConsoleColor.Cyan);

    public static void WriteWarningLine(string? value)
        => WriteColorLine(value, ConsoleColor.Yellow);

    public static void WriteSuccessLine(string? value)
        => WriteColorLine(value, ConsoleColor.Green);

    public static void WriteDangerLine(string? value)
        => WriteColorLine(value, ConsoleColor.Red);

    public static void WriteColorLine(string? value, ConsoleColor color)
    {
        var defaultColor = Console.ForegroundColor; 
        try
        {
            Console.ForegroundColor = color;
            Console.WriteLine(value);
        }
        finally
        {
            Console.ForegroundColor = defaultColor;
        }
    }
    
    public static ConsoleKeyInfo ReadKey() => Console.ReadKey(); 
}