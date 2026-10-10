# Structural Templates, Segment References and ResultList

Status: design proposal discussed 2026-10-10, **not a frozen contract and not implemented**. The user's initial HTML attribute/template idea is a useful model to refine before coding.

## Working idea

For structured documents, separate **the original format/structure** from **replaceable translatable text**:

```html
<p>First paragraph</p>
<p>Second paragraph</p>
```

Conceptually:

```text
Template:
  <p>{Result[0]}</p>
  <p>{Result[1]}</p>

Source segments:
  0 = "First paragraph"
  1 = "Second paragraph"

ResultList (initial state):
  0 = "First paragraph"
  1 = "Second paragraph"

After first completion:
  0 = "První odstavec"
  1 = "Second paragraph"
```

The `{Result[n]}` expressions above are **illustrative internal references**, not executable template syntax or literal HTML to send to a browser. A possible visual representation `<p placeholder="0">@ResultList[0]</p>` expresses the same idea but should **not be required in the source document**. `placeholder` may collide with real application attributes and DOM mutation introduces unnecessary coupling. Keep mapping separate, e.g. a request-scoped node/AST reference or synthetic `SegmentRef` known only to the format adapter.

The source/original text and current translated value should remain independently recoverable. The simplest conceptual per-segment state is:

```text
Id | OriginalText | CurrentText | State (Pending/Translated/Failed) | Revision
```

A `ResultList` **starts with the source text** for every slot, so the document is immediately reconstructable even when some translations are unfinished, rejected or unavailable. A completed translation updates only the applicable slot. An empty successful translation must not be mistaken for “not yet translated.” Results may finish out of order and must not overwrite newer revisions.

## HTML parsing and meaningful grouping

First **parse** HTML into a structured representation using a proper HTML-aware parser, not an ad hoc pattern matching opening and closing tags. Recognize void elements (e.g. `br`, `img`), comments, nested nodes, attributes, entities, and recoverable malformed HTML according to adapter policy. The parser's recovery should not silently destroy significant source meaning; expose errors when faithful processing is impossible.

For simple non-overlapping text-bearing elements, each element's coherent text may map to one reference. However, nested or mixed-content elements make literal `<tag>text</tag>` replacement ambiguous:

```html
<p>Hello <strong>beautiful</strong> world!</p>
```

Three text nodes do **not necessarily mean three good translation units**. Preserve sentence context and relative markup, for example using a **single parent semantic chunk with protected inline markup markers** while retaining a structural map to restore formatting safely. Details of how to represent protected formatting markers are intentionally open until the HTML format adapter is designed/tested.

Avoid automatic translation of `script`, `style`, code, hidden/private nontranslatable regions, or markup syntax. Select translatable attributes (`alt`, `title`, `aria-label`, input `placeholder` when appropriate) only by an explicit capability/configuration rule; translating an attribute is different from using a synthetic mapping attribute.

The format-aware processor reconstructs valid, appropriately escaped/sanitized output. Raw translated text is not directly assigned to HTML `innerHTML` as trusted markup.

## Variant alignment to reconstruct nested inline tags (proposal)

For `<p>Hello <strong>beautiful</strong> world!</p>`, a useful strategy is:

1. Extract the **inner highlighted phrase** `beautiful` from the parsed source structure; ask a compatible translation provider for a primary translation and **supported** alternatives (for example `nádherný`, `krásný`, `fascinující`).
2. Translate the **entire untagged contextual sentence** `Hello beautiful world!` to Czech, for example `Ahoj nádherný světe!`. The full sentence, not an assembly of independently translated English fragments, determines grammar and word order.
3. Locate one sufficiently reliable candidate in the complete target sentence as a **whole target word/phrase**. Here `nádherný` matches, so the format adapter annotates that target-language span and reconstructs `Ahoj <strong>nádherný</strong> světe!`.
4. If no safe, unique match can be established, **never wrap an arbitrary occurrence**. Try a permissible format-preserving strategy or retain verified plain target text with a formatting warning for cosmetic markup; semantically functional markup such as links needs a safer explicit fallback/source-preserved result.

This **inner-candidate + full-context alignment** approach is a proposed strategy, not a frozen requirement or a promise that every engine supplies synonyms. Word form changes (`krásný` versus `krásného`), repeated expressions, paraphrases, nested/crossing spans and different provider outputs can defeat literal matching. Matchers must respect language-specific morphology, word boundaries, source position/context, valid tag nesting, and the final reconstruction's safety.

Where a provider can preserve protected inline markers or validated HTML **in one full-context translation**, that may be preferable to additional inner phrase requests. Every extra translation call is subject to current eligibility, quota, cost, deadline and privacy rules; never force paid calls solely to restore bold/italic. The source-initialized `ResultList` and separate structural mapping remain unchanged. [Inline Markup Restoration](../Inline-Markup-Restoration/en.md).

## Fallback by outer anchors and significance of inline identifiers

If the tagged inner expression cannot be aligned directly to the authoritative whole translation, the user proposes translating the **outer pieces** before and after it as a second alignment attempt. For `Hello <strong>beautiful</strong> world!`, translate `Hello` and `world` separately as clues (e.g. `Ahoj` and `svět`), locate corresponding spans in `Ahoj super světe!`, and consider whether the remaining text `super` can be unambiguously tied to the original inner tag. Handle morphology such as `svět` vs `světe`, and decline to infer a span on ambiguity, repetitions or word-order changes. Never overwrite the whole-sentence translation with a composite made from fragment translations.

When no reliable matching strategy succeeds, **the default for safely cosmetic inline tags** (e.g. `<strong>`, `<i>` **without important attributes**) is to **omit those tags** from the reconstructed translation, preserving the translated words and the surrounding structural tags. When the unmatched nested tag has **`id`** or other semantically functional attribute (link, reference, required application/accessibility binding), default to **typed structural failure** instead: never remove a required identifier silently. The host can render the original segment safely or handle the flagged partial candidate according to effective Settings. A tag's name alone does not establish that it is harmless; inspect its attributes and structural role.

The policy is configurable, with a default `OmitUnmatchedCosmeticFormatting`, optional strict formatting enforcement and bounded retries/extra translation calls subject to provider quotas, client entitlement, privacy and deadline. See [Inline Markup Restoration](../Inline-Markup-Restoration/en.md).

## ResultList as the assisted editor's source of display truth

The source-filled `ResultList` and structural template allow a **left original / right current translation** editor without requiring all chunks to finish. A region left untranslated remains source text with an explicit status; an automatic HTML-capable engine may update whole structural regions while an editor repairs selected problem spans and markup annotations. The right panel can choose **safe rendered preview**, **validated source HTML**, or **both**. Keep `OriginalText`, provider output and manual `CurrentText` revisions separable so late asynchronous results cannot overwrite user corrections.

A matching user can recognize `svět`/`světe` or a paraphrase that automated variant/outer-anchor alignment could not confirm, explicitly select the desired target text span and reapply a cosmetic inline tag. ID/link-bearing markup still requires a successful structural validation rather than silent cosmetic omission. The host manages drafts, undo, acceptance and export; ResultList mapping stays an application-facing data contract rather than a fixed browser implementation. [Assisted Document Translation](../Assisted-Document-Translation/en.md).

## Lifecycle and presentation neutrality

1. Format-specific adapter parses and validates the source structure.
2. Semantic Splitter selects coherent translatable regions; assigns **temporary per-request IDs `0..N`** and builds structural references/mapping.
3. Store **original values**, initialize `ResultList` with those originals, and dispatch translation units via permitted workers.
4. On each validated completion, update the matching result slot, preserving its ID and revision; failures leave the original intact (plus a status/reason).
5. An application can project the current `ResultList` onto a structured preview, UI component state, Markdown renderer, HTML DOM or reconstructed document. This does **not** mandate JS, Blazor or a specific transport.
6. On completion/cancel, the ephemeral mapping and chunk IDs can expire; the **host** owns any finalized content/history persistence and personal translation corrections.

The representation need not literally be `List<int,string>`: use `IReadOnlyList<Segment>` or an ID-indexed mapping depending on the final type design, but maintain stable `0..N` IDs and explicit original/current/status data. Do not conflate this list with QueueMaster's durable job queues or an UI localization dictionary. An HTML node's mapping ID and provider work item ID are distinct concepts.

## Adjacent formats

- **Markdown:** parsed AST nodes and text ranges form the structural template. Preserve whitespace, links, emphasis, code fences and nested markup; emit a Markdown document, not HTML-only assumptions.
- **Plain text:** paragraph or sentence boundaries use ranges/offsets/whitespace metadata instead of a DOM.
- **Chat:** a host may use the same transient `ResultList` and segment updates, but **owns final whole-message assembly, presentation and storage**; chunk references do not become permanent message IDs or dictionary `KEY=VALUE` keys.

## Open engineering questions

Decide immutable source vs mutable current value representation; exact `SegmentRef` API and serialization, HTML AST/DOM parser library, handling mixed-content inline markers and translated word order, text inside HTML attributes, whitespace/HTML entities, how to handle broken HTML, protection against stale/out-of-order partial updates, and limits on very large `ResultList` memory usage.

Tests before implementation: simple sibling elements, empty text, nested inline tags, `<br>`/`<img>`, attributes versus synthetic references, user-supplied `placeholder` attribute collision, malformed input, HTML escaping, protected markup, asynchronous partial replacements, cancellation retaining source fallback, duplicate/out-of-order revisions, Markdown structure preservation and non-persistence of temporary chunk IDs.

Related: [Semantic Splitter](../Semantic-Splitter/en.md), [Request Settings and Live Translation](../Request-Settings-and-Live-Translation/en.md), [Translation Chains](../Translation-Chains/en.md), [Chat Translation Flow](../Chat-Translation-Flow/en.md), [Implementation Status](../../IMPLEMENTATION_STATUS.md).
