# Human Translation: Durable Backlog and On-Demand Worker Interface

Status: agreed functional direction (2026-10-10); no implementation or frozen public API.

## Principle

**Human translation is the baseline provider; the human translator is an ordinary worker that processes queued requests when available.** The host accepts work even if every translator is offline. A translator can open their chosen interface minutes, days or weeks later, inspect the waiting work by priority and compatible languages, and process it. No LibreTranslate, cloud account, AI, running browser, or permanently connected human session is required for this workflow.

The interface is a **view and worker client** over QueueMaster/application contracts; it does not own or replace the five global priority queues. Humans do not dequeue the central queue directly or bypass orchestrator admission, quotas or user permissions.

## Durable lifecycle

1. A client creates a translation request; the orchestrator normalizes Settings, ClientPolicy, provider eligibility and persistence policy. If the work is to be handled by a human **later**, the accepted human-work item is durably saved before the system promises it has been accepted for later execution.
2. QueueMaster owns the item in an existing **non-RealTime P1-P4 priority category**, with durable state such as `Pending` while no compatible human worker is connected. **P0 is reserved for automated real-time translation and must never be human-assigned**. Long absence of workers **does not delete or lose** accepted human requests.
3. When an authorized human opens a translation UI (WebUI, Blazor, desktop, TUI, etc.), the interface queries a **state snapshot**: waiting count by priority, languages/direction, age, applicable targets and optional filters. It then may subscribe to live changes. **A SystemMonitor event is not the source of truth** for these counts; connection to monitoring is optional and loss of events must not lose requests.
4. A worker can select/request a compatible work item through QueueMaster's **claim/reservation API** (or request the next eligible item). The orchestrator/QueueMaster atomically grants exclusive ownership/lease; a second human worker cannot take the same active item.
5. Human translator edits and submits the result. The shared ResultProcessor validates language/format/placeholder invariants and writes a durable completion/checkpoint under the job policy; any required editor/audit trail is preserved. Complete/fail/return results flow through the same typed provider-independent contracts as automatic engines.
6. The request owner can obtain completion later; application-facing notifications may assist but are not the durable result store. A translator logout, browser tab close or server restart must not silently delete pending work or uncommitted results.

## Human-specific readiness, ownership and failure

An offline human worker does **not** mean the provider contract is unavailable system-wide. It means no human processing capacity is presently online. Compatible pending human work remains visible on the next UI connection.

Human sessions may be long-lived and can be interrupted unexpectedly. The **ownership lease and recovery policy** must ensure at most one valid holder, allow voluntary release, safe expiry/reclaim after disconnect or inactivity, and avoid losing drafts. If a translator's lease expires and another takes over, only the current valid claim may publish final completion; later stale submissions require conflict detection rather than overwriting another worker's result. A translator may optionally save a private/recoverable draft under the configured persistence/privacy policy; drafts do not equal final publication.

Human worker capability includes authorized access to appropriate source/target languages and any specific work/project permissions. The UI should show **waiting/claimed/in progress** separately, plus per-priority totals and "mine" filters; counts can vary with active claims and permissions. The number and placement of translator UI views are host choices, not required UI dependencies of UnderstandMe.

## Hard separation: RealTime is automatic only

**P0 `RealTime` is exclusively for immediate automatic translation. A Human worker is never an eligible P0 endpoint, regardless of `Mandatory`, provider fallback order, failure of a machine engine or a client's attempts to request Human directly.** If the host has only Human configured, **P0 is inactive**: a real-time translation request returns the **original untranslated content almost immediately**. It must not be queued to Human, retained as pending manual work or shown in a translator's inbox.

In a mixed deployment, P0 selects only enabled, suitable automatic engines. If none can return a translation under the applicable real-time policy, return original content or an explicit reason as specified by the public response contract; **no human fallback**. Automatic *live segment updates* remain permitted only for eligible automatic providers within their short-lived P0 request lifetime.

**Privacy boundary:** a RealTime request may carry private chats or other user-sensitive text. It must **never be exposed to a human translator, Human task inbox, manual editor or durable manual work store solely because real-time translation was requested**. No default manual-copy, manual audit of the text or indefinite persistence. Distinct text logging/caching policies must also follow explicit privacy configuration. `RealTime | Mandatory` is urgency, **not consent to human review**.

## Private requests never enter Human queues

Effective `Privacy = Private` disallows both automatic routing **to a human translator** and **manual content audit**, whether the source is a RealTime chat, a forum conversation or another confidential item. The translation may nonetheless be stored durably in a **tenant/conversation/message-scoped result cache**, readable only by current authorized participants. **Persistent private translation results are not persistent human jobs** and are never part of the visible Human backlog.

Human work still requires a separate authorized non-P0 manual request whose effective privacy/visibility policy explicitly permits Human reading. Exposing content through a generic administrator UI is not a substitute for that authorization. [Privacy and Scoped Storage](../Translation-Privacy-and-Scoped-Storage/en.md).

## Human persistence applies only to explicitly submitted manual work

Human translation tasks are **separately and deliberately submitted** to a non-P0 persistent workflow; they must be authorized for human viewing and storage under the relevant client/target privacy policy. An operator/user may independently submit a new non-real-time manual request for content they are authorized to disclose, but **the engine must not derive or create that request automatically from P0 or use a hidden "human continuation" option**.

Such accepted P1-P4 manual tasks are durably stored before acceptance is acknowledged and can wait for a translator to open their UI. Their `Persist` property is a correctness requirement for delayed work, separate from optional step `Preserve` or an unlimited audit archive. After restart, rebuild the applicable P1-P4 human backlog from durable state without adding a sixth physical queue; P0 remains inactive in Human-only deployments.

## Scope and further choices

This agrees on durable human queueing, on-demand interface, trustworthy per-priority counts and exclusive claim/release. Still to specify: claim timeouts and heartbeat rules, draft recovery, UI choice of next versus manual selection, human capacity/skill matching, cancellation/deadline handling, translator audit permissions, accessible privacy-safe preview, manual payment/time accounting if ever needed, and how long to preserve completed human results.

Test empty/no-human sessions, persisted P1-P4 pending work over restart and long offline intervals, inactive P0 with Human-only provider and immediate untranslated return, forbidden human fallback for RealTime and private chats, counts with zero/inactive P0, per-language eligibility, concurrent claim attempts, stale claims/completion, draft recovery, cancellation, and loss of UI/SignalR connection without loss of work.

See [QueueMaster](../QueueMaster-and-Reliability/en.md), [Translation Chains](../Translation-Chains/en.md), [Request Settings](../Request-Settings-and-Live-Translation/en.md), [Architecture](../Architecture/en.md), [Implementation Status](../../IMPLEMENTATION_STATUS.md).
