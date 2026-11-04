# Incremental Markdown source-line scanner

**Status:** isolated, opt-in first milestone committed 2026-11-04. The first implementation is a **forward-only, conservative physical-line scanner**, not a complete streaming Markdown parser and not connected to the current renderer.

## Purpose

The existing `MarkdownParser.Parse(string)` builds a full `MarkdownDocument` in memory and tracks detailed source offsets, list ownership, quotes, inlines and fence metadata. Those semantics should not be sacrificed to achieve small RAM use.

`MarkdownIncrementalLineScanner` provides a separate `IAsyncEnumerable<MarkdownStreamLine>` API that can consume any decoded `TextReader` in 4 Ki-character chunks, yield completed physical lines without buffering the whole document, and retain the state of a **top-level** fenced code block. This proves out the source-position and state transitions required for eventual paged rendering.

```csharp
using Afrowave.Toolbox.ExchangeHouse.Markdown;

using var reader = new StringReader("# Installation\r\n```bash\nmake\n```\n");
await foreach (var line in MarkdownIncrementalLineScanner.ScanAsync(reader))
{
    // line.Kind: Heading, FenceOpen, CodeLine, FenceClose, ...
    Console.Write(line.Raw); // illustration only; actual terminal output needs validation
}
```

The returned event contains:

- `LineNumber`: zero-based physical line index.
- `Utf16Offset`: source offset in **decoded UTF-16 code units**, compatible with the existing Markdown parser's string offsets; it is not a UTF-8 byte offset.
- `Text` and `Terminator`: exact original decoded line and one of `"\r\n"`, `"\r"`, `"\n"` or `""`. `Raw` reconstructs the original physical line.
- `Kind`: conservative `Blank`, `Heading`, `FenceOpen`, `CodeLine`, `FenceClose`, `OpaqueContainer` or `ParagraphLine`.
- `HeadingLevel`: only for ATX headings (1–6).
- `CodeLanguage`: first 64 characters or fewer of the first word in a top-level fence's information string; null if not applicable. The same hint remains associated with code lines and the closing fence.

## Important parsing boundaries

Top-level fenced code markers follow the current parser's conservative rule: three or more matching backticks or tildes with at most three preceding spaces. A closing marker must match the opening character, be at least as long and have only trailing spaces/tabs. A `# Heading` inside an open fence remains literal `CodeLine`, not a heading. An unfinished fence remains active until EOF; no source line is discarded.

ATX headings use 1–6 hash signs with a required following space/tab or immediate line end. Explicit quote/list starts are marked as **opaque containers**, not parsed as ordinary headings; their structure and continuation ownership remain to be implemented. In particular, quoted and list-owned code fences are not yet recognized as active fence scopes. Plain paragraph lines are emitted individually; the scanner does not group paragraphs, resolve reference-style links, implement setext headings, wrap visual rows or claim CommonMark parity.

This layer does not modify `MarkdownParser.Parse`, `MarkdownWriter`, the Spectre renderer, TextMate coloring or `mdview` output. It can evolve independently under source-compatible tests.

## Resource and security contract

- Input: an already decoded `TextReader`; the caller owns and disposes it.
- Read buffer: **4,096 chars**, with one growing physical-line buffer bounded by `maxLineCharacters` (default 1,048,576, accepted range 1 to 4,194,304).
- Completed lines are yielded as soon as their full physical terminator is seen, even when CR and LF arrive in separate reads. No synthetic empty line is emitted after a final newline.
- Control characters except tab, CR and LF are rejected. The scanner never executes code, resolves links or interprets Spectre markup.
- This is a **progressive stream**: if invalid text or an overly long line occurs late, earlier lines may already have been yielded. **Do not show untrusted progressive content before a full validation pass** if all-or-nothing safety is required. The existing strict `TextFiles.Utf8DiskLineIndex.OpenAsync` provides a complete UTF-8 verification/indexing pass; a future adapter can feed its validated lines to the stream parser. A `TextReader` itself is not proof of a valid original UTF-8 file, nor does this scanner attempt to validate UTF-16 surrogate pairing.
- Cancellation is checked during reading and before each yielded line. A stopped consumer can stop reading immediately.

## Verification

```bash
dotnet test ExchangeHouse/Markdown.Tests/Afrowave.Toolbox.ExchangeHouse.Markdown.Tests.csproj
dotnet test Toolbox.sln
```

The newly added `MarkdownIncrementalLineScannerTests` check exact newline round-trips and decoded offsets, one-character-at-a-time reads, CRLF across chunk boundaries, opening/closing/unterminated fences and language hints, literal quote/list container lines, basic agreement with the existing heading parser, Unicode offsets, read-budget rejection, late unsafe controls, forward-only incremental yield, cancellation and invalid arguments.

**Tests are authored but unverified** in this execution environment (no .NET SDK); the last maintainer-confirmed full Windows baseline predates this new scanner.

## Next increments

1. Build a proper state machine that tracks list and quote ownership and emits complete, safe *block events* with bounded lookahead.
2. Compare restricted streaming output against the existing full `MarkdownParser` across an expanding golden corpus; preserve known offsets and quote/list/fence semantics.
3. Build a visual-row pager with disk-backed source/block metadata and reversible navigation, only after semantic equivalence is measured.
4. Integrate optional, digest-bound term/heading indexes for future `F/N/P` search **after** page navigation exists.

[Parser README](../../README.md) · [Implementation status](../../IMPLEMENTATION_STATUS.md) · [Future indexed search](../../../Blackboard/Tools/MdView/Docs/Indexed-Search/en.md).
