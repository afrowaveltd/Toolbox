# Opt-in long native Spectre tables across Markdown pages

## Scope

The experimental `mdview preview-native` command now supports an
**optional repeated-header native layout for long Markdown pipe tables**.
Previously, only tables with at most six physical source data rows
could be rendered as inline native Spectre grids; longer tables used
the safe compact line layout for their entire source block.

The new mode preserves that default and does **not** affect the
established `mdview preview PATH` reader.

```bash
mdview preview-native README.md --long-tables
mdview preview-native README.md --max-width 100 --center --long-tables
```

The flags `--long-tables`, `--max-width N` and `--center` can be
combined in any order and are case-insensitive. The CLI rejects
unknown or duplicate flags. The mode is **off by default**.

## Source-preserving native fragments

For a validated top-level Markdown pipe table with a matching
delimiter row, `MdViewNativeTablePagePlanner` can construct a
small native Spectre grid for a **bounded group of up to six source
data rows**. Every fragment uses the **original table header and
GFM alignment markers**, so the header is repeated visually on
continuation pages.

Native grids are source-bound rather than string-split display
approximations:

- A native fragment owns `FirstDataLine` through
  `NextDataLine` (exclusive), with no missing or duplicated
  physical data rows.
- Every fragment's `VisualRows` is measured through the actual
  Spectre render-segment pipeline, including the repeated header
  and rounded border.
- A continuing fragment starts a **fresh mixed visual page**.
  A first fragment also moves to the next page if the content
  above it leaves insufficient space.
- After the last native fragment, ordinary Markdown resumes at
  the first non-table physical source line.
- The actual source file and its validated snapshot are read-only.
  Source Markdown text is never interpreted as Spectre markup;
  cells use the established bounded literal-safe formatting.

## All-or-nothing eligibility and compact fallback

Before a long table's **first native fragment** is emitted,
`MdViewLongNativeTableEligibility.CanRenderEntireTableAsync`
does a bounded streaming preflight through the existing source
cursor. Each step plans one fragment at the **fresh-page content
height**, checking that at least one matching source row can
advance, its measured visual height fits, and the continuation
line progresses.

The preflight holds no accumulated native widgets in memory.
It checks at most **2,048 fragments** per table and then elects
the complete compact fallback if the limit is exceeded. That
represents at most 12,288 short data rows when six fit per
fragment, or fewer for wrapped/tall cells.

Native mode is refused for the entire source table when any
fragment cannot safely fit, when the grid is too narrow,
or when the scan budget is exhausted. This avoids
switching from native to compact **halfway through one table**.
The compact layout retains the original source-aware
header/column state across page boundaries, as before.

The preflight may revisit validated source lines during index
construction and during indexed page reconstruction. This
trades bounded extra I/O for avoiding a full table in RAM.
Its cap is conservative and can be reconsidered only after
measuring real large-file performance.

## Unchanged 56-byte mixed visual page index

The existing `MdViewMixedVisualPageIndex` records still occupy
**56 bytes each**. There is no schema migration.

A previously unused combination of the two legacy
compact-continuation fields represents native continuation:

| Position field | Native continuation value |
| --- | --- |
| `Line` | First unconsumed original source data line |
| `WrapRow` | `0` |
| `CompactHeaderLine` | Original source table-header line |
| `CompactColumns` | `-1` (native continuation sentinel) |

`MdViewMixedVisualPosition.NativeContinuation` constructs this
state and `IsNativeContinuation` identifies it. Compact tables
still store their normal positive column counts. The index
validates this sentinel **only when the new option is enabled**,
including its header-relative source position and zero wrap
row.

The index's `LongNativeTables` property stores the selected
mode as **instance metadata**, not an extra field per page.
`MdViewMixedVisualIndexedRenderer` uses the same choice when
reconstructing a requested canonical page and compares its
full `Start`, `Next`, row count, EOF and clipped-code flags
with the disk record before any output.

Source-line jumps remain canonical:

- The original header and delimiter lines map to the first
  native-table page.
- A data-row line inside a later fragment maps to the page
  that begins with or contains that fragment.
- Repeated headers on later pages are **visual** and do not
  introduce fake physical source lines.
- Back/Next and `Page X of Y` use disk-stored boundaries,
  not manual row approximations.

## Terminal, width and alignment behavior

The new mode uses the existing physical-width, height and
Unicode safeguards. Resizing the terminal rebuilds the mixed
index and reanchors on the original source line.

The optional `--max-width N` affects both text and native
fragment width **at measurement time**. A narrow capped
reading column can cause an otherwise eligible table to use
compact fallback. `--center` then shifts the complete
viewer (document, native table widgets, menu and status)
without changing their measured heights or source positions.

The existing source-content limit, maximum six fields per
native grid and safe compact fallback remain in effect.

## Regression tests

`MdViewLongNativeMixedPagingTests` covers:

- A 17-data-row table continued across multiple native
  pages with repeated original headers.
- Exactly-once display of every original data row, with
  unchanged source file bytes.
- Source-line jumps inside later native fragments,
  including header/delimiter lookup.
- An exactly fitting last native fragment without a
  phantom trailing document page.
- The original compact layout when the new option is absent.
- Compact fallback for a capped, narrow reading column
  even with `--long-tables` selected.
- Centered widget output and terminal-resize invalidation.
- Invalid/malformed native resume positions.
- Cancelled preflight and bounded fragment scan limit.
- First-fragment header deferral after preceding prose.
- Case-insensitive option parsing, flag combinations,
  duplicates and unrecognized arguments.
- Opt-in interactive pager output and read-only source bytes.

**Fresh .NET 10 test execution is pending for this milestone.**
The preceding maintainer report confirmed approximately 7,000
green Toolbox tests; that result predates the new opt-in
long-table code.

## Verification

```bash
git pull
dotnet test Blackboard/Tools/MdView.Tests/MdView.Tests.csproj
dotnet test Toolbox.sln
dotnet run --project Blackboard/Tools/MdView -- preview-native README.md --max-width 100 --center --long-tables
```

For a manual smoke test, open a Markdown file containing one table
with 14–20 data rows followed by prose. Navigate forward/back,
jump to a data line in a later fragment and resize the terminal.
Every later native fragment should show the original header,
but each data row should appear exactly once. Compare the same
source without `--long-tables` to confirm safe compact fallback.

Related:
[Mixed native preview](../Mixed-Native-Preview/en.md),
[mixed disk page index](../Mixed-Visual-Page-Index/en.md),
[native table fragment index](../Native-Table-Fragment-Index/en.md),
[implementation status](../../IMPLEMENTATION_STATUS.md).

## Fewer source journal requests (2026-11-15)

The whole-table native eligibility preflight continues to
scan source-bound fragments conservatively, but each
`MdViewNativeTablePagePlanner.PlanAsync` call now
requests up to six data rows plus its lookahead in a
**single bounded disk-journal read**. For a 14-data-row
table split into three fragments this means **three**
`ReadAsync` requests rather than individual requests
for every row and continuation probe.

The measured page geometry, source-line positions,
repeated headers, compact fallback, memory bounds,
original Markdown bytes and on-disk index format are
unchanged. No specific end-to-end HDD speedup is
claimed without real hardware measurements.

[Journal batching details](../Native-Table-Journal-Batching/en.md).
