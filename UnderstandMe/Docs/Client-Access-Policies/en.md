# Client Access Policies and Optional Public Translation API

Status: design agreement, 2026-10-10; not implemented. This document complements, rather than duplicates, the general orchestrator/provider/billing architecture.

## Purpose and default behavior

An installation may run Afutrans as an internal translation service **and optionally publish spare translation capacity as a public API**. Different callers must be able to use different provider tiers and resources without creating vendor-specific logic inside QueueMaster or translation workers.

**Default for ordinary, known clients**: no additional client-specific provider restriction — all configured provider families are eligible **subject to global administrator restrictions, provider capabilities and account hard limits**. A client policy can narrow those permissions. Public/anonymous access is a separately **opt-in exposed endpoint and configurable policy**. The operator can restrict it to local/free translation or explicitly permit paid/AI use, subject to global ceilings; known-client defaults do not silently turn public access on.

A public deployment is optional and is a host exposure decision, not a core library requirement. By default, merely installing the UnderstandMe library must not expose an external HTTP listener.

## Nothing but human translation is mandatory

**Human translation is the one baseline provider capability**: UnderstandMe must be able to accept, queue/assign and receive manually supplied translations without an external translation engine. This does **not** imply that a human is online, that a human must approve every automatic result, or that a synchronous RealTime call can return a human translation immediately. Human worker availability and the request's allowed wait/manual workflow determine the outcome.

LibreTranslate, AfroLT, Google, Microsoft, AI and every other automatic backend are **optional independently enabled integrations**. A human-only installation is a valid deployment, not an incomplete installation. Merely publishing a preferred free -> paid -> AI ranking does **not** enable, install or purchase any provider.

`ClientPolicy` grants permission only among the installed, **activated**, globally permitted and currently eligible providers. The default known-client policy does not *add* permissions or activate paid services; it merely adds no further per-client restriction.

**Public exposure and public entitlement are separate operator choices.** Public API routes are opt-in. When the host operator enables public service, they can explicitly select a free-only, paid-inclusive, unrestricted, human-only or custom public profile. A free/local restricted public profile is a recommended example, **not a hard-coded limit**: a philanthropist may deliberately allow anonymous callers access to every activated translator, including paid APIs, within administrator-authorized global spending rules. Conversely, an operator can disable every automatic engine, paid provider or AI integration even for themselves. No per-client policy can bypass a global hard cap.

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
| PublicAnonymous | Configurable on explicit activation: free/local, all globally enabled engines including paid, human-only or custom | Administrator-defined quota, rate, concurrency and priority rules |
| PaidClient | Provider accounts/tiers explicitly included in purchased entitlement, potentially Google/Microsoft/AI | Client's plan, word/token quota or cost ceiling |
| CustomClient | Configured per-client allow/deny choices and limits | Overrides permitted by administrator |

Provider permission can be expressed by capability/tier and by explicit endpoint/account IDs when needed. **Paid** describes the configured account's charging regime, not a permanent vendor label: a provider family may have self-hosted and cloud endpoints with different costs and permitted usage.

Client policies need explicit enable/disable status, version/effective time and straightforward admin review, but do **not** require an elaborate central identity platform. When an externally exposed service grants **client-specific paid entitlements or tracks individually paid usage**, the boundary must reliably identify that client through appropriate API/application authentication (e.g. an API key/session); the **lite GUID-only SystemMonitor identification is not appropriate for such authorizations**. However, the operator may deliberately fund **anonymous public paid translations** under a separate shared public profile: no individual paid entitlement is claimed, and all requests consume a **common administrator-authorized public spending/quota pool**. In that case individual-client metering cannot be promised without client identity. A shared hard budget and resource admission rules remain effective.

## Request Privacy is distinct from client provider entitlement

A caller's `Privacy = Private` request carries **content-access restrictions separate from the client's quota and entitlement**. A client entitled to Google does not automatically authorize sending private chats to Google; a target/owner privacy policy separately defines which remote machine processors may receive private content. Private requests are **not eligible for automatic Human routing or manual audit**, even if the client is entitled to use Human for other jobs. A separately approved manual translation request is a different operation with purpose-specific Human access.

Private translated results can be saved for authorized history reuse without making them visible to general administrators, other clients or a global shared dictionary. Translation result storage and reads must use trusted tenant/conversation/message rights, not merely the supplied ClientId or a cached result ID. A provider's per-client budget ledger may retain content-free usage, but no private chat text belongs in a public usage dashboard. See [Privacy and Scoped Storage](../Translation-Privacy-and-Scoped-Storage/en.md).

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

Finalize `ClientPolicy` schema/group precedence and provider allowlist semantics; authenticated versus anonymous client ID strategy and explicit opt-in approval of paid access for anonymous callers; publicly offered spare-capacity scheduler limits; daily/monthly client quota resets/time zones; multi-host atomic reservations; partial charge/retry accounting; permission changes mid-job; privacy of client usage reporting; and admission codes. Test allowed local fallback, forbidden Google and AI, paid entitlement, provider hard budget exhausted for every client, per-client quota exhaustion even with unused provider capacity, input spoofing, rate limiting, and fairness without starving internal work.

See [Request Admission](../Request-Admission-and-Normalization/en.md), [Provider Routing](../Provider-Routing/en.md), [Provider Billing](../Provider-Billing-and-Quotas/en.md), [QueueMaster](../QueueMaster-and-Reliability/en.md), [Architecture](../Architecture/en.md), [Implementation Status](../../IMPLEMENTATION_STATUS.md).

## Id application registry and Afutrans client-policy boundary (2026-10-20)

An optional Afrowave Id registration integration may supply authenticated **caller application identity**, public service discovery and an application-to-service grant/entitlement for Afutrans. That Id grant is an input to, **not a replacement for**, Afutrans `ClientContext` and effective `ClientPolicy`; the service still enforces its own accepted feature/provider restrictions, queue/resource caps and per-target read/write/privacy rules. A listed/public service is not an unrestricted data service. An explicitly enabled anonymous translation profile remains possible under its own capped administrator-controlled policy; it is not automatically inherited from a registry listing.

Clients can be deployed without Id; use appropriately authenticated local/standalone policies or approved anonymous profiles. Trust must be established by an authenticated request boundary (signed/audience-bound credentials, securely managed API keys or equivalent), **not** by self-declared GUIDs. REST and business SignalR request handling must use one policy decision and cannot leak target credentials. No mandatory Id SDK is introduced into the core UnderstandMe module.

See [Afutrans Registered Service Integration](https://github.com/afrowaveltd/Afutrans/blob/main/Docs/Registered-Service-Integration/en.md).

## Hosted Afutrans behind optional intermediary application (2026-10-20)

The user proposes a **reusable intermediary / service-access gateway** in front of hosted Afutrans: outside users/applications send translation or management requests to the intermediary, which optionally resolves first-party Id identity and registered application/service grants, decides admission and routes accepted requests to an otherwise non-public Afutrans service. This can be applied to other Afrowave services without embedding network/Id requirements in UnderstandMe.

- **Open mode:** explicit operator choice to accept anonymous requests for specifically published translation capabilities under a constrained `PublicAnonymous` ClientPolicy (quotas, rate, size, provider access, privacy and global budgets). No automatic right to view tenant dictionaries, add new storage, edit a database connector or administer clients.
- **Authorized mode:** Id-issued or local authenticated identity and operation-/target-scoped grants for create/update Translation Target, configure FTP/SQL connectors or secret references, read/write private catalogs, manage projects/providers and account functions. Public translation can coexist with protected administration on the same installation.
- **Double check the security boundary:** the edge is primary identity/admission gate, but the Afutrans application handler still authenticates the service-to-service caller context and rechecks effective `ClientPolicy`, target ownership, read/write capabilities, quota and sensitive action permissions. Enforce the same invariant on direct .NET, REST and SignalR internal entry points. Do not trust arbitrary forwarded identity headers, self-declared `ClientId` or that an HTTP request was routed through an expected hostname.
- Afutrans itself can have no public listener; use private network/mTLS or equivalent backend authentication. Id provides identity and grants, not dictionary/SQL/FTP secrets. **Id is optional** in standalone/offline Afutrans deployments and UnderstandMe has no compulsory network/gateway dependency.

The intermediary's actual product name, topology, internal token/envelope format, credential and revocation TTL, Gateway vs BFF or service routing, quota enforcement ownership and transport-specific contract are **deliberately undecided**. Further design deferred at the owner's request.

Reference: [Afutrans Registered Service Integration](https://github.com/afrowaveltd/Afutrans/blob/main/Docs/Registered-Service-Integration/en.md). **Design-only note, not implemented behavior.**

## Community-free deferred work and contributor capacity entitlement (2026-10-20)

**Author's preferred hosted Afutrans product model:** offer free **explicitly deferred/non-realtime** community translation within transparent finite rate/queue/word/capacity constraints. Optional contributions/subscriptions fund new VPS, self-hosted LibreTranslate workers and faster or dedicated capacity. This is an operator-configured `ClientPolicy`/capacity-entitlement feature, **not a paid prerequisite for the reusable UnderstandMe library or ordinary offline localization**.

- Anonymous `CommunityFree` may submit only those published translation operations the operator allows; it cannot create SQL/FTP storage targets, read private dictionaries or request arbitrary provider accounts. A privileged target command always requires independently checked scope even with an API Gateway.
- A verified `TenantDedicated` client may be admitted to a **separate lane and independently funded/owned worker pool**, with optional P0 eligible to that owner's compatible automatic backend; it cannot buy earlier placement on CommunityFree resources, spoof an endpoint/lane from request fields, or bypass privacy, per-target permission or provider hard budgets.
- Public free jobs with deferred results must be **durably accepted as explicit non-P0 requests** and return `Accepted`/job/status rather than promising an immediate answer. P0 RealTime is still volatile and automatic-only; there is no implicit P0 → deferred/Human escalation.
- Protect internal critical workloads but avoid starving admitted free work indefinitely. Use configurable minimum capacity share, admitted-work limits, dispatch fairness or effective-priority bounds; do not equate payment status with a QueueMaster enum or introduce per-client global physical queues by default. Concrete fairness mechanism remains to be designed/tested.
- Separate (1) client authorization/entitlement, (2) quota/capacity reservations, (3) service-contribution/subscription revenue, (4) backend Google/Microsoft spend and (5) self-hosted VPS operating costs. `ClientPolicy` enforces entitlements; the provider `UsageLedger` cannot prove a payment was received.

See [Afutrans Community Capacity and Contributor Priority](https://github.com/afrowaveltd/Afutrans/blob/main/Docs/Community-Capacity-and-Contributor-Priority/en.md). **Design only; no payment service, priority policy implementation or guarantees frozen.**

## Correction: no paid priority in CommunityFree worker pool (2026-10-20)

Commercial subscriptions buy **independently provided worker capacity / tenant-specific queue lanes**, not earlier positions in community-shared worker queues. The preceding contributor priority examples are historical; where they imply preferential use of community resources they are **superseded**. An already-owned compatible provider endpoint/AI subscription can be registered for the customer's tenant without any required Afrowave VPS fee. AfrowaveInternal P0 stays reserved; free requests use bounded explicitly deferred non-P0 CommunityFree capacity.

`ClientContext` / `ClientPolicy` verifies lane membership, tenant/target access and allowed provider accounts; request fields never self-authorize `LaneId`, P0 or provider choice. Finite shared API, QueueMaster and persistence resources remain protected, even for tenants paying their own vendor. Details: [Queue lanes and Tenant-Owned Providers](../Queue-Lanes-and-Tenant-Owned-Providers/en.md).

## No monthly community credit cliff; authorized endpoint loan (2026-10-20)

**Operator preference correction:** a free CommunityFree user should normally **wait longer** when capacity is busy, rather than be denied additional translations for exhausting an arbitrary monthly or lifetime word/request allowance. Earlier examples of `20,000 words/day`/fixed free client quotas are possible optional abuse/owner configurations, **not** the intended default. Enforce finite queue/storage, concurrency and abuse protections with adaptive `Busy/RetryLater` and fair admission, plus real provider account hard spending limits; promise no unbounded accepted backlog.

Tenant-owned endpoints may be shared to selected users or to CommunityFree through **explicit revocable provider-owner capacity offers**, but only if the submitted text's application/target owner also permits this remote provider/AI processor. Afrowave may donate idle reserved-capacity surplus to community as non-guaranteed slots without enabling paid queue-jumping or granting free users privileged P0 ownership. Customer's own realtime plus other authorized borrowed engines is a supported composite policy.

See [Capacity Lending and Backpressure](../Capacity-Lending-and-Backpressure/en.md). **Design only.**

