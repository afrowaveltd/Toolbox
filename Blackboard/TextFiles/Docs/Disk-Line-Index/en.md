# Strict UTF-8, disk-backed physical line index

**Implementation:** 2026-11-04. This is the first *source indexing* milestone toward a future native Markdown pager. It is **not** a streaming Markdown parser, a wrapped visual-line index, or a replacement for the existing complete-document renderer.

## Goal and boundaries

Large README and help files should be navigable without allocating memory proportional to the entire file. `Utf8DiskLineIndex` provides a reusable, UI-independent primitive in `Afrowave.Toolbox.Blackboard.TextFiles`, with the existing whole-document API unchanged.

On `OpenAsync`, the reader opens a local file **read-only**, validates every byte as strict UTF-8 and rejects unsafe terminal controls (except CR, LF and tab). This is a sequential, bounded-buffer pass, so a malformed sequence **at the end** rejects the file **before any page is displayed**. UTF-8 BOM is supported. The reader writes physical line start offsets to a temporary disk file; it never stores the full offset table in RAM.

CRLF (including across scan-buffer boundaries), LF and CR are treated as line endings. The index stores one 64-bit offset per physical line plus a final file-length sentinel. A trailing newline does not introduce a phantom line; internal empty lines are retained. Files with UTF-16 or UTF-32 BOMs are **not** accepted in this initial indexed mode; the existing `TextFilePreviewReader.ReadCompleteAsync` still supports those encodings for normal fully rendered Markdown documents.

## Public API

```csharp
using Afrowave.Toolbox.Blackboard.TextFiles;

await using var index = await Utf8DiskLineIndex.OpenAsync("README.md");
Console.WriteLine($"Lines: {index.LineCount}; disk index: {index.IndexBytes} bytes");

var page = await index.ReadPageAsync(firstLine: 0, maxLines: 24);
foreach (var line in page.Lines)
    Console.WriteLine(line);

if (page.HasMore)
{
    var next = await index.ReadPageAsync(page.NextLine, 24);
    // Reverse lookup also works: index.ReadPageAsync(0, 24).
}
```

`Utf8LinePage` returns the zero-based `FirstLine`, the continuation point `NextLine`, `TotalLines`, a bounded `Lines` list, and `HasMore`. The page is read using source/offset seeks; it is possible to request an earlier physical line without reparsing the entire document. All source text is rendered literally by callers, never interpreted as terminal markup.

## Resource budgets

- Scan/validation buffer: **64 KiB** of source bytes plus a small UTF-8 decoder buffer.
- Temporary offset table: **8 bytes per line + 8 bytes sentinel**, capped at **256 MiB** by default. A budget overrun reports an explicit error rather than partially indexing a file.
- One physical line: maximum **1 MiB** by default; configurable in the `OpenAsync` call from 4 KiB to 4 MiB. Oversized lines are reported explicitly on page read and never silently cut off.
- Page: up to **128 physical lines** and **4 MiB of source bytes**. A page may stop early at the byte budget; `NextLine` preserves the exact continuation point.
- Temporary index lifecycle: the random index file is opened with `DeleteOnClose` and removed when the index is disposed or indexing fails. It is *derived* local metadata; it is unrelated to the separately persistent and offline-friendly remote Markdown document cache.
- Initial scan: **O(file bytes)** before the first displayed page. Page reads after indexing seek to their indexed byte range. No constant-time first-page claim for a new multi-gigabyte source.

A successful scan validates the complete byte stream at that point. Page reads recheck each decoded line and source length to detect common modifications, but concurrent in-place same-length mutation cannot be perfectly excluded on every filesystem. For untrusted or mutable files, a future immutable snapshot/spool policy must provide stronger guarantees.

## Explicit source pager in mdview

The first consumer is a separate **raw source** pager:

```bash
dotnet run --project Blackboard/Tools/MdView -- source README.md
```

This shows complete validated physical lines with their line numbers and controls for Next page, Previous page, Go to line and Exit. It is deliberately **not** advertised as a rendered Markdown pager: it does not preserve Markdown block state, syntax colors, inline links, soft-wrap visual rows, heading navigation or browser history. The ordinary `mdview local README.md` and GitHub/HTTPS commands still use the existing complete formatted renderer.

## Verification

```bash
dotnet test Blackboard/TextFiles.Tests/Afrowave.Toolbox.Blackboard.TextFiles.Tests.csproj
dotnet test Blackboard/Tools/MdView.Tests/MdView.Tests.csproj
dotnet test Toolbox.sln
```

Regression tests cover UTF-8 BOM and Unicode, CRLF/LF/CR, cross-buffer multibyte code points and CRLF, forward/backward pages, final newline semantics, late unsafe bytes, invalid UTF-8, UTF-16 rejection, per-line and index budgets, byte-limited pages, cancellation and invalid parameters. CLI virtual-terminal tests cover paging, jump, cancellation, literal markup and rejection of unsafe input.

**No build/test execution is confirmed for this increment** in the current environment without .NET SDK. Earlier user-confirmed 6,443/6,443 Windows tests refer to previous source snapshots.

## Next Markdown milestones

1. A source-aware incremental Markdown block parser with explicit fence/list/quote state and bounded lookahead.
2. Disk-backed rendering/block index and a visual-line pager (terminal-width wrapping, colors, reverse navigation, navigation to headings and links).
3. Optional persistent/reusable content-addressed line indexes for unchanged downloaded documents, avoiding repeated full scans.
4. UTF-16/UTF-32 source indexing or safe transcoding to an immutable UTF-8 spool where appropriate.
5. Measured 1 GiB+ files on slow HDD/old hardware, including cancellation, Unicode/emoji and terminal resize.

[TextFiles README](../../README.md) · [Implementation status](../../IMPLEMENTATION_STATUS.md) · [mdview source pager](../../../Tools/MdView/Docs/Raw-Source-Pager/en.md).
