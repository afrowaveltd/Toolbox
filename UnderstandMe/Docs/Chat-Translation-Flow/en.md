# Chat and Message Translation: Transient Chunks, Host-Owned Results and Recipient Corrections

Status: architectural clarification agreed 2026-10-10; documentation only, no code or frozen API.

## Distinguish chat translation from localization dictionaries

**Chat, forum posts and other user-generated messages are not localization dictionaries.** The localization convention `KEY = VALUE` (including dictionary-less `KEY=original phrase`) is used **only for application/web UI text and configured localization dictionaries**, such as flat JSON resources. Chat translation never registers text as a global localization key.

For message translation, the format-aware Splitter produces an **ephemeral request-local** ordered list `List<(Id, Text)>`, with `Id = 0..N`. `RequestId` and per-chunk IDs are used **only to correlate and reassemble** partial translations, including out-of-order completion. They are **not persisted as chat translation keys, cache IDs or dictionary records**. The translation engine receives only needed segments, processes them, and returns translated segments/results. Transient correlation state expires when the active request is assembled/finished.

## Meaningful chunks depend on source structure

Splitter chunking is primarily useful for **larger or structured texts**. A short ordinary chat message can stay **one unit**. For longer plain text, prefer **paragraph boundaries** when the source contains recognizable paragraphs (line breaks or blank lines according to the configured source format); otherwise prefer **sentences**. The delimiter is configurable; a literal period is not always a sentence end (decimal numbers, abbreviations, URLs). For Markdown and HTML, use text-bearing structural elements as natural units, preserving formatting and content context instead of splitting blindly by punctuation.

Each `Id=0..N` remains **transient request-local correlation only**, independent of the chat application's persistent message ID and unrelated to `KEY=VALUE` dictionaries. The host receives translated chunks and assembles the final message. Details: [Semantic Splitter](../Semantic-Splitter/en.md).

## Composition, presentation and persistence belong to the caller

**The consuming chat/forum application** owns message identity, recipient permissions, assembling chunks into the entire translated message, UI display and any saved history/cache. UnderstandMe supplies a transport/framework-neutral splitting/translation/result contract and never requires a chat-specific message database.

The host may save the **complete translated message** alongside the message's source record so later authorized history views do not translate the message again. This is the chat application's choice, based on its own storage/retention and source-message revisions. It does not require the engine to retain request-local chunk keys, the P0 job or a universal translation-result database. A host may elect not to save automatic results under its policy.

`P0 RealTime` remains immediate, volatile and **automatic-only**. In a Human-only system, it returns untranslated source promptly; machine failure does not send chats to a human. Storing a completed translated message **outside the engine** does not turn the P0 job into persistent work.

## Privacy and recipient-owned corrections

An initial private-message translation is **private to the authorized reader(s)**; it is never an invitation to third-party human translators, central manual auditors, administrative text browsers or common localization dictionaries.

**A recipient who may already read the message may voluntarily correct the translation they see**, for their own readability. This optional UI action may be called personal translation correction or recipient review, but it is **not** central Human-worker auditing or a queue fallback. It occurs **after** translation, in the consuming application or an explicitly integrated editing tool, without modifying the sender's original message.

An edited result has an independent storage choice controlled by the recipient and host policy:

- **Do not cache:** apply the correction in the current recipient view/session only; no permanent corrected translation is saved simply because the recipient edited it.
- **Cache for me:** at the recipient's express opt-in, retain their corrected translation under a recipient/message/language/revision-scoped entry for their later views. The original host may separately store the initial automatic result for authorized history reuse. Editing does not silently override the shared original or other recipients' translations.
- **Optional share/promote:** if a product later offers contribution of the correction to a broader cache, **sharing is a separate explicit action and authorization decision**, not a consequence of clicking “cache.” The chat source may still be private even when an individual message happens not to contain personally identifying information. Do not infer sender/other-participant permission to publish from a recipient's desire to fix their own displayed text. Promotion to generic translation memory/localization dictionaries is not automatic.

The recipient may turn off caching even if the UI offers it; any edits must remain scoped to that recipient unless an explicitly authorized broader sharing flow exists. Whether the chat application needs a lightweight per-user revision/override audit is a host feature, not the mandatory manual **dictionary-edit audit** of the localization Editor.

## Minimal conceptual flow

```text
Chat app owns message + current reader's access
  -> Request(Privacy=Private, P0 RealTime, source text)
  -> Splitter: ephemeral chunks (0..N)
  -> Allowed automatic translation workers
  -> Returned translated chunks with request-local IDs
  -> Chat app assembles the complete translated message
  -> Chat app displays/stores it under its own history/privacy policy
  -> Optional recipient self-correction
      -> apply only now (no cache)
      -> or opt in to recipient-scoped corrected translation cache
      -> optionally separate explicitly authorized sharing of that correction
```

The engine does not need to know that the output originated in a chat, except for the request's `Privacy`, format, source/target language and effective engine-disclosure rules. Reopening a chat history is a **chat-app lookup**, not an instruction to rerun the Splitter/translator if the host already has an eligible saved translation.

## Remaining implementation choices

Finalize the Splitter's transient result ordering/reassembly API and cleanup; chat host cache/lookup integration; per-recipient correction UI and opt-in cache setting; correction versus message revision invalidation; owner permission checks; how to treat changed translations when a model improves; opt-in cross-user sharing semantics only if a consuming product needs it. No central chat storage or default global cache promotion is required.

Tests: splitter IDs are request-local and never added to dictionaries or durable engine storage; out-of-order chunks are correctly returned/reassembled by the host; translated history is reused by the chat app; recipient-only edit is not shown to another participant; no-cache edit leaves no persisted correction; opt-in cache correction is isolated by recipient/message/revision/culture; neither edit nor ordinary private translation reaches Human queues/manual audit or public monitoring.

See [Privacy and Scoped Storage](../Translation-Privacy-and-Scoped-Storage/en.md), [Request Settings](../Request-Settings-and-Live-Translation/en.md), [Localization](../Localization/en.md), [Editor and Audit](../Editor-and-Audit/en.md), [Architecture](../Architecture/en.md), [Implementation Status](../../IMPLEMENTATION_STATUS.md).
