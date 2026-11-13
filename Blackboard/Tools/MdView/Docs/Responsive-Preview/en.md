# Responsive Markdown preview: intelligent wrapping and visual pages

## Status

**Implemented in the experimental local** `mdview preview PATH` as of
2026-11-11. The existing one-shot Markdown renderer, raw source pager,
and GitHub/HTTPS browsing modes are unchanged.

```bash
dotnet run --project Blackboard/Tools/MdView -- preview README.md
```

This is a source-preserving, viewport-aware visual page reader built on
`Utf8ValidatedSnapshot`, `MdViewContainerPageCursor`, and a new temporary
disk-backed `MdViewTerminalPageIndex`.

## Terminal layout

The reader queries `IAnsiConsole.Profile.Width` and `Height` before
**each redraw**. It does not attempt to interrupt an active menu when
the window is resized. At the next render after a width/height change:

1. The previous viewport-dependent temporary page index is disposed.
2. The source-bound current physical line is kept, but its old
   within-line wrapping offset is reset because it is no longer valid.
3. A new disk page index is produced for the current width/height.
4. Back-page history for the previous dimensions is cleared.

The page reserves **16 terminal rows** for a three-line technical
header, one blank separator, a footer, warnings and the navigation
prompt. The default `linesPerPage = 0` now uses the **available
terminal height automatically**, up to a hard safety limit of 128
visual content rows. Supplying an explicit `linesPerPage` value of
1–128 retains that smaller upper limit for callers and tests.
The available content width reserves two terminal columns.

For very small terminals there may not be enough room for all prompt
options plus content. The display still offers at least one content
row; fully responsive single-row compact menus are future work.

## Header and footer

The three informational header rows are now **grey** and followed by
**one empty row** separating them from the document. The grey footer
appears after the content and contains:

```text
Page 5 of 42 | Source lines 121–132 of 410
```

Page numbers are 1-based. The total `Y` comes from a complete
bounded-memory scan of the validated source for the **current**
terminal geometry; it is not guessed from the number of source lines.
When a user manually jumps to a source line or an indexed heading,
the reader preserves the exact requested line and marks the footer
with `(jump view)` if that start is not a canonical page boundary.

## Human-friendly prose wrapping

`MdViewTerminalWordWrap` wraps source lines to the current display
width, preferring:

1. A complete sentence ending at `.`, `!` or `?`.
2. A clause boundary after a comma, colon or semicolon.
3. A whole-word boundary.
4. Splitting an exceptionally long unbreakable token only when needed.

Sentence/clause breaks are preferred only after at least half of the
available row width to avoid extremely short early lines. The
algorithm understands UTF-16 surrogate pairs and uses an approximate
terminal-cell width model for CJK and emoji characters. It never splits
a surrogate pair or changes the verified file. Internal whitespace
at visual breaks can be normalized for display; source bytes remain
unchanged.

Markdown fragments are still rendered by the established
`MarkdownSpectreRenderer`. For complex multiline lists, blockquotes,
inline links split across visual rows, tabs, combining grapheme
clusters and theme-dependent decoration, the number of **actual**
terminal rows may differ from the deterministic layout model. These
are documented experimental limitations rather than claims of exact
CommonMark visual-layout fidelity.

## Special handling of code

**Fenced code is not soft-wrapped.** Its horizontal whitespace and
indentation may carry syntactic meaning. Long code lines are displayed
as a single clipped terminal row, ending with **`→`** when additional
source text exists beyond the visible width. Below such a page, a
grey note clarifies that this is not a line break:

```text
→ Code continues beyond the visible width (not wrapped).
```

The original code is intact in the validated snapshot and on disk.
For complete unusually long source lines use the existing raw
`mdview source PATH` or a text editor. A future optional horizontal
scroll mode can make concealed code columns inspectable without
altering indentation. Code classification comes from the existing
container scanner, so Markdown-looking text inside fences is still
rendered literally.

## Index and resource budgets

`MdViewTerminalPageIndex` stores fixed-size source-line and
within-line visual-row offsets (12 bytes per page start) in an
automatically deleted temporary file. Its default disk budget is
**64 MiB**. It retains only counts and the viewport geometry in RAM;
it does not materialize every page or the complete Markdown file.
Building the index involves one additional **sequential read** of the
already validated snapshot, and occurs again when the viewport
changes. This tradeoff provides a deterministic **Page X of Y**
without an unbounded in-memory page-boundary list.

`MdViewTerminalPageRenderer` reads container-classified source via
the existing disk event journal and renders only the requested number
of visual rows. Its next position can fall partway through an
individual physical line, enabling lossless forward paging for very
long prose. The existing lazy disk heading/link source-location
picker remains compatible and does not open other files.

If the temporary page index exceeds its disk budget, creation fails
with a clear error rather than silently substituting a false page
count or loading all page boundaries into memory. The temporary file
is deleted on failure or when the preview ends.

## Regression and manual verification

```bash
dotnet test Blackboard/Tools/MdView.Tests/MdView.Tests.csproj
dotnet test Toolbox.sln
```

New tests exercise human-language punctuation, long unbreakable
words, CJK/emoji display widths, no-wrap code markers, output
boundaries across multiple visual pages of one physical line,
geometry-dependent page counts, disk-budget failure, and header/
footer source-line and page displays. Earlier source jump and
fenced-code tests also serve as regression checks.

**New focused and full tests have not been confirmed green** after
these changes. A manual Windows Terminal/Konsole resize smoke test
should verify that the next page uses the new dimensions without
needing to repaint the page that was already on screen.

[Preview documentation](../Experimental-Formatted-Preview/en.md) ·
[Disk navigation index](../Disk-Navigation-Index/en.md) ·
[Implementation status](../../IMPLEMENTATION_STATUS.md).

## Indented code safeguard (2026-11-11)

The initial responsive implementation conservatively classified
fenced code as literal. This now also includes otherwise-unowned
four-space/tab-indented lines, so they are clipped with `→` rather
than soft-wrapped. Explicit list continuation ownership remains
separate and is still treated as prose, as required for Markdown
list semantics. This extension is covered by a targeted regression
test. As in the existing index, ambiguous indentation is treated
conservatively; full CommonMark ownership remains future work.

### Automatic height correction (2026-11-11)

The initial responsive code retained the old default of 24 content
rows, so a very tall terminal still displayed short pages. This
was corrected: the default now fills as much of the viewport as
possible (up to the bounded 128-row scanner/rendering limit), while
an explicit numeric cap remains supported. A dedicated viewport
regression compares 40-row and 80-row terminals and an explicit
two-row limit. New test results remain pending.

## Code overflow notice and Markdown heading test correction (2026-11-12)

The maintainer's first responsive-preview `MdView.Tests` run reported
**174 passed, 2 failed, 0 skipped, 176 total**.

1. `CodeLineClippedWithArrowIsOneVisualRow` expected the literal
   `# Ending` after rendering an ATX heading. The shared Markdown
   renderer correctly strips the heading syntax and displays
   `Ending` as formatted text. The regression now asserts the
   rendered heading, while verifying it is not printed as literal
   Markdown source.
2. `CodeOverflowIsMarkedButCodeSourceRemainsUnchanged` searched
   for a long uninterrupted status sentence inside a **31-column**
   terminal. Terminal wrapping prevented that exact contiguous
   substring from appearing in the test output.

The preview now selects a single-row grey code-clipping notice based
on the available **content** columns:

- 56 or more: `→ Code continues beyond the visible width (not wrapped).`
- 25–55: `→ Code clipped (no wrap).`
- 14–24: `→ Code clipped`
- 8–13: `→ code`

These notices are supplementary; the trailing `→` on the displayed
code line remains the primary indicator. The source code is **not**
soft-wrapped, modified or truncated in the validated snapshot.
A new wide-terminal test ensures the full explanatory note remains
available when it fits.

The post-fix focused and complete solution suites still require
maintainer confirmation; the **174/176** result is the last reported
run before these corrections.

## List, quote and inline-aware rows (2026-11-13)

`MdViewTerminalPageIndex` and `MdViewTerminalPageRenderer`
now obtain their row boundaries from **the same**
`MdViewTerminalLineLayout.Layout` implementation. It strips
recognized Markdown list/quote syntax into stable display markers
and hanging indentation, measures only the **visible** inline
text, and retains link, emphasis and inline-code presentation
across soft wrap/page boundaries.

This replaces the earlier mismatch between raw Markdown source
length and fully rendered link labels/list markers. The renderer
writes each styled `Spectre.Console.Text` span literally and
emits one newline per model row; no wrapped fragment is
independently reparsed as a new Markdown document.

Quoted nested lists, long link labels and very long paragraphs
have focused regression tests, including direct verification
that the next page position agrees with the disk index.
Full CommonMark, tables, ambiguous lazy continuations and
terminal-specific grapheme width remain out of scope.

[Shared visual row design](../Styled-Visual-Layout/en.md).

## Compact table rows and resize (2026-11-13)

Explicitly framed, top-level pipe-table rows now become
numbered vertical cells. The viewport-dependent page
index counts every displayed cell row, so resizing the
terminal recomputes **Page X of Y** from the same compact
layout used by the renderer. Table columns are not loaded
as a full in-memory document; per-line parsing remains
bounded by the verified snapshot's line-size cap.

A table delimiter row becomes a single visual divider;
long link labels inside cells retain their styled display
text without showing the destination. Full cross-row GFM
alignment remains a future milestone.
[Compact table guide](../Compact-Pipe-Tables/en.md).
