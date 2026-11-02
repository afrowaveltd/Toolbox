# Disk-backed Markdown fence checkpoints (experimental)

**Status: implemented in the Markdown draft branch; not yet published.**

The low-memory source pager can seek arbitrary raw UTF-8 source pages via
`MarkdownDiskPageIndex`. But a physical page may start with code that belongs
to a fence opened hundreds of pages earlier. Parsing that page as an
independent document can turn `# not a heading` into an incorrect heading.

`MarkdownFenceCheckpointIndex` addresses this **specific** problem without
loading the complete source or a page index into memory.

## What it stores

A second disposable OS Temp file holds a **fixed 96-byte record for each
physical-line page**. Each record contains the global UTF-16 character offset
of its page boundary and, when a fenced block is open:

- Marker character (backtick or tilde), exact marker-run length.
- Character offset of the opening fence.
- Quote depth of the enclosing explicit quote container.
- Optional UTF-8 code-language hint of at most 64 bytes. A longer hint is
  omitted, never truncated to an invalid Unicode sequence.

Nothing from the completed text of the unfinished fenced block is retained.
The index has a configurable disk quota (256 MiB by default), and the
underlying source index has its own independent disk quota. Files are
deleted on successful disposal and on indexing errors/cancellation.
Use at most the configured physical-page text budget in RAM.

## Usage

```csharp
using Afrowave.Toolbox.ExchangeHouse.Markdown;

await using var source = await MarkdownDiskPageIndex.BuildAsync(
    "/path/to/guide.md");

await using var fences = await MarkdownFenceCheckpointIndex.BuildAsync(source);

// Jump directly to a distant page, including its previously-opened fence.
var page = await fences.ReadPageAsync(200);
foreach (var line in page.Lines)
{
    Console.WriteLine($"{line.Kind}: {line.Content}");
}
```

`MarkdownContextPage` has `Source`, `Checkpoint` and `Lines`. The lines
come from `MarkdownStreamingReader.ScanAsync` resumed from the saved state,
with **document-global UTF-16 offsets**. The scanner also retains source
line endings as before. Code lines are classified as `CodeContent` even
when the opening fence was on an earlier page.

The companion index currently performs a second bounded-memory sequential
source scan **after** the UTF-8 source index has been built. This is
deliberate: it leaves the original raw-source index stable and independently
useful. Later optimization may collect both indices in one streaming pass.

## Interactive demonstration

The sixth `Blackboard.Showcase` Markdown option still defaults to **literal
source**. It now offers `Show conservative Markdown preview` and
`Show literal source` as a reversible display toggle, plus Next, Previous
and Close. The optional preview uses the resumed scanner and the existing
Blackboard Spectre renderer. Unlike page-local parsing, code fence state
survives arbitrary page jumps.

## What is not yet solved

This is a **fence checkpoint**, not a complete CommonMark block-container
checkpoint. List continuation ownership, non-fenced blockquote structure,
paragraph merging, reference-style link resolution, syntax-highlighter
session state and terminal-width-aware page breaks remain incomplete.
The preview can still differ from full-document Markdown rendering.

Only strict UTF-8 sources (optional UTF-8 BOM) participate in these disk
indices. The separate in-memory Showcase viewer still supports BOM-marked
UTF-16 and UTF-32. A static source is required during both indexing passes.
No live HTTP-growing-file pager or translation implementation is claimed.

See [source byte-offset paging](../Disk-Backed-Paging/en.md),
[streaming scanner](../Streaming-Scanner/en.md) and
[Showcase pager](../../../../Blackboard/Samples/Blackboard.Showcase/Docs/Markdown-Paged-Source-Viewer/en.md).
