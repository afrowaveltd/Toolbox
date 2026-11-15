# Optional content-width limit for mixed Markdown preview

## Motivation

A very wide terminal is useful for source code, but running prose becomes
harder to follow when each line spans hundreds of columns. MdView now offers
an **explicit, optional maximum width for document content** in the
`preview-native` mode. It affects Markdown prose, compact pipe-table
cells and complete small native Spectre tables **through the same
measurement path**.

The physical terminal is **never resized or misreported**. The limit is
a presentation preference, not a change to the source file or the
validated source cursor.

## CLI

By default, the mixed preview continues to use the full terminal width:

```bash
mdview preview-native README.md
```

To restrict document content to at most 100 terminal columns:

```bash
mdview preview-native README.md --max-width 100
dotnet run --project Blackboard/Tools/MdView -- preview-native README.md --max-width 100
```

The flag is case-insensitive and accepts whole numbers **40 through
4096**. It follows the local file path. Omit the flag for automatic
physical-terminal width. Invalid values or unsupported syntax are
rejected by CLI parsing before opening the document.

The original `mdview preview README.md`, `mdview source` and all
remote/link browser commands remain unchanged.

## Geometry contract

The cap is expressed in **visible Markdown content columns**. The
existing pager reserves two columns of horizontal safety margin.

`MdViewMixedContentWidth` centralizes this contract:

- Physical width 220, cap 100 → effective planning width **102**,
  Markdown layout width **100**.
- Physical width 72, cap 100 → effective planning width **72**,
  Markdown layout width **70**.
- Physical width 220, no cap → effective planning width **220**,
  Markdown layout width **218**.
- Physical width 220, cap 40 → effective planning width **42**,
  Markdown layout width **40**.

These widths are independent of the terminal's actual height and
Unicode capabilities.

The **same** effective width is passed to:

1. `MdViewTerminalLineLayout.Layout` for source-aware Markdown text,
   including headings, lists, quotes, links, code clipping and compact
   pipe-table cells.
2. `MdViewNativeTablePagePlanner` and
   `MdViewSpectreTableFactory` for bounded native grids. The factory
   sets the native widget's **explicit** table width; Spectre continues
   measuring its segments against the real console profile.
3. `MdViewMixedVisualPagePlanner` for the exact visual row count
   and source/wrap continuation of a page.
4. `MdViewMixedVisualPageIndex` for all 56-byte on-disk page records.
   The index retains `MaximumContentColumns` as instance metadata.
5. `MdViewMixedVisualIndexedRenderer`, which replans the selected
   canonical page with that **identical cap**, verifies the result
   against its stored disk record and writes only after preflight.

The cap may change total `Page X of Y` and which source lines are
visible on each page. All jumps still target original physical
Markdown source lines. The index stores source line and wrap-row
positions, including active compact table continuation.

The user-facing header displays `max N columns` when the cap is
enabled. Without `--center`, header, status and navigation stay at the
physical terminal's left edge. With `--center`, the **entire viewer**
shares the reading-column gutter: file information, page status,
interactive prompts and the document body. Single-line chrome is
clipped to the capped reading width; the physical terminal dimensions
still determine resizing and safety limits. Text remains left-aligned
**within** its reading column.

## Native tables and safety

A width cap can cause an otherwise eligible native table to switch
to the compact layout. For example, a two-column table in a
220-column terminal is eligible for a wide native grid; the same
terminal capped at 40 content columns uses compact cells. This is
intentional and consistent across the entire source-table block.

Small native tables must still fit wholly on a fresh page. If a
whole native widget would fit a new page but not the space following
the current prose, it is deferred before its header. Source data
rows are not lost or duplicated across the boundary.

Tables with more than six source data rows, narrow grids, and
oversized native widgets continue to use the established compact
layout across all visual pages.

The index records the **actual physical terminal width/height and
Unicode support**. A physical resize requires rebuilding the index,
even if the configured cap means the effective content width would
remain unchanged. No global Spectre console profile properties
are modified.

## Tests

`MdViewMixedContentWidthTests` checks:

- Deterministic physical/effective/content width calculations.
- Rejection of invalid cap values.
- Different measured page counts with and without a cap, with
  binary-search source-line jumps preserved.
- Native table geometry, disk-index reconstruction and unchanged
  original physical console profile.
- Compact fallback when the cap is too narrow for a native grid.
- Terminal widths 40, 63, 64, 80, 120 and 220, including table
  deferral before a page boundary.
- Explicit checks that wrapped styled Markdown rows do not exceed
  their configured content-column budget.
- Physical terminal resizing rejected before output even with an
  unchanged cap.
- CLI flag case-insensitivity, preservation of the previous
  command, and invalid argument handling.
- Visible opt-in `max N columns` header in the interactive viewer.

The previous UI screenshots and feedback show working mixed
pagination at several terminal sizes. **New width-limit tests and
builds still need fresh maintainer verification**, as the assistant
environment does not provide a .NET SDK.

## Verification

```bash
git pull
dotnet test Blackboard/Tools/MdView.Tests/MdView.Tests.csproj
dotnet test Toolbox.sln
dotnet run --project Blackboard/Tools/MdView -- preview-native README.md --max-width 100
```

Try the same local document in a very wide terminal with and without
the option. Confirm that the content is narrower when requested,
page counts adjust, and source-line navigation still lands on the
correct canonical page. Try a short two-column Markdown table and
a longer table to compare native/compact fallback.

Related:
[Mixed preview UI](../Mixed-Native-Preview/en.md) ·
[Disk-backed page index](../Mixed-Visual-Page-Index/en.md) ·
[Implementation status](../../IMPLEMENTATION_STATUS.md).

## Center the reading column (2026-11-15)

The width cap can now be combined with `--center`:

```bash
mdview preview-native README.md --max-width 100 --center
```

The parser also accepts `--center --max-width 100`.
The entire limited-width reading column receives one
constant left margin. Native tables are padded on
the left **without vertical padding** and their actual
Spectre row height is re-measured before output.
The document page index and physical terminal
profile are unchanged by horizontal positioning.

[Centered reading column contract](../Mixed-Content-Alignment/en.md).
