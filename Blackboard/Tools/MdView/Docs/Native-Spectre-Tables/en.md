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
