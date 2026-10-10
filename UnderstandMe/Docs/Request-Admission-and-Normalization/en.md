# Request Admission, Orchestrator Policy and Graceful Provider Substitution

Status: architecture rules agreed in discussion 2026-10-10; documentation only, no implementation yet. Names and DTOs are illustrative.

## Fundamental rule

**The client states what must be translated and may suggest how; the orchestrator determines how the request can be fulfilled under administrator-configured rules.** A request that names Google API or another provider is not permission to bypass the administrator's allowed-provider list, cost/usage caps, privacy/LocalOnly restrictions or suspended queues.

The orchestrator must make a **best effort to preserve the functional intent of a valid translation request**. A forbidden or unavailable preferred provider is a reason to **substitute an acceptable eligible provider**, not a reason to reject the whole translation while a suitable authorized alternative exists. Do not silently violate the real task's hard requirements (language pair, format/placeholder integrity, source restrictions, agreed deadline, authorized spend). Refuse or fail explicitly only when the effective constraints leave no feasible way to complete it.

## The human baseline and explicitly enabled providers

**Human is the only required translation provider capability**. A host with no installed LibreTranslate, cloud API or AI module can still accept a request for a human worker and return a manually supplied result. A human provider may lack immediate capacity, so admission must distinguish a permissible queued manual request from an immediate or timed-out request that cannot be fulfilled. `RealTime | Mandatory` cannot invent human availability or require a machine provider to exist.

An engine named in a request is only a preference **among globally activated and client-authorized providers**. Host configuration decides what to activate at all. The administrator may deliberately configure an opt-in public anonymous profile that accesses all activated providers, even paid/AI, or only free/manual providers; the core must not override that choice with hard-coded free-only public restrictions. The host's global hard budget and provider-account restrictions always apply. [Client Access Policies](../Client-Access-Policies/en.md).

## Caller-specific policy before provider selection

Resolve a **trusted ClientContext** and an optional **ClientPolicy** for each incoming translation command. The administrator can give one registered paying client access to configured paid Google/Microsoft/AI endpoints while another client can use only free/local engines. A host may optionally publish idle self-hosted capacity, human work or even administrator-authorized paid/AI engines through an opt-in public API with a **configurable** public/anonymous profile. The normal profile for known clients can initially allow **all globally enabled providers**, but permissions are intersected with global hard rules and provider-account budgets. Never use the publicly self-reported ClientId from a request body as proof of paid entitlement.

Effective eligible provider list = configured/system-allowed providers **intersected** with those allowed by the client plan, compatible capabilities and current account budgets. The client's requested vendor is still only a preference. If Google is allowed globally but forbidden by this client's policy, use a compatible eligible free/local endpoint when possible and report a client-specific adjustment instead of rejecting the translation. If **no** permitted capable endpoint remains, return a structured reason. `Mandatory` and individual flags cannot grant forbidden paid access.

Client usage quotas are distinct from the organization's provider billing quotas. A public client can exhaust its own daily word quota even while the provider account has unused capacity, and vice versa. At dispatch enforce **both** current client resource/plan limits and provider-account quota atomically; if an administrator changes client entitlement, it affects future attempts even for previously accepted jobs (historical request preference remains unchanged). Public spare-capacity limits, rate/concurrency controls and optional reserved internal capacity prevent public callers from monopolizing the engine.

[Client Access Policies](../Client-Access-Policies/en.md) defines these independent constraints and optional public API profiles.

## Admission and normalized effective request

1. Receive and preserve the **OriginalRequest** (source content/reference, target language, requested provider and preferences, priority, flags, formatting/validation needs).
2. Validate that the request is well formed and the caller is authorized for this operation; resolve Translation Target and system/admin policies.
3. Produce an **EffectiveRequest / EffectiveSettings snapshot**. Apply administrator rules **before** accepting work into QueueMaster: allowed/forbidden endpoints and accounts, policy-ranked tiers/language pair overrides, queue priority limits, `Mandatory` and fallback flags, privacy, budgeting and format constraints. Preserve the original request and maintain a typed explanation of every meaningful normalization/substitution. Configuration precedence is system/administrator hard constraints -> permitted target/pipe settings -> allowed request preferences; explicit endpoint choice from a caller is a **preference**, not an unconditional command.
4. Select or rank permitted eligible candidates based on request requirements. An explicit caller request for Google API, for example, can resolve to LibreTranslate if Google is prohibited or its quota cannot support another billable attempt. Choosing a provider is deferred until dispatch where readiness/quota can be checked with fresh operational state.
5. If the request has a feasible compatible route (including a permitted wait/queue path) accept it and return an admission receipt with the effective constraints and policy adjustment notices; acceptance is **not** a claim that translation has already completed. Dispatch, translate, validate and return the real engine used and outcomes when known.
6. If no authorized suitable provider can fulfill the work, return **Rejected** at admission (or a later **Failed/Unavailable** result if the situation arose after acceptance), with a precise machine-readable reason and human-readable explanation. Never falsely claim completion or fall back to a disallowed engine.

Do not reject a structurally valid request just because it named a forbidden vendor. The orchestrator may also change an illegitimate requested priority or disregard a forbidden behavior flag under the governing settings, with an explanation, provided the semantic translation intent can still be met. Mandatory means use any available *eligible* engine rather than wait for a preferred busy one; it never overrides account limits or makes an engine capable of an unsupported language/format.

## Normalized response contract

Retain separate concepts for **admission outcome** and **translation outcome**:

- `Accepted` / `Deferred`: the orchestrator accepted the effective work (possibly waiting because a queue was administratively paused or an eligible provider is temporarily unavailable according to the request policy). Return a `RequestId`, effective priority/flags, any `PolicyAdjustment` codes and the governing limits. Do not claim an endpoint was used yet.
- `Completed`: return the verified output, `UsedProviderId`, `UsedEndpointId`, optional billable units/estimated-versus-actual charges, and any provider-substitution reasons.
- `Rejected`: a request cannot be admitted, e.g. no allowed capable provider under hard constraints, invalid request or relevant policy rejection. Include `ReasonCode` such as `NoEligibleProvider`, `UnsupportedLanguagePair`, `PaidQuotaExhausted` or `ProviderForbidden`, and clarify whether an alternative was attempted/evaluated.
- `Failed` / `Unavailable`: previously accepted work cannot be completed; return a meaningful reason, without losing the original request identity or hiding the attempted alternatives.

These categories are conceptual; finalize them using existing Essentials/WhenItFails Result/Failure conventions rather than inventing a second incompatible error framework.

Illustrative finished response:

```json
{
  "requestId": "req-123",
  "status": "Completed",
  "requestedProvider": "Google",
  "usedProvider": "LibreTranslate",
  "adjustments": ["RequestedProviderNotAllowed", "AllowedAlternativeSelected"],
  "estimatedCost": 0
}
```

The 0 cost here represents only **no chargeable API cost assumed for a locally self-hosted endpoint** in the illustration; a local server still has real infrastructure/operating costs and billing estimates must use the configured plan. A routing explanation need not reveal private administrator settings, credentials or commercial rates to unauthorized clients.

## Current operational gates have precedence

The accepted request's ranking/preferences are snapshotted at creation. However, the orchestrator continuously enforces **current operational controls at dispatch**:

- An administrator's `Pause` or `Stop` of a queue holds dispatch without rewriting queued work.
- A provider account's actual remaining quota/reservations and current **hard spending limit** govern whether any *new* chargeable provider call may start, even for previously accepted requests.
- If the preferred endpoint is disabled, not ready, exhausted or unsupported, choose an eligible permitted alternative according to the original effective routing preferences and allowed fallback rules, or wait/return a clear unavailable reason according to the request policy.
- Attempts and candidate transitions must have limits (retry count, deadline, budget); the orchestrator does not silently pay extra or purchase new quota to achieve success.

Provider unavailability by itself does not justify permanent administrative changes to that provider's configured priority. Only authorized administrator actions mutate global settings.

## Client visibility and observability

The client may know **that** the orchestrator substituted or adjusted the request and **why**, with typed reasons. A monitoring/admin view can show richer diagnostic eligibility decisions, cost estimates, retry attempts and quota notices, without exposing credentials or private source text. Administrative `/SystemMonitor` broadcasts remain optional/best-effort; admission receipts and authoritative job results are not dependent on SignalR monitoring.

If multiple candidates are selected over a long-running request, record which provider actually processed each segment or attempt rather than inventing one provider identity for the entire job. Multiple segment translations may legitimately use different eligible backends.

## Before implementation

Contract tests: caller insists on forbidden Google but allowed Libre succeeds; named Google fails quota and falls back to an allowed free provider; exhausted hard-limit paid account cannot be bypassed by `Mandatory` or explicit provider choice; valid request rejected only if no compatible authorized route exists; paused queue returns Deferred rather than misreporting an unsupported provider; original versus effective settings preserved; the response reports actual provider used and adjustment reasons; dynamic billing caps apply before new dispatch; allowed language/format/provider restrictions are maintained; error mapping reuses Essentials and WhenItFails.

See [Request Settings](../Request-Settings-and-Live-Translation/en.md), [Provider Routing](../Provider-Routing/en.md), [Provider Billing](../Provider-Billing-and-Quotas/en.md), [QueueMaster](../QueueMaster-and-Reliability/en.md), [Architecture](../Architecture/en.md), [Implementation Status](../../IMPLEMENTATION_STATUS.md).
