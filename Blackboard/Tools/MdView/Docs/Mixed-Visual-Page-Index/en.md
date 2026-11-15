# Disk-backed mixed Markdown visual page index

## What this milestone adds

`MdViewMixedVisualPageIndex` builds a **document-wide disk page index**
using the already established `MdViewMixedVisualPagePlanner`. It indexes
both ordinary source-aware Markdown rows and complete small native
`Spectre.Console.Table` widgets under the **same** deterministic visual
row budget.

`MdViewMixedVisualIndexedRenderer` reconstructs any selected canonical
page from its indexed source position. It compares the new result with the
disk record **before writing any terminal output**, then delegates to
`MdViewMixedVisualPageRenderer` for the real display.

This is a standalone implementation stage. **The current default
`mdview preview PATH` command still uses the earlier compact
`MdViewTerminalPageIndex` and `MdViewTerminalPageRenderer`.** The
experimental mixed index/renderer is not yet exposed as a CLI choice.

## Fixed-size disk records

Each indexed document page is represented by a **56-byte** record:

| Field | Size | Meaning |
| --- | ---: | --- |
| Start.Line / Start.WrapRow | 8 + 4 bytes | Physical source position at page start |
| Start.CompactHeaderLine / Start.CompactColumns | 8 + 4 bytes | Active compact-table continuation |
| Next.Line / Next.WrapRow | 8 + 4 bytes | First unconsumed source position |
| Next.CompactHeaderLine / Next.CompactColumns | 8 + 4 bytes | Continuation state carried to next page |
| VisualRows | 4 bytes | Total source-aware text/native widget height |
| Flags | 4 bytes | End-of-document and clipped-code indicators |

Fields are little-endian; page records are sequential and fixed-size.
The entire index remains in a temporary file opened with
`FileOptions.DeleteOnClose`, not an array of page boundaries in RAM.

The default disk budget is **64 MiB** and may be restricted for testing
or low-storage environments. Index creation fails with
`InvalidDataException` if the requested storage limit is exhausted.
The partially written temporary file is disposed; the validated
source document is not modified.

## Whole-document indexing

`CreateAsync(cursor, console, contentRows)` starts at
`MdViewMixedVisualPosition.Beginning` and calls the same bounded page
planner repeatedly. Each page retains exact source line, within-line
soft-wrap row and optional compact-table state.

- Ordinary Markdown uses the existing styled line layout, including
  quotes, lists, links, headings and clipped code.
- **Complete small native tables** (at most six source data rows)
  participate in the measured visual height.
- If a whole native table fits an empty page but not the remaining
  space, the page ends **before** the table header.
- Long tables, narrow grids, or over-height native tables stay
  entirely compact across page boundaries.
- A page ending exactly at EOF is marked final. Even an empty source
  gets **one empty indexed page**, not zero or an extra phantom page.
- Index construction checks progress and terminal geometry/capabilities
  so an inconsistent layout cannot silently produce incorrect page
  boundaries.

The planner reads through the caller-owned validated
`MdViewContainerPageCursor`, backed by temporary source journals.
No complete table or entire Markdown source is loaded into memory
by the mixed page index itself.

## Random page reads and source-line jumps

`GetPageAsync(pageNumber)` performs one fixed-size disk seek and reads
one record. Each record includes both the page start and its exact
`Next` source position. A renderer therefore does not need to replay
all earlier Markdown pages.

`FindPageAsync(sourceLine)` locates the **canonical page containing the
start of that physical source line**. Its overload accepting
`MdViewVisualPosition(line, wrapRow)` can also locate a particular
wrapped visual row. Both perform binary search over the disk records,
not a linear scan or a full in-memory index.

Source lines inside a complete native table resolve to the page
holding its native widget. A compact table spanning pages retains
its header/column state across page records. Direct noncanonical
custom-start rendering with reconstructed table context is a separate
future concern; callers should jump through `FindPageAsync` and
then render the corresponding canonical page.

## Index-to-renderer parity and safety

`MdViewMixedVisualIndexedRenderer.RenderPageAsync` requires:

1. The **same source cursor instance** used to build the index.
2. Unchanged terminal width, height and Unicode capability.
3. A valid page record read from the owned disk index.
4. A newly planned page matching the record's complete `Start`,
   `Next`, row count, EOF flag and clipped-code flag.
5. The usual `MdViewMixedVisualPageRenderer` block preflight,
   including exact Spectre-rendered native table height.

All preflight checks run **before any output**. An unsuitable
viewport or substituted cursor raises an error rather than
displaying mismatched content. The renderer returns the verified
page record after writing the selected canonical page.

The index is temporary and disposed deterministically. It does not
own the source cursor or snapshot; the caller must keep those alive
until indexing and rendering finish.

## Tests and current limitations

Eleven `MdViewMixedVisualPageIndexTests` cover:

- Complete small native table among regular Markdown paragraphs.
- Full-document compact fallback spanning many source pages.
- Binary-search jumps within one long wrapped physical source line.
- Header/delimiter continuation between compact visual pages.
- Two independently recognized small native tables in one document.
- Empty document and exactly filled final page without a phantom.
- Rejected terminal resize and substituted source cursor before output.
- Configurable disk-budget exhaustion with unchanged source bytes.
- Invalid page numbers, positions, arguments and disposed index.
- Cancellation before index creation or rendering.
- Real indexed page reconstruction and native widget display.

**These newly authored tests have not been executed with .NET in the
assistant environment.** The previous mixed-page planner checkpoint
was maintainer-confirmed green; it does not imply that this new index
has passed its tests.

Run after pulling the repository:

```bash
dotnet test Blackboard/Tools/MdView.Tests/MdView.Tests.csproj
dotnet test Toolbox.sln
```

Next: optionally expose the new mixed index in `mdview preview`
behind an explicit opt-in command or flag, retaining the existing
compact default. The UI integration must preserve status/footer
space, source-line jumps, page navigation and resizing; large-file
and low-memory behavior must be measured rather than assumed.

See [mixed visual page planning](../Mixed-Visual-Page-Planning/en.md),
[native table fragment index](../Native-Table-Fragment-Index/en.md),
and [implementation status](../../IMPLEMENTATION_STATUS.md).

## Opt-in terminal UI now available (2026-11-15)

The standalone mixed index now has a separate interactive caller:
`MdViewMixedPreviewPager`, available as `mdview preview-native PATH`.
It uses this disk page index and `MdViewMixedVisualIndexedRenderer`
for canonical Next/Previous pages, one-based source-line jumps
and lazy heading/link source selection. A terminal resize disposes
and rebuilds the mixed index, reanchored to the original source
line. The existing compact `mdview preview PATH` command remains
unchanged. This new UI is awaiting fresh maintainer tests.

[Usage, navigation and viewport safeguards](../Mixed-Native-Preview/en.md).

## Optional width metadata for mixed page indexes (2026-11-15)

`MdViewMixedVisualPageIndex.CreateAsync` now accepts
`maximumContentColumns` (zero for automatic). Its
`MaximumContentColumns` property retains this value as
**in-memory index metadata**, not an extra disk record
field: the existing 56-byte page format remains unchanged.
The indexed renderer replans each requested page using
the original cap, then checks its source and measured
visual geometry against the disk record before output.

Physical terminal width, height and Unicode capabilities
remain mandatory resize checks. A width cap never
overrides the physical terminal profile. The original
compact page index remains unchanged.

[Optional width guide](../Mixed-Content-Width/en.md).

## Native continuation records for opt-in long tables (2026-11-15)

The optional `LongNativeTables` index metadata now selects
source-complete native fragment pagination for long
Markdown pipe tables. The 56-byte on-disk schema remains
unchanged: the existing `CompactHeaderLine` field stores
the original table heading and `CompactColumns = -1`
distinguishes **native continuation** from a compact
positive column count. `WrapRow = 0` and `Line`
indicates the first unconsumed physical data line.

The disk index validates this sentinel only in the
opt-in mode. `MdViewMixedVisualIndexedRenderer`
reconstructs each native continuation page using the
same selected flag, validates its measured row count
and exact `Start`/`Next` source state before writing.

The header and delimiter source lines map to the
first native fragment; later data rows map to their
own canonical continuation pages. Repeated headers
are visual only and do not create extra source lines.
[Long native table contract](../Long-Native-Tables/en.md).
