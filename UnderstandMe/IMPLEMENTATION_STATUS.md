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

## 2026-10-10 — UI-neutral segments and admin-governed paid provider metering (design only)

- Clarified that `List<(SegmentId, Text)>` with stable `RequestId`, formatting metadata and incremental completed results is the **core contract**. Blazor component state, JavaScript DOM replacement and Avalonia are implementation choices of the consuming UI, not library requirements. Replaced the prior JS-centric segment description in `Docs/Request-Settings-and-Live-Translation/en.md`.
- Created `Docs/Provider-Billing-and-Quotas/en.md`. Translation provider adapters declare metering/price units and capabilities; configured actual **account/endpoint commercial plans** store subscription/package usage, included monthly units, currency, renewal, overage rates and admin-selected limits (e.g. one million words/month).
- Agreed: an administrator selects **hard stop**, **explicit paid overage within an authorized cap**, or **manual purchase/registration of an additional package**. The software measures and estimates usage but never independently buys new packages or increases spending permission.
- Defined an independent durable UsageLedger/QuotaAccount with atomic allowance reservations before chargeable dispatch and reconciliation of actual vs estimated provider charges. This handles concurrent requests and shared accounts across VPS nodes; cost records are not SignalR monitoring messages. Current hard budget/quota controls supersede dispatch, while existing queued jobs retain their accepted provider-order preference.
- Cost breakdown, included/remaining units and warnings may be shown to the admin; live SystemMonitor emits best-effort notifications only. Model details, time zone/period boundaries, actual provider counting, ambiguous timed-out billing and quota concurrency are recorded as questions 62–66 in `Docs/Open-Questions/en.md`.
- Updated `Docs/Provider-Routing/en.md`, `Docs/Request-Settings-and-Live-Translation/en.md`, `Docs/Translation-Chains/en.md`, `Docs/Architecture/en.md`, `Docs/QueueMaster-and-Reliability/en.md`, `Notes.md`, `README.md` and this status. Design/documentation only; no runtime code, packages, or tests changed.

## 2026-10-10 — orchestrator-first admission and authorized provider alternatives (agreed design only)

- Agreed that **the orchestrator, not the caller, has final authority** over effective translation request Settings, provider eligibility, priority restrictions, financial controls and operational queue commands.
- A caller naming Google API or another provider is a **preference**. Preserve OriginalRequest; normalize allowed EffectiveRequest at admission under admin policy, and report changes to the client with typed reason codes.
- **Functional best effort is the default:** a request must not be rejected solely because its preferred engine is forbidden, unavailable or blocked by a hard monthly quota. Evaluate other compatible authorized endpoints, and perform translation when feasible; reject with a precise reason only when none can meet actual hard requirements, or report failure if a previously accepted job becomes impossible.
- `Accepted/Deferred` admission receipts do not claim completion. A completed response identifies the backend actually used (per-segment provenance if needed) and provider substitutions; errors should reuse Essentials/WhenItFails rather than establishing an incompatible second framework.
- Current account hard billing limits and administrator queue pause/stop still govern every **new dispatch** for previously accepted work, while the historical original request and effective provider preference snapshot remain unchanged. `Mandatory` and caller requests cannot override them.
- Created `Docs/Request-Admission-and-Normalization/en.md` and updated `Docs/Provider-Routing/en.md`, `Docs/Request-Settings-and-Live-Translation/en.md`, `Docs/Provider-Billing-and-Quotas/en.md`, `Docs/QueueMaster-and-Reliability/en.md`, `Docs/Architecture/en.md`, `Notes.md`, `Docs/Open-Questions/en.md`, `README.md` and this status. Documentation only: no source code or tests modified or executed.

## 2026-10-10 — per-client translation access and optional public spare-capacity API (agreed design only)

- Confirmed previous admission, provider account quota and global administrator policy design already existed; identified **per-client provider entitlement** as a separate missing layer, rather than duplicating the existing commercial/QueueMaster components.
- Created `Docs/Client-Access-Policies/en.md` for `ClientContext` and `ClientPolicy`: distinct registered/internal, anonymous/public, free and paying clients; per-client allowed provider families/accounts, feature entitlements, word/cost limits, input/concurrency/rate caps and optional group policies.
- **Baseline:** ordinary known clients can use all globally permitted provider engines by default unless an administrator narrows their client policy. Optional public/anonymous Afutrans API receives its own restricted profile to share spare self-hosted LibreTranslate/free capacity **without using the operator's paid API billing accounts**. Enabling the core library does not automatically publish a public endpoint.
- The orchestrator intersects trusted client authorization with global rules, provider capabilities, provider billing availability and request requirements. If a client requests forbidden Google, fall back to an allowed capable engine (e.g. LibreTranslate); reject only if none qualifies, returning an explanatory code.
- Per-client usage quotas and the host's actual provider-account packages remain independent limits; atomically enforce both before dispatch when needed. Optional spare-capacity controls prevent public traffic from starving internal processing, without creating one queue per client or changing existing P0-P4 queue ownership.
- Client authentication for financially meaningful public requests **must not rely on unverified request-supplied IDs or the intentionally lite SystemMonitor GUID identification mechanism**.
- Updated `Docs/Request-Admission-and-Normalization/en.md`, `Docs/Provider-Routing/en.md`, `Docs/Provider-Billing-and-Quotas/en.md`, `Docs/Architecture/en.md`, `Docs/Afutrans-Integration/en.md`, `Docs/QueueMaster-and-Reliability/en.md`, `README.md`, `Notes.md`, `Docs/Open-Questions/en.md` and this status. Design-only documentation; no .NET runtime code or tests changed or run.

## 2026-10-10 — mandatory Human capability; all automatic providers optional (agreed design only)

- Clarified the minimum viable provider baseline: **Human translation is the only mandatory provider capability**. UnderstandMe and the Afutrans host must support a human-only deployment with manual work assignment, typed results and validation; immediate human availability and mandatory human review of machine results are **not** assumed.
- LibreTranslate, AfroLT, Google, Microsoft, Ollama/AI and all other automatic translation engines are **optional and individually activated plugins**. No external model, cloud API, credential, paid subscription or billing service is required by the human-only core. The default free/local -> equally ranked paid APIs -> AI preference applies only to **activated** automatic providers.
- Preserved the unrestricted **known-client default** at the client-permission layer: it does not activate any vendor and remains subject to administrator-enabled provider sets and global limits. **Public API exposure is opt-in**, but the operator chooses the public profile — free/local, Human-only, chosen paid/AI providers, or all activated providers. Free-only public access is only a template, **not a hardcoded restriction**.
- Clarified a special supported scenario: an administrator may deliberately sponsor **anonymous public paid translations** with a shared public-profile spending cap and provider-account hard limit. Only **client-specific paid entitlements** need verifiable caller identity for attribution; this is distinct from the lightweight SystemMonitor peer mechanism.
- Updated `Docs/Client-Access-Policies/en.md`, `Docs/Provider-Routing/en.md`, `Docs/Translation-Chains/en.md`, `Docs/Architecture/en.md`, `Docs/Afutrans-Integration/en.md`, `Docs/Request-Admission-and-Normalization/en.md`, `Docs/Provider-Billing-and-Quotas/en.md`, `Docs/QueueMaster-and-Reliability/en.md`, `README.md`, `Notes.md`, `Docs/Open-Questions/en.md` and this implementation status. **Design documentation only; no .NET code changed or tests executed**.

## 2026-10-10 — Human translator as durable backlog worker (agreed design only)

- Clarified that Human translation is simply **eventual processing of the existing QueueMaster five priority queues**. A human translator may be offline while new human-intended work is accepted and durably persisted. When the translator later opens any UI, they see authoritative waiting/claimed counts per priority and language, and choose/claim work. The UI does not own the queue and SystemMonitor events are never the source of truth.
- Durable human work must support **atomic exclusive claim/reservation, disconnected worker recovery, stale-claim conflict detection and preservation of completed/checkpointed results**. Specific timeout/heartbeat mechanisms remain open questions.
- Preserved the previously agreed **volatile P0 RealTime default**: eventual Human fulfillment from a live request requires an explicitly admitted, durable human continuation item correlated to the original request, not an implicit new retention policy for all RealTime work.
- Created `Docs/Human-Translation-Workflow/en.md`; updated `Docs/QueueMaster-and-Reliability/en.md`, `Docs/Translation-Chains/en.md`, `Docs/Architecture/en.md`, `Docs/Request-Settings-and-Live-Translation/en.md`, `Docs/Editor-and-Audit/en.md`, `Docs/Afutrans-Integration/en.md`, `Notes.md`, `README.md`, `Docs/Open-Questions/en.md`, and this status. **Design/documentation only; no runtime code, package or test execution.**

## 2026-10-10 — P0 RealTime automatic-only; no Human access (agreed correction, design only)

- Corrected the previous generic 'Human can process pending queue work' description: **Human never processes P0 RealTime**. RealTime is exclusively for immediate **automatic** translation, including when `Mandatory` is requested. If **only Human is activated**, P0 translation execution is **inactive** and a RealTime request **returns unchanged source text almost immediately** without enqueueing it for manual translation.
- When automatic providers fail, are unsupported, busy or prohibited, no RealTime-to-Human fallback or automatic durable human continuation may occur. The permissible P0 outcome is untranslated source text with an explanatory status as appropriate. Optional short-lived live segment updates remain automatic-only.
- **Privacy boundary:** P0 may contain private chats and must never expose those texts in Human queues, translator UI, manual editor or durable human job storage solely because RealTime was requested or failed. `Mandatory` is not consent for manual processing; any separate logging/cache follows its own explicit privacy policy.
- Human work is **only separately submitted/authorized, durable non-P0 work (P1-P4)** processed when a human translator opens an interface. Existing five global priority categories are preserved; no sixth human queue. Distinguish this from prior wording suggesting automatic Human continuations after P0, which is now **superseded**.
- Updated `Docs/Human-Translation-Workflow/en.md`, `Docs/QueueMaster-and-Reliability/en.md`, `Docs/Request-Settings-and-Live-Translation/en.md`, `Docs/Translation-Chains/en.md`, `Docs/Architecture/en.md`, `Docs/Provider-Routing/en.md`, `Docs/Request-Admission-and-Normalization/en.md`, `Docs/Localization/en.md`, `Docs/Editor-and-Audit/en.md`, `Docs/Afutrans-Integration/en.md`, `Notes.md`, `README.md`, `Docs/Open-Questions/en.md` and this status. Documentation only: no runtime code or executable tests modified or run.

## 2026-10-10 — request Privacy and durable access-scoped result reuse (agreed design only)

- Agreed a separate request-level `Privacy` classification. **Privacy controls content visibility/manual audit/processor disclosure**, whereas `Persist`, result cache, `SystemPersistent` and `LifeTime` control different types of retention; do not equate `Private` with `NoStore`.
- Private chats, forum discussions and other scoped histories may need a **durable, verified translated-result store** to avoid paying/retranslating when authorized viewers revisit history. `P0 RealTime` job execution remains **volatile, automatic-only**; persistence of the authorized final translation **does not** persist the P0 job or make it Human work.
- Private results are scoped by tenant/target/conversation/source message or segment/revision/language/format and guarded by **current source-owner read permissions at every lookup**. Source edits, deletion, access revocation and retention/expiry restrict or invalidate saved translations. Avoid automatic global dictionary `KEY=VALUE` registration, shared `SystemPersistent` promotion, cross-tenant reuse or unnecessary raw-message duplication.
- A normal Private request must **not** enter Human translation queues or manual audit/Editor, including failure paths. Private text must not leak through SystemMonitor, operational logs/errors or admin dashboard previews; content-free metrics remain possible. A **separate explicitly owner-authorized non-P0 manual translation operation** is not automatic audit/fallback and can grant purpose-limited human access.
- Private content can be processed by permitted automatic validators/engines; remote API/AI data disclosure is governed separately by the effective privacy policy rather than universally forbidden or automatically allowed.
- Created `Docs/Translation-Privacy-and-Scoped-Storage/en.md`, updated `Docs/Request-Settings-and-Live-Translation/en.md`, `Docs/Localization/en.md`, `Docs/Editor-and-Audit/en.md`, `Docs/Human-Translation-Workflow/en.md`, `Docs/Client-Access-Policies/en.md`, `Docs/Request-Admission-and-Normalization/en.md`, `Docs/Architecture/en.md`, `Docs/QueueMaster-and-Reliability/en.md`, `Docs/Provider-Routing/en.md`, `Docs/Translation-Chains/en.md`, `Docs/Afutrans-Integration/en.md`, `Notes.md`, `README.md`, `Docs/Open-Questions/en.md` and this status. **Documentation only, no runtime code or tests changed.**

## 2026-10-10 — chat chunks are transient; recipient self-correction and optional cache (agreed design correction)

- Clarified two **separate translation workflows**: `KEY=VALUE` and dictionary-less key registration are **exclusively for application/web UI localization dictionaries** (JSON and comparable stores). Chat/forum/user-authored messages are not dictionary entries and never persist dynamic source text as dictionary keys.
- A message Splitter creates request-scoped ordered `List<(Id, Text)>`, with temporary IDs 0..N **only for matching/reassembling translated chunks**. UnderstandMe translates/returns pieces. The **consuming chat/forum application** reassembles the completed translated message, controls its presentation, original source-message IDs, history/result persistence, reader permissions and retention. The engine does not need a permanent chat database or durable chunk keys.
- **An authorized message recipient may voluntarily correct their own machine-translated text** using an optional application/editor tool. This is **not** generic Human worker processing, third-party manual audit or a dictionary edit. It does not disclose the initial private translation to anyone outside the original authorized audience.
- The recipient independently chooses **no cache** (correction only for current display) or **opt in to recipient-scoped caching** of their corrected translation. The host can separately store its initial machine translation for efficient history views. Broader sharing/contribution, if offered, needs an independent deliberate permission/authorization; choosing personal cache does not publish a private source message.
- `P0 RealTime` remains volatile and automatic-only, while the host may save assembled finished text under its own history policy. No automatic fallback to Human, global UI dictionary registration, generic SystemPersistent promotion or administrator content browsing. Existing manual **UI-dictionary revision audit** is preserved and does not become mandatory auditing of private chat edits.
- Created `Docs/Chat-Translation-Flow/en.md`, rewrote chat-specific rules in `Docs/Translation-Privacy-and-Scoped-Storage/en.md`, updated `Docs/Localization/en.md`, `Docs/Request-Settings-and-Live-Translation/en.md`, `Docs/Editor-and-Audit/en.md`, `Docs/Translation-Chains/en.md`, `Docs/Architecture/en.md`, `Docs/Afutrans-Integration/en.md`, `Docs/Request-Admission-and-Normalization/en.md`, `Docs/Human-Translation-Workflow/en.md`, `README.md`, `Notes.md`, `Docs/Open-Questions/en.md` and this status. Documentation only; no source code or executable tests changed or run.

## 2026-10-10 — meaningful format-aware Splitter rules (agreed design only)

- Agreed that chunks must be **meaningful semantic units**, not fixed-size fragments. Chunking is primarily for larger/structured inputs; a short ordinary text can stay one chunk.
- For **plain text**, prefer **paragraph** boundaries where present, with configurable newline/blank-line semantics. For longer unformatted prose, use **sentence** boundaries; period-based heuristics must preserve abbreviations, numeric decimals, URLs, language punctuation and whitespace. Request `Settings` selects Auto/Paragraph/Sentence/etc. modes (conceptual names).
- For **Markdown**, chunk meaningful content of headings, paragraphs, list items, table cells and other text-bearing parsed nodes while preserving syntax and context. For **HTML**, translate appropriate text-bearing element content and retain nesting/inline formatting; never split purely by tags if it would destroy sentence meaning, and never treat scripts/styles/code as translatable content.
- Each other supported document type may later supply its own format-specific Splitter adapter. If an intact semantic chunk exceeds an endpoint's input-size limit, subdivide safely at a more granular boundary or use a suitable alternate endpoint. Preserve placeholders/links/format mapping.
- `RequestId + SegmentId=0..N` remain temporary request-local IDs; **no UI dictionary `KEY=VALUE` registration, persistent chat key or central host content store is created by splitting**. The consuming application reassembles and optionally stores the full translation.
- Created `Docs/Semantic-Splitter/en.md`; updated `Docs/Chat-Translation-Flow/en.md`, `Docs/Request-Settings-and-Live-Translation/en.md`, `Docs/Translation-Chains/en.md`, `Docs/Architecture/en.md`, `Notes.md`, `README.md`, `Docs/Open-Questions/en.md` and this status. Documentation only; **no source code or tests changed or executed**.

## 2026-10-10 — structural templates and source-initialized ResultList (proposal, not finalized)

- Captured the user's exploratory HTML reconstruction pattern: parsed structured source + **temporary integer references** to a `ResultList`; initialize every slot with **original source** so the whole document renders immediately; verified translations progressively replace slots, while unfinished/failed parts retain source.
- Proposed a format-aware parser/structural map instead of injecting literal `placeholder=0` attributes or a raw `@ResultList[0]` expression into user HTML. HTML uses appropriate parsing for void elements such as `br`, nested content and mixed text/formatting; Markdown uses a semantic AST. A nested inline sentence needs contextual translation with protected markup rather than one word per text node.
- Illustrative per-slot data: `Id`, `OriginalText`, `CurrentText`, state and revision; `0..N` IDs are **ephemeral** and not localization dictionary keys, durable queues or chat message IDs. Projection/rendering remains UI-neutral; caller/host owns final content assembly and optional persistence.
- Created `Docs/Structural-Template-and-ResultList/en.md`; updated `Docs/Semantic-Splitter/en.md`, `Docs/Request-Settings-and-Live-Translation/en.md`, `Docs/Translation-Chains/en.md`, `Notes.md`, `README.md`, `Docs/Open-Questions/en.md` and this status. **No public API/final HTML parser decision, runtime code or test changes**; detailed nesting/format contracts remain open in Questions 100–103.

## Next continuation

1. Continue discussing unresolved design questions using `Docs/Open-Questions/en.md`; do not finalize the implementation plan prematurely.
2. Agree on minimal common object/interface contracts, QueueMaster ownership/atomicity and translator buffer semantics.
3. Decide the net10/net11 API packaging strategy and existing Toolbox dependency baselines.
4. Define testable increments and only then create the formal implementation plan and begin code, tests and per-change status updates.

Source of truth: https://github.com/afrowaveltd/Toolbox (branch `master`).

