# ExchangeHouse.Markdown.Tests

xUnit regression tests for source-preserving block parsing and writing: mixed line endings, unicode, exact offsets, list/headings detection and fenced-code isolation.

Run from Toolbox root:

```bash
dotnet test ExchangeHouse/Markdown.Tests/Afrowave.Toolbox.ExchangeHouse.Markdown.Tests.csproj
```

These tests do not establish full CommonMark conformance; inline parsing, conversion, edited writer and translation behavior remain explicitly out of scope. See [implementation status](IMPLEMENTATION_STATUS.md).

## Inline tokens

`MarkdownInlineParserTests` verify source-offset-aware inline scanning, nested emphasis, simple links with parentheses, escaped delimiters, fenced-code protection and malformed literal fallback. They do not assert full CommonMark conformance.

Nested list regression suite (2026-10-31): ordered/unordered markers, hierarchy, source numbering, mixed indentation and tab columns, non-list boundaries, inline source offsets and exact round trips.

2026-10-31: Indented continuation line ownership and exact source offsets, nested ancestor return, numbered/tabbed indentation and code/blank-line boundaries have dedicated xUnit cases.

Loose-list regressions (2026-10-31): owned blank separators, multiple blank lines with tabs/CRLF, ancestor reattachment, unrelated/trailing blanks, new list marker boundaries, code-fence protection and unchanged round trips.
