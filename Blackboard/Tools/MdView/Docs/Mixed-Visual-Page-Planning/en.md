# Mixed visual page planning — Markdown and native Spectre tables

## Purpose and current status

The experimental `MdViewMixedVisualPagePlanner` and
`MdViewMixedVisualPageRenderer` now share one bounded layout for
ordinary styled Markdown rows **and** complete small native
`Spectre.Console.Table` widgets.

This milestone is a proof of exact **one-page** source/visual geometry,
not the final whole-document index. The existing
`MdViewTerminalPageIndex`, `MdViewTerminalPageRenderer` and
`mdview preview PATH` continue to use the established, maintainer-verified
compact Markdown layout. The optional `View table (Spectre)` modal also
remains unchanged. No default or CLI behavior has been switched.

## Mixed page plan

`MdViewMixedVisualPagePlanner.PlanAsync(cursor, start, console,
contentRows)` reads through the caller-owned, validated,
disk-backed `MdViewContainerPageCursor`. It creates one bounded
`MdViewMixedVisualPage` containing ordered blocks:

- `MdViewMixedVisualTextBlock`: one or more visual rows from an
  original, classified Markdown source line. It reuses
  `MdViewTerminalLineLayout` for headings, emphasis, links, lists,
  quotes, code clipping and compact pipe cells.
- `MdViewMixedVisualTableBlock`: a complete small, confidently
  recognized native Spectre table. Its `Height` includes the real
  rounded border, column headings, wrapped cells and bottom border,
  measured through `MdViewSpectreTableHeight`.

The planned `Rows` is exactly the sum of its block heights and
never exceeds the caller's remaining page content-row budget
(1–128 rows, no more than the actual terminal height).
`ContainsClippedCode` and `AtEnd` are reported separately.
The planner never prints or modifies the original Markdown.

The page's `Next` is a `MdViewMixedVisualPosition` carrying the
physical source `Line`, within-line `WrapRow`, and (when needed)
a compact-table `CompactHeaderLine` and `CompactColumns`. This
state is sufficient to resume a compact table whose header,
delimiter or individual wrapped cell crosses a visual page boundary.

## Deliberately conservative native-table rule

Native grids are used **only if all conditions are true**:

1. The source has a top-level, explicitly framed pipe header,
   followed by a matching GFM delimiter.
2. The entire table has **at most six data rows**. A six-row bounded
   read plus one lookahead proves that no further source data row
   belongs to this table.
3. Its columns fit the existing wide native-grid rule (at most six
   columns, width at least 64 and at least 14 columns per field).
4. The real measured Spectre table, with **all** data rows, fits a
   fresh content page at the current terminal profile.

If a valid small native table fits a fresh page but not the space
remaining after previous prose, the planner **ends the current page
before the table header**. The next page starts with the intact
native grid; it never displays half a rounded header/frame.

Long tables, narrow layouts and short-page overflows use the
existing **compact numbered-cell layout for their entire source
block**. The active compact table's column count and header line
travel with each page resume point, so later source rows cannot
suddenly become native fragments in the middle of the same table.

This is intentionally stricter than the separate native table
inspector and the single-table fragment index, which already support
six-row continuation chunks for long tables. The mixed page planner
does not yet repeat native headers across visual pages.

A fast, allocation-free first-character check avoids performing
two-line table lookahead disk reads on ordinary prose lines.

## Matching renderer

`MdViewMixedVisualPageRenderer.Render(console, page, theme)` uses
the planned sequence directly. Text spans are printed with the
**same** `MdViewTerminalPageRenderer.ResolveStyle` mapping as the
existing compact reader. Markdown content is rendered as literal
Spectre text, never evaluated as markup.

Before writing any part of the page, the renderer checks the
original terminal width, height and Unicode capabilities, the
total row count, each text block length, and the **real**
Spectre-rendered height of every native table widget. A resize
or altered widget rejects the page before output. The returned
source position is the exact `page.Next`.

Both planner and renderer are independent of the existing main
preview UI. This allows parity testing without disturbing the
currently green pager.

## Test coverage

Twelve `MdViewMixedVisualPageTests` cover:

- A real rounded native table between ordinary prose paragraphs,
  including literal bracketed markup and unchanged source bytes.
- A complete native table moved to the next page before its header
  when insufficient space remains on the current page.
- A 14-row table remaining compact across many pages.
- A compact header and its delimiter split at a page boundary.
- A narrow terminal retaining compact pipe cells.
- An over-height, long-cell table remaining compact across pages.
- The exact same page starts as the pre-existing compact visual
  index for ordinary headings, wrapped text, lists, quotes and code.
- **Byte-for-byte matching terminal output** between the new mixed
  renderer and existing compact renderer for a text-only page.
- Fenced or four-space-indented literal code containing pipe text never
  promoted into a native table.
- Resize detection before any page output.
- Invalid resume states, content budgets and cancellation.

**These new changes have not yet been executed with a .NET SDK
in this assistant environment.** The maintainer's previous
all-green checkpoint applies to the preceding disk-backed
single-table fragment index.

## Verification

```bash
git pull
dotnet test Blackboard/Tools/MdView.Tests/MdView.Tests.csproj
dotnet test Toolbox.sln
```

The next milestone is a **disk-backed mixed-document page-start
index** that repeatedly consumes this exact planner and stores
its full `MdViewMixedVisualPosition` records. It can then
offer a separate opt-in mixed visual preview only after index
and renderer parity, source jumps, resize and bounded-memory
behavior have been verified.

Related:
[Native table fragment index](../Native-Table-Fragment-Index/en.md) ·
[Native table page planner](../Native-Table-Page-Planner/en.md) ·
[Responsive compact preview](../Responsive-Preview/en.md) ·
[Implementation status](../../IMPLEMENTATION_STATUS.md).

## Disk-backed whole-document page index (2026-11-15)

`MdViewMixedVisualPageIndex` now builds complete temporary
page records by repeatedly calling this exact mixed planner.
Each 56-byte record stores the full source/wrap/compact-table
continuation state for both ends of a page. Disk binary
search maps a source line (or wrap-row position) to its
canonical page; indexed rendering re-plans that page and
checks its geometry against the record before output.

This implementation is **not** yet wired to the default
`mdview preview` UI. A separate opt-in navigation mode,
window-resize reindexing and user-facing source jumps
remain the next milestone.
[Mixed page index contract](../Mixed-Visual-Page-Index/en.md).
