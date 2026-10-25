# UnderstandMe Configurator — interactive terminal settings editor

Status: **approved design direction, not implemented** (2026-10-22).

## Goal

Provide an intuitive interactive terminal application for configuring UnderstandMe and Afutrans without requiring developers to study CLI switches or the JSON structure. The proposed implementation uses Spectre.Console for selection prompts, forms, tables, trees, confirmation and status output. Keep this as a **separate executable** with its own optional Spectre.Console dependency; do not pull Spectre into the core UnderstandMe library.

The application edits the canonical `UnderstandMeSettings.json`, using the exact **same shared typed settings, validators, migrations, secure persistence service and defaults** as the host. Do not replicate validation rules in the UI.

## Suggested flow

1. Load configuration by explicit path or a documented default; clearly distinguish missing, valid, invalid and unsupported-future-schema files.
2. Show hierarchical overview: Localization/Storages, Translation/Providers, QueueMaster/Worker Buffers, Scheduler, Cache/Persistence, Security/Authentication, Optional Modules.
3. For each component allow View, Add, Edit, Remove, Enable/Disable. Display current and default values, descriptions, effects, units, allowed ranges and helpful examples alongside input.
4. Keep edits in a separate draft, with Cancel/Revert and a diff/summary of pending changes. Require confirmation for destructive edits and sensitive switches.
5. Validate a full configuration before saving, with error paths/field names and proposed remedies. Warnings should not be silently treated as successes.
6. Save only validated drafts through the shared persistence layer, with concurrency checks and previous-good recovery. Do not silently write environment overrides or resolved secrets back to JSON.
7. Provide a noninteractive `validate`/CI mode when useful, but the TUI is the primary user experience.

## Architecture

- Core: typed `UnderstandMeSettings`, `LocalizationSettings`, `TranslationSettings`, component-owned settings; validation, schema migrations, JSON load/save, immutable effective configuration. No Spectre dependency.
- Separate Configurator application: Spectre.Console renderers, navigation and field editing, help and summaries. A thin UI over core services.
- Extensibility: each module/backend/storage can provide **safe metadata** describing its editable settings (human-friendly display name, documentation, units, options, constraints and secret-reference semantics). Actual validation remains authoritative in core. Never reflect or print sensitive values.
- Exit/help: clear navigation, search/filter for large provider/storage lists, terminal-size adaptation and accessibility/no-color fallback.
- Localization: ship reliable built-in English text first. Later use UnderstandMe's own offline localization with English fallback; startup and configuration repair MUST work when Translate/QueueMaster/backends are disabled or broken.

## Security and robustness

- Show **credential references**, not decrypted API keys; support separate protected secret-entry workflow if added later, without echoing secrets.
- Prevent accidental overwrite of external edits by comparing configuration revisions at save time. The program should not need admin privileges for reading ordinary configuration files, but should handle read-only paths cleanly.
- No live mutation of running host state without explicit support/authorization; editing a file and applying/restarting service are separate actions.
- Reject unsupported future schema before save to avoid stripping unfamiliar configuration sections; preserve extension data by design.
- Log diagnostics without secret values or private translation content.

## Verification before release

- Core tests: complete settings JSON round-trip, schema/version/error handling, unknown extensions, defaults, validation, atomic-or-safe save interruption, concurrent revision conflict, no credential leakage.
- TUI tests: navigation, add/edit/delete/cancel, invalid input and boundary numbers, disabled module sections, duplicate provider IDs, unsaved changes, error feedback, back/exit, unusable terminal and Unicode labels, confirmation/cancel destructive actions.
- Integration test: generated `UnderstandMeSettings.json` loads successfully into UnderstandMe and Afutrans. LibreTranslate configurations include local no-key and remote credential-reference setups, not live secrets or forced network calls.
- All public API carries XML documentation; non-obvious implementation decisions also receive concise explanatory English `//` comments. Update root README and `IMPLEMENTATION_STATUS.md` alongside code.

## Delivery order

First implement and test shared strongly typed settings plus validation and persistence. Then create the separate Spectre.Console TUI over those tested services. Add self-localization later without making the editor depend on a functioning translation backend.

## Primary deployment scenario: SSH/headless servers (2026-10-22)

- Treat **TUI as the primary configuration editor** because UnderstandMe often runs on a headless Linux server and must be manageable from an ordinary SSH terminal. Avalonia GUI is an optional later frontend sharing the same configuration service; never require a graphical session to administer the host.
- Proposed entry point: `understandme config`, with `understandme config validate` for noninteractive validation. Final executable name and packaging are to be chosen during implementation.
- Maintain an in-memory edit draft until explicit validated save; SSH hangup, terminal failure or Ctrl+C before save must leave the existing settings file untouched. Save uses checked revision and safe replacement, handles signals and narrow terminals, and must not echo secret values.
- Support common terminal modes and basic keyboard-only use; provide a plain-text/no-color fallback. The editor must remain useful even if translation backends or UnderstandMe service are stopped.
- Add automated tests for interrupted SSH-like sessions, no-save exit, read-only files, terminal-size fallback, and validated save after reconnection.


## Explicit Ctrl+S commit workflow (2026-10-22)

The TUI must prominently display the keyboard shortcut **Ctrl+S — Review & Save** in its help/footer, and handle it through the terminal input loop where supported. The entire editing session operates on an isolated in-memory draft; navigation, Add/Edit/Delete/Enable/Disable never write configuration files or mutate active service configuration.

On Ctrl+S:
1. Compute and display a concise, redacted summary/diff of **all** pending edits (additions, removals, value changes). Do not reveal secrets or sensitive connection strings.
2. Ask the user to **confirm** proceeding. Declining returns to the unchanged draft with no disk write.
3. **After confirmation**, validate the complete candidate configuration with the shared core validators. On failure, display field-specific errors, retain the editable draft in RAM, and allow another Ctrl+S attempt after correction; original file stays untouched.
4. On success, check for external modification since load, and persist via the shared safe/atomic save service. A conflict or I/O failure retains the draft and leaves the last-good persisted configuration intact.
5. Only after confirmed successful write, refresh the baseline and mark the draft clean. Show a clear saved status; never claim success earlier.

Ctrl+C, SSH disconnect, program failure or Exit before the successful Ctrl+S workflow must not save automatically. For a dirty draft on ordinary Exit, optionally warn and ask whether to discard/cancel exit, but do not save implicitly. A terminal which cannot forward Ctrl+S (e.g. flow-control interception) must offer a visible equivalent **Review & Save** menu action. Use keyboard-only interaction without requiring function keys.

The change summary may be produced before validation as requested, but no unvalidated settings may be persisted. Tests must verify no writes before Ctrl+S, redacted diffs, declined confirmation, invalid draft retention, concurrent external changes, I/O failures, repeated Ctrl+S, no-op saving, interrupted terminal sessions and final successful save. XML-document public TUI interaction contracts and comment non-obvious input/transaction behavior in source.


## Keyboard-first interaction and independent validation (2026-10-22)

Adopt a persistent, context-sensitive shortcut footer inspired by Nano/Midnight Commander; shortcuts are always shown alongside their textual labels, with a complete F1 help screen. Initial proposed bindings:

| Shortcut | Action |
| --- | --- |
| `Ctrl+V` | Validate the **in-RAM draft** without persisting or applying it |
| `Ctrl+S` | Review changes -> confirm -> validate -> safe save |
| `Ctrl+N` | New item in the current component collection |
| `Ctrl+E` | Edit the selected item |
| `Ctrl+D` | Delete selected item, with confirmation |
| `Ctrl+Z` / `Ctrl+Y` | Undo / Redo changes to the draft |
| `F1` | Context-sensitive help |
| `Esc` | Back / cancel dialog without saving |

Keyboard bindings are provisional and must be tested on common terminal emulators, SSH, tmux and shell/terminal flow-control configurations; reserve fallback menu actions when a chord is intercepted. Inside text editing controls, define unambiguous local shortcut behavior (e.g. terminal paste may also use Ctrl+V); do not silently eat text-edit actions.

`Ctrl+V` runs pure, deterministic schema/semantic/cross-component validation on the **complete current draft**, reports errors/warnings with component and field paths, and **does not touch the settings file**, activate services, call remote backends, or discard edits. Validation results refer to a particular draft revision and become stale immediately after an edit. Validation errors are actionable and may be selected to navigate to the offending field.

Keep optional `Test Connection` **distinct and opt-in**: it can contact external backends/databases, needs explicit authorization and timeouts, and is not a prerequisite to saving a syntactically and semantically valid configuration. Never transmit private text as a connectivity test.

`Ctrl+S` retains the established order: show redacted changes -> explicit confirmation -> revalidate the exact latest draft -> check external file revision -> safely save. A previous successful `Ctrl+V` is informational, not a skip-validation token; race or changed draft must not bypass save-time validation.

The footer should clearly indicate dirty/clean draft, latest validation status, selected context, and which shortcuts are available. Undo/Redo cannot mutate on-disk settings. Test input focus, shortcut conflicts, repeated validation, canceled validation, long/erroring validators, changed-draft result invalidation, SSH disconnect, keyboard-only recovery and small-screen layout.

Documentation/design only; no TUI executable implemented yet.


## Universal metadata-driven backend configuration (Dita reference, 2026-10-22)

Inspected the actual `afrowaveltd/Dita` sources: `Dita.Shared/Localization/Services/ILibreTranslateService.cs` declares text/file translation, language detection, supported languages and latency; `LibreTranslateService.cs` implements HTTP and retry/throttling behavior; `Dita.Shared/Localization/Models/AutomaticTranslationSettings.cs` contains LibreTranslate URL, API key fields, language, endpoint, throttle/timeout and also application/scheduling-specific options. `Dita.Server` consumes this Shared implementation. These are reference implementation assets, **not yet UnderstandMe public contracts**.

The TUI must not ship a handwritten settings screen for each backend. Separate:

1. **Execution contract**: translation / detection / file translation and capability discovery, implemented by an adapter. Keep backend-specific API models internally; normalize outputs and errors into UnderstandMe contracts.
2. **Configuration descriptor contract**: a safe, typed manifest of the backend settings tree. Proposed interfaces/names (design, not current code): `ITranslationBackendDescriptor`, `BackendCapabilities`, `SettingDescriptor`, `IComponentSettingsValidator`, and optionally `IBackendDiagnostics`.
3. **Generic editor**: render fields and nested groups based on manifest data (display names, stable property IDs, types, required/optional, help text, defaults, allowed ranges, choice provider, visibility/dependency rules, secure credential-reference semantics). Reuse for storage/scheduler/module manifests as well as translation backends.
4. **Validation**: reusable generic schema validation + mandatory component-specific semantic validation; diagnostics return stable field paths for jumping from `Ctrl+V` errors to inputs. The editor is not a source of truth for validity.
5. **Capabilities and live information**: distinguish *static declared* capabilities from *runtime-discovered* languages/models/health. Languages may be dynamically fetched from a backend on explicit `Refresh/Test Connection`, with offline manual entry and last-known nonsecret metadata; generic `Ctrl+V` never triggers network traffic.

Suggested field kinds include string, URI, number, boolean, enum/choice, duration, collection, structured object and **secret reference** (never print plaintext secrets). A provider can expose custom UI extensions later if a field cannot be expressed in the generic metadata model, but the common path requires no provider-specific TUI code.

Dita migration cautions: `AutomaticTranslationSettings` currently mixes provider-specific fields with app-wide scheduler and localization fields; distribute them into provider/localization/scheduler settings at the new boundary. Do not copy a literal `Key` into `UnderstandMeSettings.json`; replace with a secure credential reference. Normalize declared capabilities independently of `ILibreTranslateService` so that all providers are represented consistently. Support multiple instances of the same backend type under distinct provider IDs.

Suggested tests: descriptor completeness, generated form schema/JSON round-trip, conditional fields, malformed or untrusted manifests, provider-specific validation, mixed providers, two LibreTranslate instances with distinct policies, offline language discovery, live metadata refresh opt-in, sensitive fields redacted, dynamic plugin registration/unregistration, unknown backend IDs and backward-compatible settings migration. Document provider extension authoring in English XML docs and topic docs; explain tricky mapping logic with ordinary C# comments.

**Status: architecture proposal based on inspected Dita code; neither descriptors nor dynamic TUI rendering implemented yet.**

## Discover and browse settings files (2026-10-22)

Reviewed [Lutonet/SpectreConsoleFileBrowser](https://github.com/Lutonet/SpectreConsoleFileBrowser) as an early Spectre.Console file selector reference. The chosen direction is a modernized standalone picker inside the optional configurator, with safe working-directory-independent navigation, filtered JSON selection, localized text resources and no implicit writes. The settings file is selected before loading the edit draft; `Ctrl+V` and `Ctrl+S` continue to use the existing validation/persistence services. See the detailed [file picker review](../Configuration-File-Browser/en.md).

**Reference review only; no implementation or tests added here.**

## Reusable widget home: Blackboard (2026-10-22)

The component family [Blackboard](../../../Blackboard/README.md) is now documented as the proposed location of reusable TUI controls. The modernized Spectre.Console FileBrowser is a **separate independently packaged NuGet component** under `Blackboard/SpectreConsole/FileBrowser`, **not** part of UnderstandMe or a monolithic Blackboard package. The Configurator is a real **UnderstandMe-owned** helper application (proposed `UnderstandMe/Tools/Configurator`) that references only the Blackboard components it needs and UnderstandMe's core settings descriptors/validation/persistence. Blackboard itself may later have separate `Samples` showcasing its widgets; those are demonstrations, not production editors. The future GUI *Whiteboard* family follows the same per-widget packaging rule. See [independent packaging](../../../Blackboard/Docs/Independent-Packaging/en.md). The reusable Blackboard FileBrowser picker and its [standalone sample](../../../Blackboard/Samples/FileBrowser.Sample/README.md) are now implemented and CI-tested. **UnderstandMe's own Configurator executable is not implemented yet**; integration into the UnderstandMe settings editor remains future work.

## 2026-10-25 — first executable Configurator slice

The first UnderstandMe-owned executable now connects the independently published Blackboard FileBrowser picker to shared strict settings validation and revision-checked storage. See [First Integration](../../Tools/Configurator/Docs/First-Integration/en.md). Full descriptor forms and Ctrl+S/Ctrl+V keyboard chords remain planned; the initial editor offers visible menu equivalents for Validate and Review & Save.

## 2026-10-25 — next design discussion: local standalone preferences and self-localization

The Configurator should install and run as a **standalone TUI application** on the user's computer, without any mandatory startup parameters. Proposed primary menu: **Create configuration**, **Edit configuration**, **Test configuration**, **Configurator Settings**, **Exit**. CLI arguments such as `validate --config` remain optional automation conveniences. The specific meaning and scope of Test configuration (schema checks versus opt-in connection diagnostics) is **still to be designed** and must not be silently reduced to one category.

The Configurator needs its **own per-user preferences**, including UI language, separate from whichever host's `UnderstandMeSettings.json` is currently being edited. Initially ship two bundled, manually maintained language resources, **English (`en`) and Czech (`cs`)**, to verify localization and language switching. English must remain an unconditional **offline bootstrap fallback**, so the editor can repair a broken UnderstandMe configuration without a working translator. Later migrate UI lookup to reusable UnderstandMe localization primitives without making startup depend on online translation services. Optional update checks and additional downloadable language packs are for subsequent versions, with explicit user control; none is implemented yet.

Current implementation step only: clear the interactive terminal once at startup, not before every picker/menu action; preserve noninteractive output unmodified. See [Localization and Preferences](../../Tools/Configurator/Docs/Localization-and-Preferences/en.md).

## 2026-10-25 — parameterless bilingual main menu implemented

The [standalone Configurator](../../Tools/Configurator/README.md) now exposes **Create / Edit / Test (offline) / Configurator Settings / Exit** without mandatory CLI arguments. `ConfiguratorStrings` uses two embedded manually maintained `en` and `cs` dictionaries; even the consuming application's Blackboard FileBrowser labels are translated. The per-user Configurator preference store is separate from edited service settings and remains operational without translation backends. English is always the offline fallback for broken user preferences. See [Main Menu](../../Tools/Configurator/Docs/Main-Menu/en.md) and [Localization and Preferences](../../Tools/Configurator/Docs/Localization-and-Preferences/en.md).

The current offline Test checks strict JSON/descriptor validation **only**. Future provider connectivity diagnostics, metadata-driven provider/storage forms, downloadable languages, auto-update and self-localization through UnderstandMe itself have not been built. [GitHub CI #40231883535](https://github.com/afrowaveltd/Toolbox/actions/runs/40231883535) passed 14 Configurator tests.
