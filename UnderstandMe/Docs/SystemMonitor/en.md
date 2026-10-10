# SystemMonitor: Hierarchical Live Event Streams

Status: architecture proposal, 2026-10-10; no runtime code has been implemented.

## Routing

Every application host may expose:

| SignalR route | Events |
| --- | --- |
| `/SystemMonitor` | Aggregate stream: all modules registered in this host |
| `/AfroLT/SystemMonitor` | Events from AfroLT only |
| `/Afutrans/SystemMonitor` | Events from Afutrans only |
| `/UnderstandMe/SystemMonitor` | Events from UnderstandMe only |
| `/{ModuleName}/SystemMonitor` | Standard convention for other modules |

A standalone AfroLT server may expose both `/AfroLT/SystemMonitor` and `/SystemMonitor`, containing the same local events. When modules share one web host, it exposes one global aggregate and an independent module route for each registered source. Separate servers are not connected automatically: cross-host aggregation requires an explicit optional bridge, authentication and loop/duplicate protection.

## One publisher, multiple output feeds

A module emits one event through a transport-independent event publisher; the host fan-outs the event to its specific monitor and the global aggregate. **Do not forward Hub-to-Hub.** Modules do not depend on SignalR, UnderstandMe or Afutrans just to emit events.

All feeds serialize the **same typed event envelope**, with an explicit sender identity. Proposed fields (names not yet frozen): `EventId`, `SourceModule`, `InstanceId`, `Timestamp`, `EventName`, `Severity`, optional `CorrelationId`, diagnostic code and privacy-safe details. The aggregated feed must identify the sender; retaining the same fields on specific feeds is simpler than maintaining two wire schemas. A client subscribed to both feeds may encounter the same event twice and deduplicate using `EventId`; no event should be duplicated within one feed.

## FM-radio delivery rules

`SystemMonitor` is a **server-to-client** stream, with no client-callable business methods, scheduling, durable storage, replay or delivery acknowledgment. Absent listeners, missed messages and disconnected monitoring clients have no impact on core work. Distribute with bounded, nonblocking buffers so slow subscribers cannot obstruct translation.

Information that must survive (committed jobs, authoritative results, error/incident records and edit audits) must be stored and acknowledged by its owning subsystem; a best-effort SignalR notification does not replace persistence. Status snapshots and historical queries belong to separately authorized read APIs. Administrative commands use their own authorized endpoints, not SystemMonitor. Keep user text, credentials and sensitive diagnostics out of default event broadcasts.

## Typed event listeners and generic Message<T>

An event is published **once** by its module through a typed in-process event emitter. Two independent consumers listen for that event: the module-specific SignalR transmitter and the system-wide monitoring router/transmitter. Thus the module feed and /SystemMonitor receive the same logical event without chaining SignalR Hubs. All event listeners are passive with respect to the originating operation: a slow, disconnected or failing listener cannot alter it.

Replace the previous plain-string Message field with a **typed generic payload**: Message<T>. Different kinds of operational events can therefore carry different typed models rather than forcing every module into one generic text string. Suggested illustrative C# shape (not final API):

```csharp
public sealed record MonitorEvent<T>(
    Guid EventId,
    string SourceModule,
    string InstanceId,
    string EventName,
    string ContractId,
    int SchemaVersion,
    DateTimeOffset Timestamp,
    T Message);
```

For example, AfroLT can publish MonitorEvent<ModelLoadedInfo>, UnderstandMe can publish MonitorEvent<QueueStatusInfo>, and Afutrans can publish MonitorEvent<TranslationProgressInfo>. Business-specific report models are authored and versioned by their owning module, while all monitor feeds reuse the same outer envelope and event delivery convention.

Strong typing is preserved **inside .NET**. For remote SignalR receivers, the envelope needs a stable, language-neutral ContractId (for example "afrolt.model-loaded") and SchemaVersion so the client can select the correct deserializer for the actual T. Do not rely on CLR assembly-qualified type names or instantiate arbitrary types supplied by clients. Maintain an opt-in registry/serializer contract for supported payload models. Unknown message contracts may be displayed as raw compatible JSON data by a monitoring tool or ignored without affecting the engine. Do not treat a type discriminator as permission to deserialize arbitrary application types.

The event-router abstraction must not require every payload to derive from an all-purpose base class or reduce it to string/object before publication. The SignalR transport may serialize the resulting generic event with System.Text.Json. Any polymorphism or open-generic behavior must be tested, including .NET 10/11 and non-.NET receivers. Sender metadata, type identifiers and payload privacy rules apply to both root and module-scoped feeds.

## Verification before implementation

Test that a source event appears once on its own feed and once on the aggregate, never on another module's feed; verify sender identity, absence of listeners, disconnect/slow subscribers, permissions, and optional bridge deduplication.

See [architecture](../Architecture/en.md), [AfroLT](../AfroLT-Future-Design/en.md), [Afutrans integration](../Afutrans-Integration/en.md) and [implementation status](../../IMPLEMENTATION_STATUS.md).
