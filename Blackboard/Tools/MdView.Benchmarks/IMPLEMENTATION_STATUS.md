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
- `run.sh ROOT all|generate|measure|index [--size-mib N]
  [--seeks N] [--passes N] [--width N] [--rows N]
  [--max-width N] [--seed N] [--long-tables]`.
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
