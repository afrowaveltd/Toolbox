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
