#if DEBUG 
using BenchmarkDotNet.Configs;
#else
using DataQI.Dapper.FastCrud.Benchmarks;
#endif
using Testcontainers.MsSql;
using static DataQI.Dapper.FastCrud.Benchmarks.ConsoleEx;

WriteInfoLine("Welcome to DataQI.FastCrud's LIB performance benchmark!");
WriteLine();
WriteLine("Starting Database Setup...");
WriteWarningLine("WARNING: You'll need to have a container engine like docker running!");
WriteLine("Press any key to continue...");
ReadKey();

var sqlContainer = new MsSqlBuilder().Build();
await sqlContainer.StartAsync();
Environment.SetEnvironmentVariable("ConnectionString", sqlContainer.GetConnectionString());
DatabaseEnsurer.Execute();

WriteSuccessLine("Database setup completed.");
WriteLine();

BenchmarkSwitcher.FromAssembly(typeof(Program).Assembly).Run(args,
#if DEBUG 
    new DebugInProcessConfig()
#else
    new Config()
#endif
);