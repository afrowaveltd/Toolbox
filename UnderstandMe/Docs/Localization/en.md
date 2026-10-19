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

## UI dictionary keys are not chat translation identifiers

The `KEY = VALUE` convention and dictionary-less source registration in this document apply **only to configured application/web UI localization dictionaries**, including flat JSON or other dictionary backends. **Chat messages, private discussions, articles and other user-created content do not enter this dictionary lifecycle by default.**

A chat Splitter emits temporary ordered `(SegmentId, Text)` pieces with `SegmentId = 0..N` scoped to an active request. These IDs serve **only for correlating/reassembling partial translated results**. Do not store them as localization keys or cache keys, and never register entire private chat messages in `en.json`/`cs.json` or another global UI dictionary.

The **chat host** reassembles completed translations, decides whether to store a finished message in its own history and checks its audience permissions. Its authorized recipient may optionally correct their own displayed translation and choose whether to cache **that correction** or keep it only in the current view. Recipient-saved corrections are per-user/message/culture overrides, not shared glossary entries. A separate, explicitly authorized promotion/share mechanism (if ever added) is not implied by opting in to cache.

This separation does not interfere with ordinary UI dictionary caches and `SystemPersistent` memory intended for eligible non-private application strings. See [Chat Translation Flow](../Chat-Translation-Flow/en.md) and [Privacy and Scoped Storage](../Translation-Privacy-and-Scoped-Storage/en.md).

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


## Planned optional grammar and inline hint layers (2026-10-11)

**Agreed design direction, not implemented:** keep every basic locale dictionary as a **flat JSON key/string map**, suitable for an ordinary localizer. Optional translator-facing hints (for example `en.placeholders.json`), locale-specific variants (`cs.grammar.json`), CLDR reference rules and eventual SQL equivalents are strictly separate. They are never prerequisites for dictionary-less source-key registration or basic lookup. Failed grammar generation falls back to the known base/source phrase rather than preventing startup; strict placeholder integrity still applies to every accepted generated translation.

Preserve the existing `t[key, params string[] args]` contract and add a distinct typed-hints-before-`params` overload. Support composite plural/gender/other hints through one resolver without forcing a public API change for every new rule. Prefer typed hint records/optional unions to untyped argument bags; keep .NET 10 extensibility and note that the exact API/JSON schema is not finalized.

Afutrans can later generate and review per-locale variants using pinned offline CLDR datasets, while editors may override locale-specific rules and values. English is the Afutrans convention, not a hard requirement on UnderstandMe; native English grammar variants may also be needed. [Layered Localization and Grammar Hints](../Grammar-Hints-and-Variants/en.md) is the detailed continuation document.

## Data safety

Provisional cache entries cannot overwrite a newer authoritative value. Keys include at least target/source identity, culture and phrase identity, plus relevant context. Cache expiration and event-driven refresh are complementary. Registration events and final-translation notifications are distinct concerns; their exact event contracts remain under discussion.

Related: [QueueMaster](../QueueMaster-and-Reliability/en.md), [Notes](../../Notes.md).

## Cross-platform localization host capabilities and live dictionary exchange (2026-10-18)

## Cross-platform localization host capabilities and live dictionary exchange (2026-10-18)

**Design decision — not yet implemented.** Every implementation of the *shared localization subset* (.NET, Rust or later languages) is a fully usable offline localizer, but also exposes appropriate optional dictionary import/export/update facilities for its host environment. This does **not** mean porting the complete UnderstandMe/QueueMaster/AI translation engine to each language. The translation workflow remains in the .NET UnderstandMe/Afutrans services.

### Common dictionary contract

- The authoritative interchange unit is a **flat locale JSON dictionary** with an optional associated sidecar set for context, placeholder descriptions, allowed values, and language-specific grammar rules. Preserve stable source key identity (Afutrans convention: English key equals English source value), locale and origin/target identifiers.
- Implement native lookup, safe placeholder replacement, configurable fallback and dictionary-less source registration from absent keys, all **without requiring network access**. Optional sidecars must not block a simple translation.
- Each host can **get/export** its dictionary for an authorized translation manager and **receive/import** an updated dictionary or approved changes. Dictionary updates are not intrinsically tied to a new application version. Preserve local user overrides and validate protected placeholders before activating changes.
- Use an explicitly versioned, shared transport-independent dictionary exchange contract, suitable for REST JSON, a business SignalR Hub, and host-local file adapters. Consider source revision/ETag-like conditional writes and conflict responses; do not silently overwrite more recent local/human corrections.
- Make update status observable and ensure either the old valid dictionary or the newly validated dictionary is active (atomic replacement/snapshot semantics). A temporary invalid JSON edit should not blank out the application's UI.

### Desktop and local clients (.NET, Rust, other languages)

- Detect changes to locally configured dictionary/sidecar files and **reload them dynamically** (debounced validation and replacement) without requiring application restart. A file watcher is an implementation detail; if change detection misses an event, the next explicit reload/rescan must still work.
- Allow **optional update checks at startup** and an explicit user/application-triggered download or full dictionary synchronization from a configured HTTP REST API. Network absence or failure leaves the current local catalog and offline fallback operational.
- When configured for bidirectional sync, support submitting the local source dictionary or changed entries, with relevant sidecars, to the .NET translation service; fetching the returned updated target dictionary; and applying it safely. No additional automatic machine translator runs inside a Rust localizer.
- Server-driven change events are optional accelerators rather than the source of truth: reconnection can recheck current revisions.

### Web application hosts and passive/active translation targets

- An application can **expose authorized REST endpoints and, where useful, a dedicated business SignalR Hub** to retrieve its current dictionary/metadata and submit/publish validated updated dictionaries. The .NET translator can *pull* from that application and *push back* a translated/edited dictionary, while an active application can initiate the same exchange against Afutrans.
- REST handles authoritative snapshot/change retrieval, versioned writes and retry/resume. Business SignalR may carry corresponding authorized commands and notifications of changed dictionary revisions. These must route through the **same application validation, authorization, persistence and audit logic**; do not create two divergent translation engines.
- **Do not use the outbound-only `/SystemMonitor` monitoring Hub for dictionary reads, writes or translation commands.** A localization business Hub is separate. An update notification is never proof that data was durably accepted.
- Web hosts may use filesystem, storage adapter or database backing behind their API. Do not require exposing filesystem paths publicly; honor target access policies and disallow unauthorized writes. Dictionary exchange is distinct from private chats/documents, which must never be enrolled as ordinary dictionary keys automatically.

### Automatic connection to Afutrans

- Afutrans can register a configured Translation Target with a location/transport, permitted languages, credentials and sync policy; discover new keys and synchronize translation results **on a configured schedule or source change**. A target may be passive and never invoke the translator itself, or active and request an update.
- Automatic sync is **opt-in/configurable**, not a mandatory runtime dependency. The localizer must return usable text before an update, while an offline/disabled translation system preserves local usability.
- Share one suite of language-neutral JSON dictionary/sidecar fixtures and host exchange/reload tests between .NET and Rust, including unchanged revision, conflicts, malformed file, manual override, offline startup and reordered protected placeholders.

**Scope:** localizers own lookup and dictionary access/update; .NET UnderstandMe/Afutrans owns provider routing, automated translation, jobs, review and administration. Concrete REST routes, SignalR method names, revision tokens and authentication remain design questions; this section does not claim working endpoints.

## Dita: independent reference only, not a source-code dependency (2026-10-18)

**Explicit project boundary:** [Dita](https://github.com/afrowaveltd/Dita/tree/master/Dita.Shared/Localization) is an entirely separate application/project, consulted only for ideas and lessons learned. **Do not copy, extract, refactor, migrate or depend on Dita's localization classes or packages** when implementing UnderstandMe. Dita is not a donor module, shared runtime, or required test project.

A review of Dita's existing `JsonStringLocalizer`, `LocalizeService`, `LanguageService` and `PlaceholderService` shows useful *concepts* worth considering: .NET `IStringLocalizer` compatibility, flat locale JSON files, dictionary-less source registration, named placeholders, optional sidecar hints and translation-safe placeholder protection. Reimplement these *independently* in UnderstandMe with explicit contracts and tests, rather than treating Dita's APIs and internal assumptions as authoritative.

### First independent implementation path

1. Build a small, **standalone UnderstandMe** .NET 10 localization core that works without a translation service: flat JSON lookup, configured locale/source fallbacks and dictionary-less source registration. Preserve a usable source text even when no files exist.
2. Add deterministic `{name}` placeholder formatting, placeholder integrity checks and an optional auxiliary JSON store. Expand hints/allowed values/grammar only after the basic contract is tested.
3. Support safe local dictionary replacement/reload with tests. Keep REST, SignalR and online sync as optional host adapters layered on later.
4. Use **Afutrans**, which already has its *own* basic `Afutrans.Core/Localization/ILocalizer` and `Localizer`, as the first **real integration and test application**. Provide an adapter so its existing UI/view-model contracts and `CultureChanged` event continue to work. Its present built-in English/Czech catalogs are a transitional fixture, not UnderstandMe's mandatory runtime data.
5. Later establish shared flat-JSON/sidecar contract tests for a minimal Rust localizer; Rust will not duplicate .NET translation engines or orchestration.

Reference: [layered hints and grammar](../Grammar-Hints-and-Variants/en.md) and [host integration](../Afutrans-Integration/en.md). Keep the `UnderstandMe` library independent of both Dita and Afutrans; dependencies point from Afutrans to the independent UnderstandMe localization package, not in reverse.

**Status:** this documents intended direction only; no code, .NET build/tests or integrations have been implemented in this editing pass.

## Storage-neutral dictionary provider architecture (2026-10-19)

**Decision:** UnderstandMe must support interchangeable dictionary providers by interfaces, not be permanently coupled to JSON. Flat locale JSON is an optional default for Afutrans and a convenient interchange format, not a restriction on other consumers.

- Keep synchronous localization operations independent from persistence and transport. `ITextLocalizer` resolves a normalized catalog snapshot; `ILocalizationDictionaryStore` is the already-implemented initial injection point.
- Concrete providers may read/write JSON, other local document formats, SQL/EF Core via DataAccess, in-memory catalogs, remote REST/SignalR, Node.js services or application nodes.
- Add a separate **asynchronous** dictionary source/refresh contract for remote/database loads instead of blocking synchronous localization on network I/O. Runtime lookup continues against a valid cached snapshot, even offline.
- Treat read, write/registration, change notifications, revisions/conflicts and optional placeholder/grammar metadata as separate **capabilities**. Read-only sources are first class; dictionaryless discovery writes only where a writable target or deferred outbox is available.
- Keep dictionary exchange format (e.g. REST JSON) independent of the backing store. Reuse transport-neutral contracts; no circular dependency from the localizer to Afutrans or a particular server.
- Prefer simple interfaces and implement extra contracts when a real second provider needs them, not an overengineered factory hierarchy now.

The misleading first-slice runtime name `JsonTextLocalizer` is replaced with `DictionaryTextLocalizer` (pre-release rename). JSON-specific loading remains in `FileJsonDictionaryStore`. The first additional proof uses an in-memory test provider through the same interface.

## Logical dictionary plus logical metadata, independent of physical storage (2026-10-19)

The agreed design treats **dictionary entries and their translation/localization metadata as a coherent logical resource**, not as a mandated pair of physical JSON files. In an Afutrans file-backed installation, the resource maps to flat locale JSON dictionaries plus optional auxiliary JSON files. For a database-backed host, both can be SQL rows/tables; for another integration they may come from REST, SignalR, a remote node or custom storage. The rest of the localization and translation pipeline must not notice the physical difference.

The **same provider-facing contracts** are shared by localization and dictionary translation/editor workflows. A resolver never parses sidecar file paths or executes SQL itself; a translator does not have a separate incompatible metadata reader. The provider/adapter supplies normalized language/key/value information, optional hint/grammar metadata, and revision identity when supported. A translation accepted against one source/metadata revision must not overwrite a newer version without conflict handling.

The current first-slice `ITextLocalizer` and `ILocalizationDictionaryStore` demonstrate one injection boundary and file + in-memory operation. They **do not yet** provide the planned metadata abstraction, remote async APIs, coherent resource snapshots or database implementations. Introduce those incrementally without tying the core to a particular backend or to compulsory online services.

See [cross-module contract decision](../Architecture/en.md).

## Optional change signaling for dictionary providers and localizers (2026-10-19)

An underlying `DataAccessor` **or** a localizer/editor may detect an accepted dictionary/metadata change. The system expresses it as a **single logical change notice**, distributed only when an optional notifier/transport is configured. This is a capability, not a method/event that all providers are forced to implement. Read-only embedded desktop catalogs remain valid; remote-backed desktop or web applications may subscribe.

Events/notifications can travel via normal local application callbacks, Messenger, HTTP REST calls/webhooks, business SignalR or future transports. The event is not the transport. A normalized event ideally has a change kind, culture and target, affected key(s) or snapshot revision, and a stable change ID/correlation for deduplication; payload shape is not finalized. A successful `TryAdd` of a genuinely new source key is distinct from a failed add, file reload, translated target update or grammar-metadata correction.

Automatic translation listeners may create durable jobs **only for relevant authorized source changes**; a target translation change must not feed back into another translation request. Other optional listeners refresh local cache or notify web UI. No subscriber/no connectivity must not block localization or a successful write. Lost ephemeral announcements are reconciled from the actual authoritative store; durable translation workflow uses a persisted outbox/change-log/scan when stronger delivery is required.

DataAccessor detects changes it actually performs; detecting out-of-band SQL/file/API writes requires a provider-specific watcher, trigger/change-feed or polling. Avoid duplicate notices when both localizer and accessor observe the same operation. See [Architecture: Optional dictionary-change notifications](../Architecture/en.md).

**Status:** documented design only, not an implemented `ILocalizationDictionaryStore` event or API.

## Configurable providers, notification policy and independent discovery (2026-10-19)

The host may use `appsettings.json`, constructor-injected typed options, DI or a custom configuration provider. A localizer may be configured to publish **no change events**, or selected kinds of successful dictionary changes via local callbacks, Messenger, REST, SignalR or custom transports. Neither the JSON filename nor a Microsoft web configuration stack is compulsory for UnderstandMe.

A separate translation-service worker may periodically **pull** authorized dictionary and metadata revisions through provider interfaces. It is especially useful for SQL catalogs or applications that never publish events. The worker compares/checkpoints source changes, hands off valid jobs through the normal admission/QueueMaster path and does not duplicate active push-triggered work. Detection is optional and configurable per target, including its interval and enabled state.

Neither polling nor notifications are part of synchronous `ITextLocalizer.Get`. See [configuration and scanning contract](Configuration-and-Change-Detection/en.md).

## Source key revision versus key usage (2026-10-19)

Every target independently sets a source culture (en, cs or another) and a key mode. In dictionaryless SourceText mode, key=value in the selected source culture, and a reworded sentence is a **new key**. In Symbolic mode the identifier is stable but its source value can change; the translator must detect changed values and invalidate old target translation revisions instead of treating the source as unchanged.

An obsolete SourceText phrase often remains in a dictionary after the app no longer uses it, so comparing successive catalogs cannot establish that it is unused. Usage is an **optional** separate capability (e.g. batched aggregate last-seen reports or a current-build key manifest), not mandatory network calls from Get. Absent evidence or rare use must never trigger automatic destructive deletion. **Revised decision:** do not prune, archive or expire discovered dictionaryless phrases based on inactivity; they can be reused by other projects. See [shared translation signatures](Shared-Translation-Reuse-and-Placeholder-Signatures/en.md) and [snapshot rules](Dictionary-Snapshots-and-Key-Lifecycle/en.md).

## Cross-project translation reuse by placeholder structural signature (2026-10-19)

Afutrans or an optional shared translation service may expose a logical `FindInAnyDictionary` search over **authorized** dictionaries. Search exact source keys first, then a versioned **canonical structural signature** that keeps all fixed text literal but replaces placeholder names with stable slot identities by first occurrence, preserving repeats and target-language reordering. Example: `Hello {user}` and `Hello {customer}` both normalize to `Hello {#0}` and can reuse a validated `Ahoj {#0}` mapping to the requesting placeholder name.

Shared matching is independent of storage (JSON/SQL/remote), respects source/target culture, compatible grammar/context hints and review policy, and never replaces a project's own original key or manual override. No background scan/purge based on `LastSeen` is needed for dictionaryless keys. See [Shared translation reuse and placeholder signatures](Shared-Translation-Reuse-and-Placeholder-Signatures/en.md). **Design only.**
