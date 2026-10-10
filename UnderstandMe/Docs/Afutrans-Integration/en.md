# Afutrans Host and External Integration

Status: planned usage of UnderstandMe; existing Afutrans repo is an early experimental application.

## Host forms

Afutrans should run locally for one developer or persistently on a server managing many configured Translation Targets. Its interfaces include WebUI (first practical host), Avalonia GUI, and Terminal.Gui v2 TUI. These are presentations over reusable UnderstandMe capabilities; translation and queue logic does not belong exclusively to a UI.

Afutrans needs an on-screen text translator, file translator (Markdown, HTML, plain text and extensible types), dictionary import/export with auto-detection and manually configurable database/schema profiles, a side-by-side editor, a scheduler, a prompt/Chain editor, API integration, and optional bidirectional SignalR.

WebUI may access the host filesystem for legitimate dictionary editing, but deployed installations must restrict access to authorized roots, users and destinations. A local installation and a VPS service have different trust boundaries.

## Targets and sources

Each Translation Target describes data locations, dictionaries, default culture, target/ignored or manual-only languages, key strategy (`key=value` or symbolic), format adapter, access rights, live translation, Chain/Pipe overrides, cache behavior, sync schedule and retention. A connector's capabilities must be checked before writing back; results can be returned to the requester instead.

Configuration may add/remove targets, endpoints, workers, pipelines and prompt profiles without code changes. Scheduling can discover and reconcile missing/changed source keys and produce background jobs. Event-driven updates plus a periodic full scan (illustrative default: every three hours) are complementary.

## Operator-defined translation provider policies

Afutrans should let each Translation Target choose a reusable provider-selection profile and permitted overrides for individual jobs or Chain steps. A low-cost installation can prefer self-hosted LibreTranslate and escalate to a configured paid/AI endpoint only after a specified failure or unsuccessful mandatory validation. A different operator can make their licensed Google or Microsoft API the default. UI configuration must expose the effective provider order, compatibility filters (language/format/placeholder), eligibility and enabled fallback triggers, along with estimated/actual provider charges where the backend supports them.

This is configuration over the generic UnderstandMe provider selection boundary, not a separate Afutrans-only selection engine. Privacy/LocalOnly restrictions and spending ceilings cannot be bypassed by a UI user preference. No paid endpoint is implicitly called just because another provider returns a result. [Provider routing and cost](../Provider-Routing/en.md).

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

