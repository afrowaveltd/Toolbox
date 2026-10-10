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

## Optional remote SystemMonitor receivers and federation

**New design direction:** the monitoring orchestrator can act as a **SignalR client** as well as a publisher. It can connect to any number of explicitly configured remote `/SystemMonitor` endpoints and merge their live events with its own. This feature is optional: AfroLT, UnderstandMe and Afutrans remain usable without a receiving client, and no upstream system needs to know that it is being monitored.

Example topology:

- VPS1 broadcasts its own modules at `https://vps1.example/SystemMonitor`.
- VPS2 broadcasts its own modules at `https://vps2.example/SystemMonitor`.
- VPS3 hosts the aggregate and runs two outgoing SignalR client connections, one to each remote. Its own publisher and both remote receivers feed the same local event router.
- Administrators subscribe only to VPS3's `/SystemMonitor` to see all three machines in one stream. Module-scoped routes can continue filtering by `SourceModule` when remote events are admitted.

The **receiver** is an optional host-side adapter (working name `ISystemMonitorReceiver`). It subscribes to the remote hub's server-to-client event callback through a SignalR client connection and passes the event to the same local router/publisher interface used by local modules. The aggregate Hub remains **outbound-only**: inbound remote traffic arrives at the receiver client, not through new methods exposed on the monitoring Hub. This is simply event-driven routing, not a mandatory central collector.

### Safe federation rules

1. Preserve the **original** `EventId`, `SourceModule`, `InstanceId`, `ContractId`, `SchemaVersion`, timestamp and typed `Message<T>` payload. A forwarding node must not impersonate the original sender, allocate a fresh business event ID, or silently change the payload type.
2. Maintain optional separate relay/transport metadata (e.g. `ReceivedFrom`, `RelayPath` or bounded `HopCount`) for diagnostics and loop prevention. Do not overload the original module sender field with the relay identity.
3. Prevent retransmission loops, particularly with reciprocal or chained bridges, by checking the local host identity/relay path, bounding hops, and suppressing recently seen `(origin, EventId)` pairs using a **bounded, expiring in-memory cache**. This cache is not an event archive.
4. A remote observer can forward known typed report models via the registered contract serializer; unknown future `Message<T>` contracts may be transparently passed through as compatible serialized wire payloads or skipped with a local diagnostic. Never instantiate arbitrary CLR types advertised by remote peers.
5. Configure each remote endpoint, credentials, access permissions, permitted modules and event filters explicitly. Only connect to trusted hosts; do not expose private per-user translation data merely because monitors are federated. Avoid duplicate subscriptions to an aggregate and its module feeds unless intentionally filtered/deduplicated.
6. Keep **one bounded receive path per configured connection**, independent of core work. Disconnections do not stop the local engine or other receivers. The remote connection has its own health indicator and reconnect policy, including recovery after the default finite SignalR retry schedule. No replay, catch-up or guaranteed delivery is implied.
7. If VPS1 becomes unavailable, VPS3 continues broadcasting its own and VPS2's events. A disconnected source is visibly marked unavailable through local connection-health status; its past monitoring messages remain lost unless they were independently committed elsewhere.

This is a **fan-in of best-effort monitoring streams**. It does not combine databases, ownership of queued jobs, persistent audit histories or administrative permissions. It does not make unrelated module dependencies mandatory. Tests must cover a one-hop VPS1/VPS2 to VPS3 aggregation, disconnect/reconnect, source identity, duplicate forwarding, loops and mixed known/unknown typed payloads.

## Lite peer identification at monitoring connection time

**Scope:** this is a low-friction **known-node registration and event-source filtering** mechanism for anonymous, read-only monitoring. It is not general application authentication or protection for sensitive data. The primary risk addressed is polluted monitoring output from an unintended or misconfigured source. No central authority, OAuth/JWT, per-link 256-bit secrets, permission bureaucracy or persistent message protocol is required for the agreed basic mode.

Each monitor node has a stable `NodeId` (GUID) and descriptive identification metadata. The monitoring receiver maintains a small set of known `NodeId` values, expected endpoint addresses and optionally allowed modules or typed event contracts. When a monitoring connection starts, peers exchange lightweight connection identification as appropriate to their roles:

- The **connecting client** can present its GUID during SignalR connection establishment when the serving host needs a simple client allowlist.
- The **source server** announces a typed `MonitorHello` at the start of the event subscription, containing its `NodeId`, instance/display name, the module sources it publishes, supported `ContractId`/`SchemaVersion` identifiers and protocol version.
- A receiver such as VPS3 compares the received identification against its **configured remote URL and registered expected node** before forwarding subsequent `MonitorEvent<T>` messages to the aggregate stream. Events from unregistered or unexpected sources are ignored (or the connection closed); a source must not silently claim to be another node. For deliberately configured relay servers, the allowed downstream sources are explicitly listed.

Conceptual first-message payload:

```json
{
  "nodeId": "88969d54-4d28-43da-872a-f86852d88f3d",
  "name": "Afrowave VPS1",
  "modules": ["AfroLT", "UnderstandMe"],
  "contracts": ["afrolt.model-loaded.v1", "understandme.queue-status.v1"]
}
```

This identification exchange is **separate from business event messages** and is not a client-callable `Publish` method on the outbound-only SystemMonitor Hub. The serving host can read connection metadata from the initial HTTP/SignalR negotiation, and can send the server-to-client `MonitorHello` after connecting. Ordinary SignalR transport/protocol negotiation alone does not provide a custom node manifest, so we define it in our monitoring contract.

**Limits of this lite mechanism:** knowing or copying a GUID can enable impersonation. A GUID proves neither ownership nor cryptographic origin. The safety assumption is a deliberately narrow, non-sensitive, one-way monitoring environment with trusted, explicitly configured HTTPS endpoints and standard TLS server certificate validation. A malicious party cannot inject events into the outbound-only Hub simply by knowing the event format; the actual risk is a misconfigured/compromised source endpoint or an untrusted receiver bridge. If genuinely hostile-network guarantees become necessary later, an **optional stronger authentication mechanism** can be added without changing the typed event model or the basic GUID-based discovery convention.

In-process module listeners need no network identity check. When a remote connection fails or its NodeId is unknown, only that monitoring source disappears: translation, queued work, recovery and other monitors continue unaffected.

## Verification before implementation

Test that a source event appears once on its own feed and once on the aggregate, never on another module's feed; verify sender identity, absence of listeners, disconnect/slow subscribers, permissions, and optional bridge deduplication.

See [architecture](../Architecture/en.md), [AfroLT](../AfroLT-Future-Design/en.md), [Afutrans integration](../Afutrans-Integration/en.md) and [implementation status](../../IMPLEMENTATION_STATUS.md).
