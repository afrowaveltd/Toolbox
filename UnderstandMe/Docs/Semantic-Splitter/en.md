# Semantic Splitter: Meaningful Chunks by Input Format

Status: agreed functional direction (2026-10-10); design only. No frozen C# types or parsing implementation.

## Principle

**Split meaningful units, not arbitrary lengths of text.** Chunking is primarily for larger or structured content, or when the target provider imposes a size limit. A short ordinary message or phrase can be sent **as a single unit**, without needless splitting.

A Splitter is a pluggable **format-aware preprocessor**. It creates a request-local ordered `List<(Id, Text)>` with `Id = 0..N`, and retains in-memory structure/location metadata sufficient to map translated results to their source positions. The chunks and IDs are **temporary correlation/reassembly data**; they are not localization dictionary keys and do not need to be stored permanently. The caller/host decides how to reconstruct/render/save any full translated document or chat message.

## Proposed splitting rules

| Format | Default meaningful unit | Configurable behavior / caveat |
| --- | --- | --- |
| Short plain text | One chunk | Avoid splitting when unnecessary |
| Longer plain text with recognizable paragraphs | Paragraph | Prefer paragraph boundaries, typically blank lines or explicit configured line/paragraph delimiters |
| Unstructured/plain continuous text | Sentence | Split using sentence boundary detection (period, question/exclamation marks and language-aware rules), not each literal `.` character |
| Markdown | Semantic content within Markdown syntax nodes | Use parsed structure (headings, paragraphs, list items, blockquotes, table cells and other text-bearing nodes); preserve surrounding syntax, links, code fences, placeholders and inline formatting |
| HTML | Translatable textual content of appropriate HTML elements | Parse the DOM/element structure and keep node/element mapping; translate text content without translating tags, attributes, scripts, styles or executable markup |
| Other types | Registered format-specific splitter | Define rules only when the format is implemented, with an extensible adapter interface |

**Prefer paragraphs** for readable prose when the input actually preserves paragraphs. Otherwise use sentence units for long, poorly formatted continuous text. The user may override `Auto` with `Paragraph` or `Sentence` on plain text, and choose the delimiter/line-break interpretation appropriate to the source. A single line ending can be a paragraph boundary in chat or a soft-wrap inside a document; do not assume those are identical across inputs.

For Markdown, “contents of each tag” means **text-bearing Markdown syntax/AST nodes or coherent semantic units**, not taking raw fragments between punctuation marks. A heading, list item, paragraph or table cell is often a natural chunk. Markdown **inline tags/formatting** may be preserved inside one unit when splitting on every emphasis/link span would destroy sentence context.

For HTML, a tag's translatable text is a useful unit, but nesting and inline spans are not reasons to break an otherwise coherent sentence into one-word chunks. Use an HTML parser, select safe translatable text content, retain source element/text-node references, and ignore excluded elements such as `script`, `style`, and code/non-translatable regions according to adapter policy. The renderer reconstructs valid markup and may update individual blocks through Blazor, JS or another client UI. No translation provider may be given unrestricted executable HTML to publish as trusted output.

## Structured template + source-first ResultList (proposal, not frozen)

A promising design for HTML/Markdown is to **preserve the parsed document structure**, replace each translatable semantic region with an **internal reference**, and maintain a request-local `ResultList` keyed by temporary integer `SegmentId=0..N`. Every slot **initially contains its original source text**, so the host can render the complete source document immediately and replace slots as translated results arrive. Each slot should distinguish original/current text, pending/success/failure and revision to avoid treating empty output or stale asynchronous results incorrectly.

The illustrative rewriting `<tag placeholder="0">@ResultList[0]</tag>` is only a useful mental model, not a mandate to mutate or serialize source HTML with custom attributes. Use **HTML parser/DOM or Markdown AST mapping** to bind result slots to correct text-bearing nodes while retaining nesting and other markup. Simple sibling tags map easily; a mixed-content sentence like `<p>Hello <strong>beautiful</strong> world!</p>` needs **one coherent translation context** with protected inline formatting rather than three unrelated words. HTML void elements such as `br` and `img` need not have closing tags; use a parser instead of regex-based tag pairing. The renderer/host projects current result slots into safe final output via its preferred UI or format adapter.

See the exploratory contract in [Structural Template and ResultList](../Structural-Template-and-ResultList/en.md). This representation is **separate from QueueMaster work queues, permanent dictionary keys and any chat host storage**.

## Reattaching inline markup after contextual translation (proposed strategy)

A semantic text chunk may contain nested inline formatting. Instead of sending each text node as an independent context-free translation, keep the **entire containing phrase/sentence** as one translatable unit. One possible strategy translates the **inner tagged text** to obtain candidate forms, separately translates the **full untagged phrase**, locates candidate(s) in the target translation and wraps the recognized target span with the preserved inline tag. For example `Hello <strong>beautiful</strong> world!` can translate as `Ahoj nádherný světe!` and return `Ahoj <strong>nádherný</strong> světe!` when `nádherný` is an unambiguous match.

Exact matching alone can fail because of inflection, paraphrase, multiple occurrences and altered word order. Never bind a tag to a guessed span. When supported, a provider's protected inline markup/HTML mode can be a more economical one-request strategy; the **inner variants + full-context alignment** method is an optional alternative. The Splitter owns **source-side semantic units and markup mapping**, while ResultProcessor/format adapter validates alignment and safe output. [Inline Markup Restoration](../Inline-Markup-Restoration/en.md).

## Sentence boundaries and context

A period (`.`) or end of line can be a useful starting separator, but an implementation must account for common counterexamples: decimals (`1.96`), abbreviations (`Dr.`), dates, URLs/domains, ellipses and language-specific punctuation. Prefer a configurable sentence boundary detector over a naïve `Split('.')` so chunks remain meaningful. Preservation of punctuation, spaces and newlines matters for exact reassembly.

Chunk splitting should avoid separating placeholders (e.g. `{name}`), markup tokens, HTML entities, links, protected text, quoted code, or other format-defined invariants. Retain enough adjacent textual context (where allowed) for a good translation without injecting neighboring private content into an unintended provider request. Keep original source order even when individual translations finish out of order.

## Size limits and fallback

The chosen semantic unit remains intact **where feasible**. If it exceeds a configured provider/request size limit, subdivide recursively at safer natural boundaries (paragraph -> sentences -> smaller safe spans only when necessary), or select another capable endpoint; report a defined failure if no compliant split is possible. Do not silently truncate, omit or corrupt content merely to stay inside an API size limit.

Request `Settings` can express `SplitMode = Auto | Paragraph | Sentence | FormatAware` (illustrative), preferred line-break conventions, maximum chunk length/cost, and input format. Defaults may vary by format and Translation Target. Treat a size ceiling as a safety/compatibility constraint, not as the primary segmentation strategy.

## Boundaries with other components

- **Splitter** identifies meaningful units, preserves protection markers, tracks ephemeral IDs and format mapping.
- **Translation Pipe/Chain** chooses compatible engines and processes segments; it should not reinterpret Markdown syntax as a flat text blob.
- **ResultProcessor** restores protected placeholders, validates structure, and returns correct translated segments/results.
- **Caller/host** assembles the resulting full content and decides how to display, edit or store it. Chat message history and user-edited overrides belong to the host, not to the Splitter or UI `KEY=VALUE` dictionary subsystem.

`P0 RealTime` remains automatic-only, with no Human fallback. Marking chat text Private still restricts content visibility; choosing a different segmentation method never relaxes privacy or authorizes global storage.

## Questions and test cases before implementation

Define precise AST/DOM node grouping, paragraph recognition for single versus blank newlines, language-aware sentence boundary strategy, context retention, splitting oversized single sentences, leading/trailing whitespace handling, provider size/cost constraints, cancellation and streaming, and format-specific options as adapters are built.

Tests should cover: short message remains one chunk; formatted paragraphs split by paragraph; no-paragraph prose splits into sentences; decimals, abbreviations, URLs and ellipses remain intact; Markdown headings/lists/links/code preserve structure; HTML nested inline elements stay coherent and scripts/styles are not translated; placeholder preservation; oversized chunks safely fall back; out-of-order translated chunks map back correctly; all IDs are temporary and never written as dictionaries or message IDs.

Related: [Chat Translation Flow](../Chat-Translation-Flow/en.md), [Request Settings](../Request-Settings-and-Live-Translation/en.md), [Translation Chains](../Translation-Chains/en.md), [Architecture](../Architecture/en.md), [Implementation Status](../../IMPLEMENTATION_STATUS.md).
