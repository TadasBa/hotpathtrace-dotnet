# HotPathTrace.NET

A C# .NET performance-learning project exploring low-allocation event logging and deterministic replay for trading-style events.

## Current status

Phases 1, 2, and 3 are implemented.

Phase 1 is the readable baseline logger. It is intentionally simple and correct first, not the optimised logger.

Phase 2 adds a compact binary logger and binary replay validation while keeping the NDJSON baseline in place.

Phase 3 adds BenchmarkDotNet benchmarks that compare the current NDJSON and binary formats for write and replay scenarios.

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

The binary format is now benchmarked with BenchmarkDotNet in Release mode, but no final performance claim should go beyond the actual benchmark output you collect on your machine.

## Planned phases

1. Correct readable baseline logger. Implemented.
2. Compact binary logger. Implemented.
3. Benchmark comparison. Implemented.
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

Run benchmarks:

```powershell
dotnet run --project benchmarks/HotPathTrace.Benchmarks -c Release
```

Benchmarks should be run in Release mode. Debug builds can distort timing and memory results.

## Benchmark results

Add your measured BenchmarkDotNet summary table here after a Release run on your machine.
