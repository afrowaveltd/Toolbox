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
