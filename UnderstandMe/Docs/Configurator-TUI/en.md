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
