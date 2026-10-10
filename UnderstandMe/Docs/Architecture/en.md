# Architecture and Responsibility Boundaries

Status: planned design, 2026-10-10. This file is intentionally about responsibilities rather than a frozen class/folder structure.

## Principle

A library is a collection of independently replaceable workers and interfaces, not one hardcoded Afutrans workflow. Each component owns one concern, declares its inputs/outputs and capabilities, and uses contracts rather than concrete implementations. The composition root of an application chooses the pieces. Avoid circular implementation-project references.

## Core modules

| Module | Owns | Must not own |
| --- | --- | --- |
| QueueMaster | Global priority queues, job ownership, demand-driven dispatch, persistence/recovery contracts | Translation algorithm, UI, source parsing |
| DataAccess | Reading/writing external data and capabilities | Identifying phrases to translate, priority logic |
| TextSource | Format-aware extraction, segment metadata and request preparation | Internal queue implementation |
| Translation workers | Processing an eligible item and returning an outcome | Assignment of other workers, storage routing |
| Chain/steps | Step ordering and compatibility for an individual workflow | Ownership of all system queues |
| ResultProcessor | Structure/placeholder restoration, assembly, result routing | Choosing next translator |
| Editor | Dictionary inspection, manual edits, optional improvement requests | Alternative queue infrastructure |
| Messenger | Event transport and status projection | Commands, dispatch, prioritization |
| Localization | Standard .NET localization, dictionary lookup and provisional cache | Mandatory dependence on Afutrans service |

A human can be a translator worker, but the Editor additionally acts as an independent dictionary-management client. A new provider registers capabilities; QueueMaster does not need enum cases naming every possible community implementation.

## Composition

The host registers DataAccess, format adapters, backend endpoints, result processors, optional Chain steps, history/audit, Messenger transports and UI extensions through interfaces. Translation Targets supply connection and policy profiles. Features can be omitted entirely.

`Translation Pipe` describes policies (persistence, priority, retries, privacy, caching and selection). `Translation Chain` describes the processing steps. They are related, but not identical, and neither requires one physical queue per target/type.

The architecture must also allow asynchronous requests for additional translation work from a chain step or editor, preserving correlation to the originating job while remaining within QueueMaster's public API.

## Hierarchical SystemMonitor

Each module emits events independently. A host maps `/SystemMonitor` for events from all locally registered modules, and `/AfroLT/SystemMonitor`, `/Afutrans/SystemMonitor`, `/UnderstandMe/SystemMonitor` for each module's own events. All use one typed envelope with sender identity. The host fans out a single event to global and module feeds, without Hub-to-Hub forwarding. Remote instances only aggregate through an optional bridge. Monitoring is outbound-only, best-effort, never a storage or command channel. See [SystemMonitor](../SystemMonitor/en.md).

### Optional remote event aggregation

A monitoring host may also load an optional **SignalR client receiver** connected to multiple trusted remote `/SystemMonitor` endpoints. Received events enter the same local event router as self-generated events; its root `/SystemMonitor` combines local and imported monitoring streams, as in VPS1 and VPS2 feeding VPS3. The receiver does not add callable methods to the outbound-only Hub and does not make individual modules depend on one another.

Preserve original event/source IDs and generic message contracts. Federation is explicitly configured and uses bounded nonblocking queues, per-connection reconnect/health status, provenance and loop/deduplication protection. It remains best-effort, never a durable job or incident delivery mechanism. See [SystemMonitor](../SystemMonitor/en.md).

### Simple peer authorization for monitoring links

Modules remain independent: the **monitoring host**, not a core translation module, authenticates SignalR peers. A server-to-server receiver uses a registered `NodeId` plus a different random secret for each receiving server. The recipient checks its local allowlist and feed/module read permissions before allowing a subscription. No central directory or identity service is required. The outbound-only Hub has no `Publish` command: remote events enter federation only through separately configured trusted client receivers. Their claimed event source must match the authorized peer or explicitly permitted relayed origins. Read keys, forwarding permissions and administration rights are distinct. [SystemMonitor security](../SystemMonitor/en.md).

## Design-stage constraints

- No folder layout, package split, DI registrations or interface method signatures are final.
- .NET 10-compatible base contracts and optional .NET 11 compile-time API surfaces are being evaluated.
- Default to `System.Text.Json` where serializing JSON is actually required; do not constrain other formats to JSON representations.
- Register all accepted edits through controlled write/audit boundaries, regardless of UI.
- Afutrans is one host. Its English-key rule is not forced on every UnderstandMe consumer.

See [working notes](../../Notes.md), [QueueMaster](../QueueMaster-and-Reliability/en.md) and [open questions](../Open-Questions/en.md).

