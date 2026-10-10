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

## Verification before implementation

Test that a source event appears once on its own feed and once on the aggregate, never on another module's feed; verify sender identity, absence of listeners, disconnect/slow subscribers, permissions, and optional bridge deduplication.

See [architecture](../Architecture/en.md), [AfroLT](../AfroLT-Future-Design/en.md), [Afutrans integration](../Afutrans-Integration/en.md) and [implementation status](../../IMPLEMENTATION_STATUS.md).
