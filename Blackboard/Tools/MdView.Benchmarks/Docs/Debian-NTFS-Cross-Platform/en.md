# 1-GiB Windows vs Debian: same physical NTFS HDD

**Evidence date:** 2026-11-22. This note records a real Debian 13
benchmark performed after the published `mdview-v0.1.0-preview.1`
CLI prerelease. The benchmark itself is a separate .NET 10
utility, **not** the interactive viewer or the released executable.

## Source artifacts and reproducibility

Provided logs (from the maintainer's physical machine):

- `system-20261122T191104Z-5367.txt` — mounted disk, OS,
  available memory and SDK.
- `mdview-hdd-20261122T191110559Z-1c8198b5.jsonl` —
  109 structured events (100 one-percent journal windows).
- `mdview-hdd-20261122T191110559Z-1c8198b5.txt` —
  human-readable log of the same run.

These files were inspected as actual inputs; the measured numbers
below come from their JSONL records, not estimated console progress.

| Setting | Recorded value |
| --- | --- |
| Target OS | Debian GNU/Linux 13 (trixie), kernel 6.12.117+deb13-amd64 |
| Device and mount | `/dev/sdg2` on `/mnt/test` |
| Physical medium | rotational 74.5 GiB HDD (`ROTA=1`) |
| Filesystem driver view | `fuseblk` mounting an NTFS partition, not native Windows NTFS |
| Benchmark root | `/mnt/test/MDViewTest` |
| Existing fixture | `data/mdview-synthetic-1024MiB.md` (1,073,741,824 bytes) |
| Temp root | `/mnt/test/MDViewTest/tmp` (on same mounted filesystem) |
| Free disk space | approximately 72 GiB before test |
| Host / runtime | Linux x64, 32 logical CPUs; .NET runtime 10.0.15 |
| SDK in system report | `dotnet --version` returned 11.0.100 |
| Parameters | `measure --size-mib 1024 --seeks 100 --passes 1 --progress-step 1 --trace-journal-io` |
| Append strategy | `legacy_unconditional_position_setters` (production A) |

`fuseblk` is consistent with a userspace NTFS/FUSE driver, but
the report does not identify its exact daemon or version; do not
claim `ntfs-3g` was independently verified.
The log explicitly says the first pass is **not proven to
have cold filesystem cache**.

## Direct numeric comparison

Use the nearest preceding physical Windows *A* repeat
(`mdview-hdd-20261122T180043896Z-568a1d79`) as
the Windows reference. Both runs target the same named,
pre-existing 1-GiB fixture on the same physical HDD and
use the normal unconditional append mode; this is a
cross-OS observation, not a statistically controlled
benchmark of Windows vs Linux in general.

| Measurement | Windows A (NTFS native) | Debian A (NTFS/FUSE) |
| --- | ---: | ---: |
| Snapshot elapsed | **24,431.445 ms** | **26,511.893 ms** |
| Snapshot source throughput | 41.914 MiB/s | 38.624 MiB/s |
| Journal elapsed | **47,552.161 ms** | **58,779.744 ms** |
| Journal source throughput | **21.534 MiB/s** | **17.421 MiB/s** |
| Source bytes | 1,073,741,824 | 1,073,741,824 |
| Source lines | 807,433 | 807,433 |
| Event journal bytes | 2,510,275,305 | 2,510,275,305 |
| Journal peak process working set | 71,708,672 B (68.39 MiB) | **88,182,784 B (84.10 MiB)** |
| Journal managed allocations | 13,113,915,552 B | **12,828,807,712 B** |
| Journal GC pause | 763.672 ms | **728.412 ms** |
| Correct indexed source-line checks | 200/200 | **200/200** |
| First/repeat sampled seek median | 0.0609 / 0.0570 ms | **0.0326 / 0.0300 ms** |
| Actual append Position assignments (event / offset) | 807,433 / 807,433 | **807,433 / 807,433** |

The Debian journal took **11,227.583 ms longer
(+23.61% elapsed)** and delivered **19.10% less
source MiB/s** in this one run; the snapshot took
**8.52% longer**. No source-line corruption or
journal size difference was detected. The low median
random-seek times are **warm/page-cache measurements**:
Linux `/proc/self/io` reports `ReadBytes=0`
in each 100-sample seek stage. They must **not** be
described as cold mechanical head-seek latency.

## Where the 58.78 seconds went

All twelve timed journal stages were aggregated from the
100 progress records:

| Stage | Windows A aggregate ms | Debian A aggregate ms |
| --- | ---: | ---: |
| `EventWritePosition` | 32,208.397 | **28,429.150** |
| `OffsetWritePosition` | 5,867.457 | **17,213.375** |
| `EventWindowRead` | 562.186 | **3,888.472** |
| `ScannerAdvance` | 3,681.917 | **3,816.634** |
| `DeserializeEvent` | 1,996.896 | **2,088.647** |
| `SerializeEvent` | 1,836.631 | **1,569.332** |
| `EventPayloadWrite` | 110.480 | **102.912** |
| `EventReadPosition` | not restated here | 244.760 |
| `OffsetReadPosition` | not restated here | 134.830 |
| `OffsetTableRead` | not restated here | 108.377 |
| `EventLengthWrite` | not restated here | 81.308 |
| `OffsetWrite` | not restated here | 63.415 |
| **All 12 stages** | 46,663.147 | **57,741.212** |

Both Linux write-position timers together:
**45,642.525 ms (77.65% of journal elapsed)**,
versus Windows A **38,075.854 ms**. In contrast
to Windows, Linux spends much longer in
`OffsetWritePosition` and `EventWindowRead`,
even though `EventWritePosition` is faster.
These are stopwatch totals around application
operations, not direct kernel syscall timing.

Unlike either measured Windows A run, Debian A
showed **no 20–25-second stall near journal 70–73%**.
The 100 per-percent progress intervals have median
**581.237 ms**, maximum **889.330 ms** at the
first percent, and standard deviation about
**44.364 ms**. Specifically 69–74% took:
**596.160, 593.144, 573.266, 586.152,
593.222, 577.025 ms**. After startup,
progress was comparatively even.

The strongest interpretation is **stable but
slower application-level I/O on Linux with
NTFS/FUSE**, versus **faster yet intermittently
stalled I/O in Windows native NTFS**. The
underlying cause has **not been isolated**:
filesystem-driver choice, cache/writeback,
FUSE scheduling, CPU frequencies, background
load and disk state could all affect this
difference. No evidence here proves damaged
sectors or that Linux in general is slower.

## Safeguards and next steps

- The session ended `result=success`; all
  200 sampled source-line matches were correct.
- `TMPDIR` and journals were on the **same
  dedicated HDD**, not on the Debian system NVMe.
- The benchmark did not re-generate the source,
  reformat, remount or clear OS caches.
- **Keep the measured-fast unconditional
  append default (A)** in production. This
  single Linux run is not a reason to reintroduce
  conditional mode B or change journal format.
- The next practical validation should be
  **interactive acceptance testing of the
  actual Linux preview binary**, including
  menus, paging, resizing and SSH/TTY behavior.
- If investigating driver-specific performance
  later, first identify the FUSE driver and
  use an explicitly controlled alternative
  filesystem/mount experiment. Never change
  live mounts automatically or conclude
  that `ntfs3` would necessarily be faster.
