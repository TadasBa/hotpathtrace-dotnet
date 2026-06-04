using HotPathTrace.Core;

namespace HotPathTrace.Tests;

public class TradingEventReplayValidatorTests
{
    [Fact]
    public void Validate_ThrowsForMissingSequence()
    {
        var events = new[]
        {
            new TradingEvent(1, 1_000, TradingEventType.QuoteUpdated, 100_000, 10, 1),
            new TradingEvent(3, 2_000, TradingEventType.OrderSubmitted, 100_100, 20, 2)
        };

        var exception = Assert.Throws<ReplayValidationException>(() => TradingEventReplayValidator.Validate(events));

        Assert.Contains("Missing sequence number", exception.Message);
    }

    [Fact]
    public void Validate_ThrowsForOutOfOrderSequence()
    {
        var events = new[]
        {
            new TradingEvent(1, 1_000, TradingEventType.QuoteUpdated, 100_000, 10, 1),
            new TradingEvent(2, 2_000, TradingEventType.OrderSubmitted, 100_100, 20, 2),
            new TradingEvent(1, 3_000, TradingEventType.OrderCancelled, 100_100, 20, 2)
        };

        var exception = Assert.Throws<ReplayValidationException>(() => TradingEventReplayValidator.Validate(events));

        Assert.Contains("Out-of-order sequence number", exception.Message);
    }

    [Fact]
    public void Validate_ThrowsForDuplicateSequence()
    {
        var events = new[]
        {
            new TradingEvent(1, 1_000, TradingEventType.QuoteUpdated, 100_000, 10, 1),
            new TradingEvent(2, 2_000, TradingEventType.OrderSubmitted, 100_100, 20, 2),
            new TradingEvent(2, 3_000, TradingEventType.OrderCancelled, 100_100, 20, 2)
        };

        var exception = Assert.Throws<ReplayValidationException>(() => TradingEventReplayValidator.Validate(events));

        Assert.Contains("Duplicate sequence number", exception.Message);
    }
}
