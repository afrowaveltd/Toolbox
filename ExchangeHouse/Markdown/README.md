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

The parser recognizes fenced code within blockquotes and sufficiently indented quoted-list items (for example, `> ```csharp`). Every code line needs an explicit matching quote depth. A missing or different quote prefix ends the quoted fence and does not swallow the next Markdown block. Exact source positions, delimiters, language hints, and whitespace remain intact. [Quoted-code behavior](Docs/Quoted-Code-Fences/en.md). This is a conservative subset of CommonMark.
