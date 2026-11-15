# Source-aware native Markdown table blocks

## Purpose

The eventual inline Spectre.Console table renderer needs a page-independent
definition of a **real Markdown table block**. Counting each pipe-looking
physical line independently is insufficient: the header and GFM delimiter
may fall on different visual pages, while native grid borders and wrapped
cells consume more terminal rows than the corresponding source lines.

`MdViewSpectreTableBlockReader` is the first, bounded foundation for that
transition. It is now shared by the existing optional native table inspector.
**The default `mdview preview PATH` still renders compact, numbered pipe
cells inside the main page.** Native tables are not automatically embedded
and the existing `Page X of Y` disk index remains unchanged.

## Recognition and source boundaries

A recognized table requires:

- A top-level, explicitly framed header containing **2–32 pipe cells**.
- An immediately following GFM alignment/delimiter row with exactly the
  same number of columns (for example `---`, `:---`, `---:`, `:---:`).
- Zero or more **contiguous** data rows with the same width. The first
  non-matching physical line ends the table; it is **not consumed**.
- Only container-classified plain lines can be table members. Fenced code,
  quoted material, list-owned text and ambiguous delimiter-free prose are
  not promoted to native tables.
- Escaped pipes and pipes inside matching inline backticks use the existing
  conservative cell parser. Source text and source-line numbers are preserved.

`FindFirstHeaderAsync(cursor, firstLine, lastLineExclusive)` scans **at
most 128 candidate header lines**. It may read the single delimiter line
immediately *beyond* the visible page to validate a header that ends on the
page boundary. This lookahead does not move the displayed source position.

`ReadHeaderAsync` returns a source-bound header and the original alignment
cells. `ReadChunkAsync` consumes **at most six physical data rows**, reports
`NextDataLine` as the first unconsumed line, and probes at most one extra
line to set `HasMore`. Passing that resume line into the next call avoids
both duplication and omission. A caller that needs fewer rows may request
1–6 rows per chunk; the native height helper can determine that number
from actual bounded Spectre render segments.

These operations use the existing validated UTF-8 snapshot and
`MdViewContainerPageCursor` disk-backed journal. No complete table, source
document, or global in-memory table catalog is materialized.

## Integration today

The modal `MdViewSpectreTableViewer` now delegates table discovery, header
validation and per-row membership checks to this shared reader. This preserves
the existing modal UI, six-row cap, bounded height measurement, narrow
transpose mode and compact fallback. The reader's additional chunk API
provides a reusable source-continuation contract for the next paginator.

## Remaining work for inline native tables

A separate, explicitly enabled native block page model must be implemented
**before** changing the default formatted renderer:

1. Turn each recognized source block into a sequence of source-bound,
   bounded native chunks. Keep a cursor for the current table and a source
   line for the next ordinary Markdown block.
2. Measure each prospective native chunk using
   `MdViewSpectreTableHeight.CountRows` with the *actual* viewport profile,
   including borders, header rules, wrapping and repeated column headings.
3. Record resumable positions (including within-table row/field state) in
   a disk-backed visual index. The current `MdViewVisualPosition(Line,
   WrapRow)` alone cannot express a repeated header or partial table frame.
4. Make index and renderer consume the *same* native block layout events.
   Never count compact rows in the index and render native grid rows on
   screen: that would corrupt `Page X of Y` and jump targets.
5. Handle a native header or single row that cannot fit: fall back to the
   compact layout for that block, without losing rows or UI navigation.
6. Add regression tests for resizes, tiny terminals, source-line jumps into
   tables, cross-page table continuation, wide/narrow transpose modes and
   bounded memory/disk budgets.

This intentionally avoids a premature switch that would undo the
maintainer's confirmed green compact-preview baseline.

## Verification

Run on a machine with a .NET 10/11 SDK:

```bash
dotnet test Blackboard/Tools/MdView.Tests/MdView.Tests.csproj
dotnet test Toolbox.sln
dotnet run --project Blackboard/Tools/MdView -- preview README.md
```

The six new `MdViewSpectreTableBlockReaderTests` exercise boundary
lookahead, validated alignment, consecutive six-row chunks, non-table
termination, escaped and inline-code pipes, fenced/quoted/mismatched
rejection, invalid arguments and cancellation.

**These new tests were authored and committed but have not been executed in
this assistant environment.** Earlier maintainer-reported green runs are
not evidence that this new increment has passed.

See also [Native Spectre tables](../Native-Spectre-Tables/en.md),
[Responsive preview](../Responsive-Preview/en.md) and
[Implementation status](../../IMPLEMENTATION_STATUS.md).

## Measured continuation layout available (2026-11-14)

`MdViewNativeTablePagePlanner` now consumes the source-bound header
and builds small, actual-Spectre-measured native grid fragments with
an exact next source line or explicit compact fallback. It reuses the
native inspector's table factory and never writes output while
planning. This is a stepping stone toward an on-disk block-event
page index, not automatic native embedding in the current preview.

[Measured native table page planner](../Native-Table-Page-Planner/en.md).

## Indented literal-code exclusion (2026-11-15)

The shared native table block reader now rejects four-space-indented
literal code, matching the original Markdown line renderer's code-first
classification. Neither a header/delimiter pair nor a following data
row embedded in a literal code block may be treated as native table
cells. The mixed visual planner also skips its disk lookahead for these
lines. A new block-reader regression and a new mixed-page regression
protect this behavior; both await maintainer test verification.
