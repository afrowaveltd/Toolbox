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

## Cross-project monitoring convention

A module **emits** events; it does not own a networking Hub. Hosts may optionally register a shared, strictly **outbound** SignalR `/SystemMonitor` endpoint for live monitoring. Every module publishes an identifiable event source; if several modules share one host, a single Hub multiplexes all sources. Separate hosts may use identical `/SystemMonitor` paths on different origins. No client-callable command methods are defined on this Hub; core execution never waits for listeners.

The monitoring emitter is transport-neutral. A server that needs administrative commands serves them separately from `/SystemMonitor`. Persistence/ACK for indispensable data occurs at its authoritative subsystem regardless of event broadcasts. See [AfroLT future design](../AfroLT-Future-Design/en.md).

## Design-stage constraints

- No folder layout, package split, DI registrations or interface method signatures are final.
- .NET 10-compatible base contracts and optional .NET 11 compile-time API surfaces are being evaluated.
- Default to `System.Text.Json` where serializing JSON is actually required; do not constrain other formats to JSON representations.
- Register all accepted edits through controlled write/audit boundaries, regardless of UI.
- Afutrans is one host. Its English-key rule is not forced on every UnderstandMe consumer.

See [working notes](../../Notes.md), [QueueMaster](../QueueMaster-and-Reliability/en.md) and [open questions](../Open-Questions/en.md).

