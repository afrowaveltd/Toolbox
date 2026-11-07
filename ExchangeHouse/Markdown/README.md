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

NuGet release is deferred until the model/API matures and CI passes.

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

## Bounded forward-only Markdown source scanner (2026-11-04)

Added an **opt-in**, UI-neutral `MarkdownIncrementalLineScanner.ScanAsync(TextReader)` that yields physical lines one by one through `IAsyncEnumerable<MarkdownStreamLine>`, preserving original CRLF/LF/CR endings and UTF-16 source offsets. It tracks top-level fenced-code state (including an optional language hint), ATX headings, blank lines and simple opaque quote/list starts without allocating a complete document model. It enforces a per-line character budget and rejects unsafe terminal controls.

**This is not yet a complete incremental block parser or rendered Markdown pager.** Nested list/quote ownership, reference definitions, multiline paragraph groups and visual wrapping still belong to the existing full parser or future state-machine work. The scanner receives already decoded characters; to prevent partial display of an untrusted file that contains invalid bytes late in the stream, validate the entire file first using the TextFiles disk index or an immutable verified spool.

[Design, guarantees and limitations](Docs/Incremental-Source-Scanner/en.md). New regression tests are authored but not yet executed in this environment.

## Completed incremental block spans (2026-11-07)

The optional `MarkdownIncrementalBlockScanner.ScanAsync(TextReader)` consumes `MarkdownIncrementalLineScanner` and emits **completed, non-overlapping source ranges**: top-level headings, blank lines, consecutive simple paragraph lines, closed/unterminated top-level fenced code, and opaque single-line quote/list markers. It records UTF-16 source offsets and physical line ranges, not a duplicate string for an arbitrarily long block. Fence closure and language hints remain explicit, and unfinished code fences are distinguished at EOF.

This is an **indexing foundation**, not a substitute for the existing full Markdown parser, inline handling, nested container semantics or renderer. `mdview` is not connected to it yet. [Technical contract and examples](Docs/Incremental-Block-Spans/en.md); 16 new focused test cases and a restricted golden comparison with `MarkdownParser.Parse` were added. New tests await CI confirmation.

## Conservative explicit container metadata (2026-11-07)

Added `MarkdownIncrementalContainerScanner.ScanAsync(TextReader)`, an **additive forward-only metadata layer** recognizing explicit quote depth (including `>>` and `> >`), ordered/unordered nested list markers, only provable indented list continuations and fences inside quotes. Code lines preserve the opening fence scope: embedded headings, list and quote characters do not turn into structure. A change in explicit quote depth interrupts a quoted fence instead of fabricating a closing delimiter.

The scanner emits one `MarkdownContainerLine` per physical source line and holds a capped **32-frame list stack** plus constant-size fence metadata; no document or block text is retained beyond the current line. Critically, it **does not** guess unindented lazy continuations, deferred blank owners or outer-list ownership of a nested quote. Those remain part of the existing whole-document parser until we can reproduce them reliably. Nothing is connected to `mdview` rendering yet.

[Full behavior and limitations](Docs/Explicit-Container-Scanner/en.md). Added 18 focused cases for nested quotes, lists, explicit owners, quote-owned fences, cancellation and a restricted parser comparison; validate with `dotnet test Toolbox.sln`.

## Independent quote / ordinary-list ownership (2026-11-07)

The additive `MarkdownContainerLine` API now exposes three init-only properties—`QuoteContainerListOwnerUtf16Offset`, `QuoteContainerListDepth` and `QuoteContainerListMarker`—for **the outer ordinary-list item** that owns a quoted line. The existing `ListOwnerUtf16Offset` and `ListDepth` still represent the **quote-local list owner** where appropriate. The two stacks remain distinct and are invalidated on quote-depth or outer-owner changes.

Inside ordinary and quoted lists, fenced-code indent stripping now removes **only the owning item's required content indent** rather than all leading whitespace. This prevents excessively indented backticks from being mistaken for opening or closing code fences. It is checked against the existing whole-document parser.

This is a conservative, opt-in scanner feature: lazy continuations and deferred loose-list blank ownership are not inferred, the full renderer and mdview are unchanged, and this is not yet complete CommonMark. [Detailed ownership design](Docs/Dual-List-Ownership/en.md).

## Deferred blanks and lazy list ownership (2026-11-07)

The forward-only `MarkdownIncrementalContainerScanner` now marks uninterrupted, unindented list paragraph continuations with `MarkdownContainerLine.IsLazyListContinuation`. A blank, heading, new marker, thematic break, fence or quote boundary prevents accidental lazy ownership. This remains a conservative, opt-in source scanner and does not alter the existing full parser or mdview renderer.

New `MarkdownDeferredListOwnershipScanner.ScanAsync(TextReader)` offers **bounded lookahead** to defer classification of blank separators until the following explicitly indented list continuation proves their owner. It preserves original text, line endings, quote depth and separate outer/inner list owner metadata. `ListBlankOwnerUtf16Offset` is assigned only to proven blanks; other blanks remain unowned. Buffers are limited by both a line-count budget (default 1,024) and an aggregate UTF-16 character budget (default 1 Mi); excess input raises an explicit exception rather than silently truncating source.

[Detailed English contract](Docs/Deferred-List-Ownership/en.md). Twenty new tests cover loose-list ownership, quote scopes, continuation rules, delayed events, memory budgets and restricted parity with the full parser. The rendered CLI remains untouched.

## Container-aware completed source spans (2026-11-07)

New opt-in `MarkdownIncrementalContainerBlockScanner.ScanAsync(TextReader)` composes the existing source-line, explicit quote/list, and deferred-blank scanners into **completed, non-overlapping UTF-16 source spans** suitable for a future disk-backed Markdown block index. It preserves physical line boundaries, separate ordinary and quoted-list ownership, lazy vs explicit continuations, and resolved vs unresolved blank separators **without copying entire paragraphs or code fences**.

Span categories are top-level heading, ordinary run, quote run, one list-marker line, list-owned continuation run, blank run, and fenced code. A fence explicitly reports `Closed`, `Interrupted`, or `EndOfFile`; interrupted lines are reprocessed as their own spans, never swallowed. Long contiguous runs require constant-sized span state; the line reader and deferred-blank adapter retain their existing resource budgets.

[Full contract and limitations](Docs/Incremental-Container-Block-Spans/en.md). The scanner does **not** parse full CommonMark, collect complete list subtrees, map UTF-16 spans directly to UTF-8 byte offsets, or change current `mdview` rendering. Twenty-two focused tests were added for exact source reconstruction, quote/list ownership, fence completion, lazy/blank boundaries, Unicode, bounded reading and cancellation.


## Versioned incremental scanner checkpoint contract (2026-11-08)

`MarkdownScannerCheckpoint` and
`MarkdownScannerCheckpointCodec` establish a **versioned, bounded,
validated serialization contract** for future fast Markdown scanner
restoration. The schema records the next physical line and UTF-16 offset,
both list-ownership stacks, quote state, blank separator flags and complete
active code-fence context. JSON payloads are limited to 32 KiB; unknown
versions, malformed data, invalid offset references and over-deep lists
fail validation.

This is **not yet a resumable scanner**: current `ScanAsync` does not
export this state or accept it for restart. No snapshot identity binding
or safe random seek is claimed. `mdview preview` continues using the
already validated disk-backed classified-event journal.

[Checkpoint contract and limitations](Docs/Scanner-Checkpoints/en.md).
Five new focused tests are committed; their execution is pending.
