# Disk-backed conservative Markdown list spacing groups

**Status:** optional experimental ExchangeHouse API on draft PR #3.
This index is not automatically built by the sixth Showcase viewer and
is not a CommonMark-equivalent tight/loose list parser.

## Purpose

A line-by-line streaming reader cannot know that an earlier list item
needs extra paragraph spacing until it has seen later items. Keeping all
previous list-item objects and paragraphs in memory would defeat the
low-RAM design. `MarkdownListSpacingIndex` performs one **additional**
bounded-memory sequential scan and writes only list-group summaries and
item-marker lookup records into two uniquely named OS Temp files.

The active in-memory list-group stack is capped at 32 descriptors.
The two indexes each use fixed-size records:

- **24 bytes per list group:** source start/end (global UTF-16 offsets),
  depth, quote depth, and a loose-list *candidate* bit.
- **16 bytes per list item:** source start offset and list-group record
  number. Sorted by increasing source position for binary lookup.

The default combined temp-file quota is 256 MiB, independently of the
8-byte-per-page raw byte offset index and the sparse 512-byte checkpoint
index. Both files are deleted when disposed, or when construction fails
or is canceled. Source Markdown files are never rewritten.

## Usage

```csharp
using Afrowave.Toolbox.ExchangeHouse.Markdown;

await using var pages = await MarkdownDiskPageIndex.BuildAsync("guide.md");
await using var spacing = await MarkdownListSpacingIndex.BuildAsync(pages);

// Source offsets are UTF-16 character offsets produced by the scanner.
var group = await spacing.FindForItemAsync(markerStart: 0);
if (group is not null)
{
    Console.WriteLine(
        $"Group {group.Start}..{group.End}, loose candidate: {group.IsLooseCandidate}");
}
```

`FindForItemAsync` performs a disk-backed binary search over marker
offsets, then reads one group record. A caller with a continuation line
can use its `MarkdownStreamLine.ListOwnerStart` as the lookup key. It
returns `null` for an unrecognized item offset. `GroupCount` and
`ItemCount` are available without reading all the records.

Unlike single-line `ListSpacingHint`, the group decision is available
**retroactively** to all items after the group has finished scanning.
A later blank-separated sibling marker or a blank-separated paragraph
inside an earlier item sets `IsLooseCandidate` for the observed group.

## Tradeoffs and limits

This is a conservative, experimental grouping of line-scanner output,
not a definitive CommonMark list grammar. Marker changes and explicit
quote-depth changes separate groups; nested list groups are indexed
independently, which is not sufficient to model every CommonMark
tight/loose propagation rule. Lazy block quotes, indentation edge cases,
list interrupts and retroactive container semantics require additional
work. The result is **evidence**, not a guarantee of perfect rendering.

The additional pass increases CPU and disk I/O during opening. For
example, N list items need approximately `16 * N` bytes in the item
index; the group index uses `24 * numberOfGroups` bytes. A document
with an exceptional number of list markers can hit the configured
disk quota. The read path does not cache the complete table in RAM.
Do not claim O(1) group lookups: each request is a disk binary search.

The optional index currently is **not integrated** into the Showcase
TUI or published on NuGet. Enabling it for every file would add a
third whole-file scan, so consumer-controlled opt-in is intentional.

See [scanner details](../Streaming-Scanner/en.md),
[sparse state checkpoints](../Fence-Checkpoints/en.md), and
[low-memory Linux acceptance plan](../../../../Docs/Low-Memory-Linux-Acceptance/en.md).
