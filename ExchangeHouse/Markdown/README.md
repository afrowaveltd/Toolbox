# Afrowave.Toolbox.ExchangeHouse.Markdown

Small, UI-neutral .NET 10 Markdown parsing foundation for the ExchangeHouse family.

**Status: initial source-preserving block and conservative inline scanner, with a lossless unchanged-document writer. Not full CommonMark, an editable AST, or a Markdown converter. Not published to NuGet.**

## Current API

```csharp
using Afrowave.Toolbox.ExchangeHouse.Markdown;

var document = MarkdownParser.Parse("# Hello\r\n- item\r\n");
var heading = document.Blocks[0]; // Heading, level 1, text "Hello"
var inline = document.Blocks[0].Inlines; // source-aware inline nodes
var unchanged = MarkdownWriter.Write(document);
```

- Recognizes ATX headings (1-6), simple list markers, blank lines, ordinary lines, fenced code delimiters and fenced code contents.
- Retains exact original line text, offsets, whitespace and CRLF/LF/CR terminators.
- Does not evaluate or execute Markdown, links or raw HTML; requires no UI, filesystem or networking.
- Adds conservative inline nodes for simple emphasis, strong text, code spans, links and inline images. Nodes retain raw source spans; link/image destinations are preserved exactly along with optional titles. Fenced code remains uninterpreted.
- Keeps physical lines separate: advanced nesting, multiline paragraphs, reference-style links/images, multiline titles, tables and full CommonMark conformance are future increments.
- Writing currently reconstructs the unchanged document; **no source-transforming writer or converters are claimed**.

Documentation: [Inline parsing](Docs/Inline-Parsing/en.md), [Parser and writer](Docs/Parser-and-Writer/en.md), [ExchangeHouse architecture](../Docs/Architecture/en.md) and [implementation status](IMPLEMENTATION_STATUS.md).

## Testing

```bash
dotnet test ExchangeHouse/Markdown.Tests/Afrowave.Toolbox.ExchangeHouse.Markdown.Tests.csproj
```

NuGet prerelease publication is intended once a useful standalone API, test coverage and independent packed-consumer restore are verified. The presence of a source version alone is not evidence of publication.

## Inline images and richer link destinations (2026-10-29)

The conservative scanner distinguishes images from links, retains their raw paths and optional titles, and reports exact destination source offsets. Simple angle-bracket destinations, empty destinations and basic titles are supported. No reference-style links, file access, editable writer or full CommonMark claim. See [Images and links](Docs/Images-and-Links/en.md).

## Nested and ordered lists (2026-10-31)

The scanner now retains original ordered/unordered markers, list nesting depth and exact content offsets while exposing immutable `MarkdownDocument.ListRoots` hierarchy. Writer round-trip is unchanged. This is single-line contiguous list item support, not CommonMark-complete lists. See [Nested Lists](Docs/Nested-Lists/en.md).

## Explicitly indented list continuations (2026-10-31)

Consecutive indented paragraph lines can now belong to their list item, with owner identity, exact source offsets and unmodified Markdown output. Blank-separated loose and lazy continuations remain future work. See [List continuations](Docs/List-Continuations/en.md).

## Loose list paragraphs (2026-10-31)

Blank separators followed by suitably indented continuation paragraphs can now belong to their list item. The immutable node exposes `IsLoose`, `SeparatorBlocks` and ordered `ContentBlocks`, with original offsets and unchanged Markdown output. Unindented lazy content and blank-separated nested list containers remain unsupported. See [Loose List Paragraphs](Docs/Loose-List-Paragraphs/en.md).

## Lazy paragraph continuation (2026-10-31)

An unindented paragraph directly after an active list item or its paragraph continuation is now conservatively attached to that item. A new `MarkdownBlock.IsLazyListContinuation` flag distinguishes this case from explicitly indented continuation. The existing renderer aligns both kinds beneath the owner marker; original source offsets, inline references, and lossless writing are retained. Blank lines require the previously supported explicitly indented resumption. Headings, fences, quotes and thematic break markers interrupt lazy ownership. Full CommonMark block nesting remains unsupported.

See [Lazy list continuations](Docs/Lazy-List-Continuations/en.md).

## Quotes, fenced code and language hints (2026-10-31)

Conservative indented blockquotes and fenced code can be associated with their containing list item. Source-aware `QuoteDepth`, `EmbeddedBlocks`, `CodeFenceInfo` and `CodeLanguage` expose metadata without code execution, dependency changes or source rewriting. This does not implement full CommonMark block nesting. See [Code and Quote Containers](Docs/Code-and-Quote-Containers/en.md).

## Lists inside blockquotes (2026-11-01)

`MarkdownDocument.QuoteListRoots` now represents conservative nested numbered/unordered lists on explicitly quote-prefixed lines (`> - item`). Normal `ListRoots` remains the collection of **unquoted** list roots; original source, markers and links are unchanged. [Quoted-list documentation](Docs/Quoted-Lists/en.md). Code fences inside quoted containers and complete CommonMark semantics are not yet supported.

## Explicit quote-prefixed fenced code (2026-11-01)

The parser recognizes fenced code within blockquotes and sufficiently indented quoted-list items (for example, `> ~~~csharp`). Every code line needs an explicit matching quote depth. A missing or different quote prefix ends the quoted fence and does not swallow the next Markdown block. Exact source positions, delimiters, language hints, and whitespace remain intact. [Quoted-code behavior](Docs/Quoted-Code-Fences/en.md). This is a conservative subset of CommonMark.

## Blank-separated paragraphs inside quoted lists (2026-11-01)

A quoted list item may now own one or more physical quote-prefixed blank lines (`> ` / `>`) **only when** the following line belongs to that item's explicitly indented continuation at the same quote depth. The immutable separator/owner fields work for quoted and ordinary lists; raw source and quoted marker styling are retained. Unrelated paragraphs, nested quote levels and list markers cannot steal those separators. [Quoted loose paragraphs](Docs/Quoted-Loose-Paragraphs/en.md). This is not full CommonMark.

## Nested quoted sublists under ordinary list items (2026-11-01)

`- Step` followed by `  > - Warning` now retains both independent owners: `QuoteContainerListOwnerStart` and `QuoteContainerListMarker` describe the ordinary outer item, while `QuoteListRoots` and the existing continuation owner describe the inner quoted list. Numbered children, sufficiently indented paragraphs, quote blank separators and literal same-depth code fences are supported. Original source offsets, links and line endings are unchanged. [Examples and limits](Docs/Quoted-Lists-Inside-Lists/en.md).

## Experimental indexed streaming scanner and diskless stress (2026-11-02)

The additive MarkdownStreamingReader.ScanAsync supports bounded-memory line-wise source scanning, 64-bit offsets and open code fence metadata without buffering a large logical block. Blackboard Markdown includes a streaming-line renderer; the standalone Toolroom/Markdown/Stress app can test generated 10 MiB, 100 MiB, 1 GiB and 10 GiB Markdown without a giant file. **Not a full streaming CommonMark parser or replacement for the Showcase full-document viewer.** See [scanner details](Docs/Streaming-Scanner/en.md) and [stress tool](../../Toolroom/Markdown/Stress/README.md).

## Experimental on-disk Markdown source pager (2026-11-02)

`MarkdownDiskPageIndex.BuildAsync(path)` validates UTF-8 source and
builds an 8-byte-per-page Temp-file index. `ReadPageAsync(number)`
seeks a bounded page without retaining the full document or the whole
index in RAM. The source is never modified and the temporary index is
deleted on disposal. Supports strict UTF-8, LF/CRLF/CR and independently
configurable line/page bounds. **Only raw source paging is implemented**:
a page-local parse can misinterpret a fence crossing the boundary.
No 1 GiB/10 GiB end-to-end pager acceptance or NuGet publication is
claimed. See [disk-backed paging](Docs/Disk-Backed-Paging/en.md).

## Restartable code-fence scanning across disk pages (2026-11-02)

The experimental `MarkdownFenceCheckpointIndex` builds a second small,
disposable **512-byte-per-page** index. At each page boundary it records the
current fenced-code marker, opening offset, quote depth and optional short
language hint. `ReadPageAsync(number)` reconstructs line events with
`MarkdownStreamingReader.ScanAsync(reader, checkpoint)`, even if the opener
is far away. The state index is disk-backed and quota-limited; source text
is not retained. This is **fence-safe classification**, not full resumable
CommonMark semantics or a stable NuGet API. See
[fence checkpoints](Docs/Fence-Checkpoints/en.md).

## Resumable list and paragraph *candidates* (2026-11-02)

The page checkpoint now also stores up to 32 list indent candidates and
the preceding line's quote-depth/paragraph-continuity hints. Seeks restore
`MarkdownStreamLine.ListDepth` across page boundaries, and
`IsParagraphContinuationCandidate` exposes conservative cross-page
plain-paragraph continuity. These hints do **not** constitute complete
CommonMark list ownership, lazy continuation, or identical formatted output.
The 512-byte temporary record remains bounded and quota-controlled.
[Technical details](Docs/Fence-Checkpoints/en.md).


## Explicit list-item owner offsets in page snapshots (2026-11-02)

The resumable `MarkdownStreamCheckpoint` now preserves up to 32 actual
list-marker source offsets plus indentation and continuation columns.
`MarkdownStreamLine.ListOwnerStart` and
`IsListContinuationCandidate` expose the conservative owning item for
explicitly indented text, nested-to-parent returns and simple unindented
lazy continuation. This is backed by **512 bytes per page on disk**,
not previous-page source buffers. Fully faithful CommonMark nested
containers and visual layout remain future work.
[Details](Docs/Fence-Checkpoints/en.md).


## Sparse 512-byte state snapshots (2026-11-02)

`MarkdownFenceCheckpointIndex.BuildAsync` now defaults to **one
checkpoint every 16 pages**; `checkpointStride` accepts 1..256.
`ReadCheckpointAsync(page)` replays at most 15 preceding pages to
restore fence, list-owner, quote and paragraph candidates, holding
only one replay page in memory at a time. The original raw-source
offset index stays 8 bytes/page. `SavedCheckpointCount` exposes the
number of 512-byte records stored on disk, while `PageCount`
continues to report all source pages. This saves disk at the cost
of bounded replay CPU/I/O, **not** constant-time seeking. The entire
source still needs two initial sequential scans.
[Disk/CPU tradeoff](Docs/Fence-Checkpoints/en.md).


## Paragraph source offsets in bounded checkpoints (2026-11-02)

`MarkdownStreamLine.ParagraphStart` now identifies the start of a
conservatively recognized prose paragraph. The 512-byte temporary
checkpoint persists that 64-bit UTF-16 offset along with an optional
`ParagraphListOwnerStart`. Crossing between list child and parent,
changing explicit quote depth or encountering blank/fenced code
interrupts paragraph continuity. Seeking with sparse checkpoints
recovers the same identity without retaining previous text.
Complete CommonMark paragraphs and multi-line layout are not yet
implemented. [Context rules](Docs/Fence-Checkpoints/en.md).

## Blank-separated list hints (2026-11-02)

`MarkdownStreamLine.ListSpacingHint` now distinguishes a new, explicitly
indented paragraph of an existing item from a newly started sibling item
after a blank source line. The enum values are `None`,
`BlankSeparatedParagraph` and `BlankSeparatedItem`. This data can be
reconstructed through sparse checkpoint replay using existing metadata
without enlarging the 512-byte record. **This is not a whole-list
CommonMark tight/loose verdict**; later global list rendering must
resolve final spacing. See [checkpoint details](Docs/Fence-Checkpoints/en.md).

## Optional list-wide spacing summary on disk (2026-11-02)

`MarkdownListSpacingIndex.BuildAsync(pages)` performs an **opt-in** bounded
third source scan (the raw UTF-8 source index and sparse checkpoint index
remain independently usable). It stores only list-group summaries
(**24 bytes/group**) and sorted list-marker offsets (**16 bytes/item**)
in two disposable temporary files, with a combined 256 MiB default disk
quota. The active group stack remains bounded at 32 descriptors.

After completing the scan, `FindForItemAsync(markerStart)` can retrieve a
retroactive `IsLooseCandidate` result for an earlier item by binary
search without loading the groups or item records into RAM. Conservative
nested/quoted/type-separated groups are not a complete CommonMark list
model, and no automatic TUI integration or NuGet release is implied.
[Read the full API and limitation guide](Docs/List-Spacing-Index/en.md).

## Paging profiles (2026-11-03)

`MarkdownPagingProfiles.For` supplies validated **Balanced (40 lines/stride 16), HDD (64/1), SSD (64/32) and LowRAM (8/16)** settings. Callers may override individual values with `with` and pass them to the source and sparse context index. The sixth Showcase reader offers these profiles interactively. These are unbenchmarked tuning starting points, not exact RAM budgets. [Profile guide](Docs/Paging-Profiles/en.md).

## Reusable offline HTTPS Markdown source cache (2026-11-03)

`MarkdownRemoteDocumentCache` now stores versioned, immutable source copies in a per-user application cache, separate from disposable page/checkpoint indexes. Its `GetAsync` supports conditional HTTP `ETag`/`Last-Modified` checks, explicit offline-only access, forced refresh and marked stale fallback after connection failures. `SaveCopyAsync` exports a selected version to a permanent user path and `RemoveAsync` removes a single URL's stored versions on request. The default per-document download limit is 256 MiB and transfers use bounded buffers. This is a **library primitive**, not yet a remote Docs Reader TUI, automatic network sync, global cache quota or secure multi-user server endpoint. See [cache contract](Docs/Offline-Document-Cache/en.md).

## Cache-wide source retention and disk quota (2026-11-03)

`MarkdownRemoteDocumentCache` now enforces a configurable **1 GiB
default total cache-owned disk budget** alongside the independent
256 MiB/document transfer limit. `GetUsageAsync` reports occupied
bytes and file count without loading documents. An explicit
`PruneObsoleteVersionsAsync` preserves each valid manifest-selected
offline copy, retains recent versions for 24 hours by default and
avoids deleting anything if its manifest is missing/corrupt. Failed
quota-limited downloads leave the last usable copy intact; there is
**no automatic eviction, no total-process RAM guarantee and no
cross-process storage lock**. [Details](Docs/Offline-Document-Cache/en.md).

## Bounded offline document catalog (2026-11-03)

`MarkdownRemoteDocumentCache.ListPageAsync(skip,pageSize)` now returns a
bounded manifest page (1–64 entries), with **no network requests** or
full-catalog allocation. New source manifests retain a normalized
source URL, authenticated by matching its SHA-256 key before display.
Legacy manifests without this property remain readable and appear
with an unknown source. Enumeration order is filesystem order, not
a globally sorted stable index. The Showcase seventh demo uses this
API for its offline cache manager.
[Menu usage](../../Blackboard/Samples/Blackboard.Showcase/Docs/Offline-Cache-Manager/en.md).
