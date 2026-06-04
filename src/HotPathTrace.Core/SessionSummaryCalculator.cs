namespace HotPathTrace.Core;

public static class SessionSummaryCalculator
{
    private const ulong FnvOffsetBasis = 14695981039346656037;
    private const ulong FnvPrime = 1099511628211;

    public static SessionSummary Calculate(IEnumerable<TradingEvent> events)
    {
        ArgumentNullException.ThrowIfNull(events);

        var eventCount = 0;
        var firstSequence = 0L;
        var lastSequence = 0L;
        var checksum = FnvOffsetBasis;

        foreach (var tradingEvent in events)
        {
            if (eventCount == 0)
            {
                firstSequence = tradingEvent.Sequence;
            }

            lastSequence = tradingEvent.Sequence;
            eventCount++;

            checksum = Update(checksum, tradingEvent.Sequence);
            checksum = Update(checksum, tradingEvent.TimestampNanoseconds);
            checksum = Update(checksum, (int)tradingEvent.Type);
            checksum = Update(checksum, tradingEvent.PriceTicks);
            checksum = Update(checksum, tradingEvent.Quantity);
            checksum = Update(checksum, tradingEvent.OrderId);
        }

        return new SessionSummary(eventCount, firstSequence, lastSequence, checksum);
    }

    private static ulong Update(ulong current, long value)
    {
        return Update(current, unchecked((ulong)value));
    }

    private static ulong Update(ulong current, int value)
    {
        return Update(current, unchecked((uint)value));
    }

    private static ulong Update(ulong current, ulong value)
    {
        for (var shift = 0; shift < 64; shift += 8)
        {
            current ^= (value >> shift) & 0xFF;
            current *= FnvPrime;
        }

        return current;
    }
}
