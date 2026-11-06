# Incremental Markdown block spans

**Status:** first working, isolated block-range projection; added 2026-11-07. The full Markdown renderer is unchanged.

## What this adds

`MarkdownIncrementalBlockScanner.ScanAsync(TextReader)` consumes our existing [incremental physical-line scanner](../Incremental-Source-Scanner/en.md) and returns `IAsyncEnumerable<MarkdownStreamBlock>`. Each event describes a **completed source range**, without holding a copy of all the text in that range.

A range contains:
- `Kind`: `Heading`, `ParagraphRun`, `Blank`, `FencedCode` or `OpaqueContainer`.
- `StartUtf16Offset` (inclusive), `EndUtf16Offset` (exclusive), `Length`: decoded UTF-16 character offsets, including original line endings.
- `StartLine`, `EndLineExclusive`, `LineCount`: zero-based physical line coordinates.
- `HeadingLevel` on top-level ATX headings; `CodeLanguage` on top-level fenced code.
- `FenceClosed=false` only for a code fence reaching EOF without a matching closing fence.

These spans allow a future disk-backed block index to navigate a document without keeping paragraphs or code blocks in RAM. They are **not byte offsets**: a future UTF-8 offset-map or immutable spool is required before jumping directly into a raw file by a UTF-16 span.

## Example

```csharp
using Afrowave.Toolbox.ExchangeHouse.Markdown;

using var reader = new StringReader("""
# Installation
Run the installer.
Then configure it.

```sh
echo hello
```
""");

await foreach (var block in MarkdownIncrementalBlockScanner.ScanAsync(reader))
{
    Console.WriteLine($"{block.Kind}: lines {block.StartLine}–{block.EndLineExclusive}");
}
```

For this source, the stream yields a heading, one two-line paragraph run, a blank and a closed fenced-code span. It does not return Markdown-rendered markup. Each range is emitted when its boundary is known. A heading or blank needs no lookahead; a paragraph may need one additional physical line; a fenced code span completes on its matching closing delimiter or EOF.

## Resource and correctness model

The scanner uses the existing 4,096-character incremental line reader and its configurable line limit (default 1 Mi UTF-16 characters). Its own pending state is constant-sized: source offsets, physical line counts and the optional first fence-language token. In particular, a 100 MB code fence does **not** create a separate 100 MB string inside the block scanner. It waits for the fence to close before emitting that span.

On supported flat inputs, returned ranges are contiguous and nonoverlapping, and their reconstruction against the immutable input yields the exact original source including CRLF, LF, CR, Unicode and trailing terminators. The scanner never evaluates code, parses links, or formats Spectre.Console markup.

**Important boundaries:**
- `ParagraphRun` only groups consecutive unclassified top-level physical lines. It does not claim to reproduce CommonMark paragraph/soft-break semantics.
- List and quote markers remain **opaque one-line spans**. No source structure is silently inferred from nested quotes, list continuations or fences owned by a container. These must be handled by a later container-aware state machine.
- Fences are currently **top-level only**, matching the conservative line scanner. `FenceClosed=false` distinguishes unterminated fences.
- A decoded `TextReader` is **not** proof that the original file's UTF-8 is valid. The scanner can yield earlier spans before discovering invalid text late in a stream; all-or-nothing display of untrusted data must first use a whole-file validator such as `TextFiles.Utf8DiskLineIndex`.
- No visual row wrapping, heading navigation, phrase search, paging UI or persistent block cache is implied by this milestone.

## Regression strategy

```bash
dotnet test ExchangeHouse/Markdown.Tests/Afrowave.Toolbox.ExchangeHouse.Markdown.Tests.csproj
dotnet test Toolbox.sln
```

`MarkdownIncrementalBlockScannerTests` cover complete, disjoint span coverage with mixed line endings; headings, blanks, long paragraph and code spans; explicit unterminated fence handling; opaque quote/list boundaries; Unicode positions; forward-only laziness; cancellation; and error propagation. A restricted flat-document corpus is compared with `MarkdownParser.Parse` to ensure heading, blank, paragraph and fence types match **without claiming full parser equivalence**.

The source and tests are committed; consult the new GitHub Actions run before claiming a test pass.

## Follow-on work

1. Model nested quote/list containers as stateful completed block regions while preserving owner metadata from the existing full parser; add golden parity tests for real README examples.
2. Add incremental source offset conversion and a disk-backed block span index.
3. Build a visual-row-aware renderer/pager; then implement the separately planned [indexed README topics and F/N/P navigation](../../../../Blackboard/Tools/MdView/Docs/Indexed-Search/en.md).

[Markdown README](../../README.md) · [Implementation status](../../IMPLEMENTATION_STATUS.md).
