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
