# mdview CLI tests

xUnit tests for command grammar and the independent Spectre.Console interactive picker. The picker tests use `TestConsole` with simulated keyboard input and temporary local Markdown files. No real network is used.

```bash
dotnet test Blackboard/Tools/MdView.Tests/MdView.Tests.csproj
```

The original ten command-grammar tests passed in the user-supplied 6,443-green Windows baseline. Eleven new interactive picker cases cover Escape/back behavior, picking real Markdown, GitHub and URL entry, dangerous URL rejection, terminal capability and filename validation. The **new** tests are not yet executed/CI confirmed. Real-terminal visual verification and full command-to-render integration coverage remain pending.

[CLI implementation](../MdView/README.md).

## Owner-only repository chooser tests (2026-11-04)

Added command grammar cases for bare `OWNER`, `git OWNER`, `github:OWNER`, and explicit `local` behavior. The interactive source menu now also has an owner-only input test. New `MdViewGitHubRepositoryPickerTests` use `TestConsole` and fake HTTP to verify selecting repository names, filtering by description, cancelling without fetching any README, literal markup-safe labels, non-interactive rejection and 404 errors. These new tests await execution; the earlier green Windows baseline predates the change.

## Source pager tests (2026-11-04)

Added `MdViewSourcePagerTests` to verify the explicit raw UTF-8 source pager: Escape/exit, read-only literal output, next/back restoring the actual page start, direct jump to physical line, invalid jump feedback, refusal of noninteractive terminals, and rejection of unsafe source before any content is displayed. Command parsing also tests `source PATH`. These new cases await execution; the last user-confirmed full Windows baseline predates them.

## Document-end navigation and repeatable sessions (2026-11-04)

Added `MdViewDocumentCompletionPromptTests` (5 new cases) and `MdViewSessionTests` (8 cases), using the virtual Spectre `TestConsole`. They cover Enter-to-menu, Escape-to-exit, ignoring reserved search/pager keys, noninteractive completion without input, direct-command → menu → second document flow, cancelling the returned menu, preserving errors, suppressing prompts for unread/cancelled sources, and ensuring the raw source pager is not prompted twice. The session tests use in-memory delegate fakes and do not call the network or filesystem.

The last maintainer-confirmed full Windows baseline was green *before* this feature. These 13 new cases have **not yet been executed** in this environment.

## Disk-backed Markdown navigation (2026-11-11)

The new `MdViewDiskNavigationIndexTests`,
`MdViewDiskNavigationPickerTests` and additions to
`MdViewFormattedPreviewPagerTests` verify source-aware headings
and inline link positions in large validated Markdown snapshots.
Coverage includes fenced/indented/inline code, images, escaped links,
source UTF-16 offsets across mixed line endings, distant disk result
pages, disk budget failure recovery, long Unicode labels,
selector cancellation and navigation into local preview pages.
The index is created lazily by `mdview preview PATH` only when the
reader chooses Find heading / Find link location; the existing
GitHub browsing and normal Markdown rendering test paths remain
unchanged.

New tests are committed but **not yet maintainer-confirmed green**.
Run `dotnet test Blackboard/Tools/MdView.Tests/MdView.Tests.csproj`
and `dotnet test Toolbox.sln` before the next milestone.

## Responsive preview regressions (2026-11-11)

New `MdViewTerminalWordWrapTests` cover natural language break
priority, long words, Unicode cell widths, emoji safety and
code lines that are clipped rather than wrapped.
`MdViewTerminalPageIndexTests` cover viewport-specific page
counts, resize re-indexing, source positions within a single
very long line, code overflow, disk budget failure and bounds.
New `MdViewFormattedPreviewPagerTests` cases verify a visual
Page X of Y footer and code continuation messages; the
pre-existing source-line jump, fence and navigation scenarios
remain in place.

This source increment has **not yet been verified by a fresh
maintainer test run**. Run the targeted MdView tests and complete
Toolbox solution suite before considering this checkpoint green.

## Shared layout and renderer parity tests (2026-11-13)

Added nine `MdViewTerminalLineLayoutTests` for wrapped list
bullets/hanging indents, repeated quote gutters, long link
labels with retained link roles, emphasis and inline-code
styles, quoted numbered lists, literal code fences,
escaped/malformed markup, and disk-page-index/renderer
resume-position parity across visual pages.

The production row model is shared by the disk index and
renderer. `MdViewTerminalWordWrap.SliceProse` exposes the
display-text source positions of soft rows, and the ordinary
wrap method reuses it. The per-line inline token cursor is
monotonic to avoid quadratic behavior on a single long
source line containing many Markdown tokens.

**Fresh focused and full-solution tests pending.** The last
maintainer-confirmed all-green checkpoint predates this
increment.

## Compact table regression tests (2026-11-13)

Nine `MdViewTerminalPipeTableLayoutTests` cover
numbered compact cells, separator rows, escaped and
inline-code pipes, visible link-label styling, ambiguous
fallback, fenced-code preservation, eight-column terminal
widths, 32-column input bounds, and disk index/renderer
agreement across page boundaries.

Fresh focused/full-solution tests are **pending maintainer
verification**; retain the prior all-green state as the
baseline before these table changes.

## Native Spectre table widget tests (2026-11-13)

Added eight `MdViewSpectreTableViewerTests` verifying bounded
source-based native Spectre table chunks: framed-header and
separator recognition, rounded Unicode border in a wide
terminal, transposed field paging in a narrow terminal,
multi-chunk data navigation, safe literal rendering of
bracketed markup, cell-size clipping with unchanged source,
fenced-code rejection and noninteractive/invalid-source
failure behavior.

Three `MdViewFormattedPreviewPagerTests` cover native
table modal round-trip without losing the normal source page,
hiding the menu for unconfirmed pipe text, and retaining
compact table preview below the 28-column native threshold.

These newly added tests **await a fresh .NET run**; the last
maintainer-verified green checkpoint precedes this feature.

### Native table test build diagnostics (2026-11-14)

The first full-solution run after adding the native Spectre table
viewer was blocked by two `CS0236` errors in the fixture initializer:
its `Path` property shadowed `System.IO.Path`. The initializer
now qualifies the framework type. Also fixed `xUnit2013` in the
disk navigation index tests by asserting `Assert.Single(links)`.
Both changes are test-only, commit IDs
`ef6ee546b5513ef481cdd12ca4eb124a49531d14` and
`35e34fdc07bdd8c9228d8d3e3d911a9881d37624`.

The new native viewer tests have **not yet been run to completion**.
Run focused MdView tests and the complete `Toolbox.sln` suite.

### Native table chunk status regression (2026-11-14)

The first fully compiling native Spectre viewer test suite reported
**206 passed / 1 failed / 207 total**. The only failure was a test
case-sensitivity mismatch: the status text prints lowercase
`rows visited: 8`, but the test expected capitalized
`Rows visited: 8`. The corrected test also explicitly checks
`Data rows shown: 4` for the 24-row terminal chunk.

Commit: `3088f73e1e64b134ee9ed93afba1102c25754f43`.
No runtime table change was necessary. Run the focused and complete
solution suites again; a post-fix green result is not yet confirmed.

### Native Spectre height measurement regressions (2026-11-14)

The `MdViewSpectreTableHeightTests` cover native bordered
widget height measured from real rendering segments, including
long-cell wrapping at narrow vs. wide viewport widths, early
measurement budgets and invalid inputs. The viewer integration
`WrappedNativeTableRowsFitMeasuredTerminalHeight` checks that
a chunk containing wrapped cells fits the current terminal-row
budget without losing the remaining table.

Measurement does not write to `TestConsole`, and source rows
continue to be drawn from the existing validated disk-backed
cursor. Existing full-width, narrow-transposed and six-row-cap
tests still define the baseline.

**Fresh focused and full-solution tests are pending** after
this increment; previous native table tests were confirmed green
by the maintainer before these changes.

Additional viewer regressions verify native modal rejection
below 12 terminal rows and safe fallback when an overly long
header would exceed the available visual-row budget even
without data rows.

## Source-bound table block regression tests (2026-11-14)

Added six `MdViewSpectreTableBlockReaderTests` verifying a valid
header at a visible-page boundary with its delimiter on the next
page; six-row chunk continuation with no missing/duplicate data;
termination at the first non-table source line; escaped/inline-code
pipes and intact on-disk data; fenced, quoted and mismatched
rejection; argument and cancellation guards.

Existing native inspector discovery, header validation and data
row recognition now call the same block reader. The normal compact
page index and renderer have **not** been switched to native
rendered widgets. **These new tests are not yet executed here.**
Run focused MdView.Tests and the full Toolbox solution on Debian
or Windows, then smoke-test `mdview preview README.md`.

### Tiny-terminal native header regression correction (2026-11-14)

The Debian-focused MdView run produced **219 passed / 1 failed /
220 total**. The single failure was
`OversizedHeaderAloneDoesNotOverflowTinyNativeModal`, which supplied
no input although an unbroken 120-character header was clipped by
Spectre and the resulting five-row table could legitimately open.
The test now supplies Escape and checks safe modal navigation.
A separate test uses **word-wrappable long headings** to verify
that a genuinely over-height header returns to compact Markdown
without opening the prompt. Production height measurement is unchanged.

Commit: `a5f511165d1fafe0a94121cb23c3fb0ba9af4c4c`.
Fresh focused/full-solution runs are **pending** after this correction.

## Native table page-planner tests (2026-11-14)

Added six `MdViewNativeTablePagePlannerTests` for exact source resume
across three 6/6/2 chunks, actual Spectre-height-constrained
one-row fragments, compact narrow-terminal fallback with no source
consumption, header/first-row overflow, literal markup and unchanged
source, and validation/cancellation.

The inspector now shares an extracted native table factory with the
page planner; its existing behavior is intended to remain unchanged.
`MdViewTerminalPageIndex` and its default compact renderer are not
modified in this increment.

The maintainer confirmed the previous focused/full checkpoint green
after the clipped-versus-wrapped header regression correction. **New
tests for this planner have not yet been run with a .NET SDK here.**
[Detailed guide](../MdView/Docs/Native-Table-Page-Planner/en.md).

## Native table disk index and renderer regressions (2026-11-15)

Twelve new `MdViewNativeTableFragmentIndexTests` cover fixed-size,
disk-backed fragment lookup for 14 data rows; 6/6/2 exact continuation;
index-to-renderer parity for a selected middle fragment; header-only
tables; entire-table narrow/over-height compact fallback including
a late oversized row after earlier fragments; measured
one-row fragments with repeated headings; disk-budget failure with
unchanged validated source; invalid arguments; rejected stale/forged
headers and a different source cursor; terminal resize; cancellation
and disposed-index safety.

The table-only index and renderer are independent of the existing
`MdViewTerminalPageIndex` and `mdview preview PATH` UI. The earlier
measured table planner and native modal tests must remain green.
**Fresh focused and full-solution .NET tests are pending for these
new files.** [Design notes](../MdView/Docs/Native-Table-Fragment-Index/en.md).

## Mixed visual Markdown/native table page regressions (2026-11-15)

Added twelve `MdViewMixedVisualPageTests` covering native tables between
ordinary paragraphs; moving a whole table before a page break;
14-row compact fallback across multiple pages; a compact header and
delimiter split between pages; narrow-terminal compact fallback;
over-height long-cell tables; ordinary Markdown page-start parity
with the established disk index and exact text-only renderer-output
parity; fenced and indented literal code
pipe text; pre-output resize
rejection; and invalid source positions, budgets and cancellation.

`MdViewMixedVisualPagePlanner` and its matching renderer are
**standalone experimental components**. They do not yet change the
default local Markdown preview or the independent native table
inspector. The maintainer's preceding all-green report is the
baseline; **these ten additional tests still require a fresh .NET
run**. [Design contract](../MdView/Docs/Mixed-Visual-Page-Planning/en.md).

## Whole-document mixed visual page index (2026-11-15)

Eleven `MdViewMixedVisualPageIndexTests` verify a real small native
Spectre table inside an indexed Markdown page; complete compact
fallback across many pages; within-line wrap-row binary search;
compact table header/delimiter state across page boundaries;
separate native tables on different pages; empty source and
exactly filled final pages; stale terminal/source rejection
before writing; disk-budget failure with unchanged source;
invalid positions, disposed index and cancellation.

The new `MdViewMixedVisualPageIndex` stores fixed **56-byte**
temporary disk records and `MdViewMixedVisualIndexedRenderer`
rebuilds/validates a single canonical page against the same
source-aware mixed planner. This stage does **not** change the
default compact Markdown reader UI. **Focused and full-solution
tests are pending** for these new files.
[Contract](../MdView/Docs/Mixed-Visual-Page-Index/en.md).

## Opt-in mixed native preview and xUnit2020 cleanup (2026-11-15)

The maintainer reports the preceding MdView checkpoint:
**263 passed, zero failed, zero skipped**, with one analyzer
warning in `MdViewMixedVisualPageTests`. Replaced
`Assert.True(false, message)` with `Assert.Fail(message)`
to remove `xUnit2020` without changing the failure condition.

Added one `MdViewCommandTests` case to verify
`preview-native PATH` parsing is explicit and case-insensitive
while `preview PATH` stays compact.

Added **13 `MdViewMixedPreviewPagerTests`** covering inline
native table rendering between prose, Next/Previous indexed
navigation, canonical source-line jumps into native table data,
disk-indexed heading and link selection, invalid source jump,
long-table and narrow-terminal compact fallback, empty source,
undersized/noninteractive terminals, row control-area reserves,
invalid arguments and unsafe UTF-8 content.

The new UI and tests are committed but **fresh focused/full .NET
verification is pending**. Do not mark this increment green
until the maintainer reports the results.
[Mixed native preview guide](../MdView/Docs/Mixed-Native-Preview/en.md).
