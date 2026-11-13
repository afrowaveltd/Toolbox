# Compact pipe-table preview for narrow terminals

## Purpose

`mdview preview PATH` can now present simple, explicitly framed
Markdown pipe-table rows as **numbered vertical cells**. The approach
favors reliable reading on very narrow terminal windows, low RAM usage,
and accurate page boundaries over a visually wide grid.

Example source:

```markdown
| Name | Description |
| --- | --- |
| CPU | Executes instructions |
```

Compact display:

```text
1 │ Name
2 │ Description
────────────────
1 │ CPU
2 │ Executes instructions
```

Each source table row remains one physical source line, but can occupy
multiple **visual** rows. The shared
`MdViewTerminalLineLayout.Layout` results are consumed by
**both** the disk-backed page index and renderer. Therefore, page
counting and visible output agree even when a cell spans multiple
visual pages.

## Recognition and safety contract

`MdViewTerminalPipeTableLayout` recognizes only conservatively
unambiguous pipe rows:

- Both ends must have an actual, unescaped `|` delimiter.
- A row must have at least **two** cells and no more than **32**.
- Pipes escaped as `\|` and pipes inside backtick-delimited inline
  code do **not** separate cells.
- A row must have `Plain` container ownership, outside quotes,
  lists, headings and code fences. Ambiguous context retains ordinary
  Markdown handling.
- Separator rows such as `| :--- | ---: |` become a single light
  divider row. This is visual guidance, not a persistent table header.

Cell values use the existing conservative inline Markdown parser.
Link labels, inline code and emphasis remain styled as visible text,
while link destinations do not consume display width. Long cell values
wrap at natural word or sentence boundaries, and their continuation
rows use hanging indentation when the viewport is wide enough to
display the cell index.

A minimal eight-column text area is preserved. On exceptionally narrow
terminals the ordinal prefix can be suppressed rather than overflowing
the screen. All output uses literal `Spectre.Console.Text` segments;
neither Markdown markup nor link destinations are executed or opened.

The original validated UTF-8 snapshot is never rewritten.

## Scope and limitations

This is **not yet a full GitHub-Flavored Markdown table renderer**.
It intentionally does not compute common column widths across
multiple source rows, infer header semantics from neighboring rows,
support delimiter-free rows, guarantee CommonMark table alignment,
or attempt to convert deeply nested quoted/list tables.

These capabilities would require a separate bounded table-state
scanner with lookahead or a per-table disk index, so they are
left for a future milestone rather than storing entire tables
in RAM. The existing 1 MiB GitHub browser limit is unchanged;
this feature is for the local experimental `preview` mode.

The current representation works across terminal resize events:
the viewport-specific disk page index is regenerated and visual
cells are reflowed on the next render.

## Verification

```bash
dotnet test Blackboard/Tools/MdView.Tests/MdView.Tests.csproj
dotnet test Toolbox.sln
dotnet run --project Blackboard/Tools/MdView -- preview README.md
```

Ten new `MdViewTerminalPipeTableLayoutTests` cover simple headers,
separator rows, literal escaped/code pipes, preserving backslashes\ninside inline code, styled link cells,
ambiguous input fallback, fenced-code exclusion, very narrow
terminals, maximum column guards, and agreement between disk
page starts and renderer resume coordinates.

**Fresh .NET tests have not yet been executed or maintainer-confirmed**
for this increment. The prior all-green checkpoint belongs to
the preceding styled-layout implementation.

[Styled visual layout](../Styled-Visual-Layout/en.md) ·
[Responsive preview](../Responsive-Preview/en.md) ·
[Implementation status](../../IMPLEMENTATION_STATUS.md).

## Literal backslashes inside inline code (2026-11-13)

The compact table's display-time unescaping of `\\|` now operates
**only outside inline code spans**. A regular table cell containing
`a\\|b` displays as `a|b`, but an inline code cell containing
`a\\|b` retains the literal backslash. The recognizer still
ignores both kinds of escaped pipe for column splitting.
This protection has an explicit regression test; the original
validated snapshot never changes.

## Optional native Spectre table view (2026-11-13)

The compact vertical cells remain the **main source-page fallback**.
A new opt-in **View table (Spectre)** menu action appears on
source pages with a confirmed Markdown header + alignment separator.
It constructs the actual `Spectre.Console.Table` with rounded grey
borders and real column alignment, in small source-bounded chunks.

The native inspector has its own modal paging and returns to
the same Markdown source page afterward. This separation
deliberately preserves the exact source/visual-page index while
we develop measurement of Spectre border and wrapped-cell
heights for future seamless in-page integration.

[Native Spectre table guide](../Native-Spectre-Tables/en.md).
