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

## Entire mixed-preview screen shares one reading gutter (2026-11-15)

The first centered-column iteration repositioned only the source
document. Real terminal screenshots revealed that the file header,
`Page X of Y`, navigation menu and selected arrow remained at the
physical terminal's left edge. The **entire viewer now shares one
horizontal starting column** when `--center` is selected: document,
header, status, navigation, feedback, clipped-code notice and
disk-indexed heading/link picker.

The initial `MdViewIndentedPromptConsole` forwarding adapter did
**not** position Spectre's live selection menu: it wrapped the initial
`IAnsiConsole.Write` argument, but the native `SelectionPrompt`
injected its actual live widget later via
`ListPromptRenderHook` and `RenderPipeline`.
This adapter has been deleted.

The corrected `MdViewCenteredPromptRenderHook` implements
`IRenderHook`. It is attached in a short-lived
`RenderHookScope` **before** starting a selection prompt.
When Spectre's own live prompt hook injects its renderable, the
MdView hook applies horizontal-only `Padder` positioning to
the **actual live widget**, including its title, arrow and choices.
The effect is equivalent to a borderless, centered table container,
without changing the number of display rows. Cursor-moving and
erase `ControlCode` renderables pass through unchanged, retaining
native arrow keys, Enter, Escape and live refresh behavior.

The same scoped helper is used by the main navigation menu and
the disk-indexed heading and link pickers. Result labels are
clipped to the reading width where necessary. The hook is detached
immediately after the prompt ends, so normal document rendering
never receives a second margin.

Free-text `TextPrompt` input is deliberately **not** wrapped by
this adapter. It writes incrementally, so padding every output call
could insert spaces between typed characters. Its title instead
receives one literal gutter prefix on the original console, keeping
typed input natural. The header, file information, page status,
feedback, empty-page message and missing-index messages share the
same starting column. Text-only chrome is clipped within the
selected content-column width.

The source document is **not** padded again: its existing indexed
renderer already positions every Markdown row and native table.
Only the surrounding interface receives the additional alignment.
The disk page index, its 56-byte records and source-line jumps are
unchanged. When the terminal is no wider than the reading column,
the gutter remains zero. Terminal resize continues using the
existing re-indexing logic.

Nine `MdViewMixedScreenAlignmentTests` check the matching
header/body/footer/menu edges, selected menu indicator, Next/Previous
navigation, source-line input, heading and link pickers, empty
search results, empty pages and unchanged default alignment.
Fresh focused and full .NET tests are required before this
increment is called green.

## Native live-prompt correction and regression verification (2026-11-15)

The maintainer reported **333 tests total: 325 passing, 8 failing**
after the first whole-screen centering attempt. A real Konsole
visual test showed the bottom selection menu still starting at
column zero while the header and document were correctly centered.

The failure was traced to Spectre's live `ListPromptRenderHook`,
which emits the actual menu after a forwarding console's
`Write` method runs. The incorrect console proxy was replaced
with a scoped **render-pipeline** hook, described above.

The alignment test suite now sets
`TestConsole.EmitAnsiSequences = true` so Spectre's live
render pipeline is actually used, and strips ANSI control
sequences before asserting *visible* indentation. It also
tests the hook's exact gutter, no additional vertical rows,
and automatic scope removal after a selection. This avoids
the previous false assumptions about a default TestConsole's
captured live-prompt output.

This checkpoint **has not yet been recompiled or tested in the
assistant's environment**, because .NET SDK is unavailable.
The next maintainer run should validate both the previously failing
cases and normal interactive Konsole behavior.

## Visual verification and internal-test access (2026-11-15)

The maintainer's next real Konsole screenshot confirms the
**complete centered layout**, including the bottom live Spectre
navigation list. This visually validates the scoped render-hook
approach, independently of the automated tests.

The ensuing .NET 10 build succeeded for the production MdView
executable but encountered three `CS0122` compiler errors in
the screen-alignment tests. The tests intentionally exercise
the `internal` render hook and prompt helper. Rather than
make these implementation-only types part of the public API,
`MdView/Properties/AssemblyInfo.cs` now declares
`InternalsVisibleTo("MdView.Tests")`, following the same
pattern already present in WhenItFails.

The compiler access correction is committed; a fresh
focused/full test run is still required.

## TextPrompt leading whitespace and empty-menu selection (2026-11-15)

After the live-menu render fix, the maintainer's .NET 10 build and
test run completed compilation and reported **334 passing out of
336**, with just two centered-screen regressions failing.

The `CenteredSourceLinePromptIndentsTitleButNotEachTypedCharacter`
failure exposed a **real text-entry alignment issue**:
Spectre.Console's `TextPrompt.WritePrompt` trims the prompt markup
with `Trim()`. Supplying `new string(' ', leftGutter) + title`
therefore does not preserve the left margin. The corrected MdView
pager writes the margin as a separate literal `Text` renderable
on the current line **before** invoking the native text prompt.
It leaves the prompt title unindented and preserves native
incremental typing without inserting a gutter before each digit.

The `CenteredEmptyDocumentAndExitAreInsideReadingColumn` failure
was an incorrect assertion: the only selection choice on an empty
document is already highlighted, so Spectre displays
`> Exit mixed preview`, not an unmarked item. The regression
now checks the full selected choice with its reading-column gutter.

These were minimal fixes. No source snapshot, mixed page index,
56-byte disk record, native table planner or other viewer mode
was modified. The new code and tests require a fresh maintainer
test run before being considered verified.
