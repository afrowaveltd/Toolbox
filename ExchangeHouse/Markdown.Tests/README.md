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

## Explicit container scanner regressions (2026-11-07)

Added `MarkdownIncrementalContainerScannerTests` (**18 cases**: 15 `[Fact]` and three `[InlineData]`) covering quote depth, nested list markers and ancestor continuations, quoted and top-level fenced code, interruption on a quote-depth change, source offset/line-ending fidelity, stale owner clearing, tabs and ordered marker bounds, early streaming events, unsafe controls and cancellation. A restricted marker/fence comparison checks agreement with the existing full `MarkdownParser`. This is not yet an assertion of complete CommonMark compatibility.

[Scanner contract](../Markdown/Docs/Explicit-Container-Scanner/en.md).

## Dual list ownership regressions (2026-11-07)

Added `MarkdownIncrementalQuoteOwnershipTests` (**17 cases**), validating separate outer ordinary-list and quote-local list owner metadata, nested ordered/unordered ancestors, quote-only ownership, quoted/ordinary-list code-fence protection and interrupted quote scope. Tests compare provable owner source offsets to `MarkdownParser.Parse`, including Unicode and mixed CR/LF endings. Three additional cases specifically reject falsely opening or closing fences after stripping *too much* indentation. The earlier single failing fixture assertion was corrected: the outer list's root depth is `0`, not `-1`.

[Ownership contract](../Markdown/Docs/Dual-List-Ownership/en.md).

## Deferred ownership and lazy-continuation tests (2026-11-07)

Added `MarkdownDeferredListOwnershipScannerTests` (**20 cases**, including four parameterized invalid-budget rows), covering loose-list blanks in ordinary and quoted lists, independent outer/inner quote owners, blank runs flushed at EOF or changes of scope, explicit-only blank assignment, lazy continuation without a blank, headings/thematic breaks/fences interrupting lists, incremental emission and strict lookahead budgets.

Two existing scanner fixtures were updated to expect valid lazy paragraph ownership where the previous conservative implementation emitted generic `Plain` or `Quote` lines. The thematic-break test uses unambiguous `***` rather than `* * *`, which the existing parser can also treat as a list marker.

[Deferred ownership documentation](../Markdown/Docs/Deferred-List-Ownership/en.md).
