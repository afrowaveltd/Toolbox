# 5400 RPM HDD experiment protocol

## Experimental goal

Measure how the actual Afrowave MdView streaming / disk-index
pipeline behaves on a deliberately slow, separately mounted
rotational drive such as an **HGST 80 GB, 5400 RPM** device.
The maintainer will first execute it on Windows using the separate
H: test disk, then on Debian 13 once its exact mount point is supplied.
This document records the current design so that future changes
can be compared against the **same dataset and parameters**.

The test has three distinct storage workloads:

1. **Generate:** stream a synthetic Markdown dataset to the HDD.
2. **Measure:** build a validated source snapshot, classify the
   document into a temporary disk-backed event journal, then
   perform deterministic physical-source-line seeks (first
   sequence and immediate repeat).
3. **Index (optional):** measure the complete mixed Markdown/native
   Spectre visual-page index and sample canonical page lookups.

Run each independently. The default `all` mode combines
`generate` and `measure` but deliberately excludes `index`
because complete page measurement can be extremely slow for
a 2-GiB source.

## Exact filesystem layout

Linux (replace the placeholder with the **verified mount point**,
not a presumed default):

- Root: `/your/mounted/disk`
- Source: `/your/mounted/disk/data/mdview-synthetic-2048MiB.md`
- TEMP / snapshots / indexes / journals: `/your/mounted/disk/tmp`
- Machine-readable and text logs: `/your/mounted/disk/logs`

The Linux launcher invokes `findmnt` to refuse the OS root volume,
ensures all three directories resolve within the selected mount,
checks a disk-space reserve, and exports
`TMPDIR=/your/mounted/disk/tmp` *before starting .NET*. The C# runner
also checks that `Path.GetTempPath()` equals the selected
temporary directory. This protects against the common
measurement error of reading Markdown from the HDD but
silently writing snapshots and journals to an SSD.

Windows uses `run.ps1` (or the `run.cmd` wrapper) instead:
the default root is `H:\MDViewTest`. Before creating any files,
PowerShell verifies that H: is a local volume on a **different
physical disk** from the Windows system volume. Reparse-point
directories, insufficient space, disk roots, readonly/offline
volumes and absent/unrecognized disks are refused. The script sets
`TEMP`, `TMP`, `TMPDIR` and the free-space variable for the
child .NET process. It prepares the system report in RAM
and writes `H:\MDViewTest\logs\system-*.txt` with
filesystem/disk details **after** the measured process exits.
Neither platform launcher formats
disks or clears operating-system caches.

Do not run this test by copying just the compiled DLL or by
manually setting TEMP **after** the process starts.

## Fixture structure and allocation

The generator is deterministic and bounded. Its repeated
~70-KiB source block contains:

- Markdown headings and ordinary prose;
- multiple long ~4-KiB lines to exercise wrapping;
- inline HTTPS links;
- a top-level three-column pipe table with 24 data rows;
- a fenced C# code block;
- diacritics and non-ASCII Unicode;
- an empty line separating successive repeated sections.

It writes **2048 MiB = 2,147,483,648 bytes** exactly, using
repeated UTF-8 byte blocks and an ASCII-only final tail.
The explicit `prepare` mode builds four independently reusable
files: 8 MiB (smoke), 128 MiB (medium), 1024 MiB (large) and
2048 MiB (stress), totalling **3208 MiB**. A single size remains
selectable with `generate --size-mib N` on Linux or
`-Mode generate -SizeMiB N` on Windows. Preparing fixtures does
not run expensive index or seek measurements.

The generator never holds more than one short repeated block
in memory; it uses `FileMode.CreateNew` to prevent accidental
truncation. After creation the source remains on the HDD
for identical future re-runs.

The default MdView validated snapshot allows 16 GiB of source
data, but its **production** cursor journal/offset defaults
(512 MiB and 256 MiB) are intentionally small. This test
constructs its own cursor with **8 GiB event journal and
512 MiB offset budgets**. Only the manual benchmark changes
those limits. Production defaults and public APIs remain
unchanged. The test's 2-GiB source is suitable for its
default 1-MiB maximum physical-line limit.

The launcher refuses the run unless the free disk capacity
is at least **six times the requested source size plus
one GiB**. This is a conservative early guard, not a promise
that actual journal sizes are fixed.

## Logging protocol

Every execution creates three files; no existing logs are
overwritten:

1. A JSON Lines file `mdview-hdd-*.jsonl` with UTC events,
   arguments, source and snapshot dimensions, per-phase
   Stopwatch durations, logical throughput, GC allocation
   deltas, managed heap and process peak working set,
   process CPU time, journal bytes, cursor read-request
   count, event-stream request count, and Linux
   `/proc/self/io` deltas.
2. `mdview-hdd-*.txt` with human-readable equivalent
   events and explicit error information if a stage fails.
3. `system-*.txt`: on Linux `findmnt`, `lsblk`
   rotational indicator, `df`, free memory, kernel/OS and .NET;
   on Windows Get-Partition/Get-Disk/Get-Volume details and .NET.

The seek log preserves each requested physical source line
and measured duration. Aggregate latency includes minimum,
median, p95 and p99. A fixed seed creates the **same
selection of physical lines** within both random-seek passes.
Repeat benchmarks with the same source size and seed.

## Procedure

On **Windows** (PowerShell, from repository root), first confirm
H: is the intended empty/non-system test disk, then run:

```powershell
powershell -NoProfile -ExecutionPolicy Bypass -File .\Blackboard\Tools\MdView.Benchmarks\run.ps1 -Mode all -SizeMiB 8 -Seeks 10
powershell -NoProfile -ExecutionPolicy Bypass -File .\Blackboard\Tools\MdView.Benchmarks\run.ps1 -Mode prepare
powershell -NoProfile -ExecutionPolicy Bypass -File .\Blackboard\Tools\MdView.Benchmarks\run.ps1 -Mode measure -SizeMiB 2048 -Seeks 250 -Passes 2
```

The prepare command is a separate explicit request that writes
approximately 3.13 GiB of persistent fixture data; do not
automate or put it in standard test suites.

On **Linux**, substitute the actual mounted filesystem for
`/your/mounted/disk`. Start small and confirm routing:

```bash
git pull
bash Blackboard/Tools/MdView.Benchmarks/run.sh /your/mounted/disk all --size-mib 8 --seeks 10
```

Then run the full source/journal experiment:

```bash
bash Blackboard/Tools/MdView.Benchmarks/run.sh /your/mounted/disk all --size-mib 2048 --seeks 250
```

Later, the exact same source is reused automatically:

```bash
bash Blackboard/Tools/MdView.Benchmarks/run.sh /your/mounted/disk measure --size-mib 2048 --seeks 250 --passes 2
```

Optional comprehensive native-table geometry experiment:

```bash
bash Blackboard/Tools/MdView.Benchmarks/run.sh /your/mounted/disk index --size-mib 2048 --rows 48 --width 120 --max-width 100 --long-tables
```

Do not run the full index as part of the first smoke check.
It creates a complete disk page index and may be significantly
more expensive than the sequential journal scan.

After each execution, save/share the **JSONL, TXT and system
log** together. Do not need to upload the 2-GiB source.

## Interpretation

Do not label the first observed run `cold cache` unless
the operating-system cache was genuinely reset using
an administrator-controlled, repeatable method.
The benchmark itself never runs `drop_caches`,
reboots, remounts or calls `hdparm`; these would
risk changing global machine state and confuse
the reproducibility of the measurement.

A fresh generator immediately before scanning will likely
leave some data in the OS page cache. Therefore interpret
the measurements as **first pass vs repeated pass under
the reported environment**, not automatically as physical
HDD vs RAM.

Also distinguish:

- **`ReadRequestCount`:** source cursor API calls;
- **`EventReadRequestCount`:** calls to read contiguous
  event-journal windows;
- **`syscr`/`syscw`:** Linux process read/write syscalls;
- **`read_bytes`/`write_bytes`:** storage-accounted process
  I/O on Linux (cache-dependent and not necessarily exclusively
  this HDD's platter operations); these `/proc/self/io` values
  are unavailable on Windows, where the harness reports zero
  placeholders instead of fabricating device I/O statistics;
- **Elapsed time / p95 latency:** directly measured duration;
- **CPU time / GC allocated / working set:** compute and
  memory characteristics.

Compare like with like: mount type/options, source file,
terminal geometry, runtime, disk state, sample count and
test mode should match. An improvement in source API
calls is useful but not proof of a similar improvement
in end-to-end throughput.

## Safety and implementation status

The benchmark is a **separate non-packable tool**, not a
release feature and not part of `Toolbox.sln` by design.
It uses the existing production MdView classes but writes
test datasets, temporary snapshots/indexes and diagnostics only
inside the selected isolated test volume. It never
formats drives or deletes user data, and it does not
replace the 56-byte visual-page index format.

The Windows launcher and complete fixture set were added on
2026-11-22. **Neither real-HDD execution nor .NET compilation
has been verified in the assistant's environment.** The
maintainer must validate the initial 8-MiB smoke test on each
operating system before trusting the 2-GiB benchmark. Any compilation or runtime log will be used
to correct the harness and commit changes before interpreting
performance measurements.

[Benchmark README](../../README.md) ·
[MdView implementation status](../../../MdView/IMPLEMENTATION_STATUS.md).

## Keep diagnostics off the HDD during measured work

From 2026-11-22 the benchmark uses **managed-RAM
buffering** for all JSONL/TXT events, including progress
and seek records, and emits them to the verified
`ROOT/logs` directory only **after** measured work
finishes. The Windows and Linux launchers also defer
the `system-*.txt` diagnostic output until after the
child benchmark process finishes. The temporary root,
source snapshot, event journal, offset table and visual
page index still use the *target disk*; **those disk
operations are the point of the benchmark**.

This isolates diagnostic I/O but not managed serialization
or allocation overhead. The event-buffer quota is
32 MiB of combined UTF-8 log lines, chosen to remain
usable on older/low-RAM hosts despite the maintainer's
128-GB development PC. If a managed exception occurs,
the harness tries to persist partial logs after stopping.
A forced kill may lose all in-memory diagnostics.

CI includes the safe `--self-test-log-buffer` test on
Windows and Linux after compilation. It checks that
report files do not appear before export, then validates
both file formats after export. It does not execute any
benchmark or physical disk guards in hosted CI.
