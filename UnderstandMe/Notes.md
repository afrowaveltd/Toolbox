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

### Durable human backlog independent of translator login

**Human translation consumes pending queued work**, potentially long after requests were accepted. The translator UI does not need to be online for the orchestrator/QueueMaster to accept requests. Human-designated waiting items must be **durably saved** and visible later in the original five QueueMaster priority queues, with an authoritative per-priority/language count when a translator signs in. Human work is a normal worker path, not a sixth physical queue or independent scheduling service.

The authorized human chooses/claims compatible work via an **exclusive atomic QueueMaster reservation**. Pending items and in-progress states survive server/UI disconnect; a lease/recovery scheme avoids double assignment and stale submission overwriting newer work. Results follow normal validation, typed result and audit rules. The host/UI chooses whether to show all accessible work or filtered choices; neither an administrative SystemMonitor event nor a live translator connection owns work.

The earlier `P0 RealTime` volatile default still holds for instant requests. A human translation intended to be completed days later must be a **separately submitted and explicitly authorized durable non-P0 request**; P0 itself never spawns a human continuation and private RealTime text never enters a human queue by fallback. Human readiness has no requirement to be immediate. See [Human Translation Workflow](Docs/Human-Translation-Workflow/en.md).

## 4. Job, Pipe and Chain configuration

`TranslationRequest`, `TranslationJob`, job items, translation results and their alternatives are distinct concepts. One logical job can contain many items; one item/step can yield multiple candidate results, including arrays and structured objects instead of a single string.

`Translation Pipe` = routing/execution policy, **not** a separate physical queue. Suggested composable flags: `Persist`, `Preserve`, `CacheResult`, `Deduplicate`, `RetryOnFailure`, `ValidatePlaceholders`, `AllowFallback`, `Batchable`, `NotifyOnCompletion`, `RequireAcknowledgement`, `RequireReview`, `LocalOnly`. Keep priority as a single enum and limits/durations as ordinary options; the exact flag set is not frozen.

Pipes can be named/reused globally (`FastUI`, `ArticleAI`, `DictionarySync`, `ManualReview`, etc.), overridden by Translation Target and, where authorized, by a single request. No request override may bypass the target's security constraints.

`Translation Chain` specifies which compatible processing steps run and in what order. It supports one composed AI prompt; translate + final AI validation; or split → translate → assemble → AI language review → semantic verification. Prompts and prompt fragments are editable/versionable; a Prompt Composer combines language, format, meaning/tone, style, protected content and expected output instructions. A user may select a custom prompt for an individual job. Exact workflows are UI-configurable rather than hardcoded to one use case.

Articles and comments are split into coherent chunks, translated (potentially in parallel), reassembled in order, then receive a **mandatory final AI correction/check** according to their content policy. Review receives source and translation. Content policies distinguish faithful informal/explicit comments from more polished documentation/articles without changing meaning or tone except where explicit editorial changes are allowed. Document structure and protected tokens require verification. A failed mandatory check does not silently publish an unchecked result.

### Provider priorities, price-aware routing and bounded escalation

**Two separate priorities:** QueueMaster's P0-P4 priority determines *when* a request is handled; provider preference determines *which eligible translator* handles a Chain translation step. Provider selection is a Pipe-level policy, not a new physical queue or logic embedded in QueueMaster.

Self-hosted **LibreTranslate is a first-class economy/default provider** for installations prioritizing low direct translation costs, including this planned Afrowave deployment. But it is not forced on all users. An installation may prefer Google/Microsoft/AI even when LibreTranslate is available; another may require LocalOnly and prohibit remote API use. A provider family can have multiple configured endpoints, each with language/format/HTML/protected-token capabilities, capacity/readiness, restrictions and optional cost estimates. Unknown cost cannot safely be assumed zero.

Provider-selection profiles support explicit ordered preferences or cheapest eligible endpoint, with fallback/escalation enabled only under selected conditions (timeout/provider failure, incompatible capability, failed required placeholder/structure validation, configured semantic review or deliberate alternate result). Provider choice can be configured globally, by Pipe, per Translation Target or for allowed individual jobs and Chain steps; hard constraints for budget, privacy, formats and mandatory checks cannot be overridden. A different engine can perform quality review from the original source without taking over the initial translation. A superficially fluent result does not prove accuracy; quality-triggered escalation needs an actual configured validation/review mechanism.

Set maximum attempts, switching rules, deadlines, budget ceilings and chargeable timeout retry behavior to avoid cost explosion. Track provider selection, attempts, accepted result and estimated/actual costs without exposing original content or credentials in SystemMonitor. No provider has mandatory static enum cases or a hard-coded dependency. See [Provider routing and escalation](Docs/Provider-Routing/en.md).

### Agreed request Settings, Mandatory RealTime, segmented delivery and provider tiers

Request `Settings` are a key public configuration surface. Resolve an immutable **effective Settings snapshot at request creation** from system/Pipe/target defaults and allowed per-request overrides. Compose independent flags for compatible behaviors; the exclusive P0–P4 queue priority remains one value and is not a bit flag. Settings allow operators to configure endpoint ranking, language-specific priorities, cost and wait/fallback conditions, cache and persistent-memory behavior, split/delivery options and mandatory validation. Existing queued and running requests retain their captured policies when defaults change. Orchestrator-level administrative pause/stop overrides dispatch for every request, including Mandatory, without changing their stored priorities.

`RealTime | Mandatory`: rather than waiting for a temporarily busy preferred translator, choose the lowest-ranked *currently eligible and available* functioning endpoint permitted by the request's effective policy. Do not violate hard privacy, budget or supported-format constraints. If none exists, return an explicit unavailable/fallback outcome. Non-Mandatory RealTime can immediately return original untranslated content and asynchronously deliver verified segments when they finish.

The splitter creates an ordered `List<(SegmentId, Text)>` with IDs 0..N, together with formatting/position metadata. Each partial completion includes a separate `RequestId` to avoid collisions between documents, and a browser updates the matching paragraph/inline element. For plain text use DOM `textContent`, **not untrusted `innerHTML`**; rendered Markdown with formatting must be validated/sanitized, protected links/placeholders preserved and block-level markup wrapped by valid containers rather than an arbitrary span. Application-specific live completion notifications are **not SystemMonitor's administrative best-effort feed**; late/lost updates and session lifetime depend on the request durability/cache contract.

`SystemPersistent` is an optional **bounded, context-keyed, approved translation memory** checked before calling a translator. A request's permission to read it is distinct from permission to write/pin approved translations; ordinary transient cache and authoritative language dictionaries remain distinct. Do not store every produced translation indefinitely.

Default engine **tiers**: self-hosted/free (LibreTranslate or equivalent) -> paid translation APIs (**equal priority to each other by default**) -> AI. All tiers and language-pair preferences are administrator-configurable; no single vendor or paid API is mandatory. Automatic historical model-quality scores/rank downgrades are **not** part of the agreed initial policy because model versions change. Version-aware capability/placeholder tests are useful, but model-number magnitude is not comparable quality evidence across languages. Provider routing dry-run simulator remains a **deferred admin enhancement**, not a first implementation requirement.

See [Request Settings and Live Translation](Docs/Request-Settings-and-Live-Translation/en.md), [Provider Routing](Docs/Provider-Routing/en.md), and [QueueMaster](Docs/QueueMaster-and-Reliability/en.md).

### UI-neutral segments and paid provider accounting

**Clarification of earlier browser/JS examples:** the format-aware splitter's contract is solely to produce a stable ordered sequence `(SegmentId, Text)` with formatting/position metadata, and to correlate partial verified results by `RequestId` plus `SegmentId`. Each consumer chooses how to update its view: a Blazor component, JS DOM node, Avalonia view model or another interface. `innerHTML`, `span` and any JavaScript function were illustrations, **not required architecture**. Formatted output must still be safely rendered according to source format. User-facing incremental results are independent of administrative SystemMonitor.

**Billing is part of the configured translation provider/account model:** provider-specific charging units (words, characters, tokens, requests etc.) and supported metering rules are declared by the adapter, while the operator's account/endpoint plan supplies real negotiated prices, included packages, monthly renewals, authorized overage costs and limits. E.g. one account may include **1,000,000 words/month**, reducing its allowance as chargeable translations are dispatched and settled. Admin configures whether to stop at the hard limit, allow extra words at a configured price under a cap, or wait for manual purchase/registration of an additional package. The system does **not** decide to spend more or buy bundles on its own.

An independent durable UsageLedger/QuotaAccount tracks per-period usage, estimated versus authoritative costs and reserved units. **Chargeable dispatches must check current quotas and atomically reserve budget** across concurrent workers sharing the billing account, even for requests created before a newer admin spending restriction. Request provider preferences and job priority remain frozen when accepted; current account hard caps, like manual orchestration pause, are an operational guard on future dispatch. Unknown/timeout provider charges require tracked estimates rather than assuming zero billable use. Billing records outlive optional best-effort SystemMonitor notifications; the dashboard can display remaining quota, estimated/actual spending and approaching-limit warnings.

See [Provider Billing and Quotas](Docs/Provider-Billing-and-Quotas/en.md), [Provider Routing](Docs/Provider-Routing/en.md) and [Request Settings](Docs/Request-Settings-and-Live-Translation/en.md).

### Orchestrator-first request normalization and approved provider substitution

The **host orchestrator**, not the caller, is authoritative for a translation request's admissible priority, flags, privacy, provider list and financial limits. A caller may name Google API directly, but this is a preference, not an enforceable permission. At **admission**, preserve the original request, apply system/admin Settings, and capture a normalized effective request plus policy adjustment reasons. Choose a permissible candidate plan before queueing; a provider specified by the caller is not automatically the selected/used backend.

**Do not render a valid translation request useless just because the preferred engine cannot be used.** Default behavior is to try another compatible and authorized endpoint that satisfies the actual translation requirements. For example, if Google is not allowed, is unavailable or hits its 1,000,000-word monthly hard limit, attempt a suitable free/local translator or another admin-approved provider. No new paid billable attempt against the exhausted account is permitted. If the system finds no capable provider under mandatory language/format/privacy/budget constraints, reject with a **specific structured reason** (or fail explicitly if it was previously accepted). `Mandatory` never circumvents allowed provider sets, spend caps or a manual orchestrator queue pause.

Admission `Accepted`/`Deferred` is distinct from `Completed`/`Rejected`/`Failed`. The response makes material changes visible, retaining the `RequestedProvider` and reporting **actual** `UsedProvider` only once translation happens (per segment when work is distributed). New admin preferences apply to new requests, but current provider readiness, revoked permission and hard budget/quota gates are evaluated at new dispatch. Keep usage ledger outside SystemMonitor and avoid revealing sensitive policy or credential details to unprivileged callers. See [Request Admission and Normalization](Docs/Request-Admission-and-Normalization/en.md) and [Provider Billing](Docs/Provider-Billing-and-Quotas/en.md).

### Client-specific translation policies and spare-capacity public API

**New independent authorization policy:** an approved/known client can have an optional `ClientPolicy` that restricts permitted providers (free/local, paid API and AI), specific endpoints/billing accounts, quotas, request size, concurrency and maximum accepted request priority. The default for ordinary internal/known clients is **no extra client-level provider restriction**, subject to all existing global administrator and real provider billing limits. The optional Afutrans public/anonymous API uses a **separately configured profile** that may limit access to spare local/free capacity **or explicitly permit paid/AI providers** if the operator chooses to fund unrestricted public translations.

Resolve a trusted `ClientContext` at the API/application boundary; a client-supplied `ClientId` in the request body cannot grant a paid entitlement. In particular, the lite GUID-style SystemMonitor pairing is unrelated to authenticating financial translation usage. The orchestrator intersects global policy, verified client policy, required translation capabilities and the provider-account quota. A client's explicit Google preference is ignored when paid API access is not part of their plan; choose suitable LibreTranslate or another **authorized** backend and notify the client. Reject only if no compatible permitted provider remains. `Mandatory` cannot widen access.

Track **per-client quota** (e.g. source words/day for public users) and **per-provider account quota** (e.g. operator's one-million-words monthly Google bundle) independently, reserving allowed capacity atomically at dispatch when needed. Public API clients may have a lower permitted request-priority ceiling, per-client concurrency/rate caps and limits on their share of otherwise idle workers; do not create mandatory per-client physical queues or violate QueueMaster's five-queue architecture. Administrators control entitlement, exposure and caps. Details: [Client Access Policies](Docs/Client-Access-Policies/en.md).

### Human is the only required translation provider; all other engines optional

**Non-negotiable baseline:** UnderstandMe must support a **Human** translator as a fully valid provider/worker even without any external translation runtime, paid API, model or AI engine. Human-only is a complete valid configuration; manual work can be queued/assigned and results passed through the usual typed result/validation flow. This baseline is a contract capability, **not** an assertion that a human is always online or that every automatic translation requires human review. Per-request timing controls define waiting versus failure/source fallback when no human is currently ready.

Every non-human provider (LibreTranslate, AfroLT, Google, Microsoft, Ollama/OpenAI and others) is **an optional feature** that an administrator explicitly installs/registers/activates; no twenty-provider deployment is expected. The previously chosen free/local -> equally ranked paid APIs -> AI **default order is only a preference among providers actually enabled**. It does not automatically switch on missing engines, demand external credentials or imply that paid/AI must be present. Operators can run strictly human-only, LibreTranslate-only, paid-first or all-enabled installations.

**Client access is likewise fully configurable:** the standard known-client profile adds no extra restrictions beyond activated/global allowed providers. Public API exposure stays opt-in, but its separately configured public/anonymous profile can be restricted to free/local or human-only services, or deliberately grant **all active providers including paid/AI** if a generous operator chooses to fund them. Prior wording that appeared to make free-only public use mandatory was overly restrictive and is superseded. Global hard spending/account caps and privacy restrictions still apply. See [Client Access Policies](Docs/Client-Access-Policies/en.md), [Provider Routing](Docs/Provider-Routing/en.md) and [Translation Chains](Docs/Translation-Chains/en.md).

### P0 RealTime is automatic-only; no Human fallback (correction)

**Hard distinction:** `Human` is the mandatory **supported provider**, but **never a P0 RealTime worker**. P0 RealTime is solely for instant/short-lived machine translation. A Human-only deployment has **inactive P0** and returns the **unchanged original text almost immediately** for real-time translation requests. A machine engine timeout/unsupported language, a `Mandatory` flag or a caller's preference for Human must **never** cause a P0 item to be reclassified as human work. In a mixed installation, P0 may try other allowed **automatic** providers; if none can translate, it returns original text with an untranslated/unavailable reason. The live segmented update mechanism is similarly automatic-only for P0.

**Privacy is fundamental:** RealTime can translate **private chats**. No default storage of their original content in Human task queues, assignment to human editors or human review as failure handling. Simply requesting RealTime is not consent to share text with people or persist it for later manual processing. Any general dictionary/cache/log handling of such input must independently respect explicit client/target privacy policy.

Human work belongs in **explicitly authorized non-P0 persistent P1-P4 tasks**, queued for manual translators to process when online. Earlier notes about creating an automatic 'durable Human continuation' from a transient RealTime job are **superseded**. A user may independently submit a new non-P0 manual task with proper authorization to disclose the content, but no automatic RealTime fallback does so. QueueMaster retains the same five priority categories; P0 remains empty/inactive in a Human-only installation. See [Human Workflow](Docs/Human-Translation-Workflow/en.md), [Request Settings](Docs/Request-Settings-and-Live-Translation/en.md).

### Request Privacy and permission-scoped historical translations

Add an independent `TranslationRequest.Privacy` classification. **Privacy defines who may access text, translations, previews and manual audit; Persistence/CachePolicy/LifeTime separately define whether and how long results are saved.** A `Private` request can validly persist its translated result: histories of private chats, forums and similar sources should **not be retranslated whenever users reopen them**. The initial type can be simple (ordinary/private) with extensible policy, but a trusted Client/Target/source policy may tighten it and a caller cannot downgrade the result's effective privacy.

**Preserving a verified private result is not persisting its `P0 RealTime` job**. P0 remains volatile and machine-only, even with `Mandatory`; Human remains excluded. Store the approved result through a tenant/target/conversation/message/segment **scoped cache or host message store**, keyed by source revision/hash, target language and format; do not put private chat bodies into automatic `KEY=VALUE` discovery, global SystemPersistent translation memory, Human queues, general editor or manually accessible history/audit. Do not assume all forum posts have identical visibility: enforce the actual source-owner scope and participant rights on **every** read.

This storage must support translation reuse for authorized participants, invalidate/restrict on source edit/deletion/permission loss, and follow source retention without leaking copies across users/tenants. For private content the administrator may see safe aggregate operational status, not raw message text by virtue of system-admin access. Automatic validation can execute under approved processing rules, and sending Private text to remote Google/AI/etc. requires explicit authorization under the effective privacy/processor policy; Private does **not** inherently disable remote processing unless configured. Cost/usage telemetry is content-free. `Privacy = Private` is **not** a request for human audit or disclosure.

See [Privacy and Scoped Storage](Docs/Translation-Privacy-and-Scoped-Storage/en.md), [Request Settings](Docs/Request-Settings-and-Live-Translation/en.md) and [Localization](Docs/Localization/en.md).

### Chat translation corrections, temporary chunks and strict host ownership (agreed)

**Clarification: recipient self-correction is permitted and desirable as an optional bonus feature**, not a general Human auditor. The original machine translation of a private chat message is visible only to authorized recipients. The recipient may fix the translated wording **for themselves** in a host UI. They choose **no-cache** (only current view changes) or **opt-in recipient-scoped cache** of their corrected translation; a separate explicitly authorized sharing/promoting operation may be offered by a host, but caching alone never publishes the source/translation. Not every private message necessarily contains confidential facts, but the classification cannot be silently relaxed. This editing occurs **after** the translation, outside the engine and outside the Human backlog and normal dictionary Editor audit.

**`KEY = VALUE` means only application/web UI localization dictionaries**, e.g. JSON catalogues. Dynamic chat/forum messages do not use dictionary-less key discovery. A Splitter produces `List<(Id, Text)>` numbered 0..N with **temporary per-request IDs**, used solely for correlating translated pieces and composing the final result; these IDs are **not persisted** as keys, cache entries or chat identities. The translation core returns chunks and typed results; **the consuming chat application assembles and displays the full translated message and manages its own history/storage/retention**. Its previously saved original translation may eliminate repeated engine calls when conversation history is reopened, independently from caching any recipient-edited override.

Private source content is never sent automatically to external Human translators or manual auditors; self-edit by a participant who already has read access is different. `P0 RealTime` remains automatic-only and volatile; storing the assembled translation in host application history does not persist P0 or force central UnderstandMe chat data storage. See [Chat Translation Flow](Docs/Chat-Translation-Flow/en.md), [Privacy and Scoped Storage](Docs/Translation-Privacy-and-Scoped-Storage/en.md), [Localization](Docs/Localization/en.md).

### Semantic, configurable chunk boundaries (agreed)

Chunks are useful mostly for **large or structured text**; small normal texts can remain **one chunk**. Splitter uses the source format and Settings to choose meaningful boundaries, not fixed lengths. Plain readable text **prefers paragraphs** when available (newline/blank-line interpretation is configurable), while long continuous text **without meaningful paragraph formatting** is split by **sentences**. Periods are a rough clue, not a safe literal delimiter because of decimals (`1.96`), abbreviations and URLs; keep sentence punctuation/whitespace and language-specific boundaries intact.

**Markdown**: group text-bearing Markdown syntax/AST nodes (headings, paragraphs, list items, tables etc.) and preserve tokens/inline links/code. **HTML**: translate appropriate element/text content while retaining tag structure; avoid treating nested formatting tags as reasons to fragment one sentence into isolated words. New document types get pluggable per-format splitters when developed. If an intact meaningful unit exceeds an actual provider limit, split further at the next safe boundary or select another suitable endpoint. Avoid breaking placeholders, markup and the original reassembly mapping.

Each Splitter assigns request-local `Id=0..N` to temporary translation chunks; these are not stored as `KEY=VALUE` dictionary entries or durable chat IDs. Caller receives translated segments and owns reassembly and final storage. The policy can be overridden by `Settings` (conceptual Auto/Paragraph/Sentence/FormatAware modes). See [Semantic Splitter](Docs/Semantic-Splitter/en.md), [Chat Translation Flow](Docs/Chat-Translation-Flow/en.md).

### Structural template and source-initialized ResultList (exploratory)

The proposed HTML representation `<tag placeholder=0>@ResultList[0]</tag>` captures a useful **template + replaceable text slots** design. Parse HTML/Markdown into its own structural model first, map each meaningful translatable unit to a request-local integer `Id=0..N`, and **initialize `ResultList` with the original content** for all slots. Render source immediately; translate asynchronously; update only each verified result slot as it completes, preserving original, translated/current text, status and revision. Failed/pending results keep the source visible. IDs are temporary and unrelated to dictionary keys, durable work queues or chat message identity.

This is a **conceptual mapping, not a final decision to write synthetic attributes into HTML**. Use a proper HTML parser (void elements including `br`, nested tags, text nodes, malformed-but-recoverable source); map safe structural/AST references separately instead of overwriting real `placeholder` attributes. Mixed content (`<p>Hello <strong>beautiful</strong> world!</p>`) should remain a coherent translation chunk where possible while protected inline markers preserve formatting. Raw translated markup is not automatically trusted as executable HTML. Markdown uses AST nodes rather than HTML assumptions; chat hosts remain responsible for assembling and storing translated messages.

Next contract design must decide structure/slot API, safe HTML/Markdown node mapping, out-of-order updates, inline markup protection, empty-result state, and source fidelity. [Structural Template and ResultList](Docs/Structural-Template-and-ResultList/en.md), [Semantic Splitter](Docs/Semantic-Splitter/en.md).

### Inline tag restoration using inner alternatives and whole-sentence translation (design proposal)

For `Hello <strong>beautiful</strong> world!`, a promising approach is: translate tagged **inner phrase** `beautiful` first to gather acceptable target variants (`nádherný`, `krásný`, `fascinující` when actually supported), then translate **the complete untagged sentence** for meaning and word order (e.g. `Ahoj nádherný světe!`). Search for an unambiguous candidate target span; reattach `<strong>` around `nádherný`, reconstruct and validate the original markup around the meaningful text. Unlike splitting by DOM text nodes, context remains available.

This is **not guaranteed**: Czech gender/case/number may change inflection (`krásný` vs `krásného`), a term may recur more than once, and paraphrase may eliminate literal synonyms altogether. Use language-aware whole-word/phrase matching, ambiguity detection, source span context and valid nesting. Never wrap a random match. If the match fails, prefer a verified whole-sentence translation with an explicit lost-cosmetic-format warning, or safe source/structure-preserving fallback for semantically functional tags such as links. Nested formatting may require hierarchical annotations, not blindly matching independent strings.

`VariantAlignment` can be one **optional** format strategy. A compatible provider's validated HTML/inline-marker translation might restore tags from one context-rich call without repeated inner phrase API calls. Each extra attempt increases cost/latency and is subject to client rights, provider caps, privacy and real-time deadline; no implicit paid overage or P0 Human fallback. Keep an AST/DOM structural mapping and source-filled `ResultList`. See [Inline Markup Restoration](Docs/Inline-Markup-Restoration/en.md) and [Structural Template](Docs/Structural-Template-and-ResultList/en.md).

### Inline markup failure policy: outer-anchor alignment, cosmetic omission, significant-ID failure

After `VariantAlignment` fails to recognize an inner tagged translation, **optionally translate the source text before and after the nested tag** to find matching outer anchors in the **original whole-sentence target translation**. For `Hello <strong>beautiful</strong> world!` and a whole translation `Ahoj super světe!`, `Hello` → `Ahoj` and `world` → `svět` could help identify the middle `super` when morphology-aware matching recognizes `svět/světe`. This is only reliable when both boundaries are unique, compatible with word order and context; do not arbitrarily place markup or replace the actual whole translation with a different adjective taken from inner alternatives.

**Default fallback:** where alignment stays uncertain, omit unmatched `i`, `strong`, `em` or other explicitly allowlisted **cosmetic-only** inline markup; preserve **all** validated full-sentence translated text and surrounding structural tags (e.g. keep `<p>`). Emit a nonfatal `FormattingOmitted` status. If the unmatched original tag carries **`id`** or required semantic/reference/accessibility/interactive attributes (including important links), default to **typed `MarkupRestorationFailed`**, never silently drop or randomly reassign the identifier. The tag name alone does not confer cosmetic status: consider attributes and dependencies. The host may show source content or a clearly marked partial preview according to its policy.

Configurable `InlineMarkupFailurePolicy` allows stricter cosmetic requirements and a limited set of recovery strategies (protected markup, direct variants, outer anchors, alternative provider) subject to ClientPolicy, privacy, per-attempt cost/quota and deadline. P0 remains automatic only; there is no Human fallback. See [Inline Markup Restoration](Docs/Inline-Markup-Restoration/en.md) and [Structural Template and ResultList](Docs/Structural-Template-and-ResultList/en.md).

### Afutrans assisted translation: HTML-native providers, complex-HTML AI, editor preview/source (agreed direction)

Provider capability matters: a configured translator may support **HTML translation directly**, reducing the need for split/variant matching, though its response **still needs parsing and structural validation** (`id`, links, functional attributes, nested elements, placeholders). For **complex HTML**, an Afutrans **format-specific assisted profile** may prefer a **capable activated AI model** over ordinary providers if authorized by system/ClientPolicy/privacy and cost/quota. This is a configurable ranking, **not** a mandatory AI dependency, automatic purchase or replacement of ordinary text's local/free -> paid -> AI preference. AI may excel at preserving meaning/HTML context but must **not** be assumed always right.

**Assisted document editor (optional Afutrans host):** rendered HTML original on the **left**, translated content on the **right** with **rendered preview / source HTML / combined** modes. Link the two through temporary `RequestId/SegmentId` or structural references. Source-initialized `ResultList` exposes successfully translated, untranslated, format-warning and structural-failure regions, rather than silently losing problem text. An authorized user can inspect a specific region, directly correct translation or markup (including a `strong` span that automatic alignment missed), request alternate translation, explicitly ask an eligible AI engine for a proposal limited to selected content, or accept/undo/save/export. Keep draft and source separately; **stale async AI/provider replies cannot overwrite human edits**. Validation prevents publishing structurally broken ID/links as valid.

Safe HTML preview must not execute embedded scripts or arbitrary imported resources; provider HTML/AI outputs are untrusted until validated/escaped by the adapter. AI suggestions are non-destructive and cost/privacy-controlled. This is an **explicit non-P0** user editing session, neither an automatic Human fallback nor an invitation to manual auditing of private chats. Host owns draft/checkpoints/editor UI and exported document; UnderstandMe offers provider/segment/validation contracts. This differs from manual UI dictionary edits (mandatory revision audit) and optional chat recipient-specific correction. See [Assisted Document Translation](Docs/Assisted-Document-Translation/en.md), [Editor and Audit](Docs/Editor-and-Audit/en.md).

### Never a dead end: direct autosaved manual document editing and reversible history (agreed)

**Fundamental UX:** there must always be a system-level **authorized manual path** to finish translation or input missing target-language text when automatic providers, AI, format adapters or structural alignment cannot do the job. This need not be embedded in every request mode, but Afutrans must not leave a legitimate translation uneditable. Human is the only mandatory provider capability, and human-only projects can create translations without automatic engine access. Unsupported structured output can still be authored manually as a draft, without false promises of validated structure. This **does not** activate Human fallback for private `P0 RealTime`, bypass client permissions/privacy or ignore publish-time structure checks.

**Simple responsive assisted editor:** wide display = rendered source left and editable translated document right; narrow display = stacked views or a simple `Original | Translation | Source` selector. Both right-hand **rendering and HTML/Markdown source can be edited directly** with click/tap/typing (no mandatory edit modal or Save button); both reflect **one synchronized draft**. Editing a line/region in one should be reflected in the other as soon as valid re-render is available. Preserve focus/caret where practical.

**Autosave:** immediately show edits and **automatically queue durable saves** in the background, with bounded debounce/flush. Persist every accepted logical edit, show real `Editing/Saving/Saved/Offline-Retrying/Conflict/Failed` states, and **never claim saved before storage acknowledges**. Temporarily malformed HTML stays editable and can be saved as a draft while Preview shows last valid safe parse, not fabricated valid HTML. No silent loss on transient failure; provide retry/recovery.

**History and revert:** each committed logical edit has a revision (actor, time, segment/document, before/after or diff, source version), visible in history. Undo or revert to any retained revision creates a **new revision**, preserving prior history, with optimistic concurrency so delayed provider/AI results cannot overwrite newer manual work. The assisted document host owns draft persistence; this is separate from QueueMaster `Persist`, temporary Splitter `Id=0..N`, optional `Preserve` and the mandatory audit of **authoritative UI dictionary edits**. Private-chat recipient corrections retain separate cache/no-cache choice. See [Direct Editing, Autosave and Revisions](Docs/Direct-Editing-Autosave-and-Revisions/en.md).

### Word-like WYSIWYG, VS Code-like source and live AI editing (agreed)

Afutrans translated-document editor should work **as simply as Word in WYSIWYG mode**: drag-select target text, press **Ctrl+B** for `<strong>` or **Ctrl+I** for `<em>` (or use contextual toolbar; equivalent platform shortcuts) to restore missed formatting. Format adapters manipulate safe selected semantic ranges and retain nested tag identity/attributes. **Source mode** should feel like a practical code editor with direct HTML/Markdown typing, syntax coloring, tag/bracket pairing, indentation and inline parse errors. Both are views of **one shared edited target document**, not two independent copies; keep caret/selection and safe preview in sync. Responsive narrow displays must remain fully editable.

**Manual autosave timing is precisely 1 second of inactivity**: **each accepted user content or formatting edit restarts a 1,000 ms debounce**; at idle expiration begin background durable save, show accurate `Saving/Saved/Retry/Conflict` status, and flush pending work on blur/navigation when possible. The editor need not make one server write per keystroke or force a Save button. A partial/incomplete HTML tag is still a valid editable draft to autosave after a one-second pause; validation gates publishing, not working draft persistence. Group bursts of typing into understandable history revisions; revert appends a new revision.

**AI can act like a visible IDE editing agent**: explicitly invoked by the authorized user, it modifies the chosen target region or HTML draft *before their eyes*, progressively applying structured patches that users can observe. Provide pause/stop, inspect diff, accept/keep, edit-over, and **undo/revert the whole agent pass**. The user's manual edit wins over stale AI/provider completions, and agent modifications are tracked/attributed with a base revision and bounded checkpoints rather than a persisted history entry per generated token. Agent suggestions do not secretly publish, bypass source/markup validation, send unauthorized content to external AI, or make AI a required provider. Original document remains recoverable. Details: [Direct Editing, Autosave and Revisions](Docs/Direct-Editing-Autosave-and-Revisions/en.md).

### Click source element -> wrap chosen translated text with all attributes (agreed WYSIWYG)

Afutrans can repair lost formatting/structure even more simply than typing HTML: **click a word in the original rendered document**, resolve the original **enclosing HTML element** (with a small nested-element/ancestor chooser if necessary) and hold a temporary **parsed tag snapshot in memory**. This captures **the full opening element and all its original attributes (`class`, `id`, `data-*`, accessible/functional attributes etc.) plus matching closing semantics**, not just `strong` or `i`. Then **drag-select the corresponding translated words on the right**; the WYSIWYG editor creates a structurally equivalent wrapper around the **selected target wording**. Example: `<strong id="emphasis" class="marked">beautiful</strong>` copied onto selected `super` becomes `<strong id="emphasis" class="marked">super</strong>`. No fragment retranslation, AI, or manual tag typing is needed.

Use parsed DOM/AST structural insertion, not unchecked raw-tag string concatenation. **Preserve original attributes where safe, without silently deleting/renaming `id`**; **do not validate ID uniqueness in either document**, because repeated IDs can already exist in source HTML and are outside the translator's scope; preserve IDs exactly, and separately handle safe structural insertion and preview restrictions for active markup. Original and translated previews must be isolated as separate documents so the two legitimate matching IDs are not a collision. A `p`/`br`/void/structurally incompatible element cannot blindly be applied around any inline selection; display an actionable conflict or select the appropriate whole block. Source remains immutable; the operation is one reversible manual change and restarts the **1,000 ms idle autosave**. Relevant `Privacy` and editor authorization remain in force. See [Source Tag Transfer](Docs/Source-Tag-Transfer/en.md), [Direct Editing and Autosave](Docs/Direct-Editing-Autosave-and-Revisions/en.md).

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

### 2026-10-11 — layered localization, combinable hints and CLDR references (agreed design direction)

**The dictionary-less start is a hard product requirement:** missing dictionaries, hints, grammar variants, CLDR data, AI or network must not prevent first use. Basic `en.json`, `cs.json` and other locale files remain flat key/string maps, usable by simple third-party JSON localizers. Store generated placeholder descriptions/examples separately (e.g. `en.placeholders.json`), and localized grammar variants/overrides in sidecar files (e.g. `cs.grammar.json`) or separate SQL tables. Prefer a usable fallback phrase over perfect grammar; never accept corrupted placeholder identities/multiplicity.

**Inline hint API:** keep the original indexer accepting `params string[] args` *unchanged* for source and binary compatibility. Introduce an overload taking a typed hint or composite-hints container before the `params` tail; C# optional parameters cannot be skipped positionally. Hint types (plural, gender, case, condition) may each have specific fields; support combining multiple selectors within one context. Use optional unions for standard hint variants in newer .NET while preserving an extensible .NET 10-friendly abstraction. Exact signatures/union syntax are not yet settled. Keep actual runtime arguments distinct from safe-to-store translator hints.

**Automation:** Afutrans may consult the now-vendored pinned Unicode CLDR 48.2.3 JSON to discover target-language plural categories, generate sample translations/variants, validate them, store them outside the basic dictionary and expose locale-specific edits/overrides in the editor. CLDR rule evaluation/import is **not yet implemented**. Do not assume English `one/other` categories suffice in Czech or vice versa; don't infer global category logic from a few equal-looking examples. Native source-language forms can also use selectors. See [Layered Localization and Grammar Hints](Docs/Grammar-Hints-and-Variants/en.md).

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

### Typed SystemMonitor events and passive event listeners

Operational events are emitted once by each module through a typed publisher; the dedicated module SignalR transmitter and the aggregate /SystemMonitor router are both passive listeners, not a chain of Hubs. The shared event envelope uses a **generic Message<T>** rather than a string; report models are chosen and versioned by the originating module (model loading, queue state, translation progress, etc.). The root and module feeds carry the same event type/envelope and retain EventId, SourceModule and InstanceId. For wire interoperability, pair the strongly typed .NET payload with a stable, language-neutral ContractId and SchemaVersion; receivers deserialize known contracts via a registry and do not resolve arbitrary CLR types from remote input. Missing clients or listener failures have no effect on work. See [SystemMonitor design](Docs/SystemMonitor/en.md).

### Remote SystemMonitor aggregation through optional SignalR clients

A monitoring orchestrator can optionally connect as a **SignalR client** to one or more remote `/SystemMonitor` sources. Example: VPS3 subscribes to VPS1 and VPS2 and republishes their events alongside its own on VPS3's `/SystemMonitor`; the administrator needs only the VPS3 connection. This uses the existing event listener/router model and does **not** add client-callable functions to the monitoring Hub. Each original generic event retains its sender/module/instance, event ID and typed payload contract. Cross-host forwarding must prevent loops and duplicate events, configure authentication and trust, and stay bounded/nonblocking. Remote disconnects/reconnects affect only visibility of that source; missed monitoring broadcasts are not recovered from SignalR. No external module dependency or persistent message bus is mandatory. See [SystemMonitor federation](Docs/SystemMonitor/en.md).

### Lite GUID identification for monitoring links

The basic SystemMonitor pairing is intentionally lightweight: **NodeId is a GUID known to the receiver**, not a bearer secret or complete authorization system. When establishing a remote monitoring connection, the client may announce its node GUID as connection metadata; the source transmits a `MonitorHello` declaring its own NodeId, display identity, offered modules, event contract versions and instance information before the regular `MonitorEvent<T>` stream. VPS3 accepts only a declared node matching its configured HTTPS endpoint/allowlist and expected source modules. This is source filtering to avoid polluting an anonymous, read-only monitor, not cryptographic identity proof.

No mandatory per-recipient API keys, OAuth server, JWT signing, shared identity authority or rotation machinery in the lite design. GUIDs can be copied/spoofed; remote connections therefore use fixed trusted HTTPS addresses and normal server certificate verification. The outbound-only Hub has no remote Publish method; incoming federation events originate only at our independently configured SignalR client receiver. Optional stronger security could be introduced later if needed without affecting the typed event model. See [SystemMonitor design](Docs/SystemMonitor/en.md).

### Admin-approved self-enrollment of a new monitoring node

A newly started server (e.g. VPS4) may ask VPS3 to register as a source by submitting its public `ClientId` GUID and a simple declaration of its identity, endpoint and offered module/event types. Registration is **Pending**, never auto-approved. The recipient saves that small pending request and emits a typed `MonitorRegistrationRequested` event over `/SystemMonitor`; if an administrator watches live they can open the request and approve/deny immediately, otherwise the record waits for later review. Decisions happen through a separate authenticated admin API, not a callable method on the read-only Hub.

Approval issues a **new recipient-generated GUID `ClientKey`** bound to the approved `ClientId` and sources. The applicant retrieves it on a private enrollment/control path, proves that it owns the original pending request (e.g. one-time claim ticket), stores it and supplies `ClientId` plus `ClientKey` in HTTPS headers for subsequent control requests. The approval key is not public event data. The registered remote source can then be picked up automatically by VPS3's independent SignalR receiver. This keeps monitoring lossy and one-way while approval is durable. GUID `ClientKey` is still a small bearer credential, not a public identification field. [SystemMonitor enrollment design](Docs/SystemMonitor/en.md).

### Adaptive SystemMonitor identity checks for familiar nodes

Once a node is administrator-approved, its private recipient-issued `ClientKey` GUID need not accompany every ordinary monitoring connection. An ongoing SignalR session already has its assigned node identity. For a new/reconnected session, the receiver can compare the public `ClientId`, expected remote endpoint, connection/IP hints and declared module profile to its approved-node baseline. If something looks different, it asks for the stored private `ClientKey` over the separate HTTPS verification path before admitting events. This is the doorman asking a familiar visitor for ID after an unusual change.

No credential challenge is performed by a client-callable method on `/SystemMonitor`; it remains outgoing-only. Be explicit that public GUID/header/IP familiarity is spoofable and constitutes only **lightweight recognition**, not proven identity for every new session. Challenge every new connection when stronger authenticity is necessary. Key details: [SystemMonitor design](Docs/SystemMonitor/en.md).

### Clickable monitoring filters and human-controlled incident decisions

The combined SystemMonitor stream may be high-volume. Each admin dashboard must provide a convenient interactive filter builder across servers/original ClientIds, relays, modules, instances, severities/results, contract/event names, correlation IDs, time and searchable diagnostics, supporting multi-select and saved personal presets. Client display filtering does **not** modify publishing, other clients' views, or the approved-node registry.

At the router's admission boundary, an invalid/unexpected event is **recorded as an incident and discarded**, not converted into an automatic source block or revocation. Repeated identical incidents may be counted/aggregated to avoid storage or UI floods. The operator decides separately whether to ignore it, inspect the source, manually mute, manually block/revoke, or change expected-source settings. A proper new-node registration remains Pending, not an incident solely because it is new. Critical incident storage persists independently of the SignalR live notifications. See [SystemMonitor](Docs/SystemMonitor/en.md).

## 11. Source repositories and document maturity

- Toolbox: https://github.com/afrowaveltd/Toolbox
- Afutrans: https://github.com/afrowaveltd/Afutrans

The current Afutrans WebTranslator is the practical first web-host skeleton, not a completed translation service. Only design documentation is being prepared now; no agreed behavior should be represented as already implemented.

For decisions needing further discussion see [Open Questions](Docs/Open-Questions/en.md); for the verified continuation point see [Implementation Status](IMPLEMENTATION_STATUS.md). Detailed topic notes are indexed in [README](README.md).

