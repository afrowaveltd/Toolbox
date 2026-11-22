# Opt-in journal I/O stage diagnostics

## Scope and rationale

The MdView container cursor builds a persistent temporary event journal
for classified Markdown physical source lines. Each new source line is
serialised as a JSON event with a four-byte little-endian length, while
a separate journal stores its eight-byte event offset.

In a real 2026-11-22 Windows H: test, a **2-GiB synthetic Markdown file**
was fully processed with **1,614,865 physical lines**, an event journal
of **5,022,177,346 bytes**, all **200** sampled source-line seek
checks correct, and a peak process working set of **72,130,560 bytes**.
The scan took **123.167 seconds** with several unexplained wall-time stalls
inside `MdViewContainerPageCursor.ReadAsync`. Collection pauses were
not long enough to explain those stalls. The logs do **not** prove a
hardware problem or an exact Windows I/O bottleneck.

The existing cursor writes an event header, event payload and offset
via separate `FileStream.WriteAsync` operations for each line; the
2-GiB fixture represents roughly **4.84 million logical write calls**.
Before optimizing, we need to identify whether elapsed time accumulates
in scanner advancement, serialization, or a specific write operation.

## Optional implementation details

`MdViewJournalScanTimings` is **internal**, accessible to the separate
benchmark and regression-test assemblies via existing friend access.
The production cursor's `ScanTimings` property is **null by default**.
When null, each stage skips high-resolution timestamp collection and
does not create per-line diagnostic objects.

When enabled, eight separate, non-overlapping logical operation
boundaries are measured. The first five were introduced before the
128-MiB profiling baseline; the remaining three were added afterward
to cover previously unaccounted journal replay:

1. `ScannerAdvance` — await incremental scanner `MoveNextAsync`.
2. `SerializeEvent` — UTF-8 JSON event serialization.
3. `EventLengthWrite` — await the four-byte record-length write.
4. `EventPayloadWrite` — await the JSON record write.
5. `OffsetWrite` — await the eight-byte source-line-offset write.
6. `OffsetTableRead` — await the contiguous offset-table read.
7. `EventWindowRead` — await the bounded contiguous event read.
8. `DeserializeEvent` — parse a stored JSON event back into a line.

Each operation updates fixed-size in-RAM counters: number of completed
calls, cumulative ticks, worst call, number of calls lasting at
least 100 ms, and source line/journal offset for the worst call.
`TakeWindowAndReset()` returns an immutable per-progress snapshot.
No diagnostic file, console write, reflection serialization, unbounded
collection or external telemetry call happens from the cursor.

These are **logical await boundaries**. Even if a write reports a large
elapsed time, it may include runtime and OS scheduling, managed code,
filesystem buffering or device I/O. It is **not** a direct measurement
of actual physical platter writes or kernel I/O operations. The sum of instrumented stages includes **both journal
construction and its readback**, but excludes other cursor
work such as buffer allocation, offset validation, line
copying and collection management. It also excludes the
benchmark's own instrumentation outside the cursor.

## Benchmark activation

Normal (unprofiled) measurements remain comparable to previous
results. The trace is an explicit opt-in:

```powershell
# Run from Toolbox root, using the prepared H: source.
.\Blackboard\Tools\MdView.Benchmarks\run.cmd measure -SizeMiB 1024 -Seeks 100 -Passes 1 -ProgressStep 1 -TraceJournalIo
```

On Linux, after verifying the separate HDD mount:

```bash
bash Blackboard/Tools/MdView.Benchmarks/run.sh /your/mounted/disk measure --size-mib 128 --seeks 100 --passes 1 --progress-step 1 --trace-journal-io
```

The harness adds a nested `journal_scan_stages` object to each
`journal_progress` event only when the trace is enabled; it is
`null` otherwise. For every stage it records `Calls`, `TotalMs`,
`MaxMs`, `CallsAtLeast100Ms`, `MaxSourceLine` and
`MaxJournalOffset`. The object also contains `AccountedMs`
(the sum of the eight measured stages). Compare this sum with
`interval_cursor_read_total_ms` and `interval_elapsed_ms`,
mindful that tracing per-line stages adds measurement overhead.
The benchmark's existing RAM-buffered logger persists JSONL/TXT
only **after** the measured run.

The 128-MiB five-stage diagnostic test completed successfully
on the physical Windows H: HDD on 2026-11-22 with 200/200 sampled
seek checks. It left approximately **2.77 seconds** of the
4.16-second journal scan outside the measured five stages; this
motivated adding the three readback stages. The **next experiment**
should use the unchanged **1-GiB** fixture with all eight stage
counters. Do not infer disk failure or a speed improvement from
one traced run. Do not call a smoke-test CI run a physical-HDD
throughput benchmark.

## Correctness and safety

All existing journal bounds, read-back format, cancellation checks,
faulted-cursor handling, line numbering and source ownership remain
unchanged. The xUnit regression test
`OptionalJournalTimingsMeasureEachWriteWithoutChangingJournal`
checks stage-call counts including readback, exact source
text, cached backward navigation without new *writes* but
with fresh *reads*, partial forward continuation and
resettable counters. The existing MdView test suite continues to
cover cancellation, journal-cap exhaustion and offset-table limits.

Neither write batching nor a persisted format change is part of
this stage. Once field measurements identify a material bottleneck,
the next step can be an isolated batching experiment with strict
regressions and before/after HDD measurements.

See [benchmark methodology](../../../MdView.Benchmarks/Docs/HDD-Benchmark/en.md)
and [results from 2026-11-22](../../../MdView.Benchmarks/Docs/Results-2026-11-22/en.md).
