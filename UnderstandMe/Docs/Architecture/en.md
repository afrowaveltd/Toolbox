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

## Provider routing is not queue priority

Keep the **translator provider selection policy** outside the QueueMaster's five job-priority queues. The QueueMaster retains ownership of job scheduling and atomic dispatch; a Pipe-level selector uses provider/endpoint capability declarations, configured order, per-request constraints and cost caps to determine compatible candidates and permitted escalation. Neither a worker adapter nor the global queue hard-codes LibreTranslate, Google, Microsoft or AfroLT as a mandatory dependency.

One installation may prioritize local LibreTranslate and escalate only when necessary; another can prioritize a paid provider and use the others as fallback. Selection is configurable globally, by Translation Target/Chain step and, if allowed, for an individual request. Strict LocalOnly, formats/languages and spending constraints are not waived by overrides. Validation and review remain explicit Chain/ResultProcessor responsibilities. [Provider routing](../Provider-Routing/en.md).

## Hierarchical SystemMonitor

Each module emits events independently. A host maps `/SystemMonitor` for events from all locally registered modules, and `/AfroLT/SystemMonitor`, `/Afutrans/SystemMonitor`, `/UnderstandMe/SystemMonitor` for each module's own events. All use one typed envelope with sender identity. The host fans out a single event to global and module feeds, without Hub-to-Hub forwarding. Remote instances only aggregate through an optional bridge. Monitoring is outbound-only, best-effort, never a storage or command channel. See [SystemMonitor](../SystemMonitor/en.md).

### Optional remote event aggregation

A monitoring host may also load an optional **SignalR client receiver** connected to multiple trusted remote `/SystemMonitor` endpoints. Received events enter the same local event router as self-generated events; its root `/SystemMonitor` combines local and imported monitoring streams, as in VPS1 and VPS2 feeding VPS3. The receiver does not add callable methods to the outbound-only Hub and does not make individual modules depend on one another.

Preserve original event/source IDs and generic message contracts. Federation is explicitly configured and uses bounded nonblocking queues, per-connection reconnect/health status, provenance and loop/deduplication protection. It remains best-effort, never a durable job or incident delivery mechanism. See [SystemMonitor](../SystemMonitor/en.md).

### Lite SystemMonitor peer identification

SystemMonitor's basic pairing is **identification/filtering**, not full application authentication. Each node has a known GUID `NodeId`. Remote receiver configurations list the expected node and its HTTPS endpoint; the source announces its identity, offered module sources and typed event contracts in an initial `MonitorHello`. An unknown or mismatched node is ignored, and in-process listeners require no credentials. A connecting monitor client may identify itself by GUID during connection establishment if the serving host uses a client allowlist.

No default shared high-entropy key, OAuth/JWT provider or central identity service is needed in this limited anonymous, outbound-only monitoring scenario. GUIDs are not secrets and do not cryptographically authenticate a source; trusted endpoint configuration and normal TLS certificate checks remain important. The monitoring Hub has no client-callable publishing function. Further authorization can be an optional extension if the threat model changes. See [SystemMonitor identification](../SystemMonitor/en.md).

### Pending monitor registrations and administrator approval

An unknown monitoring node may request enrollment through a separate small HTTPS control endpoint and identify itself by public `ClientId` GUID, its remote endpoint and offered modules. **Enrollment is never automatic.** The receiving monitor host persists the pending request, then optionally broadcasts a typed `MonitorRegistrationRequested` event. An administrator can approve/deny immediately if listening or later from the stored requests, using a separate authorized API/UI. On approval the recipient issues a **new private random `ClientKey` GUID**, binds it to `ClientId` and permitted sources, and makes it retrievable only by the original applicant through a separate enrollment claim channel. The approved client can subsequently present both IDs in HTTPS control request headers.

The event stream must not carry this private key; SignalR monitoring remains purely outbound and best-effort. Enabling a source after approval uses an independently configured receiver SignalR client and normal HTTPS source checks. The issued credential secures recipient-side control/enrollment and is not automatically authentication for the reverse-direction signal stream. See [SystemMonitor enrollment](../SystemMonitor/en.md).

### Risk-triggered challenge for recognized monitor nodes

An already approved monitor client is normally recognized by its public `ClientId` and familiar connection metadata (expected host, observed network profile and declared sources); its private, recipient-issued `ClientKey` **need not be sent on every routine monitoring reconnection**. If the peer's network/source/declared modules change unexpectedly, the recipient requests the private GUID through a separate HTTPS connection-verification step and admits the new connection only after a match. An existing live SignalR connection retains its associated node identity.

This is a **lightweight familiarity heuristic**, not identity proof: headers and IP addresses can be imitated, and NAT/proxy addresses can be shared or change. Installations needing stronger guarantees can challenge each new connection. The SystemMonitor Hub remains outgoing-only; key challenges and any other control exchanges happen outside its application methods. [Adaptive SystemMonitor challenge](../SystemMonitor/en.md).

### Administrator-driven monitoring filters and incident handling

The aggregate monitoring dashboard offers clear independent interactive view filters by source/VPS, module/instance, severity, event type, correlation/job ID and text, with saved presets. A user's display filter never changes which module is trusted or which other monitor clients can see events.

The router checks incoming event validity separately. On a suspicious or malformed event, it **records a durable, safe incident and drops that event** without changing the sender's registration. Only a human administrator can later mute a source in their view, manually revoke/block a registered origin or stop a managed receiver. Repeated incidents can be counted/deduplicated to prevent logging floods. Legitimate new enrollment requests are kept as Pending, not automatically classified as malicious. See [SystemMonitor design](../SystemMonitor/en.md).

## Design-stage constraints

- No folder layout, package split, DI registrations or interface method signatures are final.
- .NET 10-compatible base contracts and optional .NET 11 compile-time API surfaces are being evaluated.
- Default to `System.Text.Json` where serializing JSON is actually required; do not constrain other formats to JSON representations.
- Register all accepted edits through controlled write/audit boundaries, regardless of UI.
- Afutrans is one host. Its English-key rule is not forced on every UnderstandMe consumer.

See [working notes](../../Notes.md), [QueueMaster](../QueueMaster-and-Reliability/en.md) and [open questions](../Open-Questions/en.md).

