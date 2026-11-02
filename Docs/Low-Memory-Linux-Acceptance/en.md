# Minimal Debian / 1 GiB RAM + rotational disk Markdown acceptance (planned field test)

**Status:** protocol updated on 2026-11-02; **not yet run on the proposed
1-vCPU / 1-GiB Debian virtual machine with rotational-disk backing**. This is an acceptance procedure,
not benchmark evidence. Keep all measurements, versions and exceptions.

## Suggested virtual machine

- Debian 13 minimal installation, terminal-only, no desktop environment.
- **One virtual CPU and 1 GiB of guest RAM** for the first baseline.
  Increase memory to 2 GiB only if the initial configuration cannot
  complete the chosen workload; keep all other settings unchanged.
- A virtual disk backed by an actual **rotational HDD** where possible,
  with enough free space for sources and temporary indexes. Preserve
  the rotational backing across all RAM comparisons.
- Disk type matters: a guest disk merely advertising `ROTA=1` does
  **not** establish physical HDD latency when the host stores its
  virtual-disk image on SSD. Record host disk backing; if physical HDD
  is unavailable, record any explicit host IOPS/latency throttling
  as a *simulation*, not a physical HDD test.
- Prefer a **.NET 10 runtime-only guest** and a Release build produced
  separately on a more capable compatible Linux machine. Installing
  the SDK and building in the small guest is a different, much heavier
  workload; do not mix it with the reader runtime baseline.
- `git`, GNU `/usr/bin/time` (Debian `time` package), and basic tools.
  `vmstat` (procps) and optional `iostat` (sysstat) help observe
  RAM, swap, and disk performance.
- Prefer a snapshot before running the tests. Preserve swap settings
  during comparable runs; record swap-in/out to identify cases where
  slow disk paging masks a memory bottleneck.

Record `uname -a`, `free -h`, `dotnet --info`, available disk space
(`df -h`), `lsblk -o NAME,TYPE,ROTA,SIZE,MODEL` and the host's real
storage backing before the first run. In the guest, `ROTA` is only
a device hint, not proof of physical HDD storage.

Before changing RAM, record the exact VM configuration, attached
virtual-disk controller, disk cache policy and swap configuration.
**Change one variable at a time**: 1 GiB -> 2 GiB RAM only if needed,
same disk, guest OS, CPU count, seed, source and application revision.

## Obtain the exact review branch

Run these commands in a normal user shell, not as root:

```bash
git clone --branch feature/markdown-streaming-stress-20261102 \
  https://github.com/afrowaveltd/Toolbox.git
cd Toolbox
git rev-parse HEAD
dotnet --info
free -h
df -h
```

The branch is intentionally still in draft PR #3 and can move. Record
the resolved commit SHA with every test report to identify the exact code.

## Build separately, then measure only execution

Compilation, NuGet restore and test hosts may use much more memory than
the standalone reader. Ideally, publish a Linux x64 Release build on a
more capable machine and copy the resulting output directory to the VM:

```bash
# On a stronger compatible build machine, from Toolbox root:
dotnet publish -c Release \
  Toolroom/Markdown/Stress/Afrowave.Toolbox.Toolroom.Markdown.Stress.csproj \
  -r linux-x64 --self-contained false -o ./artifacts/markdown-stress-linux-x64
```

Copy the entire publish directory to the guest (for example via SSH).
Install the compatible .NET 10 runtime on the guest and verify with
`dotnet --info`. Use the equivalent guest path below:

```bash
# On the 1-GiB Debian guest:
vmstat 1
# In a separate terminal, from the copied publish directory:
/usr/bin/time -v dotnet \
  ./Afrowave.Toolbox.Toolroom.Markdown.Stress.dll \
  --size 10M --seed 42 --max-heap-mib 128
```

`vmstat 1` is a foreground monitoring command: keep it in a separate
SSH session and stop it after the measured run. If there is no other
build machine, it is acceptable to build on the guest with the
.NET 10 SDK, but **measure the compiled Release app separately** and
report SDK/restore RAM as a separate test. This first phase exercises
virtual stream classification and safe rendering, not disk paging.

If the first run is successful, try a 100 MiB virtual Markdown stream:

```bash
/usr/bin/time -v dotnet \
  ./Afrowave.Toolbox.Toolroom.Markdown.Stress.dll \
  --size 100M --seed 42 --max-heap-mib 128
```

The scanner generates data on demand and discards rendered output;
**100 MiB is not a file kept in RAM**. A successful result verifies
virtual streaming, not the complete disk-indexed reader or exact
CommonMark output. The runner's heap sampling limit is a guard on
*growth* only, not a hard process RSS cap. GNU `time -v` reports
maximum resident set size observed by the operating system; its
units on Linux are kilobytes.

Only after both complete and metrics are reviewed, opt into `--size 1G`
and later `--size 10G --no-render`, as time and storage allow.
These options have not yet been accepted on the low-memory VM.

If a workload fails, determine whether it was a managed heap budget
failure, an OS out-of-memory kill, severe swapping, storage exhaustion,
or simply a slow HDD. **Keep the HDD and CPU configuration unchanged**
when repeating at 2 GiB. Compare identical command lines and seeds.
No unmeasured 1-GiB success claim should be made.

## Smaller regression tests

```bash
dotnet test -c Release \
  ExchangeHouse/Markdown.Tests/Afrowave.Toolbox.ExchangeHouse.Markdown.Tests.csproj
dotnet test -c Release \
  Blackboard/SpectreConsole/Markdown.Tests/Afrowave.Toolbox.Blackboard.SpectreConsole.Markdown.Tests.csproj
```

The test runner and SDK are deliberately excluded from reader-runtime
RSS comparisons. If terminal tests time out or are killed, record the
exit code, which test failed, swap usage, and whether the kernel
reported an out-of-memory kill.

## Rotational-disk metrics and end-to-end pager follow-up (not yet covered)

The guest should record `vmstat 1` swap-in/swap-out (`si`/`so`),
block I/O and CPU wait (`wa`) during runtime. Where available, capture
`iostat -xz 1` for queue, utilization and per-request latency; be
careful that virtual storage metrics may not expose the underlying
physical HDD's queue. The interactive pager will be especially
sensitive to seek latency when reconstructing sparse checkpoints
or binary-searching the optional list-group index.

For a later physically stored Markdown corpus, compare repeated
backward/middle/end page seeks using identical sources and the same
guest configuration. Evaluate checkpoint strides **1, 4 and 16**:
smaller stride uses more temporary index bytes but can reduce replay
I/O on a rotational disk. This parameter comparison is **planned**;
the current virtual stress runner does not measure it.

## End-to-end pager follow-up (not yet covered)

The current virtual stress harness does not create large physical
Markdown files and does not drive the local disk pager. The future
interactive acceptance must separately check: open a local UTF-8 file;
build the raw byte-offset and optional sparse 512-byte context indexes;
jump directly to the middle/end; page backwards and forwards; toggle
literal/conservative preview; check correct fence and list ownership;
and verify deletion of all temporary index files after exit.

The optional `MarkdownListSpacingIndex` adds a separate bounded
sequential summary pass and two small disk files. It is **not wired to
the sixth Showcase menu yet**. Do not claim that the virtual stress
harness benchmarks this optional component.

## Acceptance and reporting

Record each tested commit and command, exit code, wall time, maximum
RSS from `time`, runner-reported sampled managed heap, free RAM/swap,
Linux kernel, .NET runtime, and any error log. Compare repeated
identical runs rather than relying on one datapoint.

The engineering goal is to establish whether the selected reader
workloads remain usable on a **1-GiB / 1-vCPU / physically rotational
HDD** Debian VM; if not, identify a measured minimum RAM setting
while keeping the HDD unchanged. A successful test will be documented
as a *specific measured workload*. Do not generalize that every
supported Markdown page or giant document runs within 1 GiB, and do
not equate a virtual streaming pass with end-to-end HDD acceptance.
