# MdView HDD benchmark harness

**Separate, manual-only .NET 10 benchmark.** This project is deliberately
not part of `Toolbox.sln` or the normal xUnit test suite. It measures
validated Markdown snapshot creation, disk-backed container journals,
random source-line seeks and, optionally, the complete mixed visual
page index. The current goal is to compare the same run on a slow 80-GB HDD
(25–45 MB/s sequential throughput, approximately 20 ms average seek)
and faster storage, first on Windows H: and later on Linux.

This tool does not alter the default MdView CLI. It does not clear
Linux caches, format disks, delete datasets or request root privileges.
All files are confined to a **separate test volume**, verified
by the platform-specific launcher. The normal solution tests never
invoke this utility or generate large files.

## Windows — H:\MDViewTest

Use Windows PowerShell 5.1+ with .NET 10 SDK. The launcher defaults
to `H:\MDViewTest`, or accepts `-Root` /
`MDVIEW_BENCH_ROOT` to select another dedicated test volume.
It identifies the physical disk behind the drive letter and
**refuses the Windows system disk** (including a different partition
on that same disk). It also rejects junctions/symlinks in the
benchmark directory, checks free space and redirects .NET TEMP/TMP
to the same volume.

From the Toolbox repository root, run this **small test first**:

```powershell
powershell -NoProfile -ExecutionPolicy Bypass -File .\Blackboard\Tools\MdView.Benchmarks\run.ps1 -Mode all -SizeMiB 8 -Seeks 10
```

Prepare the complete reusable test set, explicitly and without
running the expensive benchmarks:

```powershell
powershell -NoProfile -ExecutionPolicy Bypass -File .\Blackboard\Tools\MdView.Benchmarks\run.ps1 -Mode prepare
```

This produces 8-, 128-, 1024- and 2048-MiB Markdown files in
`H:\MDViewTest\data` (3208 MiB total), reusing compatible
existing fixtures without overwriting them.

After inspecting smoke-test logs, run the main HDD scenario:

```powershell
.\Blackboard\Tools\MdView.Benchmarks\run.cmd all
```

This defaults to a **2-GiB** generation/reuse plus journal and seek
measurement. Or separate generation from measurement:

```powershell
.\Blackboard\Tools\MdView.Benchmarks\run.cmd generate -SizeMiB 2048
.\Blackboard\Tools\MdView.Benchmarks\run.cmd measure -SizeMiB 2048 -Passes 2
```

For the optional full visual index use
`run.cmd index -SizeMiB 2048 -LongTables` only after the
smoke and journal tests have been validated.

Windows output is kept under `H:\MDViewTest\data`,
`H:\MDViewTest\tmp` and `H:\MDViewTest\logs`.
System reports describe the physical disk and filesystem;
Linux-only `/proc/self/io` counters are not available on Windows.
The launcher does not guarantee an OS-cold filesystem cache.

## On Debian 13

Mount the HDD first. **The Linux mount point is not assumed**:
replace `/your/mounted/disk` below with the actual mount
point, or set `MDVIEW_BENCH_ROOT`. Verify:

```bash
findmnt -T /your/mounted/disk
lsblk -o NAME,SIZE,ROTA,TYPE,FSTYPE,MOUNTPOINTS
df -hT /your/mounted/disk
```

Run all commands from the Toolbox repo root, as the ordinary user.
Use **`bash`** to execute the launcher; GitHub's file API does not
promise the executable bit for new scripts.

### First, a small smoke test

```bash
git pull
bash Blackboard/Tools/MdView.Benchmarks/run.sh /your/mounted/disk all --size-mib 8 --seeks 10
```

This generates an 8-MiB fixture, validates the temporary path, scans
the journal and performs a pair of repeatable seek sequences. Check
the resulting JSONL/TXT logs before starting a 2-GiB run.

### Real 2-GiB HDD test

```bash
bash Blackboard/Tools/MdView.Benchmarks/run.sh /your/mounted/disk all --size-mib 2048 --seeks 250
```

The streaming generator creates
`/your/mounted/disk/data/mdview-synthetic-2048MiB.md` with an **exact
2,147,483,648-byte size (2 GiB)**. This is roughly 2.15 GB in
decimal disk units. The fixture repeats Markdown headings, prose
with long wrapping lines, inline links, 24-row pipe tables,
fenced C# code and Unicode data. It never constructs 2 GiB in RAM.
The generated dataset is retained for later before/after comparisons.

The launcher creates three directories under the selected drive
(Windows uses the same `data/tmp/logs` names):

```text
/your/mounted/disk/
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
does not match `/your/mounted/disk/tmp`. The launcher also refuses the
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
bash Blackboard/Tools/MdView.Benchmarks/run.sh /your/mounted/disk measure --size-mib 2048 --passes 2 --seeks 250
```

The source file is **reused**, not overwritten. Each pass builds
and disposes a fresh validated snapshot and journal.
The first and second random-seek sequences within one pass
use identical line numbers in identical order.

### Optional complete visual page-index test

```bash
bash Blackboard/Tools/MdView.Benchmarks/run.sh /your/mounted/disk index --size-mib 2048 --width 120 --rows 48 --max-width 100 --long-tables
```

This can be substantially slower on a 5400 RPM HDD because it
measures **all mixed Markdown/native table pages**. It is
intentionally an independent mode and is **not** run by
`all`. The index remains disk-backed and temporary, with
a benchmark-only 256-MiB page-index budget. The benchmark
also samples exact canonical source-line → page lookups.

## Logs to share

Please send the three text-based files from the relevant run in
`/your/mounted/disk/logs/`:

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

- The explicit `prepare` mode generates 8/128/1024/2048 MiB fixtures;
  `--size-mib` controls individual `all/generate/measure/index`
  operations and does not change the fixed prepare set.
- Default individual fixture is **2 GiB**; `--size-mib` accepts 8–8192.
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
- `--seeks` accepts 1–2000, `--passes` 1–4,
  `--progress-step` 1–10 (default 10). Windows uses
  `-ProgressStep`. With RAM-buffered logging, a 1%
  interval records more in-memory diagnostic events
  without additional progress-log writes on the HDD;
  serialization still incurs CPU/GC overhead, so use
  this mode for **diagnostics**, not strict throughput comparisons.
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
  leave temporary artifacts; inspect `/your/mounted/disk/tmp`
  before removing **only files created by this test**.
- The harness has Windows and Linux launchers, but remains a manual,
  non-release benchmark utility and creates no NuGet release.
- Windows H: **8-MiB smoke, all four fixture preparations,
  two-pass 128-MiB journal/seek and single-pass 1-GiB
  journal/seek benchmarks passed** in maintainer logs dated
  2026-11-22. A 23.8-second progress gap at 60–70%
  of the 1-GiB journal build repeated in a second run.
  CPU, GC collection counts and allocations stayed similar
  to normal intervals; the exact cause remains unknown.
  A diagnostic `-ProgressStep 1` option on Windows
  (or `--progress-step 1` on Linux) now samples narrower
  intervals including GC pause time. Two-GiB
  journal/index measurements and the Linux real-HDD
  comparison are still pending. No automatic schedule.
- [2026-11-22 Windows H: results](Docs/Results-2026-11-22/en.md)
  document the observed 8-MiB, 128-MiB and 1-GiB
  results and their measurement caveats. New builds
  record per-phase `gc_gen*_delta` and `gc_pause_ms_delta`,
  plus journal-progress interval timing, allocation, GC
  collection/pause, CPU and memory counters. New per-window
  diagnostics separately time all 128-line `ReadAsync` calls,
  the longest cursor request (with its line and journal offset),
  and **the prior progress log write duration**. This helps
  distinguish cursor stalls from the benchmark logger itself.

[Full experiment plan](Docs/HDD-Benchmark/en.md) ·
[MdView implementation status](../MdView/IMPLEMENTATION_STATUS.md).

## RAM-buffered diagnostics (all modes, Windows and Linux)

**The tested filesystem only hosts actual workload data:** the
synthetic Markdown input, validated source snapshots, temporary
container-event journal and offsets, and optional visual-page index.
These remain on the specified HDD for an honest I/O workload.

**Measurement and reporting are separated:**

- During the timed workload, `BenchLog` serializes diagnostic
  events into bounded managed-memory collections. It performs
  **no JSONL/TXT file writes and no progress console writes**.
  Timestamps remain those of the original events.
- After `session_completed`, the logger creates unique files
  under `ROOT/logs` and writes the collected `*.jsonl` and
  `*.txt` in one export stage. On a handled exception it
  attempts the same export with a `session_failed` event.
- Windows `run.ps1` and Linux `run.sh` collect their
  platform/system report in memory before the benchmark
  and write `system-*.txt` **only after the benchmark process
  exits**, including when the process reports failure.
- Diagnostics are limited to **32 MiB total UTF-8 output**
  across JSONL and TXT. An exceeded quota fails closed
  rather than spilling intermediate diagnostic writes onto
  the test disk. Unlike actual MD datasets, log records
  should stay tiny even on a low-RAM computer.
- A hard kill, power interruption or process crash cannot
  export the in-RAM log. This is an intentional trade-off
  for measurement isolation; normal managed exceptions
  still trigger a best-effort diagnostic export. Also,
  this does not pin RAM pages against OS paging.
- Source creation (`generate` / `prepare`) necessarily writes
  test data; these are not read-only measurements.
  The `measure` mode never regenerates the prepared source.
- The CLI may appear quiet during measurement because
  even per-progress console output is deliberately withheld
  until the measured phase completes.

**Isolation verification:** the CI matrix now runs
`--self-test-log-buffer` on both Ubuntu and Windows.
The command verifies no log directory/files exist while
events are collected, then checks exported JSONL/TXT
contents after explicit persistence. It is a synthetic
logger unit smoke test on the CI runner's temporary
directory, **not a real-HDD throughput test**.

The first 1-GiB run using RAM-only diagnostics
(`mdview-hdd-20261122T143043809Z-ec6979b9`) also
**passed all 200 deterministic seek checks**, with a
48.485-second journal scan, 68.65-MiB peak working set,
and two cursor stalls of 15.273 and 9.708 seconds at
66–68%. There were no progress-log disk writes while
measured work was running; this isolates logging but
does not remove stalls within cursor requests.
See [measured results](Docs/Results-2026-11-22/en.md).

**2-GiB stress stage passed on Windows H:** the paired
2026-11-22 logs `mdview-hdd-20261122T143553385Z-a046cac3`
confirm an unchanged synthetic source and
**1,614,865 source lines**, a **4.677-GiB event journal**,
**50.209-s validated snapshot**, and **123.167-s journal scan**.
All **200 sampled indexed seeks** passed.
Peak process working set was just **68.79 MiB**.
The journaling step is slower than linear scaling
due to multiple large cursor stalls; its raw managed
allocation volume is about 12.21× the source bytes.
See [measured results](Docs/Results-2026-11-22/en.md).

**Next investigation:** isolate the slow path inside
`MdViewContainerPageCursor.ScanUntilAsync` using
optional low-overhead diagnostics: time scanner
advancement, JSON serialization, event-record writes
and offset writes separately, without writing reports
on the measured disk. The implementation currently
makes three asynchronous `FileStream.WriteAsync`
calls per scanned source line (record-length prefix,
record payload, offset entry). The 2-GiB fixture
therefore represents about **4.84 million logical
journal write calls**. This is a performance
optimization candidate, not an established explanation
of the I/O stalls. Batch writes only after
correctness/cancellation/error-recovery tests and
before/after benchmarks.



## Opt-in journal stage tracing (2026-11-22)

After successfully processing 2 GiB on the physical Windows HDD,
the next experiment is an **eight-stage build-and-replay profile** of
`MdViewContainerPageCursor`. This is a diagnostic mode, not
a new journal format and not an optimization yet.

| Windows launcher | Linux launcher | Default |
| --- | --- | --- |
| `-TraceJournalIo` | `--trace-journal-io` | Off |

Example, after `git pull`, on the existing 1-GiB fixture (after a successful 128-MiB smoke run):

```powershell
.\Blackboard\Tools\MdView.Benchmarks\run.cmd measure -SizeMiB 1024 -Seeks 100 -Passes 1 -ProgressStep 1 -TraceJournalIo
```

Each `journal_progress` JSONL event includes the optional
`journal_scan_stages` object. It has per-window summaries for
`ScannerAdvance`, `SerializeEvent`, `EventLengthWrite`,
`EventPayloadWrite`, `OffsetWrite`, `OffsetTableRead`,
`EventWindowRead`, `DeserializeEvent` and `AccountedMs`.
Every stage tracks `Calls`, `TotalMs`, `MaxMs`,
`CallsAtLeast100Ms` and the source line/journal offset
of the longest call. Compare these with
`interval_cursor_read_total_ms`. A large `MaxMs`
at a file-write stage is evidence of delay **during that
await**, not proof of mechanical HDD latency.

The trace adds clock reads per scanned/replayed source row and
can perturb throughput. The first 128-MiB five-stage
smoke test has now **passed** (all 200 seeks correct).
Its three write awaits consumed only **44.75 ms**
of the 4.16-s journal phase. The new readback
metrics were added afterward, so their first
physical-HDD measurements will come from a
new run using the unchanged 1-GiB source. The
JSONL/TXT logger remains buffered in RAM and exports only
after the benchmark. No new progress writes are generated
on the slow disk. See the
[technical design](../MdView/Docs/Journal-Io-Diagnostics/en.md).

## 1-GiB eight-stage follow-up; four synchronous position timers

The successful 2026-11-22 1-GiB physical
HDD run `mdview-hdd-20261122T145716681Z-543ebae1`
reproduced a **23.035-second** stall
at 67–68% source progress (about
1.682–1.707 GB of journal bytes).
All eight existing stage timers totaled
only **75.655 ms** in that window,
none with a 100ms operation. The
previous progress log took only
**0.0238 ms**, and GC pause was
**7.797 ms**. This is evidence of
unmeasured work **inside** cursor
`ReadAsync`.

The observer now includes four more
time counters:
`EventWritePosition`, `OffsetWritePosition`,
`OffsetReadPosition`, `EventReadPosition`.
These surround synchronous FileStream
position changes that occur before the
timed async read/write operations. The
mode stays `-TraceJournalIo` (Windows)
or `--trace-journal-io` (Linux), off
by default. Journal bytes, offset
format, cancellation and bounds are
unchanged.

**Recommended next manual run** on
the unchanged 1-GiB file:

```powershell
git pull
.\Blackboard\Tools\MdView.Benchmarks\run.cmd measure -SizeMiB 1024 -Seeks 100 -Passes 1 -ProgressStep 1 -TraceJournalIo
```

Inspect the four new stage maxima in
`journal_progress.journal_scan_stages`
against the longest cursor request.
A large position-setter time may include
runtime scheduling or filesystem state,
not necessarily actual physical seek
latency or media defects.

### Scoped journal append seek optimization experiment

The 1-GiB twelve-stage H: trace
`mdview-hdd-20261122T150433389Z-44ba10df`
completed correctly and showed **37.10 seconds**
of the 46.51-second journal phase spent in
synchronous `FileStream.Position` setters
for event and offset appends. The new cursor
implementation avoids resetting Position
when the stream is already at the
required append offset.

The regular `-TraceJournalIo` option
now emits two **cumulative** diagnostic
counters inside each `journal_progress`:

- `event_write_position_setter_calls`
- `offset_write_position_setter_calls`

They count actual seek *assignments*,
not pages, disk operations or profiler
samples. Correctness of backward replay
and later forward extension is checked
by a regression test. There is no format
or public API change. The next unchanged
1-GiB physical-HDD run is required to
confirm improvement and verify the
expected low number of actual setters.

### Regression after conditional-seek change and A/B control

Physical Windows H: follow-up
`mdview-hdd-20261122T154929922Z-93528de2`
completed successfully, including all 200
sampled indexed source-line checks. Its
journal took **125.348 s**, compared with
**46.506 s** before the conditional
position-setter change: **2.70× slower**.
Both actual setter counters were **0**
throughout, but `EventPayloadWrite`
became the bottleneck at **115.955 s**
(92.5% of journal time). Working set
remained small (**69.31 MiB** peak).

Do **not** interpret the elimination
of redundant seeks as a measured
speed improvement. To isolate causality
from uncontrolled HDD write-back and
cache conditions, a benchmark-only
`-ForceAppendPositionSetters` switch
restores the original unconditional
per-record `FileStream.Position`
assignments. The default has **not**
been reverted yet: the next action
is controlled A/B/A or B/A/B testing.

```powershell
git pull
# B (conditional): default
.\Blackboard\Tools\MdView.Benchmarks\run.cmd measure -SizeMiB 1024 -Seeks 100 -Passes 1 -ProgressStep 1 -TraceJournalIo
# A (legacy): forced setters for benchmark only
.\Blackboard\Tools\MdView.Benchmarks\run.cmd measure -SizeMiB 1024 -Seeks 100 -Passes 1 -ProgressStep 1 -TraceJournalIo -ForceAppendPositionSetters
# B again: repeat the first command without the switch
```

Each run produces separately named RAM-buffered JSONL/TXT files;
the source is reused rather than regenerated. Compare
`session_start.journal_append_mode`, total
`journal_sequential_pass_1` time,
`EventPayloadWrite`, `EventWritePosition`,
`OffsetWritePosition`, memory peak and
both cumulative setter counts. If the
forced (legacy) mode consistently wins,
we can revert the ineffective change
without altering production journal format.

### 18:00 UTC: legacy write behavior reproduced

The physical H: benchmark-only A/B control
`-ForceAppendPositionSetters` was actually exercised
on the existing 1-GiB source. Its journal finished in
**47.552 s**, versus **125.348 s** with conditional
position assignment and **46.506 s** with the original
unconditional code (the A runs differ by only **2.25%**).
The forced variant performed exactly **807,433**
position assignments on each journal stream,
preserved **200/200** correct indexed checks,
and used **68.39 MiB** peak working set.
The new stage diagnosis shows **32.208 s** in the
event position setter, **5.867 s** in the offset
setter and only **0.110 s** in the event payload
writes, in sharp contrast to **115.955 s** in
payload writes for conditional mode.

**Next:** finish **B–A–B** by running the default
(no `-ForceAppendPositionSetters`) command once
more. The original production conditional change
has **not been reverted** in this commit.
See [2026-11-22 detailed results](Docs/Results-2026-11-22/en.md).
