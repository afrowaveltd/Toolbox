# Shared visual Markdown layout: lists, quotes and inline links

## Scope and motivation

`mdview preview PATH` now uses a **single line-local display model** for
both the disk-backed visual page index and actual terminal rendering.
Previously `MdViewTerminalPageIndex` counted wrapped **raw Markdown
source**, while `MdViewTerminalPageRenderer` rendered each wrapped
piece through the Markdown parser again. This could miscount visible
rows, repeat list markers, lose inline link styles, and split
container structure at page boundaries.

The new `MdViewTerminalLineLayout` consumes
`MarkdownContainerLine` values from the already validated,
incremental Markdown container scanner. For each physical line it
produces a bounded list of visual rows containing literal styled
segments. **Both the disk index and renderer consume the exact
same row definition**. No whole-document Markdown AST is created
in memory, and the validated original source bytes remain unchanged.

## What readers see

- **Bullet and numbered lists:** the marker is shown on the first
  visual row; wrapped continuation rows use hanging indentation
  rather than repeating the bullet or number. Nested ownership
  follows the conservative scanner's confirmed list depth.
- **Quotes:** an explicit source quote marker is presented as a
  `│ ` prefix. It remains visible on each wrapped continuation
  row, including combined quoted and numbered-list content.
- **Inline links:** only the visible label is displayed; the
  destination string does not consume terminal columns. Link
  styling continues across soft row/page boundaries.
- **Inline emphasis and code:** Markdown syntax delimiters
  are removed from ordinary display text, while semantic styles
  are preserved when spans cross wrapped rows.
- **Images:** image alternative text receives a literal
  `[Image: ...]` label. Neither image nor link destinations
  are automatically opened by the local preview.
- **Fenced and conservatively indented code:** remain literal,
  without soft wrapping. The trailing `→` still indicates a
  clipped right edge; no source code is modified.

A long physical line can span multiple visual pages. Its continuation
position is still represented by the existing source-line and
within-line visual-row pair. The disk page index keeps its
existing 64 MiB default budget, and the renderer processes only
the source lines needed for the current page.

## Consistency contract

```text
validated UTF-8 snapshot
    -> incremental container scanner
    -> MdViewTerminalLineLayout.Layout(line, columns)
        -> page index: count layout rows and record page starts
        -> page renderer: print the same styled layout rows
```

Each visual row is rendered from safe `Spectre.Console.Text`
segments, not interpreted as Spectre markup. One explicit newline
is emitted for every indexed row. This avoids the extra parsing
step that previously changed the number of output rows or
reinterpreted a Markdown-looking continuation as a new list item.

`MdViewTerminalWordWrap.SliceProse` returns UTF-16-safe offsets
into the **already visible text** so long link labels can remain
styled while wrapping. The ordinary word-wrap entry point reuses
these same offsets to keep both wrapping paths consistent.

The line-local algorithm uses a monotonic token cursor instead of
restarting from the beginning of the inline token list for each
wrapped row. Only the current bounded source line and its visual
rows occupy RAM. Styling is not cached for an entire document.

## Deliberate limitations

This is **not yet full CommonMark**. The incremental container
scanner deliberately uses conservative rules for lazy list
continuation, nested/quoted containers and ambiguous indentation.
Multiline inline links, reference-style links, setext headings,
full multi-row GFM table column alignment, and advanced nested
constructs remain outside the new layout contract. Explicit
pipe-framed table rows have a separate compact-cell fallback. Complex grapheme clusters and differences in
terminal Unicode cell widths can still change physical wrapping.
The displayed total page count is exact for our deterministic
line-layout model, not for every possible terminal font.

Fenced code and source snapshots are unaffected, and the verified
GitHub browser's independent 1 MiB file limit is unchanged.

## Verification

```bash
dotnet test Blackboard/Tools/MdView.Tests/MdView.Tests.csproj
dotnet test Toolbox.sln
```

The new regression suite covers list hanging indents, long quoted
paragraphs, quoted numbered lists, link labels and styling across
wraps, strong/inline-code spans, literal fenced source, malformed
markup, and agreement between consecutive disk-indexed page
boundaries and the renderer's next resume position.

**These source changes have not been compiled or test-executed in
the current tool environment.** The previous maintainer-confirmed
green checkpoint predates this increment; request a fresh test
run before claiming the new version green.

[Responsive preview](../Responsive-Preview/en.md) ·
[Disk navigation index](../Disk-Navigation-Index/en.md) ·
[Implementation status](../../IMPLEMENTATION_STATUS.md).

## Conservative compact pipe-table cells (2026-11-13)

The shared `MdViewTerminalLineLayout` now delegates
clearly framed, top-level pipe-table rows to the
`MdViewTerminalPipeTableLayout` component. It renders
each source row as a sequence of numbered vertical cells,
using the same styled visible-text wrapping as prose.
The disk page index and terminal renderer therefore agree
on the number and positions of visual rows.

Only rows enclosed by explicit, unescaped `|` delimiters,
with 2–32 columns, are recognized. Pipes inside backtick
code spans and escaped pipes do not create new cells.
A conventional alignment-marker row becomes one
divider. Ambiguous/unframed source and code blocks
remain handled by their previous code paths.

This is a compact low-RAM reading mode rather than a
column-width-aware GFM table engine; header inference,
nested table ownership and proper column alignment remain
separate future work. [Detailed guide](../Compact-Pipe-Tables/en.md).
