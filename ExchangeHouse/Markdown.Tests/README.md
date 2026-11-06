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

## Incremental source stream tests (2026-11-04)

Added `MarkdownIncrementalLineScannerTests` (**16 test cases**) for exact physical line endings, UTF-16 source offsets, CRLF split between reads, top-level fence state/language hints, flat ATX heading parity, opaque quote/list preservation, forward-only lazy reads, line limits, unsafe controls and cancellation. This experimental scanner is separate from the existing fully parsed `MarkdownDocument` and must not be mistaken for complete CommonMark or a formatted visual pager. New tests are committed but **not yet executed in this environment**.

[Scanner contract](../Markdown/Docs/Incremental-Source-Scanner/en.md).

## Incremental completed-block span tests (2026-11-07)

Added `MarkdownIncrementalBlockScannerTests` with 16 cases for exact non-overlapping source coverage, CRLF/Unicode offsets, top-level headings and blank blocks, constant-sized metadata for long paragraph/code ranges, explicit unclosed fences, opacity of quote/list lines, one-line lookahead, cancellation, late-invalid controls and invalid budgets. A **restricted flat grammar** corpus is compared with `MarkdownParser.Parse` for heading/paragraph/blank/fence classification, without claiming equivalence for nested containers.

The new tests were committed but should be considered unverified until the next CI or maintainer run. [Scanner contract](../Markdown/Docs/Incremental-Block-Spans/en.md).
