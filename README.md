# HotPathTrace.NET

A C# .NET performance-learning project exploring low-allocation event logging and deterministic replay for trading-style events.

## Current status

Phases 1 and 2 are implemented.

Phase 1 is the readable baseline logger. It is intentionally simple and correct first, not the optimised logger.

Phase 2 adds a compact binary logger and binary replay validation while keeping the NDJSON baseline in place.

The baseline flow is:

1. Generate a deterministic set of synthetic trading events.
2. Write them as NDJSON, which means one JSON object per line.
3. Read the file back during replay.
4. Validate sequence numbers so missing, duplicated, or out-of-order events are reported clearly.
5. Print a small session summary with a deterministic checksum.

NDJSON is useful here because it is easy to inspect with a text editor and easy to replay line by line.

The binary format is more compact on disk:

1. A 24-byte header stores the `HPTL` magic bytes, format version, record size, event count, and expected checksum.
2. Each event record is exactly 37 bytes in this order: sequence, timestamp, type byte, price, quantity, order id.

The binary format has not yet been benchmarked. No speed or allocation claim is made at this stage.

## Planned phases

1. Correct readable baseline logger. Implemented.
2. Compact binary logger. Implemented.
3. Benchmark comparison.
4. Bounded background logging pipeline.
5. Optional ring-buffer experiment.
6. Final benchmark report and technical write-up.

## Scope

This is an educational performance-engineering experiment, not a production trading component.

## Commands

Generate a session:

```powershell
dotnet run --project src/HotPathTrace.Cli -- generate --events 10000 --output artifacts/session.ndjson --seed 42
```

Replay a session:

```powershell
dotnet run --project src/HotPathTrace.Cli -- replay --file artifacts/session.ndjson
```

Generate a binary session:

```powershell
dotnet run --project src/HotPathTrace.Cli -- generate-binary --events 10000 --output artifacts/session.bin --seed 42
```

Replay a binary session:

```powershell
dotnet run --project src/HotPathTrace.Cli -- replay-binary --file artifacts/session.bin
```
