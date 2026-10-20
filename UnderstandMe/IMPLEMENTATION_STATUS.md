# UnderstandMe implementation status

Last updated: 2026-10-11  
Phase: **first offline localization source committed; most UnderstandMe modules are still design-stage**.

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

## 2026-10-10 — inline markup restoration via variant alignment (design proposal, not finalized)

- Recorded a potential strategy for nested HTML/Markdown formatting based on the user's example: `Hello <strong>beautiful</strong> world!` → separately translate inner `beautiful` (candidate outputs `nádherný`, `krásný`, `fascinující` where available), translate the **whole untagged sentence** `Hello beautiful world!` for context, identify the matching target span and reattach `<strong>` to yield `Ahoj <strong>nádherný</strong> světe!`.
- This approach differs from splitting each DOM text node into a standalone translation, preserving target-language syntax and word order. It is one **optional `VariantAlignment` strategy**, not a mandated two-call protocol: `ProtectedMarkup` / validated provider-supported HTML/inline-marker translation may use one call instead.
- Identified edge cases: Czech morphology (`krásný`/`krásného`), duplicate target expressions, paraphrase, nested/overlapping/crossing target spans, nontranslatable content and functional links. Restore formatting **only on reliable alignment**; never wrap arbitrary matches. A cosmetic formatting warning/plain translation may suffice for emphasis; lost functional links require safe structure-preserving/source fallback.
- Additional inner translations incur provider costs/latency and require permission under current ClientPolicy, quota hard caps, privacy and deadline; no automatic paid overage and no `P0` Human fallback. `ResultList` stays source-filled initially and holds verified text separately from safe structural markup annotations.
- Created `Docs/Inline-Markup-Restoration/en.md`; updated `Docs/Structural-Template-and-ResultList/en.md`, `Docs/Semantic-Splitter/en.md`, `Docs/Translation-Chains/en.md`, `Docs/Request-Settings-and-Live-Translation/en.md`, `Notes.md`, `README.md`, `Docs/Open-Questions/en.md` and this status. **Design documentation only: no runtime implementation, package or executable tests modified or run.** Open contract choices recorded as questions 104–107.

## 2026-10-10 — staged outer-anchor alignment, cosmetic omission and strict IDs (agreed design refinements only)

- Refinement of nested inline formatting restoration: after attempting **inner-text candidate alignment**, optionally translate **the text before and after the nested element** and search for unique, reliable target-language outer anchors inside the authoritative whole-sentence translation. Example: `Hello` -> `Ahoj`, `world` -> `svět` may help locate the center span of `Ahoj super světe!` when the matcher understands `svět/světe`; only wrap a span with sufficient evidence. Do **not** synthesize/overwrite the whole translation using alternatives returned by inner-fragment calls.
- **Default if restoration remains uncertain:** remove only safe **cosmetic** inline markup (`i`, `em`, `strong` etc. with **no significant attributes**), preserving the translated text and outer structural HTML, and report nonfatal `FormattingOmitted`.
- **Default if an unmatched nested tag has `id` or required semantic/reference/link/interactive/accessibility bindings:** return a typed **`MarkupRestorationFailed`** structural error rather than silently omit or misplace the identifier. The host decides on safe original/source preservation or an explicitly marked partial preview; no successful validated HTML claim on structure failure.
- Configurable markup restoration/failure Settings determine whether to try protected marker mode, inner alternatives, outer anchors, other eligible providers and whether cosmetic formatting also requires strict preservation. Every extra translation call remains bounded by administrator/client policy, privacy, provider quota/cost limits and deadline. `P0` never falls back to Human.
- Updated `Docs/Inline-Markup-Restoration/en.md`, `Docs/Structural-Template-and-ResultList/en.md`, `Docs/Semantic-Splitter/en.md`, `Docs/Translation-Chains/en.md`, `Docs/Request-Settings-and-Live-Translation/en.md`, `Notes.md`, `README.md`, `Docs/Open-Questions/en.md` and this status. **Design-only changes; no source files, runtime implementation or executable tests changed or run.** New detailed questions 108-111.

## 2026-10-10 — native HTML providers and Afutrans assisted document editing (agreed design only)

- Refined provider routing by **actual HTML/structured-format capabilities**. Some registered providers can translate HTML directly with less reconstruction; response must still be parsed and validated for nested markup, `id`, links, functional attributes and protected placeholders. A structurally valid-looking native HTML or AI translation is not automatically verified correct.
- For **complex HTML**, an **opt-in/configured Afutrans assisted-document provider profile** may prefer eligible **AI models** able to use full context; no hard-coded mandatory AI, automatic cost escalation or change to ordinary text's existing free/local -> paid -> AI preference. Every route obeys ClientPolicy, global admin rules, privacy, capabilities and current paid account quotas.
- Planned **assisted Afutrans HTML editor**: rendered original left; translated output right with **rendered preview / HTML source / split** modes and mapped source/result segments, including untranslated/source-preserved regions, formatting warnings and structural errors.
- An authorized editor may correct text/HTML and missing inline tag placement themselves, choose alternative translation, request **selective opt-in AI help** for a chosen region, review and accept suggestions, undo, validate, save drafts and export. A late AI/provider completion must not overwrite newer manual revisions.
- Retain existing **source-initialized ResultList**, HTML-safe rendering, validation of required `id`/links before qualified publish and typed partial outcomes. Safe preview must not execute arbitrary original/translated scripts or remote resources. Assisted editor is **explicit and non-P0**, not generic Human queue fallback or audit of private chats; separate from mandatory UI dictionary edit audit and private recipient self-correction.
- Created `Docs/Assisted-Document-Translation/en.md`; updated `Docs/Editor-and-Audit/en.md`, `Docs/Afutrans-Integration/en.md`, `Docs/Provider-Routing/en.md`, `Docs/Inline-Markup-Restoration/en.md`, `Docs/Structural-Template-and-ResultList/en.md`, `Docs/Translation-Chains/en.md`, `Docs/Architecture/en.md`, `Docs/Request-Settings-and-Live-Translation/en.md`, `README.md`, `Notes.md`, `Docs/Open-Questions/en.md` and this status. **Documentation/design only: no runtime code or executable tests changed or executed**. Questions 112-116 remain for interface/behavior design.

## 2026-10-10 — always-available manual completion and direct autosaved revisioned Afutrans editor (agreed design)

- User established the **no-dead-end translation principle**: even when automatic/AI providers are absent, over quota, unable to translate a segment, or cannot restore structured HTML, **an authorized user must have a system-level manual route** to input/repair target content. Not every request mode needs inline manual editing. Human-only remains a valid installation; unimplemented format-specific adapters can still allow a manual text/source draft without claiming validated structure.
- Afutrans assisted editor must be **simple and responsive**: rendered original left / translated right on wide screens; narrow screens use stacked or switchable `Original | Translation | Source` views. The user can **click/tap and directly replace translated text in both rendered Preview and HTML/Markdown source** with one synchronized host-owned draft; no mandatory editor popup or manual Save button.
- **Automatically save every accepted direct edit on the background persistence path**. Show immediate optimistic local text and honest durable acknowledgement with `Editing/Saving/Saved/Retrying/Conflict/Failed` status. Bounded debounce/batching and flush are allowed; never report Saved before storage confirms and never silently discard pending edits after transient failure. No false claim about crash/offline durability without implemented storage.
- Each **committed logical edit enters immutable host-owned version/revision history** with actor, time, before/after or a recoverable diff. Provide simple history comparison, undo and **revert as a new revision** (never rewrite older versions). Stale provider/AI completions or conflicting concurrent edits cannot silently overwrite newer manual draft work.
- Temporarily invalid HTML source is a valid **editable autosaved draft**, with parsing diagnostics/last known safe preview; fully validated publishing/export must still enforce important IDs, links, placeholders and structure. This manual edit capability must not bypass privacy/authorization, turn P0 RealTime into Human fallback or globally expose private chat text.
- Created `Docs/Direct-Editing-Autosave-and-Revisions/en.md`; updated `Docs/Assisted-Document-Translation/en.md`, `Docs/Editor-and-Audit/en.md`, `Docs/Afutrans-Integration/en.md`, `Docs/Architecture/en.md`, `Docs/Structural-Template-and-ResultList/en.md`, `Docs/Request-Settings-and-Live-Translation/en.md`, `Notes.md`, `README.md`, `Docs/Open-Questions/en.md` and this implementation status. **Design/documentation only; no runtime implementation, packages or automated tests changed/executed.** Detailed implementation questions 117–121 recorded.

## 2026-10-10 — WYSIWYG shortcuts, 1-second manual autosave and visible AI editor agent (agreed design)

- Refined Afutrans assisted editor into two synchronized presentations over one target draft: **Word-like WYSIWYG** (drag-select a target span, use **Ctrl+B** to insert/toggle `strong`, **Ctrl+I** for `em`, platform equivalents/contextual toolbar) and **VS Code-style Source** (direct HTML/Markdown, syntax highlighting, automatic indentation/tag pairing and inline errors). Safe AST/selection mapping retains meaningful structural identifiers/links; UI remains responsive on narrow screens.
- **Manual edit autosave idle delay is fixed at exactly 1,000 ms after the most recent accepted manual change**, resetting on every keystroke/formatting operation; earlier blur/navigation/view-change flushes allowed. A save starts at idle expiry (not guaranteed to complete in one second); `Saved` is shown **only after durable acknowledgement**. IME/composition and unfinished HTML are handled without loss, and invalid interim source remains savable as an editable draft, not publishable valid HTML.
- Manual typing bursts become comprehensible, attributable, reversible logical history revisions; restore/revert creates a new revision. Ordinary editing never requires a Save button. Both rendered and source views share one draft, and the original document stays recoverable.
- **Optional AI editing agent** inspired by an IDE agent may be **explicitly invoked** to visibly and progressively modify the selected translated document/region. Changes are structured/revision-scoped, attributed to an agent, displayed with review/diff state, and subject to user **pause/stop/accept/manual override/grouped revert**. Agent edits may have internal draft checkpoints instead of per-token committed revisions; never overwrite newer manual work or publish without valid document structure/user control. All AI cost, client permissions and private content restrictions still apply.
- Updated `Docs/Direct-Editing-Autosave-and-Revisions/en.md`, `Docs/Assisted-Document-Translation/en.md`, `Docs/Editor-and-Audit/en.md`, `Docs/Afutrans-Integration/en.md`, `Docs/Architecture/en.md`, `Notes.md`, `README.md`, `Docs/Open-Questions/en.md` and this status. **Design documentation only, with no .NET implementation or executable tests run.** Detailed outstanding contracts/tests enumerated as questions 122–126.

## 2026-10-10 — source tag pickup and manual target-range wrapping (agreed design only)

- Afutrans WYSIWYG can let an authorized user **click text in the rendered original**, choose its enclosing inline tag (or suitable ancestor) and temporarily capture the **entire parsed original element shell**: tag name, attributes including `class`, `id`, `data-*`, other permitted attributes and matching closing semantics. The user **drag-selects the intended translated words** on the right, and the host format adapter wraps that text with the copied element **without changing the translated wording**. It is a single undoable/revisioned manual operation with the agreed **1-second after-last-manual-change autosave**; no AI/provider call is required.
- Security and structure remain distinct: no unintended active scripts/event handlers in preview and no illegal void/block tag wrapping around an arbitrary inline range. Preserve the source tag's identity and attributes, show a comprehensible error when faithful/safe wrapping is not possible, and keep source/translated previews isolated. Created `Docs/Source-Tag-Transfer/en.md` and updated the direct editing, assisted editor, structural template, markup restoration, Afutrans, README, Notes and open questions.

## 2026-10-10 — source ID duplication is NOT a translation validation error (agreed correction)

- **Superseded** earlier proposed target-only `id` uniqueness enforcement. Source HTML can be malformed, including **multiple elements with the same `id`**; translating or copying its tags must **never fail merely because a source or target ID is duplicated**. Do **not** add ID-uniqueness checks to the translation pipeline, manual tag transfer, autosave, draft validation or export/publish gate.
- Preserve copied `id` and other source attributes **verbatim**; do not silently rewrite, renumber, deduplicate or delete them. Translation is responsible for faithful transfer, not repairing/linting the source HTML. Any future diagnostic of duplicate identifiers is a **separate optional HTML quality tool**, not a required translation step.
- Keep only actual **structural insertion safety and preview security** controls needed to avoid breaking editing or executing untrusted content; these checks must not be disguised ID linting. Corrected `Docs/Source-Tag-Transfer/en.md`, `Docs/Direct-Editing-Autosave-and-Revisions/en.md`, `Docs/Assisted-Document-Translation/en.md`, `Docs/Structural-Template-and-ResultList/en.md`, `Docs/Inline-Markup-Restoration/en.md`, `Docs/Editor-and-Audit/en.md`, `Docs/Afutrans-Integration/en.md`, `Notes.md`, `README.md`, and `Docs/Open-Questions/en.md`.
- All changes are **design documentation only**; no .NET source/tests were modified or executed. Resume implementation from the latest master and this status, not superseded duplicate-ID checks.

## 2026-10-10 — future document normalizer separated from translation (scope note only)

- Reserved HTML/document validation and normalization for a **separate future Toolbox document-processing tool**, not UnderstandMe. Illustrative future behavior: case-sensitive processing of source IDs, optional normalized `lowercase_underscore` format (`UserProfile` -> `user_profile`), and matching updates/validation for references in related JavaScript (`document.getElementById("UserProfile")` -> `document.getElementById("user_profile")`). The actual tool is **not being designed or implemented now**.
- **UnderstandMe must preserve original IDs and attributes exactly as received**, and must not check duplicate-ID uniqueness, rename IDs, rewrite related JavaScript or block translations merely because source HTML has errors. This supersedes any older translation-side normalization/linting suggestions.
- Updated `Docs/Source-Tag-Transfer/en.md` and this status only. Documentation/scope note; no runtime code, tests or new document-tool project added.

## 2026-10-11 — layered grammar and hint contract captured (design only)

- Audited the latest GitHub state of Toolbox UnderstandMe and Afutrans, including the CLDR data commit. Established that dictionary-less and placeholder requirements were already described, but the newer *flat dictionary + optional grammar/hint layers* and backwards-compatible indexer overload were not recorded together.
- Added `Docs/Grammar-Hints-and-Variants/en.md` as the continuation specification: dictionary-less must work even without sidecars/CLDR/network; base `en.json`/`cs.json` stay flat; translator-facing placeholder hints, runtime selector hints, grammar variant/override stores and versioned metadata remain separate.
- Captured preserving the existing `params string[] args` API and adding a typed/combinable-hints-before-`params` overload, one common resolver, .NET 10 compatibility and optional use of newer C# unions. Exact public types and representation remain **to be finalized**.
- Recorded optional CLDR-driven generation, strict placeholder validation, editor corrections per locale, verified-safe deduplication, source-language grammar use and resilient fallbacks. The linked Afutrans repository already has pinned CLDR 48.2.3 source JSON; it does **not** implement the CLDR interpreter or grammar engine.
- **Verification:** checked upstream repository file contents and latest heads; this change is documentation only. **No build/tests run**, no runtime or .NET API implemented.

## Next continuation

1. Continue discussing unresolved design questions using `Docs/Open-Questions/en.md`; do not finalize the implementation plan prematurely.
2. Agree on minimal common object/interface contracts, QueueMaster ownership/atomicity and translator buffer semantics.
3. Decide the net10/net11 API packaging strategy and existing Toolbox dependency baselines.
4. Start with the smallest dictionary-less + flat JSON + protected placeholder vertical slice and backwards-compatible localization overload, as recorded in [Layered Grammar and Hints](Docs/Grammar-Hints-and-Variants/en.md). Keep CLDR and sidecars optional.
5. Define testable increments and only then create the formal implementation plan and begin code, tests and per-change status updates.

Source of truth: https://github.com/afrowaveltd/Toolbox (branch `master`).


## 2026-10-18 — first code slice: JSON localizer

- Created the net10 standalone `Localization` core: ITextLocalizer, ILocalizationDictionaryStore, FileJsonDictionaryStore, JsonTextLocalizer, LocalizationOptions.
- Added UnderstandMe.Tests with 14 xUnit tests and registered it in Toolbox.sln.
- Supports locale fallback, first-start key registration into flat source JSON, named placeholders, explicit Reload, and safe per-instance file writes.
- Dita remains fully separate and was used only as inspiration. Neither Dita nor Afutrans (including user-authored WebTranslator) was changed.
- Placeholders variants, auxiliary JSON, file-watching, automatic translation, CLDR, REST/SignalR and Afutrans integration remain future steps.
- **Verification:** dotnet SDK was not available in the editing environment. Code and tests were committed, but not compiled or executed locally. Do not call them GREEN until verified.
- [Interfaces, usage, limitations and test instructions](Docs/Localization/First-Implementation/en.md).

## 2026-10-18 — verified GitHub Actions baseline

- Verified GitHub Actions [run #39167610994](https://github.com/afrowaveltd/Toolbox/actions/runs/39167610994), commit `b5ec307506fbde97608b35dfff9cc14d5620bd10` on `master`.
- Restore, solution build and test phases **all succeeded**, with **zero build warnings**.
- **UnderstandMe.Tests: 14 passed / 0 failed.** Other solution suites: 1475 + 3198 + 1241 passed, for **5928 tests passed total**.
- The runner used .NET 10 on Ubuntu; this validates the initial localizer APIs and tests, **not Afutrans runtime integration, .NET 11 nor Rust**.
- Next implementation step: named-placeholder structure checks and sidecar context/variant metadata, then Afutrans adapter without editing the user-built WebTranslator.

## 2026-10-19 — storage-agnostic localization decision and code refinement

- Confirmed the original `ILocalizationDictionaryStore` is the minimum backend-neutral contract. A local JSON file is one adapter for Afutrans; future UnderstandMe backends may be memory, databases, remote services, or Node.js/remote nodes.
- Renamed the unreleased runtime `JsonTextLocalizer` to `DictionaryTextLocalizer`, reflecting its existing interface-only dependency. No Dita or Afutrans source changes.
- Added one new in-memory store xUnit test for source registration, target lookup and catalog refresh, independent of files/JSON.
- Future async dictionary-source and optional writer/notification capabilities are planned but **not implemented**. Remote refresh must not block synchronous UI localization.
- The initial 14/14 tests were verified previously; this rename/15th test requires an independent CI check before claiming success.

## 2026-10-19 — verified storage-neutral refactor and XML documentation follow-up

- Verified GitHub Actions run [#39315844408](https://github.com/afrowaveltd/Toolbox/actions/runs/39315844408): solution build and test steps successful.
- **UnderstandMe 15/15 tests passed**, including `Memory_store_works_without_json_files_or_translation_engines`. Toolbox total **5,929 tests passed** (15 + 1475 + 3198 + 1241).
- The run emitted **7 CS1591 XML documentation warnings** related to new public constructors/methods, not runtime failures. XML docs were added in this follow-up commit; their warning resolution awaits its CI build.
- Core APIs remain synchronous local/cached; future remote/DB sources require separate async contracts. No remote provider was implemented.

## 2026-10-19 — verified XML documentation clean-up (final CI)

- Verified [GitHub Actions run #39316162045](https://github.com/afrowaveltd/Toolbox/actions/runs/39316162045) for source commit `fe3bd64ba8b6e4c3cd7b367644c5a9f84319502a`: restore/build/test workflow completed successfully.
- **Build: 0 warnings, 0 errors.** The seven prior CS1591 warnings were resolved by XML documentation on public methods and constructors.
- **UnderstandMe: 15/15 passed. Toolbox total: 5,929/5,929 passed** (15 + 1475 + 3198 + 1241).
- Scope: `DictionaryTextLocalizer` + pluggable synchronous store, in-memory test backend, JSON file adapter. No real remote DB/Node backend, no automatic translate, no Afutrans integration or Rust port has yet been delivered.

## 2026-10-19 — logical dictionary/metadata provider contract across localization and translation

- Confirmed a universal UnderstandMe requirement: dictionaries and associated hint/grammar metadata are **logical resource categories**, not necessarily separate JSON files. Afutrans uses flat JSON plus auxiliary JSON; other hosts may use SQL (one/multiple tables), remote services, custom document stores or other nodes.
- Both localization and translation/editor workflows **must use common provider contracts** to access these resources rather than building independent file/SQL/HTTP paths with diverging semantics.
- Keep transport, persistence, dictionary representation and metadata representation independent. Coherent source-key/culture/domain identity, optional revisions and prevention of lost manual edits belong to the contract.
- Existing file and in-memory adapters remain valid; **metadata abstraction, DB/remote implementations, async refresh and coherent resource revisioning are planned, not implemented**. The already verified 15 tests/5,929 Toolbox tests remain the last confirmed runtime baseline; this update is documentation only.
- Updated `Docs/Architecture/en.md`, `Docs/Localization/en.md` and `Docs/Grammar-Hints-and-Variants/en.md` to preserve this cross-module rule.

## 2026-10-19 — optional event/callback change notifications

- Confirmed dictionary change detection/publishing is an **optional provider or localization-service capability**, not a mandatory requirement for every DataAccessor or localizer. Suitable sources include an accepted DataAccessor write, dictionary-less registration, and adapter-specific out-of-band file/SQL change detection.
- Clarified `DictionaryChanged` as a *logical notification*, with independently selected delivery via C# events/callbacks, Messenger, authenticated REST callbacks or business SignalR; desktops with bundled read-only catalogs need no publisher/listener, while online desktops may opt in.
- A single accepted change should be reported once logically, with stable IDs/revisions to deduplicate listeners. Source key additions may trigger authorized automatic translation via QueueMaster; updates to translated target values must not recursively enqueue translation. Cache refresh and external detection are distinct event semantics.
- Event delivery is best-effort unless supported by an appropriate durable change-log/outbox and reconciliation process; publishing or subscribing cannot be required for successful writes, local lookup, or dictionaryless startup.
- Design-only documentation updated in Architecture, Localization and Afutrans Integration. No new runtime notifier, database trigger, REST/SignalR API or tests were added. Last verified code baseline remains 15/15 UnderstandMe and 5,929/5,929 Toolbox tests.

## 2026-10-19 — configurable change delivery and translation-service polling

- Confirmed **fully pluggable host-owned configuration**: appsettings/IConfiguration, constructor/typed options, dependency injection, custom providers or each application's own settings backend. Source selection, provider settings, notification enabled/disabled, selected transports and scan policy are independent choices.
- Confirmed **scheduled pull discovery** as a parallel/alternative to push events. A translation-service-owned scanner can periodically inspect SQL or other dictionary/metadata stores that never emit notifications, compare revisions or snapshots and submit eligible work through shared contracts.
- Push and poll must converge on deduplicated/idempotent admission; the scanner may not be a second translator or QueueMaster priority scheduler. Reconciliation and checkpoint handling must not drop accepted source changes when scans fail.
- Respect Afutrans Scheduler design (persist `NextRunAtUtc`, delay after startup, rest interval from completion, no self-overlap, finish on accepted work handoff). Intervals per target are configurable and polling may be fully disabled.
- Added `Docs/Localization/Configuration-and-Change-Detection/en.md` and linked Architecture, Localization and Afutrans Integration. Documentation only; no configuration-source/notification/polling interfaces or implementation/tests were added. Last verified code baseline: UnderstandMe 15/15, Toolbox 5,929/5,929.

## 2026-10-19 — source snapshot comparison, per-target language and obsolete-key handling

- Documented per-target source culture (en, cs, etc.) and SourceText/dictionaryless vs Symbolic key strategy. SourceText source entries use key=value. Symbolic source values must be compared in addition to keys and trigger stale-revision translation/review when changed.
- Planned per-target last successfully reconciled source snapshot/checkpoint (full dictionary for small targets; revision/change feed/key+value hashes for large SQL/API targets). Advance only after durable work admission, with push/poll idempotency and conflict handling.
- Clarified that dictionary comparison **cannot detect an unused key still present** in a dictionaryless source catalog. Proposed optional build/version key manifests, batched anonymous per-key last-seen evidence, and explicit removal decisions.
- Proposed Active → SuspectedUnused → Archived reversible lifecycle; physical Purged state optional and **off by default**, especially protecting rare messages and offline/older deployments.
- Added detailed design and test cases in Docs/Localization/Dictionary-Snapshots-and-Key-Lifecycle/en.md, linked scanner/localization/Afutrans docs. **Documentation only; no source or tests changed**. Last verified baseline remains UnderstandMe 15/15, Toolbox 5,929/5,929.

## 2026-10-19 — retain dictionaryless phrases; cross-project signature search

- **Supersedes** the previous proposed `LastSeen` / SuspectedUnused / Archived / Purged lifecycle **for dictionaryless SourceText entries**. Once a source phrase is discovered, keep it and its translations even if no app currently references it; it can be reused in other authorized applications.
- Proposed optional `FindInAnyDictionary` capability against authorized dictionaries or shared translation memory, using exact lookup then canonical signature comparison that ignores placeholder **names**, not literal text, count, repeated identity, format or compatible grammar/context requirements. Example `Hello {user}` = `Hello {customer}` structurally (`Hello {#0}`).
- Target-language shared templates need validated canonical slot remapping to the consuming project's placeholder names, preserving reordering and occurrence constraints. Manual project overrides remain authoritative. Source/default locale may differ per target.
- Keep Symbolic key source-value revision detection. SourceText rewording creates a new key but does not remove old dictionaryless translations. Physical removal is a separate explicitly authorized action, not an inactivity policy.
- Updated source snapshot and configuration docs to supersede older retirement recommendations; added `Docs/Localization/Shared-Translation-Reuse-and-Placeholder-Signatures/en.md`. **Documentation only, not implemented or tested.** Last tested baseline remains 15/15 UnderstandMe and 5,929/5,929 Toolbox.

## 2026-10-19 — shared SQL dictionary, case-insensitive candidates, hint-aware reuse

- Confirmed a **shared backing SQL dictionary** may serve multiple applications natively through provider contracts, independently of Afutrans. Individual applications still have their own source languages, manual overrides, scopes and permissions.
- Planned Smart key matching: exact project key first, optionally case-insensitive canonical candidate with source locale and ambiguity checks (e.g. `US`/`us`), then placeholder-normalized signature; translation casing follows **target-language presentation policy** rather than naive title-casing.
- Cross-project reuse must compare relevant placeholder hints, default/illustrative values, allowed-value lists, grammatical gender/case/number/person, target locale and context. Divergent finite-value sets do not automatically share, even if structural signatures match. Numeric grammar is **locale dependent**, not universally identical across languages.
- Allow optional per-entry `ProjectOnly`/no-share metadata plus independent application/tenant access controls; opt-out matters for special domain wording, approved overrides, private translations and legal/licensing constraints.
- Deterministic alphabetic JSON write order remains useful for Git/review, but `.NET Dictionary` already provides hash-based runtime lookup. Optional SQL/canonical-signature indices and bounded revision-aware remote caches should be benchmarked with realistic 60,000-item workloads and multiple languages.
- Added `Docs/Localization/Shared-Dictionary-Indexing-and-Casing/en.md` and cross-links to signature, grammar and localization docs. **Documentation only:** no schema, interface, search algorithm, cache code or new tests; last observed verified code baseline remains UnderstandMe 15/15 and Toolbox 5,929/5,929.

## 2026-10-19 — key Capitalization policies and pluggable article stores

- Proposed `Capitalization` matching enum (`Smart=0`, `Exact=1`, `IgnoreCase=2`); `Smart` is the default. Exact precedence and collision checks remain; target presentation capitalization is a separate, locale-dependent responsibility.
- Recorded SQL, MongoDB and future indexed AjisFS as interchangeable storage providers. AjisFS indexed storage is a design target, not implemented. Do not generalize database speed without reproducible benchmarks.
- Designed host-side `IArticleContentStore`, `IArticleMetadataStore`, optional `IArticleQuery` and publication capabilities. ArticleId/culture/revision/content and localized title/date metadata may be stored in folders (`en.md`, `cs.md`, `metadata.json`), SQL rows, MongoDB documents, AjisFS or hybrid storage.
- Translation core stays blind to storage location and format; source revision protection, safe coherent publish and protection for manual edits are required. Articles are not automatically promoted into global UI localization dictionaries.
- Added Docs/Document-Storage-and-Translation/en.md and updated Architecture, Afutrans Integration and shared dictionary matching notes. **Documentation only; no code/tests changed.** Last verified baseline: UnderstandMe 15/15, Toolbox 5,929/5,929.

## 2026-10-19 — proposal: relocate reusable CLDR reference data into optional shared module

- Rechecked GitHub: `UnderstandMe` currently has no `Jsons` directory; official pinned **CLDR 48.2.3** resources, provenance manifest and Unicode license are in `Afutrans/Afutrans.Core/Jsons/Cldr`.
- Proposed one canonical shared optional Toolbox language-rule module (`UnderstandMe.Cldr` or equivalent) using a neutral `ILanguageRulesProvider` contract. UnderstandMe can use it for plural/ordinal/range/locale rule selection; Afutrans for grammar variant generation, translating/editing and import. Ordinary localization remains fully functional without it.
- Geographic/territory CLDR resources are potentially host-specific; their presence must not force every library consumer to load all data. Preserve license/manifest, override and source provenance.
- Migration must happen after provider/contract tests and consumer integration, not before. **No data or code was moved**, no compiler/runtime behavior changed and no new tests ran. Last verified baseline remains 15/15 UnderstandMe, 5,929 Toolbox.

## 2026-10-19 — optional shared language metadata and country-based language picker

- Rechecked Afutrans JSON: `languages.json` has 185 `code/name/native/optional rtl` entries, and CLDR 48.2.3 `territoryInfo.json` provides country-language associations including Uganda. Coverage differs; not every territory language has an entry in the smaller endonym file.
- Proposed optional `ILanguageCatalog`, `ILanguageTerritoryProvider`, provider-specific language availability and script-aware direction contracts. This supports native language names in a UI picker, locale resource discovery, LibreTranslate capability filtering and Uganda recommendations with **Show all** (e.g. French).
- Direction is tied to language+script where relevant; a coarse `rtl` flag must not blindly flip an app or individual option. Basic dictionaryless localization remains fully usable without catalog metadata.
- Added `Docs/Language-Catalog-and-Country-Selection/en.md` and crosslinks in Architecture and Grammar. **Documentation only**; no source data moved, no code or tests changed. Last verified Toolbox baseline 5,929/5,929, UnderstandMe 15/15.

## 2026-10-19 — editable language catalog JSON slice (verified code)

- Added `LanguageCatalogEntry`, `ILanguageCatalogReader`, optional `ILanguageCatalogEditor`, and `FileJsonLanguageCatalogStore` under `UnderstandMe/LanguageCatalog`. Works with existing 185-language array format from Afutrans (without moving/changing its source JSON), and supports independent read, upsert/edit and explicit removal.
- JSON adapter preserves unknown custom fields while editing, sorts entries by code, detects case-only duplicate tags, rejects invalid JSON and unsafe tags, writes via same-directory temp file replacement. Writes are synchronized within the store instance, **not** cross-process transactions.
- Added nine LanguageCatalog xUnit tests, taking the UnderstandMe suite from 15 to **24**. Fixed earlier faulty initializer in `DictionaryTextLocalizer` that prevented previous CI builds.
- Verified [GitHub Actions #39329364298](https://github.com/afrowaveltd/Toolbox/actions/runs/39329364298): UnderstandMe **24/24**; whole Toolbox **5,938/5,938** (24 + 1,475 + 3,198 + 1,241); **0 warnings, 0 errors**.
- Evaluated future broader sources: official Unicode CLDR `cldr-localenames-full` for multilingual display names, IANA Language Subtag Registry for valid registered tags/aliases. CLDR English names are **not** guaranteed endonyms; preserve manually supplied native names and script/RTL metadata.
- Next: importer with preview, provenance, conflict resolution, user override layer and deletion tombstones; optionally interactive editor/CLI and pluggable SQL stores. These are **not** implemented and must not overwrite manual adjustments during upstream refreshes. See Docs/Language-Catalog-and-Country-Selection/en.md.

## 2026-10-19 — additional pinned CLDR JSON inventory in Afutrans

- Confirmed and linked 19 more source-preserving Unicode CLDR 48.2.3 JSON datasets in `Afutrans.Core/Jsons/Cldr` for language/territory/script names, layout direction, number systems and week conventions.
- [Separate additional manifest](https://github.com/afrowaveltd/Afutrans/blob/main/Afutrans.Core/Jsons/Cldr/manifest-additional.json) identifies every original upstream path and Git blob SHA; the original Afutrans `languages.json` and CLDR data remain untouched.
- These are reference files, not automatically active in UnderstandMe; reuse through a future tested optional language catalog / CLDR provider without making dictionaryless startup depend on them.
- Documentation only in Toolbox; Afutrans JSON assets have been committed separately. No Toolbox source/tests changed; last verified Toolbox run remains 5,938/5,938.

## 2026-10-20 — language picker ready before application serves requests

- Inspected the historical public [`AfrowaveWebPublic/Id`](https://github.com/afrowaveltd/AfrowaveWebPublic/tree/master/Id) bootstrapping pattern and the user's explicit requirement: language names/options should be discovered, registered and sufficiently translated **before the app starts serving users**, not on first page render.
- Proposed optional host-owned, readiness-aware language selector bootstrap: discover `Locales/*.json`, normalize valid language IDs, resolve endonyms/CLDR fallback, register new phrases, submit missing name translation work according to configured policy, and atomically activate a coherent snapshot. Later locale/provider additions use independent reconciliation and snapshot swap.
- Separate default `LocalReady` (complete locally usable selection, offline safe) and opt-in strict/bounded `RequireLocalizedLabels` startup readiness so a failed network translator cannot accidentally hang every installation. Translation/country-label preparation must not run inside the core localizer, language JSON store or Razor dropdown.
- Keep naming aligned with existing user patterns (`I...Manager`, `I...Service`, `...Async`, constructor DI, small interfaces); exact bootstrap API names deliberately remain open. Existing old `TranslateLanguageNamesAsync` implementation loops over **countries** despite the method name; new implementation must distinguish both responsibilities.
- Updated `Docs/Language-Catalog-and-Country-Selection/en.md` and `Docs/Architecture/en.md`. **Documentation only, not implemented or tested**. Last independently verified functional baseline remains UnderstandMe 24/24 and Toolbox 5,938/5,938.

## 2026-10-20 — optional Id app/service registry boundary for Afutrans

- Captured the user's platform architecture: Id provides one first-party identity, app/brand registry and separately scoped service grants; Afutrans may be a registered service used by Id and explicitly allowed third-party community apps.
- Registry visibility/service application metadata **does not authorize** translations, reveal tenant dictionaries, grant system privileges, or give Id permission to modify another app's roles. Core `ClientContext`/`ClientPolicy` enforcement applies identically to business REST/SignalR and direct use.
- App-owned first-party session cookies with an Id-only login session replace cross-domain cookie sharing. Translation/API clients authenticate separately; optional SFTP/SQL/API target connectors have owner-scoped secret references and read/write capabilities; no plaintext untrusted FTP.
- Linked [Afutrans service integration design](https://github.com/afrowaveltd/Afutrans/blob/main/Docs/Registered-Service-Integration/en.md) and updated Toolbox Id review, UnderstandMe Afutrans integration and Client-Access policy. **Documentation only, no code or tests modified.** Last previously verified Toolbox functional suite remains 5,938/5,938.

## 2026-10-20 — Id HA, owner-controlled insecure sources, shared community corpus

- Id must be a logically singular, highly available identity/registry/entitlement source of truth with at least two independent service instances, replicated/fenced database writes and quorum/witness design; disaster recovery requires independently tested backups, key continuity, documented failover/RPO/RTO, and application-owned local-session continuity rules. A mere two-way unsupervised master/master mirror risks split brain.
- Application owners may explicitly opt in to insecure test FTP/HTTP source connectors with visible disclosure. Connector isolation and platform/tenant credential boundaries are mandatory; Id SSO and authorization-code endpoints remain encrypted. Reference: [Docs/Id-High-Availability-and-Replication/en.md](../Docs/Id-High-Availability-and-Replication/en.md).
- Shared 'translation of everything' is a logical cross-project index of **owner-published or otherwise authorized** compatible phrases, not automatic access to private customer dictionaries. Afutrans discloses its technical access to connected content and offers per-entry/eligible-whole-dictionary sharing opt-in; provenance, context, placeholders, licensing, review and manual overrides remain intact.
- Added cross-project requirements in shared translation reuse, privacy, Afutrans integration, Id review and [Afutrans community sharing design](https://github.com/afrowaveltd/Afutrans/blob/main/Docs/Connector-Trust-and-Community-Dictionaries/en.md). **Documents only; no runtime APIs, deployment or tests changed.** Last separately verified Toolbox functional baseline remains 5,938/5,938.

