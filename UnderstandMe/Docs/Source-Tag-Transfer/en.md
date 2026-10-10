# Source Tag Transfer: Click Original, Select Translation, Reapply Element

Status: agreed Afutrans WYSIWYG interaction direction, 2026-10-10. Design only; no editor implementation or frozen public API.

## One-step formatting recovery

A document editor with an original preview on the left and editable translation on the right should support a direct **pick source element -> wrap selected target text** workflow:

1. The authorized editor **clicks text in the rendered original**. Using the parsed DOM/AST, Afutrans determines the element wrapping that source text, defaulting to the nearest meaningful inline formatting element. For nested markup, the user may inspect/choose an ancestor instead of silently choosing an arbitrary level.
2. The editor stores an **in-memory source element snapshot**, including **element/tag name, namespace, complete original attributes with values, original identity and corresponding closing-element semantics**, source node identity and structural role. E.g. `<strong class="hero" id="important">...</strong>`; do not store merely `strong` and drop `class`/`id`.
3. The editor visibly indicates the **selected source style/element** (e.g. a small status chip or outline), and the user **drags across the corresponding translated word/phrase** on the right.
4. The formatting command inserts a structurally valid target element around **the selected target-language span**, preserving the selected text unchanged and transferring the original element's **full attribute set** (subject to the validation/permissions below), with the correct closing tag. The editor should construct an element through its HTML/AST adapter, **not string-concatenate untrusted opening/closing tags**.
5. The right preview updates immediately; the action is a **single undoable manual edit**, restarts the already agreed **1,000 ms after-last-manual-edit autosave**, and enters normal revision history. Source remains immutable. The user may pick another element and repeat.

Example:

```html
Original:
<p>Hello <strong id="headline-emphasis" class="highlight" data-role="accent">beautiful</strong> world!</p>

Machine translated:
<p>Ahoj super světe!</p>

Click "beautiful" inside original <strong>, then drag-select "super" in translated view:

<p>Ahoj <strong id="headline-emphasis" class="highlight" data-role="accent">super</strong> světe!</p>
```

**The word is not retransmitted or substituted**. This is an explicit source-to-target *format/attribute transfer*, so the user can handle cases where the machine could not align translated forms.

## Preserve source attributes without enforcing ID uniqueness

The **user's intent is to transfer the entire element**, not only bold/italic semantics: preserve `class`, `id`, `title`, `data-*`, ARIA, relevant namespace, custom approved attributes and closing semantics. The safe way to honor that intent is to **clone a parsed element shell with its original attributes** onto the selected target range, then ensure the copied element is inserted in the intended target range without unintended attribute changes. A copied `id` remains **exactly as in the source**; do not silently rename/drop it. **Do not validate uniqueness of IDs in either source or target as a translation admission, save or publication gate.**

Required checks:

- **ID duplicates are outside translation scope:** source HTML can already contain repeated `id` values. Afutrans **must not reject a translation or manual tag transfer because an ID appears more than once**, in the source **or** the translated document. Preserve/copy each `id` verbatim. Do not repair, uniquify, rename, delete or lint duplicate IDs as a translation requirement. Any future HTML quality/lint tool can diagnose duplicates separately and optionally; it is not part of the translator's success criteria. Original/translated previews should use isolated safe rendering contexts for independent interaction, not for comparing or enforcing ID uniqueness.
- **Significant behavior:** copying `href`, `for`, `aria-labelledby`, `aria-describedby`, `data-*`, anchor targets, namespace-specific bindings or custom attributes must preserve necessary relationships and satisfy host security policy. Attributes and URL destinations can be content-sensitive. A source element may carry security-sensitive or disallowed event handlers, script URLs or active behavior; the editor must **not execute or blindly trust** them in its preview. Preserve their source metadata for review where allowed; reject/flag unsupported or unsafe transfer rather than pretending the full element was faithfully applied. For a faithful translated export, preserve original identity/reference attributes; **do not infer that their values were valid or unique in the original**.
- **HTML content model:** inline `strong`, `em`, `span` commonly wrap inline selections; block/void elements such as `p`, `div`, `br`, `img`, `input` are **not arbitrarily wrapped around a selected word**. For nonwrappable tags, offer selecting the appropriate whole structural block, another valid insertion/replacement operation or a typed editing failure, without corrupting HTML.
- **Existing nested tags and spans:** validate nesting and overlapping selections. Preserve all selected words and original target formatting, and do not create crossing HTML tags. An explicit tag-transfer operation should not silently replace unrelated target structure.
- **Attribute translation:** preserve exact source attribute values for structural fidelity by default. If an attribute is itself translatable (e.g. `title`, `aria-label`, `alt` in appropriate contexts), that requires a separate, explicit localized-attribute workflow rather than accidental translation or loss during cloning. Original attribute values must remain available for comparison.

If full faithful transfer cannot be completed, **keep the current target content unchanged and show what blocked the operation**, allowing the user to resolve the conflict. Do not discard an `id` and declare success. The default cosmetic-tag-omission rule for **automated** restoration does not override a user's **explicit request to copy a particular original element with its attributes**.

## Interaction and responsibilities

This is an **Afutrans assisted editing command**, not translation engine or QueueMaster logic. The source/target UI identifies parsed source element and selected target-language range; the format adapter performs the structural transfer; ResultList/draft mapping updates the target view and validation; the host saves and records revisions. The engine itself does not need to know whether the original clicked `strong` was restored manually.

The editor may expose a compact **Pick source tag** visual mode, while ordinary text selection on the original still remains available. After successful application, the selected source tag may stay armed for repeated application if configured; offer a clear cancel/escape indication. Use keyboard/accessibility alternatives for users who cannot drag-select.

Works without AI, external translation providers or any heuristic matching of word forms. In assisted mode, this is a simple manual escape hatch when `VariantAlignment` or `OuterAnchorAlignment` could not safely identify the target phrase. It is **not** an automatic Human fallback from private P0 RealTime.

## Open decisions and tests

Decide how nearest enclosing element/ancestor picker handles nested DOM and text nodes, what visual indicator shows the armed source tag, multi-node range wrapping behavior, source/target region identity, exact DOM patch schema, allowed attribute cloning and supported nonwrappable elements, and whether user-controlled copy includes optional localized-attribute editing.

Test:
- original `<strong class="x" id="y" data-role="z">beautiful</strong>`, target selected text gets the same tag **and identical approved attributes** with proper closing;
- original source remains unchanged, unselected translated text remains unchanged, user-assigned target word is not replaced by machine alternatives;
- nested source allows deliberate selection of correct ancestor tag;
- source HTML containing two elements with the same `id` is accepted and translated without diagnosing/blocking the duplicate; original IDs are copied unchanged; repeated IDs in the target do not prevent manual tag wrapping, saving or export;
- source and target preview IDs do not collide across isolated documents;
- disallowed `onclick`/`javascript:` cannot execute in preview, with explicit failure or safe handling;
- void/block tags are not used to wrap illegal inline spans;
- existing links/nested markup stay valid;
- manual tag transfer triggers the 1-second idle autosave, history and one-step undo/revert;
- mobile selection and keyboard accessible alternative remain usable.

Related: [Direct Editing](../Direct-Editing-Autosave-and-Revisions/en.md), [Assisted Document Translation](../Assisted-Document-Translation/en.md), [Structural Template and ResultList](../Structural-Template-and-ResultList/en.md), [Inline Markup Restoration](../Inline-Markup-Restoration/en.md), [Implementation Status](../../IMPLEMENTATION_STATUS.md).
