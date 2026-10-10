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

## Pairwise key authorization for monitoring links

**Agreed direction:** preserve module isolation, but require every inter-server monitoring connection to be explicitly trusted. Each node has a public `NodeId` (a GUID is suitable for identification) and a **secret access key registered at the receiving endpoint**. Configure secrets **per link/recipient**, not a single reusable secret shared with every server. No central identity service is required.

For example, when VPS3 subscribes to VPS1's outgoing `/SystemMonitor`, VPS1 is the authorization receiver: VPS1 registers the access key presented by the VPS3 SignalR client and grants `monitor.read` for its permitted feeds. VPS2 independently registers a different key for VPS3. If VPS3 later serves an external monitor, that client presents a key registered on VPS3. Unknown, disabled or revoked keys are denied **before** event subscription. Registration is a configuration/admin operation outside the event-only Hub.

Suggested minimal trust record: `NodeId`, human-readable peer name, **hash of its AccessKey**, permitted module scopes and feed(s), enabled/revoked status, and optional creation/expiration time. A generated GUID v4 can serve as a node ID, but use a dedicated cryptographically random 256-bit opaque token for the access secret (e.g. `RandomNumberGenerator.GetBytes(32)`). Treat it as a simple bearer/API key rather than implementing an original cryptographic protocol. Store it securely, never embed it in events, URLs, exception text or logs, compare using safe verification, and provide replacement/revocation. Require TLS for all remote connections.

**Authorize the direction of each connection.** A normal client cannot inject an event into `/SystemMonitor` because the Hub exposes no client-callable `Publish` method. Remote event ingress to a federation happens **only through the explicitly configured outgoing SignalR receiver client**. Authenticate the TLS remote host, bind its connection configuration to the expected peer and approved source-module list, and reject or quarantine events claiming an unexpected source. `SourceModule` or `NodeId` fields within a `Message<T>` envelope are not, by themselves, proof of identity. A relay must be specifically authorized to forward named upstream origins; a trusted but compromised relay otherwise remains capable of lying, so this is connection-level trust rather than end-to-end signed provenance.

In .NET clients, use SignalR's standard `AccessTokenProvider` / `Authorization: Bearer` mechanism to **transport** the opaque secret; a small custom authentication handler can validate the registered key and apply authorization policy to the Hub. No JWT issuance or OAuth provider is needed for this internal API-key arrangement. Browser SignalR clients can transmit tokens in a URL query on certain transports, so an admin browser UI should preferably use its existing authenticated web session and avoid leaking shared node keys in query logs. Server-to-server .NET clients can send the header instead. On key revocation, explicitly terminate the affected active connection or recheck access; normal SignalR authentication does not automatically revoke established connections.

**Fail closed for untrusted peers; fail open for engine availability.** A rejected monitoring connection results in no stream access and an independent diagnostic record, but it never pauses translation, model recovery or other module operations. Security applies identically to root and module-specific monitor routes.

## Verification before implementation

Test that a source event appears once on its own feed and once on the aggregate, never on another module's feed; verify sender identity, absence of listeners, disconnect/slow subscribers, permissions, and optional bridge deduplication.

See [architecture](../Architecture/en.md), [AfroLT](../AfroLT-Future-Design/en.md), [Afutrans integration](../Afutrans-Integration/en.md) and [implementation status](../../IMPLEMENTATION_STATUS.md).
