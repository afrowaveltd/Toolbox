# Central configuration: UnderstandMeSettings, LocalizationSettings and TranslationSettings

Status: **partially implemented** (2026-10-22). Typed root settings, offline validation, strict JSON file loading/saving and initial component descriptors are implemented. Runtime adapter activation, queue scheduling and TUI remain design work. Existing `LocalizationOptions` remains compatible.

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

Component settings models and descriptor validation are now available in `UnderstandMe/Settings`. Runtime provider adapters, scheduler integration and TUI remain unimplemented. See [Settings Descriptors](../Settings-Descriptors/en.md).


## JSON configuration persistence and in-code documentation (2026-10-22)

- Default host configuration file: `UnderstandMeSettings.json`. Use standard UTF-8 JSON serialized with `System.Text.Json`, rooted in the typed `UnderstandMeSettings` configuration facade; Afutrans should consume the same settings contracts rather than implementing a parallel configuration model.
- Include explicit `SchemaVersion` for versioned upgrades. Define migration and unknown-property policy before enabling persistent writes; on unsupported newer versions fail safely rather than overwriting settings with defaults. Preserve extension sections through a deliberately designed extension mechanism, not accidental lossy deserialization.
- Loading: parse, migrate supported versions, validate global and individual component options, and only then activate a whole coherent immutable settings snapshot. Errors retain last known-good runtime config; missing initial file may produce clearly documented defaults but must not silently enable external services.
- Saving: serialize validated settings to a temporary file in the same filesystem, flush, then perform an atomic replacement where supported. Account for platform-dependent replacement semantics and crashes; a backup/rollback strategy and read-modify-write/concurrent-update protection (revision or compare-and-swap) are required for multi-writer scenarios. Avoid advertising atomicity across remote/network filesystems without a proven guarantee.
- Secrets: never serialize live API tokens, passwords or decrypted connection credentials; store secret references resolved through environment/host secret facilities at runtime. Sanitize logs, failures and any configuration export. Permission-limit the settings file where appropriate.
- Optional IConfiguration integration should coexist with direct file load/save; define authoritative source and precedence so environment overrides are not inadvertently persisted or secrets leaked.
- JSON itself remains standards-compliant and comment-free; code comments are in C# sources. If comments in user-edited config are desired later, consider a separately documented JSONC input mode rather than writing nonstandard comments into the canonical file.
- Code documentation policy: English XML documentation for every exposed settings type, public field/property, method, enum member, parameter/return/exception as relevant; add concise ordinary `//` comments around non-obvious internal behavior or important configuration decisions (e.g. why a queue buffer capacity is bounded), not redundant narration of self-explanatory statements. Keep examples, README and topic documentation consistent.
- Tests when implemented: missing/empty/corrupt file, UTF-8 and JSON escape equivalence, duplicate IDs, unknown/new schema, migration rollback, save interruption, read-only directory, concurrent writers, invalid module configuration, default security behavior, secret redaction, reload while jobs run and per-provider option serialization.

This section specifies architecture and documentation standards only. No JSON settings load/save engine or settings models were implemented in this change.
