using System.Reflection;
using BenchmarkDotNet.Columns;
using BenchmarkDotNet.Configs;
using BenchmarkDotNet.Diagnosers;
using BenchmarkDotNet.Engines;
using BenchmarkDotNet.Exporters;
using BenchmarkDotNet.Jobs;
using BenchmarkDotNet.Loggers;
using BenchmarkDotNet.Order;
using BenchmarkDotNet.Reports;
using Perfolizer.Horology;

namespace DataQI.Dapper.FastCrud.Benchmarks;

public static class Consts
{
    public const bool UseMemoryDiagnoser = true;
    public const int IterationCount = 20;
    public const int InvocationCount = 500;
    public const int IncreaseOperationCount = UseMemoryDiagnoser ? InvocationCount : 0;
    public const int OperationCount = IterationCount * InvocationCount + IncreaseOperationCount;
}

public class Config : ManualConfig
{
    public Config()
    {
        AddLogger(ConsoleLogger.Default);
        AddExporter(MarkdownExporter.GitHub);

        if (Consts.UseMemoryDiagnoser) AddDiagnoser(MemoryDiagnoser.Default);
        
        AddColumn(new LibColumn());
        AddColumn(TargetMethodColumn.Method);
        AddColumn(StatisticColumn.Mean);
        AddColumn(StatisticColumn.StdDev);
        AddColumn(StatisticColumn.Error);
        AddColumn(BaselineRatioColumn.RatioMean);
        AddColumnProvider(DefaultColumnProviders.Metrics);

        AddJob(Job.Dry
            .WithWarmupCount(0)
            .WithStrategy(RunStrategy.Monitoring)
            .WithIterationCount(Consts.IterationCount)
            .WithInvocationCount(Consts.InvocationCount)
        );

        WithSummaryStyle(SummaryStyle.Default.WithTimeUnit(TimeUnit.Millisecond));
        WithOrderer(new DefaultOrderer(SummaryOrderPolicy.Declared));
        WithOptions(Options | ConfigOptions.JoinSummary);
    }
}

public class LibColumn : IColumn
{
    public string Id => nameof(LibColumn);
    public string ColumnName => "Lib";
    public string Legend => "The lib being tested";

    public bool IsDefault(Summary summary, BenchmarkCase benchmarkCase) => false;
    public string GetValue(Summary summary, BenchmarkCase benchmarkCase)
    {
        var type = benchmarkCase.Descriptor.WorkloadMethod.DeclaringType;
        return type!.GetCustomAttribute<DescriptionAttribute>()?.Description 
               ?? type!.Name.Replace("Benchmark", string.Empty);
    }

    public string GetValue(Summary summary, BenchmarkCase benchmarkCase, SummaryStyle style) => GetValue(summary, benchmarkCase);

    public bool IsAvailable(Summary summary) => true;
    public bool AlwaysShow => true;
    public ColumnCategory Category => ColumnCategory.Job;
    public int PriorityInCategory => -10;
    public bool IsNumeric => false;
    public UnitType UnitType => UnitType.Dimensionless;
    public override string ToString() => ColumnName;
}