# MdView.Benchmarks — Implementation status

## 2026-11-15 — initial isolated 2-GiB HDD benchmark harness

**State:** implemented and committed; **not yet compiled, run or measured**
on the maintainer's Debian 13 / HGST 80 GB 5400 RPM test disk.
The previous ~7,000+ Toolbox regression tests and more recent
maintainer-confirmed green MdView checkpoints predate this new
separate benchmark project.

### Purpose and command surface

- New non-packable .NET 10 console project:
  `Blackboard/Tools/MdView.Benchmarks/MdView.Benchmarks.csproj`.
  Not registered in `Toolbox.sln` intentionally, so normal
  builds/tests and NuGet releases do not run long I/O benchmarks.
- `run.sh ROOT all|prepare|generate|measure|index
  [--size-mib N] [--seeks N] [--passes N]
  [--width N] [--rows N] [--max-width N]
  [--seed N] [--progress-step 1..10] [--long-tables]`.
  Windows supports the same modes via `run.ps1` /
  `run.cmd`, using `-ProgressStep` for the
  optional diagnostic interval.
- Launcher requires a separately mounted non-root filesystem
  (expected `/mnt/TEST`), guards every
  `data/tmp/logs` destination against escape or a different
  nested mount, checks conservative free space, then exports
  `TMPDIR`, `TMP` and `TEMP` before starting .NET.
  `Path.GetTempPath()` must equal `ROOT/tmp`.
  Nothing formats a disk, requires elevated privileges,
  removes OS caches or deletes source files.
- Streaming deterministic Markdown generator writes
  exactly `--size-mib` MiB (default **2048 MiB / 2 GiB**)
  directly to `ROOT/data/mdview-synthetic-NMiB.md`
  using `FileMode.CreateNew`. Fixture includes prose,
  wrapping, Markdown headings, links, 24-row GFM tables,
  fenced code and UTF-8/Unicode. Existing matching-size
  fixtures are reused only when their signature matches;
  no full-file hash is claimed. Interrupted partial
  data is retained for manual inspection rather than
  silently overwritten or deleted.
- `measure`: creates private validated snapshot on
  test volume, constructs production
  `MdViewContainerPageCursor` with *benchmark-only*
  **8 GiB journal / 512 MiB offsets** ceilings,
  sequentially scans all source lines in 128-row windows,
  then performs fixed-seed random source-line seek lists
  twice. Each pass disposes snapshots/journals before
  the next. Original production budgets stay unchanged.
- `index` (separate opt-in): measures the complete
  `MdViewMixedVisualPageIndex` build and canonical
  GetPage/FindPage sample lookups, with a benchmark-only
  256-MiB page index cap and fixed TestConsole geometry.
  This may be very slow for a 2-GiB source.
- Persistent logs under `ROOT/logs`:
  `mdview-hdd-*.jsonl` + `mdview-hdd-*.txt`
  (structured phases, individual seek samples and
  p50/p95/p99, timings, CPU/GC/RSS, journal counts,
  Linux `/proc/self/io` deltas and failure details),
  plus `system-*.txt` (mount, rotational flag,
  filesystem, free space, system/.NET info).
- A small **8-MiB** smoke test is the required first
  validation. The initial file may be warmed in page
  cache by generation, so log labels only distinguish
  first and repeated passes; genuine cold-cache
  conditions require administrator-controlled
  external procedures and are never assumed.

### Project documentation

- `README.md`: quickstart, disk safety and log delivery.
- `Docs/HDD-Benchmark/en.md`: detailed methodology,
  workload, data format, interpretation and limitations.
- This `IMPLEMENTATION_STATUS.md`: state and continuation.

### Initial verification and next tasks

1. Maintainer mounts the test disk, confirms it is not
   the system volume, and makes `/mnt/TEST` writable.
2. Run
   `bash Blackboard/Tools/MdView.Benchmarks/run.sh
   /mnt/TEST all --size-mib 8 --seeks 10`.
   Fix any build/launch issue immediately on GitHub.
3. Run the real two-gibibyte scenario:
   `bash Blackboard/Tools/MdView.Benchmarks/run.sh
   /mnt/TEST all --size-mib 2048 --seeks 250`.
4. Send all three corresponding log files.
5. After reviewing actual measured results, optionally
   repeat `measure --passes 2` and invoke
   `index --long-tables` separately.
6. Compare first/repeated timings and cache/IO
   statistics cautiously. Do **not** equate journal
   API read counts with physical platter operations
   or claim first pass is automatically a cold cache.
7. Record verified results here; if improving bottlenecks,
   commit, document and test each change separately.

No runtime numbers, throughput speedups or green
benchmark compilation result are claimed yet.

## 2026-11-15 — launch hardening and static validation

- Follow-up launcher safeguards committed:
  `f3284dcd4e9ec32fd1a9103091fc2daa41cd1fe0`
  transmits the **actual df-mounted volume free space**
  to the process instead of assuming `DriveInfo`
  identifies a nested mount correctly; commit
  `72e7ccc9aec6ebdd6c48d31eae2ccaee37b6ab76`
  also refuses system-filesystem bind mounts
  (matching MAJ:MIN) and tmpfs/ramfs/overlay targets.
- Updated the .NET harness to require this
  launcher-measured free-space environment variable
  (`3ef82d0841bd35e8c0e3fe6a3e826ae77c3be546`).
  Removed phase logging of live disposable snapshot/index
  objects in favor of scalar performance metrics
  (`72da4ff0f929461162638963b2cb47ee5d7e9320`).
- Inspected the latest GitHub source, including a
  lexical bracket-balance check of the C# program:
  no unmatched braces, brackets or parentheses found.
  **This is not a .NET compilation or runtime test.**
- The initial 8-MiB smoke test on a separately
  mounted disk is still required before the full
  2-GiB HDD experiment.

## 2026-11-22 — Windows H: and cross-platform fixture preparation

**Code committed, real-disk validation pending.** The initial
benchmark only supported the `run.sh` Linux launcher. This update:

- Adds a PowerShell 5.1+ `run.ps1` and Command Prompt `run.cmd`
  launcher for Windows, defaulting to `H:\MDViewTest`. Optional
  `-Root`/`MDVIEW_BENCH_ROOT` allows another explicitly designated
  test filesystem. Physical-disk checks use Get-Partition to ensure
  the target drive is not a partition on the Windows system disk.
  Offline/readonly disks, nonlocal volumes, volume roots, symlink/
  junction directories and insufficient free space are refused.
- Redirects `TEMP`, `TMP`, `TMPDIR` and the volume free-space
  measurement before launching .NET, and creates a system report
  under `logs`. The fixture dataset and temporary snapshot,
  journal and index files remain confined to the test disk.
- Adds manual `prepare` mode to the existing .NET harness:
  streaming generation and safe reuse of exact-size marker-verified
  `8/128/1024/2048` MiB Markdown fixtures (3208 MiB in total).
  The existing `all/generate/measure/index` flows remain intact.
- Updates .NET temporary-directory comparison for Windows
  case-insensitivity, preserving case-sensitive Linux comparison.
- Changes the Linux launcher to require a supplied directory
  or `MDVIEW_BENCH_ROOT`, rather than assuming `/mnt/TEST`.
  `prepare` uses a 2-GiB minimum reserve in the Linux preflight.
- Updated README and English HDD methodology documentation.

**Verification:** GitHub changes and content presence checked;
no .NET SDK, Windows PowerShell runtime or actual 80-GB test disk
is available within the assistant's execution environment.
No successful build/test or measured throughput is claimed.
Next: on the real Windows H: volume, run
`run.cmd all -SizeMiB 8 -Seeks 10`, inspect
`H:\MDViewTest\logs`, then `run.cmd prepare` and
`run.cmd measure -SizeMiB 2048 -Seeks 250 -Passes 2`.
After the Linux mount point is provided, repeat the 8-MiB
smoke test through `run.sh` and compare results.

### Cross-platform build CI

The main Toolbox solution intentionally excludes the isolated benchmark,
so the ordinary matrix would not compile it. A separate GitHub Actions
job now builds `MdView.Benchmarks.csproj` on both Ubuntu and Windows
using .NET 10, without invoking `run.sh`, `run.ps1`, creating large
fixtures or requiring a real HDD. Cross-platform build results remain
pending until that CI run finishes. This is not a substitute for
real-HDD smoke tests on Windows H: and the future Linux mount.

### Launcher syntax coverage

The two-platform benchmark CI job now validates the PowerShell
script through PowerShell's own AST parser and the Linux launcher
through `bash -n` before compiling the project. These are
syntax checks only; the separate physical-disk guards and benchmark
workloads are deliberately not run on hosted CI disks.

## 2026-11-22 — verified Windows H: smoke + complete fixtures

**Actual maintainer logs reviewed:** successful 8-MiB smoke test and
`prepare` fixture generation on separate physical Windows disk 8
(Hitachi HTS541680J9SA00, USB/NTFS), with system disk 7.
Recorded runtime: .NET 10.0.14; reported CLI SDK: .NET 11 RC3.
See [dated Windows baseline](Docs/Results-2026-11-22/en.md)
for complete evidence, rates, memory and cache caveats.

- 8 MiB: generated, source snapshot **282.257 ms**,
  sequential journal **530.133 ms** and **19,569,254**
  bytes of journal; max working set **62,078,976** bytes.
  Random seek medians **0.2020 ms first**,
  **0.1374 ms repeat**. Correct source lines; the
  recorded seek durations are cached/indexed latencies,
  not established physical HDD seek times.
- Prepared data: existing 8 MiB reused, with
  **128 / 1024 / 2048 MiB** generated at
  **40.912 / 44.754 / 43.727 MiB/s** respectively.
  `fixture_set_completed` reports 3208 MiB.
- Found stale `free_bytes` reporting within a single
  `prepare` invocation: originally the launcher sampled
  the volume only once. Patched Windows to refresh current
  `DriveInfo` available space at every generator preflight,
  bounded by the initial launcher-verified allowance;
  Linux still marks its df-derived measurement as initial.
  A fresh local run is required to verify this follow-up.
- GitHub Actions run
  [45022663997](https://github.com/afrowaveltd/Toolbox/actions/runs/45022663997)
  **SUCCESS** before this follow-up patch, including
  both isolated benchmark compilation/launcher syntax checks.

**Next:** `run.cmd measure -SizeMiB 128 -Seeks 100 -Passes 2`.
Keep the existing `H:\MDViewTest\data` directory intact.
After 128 MiB, compare the journal:source ratio and managed
allocations before running 1/2 GiB and the opt-in visual index.

## 2026-11-22 — Windows H: 128-MiB two-pass regression evidence

**Result: PASS for both complete passes, no runtime errors.**
The maintainer supplied `mdview-hdd-20261122T124024080Z-a39cf10e`
as both JSONL and text. Same `H:\MDViewTest` physical USB/NTFS HDD,
runtime .NET 10.0.14, seed 20261115.

- Snapshot: 134,217,728 bytes / 100,930 source lines;
  offset index: 807,448 bytes. Snapshot wall times
  **3,314.893 ms** (pass 1) and **3,490.572 ms** (pass 2).
- Complete journal was identically **313,601,334 bytes**
  in both passes (**2.3365×** source); successful sequential
  scans **4,102.406 ms** (31.201 MiB/s) and **2,770.607 ms**
  (46.199 MiB/s), 32.46% lower elapsed on pass 2.
- Per-phase managed allocation totals:
  **1,639,326,568** and **1,638,737,728** bytes
  (about 12.21× source). Process peak working set:
  **69,615,616** and **70,672,384** bytes respectively.
  Original `gc_gen0/gen1/gen2` metrics are process-lifetime
  cumulative counts, so they must not be mislabeled phase
  deltas; first pass journal snapshot ended with 93/92/92,
  second pass ended with 187/185/185.
- All **400** deterministic sampled requests (100 each
  in first/repeat series for 2 passes) returned the correct
  source line. Median request times across the four series:
  **0.0546 / 0.0597 / 0.0562 / 0.0582 ms**.
  These reflect warm indexed/journal reads and are not
  verified physical HDD seeks.
- Both passes completed with exactly **989** cursor
  requests and **1,777** event stream reads. Windows
  `proc_io_delta` is unsupported and zero-filled.

**Follow-up instrumentation change:** `TimedAsync`
now adds `gc_gen0_delta/gc_gen1_delta/gc_gen2_delta`
on success and failure while preserving old cumulative
`gc_gen0/gc_gen1/gc_gen2` fields for compatibility.
This measures GC collections attributable to each timed
phase on the next run, not a retrospective alteration of
the supplied logs. Production MdView code was untouched.

**Next stage:** run the prepared 1-GiB fixture with
`run.cmd measure -SizeMiB 1024 -Seeks 100 -Passes 1`
after pulling current master; inspect ~2.34-GiB expected
journal and GC activity, then decide whether 2 GiB
and optional visual index are warranted.

## 2026-11-22 — Windows H: 1-GiB benchmark completed; phase stalls to investigate

**Result: PASS.** Maintainer provided paired logs
`mdview-hdd-20261122T124433618Z-31f383d5` (JSONL/TXT).
On the previously verified Windows H: USB/NTFS Hitachi HDD,
`measure -SizeMiB 1024 -Seeks 100 -Passes 1` ran under
**.NET runtime 10.0.15**, compared with runtime 10.0.14 in
the earlier 8/128-MiB logs.

- Validated snapshot: **1,073,741,824 bytes**,
  **807,433 source lines**, offset index **6,459,472 bytes**;
  **24,711.728 ms** (41.438 MiB/s), only **430,672
  GC-allocated bytes**, **zero** GC collections in this phase.
- Complete event journal: **2,510,275,305 bytes**
  (**2.33788×** source = 2.33788 GiB);
  **47,203.060 ms** (21.694 MiB/s), **13,111,191,408 bytes**
  in cumulative managed allocations during the phase
  (12.211× the source), with **743/742/742 Gen0/1/2**
  collections counted *as phase deltas*.
  Peak process working set: **71,610,368 bytes (68.29 MiB)**.
- Two 100-seek sequences: **200/200 correct** starting
  physical lines; first median **0.0714 ms**, p95 **0.1009 ms**,
  repeat median **0.0667 ms**, p95 **0.0959 ms**.
  Journal cache makes these *not* physical HDD seeks.
- Complete pass: **6,509** cursor requests,
  **12,817** journal event stream read requests,
  `session_completed` with no reported failures.
- **Major latency outlier:** the 60%→70% journal progress
  window took **23.805 s**, versus roughly 2.2–2.9 s
  for other post-start ten-percent windows. That single
  gap accounts for about half the journal scan wall time.
  Cause **unconfirmed**: could be GC, disk I/O, external
  scheduling/antivirus or another source. Windows per-process
  physical I/O diagnostics are not implemented; zero
  `proc_io_delta` values do not establish absence of I/O.
  Do not attribute the stall to a specific component without
  finer timing evidence.

**Follow-up committed instrumentation:** `journal_progress`
now also logs, per ten-percent window, elapsed ms, managed
allocation bytes, Gen0/Gen1/Gen2 collection deltas,
process CPU ms, event-journal bytes written, and current
working set. Original progress fields are retained.
This is a benchmark-only diagnostic change; no production
MdView serializer or API was changed.

**Next:** repeat the **already generated 1-GiB file** with
`run.cmd measure -SizeMiB 1024 -Seeks 100 -Passes 1`
after `git pull`. Examine ten-percent interval metrics to
determine whether the 60%→70% latency spike recurs and
whether GC collections or CPU usage accompany it. The
2-GiB source is prepared but **not yet measured**;
its estimated event journal is ~4.68 GiB based on the 1-GiB
ratio (below the benchmark-only 8-GiB cap, not an observed
2-GiB figure). Do not infer constant memory for every
input from the synthetic dataset alone.

## 2026-11-22 — repeatable 60–70% stall; finer diagnostics

**Windows H: 1-GiB repeat passed.** Maintainer-provided
`mdview-hdd-20261122T124949240Z-f8e63090.{jsonl,txt}`
completed with all **200** source-line seek checks correct,
24,557.974 ms snapshot, 46,629.183 ms journal and
2,510,275,305 bytes of journal.

A major wall-time outlier reproduced in the same 60–70%
journal window: **24,682.724 ms** (previous run
~23,805 ms); typical interval median **2,279.064 ms**.
Despite the delay, that window's CPU **4,937.5 ms**,
managed allocations **1,311,461,688 bytes**, GC
collections **74/74/74**, journal growth **251,123,906
bytes**, and working set **69,091,328 bytes** were
comparable to adjacent intervals. **Cause unknown**:
potential disk I/O or Windows/filesystem/scheduler wait;
GC collections alone do not measure pause time.

**Code change:** Added benchmark-only configurable
`--progress-step 1..10` (default 10), forwarded from
Windows `-ProgressStep` and accepted by Linux launcher
passthrough. Progress events additionally report
`interval_gc_pause_ms`; `timed_phase` reports
`gc_pause_ms_delta`. Both use the .NET 10
`GC.GetTotalPauseDuration()` API. No production cursor
or file format was changed. These new metrics are
unverified on real HDD until the next run, and
smaller progress steps alter diagnostic I/O overhead.

**Next:** verify GitHub Actions Linux/Windows benchmark
builds and PowerShell syntax. Then run
`run.cmd measure -SizeMiB 1024 -Seeks 100 -Passes 1
-ProgressStep 1` after `git pull`. Compare GC pause
time to stall wall time; inspect narrowed journal
offset range and Windows disk activity if needed.
2-GiB fixture remains prepared but untested.

## 2026-11-22 — GC pause exonerated for repeated 1-GiB stall

**Maintainer log verified:** `mdview-hdd-20261122T125720117Z-015b7a39`
(JSONL/TXT), a successful 1-GiB `-ProgressStep 1` run.
Snapshot **24,805.267 ms**, journal **48,091.018 ms**,
peak working set **71,741,440 bytes**, journal size
**2,510,275,305 bytes**, GC pause for the entire scan
**730.739 ms**; 200/200 seek requests passed.

The **single 69–70%** interval took **24,918.810 ms**
(median other 99 windows **221.017 ms**), with
**656.25 ms CPU**, **131,414,008 bytes allocated**,
**7/7/7 collections**, and **only 8.182 ms** of
actual GC pause duration (0.033% of the interval).
Its exact recorded journal offset boundaries are
**1,732,164,328 → 1,757,269,452 bytes** and
source-line boundaries **557,184 → 565,248**.
This is the third successful 1-GiB run showing a
repeatable approximately 25-second stall. GC pauses
cannot explain the extra elapsed time. Cause is not
proven to be disk I/O or a fault in the physical drive.

**Important instrumentation ambiguity discovered:**
the progress elapsed measurement includes the preceding
`BenchLog.Write` call, which synchronously writes JSONL,
TXT and console output on the test volume. We therefore
cannot distinguish a slow progress-log write from
a slow cursor request using the supplied run alone.

**Follow-up committed benchmark-only patch:** each progress
event now records total and maximum time spent inside
individual 128-line `cursor.ReadAsync` calls, a >100ms
request count, starting source line and before/after
journal offsets of the slowest call, and the
`previous_progress_log_write_ms` value. It adds no
per-request log writes and preserves existing log keys.
Neither production MdView cursor nor its persistent
journal schema was changed.

**Next:** check Linux/Windows benchmark CI and re-run
`run.cmd measure -SizeMiB 1024 -Seeks 100 -Passes 1
-ProgressStep 1` after pulling master. Compare the
cursor call durations against previous-progress logging
duration at the 69–70% boundary. The prepared 2-GiB
dataset is not yet benchmarked. Continue to avoid
unverified disk-health claims and keep cold-cache
assumptions explicit.

## 2026-11-22 — RAM-only diagnostics during HDD benchmark

**New maintainer evidence:** the paired 13:02 UTC
1-GiB logs `mdview-hdd-20261122T130231087Z-d6558207`
show successful snapshot and journal passes, with
200/200 correct seek checks. The 1-GiB journal
was again **2,510,275,305 bytes**, scan time
**50,242.984 ms**, process peak working set
**72,560,640 bytes** and entire-scan GC pause
**1,022.423 ms**.

The previous 69–70% stall **did not recur** (only
362.070 ms). Instead, 53–54%, 55–56% and
56–57% took **1,985.259 / 6,824.672 /
5,259.895 ms**. Their slowest individual
`cursor.ReadAsync` calls were **1,184.129 /
2,725.929 / 1,899.474 ms**; prior progress
diagnostic writes were below **0.24 ms**.
This is evidence of slow cursor calls with
variable source-line location, not that logging
caused those stalls or that the disk is defective.

**User-requested isolation is committed:**

- `BenchLog` now stores JSONL/TXT diagnostic
  records **only in managed RAM** while any
  workload is timed (no per-progress file or
  console writes). Both reports are exported
  to unique `ROOT/logs` filenames only after
  `session_completed`, or best-effort after a
  handled failure.
- Memory cap: **32 MiB total serialized
  UTF-8 log output**. The logger will not
  silently spill its buffered diagnostics to
  the target HDD under memory pressure.
  Abrupt process termination may lose logs.
- Windows `run.ps1` defers its system report
  until after benchmark process exit; Linux
  `run.sh` captures the report in a shell
  variable and likewise exports it after exit.
  Both launchers still verify the target
  physical disk/filesystem and keep
  source/snapshot/journal/index temp data on
  the selected test drive.
- The isolated benchmark CI matrix now invokes
  `--self-test-log-buffer`, which exercises
  in-memory logging and delayed JSONL/TXT
  persistence without a physical HDD.
  A passed CI run is required before claiming
  the new code is verified on Windows/Linux.

**Next:** inspect the CI results for
`8f1071408328c6874773591618ffcee24306cbe2`
and this documentation commit; after successful
compilation, collect one identical 1-GiB
`-ProgressStep 1` run for a clean comparison.
The physical 2-GiB test is not yet performed.
See [Windows HDD evidence](Docs/Results-2026-11-22/en.md).

### Verification — RAM diagnostics

The updated `MdView benchmark compile` GitHub
Actions workflow
[run 45026094106](https://github.com/afrowaveltd/Toolbox/actions/runs/45026094106)
passed **both Windows and Ubuntu builds** and the
new `--self-test-log-buffer` on each OS. It also
passed the ordinary Toolbox build/test and documentation
check; the Windows UI regression job's individual
test steps passed as well. This verifies isolated
RAM logging and its delayed export on CI runners,
but **not physical-HDD behavior after the change**.
Windows/Linux launchers were syntax-checked only on CI;
real-disk safety guards are exercised by the maintainer.

## 2026-11-22 — 1-GiB HDD benchmark verified with RAM-only diagnostics

**Maintainer's new paired logs:** 
`mdview-hdd-20261122T143043809Z-ec6979b9.{jsonl,txt}`.
The session explicitly reports
`diagnostic_output: memory_buffered_then_persisted_after_test`
and completed successfully, proving one real Windows H:
run through the new logger path (RAM buffering and
post-test export). 1-GiB source, 807,433 source lines,
1% progress, one journal pass, 200/200 correct
deterministic seek checks.

- Snapshot **25,591.460 ms**; journal **48,484.762 ms**,
  journal size **2,510,275,305 bytes**; managed
  journal allocations **13,111,881,424 bytes**.
- Peak process working set **71,987,200 bytes**
  (68.65 MiB); **743/742/742** journal-phase GC
  collections and **702.752 ms** total GC pause.
- The largest stalled windows shifted again:
  **66–67%: 15,272.928 ms**, and
  **67–68%: 9,708.212 ms**. The cursor requests
  account for virtually all of those durations;
  the longest single `ReadAsync` was
  **13,194.393 ms** starting at source line
  **539,904** and journal offset
  **1,678,489,154 bytes**. Window GC pauses were
  only **8.384/7.513 ms**, and preceding progress
  log serialization times **0.0155/0.0206 ms**.
- Relative to 13:02 disk-logged run, journal wall
  time decreased **3.50%**; this is not an isolated
  causal estimate due to uncontrolled caches and
  highly variable stalls. Random indexed seek
  timings are warm-cache results, not physical
  disk-head seeks.

**Current assessment:** RAM-only diagnostics
successfully removes synchronous diagnostic file
writes from measured phases, but long cursor stalls
persist. Cause remains unproven; no evidence supports
claiming the HDD is failing or that GC caused the
25-second gap. Correctness, journal size and
managed peak memory are stable across runs.

**CI:** previously confirmed
[45026296390](https://github.com/afrowaveltd/Toolbox/actions/runs/45026296390)
completed **SUCCESS** on Windows/Linux benchmark compile,
RAM logger self-tests, normal Toolbox build and tests,
Windows UI regressions, and Markdown docs links.

**Next action:** run the prepared 2-GiB
`run.cmd measure -SizeMiB 2048 -Seeks 100 -Passes 1
-ProgressStep 1` after `git pull`. Expected journal
is ~4.68 GiB based on synthetic ratio, below the
8-GiB test-only maximum; free disk preflight applies.
This is **not yet a measured 2-GiB result**.

## 2026-11-22 — 2-GiB HDD benchmark completed successfully

**Maintainer evidence reviewed:**
`mdview-hdd-20261122T143553385Z-a046cac3`
(JSONL + TXT). Full successful Windows H: test
`measure -SizeMiB 2048 -Seeks 100 -Passes 1
-ProgressStep 1`, .NET 10.0.15 and
**RAM-buffered diagnostic export**. Source
**2,147,483,648 bytes / 1,614,865 lines**;
validated snapshot **50,209.278 ms**; full
journal **5,022,177,346 bytes (4.677 GiB)**;
sequential journal scan **123,166.942 ms**
(16.628 MiB/s). All **200/200 sampled
source-line seeks correct**.

Peak process working set **72,130,560 bytes
(68.79 MiB)**, just **143,360 bytes** above the
1-GiB RAM-logged test. GC allocations during
journal scan **26,227,462,992 bytes**,
collections **1485/1484/1484**, aggregate GC
pause **1,394.987 ms**. Journal, allocations,
GC counters, and snapshot time approximately
double; journal wall time is **2.54×** the
1-GiB baseline due to irregular stalls.

Five progress intervals took **74.429 s /
60.4% of journal wall time**:
34–35% **23.847 s** (max `ReadAsync`
**18.556 s**), 53–54% **16.543 s**,
73–74% **17.513 s**, 98–99% **2.883 s**,
99–100% **13.644 s**. Typical sub-1.5-s
window median **445.375 ms**. GC pause
in the longest interval was **14.988 ms**,
so GC suspension does not explain the
stall. The largest cursor call begins at
source line **557,056** / journal
offset **1,731,783,858 bytes**.

Source inspection identifies three
`FileStream.WriteAsync` calls per physical
source line (prefix, JSON payload, offset);
at this fixture size approximately
**4.84 million logical writes**. This is
a potential write batching/GC optimization,
**not** a proven stall root cause.
No production code was changed in this
results/documentation step.

**NEXT — controlled journal I/O diagnosis:**
Instrument sub-operations within
`MdViewContainerPageCursor.ScanUntilAsync`
(scanner, JSON serializer, event prefix,
payload and offset I/O) *optionally*, with
low overhead and RAM-buffered counters.
Test and compare before modifying output
format, write batching or cancellation
handling. Do not claim a physical HDD fault:
no actual Windows physical I/O tracing is
included. A Linux physical-HDD comparison
and visual-page-index benchmark remain
pending. See the [2-GiB measured-results
section](Docs/Results-2026-11-22/en.md).

## 2026-11-22 — optional journal operation timings available

Following the completed 2-GiB HDD baseline (all
**200/200** sampled seeks correct), an internal
fixed-size `MdViewJournalScanTimings` accumulator
now times scanner advancement, JSON serialization,
event length write, event payload write and offset
write **only** when explicitly attached to a
`MdViewContainerPageCursor`. No public interface,
persistent journal format or production write
behavior changes. Cumulative time, worst call,
calls >=100 ms, worst source line and journal offset
are captured. `TakeWindowAndReset()` supports
bounded per-percentage telemetry.

The harness accepts `--trace-journal-io` (Windows
`-TraceJournalIo`) and emits
`journal_scan_stages` inside RAM-buffered
`journal_progress` entries. It defaults to off.
The GitHub Actions benchmark matrix additionally
runs the focused regression test on Linux/Windows
(no large file generation). Until CI is green,
compile/correctness is **pending verification**.
No physical HDD trace has yet been collected with
this option and performance improvements are **not
claimed**.

**NEXT:** verify CI and run
`run.cmd measure -SizeMiB 128 -Seeks 100 -Passes 1
-ProgressStep 1 -TraceJournalIo`. If correct,
repeat with 1 GiB on the prepared fixture;
inspect per-stage slowest operation and aggregate
time before deciding whether write batching is
worth implementing. The independent full visual-page
`index` performance baseline and Linux physical
HDD comparison remain pending.

## 2026-11-22 — physical 128-MiB profile and readback expansion

The maintainer supplied and we analyzed both files
`mdview-hdd-20261122T145233390Z-5ca3b47a.{jsonl,txt}`.
Session: **Windows H:, .NET 10.0.15, one pass,
128 MiB, 100 sampled seeks repeated twice**,
`-ProgressStep 1 -TraceJournalIo`,
RAM-buffered diagnostics. Completed successfully.

**Snapshot:** **3,455.2965 ms**, 37.045 MiB/s,
100,930 physical source lines, 807,448-byte index.
**Journal:** **4,159.7241 ms**, 30.771 MiB/s,
313,601,334 bytes. Managed allocations
1,640,653,072 bytes, journal GC pause
96.631 ms, peak process working set
71,892,992 bytes (68.56 MiB).
All 200/200 sampled seeks returned the
expected source lines; warm first/repeat
medians **0.0496/0.0489 ms**.

The 100 progress windows had median
**35.504 ms**, maximum **110.301 ms**
(1%, startup) and **zero** cursor
requests above 100 ms. The five measured
stages totaled: scanner **772.621 ms**,
serializer **573.825 ms**, event prefix
write **15.907 ms**, event payload write
**17.226 ms**, offset write **11.617 ms**.
All writes combined: **44.750 ms**.
Accounted original stages: **1,391.197 ms**,
leaving about **2,768.527 ms** of the
overall journal scan outside them.
This *cannot* be assigned to reads
without measuring them; the first
profile did not time readback.

**Implemented next:** three additional
optional readback stages:
`OffsetTableRead`, `EventWindowRead`
and `DeserializeEvent`. They are now
reported in the same RAM-buffered
`journal_scan_stages` progress JSON.
The focused MdView test covers source
correctness, first-stage calls, cached
navigation (reads without writes),
continuation, and resets. Runtime
journal format and public API unchanged.
New CI verification is pending until
latest workflow completes.

**NEXT:** after green CI, run the
existing 1-GiB fixture with
`run.cmd measure -SizeMiB 1024 -Seeks 100
-Passes 1 -ProgressStep 1 -TraceJournalIo`.
If a stall occurs, compare the
three new readback-stage maxima with
the five writer/scanner maxima and
`interval_cursor_read_total_ms`.
No specific disk hardware diagnosis
has been established.

## 2026-11-22 — 1-GiB eight-stage trace, 12-stage position follow-up

Received full paired
`mdview-hdd-20261122T145716681Z-543ebae1`
results (Windows H: HDD, .NET 10.0.15,
prepared 1-GiB Markdown, 1% progress,
eight-stage `-TraceJournalIo`,
RAM-buffered log export).
Snapshot **25,123.997 ms**, 807,433
source lines. Journal **47,723.939 ms**,
**2,510,275,305 bytes**, process
peak **73,170,944 bytes (69.781 MiB)**,
GC pause **753.859 ms**. All
**200/200 sampled indexed seeks
returned correct source lines**.

Eight stage totals:
scanner **3,807.094 ms**, serialization
**1,838.367 ms**, event-length
write **88.150 ms**, event-payload
write **115.499 ms**, offset write
**68.508 ms**, offset read
**80.465 ms**, event read
**573.736 ms**, deserialization
**1,920.612 ms**. Total
**8,492.430 ms** of 47,723.939-ms
journal wall time.

**Reproduced bottleneck:** 67–68%
interval **23,035.088 ms**, journal
**1,682,002,689–1,707,059,208 bytes**.
Worst cursor `ReadAsync` **14,803.923 ms**
at source line 544,512 and journal
offset 1,692,778,274 bytes.
Eight stages accounted for only
**75.655 ms** and GC pause
**7.797 ms**; previous log write
**0.0238 ms**. None of the eight
stage types had a 100-ms+ call.
The large stall is inside
`ReadAsync` but **outside** the
eight instrumented operations.

Code reviewed: assigning
`_events.Position` /
`_offsets.Position` occurs before
per-read/write time measurement.
Added four opt-in timing stages:
`EventWritePosition`,
`OffsetWritePosition`,
`OffsetReadPosition`,
`EventReadPosition`.
Now **twelve fixed-size in-memory
stage counters**, disabled by default.
The focused test verifies first
read/write, cached backward reading,
forward extension and counter reset.
Source format, limits, failure-closed
semantics and write buffering remain
untouched. CI for new revision
must be checked.

**NEXT:** physical 1-GiB rerun after
green CI:
`run.cmd measure -SizeMiB 1024
-Seeks 100 -Passes 1 -ProgressStep 1
-TraceJournalIo`. Check position
setter worst call/offset against
long cursor calls and 1.69-GB
journal hotspot. Do not claim
a root cause or hardware failure
until directly measured.
