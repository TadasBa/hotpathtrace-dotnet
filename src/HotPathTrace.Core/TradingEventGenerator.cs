namespace HotPathTrace.Core;

public static class TradingEventGenerator
{
    private const int DefaultSeed = 12345;

    public static IReadOnlyList<TradingEvent> Generate(int eventCount, int? seed = null)
    {
        ArgumentOutOfRangeException.ThrowIfNegative(eventCount);

        var random = new Random(seed ?? DefaultSeed);
        var events = new List<TradingEvent>(eventCount);
        long timestampNanoseconds = 1_000_000_000;

        for (var index = 0; index < eventCount; index++)
        {
            var sequence = index + 1L;
            timestampNanoseconds += random.Next(1_000, 50_001);

            events.Add(new TradingEvent(
                Sequence: sequence,
                TimestampNanoseconds: timestampNanoseconds,
                Type: (TradingEventType)random.Next(0, 4),
                PriceTicks: random.NextInt64(10_000, 500_001),
                Quantity: random.Next(1, 1_001),
                OrderId: 100_000 + random.NextInt64(0, 900_000)));
        }

        return events;
    }
}
