# ExchangeHouse.Markdown.Tests

xUnit regression tests for source-preserving block parsing and writing: mixed line endings, unicode, exact offsets, list/headings detection and fenced-code isolation.

Run from Toolbox root:

```bash
dotnet test ExchangeHouse/Markdown.Tests/Afrowave.Toolbox.ExchangeHouse.Markdown.Tests.csproj
```

These tests do not establish full CommonMark conformance; inline parsing, conversion, edited writer and translation behavior remain explicitly out of scope. See [implementation status](IMPLEMENTATION_STATUS.md).
