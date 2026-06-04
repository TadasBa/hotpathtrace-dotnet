using HotPathTrace.Core;

namespace HotPathTrace.Tests;

public class NdjsonTradingEventPersistenceTests
{
    [Fact]
    public void WriteAndRead_RoundTripPreservesIdenticalEvents()
    {
        var events = TradingEventGenerator.Generate(15, seed: 42);
        var filePath = CreateTestFilePath();

        NdjsonTradingEventWriter.Write(filePath, events);
        var replayedEvents = NdjsonTradingEventReader.Read(filePath);

        Assert.Equal(events, replayedEvents);
    }

    [Fact]
    public void SummaryChecksum_IsTheSameBeforeWritingAndAfterReplay()
    {
        var events = TradingEventGenerator.Generate(50, seed: 123);
        var filePath = CreateTestFilePath();

        var expectedSummary = SessionSummaryCalculator.Calculate(events);

        NdjsonTradingEventWriter.Write(filePath, events);
        var replayedEvents = NdjsonTradingEventReader.Read(filePath);
        var actualSummary = SessionSummaryCalculator.Calculate(replayedEvents);

        Assert.Equal(expectedSummary, actualSummary);
    }

    private static string CreateTestFilePath()
    {
        var directory = Path.Combine(AppContext.BaseDirectory, "TestArtifacts");
        Directory.CreateDirectory(directory);

        return Path.Combine(directory, $"{Guid.NewGuid():N}.ndjson");
    }
}
