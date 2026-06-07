using BenchmarkDotNet.Running;

namespace HotPathTrace.Benchmarks;

internal static class Program
{
    public static void Main(string[] args)
    {
        BenchmarkFileSizeReporter.PrintReferenceSizes();
        BenchmarkRunner.Run<EventLogFormatBenchmarks>();
    }
}
