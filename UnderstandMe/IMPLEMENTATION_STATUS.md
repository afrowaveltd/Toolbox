# UnderstandMe implementation status

Last updated: 2026-10-10  
Phase: **requirements/design preparation; implementation has not begun**.

This is the continuation point for UnderstandMe work in the GitHub source of truth. Keep it updated after every later change. For established functional intent see [Notes.md](Notes.md). Do not mistake planned capabilities for implemented features.

## Verified repository baseline

- Project: `UnderstandMe/UnderstandMe.csproj`, currently a `net10.0` class-library skeleton.
- Declared package references: `Afrowave.Toolbox.Essentials 0.2.0` and `Afrowave.Toolbox.WhenItFails 0.1.0`. The latter is the **declaration in this csproj**, not a claim about the newest WhenItFails release; re-evaluate before implementation.
- Existing `Notes.md` contained an initial informal outline; this documentation pass organizes the design while retaining the key concepts.
- No QueueMaster, translation workers, placeholder engine, DataAccess adapters, Messenger, Editor, or tests have been implemented or verified in UnderstandMe.
- Related Afutrans repository contains a working-design/experimental front-end foundation. Current checked `WebTranslator` and `Afutrans.Core` projects target `net11.0`; future integration is not implemented here.

## 2026-10-10 — preparatory design documentation

- Recorded the modular, no-circular-dependencies architecture and explicit responsibilities of QueueMaster, DataAccess, TextSource, Translation, ResultProcessor, Editor, Messenger and Localization.
- Recorded the five-priority queue model, event `AddedToQueue` only on empty-to-nonempty transition, small translator FIFO buffers with pull-based refill, binary `IsReady`, short-retry/recovery concepts, and P0 RealTime volatility.
- Recorded dictionary-less source-key discovery, separate provisional cache, placeholder hint validation, `Update_Cache` and periodic reconciliation.
- Recorded configurable Translation Pipes/Chains, backend compatibility, AI prompts, chunked long-form translation, mandatory final AI review for articles/comments, and configurable tone policies.
- Recorded persisted jobs vs optional `Preserve`/`StepHistory`, bounded `LifeTime`, independent persistent rollback, mandatory manual-edit audit and optional AI semantic auditing.
- Recorded standalone SQLite / configurable EF Core persistence direction, `System.Text.Json` default, AJIS as a future optional adapter, and shared .NET 10/11 design discussion.
- Added project README and English topic files under `Docs/<Topic>/en.md`, plus explicit open questions. These are **design statements**, not tests or implemented code.

## Verification for this documentation-only change

- Examined live GitHub repository listings and relevant project files in Toolbox and Afutrans.
- No code, solution, package references or target frameworks are changed by this pass.
- No build/test execution or pipeline verification is claimed. The next coding phase must establish a new test baseline.

## 2026-10-10 — future native translation and upstream conduct (design only)

- Documented a possible independent C#/.NET native translator using compatible Argos models, CTranslate2 interop and model/process isolation rather than copying LibreTranslate service code.
- Recorded potential distributed workers with different subsets of directional models, bounded model memory/concurrency and automatic restart/recovery. The actual stability and memory benefit is not yet measured.
- Established transparent upstream credit, respectful collaboration and an offer to share useful improvements with LibreTranslate/Argos maintainers as an intended development practice.
- Recorded the verified differing upstream source licenses (LibreTranslate AGPL-3.0, Argos Translate MIT/CC0, CTranslate2 MIT) and flagged **individual model-package redistribution/license verification as unresolved**.
- No implementation, binary, package reference, test or runtime behavior changed.

## 2026-10-10 — AfroLT complementary host/API concept (design only)

- Clarified that the proposed AfroLT native .NET service **coexists with LibreTranslate**: keep LibreTranslate as a supported, useful backend, especially for HTML/placeholder translation, instead of rebuilding every feature unnecessarily.
- Added optional minimal web translation UI with separately loadable CSS themes, plus Microsoft-generated OpenAPI and a Scalar interactive API reference instead of a required Swagger UI stack.
- Noted the ASP.NET Core 11 OpenAPI 3.2 default and possible need for explicit 3.1 client compatibility; endpoints, licensing, browser security and interface contracts are **not yet finalized**.
- Added `Docs/AfroLT-Future-Design/en.md` and open questions. No code, packages or tests changed.

## 2026-10-10 — stable OpenAPI documentation and translation suggestions (design only)

- Planned `/api-docs` -> `/api-docs/latest`, fixed `/api-docs/v3.2/` and `/api-docs/v3.1/` references, and stable machine-readable specifications. OpenAPI specification versions are independent of AfroLT HTTP API contract versions.
- Added older/non-.NET HTTP JSON client interoperability, optional LibreTranslate-compatible adapters, and explicit backward-compatibility policies as design concerns.
- Planned a LibreTranslate-inspired suggestion API: collect proposals, optionally review/approve, apply verified corrections to a translation memory when authorized, and curate training datasets separately. Suggestions must not automatically retrain the deployed neural model.
- Updated the future AfroLT design, notes and open questions; no implementation or tests were changed.

## 2026-10-10 — standalone AfroLT + optional SignalR (design only)

- Established AfroLT as an independently usable .NET translation library/service that has **no required UnderstandMe or Afutrans dependency**.
- Planned one engine with optional direct .NET, REST JSON and SignalR transports. SignalR is for persistent bidirectional requests/events and may be absent in embedded mode; REST remains available independently.
- Separated AfroLT's own Hub and worker capacity/recovery controls from UnderstandMe's optional QueueMaster and Messenger integration through an adapter.
- Recorded default System.Text.Json wire DTOs, optional MessagePack evaluation, .NET 10/11/other-client compatibility, and open reliability/access-control questions.
- Updated `Docs/AfroLT-Future-Design/en.md`, `Notes.md`, and `Docs/Open-Questions/en.md`. Documentation only: no code, dependencies, project targets or tests changed.

## 2026-10-10 — REST/SignalR command parity (design only)

- Confirmed AfroLT exposes one set of application commands, request/response DTOs and validation/error behavior across direct .NET calls, REST and the single optional SignalR Hub.
- Transport adapters handle only transport-specific envelopes (HTTP status codes or SignalR connection/events), never separate translation logic; job IDs, results, policies and idempotency are shared.
- Added a requirement for command-level tests and REST/SignalR parity contract tests. No code or test implementation was performed.

## 2026-10-10 — dedicated AfroLT administration Hub (design only)

- Planned **two distinct optional SignalR surfaces** in the AfroLT host: the existing one public translation Hub and the administrator-only \`/admin\` Hub for live monitoring and privileged requests.
- Confirmed that success/failure/model/recovery events are emitted independently of subscribers; zero listeners must never change engine behavior. Admin clients receive a current snapshot and live deltas; noncritical telemetry need not be durable.
- Critical error records and mandatory edit audits remain independent of SignalR; administrative commands use the existing common authorized command handlers, not the Hub or the passive Messenger as a second engine.
- Added access control, tenant data privacy and backpressure as explicit design constraints. No implementation or tests changed.

## 2026-10-10 — best-effort SignalR versus guaranteed operations (design only)

- Confirmed the user's FM-radio principle: SignalR is an optional transient event/command transport, never an archive, reliability owner or prerequisite for core translation.
- Operations classify data at the source: telemetry/monitoring can be lost; durable jobs, accepted results, critical incidents and edit audit persist through their own stores/acknowledgements. SignalR may announce durable state but cannot substitute for it.
- Subscriber absence, disconnects and missing replies cannot block, cancel, reverse or change already accepted core work. Durable command retries need identity/idempotency. No Hub code was implemented or tested.

## 2026-10-10 — SystemMonitor shared outgoing-only event convention (design only)

- Recorded user-selected `/SystemMonitor` as the shared optional SignalR route for **server-to-client monitoring only** in AfroLT, UnderstandMe and Afutrans.
- Each module emits typed events independently; a single host Hub aggregates named sources to avoid conflicting paths. Different processes may each expose the same route on their own host/origin.
- Removed the earlier ambiguity between an admin observation Hub and admin commands: `/SystemMonitor` has no callable business methods; privileged commands remain separate and use shared application handlers.
- Live events may be missed and are never authoritative storage; core jobs, audit, critical incidents and final results retain independent durable ownership. No listener/response may block work.
- Updated `Docs/AfroLT-Future-Design/en.md`, `Docs/Architecture/en.md`, `Docs/Afutrans-Integration/en.md`, `Notes.md`, and `Docs/Open-Questions/en.md`. Documentation only; no code, dependencies or tests changed.

## 2026-10-10 — hierarchical SystemMonitor design

- Added one aggregate `/SystemMonitor` feed and named `/AfroLT/SystemMonitor`, `/Afutrans/SystemMonitor`, `/UnderstandMe/SystemMonitor` feeds.
- One typed event and sender identity are used throughout; host-level fan-out, not Hub-to-Hub relaying. Optional remote bridges require explicit configuration.
- Monitoring is outbound-only and best-effort, without command or persistence responsibilities.
- Created `Docs/SystemMonitor/en.md` and synchronized related design documents. No code or tests changed.

## 2026-10-10 — generic SystemMonitor event payloads (design only)

- Agreed that module-specific monitor transmitter and aggregate SystemMonitor event router are passive subscribers to the same module-emitted event, not SignalR Hub-to-Hub relays.
- Replaced the proposed plain-string Message with a generic Message<T> payload in the shared event envelope; different event types can carry their own typed report DTOs.
- Proposed ContractId and SchemaVersion for interoperable .NET 10/11 and non-.NET deserialization without trusting arbitrary CLR type names, plus an explicit supported-type registry.
- Recorded serialization, passive listener and global/module feed parity tests for the later implementation phase. Updated Docs/SystemMonitor/en.md, Notes.md, and Docs/Open-Questions/en.md. Documentation only; no runtime code or test suite changed.

## 2026-10-10 — optional remote SystemMonitor receivers (design only)

- Agreed that an orchestrator may optionally act as a SignalR client, subscribing to multiple remote root SystemMonitors and routing their live events together with its own.
- Recorded the three-server scenario: VPS3 receives VPS1 and VPS2 event streams and provides one aggregate `/SystemMonitor` for an administrator.
- No inbound methods are added to the outbound-only monitor Hub. A separate receiver adapter and the existing event router implement the fan-in.
- Preserve typed event contracts, original sender and EventId; require explicit trusted endpoints, bounded nonblocking delivery, reconnect health, deduplication and prevention of forwarding loops.
- Remote monitoring remains best-effort: missed events are not replayed or persisted by SignalR, and remote outages cannot stop the local core.
- Updated `Docs/SystemMonitor/en.md`, `Docs/Architecture/en.md`, `Notes.md`, `Docs/Open-Questions/en.md`, and this status. No code, packages or tests changed.

## 2026-10-10 — pairwise monitoring authorization (design only)

- Agreed that independent nodes connect only when the receiving SystemMonitor host has registered and authorized their access key. NodeId may be GUID; secrets are separately generated cryptographically random 256-bit tokens and are scoped per receiving peer/link.
- Read access is limited to approved system/module routes; the key can be revoked/rotated without a central identity provider. Use standard SignalR bearer header transport over TLS and secure secret storage.
- The event-only SignalR Hub exposes no client-side publish operation. Remote event ingestion occurs exclusively through configured receiver clients, which validate expected remote peer and permitted original sources/relay rights rather than trusting sender fields.
- Unknown credentials are denied but disconnected/untrusted monitors do not interfere with core work. Recorded connection-revocation, spoofing, relay allowlist and access-scoping questions/tests.
- Updated `Docs/SystemMonitor/en.md`, `Docs/Architecture/en.md`, `Notes.md`, `Docs/Open-Questions/en.md` and this status. No source code or tests changed.

## 2026-10-10 — simplified SystemMonitor peer identification (design only)

- **Supersedes the earlier same-day proposal for mandatory per-link 256-bit bearer keys.** The agreed baseline is a lite GUID `NodeId` known to the receiving monitor, plus a `MonitorHello` describing the originating node and what modules/event contracts it will send. Optional connecting-client GUID metadata can support a simple allowlist.
- For VPS3-style federation, the receiver compares declared origin with the explicitly configured HTTPS endpoint and registered node/module sources before forwarding live events. Unknown or inconsistent sources are ignored without affecting work.
- Documented the intentional trust boundary: GUID is identification, **not strong authentication** and can be copied; use normal HTTPS server-certificate verification. No mandatory OAuth, JWT, per-link secret distribution or central account system for anonymous outbound-only monitoring.
- The SystemMonitor Hub still exposes no client-callable event publishing methods; incoming remote monitoring events are accepted only through the configured SignalR receiver client.
- Updated `Docs/SystemMonitor/en.md`, `Docs/Architecture/en.md`, `Notes.md`, `Docs/Open-Questions/en.md` and this status. Documentation only; no runtime implementation or test suite changed.

## 2026-10-10 — administrator-approved monitoring enrollment (design only)

- Added a self-enrollment request for new nodes (example VPS4): public ClientId GUID, declared endpoint and modules; the receiving host stores it as **Pending**, never approves unknown nodes automatically.
- A typed `MonitorRegistrationRequested` event is broadcast through the read-only `/SystemMonitor` so an online administrator may review immediately; offline administrators use the durable pending list. Approve/deny is performed through a separate authorized administrative API/UI.
- Upon approval, the receiving host creates a new **private random GUID ClientKey** associated with the approved ClientId and allowed sources. The applicant retrieves it through an independent protected claim/poll flow and later presents both values in HTTPS control headers. The key must never appear in monitoring events; although it is a GUID, it acts as a small bearer credential.
- Distinguishes enrollment/control from the reverse-direction pull connection: the aggregator's SignalR receiver connects to the approved source under separately validated HTTPS server identity. SignalR monitor Hub stays strictly server-to-client.
- Documented pending-registration durability, notification best effort, revocation, one-time claim security and anti-flood tests. Updated `Docs/SystemMonitor/en.md`, `Docs/Architecture/en.md`, `Notes.md`, `Docs/Open-Questions/en.md`, and this status. No runtime code, package or test implementation changed.

## 2026-10-10 — on-demand GUID challenge for familiar monitoring nodes (design only)

- Refined the administrator-approved SystemMonitor pairing with a doorman-style, **risk-triggered identity challenge**. After first approved activation, normal active connections require no additional GUID transmission; routine new/reconnected sessions may use the approved public ClientId and familiar network/source metadata without sending ClientKey every time.
- If the apparent connection identity changes unexpectedly, the recipient requests the **private recipient-issued ClientKey GUID** over a separate HTTPS connection-verification path before trusting new monitoring events. The outbound-only SystemMonitor Hub still has no client-callable business methods.
- Clarified the security tradeoff: IP/header/public-GUID familiarity is spoofable and **does not strongly authenticate a newly reconnected node**. Challenge-every-new-connection remains a possible stricter option.
- Updated `Docs/SystemMonitor/en.md`, `Docs/Architecture/en.md`, `Notes.md`, `Docs/Open-Questions/en.md` and this status. No implementation or test execution occurred.

## 2026-10-10 — operational blocking of noisy monitor sources (design only)

- Clarified admin response to misbehaving SystemMonitor origins: `Mute` hides events only in a presentation, `Block` drops a source at router ingress, and `Disconnect` stops a controlled remote SignalR receiver connection / revokes subsequent access.
- Preserve original `ClientId` plus immediate relay information so a particular forwarded origin can be blocked without losing all other sources from the same VPS.
- Noted that the light identity system uses a claimed sender ID as a practical filtering handle rather than guaranteed identity proof. Add bounded ingress/rate limits for noisy streams and registration attempts, and prevent any monitor remediation from interfering with actual processing.
- Updated `Docs/SystemMonitor/en.md`, `Docs/Open-Questions/en.md` and this status. Documentation only; no source changes or tests run.

## 2026-10-10 — interactive monitoring filters and incident-first admission (design only)

- Defined a clickable SystemMonitor view-filter interface where each administrator independently selects source/VPS, original ClientId, relay, module/instance, severity/outcome, event contract, correlation/job ID and search/time criteria; presets are supported. Presentation filtering changes neither a publisher nor another subscriber's view.
- **Supersedes the suggestion of automatic blocking for bad requests/events:** an invalid or suspicious inbound monitoring event is recorded as a safe, persistent incident and then dropped; no automatic registry block, revocation or disconnection. Correct new-node enrollment requests still become Pending.
- Only an authorized human administrator chooses subsequent actions: inspect, ignore, display mute, manually block/revoke or disconnect. Repetitive incidents may be aggregated and incoming event buffers bounded without promoting this to automatic trust-state changes.
- Updated `Docs/SystemMonitor/en.md`, `Docs/Architecture/en.md`, `Docs/Afutrans-Integration/en.md`, `Notes.md`, `Docs/Open-Questions/en.md`, and this status. Documentation only; no code or tests changed.

## 2026-10-10 — per-request translation provider priority and cost-aware fallback (design only)

- Reaffirmed that discussing, documenting and validating modular rules comes before implementing blocks. No new source code, test suite or actual translation integration was created in this change.
- Added `Docs/Provider-Routing/en.md` for **independent translation provider preferences**, economics and explicit escalation. QueueMaster's five **job priority** queues remain distinct from provider/endpoint selection priority.
- LibreTranslate remains a supported low-direct-cost self-hosted provider, but not a mandatory default: operator policies may prefer Google, Microsoft, AfroLT or AI, or prohibit all remote endpoints (`LocalOnly`).
- Provider-selection policies can apply through system defaults, named Pipes, Translation Targets, individual permitted jobs and Chain steps; hard constraints (language/format support, readiness, privacy, spending cap and mandatory validation) are not bypassed by overrides.
- Fallback/quality escalation requires an explicit trigger and configured limits; a provider's reputation alone does not prove translation quality. Budget, retry billing, latency and cross-provider feature compatibility remain design/test topics.
- Updated `Docs/Translation-Chains/en.md`, `Docs/Architecture/en.md`, `Docs/Afutrans-Integration/en.md`, `README.md`, `Notes.md`, `Docs/Open-Questions/en.md` and this status. No package, worker, queue or monitoring implementation changed.

## 2026-10-10 — additional provider design questions identified (discussion candidates)

- Reviewed the existing provider-routing, QueueMaster, Chain and open-questions documents for remaining gaps. Added six **unresolved candidates** to `Docs/Open-Questions/en.md` (items 53–58), not new agreed implementation requirements.
- Questions cover wait-versus-paid-fallback-versus-fail on cheaper endpoint congestion; translation memory and domain glossaries distinct from provisional cache; no-cost provider routing dry-run/explanations; configuration snapshot semantics for in-flight jobs; active endpoint capability tests; and evidence-based quality by language pair/domain.
- Suggested discussing wait/fallback/fail first, because it affects both operating cost and job deadlines. No source code, packages, tests or settled provider policy were changed.

## 2026-10-10 — request Settings, Mandatory RealTime and segmented live translation (agreed design only)

- Settled the six provider/runtime design topics discussed with the user; preserved as **agreed architectural behavior** rather than mere backlog suggestions.
- Created `Docs/Request-Settings-and-Live-Translation/en.md` defining an effective typed Settings snapshot when a logical request is created, separate P0–P4 QueuePriority and composable behavior flags, including `Mandatory`. New defaults apply to new requests; already accepted queued/in-flight work retains original policies. Orchestrator pause/stop has operational precedence.
- `RealTime + Mandatory` should avoid waiting for an occupied preferred endpoint and choose an available, compatible, policy-acceptable translator now, without bypassing budget/privacy/format limits. Ordinary non-Mandatory RealTime may return source text immediately and continue delivering later translated segments.
- Splitter outputs stable ordered `(SegmentId, Text)` from 0..N; `RequestId` disambiguates concurrent documents. Browser replaces only verified returned segments (textContent for plain text, sanitized format-aware HTML for Markdown blocks); application-facing update delivery is separate from SystemMonitor and request lifetime/durability remains explicit.
- `SystemPersistent` was accepted as **optional, bounded approved translation memory** checked before provider routing, separate from dictionary, provisional cache and historical logs. Reading and promoting/writing are independently configurable to avoid unlimited growth.
- Confirmed economical default provider tiers: local/free, including LibreTranslate, then paid APIs **equal priority by default**, then AI. Operators may override globally/per language pair. No automatic permanent language quality rank based on stale model observations; capability validation may be redone when a model version changes.
- Provider-selection dry-run simulator was **deferred** until Settings/provider routing are implemented; no new immediate dependency or feature committed as implemented.
- Updated `Docs/Provider-Routing/en.md`, `Docs/Translation-Chains/en.md`, `Docs/Localization/en.md`, `Docs/QueueMaster-and-Reliability/en.md`, `Docs/Architecture/en.md`, `Notes.md`, `Docs/Open-Questions/en.md`, `README.md` and this status. No .NET implementation or executable tests were changed or run.

## Next continuation

1. Continue discussing unresolved design questions using `Docs/Open-Questions/en.md`; do not finalize the implementation plan prematurely.
2. Agree on minimal common object/interface contracts, QueueMaster ownership/atomicity and translator buffer semantics.
3. Decide the net10/net11 API packaging strategy and existing Toolbox dependency baselines.
4. Define testable increments and only then create the formal implementation plan and begin code, tests and per-change status updates.

Source of truth: https://github.com/afrowaveltd/Toolbox (branch `master`).

