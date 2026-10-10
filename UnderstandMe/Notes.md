# UnderstandMe — Working Design Notes

Last reviewed: 2026-10-10  
Status: **pre-implementation design discussion**; this document consolidates the currently agreed intent. It is not a finalized specification or an implementation plan.

## 1. Vision and scope

UnderstandMe provides modular, reusable .NET tooling for localization, globalization, translation, dictionary management, and translation automation. **Afutrans** is a configurable application/service built on these libraries: a locally run tool for one developer or a persistent server handling many Translation Targets. WebUI is the first practical Afutrans host; Avalonia GUI and Terminal.Gui v2 TUI are planned as additional interfaces.

A host may use only the localization components, only manual translation, only machine translation, or a full pipeline. Nothing in the UnderstandMe core must require Afutrans, an AI service, a particular UI or one fixed processing chain.

Core principles: single responsibility; interfaces and capabilities rather than concrete dependencies; no circular implementation dependencies; configurable/overridable policies; standard typed C# models across interfaces; no unnecessary format conversions; understandable defaults and extensible modules.

## 2. Module boundaries (conceptual, NOT final project/folder layout)

- **QueueMaster (Core / Orchestrator)**: accepts logical batches and items, owns five global priority queues and job-state transitions, manages optional persistence and recovery, and gives the next eligible item to a translator buffer requesting work.
- **DataAccess**: raw bidirectional connectors for local or remote files, SQL, APIs, FTP, SignalR-connected systems, and other locations. Read/write/watch capabilities depend on the provider and permissions. Source and destination can differ.
- **TextSource**: interprets material received via DataAccess or direct API input; recognizes supported text/document/dictionary formats; determines what should be translated, protects format structure, supplies policies, and creates requests for QueueMaster. It does not own separate priority queues.
- **Translation workers**: provider instances (LibreTranslate, Google, Microsoft, Ollama, other AI systems, or a human worker) that accept compatible requests and return results or failures. No worker owns the global queue.
- **Translation Chain / step processors**: configurable composition of splitter, translator, assembler, language/style reviewer, semantic validator, and additional community stages.
- **ResultProcessor**: reconstructs documents, restores placeholders and protected structure, validates completed outputs, and returns results or routes them through DataAccess.
- **Editor**: first-class interactive dictionary management; can fetch source and target dictionaries through DataAccess, compare and modify entries, request alternative machine/AI results, and create its own background improvement batches through QueueMaster. Separately, a human can join the worker pool with language capabilities and a long-running FIFO buffer.
- **Messenger**: strictly reports/routes events, progress and status to authorized subscribers, locally or via SignalR/API. It NEVER assigns work, selects translators or schedules jobs.
- **Localization**: ASP.NET/.NET `IStringLocalizer` integration, optional middleware, dictionary-less key discovery, storage adapters, placeholder hints and cache.

Each area exposes stable interfaces. Community implementations may plug into appropriate roles without QueueMaster knowing their concrete type.

## 3. QueueMaster and translator buffering

Five priority levels are currently envisaged: `RealTime` (P0), `Interactive` (P1), `High` (P2), `Normal` (P3), `Background` (P4). Names other than RealTime are provisional.

The global queues own waiting work. Every translator endpoint has a **small, configurable FIFO buffer** (illustrative capacity: five items). A buffer requests **one** new item when a slot is available; QueueMaster selects the highest-priority compatible waiting item. No jumping, preemption, or priority rearrangement *within* a translator buffer. Faster endpoints naturally request and receive more work; no throughput-weighted dispatcher is required.

`AddedToQueue` fires **only** when an individual priority queue transitions atomically from empty to nonempty. It wakes idle buffers without a broadcast for every enqueue. A configurable polling check (initial idea: 500 ms) protects against missed notifications. The orchestrator checks all priorities and compatibility when a buffer asks; the event itself need not select a priority.

Translator endpoint readiness has only one externally visible state variable: `IsReady` (boolean). Supported language pairs, buffer occupancy, rate limits and capabilities are separate data. Failed endpoints stop receiving new work and undergo periodic recovery checks. A short provider outage may use configurable retries (historical LibreTranslate experiment: 2-second interval, 10 attempts). Eligible work in a stalled buffer may later be returned/reassigned to other compatible endpoints, with ownership guards to avoid duplicate publication. Exact retry/reassignment algorithms are NOT yet specified.

P0 RealTime work is deliberately volatile and short lived: do not persist every live translation request. Other tasks may use SQL-backed job persistence according to their policy. Registering new localization keys is independent of retaining the P0 job.

## 4. Job, Pipe and Chain configuration

`TranslationRequest`, `TranslationJob`, job items, translation results and their alternatives are distinct concepts. One logical job can contain many items; one item/step can yield multiple candidate results, including arrays and structured objects instead of a single string.

`Translation Pipe` = routing/execution policy, **not** a separate physical queue. Suggested composable flags: `Persist`, `Preserve`, `CacheResult`, `Deduplicate`, `RetryOnFailure`, `ValidatePlaceholders`, `AllowFallback`, `Batchable`, `NotifyOnCompletion`, `RequireAcknowledgement`, `RequireReview`, `LocalOnly`. Keep priority as a single enum and limits/durations as ordinary options; the exact flag set is not frozen.

Pipes can be named/reused globally (`FastUI`, `ArticleAI`, `DictionarySync`, `ManualReview`, etc.), overridden by Translation Target and, where authorized, by a single request. No request override may bypass the target's security constraints.

`Translation Chain` specifies which compatible processing steps run and in what order. It supports one composed AI prompt; translate + final AI validation; or split → translate → assemble → AI language review → semantic verification. Prompts and prompt fragments are editable/versionable; a Prompt Composer combines language, format, meaning/tone, style, protected content and expected output instructions. A user may select a custom prompt for an individual job. Exact workflows are UI-configurable rather than hardcoded to one use case.

Articles and comments are split into coherent chunks, translated (potentially in parallel), reassembled in order, then receive a **mandatory final AI correction/check** according to their content policy. Review receives source and translation. Content policies distinguish faithful informal/explicit comments from more polished documentation/articles without changing meaning or tone except where explicit editorial changes are allowed. Document structure and protected tokens require verification. A failed mandatory check does not silently publish an unchecked result.

## 5. Localization and dictionary-less start

Use standard .NET localization concepts. Hosts configure default culture, dictionaries/storage, target languages, ignored/manual-only languages, live translation and monitoring. For dictionary-less mode, a missing source key is registered in the default dictionary where `KEY = VALUE`. General UnderstandMe should not mandate English; Afutrans and participating Afutrans modules conventionally use English source keys.

Lookup flow: find KEY in target dictionary; if absent check/register default-language KEY and placeholder hints if genuinely new; request a **single fast translation into the current user's language** only on a suitable cache miss. Cache it as **provisional** and return it promptly, or fall back to source text on failure/timeout. Background translation independently produces authoritative dictionary entries for configured languages.

After provisional delivery, subsequent lookup/refresh checks may promote the cache to a final dictionary value. `Update_Cache` (local or SignalR) notifies clients after a committed translation change and can invalidate/refresh entries earlier. Periodic full reconciliation (illustrative default ~3 hours) and startup/reconnect catch-up protect against missed events. Cache keys must distinguish target/source identity, language, key and relevant translation context.

A flat JSON dictionary may be ```json
{ "We found {count} dictionaries in {folderCount} folders": "We found {count} dictionaries in {folderCount} folders" }
```
and `placeholders.json` may be ```json
{ "We found {count} dictionaries in {folderCount} folders": ["5", "2"] }
```
Hints follow occurrence order; missing/extra hints are validated and skip the current automatic attempt. Original placeholder text and identities are preserved in memory; translation-time markers identify occurrences by original index (NOT by translated order). The selected protection method may be HTML, tokens, differential translation, or AI repair according to provider capabilities. Every accepted result must retain each expected placeholder occurrence exactly once, even after source-to-target reordering. Human editing remains available. Grammatical pluralization is not a required part of the initial scope.

The `IStringLocalizer` synchronous API and live asynchronous translation have different timing contracts; their integration requires explicit design, without blocking normal lookups indefinitely.

## 6. Persistence, lifecycle and cache

Default operational storage for standalone Afutrans: SQLite. Larger deployments may use pluggable EF Core database providers (e.g. PostgreSQL or SQL Server). The **internal operational database** is separate from DataAccess connectors that read/write client dictionaries or articles.

`Persist` keeps recoverable job state; `Preserve` optionally stores every intermediate step/output (pre-split, chunks, translated chunks, assembly, AI review/manual revision) in `StepHistory`. Independent `LifeTime`/retention policies can clear transient or obsolete content without deleting the final client-owned result. Suggested modes include transient, until client acceptance, time-limited, until source deletion, or explicit/manual retention; precise names/options remain open.

`FinalizeAndFlush` represents acknowledged handoff plus controlled cleanup of a job's disposable working data. Rollback is a **separate persistent, stoppable, auditable workflow**, able to return to a retained checkpoint and resume using a different backend/prompt/chain branch; it does not rewrite old history. Neither recovery nor rollback can recreate data already purged.

Cache is independent of work history and may retain reusable generic phrases (such as greetings), with context, source/target language, privacy scope and TTL respected. Do not retain private text just to increase hit rates.

## 7. Editor, mandatory audit and AI auditing

The Editor can load side-by-side source/target dictionaries and invoke AI or machine-translation alternatives without disrupting authoritative values. Optional improvement requests default to P4/background; their failure is nonfatal and changes nothing. A human translator worker can advertise known languages and process its own long-running buffer.

**All committed manual dictionary edits must be durably audited**, independently of `Preserve`, job cleanup, and UI technology. Audit includes actor, timestamp, source and target, key, before/after values, revision lineage, and restore actions. Restoring an earlier revision creates a NEW revision. Auditing belongs to the controlled write path, so API/community editors cannot bypass it.

An optional AI Audit plugin compares the current human-edited translation to the ORIGINAL source (not the previous translation) to detect substantive changes in meaning, omissions, additions or disallowed tone changes. Policy can report, create an incident, request human review, suggest a fix or perform an explicitly authorized corrective edit. Findings are model assessments, not proof of malice; AI corrections also become audited revisions.

## 8. Communication and monitoring

Messenger is an information-delivery layer only. SignalR supports progress, `Update_Cache` and client/server requests. Durable messages use unique IDs, acknowledgement after durable acceptance, an Outbox/Inbox-style replay/deduplication mechanism and catch-up on reconnection; do not equate network ACK with completed translation. Ephemeral RealTime requests and lightweight telemetry do not need the same persistence guarantees.

Admin monitoring may display authorized text, client-targeted progress, queues, buffers, utilization, failures and histories. A separate anonymous public dashboard may expose aggregate work animation/counters ONLY, without client identity or content. Client subscribers see only their authorized jobs. UI administration uses dedicated configuration services, not Messenger acting as a scheduler.

## 9. Platform and serialization direction

Prefer Microsoft/.NET tools when they meet requirements; `System.Text.Json` is the default. No mandatory Newtonsoft.Json dependency; AJIS is a possible future interchangeable format adapter. Exchange typed C# objects, not universal JSON trees.

UnderstandMe starts from a `net10.0` skeleton. The design direction is shared behavior and stable .NET 10-compatible contracts, with optional modern compile-time API/facades/features for `net11.0` (e.g. C# unions). Afutrans hosts target .NET 11. Public result/failure conventions should reuse Essentials and WhenItFails rather than invent a parallel error subsystem. Detailed packaging, native-union API shape and framework-targeting strategy remain open.

## 10. Future native translation worker and upstream collaboration

**Design exploration only; not an implementation commitment.** Current Afutrans/UnderstandMe translation will continue to support self-hosted LibreTranslate endpoints. A future independently developed C#/.NET native translation worker could consume compatible Argos language-model packages via CTranslate2 and tokenization/segmentation components, without copying LibreTranslate's HTTP/server implementation. It would be available as an in-process library or an isolated worker service, with language-pair capabilities, per-instance model inventories, memory/concurrency budgets, optional model unload/reload, and automatic recovery after ordinary failures. For native crashes, an external supervisor/process restart and durable queue ownership recovery are required; in-process C# exception handling alone cannot guarantee recovery. Model overhead, stability and throughput must be **benchmarked**, not assumed to improve merely by replacing Python.

Multiple worker instances could hold **different subsets of directional translation models** (not arbitrarily ten languages each) across virtual machines or physical hosts. QueueMaster routes only to compatible ready instances; indirect language-pair translation can be composed in a Chain when explicitly permitted. Preserve the current minimal endpoint readiness contract (`IsReady`) and separate capability metadata.

### Open-source conduct and provenance

This work should complement rather than impersonate or copy the work of LibreTranslate and Argos Translate. Clearly acknowledge the upstream projects, link to their repositories, document which components and models we actually use, and offer reusable .NET integration, reliability improvements, interoperability or upstream contributions to their maintainers when useful. Maintain truthful attribution even if the new module has independently written code.

At the time of this note, **LibreTranslate source is AGPL-3.0**; **Argos Translate is MIT/CC0 dual-licensed**; and **CTranslate2 is MIT-licensed**. Distinguish use of an independent model/runtime integration from copying or modifying LibreTranslate source. Do not assume that every downloaded `.argosmodel` package has a sufficiently clear redistribution license just because its tooling is open source: verify the provenance, rights, bundled dependencies, attribution requirements and distribution terms of each shipped model. Record third-party notices and consult legal review for any planned mixed-license distribution or network-service derivative.

References:
- LibreTranslate: https://github.com/LibreTranslate/LibreTranslate
- Argos Translate: https://github.com/argosopentech/argos-translate
- CTranslate2: https://github.com/OpenNMT/CTranslate2
- Open discussion of individual model-package licensing: https://github.com/argosopentech/argos-translate/issues/533

### AfroLT as a complementary future service

AfroLT is a prospective **independently implemented** .NET translation library/HTTP service, not a replacement for LibreTranslate. Keep **LibreTranslate first-class**, especially for HTML translations and placeholder-protected markup, where duplicating mature behavior is unnecessary. AfroLT may implement similar user-facing features by different engineering choices (bounded memory/concurrency, model subsets, fault isolation, recovery and clean .NET integration).

Two new desired host features: a lightweight interactive translation web page with **external CSS/theme support**, and an HTTP API documented using **Microsoft's OpenAPI generator with Scalar UI**, not a required Swagger UI/Swashbuckle stack. On .NET 11 the current Microsoft OpenAPI default is 3.2; allow emitting 3.1 when client tooling needs it. The web translator, administrative controls and API reference are separate surfaces. Precise service endpoints, theme loading, security and compatibility remain open questions.

See [AfroLT future design](Docs/AfroLT-Future-Design/en.md) for the standalone proposal and upstream references.


### Versioned documentation, older clients, and suggestions

AfroLT API docs should offer `/api-docs` -> `/api-docs/latest` plus persistent `/api-docs/v3.2/`, `/api-docs/v3.1/` and corresponding machine-readable specifications. OpenAPI specification versions are distinct from AfroLT HTTP API versions. Maintain usable older API routes/adapters and ordinary language-neutral JSON contracts; a historical documentation page alone does not guarantee runtime compatibility.

Preserve the LibreTranslate-inspired `Suggest a better translation` feature. Ingest proposals into a separate store with optional human/AI quality review against the original. Approved suggestions may become auditable translation-memory overrides for immediate use or enter a separately curated future model-training dataset. Submission does not directly retrain or overwrite deployed model weights. See [AfroLT future design](Docs/AfroLT-Future-Design/en.md).

### AfroLT independence and three interface options

**AfroLT must be standalone-capable.** Neither UnderstandMe nor Afutrans is a mandatory dependency. One translation/model/capacity/recovery core can be exposed through direct .NET calls, versioned HTTP REST/JSON + OpenAPI/Scalar, and an optional self-contained SignalR Hub for persistent bidirectional client communication (jobs, progress, completion). A separate optional integration adapter connects it to UnderstandMe QueueMaster and Messenger; the core never depends on those external modules. Use plain typed wire DTOs; native unions stay an internal .NET 11 API concern. REST remains available without SignalR, and embedded mode needs no web server. See [AfroLT design](Docs/AfroLT-Future-Design/en.md).

### One command model across REST and SignalR

AfroLT's direct .NET API, REST endpoints and its optional single SignalR Hub are thin entrances into the **same application command handlers**. Commands such as translation, supported languages, status and suggesting corrections accept/return the same logical typed request/response DTO models regardless of transport. The application service, not the adapter, owns validation, authorization policy, idempotency, model selection, recovery and error semantics. HTTP status/headers, Hub method replies and push events are only delivery-specific envelopes; a queued job may be polled by REST or followed over SignalR using the same job ID. Verify parity with shared command tests and transport-level contract tests.

See [AfroLT future design](Docs/AfroLT-Future-Design/en.md).

### Administrative SignalR Hub and optional monitoring

Alongside the one public translation Hub, AfroLT Hosting may expose an explicitly authorized **\`/admin\` SignalR Hub** for live events and operational monitoring. Translation requests, business commands and DTOs remain shared with the same underlying application services, not separately implemented per Hub. Operational sources publish success/failure, worker/model readiness, recovery and capacity events regardless of subscribers. On connection, an authorized monitor receives a snapshot followed by live deltas. No subscribers means no translation interruption. Important errors and audit events persist independently of SignalR; high-volume telemetry may be ephemeral. Privileged administrative actions (model reload, maintenance etc.) are executed through separate authorized common command handlers, not by the event publisher. Protect per-client data and admin access. See [AfroLT future design](Docs/AfroLT-Future-Design/en.md).


### SignalR is a best-effort broadcast, not an archive

SignalR is an optional live transmission channel, comparable to an FM radio: observers may listen, ask questions and receive answers, but the engine must not depend on their presence or replies. Messages that **may be lost** (telemetry, progress, monitoring event broadcasts) use best-effort SignalR and can be dropped when no client is listening. Work, results, incidents or edit changes that **must not be lost** are committed/acknowledged in the responsible durable subsystem, independently of their optional SignalR notification. Failure to deliver/acknowledge an event, or a missing response to a client request, must not stall, cancel, undo or redirect the underlying accepted work. Request retries must use idempotency/command identity to avoid accidental double execution. See [AfroLT future design](Docs/AfroLT-Future-Design/en.md).

### Hierarchical SystemMonitor routing

`/SystemMonitor` aggregates events from all module publishers registered in one host. `/AfroLT/SystemMonitor`, `/Afutrans/SystemMonitor` and `/UnderstandMe/SystemMonitor` stream only their named modules. Each module emits one typed event, distributed to its module feed and global feed with the same envelope plus sender/instance identity and event ID. Separate hosts require an explicit bridge for remote aggregation. All these SignalR routes remain outbound-only, best-effort and independent of command processing or durable records. See [SystemMonitor](Docs/SystemMonitor/en.md).

## 11. Source repositories and document maturity

- Toolbox: https://github.com/afrowaveltd/Toolbox
- Afutrans: https://github.com/afrowaveltd/Afutrans

The current Afutrans WebTranslator is the practical first web-host skeleton, not a completed translation service. Only design documentation is being prepared now; no agreed behavior should be represented as already implemented.

For decisions needing further discussion see [Open Questions](Docs/Open-Questions/en.md); for the verified continuation point see [Implementation Status](IMPLEMENTATION_STATUS.md). Detailed topic notes are indexed in [README](README.md).

