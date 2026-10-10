# Client Access Policies and Optional Public Translation API

Status: design agreement, 2026-10-10; not implemented. This document complements, rather than duplicates, the general orchestrator/provider/billing architecture.

## Purpose and default behavior

An installation may run Afutrans as an internal translation service **and optionally publish spare translation capacity as a public API**. Different callers must be able to use different provider tiers and resources without creating vendor-specific logic inside QueueMaster or translation workers.

**Default for ordinary, known clients**: no additional client-specific provider restriction — all configured provider families are eligible **subject to global administrator restrictions, provider capabilities and account hard limits**. A client policy can narrow those permissions. Public/anonymous access is a distinct explicitly configured client profile; it is not granted unlimited use of paid API accounts simply because ordinary known clients have no extra restrictions.

A public deployment is optional and is a host exposure decision, not a core library requirement. By default, merely installing the UnderstandMe library must not expose an external HTTP listener.

## Distinct responsibilities

- `ClientContext`: trusted identification of the caller (client/application ID, authenticated principal or designated anonymous/public profile, optional group/tenant). Resolve this at the API/application boundary; **do not trust an arbitrary self-declared ClientId in the request body** to authorize spend.
- `ClientPolicy`: permissions and limits associated with the verified client, optionally inherited from a named group or plan. It determines which provider families/endpoints and billing accounts the client can use, allowed features, per-client budget/usage quotas, maximum request size/concurrency/rate and, if configured, request-priority ceilings.
- `OrchestratorSettings`: administrator/system-wide hard restrictions (enabled backends, LocalOnly/privacy constraints, permitted provider billing accounts, current quota hard caps, queue pause/stop, infrastructure resources). These have precedence over any client preference or entitlement.
- `TranslationRequest / EffectiveSettings`: content, desired language, format, queue priority/flags, provider **preferences**, deadlines and requested features. A caller can suggest Google without gaining permission to use it.
- `ProviderAccountPlan + UsageLedger`: independent authoritative price/package account records and reservation/settlement of real provider charges; not a per-client permission check by itself.

Evaluate the **intersection of permissions**: an engine must be enabled by the system, permitted by the resolved client policy, compatible with request hard requirements, ready at dispatch and allowed by current provider/account quota. Request flags such as `Mandatory` cannot widen access. A provider account hard cap can disallow new paid calls even for an otherwise entitled paid client.

## Policy precedence and default profiles

Policy resolution: administrative hard constraints > resolved named client/group policy and any explicit client-specific overrides > Translation Target/Pipe defaults where applicable > individual request preferences. Client and system rules can reduce the pool of endpoints; request preferences only rank **permitted** ones. Avoid treating a plan or group label as authentication.

Illustrative profiles, not hard-coded types:

| Profile | Eligible translation providers | Optional resource controls |
| --- | --- | --- |
| InternalDefault | All enabled and globally allowed engines by default | Regular orchestrator/account limits |
| RegisteredFree | Local/free endpoints such as LibreTranslate | Word/day quota, rate/concurrency cap |
| PublicAnonymous | Only administrator-approved free/local endpoints | Strict input/rate/concurrency limits; spare-capacity priority |
| PaidClient | Provider accounts/tiers explicitly included in purchased entitlement, potentially Google/Microsoft/AI | Client's plan, word/token quota or cost ceiling |
| CustomClient | Configured per-client allow/deny choices and limits | Overrides permitted by administrator |

Provider permission can be expressed by capability/tier and by explicit endpoint/account IDs when needed. **Paid** describes the configured account's charging regime, not a permanent vendor label: a provider family may have self-hosted and cloud endpoints with different costs and permitted usage.

Client policies need explicit enable/disable status, version/effective time and straightforward admin review, but do **not** require an elaborate central identity platform. For externally exposed chargeable services, the boundary must recognize the real caller through appropriate application/API authentication (e.g. an API key/session); the **lite GUID-only SystemMonitor identification is not an appropriate model for authorizing billable translation requests**.

## Best-effort functional fallback

A caller might request Google API but their `ClientPolicy` allows only self-hosted LibreTranslate. On admission, the orchestrator preserves the original request, derives effective allowed providers, changes the preferred provider where necessary and reports `RequestedProviderNotAllowedForClient` or a similar typed adjustment. It **translates with an allowed suitable alternative if one exists**, rather than rejecting solely because a preferred paid endpoint is disallowed.

If no allowed compatible route remains, return a clear `NoClientEligibleProvider` / `ClientQuotaExceeded` / `NoCapacityWithinPolicy` reason as appropriate. Never dispatch a client-restricted paid call secretly, even after `Mandatory`, a model error, a timeout, or automatic escalation. Keep the core principle: the orchestrator tries to fulfill the translation intent without violating its access/financial constraints.

## Client quotas and provider account quotas are independent

Two limits are checked before billable or capacity-consuming dispatch:

1. **Per-client allowance** (e.g. public API 20,000 source words per day, paid plan 1,000,000 words/month, or a maximum estimated cost). Counts against that client's effective plan and prevents a single client monopolizing resources.
2. **Provider-account allowance** (e.g. the organization's Google account includes 1,000,000 words/month with hard cap and optional administrator-authorized overage). Counts all clients who use the shared provider account.

Accounting units can differ; translate each attempt to the relevant client-plan and provider-account metering definitions. Reserve expected allowance **atomically** for both dimensions before dispatch; if either hard limit fails, do not start the attempt. Reconcile authoritative provider usage and recorded client usage after completion/ambiguous timeout; do not use SystemMonitor as a billing ledger. A client cannot gain provider budget by changing its request-supplied ID.

A no-charge local backend may still have finite CPU/RAM/concurrency. Capacity and rate limits remain relevant even without a monetary provider charge. Protect internal high-priority work via configurable reserved capacity or scheduler fairness; **do not automatically reinterpret P0-P4 priority as payment status**. Public spare-capacity work can be assigned a lower allowed priority, bounded queue, rate/concurrency limit or preconfigured maximum share, without impacting the module's independent operation.

## Operator control and privacy

A host administrator can configure/revise named profiles, register clients, grant provider families/endpoint accounts, restrict feature use, set client quotas, pause or disable public API exposure and see consumption by client/plan/provider. The system enforces configured policy mechanically. An administrative policy change may narrow **future dispatch** under current operational access rules even for an old accepted job, while preserving the request's historical effective preference snapshot and returning a clear explanation for changed feasibility.

Keep authentication, customer billing data and translation content out of publicly visible SystemMonitor events. Administrative reports may provide aggregate usage and rate-limit/permission diagnostics without revealing other clients' text, commercial terms or API credentials.

Public API request/reply contracts remain shared across REST, direct .NET and optional SignalR business transports; authorizing at the common command/application layer avoids mismatched rules between endpoints. Limiting abusive request volume should yield explicit backpressure/rate-limit responses, not silently block or de-register client identities automatically.

## Open implementation details

Finalize `ClientPolicy` schema/group precedence and provider allowlist semantics; authenticated versus anonymous client ID strategy; publicly offered spare-capacity scheduler limits; daily/monthly client quota resets/time zones; multi-host atomic reservations; partial charge/retry accounting; permission changes mid-job; privacy of client usage reporting; and admission codes. Test allowed local fallback, forbidden Google and AI, paid entitlement, provider hard budget exhausted for every client, per-client quota exhaustion even with unused provider capacity, input spoofing, rate limiting, and fairness without starving internal work.

See [Request Admission](../Request-Admission-and-Normalization/en.md), [Provider Routing](../Provider-Routing/en.md), [Provider Billing](../Provider-Billing-and-Quotas/en.md), [QueueMaster](../QueueMaster-and-Reliability/en.md), [Architecture](../Architecture/en.md), [Implementation Status](../../IMPLEMENTATION_STATUS.md).
