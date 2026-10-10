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

## Operator-defined translation provider policies

Afutrans should let each Translation Target choose a reusable provider-selection profile and permitted overrides for individual jobs or Chain steps. A low-cost installation can prefer self-hosted LibreTranslate and escalate to a configured paid/AI endpoint only after a specified failure or unsuccessful mandatory validation. A different operator can make their licensed Google or Microsoft API the default. UI configuration must expose the effective provider order, compatibility filters (language/format/placeholder), eligibility and enabled fallback triggers, along with estimated/actual provider charges where the backend supports them.

This is configuration over the generic UnderstandMe provider selection boundary, not a separate Afutrans-only selection engine. Privacy/LocalOnly restrictions and spending ceilings cannot be bypassed by a UI user preference. No paid endpoint is implicitly called just because another provider returns a result. [Provider routing and cost](../Provider-Routing/en.md).

## Optional spare-capacity API and per-client plans

Afutrans may offer a **public API using spare capacity** of its own locally run LibreTranslate/other free endpoint(s), with administrators deciding whether the public endpoint is exposed at all. A `ClientPolicy` profile distinguishes normal internal users, named customers with paid-provider entitlements, registered free users and anonymous public users. A paid API account must not become accessible to a public/free client merely because Google is enabled for internal work.

Admin configuration includes permitted provider tiers/accounts, client quotas (words/day or other meter), maximum request/concurrency/rate, client priority ceilings, reserve for internal work and user-visible reasons for using a different provider. By default established/internal users have no additional provider restriction; the public/anonymous profile should be separately restricted. A **free/local-only public profile is one selectable configuration, not an invariant**; an administrator may explicitly choose broader public provider access or human-only operation. The same admission enforcement applies to direct .NET/REST/business SignalR entry points. See [Client Access Policies](../Client-Access-Policies/en.md).

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

