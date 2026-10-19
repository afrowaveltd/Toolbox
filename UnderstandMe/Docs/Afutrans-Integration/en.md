# Afutrans Host and External Integration

Status: planned usage of UnderstandMe; existing Afutrans repo is an early experimental application.

## Host forms

Afutrans should run locally for one developer or persistently on a server managing many configured Translation Targets. Its interfaces include WebUI (first practical host), Avalonia GUI, and Terminal.Gui v2 TUI. These are presentations over reusable UnderstandMe capabilities; translation and queue logic does not belong exclusively to a UI.

Afutrans needs an on-screen text translator, file translator (Markdown, HTML, plain text and extensible types), dictionary import/export with auto-detection and manually configurable database/schema profiles, a side-by-side editor, a scheduler, a prompt/Chain editor, API integration, and optional bidirectional SignalR.

WebUI may access the host filesystem for legitimate dictionary editing, but deployed installations must restrict access to authorized roots, users and destinations. A local installation and a VPS service have different trust boundaries.

## Targets and sources

Each Translation Target describes data locations, dictionaries, default culture, target/ignored or manual-only languages, key strategy (`key=value` or symbolic), format adapter, access rights, live translation, Chain/Pipe overrides, cache behavior, sync schedule and retention. A connector's capabilities must be checked before writing back; results can be returned to the requester instead.

Configuration may add/remove targets, endpoints, workers, pipelines and prompt profiles without code changes. Scheduling can discover and reconcile missing/changed source keys and produce background jobs. Event-driven updates plus a periodic full scan (illustrative default: every three hours) are complementary.

## Human-only hosting and configurable provider activation

Afutrans must remain usable as a **manual translation, dictionary and review host even when no machine/AI backend is installed**. Human translation is the required baseline provider capability; LibreTranslate, AfroLT, commercial translation APIs and AI models are optional activated features. The administrator may run everything manually, install only LibreTranslate, permit paid engines for selected clients, or activate all supported engines without needing to rework the host.

An optional public translation API is **not automatically exposed**. When enabled, the operator chooses whether it offers only spare local capacity, human work, selected paid/AI backends, or all enabled providers (e.g. a philanthropic public service). Any global authorized budget, account quotas, privacy constraints and public usage controls still apply. No default template imposes a permanent prohibition on paid translation for public callers.

## Live translation versus manual work

Afutrans uses automatic translation engines **only** for `P0 RealTime`. When the deployment has only a Human provider, P0 is inactive and returns untranslated source immediately. No live request automatically creates Human work even after an engine failure or with `Mandatory`. Manual translation requires a separate authorized, durably queued non-P0 request; the translator inbox shows only such requests. This also preserves the privacy of conversational text. See [Human Translation Workflow](../Human-Translation-Workflow/en.md).

## On-demand human translation inbox

Afutrans translation UI can be closed for hours or days while **the host keeps accepting and durably storing human-intended work**. On login, an authorized human translator sees authoritative counts **by each of the five QueueMaster priorities**, language directions, waiting versus claimed/in-progress state and compatible available requests. The translator chooses an item or accepts the next suitable item via an atomic QueueMaster claim, edits and submits a validated result. Session disconnection must not erase the task; ownership can be released/recovered without duplication.

This is a presentation/work client of the common QueueMaster/Human provider contract, not an alternative scheduler or dedicated physical human queue. WebUI, Avalonia and TUI can share the same command/state model. [Human Translation Workflow](../Human-Translation-Workflow/en.md).

## Assisted document translation workspace

Afutrans should offer an **assisted HTML/document editor**: rendered original at left, translated output at right, and right-side **render / HTML source / split** views. Allow a user to select a source/target region to see the original segment, current translation, provider, alternates and any formatting/structural warnings. Source-filled ResultList slots remain visible for untranslated segments instead of dropping them. Use a safe preview that does not execute imported scripts or trust arbitrary translated HTML.

The authorized editor can manually correct text, bind a lost inline `strong`/`i` to the right target-language word after recognizing morphological correspondence, retry a selected region, or request **an explicit AI suggestion**. Suggestions and late provider completions must never silently overwrite later human edits. Publishing/exporting requires appropriate structure/ID/link/placeholder validation and host-owned draft/version persistence.

Prefer **an actually HTML-capable endpoint** for supported HTML input and verify its output. For complex nesting/mixed markup, an **assisted-HTML format-specific profile can prefer an eligible AI provider** by default, if the administrator has enabled it and its privacy/cost quotas allow it. This **does not override** the ordinary text default provider ranking or require AI to be installed; AI is not guaranteed perfect. Human-only hosts can still edit/translate manually. [Assisted Document Translation](../Assisted-Document-Translation/en.md).

## Simple responsive assisted editor with always-available manual input

Afutrans must offer an **authorized manual path to enter a translation even when every automatic backend, AI or structured-content mode fails/is disabled**. The user may type/replace the target text from scratch; the lack of a capable engine is not a terminal obstruction to document translation. This is an explicit editing workflow and does not grant access to private content or redirect P0 messages to Human.

On wide screens, retain **rendered original left / translated document right**. The right panel's **rendered Preview and HTML Source are both directly editable** by clicking/tapping the text/line. Do not force a special edit modal or Save button. On small displays use a practical vertical stack or `Original | Translation | Source` switch, preserving full edit functionality. A single synchronized draft backs both views, with inline error annotations and readable `Saving/Saved/Retry/Conflict` feedback.

**Autosave each accepted edit in the background** with durable acknowledgement and recovery from transient write failures; temporarily invalid HTML source must remain editable as a draft, with last valid preview if necessary. **Keep an attributable version history and permit reverting any retained version**, where the revert itself creates a new revision. AI/provider suggestions are optional, must not overwrite newer manual changes and require explicit approval in assisted sessions. [Direct Editing, Autosave and Revisions](../Direct-Editing-Autosave-and-Revisions/en.md).

## WYSIWYG/Source controls and directly visible AI editing

Afutrans should feel like **Word in rendered-edit mode** and a **familiar code editor in HTML/Markdown Source mode**. Users can drag-select translated text and click **Bold/Italic** or press **Ctrl+B/Ctrl+I** to insert/repair safe `strong`/`em` markup automatically, with no manual tag editing required. Source mode supplies syntax highlighting, tag pairing, indentation and inline diagnostics while editing the same synchronized draft. Keep layout responsive and simple on desktop and mobile.

Every manual change, including a formatting shortcut, **resets an exact 1,000 ms autosave idle timer**. After a full second without another manual change, the editor sends the draft save in the background; preserve status and revisions, allow undo/revert, and distinguish saving from server-acknowledged saved. Invalid half-typed HTML can still be durably stored as a draft, not published as valid.

A user can explicitly ask the optional AI agent to **change the document directly and visibly**, akin to an IDE assistant applying edits on-screen. Limit edits to the authorized scope, show progressive changes and diff/review state, support pause/stop/accept/undo of a whole agent pass and prevent stale AI patches from overwriting later manual input. Saving/checkpointing agent operations uses revision control without cluttering the history with one record per generated token. The human user remains final arbiter; model suggestions never bypass document privacy, provider quotas, structural validation or publishing controls. [Direct Editing, Autosave and Revisions](../Direct-Editing-Autosave-and-Revisions/en.md).

## One-click source-tag pickup and target wrapping

A user who sees missing markup in the translation can **click its formatted source text on the left**, arm the appropriate parsed original tag/ancestor with **all original attributes**, then **drag-select the intended words on the right**. Afutrans applies the same element around that target-language selection via safe DOM/AST editing, including supported `class`, `id`, `data-*` and other attributes; it does not alter the translation text or require hand-writing tags in Source mode. The source tag selection is visibly indicated and can be cancelled.

Do not silently rename a source `id` or reject duplicate `id` values: the source may already be non-unique, so **copy the ID exactly and do not lint or enforce uniqueness**. Do not execute imported event handlers or unsafe links in preview. Unsupported block/void element wrapping or structural/security conflicts get understandable feedback, allowing a safe correction. The applied transfer is **one undoable revisioned manual edit** using the existing one-second after-last-change autosave. This is a fully optional editor UX independent of installed translators/AI. [Source Tag Transfer](../Source-Tag-Transfer/en.md).

## Operator-defined translation provider policies

Afutrans should let each Translation Target choose a reusable provider-selection profile and permitted overrides for individual jobs or Chain steps. A low-cost installation can prefer self-hosted LibreTranslate and escalate to a configured paid/AI endpoint only after a specified failure or unsuccessful mandatory validation. A different operator can make their licensed Google or Microsoft API the default. UI configuration must expose the effective provider order, compatibility filters (language/format/placeholder), eligibility and enabled fallback triggers, along with estimated/actual provider charges where the backend supports them.

This is configuration over the generic UnderstandMe provider selection boundary, not a separate Afutrans-only selection engine. Privacy/LocalOnly restrictions and spending ceilings cannot be bypassed by a UI user preference. No paid endpoint is implicitly called just because another provider returns a result. [Provider routing and cost](../Provider-Routing/en.md).

## Optional spare-capacity API and per-client plans

Afutrans may offer a **public API using spare capacity** of its own locally run LibreTranslate/other free endpoint(s), with administrators deciding whether the public endpoint is exposed at all. A `ClientPolicy` profile distinguishes normal internal users, named customers with paid-provider entitlements, registered free users and anonymous public users. A paid API account must not become accessible to a public/free client merely because Google is enabled for internal work.

Admin configuration includes permitted provider tiers/accounts, client quotas (words/day or other meter), maximum request/concurrency/rate, client priority ceilings, reserve for internal work and user-visible reasons for using a different provider. By default established/internal users have no additional provider restriction; the public/anonymous profile should be separately restricted. A **free/local-only public profile is one selectable configuration, not an invariant**; an administrator may explicitly choose broader public provider access or human-only operation. The same admission enforcement applies to direct .NET/REST/business SignalR entry points. See [Client Access Policies](../Client-Access-Policies/en.md).

## Private historical chat and forum translation

Afutrans may translate private chats or forum history and **save verified translations** so authorized readers reopening a message do not trigger new machine translations. Save in the source application's conversation/message-scoped result store (or configured access-aware adapter), **not** automatically in globally shared dictionaries or the general Editor. `P0` work remains volatile and Human-ineligible; result retention is independent.

`Privacy = Private` excludes automatic manual audit and content browsing by general administrators/translators, even when the translation result is persistent. The host controls who can view source messages and their translated results and must recheck access on every history lookup; content-free aggregate processing/status metrics may remain visible. Explicit consent and separately initiated non-P0 manual work would be required for intentional translation by a named human. [Privacy and Scoped Storage](../Translation-Privacy-and-Scoped-Storage/en.md).

## Recipient-corrected message translations

Afutrans (or any consuming chat/forum application) may embed an **optional recipient translation-correction tool**. Once a private message has been automatically translated, **its authorized recipient** may improve the translation for personal readability. This is not a Human worker request, general translator audit or dictionary correction; it must not make the source text visible to unrelated operators.

The recipient chooses whether that correction is **used only in the current view** or **cached for their own later viewing**. The app itself owns both original history results and any personal corrected override, keyed by its own message/revision, recipient and language. Optional broader contribution/publication is a **separate deliberate, authorized operation**, not an automatic consequence of permitting personal cache.

The Splitter's `0..N` IDs only correlate chunks within one translation request. The chat application reassembles full text and decides what, if anything, to save; unlike UI dictionary targets it does **not** register dynamic text as `KEY=VALUE`. [Chat Translation Flow](../Chat-Translation-Flow/en.md).

## Communication

Messenger delivers authorized local/SignalR events and status projections. Clients may submit requests, receive acceptance and progress, and confirm results. Reconnect uses replay/deduplication for durable messages. `Update_Cache` informs connected localizers of committed changed values. Messenger does NOT instruct workers or own queues.

Admin dashboard: detailed authorized targets, job text/history, work progress, priorities and backend utilization. Public dashboard: only anonymous aggregate counters and work visualization, with no client/text disclosure.

## Self-localization

Afutrans and compatible third-party Afutrans modules use English source strings and dictionary-less registration so new UI keys are discovered without prebuilding catalogs. UnderstandMe remains flexible enough for unrelated apps whose source language or key policy differs.

## Hierarchical SystemMonitor

Afutrans can publish its own events at `/Afutrans/SystemMonitor` and all installed module events at `/SystemMonitor`. Co-hosted UnderstandMe and AfroLT modules also have dedicated `/UnderstandMe/SystemMonitor` and `/AfroLT/SystemMonitor` feeds. Each event has the same typed format with sender identity. This is a best-effort outbound feed, never a command or archive. See [SystemMonitor](../SystemMonitor/en.md).

## Live monitoring UI

Afutrans may expose a clickable dashboard for the combined `/SystemMonitor` feed. Each administrator can select precisely which hosts, modules, instances, severities, event types and correlation IDs to display and save personal presets; filtering the view does not alter upstream publishers, the approval registry or other clients' views.

A monitoring admission failure is recorded as an incident and the invalid event is discarded, never auto-blocking the node. Incident review and any manual actions (mute, block, revoke or disconnect) are performed by authorized administrators through a separate control surface. Repeated incidents may be aggregated to keep the dashboard readable. See [SystemMonitor design](../SystemMonitor/en.md).

## Current repository findings (2026-10-10)

- Toolbox `UnderstandMe/UnderstandMe.csproj`: net10.0 skeleton, project references declared to the Essentials and WhenItFails package IDs; no implemented workflow verified.
- Afutrans `WebTranslator/WebTranslator.csproj` and `Afutrans.Core/Afutrans.Core.csproj`: target net11.0.
- Afutrans repository also contains current Avalonia and Terminal.Gui prototypes and a historical implementation plan for their shared UI skeleton. That older plan is not a specification for the future UnderstandMe/Afutrans engine.

Source repositories: [Toolbox](https://github.com/afrowaveltd/Toolbox) · [Afutrans](https://github.com/afrowaveltd/Afutrans).


## Rust localization client interoperability (planned, 2026-10-18)

Rust applications (including future Textscape components) need **only a small native localization runtime**, not a port of UnderstandMe, its translator providers or Afutrans orchestration:

- Load and use the **same flat JSON dictionaries** as the .NET localizer; provide normal lookup, placeholder substitution, fallback and optional dictionary-less registration of missing UI keys. Use compatible auxiliary hint/rule JSON **only when needed** for shared behavior. Remain usable fully offline without a .NET runtime.
- Include an **optional REST/HTTP client** capable of submitting a source dictionary (and relevant auxiliary metadata) to a configured .NET Afutrans/UnderstandMe service, requesting a target language translation/update, and **receiving the translated/updated dictionary** through the same ordinary versioned JSON REST API available to other clients. Rust does not implement translation providers, queues, editing/review workflows or an independent translation service.
- Keep the operation **explicitly optional**: local UI lookup, dictionary-less startup and fallback must not block on server connectivity. No mandatory online registration, telemetry or always-running connection.
- Define request/response contracts shared with .NET. A small translation can return the dictionary directly; long-running processing may return a job identifier for later status/result retrieval, without duplicating server queue logic in Rust. Concrete HTTP routes, JSON envelope and auth policies remain to be designed; no endpoints are implemented yet.
- For returned data, validate JSON and protected placeholder compatibility; preserve existing local/manual overrides and use a safe update (e.g. write temporary file, validate, then atomic replace) so an interrupted transfer cannot destroy a usable local dictionary. Respect source revisions and existing hint metadata on import.
- Common contract/conformance examples should verify the **same results** for equivalent Rust/.NET localizations from the same dictionary and metadata, but only for the intentionally shared runtime subset.

**Ownership boundary:** Rust = localizer + optional REST client; .NET UnderstandMe/Afutrans = translation, automation, provider access, editor and authoritative dictionary management. The file format and REST interface are interoperable; no duplication of the full .NET system is intended. This is a design decision only, with no current implementation or test evidence.

## Host-driven dictionary exchange via local adapters, REST and business SignalR (2026-10-18)

## Host-driven dictionary exchange via local adapters, REST and business SignalR (2026-10-18)

**Clarification of the Rust client note above:** dictionary exchange is a capability of **all appropriate localization hosts**, not a Rust-only feature. A Rust localizer remains minimal: read/format/reload/export/import flat JSON dictionaries and optional metadata, plus an optional REST client. It never embeds the .NET translation engine.

- **Desktop:** hot-reload a changed local dictionary/sidecar safely; optionally check for updates via a configured REST endpoint at startup or on user request. Keep working without any network connection.
- **Web:** provide authorized REST endpoints and optionally a separate **business SignalR** interface for retrieving and returning whole dictionaries or validated changes. Afutrans may pull a web app's dictionaries, translate/review them and push back the result; a web app may also initiate the exchange itself. Both directions are legitimate.
- **Automation:** a configured Afutrans Translation Target may discover/update dictionaries periodically or react to change notifications, applying target rules and existing workflow. Neither a web frontend nor a desktop app needs to run or reimplement the translator/queue engine.
- **Interoperability:** flat JSON dictionaries and optional per-key sidecar files remain portable between projects and runtimes. Exchange the relevant sidecar context, placeholder metadata and approved grammar variants together with dictionary entries.
- **Reliability:** validate returned JSON and placeholder integrity, preserve manual overrides, track source revisions to avoid stale overwrites, and activate an accepted update atomically. REST endpoints (or equivalent persistent store) remain authoritative; SignalR update messages can be best-effort and replayed from revision checks.
- **API separation:** dictionary business methods belong in an authorized localization interface, **not** the existing outbound-only `/SystemMonitor` Hub. REST and a business SignalR interface use the same application contracts, authorization, persistence and validation, and do not assume a specific UI framework.
- **No forced infrastructure:** network sync, hosting public endpoints and automatic translation are optional. The local dictionary-less first-use case works in an offline app with no Afutrans service running.

The exact methods, routes, update/version envelopes and transport specifics are deliberately pending. Documentation-only planned behavior; no implementation or tests claimed.

## Source-key discovery as optional translation trigger (2026-10-19)

Afutrans can be configured as a **subscriber** to dictionary-change notifications generated by a target's DataAccessor or localizer. Delivery may be a local event/Messenger, authenticated REST callback/webhook or a business SignalR call; none is required for hosts that bundle read-only desktop dictionaries. Online-backed desktop targets may participate just like web targets.

A `SourceKeyAdded` or other configured source revision change can request an eligible translation workflow through the normal QueueMaster admission path. Target-language `TranslationUpdated`, metadata refresh or cache reload should **not recursively trigger another automatic translation**. Correlation/operation IDs, accepted revisions and idempotent job admission prevent duplicate work when a file watcher and application write hook report the same logical change.

SignalR/ephemeral callbacks are optional accelerators; a configured periodic or startup scan, persisted change record or outbox is needed when translation jobs must survive disconnected listeners. No delivery callback is allowed to block or roll back a successful dictionary commit. REST and SignalR business contracts remain separate from outbound-only `/SystemMonitor`. The relevant target policy determines which languages, providers and permissions apply.

**Documentation/design only:** no target event emitter, REST/SignalR endpoint or automatic job admission implemented in this change.

