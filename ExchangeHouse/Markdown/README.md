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
