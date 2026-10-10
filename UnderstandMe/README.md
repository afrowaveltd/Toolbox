# Afrowave.Toolbox.UnderstandMe

UnderstandMe is a planned, modular localization, globalization, translation, and translation-workflow toolkit for .NET applications. It belongs to [Afrowave Toolbox](https://github.com/afrowaveltd/Toolbox).

> **Status: design / preparation.** The repository currently contains a .NET 10 class-library skeleton. This README describes the intended system, **not implemented features**.

## Purpose

Provide reusable building blocks for software localization, automatic and human translation, document and dictionary processing, and configurable work orchestration. Consumers can use a small subset or assemble a complete translation service. [Afutrans](https://github.com/afrowaveltd/Afutrans) is a planned full-featured host and editor built from these building blocks; it is not a prerequisite for using UnderstandMe.

## Design principles

- One responsibility per module, explicit interfaces, replaceable implementations and no implementation-level circular dependencies.
- A `QueueMaster` core manages five priority queues, job ownership, durable recovery when enabled, and demand-driven dispatch to translators' small FIFO buffers.
- A `Messenger` distributes events and authorized status information; it does **not** assign work.
- Translation Chains compose configurable translation, splitting, assembly, review, and validation steps.
- Standard .NET localization, optional dictionary-less registration, configurable storage and short-lived RealTime translation are first-class use cases.
- **Human translation is the mandatory baseline provider capability.** All machine/AI translation providers (LibreTranslate, AfroLT, Google, Microsoft, Ollama and others) are optional integrations that administrators activate individually; a human-only deployment is valid.
- Provider priority is separate from job priority; self-hosted LibreTranslate, paid APIs and AI engines are selectable per policy, target and permitted request overrides.
- Prefer Microsoft/.NET components when appropriate, including `System.Text.Json`, while keeping external formats and translation providers pluggable.
- Core/library compatibility starts at .NET 10; .NET 11-specific features and API surfaces are design topics, not yet implemented.

At admission the orchestrator **normalizes a client's requested provider and settings under administrator/system constraints**. An explicit Google API preference can be replaced by an eligible allowed backend when that provider is forbidden, unavailable or has exhausted its hard monthly quota. A valid request is rejected only when no authorized compatible translation route exists, with a precise reason. Acceptance is not completion: the eventual response identifies the actual provider used and any substitutions, without ever allowing a request flag to bypass spending caps or orchestrator pauses.

## Request-specific behavior

A request resolves typed effective `Settings` from configured defaults and permitted overrides, with **queue priority separate from behavior flags and provider priority**. `RealTime` plus `Mandatory` may immediately use a currently available acceptable translation backend; ordinary RealTime may return original content and update translated paragraphs as segments finish. Admin queue pause/stop takes precedence, and existing requests retain their accepted policy when configuration changes. Optional bounded `SystemPersistent` approved translation memory can be checked before selecting a provider, without growing an unbounded store.

The default provider preference is economical/free or local translation (such as LibreTranslate), then equally ranked paid APIs, then AI, all configurable globally or for particular language pairs. Model capabilities can be retested on version changes, while historical model quality scores are not used for automatic permanent ranking. See [Request Settings, memory and live segment updates](Docs/Request-Settings-and-Live-Translation/en.md).

The splitter's `(RequestId, SegmentId)` partial translation model is **UI-neutral**: applications may use Blazor, JavaScript, Avalonia or another rendering mechanism; no DOM/JS function is a dependency. Paid provider adapters declare pricing/metering capabilities, while each configured subscription account holds its own monthly included usage, optional overage price and an administrator-approved hard/soft limit. Accounting and quota reservations are persistent and independent of SystemMonitor; no automatic extra package purchase occurs.

## Client access and public API policies

Optional per-client `ClientPolicy` settings determine which enabled provider families, endpoints or paid API accounts each trusted client may use, along with their own word/cost quotas and request rate/concurrency limits. **Ordinary known clients are unrestricted by default at the client layer**, while administrator/system hard limits still apply; a separately configured public/anonymous profile can share unused self-hosted LibreTranslate capacity **without exposing paid APIs**. Provider-account budgets and client-plan quotas are enforced independently. An explicit Google request from a client not allowed to use it is rerouted to another permitted suitable engine where feasible.

No publicly accessible translation listener is automatically exposed merely by using the library; authenticated client context and business API entitlement are independent from the minimal GUID identity of the outbound-only SystemMonitor. See [Client Access Policies](Docs/Client-Access-Policies/en.md).

## Provider activation is optional, public entitlements are configurable

The host may configure **zero automatic translation engines** and operate exclusively with human translators. The library must support human-work assignment, manual results and relevant validation, though it cannot guarantee immediate human availability. Default free/local -> paid -> AI ordering ranks **only enabled machine backends**; it never enables a vendor or forces credentials/paid calls. Every provider other than Human is an optional feature.

Ordinary known clients have no extra default provider restrictions beyond global host policy. Public APIs are opt-in, and their client profile is **administrator-selectable**: free/local only, human only, selected paid/AI providers or fully open access to all enabled providers, with the configured global budgets and resource limits. A generous public operator is supported just as much as a strictly offline/human-only operator. See [Client Access Policies](Docs/Client-Access-Policies/en.md) and [Provider Routing](Docs/Provider-Routing/en.md).

A Human worker can process **durably queued translation requests whenever a translator opens an interface**, even days later. The UI shows waiting and claimed Human work in existing P1-P4 priorities and eligible languages (P0 is automatic-only/inactive when only Human is configured); QueueMaster grants exclusive claims, tracks completion and recovers interrupted work. No extra human queue, continuously connected person or machine provider is required. There is no automatic Human follow-up from RealTime; any manual work is submitted separately as a durable non-P0 request.

## RealTime execution scope

`P0 RealTime` uses **automatic providers only**. In a Human-only installation P0 is inactive and a real-time request returns original content immediately. `Mandatory` does not change that or add work to the Human backlog. Human translation requires a distinct authorized, persistent **P1-P4 request**. See [Human Translation Workflow](Docs/Human-Translation-Workflow/en.md).

## Privacy and translation history reuse

Request `Privacy` is separate from queue priority, provider choice and result persistence. A **Private** chat/forum translation can be retained for future authorized history views without keeping its volatile P0 execution alive. Saved translations are restricted to the original source's tenant/conversation/message and current readership permissions, not shared globally or exposed to Human translators, manual auditors or general administrative UI. Automatic checks and permitted processing providers remain configurable. A source change, deletion or lost access must invalidate/restrict the cached translation. See [Privacy, access-scoped translations and retention](Docs/Translation-Privacy-and-Scoped-Storage/en.md).

## Chat translation and recipient corrections

UI/application localization dictionaries use `KEY=VALUE`. **Chats and user-generated messages do not.** Their Splitter chunk IDs `0..N` exist only for matching results during a request; the translation engine returns translated chunks while the **consuming chat application assembles the full text, displays it and owns saved history**.

After an automatic private translation, the **authorized recipient** may optionally correct their own displayed version in a host UI. The correction may remain session-only (**no cache**) or be cached at that recipient's **explicit choice**; broader sharing, if offered, requires a separate authorized choice. This is not the generic Human translator queue or manual audit. `P0` remains volatile and machine-only, even when the chat application retains its finished translation outside the engine. See [Chat translation flow and recipient corrections](Docs/Chat-Translation-Flow/en.md).

## Meaningful document and message splitting

The Splitter prefers **semantic chunks**, not arbitrary length: short text can stay intact, formatted plain text usually splits by paragraph, unformatted continuous prose by sentence, Markdown by meaningful document nodes and HTML by translatable element content. Configurable `Settings` controls the mode; provider limits may trigger safe secondary splitting. Temporary `0..N` IDs are used only to match chunks, and the caller reconstructs the translated document or chat message. See [Semantic Splitter](Docs/Semantic-Splitter/en.md).

## Structural templates and progressive translations

For HTML/Markdown, a proposed representation keeps a **parsed structural template** with request-local references to a `ResultList`. Each slot begins with **the original source text**, so uncompleted parts remain visible; verified translations update slots progressively without changing the document layout. Prefer AST/DOM mapping rather than inserting literal synthetic `placeholder` attributes into user HTML; nested formatting remains protected and rendered safely. Design proposal, not yet final public API. See [Structural Template and ResultList](Docs/Structural-Template-and-ResultList/en.md).

## Planned areas

| Area | Responsibility |
| --- | --- |
| QueueMaster | Prioritized work management, persistence abstraction, recovery, buffer dispatch |
| DataAccess | Read/write access to files, databases, APIs and other stores |
| TextSource | Understand input formats and generate translation requests |
| Translation/Chains | Interchangeable workers, prompt composition and processing sequences |
| ResultProcessor | Restore protected content, assemble and route outputs |
| Editor | Compare/edit dictionaries, request suggestions, provide human translation |
| Messenger | Local/remote event delivery and monitoring projection |
| Localization | Standard .NET localization, dictionary-less registration, placeholders and caching |

## Documentation

- [Working requirements and agreed principles](Notes.md)
- [Architecture and module boundaries](Docs/Architecture/en.md)
- [Localization and placeholders](Docs/Localization/en.md)
- [QueueMaster, scheduling and reliability](Docs/QueueMaster-and-Reliability/en.md)
- [Human translation worker and durable on-demand backlog](Docs/Human-Translation-Workflow/en.md)
- [Translation Chains and backend workers](Docs/Translation-Chains/en.md)
- [Orchestrator request admission and provider substitution](Docs/Request-Admission-and-Normalization/en.md)
- [Provider preferences, cost and escalation](Docs/Provider-Routing/en.md)
- [Provider pricing, monthly packages, quota monitoring and spending limits](Docs/Provider-Billing-and-Quotas/en.md)
- [Editor, audit and rollback](Docs/Editor-and-Audit/en.md)
- [Afutrans integration and interfaces](Docs/Afutrans-Integration/en.md)
- [AfroLT future lightweight .NET translation service](Docs/AfroLT-Future-Design/en.md)
- [Hierarchical SystemMonitor live-event routing](Docs/SystemMonitor/en.md)
- [Open design questions](Docs/Open-Questions/en.md)
- [Current implementation status and continuation point](IMPLEMENTATION_STATUS.md)

Only English documentation is maintained manually for now; later translations belong in the same topic directories as additional language-coded Markdown files.

## Current repository facts

`UnderstandMe.csproj` currently targets `net10.0` and declares package references to `Afrowave.Toolbox.Essentials 0.2.0` and `Afrowave.Toolbox.WhenItFails 0.1.0`. These are current project-file declarations, **not** the decided future dependency policy. No working translation engine, QueueMaster or integration tests are claimed here.

The current Afutrans codebase is an experimental application skeleton. Its existing web project and shared core target `net11.0`; future integration work is still to be designed.

**Source of truth:** [Toolbox GitHub repository](https://github.com/afrowaveltd/Toolbox).

