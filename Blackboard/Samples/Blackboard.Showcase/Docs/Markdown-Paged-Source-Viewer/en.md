# Indexed local Markdown source pager (Showcase experiment)

**Available on the Markdown streaming draft branch (2026-11-02).**
The sixth Showcase menu item, **Markdown — disk-indexed source pages
(experimental)**, is an interactive proof of a low-RAM bidirectional reader.

## Try it

```bash
dotnet run --project Blackboard/Samples/Blackboard.Showcase
```

Select the sixth item, choose an existing local `.md` or `.markdown`
through FileBrowser, and use **Next page**, **Previous page** or **Close
pager** (Escape). Each default page holds 40 physical Markdown source lines.
The reader validates the input first, writes a small temporary byte-offset
index on disk, and loads only the currently requested page. Both the
source and the directory containing it remain read-only.

This demo intentionally prints **literal Markdown source**, not parsed/
syntax-colored Markdown presentation. A 40-line page could start in the
middle of a code fence or list, and interpreting it by itself could produce
incorrect formatting. Correct contextual rendering requires later
parser-state checkpoints. The fifth Showcase demo remains the complete,
fully formatted but in-memory Markdown viewer.

## Why this matters

The index uses 8 bytes per page on temporary disk; its growth is not tied
to the text length in RAM. You can visit page 100, then page 5, without
rereading the intervening pages. Empty files, malformed UTF-8, embedded
terminal controls and oversized single lines are handled explicitly.
Only strict UTF-8 with an optional UTF-8 BOM is currently supported here
(the existing complete viewer separately supports BOM-marked UTF-16/32).

**Current limits:** the entire file is scanned once to build/validate its
index before reading starts; download-as-you-read, incremental index growth,
cross-page semantic rendering, terminal-width-aware visual page heights and
search are not yet implemented. Large-file *capacity* follows from the
bounded design, but the 1 GiB/10 GiB end-to-end interactive viewer has
not been acceptance-tested. Avoid claiming CommonMark equivalence.

The underlying UI-neutral API is
[ExchangeHouse.MarkdownDiskPageIndex](../../../../../ExchangeHouse/Markdown/Docs/Disk-Backed-Paging/en.md).
The remote spool already produces a seekable Temp file and can be indexed
**after** download, but this menu still selects local files only.
See [remote caching](../Remote-Markdown-Cache/en.md).

## New fenced-code-aware preview

The source page demo also builds a separate on-disk fence checkpoint index
and provides a reversible **Show conservative Markdown preview** toggle.
It preserves fenced-code classification across page boundaries, including
quoted fences and mismatched closure lengths. The preview is still incomplete
for paragraph/list/reference semantics. Use the default literal source
mode whenever exact raw text matters. See
[checkpoint design](../../../../../ExchangeHouse/Markdown/Docs/Fence-Checkpoints/en.md).

## Restored list/quote/paragraph hints (2026-11-02)

The conservative preview receives list indentation candidates plus
previous-line quote depth and plain-paragraph continuity from a fixed-size
512-byte checkpoint per page. This reduces loss of context when seeking
into a long list or a repeated quote, but **does not guarantee** correct
nested-list formatting or CommonMark paragraph ownership. Literal-source
mode remains the default and the authoritative fallback.


## Actual list-owner offsets (2026-11-02)

Indexed source lines now expose a conservative **owner source offset** for
nested and continuing list paragraphs. Unlike the earlier depth-only
checkpoint, a later page can distinguish the containing parent from a
child list item. The source-preview menu is unchanged; the renderer has
not yet reached complete contextual CommonMark formatting. Literal
source remains the safe default.


## Sparse checkpoint replay (2026-11-02)

Instead of persisting a full 512-byte structural record on every page,
the preview now uses an index with default **16-page spacing**.
Requested pages in between recover their context by replaying only the
intervening pages, one at a time. The original 8-byte/page raw-source
offset index and the Next/Previous menu are unchanged. Replay is
bounded (maximum 15 source pages for the default), not instantaneous;
small strides may be preferable for very large physical lines or
high-frequency random seeks. [Technical details](../../../../../ExchangeHouse/Markdown/Docs/Fence-Checkpoints/en.md).
