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

37. **Monitor node self-enrollment:** finalize the small separate HTTPS request endpoint, `ClientId` + declared source metadata, durable pending registration record, and typed non-secret `MonitorRegistrationRequested` notification to live administrators. Never auto-approve.
38. **Admin decision and client claim:** define authorized approve/deny UI and audited decision, recipient-generated GUID `ClientKey` bound to the approved client/source permissions, safe one-time applicant claim/poll flow after offline approval, replacement/revocation and HTTPS headers for subsequent control calls. Do not publish the key on SignalR.
39. **Enrollment/source-direction clarity:** distinguish the credential used on VPS4 -> VPS3 enrollment/control from VPS3 -> VPS4 read-only SignalR monitoring; configure and validate remote server identity independently. Approval may start automatic source monitoring without changing the outgoing-only Hub.
40. **Enrollment tests:** concurrent/duplicate requests, administrator absent/present, rejected and approved nodes, secret-claim theft, expired claim tickets, registration flooding, and rejected/failed remote connection without impacting existing monitoring or other work.

41. **Adaptive monitor familiarity:** define the small approved-node connection profile (ClientId, configured HTTPS endpoint, stable versus changeable network hints, module list); distinguish active connections from newly established/reconnected sessions and document the deliberate spoofing risk of accepted unchanged-looking connections.
42. **On-demand GUID challenge:** define which changes trigger `IdentityChallenge`, where HTTPS challenge/response occurs outside the event-only Hub, challenge expiry and connection binding, and how the recipient compares the private recipient-issued ClientKey before forwarding data.
43. **Compatibility and risk-level tests:** known routine reconnect with only ClientId, changed IP behind NAT/proxy, changed source-module claims, successful/failed/no response to the GUID challenge, spoofed familiar headers/IP, plus optional challenge-every-session policy for stricter deployments. No scenario may stop core work.

44. **Interactive administrator filters:** finalize independent multi-select source/VPS, module/instance, event type/severity, correlation/job, full-text/diagnostic and time filters; include saved personal presets, default views, reset, and performance with large incoming event rates.
45. **Incident-and-discard on bad input:** define admission validation and durable incident schema; invalid/mismatched event is dropped, its registered source is **never blocked automatically**. Distinguish new legitimate Pending registrations from rejected regular event input.
46. **Admin-only node decisions:** define read and manual controls for incident review, view-only Mute, ingress Block, revocation and disconnect; ensure no automatic sanctions are triggered by counts of incidents.
47. **Robustness tests:** per-admin UI filters do not affect other admins or source feeds; repeated incidents aggregate safely, invalid messages do not enter normal streams, unaffected nodes stay live, and ingestion quotas shed load without changing trust state.

48. **Provider priority vs job priority:** define minimal endpoint capabilities and the Pipe-owned selection policy; verify independent QueueMaster P0-P4 dispatch and provider order/cost ranking, including configurable Google-first and LibreTranslate-first profiles.
49. **Provider cost model:** define per-character/token/request/local compute estimates, handling unknown prices/currencies, spending caps, quota/latency constraints, and whether costs are recorded per job/step/target; explicitly avoid assuming local translation has zero cost.
50. **Escalation/fallback decision:** define exact retryable failures versus validation-based escalation and mandatory AI/human review; determine attempt limits, per-endpoint cooldown, ambiguous timeout retry billing/idempotency and fallback exhaustion outcomes.
51. **Provider override hierarchy and privacy:** settle global -> Pipe -> Translation Target -> allowed job/Chain-step precedence without relaxing source/target language, format/HTML, protected placeholders, LocalOnly, forbidden remote providers or budget caps.
52. **Provider routing tests:** primary compatible provider, unsupported pair/capabilities, endpoint not ready, LocalOnly never calls external APIs, paid-primary preference, cost-first preference, budget rejection, validation failure escalation, exhausted providers and independent monitoring of attempts/cost estimates.

53. **Mandatory RealTime edge cases (baseline agreed):** Mandatory selects an available eligible engine without waiting for its preferred busy provider, while non-Mandatory RealTime can return untranslated source and update segments later. Define exact no-endpoint and maximum-cost outcomes, deadline and optional continuation cancellation policy; never imply that Mandatory bypasses limits.
54. **SystemPersistent bounds (baseline agreed):** opt-in bounded, persistent, context-aware approved translation memory is checked before providers and remains separate from authoritative dictionaries and provisional cache. Decide concrete key schema, read/write flags, quotas, promotion/approval, invalidation and deletion.
55. **Provider routing dry-run (deferred):** admin-side simulation is a useful future enhancement but is explicitly **not required for the first implementation**. Revisit after stable selection/Settings contracts, with no paid work performed during a dry run.
56. **Immutable Settings at request creation (baseline agreed):** changed admin defaults apply to new requests, queued/in-flight items retain their captured policy; orchestrator pause/stop overrides execution. Settle public typed Settings groups, valid flag combinations, snapshot storage/version, explicit cancellation/revoked credential behavior and request-bound deadline semantics.
57. **Version-aware capability tests (baseline agreed):** verify endpoint language/format/placeholder behavior and re-evaluate when the detected model version changes. Decide probe corpus, cadence, capability-result TTL and avoiding charged self-tests unless explicitly permitted.
58. **Manual language-pair preferences (baseline agreed):** default engine tiers free/local -> equally ranked paid APIs -> AI, with operator overrides per language pair and without automatic historical quality ranking. Define deterministic tie-breaking among eligible same-tier providers and a stable configuration/override representation.

59. **Segmented live updates:** define `RequestId`, ordered `SegmentId=0..N`, source-format boundaries, partial results in arbitrary completion order, duplicate/outdated results, HTML block versus inline DOM wrappers, safe plain-text `textContent` versus validated HTML insertion and application delivery/reconnect/cancellation separate from SystemMonitor.
60. **Flags versus enums:** name the independently composable request behavior flags (especially Mandatory and SystemPersistent read/write), exclusive QueuePriority and wait strategy, and validate meaningful combinations without a monolithic options object.
61. **Orchestrator command precedence tests:** queued/running request retains old policy when global rankings change; administrative pause stops new dispatch even for Mandatory; later resume uses captured request settings rather than newly chosen provider priorities.

Current status and next continuation point: [Implementation Status](../../IMPLEMENTATION_STATUS.md).

