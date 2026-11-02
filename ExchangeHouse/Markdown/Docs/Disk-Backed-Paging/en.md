# Disk-backed Markdown source paging (experimental)

**Status (2026-11-02): implemented on the Markdown streaming draft branch;
not a published NuGet feature.** This is a bounded-memory **source index**,
not a contextual Markdown AST or a completed interactive pager.

## Why a second index?

`MarkdownStreamingReader.ScanAsync(TextReader)` knows the active fence and
streaming character offsets but is forward-only. A user who has reached a
distant page must be able to go back without retaining every previously read
line or replaying gigabytes of input.

`MarkdownDiskPageIndex` stores **only one 8-byte little-endian UTF-8 byte
offset per page**, in a uniquely named disposable OS Temp file. It can later
seek any page with an independent source stream. It does not load the complete
Markdown text, the complete index or the entire syntax tree into RAM.

The default page contains 40 physical lines; a physical line has a configurable
64 KiB maximum in UTF-8 bytes, with hard configuration bounds (up to 128 lines
and 1 MiB per physical line). Temporary index storage is also capped
(default 256 MiB, configurable with `MaxIndexBytes`) so an adversarial
file containing billions of one-byte lines cannot exhaust disk unchecked.
A page is bounded by those limits, even if the
source is several gigabytes long. The index space is proportional to **number
of pages**, not byte length; e.g. 1,000,000 lines at 40 lines/page yields
25,000 eight-byte offsets, about 200 KiB on temporary disk.

## API

```csharp
using Afrowave.Toolbox.ExchangeHouse.Markdown;

await using var pager = await MarkdownDiskPageIndex.BuildAsync(
    "/path/to/README.md",
    new MarkdownDiskPageOptions
    {
        LinesPerPage = 40,
        MaxLineBytes = 64 * 1024,
        MaxIndexBytes = 256L * 1024 * 1024
    },
    cancellationToken);

Console.WriteLine($"{pager.LineCount} lines, {pager.PageCount} pages");

var last = await pager.ReadPageAsync(pager.PageCount - 1, cancellationToken);
var first = await pager.ReadPageAsync(0, cancellationToken);
// Each page reports its number, first source line, source byte range,
// physical line count and original text.
Console.Write(first.Content);
```

Only seek if `PageCount > 0`: empty source has no pages.

## UTF-8 and safety

- Accepts strict UTF-8 with an optional UTF-8 BOM. Invalid UTF-8, embedded
  NUL, ESC and other unsafe Unicode controls (except TAB/CR/LF) fail indexing.
- Counts CRLF as one newline and also supports LF and CR line endings.
- The byte offsets are measured in the original source bytes; each page is
  decoded with a strict UTF-8 decoder. The BOM is omitted from the first
  page's displayed content.
- Never silently truncates a line; exceeding `MaxLineBytes` throws.
- Validates the full source during the initial scan; the scan cost is
  proportional to source size. Seeking afterward reads only the requested
  offset pair and page range.
- On cancellation or failure, a partial index is deleted. Disposal deletes
  the completed index, **not the original source**.
- File size and last-write timestamp are rechecked before page reads to catch
  ordinary source changes; same-size changes with forged or preserved
  timestamps cannot be ruled out. Do not use as a hostile-file security
  boundary.

## Important scope

`MarkdownSourcePage.Content` is **raw Markdown source**. A page may begin
inside a fenced code block, quotation, list or multiline reference. Passing
it alone to `MarkdownParser.Parse` does **not** reconstruct the exact
whole-document meaning. A fully formatted bidirectional reader still needs
an incrementally indexed semantic state checkpoint or a correct lookback
strategy. The current bounded forward scanner and the disk byte index are
complementary, not equivalent.

The disk index presently supports UTF-8 only; BOM-marked UTF-16/32 are still
supported by the existing **in-memory Showcase file viewer**, but are not
yet indexed by this API. A future encoding-aware source adapter can bridge
them without changing the indexing contract.

The raw-source index remains independently useful. To persist only
the bounded fence/list/quote/paragraph candidate state at page boundaries, use the optional
[512-byte bounded ownership checkpoint index](../Fence-Checkpoints/en.md) and call
`ReadPageAsync` on that companion index. Its resumed scanner recognizes
code content when the opener is on a preceding page; it does **not**
reconstruct all CommonMark list, quote or reference semantics.

## Remote document integration (future)

`RemoteMarkdownTempDocument` in the Showcase already spools a trusted HTTPS
response to an ordinary seekable Temp file. After its download completes,
the same index can be built over that file:

```csharp
await using var remote = await RemoteMarkdownTempDocument.DownloadAsync(
    httpClient, uri, cancellationToken: cancellationToken);
await using var pager = await MarkdownDiskPageIndex.BuildAsync(
    remote.TempPath, cancellationToken: cancellationToken);
```

This example spans **two projects** and is not yet a Showcase menu option.
Neither HTTP live-download paging nor continuous translation nor a
bidirectional terminal viewer is implemented by this index.

See [streaming scanner](../Streaming-Scanner/en.md) and the
[Showcase remote-spool documentation](../../../../Blackboard/Samples/Blackboard.Showcase/Docs/Remote-Markdown-Cache/en.md).
