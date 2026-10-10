# Editor, Manual Translation, History and AI Audit

Status: preliminary design.

## Editor is more than a human worker

The Editor can directly request particular source and target dictionaries via DataAccess; show phrases side-by-side; inspect missing, stale or existing translations; select a proposed result from multiple backends; edit, validate and save accepted translations. It may generate its own logical improvement batch through QueueMaster without waiting for an automatic source watcher.

Optional editor improvement requests default to low/background priority. Their failure is **nonfatal** and must not modify the existing dictionary. Manual saving is a distinct write operation, not an unnecessary trip through an async queue.

A human can separately register as a normal translation worker, declare known languages and use a small long-running buffer, accepting work as availability allows. This is not mandatory for an Editor UI.

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

