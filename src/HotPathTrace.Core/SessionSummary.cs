namespace HotPathTrace.Core;

public readonly record struct SessionSummary(
    int EventCount,
    long FirstSequence,
    long LastSequence,
    ulong Checksum);
