# Configuration, Change Notifications and Scheduled Dictionary Discovery

Status: **architecture specification only** (2026-10-19). This does not claim a released configuration schema, notifier implementation, Scheduler or database connector.

## Goal and dependency direction

UnderstandMe must support applications with entirely different architectures. Storage, metadata, localization, translation and change discovery use common **logical contracts**, not fixed JSON paths, fixed `appsettings.json`, mandatory web APIs, or one host's scheduling service.

Each **consuming application is the composition root**. It selects the concrete adapters and policies; UnderstandMe does not choose providers for the application. Afutrans is one optional application/translation host, not a required dependency.

## Pluggable configuration

Configuration can arrive from `appsettings.json` / `IConfiguration` (including `IOptions<T>` if the host wants), constructor parameters and typed options, dependency injection, an application's settings database, environment/secrets provider, or a custom configuration-source interface. No component must parse `appsettings.json` directly. A self-contained application can pass options without referencing ASP.NET or a specific configuration package.

Design an optional configuration access/notification interface **only when a concrete second host or changeable source needs it**. A host may resolve an immutable effective options snapshot and pass it to the localizer, accessor, notifier and target scanner. It must define clear precedence for overlapping sources (e.g., explicit override versus file settings), validate incompatible choices at setup, and avoid undocumented merging. Secrets/endpoints/credentials stay with the host's secret provider and do not appear in global events or logs.

Settings must cover *independently*:

- Dictionary + metadata providers, culture and fallback policy, identity/target scope, read-only/writable capabilities.
- Whether changes are detected/emitted at all and which *kinds* of committed changes are notified; no-event mode is a normal configuration.
- Zero or more delivery adapters (in-process event/callback, Messenger, authenticated REST callback, business SignalR, custom integration). No mandatory transport; host can choose none, one or several.
- Optional active/push change detection and optional independent periodic scanning/polling for each Translation Target. Both may be enabled together; one must not suppress the other.
- Per-target scan policy, interval, enablement, startup behavior, credentials/authorization and locale scope. Scheduler details remain owned by a host, not the localizer core.

### Illustrative JSON *for a future host*, not a committed API schema

```json
{
  "UnderstandMe": {
    "Localization": {
      "SourceCulture": "en",
      "DictionaryProvider": "file-json",
      "MetadataProvider": "file-json",
      "Notifications": {
        "Enabled": false,
        "Transports": []
      }
    }
  },
  "Afutrans": {
    "Targets": [
      {
        "Id": "example-sql-target",
        "Discovery": {
          "Enabled": true,
          "Mode": "poll",
          "Interval": "00:15:00"
        }
      }
    ]
  }
}
```

Note: illustrative identifiers such as `file-json`, `poll` and option names are **not** implemented or frozen. The same configuration can be passed as typed C# options or produced by a custom provider; another application may store it in SQL/AJIS or construct it programmatically.

## Two complementary paths for detecting change

**1. Push / change notifications.** An accessor or localizer that knows it successfully committed a new source phrase, translation or metadata revision may report one logical change. Its notifier adapter may distribute that notice through the host-selected transport(s). External storage notifications, watchers and change feeds can also detect out-of-band updates where available. A delivery is *not* the commit and is not a prerequisite for a successful write.

**2. Pull / scheduled discovery.** A separately enabled translation-service **scanner worker** checks an authorized target's current dictionary/metadata through the same provider interfaces. It can operate when a target never emits events, e.g. SQL tables edited by another program, legacy applications or read-only desktop resource bundles exposed through a query API. The scanner is not itself a translation provider worker: it detects differences and safely submits appropriate translation requests to the normal QueueMaster/admission pipeline.

A source supporting revision/ETag/change token should be queried cheaply first, then changed entries retrieved. Where no revision exists, scan and diff the normalized catalog or maintain a safe hash/snapshot as appropriate. Record the last successfully reconciled revision/snapshot, and ensure interrupted scans do not advance the checkpoint ahead of admitted work. A different provider can choose its own efficient comparison strategy. Discover newly added or changed *source* keys distinctly from edited target translations or metadata; never recursively trigger translations of already translated target updates.

**Scheduler integration:** follow Afutrans's already documented scheduling semantics (persist `NextRunAtUtc` for durable scheduled tasks, respect startup delay, measure rest interval from completion, no overlapping executions of the same task). One target scan ends once it has completed its scan **and safely handed off work**, not when long-running translations finish. Different targets can run concurrently; QueueMaster alone owns job priority, admission, work durability and actual translation execution. Poll frequency remains fully configurable per target, including disabled.

## Delivery guarantees and correctness

- Notifications, scans and repeated callbacks can describe the same change. Use a stable source/target/revision identity or change ID and **idempotent work submission**; do not create duplicate jobs from push plus polling.
- On data failure, the localizer keeps the previous good offline snapshot and does not block UI. A failed scan does not mark unseen changes as processed; later scans may retry.
- Notifications may be best-effort (local events or SignalR) while durable source discovery requires checkpoints/outbox/reconciliation. Avoid a false promise that a successful HTTP/SignalR send means a translation job was completed.
- Keep service authorization, tenant data scopes and permissions the same regardless of notification transport or scan strategy. Do not automatically enroll private chats or documents in public UI dictionaries.
- A desktop app with immutable bundled dictionaries may enable neither a publisher nor a scanner. A network-backed desktop app may choose the very same policies as a web app.
- Configure change detection separately from subscription; the originating application decides whether/what to publish, and Afutrans independently decides whether/what to observe or scan.

## Proposed tests for a future implementation

1. Configure through direct options, `IConfiguration` binding and a custom source; confirm identical effective settings for identical input, with documented precedence.
2. Notification disabled: no dispatch attempted, source registration and localization still succeed.
3. Local-event and REST/SignalR adapters produce equivalent normalized change IDs without duplicating a logical change.
4. SQL/in-memory source modified without push: scanner detects the added source key, admits one job and retains a safe checkpoint.
5. Push and periodic scan report one revision: one translation job only; edits to a translated target never cause an infinite translation loop.
6. Offline or failed scan: old catalog remains usable, checkpoint is not advanced, and later scanning can recover.
7. Same target scan cannot overlap itself; separate targets may proceed independently; no scanner assigns its own QueueMaster priority.

**Current implementation baseline:** `ITextLocalizer`, `DictionaryTextLocalizer`, `ILocalizationDictionaryStore` and `FileJsonDictionaryStore`, with a test-only memory provider. No configuration-source interface, notifier, SQL polling worker or network transport has been added in this documentation change.
