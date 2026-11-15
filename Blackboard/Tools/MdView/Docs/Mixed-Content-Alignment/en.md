# Optional centered reading column

## Purpose

The mixed Markdown/native Spectre preview can optionally **center its
reading column** within a wider physical terminal. This is a presentation
choice for comfortable long-form reading, especially when the terminal
window is much wider than the document-body width.

The existing left-aligned display remains the **default**. Centering is
available only as an explicitly selected `preview-native` CLI option
and does not affect `mdview preview`, raw source paging, or any
remote/browser mode.

## CLI

Use a bounded document-content width and then center the reading
column within the available terminal space:

```bash
mdview preview-native README.md --max-width 100 --center
dotnet run --project Blackboard/Tools/MdView -- preview-native README.md --max-width 100 --center
```

Both options may appear in either order:

```bash
mdview preview-native README.md --center --max-width 100
```

The parser is case-insensitive and rejects repeated or unrecognized
options. `--max-width N` still accepts **40–4096 content columns**.
`--center` without a maximum width is valid but normally has no
visible effect: the automatic reading column already consumes the
full physical terminal width.

The preview header indicates `centered` whenever this opt-in setting
is selected, as well as `max N columns` when applicable.

## Width and vertical geometry

`MdViewMixedContentAlignment.LeftGutter` calculates an integer left
gutter from the **physical** terminal width and the already shared
`MdViewMixedContentWidth.EffectiveTerminalWidth`:

```text
effectiveWidth = min(physicalWidth, maxContentColumns + 2)
leftGutter = floor((physicalWidth - effectiveWidth) / 2)
```

For automatic width (`maxContentColumns = 0`), the effective width is
the physical terminal width, so the gutter is zero.

Examples:

| Terminal width | Content width cap | Centered gutter |
| ---: | ---: | ---: |
| 200 | 100 | 49 |
| 200 | 40 | 79 |
| 72 | 100 | 0 |
| 200 | automatic | 0 |

This positions the **whole reading column**, not each individual
short line independently. Lines remain left-aligned relative to one
another within the column, preserving lists, hanging indents and
line-to-line visual alignment.

### Ordinary Markdown

Every previously measured `MdViewTerminalDisplayRow` receives the
same literal left margin **before** its styled spans. Inline styles
still come from the established
`MdViewTerminalPageRenderer.ResolveStyle`; Markdown remains
literal-safe and does not pass through Spectre markup evaluation.

### Native Spectre tables

`MdViewMixedContentAlignment.IndentTable` wraps a validated native
table in `Spectre.Console.Padder` with horizontal left padding
and **zero top, right and bottom padding**. Its original explicit
width remains unchanged.

Before the first byte of a mixed page is written,
`MdViewMixedVisualPageRenderer` measures the padded table through
the same `MdViewSpectreTableHeight.CountRows` segment-based helper
used to plan the native table. It must occupy **exactly the same
number of visual rows** as the unpadded indexed widget. If
horizontal positioning changes the actual native widget height,
rendering rejects the page rather than silently desynchronizing
`Page X of Y`.

No fake physical terminal width is supplied to Spectre.
No wrapping, header repetition or underlying Markdown source
positions are recalculated solely for centering.

## Disk index and navigation

The shared `MdViewMixedVisualPageIndex` remains **56 bytes per
page record**. Centering does not add or remove source display rows,
so page boundaries and source-line binary-search jumps are
unchanged. The selected `--max-width` still controls the actual
source wrapping and native table widths at **index creation**.

`MdViewMixedVisualIndexedRenderer.RenderPageAsync` accepts a
trailing optional `centerContent` argument. It reconstructs
the original source-aware page, validates source and geometry,
then passes the requested horizontal positioning to the
shared page renderer.

The centered layout also respects physical terminal resize
detection. The existing interactive viewer continues rebuilding
its disk index when terminal dimensions change, even when the
capped reading width stays constant.

## Limits and compatibility

- Default display: left-aligned, unchanged.
- Width cap: still optional; without spare horizontal space,
  centering adds no padding.
- Long native tables: retain conservative compact fallback
  across all pages.
- Tiny terminals: retain the established safe-mode guard.
- Source documents: never modified.
- Index format: unchanged.
- Extra dependency: none; the renderer reuses Spectre.Console's
  existing `Padder` widget.

This increment does **not** introduce right-aligned prose,
justify text, or individually centered headings. All rows share
one constant reading-column gutter.

## Tests

`MdViewMixedContentAlignmentTests` covers:

- Exact gutter calculations for full, capped and narrower
  physical terminal widths.
- No extra native table height after adding a horizontal gutter.
- Centered text and rounded table borders sharing one left gutter.
- Source bytes unchanged and square-bracketed markup displayed
  as literal Markdown data.
- Indexed page reconstruction and canonical source-line jumps
  producing the same page record with centered/left output.
- Identical output when centering is enabled without a width cap.
- Case-insensitive `--center` parsing and both option orders.
- Repeated/unknown options rejected.
- Opt-in UI status and unchanged terminal profile.

**Fresh focused/full .NET verification is pending on the
maintainer's machine.** The assistant environment does not have
the .NET SDK, so successful compilation/tests are not claimed
until the new code is tested.

## Verification

```bash
git pull
dotnet test Blackboard/Tools/MdView.Tests/MdView.Tests.csproj
dotnet test Toolbox.sln
dotnet run --project Blackboard/Tools/MdView -- preview-native README.md --max-width 100 --center
```

For comparison, run the same command without `--center`. Verify
that page counts, physical source line ranges and native table
rows agree; only the horizontal placement should differ.

See [content width](../Mixed-Content-Width/en.md),
[mixed native preview](../Mixed-Native-Preview/en.md), and
[implementation status](../../IMPLEMENTATION_STATUS.md).
