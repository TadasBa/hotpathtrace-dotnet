using HotPathTrace.Core;

namespace HotPathTrace.Benchmarks;

internal static class BenchmarkFileSizeReporter
{
    private static readonly int[] EventCounts = [1_000, 10_000, 100_000];

    public static void PrintReferenceSizes()
    {
        var workingDirectory = Path.Combine(
            Path.GetTempPath(),
            "HotPathTrace.Benchmarks",
            $"{nameof(BenchmarkFileSizeReporter)}-{Guid.NewGuid():N}");

        Directory.CreateDirectory(workingDirectory);

        try
        {
            Console.WriteLine("Reference file sizes for deterministic sample sessions:");

            foreach (var eventCount in EventCounts)
            {
                var events = TradingEventGenerator.Generate(eventCount, seed: 42);
                var ndjsonPath = Path.Combine(workingDirectory, $"{eventCount}.ndjson");
                var binaryPath = Path.Combine(workingDirectory, $"{eventCount}.bin");

                NdjsonTradingEventWriter.Write(ndjsonPath, events);
                BinaryTradingEventWriter.Write(binaryPath, events);

                var ndjsonSize = new FileInfo(ndjsonPath).Length;
                var binarySize = new FileInfo(binaryPath).Length;
                var ratio = (double)ndjsonSize / binarySize;

                Console.WriteLine(
                    $"  {eventCount:N0} events: NDJSON {ndjsonSize:N0} bytes, binary {binarySize:N0} bytes, ratio {ratio:F2}x");
            }

            Console.WriteLine();
        }
        finally
        {
            if (Directory.Exists(workingDirectory))
            {
                Directory.Delete(workingDirectory, recursive: true);
            }
        }
    }
}
