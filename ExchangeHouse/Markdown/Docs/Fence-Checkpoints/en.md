# Disk-backed Markdown fence checkpoints (experimental)

**Status: implemented in the Markdown draft branch; not yet published.**

The low-memory source pager can seek arbitrary raw UTF-8 source pages via
`MarkdownDiskPageIndex`. But a physical page may start with code that belongs
to a fence opened hundreds of pages earlier. Parsing that page as an
independent document can turn `# not a heading` into an incorrect heading.

`MarkdownFenceCheckpointIndex` addresses this **specific** problem without
loading the complete source or a page index into memory.

## What it stores

A second disposable OS Temp file holds a **fixed 512-byte record for each
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

## Bounded list, quote and paragraph candidates (2026-11-02)

Each 512-byte record now additionally stores **up to 32 list indentation
columns** (one byte per candidate), the previous physical line's quote
depth, and whether that line might belong to an unfinished prose paragraph.

The scanner and checkpoint writer share one `AdvanceCheckpoint` state
transition, avoiding a second independently implemented list grammar.
After a seek, `MarkdownStreamLine.ListDepth` is restored from this bounded
candidate stack. The new `IsParagraphContinuationCandidate` flag is true
when consecutive prose lines share the same explicit quote depth, including
at a page boundary. An intervening blank line, fenced code or a change of
quote depth interrupts that candidate continuity. The name **candidate**
is deliberate: Markdown permits lazy list and quote continuations,
setext headings and other constructs requiring richer block parsing.

This does not preserve a complete logical paragraph or a full list/tree
AST. The streaming Spectre renderer has no guarantee of visually identical
list indentation compared with whole-document parsing. For exact input,
use source mode; for fully modeled small documents use the existing
`MarkdownParser.Parse` path.

## What is not yet solved

This is a **fence checkpoint**, not a complete CommonMark block-container
checkpoint. Complete list continuation ownership, non-fenced blockquote structure,
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


## List item owner offsets and prose continuation (2026-11-02)

The fixed **512-byte checkpoint** is still bounded, but now also stores a
stack of at most **32 list owner descriptors**. Each descriptor contains a
64-bit **global UTF-16 source offset** of the list marker and its indentation
and content-indentation columns. A page can thus resume a nested list
without retaining its preceding items or copying their text.

`MarkdownStreamLine.ListOwnerStart` identifies the conservatively recognized
owner; `IsListContinuationCandidate` differentiates an earlier item's
continuation from a newly declared marker. Explicitly indented paragraphs
may return to a parent list item after a nested child. Immediate unindented
**lazy** continuation is a candidate only without intervening blank/fence
or quote-container transition. Blank-separated continuation requires
sufficient explicit indentation.

The scanner and disk index compute owner transitions through the same
`MarkdownStreamingReader.AdvanceCheckpoint` routine. Reverse-page
regression tests compare these owner offsets to an uninterrupted scan.

**Important:** these are conservative Markdown *ownership candidates*, not
a complete CommonMark list AST, lazy block quote parser, or syntax-perfect
renderer. The old 128-byte experimental record format was temporary and
has no compatibility promise; each index is regenerated on startup. A file
with extremely many short lines may exhaust the independent checkpoint
disk quota before reaching EOF, even though reading individual pages is
bounded. List nesting beyond 32 candidates is not preserved.


## Sparse checkpoints and bounded replay (2026-11-02)

The **current** default is now one persisted **512-byte record per 16
source pages**, rather than one record per page. `CheckpointStride` is
configurable from 1 (the original dense behavior) to 256 pages through
`MarkdownFenceCheckpointIndex.BuildAsync(..., checkpointStride: 16)`.
`PageCount` remains the total source-page count;
`SavedCheckpointCount` counts records physically stored on disk.

To visit page N, `ReadCheckpointAsync(N)` reads the record at
`floor(N / stride) * stride`, then **replays at most stride - 1 earlier
source pages** using the same streaming scanner and
`AdvanceCheckpoint`. The reader holds one bounded page's text at a time;
nothing from an earlier page needs to stay allocated after replay.
`ReadPageAsync(N)` then classifies exactly the requested source page.

This is a disk-space / random-seek-CPU tradeoff, **not free constant-time
random access**. Large strides reduce temporary disk usage, but increase
source I/O and reclassification work at arbitrary seeks. For 1,000,000
physical lines with 40 lines per page, the default saves just 1,563
checkpoint records (800,256 bytes), compared with 25,000 records
(12,800,000 bytes) at stride 1. The underlying **8-byte/page byte offset
index is unchanged**.

The checkpoint disk quota (default 256 MiB) applies to the number of
**stored** records, not the total number of pages. The full source is
still validated and scanned twice during index creation; sparse state
does not skip that validation. Replay also checks that the source has
not changed via the underlying source index; cancellation and errors
are propagated. No persistent cross-session index is introduced.
