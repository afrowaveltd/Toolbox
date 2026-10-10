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

## Host-owned chat history, not engine-owned chunk keys

**Chat translation and application-localization dictionaries are different workflows.** `KEY=VALUE` dictionary-less registration belongs to configured UI application/web localization dictionaries (JSON etc.), never chat/user-generated message translation. A Splitter produces **temporary request-local** `(SegmentId, Text)` pairs numbered 0..N; IDs only correlate translated pieces while the request runs. The engine must not persist these IDs as dictionary keys or message IDs.

The **chat/forum application** owns its message identity, source access rights, assembling returned translated chunks into the entire translated text, the finished message view and **all saved history/result caching**. UnderstandMe merely processes/supplies translated segments and applicable privacy metadata; it does not have to maintain chat records or automatically save source/translated message bodies in a central translation store.

For an authorized reader who opens history later, the **host application** may reuse a saved complete translation, scoped to its message and reader permissions, rather than calling an automatic engine again. This is legitimate private storage **outside** the volatile `P0 RealTime` job. The host can retain a translated message under its chosen source retention and permission policy; it need not keep raw intermediate chunks, request-local indices or a copy of the full processing history.

When the owner edits/deletes a message, its translated version may become stale or need deletion. On every history access, the chat application checks current source-message visibility and recipient authorization. No cached translation from one private chat is shared into another user's result merely due to identical text.

See [Chat Translation Flow](../Chat-Translation-Flow/en.md) for recipient corrections and opt-in cache behavior.
## No external Human audit; optional correction by the recipient

When `Privacy = Private`:

- **No automatic Human-provider routing, queue fallback, third-party manual auditor, general Editor browsing, or administrator-visible source/translation text.** These are prohibited even when the host has saved the finished chat translation.
- **The authorized recipient may personally edit the translated message they see** to fix a bothersome or inaccurate translation. This optional feature is a **self-correction**, not a centralized manual audit or an implied request to a professional Human worker. It can be implemented entirely in the chat application's editor.
- **Caching an edit is recipient-controlled:** apply it only in the current view when caching is off, or explicitly cache the corrected result **for that recipient and message** if the user opts in and the host allows it. No-cache editing must not quietly create a persistent correction.
- **Sharing is separate from caching.** A voluntary wider contribution/publication of a corrected translation requires its own explicit action and applicable disclosure/owner authorization; a private message need not automatically be public just because its reader considers its text harmless. A cached personal correction is never automatically promoted to global `SystemPersistent` or localization dictionaries.
- A permitted **automatic** check can run inside the effective privacy/provider rules; private content is not a reason to silently invoke Human audit. SystemMonitor and general admin views receive content-free operational metadata only, without original text, corrections, translated chunks, prompt values or private exception details.
- Mandatory revision audit for **actual manual edits of authoritative UI dictionaries** remains intact, but is not invoked merely because a recipient edited their own chat translation.

Distinct, deliberately authorized non-P0 manual translation work remains possible when appropriate; it is never created by RealTime fallback or by pressing an ordinary personal edit button.
## Host result lookup and translator boundary

The orchestrator accepts `Privacy` and enforces applicable policy on automatic provider eligibility and any content disclosure. The Splitter sends only needed message chunks to eligible automatic workers and returns each result with temporary `RequestId`/`SegmentId` correlation. **The host application reassembles the completed text**, displays it and independently decides whether to retain the original translated result for history reuse.

The host, not the translation engine, owns permissions, message revisions, saved translation lookup, optional personal correction cache and its deletion. If no stored complete translation is available, it may request translation again. A caller without access to the original message must not receive the cached result. This does **not** require a generic durable chat translation database in UnderstandMe.

Any recipient correction is an application-level override, with opt-in/no-cache behavior. It does not alter another reader's default translation and cannot be served cross-tenant or globally by default. P0 remains machine-only and volatile whether or not the host keeps a finished translation.

No automatic provider is entitled to read the entire message history for a single segment. Follow host and target privacy rules for remote providers; billing/monitoring must not contain chat text.
## Before implementation

Specify privacy types and effective authorization, ephemeral segment/result contract, host-side finished-message reassembly, history cache interface (if any), cache invalidation on source edits/deletion, per-recipient self-edit UI, recipient choice of no cache versus personal cache, and optional separately authorized sharing flow. Keep the UI `KEY=VALUE` dictionary lifecycle entirely independent of chat translation. Do not impose a chat storage engine on UnderstandMe.

Test chat chunk IDs never persist as dictionary keys; out-of-order pieces reconstruct correctly in the host; history lookup avoids needless engine calls; Private results never reach Human queues/third-party audit/admin text views; a recipient edit changes only their own display; disabling edit caching writes nothing; opt-in cached edits are recipient-scoped and invalidated with source changes; sharing cannot occur through accidental cache promotion. Preserve P0 volatility and machine-only routing.
See [Request Settings](../Request-Settings-and-Live-Translation/en.md), [Localization](../Localization/en.md), [Editor and Audit](../Editor-and-Audit/en.md), [Human Workflow](../Human-Translation-Workflow/en.md), [Client Access Policies](../Client-Access-Policies/en.md), [Implementation Status](../../IMPLEMENTATION_STATUS.md).
