# Markdown disk-paging resource profiles

**Status:** experimental, on draft PR #3. The presets are explicitly
selected and are not automatically detected or performance-certified.

| Profile | Source lines per page | Checkpoint interval | Tradeoff |
| --- | ---: | ---: | --- |
| Balanced (default) | 40 | 16 pages | Existing behavior |
| HDD | 64 | 1 page | Larger temporary checkpoint index; avoid replay of earlier pages on seeks |
| SSD | 64 | 32 pages | Less checkpoint disk storage; more replay I/O on seeks |
| LowRAM | 8 | 16 pages | Smaller decoded page buffer; more page/index operations |

Each profile keeps the maximum physical line at 64 KiB of UTF-8, an
independent 256 MiB source byte-offset index quota, and a 256 MiB
structural checkpoint quota. The source index stores 8 bytes/page;
saved structural checkpoints store 512 bytes each. A profile **does
not** enable the optional third-pass list spacing index.

These settings are **not process memory limits**. A source page may
need additional decoded strings, line events, renderer allocations and
runtime memory. The HDD preset does not yet introduce a 50–100 MiB
in-memory cache; it reduces checkpoint replay by saving more
checkpoints. Measure cache effectiveness separately before introducing it.

## C# usage

```csharp
using Afrowave.Toolbox.ExchangeHouse.Markdown;

var settings = MarkdownPagingProfiles.For(MarkdownPagingProfile.Hdd);
settings = settings with { CheckpointStride = 4 }; // optional override
settings.Validate();

await using var pages = await MarkdownDiskPageIndex.BuildAsync(
    "/path/to/README.md", settings.CreateSourceOptions());
await using var context = await MarkdownFenceCheckpointIndex.BuildAsync(
    pages, maxIndexBytes: settings.MaxCheckpointIndexBytes,
    checkpointStride: settings.CheckpointStride);
var firstPage = await context.ReadPageAsync(0);
```

`MarkdownPagingSettings` is a record with overridable
`LinesPerPage`, `CheckpointStride`, `MaxLineBytes`,
`MaxSourceIndexBytes` and `MaxCheckpointIndexBytes`. Validation
enforces source-page limits and checkpoint stride 1..256. The
`Profile` field continues to label the original preset even
if settings are individually customized.

## Console integration

The sixth Blackboard Showcase demo prompts for Balanced/HDD/SSD/LowRAM
after picking a local Markdown file, *before* creating either index.
It prints the settings selected; Escape cancels without indexing.
Changing profiles requires rebuilding indexes. The literal source
remains the default display, with optional conservative preview.

An HDD VM device flag such as `ROTA=1` is not proof that the host
actually stores the virtual-disk image on a rotating disk. Profile
selection is deliberate, not a storage auto-detection claim.

## Planned 1 GiB / rotational-HDD VM acceptance

On Debian minimal with one vCPU and 1 GiB RAM, compare Balanced and HDD
first using the **same file, revision, navigation sequence and disk
backing**. Then compare SSD/LowRAM and optional stride 1/4/16.
Record time, maximum RSS, swap, random I/O, and disk wait. If 1 GiB
is insufficient, increase only RAM to 2 GiB; keep the same HDD.
No real-HDD VM benchmark has yet been completed.

[Source paging](../Disk-Backed-Paging/en.md) ·
[Sparse checkpoints](../Fence-Checkpoints/en.md) ·
[Showcase viewer](../../../../Blackboard/Samples/Blackboard.Showcase/Docs/Markdown-Paged-Source-Viewer/en.md) ·
[Debian acceptance protocol](../../../../Docs/Low-Memory-Linux-Acceptance/en.md).
