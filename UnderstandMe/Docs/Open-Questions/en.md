# Open Design Questions (Not Yet Decided)

This is a discussion backlog, **not** an implementation plan. Decisions recorded in [Notes](../../Notes.md) should not be silently replaced while implementing a new feature.

1. **Precise contracts and layering:** interface method shapes, assembly/package boundaries and DI ownership; no circular implementation dependencies.
2. **Multi-targeting:** decide whether UnderstandMe ships one `net10.0;net11.0` project, a stable net10 core plus net11 adapter, or both. Native C# union facades must not break .NET 10 plugin consumers. Determine WhenItFails dependency baseline after inspecting release/package compatibility; the current project file still declares 0.1.0.
3. **Job and result schemas:** object identity, source metadata, chunk tracking, multiple alternative results, selected result, lineage and idempotent completion.
4. **Atomic QueueMaster behavior:** ownership/lease rules for buffer refill, endpoint death, retries, buffer hand-off, fairness across clients, and starvation prevention without breaking small FIFO buffers.
5. **Synchronous localizer versus live async translation:** explicit API and timeout design, avoiding unbounded waits.
6. **Placeholder protection:** per-backend/language validation experiments for HTML, tokens, difference-based restoration, AI repair; escaping and literal braces.
7. **Storage/retention:** EF Core provider and migration strategy, history deletion guarantees, audit-retention limits, backup/recovery and confidential-content policies.
8. **Chain configuration:** schema, composition UI, compatibility checks, versioning of prompts and profiles, multi-step results and optional branches.
9. **Human queues:** long-run reservations, disconnection, reassignment, and manual correction permissions.
10. **Messenger and security:** durable event vs telemetry guarantees, authorization for per-client SignalR subscription, public aggregate privacy, replay and acknowledgment.
11. **Source format plugins:** model detection for flat/structured JSON and SQL; conflict-safe writeback, allowed filesystem roots, extensibility for AJIS.
12. **Verification before coding:** define unit/integration tests for queue transitions, exact placeholder multiplicity, recovery, editing audits, optional AI checks and .NET 10/11 compatibility.

13. **Native translation feasibility:** prototype .NET-to-CTranslate2 interop; measure baseline Argos/LibreTranslate versus a constrained native worker on a small VPS; test model loading, tokenization, segmentation, concurrency, crashes, independent process restart and failover.
14. **Attribution and licenses:** resolve the individual Argos model-package license/provenance/redistribution question, required third-party notices, and which improvements to propose upstream to LibreTranslate/Argos/CTranslate2.

15. **AfroLT service design:** keep LibreTranslate for HTML/placeholder workflows; decide the independently implemented service's HTTP compatibility surface, capability advertisement, default HTML fallback, and how model health/restart are exposed.
16. **AfroLT WebUI and OpenAPI:** define externally sourced CSS/theme policy, access control of instance settings, Scalar API reference publication policy, and OpenAPI 3.2 versus 3.1 client compatibility.

17. **Versioned reference paths versus API versions:** prototype stable Scalar/spec routes and older-client compatibility tests; separately version the actual HTTP API and define deprecation terms.
18. **Translation improvement feedback:** reviewable `/suggest`-compatible input, source-based AI/human approval, audit and consent/provenance, abuse controls, immediate translation-memory overrides and separately curated model training.
19. **Cross-platform interoperability:** test non-.NET, legacy HTTP clients and optional LibreTranslate-compatible DTOs and endpoints; document stable JSON/error contracts.

20. **AfroLT standalone packaging and optional adapters:** ensure zero dependency on UnderstandMe/Afutrans, small embedded-core deployment, optional self-hosting and SignalR host assemblies, and one authoritative internal translation service.
21. **SignalR contract:** choose versioned hub route and method/event DTOs, JSON default versus optional MessagePack, bounded batch/stream semantics, reconnect/reconciliation, authentication/tenant isolation, quotas, cancellation and idempotent job acceptance.

22. **Shared transport-neutral command contracts:** choose minimal typed command handlers and caller context; define the exact request/response DTO and error representation shared by direct .NET, REST and the single SignalR Hub; verify REST/Hub parity and separate transport envelopes without duplicate business logic.

23. **AfroLT admin Hub:** specify auth and per-tenant visibility for \`/admin\`; current status snapshot and event contract (success/failure, worker/model state, recovery, capacity), subscription filters, bounded fanout/backpressure, event ordering and privacy; separate durable operational/audit records from ephemeral SignalR notifications.
24. **Admin command security and parity:** define which privileged administrative actions are exposed and ensure admin Hub, REST and direct .NET entry points invoke the same authorized application command handlers rather than duplicating work or involving passive Messenger as a scheduler.

25. **Hierarchical SystemMonitor contract:** define the shared typed event envelope, sender and event IDs, filters and access controls for `/SystemMonitor` and `/{ModuleName}/SystemMonitor`; verify one source event fans out to both routes without unrelated events or duplicated publications.
26. **Cross-host aggregation:** optional explicit bridging between separate processes, with origin preservation, deduplication, security and loop prevention. No automatic federation by matching route names.
27. **Monitor reliability tests:** zero listeners, slow/disconnected clients, privacy-safe diagnostics, bounded nonblocking fan-out, and no effects on business commands or durable writes.

28. **Typed monitoring payloads:** settle the MonitorEvent<T>/Message<T> generic interface and contract registry, including stable wire ContractId/SchemaVersion, System.Text.Json serialization for different T, unknown models, .NET 10/11 and cross-language client compatibility.
29. **Passive listener parity:** validate that the dedicated module transmitter and aggregate event-router listener each receive the same logical generic event, maintain its original payload model, and cannot delay or disrupt the publisher.

30. **Optional remote monitoring client:** specify configurable remote URLs, authentication/trust, source filters and a long-lived SignalR client reconnect policy for VPS3-style multi-host aggregation; do not make remote reception mandatory.
31. **Federation metadata and safety:** preserve the original typed event, origin and EventId, define optional relay path/hop metadata, per-source health reporting and bounded deduplication/loop prevention for chained or reciprocal connections.
32. **Cross-host contract tests:** simulate two remote monitors feeding a third, disconnect/reconnect with gaps, module-scoped filtering, unknown generic payloads, malicious/untrusted senders and bounded fan-in without blocking local work.

33. **Lite SystemMonitor handshake:** finalize which identifying GUIDs/metadata travel at SignalR connection establishment and which fields the source announces via `MonitorHello` (node name, modules, typed contract versions and instance metadata).
34. **Known-node checks:** define the receiver's configured endpoint + expected NodeId registry, module/source consistency checks and expected behavior for unknown/mismatched nodes, including optional relay-source declarations.
35. **Threat-model boundary:** accept that GUID is a public identifier, not cryptographic authentication. Test trusted HTTPS endpoint/certificate validation, explicit receiver configuration and absence of any remote client-callable Publish method; keep stronger credentials optional if requirements change.
36. **Monitor identification tests:** connection announcement precedes normal events, unknown GUID is rejected/ignored, misdeclared event source is not forwarded, and failures never block local work.

Current status and next continuation point: [Implementation Status](../../IMPLEMENTATION_STATUS.md).

