# Request Settings, Live Translation and Segment Updates

Status: architecture/design direction agreed on 2026-10-10. Not implemented; public API, option names and wire formats are illustrative.

## Settings as the policy boundary

Every request resolves an **effective immutable settings snapshot at creation/acceptance**. A Settings object (possibly composed from multiple smaller option groups) must be capable of expressing the desired behavior without requiring the request caller to know the internals of QueueMaster or individual translators.

The effective snapshot combines system/host defaults, administrator-configured Pipe and Translation Target policy, and explicit request flags/options **only where overrides are allowed**. Provider ranking, language-specific ranking, eligible engine lists, cost ceiling, urgency/deadline, cache/memory policy, split format, fallback triggers, output validation and notification strategy can all be configured. More specific preferences cannot override hard system privacy, spend or permission restrictions. Avoid an unmaintainable monolithic settings class: one configuration facade may contain typed groups such as `Scheduling`, `ProviderSelection`, `Caching`, `Validation` and `LiveDelivery`.

**Do not conflate** `QueuePriority` (one exclusive P0-P4 value) with independent request behavior flags (bitmask) or endpoint/provider ranking. Conceptual flags may include `Mandatory`, `AllowFallback`, `UseSystemPersistent`, `StoreSystemPersistent`, `ValidatePlaceholders` and `NotifyOnCompletion`. Exact names, allowed combinations and validation rules remain open. Bitmask flags represent independent opt-in capabilities; mutually exclusive states (e.g. priority level or wait strategy) should remain enums/options rather than conflicting flags.

## Orchestrator admission and client preferences

A request's explicit requested provider (e.g. Google API) is a **preference**, not authority to call that endpoint. The orchestrator first validates the request and normalizes the effective Settings under higher-priority administrator/host policies. It preserves the original, records changed provider/priority/flag choices and informs the caller about policy adjustments in the admission receipt or final result.

A valid request should **not lose its translation function merely because the named provider is disallowed or its billing quota is exhausted**: search for an eligible, compatible, policy-permitted alternate provider and continue when possible. If none can meet the hard requirements (language, format, permitted account spending, privacy, etc.), return a typed rejection or subsequent unavailable/failure reason. A caller's `Mandatory` flag cannot bypass administrator-approved limits. Active orchestration pauses and current hard billable limits still govern dispatch for old as well as new effective-request snapshots.

Keep `RequestedProvider` and `UsedProvider` distinct; a provider can only be reported as **actually used after an attempt has run**, never merely after QueueMaster accepted a job. See [Request Admission and Normalization](../Request-Admission-and-Normalization/en.md).

## RealTime behavior

- **RealTime + Mandatory:** do not wait for the **preferred** occupied provider. Select the first currently available, compatible and acceptable endpoint in the configured effective provider tier/order, considering source/target language, format/protected placeholders, configured quality/latency/cost limits and allowed escalation. Mandatory means the system should *try available alternatives now*, **not** promise success, bypass budget/LocalOnly constraints, or endlessly wait. If no acceptable endpoint exists, report a defined unavailable/fallback outcome rather than violating hard rules.
- **RealTime without Mandatory:** the calling page/service may return the original untranslated text immediately, while an eligible background continuation produces and emits verified segment translations when ready. No automatic paid escalation is implied; backend work follows the request's configured provider preference, wait/timeout and lifetime.
- Other queue priorities (P1-P4) can independently choose whether to wait, fail or fall back within their own deadline and cost policy; `Mandatory` is not itself a substitute for QueueMaster priority.

If a user leaves the page, whether the continuation is canceled, kept briefly for reuse or persisted depends on the original request policy. Existing default P0 real-time operations remain **volatile** unless explicitly designed as a different durable job; sending an asynchronous browser update is not proof of durable work.

## Delayed human continuation is durable work

A Human provider may have **no connected translator** when a request arrives. When the effective request asks for eventual human fulfillment, the orchestrator commits a durable human work item so it can wait in the normal QueueMaster priority queues until someone opens the translator UI. The response can indicate accepted/pending rather than pretending a human result is already available.

The ordinary `P0 RealTime` behavior remains volatile and can immediately return the original untranslated content. If a delayed manual result is desired in addition, accept a **separate durable human follow-up** correlated to the original request/segments; it is not a hidden decision to preserve every transient P0 request. The translator UI, persistence and claim state are independent of how the final user-facing content is rendered. See [Human Translation Workflow](../Human-Translation-Workflow/en.md).

## Splitter and UI-agnostic incremental delivery

The **contract is not a JavaScript or HTML API**. The splitter produces an ordered list `(SegmentId, SourceText)` with `SegmentId = 0..N`, plus enough original format and position metadata to reconstruct the content. Split on semantic/paragraph boundaries where appropriate; do not break Markdown links, HTML structure, fenced code, placeholders or other protected constructs. Different text/document types may use different format-aware splitters.

Each translation request has a stable `RequestId` to separate identical segment IDs in different documents. A completion is conceptually `TranslationSegmentCompleted(RequestId, SegmentId, TargetCulture, VerifiedContent, Revision)`. Consumers map the returned identity to their existing display/content model, ignore unrelated translations, and apply only the newest relevant completion regardless of arrival order. No consumer must use a DOM, `span` elements, JavaScript or a particular rendering framework.

**Examples of adapters, not requirements:**

- Blazor updates component state for the matching segment and lets the component rerender.
- A browser JavaScript client updates a matching DOM node (e.g. `data-request-id` and `data-segment-id`), using `textContent` for plain text.
- An Avalonia app updates the corresponding view-model item; other front ends use their native equivalents.
- For formatted Markdown/HTML documents, the adapter restores structure through format-aware parsing/rendering and validated/sanitized markup rather than assigning arbitrary translated text directly to `innerHTML`.

The host chooses its own result/update transport: component callbacks, direct .NET calls, SignalR, SSE, polling or another interface. The **administrative `/SystemMonitor` is not the delivery mechanism for translated user content**. When an update is missed, the client may fetch currently available verified results if the request's caching/durability rules allow it. Volatile P0 work may expire. The fundamental rule is *identify the segment, translate it independently, notify completion, update only that segment*.

## Optional SystemPersistent translation memory

Treat `SystemPersistent` as an optional **bounded permanent/reusable translation memory**, not an unbounded history or indiscriminate log of every translation. A request can opt to **read** this layer before provider routing; permission to **write/promote** into it is a separate decision. Only configured validated/approved translations should be promoted automatically or manually. Dictionary-authoritative translations take precedence; then a permitted context-matched SystemPersistent memory match; then provisional cache; then a live provider request when necessary.

Stable memory keys include at least source text/key hash, source/target culture, Translation Target/project/domain, context/terminology/format constraints and relevant version. Do not reuse a translation between incompatible documents, glossary revisions or protected-placeholder structures. Support explicit pinning/approval, per-target quotas/retention, listing/removal, and safe invalidation on a source or approved-translation change. Ordinary provisional cache remains short-lived and separate.

## Settings changes and orchestrator precedence

Routing/validation/cost policies are captured from **effective Settings when a new logical request is created**, not continuously recomputed for items that are already queued or in flight. An administrator's new provider order applies to **new requests**; old items retain their original routing preferences. An already-running provider call is allowed to complete under its request settings unless orchestration explicitly stops/cancels it.

**The orchestrator has operational precedence.** An administrator can pause/stop a queue; its waiting items do not start until resumed, regardless of their Request priority or `Mandatory` flag. Pause is not silent reinterpretation of the requests' provider policies. A serious global disable/credential revocation may prevent future dispatch for operational/safety reasons; if it conflicts with a request's snapshot, record a clear failure/paused outcome rather than quietly changing its historical policy.

Preserve the effective settings version/hash with durable job metadata so that delayed work or restart/resume remains reproducible. For volatile work keep only the scoped snapshot needed until its lifetime ends.

## Paid provider usage settings

The request's effective `Settings` can declare whether paid providers are eligible, a spending cap and fallback behavior when a subscription account has exhausted its included monthly units. The provider declares metering/pricing capabilities, while each configured billing account declares its own package, quota and overage policy. **Hard/authorized spend limits are checked afresh at chargeable dispatch**, even when provider-order preferences were frozen at request creation. This is an orchestrator/account-level operational constraint, not rewriting an accepted request's priority. `Mandatory` cannot exceed a hard cap or purchase another package. [Provider Billing and Quotas](../Provider-Billing-and-Quotas/en.md).

## Provider language-specific ordering and capability checks

Default provider tiers: **self-hosted/free translation (LibreTranslate or a comparable endpoint) → paid translation APIs (same preference tier by default) → AI-based translation/review**. This is a configurable starting policy, not hard-coded vendor logic; an administrator may prioritize a paid API first or change preferences **per language pair**. Within equal tier prefer a deterministic eligible/ready selection rule; no automatic permanent quality downgrading is inferred from a single past poor translation.

Providers report model ID/version and supported language pair/format capabilities where available. Run small **version-aware capability/placeholder tests** at controlled moments (initial setup, manual check, changed model version) and do not silently trust old test observations after a model changes. The observed model version number is identity/context metadata, **not** a cross-language quality score: model 1.96 for one language does not imply higher translation quality than model 1.0 for another. Admin-configured per-language priorities take precedence over speculative automated quality ranking.

The provider-routing dry-run simulator is useful but **deferred** until the core Settings and selection contracts are stable. It is an admin convenience, not required for the first working implementation.

## Verification before implementation

Tests should cover RealTime + Mandatory selecting an available acceptable backend without waiting; RealTime returning original text and applying out-of-order segment results; missing/lost updates; plain text safely inserted into the DOM; Markdown paragraph boundaries and placeholder integrity; SystemPersistent permission and context isolation; request settings frozen across priority changes; global queue pause overriding dispatch; same-tier provider selection; model-version changes invalidating previous capability observations.

See [Provider Routing](../Provider-Routing/en.md), [QueueMaster](../QueueMaster-and-Reliability/en.md), [Localization](../Localization/en.md), [Translation Chains](../Translation-Chains/en.md), [Implementation Status](../../IMPLEMENTATION_STATUS.md).
