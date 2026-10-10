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

## Privacy is independent from retention and queue priority

`TranslationRequest.Privacy` (conceptual property name; type TBD) expresses confidentiality and **who can view the source and translated results**. It is distinct from `Persist`, `CachePolicy`, `LifeTime`, `SystemPersistent` and P0-P4 `QueuePriority`. An administrator/client/target policy may **tighten**, but not silently weaken, a declared Private scope. A request marked **Private** must not be routed automatically to Human or exposed for manual audit, even if it is not P0. A distinct non-P0 manual translation service is possible **only with explicit authorization from the content owner and access limited to the assigned worker**; the original private request never silently changes purpose. Automated validation and permitted automatic providers remain eligible, according to the effective privacy policy.

**Private does not mean ephemeral.** A private chat/forum/history translation may be retained as an **access-restricted translation result** and returned to the same authorized audience on subsequent history views. This avoids repeatedly translating unchanged content. `P0 RealTime` job state stays volatile; storing its verified result in a separate privacy-scoped translation cache does **not** make the P0 job persistent or send it to the Human backlog. The scope/reader authorization, owner-specific source identity/revision, language and retention must be checked on every cache lookup; do not leak a private result into global dictionaries or shared SystemPersistent memory.

Manual review is prohibited for Private source/result contents; SystemMonitor and ordinary admin UI may see only safe content-free operational data. Privacy policy also governs which outside machine/AI endpoints may receive that text; it does not automatically force local-only unless the controlling policy says so. [Privacy and Scoped Storage](../Translation-Privacy-and-Scoped-Storage/en.md).

## RealTime behavior — automatic only, no human access

`P0 RealTime` is **exclusively for automated, immediate output**. Human is **never an eligible worker** for P0, even when `Mandatory` is set or automatic candidates fail. A host with **only the Human provider** does not activate P0 translation execution: an incoming RealTime request returns **the unchanged original text virtually immediately**, without enqueueing, awaiting, or exposing it to a human.

- **RealTime + Mandatory:** select the first currently available, compatible and acceptable **automatic** endpoint according to the effective provider policy, avoiding waits for a preferred busy engine. If no automatic endpoint is eligible or timely, return the original text (with an appropriate untranslated/unavailable status/reason) rather than violate hard limits or route to Human.
- **RealTime without Mandatory:** the page/service can display original text at once and receive verified **automatic** translated segments later while a short-lived RealTime request remains active. This never implies a Human fallback or persistent manual task.
- **Privacy:** RealTime can carry **private chat messages**. Do not persist or expose source text to Human queues, manual editors or reviewers automatically. `Mandatory` does not imply authorization for manual access. Any logging/cache is subject to separately approved data-handling settings and must not silently retain sensitive chat text.
- Other P1-P4 priorities can queue explicitly authorized Human requests with durable storage and human-access policy, distinct from RealTime. The user's deliberate non-P0 manual submission is the only way to create such human work.

## Separate manual translation workflow

A human may be offline while the system collects **explicitly submitted and authorized non-P0 manual requests**. Those requests are durably accepted under P1-P4 and are later claimed through QueueMaster by a human who opens a translation UI. **Never auto-convert a P0 request into a durable human continuation.** If the user independently requests manual treatment of text they are authorized to disclose, it is a **new manual request**, not a RealTime fallback or an implied `Persist` flag. See [Human Translation Workflow](../Human-Translation-Workflow/en.md).

## Splitter and UI-agnostic incremental delivery

The **contract is not a JavaScript or HTML API**. The splitter produces an ordered list `(SegmentId, SourceText)` with `SegmentId = 0..N`, plus enough original format and position metadata to reconstruct the content. Split on semantic/paragraph boundaries where appropriate; do not break Markdown links, HTML structure, fenced code, placeholders or other protected constructs. Different text/document types may use different format-aware splitters.

Each translation request has a stable `RequestId` to separate identical segment IDs in different documents. A completion is conceptually `TranslationSegmentCompleted(RequestId, SegmentId, TargetCulture, VerifiedContent, Revision)`. Consumers map the returned identity to their existing display/content model, ignore unrelated translations, and apply only the newest relevant completion regardless of arrival order. No consumer must use a DOM, `span` elements, JavaScript or a particular rendering framework.

**Examples of adapters, not requirements:**

- Blazor updates component state for the matching segment and lets the component rerender.
- A browser JavaScript client updates a matching DOM node (e.g. `data-request-id` and `data-segment-id`), using `textContent` for plain text.
- An Avalonia app updates the corresponding view-model item; other front ends use their native equivalents.
- For formatted Markdown/HTML documents, the adapter restores structure through format-aware parsing/rendering and validated/sanitized markup rather than assigning arbitrary translated text directly to `innerHTML`.

The host chooses its own result/update transport: component callbacks, direct .NET calls, SignalR, SSE, polling or another interface. The **administrative `/SystemMonitor` is not the delivery mechanism for translated user content**. When an update is missed, the client may fetch currently available verified results if the request's caching/durability rules allow it. Volatile P0 work may expire. The fundamental rule is *identify the segment, translate it independently, notify completion, update only that segment*.

## Chat-specific transient correlation and recipient corrections

For chat/forum/user-generated text, `RequestId` and splitter-generated `SegmentId = 0..N` exist only **during the translation request** to match translated chunks. Unlike localization dictionary `KEY=VALUE` entries, **chunk IDs are never durable keys**. UnderstandMe returns the segments and metadata; the **consuming chat application** reassembles the entire translated message and independently manages its message history and retention.

An authorized recipient may edit their own displayed translation after it is returned. This is **personal review/self-correction**, not a Human translator audit or a P0 fallback. The recipient can decide to apply the correction without caching, or explicitly opt in to a **recipient-scoped cached correction**. Storing the chat application's *initial translated message* under its own history policy is a different choice from storing the recipient's *edited override*. Neither turns the volatile P0 job persistent nor sends it to Human work queues. Sharing an edit more broadly requires its own explicit authorization; 'cache' does not itself mean 'publish'. [Chat Translation Flow](../Chat-Translation-Flow/en.md).

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
