# MdView HDD benchmark harness

**Separate, manual-only .NET 10 benchmark.** This project is deliberately
not part of `Toolbox.sln` or the normal xUnit test suite. It measures
validated Markdown snapshot creation, disk-backed container journals,
random source-line seeks and, optionally, the complete mixed visual
page index. The current goal is to compare the same run on the
HGST 80 GB / 5400 RPM HDD and faster storage.

This tool does not alter the default MdView CLI. It does not clear
Linux caches, format disks, delete datasets or request root privileges.
All files are confined to a **separately mounted test filesystem**.

## On Debian 13

Mount the test HDD first (assumed mount path `/mnt/TEST`). Verify:

```bash
findmnt -T /mnt/TEST
lsblk -o NAME,SIZE,ROTA,TYPE,FSTYPE,MOUNTPOINTS
df -hT /mnt/TEST
```

Run all commands from the Toolbox repo root, as the ordinary user.
Use **`bash`** to execute the launcher; GitHub's file API does not
promise the executable bit for new scripts.

### First, a small smoke test

```bash
git pull
bash Blackboard/Tools/MdView.Benchmarks/run.sh /mnt/TEST all --size-mib 8 --seeks 10
```

This generates an 8-MiB fixture, validates the temporary path, scans
the journal and performs a pair of repeatable seek sequences. Check
the resulting JSONL/TXT logs before starting a 2-GiB run.

### Real 2-GiB HDD test

```bash
bash Blackboard/Tools/MdView.Benchmarks/run.sh /mnt/TEST all --size-mib 2048 --seeks 250
```

The streaming generator creates
`/mnt/TEST/data/mdview-synthetic-2048MiB.md` with an **exact
2,147,483,648-byte size (2 GiB)**. This is roughly 2.15 GB in
decimal disk units. The fixture repeats Markdown headings, prose
with long wrapping lines, inline links, 24-row pipe tables,
fenced C# code and Unicode data. It never constructs 2 GiB in RAM.
The generated dataset is retained for later before/after comparisons.

The launcher creates three directories under the mounted drive:

```text
/mnt/TEST/
  data/    mdview-synthetic-2048MiB.md
  tmp/     private validated snapshots, line indexes and
           classified journals (normally delete-on-dispose)
  logs/    mdview-hdd-<UTC timestamp>-<id>.jsonl
           mdview-hdd-<UTC timestamp>-<id>.txt
           system-<UTC timestamp>-<pid>.txt
```

**All temp files are redirected by setting `TMPDIR`, `TMP` and
`TEMP` before .NET starts.** Both `Path.GetTempPath()` and the
validated snapshot's own temporary directory use the benchmark
drive. The program refuses to run if the resolved .NET TEMP path
does not match `/mnt/TEST/tmp`. The launcher also refuses the
system root filesystem, unexpected nested mounts/symlinks and
runs without a conservative disk free-space reserve.
It never formats or mounts any device itself.

The benchmark builds a validated private snapshot, scans the
entire source into the event journal and performs deterministic
random physical-line reads twice using the same source-line
sequence. It records every sampled seek and the distribution
of durations (minimum/median/p95/p99/maximum).

### Repeated comparison runs

```bash
bash Blackboard/Tools/MdView.Benchmarks/run.sh /mnt/TEST measure --size-mib 2048 --passes 2 --seeks 250
```

The source file is **reused**, not overwritten. Each pass builds
and disposes a fresh validated snapshot and journal.
The first and second random-seek sequences within one pass
use identical line numbers in identical order.

### Optional complete visual page-index test

```bash
bash Blackboard/Tools/MdView.Benchmarks/run.sh /mnt/TEST index --size-mib 2048 --width 120 --rows 48 --max-width 100 --long-tables
```

This can be substantially slower on a 5400 RPM HDD because it
measures **all mixed Markdown/native table pages**. It is
intentionally an independent mode and is **not** run by
`all`. The index remains disk-backed and temporary, with
a benchmark-only 256-MiB page-index budget. The benchmark
also samples exact canonical source-line → page lookups.

## Logs to share

Please send the three text-based files from the relevant run in
`/mnt/TEST/logs/`:

- `mdview-hdd-*.jsonl` — machine-readable events with UTC
  timestamps, elapsed phase times, logical MiB/s, GC-allocated
  bytes, working-set/peak memory, process CPU time, journal
  bytes, per-request counters, individual seek samples and
  Linux `/proc/self/io` deltas.
- `mdview-hdd-*.txt` — human-readable equivalent, with
  failures recorded as `session_failed`.
- `system-*.txt` — mount source/filesystem/options, rotational
  flag, disk space, CPU/OS, free memory and .NET version.

Do not interpret `read_bytes` or `EventReadRequestCount` as exact
rotational platter I/O. Filesystem cache and read-ahead affect
the numbers. In particular **the first run is not guaranteed to
have a cold OS page cache**: file generation and prior test
activity can warm it. A valid cold-cache comparison requires a
controlled OS-level method, such as a suitable reboot/remount
process performed by the administrator. The launcher never
flushes caches or invokes privileged commands.

## Safety, bounds and known limitations

- Default fixture is **2 GiB**; `--size-mib` accepts 8–8192.
  This is a real streaming file, not a sparse allocation.
- The launcher checks at least `6 × dataset size + 1 GiB`
  of free capacity at start. The 80-GB drive should therefore
  have plenty of room, provided it is genuinely empty.
- Snapshot size is capped just above the requested source size.
  For this explicitly manual test only, the journal budget
  is raised to **8 GiB** and offsets to **512 MiB** so a
  2-GiB Markdown file can be scanned fully. Production
  defaults (512/256 MiB) remain unchanged.
- Test source rows remain under the validated reader's
  default 1-MiB per-line limit.
- `--seeks` accepts 1–2000, `--passes` 1–4.
- A source file already at the selected benchmark path
  is never truncated. The generator reuses it only if its
  exact byte length and synthetic-file marker match.
  Reuse does not calculate a full-file hash.
- On interruption, a partial newly generated source is
  intentionally preserved for inspection rather than
  silently deleted. Rename/remove **only this specific
  benchmark file** manually before retrying generation.
- Journal, snapshot and index temporaries are normally
  deleted when disposed. A force-killed process may
  leave temporary artifacts; inspect `/mnt/TEST/tmp`
  before removing **only files created by this test**.
- The harness is supported on Debian/Linux, not a general
  user-facing benchmark feature, and creates no NuGet
  release. It has no automatic long-running schedule.

[Full experiment plan](Docs/HDD-Benchmark/en.md) ·
[MdView implementation status](../MdView/IMPLEMENTATION_STATUS.md).
