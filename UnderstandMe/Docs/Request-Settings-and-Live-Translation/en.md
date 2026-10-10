# Request Settings, Live Translation and Segment Updates

Status: architecture/design direction agreed on 2026-10-10. Not implemented; public API, option names and wire formats are illustrative.

## Settings as the policy boundary

Every request resolves an **effective immutable settings snapshot at creation/acceptance**. A Settings object (possibly composed from multiple smaller option groups) must be capable of expressing the desired behavior without requiring the request caller to know the internals of QueueMaster or individual translators.

The effective snapshot combines system/host defaults, administrator-configured Pipe and Translation Target policy, and explicit request flags/options **only where overrides are allowed**. Provider ranking, language-specific ranking, eligible engine lists, cost ceiling, urgency/deadline, cache/memory policy, split format, fallback triggers, output validation and notification strategy can all be configured. More specific preferences cannot override hard system privacy, spend or permission restrictions. Avoid an unmaintainable monolithic settings class: one configuration facade may contain typed groups such as `Scheduling`, `ProviderSelection`, `Caching`, `Validation` and `LiveDelivery`.

**Do not conflate** `QueuePriority` (one exclusive P0-P4 value) with independent request behavior flags (bitmask) or endpoint/provider ranking. Conceptual flags may include `Mandatory`, `AllowFallback`, `UseSystemPersistent`, `StoreSystemPersistent`, `ValidatePlaceholders` and `NotifyOnCompletion`. Exact names, allowed combinations and validation rules remain open. Bitmask flags represent independent opt-in capabilities; mutually exclusive states (e.g. priority level or wait strategy) should remain enums/options rather than conflicting flags.

## RealTime behavior

- **RealTime + Mandatory:** do not wait for the **preferred** occupied provider. Select the first currently available, compatible and acceptable endpoint in the configured effective provider tier/order, considering source/target language, format/protected placeholders, configured quality/latency/cost limits and allowed escalation. Mandatory means the system should *try available alternatives now*, **not** promise success, bypass budget/LocalOnly constraints, or endlessly wait. If no acceptable endpoint exists, report a defined unavailable/fallback outcome rather than violating hard rules.
- **RealTime without Mandatory:** the calling page/service may return the original untranslated text immediately, while an eligible background continuation produces and emits verified segment translations when ready. No automatic paid escalation is implied; backend work follows the request's configured provider preference, wait/timeout and lifetime.
- Other queue priorities (P1-P4) can independently choose whether to wait, fail or fall back within their own deadline and cost policy; `Mandatory` is not itself a substitute for QueueMaster priority.

If a user leaves the page, whether the continuation is canceled, kept briefly for reuse or persisted depends on the original request policy. Existing default P0 real-time operations remain **volatile** unless explicitly designed as a different durable job; sending an asynchronous browser update is not proof of durable work.

## Splitter and partial browser replacement

A format-aware splitter outputs an ordered list of stable pairs `(SegmentId, SourceText)` with `SegmentId = 0..N`, plus enough original formatting/position metadata to put pieces back in order. Use paragraph or semantic block boundaries for articles/Markdown; do not split HTML tags, link syntax, fenced code, placeholders or other protected constructs. Translate only extracted human-readable segments.

One logical request must also have a unique `RequestId` so that two documents containing segment 0 do not collide. A completion message is conceptually `TranslationSegmentCompleted(RequestId, SegmentId, TargetCulture, VerifiedTextOrMarkup, Revision)`; the renderer associates it with the matching DOM container, ignores events for other pages/cultures and handles out-of-order or repeated completions idempotently.

Example of a plain-text DOM target (illustrative, not a frozen wire format):

```html
<span data-request-id="req-123" data-segment-id="0">Original paragraph text</span>
```

Use `element.textContent = translatedText` for plain text. For Markdown/HTML documents, first parse source into a safe rendering structure, add stable request/segment attributes on **appropriate block or inline containers**, and insert **sanitized** translated markup only when the format adapter explicitly returns trusted validated HTML. Raw translation text must not be assigned directly to `innerHTML`. A `span` is suitable for inline text, but not a generic wrapper around block-level Markdown paragraphs; choose elements consistent with valid HTML.

The browser can receive segment results through an application's dedicated response/update channel (SignalR, SSE or another delivery contract). **Do not overload the best-effort administrative `/SystemMonitor` with user-facing translations**. The authoritative translation result/cache and, where enabled, accepted job storage are separate; after reconnect, clients may fetch the current verified state. In volatile P0 mode the update itself may be lost according to the documented lifetime policy.

## Optional SystemPersistent translation memory

Treat `SystemPersistent` as an optional **bounded permanent/reusable translation memory**, not an unbounded history or indiscriminate log of every translation. A request can opt to **read** this layer before provider routing; permission to **write/promote** into it is a separate decision. Only configured validated/approved translations should be promoted automatically or manually. Dictionary-authoritative translations take precedence; then a permitted context-matched SystemPersistent memory match; then provisional cache; then a live provider request when necessary.

Stable memory keys include at least source text/key hash, source/target culture, Translation Target/project/domain, context/terminology/format constraints and relevant version. Do not reuse a translation between incompatible documents, glossary revisions or protected-placeholder structures. Support explicit pinning/approval, per-target quotas/retention, listing/removal, and safe invalidation on a source or approved-translation change. Ordinary provisional cache remains short-lived and separate.

## Settings changes and orchestrator precedence

Routing/validation/cost policies are captured from **effective Settings when a new logical request is created**, not continuously recomputed for items that are already queued or in flight. An administrator's new provider order applies to **new requests**; old items retain their original routing preferences. An already-running provider call is allowed to complete under its request settings unless orchestration explicitly stops/cancels it.

**The orchestrator has operational precedence.** An administrator can pause/stop a queue; its waiting items do not start until resumed, regardless of their Request priority or `Mandatory` flag. Pause is not silent reinterpretation of the requests' provider policies. A serious global disable/credential revocation may prevent future dispatch for operational/safety reasons; if it conflicts with a request's snapshot, record a clear failure/paused outcome rather than quietly changing its historical policy.

Preserve the effective settings version/hash with durable job metadata so that delayed work or restart/resume remains reproducible. For volatile work keep only the scoped snapshot needed until its lifetime ends.

## Provider language-specific ordering and capability checks

Default provider tiers: **self-hosted/free translation (LibreTranslate or a comparable endpoint) → paid translation APIs (same preference tier by default) → AI-based translation/review**. This is a configurable starting policy, not hard-coded vendor logic; an administrator may prioritize a paid API first or change preferences **per language pair**. Within equal tier prefer a deterministic eligible/ready selection rule; no automatic permanent quality downgrading is inferred from a single past poor translation.

Providers report model ID/version and supported language pair/format capabilities where available. Run small **version-aware capability/placeholder tests** at controlled moments (initial setup, manual check, changed model version) and do not silently trust old test observations after a model changes. The observed model version number is identity/context metadata, **not** a cross-language quality score: model 1.96 for one language does not imply higher translation quality than model 1.0 for another. Admin-configured per-language priorities take precedence over speculative automated quality ranking.

The provider-routing dry-run simulator is useful but **deferred** until the core Settings and selection contracts are stable. It is an admin convenience, not required for the first working implementation.

## Verification before implementation

Tests should cover RealTime + Mandatory selecting an available acceptable backend without waiting; RealTime returning original text and applying out-of-order segment results; missing/lost updates; plain text safely inserted into the DOM; Markdown paragraph boundaries and placeholder integrity; SystemPersistent permission and context isolation; request settings frozen across priority changes; global queue pause overriding dispatch; same-tier provider selection; model-version changes invalidating previous capability observations.

See [Provider Routing](../Provider-Routing/en.md), [QueueMaster](../QueueMaster-and-Reliability/en.md), [Localization](../Localization/en.md), [Translation Chains](../Translation-Chains/en.md), [Implementation Status](../../IMPLEMENTATION_STATUS.md).
