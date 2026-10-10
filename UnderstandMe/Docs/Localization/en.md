# Localization, Dictionary-less Start and Placeholders

Status: requirements design; no localization engine is implemented in the current UnderstandMe skeleton.

## Modes and storage

Support standard .NET localization abstractions, including `IStringLocalizer`, plus optional middleware/integration for applications and web apps. Source/target cultures, fallback cultures, dictionary-less registration, key/value convention, ignored or manual-only target languages, live translation and background processing must be configurable per target.

Dictionary-less start means the application can register previously unseen phrases without pre-authored catalogs. In its simplest form, the source-language dictionary stores `KEY = VALUE`. Alternative projects may use stable symbolic keys with source-language values supplied separately. Afutrans/plugins use English phrases as the default source convention; UnderstandMe itself should allow other source languages.

Dictionary stores can be local flat JSON (`en.json`, `de.json`, etc.), structured JSON, SQL or another configured DataAccess adapter. Culture identifiers must be configurable rather than permanently limited to two-letter names; regional languages may need locale tags.

## Lookup lifecycle

1. Look up the key in the target-language dictionary. If present, return the authoritative value.
2. On a miss, check the default-language dictionary; register `KEY = VALUE` and any placeholder hints only if newly discovered. Signal the change.
3. If the target-language provisional translation is not already cached, request one quick, **nonpersistent** translation for the current user's language according to live-translation policy and timeout.
4. Return that usable validated result promptly, or the source/fallback on failure; cache a provisional result separately from the authoritative dictionary.
5. After delivery, or on a later lookup/expiry, check whether an authoritative translation has arrived and promote/replace the cache value.
6. A committed translation change may publish `Update_Cache`, locally or via SignalR, to accelerate refresh. Lost events are repaired via periodic reconciliation (illustrative interval: three hours) and startup/reconnect catch-up.

Synchronous `IStringLocalizer` consumers must not be assumed able to await a remote backend; an asynchronous API/host strategy is a separate open design point.

## SystemPersistent memory versus provisional cache

The optional `SystemPersistent` translation-memory tier provides durable, bounded reuse of **explicitly selected and verified** translations. It is not a duplicate of every request history and must not expand forever. Request Settings can opt in to **lookup** before requesting a backend; **writing/promoting** an entry is a separate, explicitly controlled operation. Preserve authoritative target dictionaries as the highest-priority answer, then allowed context-matched SystemPersistent entries, then provisional cache, then the live backend.

Match on source/target culture, Translation Target/domain, context/format, source identity and protected placeholder structure; reject stale/incompatible glossary or source revisions. Support quotas, removal/expiry policy and pinned entries where applicable. RealTime non-Mandatory requests can immediately display source text and later replace only the completed segment identified by `RequestId` and `SegmentId`. User-facing segment updates use a separate application channel rather than SystemMonitor. [Request Settings and Live Translation](../Request-Settings-and-Live-Translation/en.md).

## Human-only installations and privacy of RealTime content

For RealTime translation the resolver may immediately return source text when no activated eligible **automatic** engine is available; if only Human is configured, P0 translation is **inactive**, and no Human job may be created by fallback. Non-Mandatory live partial updates, when configured, must come from automated translators only. In particular, a RealTime lookup of a private chat message must not disclose that message to a human translator or persist it as manual work. A separate durable manual job requires explicit authorized non-P0 submission; any dictionary registration/cache is independently governed by target privacy policy. [Human Translation Workflow](../Human-Translation-Workflow/en.md).

## Private translations can be reused without making chat content public

For chat, forum or similar history, `Privacy = Private` may coexist with a **durably stored verified translation result**, under host/target retention settings. The P0 job remains volatile; its private result is cached/persisted in a **viewer-authorized, conversation/tenant-scoped** translation store so revisiting history does not retransmit an unchanged message to the backend.

Private message bodies must **not** be auto-registered as public/default-language `KEY = VALUE` dictionary entries or promoted into global `SystemPersistent` memory. Keep the application-local dictionary-less workflow for eligible public/application UI strings, but private chat history uses **its source owner's protected identity and access rules**, source revision/hash, format and language in its own lookup key. Check present permissions on every read. An edit, delete, retention expiry or revoked chat membership must invalidate/restrict the translation as appropriate. This is separate from real-time Human exclusion and from ordinary provisional cache.

A private-result lookup may be served to an **authorized participant** without invoking an engine again; translation existence must not imply permission to read the source. See [Privacy and Scoped Storage](../Translation-Privacy-and-Scoped-Storage/en.md).

## Placeholder hints

For a source string such as `We found {count} dictionaries in {folderCount} folders`, a flat hint store may be:

```json
{
  "We found {count} dictionaries in {folderCount} folders": ["5", "2"]
}
```

Hints map by **occurrence index** in the original key (0, 1, ...). They are illustrative values for the translator, not necessarily runtime replacement arguments. The translator must compare the number of hints against occurrences; a mismatch skips the current automatic attempt for later correction.

Placeholder identity and original content remain available in memory. During translation, each original occurrence is protected with a marker carrying its index. Restoring markers solely in the new order of appearance would be wrong for languages that reorder parts of the sentence. Accept only a result with the exact expected set and multiplicity of placeholder indices; never save a broken string.

No delimiter (ASCII, Unicode or HTML) is guaranteed preserved by every engine. Protection strategies are interchangeable: provider-compatible HTML markers, textual tokens, differential translation experiments and optional AI repair. Test per provider/language pair. Placeholder correctness is mandatory; perfect grammar (plural/gender inflection) is not part of the initial goal.

## Data safety

Provisional cache entries cannot overwrite a newer authoritative value. Keys include at least target/source identity, culture and phrase identity, plus relevant context. Cache expiration and event-driven refresh are complementary. Registration events and final-translation notifications are distinct concerns; their exact event contracts remain under discussion.

Related: [QueueMaster](../QueueMaster-and-Reliability/en.md), [Notes](../../Notes.md).

