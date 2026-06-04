namespace HotPathTrace.Core;

public static class TradingEventReplayValidator
{
    public static void Validate(IEnumerable<TradingEvent> events)
    {
        ArgumentNullException.ThrowIfNull(events);

        var hasPrevious = false;
        var previousSequence = 0L;

        foreach (var tradingEvent in events)
        {
            if (!hasPrevious)
            {
                if (tradingEvent.Sequence > 1)
                {
                    throw new ReplayValidationException(
                        $"Missing sequence number detected during replay. Expected sequence 1, but the first event sequence was {tradingEvent.Sequence}.");
                }

                if (tradingEvent.Sequence < 1)
                {
                    throw new ReplayValidationException(
                        $"Out-of-order sequence number detected during replay. The first event sequence must be positive and start at 1, but it was {tradingEvent.Sequence}.");
                }

                hasPrevious = true;
                previousSequence = tradingEvent.Sequence;
                continue;
            }

            var expectedSequence = previousSequence + 1;

            if (tradingEvent.Sequence == expectedSequence)
            {
                previousSequence = tradingEvent.Sequence;
                continue;
            }

            if (tradingEvent.Sequence == previousSequence)
            {
                throw new ReplayValidationException(
                    $"Duplicate sequence number detected during replay. Previous sequence was {previousSequence} and the current event repeated it.");
            }

            if (tradingEvent.Sequence < previousSequence)
            {
                throw new ReplayValidationException(
                    $"Out-of-order sequence number detected during replay. Previous sequence was {previousSequence}, but the current event sequence was {tradingEvent.Sequence}.");
            }

            throw new ReplayValidationException(
                $"Missing sequence number detected during replay. Expected sequence {expectedSequence}, but the current event sequence was {tradingEvent.Sequence}.");
        }
    }
}
