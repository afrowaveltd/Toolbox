# Editor, Manual Translation, History and AI Audit

Status: preliminary design.

## Editor is more than a human worker

The Editor can directly request particular source and target dictionaries via DataAccess; show phrases side-by-side; inspect missing, stale or existing translations; select a proposed result from multiple backends; edit, validate and save accepted translations. It may generate its own logical improvement batch through QueueMaster without waiting for an automatic source watcher.

Optional editor improvement requests default to low/background priority. Their failure is **nonfatal** and must not modify the existing dictionary. Manual saving is a distinct write operation, not an unnecessary trip through an async queue.

A human can separately register as a normal translation worker, declare known languages and use a small long-running buffer, accepting **explicitly submitted persistent P1-P4 work** as availability allows. Human never processes P0 RealTime requests; RealTime is reserved for automatic providers, and if only Human exists P0 remains inactive and returns original content. This is not mandatory for an Editor UI. [Human translation workflow](../Human-Translation-Workflow/en.md).

## Assisted HTML/document translation editor in Afutrans

Separate three human activities: **explicit Human-provider backlog**, **manual authoritative UI dictionary edits** (with mandatory dictionary revision audit) and **assisted correction of a translated document**. The third is an optional, deliberately opened Afutrans document-review session, not a central auditor that automatically receives chat/private RealTime content. A document owner/editor with appropriate access can inspect and change their own document; no unrelated human access is implied.

The assisted interface places **rendered original on the left** and **translated version on the right**, with right-side **Preview / HTML Source / Split** modes. Clicking either side maps to the corresponding original/translated segment and its source-first ResultList. Show translated, source-preserved/untranslated, formatting-omitted, invalid-structure and manually edited states; never pretend a region is translated just because HTML rendering succeeded.

The editor can **directly correct translated text or safely edit its HTML**, select formatting/word spans that automated alignment could not find, request alternative candidates, try provider translation for a selected region or **explicitly ask a permitted AI endpoint** to suggest a revision. AI suggestions are separate non-destructive candidates: the user chooses whether to accept them, and late async suggestions cannot overwrite newer manual edits (revision/conflict checks). Undo, restore source, validate, save draft and export/publish are desired capabilities. An authorized user can solve inflection or ambiguity manually rather than require another expensive machine pass.

HTML preview needs safe isolated rendering of untrusted documents/scripts/resources. Validate ID/links/functional attributes, placeholders and full-document structure before publishing, and keep unresolved source regions visible. **Manual document review does not change the mandatory dictionary-edit audit rule**, and private chat recipient-only corrections retain their separate scope. See [Assisted Document Translation](../Assisted-Document-Translation/en.md).

## Direct editing, autosave and revision history (Afutrans)

**No successful machine translation is required for an authorized user to finish a document manually.** The user may write the target text from scratch or repair an existing result. In assisted mode the right-hand **rendered preview is directly editable**, as is the corresponding **HTML/Markdown source line/region**. A simple click/tap starts editing: the user should not need a separate modal or mandatory Save button. Both representations share one draft and a safe source/render mapping. Responsive layouts retain full editing on narrow/mobile screens.

**Persist each accepted change automatically in the background**, with immediate local display and explicit durable acknowledgement. Short debounce/batching is permitted, but unrecoverable lost changes or falsely reporting `Saved` are not. Preserve a visible state for network/offline/retry/conflict/save error. Source drafts with mid-typing incomplete HTML remain editable and saved as drafts, while previews retain a known valid representation if required.

Every committed logical edit creates a **versioned history item** (who, when, what changed); restoring an older version must be implemented as a **new revision**, so history stays intact. Provide simple undo, history browsing and revert. Late provider/AI completions must not overwrite newer user edits. Document revision storage is host-owned; **manual UI-dictionary edits** still get the separately mandatory revision audit below, while **private chat recipient corrections** remain subject to their own explicit cache/no-cache choice. [Direct Editing, Autosave and Revisions](../Direct-Editing-Autosave-and-Revisions/en.md).

## Translator interface over the durable backlog

As a **Human worker**, the operator can connect at any later time to inspect durable pending work in the existing QueueMaster queues: per-priority counts, compatible language directions, status (pending/claimed/in progress) and items available to that person. Nothing requires the interface or its operator to be online when requests arrive. A human translation item is claimed **exclusively through the QueueMaster worker contract**, not directly dequeued from UI code; interruption or disconnect needs safe release/recovery without losing the original item.

The Editor remains a distinct dictionary management and audited revision tool; an optional host UI may combine human-work inbox and editor screens, but their responsibilities and work lifecycles remain separate. A human worker's final output follows the same typed result validation and source protection as other translation providers. For delayed Human work, persistent acceptance is required **only for a separately authorized non-P0 manual request**; RealTime never creates such work, even if its translation result is saved privately for history reuse. See [Human Translation Workflow](../Human-Translation-Workflow/en.md).

## Private translated content is not manual-audit material

A request with effective `Privacy = Private` is excluded from automatic Human routing, manual quality audit, general dictionary browsing, and human-visible prompt/source/result history. An independently requested non-P0 Human translation would require explicit owner authorization and narrowly scoped translator access; it is not a manual audit or a fallback. This is true **even when its successful translation is stored durably** for authorized chat/forum history lookup. Private result persistence is not permission for other users, translators or admins to inspect the content.

The mandatory **manual dictionary revision audit** below remains intact for real **manual edits of authoritative dictionaries**; it does not mean that automatically generated private chat translations must be exposed to a human auditor or registered as public dictionary keys. Operational usage/status may be audited with **no raw private content**. Automated checks may operate within the effective privacy and provider-disclosure policy. See [Privacy and Scoped Storage](../Translation-Privacy-and-Scoped-Storage/en.md).

## Recipient self-correction is a separate optional feature

**A private chat recipient is allowed to correct the translated message they personally received.** This is performed through the chat application's own UI or a specifically integrated translation-correction tool, **not** the central Human translation backlog, unrelated auditors or a generic dictionary Editor listing everyone's messages.

The correction affects only that recipient's visible result. They can choose **no cache** (only the current view changes), or **opt in to a recipient-scoped saved correction** for later authorized views. Source-message ownership and privacy are unchanged; choosing cache does not give permission to make the original or edited private message globally available. Broader sharing, if provided by a host, needs a separate explicit and authorized operation.

The following **mandatory dictionary revision audit** still applies to manual edits of authoritative localization dictionaries. It does **not** mandate creating a global/manual audit record of private chat translation corrections. A host may maintain scoped metadata/revision controls to manage its own recipient overrides without publishing their contents. [Chat Translation Flow](../Chat-Translation-Flow/en.md).

## Mandatory revision audit

Every **committed manual dictionary change**, whether originating from WebUI, GUI, TUI or a plugin/API, must create an audit record. Store at least actor, timestamp, target, language, key, old and new value, revision lineage and the nature of edit/restore. Ensure change and audit record are committed together or neither is accepted. Editors must not erase their own audit trails.

Manual-edit audit is independent of optional `StepHistory`, `Preserve` and `FinalizeAndFlush` job cleanup. An undo/restore becomes a NEW audited revision; it does not rewrite history. Retention of audit data must be governed separately by an explicit audit policy.

Saving through stale editor state must detect conflicts: new edits cannot silently overwrite a newer authoritative revision.

## Optional AI Audit module

An independently registered AI Audit worker can scan committed manual revisions and compare the edited translation against the ORIGINAL source rather than the previous translation. Based on selectable policies, it can detect semantic reversals, material omissions or additions, disallowed tone changes, and broken protected content.

Actions may be: report through Messenger, create an incident, request human intervention, suggest a correction, or perform a separately authorized auditable correction. Model findings are probabilistic, not proof of deliberate manipulation. Check current revision before auto-correcting; prevent review/correction loops.

## Optional translation history and rollback

`Preserve` stores outputs per processing step (before split, chunks, alternatives, reassembly, AI revisions and edits) in `StepHistory`, subject to a separate `LifeTime` policy. `Persist` remains solely about crash recovery. Results can form branches, with a selected final variant.

Rollback is an **independent durable workflow** which may pause at a retained checkpoint, restore the earlier result as a new version, and continue through different prompts/workers without destroying original history. It cannot restore data already purged.

See [Translation Chains](../Translation-Chains/en.md), [Notes](../../Notes.md).

