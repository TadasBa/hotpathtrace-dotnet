# HotPathTrace.NET

A C# .NET performance-learning project exploring low-allocation event logging and deterministic replay for trading-style events.

## Current status

Phase 1 is implemented.

This phase is the readable baseline logger. It is intentionally simple and correct first, not the optimised logger.

The baseline flow is:

1. Generate a deterministic set of synthetic trading events.
2. Write them as NDJSON, which means one JSON object per line.
3. Read the file back during replay.
4. Validate sequence numbers so missing, duplicated, or out-of-order events are reported clearly.
5. Print a small session summary with a deterministic checksum.

NDJSON is useful here because it is easy to inspect with a text editor and easy to replay line by line.

## Planned phases

1. Correct readable baseline logger. Implemented.
2. Compact binary logger.
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
