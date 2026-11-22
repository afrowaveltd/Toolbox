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
