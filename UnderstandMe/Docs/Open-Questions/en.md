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

Current status and next continuation point: [Implementation Status](../../IMPLEMENTATION_STATUS.md).

