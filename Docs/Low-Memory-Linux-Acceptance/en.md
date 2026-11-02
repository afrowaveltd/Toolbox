# Minimal Debian / 2 GiB RAM Markdown acceptance (planned field test)

**Status:** protocol prepared on 2026-11-02; **not yet run on the proposed
1-vCPU / 2-GiB Debian virtual machine**. This is an acceptance procedure,
not benchmark evidence. Keep all measurements, versions and exceptions.

## Suggested virtual machine

- Debian 13 minimal installation, terminal-only, no desktop environment.
- One virtual CPU, 2 GiB assigned memory, a regular virtual disk with
  enough free space for repository, NuGet packages and temporary indexes.
- .NET 10 SDK + runtime for source builds. The user should install these
  from the configured approved .NET package source; check `dotnet --info`.
- `git`, `/usr/bin/time` (Debian `time` package), and basic tools.
- Prefer a snapshot before running the tests. Do not disable swap or
  modify production system settings merely to improve benchmark numbers.

Record `uname -a`, `free -h`, `dotnet --info` and available disk space
(`df -h`) before the first run. Do not confuse target .NET version with
a demand for a desktop GUI.

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
the standalone reader. Measure reader runtime separately:

```bash
dotnet build -c Release \
  Toolroom/Markdown/Stress/Afrowave.Toolbox.Toolroom.Markdown.Stress.csproj

/usr/bin/time -v dotnet \
  Toolroom/Markdown/Stress/bin/Release/net10.0/Afrowave.Toolbox.Toolroom.Markdown.Stress.dll \
  --size 10M --seed 42 --max-heap-mib 128
```

If the first run is successful, try a 100 MiB virtual Markdown stream:

```bash
/usr/bin/time -v dotnet \
  Toolroom/Markdown/Stress/bin/Release/net10.0/Afrowave.Toolbox.Toolroom.Markdown.Stress.dll \
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

The engineering goal is steady low-memory operation on a 2-GiB
terminal-only VM, with a 1-vCPU throughput tradeoff. A successful
test will be documented as a *specific measured workload*; do not
generalize that every supported Markdown page or giant document can
run under a fixed 128-MiB or 2-GiB ceiling.
