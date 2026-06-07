namespace HotPathTrace.Core;

public static class BinaryTradingEventReader
{
    public static IReadOnlyList<TradingEvent> Read(string filePath)
    {
        ArgumentException.ThrowIfNullOrWhiteSpace(filePath);

        using var stream = File.OpenRead(filePath);
        using var reader = new BinaryReader(stream);

        ValidateMagic(ReadRequiredBytes(reader, BinaryLogFormat.MagicBytes.Length, "header magic identifier"));

        var version = ReadRequiredInt32(reader, "header format version");
        if (version != BinaryLogFormat.CurrentVersion)
        {
            throw new InvalidDataException(
                $"Unsupported binary log format version {version}. Expected version {BinaryLogFormat.CurrentVersion}.");
        }

        var recordSize = ReadRequiredInt32(reader, "header record size");
        if (recordSize != BinaryLogFormat.RecordSize)
        {
            throw new InvalidDataException(
                $"Unsupported binary log record size {recordSize}. Expected record size {BinaryLogFormat.RecordSize}.");
        }

        var eventCount = ReadRequiredInt32(reader, "header event count");
        if (eventCount < 0)
        {
            throw new InvalidDataException($"Binary log event count cannot be negative. Found {eventCount}.");
        }

        var expectedChecksum = ReadRequiredUInt64(reader, "header expected checksum");

        var events = new List<TradingEvent>(eventCount);

        for (var index = 0; index < eventCount; index++)
        {
            events.Add(ReadEventRecord(reader, index));
        }

        if (reader.BaseStream.Position != reader.BaseStream.Length)
        {
            var trailingBytes = reader.BaseStream.Length - reader.BaseStream.Position;
            throw new InvalidDataException(
                $"Binary log contains {trailingBytes} unexpected trailing byte(s) after the declared events.");
        }

        TradingEventReplayValidator.Validate(events);

        var actualSummary = SessionSummaryCalculator.Calculate(events);
        if (actualSummary.Checksum != expectedChecksum)
        {
            throw new InvalidDataException(
                $"Binary log checksum validation failed. Header checksum was 0x{expectedChecksum:X16}, but replayed events produced 0x{actualSummary.Checksum:X16}.");
        }

        return events;
    }

    private static TradingEvent ReadEventRecord(BinaryReader reader, int eventIndex)
    {
        var sequence = ReadRequiredInt64(reader, $"event record {eventIndex + 1} sequence");
        var timestampNanoseconds = ReadRequiredInt64(reader, $"event record {eventIndex + 1} timestamp");
        var eventTypeValue = ReadRequiredByte(reader, $"event record {eventIndex + 1} type");

        if (!Enum.IsDefined(typeof(TradingEventType), (int)eventTypeValue))
        {
            throw new InvalidDataException(
                $"Event record {eventIndex + 1} contained unsupported event type value {eventTypeValue}.");
        }

        var priceTicks = ReadRequiredInt64(reader, $"event record {eventIndex + 1} price");
        var quantity = ReadRequiredInt32(reader, $"event record {eventIndex + 1} quantity");
        var orderId = ReadRequiredInt64(reader, $"event record {eventIndex + 1} order id");

        return new TradingEvent(
            sequence,
            timestampNanoseconds,
            (TradingEventType)eventTypeValue,
            priceTicks,
            quantity,
            orderId);
    }

    private static void ValidateMagic(byte[] magicBytes)
    {
        if (!magicBytes.SequenceEqual(BinaryLogFormat.MagicBytes))
        {
            throw new InvalidDataException("Binary log magic identifier was invalid. Expected ASCII bytes for 'HPTL'.");
        }
    }

    private static byte[] ReadRequiredBytes(BinaryReader reader, int length, string fieldName)
    {
        var bytes = reader.ReadBytes(length);
        if (bytes.Length != length)
        {
            throw new InvalidDataException($"Binary log is truncated while reading {fieldName}.");
        }

        return bytes;
    }

    private static int ReadRequiredInt32(BinaryReader reader, string fieldName)
    {
        try
        {
            return reader.ReadInt32();
        }
        catch (EndOfStreamException exception)
        {
            throw new InvalidDataException($"Binary log is truncated while reading {fieldName}.", exception);
        }
    }

    private static long ReadRequiredInt64(BinaryReader reader, string fieldName)
    {
        try
        {
            return reader.ReadInt64();
        }
        catch (EndOfStreamException exception)
        {
            throw new InvalidDataException($"Binary log is truncated while reading {fieldName}.", exception);
        }
    }

    private static ulong ReadRequiredUInt64(BinaryReader reader, string fieldName)
    {
        try
        {
            return reader.ReadUInt64();
        }
        catch (EndOfStreamException exception)
        {
            throw new InvalidDataException($"Binary log is truncated while reading {fieldName}.", exception);
        }
    }

    private static byte ReadRequiredByte(BinaryReader reader, string fieldName)
    {
        try
        {
            return reader.ReadByte();
        }
        catch (EndOfStreamException exception)
        {
            throw new InvalidDataException($"Binary log is truncated while reading {fieldName}.", exception);
        }
    }
}
