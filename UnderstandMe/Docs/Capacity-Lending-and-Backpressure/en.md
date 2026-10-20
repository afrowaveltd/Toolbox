# Voluntary Backend Buffer Lending and Adaptive Backpressure

**Status: architecture/product policy agreed 2026-10-20; no implementation or new runtime tests.** Builds on [Dynamic Queue Lanes and Tenant-Owned Providers](../Queue-Lanes-and-Tenant-Owned-Providers/en.md) and the [Afutrans Cooperative Backend Sharing product concept](https://github.com/afrowaveltd/Afutrans/blob/main/Docs/Cooperative-Backend-Sharing/en.md). This supersedes older hosted suggestions of fixed per-user monthly translation credits as a default and generic cross-lane resource prohibition **where an explicit donor-controlled loan is separately authorized**.

## Separate queue ownership from physical backend lending

QueueMaster conceptually retains **distinct lane queues** such as `AfrowaveInternal`, `CommunityFree`, `TenantDedicated`. P0–P4 are urgency classes inside each lane, not money tiers. Physical provider endpoints have **small bounded FIFO buffers**, and one eligible free buffer slot may be *temporarily leased* to execute an authorized job from another lane. Work ownership, original queue lane, tenant/target permission, provider accounting and source/result visibility never migrate with the buffer. The **endpoint's resource owner** must consent to lending.

Default isolation remains: no commercial queue jumping inside CommunityFree; no tenant's account or AI model offered to others without separate owner authorization; no implicit fallback when an endpoint dies. Lending is an explicit **resource-owner and consuming-lane permission** and is **revocable/expiring**, not blanket cross-tenant eligibility.

Examples:

- Afrowave's idle first-party P0-capable translation workers may offer explicitly configured spare buffer slots to community non-P0 jobs as **best-effort free capacity**, while respecting reserved first-party slots. Public P0 borrowing is independently opt-in, genuinely immediate only while available and always **nonguaranteed**; a rejected/expired P0 never silently turns into durable work.
- Tenant A runs its own AI model and voluntarily offers compatible AI translation/review capacity to approved community tasks; Afrowave may separately offer its own LibreTranslate capacity to A for eligible jobs. Two permissions, separate, **no forced symmetric barter**.
- Tenant B uses a private engine for genuine P0 realtime and chooses the allowed community/deferred engine for non-P0 tasks (or vice versa); policy selects compatible endpoints by job/format/locale and privacy rather than routing a whole customer to exactly one provider.

## Mechanism: slot borrowing without unsafe preemption

1. Operator or owner enables an `EndpointCapacityOffer` candidate concept containing original owner, allowed borrower lane(s)/scopes, capabilities, privacy classes, concurrent available slots, optional per-time/provider-cost cap, expiration, minimum reserved capacity for the owner, and permitted P0 vs deferred request categories.
2. When an endpoint's *individual* buffer has an eligible free slot, a short duration lease allows QueueMaster to select the next compatible request from an approved borrower lane. The borrower's queued work is not reprioritized against unrelated requests in the donor's own queue; lender reservations and protected resources take precedence.
3. The Translator buffer keeps FIFO and atomic job claims. The lease is **not** authority for a borrower to inspect the donor's backend keys, configuration, queue or results. A provider can be shared only through authenticated worker-facing execution and scoped result return.
4. On owner's higher-priority demand or explicit offer withdrawal, stop **new borrowed leases immediately**, but let already-running non-preemptible LibreTranslate/AI calls finish within configured maximum item duration. Reclaim slots at completion/buffer boundary. For truly urgent internal P0 SLA, keep enough capacity **always reserved**, or implement cancellation only when provider protocol, billing and idempotency support it; do not claim instantaneous preemption.
5. If a borrowed provider fails, expires, is revoked, exceeds its bill-to cost hard cap or becomes ineligible for the content's processor/privacy rules, only compatible policy-allowed endpoints may receive new work. Persisted borrower jobs remain in their original lane/retry ledger and are re-admitted safely; P0 stays volatile.

Any capacity offer is **nonguaranteed** unless owner has explicitly contracted and provisioned actual reserved resources. Display opportunistic availability with timestamp and accurate failure/degraded conditions. Do not promise an advertised realtime SLA merely because the internal P0 pool is currently idle.

## Rate, backlog, fairness and no artificial translation-credit cliff

Preferred hosted CommunityFree behavior: accept **explicit deferred P1–P4** jobs as long as reliable storage/backlog and real downstream capacity can reasonably support them; slow down dispatch and provide honest queue position/status under load instead of using arbitrary monthly request/word credit limits as the default. Do **not** interpret this as permission for infinite accepted work.

Handle overload with adaptive admission/backpressure, finite in-flight/queued bytes, caller/group fair scheduling, abuse rate protections, max accepted backlog/retention, durable work ACK and client-visible retry/deferral. Rate controls limit bursts/instantaneous resource consumption, not a permanent monthly tally forcing payment. If capacity is truly exhausted, return `Busy`/`RetryLater`/`QueueFull` with safe retry guidance rather than accepting and dropping a request. Hard spending/provider-account caps remain binding for third-party metered backends. An application owning its own backend can avoid artificial total-word quotas while obeying its actual account/host capacity.

Public/anonymous fairness must prevent a single caller from holding all pending slots, without third-party tracking, behavioral profiles or cross-tenant content logging. Free scheduling policy must not be influenced by a separate tenant's monthly payment; the isolated protected community pool remains intact.

## Provider-owned endpoints, optional reciprocal compute, and privacy

A backend owner may share *selected* capabilities (language pairs, plain text/HTML, AI translation/review), resource budget and borrowers. The actual source/target owner must also permit content processing by that machine/provider, especially third-party AI services and private messages. **Two-sided authority is necessary:** provider grants compute rights; content owner grants processor visibility. Human work remains distinct; no P0 fallback to Human.

Customer-hosted LibreTranslate/Ollama may be connected by HTTPS endpoint or via authenticated **outbound worker agent/tunnel** when home/office firewall has no public inbound port. Agent pulls authorized work from the hosted scheduler through a verified endpoint/lease, limiting outbound data and avoiding generic internal network proxy/SSRF. Include replay, reconnect, duplicate delivery, cancellation and long-running provider call tests.

Meter usage at the real account owner: donated cloud Google/Microsoft/AI capacity must have an explicit permitted budget or hard cap, cannot silently charge another tenant/Afrowave. Donor capacity usage and receiver queue status can be reported in aggregate without exposing user text or credentials. No **automatic reciprocal rights** simply because both entities offered resources.

## Provisional concepts, not frozen C# interfaces

`CapacityOffer`, `EndpointSlotLease`, `OwnerReservedSlots`, `AllowedBorrowerLaneIds`, `CapabilityScope`, `Expiry`, `LeaseRevocation`, `CallerBackpressurePolicy`, `QueueEstimatedWait` and `OutboundWorkerAgent` are descriptive only. Integrate with existing QueueMaster effective request snapshot, Translater buffer FIFO, durable WorkLedger/usage and optional Messenger/SignalR events. A borrowed slot is transient capacity; it is **not** another queue, user permission to a provider account or a new kind of translation job.

## Acceptance tests

1. Free users submit indefinitely at sustainable intake: no per-month 'credits exhausted' rejection; backlog slows processing fairly without losing accepted work. An intentional flood triggers finite safe backpressure, not RAM/disk failure.
2. Idle internal worker buffer offered to CommunityFree: borrowed short non-P0 jobs run only when owner-reserved minimum unaffected; internal P0 arrival stops new loans immediately and does not claim unsafe preemption of a running translation.
3. Internal P0 spare advertised to external callers only as explicitly enabled **best effort**, with no guaranteed availability or conversion to deferred/Human on failure.
4. Two separately scoped resource-sharing offers (customer AI → CommunityFree; Afrowave LibreTranslate → customer) work independently, honor sender content/AI privacy and donor hard budgets, and independently revoke.
5. Customer self-hosted engine works through outbound agent without exposing inbound ports; untrusted node cannot use agent as arbitrary internal network proxy.
6. Borrowed backend fails or quota expires mid-request: durable borrower work safely retries in its original lane and preserves job identity; no cross-tenant data/account costs leak.
7. Same queue lane can route own P0 to owned backend and explicit non-P0 to an allowed borrowed provider under separate capability/retention policies.

**Implementation:** architecture only. No `CapacityOffer`, new worker lease, multi-owner provider onboarding, outbound agent, adaptive queue admission or tests implemented.
