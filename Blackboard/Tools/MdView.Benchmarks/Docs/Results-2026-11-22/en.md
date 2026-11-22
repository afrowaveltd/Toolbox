# Windows H: slow-HDD baseline — 2026-11-22

## Evidence

The maintainer provided the complete text and JSONL logs of two
successful runs, plus the Windows system report, recorded in UTC:

- `system-20261122T123405Z-25680.txt`
- `mdview-hdd-20261122T123408962Z-04a6b26f.{txt,jsonl}`
- `mdview-hdd-20261122T123503121Z-154ce17d.{txt,jsonl}`

These are **real-HDD observations** from the maintainer's machine,
not GitHub Actions simulations. Individual benchmark readings are
not proofs of cross-machine performance or physical platter I/O.

## Verified test setup

- Windows NT 10.0.26340, 32 logical CPU cores.
- Target `H:\MDViewTest`, NTFS on a USB-attached
  Hitachi HTS541680J9SA00, disk 8, nominal 80 GB.
- Windows system disk is disk 7; physical disk isolation
  check passed. Temporary root `H:\MDViewTest\tmp`.
- Benchmark runtime **.NET 10.0.14**, with local
  `dotnet --version` returning
  `11.0.100-rc.3.26522.148` (SDK and runtime differ).
- About 79.9 billion decimal bytes were available at
  the initial smoke-test preflight.

## 8-MiB smoke test (`all --size-mib 8 --seeks 10`)

- **PASS**: `session_completed`; no `session_failed`.
  Synthetic source generated at exactly 8,388,608 bytes
  (6,274 physical lines).
- Generation: **146.648 ms**, **54.552 MiB/s** logical.
- Validated snapshot: **282.257 ms**, **28.343 MiB/s** logical;
  only **334,120 GC-allocated bytes** in the timed snapshot phase.
- Sequential event-journal scan: **530.133 ms**,
  **15.091 MiB/s** logical source throughput, with
  **103,460,968 GC-allocated bytes** in that phase and
  a **62,078,976-byte (59.20 MiB)** peak working set.
  Six Gen0, six Gen1 and six Gen2 collections were recorded
  by the phase log as cumulative collection counts; they
  are not necessarily deltas for this phase.
- Event-journal footprint: **19,569,254 bytes** (~2.333×
  the source length). Total **70 source cursor requests**
  and **119 event-stream read requests** in the full pass.
- First sequence of 10 random source-line seeks:
  median **0.2020 ms**, p95 **0.7853 ms**.
  Repeated identical sequence: median **0.1374 ms**,
  p95 **0.3117 ms**. All requested positions returned
  their expected canonical physical line.
- The sub-millisecond seek times likely reflect filesystem
  caches and a prepared event journal, **not** an HDD head
  movement taking under one millisecond. No OS cache flush
  or independently verified cold-cache run occurred.
- Windows process I/O values in `proc_io_delta` are zeros:
  the harness only implements Linux `/proc/self/io`
  accounting. Do not interpret these values as zero HDD I/O.

## Fixture preparation (`prepare`)

**PASS**: `fixture_set_completed` and `session_completed`.

| Fixture size | Status | Generation time | Logical rate |
| --- | --- | ---: | ---: |
| 8 MiB | Reused marker-verified file | — | — |
| 128 MiB | Generated | 3,128.664 ms | 40.912 MiB/s |
| 1,024 MiB | Generated | 22,880.472 ms | 44.754 MiB/s |
| 2,048 MiB | Generated | 46,835.887 ms | 43.727 MiB/s |

All four file sizes are present according to the completed
generator log, totalling **3,208 MiB = 3.1328125 GiB**
of persistent test Markdown. The generator does not
calculate full-file hashes, so byte-for-byte integrity
has not been independently verified.

## Follow-up and caveats

1. Initial `prepare` logs repeated **the same free_bytes
   value** across subsequent fixtures because it was captured
   by the launcher once. This is a reporting limitation, not a
   generation failure. A follow-up patch refreshes Windows
   free space via `DriveInfo` before each fixture while retaining
   the verified launch-time limit. Linux still reports its
   initial verified `df` allowance, explicitly marked.
2. The event journal is roughly 2.33× the 8-MiB source size
   and its scan allocated approximately 12.3× source bytes
   in cumulative managed allocations. This merits
   profiling on 128 MiB and 1 GiB before claiming a
   scalable memory/performance improvement.
3. Next recommended run: `run.cmd measure -SizeMiB 128
   -Seeks 100 -Passes 2`, then consider a **1-GiB**
   run and finally **2 GiB** when data confirms the
   budget. There is no need to regenerate prepared data.
4. Full mixed visual-page `index` is a separate opt-in
   benchmark and has **not** been tested here.
5. Linux comparison is awaiting the maintainer's exact
   mount path. Keep the parameters and warm/cold-cache
   methodology as comparable as possible.

[Main benchmark README](../../README.md) ·
[Benchmark implementation status](../../IMPLEMENTATION_STATUS.md).

## 128-MiB two-pass journal benchmark — 2026-11-22 12:40 UTC

The maintainer supplied the completed `measure --size-mib 128
--seeks 100 --passes 2` JSONL and TXT diagnostic logs. They
contain one successful session and no errors, on the same
Windows H: HDD as the 8-MiB smoke-test baseline.

| Metric | Pass 1 | Pass 2 |
| --- | ---: | ---: |
| Validated snapshot | 3,314.893 ms | 3,490.572 ms |
| Snapshot throughput | 38.614 MiB/s | 36.670 MiB/s |
| Full journal scan | 4,102.406 ms | 2,770.607 ms |
| Journal throughput | 31.201 MiB/s | 46.199 MiB/s |
| Journal bytes | 313,601,334 | 313,601,334 |
| Managed allocations in journal phase | 1,639,326,568 B | 1,638,737,728 B |
| Peak process working set (cumulative) | 69,615,616 B | 70,672,384 B |
| Cursor requests after all seek series | 989 | 989 |
| Journal event read requests | 1,777 | 1,777 |

Both passes processed **100,930** physical source lines.
Their source-line offset index is **807,448** bytes.
The journal:source length ratio is **2.33651**,
close to the smoke test's approximately 2.33.
The sequential scan is **32.46% shorter** in the
second pass, with the *same* journal size and request counts.
The first pass may have benefited from OS caches too;
no cold-cache classification is claimed.

The four sets of **100 deterministic seeks each** all
returned valid starting source lines. Median request times:
**0.0546, 0.0597, 0.0562 and 0.0582 milliseconds**;
their p95 values were **0.0727, 0.1028, 0.0781 and
0.0788 milliseconds**. In particular, these are measured
post-journal indexed lookups, **not** physical HDD head
positioning latencies.

Managed allocations during each journal scan are about
**12.21×** the 128-MiB source size, despite a peak process
working set under 68 MiB. The old per-phase log entries
include **cumulative** `gc_gen*` counts: the journal
scan ended at 93/92/92 on pass 1, and 187/185/185 on
pass 2, not two independent phase counts. The harness
now also reports `gc_gen*_delta` for subsequent runs.
The journal creates per-line JSON event records and
several bounded buffers, possible allocation hot spots
that require profiling rather than speculative optimization.
Windows `proc_io_delta` fields remain zero placeholders.

The next safe experiment uses the *already generated*
1,024-MiB source and only one pass:

```powershell
.\Blackboard\Tools\MdView.Benchmarks\run.cmd measure -SizeMiB 1024 -Seeks 100 -Passes 1
```

The 128-MiB journal size suggests approximately
**2.34 GiB of event journal** at 1 GiB of source, if
the same per-line content mix scales linearly.
This estimate is not a measured 1-GiB result.
Monitor free disk space, GC deltas and total runtime
before increasing to 2 GiB. No automatic schedule is
configured, and the full visual-page index remains
separately opt-in.

## 1,024-MiB single-pass measurement — 2026-11-22 12:44 UTC

New complete maintainer-provided logs:
`mdview-hdd-20261122T124433618Z-31f383d5.jsonl`
and the same-base-name `.txt`. The benchmark finished
successfully on Windows H: (same verified HDD and path),
now using **.NET runtime 10.0.15**. This runtime revision
differs from the 10.0.14 used in the earlier measurements.

| Metric | Observed |
| --- | ---: |
| Source bytes / lines | 1,073,741,824 / 807,433 |
| Validated snapshot | 24,711.728 ms (41.438 MiB/s) |
| Snapshot managed allocations | 430,672 B |
| Snapshot phase Gen0/Gen1/Gen2 deltas | 0 / 0 / 0 |
| Source-line offset index | 6,459,472 B |
| Full event journal | 2,510,275,305 B (2.33788 GiB) |
| Journal phase wall time | 47,203.060 ms (21.694 MiB/s) |
| Journal phase CPU time | 54,156.250 ms (process CPU, multi-thread) |
| Journal phase managed allocations | 13,111,191,408 B (12.211 GiB) |
| Journal phase Gen0/Gen1/Gen2 deltas | 743 / 742 / 742 |
| Peak process working set | 71,610,368 B (68.29 MiB) |
| Cursor / event read requests | 6,509 / 12,817 |
| Random source-line requests | 200/200 valid |

First random sequence of 100 positions: **0.0714 ms median**,
**0.1009 ms p95**, **0.9285 ms maximum**. Identical
repeated sequence: **0.0667 ms median**,
**0.0959 ms p95**, **0.1390 ms maximum**.
These are warm, post-journal indexed reads, not isolated
mechanical-drive seeks.

### Disproportionate 60%→70% latency interval

Timestamps of the journal progress events reveal these
intermediate wall-clock durations:

| Progress window | Elapsed |
| --- | ---: |
| 0–10% (starts after snapshot metadata log) | 3.333 s |
| 10–20% | 2.243 s |
| 20–30% | 2.488 s |
| 30–40% | 2.249 s |
| 40–50% | 2.296 s |
| 50–60% | 2.278 s |
| **60–70%** | **23.805 s** |
| 70–80% | 2.929 s |
| 80–90% | 2.720 s |
| 90–100% | 2.863 s |

The ~23.8-second interval is approximately **21 seconds
slower than typical adjacent windows**. The full journal
still completed and all requested rows remained correct.
Root cause is **unknown**. The previous console progress
events provided only timestamps and journal sizes.
A subsequent benchmark-only change adds per-window
elapsed time, managed allocations, Gen0/1/2 collection
deltas, process CPU time, journal byte increments and
working set, without modifying production behavior.
These additional metrics may help distinguish a GC/CPU
spike from I/O pauses, but cannot prove physical-disk
read/write timing.

### Capacity and next experiment

The 1-GiB journal/source ratio **2.33788×** is close
to the 128-MiB baseline ratio **2.33651×**. Extrapolating
the same synthetic fixture mix suggests a journal near
**4.676 GiB** for the prepared 2-GiB source. That is
below the benchmark-only 8-GiB journal ceiling, but not
a guaranteed upper bound for other documents.
The offset index size and source line count remain
linear for this fixture.

**Recommended next test:** pull the updated benchmark
and repeat the same 1,024-MiB / 100-seek / one-pass test
to localize the stall using the new progress metrics:

```powershell
git pull
.\Blackboard\Tools\MdView.Benchmarks\run.cmd measure -SizeMiB 1024 -Seeks 100 -Passes 1
```

Do not regenerate fixture files, clear OS caches or
infer true cold-cache reads from this test. Following
stall diagnosis, the 2-GiB test can proceed explicitly.

## Repeated 1,024-MiB scan — 2026-11-22 12:49 UTC

The maintainer provided a second completed run:
`mdview-hdd-20261122T124949240Z-f8e63090`
(JSONL and TXT). It uses the same Windows H: HDD, generated
Markdown data, one pass, and 100+100 deterministic seek checks;
the runtime remains .NET 10.0.15. All phases succeeded,
and all 200 seek checks returned correct starting lines.

| Metric | Previous 1-GiB run | Repeat |
| --- | ---: | ---: |
| Validated snapshot | 24,711.728 ms | 24,557.974 ms |
| Sequential journal scan | 47,203.060 ms | 46,629.183 ms |
| Event-journal size | 2,510,275,305 B | 2,510,275,305 B |
| Journal-phase GC allocated bytes | 13,111,191,408 | 13,111,244,096 |
| Journal Gen0/Gen1/Gen2 deltas | 743/742/742 | 743/742/742 |
| Peak working set | 71,610,368 B | 72,204,288 B |
| First/repeat seek median | 0.0714/0.0667 ms | 0.0615/0.0560 ms |
| **60–70% progress interval** | **23,805 ms** | **24,683 ms** |

**Repeatable anomaly:** the 60%–70% progress interval had
**24,682.724 ms wall time** on the new run, approximately
**10.83×** the median **2,279.064 ms** of the remaining
nine intervals. This was between journal sizes
**1,506,145,546** and **1,757,269,452 bytes**.

The same interval reported **4,937.500 ms process CPU**,
**1,311,461,688 managed allocated bytes**, **74/74/74
Gen0/1/2 collections**, **251,123,906 added journal
bytes** and **69,091,328 working-set bytes**. Neighboring
intervals show comparable CPU, allocation, collection
and journal-growth measurements. CPU time includes
parallel execution; the difference from elapsed time
must not be interpreted literally as exact wait time.

The evidence supports a **repeatable wall-time stall**,
not a proportional increase in parser work, allocations
or number of GC events. It does **not** identify the
cause: Windows write-cache throttling, filesystem or
device I/O, antivirus/scheduling and GC pause time
remain possibilities. Neither zero-valued
Windows `proc_io_delta` placeholders nor
collection counts determine which subsystem stalled.

### Diagnostic follow-up

The benchmark (not production MdView) now accepts
`--progress-step N` (1–10, default 10; Windows
`-ProgressStep`). At 1% increments, each logged
interval reports existing wall/CPU/allocations/GC count
metrics plus `interval_gc_pause_ms`, derived from
`GC.GetTotalPauseDuration()` (available on .NET 10).
The `timed_phase` events also include
`gc_pause_ms_delta`.

Run after pulling the latest Toolbox commit:

```powershell
git pull
.\Blackboard\Tools\MdView.Benchmarks\run.cmd measure -SizeMiB 1024 -Seeks 100 -Passes 1 -ProgressStep 1
```

This **reuses the exact existing 1-GiB fixture**;
it never rewrites source data, clears OS caches or
changes disk formatting. Smaller intervals produce
more synchronous progress logging on the slow HDD;
this may perturb absolute timings. Compare the
**location and profile of stalls** rather than treating
the new total runtime as a strict apples-to-apples
performance benchmark. Do not run the 2-GiB benchmark
by default until the anomalous behavior is better understood.

## 1%-resolution diagnosis — 2026-11-22 12:57 UTC

The maintainer supplied a completed diagnostic run:
`mdview-hdd-20261122T125720117Z-015b7a39.{jsonl,txt}`,
on the same Windows H: HDD and the existing 1-GiB source,
with `-Seeks 100 -Passes 1 -ProgressStep 1`. All phases
completed successfully, including **200/200 sampled
source-line seeks**; there is no `session_failed`.
.NET runtime: **10.0.15**.

| Measurement | Observation |
| --- | ---: |
| Validated snapshot | 24,805.267 ms (41.282 MiB/s) |
| Event journal | 2,510,275,305 bytes |
| Sequential scan | 48,091.018 ms (21.293 MiB/s) |
| Managed allocations in journal phase | 13,111,998,096 bytes |
| Journal-phase GC Gen0/Gen1/Gen2 | 743 / 742 / 742 |
| Total journal-phase GC pause duration | **730.739 ms** |
| Peak process working set | 71,741,440 bytes (68.42 MiB) |
| Seek medians (first / repeat, each 100) | 0.0610 / 0.0553 ms |

The previous 10%-resolution runs both showed an anomaly
in the 60–70% interval. The finer log localizes this
to the **69–70% interval**:

- `source_lines_scanned`: **557,184 → 565,248**.
- `journal_bytes`: **1,732,164,328 → 1,757,269,452**.
- Elapsed wall: **24,918.810 ms**, versus **221.017 ms**
  median for the other 99 one-percent windows.
- CPU: **656.25 ms** in the outlier window.
- Managed allocations: **131,414,008 bytes**.
- Collections: **7/7/7** (Gen0/Gen1/Gen2).
- **Actual GC pause: only 8.182 ms** in this window,
  approximately 0.033% of its wall time. The GC did not
  account for the roughly 24.7-second extra elapsed
  interval as a collection stop-the-world pause.

The discrepancy is repeatable in **three independent
1-GiB runs**, always just before the journal passes
roughly 1.75 billion bytes. It is consistent with
a wait in I/O, OS scheduling, storage/cache management,
console logging or another blocking operation; **it is
not proof of a defective HDD or of a specific Windows
write-cache mechanism**.

### Improved attribution before further hardware testing

The log's `interval_elapsed_ms` is measured between
progress timestamps, and therefore also includes the
**previous** progress event's synchronous writes to
the text file, JSONL file and console. GC and CPU
measurements alone cannot distinguish a slow
`MdViewContainerPageCursor.ReadAsync` call from a
slow benchmark log write.

A follow-up benchmark-only patch now records these fields
in each `journal_progress` entry, without additional
per-request writes to the slow disk:

- `interval_cursor_read_count`: bounded cursor calls;
- `interval_cursor_read_total_ms`: elapsed wall
  time collectively spent awaiting cursor requests;
- `interval_cursor_read_max_ms`: the single slowest
  cursor call within that progress window;
- `interval_cursor_read_above_100ms`: request count
  crossing 100 ms;
- `interval_cursor_max_start_line` and
  `interval_cursor_max_journal_before/after`:
  the starting source line and pre/post journal offset
  of that slowest call;
- `previous_progress_log_write_ms`: duration of the
  **previous** progress logger call, which was included
  in this progress interval's measured elapsed time.

These fields can distinguish cursor-level blocking from
logging overhead and localize a slow request. They still
cannot directly identify a kernel disk write, a driver or
the physical device without an OS-level trace.

Next minimally invasive experiment, after pulling
the updated branch:

```powershell
git pull
.\Blackboard\Tools\MdView.Benchmarks\run.cmd measure -SizeMiB 1024 -Seeks 100 -Passes 1 -ProgressStep 1
```

The prepared test file remains unchanged. This benchmark
does not clear caches, format devices or overwrite data.
Consider a Windows disk Active Time / response-time trace
**only if** the new telemetry confirms a cursor-level wait;
do not infer a hardware failure. The 2-GiB test is still
pending until this repeated wait is attributed.

## 2026-11-22 13:02 UTC — cursor attribution and isolated log proposal

The supplied paired log files
`mdview-hdd-20261122T130231087Z-d6558207`
cover a **successful** 1-GiB, one-pass Windows H:
`-ProgressStep 1` run with 200/200 correct sampled
source-line seeks. Runtime: .NET 10.0.15.

- Snapshot: **26,377.7095 ms**, 38.821 MiB/s.
- Full journal: **50,242.984 ms**, 20.381 MiB/s,
  **2,510,275,305 bytes**. Managed allocations during
  journal scan: **13,112,108,976 bytes**, GC pause
  duration: **1,022.423 ms**, peak process working
  set: **72,560,640 bytes** (69.20 MiB).
- The median of all 100 one-percent windows was
  **350.847 ms**; unlike earlier runs, the
  **69–70% window was normal** (**362.070 ms**).
- Instead, notable intervals were **53–54%**:
  1,985.259 ms with a 1,184.129-ms longest cursor
  request; **55–56%**: 6,824.672 ms with a
  2,725.929-ms longest request and 5 requests
  above 100 ms; **56–57%**: 5,259.895 ms with
  a 1,899.474-ms longest request and 5 requests
  above 100 ms.
- During those three slow windows the previous
  progress log writes were respectively
  **0.207 ms, 0.230 ms and 0.229 ms**. The
  aggregated `interval_cursor_read_total_ms`
  almost entirely accounts for the intervals'
  elapsed time. The extra delay therefore occurred
  **inside measured cursor calls**, not in the
  diagnostic logger calls in this run.
- The longest 55–56% request began at physical
  source line **447,104**, with event journal
  offset **1,389,922,194 bytes** before the
  call. This is a localization for further
  instrumentation, **not** proof of a specific
  disk operation or a disk fault.
- Since the long delay no longer falls at the same
  source line and the number/size of classified
  events remain stable, a content-specific pathological
  Markdown construct becomes a weaker hypothesis.
  OS write-back / filesystem cache pressure /
  storage latency remains possible but unverified.

### Follow-up: keep diagnostic writes in RAM

The maintainer requested that only genuine Markdown
workload data be written on the slow disk **during**
the timed work. That was implemented in two commits:
`90b733d4214a67f48f7cda29220a56112d484b48`
(RAM-buffered diagnostic writer and post-test system
reports on Windows/Linux) and
`8f1071408328c6874773591618ffcee24306cbe2`
(CI logger smoke test).

The logger now collects all JSONL and TXT events in
managed RAM up to a 32-MiB combined UTF-8 ceiling,
without writing progress to the console during the
measure. It attempts a post-test export even on a
handled benchmark exception. It preserves the source
and test disk's journal/index temp paths, and does
not flush the Windows OS file cache. This removes
diagnostic HDD writes but **not the journal's own
writes** (which are part of the measured workload).
A RAM buffer does not guarantee the OS never swaps.

Run once more with identical prepared data and
parameters, after updating the repository, to
compare behavior under isolated diagnostic logging:

```powershell
git pull
.\Blackboard\Tools\MdView.Benchmarks\run.cmd measure -SizeMiB 1024 -Seeks 100 -Passes 1 -ProgressStep 1
```

A new result is needed to establish whether the
changed logging affects the stalls. The physical
2-GiB measurement remains pending.

## RAM-buffered-log confirmation — 2026-11-22 14:30 UTC

The maintainer supplied
`mdview-hdd-20261122T143043809Z-ec6979b9`
(TXT and JSONL) from the **first physical Windows H:
1-GiB run using the updated memory-buffered logger**.
The `session_start` record confirms
`diagnostic_output = memory_buffered_then_persisted_after_test`
and a combined diagnostic RAM quota of **33,554,432 B**
(32 MiB). The benchmark was run with a
**1% progress interval, one pass, 100 deterministic
seek positions repeated twice**, on runtime .NET 10.0.15.

| Measurement | RAM-buffered run | Previous 13:02 run (disk logs) |
| --- | ---: | ---: |
| Validated snapshot | **25,591.460 ms** | 26,377.710 ms |
| Journal scan | **48,484.762 ms** | 50,242.984 ms |
| Journal size | **2,510,275,305 B** | 2,510,275,305 B |
| Journal managed allocations | **13,111,881,424 B** | 13,112,108,976 B |
| Journal GC pause time | **702.752 ms** | 1,022.423 ms |
| Journal GC Gen0 / Gen1 / Gen2 | 743/742/742 | 743/742/742 |
| Peak working set | **71,987,200 B** (68.65 MiB) | 72,560,640 B |
| Random seek correctness | **200/200** | 200/200 |
| First/repeat seek median | **0.0598/0.0579 ms** | 0.0863/0.0888 ms |

**Performance interpretation:** the scan took **3.50%
less wall time**, but the difference cannot be
attributed entirely to the changed logging: the
stalls are irregular, page caching is uncontrolled,
and this is not a controlled repeated A/B experiment.
The median one-percent interval on the RAM-buffered
run was **216.869 ms**. No claim is made about
uncached mechanical seeking.

The principal new stall occurred over consecutive
one-percent intervals:

| Progress | Interval wall | Cumulative cursor time | Longest individual cursor call | GC pause | Previous progress log write |
| --- | ---: | ---: | ---: | ---: | ---: |
| **66–67%** | **15,272.928 ms** | 15,272.904 ms | **13,194.393 ms** | 8.384 ms | 0.0155 ms |
| **67–68%** | **9,708.212 ms** | 9,708.180 ms | **3,782.650 ms** | 7.513 ms | 0.0206 ms |

Together these two intervals account for **24.981 s
of the 48.485-s journal scan**. The slowest cursor
call started at physical source line **539,904**,
with journal offset **1,678,489,154 B** before that
request and **1,678,829,282 B** after it. The next
interval's longest cursor request began at line
**543,616** and journal offset **1,689,968,968 B**.
The journal's physical file I/O, Windows filesystem
cache, external scheduling and other blocking
operations are not individually timed by the
instrumentation.

**What is established:** the RAM-buffered benchmark
exported 109 successful events including
`session_completed` and all seek summaries. The
progress logger performs no disk writes during the
test; its recorded CPU/serialization time for previous
events was only hundredths of a millisecond after
startup. Stalls remained fully inside
`MdViewContainerPageCursor.ReadAsync` call
durations, while GC pause durations remained tiny
within those windows.

**What is not established:** the latency cannot yet
be assigned specifically to NTFS, the mechanical
head, OS write-back throttling, antivirus or the
Markdown serializer. Windows
`proc_io_delta` remains unsupported and
zero-filled, not evidence of zero disk I/O.

**Next experiment:** progress to the already generated
2,048-MiB fixture with memory-buffered diagnostics:

```powershell
git pull
.\Blackboard\Tools\MdView.Benchmarks\run.cmd measure -SizeMiB 2048 -Seeks 100 -Passes 1 -ProgressStep 1
```

Expect approximately **4.676 GiB** of journal based on
the measured 1-GiB synthetic workload ratio. This is an
**estimate**, below the benchmark-only 8-GiB journal
budget, not a proven guarantee. Inspect the actual
free-space preflight and resulting journal before
any more aggressive or visual-index experiments.

## 2,048-MiB HDD stress run — 2026-11-22 14:35 UTC

The maintainer supplied
`mdview-hdd-20261122T143553385Z-a046cac3.{jsonl,txt}`.
This is the first **full 2-GiB journal and 200-seek
measurement** on the verified Windows H: USB/NTFS
mechanical HDD. `Mode=measure`,
`SizeMiB=2048`, `Seeks=100`, `Passes=1`,
`ProgressStep=1`; runtime .NET **10.0.15**.
The session confirms RAM-buffered diagnostics with
post-measurement export, succeeded without any
`session_failed` event, and completed **200/200**
source-line checks.

| Metric | Observed 2 GiB | 1-GiB RAM-logged reference |
| --- | ---: | ---: |
| Source bytes | **2,147,483,648** | 1,073,741,824 |
| Source physical lines | **1,614,865** | 807,433 |
| Validated snapshot | **50,209.278 ms** | 25,591.460 ms |
| Snapshot throughput | **40.789 MiB/s** | 40.013 MiB/s |
| Offset index | **12,918,928 B** | 6,459,472 B |
| Serialized journal | **5,022,177,346 B (4.677 GiB)** | 2,510,275,305 B |
| Journal/source size ratio | **2.33863×** | 2.33788× |
| Journal scan wall time | **123,166.942 ms** | 48,484.762 ms |
| Journal logical throughput | **16.628 MiB/s** | 21.120 MiB/s |
| Journal managed allocation volume | **26,227,462,992 B** | 13,111,881,424 B |
| Journal GC Gen0/Gen1/Gen2 | **1485/1484/1484** | 743/742/742 |
| Journal GC total pause | **1,394.987 ms** | 702.752 ms |
| Peak process working set | **72,130,560 B (68.79 MiB)** | 71,987,200 B (68.65 MiB) |
| First/repeated seek median | **0.0813 / 0.0712 ms** | 0.0598 / 0.0579 ms |
| Seek correctness | **200/200** | 200/200 |

**Scaling findings:** the snapshot, journal size, managed
allocation volume, GC counts and GC pause time
approximately doubled as the source doubled.
Peak working set increased by only **143,360 bytes**
for this synthetic input. Thus there is no sign of
a proportional **working-set** increase in these
measurements; this does not prove a bound for all
Markdown or OS configurations. The journal
wall time grew **2.54×**, not 2×, due largely to
intermittent cursor stalls.

### Where 2-GiB journal scan was delayed

The 100 one-percent `journal_progress` intervals
report **five exceptionally slow windows**:

| Progress interval | Wall time | Max individual `cursor.ReadAsync` | Requests >100 ms | GC pause |
| --- | ---: | ---: | ---: | ---: |
| **34–35%** | **23,847.164 ms** | **18,556.002 ms** | 6 | 14.988 ms |
| **53–54%** | **16,543.091 ms** | **2,657.417 ms** | 14 | 15.953 ms |
| **73–74%** | **17,512.557 ms** | **2,796.190 ms** | 12 | 16.364 ms |
| **98–99%** | **2,882.565 ms** | **850.242 ms** | 3 | 15.756 ms |
| **99–100%** | **13,644.015 ms** | **2,275.818 ms** | 13 | 18.087 ms |

These five intervals account for **74.429 s**
(approximately **60.4%** of the entire 123.167-s
journal phase). In contrast, among intervals
below 1.5 s, the median was **445.375 ms**.
The aggregated individual cursor call times
almost exactly account for each interval's
wall time. GC pause time in the worst 34–35%
interval is **15 ms**, far too short to account
for its 23.85-s wall duration. Managed allocations
and collection counts were ordinary for each
one-percent window.

**Largest single cursor call:** starts at physical
source line **557,056**, with event-journal offset
**1,731,783,858 B** before and **1,732,164,328 B**
after the call. It lasted **18,556.002 ms**.

This event offset is notably close to the
**1.68–1.71-billion-byte journal region** of
the prior 1-GiB RAM-logged run's large
66–68% delays. The problematic **percentages**
differ because the total inputs are different.
That is a *correlation*, not proof of an
absolute journal-size threshold or a particular
NTFS/device cache mechanism. Other large
2-GiB delays occurred at much larger journal
sizes (around 2.67, 3.70 and 4.99 GB), so a
single fixed threshold would be an inadequate
explanation.

All of these elapsed times occur while awaiting
cursor requests. The benchmark does **not**
separately time individual operations inside
`ScanUntilAsync`: source scanner advancement,
serialization, event-size write, event-payload
write, offset write and possible OS I/O
stalls remain conflated. Per-process Windows
`proc_io_delta` values are unsupported and
zero placeholders, **not** zero measured I/O.
Random post-journal cursor lookups are **warm
indexed reads**, not mechanical HDD seek timing.

### Source inspection and next performance experiment

Source inspection of
`Blackboard/Tools/MdView/MdViewContainerPageCursor.cs`
shows that each successfully scanned physical
line is serialized to a UTF-8 JSON payload and
then written with **three asynchronous
`FileStream.WriteAsync` calls**: a 4-byte event
length, the event payload, and an 8-byte offset.
At 1,614,865 lines this amounts to approximately
**4,844,595 journal/offset write calls**.
The 2-GiB session also accumulated
**26.227 billion bytes of managed allocations**
(12.213× source), even though live working
memory stayed under 69 MiB.

**Recommended diagnostic next step:** add opt-in,
low-overhead operation-level time aggregation to
the journal path (scanner, serializer, event
writes, offset writes). Keep output buffered in
RAM and exclude log export from timed phases.
Verify a new, small fast fixture on CI and
the existing 1-GiB/2-GiB HDD baseline before
trying to optimize journal writes into batches.
Any batching must preserve exact event offsets,
size-budget guards, cancellation, failure-closed
semantics, readable older journals, and accurate
line numbering; existing tests should be
extended before production changes. This is a
plan only; no production cursor or journal
format has been altered by this log review.

## 2026-11-22 — opt-in journal stage breakdown implemented

Following the successful **2,048-MiB Windows H:**
baseline and the source inspection described above,
two implementation commits introduced optional,
in-memory, per-stage journal diagnostics:
`41d9e6eae53a4d6ac8133e81a33d274137e47692`
(the fixed-size internal observer and xUnit regression)
and `da72fe6c88e164155b4a64c212838ddd1adecdab`
(benchmark option, RAM-buffered JSONL progress field,
Windows launcher switch and Windows/Linux CI matrix
regression invocation).

This is an **instrumentation-only** change:
- `MdViewJournalScanTimings` uses
  `Stopwatch.GetTimestamp()` and an array of five
  counters to measure logical scanner advancement,
  JSON event serialization, the event length write,
  the event payload write and the offset write.
- `TakeWindowAndReset()` creates an immutable
  `MdViewJournalTimingWindow`. Every stage includes
  `Calls`, `TotalMs`, `MaxMs`,
  `CallsAtLeast100Ms`, `MaxSourceLine`,
  `MaxJournalOffset`; `AccountedMs` sums
  the five time totals.
- By default the cursor's internal observer is null.
  Opt in using `--trace-journal-io` on Linux
  or `-TraceJournalIo` on Windows.
- `journal_scan_stages` is serialized in
  `journal_progress` at each configured progress
  interval; no per-line JSONL records or progress
  disk writes occur during measurement.
- Because timed async methods include scheduling
  and runtime effects, operation times alone cannot
  prove which OS/device layer delayed a write.
  The trace itself changes benchmark overhead.
- Cursor journal record bytes, offsets, size limits,
  cancellation and failure semantics are unmodified.

**Awaiting external physical-HDD evidence** after
CI verification: start with 128 MiB and then 1 GiB
on the already prepared dataset with
`-ProgressStep 1 -TraceJournalIo`. No speedup
is claimed until comparing results and, if needed,
making a separate write-batching change.

## 128-MiB five-stage journal profile — 2026-11-22 14:52 UTC

The maintainer uploaded both complete,
matching `mdview-hdd-20261122T145233390Z-5ca3b47a`
JSONL/TXT files. The run completed on
Windows H: with **1% progress**, **one
128-MiB journal pass**, `TraceJournalIo=true`
and a post-test RAM-log export, using
.NET **10.0.15**. No `session_failed`
record is present. The log contains
100 `journal_progress` records and
two `random_seek_summary` records;
all **200 sampled seeks** returned the
expected source lines.

| Item | Observed |
| --- | ---: |
| Source lines / size | **100,930 / 134,217,728 B** |
| Snapshot and offset index | **3,455.2965 ms / 807,448 B** |
| Event journal | **313,601,334 B** |
| Journal scan | **4,159.7241 ms** (30.771 MiB/s) |
| Journal-phase managed allocations | **1,640,653,072 B** |
| Journal-phase GC pause duration | **96.631 ms** |
| Peak process working set | **71,892,992 B** (68.56 MiB) |
| First seek median / repeat median | **0.0496 / 0.0489 ms** |
| 1% window median / maximum | **35.504 / 110.301 ms** |
| Cursor ReadAsync calls >= 100 ms | **0** |

The five new-to-benchmark **construction**
stage totals in the 100 progress records
were:

| Stage | Total calls | Wall time (ms) | Longest single operation (ms) |
| --- | ---: | ---: | ---: |
| `ScannerAdvance` | 100,931 | **772.621** | 6.8147 |
| `SerializeEvent` | 100,930 | **573.825** | 16.8310 |
| `EventLengthWrite` | 100,930 | **15.907** | 0.0098 |
| `EventPayloadWrite` | 100,930 | **17.226** | 0.0487 |
| `OffsetWrite` | 100,930 | **11.617** | 0.0151 |
| **Total accounted** | | **1,391.197** | |

The scanner attempted to advance the
iterator once beyond EOF, explaining the
one-call difference between
`ScannerAdvance` and serialized line
count.

All three logical **write-await** stages
together accounted for **44.750 ms**.
They never exceeded the 100-ms
threshold. The **remaining approximately
2,768.527 ms** of the 4.16-second
journal phase were **not measured by
these five stages**, and cannot simply
be attributed to disk writes, physical
disk reads or the Markdown parser.
Reading offsets/events back from the
journal, deserializing JSON, allocating
buffers and other cursor work are
possible contributors.

The 128-MiB traced run did **not**
reproduce the multi-second stalls
seen at 1/2 GiB. The maximum whole
1% progress window was **110.3 ms**
at startup, and the median was
**35.5 ms**. Compared with the earlier
128-MiB untraced runs, differences
must not be presented as an
optimization speedup; runtime caches
and profiling overhead differ.

### Instrumentation follow-up

The five-stage trace was expanded in
commit
`bfa0847a2bfe1761efc6a065f39ed2e27cd2d4b8`
with three additional, bounded-RAM
logical stages: `OffsetTableRead`,
`EventWindowRead` and
`DeserializeEvent`. These operate
during the **replay** portion of each
`cursor.ReadAsync`, even when the
source line is already journaled.

**Important:** the current 128-MiB
log predates these additions; do not
report invented readback-stage times
for this run. The benchmark now
reports all eight stage totals in
the same `journal_scan_stages`
JSONL object without changing
persistent journal bytes, public
APIs or the normal disabled-by-default
production path.

**Next controlled real-HDD run**, after
updating to a green CI revision:

```powershell
git pull
.\Blackboard\Tools\MdView.Benchmarks\run.cmd measure -SizeMiB 1024 -Seeks 100 -Passes 1 -ProgressStep 1 -TraceJournalIo
```

Use the existing fixture. Inspect
whether any new stage explains
repeated 1-GiB stalls before planning
write buffering or further physical
device investigations.

## 1-GiB eight-stage trace — 2026-11-22 14:57 UTC

Physical Windows H: HDD results submitted as
matching paired files
`mdview-hdd-20261122T145716681Z-543ebae1.{jsonl,txt}`.
One pass, prepared 1-GiB synthetic Markdown fixture,
`-ProgressStep 1 -TraceJournalIo`,
.NET 10.0.15. There are **100 progress records**,
2 random-seek summaries, a successful
`session_completed`, and no failures.
Diagnostics remained buffered in RAM during
measurement.

| Metric | Observed |
| --- | ---: |
| Source bytes / lines | **1,073,741,824 / 807,433** |
| Snapshot elapsed | **25,123.997 ms** (40.758 MiB/s) |
| Event journal bytes | **2,510,275,305** |
| Journal elapsed | **47,723.939 ms** (21.457 MiB/s) |
| Journal managed allocations | **13,113,580,248 bytes** |
| Journal GC pause | **753.859 ms** |
| Maximum process working set | **73,170,944 B (69.781 MiB)** |
| First/repeated warm indexed seek median | **0.0575 / 0.0535 ms** |
| Correctness | **200/200 sampled seeks** |

### All eight journal stages, summed across 100 windows

| Stage | Aggregate time, ms | Count |
| --- | ---: | ---: |
| `ScannerAdvance` | **3,807.094** | 807,434 |
| `SerializeEvent` | **1,838.367** | 807,433 |
| `EventLengthWrite` | **88.150** | 807,433 |
| `EventPayloadWrite` | **115.499** | 807,433 |
| `OffsetWrite` | **68.508** | 807,433 |
| `OffsetTableRead` | **80.465** | 6,309 |
| `EventWindowRead` | **573.736** | 12,617 |
| `DeserializeEvent` | **1,920.612** | 807,433 |
| **Instrumented total** | **8,492.430** | |

The three write-await totals sum to
**272.157 ms**, compared with
**47,723.939 ms** for the entire phase.
The difference (about 39.2 seconds)
is **unaccounted time**, not evidence
that the HDD was idle or that the
write calls performed no physical I/O.
`FileStream` may defer operating-system
work into other synchronous operations.

The typical one-percent interval median
was **218.225 ms**. But the **67–68%**
interval lasted **23,035.088 ms**,
starting at journal offset
**1,682,002,689 bytes** and ending at
**1,707,059,208 bytes**. In this
window, six cursor calls exceeded
100ms; the longest single `ReadAsync`
lasted **14,803.923 ms**, starting
at source line **544,512**, journal
offset **1,692,778,274 bytes**
and ending after a call at
**1,693,174,920 bytes**.

**This interval's eight operations
accounted for just 75.655 ms**.
The interval GC pause was **7.797 ms**;
its previous diagnostic log write
took **0.0238 ms**. None of the
eight measured operation types
reported a call >=100ms.
The time is therefore within cursor
`ReadAsync` but **outside** these
eight timed operation boundaries.
It is not directly attributable to
JSON serialization, scanning,
a timed async write/read await,
or GC stop-the-world.

Several other progress windows had
shorter unaccounted cursor stalls:
31–32% **198.529 ms** largest
`ReadAsync`; 59–60%
**233.055 ms**; 68–69%
**487.825 ms**; 69–70%
**178.127 ms**. All were absent
from the eight instrumented
stage maxima.

### Investigation and code change

Source inspection showed an important
unmeasured operation before each
read/write timer: synchronous
`FileStream.Position` setters for
the event-journal file and its
offset table, both during append
and replay. Because each position
setter runs *before* the corresponding
async I/O stopwatch begins, it may
carry filesystem/runtime overhead
without appearing in existing stage
timings. This is a **candidate**,
not yet proven to be the source of
the long stalls.

In commit
`a87d144045fb764a1e8efc0e2b81e51312d829c4`,
the optional observer was extended
from eight to **twelve** counters:
`EventWritePosition`,
`OffsetWritePosition`,
`OffsetReadPosition` and
`EventReadPosition`. The cursor now
times those setters separately.
No persistent journal format,
public interface, buffer/flush
behavior, cancellation or budget
logic was changed.

**Next experiment:** on the *same
prepared source*, once CI confirms
build and correctness, rerun with
`-ProgressStep 1 -TraceJournalIo`.
Compare each position-setter's
`MaxMs` and source line/offset with
the longest cursor request in the
1.68–1.71 GB journal region.
Do not assert a physical disk
seek measurement or a successful
performance optimization.

## 1-GiB twelve-stage journal trace — 2026-11-22 15:04 UTC

Supplied complete paired physical Windows H:
files: `mdview-hdd-20261122T150433389Z-44ba10df.{jsonl,txt}`.
.NET **10.0.15**, one prepared 1-GiB source
pass, 1% progress, RAM-buffered logs and
twelve optional journal stages. **All 200
sampled source seeks matched the source**.
There were 100 progress events, two
seek-summary events and a successful
session-completed record.

| Metric | Value |
| --- | ---: |
| Source size / physical source lines | **1,073,741,824 B / 807,433** |
| Snapshot phase | **24,956.187 ms** |
| Journal phase | **46,506.152 ms** |
| Event journal | **2,510,275,305 B** |
| Journal managed allocations | **13,113,772,008 B** |
| Journal GC pause | **724.711 ms** |
| Peak working set | **71,368,704 B (68.06 MiB)** |
| First / repeated warm seek medians | **0.0666 / 0.0568 ms** |
| Full twelve-stage accounted time | **45,595.327 ms** |

| Instrumented group | Aggregate (ms) |
| --- | ---: |
| Scanner advance | 3,747.503 |
| JSON event serialization | 1,831.597 |
| Three async write awaits combined | 276.660 |
| Offset table read await | 79.647 |
| Event window read await | 559.918 |
| JSON event deserialization | 1,850.079 |
| **Event append position setters** | **11,659.125** |
| **Offset append position setters** | **25,440.455** |
| Offset read position setters | 45.544 |
| Event read position setters | 104.799 |
| **Total across all twelve** | **45,595.327** |

**Root-cause localization:** the
69–70% one-percent window consumed
**13,706.391 ms**, of which
`OffsetWritePosition` accounted for
**13,545.21 ms**; its longest individual
setter took **13,489.504 ms** at
source line 564,885 / journal byte
1,756,107,492. The next 70–71%
window consumed **9,657.322 ms**.
Its offset append setters totaled
**5,839.18 ms** and its event append
setters **3,729.64 ms**; the longest
individual operations were **2,923.605**
and **1,506.843 ms**, respectively.
The two windows together lasted
**23,363.713 ms**. Overall event +
offset append position-setting work
was **37,099.580 ms** (about 79.8%
of journal wall time).

Unlike the preceding eight-stage
trace, the main intermittent delays
are now **inside measured synchronous
FileStream.Position setters**.
The evidence supports removing redundant
position assignments on sequential
appends, not blaming the Markdown parser
or individual async write-await calls.
Whether the runtime is flushing internal
buffers, how the OS schedules disk I/O,
and whether device latency contributes
are not established by these logs.

**Separate implementation change:**
the cursor now tests whether the event
and offset streams are already at
their desired append positions before
assigning `Position`. Previously the
setters were called for every source
record even during sequential appends.
Tests verify that after backward
cached reads the needed append
repositioning happens once per stream
and no line data or Markdown context
is lost. Two cumulative actual-setter
counters appear in progress JSONL,
while all existing opt-in phase
timers remain unchanged.

**Next physical experiment after
green CI**: repeat the same 1-GiB
source with `-TraceJournalIo` and
`-ProgressStep 1` and compare
correctness, median windows, hotspot,
total journal time and actual setter
counts. Do not claim an optimization
speedup before this measurement.

## 1-GiB post-optimization measurement — 2026-11-22 15:49 UTC

Paired complete Windows H: files
`mdview-hdd-20261122T154929922Z-93528de2.{jsonl,txt}`.
Same `H:\MDViewTest\data\mdview-synthetic-1024MiB.md`
input path, one pass and 100 indexed random
seek checks. 1% progress, 12 journal stages,
RAM-buffered logs until after measured work;
.NET 10.0.15. Run ended `success` with
all **200/200** source-line samples correct.

| Measurement | Before: unconditional Position (15:04) | After: conditional Position (15:49) |
| --- | ---: | ---: |
| Snapshot | **24,956.187 ms** | **24,283.946 ms** |
| Journal phase | **46,506.152 ms** | **125,347.666 ms** |
| Effective source-throughput (journal phase) | **22.02 MiB/s** | **8.17 MiB/s** |
| Source lines | 807,433 | 807,433 |
| Journal event bytes | 2,510,275,305 | 2,510,275,305 |
| Journal allocated managed bytes | 13,113,772,008 | 12,831,621,336 |
| Journal GC pauses | 724.711 ms | 817.440 ms |
| Peak working set | **68.06 MiB** | **69.31 MiB** |
| Correct seek samples | **200/200** | **200/200** |
| First/repeat warm seek median | 0.0666/0.0568 ms | **0.0588/0.0579 ms** |

This was **2.695× slower (169.5% more
elapsed time)** for the journal phase,
not a successful throughput optimization.
First-pass cold-cache state is *not*
proven comparable, and no low-level
drive latency counters are available.

### Timing breakdown after conditional setters

| Stage | 1-GiB aggregate ms |
| --- | ---: |
| `EventPayloadWrite` | **115,955.056** |
| `ScannerAdvance` | 3,700.705 |
| `DeserializeEvent` | 1,872.787 |
| `SerializeEvent` | 1,422.854 |
| `EventWindowRead` | 656.512 |
| `OffsetReadPosition` | 175.696 |
| `EventReadPosition` | 165.623 |
| `OffsetTableRead` | 118.846 |
| `OffsetWrite` | 61.913 |
| `EventLengthWrite` | 60.717 |
| `EventWritePosition` | **25.227** |
| `OffsetWritePosition` | **22.470** |
| **Accounted (all 12)** | **124,238.405** |
| Journal wall | **125,347.666** |

Crucial: Both cumulative
`event_write_position_setter_calls`
and `offset_write_position_setter_calls`
are **0 in all 100 intervals**.
The implementation change is therefore
activated and prevents redundant append
position setters in this workload.
The blocked time has migrated from
`FileStream.Position` to
`EventPayloadWrite`, where it now
accounts for **92.5%** of journal time.
The 1% interval median increased to
**1,120.469 ms**; worst intervals
were 74% **3,409.1 ms**, 75%
**3,377.1 ms**, 73% **3,327.5 ms**,
and 76% **3,284.4 ms**.
The worst *single* event-payload write
was **203.032 ms** (source line 782,453,
journal offset 2,432,591,519).
The delay became sustained rather than
concentrated in one or two windows.

### Decision and follow-up

Do **not** declare the previous
conditional-setter change an
optimization success. The current
run is observational; thermal/cache
conditions and other HDD activity
can vary. Added a separate
benchmark-controlled, internally
scoped `ForceJournalAppendPositionSetters`
A/B mode to reproduce the original
unconditional-setter code path,
without modifying the normal
viewer or journal format.

The benchmark's `session_start`
will now explicitly name either
`conditional_position_setters`
(default) or
`legacy_unconditional_position_setters`
(`-ForceAppendPositionSetters`).
The forced mode should reach
**807,433** actual assignments for
*each* append stream, whereas the
conditional mode remains at zero
for the sequential benchmark.

**Next experiment:** on the same
prepared source, run conditional B,
forced legacy A, then conditional B
again with exactly the same settings.
Compare per-stage timing, correctness
and aggregate performance before
deciding to revert or redesign. A
single test does not implicate a
specific HDD surface error.

## 2026-11-22 — first verified A/B-control HDD run (18:00 UTC)

The attached, physical Windows H: 1-GiB run
`mdview-hdd-20261122T180043896Z-568a1d79.{jsonl,txt}`
used the same existing synthetic source, one pass, 100 random
seek samples per stage, `-ProgressStep 1 -TraceJournalIo`,
and `-ForceAppendPositionSetters`.
`session_start.journal_append_mode` explicitly records
`legacy_unconditional_position_setters`.
The test **completed successfully** with all **200/200**
source-line checks correct.

| Value | Baseline A (15:04 UTC) | Conditional B (15:49 UTC) | Repeated forced A (18:00 UTC) |
| --- | ---: | ---: | ---: |
| Snapshot ms | 24,956.187 | 24,283.946 | **24,431.445** |
| Journal ms | **46,506.152** | **125,347.666** | **47,552.161** |
| Journal MiB/s | 22.019 | 8.169 | **21.534** |
| Journal managed allocated B | 13,113,772,008 | 12,831,621,336 | **13,113,915,552** |
| Journal GC pause ms | 724.711 | 817.440 | **763.672** |
| Journal peak working-set B | 71,368,704 | 72,675,328 | **71,708,672** |
| Journal physical bytes in event stream | 2,510,275,305 | 2,510,275,305 | **2,510,275,305** |
| Source lines | 807,433 | 807,433 | **807,433** |
| Source-line seek checks | 200/200 | 200/200 | **200/200** |
| Actual event/offset append-position setters | not separately counted | 0 / 0 | **807,433 / 807,433** |
| First/repeat warm seek median ms | 0.0666 / 0.0568 | 0.0588 / 0.0579 | **0.0609 / 0.0570** |

Restoring the unconditional append-position assignments made
the journal phase **2.636× faster than B**. The repeated A
time is only **2.25% slower** than the earlier A result.
This is strong evidence that the conditional-position
change negatively affects the current `FileStream` / HDD
write path, though a repeat of B is necessary to complete
the planned **B–A–B** sequence and account for uncontrolled
machine and disk effects. No production change was made.

### New forced-A stage breakdown

All 100 progress windows were collected. Total measured
time in the 12 stages:

| Stage | Forced A ms | Conditional B ms |
| --- | ---: | ---: |
| `EventWritePosition` | **32,208.397** | 25.227 |
| `OffsetWritePosition` | **5,867.457** | 22.470 |
| `EventPayloadWrite` | **110.480** | **115,955.056** |
| `ScannerAdvance` | 3,681.917 | 3,700.705 |
| `SerializeEvent` | 1,836.631 | 1,422.854 |
| `DeserializeEvent` | 1,996.896 | 1,872.787 |
| `EventWindowRead` | 562.186 | 656.512 |
| 12-stage accounted | **46,663.147** | **124,238.405** |

The stage where a process blocks differs substantially:
in A, `EventWritePosition` (and sometimes
`OffsetWritePosition`) bears almost all write-back
latency; in B, the `EventPayloadWrite` await dominates.
The result is not simply a renamed timer:
the elapsed journal time is **47.55 s vs 125.35 s**,
while scanner advances and byte counts are almost equal.
The A phase used **51,109.375 CPU-ms** and the B phase
**16,421.875 CPU-ms**, consistent with substantially
more I/O-related waiting in B; these process-wide CPU
metrics do not prove a specific OS mechanism.

A residual ~25-second interruption remains in forced A
at 72–73% (20,900.6 + 4,061.8 ms), while the earlier
unconditional A saw ~23.4 seconds at 70–71%
(13,706.4 + 9,657.3 ms).
These spans overlap approximately **1.73–1.83 GB**
of event-journal growth. They may relate to file
growth, NTFS write-back, cache or other OS/disk state,
not necessarily a defective HDD sector. This physical
I/O behavior is a separate question from the large
A-versus-B sustained-throughput regression.

**Next experiment:** run B again on the same unchanged
1-GiB source, without `-ForceAppendPositionSetters`.
Keep `-Seeks 100 -Passes 1 -ProgressStep 1 -TraceJournalIo`.
If B is again dramatically slower while A remains
~46–48 seconds, recommend reverting conditional
append-position behavior in production while preserving
the scan instrumentation; then test and document
the revert on Windows/Linux. Avoid changing journal
serialization, memory caps or cancellation semantics
during this targeted comparison.
