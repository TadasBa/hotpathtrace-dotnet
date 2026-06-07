namespace HotPathTrace.Core;

public static class BinaryTradingEventWriter
{
    public static void Write(string filePath, IReadOnlyList<TradingEvent> events)
    {
        ArgumentException.ThrowIfNullOrWhiteSpace(filePath);
        ArgumentNullException.ThrowIfNull(events);

        var summary = SessionSummaryCalculator.Calculate(events);

        using var stream = File.Create(filePath);
        using var writer = new BinaryWriter(stream);

        writer.Write(BinaryLogFormat.MagicBytes);
        writer.Write(BinaryLogFormat.CurrentVersion);
        writer.Write(BinaryLogFormat.RecordSize);
        writer.Write(events.Count);
        writer.Write(summary.Checksum);

        foreach (var tradingEvent in events)
        {
            if (!Enum.IsDefined(tradingEvent.Type))
            {
                throw new ArgumentException($"Event sequence {tradingEvent.Sequence} contains unsupported event type value {(byte)tradingEvent.Type}.");
            }

            writer.Write(tradingEvent.Sequence);
            writer.Write(tradingEvent.TimestampNanoseconds);
            writer.Write((byte)tradingEvent.Type);
            writer.Write(tradingEvent.PriceTicks);
            writer.Write(tradingEvent.Quantity);
            writer.Write(tradingEvent.OrderId);
        }
    }
}
