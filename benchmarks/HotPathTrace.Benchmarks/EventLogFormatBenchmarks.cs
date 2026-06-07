using BenchmarkDotNet.Attributes;
using HotPathTrace.Core;

namespace HotPathTrace.Benchmarks;

[MemoryDiagnoser]
public class EventLogFormatBenchmarks
{
    private string _benchmarkDirectory = string.Empty;
    private string _readNdjsonPath = string.Empty;
    private string _readBinaryPath = string.Empty;
    private string _writeNdjsonPath = string.Empty;
    private string _writeBinaryPath = string.Empty;
    private IReadOnlyList<TradingEvent> _events = Array.Empty<TradingEvent>();

    [Params(1_000, 10_000, 100_000)]
    public int EventCount { get; set; }

    [GlobalSetup]
    public void GlobalSetup()
    {
        _events = TradingEventGenerator.Generate(EventCount, seed: 42);
        _benchmarkDirectory = Path.Combine(
            Path.GetTempPath(),
            "HotPathTrace.Benchmarks",
            $"{nameof(EventLogFormatBenchmarks)}-{EventCount}-{Guid.NewGuid():N}");

        Directory.CreateDirectory(_benchmarkDirectory);

        _readNdjsonPath = Path.Combine(_benchmarkDirectory, $"read-{EventCount}.ndjson");
        _readBinaryPath = Path.Combine(_benchmarkDirectory, $"read-{EventCount}.bin");
        _writeNdjsonPath = Path.Combine(_benchmarkDirectory, $"write-{EventCount}.ndjson");
        _writeBinaryPath = Path.Combine(_benchmarkDirectory, $"write-{EventCount}.bin");

        NdjsonTradingEventWriter.Write(_readNdjsonPath, _events);
        BinaryTradingEventWriter.Write(_readBinaryPath, _events);
    }

    [GlobalCleanup]
    public void GlobalCleanup()
    {
        if (Directory.Exists(_benchmarkDirectory))
        {
            Directory.Delete(_benchmarkDirectory, recursive: true);
        }
    }

    [Benchmark]
    public void WriteNdjson()
    {
        NdjsonTradingEventWriter.Write(_writeNdjsonPath, _events);
    }

    [Benchmark]
    public void WriteBinary()
    {
        BinaryTradingEventWriter.Write(_writeBinaryPath, _events);
    }

    [Benchmark]
    public int ReadNdjsonAndValidate()
    {
        var replayedEvents = NdjsonTradingEventReader.Read(_readNdjsonPath);
        TradingEventReplayValidator.Validate(replayedEvents);
        var summary = SessionSummaryCalculator.Calculate(replayedEvents);

        return summary.EventCount ^ unchecked((int)summary.Checksum);
    }

    [Benchmark]
    public int ReadBinaryAndValidate()
    {
        var replayedEvents = BinaryTradingEventReader.Read(_readBinaryPath);
        var summary = SessionSummaryCalculator.Calculate(replayedEvents);

        return summary.EventCount ^ unchecked((int)summary.Checksum);
    }
}
