using HotPathTrace.Core;

namespace HotPathTrace.Tests;

public class TradingEventGeneratorTests
{
    [Fact]
    public void Generate_ReturnsRequestedEventCount()
    {
        var events = TradingEventGenerator.Generate(25, seed: 42);

        Assert.Equal(25, events.Count);
    }

    [Fact]
    public void Generate_CreatesContiguousSequenceNumbersStartingAtOne()
    {
        var events = TradingEventGenerator.Generate(10, seed: 42);

        for (var index = 0; index < events.Count; index++)
        {
            Assert.Equal(index + 1, events[index].Sequence);
        }
    }

    [Fact]
    public void Generate_IsDeterministicForTheSameSeed()
    {
        var firstRun = TradingEventGenerator.Generate(20, seed: 7);
        var secondRun = TradingEventGenerator.Generate(20, seed: 7);

        Assert.Equal(firstRun, secondRun);
    }

    [Fact]
    public void Generate_CreatesMonotonicallyIncreasingTimestamps()
    {
        var events = TradingEventGenerator.Generate(30, seed: 99);

        for (var index = 1; index < events.Count; index++)
        {
            Assert.True(
                events[index].TimestampNanoseconds > events[index - 1].TimestampNanoseconds,
                $"Timestamp at index {index} should be greater than the previous timestamp.");
        }
    }
}
