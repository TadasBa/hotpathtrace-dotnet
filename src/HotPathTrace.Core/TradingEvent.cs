namespace HotPathTrace.Core;

public readonly record struct TradingEvent(
    long Sequence,
    long TimestampNanoseconds,
    TradingEventType Type,
    long PriceTicks,
    int Quantity,
    long OrderId);
