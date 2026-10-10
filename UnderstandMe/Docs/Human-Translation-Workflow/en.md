# Human Translation: Durable Backlog and On-Demand Worker Interface

Status: agreed functional direction (2026-10-10); no implementation or frozen public API.

## Principle

**Human translation is the baseline provider; the human translator is an ordinary worker that processes queued requests when available.** The host accepts work even if every translator is offline. A translator can open their chosen interface minutes, days or weeks later, inspect the waiting work by priority and compatible languages, and process it. No LibreTranslate, cloud account, AI, running browser, or permanently connected human session is required for this workflow.

The interface is a **view and worker client** over QueueMaster/application contracts; it does not own or replace the five global priority queues. Humans do not dequeue the central queue directly or bypass orchestrator admission, quotas or user permissions.

## Durable lifecycle

1. A client creates a translation request; the orchestrator normalizes Settings, ClientPolicy, provider eligibility and persistence policy. If the work is to be handled by a human **later**, the accepted human-work item is durably saved before the system promises it has been accepted for later execution.
2. QueueMaster owns the item in one of the existing P0-P4 priority categories, with durable state such as `Pending` while no compatible human worker is connected. Long absence of workers **does not delete or lose** accepted human requests.
3. When an authorized human opens a translation UI (WebUI, Blazor, desktop, TUI, etc.), the interface queries a **state snapshot**: waiting count by priority, languages/direction, age, applicable targets and optional filters. It then may subscribe to live changes. **A SystemMonitor event is not the source of truth** for these counts; connection to monitoring is optional and loss of events must not lose requests.
4. A worker can select/request a compatible work item through QueueMaster's **claim/reservation API** (or request the next eligible item). The orchestrator/QueueMaster atomically grants exclusive ownership/lease; a second human worker cannot take the same active item.
5. Human translator edits and submits the result. The shared ResultProcessor validates language/format/placeholder invariants and writes a durable completion/checkpoint under the job policy; any required editor/audit trail is preserved. Complete/fail/return results flow through the same typed provider-independent contracts as automatic engines.
6. The request owner can obtain completion later; application-facing notifications may assist but are not the durable result store. A translator logout, browser tab close or server restart must not silently delete pending work or uncommitted results.

## Human-specific readiness, ownership and failure

An offline human worker does **not** mean the provider contract is unavailable system-wide. It means no human processing capacity is presently online. Compatible pending human work remains visible on the next UI connection.

Human sessions may be long-lived and can be interrupted unexpectedly. The **ownership lease and recovery policy** must ensure at most one valid holder, allow voluntary release, safe expiry/reclaim after disconnect or inactivity, and avoid losing drafts. If a translator's lease expires and another takes over, only the current valid claim may publish final completion; later stale submissions require conflict detection rather than overwriting another worker's result. A translator may optionally save a private/recoverable draft under the configured persistence/privacy policy; drafts do not equal final publication.

Human worker capability includes authorized access to appropriate source/target languages and any specific work/project permissions. The UI should show **waiting/claimed/in progress** separately, plus per-priority totals and "mine" filters; counts can vary with active claims and permissions. The number and placement of translator UI views are host choices, not required UI dependencies of UnderstandMe.

## Relationship to P0 RealTime and Persistence

Previously agreed: ordinary `P0 RealTime` requests are volatile and may return source text immediately. This remains valid. **A translation explicitly queued for eventual human work must be durable**, regardless of when a translator connects.

If a transient RealTime request should also trigger later manual completion, create/accept a **separate durable human continuation item** (with correlation to its original RequestId/segment) according to the user's effective policy. Do not silently persist every P0 request just because Human is a supported provider; do not promise indefinite delivery on a volatile request without committing the human continuation. Its final result can update an authoritative dictionary, SystemPersistent memory or a client-facing result as permitted; never assume browser session remains open.

The `Persist` state of a human backlog item is a correctness requirement for delayed human fulfillment, not optional step-history `Preserve` or an unlimited audit archive. Reconciliation after restart reconstructs the existing five priority queues from durable pending and in-progress items without creating independent human-only queues.

## Scope and further choices

This agrees on durable human queueing, on-demand interface, trustworthy per-priority counts and exclusive claim/release. Still to specify: claim timeouts and heartbeat rules, draft recovery, UI choice of next versus manual selection, human capacity/skill matching, cancellation/deadline handling, translator audit permissions, accessible privacy-safe preview, manual payment/time accounting if ever needed, and how long to preserve completed human results.

Test empty/no-human sessions, persisted pending work over restart and long offline intervals, counts per P0-P4, per-language eligibility, concurrent claim attempts, stale claims/completion, draft recovery, cancellation, and loss of UI/SignalR connection without loss of work.

See [QueueMaster](../QueueMaster-and-Reliability/en.md), [Translation Chains](../Translation-Chains/en.md), [Request Settings](../Request-Settings-and-Live-Translation/en.md), [Architecture](../Architecture/en.md), [Implementation Status](../../IMPLEMENTATION_STATUS.md).
