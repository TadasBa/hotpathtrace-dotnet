# HotPathTrace.NET

A C#/.NET experiment for recording and replaying deterministic trading-style events.

The project compares two log formats:

* **NDJSON** — readable, easy to inspect, useful as a correctness baseline.
* **Binary** — compact fixed-layout format with replay validation and checksum checking.

The goal is to understand how log format choices affect file size, write time, replay time, and managed memory allocation.

## What it does

* Generates deterministic synthetic events from a seed.
* Writes events to NDJSON.
* Writes events to a compact binary format.
* Replays both formats.
* Validates missing, duplicated, or out-of-order event sequences.
* Stores a checksum in the binary header to detect changed event contents.
* Benchmarks both formats with BenchmarkDotNet.

## Binary format

The binary log uses:

* a 24-byte header;
* fixed 37-byte event records.

Header fields:

```text
Magic bytes | Version | Record size | Event count | Checksum
```

Event record fields:

```text
Sequence | Timestamp | Type | Price | Quantity | OrderId
```

## Commands

Generate NDJSON:

```powershell
dotnet run --project src/HotPathTrace.Cli -- generate --events 10000 --output artifacts/session.ndjson --seed 42
```

Replay NDJSON:

```powershell
dotnet run --project src/HotPathTrace.Cli -- replay --file artifacts/session.ndjson
```

Generate binary:

```powershell
dotnet run --project src/HotPathTrace.Cli -- generate-binary --events 10000 --output artifacts/session.bin --seed 42
```

Replay binary:

```powershell
dotnet run --project src/HotPathTrace.Cli -- replay-binary --file artifacts/session.bin
```

Run tests:

```powershell
dotnet test
```

Run benchmarks:

```powershell
dotnet run --project benchmarks/HotPathTrace.Benchmarks -c Release
```

## Tech stack

* C#
* .NET 10
* xUnit
* BenchmarkDotNet

## Scope

This is a learning and measurement project. It does not connect to real exchanges, execute trades, or implement a production logging system.

## Benchmark snapshot

Local BenchmarkDotNet run on Windows 10, Intel Core i7-6500U, .NET 10.0.8.

| Scenario                  |               NDJSON |              Binary | Result                               |
| ------------------------- | -------------------: | ------------------: | ------------------------------------ |
| File size, 100,000 events |         11,459,589 B |         3,700,024 B | Binary ~3.1x smaller                 |
| Write, 100,000 events     | 101.2 ms / 30,478 KB |    28.0 ms / 4.4 KB | Binary faster, much lower allocation |
| Replay, 100,000 events    | 139.7 ms / 42,765 KB | 89.8 ms / 52,325 KB | Binary faster, but higher allocation |

These results are from one local run and should be treated as implementation-specific, not universal performance claims.
