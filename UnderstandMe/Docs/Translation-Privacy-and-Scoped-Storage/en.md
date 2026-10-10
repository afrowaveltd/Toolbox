# Translation Privacy, Scoped Reuse and Content Access

Status: architectural rules agreed on 2026-10-10; no runtime implementation or frozen DTO names.

## Key distinction: Privacy and Persistence are independent

A translation request includes an explicit `Privacy` property, separately from P0-P4 `QueuePriority`, behavior flags and storage/lifetime Settings.

- **Privacy** governs who may read the source, stored translation, related history, previews and audit content, and which processing routes can receive it.
- **Persistence / CachePolicy / LifeTime** governs whether/where a successful translation or transient work state is retained and for how long.
- **Provider selection** independently determines eligible automatic engines, subject to applicable client/target data-disclosure policy. `Private` does **not** automatically mean that all remote engines are disabled; private-content transmission to any external API is allowed only if the effective target/client privacy policy explicitly permits the relevant processor.
- **QueuePriority** (especially `P0 RealTime`) governs work urgency and lifecycle. P0 execution remains volatile, machine-only, and has no Human fallback even when `Mandatory`.

Thus **`Privacy = Private` plus a durable, access-restricted translated result is valid**. It must not be treated as `DoNotStore`. Likewise `Private` is **not** consent for human review.

Start with a simple request-level `Private` versus ordinary/default classification (exact C# enum or typed property TBD); avoid a hard-coded assumption that every forum is public or every chat has identical permissions. A containing Chat/Target policy may require Private even if the client omits it. An untrusted request cannot weaken the privacy level imposed by the owner/administrator. Scope and audience are separate authorization attributes, **not** just a Boolean.

## Persist successful private translations for history reuse

Chats, forum conversations and similar historical content may need saved translations so an authorized reader who reopens the history does **not pay and wait for the same translation again**. The result belongs in an **access-scoped translated-content store / translation cache**, not in a universally visible dictionary, translator work inbox or global unpartitioned SystemPersistent memory.

A `P0 RealTime` execution can complete and be discarded as a volatile job while its **validated final translation** is saved in a separate durable store when `CachePolicy`, permissions and source retention allow it. Save the result through a bounded owner-specific storage adapter or alongside the original host's message record, whichever is configured; do **not** require preserving the P0 request, full execution history or intermediate text. Consider storing only the translation and a reference/hash to the original message when the host already retains original chat content.

Suggested conceptual private result identity: `TenantId/TargetId + ConversationOrDocumentId + MessageOrSegmentId + SourceRevisionOrHash + SourceCulture + TargetCulture + relevant format/placeholder context`. The exact schema is still open. Cache lookup must authenticate the reader against **the current underlying source access rights**, not merely possess a cached ID or once have had permission. A translation from one private conversation must not be served as a shared global hit for another conversation solely because the source text matches.

On source edit, deletion, retention expiration, context change or changed conversation membership, invalidate/expire/restrict the related translations in step with the authoritative host policy. Do not indefinitely preserve a deleted private message through a translation cache or backup without an explicit applicable retention rule. Concurrent requests for the same authorized source/culture can deduplicate work within that security scope.

## Strict Human and manual-audit exclusion

When `Privacy = Private`:

- **No Human translator or manual quality auditor** may receive this content through an automatic or fallback path. It is never published in any Human work queue, translator UI, dictionary Editor, manual review panel or human-visible step history.
- A configured **automatic** validation step (placeholders, syntax, language/format; privacy-authorized automated review engine) may examine the content as its approved processing path; this is not a manual audit.
- SystemMonitor and general admin dashboards may show **content-free** operational facts (RequestId, processing status, safe error codes, provider, estimated units/cost, latency), but **must not expose** message text, translations, sensitive placeholder values, prompt contents or private content in exception/error details. A human administrator's ability to configure or manage a provider does **not** automatically grant permission to browse someone else's conversations.
- The existing mandatory **revision audit for manually edited authoritative dictionaries** still applies to such *manual edits*, but is **not** a requirement to expose private chat contents or create a manual audit record for every automatic private translation. If system-level operational audits are needed, use content-free records with the same scoped authorization.

No silent downgrade `Private -> Public`, promotion to a globally shared SystemPersistent entry, or reuse of private translated text to train another provider. Any entirely separate voluntary/manual disclosure of a message would require a **new, independently authorized non-P0 operation** and must not be treated as a fallback or a way to bypass the original private restriction.

## Client-facing contract and retrieval

The orchestrator receives `Privacy`, normalizes it under stronger client/target policies, and provides effective classification/allowed storage and processing options in the typed request context. Preserve the original request and note any administrator-imposed tightening. `Private` is enforced at ingestion, provider eligibility, caching, result access, events and deletion.

For translated content already saved, the host retrieves a permitted translation and returns it to an authorized viewer without calling the translation provider again. If the source text changed, the translation is stale or the viewer cannot access the underlying message, do not serve that cache entry. Language/culture differences result in separate translation identities. Subsequent normal P0 lookups still use automatic engines only and may return original text immediately if no eligible engine exists.

No automatic translation engine has a right to read the entire message history merely because it was given a translation segment. Expose only the current required content through the approved adapter; provider request/response logging must respect private retention policy.

## Before implementation

Specify actual `Privacy` type and inheritance, privacy scope and authorization interface, exact cache identity/storage adapter, deduplication behavior, result TTL/retention rules, owner-triggered deletion, access changes, private data in backup/logs, and permission for external AI/cloud endpoints. Define a simple host-integrated API for private translation lookup/writeback that does not grant generic access to all chat messages.

Tests: private P0 translation is persisted as **result only** and reused on history reopen; P0 execution is volatile; private chat never appears in Human or manual audit UI; unauthenticated/other-chat/other-tenant retrieval is rejected even with cached ID; membership revocation applies immediately; source edit/delete invalidates results; context/culture partition works; monitoring/errors do not leak text; local-only or remote-enabled provider policy is enforced; `Mandatory` cannot weaken privacy.

See [Request Settings](../Request-Settings-and-Live-Translation/en.md), [Localization](../Localization/en.md), [Editor and Audit](../Editor-and-Audit/en.md), [Human Workflow](../Human-Translation-Workflow/en.md), [Client Access Policies](../Client-Access-Policies/en.md), [Implementation Status](../../IMPLEMENTATION_STATUS.md).
