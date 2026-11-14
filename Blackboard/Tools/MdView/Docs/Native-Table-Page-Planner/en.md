# Native table page planning (measured visual fragments)

## Goal

The previous native Spectre table inspector is a modal view. It does not
yet participate in the local Markdown pager's disk-backed \`Page X of Y\`
index. The new \`MdViewNativeTablePagePlanner\` is a **non-rendering**
intermediate step toward that integration.

Given a validated \`MdViewSpectreTableHeader\`, a physical data-line
resume position, an actual Spectre console profile and an available
visual-row budget, the planner constructs at most one **native
\`Spectre.Console.Table\` fragment**, measures it using the real rendered
segment stream, and returns its exact next physical source line.

**This is not a new default UI mode.** \`mdview preview PATH\` still
uses the existing compact pipe-cell presentation and its previously
verified disk-backed visual index. \`View table (Spectre)\` still opens
the separate modal inspector.

## Shared presentation contract

\`MdViewSpectreTableFactory\` now owns the same literal-safe table
construction already used by the inspector. The factory was extracted
without changing the inspector's visual rules:

- Rounded grey native border.
- Header alignment based on source delimiter cells.
- Literal source cell presentation: bracketed content is not executed
  as Spectre markup.
- A maximum of 256 source characters per visible data cell, with
  explicit ellipsis clipping. The source file is never modified.
- A maximum of 96 characters per header cell, as before.

The inspector delegates table creation and cell formatting to this
factory. The new planner uses exactly the same factory and
\`MdViewSpectreTableHeight.CountRows\` measurement helper.

## Page fragment contract

\`PlanAsync(cursor, header, firstDataLine, console,
availableContentRows, maximumSourceRows)\` returns
\`MdViewNativeTablePageChunk\`. It contains:

- \`HeaderLine\`: the original zero-based Markdown table header.
- \`FirstDataLine\`: the physical source data line requested by caller.
- \`NextDataLine\`: the **first unconsumed** source line.
- \`DataRows\`: accepted data source rows, from 0 to 6.
- \`VisualRows\`: precisely measured Spectre widget height, including
  native borders and column headers.
- \`HasMore\`: whether the immediately following source line belongs
  to the same Markdown table.
- \`Widget\`: the bounded, caller-owned renderable, or null on fallback.
- \`FallbackReason\`: one of \`None\`, \`NarrowTerminal\`,
  \`HeaderTooTall\`, \`FirstRowTooTall\` or \`NoContinuationRow\`.

The caller must supply the actual number of **remaining content rows**,
not the full screen height. Measurement uses the current Spectre
console width/capabilities and stops early at the height budget.
A candidate row that overflows is not consumed; the preceding accepted
rows remain valid. On a fallback, **zero source rows are consumed** and
the source resume line is unchanged.

A native fragment currently requires a **wide multi-column grid**:
at least 64 columns, at least 14 columns per Markdown field and
at most six source columns. Otherwise the planner explicitly asks
for compact fallback. This avoids pretending a narrow or transposed
table already has an appropriate in-page source-indexing model.

When a table spans several page fragments, pass the previous
\`NextDataLine\` into the next planning request **along with the
original header**. Each fragment repeats the native table's
column headings, and the measured repeated border/header geometry
is included in its \`VisualRows\`.

The planner scans at most six data source rows plus one trailing
lookahead, using the validated snapshot's existing on-disk cursor.
It writes nothing to the console, retains no entire table in RAM,
and never changes input Markdown or the old page index.

## Why this is separate from the main page index

The current \`MdViewTerminalPageIndex\` records pairs
\`(PhysicalLine, WrapRow)\`. These suffice for compact
source-line rendering but are not enough to represent a partially
rendered native table whose frame and column header repeat on the
next page.

The next implementation increment needs a disk-backed **visual
block-event index**, recording source anchors plus table-fragment
state. Its renderer must consume the same native widget measurements,
fallback decisions and resume positions as the planner. Only after
that parity is tested should \`mdview preview\` offer optional in-page
native grids. The existing compact layout should remain the safe
fallback for narrow screens and any over-height table block.

## Verification

Run after pulling new commits:

\`\`\`bash
dotnet test Blackboard/Tools/MdView.Tests/MdView.Tests.csproj
dotnet test Toolbox.sln
dotnet run --project Blackboard/Tools/MdView -- preview README.md
\`\`\`

Six new \`MdViewNativeTablePagePlannerTests\` exercise exact
six-row continuation across a 14-row table; height-constrained
one-row fragments; narrow fallback with zero consumed lines;
header and first-row overflow; source/markup safety; invalid
budgets, nonexistent continuation and cancellation.

The current maintainer-confirmed all-green checkpoint **precedes**
these changes. The new planner and tests have not been run with
the .NET SDK in the assistant environment; do not claim them green
until the maintainer verifies the updated build.

Related documentation:
[Source table blocks](../Native-Table-Blocks/en.md) ·
[Native Spectre inspector](../Native-Spectre-Tables/en.md) ·
[Responsive preview](../Responsive-Preview/en.md) ·
[Implementation status](../../IMPLEMENTATION_STATUS.md).
