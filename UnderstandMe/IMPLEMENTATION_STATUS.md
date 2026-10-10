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

## Next continuation

1. Continue discussing unresolved design questions using `Docs/Open-Questions/en.md`; do not finalize the implementation plan prematurely.
2. Agree on minimal common object/interface contracts, QueueMaster ownership/atomicity and translator buffer semantics.
3. Decide the net10/net11 API packaging strategy and existing Toolbox dependency baselines.
4. Define testable increments and only then create the formal implementation plan and begin code, tests and per-change status updates.

Source of truth: https://github.com/afrowaveltd/Toolbox (branch `master`).

