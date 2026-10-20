# Dynamic Queue Lanes and Tenant-Owned Translation or AI Backends

**Status: revised architecture decision, 2026-10-20; documentation only, not implemented.** This explicitly supersedes the older hosted Afutrans idea of giving paid clients higher priority over free community requests on the *same* worker pool. The Afutrans product design is [Dedicated Capacity, BYO Backends and Transparency](https://github.com/afrowaveltd/Afutrans/blob/main/Docs/Dedicated-Capacity-BYO-Backends-and-Transparency/en.md).

## The agreed service model

| Lane | Permitted work | Reserved backend capacity |
| --- | --- | --- |
| `AfrowaveInternal` | Our first-party applications, including eligible P0 RealTime | Our protected first-party compute/LibreTranslate workers |
| `CommunityFree` | Approved anonymous/registered explicit **deferred** non-P0 P1–P4 translations | A protected free community pool; paying customers never jump its queue or remove its workers |
| `TenantDedicated` | A particular authorized customer/project, P0–P4 subset by config | Its own managed VPS/LibreTranslate workers, **its own** LibreTranslate instance, Google/Microsoft provider account, Ollama/AI or other compatible registered endpoint |

**Key rule:** a customer is not paying to outrank someone in the community pool. They are paying to fund independent capacity *or are using their already-owned capacity with no compulsory rental fee*. No global commercial priority overrides community worker assignments. A dedicated lane without **independent worker pool binding and resource quotas** would not provide real isolation.

## QueueMaster remains one common scheduling concept

QueueMaster owns work admission handoff, priority selection, durable non-P0 leases and dispatch to bounded FIFO worker buffers. The original five categories **P0 RealTime, P1 Interactive, P2 High, P3 Normal, P4 Background** are preserved **inside each lane** rather than being precisely five system-wide physical queues. Standalone/local hosts can instantiate a single default lane; hosted multi-tenant Afutrans may register lanes dynamically. A lane can disable priorities (community generally disables P0); there is no compulsory new `Paid` priority.

Do **not** create a separate lane for each anonymous free caller: they use the same CommunityFree lane and capacity share. A new TenantDedicated lane must be explicitly enrolled/authorized for a real resource owner and bound only to that owner's allowed worker pool/endpoint accounts. A request-provided `LaneId`, `Priority=P0` or endpoint URL is never itself authority.

Existing P0 invariants survive: immediate/volatile/automatic-only, no Human provider and no silent conversion of private chat into durable or manual work. A free request that can wait is **separately and explicitly submitted as a durable non-P0 job** with an Accepted/Deferred receipt. P1–P4 jobs preserve captured settings, current administrative limits, durable idempotent claims and result privacy. Workers retain a small per-endpoint FIFO and compatible-job pull semantics.

Suggested lane configuration dimensions, **not frozen API types**: `LaneId`, `OwnerTenant`, `LaneKind`, `AllowedPriorities`, `WorkerPoolId`, `AllowedEndpointIds`, `CapacityLimits` (queue size, bytes, request rate/concurrency, CPU/network/memory), `TargetAccessPolicy`, `PersistencePolicy`, `FallbackPolicy` (default none across tenant/community boundaries), `CostOwner`, `State/Version` and source+result retention rules.

### Lifecycle

Register authorized owner/target → validate provider endpoints and trusted credentials → create/start lane in nonpublic initializing state → attach verified worker pool → `Ready` admission → optional `Paused`/`Draining` during changes → securely retire. Persist/recover accepted non-P0 work across host restarts; drain/cancel with explicit policy and avoid duplicate execution, orphaned work or exposure to newly assigned tenants. Only approved owner/admin may attach or remove endpoints or secrets.

### Prevent the shared control plane from being overwhelmed

Even if customers provide their own LibreTranslate or cloud credentials, their requests consume **our gateway, scheduler, storage, validation, SignalR and network bandwidth**. Bound those shared resources per lane, prevent infinite queue growth, use overload backpressure and protect internal/community allocations. Dedicated backend customers need not face arbitrary per-word caps, but neither physical VPS compute nor external provider API quotas are infinite. The right to use owned resources is not the right to exhaust a shared server.

## Bring Your Own Provider (BYOP), including AI

- Provider registration is tenant-scoped and optional: local/customer-hosted LibreTranslate, managed dedicated LibreTranslate VPS, customer-owned Microsoft/Google translation API account, Ollama/self-hosted AI, or an approved externally hosted AI integration. One lane may include multiple eligible endpoints with distinct budgets/credential scopes and explicit preferred fallback.
- Provider adapter declares real supported capabilities: source/target language, plain text/HTML/document, placeholder fidelity, synchronous/batch, metering, privacy classification, capacity and applicable AI translation/review features. **Not every AI endpoint implements a conventional translation contract.** Reuse generic endpoint management while keeping capability-specific workers.
- Configure secrets in a tenant-scoped service vault or an **owner-operated agent**; **do not store raw keys/SQL/FTP secrets in Id**, expose them in public monitoring, or use a customer-supplied arbitrary URL as an unrestricted internal HTTP proxy. Authorize egress/destination and prevent SSRF/DNS rebinding/cross-tenant network or secret access. When insecure owner-selected test protocols are supported, explain risks and isolate their worker.
- Bind `EndpointId` and `ProviderAccountRef` to a trusted tenant/lane/target and approved privacy policy. Usage charges from Google/Microsoft/AI belong to the actual credential/account owner and its account hard limits; no fallback to Afrowave's own paid provider or community workers unless both owners explicitly authorize a budgeted exception.
- Cloud AI may receive confidential content: target owner permits which processors may see what data. Optional AI jobs and text translation use the same lane isolation but not necessarily the same content-processing policy or result verification.

## Accounting and transparency

The hosting operator's optional public cost/funding dashboard displays **real received contributions, managed VPS rental, network, backups, payment fees/taxes, agreed setup/support, community funding and capacity actually added**. Avoid claiming 100% reaches VPS if unavoidable operating charges exist; distinguish donor-supported community pool from privately funded tenant machines. Respect privacy of payer identities and translated text.

Customer-owned backend use carries **no mandatory Afrowave VPS rental**. Future optional management/support terms must be clear before enrollment; the host does not invent a payment debt when a tenant already pays its provider. Distinguish contributor receipts from the separate metered provider `UsageLedger` and from fixed VPS operational costs.

## Acceptance tests for future implementation

1. Commercial tenant floods its lane while public jobs flow: no commercial worker can consume community or internal reserved worker slots; free deferred progress remains available.
2. Free request spoofs `LaneId=AfrowaveInternal`, `P0`, another tenant's `EndpointId` or provider credentials: rejected/normalized before dispatch.
3. Customer connects own LibreTranslate without VPS rental; Google/Microsoft or Ollama endpoint is used only for its approved capabilities, own credentials, own bill-to plan and target privacy.
4. Provider fails/exhausts quota: tenant work reports/queues honestly and never silently borrows other owners' workers or provider accounts.
5. Create, pause, drain, restart and remove dynamic lane with durable jobs in flight: no duplicate effects, stale grants, leaked content or lost accepted work.
6. Dedicated customer offers unlimited requests: physical and **shared** host safeguards reject excess load rather than exhausting the gateway/QueueMaster/DB.
7. Standard Human-only/standalone UnderstandMe works without Id, Gateway, public endpoints, subscriptions or any automatic providers.

**Implementation:** this does not create a QueueLane type, tenant registry, account integration, queue runner, BYOP connector or tests. It is a documentation-level correction to the earlier single-global-queue design.

## Voluntary capacity lending and adaptive community backpressure (2026-10-20)

**Refinement:** keep isolated lane job ownership but optionally *lend a compatible **idle Translator buffer slot*** from one owner's backend to another **expressly authorized** lane. Examples include Afrowave giving CommunityFree occasional unused internal realtime-capable slots, or a customer voluntarily donating an AI provider while being allowed to use a separately shared LibreTranslate endpoint. Owners opt in per direction with allowed capabilities, borrower groups, processor privacy, actual paid-provider spend caps, expiry and reserved-owner capacity. Lending neither merges the queues nor sells priority over community work; default remains no cross-lane sharing.

When the donor needs its own capacity, stop new borrowed dispatch immediately and reclaim on a bounded **in-flight work completion**. Noninterruptible backend calls cannot be instantly preempted: retain truly guaranteed P0 capacity reserved, or use explicit safe cancellation when available. Any public use of spare P0 is **nonguaranteed**. A customer can use its own P0 engine and choose authorized alternative for other work.

CommunityFree should **prefer slower deferred processing over arbitrary monthly/total translation credit expiration**. Preserve practical queue/backlog/storage bounds, per-caller burst controls, actual cloud account hard costs, privacy checks and `Busy/RetryLater` when no safe admission exists. Explicitly accepted deferred work is durable; P0 is volatile and never silently becomes Human/persistent.

See [Capacity Lending and Backpressure](../Capacity-Lending-and-Backpressure/en.md) and [Afutrans Cooperative Backend Sharing](https://github.com/afrowaveltd/Afutrans/blob/main/Docs/Cooperative-Backend-Sharing/en.md). **Design only.**

