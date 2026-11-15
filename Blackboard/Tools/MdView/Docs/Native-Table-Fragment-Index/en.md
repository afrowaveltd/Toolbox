# Native table fragment index — disk-backed measured page geometry

## Status and scope

This milestone adds a **table-only**, disk-backed index for bounded native
Spectre.Console table fragments. It complements the existing
`MdViewSpectreTableBlockReader` and `MdViewNativeTablePagePlanner`.

**It is not yet integrated into the main `mdview preview PATH` page
renderer.** Ordinary prose, list, quote and compact pipe-table display,
source jumps and the existing `Page X of Y` index remain unchanged.
The independent `View table (Spectre)` modal also retains its current
behavior.

The goal is to establish a verifiable source-to-native-fragment contract
before introducing mixed document blocks and measuring the remaining
content height on a partially filled Markdown page.

## Index creation

`MdViewNativeTableFragmentIndex.CreateAsync` accepts:

- A caller-owned `MdViewContainerPageCursor` over a validated snapshot.
- A source-bound `MdViewSpectreTableHeader`. The reader checks its
  original source header, labels and separators again before indexing.
- The actual `IAnsiConsole` profile for native Spectre measurement.
- A fixed `contentRows` budget (1–128) for **each** fragment.
- A maximum disk budget (64 MiB by default).

The builder repeatedly invokes the existing measured
`MdViewNativeTablePagePlanner`. Each fragment contains at most six
source data rows and repeats the native column header. Only the actual
Spectre segment-rendered height, source data interval, and next-line
flag are written to the index.

Each on-disk record is **28 bytes**:

| Field | Type | Size |
| --- | --- | ---: |
| FirstDataLine | Int64 | 8 bytes |
| NextDataLine | Int64 | 8 bytes |
| DataRows | Int32 | 4 bytes |
| VisualRows | Int32 | 4 bytes |
| HasMore (0 / 1) | Int32 | 4 bytes |

The fixed content-row budget must not exceed the physical console height.
The builder stores no list of fragment positions or Spectre widgets in
memory. The index is a temporary file with `DeleteOnClose` semantics
and must be disposed. Read-only source snapshots and the source cursor
remain owned by the caller.

### Complete-table fallback

If a native fragment cannot safely fit, the index discards **all**
partial native records and reports a reason:

- `NarrowTerminal`
- `HeaderTooTall`
- `FirstRowTooTall`
- `NoContinuationRow` (when a previously expected continuation is
  no longer present)

`IsNative == false`, `FragmentCount == 0`, `SourceDataRows == 0`,
and `NextDataLine == HeaderLine + 2` mean no data source rows were
accepted. A future mixed document renderer must use its existing
**compact** table layout for the complete block instead of mixing
native and compact fragments without a shared page plan.

A table containing only a valid header and delimiter (zero data
rows) is still indexed as **one native header-only fragment**, provided
its border and header fit the visual budget.

If the temporary index budget is exceeded or source decoding fails,
creation throws, and its temporary file is disposed without changing
the original Markdown.

## Random source-line jumps

`GetFragmentAsync(fragmentNumber)` reads one 28-byte record via a
fixed-size disk seek. The returned `MdViewNativeTableFragment`
exposes its original data range and actual native visual height.

`FindFragmentAsync(sourceLine)` uses **binary search** over the disk
records. Source data lines map to exactly one fragment. The original
header and delimiter map to fragment zero; non-table lines return
null. No full-record array needs to be loaded to find a distant row.

A fragment's `NextDataLine` is the first **unconsumed** source row,
so a successive fragment starts at precisely that line. An inline
renderer can repeat the native header but must not repeat or skip
source data.

## Index/renderer parity

`MdViewNativeTableFragmentRenderer.RenderAsync` takes an indexed
fragment number and regenerates its small native widget from the same
validated source and planner. Before it writes anything to the
console, it verifies:

1. The index is fully native (no compact fallback).
2. The caller supplies the **same source cursor instance** and a header
   whose labels and alignment cells still match the validated source.
3. Terminal width, height and Unicode capability match the index.
4. Recomputed source line bounds, row count, Spectre-rendered height
   and continuation flag all match the disk record.

A stale viewport or source/measurement mismatch therefore rejects
rendering **before output**. A terminal resize requires rebuilding the
index. This is the foundation for using one layout contract from both
page measurement and actual display.

This component renders **one table fragment only**. It does not yet
draw a surrounding Markdown paragraph, page header/footer or the
main navigation prompt.

## Next integration milestone

The main disk-backed Markdown page index currently records only
`(PhysicalLine, WrapRow)` positions for compact source-line layout.
Native tables also require a continuation state carrying the
original header and the next source data line, with a repeated
header and border measured on subsequent pages.

The next step is a **mixed visual-block event plan**: ordinary Markdown
rows + native table fragments + compact fallbacks under one remaining
page-height budget, with disk-backed page starts and a renderer that
consumes the same events. It must handle a table beginning after
ordinary prose, page breaks before a too-tall native header, jumps
into a table, and terminal resize. Only after that parity is
verified should the user interface offer embedded native tables
as an opt-in mode.

## Verification

After pulling the latest Toolbox commits:

```bash
dotnet test Blackboard/Tools/MdView.Tests/MdView.Tests.csproj
dotnet test Toolbox.sln
dotnet run --project Blackboard/Tools/MdView -- preview README.md
```

The eleven `MdViewNativeTableFragmentIndexTests` cover 6/6/2
continuation through a 14-row source table, binary-search source
jumps, indexed rendering of only the chosen fragment, header-only
tables, compact fallback for narrow and short displays, five
measured one-row fragments with repeated headers, budget failure
without source mutation, stale header rejection, viewport resize,
invalid arguments, substituted source/header rejection, cancellation,
and disposal.

**Fresh focused/full tests for this increment are pending the
maintainer's .NET SDK run.** The previous table page planner
checkpoint was maintainer-confirmed green before these files.

Related:
[Native page planner](../Native-Table-Page-Planner/en.md) ·
[Native table blocks](../Native-Table-Blocks/en.md) ·
[Implementation status](../../IMPLEMENTATION_STATUS.md).
