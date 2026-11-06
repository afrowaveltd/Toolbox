# Incremental explicit Markdown container scanner

**Status:** additive, isolated source metadata scanner implemented on 2026-11-07. This is an intermediate step toward container-aware streaming Markdown blocks, **not** a renderer, complete CommonMark parser, or replacement for the current `MarkdownParser.Parse`.

## Motivation and design

The old incremental line scanner already recognizes top-level code fences and headings, but must not guess whether a line of text belongs to a list item or quote. Existing `MarkdownParser` tracks separate ordinary and quoted list stacks, explicit indentation, nested markers, quoted code fences and blank-separated continuations. We can preserve the most reliable part of those semantics **without allocating the whole document** by emitting a metadata event for each completed physical line.

`MarkdownIncrementalContainerScanner.ScanAsync(TextReader)` consumes `MarkdownIncrementalLineScanner.ScanAsync` and yields `MarkdownContainerLine` events with:

- The original `MarkdownStreamLine` in `Source`, preserving exact source text, CR/LF/CRLF, zero-based physical line number and UTF-16 source offset.
- `Kind`: `Plain`, `Blank`, `Quote`, `ListItem`, `ListContinuation`, `FenceOpen`, `CodeLine` or `FenceClose`.
- `QuoteDepth`: number of **explicit** quote markers (0–32). Nested `>>` and `> >` retain the same depth.
- `ListDepth`: 0-based **explicit list nesting** or -1 when no owner is supported by source indentation; `ListMarker` and `ListOwnerUtf16Offset` identify the actual marker/owner only when known.
- `FenceStartUtf16Offset`: original opening delimiter position when a top-level or quote-owned fence is active. `CodeLanguage` contains its bounded first language token, if any.

This is per-line bounded metadata (aside from the original single line), **not a copy of each list, quote or code block**.

## Supported operations

```csharp
using Afrowave.Toolbox.ExchangeHouse.Markdown;

using var reader = new StringReader("> - Install\n>   - Linux\n>     apt install ...");
await foreach (var line in MarkdownIncrementalContainerScanner.ScanAsync(reader))
{
    Console.WriteLine(
        $"{line.Source.LineNumber}: {line.Kind}, quote={line.QuoteDepth}, list={line.ListDepth}");
}
```

- Explicit ordered list markers (up to nine digits; `1.`, `2)`) and unordered markers (`-`, `+`, `*`) are recognized with the existing parser's four-column tab-stop and 256-column indentation rules.
- Markers nested beneath a previous item use a bounded (32-level) stack. An explicitly indented continuation is attributed to the deepest compatible list item. When an ancestor is selected, deeper stale frames are discarded. A change in quote depth resets the stack.
- A nonblank ordinary/quoted line that lacks provable list ownership ends the active list context, avoiding later false attribution to a stale list.
- Fences opened within **explicit quotes** retain the opening quote depth. While active, `# Heading`, list markers, and nested quote-like text are literal `CodeLine`. A matching closing fence must have the right marker, sufficient length and only spaces or tabs after it.
- If quote depth differs from that of the opener, a quote-owned fence is **interrupted**, not auto-closed. The mismatching line is reclassified normally; no synthetic closing code fence is invented.
- A top-level fence treats quote and list-like lines as literal code until its matching closer, regardless of their appearance.

## Deliberate limitations

This is an **explicit-markers** scanner, not a semantic list or quote AST. In particular:

- Lazy/unindented paragraph continuation ownership is **not** inferred. Whitespace-only blank lines do not acquire a list owner here; deferred blank/loose-list ownership remains a full-parser feature.
- Ordinary list items containing quotes can require an outer list owner distinct from the inner quoted list. This intermediate layer intentionally does **not** claim those outer owners. Only quote-depth-local list ownership is attached where explicit indentation proves it.
- The scanner does not yet parse headings *inside* quotes, setext headings, thematic breaks, reference definitions, inline links, or visual rows.
- The original `MarkdownIncrementalBlockScanner` remains focused on top-level source spans; it has **not** been switched to this container scanner until we can confirm complete container boundary semantics.
- Output is progressive: a `TextReader` does not prove that the original file is valid UTF-8. Do not send its events to an untrusted terminal without a previous full-source validation pass (e.g., `TextFiles.Utf8DiskLineIndex` or a verified immutable spool).

The state contains a maximum of 32 list frames, current quote depth, and one active fence's marker, length, language hint, source position and possible explicit owner. Each line has the same configurable character limit as the original scanner, by default 1 Mi UTF-16 code units. No additional disk index is created by this component.

## Verification

```bash
dotnet test ExchangeHouse/Markdown.Tests/Afrowave.Toolbox.ExchangeHouse.Markdown.Tests.csproj
dotnet test Toolbox.sln
```

`MarkdownIncrementalContainerScannerTests` cover nested `>` depth, quote depth changes, nested ordered and unordered lists, ancestors and explicit continuations, quoted and top-level protected fences, depth-mismatch reclassification, source/offset preservation, stale owner clearing, tab stops, marker validation, cancellation, invalid terminal controls and incremental reads. A restricted corpus compares supported marker and fence kinds with the existing full parser; it is not a claim of full CommonMark parity.

## Next work

1. Extend the container state model to distinguish an outer ordinary list owner from quote-local list ownership; add test cases matching `MarkdownParser.QuoteContainerListOwnerStart`.
2. Define safe completed block events for quote/list-owned spans, including deferred blank ownership, interruption and nesting boundaries.
3. Reconcile these events with the existing parser using real README fixtures, then introduce disk-backed block indexing and the visual Markdown pager.

[Markdown README](../../README.md) · [Incremental block spans](../Incremental-Block-Spans/en.md) · [Implementation status](../../IMPLEMENTATION_STATUS.md).
