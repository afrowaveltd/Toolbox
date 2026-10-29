# Afrowave.Toolbox.ExchangeHouse.Markdown

Small, UI-neutral .NET 10 Markdown parsing foundation for the ExchangeHouse family.

**Status: first, intentionally limited block scanner and lossless writer. Not a complete CommonMark parser, full AST or Markdown conversion library. Not published to NuGet.**

## Current API

```csharp
using Afrowave.Toolbox.ExchangeHouse.Markdown;

var document = MarkdownParser.Parse("# Hello\r\n- item\r\n");
var heading = document.Blocks[0]; // Heading, level 1, text "Hello"
var unchanged = MarkdownWriter.Write(document);
```

- Recognizes ATX headings (1-6), simple list markers, blank lines, ordinary lines, fenced code delimiters and fenced code contents.
- Retains exact original line text, offsets, whitespace and CRLF/LF/CR terminators.
- Does not evaluate or execute Markdown, links or raw HTML; requires no UI, filesystem or networking.
- Keeps each physical line separate: nesting, multiline paragraphs, inline emphasis, links, tables, reference definitions and true CommonMark conformance are future increments.
- Writing currently reconstructs the unchanged document; **no source-transforming writer or converters are claimed**.

Documentation: [Parser and writer](Docs/Parser-and-Writer/en.md), [ExchangeHouse architecture](../Docs/Architecture/en.md) and [implementation status](IMPLEMENTATION_STATUS.md).

## Testing

```bash
dotnet test ExchangeHouse/Markdown.Tests/Afrowave.Toolbox.ExchangeHouse.Markdown.Tests.csproj
```

NuGet release is deferred until the model/API matures and CI passes.
