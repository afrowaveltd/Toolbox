# Assisted Editor: Direct Editing, Autosave and Reversible History

Status: agreed UX/architecture principles from 2026-10-10; design only, no implementation or finalized public APIs.

## Fundamental rule: every authorized task has a manual completion path

**"This machine engine cannot do it" must never be the end of the user's ability to obtain or enter a translation.** An authorized user of Afutrans can always type, replace, or correct the translated content through a reachable **manual authoring path**, even if no automatic translator or AI is installed, a paid quota is exhausted, a provider does not understand the source format, or structural restoration failed.

The exact UI does not need to expose manual editing within every request mode; there must be an **explicit system-level way to enter, edit and save translations** for authorized projects/content. The human-only installation remains valid. This principle does **not** mean a forbidden provider must be invoked, an unauthorized viewer is allowed to edit private content, a structurally invalid document may be published as validated, or `P0 RealTime` may automatically enqueue a private chat message for a Human worker.

If a particular format lacks a specialized parser/editor, retain a **basic source/plain-text manual entry or safe import/export** route instead of claiming no translation is possible. Unsupported structured formats need not promise validated reconstruction; the user must be told what structure validation remains unavailable. Provider task failure and **availability of a manual authoring path** are separate outcomes.

## Direct edits in rendered and source views

The assisted document editor defaults to **low-friction direct editing**:

- In **rendered translation preview**, an authorized user can click/tap a translated paragraph, heading, list item or selected coherent segment and **type over it in place**, without navigating through a mandatory dialog, special mode or separate Save button.
- In **HTML/Markdown source mode**, the same user can click a corresponding line/region and **edit the actual translated source** directly. Source and rendered views are two representations of **one draft document**, not disconnected versions; changing either keeps the shared model and the other view synchronized whenever the current input parses safely.
- The **source/original panel** is read-only by default; the translated document remains editable. This preserves the immutable original against accidental modifications.
- Translation/markup restoration errors can be resolved by selecting/editing the affected region. Users can change text, fix inflection, remove cosmetic markup or manually reattach a known `strong`/`i` span. The system must not demand successful automatic span alignment before permitting authorized manual correction.
- If the user selects a region not supplied by an automated translator, initialize its editable target with the **original source text** and let the user replace it from scratch.

Treat "line" as **editor affordance**, not necessarily the exact semantic unit: visual line wrapping is different from a Markdown/HTML AST node. Preserve caret/selection, source position and linked segment identity across source/preview toggles and background translated updates. The editor should remain usable with keyboard, touch and accessible controls.

## Word-like WYSIWYG and code-editor keyboard behavior (agreed)

**WYSIWYG is a real directly editable formatted document surface, not a read-only preview with an edit modal.** A user may drag-select translated text, press `Ctrl+B` to toggle bold (`<strong>`) or `Ctrl+I` to toggle italic (`<em>`), or use a small contextual formatting toolbar. Provide platform-equivalent `Cmd+B`/`Cmd+I` on macOS, clear focus/selection behavior, and simple undo/redo shortcuts. The editor creates/removes **validated semantic markup** around the selected translated range; users do not need to type tags to restore missed formatting. Selection may span mixed inline nodes: keep text, safe nesting, existing `id`, links, attributes and source/target segment mapping intact instead of string-replacing HTML around arbitrary offsets.

**Source mode behaves like a practical code editor**: direct HTML/Markdown typing, syntax coloring, indentation, matching tag pairs/brackets, familiar editing shortcuts, location/line selection, and inline parse/validation diagnostics. These are editor UI features, not new translation-engine dependencies. An editing line may contain only part of one semantic chunk; keep the structural map and caret stable when switching modes. Shortcuts depend on current focus, source/preview mode and platform; do not steal browser or accessibility shortcuts indiscriminately.

Both editing modes change **one shared translated draft**. A WYSIWYG formatting command is a normal manual edit for autosave, history and undo. Rebuild the safe visual projection after a valid source edit; if the source is temporarily malformed, preserve what was typed and keep the last valid preview until syntax is repaired.

## Pick complete source tag, then wrap a target-language selection (agreed)

An even simpler manual repair path than typing source tags: the user **clicks the formatted text in the original panel**; Afutrans resolves its enclosing parsed HTML element and remembers the **entire opening element** (tag name **and all original attributes/values, especially `class`, `id`, `data-*`**), plus matching closing-element and source identity. The UI indicates which element is selected. The user then **drag-selects the intended translated phrase** on the right; the WYSIWYG editor wraps only this target-language selection in a structurally equivalent element, retaining every **allowed and safely preserved attribute** and the original closing semantics.

Example: `<strong id="emphasis" class="marked">beautiful</strong>` in source + selecting `super` in target inserts `<strong id="emphasis" class="marked">super</strong>` without changing the translated word. The transfer is not restricted to `Ctrl+B`/`Ctrl+I`: arbitrary supported original inline tags and their full structural identity matter. For nested markup, a visible nearest-tag/parent selection permits choosing **which** enclosing source element is copied.

This operation needs HTML/AST-safe element-shell cloning and validation rather than string concatenation. **Never silently rename/drop `id` or other meaningful attributes.** **A repeated `id` is not a transfer error**, even if repeated in the original or target: copy its value unchanged and do not enforce uniqueness. If the element cannot structurally wrap the chosen inline selection or an unsafe executable attribute cannot be rendered safely, show an actionable conflict or alternate operation. Both document previews must be isolated so duplicate IDs across **source versus target** do not count as a single-document collision. Unsafe event handlers, active URLs and untrusted attributes are not executed in preview, nor are they silently published; unsupported transfer is flagged. This is a **single undoable edit**, resets the **1-second manual autosave idle timer**, and enters draft revision history. [Source Tag Transfer](../Source-Tag-Transfer/en.md).

## AI agent that visibly edits the document (agreed direction)

Afutrans may offer an **explicitly started AI editing agent**, inspired by a developer's visible code-editing assistant: the authorized user instructs it in ordinary language (e.g. "fix HTML formatting in this section", "translate the untranslated paragraphs", "repair Czech declension"), then sees **the selected translated document change progressively** instead of only receiving a detached block of suggested text.

The user **retains control**: they can pause/stop an active edit, inspect which spans the agent intends to change, accept/keep the finished changes, reject/revert its edits (preferably as **one undoable agent operation**), or manually take over any region. The AI may suggest or write into a **revision-tracked working draft**, but those changes are clearly identified as `AgentEditing`/`AgentProposed` until accepted under the configured editing mode; visible interim edits do **not** mean irreversible publication or privileged write access.

Use **structured patch/segment operations** against the current draft with expected base revision, not untrusted model-produced JavaScript or uncontrolled raw keystroke injection. Render safe incremental previews/changes as patches become valid; never treat a partially streamed HTML tag as valid final markup. Protect immutable source, required `id` and links, placeholders, document permissions, provider privacy restrictions and spending budgets. Revalidate every patch or batch, and prevent an AI patch/late reply from overwriting newer human edits; pause and surface a conflict when necessary.

Agent changes can be checkpointed and background-saved as **attributed agent draft revisions** using the same durable storage contract. The explicit **1-second idle timer below governs manual edits**, not a requirement to generate a server revision for every streamed AI token. Group an agent run into one logical reversible operation with internal safe checkpoints; publication/export remains a separate validated action. Distinguish agent-accepted revisions from user-authored revisions in history. This does not enable AI unless explicitly activated and authorized.

## Responsive, simple layout

- **Wide screens:** original rendered document left, translated document right; on right offer Preview, Source and (if useful) Split mode.
- **Narrow/mobile screens:** stack views or use a simple switch `Original | Translation | Source`; retain edit access, navigation and unsaved indicators without requiring horizontal scrolling through two tiny panels.
- Keep primary actions discoverable but minimal: click/tap to edit, undo, revision history, optional ask-AI/retranslate this part, validated export. Avoid mandatory confirmation dialogs for each ordinary text edit.
- Highlight not-yet-translated, machine-uncertain and structurally invalid regions without preventing manual work on them.

These are UX contracts for Afutrans presentation; other hosts may use WebUI, Avalonia or TUI while sharing the same draft/revision APIs.

## Autosave contract: immediate interaction, durable acknowledgement

**Every accepted manual edit starts/restarts an exact 1,000 ms inactivity debounce.** If no new manual change arrives for **1 second**, automatically dispatch the current draft change to background durable persistence, **without requiring a Save button**. The view updates optimistically on each keystroke/formatting command. Further manual input resets the timer; a continuous typing session therefore batches naturally. Flush pending changes earlier on blur, view switch, navigation, explicit session end or other safe lifecycle boundaries when possible. The 1-second timer controls **when to start a save**, not a guarantee that storage completes in 1 second. An implementation must also account for IME/composition events rather than committing half-composed text.

Distinguish `Editing`, `Saving`, `Saved`, `Offline/Retrying`, `Conflict` and `SaveFailed` states. **Never display "Saved" until the durable store acknowledges the revision.** When connectivity/persistence fails, keep the pending edit available locally/in memory until safely retried or give an explicit recoverable export/copy path; do not silently discard an unsaved translation. No arbitrary promise of durability through browser crashes is made unless durable offline storage is actually implemented and tested.

Use per-document/segment **optimistic concurrency** (e.g. expected revision, request/client edit ID) so a background provider completion, another editor or a retry cannot overwrite a newer manual change. Duplicate delivery is idempotent; conflicts must be surfaced and resolved without silently dropping either version.

Autosaving a work-in-progress HTML **source draft** is different from validating/publishing it. Mid-typing syntax may be temporarily broken; save that draft as a draft, keep the last structurally valid preview where necessary, and present parsing errors inline. The rendered preview must be isolated/sanitized and must not execute imported scripts, untrusted event handlers or arbitrary remote resources.

## Durable, inspectable edit history and revert

Each **committed change** becomes a versioned, attributable draft edit; retain timestamp, actor/authorized editor, document/source version, affected segment/region, before/after contents or equivalent recoverable diff, reason/type (manual, provider suggestion accepted, restore, etc.), and resulting revision ID under the host's audit/retention policy.

High-frequency keystrokes and formatting shortcuts are coalesced into a **logical user edit** for understandable revision history: the one-second idle autosave may commit one revision for a burst of manual input. **Autosave reliability cannot be weakened** by history compaction. An AI agent run is a distinct attributed, reversible logical edit group with checkpoints rather than an unreadable revision per streamed token. Draft history and checkpoint retention must be explicit and usable across sessions; the history interface shows saved versions and allows previewing, comparing and reverting an individual region or whole document where authorized.

**Revert does not erase history.** Restoring version N creates a **new** version N+K whose content matches the chosen earlier state, preserving both intervening revisions and the revert event itself. Revisions are immutable once committed and concurrent restore commands require a version check. The mandatory revision-audit rule for authoritative **UI localization dictionary edits** is unchanged; document-edit history is a separate host-owned versioned workflow.

Provider/AI outputs are non-destructive proposals unless explicitly accepted. A stale completion arriving after a user's direct edit must not overwrite the edited region. The user can manually override automation indefinitely where authorized.

## Validation and boundaries

Translation success and final document structural validity are separate. Users may save an incomplete or invalid **draft** and keep editing until it is valid. Export/publish under strict structured-output contracts must still check required `id`, links, accessibility/functional attributes and placeholders. Failure should identify the problematic region and offer direct manual correction, rather than permanently blocking access to the editor or stripping significant markup.

Human manual authoring is **never an automatic P0 fallback**. In a Human-only deployment P0 remains inactive and returns source immediately. The authorized user can independently open a non-P0 assisted document workspace to translate by hand. Ordinary Private chat content is not routed automatically to unrelated editors or auditors; private-chat recipient self-correction has its own per-user, optional cache/no-cache rules and is not overridden by general Afutrans draft autosave.

Background saving of a document the editor is authorized to work on does not imply publishing it or making it globally available. Document permissions and data retention constrain autosave and edit history.

## Implementation checkpoints and tests

Define a small, versioned host-facing `EditDraft` / `SaveDraftRevision` / `ListRevisions` / `RevertRevision` contract (illustrative names), change-batching/acknowledgement semantics, conflict behavior, HTML source validation and safe preview mapping.

Test: selecting text in WYSIWYG and pressing Ctrl+B/Ctrl+I (or platform equivalent) produces safe semantic markup with intact text and existing IDs/links; code-mode syntax highlighting/tag pairing and view synchronization; a formatting shortcut counts as an edit; a 1000 ms idle timer is reset on each manual change and dispatches a save **only after the last change**, while blur/navigation flushes early; typing an incomplete tag and waiting one second saves an invalid-but-recoverable draft without publishing invalid HTML; AI run visibly patches a selected region, can pause/stop/undo as one attributed operation, checkpoints without token-per-revision noise and never overwrites intervening manual edits; no AI/no translation backend permits manual authoring; unsupported format can be translated manually as text without fabricated structural assurances; inline render edits and source edits synchronize; mobile stacked mode remains fully editable; autosave commits without Save button; pending saves survive transient transport loss to the extent implemented and never falsely show Saved; duplicate retries are idempotent; edits after provider suggestions take precedence; history lists sequential revisions; reverting creates a new revision; temporarily invalid HTML draft is retained but cannot publish as validated; mandatory dictionary revision audit still applies when editing UI catalogues; Private chat content cannot appear as a generic assisted document automatically.

Related: [Assisted Document Translation](../Assisted-Document-Translation/en.md), [Editor and Audit](../Editor-and-Audit/en.md), [Structural Template and ResultList](../Structural-Template-and-ResultList/en.md), [Human Translation Workflow](../Human-Translation-Workflow/en.md), [Implementation Status](../../IMPLEMENTATION_STATUS.md).
