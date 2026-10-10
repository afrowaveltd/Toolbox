# Editor, Manual Translation, History and AI Audit

Status: preliminary design.

## Editor is more than a human worker

The Editor can directly request particular source and target dictionaries via DataAccess; show phrases side-by-side; inspect missing, stale or existing translations; select a proposed result from multiple backends; edit, validate and save accepted translations. It may generate its own logical improvement batch through QueueMaster without waiting for an automatic source watcher.

Optional editor improvement requests default to low/background priority. Their failure is **nonfatal** and must not modify the existing dictionary. Manual saving is a distinct write operation, not an unnecessary trip through an async queue.

A human can separately register as a normal translation worker, declare known languages and use a small long-running buffer, accepting **explicitly submitted persistent P1-P4 work** as availability allows. Human never processes P0 RealTime requests; RealTime is reserved for automatic providers, and if only Human exists P0 remains inactive and returns original content. This is not mandatory for an Editor UI. [Human translation workflow](../Human-Translation-Workflow/en.md).

## Translator interface over the durable backlog

As a **Human worker**, the operator can connect at any later time to inspect durable pending work in the existing QueueMaster queues: per-priority counts, compatible language directions, status (pending/claimed/in progress) and items available to that person. Nothing requires the interface or its operator to be online when requests arrive. A human translation item is claimed **exclusively through the QueueMaster worker contract**, not directly dequeued from UI code; interruption or disconnect needs safe release/recovery without losing the original item.

The Editor remains a distinct dictionary management and audited revision tool; an optional host UI may combine human-work inbox and editor screens, but their responsibilities and work lifecycles remain separate. A human worker's final output follows the same typed result validation and source protection as other translation providers. For delayed work, persistent acceptance is required even if the originating request was realtime and returned the source text. See [Human Translation Workflow](../Human-Translation-Workflow/en.md).

## Private translated content is not manual-audit material

A request with effective `Privacy = Private` is excluded from all Human translation, manual quality audit, general dictionary browsing, and human-visible prompt/source/result history. This is true **even when its successful translation is stored durably** for authorized chat/forum history lookup. Private result persistence is not permission for other users, translators or admins to inspect the content.

The mandatory **manual dictionary revision audit** below remains intact for real **manual edits of authoritative dictionaries**; it does not mean that automatically generated private chat translations must be exposed to a human auditor or registered as public dictionary keys. Operational usage/status may be audited with **no raw private content**. Automated checks may operate within the effective privacy and provider-disclosure policy. See [Privacy and Scoped Storage](../Translation-Privacy-and-Scoped-Storage/en.md).

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

