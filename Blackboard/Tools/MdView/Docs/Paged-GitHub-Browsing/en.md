# GitHub Markdown paging: first bounded interactive milestone

**Status:** opt-in `mdview browse-git` and no-argument GitHub selection
now display **source-line pages** for long Markdown documents, while short
documents retain their previous full-render flow. This is the first
interactive step toward a page-based, low-memory Markdown reader.

## Try it

```bash
dotnet run --project Blackboard/Tools/MdView -- browse-git afrowaveltd MemView
```

The same behavior is available by running `mdview` without arguments
and choosing **GitHub repository (browse Markdown links)**.

After the README is loaded, long documents present at most **24 physical
source lines** per page, with an additional limit of **16,384 UTF-16
characters** sent to the renderer per page. The screen displays a
source-line range, for example:

```text
GitHub: afrowaveltd/MemView @ main / README.md
Navigable Markdown links: 12
Page: source lines 1–24 / 200
```

The terminal menu exposes:

- **Next page** and **Previous page**, when the respective lines exist.
- **Go to heading**: choose one of up to 512 indexed headings; the
  selected heading becomes the first source line of the next display.
- **Go to source line**: enter an absolute, 1-based physical source line.
- **Follow a link**: the familiar parser-backed selection of all
  supported relative Markdown links from the current document.
- **Back / Forward**: history of visited Markdown documents and
  fragments; distinct from moving between pages of one document.
- **Exit browser / Escape**.

For **24 or fewer source lines**, the existing full-document rendering
and menu order remain unchanged.

## Implementation

`MdViewSmallMarkdownPageIndex` uses the shared
`ExchangeHouse.Markdown.MarkdownParser` to classify a complete
**already verified and size-limited** Markdown document. It maps
source-character offsets to physical lines with binary search.
Individual pages are rendered from bounded source fragments only.

When a page begins inside fenced code, the parser's full-document
classification is consulted. Fence openers, code lines and fence closers
are output as literal `Spectre.Console.Text`, not reparsed as
potential headings, lists, links or executable terminal markup.
This prevents cross-page code from turning into Markdown elements.
Other prose segments use the existing Markdown renderer; complex
list/quote formatting across page boundaries is still experimental.

The active GitHub browser keeps **only the current** document string,
page index, link catalog and heading list. It reloads/rebuilds them
only when the verified content SHA-256 or the repository file changes,
**not** on every Next/Previous page. Network access is still handled
exclusively by `GitHubRepositoryFileSource`, with exact-ref file
identity, verified disk cache, and explicit Stale/offline warnings.

An exceptionally long physical line is **shortened for display**, not
removed from the verified cached document. The UI warns the reader and
continues from the following source line on the next page. The cutoff
does not split a UTF-16 surrogate pair. To inspect complete unusual
lines in a local copy, use the dedicated raw source pager.

## Limits and next milestones

The current GitHub link browser still requires a fully verified
Markdown file of **at most 1 MiB**. Within this cap, whole-document
parser/index memory is finite. **This is not a streaming reader for
1 GiB files**, and the page count is expressed in physical source
lines rather than wrapped visual terminal rows. A very long visible
line can still occupy more than one terminal row.

The first page renderer does not preserve full multi-line TextMate
highlight state when a code fence crosses page boundaries; code is
displayed safely as literal text in that case. Source-page fragments
can also lose some nested-list/blockquote context. Neither limitation
should be silently described as resolved.

For the large-document path, reuse the existing
`Utf8ValidatedSnapshot`, `Utf8DiskLineIndex`,
`MdViewDiskCheckpointIndex` and `MdViewCheckpointPageReader`
rather than raising this browser's 1 MiB RAM limit. Future work
includes a disk-backed heading/link index, source-stable syntax
highlight sessions and accurate visual-row pagination.

## Verification

```bash
dotnet test Blackboard/Tools/MdView.Tests/MdView.Tests.csproj
dotnet test Toolbox.sln
```

New regression cases exercise page budgets, C fenced code across
boundaries, Unicode source offsets and surrogate-safe truncation,
Next/Previous visibility, heading and source-line jumps, and
link navigation while paging. Existing short-document virtual-terminal
flows should remain unchanged.

**New test execution is pending maintainer confirmation.**

[MdView README](../../README.md) ·
[Link navigation](../Link-Navigation/en.md) ·
[Experimental local preview](../Experimental-Formatted-Preview/en.md) ·
[Implementation status](../../IMPLEMENTATION_STATUS.md).
