# QueueMaster, Priorities and Reliability

Status: agreed operational concept; concrete synchronization and SQL schema are not yet designed.

## Work ownership

QueueMaster is the logical owner of dynamically registered **queue lanes**, each retaining the familiar P0–P4 priority classes where enabled. A standalone host may have one default five-priority lane. Producers submit logical jobs, batches or items through its API. Translators do **not** dequeue central work directly; their individual small FIFO buffers request the next compatible item when capacity appears.

Provisional priority names: P0 `RealTime`, P1 `Interactive`, P2 `High`, P3 `Normal`, P4 `Background`.

An endpoint buffer is intentionally small (five items is an initial tuning candidate). Every free slot can request **one** item. QueueMaster chooses the highest-priority waiting item compatible with that endpoint. Within an endpoint buffer, work remains strict FIFO; there is no priority-based skipping, preemption or buffer rearrangement. This bounds queued-ahead work without a complex load-balancing algorithm. Faster translators naturally request replenishment more often.

## Captured request policies versus orchestrator commands

A request captures its effective `Settings` when created. `QueuePriority` remains exactly one level P0–P4, while optional behavior flags (for example `Mandatory`) may be combined independently. A RealTime Mandatory request may select another ready, compatible **automatic** endpoint when its preferred machine engine is busy rather than waiting, subject to configured eligibility and spending limits; a RealTime request without that flag may return source text and continue translating by segments.

Changing the configured provider priority or selection flags applies **only to new logical requests**. Waiting or running requests keep their accepted policy snapshot and their original job priority. **Administrative orchestrator control has precedence:** pause/stop of a queue prevents further dispatch despite Mandatory flags, without reinterpreting the queued items. A paused queue may resume later under the original request snapshots. Credential revocation/unavailable endpoints require explicit operational failure/recovery handling rather than silent policy rewriting. [Request Settings and Live Translation](../Request-Settings-and-Live-Translation/en.md).

## Request admission before central queueing

The orchestrator validates and **normalizes a caller's requested provider and optional settings before QueueMaster accepts the job**. A direct preference for Google API cannot force an endpoint forbidden by administrator policy. When a requested endpoint is disallowed or lacks usable quota, the orchestrator keeps the translation feasible by selecting/ranking other allowed compatible workers whenever possible; reject the request with a precise reason **only if there is no feasible authorized route**. The accepted work keeps the original request ID, a frozen effective policy snapshot and any notices of substitutions. A queue receipt means `Accepted`, not `Completed`.

At dispatch, current queue pause and account hard limits may hold a previously accepted job or cause another **already-authorized** endpoint to be chosen under the captured selection policy. QueueMaster remains responsible for ownership, priorities and atomic dispatch, not for vendor authorization or the commercial selection calculation. Explicit rejection before acceptance is distinct from a clear failure/unavailability after a formerly feasible job becomes impossible. [Admission rules](../Request-Admission-and-Normalization/en.md).

## Shared capacity, clients and admission policy

The orchestrator supplies QueueMaster only **admitted effective requests** after applying the caller's `ClientPolicy` and global administrator settings. Client identity and per-client quotas are **not new global job priorities**; explicit dedicated tenant queues are dynamically enrolled with **separate** worker pools. Do not create a lane for every anonymous community client by default. Client-specific caps and reserved internal capacity can be admission or dispatch constraints; QueueMaster keeps ownership of the same five priority queues, compatible endpoint handoff and orchestration control.

An optionally public Afutrans deployment uses a **protected community LibreTranslate worker pool**, separate from internal realtime and dedicated tenant resources. Any surplus-resource donation must be an explicit owner/operator decision preserving reserved capacity. Choose explicit rate/concurrency/queued-item caps and fairness/resource-reservation behavior before implementation. Validate the trusted caller identity at the external application boundary; at dispatch check current client caps **and** current provider account limits, preserving the originally accepted request's preferences. [Client Access Policies](../Client-Access-Policies/en.md).

## Chargeable attempts and operational quota guards

A request's provider preference is frozen when it is created, but **the right to incur new billable usage is checked at dispatch** using the current, administrator-approved account quotas and hard spending limits. A job already in the queue must not silently exceed the current provider billing cap simply because it was accepted earlier. If the selected endpoint has no permitted quota, Pipe/provider routing can choose another already-authorized eligible endpoint, wait for plan renewal/admin action, or return a defined unavailable outcome under the original request policy.

QueueMaster continues owning **lane-scoped priority queues and work leases** and supports deliberately enrolled tenant lanes. It does not turn subscriptions into a new global Paid priority or embed billing rules. An independent usage service atomically reserves expected metered units across all workers sharing that account and reconciles actual/estimated use after the attempt; attempts with unknown provider-side charge remain tracked. Billing evidence is durable and **not** SignalR telemetry. [Provider Billing and Quotas](../Provider-Billing-and-Quotas/en.md).

## Human-only execution is valid

**Human is the only mandatory supported translation provider**. QueueMaster must be able to enqueue/reserve/release compatible manual work for a Human worker and accept the completed human result without any automatic provider or paid billing account. Human worker capacity may be long-lived or zero while a person is away; the orchestrator's effective request wait/deadline/persistence policy decides whether the request may remain pending or must return unavailable/original text. The existence of the Human contract does not imply instantaneous availability.

LibreTranslate, AfroLT and all other automatic endpoints are optional registered features. If none are activated, the global queue contract and recoverable human job lifecycle remain usable; no phantom machine buffer, translator model or AI reviewer is required. [Translation Chains](../Translation-Chains/en.md), [Provider Routing](../Provider-Routing/en.md).

## Durable human backlog; no Human in P0 RealTime

Human translators consume **durable non-RealTime P1-P4** requests from permitted QueueMaster lanes. There is **no sixth global priority class named Human**; a customer lane is a separate resource domain, not a human-priority queue. Human work is persisted when accepted regardless of whether a translator is online. The translator UI can open later, show pending/in-progress totals grouped by the existing priorities (P0 shown as inactive/zero for Human), and atomically claim compatible P1-P4 work with exclusive lease/release, validation and recovery.

**P0 RealTime is an automatic-only, volatile dispatch path.** The Human provider **must never be assigned P0 items**. In a Human-only installation **P0 is not activated and incoming RealTime translation returns the source text essentially immediately**. Even if automatic providers exist but fail or cannot satisfy the policy, no RealTime item may escalate to Human; `Mandatory` cannot change this. A real-time message may contain private chat content and must not be persisted as Human work, automatically copied into a translator inbox or exposed for later manual review.

Delayed human translation is permitted **only by a distinct, explicitly submitted and authorized, durable non-P0 request**, with approved human visibility/storage. It is **not** an automatic continuation or fallback of P0, even when a client would otherwise prefer to obtain a translation later. The normal snapshot and notification distinctions remain: QueueMaster's persisted job state is authoritative, SystemMonitor is not a work database. Details: [Human Translation Workflow](../Human-Translation-Workflow/en.md).

## Notifications

`QueueEmpty` represents whether an individual lane+priority queue has no items. An enqueue that **atomically changes a given queue from zero items to one** triggers the common `AddedToQueue` event; enqueues to already nonempty queues do not. Idle buffers can wake and request work. A periodic check (initial suggestion ~500 ms) provides a safety net. A buffer that becomes free while compatible work already exists also requests more.

QueueMaster (not the event) selects the priority and ensures one owner per item. A partially empty buffer can be replenished one slot at a time; default size and thresholds remain subject to tests.

## Endpoints, failures and recovery

An endpoint has `IsReady` as a single boolean readiness state. Capabilities (source/target languages, HTML, batch/alternatives, model/context, privacy) and available capacity are separate. An unsupported language is not a server failure.

Temporary endpoint outages are expected. Retry cadence/count is configurable; a previous LibreTranslate experiment used 2-second retries up to 10 attempts. During transient unavailability, a small buffer can remain reserved briefly or eligible unprocessed work can be reassigned when another compatible endpoint requests capacity. Avoid both duplicate publishes and starvation; exact leasing/retry/hand-off details will be designed and tested later. Periodically check unavailable endpoints and allow them to rejoin automatically.

## Private durable results do not change volatile P0 work

**Privacy and Persistence are independent.** A request can have `Privacy = Private` and still keep an authorized, stored final translation of a chat/forum message so history does not have to be translated every time. The **P0 RealTime work item remains volatile** and automatic-only; only its validated result can be separately committed to an access-scoped translation store/cache under the request's retention and source-owner policy. It does not become a pending Human assignment or a globally shared dictionary entry.

QueueMaster owns execution and durability of explicit non-P0 work, not per-conversation authorization or content retrieval. The application/storage adapter owns scoped result lookup, current reader permission checks, source-revision matching, deletion/expiry propagation and safe data storage. Private content must not enter manual audit, Human worker buffers or content-bearing monitoring events. **Store only required results and metadata**, not full P0 histories by default. [Privacy and Scoped Storage](../Translation-Privacy-and-Scoped-Storage/en.md).

## Persistence

P0 RealTime requests are volatile/fire-and-forget, without durable job history. Normal/long-running tasks may use `Persist` so an acknowledged request survives restarts. Default standalone operational database: SQLite; larger deployments can select supported EF Core database providers. This is **not** the same as the source/target dictionary storage.

A durable acceptance ACK means the request has been committed to durable work storage, not merely enqueued in memory, and not completed. A transient live request has its own non-durable response semantics. Track item ownership and completed checkpoints so interrupted work can be resumed safely. Outbox/Inbox with event IDs and acknowledgements supports at-least-once delivery plus idempotent effects.

Operational state must recover even if the process stops after a DB commit and before inserting an item into an in-memory queue. Build in-memory queues from durable state when needed. SignalR reconnection, local events, and ~3-hour source reconciliations cover different problems and should remain independent.

## Lifecycle

Keep `Persist` (recoverable job execution), `Preserve` (optional step outputs/history), and `LifeTime` (cleanup policy) separate. `FinalizeAndFlush` can delete disposable working state only after the receiver acknowledges acceptance and the relevant policies permit it. Mandatory manual-edit audit is not disposable with a job.

Related: [Translation Chains](../Translation-Chains/en.md), [Notes](../../Notes.md).

## Contribution-funded capacity is a service admission policy (2026-10-20)

Hosted Afutrans can offer free **durable deferred** translations on bounded community capacity and optional contribution-funded faster or dedicated LibreTranslate worker capacity. This belongs to host Orchestrator/`ClientPolicy` admission, allowed priority ceilings, compatible worker allocation and shared resource quotas; it is **not a sixth central queue or a new P0-P4 priority meaning**. QueueMaster retains five logical priorities, bounded endpoint FIFO buffers and pull-based dispatch.

- P0 remains volatile/automatic-only and requires actual realtime service eligibility/capacity. A waiting free translation that can finish later must be explicitly submitted as a **non-P0 durable job** and clearly identified to the caller; it never silently converts a private chat P0 request into persistent/human review work.
- Contributor entitlement can authorize requests at an existing faster compatible priority or a reserved/isolated worker pool, subject to the current orchestrator limits; payment cannot overrule admin pause/stop, budget ceilings, format/privacy restrictions, worker `IsReady` or secure tenant authorization.
- Concrete anti-starvation policy for accepted `CommunityFree` jobs must guarantee some progress under sustained priority load, while maintaining internal workload reservation and no scheduler-owned vendor billing. This may be by minimum worker-capacity reservation or quota/fair dispatch constraints; aging policy needs explicit design to avoid contradicting frozen request priorities.
- Admission and the worker queue may impose bounded waiting, max queue depth, per-client word/rate/concurrency, and fair rejection/backpressure rather than indefinitely accepting work with no resources.

See [Client Access Policies](../Client-Access-Policies/en.md) and [Afutrans capacity proposal](https://github.com/afrowaveltd/Afutrans/blob/main/Docs/Community-Capacity-and-Contributor-Priority/en.md). **Design only; no change to QueueMaster implementation.**

## 2026-10-20 — dynamic isolated queue lanes supersede shared paid priority

**Superseding user decision:** paid tenants must not jump ahead of CommunityFree users in the same queue/worker pool. Afrowave gets a protected internal lane including eligible P0 RealTime; `CommunityFree` receives its own protected shared worker pool for explicit deferred non-P0 work; each authorized `TenantDedicated` gets a separate queue lane **and its own backend resource capacity**. Customer can rent separately managed LibreTranslate VPS or attach their **own** LibreTranslate, Microsoft/Google Translator, Ollama/AI or other compatible endpoint, without a compulsory VPS subscription. Existing P0–P4 priorities remain *within each lane*.

QueueMaster owns dynamic lane-specific queues and leases, not exactly five physical queues globally. A new lane only exists after explicit tenant enrollment, not automatically for every anonymous caller. Worker/endpoint binding, provider-account billing ownership and shared gateway/DB/queue resource limits must enforce real isolation. No automatic cross-lane resource fallback; no purchase of queue position in the community pool.

See [Queue lanes and Tenant-Owned Providers](../Queue-Lanes-and-Tenant-Owned-Providers/en.md) and [Afutrans dedicated-capacity decision](https://github.com/afrowaveltd/Afutrans/blob/main/Docs/Dedicated-Capacity-BYO-Backends-and-Transparency/en.md). **Design only, no scheduler/host code or tests.**

## Idle backend buffer lending without cross-lane queue mixing (2026-10-20)

Separate QueueLanes remain, with P0-P4 priorities inside each, but capacity can be **temporarily lent**: a donor-owned provider buffer that has an eligible free slot may pull one **authorized** borrower-lane request under a short-lived `CapacityOffer`/slot lease. Keep the original work lane, durable job identity, tenant/target privacy, provider cost owner and per-buffer FIFO. This does **not** mean an open consumer can choose a different tenant's engine arbitrarily or preempt community work.

Afrowave may voluntarily offer *unused* first-party P0-capable backend slots to CommunityFree **without guaranteeing external realtime**, with internal protected minimum; owner P0 demand immediately stops new loans but in-flight nonpreemptible requests complete/drain within bounds. For strict internal realtime, reserve at least the required always-available slots instead of pretending an already-running call can be interrupted without provider support.

Prefer **throttled durable non-P0 community admission + fair waiting** over permanent translation-credit exhaustion, subject to real bounded queue bytes/storage, denial-of-service protection and honest backpressure. No infinite queue promise, no silent P0 persistence or Human fallback. Home machines may use authenticated outbound-only worker agents when inbound ports are unavailable.

See [Capacity Lending and Backpressure](../Capacity-Lending-and-Backpressure/en.md). **Architecture only.**

