# Translation Pipes, Chains and Pluggable Workers

Status: design proposal and agreed functional direction; not implemented.

## Different concepts

- **Queue**: physical waiting work by priority, owned by QueueMaster.
- **Buffer**: a small FIFO reservation of items owned by an endpoint/worker.
- **Pipe**: selection/execution policies (priority, persistence, caching, retries, provider preference, privacy, review, retention).
- **Chain**: sequence of compatible processing steps, potentially with branch/alternative results.
- **Worker**: one operation that accepts a compatible input and returns one or more results, or an error; it need not understand the whole Chain.

Policies should be easy to configure with flags for combinations, enums for exclusive states, and scalar limits for durations/size. New community workers should be registered via interfaces/capabilities, not by extending a central list of hardcoded implementation names. Global pipe profiles may be overridden per target and, subject to policy constraints, per job.

## Translation providers

A provider describes a family (LibreTranslate, Google, Microsoft, Ollama, OpenAI, Human). An endpoint is one configured instance with its own URI/credentials, capabilities, languages, buffer size, concurrency and `IsReady`. Providers/endpoints and Translation Targets are addable/removable via configuration; new requests follow updated selection rules. Do not route unsupported pairs to an endpoint.

Translation alternatives may be produced by multiple providers in parallel for an editor, or sequentially for cost-sensitive automatic fallback. A failure can retry or choose another compatible endpoint. QueueMaster does not need to know what semantic operation the worker performs.

## Human translation as the zero-external-engine baseline

The library **must support the Human translation worker** without LibreTranslate, any paid API, AI or model/runtime dependency. A human-only workflow may queue an item for manual pickup, assign it to an authorized human worker, accept an edited/translated result, validate its protected structure, and return it through the same typed result contract as any other provider. No particular UI application is required to implement the worker contract.

A Human provider is a baseline **capability**, not an assurance that a human operator is currently ready or a blanket requirement that human review is part of every Chain. Without an available worker, work may wait in a permitted human workflow or fail/return original text under the request's configured deadline and flags. Machine translators, AI reviewers and their respective Chain stages must remain entirely optional.

## Human work consumes persisted backlog when a translator opens the UI

The human translation workflow is **not dependent on anyone being online at admission time**. Accepted work intended for later human translation remains persistently pending in QueueMaster's existing five priority categories until a compatible authorized translator opens a client, views waiting counts by priority/language and claims an item. Work can be processed later without losing its original request identity or changing the queue priority. Human UI may be Blazor/WebUI, desktop or another client, none mandatory.

A claimed item has exclusive durable ownership/release and recovery behavior; final human output follows the same shared validation and typed result contract. No new parallel physical queue belongs to the Editor. Ordinary volatile RealTime never feeds Human. If a user separately authorizes manual translation of content, they submit a **new, durable non-P0 manual request**, not a continuation created automatically from RealTime. See [Human Translation Workflow](../Human-Translation-Workflow/en.md).

## RealTime may never select a Human worker

**P0 RealTime executes only with automatic translation engines.** Human is unavailable as a candidate regardless of provider order or `Mandatory`; when only Human is configured, P0 translation stays inactive and the caller receives the unchanged source text immediately. An automatic translation failure cannot create a durable human follow-up or disclose P0 content (including private chats) to a Human queue/editor. RealTime segment delivery is permitted only from eligible automatic engines. An actual Human task requires an **independently authorized non-P0 persistent request**, placed in the regular P1-P4 human backlog. See [Human Translation Workflow](../Human-Translation-Workflow/en.md).

## Provider priorities, price-aware selection and escalation

Worker eligibility and provider priority are separate from QueueMaster's **job priority**. A Translation Pipe's provider-selection policy can rank compatible endpoints by explicit preference (LibreTranslate then Google, or Google first) or by estimated cost. Global defaults, named Pipe policies, Translation Targets and permitted per-request overrides define the effective order, with hard privacy, capabilities and budget limits never overridable by an individual request.

Each candidate must support the source/target language, format/HTML, placeholder requirements, capacity and privacy constraints before selection. Fallback to a second backend is an **explicit configured option**, not a universal rule: try another provider after eligible transient failures, failed mandatory validation or a required review, subject to bounded retries, deadline and maximum spend. Do not equate provider reputation with a measured quality score. A Chain can select a different engine for semantic review than for its initial cheap translation, or prohibit all paid providers in a LocalOnly/Economy profile.

Providers and endpoints remain replaceable plugins; QueueMaster does not own the commercial selection policy. Details and provisional examples: [Provider routing and cost](../Provider-Routing/en.md).

## Request Settings and incremental translation

A Translation Request resolves its Settings when created: exclusive P0–P4 queue priority, composable independent flags such as `Mandatory`, and appropriate provider, cache, split and delivery options. QueueMaster owns job priority, whereas the Pipe selects eligible providers according to the request's **captured** effective policy. An orchestrator pause stops dispatch, even for urgent or mandatory requests; administrator preference changes do not silently rewrite work already accepted.

For documents and other long texts, a format-aware splitter supplies ordered `(SegmentId, Text)` pairs and sufficient format metadata. Completions identify both `RequestId` and `SegmentId`, so **any consuming UI** (Blazor, JavaScript, Avalonia, etc.) can update only the matching paragraph or content unit using its own presentation mechanism. A browser adapter can choose `textContent` for plain text and validated/sanitized HTML rendering for formatted Markdown, but the **core contract has no DOM or JavaScript dependency**. Translation results use an application-facing update path, **not** the administrative SystemMonitor. [Request Settings and Live Translation](../Request-Settings-and-Live-Translation/en.md).

## Configurable chain examples

- `FastUI`: short machine translation → placeholder/format validation → provisional cache.
- `DictionarySync`: discover new keys → translate configured languages → validate → write authoritative dictionaries → `Update_Cache`.
- `ArticleAI`: logical split → translate chunks → reassemble → mandatory AI fidelity/language review → final validation → result.
- `ManualReview`: human/AI alternatives → manual selection/edit → auditable publication.

An AI pass can combine translation, tone and constraints in one composed prompt; two-pass translation + final review; or separate translation, style revision and semantic validation. The web-host editor should let a user build/reorder/configure these chains. Individual steps choose prompts/models. Prompt templates and fragments are versioned/configurable with inspectable composed final prompts.

## Private request handling in Chains

The request's effective `Privacy` determines visibility and permitted processor hand-offs **independently of how long translated results are kept**. A `Private` request can use an authorized machine-only translation and automated validation Chain, then retain just its verified final output in a conversation-scoped cache for later permitted history views. It must **not** automatically branch to Human/manual audit or promote private text into global translation memory/dictionaries. `P0 RealTime` remains volatile and automatic-only even when its separately stored result is long-lived. Private result access checks and deletion remain the host/source-owner's responsibility. [Privacy and Scoped Storage](../Translation-Privacy-and-Scoped-Storage/en.md).

## Chat translation is not localization dictionary processing

Chat translation can use the existing format-aware Splitter and automatic translation Chain without dictionary-sync steps. For chats, **chunk IDs `0..N` are request-local temporary correlation only**. The engine returns translated pieces, and **the consuming chat application assembles, displays and optionally saves the whole translated message**. Do not register those IDs as UI `KEY=VALUE` dictionary entries or require a central chat-result repository.

A message recipient may voluntarily amend their **own displayed translated text** in an optional host-side editing tool and select **cache correction** or **do not cache**. This must not trigger generic Human manual-audit jobs, automatically modify a shared authoritative dictionary, or disclose the source to other recipients. The accepted correction and optional recipient cache are **post-processing in the chat app**, not a translator provider. See [Chat Translation Flow](../Chat-Translation-Flow/en.md).

## Source fidelity and structure

Human comments may be explicit, emotional or sarcastic. Preserve source meaning, wording intensity and tone unless the target's explicit editorial policy authorizes changes. Documentation may allow professional polishing. Distinguish faithful translation from optional editorial rewriting. An AI semantic check compares proposed output to the **original**, not a previous generated translation.

Split long articles/documents on meaningful boundaries, keep stable part IDs and order, translate chunks, and assemble them. Document-format adapters must protect Markdown/HTML syntax, links, code and protected tokens. Structured outputs from Ollama/AI can be validated against agreed schemas; parsable JSON alone does not establish semantic correctness.

Results are typed objects and may hold multiple variants from multiple models or branches. Any preserved compatible result may serve as input to another step or to a persistent rollback/resume workflow. A required final check cannot be bypassed by marking an earlier step completed.

Related: [Editor and Audit](../Editor-and-Audit/en.md), [Notes](../../Notes.md).

