# Responsive Markdown preview: intelligent wrapping and visual pages

## Status

**Implemented in the experimental local** \`mdview preview PATH\` as of
2026-11-11. The existing one-shot Markdown renderer, raw source pager,
and GitHub/HTTPS browsing modes are unchanged.

\`\`\`bash
dotnet run --project Blackboard/Tools/MdView -- preview README.md
\`\`\`

This is a source-preserving, viewport-aware visual page reader built on
\`Utf8ValidatedSnapshot\`, \`MdViewContainerPageCursor\`, and a new temporary
disk-backed \`MdViewTerminalPageIndex\`.

## Terminal layout

The reader queries \`IAnsiConsole.Profile.Width\` and \`Height\` before
**each redraw**. It does not attempt to interrupt an active menu when
the window is resized. At the next render after a width/height change:

1. The previous viewport-dependent temporary page index is disposed.
2. The source-bound current physical line is kept, but its old
   within-line wrapping offset is reset because it is no longer valid.
3. A new disk page index is produced for the current width/height.
4. Back-page history for the previous dimensions is cleared.

The page reserves **16 terminal rows** for a three-line technical
header, one blank separator, a footer, warnings and the navigation
prompt. Display height is bounded by the requested \`linesPerPage\`
upper limit (default 24) as well as the remaining viewport rows.
The available content width reserves two terminal columns.

For very small terminals there may not be enough room for all prompt
options plus content. The display still offers at least one content
row; fully responsive single-row compact menus are future work.

## Header and footer

The three informational header rows are now **grey** and followed by
**one empty row** separating them from the document. The grey footer
appears after the content and contains:

\`\`\`text
Page 5 of 42 | Source lines 121–132 of 410
\`\`\`

Page numbers are 1-based. The total \`Y\` comes from a complete
bounded-memory scan of the validated source for the **current**
terminal geometry; it is not guessed from the number of source lines.
When a user manually jumps to a source line or an indexed heading,
the reader preserves the exact requested line and marks the footer
with \`(jump view)\` if that start is not a canonical page boundary.

## Human-friendly prose wrapping

\`MdViewTerminalWordWrap\` wraps source lines to the current display
width, preferring:

1. A complete sentence ending at \`.\`, \`!\` or \`?\`.
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
\`MarkdownSpectreRenderer\`. For complex multiline lists, blockquotes,
inline links split across visual rows, tabs, combining grapheme
clusters and theme-dependent decoration, the number of **actual**
terminal rows may differ from the deterministic layout model. These
are documented experimental limitations rather than claims of exact
CommonMark visual-layout fidelity.

## Special handling of code

**Fenced code is not soft-wrapped.** Its horizontal whitespace and
indentation may carry syntactic meaning. Long code lines are displayed
as a single clipped terminal row, ending with **\`→\`** when additional
source text exists beyond the visible width. Below such a page, a
grey note clarifies that this is not a line break:

\`\`\`text
→ Code continues beyond the visible width (not wrapped).
\`\`\`

The original code is intact in the validated snapshot and on disk.
For complete unusually long source lines use the existing raw
\`mdview source PATH\` or a text editor. A future optional horizontal
scroll mode can make concealed code columns inspectable without
altering indentation. Code classification comes from the existing
container scanner, so Markdown-looking text inside fences is still
rendered literally.

## Index and resource budgets

\`MdViewTerminalPageIndex\` stores fixed-size source-line and
within-line visual-row offsets (12 bytes per page start) in an
automatically deleted temporary file. Its default disk budget is
**64 MiB**. It retains only counts and the viewport geometry in RAM;
it does not materialize every page or the complete Markdown file.
Building the index involves one additional **sequential read** of the
already validated snapshot, and occurs again when the viewport
changes. This tradeoff provides a deterministic **Page X of Y**
without an unbounded in-memory page-boundary list.

\`MdViewTerminalPageRenderer\` reads container-classified source via
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

\`\`\`bash
dotnet test Blackboard/Tools/MdView.Tests/MdView.Tests.csproj
dotnet test Toolbox.sln
\`\`\`

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
