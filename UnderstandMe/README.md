# Afrowave.Toolbox.UnderstandMe

UnderstandMe is a planned, modular localization, globalization, translation, and translation-workflow toolkit for .NET applications. It belongs to [Afrowave Toolbox](https://github.com/afrowaveltd/Toolbox).

> **Status: first offline localization core committed; translation and orchestration remain design-stage.** The .NET 10 project now contains standalone JSON localization code and a separate test project. GitHub Actions [run #39407132671](https://github.com/afrowaveltd/Toolbox/actions/runs/39407132671) passed **5,957 solution tests** (including **43 UnderstandMe tests**), with **zero build warnings/errors**, after adding standalone placeholder validation. See [first implementation](Docs/Localization/First-Implementation/en.md).

## Latest implementation: target locale repair scanning

The read-only `LocalizationTranslationScanner.Scan` compares source dictionary entries against one exact target locale. It reports missing entries and translations with invalid named placeholders without modifying source/target files, triggering network providers, or disturbing dictionaryless registration. A valid parent-locale fallback does not hide a missing exact target translation. Scheduling, retries/backoff and candidate publishing remain future integration work. New scanner tests have been committed; CI verification is pending. See [implementation status](IMPLEMENTATION_STATUS.md).

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

## Restoring inline formatting after contextual translation

A proposed strategy for nested HTML/Markdown is to translate **the full sentence for meaning**, separately obtain alternative translations of highlighted inner text, find a reliable matching span in the target-language sentence and restore the original safe markup around it. Example: `Hello <strong>beautiful</strong> world!` → `Ahoj <strong>nádherný</strong> světe!`. Exact matching alone fails with inflection, repeated words or paraphrase, so ambiguous matches must not lead to wrong markup. When supported, a single validated markup-preserving provider call is an alternative; additional translation calls are subject to cost/privacy/latency limits. **Design proposal, not implemented.** [Inline Markup Restoration](Docs/Inline-Markup-Restoration/en.md).

## Nested inline formatting failure policy

If translated inner-word variants cannot be matched to a full-sentence translation, a further optional attempt translates the **surrounding prefix/suffix** and uses their target-language matches (including inflection) to infer the highlighted span. If the match remains uncertain, **default to retaining the full translated text while omitting only harmless cosmetic tags** such as `strong`/`i`. **Never silently omit an inline tag with `id` or another significant function/binding**: return a typed structural error and let the host apply its configured safe fallback. Extra attempts, stricter styles and spending limits are configurable. See [Inline Markup Restoration](Docs/Inline-Markup-Restoration/en.md).

## Assisted HTML/document translation in Afutrans

Some translation providers accept **HTML directly**, avoiding much tag reconstruction; their results still require validation of structure, IDs, links and placeholders. For **complex HTML**, an explicit format-specific assisted profile may **prefer a capable enabled AI model** when authorized and within budget, without changing ordinary plain-text routing or requiring AI. No model is assumed perfectly faithful.

Afutrans can offer a **side-by-side editor**: original document rendered left; translated document right with **render / HTML source / split** modes, linked segment selection and visible untranslated/error regions. An authorized user can repair text or format alignment manually, request an alternative for a selected region or invoke **opt-in AI assistance** and inspect the proposal before accepting. Draft revisions prevent late automatic responses from overwriting user edits; publishing/export requires validated structure. [Assisted Document Translation](Docs/Assisted-Document-Translation/en.md).

## Manual-first completion and automatically saved editing

An authorized Afutrans user must **always have a manual route to enter or repair a translation**, regardless of machine/AI provider failures, missing engines or unsupported format-specific translation. In the responsive assisted editor, click/tap and edit **directly in the rendered translated panel or its HTML/Markdown source**, with both views synchronized to one draft (wide side-by-side or simple stacked/mobile switch).

Changes appear immediately and **autosave in the background** without a mandatory Save button. Show real save/failed/conflict states; keep incomplete HTML editable as a draft while protecting valid preview/publish. Every committed logical edit has recoverable **revision history**, and reverting an old state creates a **new revision**, never erasing the trail. A stale AI/provider result may not overwrite manual edits. This is an explicit authorized editor, not a Human fallback from private P0 RealTime; private-chat recipient edits retain their separate cache/no-cache rules. See [Direct Editing, Autosave and Revisions](Docs/Direct-Editing-Autosave-and-Revisions/en.md).

## Word-like WYSIWYG and live AI edits

Afutrans' assisted editor is **WYSIWYG like Word** for translated text: select a span and press `Ctrl+B`/`Ctrl+I` (or a formatting button) to insert safe `strong`/`em` markup. The **Source** view supports direct HTML/Markdown editing with code-editor conveniences, sharing the same document. Every **manual** edit resets a **1,000 ms inactivity timer**; after 1 second of no further changes, autosave begins in the background and creates a recoverable logical revision. Revert never erases history; invalid in-progress HTML can save as a draft without passing export validation.

Optionally, an authorized user may invoke an **AI agent to edit the chosen document/region visibly as it works**, with revision-scoped live patches, pause/stop, inspect/accept and a grouped revert. The human retains final control; newer manual edits cannot be overwritten by stale agent updates. Agent-specific checkpoints do not create a revision for every token. AI remains optional. [Direct Editing, Autosave and Revisions](Docs/Direct-Editing-Autosave-and-Revisions/en.md).

## Transfer source formatting by selecting translated text

Afutrans assisted WYSIWYG lets the user **click formatted text in the original**, remember its enclosing parsed element **with complete original attributes** (`class`, `id`, `data-*`, etc.) and **drag-select the corresponding translated words**. The editor safely wraps those words in the original element without changing their translation, even when automatic markup alignment failed. Nested source elements offer an ancestor choice; tag/attribute copy must validate faithful `id`/attribute copying, safe wrapping and preview security **without policing duplicate IDs in source or target** rather than silently discarding important properties. The action autosaves after the usual one-second idle delay and is fully undoable. [Source Tag Transfer](Docs/Source-Tag-Transfer/en.md).

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
- [Layered localization, optional grammar and composable inline hints](Docs/Grammar-Hints-and-Variants/en.md)
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

### Dynamic queue lanes and Bring Your Own providers (planned)

Hosted Afutrans may isolate `AfrowaveInternal`, `CommunityFree` and customer-specific `TenantDedicated` queue lanes, **each with its own worker capacity**. Commercial tenants do **not** buy priority over free community work; they rent their own LibreTranslate VPS or connect their own compatible LibreTranslate, Google/Microsoft, Ollama/AI backends without compulsory VPS fee. QueueMaster retains P0–P4 **inside** each dynamically managed lane. Tenant security, physical resource limits, privacy and provider-billing ownership remain enforceable. See [Queue Lanes and Tenant-Owned Providers](Docs/Queue-Lanes-and-Tenant-Owned-Providers/en.md).

### Opportunistic backend sharing and honest free queues (planned)

CommunityFree is intended to **slow down, not exhaust arbitrary monthly translation credits**: accept explicit durable deferred work while finite queue/storage and abuse-safe admission permit; provide clear status/backpressure when genuinely full. Protected AfrowaveInternal, CommunityFree and TenantDedicated queue lanes may optionally borrow **idle** worker buffer slots from an owner who explicitly permits sharing, including surplus internal realtime-capable worker capacity or a tenant's contributed AI/LibreTranslate backend. Borrowed resources are **nonguaranteed**, safe to reclaim at future buffer boundaries and always subject to content-owner privacy, provider-owner access/cost policy and reserved internal capacity. Customer-hosted engines may connect via authenticated outbound agent without exposing inbound ports. See [Capacity Lending and Backpressure](Docs/Capacity-Lending-and-Backpressure/en.md).

### Afrowave community-first access and verifiable use of contributions (planned)

Shared Afrowave translation service is designed for **maximum feasible free community access with equal terms from day one and permanently**, not paid queue-jumping, arbitrary credits or personal profit. Slow, fairly process deferred work when capacity is busy; reserve genuine overload/backpressure for actual service protection. All optional contributions are allocated to real and disclosed operating value (VPS/LibreTranslate, AI provider credits, network/backups) with public private-data-safe **last-cent** financial reconciliation and donor-specific receipts; unspent funds remain explicitly held. No billing or payment dependency enters UnderstandMe. See [Afrowave Community Access and Transparent Funding](../Docs/Afrowave-Community-Access-and-Transparent-Funding/en.md).

### Strict named-placeholder validation (first implementation slice)

`LocalizationPlaceholderValidator.HasMatchingPlaceholders(source, candidate)` checks that a proposed translation contains **exactly the original named placeholders including repeated counts**, regardless of order. It understands escaped `{{`/`}}` braces and rejects malformed bracket sequences. This standalone opt-in utility is for future translation admission and does not change dictionaryless registration, JSON files or existing localizer API. See [Localization first implementation](Docs/Localization/First-Implementation/en.md).

### Verified placeholder validator tests

The optional named-placeholder validator and its 19 new test cases passed in GitHub Actions [run #39407132671](https://github.com/afrowaveltd/Toolbox/actions/runs/39407132671): **43 UnderstandMe and 5,957 total tests, zero warnings/errors**. It does not yet enforce validity on existing dictionary entries.

### Invalid localized placeholders: safe fallback in the runtime

`DictionaryTextLocalizer` now defaults to **strict validation** of localized `{name}` placeholders (same exact names and occurrence counts as the source template). Missing, extra, renamed or malformed target placeholders cause fallback to a valid parent locale or original source template **before caller values are inserted**, so useful runtime values are not lost. Use `LocalizationOptions.ValidateLocalizedPlaceholders=false` only for intentional legacy opt-out. The localizer remains completely offline and non-destructive; it does not erase dictionaryless entries. Future optional async rendered-text rescue, error events, incidents and Afutrans retranslation are design only. [Details](Docs/Localization/Invalid-Placeholder-Fallback/en.md).

### Focused invalid target-translation repair hook (.NET 10)

`ITextLocalizer.InvalidateTranslation(culture, key)` can remove just one invalid target-culture key/value, **never its source or any unrelated translation**. The file JSON dictionary store supports safe conditional removal when its observed value has not changed; read-only adapters need not implement the optional write interface. Strict runtime placeholder validation can invoke this automatically (default; disable with `LocalizationOptions.AutoInvalidateInvalidTargetTranslations=false`) while immediately returning a correct parent/source fallback. A future Afutrans scanner will notice the missing locale key even when source keys are unchanged and regenerate only that target translation; **the scanner/event publishing and online rendered-sentence rescue are not implemented yet**. See [Invalid Placeholder Fallback](Docs/Localization/Invalid-Placeholder-Fallback/en.md).
