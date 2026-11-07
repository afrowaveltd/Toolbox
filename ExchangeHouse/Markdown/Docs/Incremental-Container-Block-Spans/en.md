# Completed container-aware Markdown source spans

**Status:** implemented as a separate, opt-in scanning layer on 2026-11-07. It is an **indexing primitive**, not the Markdown renderer and not a full CommonMark AST.

## Purpose and architecture

The incremental pipeline now has four explicitly separated responsibilities:

1. `MarkdownIncrementalLineScanner`: read and validate individual decoded physical lines, preserving CR, LF, CRLF and UTF-16 offsets.
2. `MarkdownIncrementalContainerScanner`: recognize conservative list markers, quote depth, separate ordinary/quoted list ownership, uninterrupted lazy paragraphs and quote-owned fences.
3. `MarkdownDeferredListOwnershipScanner`: delay a *bounded* run of blank lines until an explicitly indented continuation confirms its owner.
4. **`MarkdownIncrementalContainerBlockScanner`**: turn these events into completed, **nonoverlapping contiguous source ranges** that can later be written to a disk-backed block index.

No stage caches an entire Markdown document, and the new span scanner does not copy paragraph text or fenced code into its output. It keeps only one pending range, while the underlying line and blank scanners maintain their own explicit budgets.

## Public API

```csharp
using Afrowave.Toolbox.ExchangeHouse.Markdown;

using var reader = new StringReader("# Installation\n- First\n  detail\n");

await foreach (var span in MarkdownIncrementalContainerBlockScanner.ScanAsync(reader))
{
    Console.WriteLine(
        $"{span.Kind} [{span.StartUtf16Offset}, {span.EndUtf16Offset})" +
        $" lines {span.StartLine}..{span.EndLineExclusive}");
}
```

`MarkdownContainerBlockSpan` includes:

- `StartUtf16Offset`, `EndUtf16Offset`, `Length`: half-open offsets in **decoded UTF-16 characters**, including physical line endings.
- `StartLine`, `EndLineExclusive`, `LineCount`: half-open zero-based physical source line ranges.
- `Kind`: `Heading`, `PlainRun`, `QuoteRun`, `ListItem`, `ListContentRun`, `BlankRun`, or `FencedCode`.
- `QuoteDepth`, `ListDepth`, `ListOwnerUtf16Offset`: exact *explicit* owner/scope where known.
- `QuoteContainerListOwnerUtf16Offset`: separate outer ordinary-list owner for a quoted span; `ListBlankOwnerUtf16Offset`: deferred separator owner; `IsLazyListContinuation`: flags unindented list paragraphs.
- `ListMarker`, `HeadingLevel`, `CodeLanguage`, `FenceStartUtf16Offset`: sparse metadata relevant only to their corresponding block categories.
- `FenceEnd`: `NotApplicable`, `Closed`, `Interrupted` or `EndOfFile`.

These are **source ranges, not UTF-8 byte offsets**. The future disk index must map decoded coordinates to validated physical source bytes before implementing random access. `ListItem` represents the list item's **marker line only**, not a fully buffered list subtree.

## Completion and grouping rules

Two consecutive non-fence lines merge only when their classification and all relevant scope/owner metadata match, and their source offsets and line indices are adjacent. A top-level heading and each explicit list-item marker are always emitted independently, even when consecutive. Literal quote text may be grouped by quote depth and outer owner. Ordinary or quoted list continuation runs may be grouped only while the list owner, depth and lazy/explicit classification are the same. Blank separators merge only when the *resolved* blank owner, quote depth and outer owner match.

A fenced-code source span is retained until one of three outcomes:

1. **`Closed`**: a valid matching closing fence with the same opening source offset is emitted. Its physical line belongs to the span.
2. **`Interrupted`**: the container scanner changes quote depth or outer-list owner and reclassifies the next line outside the fence. The current span ends *before* that line; the interrupting line is still emitted in its own new span. No phantom closing delimiter is created.
3. **`EndOfFile`**: the stream ends without a closing delimiter. Its remaining content is included in the span and explicitly marked as unterminated.

This is intentionally not a complete Markdown rendering rule: container interrupts and paragraph grouping are based on the conservative line scanner, not all CommonMark constructs. The existing whole-document `MarkdownParser` remains the semantic reference; the known overindented list-fence *opening* behavior difference is still documented in [Dual List Ownership](../Dual-List-Ownership/en.md).

## Resource, safety and failure behavior

The new stage itself stores only **one pending source range**. Its dependency budgets remain in force:

- 4,096-char read chunks and a configurable maximum decoded physical line length (default 1 Mi UTF-16 characters, maximum 4 Mi).
- Two capped list nesting stacks (32 frames each).
- Deferred separator buffers capped by default at **1,024 physical blank lines** and **1 Mi aggregate UTF-16 characters**; exceeding either fails with `InvalidDataException`.
- Cancellation is propagated through the entire asynchronous chain. No phantom blank line is generated after an EOF newline.

A run of millions of unbroken paragraph or code lines can delay the next span event until its end; it does *not* require proportional RAM. If a future UI needs early partial rendering of an enormous single run, it will need bounded subrange checkpoints in the disk index rather than claiming that every stream event immediately produces a page.

**Security:** Receiving decoded text from a `TextReader` does not prove valid original UTF-8 or complete terminal safety. A late invalid character may cause an exception after earlier ranges have already been yielded. Before displaying untrusted input, perform whole-file validation and use an immutable source/cache snapshot (for example through the existing `Utf8DiskLineIndex` / verified document cache), then map source positions against that snapshot.

## Testing

```bash
dotnet test ExchangeHouse/Markdown.Tests/Afrowave.Toolbox.ExchangeHouse.Markdown.Tests.csproj
dotnet test Toolbox.sln
```

`MarkdownIncrementalContainerBlockScannerTests` includes 22 cases covering complete, contiguous mixed-ending source reconstruction; independent list/quote ownership; resolved and unresolved blank runs; separation of lazy and explicit continuations; quoted and ordinary fenced-code lifetimes and interruption; exact Unicode source offsets; large paragraph/code ranges; early streaming events; cancellation and inherited resource budgets. Existing parser round-trip checks are used on supported samples. This is **not** a complete CommonMark equivalence claim.

## Next milestones

1. Reconcile the full parser's **list-owned opening fence indentation** behavior with the conservative streaming scanner under golden cross-parser tests, preserving old API and renderer behavior until a compatible decision is made.
2. Introduce compact, versioned **disk-backed span indexing** keyed by a verified document digest, so UTF-16 source intervals can be navigated without storing all spans in RAM.
3. Build a **visual-row-aware** rendered pager and backward/forward navigation over indexed block boundaries. Its UI work should remain separate from the already functional `mdview source` raw-source pager.
4. Later integrate [on-demand heading and phrase search](../../../../Blackboard/Tools/MdView/Docs/Indexed-Search/en.md) with `F`, `N`, `P` keys and optional configurable English README topics. These keys remain inactive today.

[Markdown README](../../README.md) · [Deferred blank ownership](../Deferred-List-Ownership/en.md) · [Implementation status](../../IMPLEMENTATION_STATUS.md).
