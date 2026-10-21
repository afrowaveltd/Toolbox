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
