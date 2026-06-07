using HotPathTrace.Core;

namespace HotPathTrace.Tests;

public class BinaryTradingEventPersistenceTests
{
    [Fact]
    public void WriteAndRead_RoundTripPreservesIdenticalEvents()
    {
        var events = TradingEventGenerator.Generate(15, seed: 42);
        var filePath = CreateTestFilePath();

        BinaryTradingEventWriter.Write(filePath, events);
        var replayedEvents = BinaryTradingEventReader.Read(filePath);

        Assert.Equal(events, replayedEvents);
    }

    [Fact]
    public void OriginalAndReplayedSummaries_AreEqual()
    {
        var events = TradingEventGenerator.Generate(50, seed: 123);
        var filePath = CreateTestFilePath();

        var expectedSummary = SessionSummaryCalculator.Calculate(events);

        BinaryTradingEventWriter.Write(filePath, events);
        var replayedEvents = BinaryTradingEventReader.Read(filePath);
        var actualSummary = SessionSummaryCalculator.Calculate(replayedEvents);

        Assert.Equal(expectedSummary, actualSummary);
    }

    [Fact]
    public void Read_RejectsInvalidMagic()
    {
        var events = TradingEventGenerator.Generate(5, seed: 42);
        var filePath = CreateTestFilePath();

        BinaryTradingEventWriter.Write(filePath, events);
        WriteBytes(filePath, 0, new byte[] { (byte)'X' });

        var exception = Assert.Throws<InvalidDataException>(() => BinaryTradingEventReader.Read(filePath));

        Assert.Contains("magic identifier", exception.Message);
    }

    [Fact]
    public void Read_RejectsUnsupportedVersion()
    {
        var events = TradingEventGenerator.Generate(5, seed: 42);
        var filePath = CreateTestFilePath();

        BinaryTradingEventWriter.Write(filePath, events);
        WriteBytes(filePath, BinaryLogFormat.MagicBytes.Length, BitConverter.GetBytes(2));

        var exception = Assert.Throws<InvalidDataException>(() => BinaryTradingEventReader.Read(filePath));

        Assert.Contains("Unsupported binary log format version", exception.Message);
    }

    [Fact]
    public void Read_RejectsTruncatedBinaryData()
    {
        var events = TradingEventGenerator.Generate(5, seed: 42);
        var filePath = CreateTestFilePath();

        BinaryTradingEventWriter.Write(filePath, events);
        TruncateLastByte(filePath);

        var exception = Assert.Throws<InvalidDataException>(() => BinaryTradingEventReader.Read(filePath));

        Assert.Contains("truncated", exception.Message, StringComparison.OrdinalIgnoreCase);
    }

    [Fact]
    public void Read_RejectsChecksumMismatchWhenEventDataChanges()
    {
        var events = TradingEventGenerator.Generate(5, seed: 42);
        var filePath = CreateTestFilePath();

        BinaryTradingEventWriter.Write(filePath, events);

        var firstEventTimestampOffset = BinaryLogFormat.HeaderSize + sizeof(long);
        WriteBytes(filePath, firstEventTimestampOffset, new byte[] { 0xFF });

        var exception = Assert.Throws<InvalidDataException>(() => BinaryTradingEventReader.Read(filePath));

        Assert.Contains("checksum validation failed", exception.Message, StringComparison.OrdinalIgnoreCase);
    }

    [Fact]
    public void Read_UsesExistingSequenceValidation()
    {
        var events = TradingEventGenerator.Generate(5, seed: 42);
        var filePath = CreateTestFilePath();

        BinaryTradingEventWriter.Write(filePath, events);

        var secondRecordSequenceOffset = BinaryLogFormat.HeaderSize + BinaryLogFormat.RecordSize;
        WriteBytes(filePath, secondRecordSequenceOffset, BitConverter.GetBytes(1L));

        var exception = Assert.Throws<ReplayValidationException>(() => BinaryTradingEventReader.Read(filePath));

        Assert.Contains("Duplicate sequence number", exception.Message);
    }

    private static string CreateTestFilePath()
    {
        var directory = Path.Combine(AppContext.BaseDirectory, "TestArtifacts");
        Directory.CreateDirectory(directory);

        return Path.Combine(directory, $"{Guid.NewGuid():N}.bin");
    }

    private static void WriteBytes(string filePath, long offset, byte[] bytes)
    {
        using var stream = new FileStream(filePath, FileMode.Open, FileAccess.Write);
        stream.Position = offset;
        stream.Write(bytes, 0, bytes.Length);
    }

    private static void TruncateLastByte(string filePath)
    {
        using var stream = new FileStream(filePath, FileMode.Open, FileAccess.Write);
        stream.SetLength(stream.Length - 1);
    }
}
