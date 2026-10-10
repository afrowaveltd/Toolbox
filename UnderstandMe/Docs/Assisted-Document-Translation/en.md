# Assisted Document Translation: Provider-Native HTML, AI and Human Editing

Status: design direction discussed 2026-10-10; **not implemented; exact UI and public contracts remain open**.

## Purpose and boundary

**Afutrans assisted translation** is an explicitly opened **document editing/review workflow**, not a silently invoked Human translator, global manual audit, or RealTime chat fallback. A user loads an HTML page/document they have permission to view and edit, receives an automatic draft from permitted engines, compares original and translated outputs, manually fixes any part, optionally requests targeted AI assistance, previews the result and decides when to publish/export.

UnderstandMe supplies provider capabilities, format-aware parsing, segmentation, source-initialized ResultList, structured result/validation, draft/revision and optional review-operation contracts. The **Afutrans WebUI** (or future desktop/TUI host) owns presentation, editor lifecycle, project/document storage and export. Never require an interactive editor to translate ordinary requests.

This is distinct from (a) localization dictionary `KEY=VALUE` edits with mandatory authoritative-dictionary revision audit and (b) private chat recipients correcting only their own translated view. Document author/editor permissions and applicable `Privacy` govern who can see the source and translated document.

## Format-aware provider selection, including native HTML

Translation providers differ in how they handle structure. A provider endpoint's probed/configured capabilities can include:

- `PlainText`, `HtmlTranslation`/structured input and preservation of source element identity, `ProtectedInlineMarkup`, `Alternatives`, supported languages/formats, maximum input and output sizes;
- cost, availability, quality/format validation status and privacy/disclosure constraints for an individual endpoint/model version.

**Native HTML mode is a preferred candidate where the provider actually supports it reliably:** submit approved translatable document content in its supported HTML mode, preserve untouched structural/functional elements, parse the response and validate identity-bearing tags, `id`, links, essential attributes, placeholders, and expected translatable regions. Native HTML support **reduces reconstruction work**, but does not imply that any output is automatically correct: a model/provider might omit nodes or alter attributes.

For **complex HTML** (deep nesting, mixed-content sentences, many inline tags/links, complex lists/tables or failed markup restoration), an **AI provider is a sensible preferred candidate in an opt-in format-specific assisted-HTML profile** if it is activated, allowed for this client/content and meets language/structured-output requirements. A capable AI model can use context to maintain markup and sentence meaning. **Do not promise perfect HTML or perfect semantics** without structural validation. A named AI vendor or even "AI" is not intrinsically mandatory: Human-only hosts remain valid; another HTML-capable machine API may be better/cheaper under real capabilities.

**Provider ranking remains configurable** by host, ClientPolicy, Translation Target, request and format complexity. This profile may favor capable AI for complex HTML **without changing the previously agreed global automatic routing preference** (free/local -> paid APIs -> AI) for ordinary text. Merely selecting this profile cannot activate/pay for AI, grant external text disclosure or override current client quota, provider-account spending cap, privacy policy or orchestrator queue pause.

A possible format strategy order under operator-approved policy:

1. For a supported HTML document, use reliable provider-native HTML translation or protected-markup translation, with full structural verification.
2. For complex HTML, prefer an enabled eligible AI model when the assisted-document profile is configured to favor AI; direct native HTML and AI may be overlapping provider capabilities.
3. For remaining untranslatable/uncertain regions, use semantic Splitter + ResultList plus protected markers, VariantAlignment, optional OuterAnchorAlignment and configured cosmetic-omission/strict-ID failure behavior.
4. **Always produce a truthful partial draft**: successfully translated regions become translated; unresolved regions retain their original source and are labelled. Do not silently treat untranslated English as Czech, fabricate missing nodes, remove essential `id`s or links, or declare a partially invalid page finished.
5. Leave any remaining corrections to the explicitly authorized human editor, or expose a **user-invoked** AI suggestion/retry for a selected region. The editor controls acceptance of suggestions.

The routing decision uses actual adapter capabilities and observed validation, not a hard-coded "all AI is perfect" assertion or unverifiable quality rankings.

## Proposed assisted editor in Afutrans

Primary layout:

- **Left: Original** — structurally rendered source HTML with document structure intact. Optional source inspection; read-only by default during translation review.
- **Right: Translation** — rendered translated document side-by-side with **Source / Preview / Split** views on the right. "Source" edits the translated HTML (with syntax validation); "Preview" displays safely rendered output; "Split" can display both without losing the original left pane.
- **Linked selection** — clicking a region on either side highlights its corresponding source/target `SegmentId` or structural node, shows original/current text, provider, status and formatting warnings. A source-first `ResultList` makes pending/failed parts visible unchanged.
- **Action controls** — `Edit manually`, `Translate this segment`, `Ask AI for suggestion`, `Use alternative`, `Retry formatting`, `Keep original`, `Accept this segment`, `Undo`, `Validate page`, `Save draft`, `Export/Publish`, subject to permission and configured capabilities.

Show visible per-region states rather than treating everything as translated: `Original/Pending`, `Translated`, `NeedsReview`, `FormattingOmitted`, `StructuralFailure`, `ManuallyEdited`, `Accepted`. The exact names are conceptual. A formatting warning does not necessarily indicate a semantic translation failure; a lost required `id`/link is a structural failure and prevents an unqualified publish/valid result.

The editor permits **human recognition of alignments that automatic matching missed** (such as Czech morphology and reworded equivalents). The editor can select the intended translated target span, attach the original safe inline tag or choose to omit a cosmetic tag, then revalidate. This gives an escape hatch for cases where even AI/variant matching cannot confidently place the correct `strong`/`i` tag.

## Mandatory usability baseline: no dead end when automatic translation fails

**System-wide manual authoring is always available to an authorized document translator.** No machine/AI backend is a prerequisite for creating a target-language document. If an automatic engine cannot translate a segment or fails to preserve required HTML structure, the user can **type a replacement directly**, even if this source format lacks a specialized automatic translator. Any missing structured parser/validator is reported truthfully and never treated as automatic validation success. The document remains workable as a draft.

**The simplest editor interaction is edit-in-place** in both right-hand views:

- In **rendered Preview**, click/tap a paragraph/line to directly edit the visible translated text; an explicit "Edit manually" modal is not mandatory.
- In **Source**, click a line/region of translated HTML and type directly. Both modes operate on a single shared draft; safely reparse and refresh preview as changes become valid. Source editing may temporarily produce incomplete tags; preserve the draft and show parsing diagnostics, with the **last valid render** displayed where necessary.
- On wide displays use original left/translated right; on small/mobile displays stack or switch `Original | Translation | Source` without losing edit controls. Optimize for responsive layout, keyboard/touch accessibility, caret preservation and simple use.

**All accepted edits autosave in the background**: immediate optimistic display, short bounded debounce/batching, durable save acknowledgement, visible `Saving/Saved/Retrying/Conflict/Failed` status. Never claim Saved before persistence acknowledgement; retry or offer a recoverable path when storage fails. Avoid a required manual Save button, but preserve deliberate publish/export approval and structural validation.

**Every committed logical change enters revision history** with author, time, region, before/after or recoverable diff and document/source revision. Undo/revert uses a **new revision**, never deletion of historical versions; keep selectable revision previews and restore both segment and document versions where allowed. Provider or AI results arriving after a manual edit cannot overwrite that newer draft. This is document-host revision history, distinct from optional translation-step `Preserve` and mandatory localization dictionary manual-edit audit. The per-recipient private chat correction workflow retains its separate optional no-cache policy; the assisted document editor's autosave does not override it.

See [Direct Editing, Autosave and Revisions](../Direct-Editing-Autosave-and-Revisions/en.md) for the detailed UX/state/recovery contract.

## Word-style formatting, VS Code-style source, live AI agent

Afutrans **rendered translation pane is WYSIWYG**: click/drag to select words or phrases, then toggle formatting with familiar shortcuts such as **`Ctrl+B`** (bold/`<strong>`) and **`Ctrl+I`** (italic/`<em>`), matching toolbar buttons or platform equivalents. The format adapter maps the selection to valid translated-document markup and preserves protected IDs, link structure and nesting. This explicitly lets users repair a bold/italic span that the automatic variant/outer-anchor matcher failed to position.

The translated **Source** pane acts like a straightforward **VS Code-style HTML/Markdown editor** with syntax highlighting, paired tags/brackets, indentation, inline diagnostics and direct code edits. Both WYSIWYG and Source edit the **same document draft**; source edits refresh the safe rendered view when valid, and temporary malformed HTML does not erase the in-progress draft. A mobile layout uses a simple responsive view switch; avoid excessive menus.

**Manual autosave uses a fixed 1-second idle debounce**: restart the timer on *every* manual content/format change and dispatch durable saving **1,000 ms after the most recent change**. Blur/navigation/view switching should flush outstanding edits as feasible; show storage acknowledgment/status and do not claim 'Saved' before confirmation. Save temporarily invalid source as a draft and retain the last valid safe preview. Logical grouped changes become reversible revision-history entries.

An optional **visible AI editing agent** (explicit invocation, inspired by the *workflow* of an IDE coding agent) can modify a selected paragraph, region or document progressively **before the user's eyes**, not just offer disconnected chat advice. Show editable candidate patches/diffs or clearly marked in-draft agent changes, with **Pause / Stop / Accept / Revert** and user override at any point. User retains final decision; agent edits are non-destructive, revision-scoped, tracked in history and ideally revertible **as one operation**. An intervening manual edit must block stale AI writes from replacing it. Stream/animate applied *safe structured patches*, not uncontrolled HTML/scripts or one persisted revision per token; every patch and final publish must respect structure, document permission, provider privacy and budget.

This AI assisted editing is optional. A Human-only installation retains all ordinary WYSIWYG/Source/manual functionality. [Direct Editing, Autosave and Revisions](../Direct-Editing-Autosave-and-Revisions/en.md).

## Safe, reversible editing lifecycle

- Preserve the **immutable original**, provider output and current user draft separately; keep a record of source version, translated segment state and manual changes sufficient for undo/review under configured host retention. Applying a suggestion must not silently replace later manual edits or other accepted segments.
- AI suggestions are **non-destructive candidates**. Show the proposal (and ideally target-region comparison), validate protected placeholders/markup before accepting, and commit only on an explicit user action when interactive review is required.
- Handle **asynchronous stale results**: a provider result arriving after a user manually edited segment must not overwrite their change. Use optimistic revision/version checks and a conflict or ignored-stale result outcome.
- Parse source HTML and preview output with a **safe document rendering boundary**: never run imported scripts, untrusted event handlers or arbitrary remote resources inside the editor just to show a preview. Do not trust AI-created HTML or use unsanitized `innerHTML`; reconstruct/validate markup with configured safe policies while preserving needed document structure where possible.
- Save/export only what host permission and document revision policies allow. A fully faithful exported page requires identity-bearing `id`, valid links/functional attrs, protected placeholders and structure checks; unresolved critical errors must be reported, not hidden. Optional editor checkpoints/history can retain work so a session is resumable.
- Saving a **manual authoritative localization-dictionary** edit still requires its previously agreed mandatory revision audit. Document draft edits have their own governed version/history policy; do not confuse them with private chat recipient overrides or force global auditing of private content.

## Queues and AI actions

An editor opened by an authorized human is an **explicitly initiated assisted session**, not a Human worker fallback from `P0 RealTime`. The translation/AI suggestions can be ordinary admitted non-P0 work (for example `P1 Interactive`) through the existing QueueMaster and provider Pipe; no sixth queue is required. The human may edit directly without waiting for a special worker queue, while requested automatic translation jobs are owned and scheduled normally.

An optional AI button for a **selected paragraph/span** is a new authorized processing request. It must show/obey applicable client access, external-provider privacy, current quota/cost caps, and document content disclosure rules. It is never a blank check to send a whole private document to an external model because the user clicked on one word. Local AI versus external AI are distinguishable capability/permission choices.

## Worked example

```html
<p>Hello <strong>beautiful</strong> world!</p>
```

Suppose the first pass yields `Ahoj super světe!` but cannot confidently align the `strong` span. Under the cosmetic-omission default the draft is safely rendered as:

```html
<p>Ahoj super světe!</p>
```

The right-side inspector shows `FormattingOmitted: strong`, the original phrase `beautiful`, known translated alternatives and the source/target range. A human who understands Czech can select `super` (or edit the whole translated sentence), apply `strong` to that target range, validate and accept. An eligible AI provider may alternatively propose a better structure-preserving version, but the user keeps the final decision in assisted mode.

If the original nested tag carries required `id` or link semantics, retain a **structural error** until a valid restoration or explicit host-approved policy resolves it; don't claim the altered document is structurally correct.

## Open implementation decisions

Decide actual provider capability probe and HTML quality thresholds; complexity scoring for AI-first format profiles; how source/target linked selections handle reordered nodes; right-side source/preview/split representation; script/style/attribute whitelist and isolated rendering; segment revision/conflict behavior; draft/checkpoint storage; controlled AI-suggestion UI/cost disclosure; autosave and export acceptance criteria.

Tests: native HTML endpoint returning correct structure, native HTML endpoint dropping an `id`/anchor (rejected); complex HTML AI-first preference when enabled and allowed; disabled/forbidden AI fallback without automatic spending; partial output with visibly untranslated source, source/preview mapping, morphology/manual inline-tag restore, safe source editing, unsafe script preview isolation, AI suggestion opt-in only, stale AI reply never overwrites manual text, and failed structural validation preventing unqualified publish.

Related: [Inline Markup Restoration](../Inline-Markup-Restoration/en.md), [Structural Template and ResultList](../Structural-Template-and-ResultList/en.md), [Semantic Splitter](../Semantic-Splitter/en.md), [Provider Routing](../Provider-Routing/en.md), [Editor and Audit](../Editor-and-Audit/en.md), [Afutrans Integration](../Afutrans-Integration/en.md), [Implementation Status](../../IMPLEMENTATION_STATUS.md).
