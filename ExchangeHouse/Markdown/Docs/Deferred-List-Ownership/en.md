# Deferred blank separators and lazy Markdown list continuations

**Implemented:** 2026-11-07. This is an opt-in, source-preserving streaming layer in `ExchangeHouse.Markdown`, not yet a formatted or paged `mdview` renderer.

## Why this needs a separate step

A Markdown blank line between list paragraphs cannot always be assigned to an item when it is first read. For example:

```markdown
- First item

  The next paragraph is explicitly indented.
```

The blank separator belongs to the earlier item **only after the following indented paragraph confirms the owner**. However, if the next line starts a new item, is unindented prose or leaves the quote container, the blank should remain unowned.

A second, different rule concerns an unindented **lazy continuation** directly after a list item:

```markdown
- First item
continued prose without indentation
```

The second line can belong to the item **only while no blank or interrupting block starter has broken the context**.

## APIs

The existing `MarkdownIncrementalContainerScanner.ScanAsync` now recognizes a conservative `ListContinuation` with `IsLazyListContinuation=true` when the current unindented paragraph is eligible. It does **not** infer laziness after a blank, when quote depth or enclosing list owner changes, or when the line begins a heading, a new list marker, a code fence, a quote marker or a thematic break.

The public `MarkdownContainerLine` record has two additional **init-only** properties:

- `IsLazyListContinuation`: true only when ownership was inferred from an uninterrupted paragraph in the current active list stack.
- `ListBlankOwnerUtf16Offset`: source offset of a list item that was proven to own a pending blank separator. Null if unresolved or unowned.

The new `MarkdownDeferredListOwnershipScanner.ScanAsync(TextReader)` consumes the explicit container scanner, **holds a bounded run of blank lines**, and emits them only when the following event establishes ownership or disproves it. When the following event is an **explicitly indented** `ListContinuation` in the same quote depth and with the same outer ordinary-list owner, it adds `ListBlankOwnerUtf16Offset` to the blank events using a copy of their original records. A new list marker, lazy/unindented prose, changed quote/outer-owner scope, or EOF flushes the blanks without attaching a speculative list owner. Input order, exact original text and line endings are preserved.

```csharp
using Afrowave.Toolbox.ExchangeHouse.Markdown;

using var reader = new StringReader("- Item\n\n  Continuation\n");
await foreach (var line in MarkdownDeferredListOwnershipScanner.ScanAsync(reader))
{
    Console.WriteLine(
        $"{line.Source.LineNumber}: {line.Kind}; blank owner = {line.ListBlankOwnerUtf16Offset}");
}
```

As with the existing scanner, this operates on decoded UTF-16 positions, **not UTF-8 byte offsets**. It does not parse inline Markdown or determine visual terminal rows.

## Bounded memory and failure policy

The underlying reader retains only the current physical line and two maximum 32-frame list stacks. The deferred adapter adds at most:

- **1,024 pending blank line records** by default (`maxPendingBlankLines`).
- **1,048,576 UTF-16 characters** of aggregate pending source by default (`maxPendingBlankCharacters`).

Both limits are configurable positive integers. If either budget is exceeded, the reader fails explicitly with `InvalidDataException`; it never silently truncates a pending run or pretends its ownership is known. Cancellation propagates. A blank-run event may be delayed until its next nonblank line or EOF, while a nonblank event is available as soon as its physical line is complete. This is a resource-aware intermediate component, not yet the final disk-backed implementation for pathological multi-gigabyte documents.

For untrusted content, the already decoded `TextReader` is **not** proof that the file was valid UTF-8. Complete source validation (for example using `TextFiles.Utf8DiskLineIndex`) must precede terminal rendering to prevent partial unsafe output.

## Parsing boundaries and known differences

- Only provable **explicit** list ownership of blank separators is assigned. Lazy continuations are allowed only across uninterrupted nonblank prose and carry a separate flag.
- Explicit quoted list items remain separate from the enclosing ordinary list; scope comparisons include **both** quote depth and outer item owner.
- The original `MarkdownParser.Parse` is the semantic reference for supported blank and lazy cases. It may still differ on more complex nesting, interrupted code fences or ambiguous quote/list ownership.
- A tracked opening-code-fence indentation difference between the full and incremental scanners remains unresolved; see [Dual List Ownership](../Dual-List-Ownership/en.md).
- No completed rendered container blocks, visual-row paging, `F/N/P` search or full CommonMark guarantees are claimed. `mdview` remains unchanged.

## Verification

```bash
dotnet test ExchangeHouse/Markdown.Tests/Afrowave.Toolbox.ExchangeHouse.Markdown.Tests.csproj
dotnet test Toolbox.sln
```

`MarkdownDeferredListOwnershipScannerTests` adds 20 cases covering loose-list blank ownership in ordinary and quoted containers, separated outer/inner owners, unmatched/changed scopes, EOF, unindented lazy paragraphs, headings/fences/thematic breaks, bounded reads, delayed emission, cancellation-friendly enumeration and explicit buffer budget failures. Existing quote/list marker fixtures were adjusted where the new lazy classification correctly replaces earlier generic text classifications.

[Markdown README](../../README.md) · [Explicit container scanner](../Explicit-Container-Scanner/en.md) · [Implementation status](../../IMPLEMENTATION_STATUS.md).
