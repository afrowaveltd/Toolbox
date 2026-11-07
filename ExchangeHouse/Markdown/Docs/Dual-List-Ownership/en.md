# Incremental Markdown: separate outer-list and quoted-list owners

**Implemented:** 2026-11-07, conservative additive metadata in `MarkdownIncrementalContainerScanner`. This page describes only *explicit* ownership, not CommonMark-complete list/quote parsing or an enabled mdview feature.

## Why two owner identities are necessary

Consider this Markdown:

```markdown
- Outer item
  > - Inner quoted item
  >   - Nested quoted item
  >     continuation
- Next outer item
```

The content is inside two **different list hierarchies**. The *outer ordinary list* owns the quote container; the *quote-local list* owns its own numbered/bulleted items and explicitly indented text. Tracking only one stack would either lose the enclosing item, or incorrectly nest the quoted list under the ordinary list.

The full `MarkdownParser.Parse` already distinguishes `MarkdownBlock.QuoteContainerListOwnerStart` from `MarkdownBlock.ListContinuationOwnerStart` and the list-marker owner. The incremental scanner now keeps the same **separation of identities** where it can prove ownership directly from explicit source indentation.

## Public metadata

`MarkdownContainerLine` retains its existing positional record constructor and adds three init-only properties:

| Property | Meaning |
| --- | --- |
| `QuoteContainerListOwnerUtf16Offset` | UTF-16 source offset of the **outer ordinary-list marker** that explicitly owns this quote |
| `QuoteContainerListDepth` | Zero-based nesting depth of that outer ordinary-list item, or `-1` without an owner |
| `QuoteContainerListMarker` | Original source marker (`-`, `1.`, `2)`, etc.) of the outer item |

The existing `ListOwnerUtf16Offset` remains the owner of the **current list hierarchy**. On an explicitly quoted line, this means the quote-local list; on an ordinary line, the ordinary list. `ListDepth` is similarly relative to that hierarchy. All offsets are from the original decoded UTF-16 stream, **not UTF-8 byte offsets**.

For a quote line with an outer list but *no inner list*, `QuoteContainerListOwnerUtf16Offset` may be present while `ListOwnerUtf16Offset` stays null. On an inner quoted list item, both are present and generally identify different source positions. The `Source` property preserves the original physical line, character offset and CR/LF/CRLF ending.

## Scanner state and boundaries

- Two independent lists of frames are maintained: ordinary-list frames (up to 32) and quote-local list frames (up to 32). Each frame records its original marker, marker indent, content indent and source offset. Indentation uses four-column tab stops, bounded to 256 columns.
- Before accepting a line as a standalone `>` quote, the scanner probes whether its leading indentation explicitly matches a previously known ordinary-list item. This is important for `"  > ..."` belonging to `"- item"`.
- Quote-local frames are invalidated whenever either **quote depth** or **outer ordinary-list owner identity** changes. A new outer item cannot inherit another item's quoted-list stack, even if both use exactly one `>` character.
- If a standalone quoted line cannot be assigned to an ordinary list, the previous ordinary-list frames are cleared. A later indented quote cannot revive a stale owner.
- Fences inside quoted content hold their original **outer owner** and optional **inner quote-list owner** for every body line and the matching closing delimiter. The quote depth and outer owner must remain stable for the fence to continue. An interruption reclassifies the current line without inventing a close.
- Indented fenced-code delimiters inside a list can be detected after removing *provably owned* indentation. A fence without quote-local list indentation remains a quote-owned block, not an attachment to the previous inner item.
- The syntax is bounded and forward-only: one source line plus at most 32 frames per list stack and constant-sized fence state, rather than buffered complete documents or code blocks.

## What the scanner intentionally does not do

The scanner does **not** infer unindented lazy list paragraphs, deferred ownership of blank lines in loose lists, all nested quote/ordinary-list combinations, semantic inline formatting, setext headings, or visual terminal rows. A blank line may have a proven *outer* quote owner without any speculative *inner* list owner. A source line may be conservatively classified as `Quote` instead of a fully parsed nested block.

The existing `MarkdownParser` and `MarkdownWriter` remain the semantic baseline; `mdview` continues using the existing renderer and no new search/navigation controls are enabled here.

An arbitrary `TextReader` is not evidence of valid UTF-8. The progressive scanner may yield early lines before detecting a late invalid control character; do not expose untrusted progressive output to a terminal until the original file has passed whole-source validation (for example through `TextFiles.Utf8DiskLineIndex`).

## Opening code-fence indentation parity (2026-11-07)

The whole-document `MarkdownParser.Parse` now uses the same conservative
opening-fence indentation rule as `MarkdownIncrementalContainerScanner`
for explicitly continued ordinary-list and quote-local list items:
strip **only the list item's required content indentation** before testing
the delimiter. Up to three additional spaces are permitted; four or more
additional spaces remain indented content rather than opening a fence.
The full parser's original source and paragraph content offsets are kept.

For example, under a `- item` marker (content indentation of two columns),
a fence preceded by two through five spaces is recognized; one preceded by
six spaces is not. The same boundary applies after a quote prefix, such as
`> - item` and a quote-prefixed fence marker with six spaces after `>`.

The regression `FenceOpeningIndentationBoundaryMatchesFullParser` covers
both ordinary and quote-local lists at all five boundaries, checking
classification, fence offsets and unchanged-source round-tripping.
The earlier `OverIndentedListFenceOpenerIsNotRecognizedAsCode` regression
and its quoted counterpart cover the conservative rejection case.

This closes **one specific previously recorded compatibility discrepancy**;
it is not proof of complete CommonMark, nested container or tab-indentation
equivalence. Do not switch the formatted `mdview` renderer to the streaming
parser without broader golden comparisons and fresh passing CI.

## Tests and verification

```bash
dotnet test ExchangeHouse/Markdown.Tests/Afrowave.Toolbox.ExchangeHouse.Markdown.Tests.csproj
dotnet test Toolbox.sln
```

`MarkdownIncrementalQuoteOwnershipTests` contains targeted regressions for adjacent outer items, nested ordered and unordered outer lists, independent inner list items/continuations, quote-only metadata, changes in quote depth, mixed newlines/Unicode offsets, literal fences, and early streaming output. Supported cases compare owner source offsets to `MarkdownParser.Parse` instead of checking only the new parser's self-consistency. See repository CI for the verified results.

## Next implementation steps

1. Add positive tests for the original full parser's deferred blank/loose-list ownership and distinguish those *unresolved* cases in the streaming API rather than guessing.
2. Build completed container-aware block spans with explicit interruption and scope boundaries.
3. Convert validated source offsets to byte-positioned, disk-backed block indexes for a colored Markdown pager, then implement on-demand topic/phrase indexing.

[Markdown README](../../README.md) · [Explicit container scanner](../Explicit-Container-Scanner/en.md) · [Implementation status](../../IMPLEMENTATION_STATUS.md).
