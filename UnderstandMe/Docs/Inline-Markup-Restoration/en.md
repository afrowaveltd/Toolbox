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
6. If the mapping is missing, ambiguous, overlaps incompatibly or would create invalid nesting, **do not wrap an arbitrary word**. Try another **permitted and budget-bounded** markup-preserving strategy; otherwise emit verified plain translated text with a typed format-restoration warning for optional cosmetic tags. For semantically functional tags (e.g. links), omission can change document behavior: use a structure-preserving fallback or return a clearly marked unavailable/source-preserved section according to policy, rather than silently removing a link.

All resulting per-segment status/revisions remain correlated through ephemeral `RequestId + SegmentId`; **the consuming host** assembles, renders and optionally stores final text. No permanent dictionary keys/chunk IDs are generated.

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
- `Fallback`: bounded retry with another eligible strategy/provider; then explicit style-loss or source-preserving result where appropriate.

Provider calls for inner spans incur **additional costs, quota reservations, latency and processing overhead**. The orchestrator still applies ClientPolicy, account hard caps, privacy and requested deadline to **every** such call; `Mandatory` cannot force paid overage. P0 must remain automated and volatile; no Human/manual-audit fallback to recover tags.

## Result shape and safe projection

Conceptual rendering state may separately hold:

```text
Segment: OriginalText | CurrentTranslatedText | Status | Revision
InlineTagMap: SourceTagId -> TargetSpan? | AlignmentStatus | Reason
```

Do not make `CurrentTranslatedText` contain untrusted executable HTML. The format adapter projects translated text plus validated annotations into rendered HTML/Markdown for the consuming UI. The original structure is retained for source fallback when no safe result exists; status flags distinguish `TranslationCompleted` from `FormattingRestored`.

## Test matrix before coding

Test exact and alternative match from the example, grammatical inflection, repeated candidate ambiguity, missing synonym due to paraphrase, nested and adjacent inline markup, crossed/overlapping spans, mixed RTL/LTR language scripts, multiword tags, empty tagged text, links and attributes, placeholders, incomplete HTML, duplicate/out-of-order segment updates, extra-call quota exhaustion, privacy settings forbidding an external provider, and graceful no-style/source-preserved fallback. Verify original message content and markdown/HTML structure are not silently corrupted.

Related: [Structural Template and ResultList](../Structural-Template-and-ResultList/en.md), [Semantic Splitter](../Semantic-Splitter/en.md), [Translation Chains](../Translation-Chains/en.md), [Provider Billing](../Provider-Billing-and-Quotas/en.md), [Implementation Status](../../IMPLEMENTATION_STATUS.md).
