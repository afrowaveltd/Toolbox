# Shared cache management, warmup, and usage telemetry

Status: design only (2026-10-21). No runtime cache APIs, persistence, or tests are implemented by this document.

## Separation of concerns
- **Memory cache** holds currently useful completed translations and has a bounded memory budget/eviction policy.
- **Persistent translation memory** stores approved/reusable translations across restarts; evicting RAM must never silently delete durable records.
- **Usage telemetry** aggregates counts rather than storing private message content or user identities.
- **In-flight coordination** coalesces concurrently missing identical, authorized translation requests (single-flight); a cache hit alone does not solve provider stampedes.

## Source-language partition and translation identity
- Keep a distinct source-language cache partition (`cache_en`, `cache_cs`, `cache_ar`, `cache_fr`, etc.). Never mix English-source and Czech-source entries into one partition just because surface text matches.
- Within a partition, distinguish target locale, exact decoded source phrase/key, placeholder signature, provider/policy-relevant context and privacy/tenant-sharing scope. Consider variants and source-revision identity when applicable.
- Sharing is permitted only for strict `SetValueFromKey=true` source storages whose decoded source entries satisfy ordinal key=value, and only where sharing/access policy authorizes it. Invalid auto-source entries must be conditionally deleted from their source dictionary and never admitted to shared memory; the later source lookup can re-register the missing key. Explicit-key dictionaries remain project-isolated even when an occasional key happens to equal its value.
- Canonical locale identifiers must preserve meaningful regional distinctions (e.g. `pt` vs `pt-BR`); define normalization consistently across cache and storage before implementation.

## Proposed management operations
- `GetAllCachedValues()`: inspect permitted partitions and entries, preferably paginated/filtered to avoid giant snapshots.
- `GetCachedValues(string sourceLang)`: inspect one source-language partition; optionally later overload by target locale.
- `FlushAllCache()`, `FlushCache(string sourceLang)`: evict volatile memory entries only; do not destroy persisted dictionaries or request history. Access requires explicit administrative entitlement.
- `AddCachedValue(...)`, `AddCachedValues(...)`, `RemoveCachedValue(...)`: validated single/bulk admission or invalidation; writes obey source-locale partition, privacy, placeholder and current-source conditions. Specify durable promotion separately.
- `PreloadCache(...)`: warm selected authorized dictionaries or high-frequency phrases, with bounded memory and provider-free loading from validated existing stores. A preload must not automatically translate everything.
- `GetCacheStatistics(...)`: occupancy, hits, misses, translation requests avoided, in-flight coalescing, evictions, admission failures and approximate cost savings only if actually measured and labeled.
- For large caches, inspection APIs should prefer pagination/streaming; strict admin authorization, redaction and rate limits.

## Loading modes
- **Lazy**: populate only after actual use.
- **Preload**: import selected existing approved dictionaries at startup.
- **Hybrid**: warm selected frequent entries, then load other entries on demand (proposed default, subject to measurement).
- Preloading is configurable per storage/host. Avoid blocking core startup on an optional cache warmup failure.

## Frequency-driven persistence
- Track successful cache hits/uses by privacy-safe aggregate translation identity; distinguish hits from requests, initial misses and retries. Counting every render must not equate to independent demand.
- Optional threshold/window and decay determine which verified, eligible, nonprivate entries to promote to persistent translation memory. Promotion must recheck source revision, sharing policy and placeholder structure, and be idempotent across workers.
- No private chats, sensitive message bodies or identifying data are automatically elevated into global shared memory. Even a frequent private message remains private unless separately authorized.
- Statistics and memory growth are bounded; counters need concurrency safety and defined overflow/saturation behavior.

## Required implementation verification
- XML docs for all public types, members, params, returns, and exceptions.
- Unit/edge tests: source-language isolation; target-language differentiation; case/region normalization; escaped JSON and literal Unicode; malformed source key=value rejection; tenant/private isolation; preload/lazy/hybrid behavior; missing providers; expired/stale values; repeated flush; flush-vs-read and add-vs-flush concurrency; duplicate batch entries; memory bounds and eviction; counter overflow; single-flight successes/failures/cancellations; durable promotion thresholds and idempotence.
- Integration tests against separately configured LibreTranslate localhost and optional public host without checking API credentials into Git; no live provider calls in ordinary unit tests.


## Application, owner and community cache scopes

Cache sharing is an explicit, independently configurable policy for each registered application. A registered application has an immutable `ApplicationId` and an authenticated `OwnerId` (or tenant identity); neither identifier itself grants access.

Proposed modes:
- `ApplicationPrivate`: cache entries are available only to the registering application, even when other applications have identical source/target text.
- `OwnerShared`: entries may be reused by authorized applications of the same owner, without allowing unrelated owners to read them.
- `CommunityShared`: eligible validated entries may be reused across consenting applications/owners under the community access policy.

A private application can optionally **read** eligible public/community entries without **publishing** its own translations publicly. Distinguish `ReadScopes` from `WriteScope` so opt-in reuse never implies unwanted disclosure. Owner-level sharing must require authenticated owner/tenant membership and authorization; raw user-supplied `ApplicationId` or `OwnerId` strings are not credentials. Distinguish ownership reassignment and revocation from immutable translation provenance; never silently expose prior private entries to a new owner.

Every scope remains partitioned by canonical **source locale**; within each partition, keys include effective scope identity, target locale, validated source phrase, compatible placeholder signature and relevant translation context. `SetValueFromKey=true` source invariant remains necessary for cross-project reuse. Explicit-key dictionaries remain project-scoped even when owner or community sharing is enabled for other eligible dictionaries.

All management operations (`GetCachedValues`, `FlushCache`, `PreloadCache`, bulk adds, statistics, persistent promotion) must authorize their scope and be unambiguous about whether they affect application, owner or community entries. A scope-local flush must not evict another owner's entries. Admin global flush requires elevated permission. Usage counters are scoped and opt-in, and never leak another application's private phrases through aggregated views.

Required edge tests: two apps of the same owner sharing under `OwnerShared`; unrelated owners denied; app-private isolation; community opt-in and read-only reuse; source locale segregation; forged application/owner IDs rejected; owner transfer/revocation; concurrent writes and flushes across scopes; unauthorized statistics and enumeration blocked; fallback behavior when a shared entry is missing or ineligible.

This section is a design contract only, not an implemented identity, authentication or caching API.


## P0 RealTime private content exclusion (security decision)

- **P0 RealTime request contents and translated results must never be automatically inserted into application, owner or community translation caches, persistent translation memory, frequently-used phrase tables, or source-text-level usage telemetry.** This is a hard admission rule above the caller's preferred cache write scope.
- In-memory request-local state may exist only for necessary active processing and must be discarded on completion/cancellation/expiry. No persistent in-flight payloads or automatic promotion on popularity thresholds.
- Aggregate operational metrics (counts, latency, faults) may be collected without request text, translations, recipient identity or content-derived keys. Avoid logging sensitive payloads, including exception messages and diagnostics.
- An authorized consuming chat application may separately retain the completed translation in its own access-controlled conversation history subject to its own policy; that is not a shared translation cache or P0 queue persistence.
- Optional **read-only** lookup of pre-existing explicitly shareable dictionary translations can be permitted under policy, but P0 lookups must not create entries, increment phrase-level popularity statistics, expose lookup logs, or promote the content. Strict privacy-sensitive deployments may turn even reads off.
- Cache APIs must reject P0 write/preload/promotion requests rather than relying on application self-reporting of whether content is private. Enforce against all cache scopes and all transports, including direct .NET, REST, SignalR, and batch ingestion; use trusted request classification.
- Tests: P0 cannot write to any cache scope; P0 cannot update content-specific counters; P0 cancellation/exception cannot leave durable content; P0 read-only access does not mutate or reveal lookup keys; request class spoofing does not bypass policy; mixed batches reject or isolate P0 items; other non-P0 durable translation workflows remain operational.

Design decision only: no cache runtime enforcement is implemented yet.

## Confirmed P0 cache direction (2026-10-21)

- P0 RealTime **may read** existing, authorized, validated shared dictionary cache entries to avoid redundant provider calls; this is the supported normal behavior, not a forced cache bypass.
- P0 **must never write** request content or results into any engine cache (application-private, owner-shared, community-shared), persistent translation memory, or phrase-level access statistics. No automatic insertion, promotion or warming caused by P0 misses.
- P0 read lookups are side-effect-free with respect to entry storage and per-phrase statistics; aggregate content-free operational counters remain separate. Reads enforce cache scope permissions and source/target identity. A miss proceeds through transient translation without cache write-back.
- Test P0 hit/miss, all cache scopes, blocked writes and bulk routes, zero per-entry hit-count changes, and permission checks. Implementation pending.


## Proposed P0 performance refinement: isolated ephemeral memoization (2026-10-21)

Repeated real-time phrases (e.g. `Hello` in a group chat) should not trigger the same provider work repeatedly. This **proposal refines** the earlier blanket no-cache language without weakening the prohibition on exporting private content to shared or durable translation memory.

- P0 may read existing authorized dictionary/shared cache entries as already agreed.
- On a miss, optionally coalesce in-flight equivalent requests and keep a bounded, short-TTL **conversation/tenant-scoped transient memo** in process memory; only authorized readers within that scope can reuse it. Never expose its contents via global cache enumeration, statistics, logs, or debug endpoints.
- An optional hit threshold in the same isolated session can govern whether to retain a transient memo at all; do not require repeated expensive translations before single-flight deduplication. Use keyed request identity including source/target language and appropriate translation settings.
- **Frequency does not confer publication consent**: no P0 text or translated result is promoted to community, owner-shared, or durable memory merely due to repetition, even 30+ occurrences.
- Durable frequent-phrase catalogs must be populated from explicitly approved, nonprivate sources (e.g. curated greeting dictionaries) or independently consented workflows, not mined from P0 conversation content. Such entries may be read by P0.
- Expire and clear transient payloads on TTL, session termination, revocation, and resource pressure. Do not persist transient content across restart or leak cross-chat. A durable application-owned authorized chat history is a separate system, not translation cache.
- Tests needed: 30 identical messages coalesce/reuse without 30 provider requests; concurrent hits; two private chats cannot read each other's memo; expiry/revocation; no P0 durable writes or phrase telemetry; cache scope authorization; sensitive repeated text never automatically promoted.

**Status: proposed design refinement, not yet implemented or finalized as an API contract.**


## P0 RealTime logical lanes and UseRealtimeCache (2026-10-21)

- Adopt a request-level flag named `UseRealtimeCache`, resolved under effective host/application policy during request admission. Two **logical P0 lanes** share one P0 priority, scheduling budget and worker pool: `P0/Cached` for eligible short-lived reuse, and `P0/Uncached` for no request-content retention. This is not an extra sixth priority or a separate unbounded worker queue.
- `UseRealtimeCache=false`: P0 may still read existing authorized, pre-approved dictionary translation memory, as previously agreed, but it must not save the request/result into any P0 memo. Define a separate cache-read policy if hosts require disabling even pre-approved reads.
- `UseRealtimeCache=true`: P0 may use bounded TTL transient memoization **only within an authenticated and authorized isolation scope** (e.g. conversation/session); its content cannot be promoted to owner/community caches or persisted automatically. Same-language/text equivalence alone never permits cross-conversation reuse. Coalesce simultaneous equivalent work within the same permitted scope, including no-cache in-flight deduplication when it does not retain results afterward.
- A caller cannot force caching of private content outside the server's allowed scope. The effective flag/policy is set on admission and must be propagated to every translation stage; no downgrade from uncached to cached via fallback, retry, event handlers, or provider adapter.
- Never collect P0 phrase-frequency data for persistent translation memory by either lane. Aggregate content-free operational counters are acceptable. Repeated `Hello` benefits from temporary scope-local reuse when opted in, but repetition cannot authorize community publication.
- The future API needs XML comments for `UseRealtimeCache` and clear names for scope/TTL/limits; tests must cover true/false, policy override, scoped hit/miss, eviction, burst traffic fairness, in-flight request merging, cancellation, fallback, and absence of durable/shared writes. No actual scheduler/queue implementation exists yet.

Design update only. 


## P0 transient cache sliding inactivity expiration (2026-10-21)

- For requests admitted to the authorized `UseRealtimeCache=true` logical P0 lane, use configurable **sliding expiration** rather than automatically persisting repeated chat phrases. Proposed baseline `RealtimeCacheSlidingExpiration = TimeSpan.FromMinutes(5)`; actual TTL is host-policy bounded.
- Start the inactivity deadline when a successful translation is inserted. Each authorized cache hit within the same isolation scope resets the entry's expiration to `now + TTL`. If no hit occurs for five minutes, treat the entry as expired and discard it. Expired content must never be returned; physical cleanup can be lazy on access plus periodic sweeping.
- Keep source locale, target locale, content/placeholder context, policy and authorized conversation/session scope in cache identity. Repeat requests from unrelated conversations must not revive each other's entries. The setting cannot authorize global/shared publication or disk persistence, and neither content nor per-phrase popularity is logged.
- Bound total memory, entry size and item count independently of the inactivity timeout. Under memory pressure, evict earlier even if recently used; a five-minute sliding TTL is not a minimum retention guarantee. Clear on session revocation/closure, provider safety invalidation and application shutdown.
- A cache miss can use single-flight in-flight coalescing before storing a successful result; failures must not poison the cache. For `UseRealtimeCache=false`, never insert into or refresh this transient cache.
- Required tests: hit at 4m59s extends lifetime; miss after 5m inactivity; exact expiry boundary; repeated active hits; concurrent hit and eviction; race at expiry; fake clock rather than sleep; same phrase isolated by conversation and target; memory-pressure eviction; cancellation/failure not cached; no persistent/public promotion or content-level stats.

**Design agreed; implementation and tests pending.**


## Configurable promotion threshold for P0 transient reuse (2026-10-21)

- Proposed configuration property `PermanentlyStoreAfterCalls` (integer, default `int.MaxValue`) controls the number of eligible successful uses of a transient P0 entry within its sliding-expiration lifetime before evaluating persistence. A threshold of 50 means the entry is considered on its 50th use without expiration; an expired/evicted entry loses its counter and must start over. Explicit authorized manual persistence is independent of this threshold.
- **Frequency is not authorization**: a threshold can trigger persistence only when the application's trusted policy explicitly permits durable storage of that content **and** the destination scope (application/private, owner, or community) has independently valid access/publication consent. A private chat must not be made community-readable simply because its contents recur.
- `int.MaxValue` is a disabled sentinel (do not promote automatically even if the counter saturates); reject zero and negative thresholds, and use saturating counters. Clarify whether duplicate rendering or an independent request constitutes a 'call' before API implementation. P0 ephemeral use counters are request-local/scope-local, never phrase popularity telemetry or exported diagnostics.
- Promotion must revalidate source/target and privacy policy, enforce atomic idempotent writes, and never run automatically when P0 durable-content policy is disabled. Sliding TTL may still reset independently of the threshold.
- Tests: 49/50/51 uses, expiry before threshold, saturation, default sentinel, invalid thresholds, concurrency at threshold, revoked consent, private vs community destination and explicitly authorized manual persist.

Design only; no implementation or tests added.


## RealTime durable application cache and minimal administrative visibility (2026-10-21)

- Realtime threshold-based persistence is eligible **only for the owning application's private, access-controlled durable cache**, never automatically for `OwnerShared` or `CommunityShared`. Keep the default `PermanentlyStoreAfterCalls=int.MaxValue` sentinel to disable automatic promotion. Explicit authorized opt-in remains required for application-private persistence.
- An application's private cache is not a public API surface. Requests must be bound to authenticated application membership and a trusted server-side scope, with explicit role and operation policies. Caller-supplied `ApplicationId` alone is not authorization.
- Separate **cache maintenance** from **content disclosure**: `Owner` and `Administrator` roles can be granted narrow invalidation of an exact entry (delete by exact key/locale/scope or opaque handle) without automatically receiving broad content-read or enumeration permissions. Prefer opaque handles and no content return for maintenance operations.
- Normal production deployments **do not expose cache enumeration** (`GetAllCachedValues` and unrestricted `GetCachedValues`) through external APIs. Full-dump diagnostics may be exposed only in explicit development/debug deployments with independently authenticated, authorized, audited access, never merely by a caller toggling a `Debug=true` argument. Scrub private content from logs and monitoring.
- **Knowledge of the exact key is not permission to retrieve private content.** Exact-key reads require application authorization and, for private chat records, the original conversation/participant authorization or a narrower approved scope. Resist guessing/enumeration and cross-tenant probing by denying unauthorized lookups without revealing whether a key exists; apply rate limits and response shaping.
- Promotion and administrative removal must be atomic, scoped, auditable and safe under concurrent writes; deletion cannot cross an application boundary, and removal must not expose the removed value.
- Keep ordinary P0 transient cache and any app-private persistent cache separate from the chat application's own durable conversation history. Apply content lifetime and deletion requirements independent of cache TTL.

Required tests: unauthorized exact-key reads; guessed keys; cross-app and cross-conversation isolation; Owner/Administrator delete-without-read; deleted-key concurrency and audit; production enumeration forbidden; debug-mode server-side enforcement; role revocation; community-promotion prohibition; threshold persistence scoped to one application; no data in logs/errors; timing and presence-oracle checks.

Design only; runtime authorization and APIs are not yet implemented.
