# Inline Markup Restoration by Contextual Translation and Variant Matching

Status: promising proposal discussed 2026-10-10; **not a finalized algorithm or implemented public API**. Complements Semantic Splitter, Structural Template and ResultList.

## User-proposed approach

For mixed-content markup, do not force every inline HTML node to become a separate, context-free chunk. Translate the **inner tagged phrase** to identify plausible target-language wording, then translate the **whole surrounding sentence/semantic chunk without its literal formatting tags**. Locate the translated inner phrase (or acceptable alternatives) inside the contextual full translation, then reconstruct the **original inline tag around that target span**.

Example:

```html
<p>Hello <strong>beautiful</strong> world!</p>
```

```text
Inner source:  beautiful
Inner variants: nádherný | krásný | fascinující

Whole source:  Hello beautiful world!
Whole target:  Ahoj nádherný světe!

Match:         nádherný
Reconstructed: <p>Ahoj <strong>nádherný</strong> světe!</p>
```

This differs from returning three isolated translations of `Hello`, `beautiful`, `world!`: the whole-sentence translation supplies grammar and word order, while a second translation supplies **alignment hints** for restoring inline formatting. The initial source-filled `ResultList` remains valid until the whole translated chunk and its markup projection pass validation.

## Proposed format-adapter flow

1. Parse HTML/Markdown with a proper structure-aware parser. Identify a **semantic outer chunk** and its ordered, potentially nested **inline markup spans**. Keep original source text and the exact tag identity/allowed attributes in structural mapping, rather than exposing raw HTML to a plain-text provider.
2. For each eligible marked inner text span, request candidate translations (one or more alternatives **only if the configured provider actually supports them**, or when further permitted calls are worthwhile). Keep text, associated source span identity and provider provenance. Purely identical repeated phrases can potentially share candidate work within the same authorized request scope.
3. Obtain a contextual translation for the whole semantic chunk after extracting its source text and protecting/removing formatting markup appropriately. Keep the whole translated text as the **authoritative semantic output**.
4. Match inner candidate(s) against the whole translation as **complete target-language token/phrase spans**, not arbitrary substrings. Consider normalized Unicode/case/whitespace, safe punctuation boundaries and morphology where a suitable language-aware matcher is available. Record `SpanStart/Length`, candidate identity and confidence/ambiguity.
5. If exactly one sufficiently reliable span exists, add a structural **format annotation** referencing that target-text range. Reconstruct the original allowed inline element around the matched target span via the format adapter; preserve meaningful nesting/order and produce valid, properly escaped markup. No literal provider output becomes trusted executable HTML.
6. If the mapping is missing, ambiguous, overlaps incompatibly or would create invalid nesting, **do not wrap an arbitrary word**. Before falling back, optionally try **prefix/suffix source-fragment translations** to find target-side boundary anchors; only use a unique and reliable aligned span. Then follow the configured **default cosmetic-omit versus strict-identifier failure** policy below. Semantically functional tags (links/identity/anchors) must not be silently removed.

All resulting per-segment status/revisions remain correlated through ephemeral `RequestId + SegmentId`; **the consuming host** assembles, renders and optionally stores final text. No permanent dictionary keys/chunk IDs are generated.

## Second attempt: align the surrounding fragments (proposed)

When no inner-word candidate can be found in the authoritative whole-sentence translation, **do not immediately drop the tag**. The user's refinement is to also translate the text **before** and **after** the marked span, e.g.:

```html
<p>Hello <strong>beautiful</strong> world!</p>
```

```text
Inner source/variants:    beautiful -> nádherný | krásný | dokonalý
Whole source:             Hello beautiful world!
Whole translated result:  Ahoj super světe!
Prefix source/translation: Hello -> Ahoj
Suffix source/translation: world -> svět
Context match:            Ahoj ... světe! (svět/světe morphological relation)
Candidate middle span:    super
```

A sufficiently reliable **unique** pair of outer anchors may identify the gap corresponding to the original tagged text, even if a direct inner-word translation did not appear inside the full translation. This is **contextual alignment evidence**, not permission to replace the full translation with words from the separate calls. For the example, if the prefix/suffix positions and target word-order correspondence are demonstrably unambiguous, restoring `<strong>super</strong>` is plausible. The algorithm should require exact word boundaries and normalized/linguistic matching for inflection (e.g. `svět` vs `světe`) and should check that no competing or reordered matches undermine the boundary inference.

**Do not assume every translated inner phrase lies literally between translated prefix and suffix.** Languages may reorder constituents, collapse a phrase, or rewrite the sentence idiomatically. If anchors are missing, duplicated, contradictory, cross over, or leave an unreasonable span, mark alignment uncertain and use the configured fallback. Translation requests for both surrounding fragments are **optional and bounded** by endpoint capacity, cost, privacy, time and request policy.

## Default and configurable failure policy

Classify each **original parsed inline element** by whether losing it would affect only presentation or break required structure/identity:

| Inline markup kind | Default if no reliable target span |
| --- | --- |
| Pure cosmetic formatting from a safe allowlist (e.g. plain `i`, `em`, `strong` with no significant attributes) | **Drop the unplaceable inner formatting tag**, preserve the complete validated whole-sentence translation and its enclosing structural tags. Report `FormattingOmitted` / warning, not a translation failure |
| Any tag containing an `id` attribute or another required identifier/reference | **Strict failure** by default. Do not silently delete, reassign, duplicate or attach its identity to an arbitrary translated word. Return a typed `MarkupRestorationFailed` failure/partial-result status; the host chooses a safe source-preserving display |
| Semantically functional inline elements such as links, anchors, variables, input associations, interactive or accessibility-relevant attributes | Treat as structurally significant; attempt another authorized structure-preserving method or return strict failure/source-preserving fallback, not invisible data loss |
| Cosmetic formatting with host-declared custom rules | Follow effective `InlineMarkupFailurePolicy`, provided higher-priority structural/privacy requirements remain enforced |

**A tag is not purely cosmetic just because its tag name is `span`, `i` or `strong`**: `id`, event-independent application bindings, `data-*` dependencies, accessibility attributes and CSS classes with significant behavior can change the result. Allow cosmetic removal only when the **element AND its attributes** are explicitly safe to omit. This must not delete the text **inside** the tag, remove enclosing block elements (`p`, list structure etc.) or alter the authoritative translated wording.

A conceptual configurable policy:

- `OmitUnmatchedCosmeticFormatting` (**default**): keep the full translation with only safely optional tags omitted and provide a nonfatal warning.
- `StrictStructure`: fail/return a source-preserving result when any required formatting is lost, including cosmetic tags if the client opts in.
- `AllowBoundedAlignmentRetries`: try inner variants, then **outer-prefix/suffix anchors**, and optionally protected-markup or another permitted provider strategy within a configured attempt/cost/deadline budget.
- Per-tag/attribute allowlist and identifier rules remain under administrator/target Settings. **Request overrides cannot relax stronger structural guarantees**.

An identity-bearing nested element for which formatting cannot safely be reconstructed is a **typed failure, not a completed translation with secretly removed ID**. Integrate error reporting with existing Essentials/WhenItFails Result conventions rather than inventing a second error subsystem. The translated plain text may be retained as an internal candidate or exposed as an explicitly marked partial preview when policy allows, never misrepresented as fully valid HTML.

## Why exact string matching is insufficient

- **Inflection:** `beautiful` may yield `krásný` in isolation but appear as `krásného`, `krásná` or `krásným` in its full sentence. Exact variant matching can miss correct spans. Morphological matching must still guard against false positives.
- **Repeated words:** `beautiful beautiful world` could match two identical candidates; select a target span only with adequate context/relative alignment. Never arbitrarily pick the first occurrence.
- **Paraphrase/idiom:** a whole translation might express the meaning without any literal synonym occurring. A missing match is possible and not necessarily a translation error.
- **Multiple/nested tags:** `<em><strong>...</strong></em>` may cover the same span, while adjacent inline tags can map to overlapping/crossing spans after target-language reordering. Reconstruction must produce a valid hierarchy and preserve identity; overlapping target spans are not blindly nested.
- **Functional inline content:** `<a href=...>text</a>`, variables, code and protected placeholders have additional invariants. Tags like links require more conservative behavior than cosmetic `strong` or `em`.
- **Provider differences:** not all APIs return alternatives or consistent wording between inner and whole translation, even with the same model. Never invent synonyms or assume output alternatives are always available.

## Multiple configurable strategies, not a forced double translation

The user proposal is **one useful matching strategy**, especially with plain-text providers. A provider might support safe HTML translation or protected inline markers directly; in that case a **single full-context call with protected formatting** can be preferable. Model these as interchangeable `InlineMarkupRestorationStrategy` capabilities/policy, without fixing the exact interface now:

- `ProtectedMarkup`: translation preserving validated inline tags/markers in a context-rich request, if supported and verified;
- `VariantAlignment`: user-proposed inner-phrase candidate translation + whole-context translation + target-span matching and safe tag restoration;
- `OuterAnchorAlignment`: if matching the inner variants fails, translate prefix/suffix fragments around the marked content, match target-side outer anchors (including safe inflection handling), and infer the highlighted span only on unambiguous evidence;
- `Fallback`: bounded retry with another eligible strategy/provider; then **omit unmatched cosmetic formatting by default** or **fail strictly** for identity-bearing/functional elements.

Provider calls for inner spans incur **additional costs, quota reservations, latency and processing overhead**. The orchestrator still applies ClientPolicy, account hard caps, privacy and requested deadline to **every** such call; `Mandatory` cannot force paid overage. P0 must remain automated and volatile; no Human/manual-audit fallback to recover tags.

## Provider-native HTML, AI and assisted manual alignment

Not every provider requires reconstructing inline tags from separate phrase translations: **some adapters can request structured HTML translation natively**, while some models accept context-rich prompts with protected markup. Detect the specific endpoint capability and then **validate** returned tags, `id`, links, required attributes, protected placeholders and structural nesting. A valid-looking AI-generated page is not automatically structurally or semantically correct.

For **complex HTML**, an **optional format-specific AssistedHTML profile** may prefer an eligible AI endpoint, even when ordinary text translation still defaults to local/free -> paid -> AI. Capability-based routing and admin/client/privacy/budget caps control eligibility. If an attempt cannot preserve formatting/structure, retain original result slots, mark translation and formatting states independently, and give an **authorized user** a chance to repair it in a side-by-side Afutrans editor (rendered source left; render/source/split translation right). The user can explicitly select the correctly inflected target word and reattach an `i`/`strong` annotation when automatic alignment was inconclusive.

The user can also **manually edit the draft** or request a **targeted AI proposal for a selected region**, subject to explicit authorization and provider costs; AI output does not override a later human draft revision automatically. For significant tag identity/links, maintain a structural failure until resolved; for unmatched cosmetic formatting, the default omission policy still applies. No part of this assisted workflow is automatic Human auditing of P0/private chat content. [Assisted Document Translation](../Assisted-Document-Translation/en.md).

## Manual alignment by transferring an entire original tag

Automated `ProtectedMarkup`/`VariantAlignment`/`OuterAnchorAlignment` need not be the last chance to restore formatting. An authorized Afutrans editor can **click on source text to choose its containing parsed tag**, storing the **original opening element with all attributes and appropriate closing semantics** in an in-memory source-tag selection, then **drag-select the correctly corresponding translated phrase**. The format adapter applies that full tag as a safe, validated wrapper without replacing the translated words. Thus the human, not a probabilistic algorithm, decides that `super` in `Ahoj super světe!` corresponds to source `<strong class="x" id="y">beautiful</strong>`.

This supports preserving **`class`, `id`, `data-*`, ARIA and other meaningful attributes** even when machine translation cannot align the text. The transfer must preserve original ID values **without checking uniqueness**, while separately handling invalid nesting and unsafe active markup; duplicated IDs in the original or target are not a translation failure. Semantically significant links/attribute dependencies are copied faithfully, not silently rewritten. **Never silently drop or invent a different `id`**; if exact safe insertion is impossible, show the conflict and leave the target unchanged. The act is manual WYSIWYG editing (one undoable revision and 1-second autosave), not translation-engine code. [Source Tag Transfer](../Source-Tag-Transfer/en.md).

## Result shape and safe projection

Conceptual rendering state may separately hold:

```text
Segment: OriginalText | CurrentTranslatedText | Status | Revision
InlineTagMap: SourceTagId -> TargetSpan? | AlignmentStatus | Reason
```

Do not make `CurrentTranslatedText` contain untrusted executable HTML. The format adapter projects translated text plus validated annotations into rendered HTML/Markdown for the consuming UI. The original structure is retained for source fallback when no safe result exists; status flags distinguish `TranslationCompleted` from `FormattingRestored`.

## Test matrix before coding

Test exact/alternative match; inner synonyms absent but outer prefix/suffix translations locate a unique target span (including `svět` vs `světe`); anchor ambiguity/repetition and reordered sentence; grammatical inflection and paraphrase; tag omission for `<strong>`/`<i>` with no critical attributes while preserving child text and outer `<p>`; **strict failure when `id` or semantically functional attributes are present**; `<a href>` link preservation; custom strict cosmetic setting; nested/adjacent tags, crossing spans, RTL/LTR scripts, placeholders, incomplete HTML, duplicate/out-of-order updates, exhaustion of extra-call budget and privacy limits. Verify no target translation is fabricated from separately translated fragments and no significant markup identity is lost. Verify original message content and markdown/HTML structure are not silently corrupted.

Related: [Structural Template and ResultList](../Structural-Template-and-ResultList/en.md), [Semantic Splitter](../Semantic-Splitter/en.md), [Translation Chains](../Translation-Chains/en.md), [Provider Billing](../Provider-Billing-and-Quotas/en.md), [Implementation Status](../../IMPLEMENTATION_STATUS.md).
