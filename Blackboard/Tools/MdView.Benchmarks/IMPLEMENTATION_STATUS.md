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
