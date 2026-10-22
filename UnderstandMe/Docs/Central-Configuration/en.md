# Central configuration: UnderstandMeSettings, LocalizationSettings and TranslationSettings

Status: design direction (2026-10-22), **not yet implemented**. Keep existing `LocalizationOptions` runtime API compatible while introducing the new façade incrementally.

## One composition root, typed groups

`UnderstandMeSettings` is the host-facing configuration root with `Localization` and `Translation` groups. Modules receive focused, validated effective settings rather than a mutable giant configuration object. Bind from .NET configuration (including environment variables), support manual code setup, validate once at startup, and preserve immutable per-request effective snapshots. Never expose backend credentials in public diagnostics or serialized snapshots.

`LocalizationSettings` owns source language defaults, registered `LocalizationStorageSettings` collection and per-storage source language/`SetValueFromKey` invariant, missing-key registration, placeholder policy, language fallback, and `LocalizationCacheSettings` scoped per application/owner/community and source locale. A single source language default does not override each registered storage's authoritative language. Public shared cache requires strict automatic source entries, authorization, and partitioned source-language identity. Incompatible storage configurations fail validation; explicit-key dictionaries are project isolated.

`TranslationSettings` owns provider/backend registrations with capabilities, allowed language pairs, priorities, endpoints and credential references; routing/fallback policy; `QueueSettings`; retry/admission; translation-pipe profiles; request validation; and `RealtimeSettings` including `UseRealtimeCache`. Human-only deployment is valid and does not activate machine P0 translation. Backend settings cannot silently enable paid services. Credential configuration uses secure providers/environment and never writes secrets to README, logs or cache.

`QueueSettings` describes dynamic queue lanes, P0-P4 scheduling weights/caps and default worker FIFO buffer capacity (initial candidate: five). **P0 cached and uncached are logical lanes of the same P0 priority**, not new global priority classes. Configurable lane counts must not silently redefine P0-P4 priorities; dynamically enrolled lanes require explicit resource and fairness policies. Pending accepted jobs use immutable effective request configuration; administrative pause/stop and current spending caps still apply at dispatch.

`RealtimeSettings` owns opt-in `UseRealtimeCache`, scoped in-memory sliding expiry (proposed 5 minutes), bounds and optional private-app durable promotion only where independently authorized; uncached P0 never stores contents. Both P0 logical lanes may read authorized approved shared dictionary memory. Never auto-publish P0 contents into owner/community memory.

## Configuration levels and ownership

1. Host/admin global constraints and registration (hard upper bounds).
2. Storage-specific source settings and per-application/owner/cache policies.
3. Translation pipe/profile defaults and allowed per-target language overrides.
4. Client/application permissions and quotas.
5. Explicit request preferences, normalized at admission into an immutable effective snapshot.

The request may select `UseRealtimeCache` only within allowed privacy and resource constraints; never change a waiting request's frozen policy when hot-reloading defaults. Preserve current security revocations and spending caps at dispatch.

## Illustrative C# API, not compiled code

```csharp
var settings = new UnderstandMeSettings
{
    Localization = new LocalizationSettings
    {
        Storages = [ /* storage definitions and their own source locales */ ],
        Cache = new LocalizationCacheSettings { /* source-locale partitions and scope */ }
    },
    Translation = new TranslationSettings
    {
        Providers = [ /* LibreTranslate, Human, and optional others */ ],
        Queues = new QueueSettings { DefaultWorkerBufferCapacity = 5 },
        Realtime = new RealtimeSettings { UseRealtimeCache = false }
    }
};
```

Use real strongly typed configuration and collection elements when implementing, not anonymous string dictionaries; publish complete XML docs and validation messages. The names above remain proposals except `SetValueFromKey` and `UseRealtimeCache` which were agreed as concepts.

## Migration and verification

- Keep `LocalizationOptions` working while introducing a host-facing `LocalizationSettings`; define a single normalization/mapping source rather than parallel, diverging policies.
- Explicitly distinguish `SetValueFromKey` strict source equality from the legacy `RegisterMissingSourceKeys` switch. Source mismatch on auto-source storage means conditional deletion of the whole bad entry, never guessing which side is correct.
- Test per-storage default language and strict mode, invalid options, duplicate IDs, unknown providers, human-only operation, source-locale cache partition, tenant isolation, P0 cached/uncached, limits and buffer boundaries, effective snapshot immutability, config reload, secret redaction and concurrent requests.
- Enforce validation at startup/admission and test error paths. XML-document all public types, settings, properties and enums.


## Component-owned settings and precedence (2026-10-22)

Each backend, storage, scheduler, and optional module owns a **strongly typed component settings object**. The central `UnderstandMeSettings` composition root declares which components are registered/enabled, references their identities, and provides common defaults and hard host policies. It must not become a giant bag of provider-specific options.

- `TranslationSettings.Providers`: registered provider configurations with stable `ProviderId`, enabled flag, capabilities, languages, priorities, and per-provider `Authentication`, `Timeouts`, `Retry/Backoff`, `CircuitBreaker`, `Concurrency`, `HealthChecks`, and error/fallback handling. An HTTP LibreTranslate adapter can own API key references, endpoint and HTTP failures; a local AI provider can own model selection, memory and process recovery instead. Modules validate their own specific fields.
- `LocalizationSettings.Storages`: each storage has stable identity, type/connection reference, authoritative source locale, strict `SetValueFromKey`, persistence/refresh settings, read/write capability and local cache participation rules. Storage credentials are secret references rather than plaintext diagnostics.
- `TranslationSettings.Schedulers` and `Queues`: scheduler instances own job definitions, startup delay, interval, misfire behavior and concurrency; QueueMaster owns dispatch lanes/P0–P4 priorities and worker FIFO buffer defaults. They are distinct responsibilities.
- Other components (`Cache`, `Messenger`, `SystemMonitor`, `Editor`, `DataAccess`, `TranslationPipes`) similarly receive their own typed settings and enabled/disabled registrations. An unused module should not require configuration or activation.

### Precedence and lifecycle

1. Host/system hard constraints: security, privacy, maximum cost, permitted provider classes, and global resource budgets.
2. Component-local configuration: authentication, error classification, retry policies, health checks, storage parameters, and local capacity subject to hard constraints.
3. Application/tenant policy and profile/pipe defaults, with explicit permissions.
4. Individual request options, normalized at admission; a request preference never grants authority to an otherwise forbidden provider.

Reject invalid component configuration before activation, including missing credential references, duplicate IDs, invalid retry intervals, impossible timeout/backoff combinations, incompatible language pairs, and contradictory privacy/cache scope. Distinguish **transient** provider failures (bounded retry), **permanent** failures (no blind retry), authentication failures (disable/reconfigure until repaired), throttling/quota (honor Retry-After and budget), and cancellation (do not retry). Error handling must not endlessly enqueue the same translation.

Effective request settings are immutable snapshots. Mutable operational state (healthy/unhealthy, paused, retry budget, circuit open, credential rotated) stays separate from configuration. Hot reload of component settings must be validated and applied atomically to new requests; running work keeps its accepted policy unless revoked authorization or hard limits prevent execution. Never serialize resolved secrets into snapshots, monitoring, logs, or public OpenAPI documentation.

All public settings types, properties, validation results and exception paths require English XML comments. Test precedence, default values, isolated components, auth failures, retry/circuit edge cases, zero/negative settings, concurrent reload, disable/enable, multi-storage source-language isolation, human-only deployment and no secrets in telemetry.

This section records architecture only; no runtime component settings classes exist yet.
