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

When enabled, **twelve separate, non-overlapping logical operation
boundaries** are measured. The first five were introduced before
the 128-MiB profiling baseline; three readback stages were added
before the 1-GiB profile. That profile exposed a very long
unaccounted synchronous operation, motivating four more stages
around stream-position setters:

1. `ScannerAdvance` — await incremental scanner `MoveNextAsync`.
2. `SerializeEvent` — UTF-8 JSON event serialization.
3. `EventLengthWrite` — await the four-byte record-length write.
4. `EventPayloadWrite` — await the JSON record write.
5. `OffsetWrite` — await the eight-byte source-line-offset write.
6. `OffsetTableRead` — await the contiguous offset-table read.
7. `EventWindowRead` — await the bounded contiguous event read.
8. `DeserializeEvent` — parse a stored JSON event back into a line.
9. `EventWritePosition` — set `_events.Position` before appending.
10. `OffsetWritePosition` — set `_offsets.Position` before appending.
11. `OffsetReadPosition` — set `_offsets.Position` before indexed read.
12. `EventReadPosition` — set `_events.Position` before bulk event read.

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
(the sum of the twelve measured stages). Compare this sum with
`interval_cursor_read_total_ms` and `interval_elapsed_ms`,
mindful that tracing per-line stages adds measurement overhead.
The benchmark's existing RAM-buffered logger persists JSONL/TXT
only **after** the measured run.

The 128-MiB five-stage diagnostic test completed successfully
on the physical Windows H: HDD on 2026-11-22 with 200/200 sampled
seek checks. It left approximately **2.77 seconds** of the
4.16-second journal scan outside the measured five stages; this
motivated adding three readback stages.

The subsequent **1-GiB eight-stage** test
`mdview-hdd-20261122T145716681Z-543ebae1`
also passed all 200 seeks, but its **67–68%** interval
lasted **23,035 ms**, including a **14,804-ms**
`ReadAsync`. All eight measured stages together
totalled just **75.655 ms** in that interval,
and GC pauses were **7.797 ms**. None of the
eight stages showed a call of 100 ms or longer.
The test therefore did not explain that stall,
but narrows the missing time to other cursor
work, including synchronous `FileStream.Position`
setters which previously occurred *outside*
the read/write timing windows.

**The next experiment** uses twelve stage counters,
with the four stream-position stages added. They
measure the *setter itself*, and may reveal stalls
caused by filesystem buffering or runtime behavior
when transitioning between reads and writes.
This remains a hypothesis: such times still
are not hardware device metrics. Do not call
a smoke-test CI run a physical-HDD benchmark.

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

## Observed cause and minimal corrective experiment (2026-11-22)

The 1-GiB, twelve-stage physical Windows H: HDD trace
`mdview-hdd-20261122T150433389Z-44ba10df`
completed correctly (200/200 indexed seek checks).
Journal scan elapsed: **46,506.152 ms**. The 12 measured
stages accounted for **45,595.327 ms** of this phase.

The position stages dominated:

| Stage | Total | Longest observed setter |
| --- | ---: | ---: |
| `EventWritePosition` | **11,659.125 ms** | **1,506.843 ms** |
| `OffsetWritePosition` | **25,440.455 ms** | **13,489.504 ms** |

The 69–70% and 70–71% progress windows together took
**23,363.713 ms**. The largest 69–70% delay happened
while setting `_offsets.Position` at source line
**564,885**, journal offset **1,756,107,492 B**.
Unlike previous eight-stage traces, the synchronous
position setter now accounts for the delay. A
position assignment on a buffered stream can entail
more work than an in-memory pointer update; the
profile does **not** identify an HDD media defect or
prove a specific kernel flush.

### Implemented experiment

Both journal append paths now use:

```csharp
if (stream.Position != requiredEndPosition)
{
    stream.Position = requiredEndPosition;
    actualSetterCount++;
}
```

The getter remains an O(1)-sized state lookup in
typical implementations, but its actual cost is
included in the existing `EventWritePosition` /
`OffsetWritePosition` stage timers during manual
profiling. Already-correct positions are not reset.
After navigation to a cached older page changes the
file cursor, the subsequent append repositions once.
The internal counters distinguish actual setters
from timing-stage calls, which remain one per appended
record when profiling.

The benchmark now includes **cumulative**
`event_write_position_setter_calls` and
`offset_write_position_setter_calls` per
`journal_progress` entry. Tests require zero
append seek setters across an initial 128-row page,
and exactly one per stream after each backward
read followed by another forward 128-row page.
This is a scoped intervention, not a journal
format migration or an attempt to bypass size
limits or cancellation.

**Measure again before declaring success:**

```powershell
git pull
.\Blackboard\Tools\MdView.Benchmarks\run.cmd measure -SizeMiB 1024 -Seeks 100 -Passes 1 -ProgressStep 1 -TraceJournalIo
```

Compare total journal time, source correctness,
memory peak, both cumulative setter counts and the
twelve-stage position totals. The first-pass cache
state and HDD behavior are not controlled enough to
claim a precise speedup from a single comparison.

## 1-GiB post-change regression and controlled A/B experiment

The physical Windows H: measurement
`mdview-hdd-20261122T154929922Z-93528de2`
confirmed that the conditional append mode performed **0**
`EventWritePositionSetterCount` and **0**
`OffsetWritePositionSetterCount` actual assignments throughout
the 807,433-row scan. The journal is byte-for-byte equal in length
(**2,510,275,305 bytes**) to the earlier trace, with all
**200/200** indexed seek samples correct.
The journal scan nevertheless took **125,347.666 ms**
(previous: **46,506.152 ms**). The largest stage became
`EventPayloadWrite` (**115,955.056 ms**), i.e. **92.5%**
of the new journal phase. Append-position timer total dropped
from **37,099.580 ms** to **47.697 ms**. The latency did not
disappear; it moved from the position setters to the
payload write awaits and increased overall.

This does not prove the whole difference is due to that
single change, because the physical HDD, its write cache,
antivirus activity and thermal/cache state are uncontrolled.
The test also does not demonstrate a defective physical disk.

For controlled comparison, the cursor now exposes an
**internal** `ForceJournalAppendPositionSetters` A/B
switch, used only by the manual benchmark. When enabled,
both append streams set `Position` for **every** row, as
they did before the conditional-append change. With the
default false, only genuinely needed position changes
are performed. Tests cover 384 rows, navigation backwards
and forwards, fence preservation and exact setter counts
in each variant. There are no changes to physical journal
format, readback API, disk limits, or cancellation behavior.

Benchmark controls:

```powershell
# B: current conditional behavior
.\Blackboard\Tools\MdView.Benchmarks\run.cmd measure -SizeMiB 1024 -Seeks 100 -Passes 1 -ProgressStep 1 -TraceJournalIo

# A: original unconditional Position calls (benchmark only)
.\Blackboard\Tools\MdView.Benchmarks\run.cmd measure -SizeMiB 1024 -Seeks 100 -Passes 1 -ProgressStep 1 -TraceJournalIo -ForceAppendPositionSetters
```

The `session_start` diagnostic record now names
`journal_append_mode` explicitly, and
`event_write_position_setter_calls` and
`offset_write_position_setter_calls` must be zero
for the uninterrupted conditional scan, or grow to
807,433 each for forced mode.

An alternating **B/A/B** sequence (or A/B/A when comparing
from a B baseline) is more informative than one
before/after pair. Keep the prepared source, machine,
physical disk, seek count and progress settings fixed.
Each phase is a separate process and preserves old logs.
Stop if disk-health monitoring suggests developing failure.

## Forced legacy A/B physical validation — 18:00 UTC

The physical Windows H: 1-GiB benchmark
`mdview-hdd-20261122T180043896Z-568a1d79`
selected `legacy_unconditional_position_setters`
via the benchmark-only internal override.
In this successful run, `807,433` scanned
source lines produced exactly `807,433`
actual assignments to **each** append
stream's `Position`. All 200 sampled
source-line seeks passed. Peak working set
was **68.39 MiB**.

The journal time returned to **47.552 s**,
near the prior original-code A baseline
of **46.506 s** and markedly faster
than the current conditional B time
of **125.348 s** (2.636× difference).
The critical cost distribution reversed:

- forced A: `EventWritePosition=32.208 s`,
  `OffsetWritePosition=5.867 s`,
  `EventPayloadWrite=0.110 s`;
- conditional B: `EventWritePosition=0.025 s`,
  `OffsetWritePosition=0.022 s`,
  `EventPayloadWrite=115.955 s`.

The current likely cause is changed
`FileStream` buffering/write-flush
behavior when unconditional `Position`
assignments are removed. This is an
**inference**, not a verified runtime
implementation detail. Notably A
still has a ~25 s stall near 72–73%
as the event journal grows past
~1.7–1.8 GB; the previous A saw
a similar-size stall near 70–71%.
That remains independent of the
sustained B regression.

**Next:** complete B–A–B by repeating
the conditional variant, unchanged,
on the same prepared physical source.
If reproduced, revert conditional
append-position assignments to the
working original behavior, then verify
on Windows and Linux, and document.
No production behavior changed on the
basis of this first A repeat alone.

## Decision after completing the physical B–A–B (18:19 UTC)

The third experiment
`mdview-hdd-20261122T181955063Z-bfed61a5`
repeated the conditional B path on the same
1-GiB Windows H: test. It completed successfully:
807,433 rows, the unchanged 2,510,275,305-byte
journal, 200/200 correct line-seek checks,
and 70.84 MiB peak working set.
Journal wall time: **125.310 s**;
`EventPayloadWrite`: **115.917 s**;
zero actual append-position assignments.
The final B differs by **0.030%** from the
first B (**125.348 s**). The intervening
original A run took **47.552 s**.
The conditional variant is therefore
reproducibly **2.635× slower** on the
measured physical HDD workload.

**Decision implemented** in
`a78133bb19003f31ec3bdf4225fe485529dcf3f8`:
production cursor defaults to unconditional
event and offset `FileStream.Position`
assignments on every append (A), restoring
the behavior validated by the earlier and
forced-original A runs. The optional B
behavior remains available **only by
explicit internal benchmark selection**.
This preserves the exact journal format
and RAM/disk budgets.

To reproduce after this commit:

```powershell
# A (current production and benchmark default)
.\\Blackboard\\Tools\\MdView.Benchmarks\\run.cmd measure -SizeMiB 1024 -Seeks 100 -Passes 1 -ProgressStep 1 -TraceJournalIo

# B (experimental, slower on this Windows HDD)
.\\Blackboard\\Tools\\MdView.Benchmarks\\run.cmd measure -SizeMiB 1024 -Seeks 100 -Passes 1 -ProgressStep 1 -TraceJournalIo -ConditionalAppendPositionSetters
```

For Linux direct CLI, the second switch is
`--conditional-append-position-setters`.
The historical `-ForceAppendPositionSetters`
still selects A, now redundant; simultaneous
A and B switches fail validation.

**Separate open issue:** A still experiences
occasional multi-second stalls near 70–73%
of the ~2.51-GB journal. These are distinct
from B's continuous slow payload writes.
Do not attribute either to damaged media
or claim a low-level FileStream flush
mechanism without additional evidence.
