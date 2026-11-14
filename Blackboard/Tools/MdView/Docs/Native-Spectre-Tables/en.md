# Native Spectre.Console table inspector

## Purpose

`mdview preview PATH` now offers **View table (Spectre)** when the
current visual source page contains a confidently recognized Markdown
pipe-table header immediately followed by a matching GFM delimiter row.

This launches a **real `Spectre.Console.Table`**, not a handwritten
box drawing or a sequence of simulated numbered cells. The native
widget provides a rounded grey border, aligned headers and automatic
cell wrapping. Source Markdown is read only from the already validated
UTF-8 snapshot and existing disk-backed container cursor.

The main Markdown preview retains its previous **compact vertical cell
layout and exact disk-indexed Page X of Y**. The native table inspector
is deliberately a modal action: a full Spectre table can add borders
and wrap rows differently from the current line-local page index.
It must not silently corrupt page numbering or main-document source
positions. Returning from the inspector redraws the same source page.

## How to use

```bash
dotnet run --project Blackboard/Tools/MdView -- preview README.md
```

When a source page includes, for example:

```markdown
| Name | Capacity | Status |
| :--- | ---: | :---: |
| Main | 512 GB | OK |
| Backup | 2 TB | Ready |
```

the reader's navigation menu gains **View table (Spectre)**. Choose it
to see the actual Spectre table with the correct column headers,
round borders and separator-rule alignment. A wide terminal presents
the native multi-column grid. A narrow terminal instead presents a
rounded two-column **Column / Value** table, preserving all fields by
paging across columns when necessary.

The native inspector requires **at least 28 terminal columns**.
Below that, the option remains hidden; the existing compact Markdown
source preview remains available.

Use **Next table chunk** to continue through larger tables, or
**Back to Markdown preview** (or Escape) to return to the source page.
The inspector rechecks terminal width and height at each chunk.

## Bounded-memory contract

The inspector uses `MdViewContainerPageCursor`, which stores source
classification events in a capped temporary disk journal. No full
table is materialized in RAM. Only one bounded group is used to build
each real Spectre widget:

- At most **6 source data rows** in one wide-grid chunk. The live
  terminal height can lower the chunk size further.
- At most **8 visible fields** per transposed chunk, again reduced
  when the terminal is short.
- At most **256 source UTF-16 code units** of each cell are rendered;
  smaller viewport-dependent limits can apply. Long cells show `…`
  explicitly. The source file and validated snapshot remain intact.
- At most **32 columns** accepted in one source pipe row, reusing the
  existing conservative `MdViewTerminalPipeTableLayout` parser.
- The matching delimiter row determines whether a framed pipe row
  is a real Markdown table; ambiguous pipes or code-fence contents
  never trigger the native table menu.

A native table is **never** built from a whole gigabyte document.
Normal paging, link indexes and other functions retain their
existing budgets.

## Safety and formatting

Native Spectre table headers use `Markup.Escape`; cell values use
literal `Spectre.Console.Text` renderables. Therefore strings such as
`[red]not markup[/]` from an untrusted Markdown file are displayed
as data, never evaluated as terminal decoration.

Cell text is converted through the existing conservative Markdown
inline parser so visible link labels are shown without their hidden
destinations. Inline code and escaped pipes keep their existing
source-safety rules. For the first version, **cell-level rich syntax
colors are not fully reproduced** in the modal widget; the default
Markdown preview continues to provide its separate inline styling.

## Scope and next step

This is the first native widget integration, **not yet automatic
in-page rendering of all GFM tables**. That requires a bounded
table-block state machine (with header context and page-break rules)
and precise measurement of Spectre's rendered border and wrapped
cell rows. A future milestone can share that measured row geometry
with `MdViewTerminalPageIndex` before switching the primary Markdown
screen to true native tables.

Current recognition intentionally requires a top-level, explicitly
framed header **followed immediately** by a matching pipe delimiter
row. Nested table constructs, delimiter-free tables and full GFM
interpretation remain out of scope.

## Verification

```bash
dotnet test Blackboard/Tools/MdView.Tests/MdView.Tests.csproj
dotnet test Toolbox.sln
dotnet run --project Blackboard/Tools/MdView -- preview README.md
```

Eight `MdViewSpectreTableViewerTests` cover correct header/delimiter
recognition, real rounded borders, grid/transpose modes, bounded
row/field navigation, malicious-looking literal markup, long-cell
clipping with intact source, fence exclusion and invalid-source
rejection. Three additional preview integration tests verify opening
and returning from the modal viewer, absence of the option for
non-table text, and compact fallback for terminals under 28 columns.

**The new source/test changes have not yet been compiled or executed
in this assistant environment.** The last confirmed green checkpoint
belongs to the previous compact pipe-table implementation.

[Compact table layout](../Compact-Pipe-Tables/en.md) ·
[Responsive preview](../Responsive-Preview/en.md) ·
[Implementation status](../../IMPLEMENTATION_STATUS.md).

## Render-segment height-aware native chunks (2026-11-14)

The native modal now has a dedicated `MdViewSpectreTableHeight`
helper. Spectre's `IRenderable.Measure` supplies horizontal
constraints, not an exact vertical row count. The helper instead
counts the real rendered `Segment` line breaks produced by the
**same Spectre pipeline** and the **current** terminal capabilities
and width, including rounded borders, header rules and wrapped cells.

Before accepting each additional source row (or transposed field),
the viewer creates a **bounded candidate native table**, measures
its actual row count, and adds it only if the widget still fits the
remaining terminal height. Otherwise that source row/field is kept
for **Next table chunk**, with no omission or duplication.
The previous six-source-row and eight-field maximums remain hard
upper bounds. A 512-rendered-row guard prevents runaway
measurement even on unusually tall terminal profiles.

The modal reserves terminal space for its header, status line and
selection prompt. Grey header/status text is clipped to one visual
row instead of wrapping unexpectedly. For smaller terminal heights,
the optional informational note is omitted to preserve navigation
space. If even a one-row native table cannot fit, the modal
returns to the existing compact Markdown preview rather than
repeatedly offering an unrenderable chunk.

This remains a **modal native table**: the main disk-backed visual
page index still uses its compact table layout and retains its
verified source-line positions. Embedded native tables in normal
Markdown pages still require a bounded block-aware layout/index
that handles the exact border rows across page boundaries.

New test coverage: `MdViewSpectreTableHeightTests` checks the
read-only measurement contract, narrow/wide wrapped-cell heights,
early row-budget stopping and invalid arguments.
`WrappedNativeTableRowsFitMeasuredTerminalHeight` verifies that
the visible native table stays within the measured viewport
budget while retaining next-chunk navigation.

**New source and test changes are pending a fresh .NET run.**
The previous native-table tests were maintainer-confirmed green
before this incremental height-measurement change.

### Minimum geometry and header preflight (2026-11-14)

The native inspector now requires **at least 28 terminal columns
and 12 terminal rows**. This avoids opening an unusable modal in a
very short terminal; the original compact Markdown source remains
accessible. Even a table with no data rows must pass an actual
render-segment height check for its native header and border.
An oversized table heading that cannot fit returns safely
without presenting an unreachable navigation prompt.

Two extra regressions test these guards:
`TooShortTerminalFallsBackWithoutOpeningNativePrompt`
and `OversizedHeaderAloneDoesNotOverflowTinyNativeModal`.
The new test cases, including height measurement, still await
the maintainer's fresh .NET run.

## Shared source-bound table recognition (2026-11-14)

The inspector now uses `MdViewSpectreTableBlockReader` to locate a
header plus matching delimiter and to validate individual data rows.
The reader also exposes a six-row `ReadChunkAsync` API with a precise
`NextDataLine` and one-line `HasMore` lookahead. A table header on
the final visible line can be recognized using the delimiter just
beyond that page. All inputs remain validated, source-bound and
disk-backed; the default compact page layout and modal controls do
not change. These new source/tests await fresh execution.

[Full table-block design and inline pagination requirements](../Native-Table-Blocks/en.md).

### Clipped versus wrapped table headers (2026-11-14)

The 64-column × 12-row regression revealed an important distinction:
an extremely long **unbreakable** header can be clipped to fit the
native Spectre table width, so the modal can correctly open and must
remain dismissible with Escape. A long **word-wrappable** header can
instead occupy several actual rendered rows and exceed the five-row
table budget available at that height. Only the latter must fall back
without prompting.

The tests now cover these cases separately rather than assuming that
character count alone implies measured table height. The source
Markdown is never modified and the height preflight remains based on
Spectre's real output segments.

Maintainer's last focused test run: **219 passed, 1 failed, 220 total**
before this test correction. A fresh run is still required.
