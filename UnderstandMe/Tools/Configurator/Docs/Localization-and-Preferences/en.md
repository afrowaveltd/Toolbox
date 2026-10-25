# Configurator localization, launcher and user preferences

**2026-10-25 — design direction under discussion, not an implementation claim.** The existing Configurator is a first functional editor slice; the behaviors below remain future increments.

## Independent application state

The Configurator is a standalone user application, not the host whose `UnderstandMeSettings.json` it edits. Its UI language and future optional update preferences belong in a separate **per-user Configurator settings** document. Keep editing several host configurations independent from the app's own preferences. Use OS-appropriate user configuration directories (XDG-style location on Linux, AppData on Windows); exact precedence, schema and naming remain to be agreed. No write to the currently edited host settings just to change the editor's language.

## Two initial languages and self-localization

Bundle **`en` and `cs`**, initially translated and maintained manually, to exercise real localization behavior, not a hardcoded English UI. Use stable UI message keys and preserve placeholder safety. The editor must always start offline with English strings, even when the target UnderstandMe installation, provider configuration, local dictionaries or network connection is broken. Later adapt the UI to UnderstandMe's own localization facilities with a small, safe bootstrap path rather than creating a second translation system inside the editor.

Future optional features: update checks and downloads of more language packs, subject to explicit user preferences and safe integrity verification. No network access should become a prerequisite to launch the editor.

## Parameterless startup

The main TUI launcher should expose all functions without required command-line parameters. Proposed first-level entries:

- **Create configuration**: prepare a new, validated draft and explicitly choose where to save.
- **Edit configuration**: select existing settings with Blackboard FileBrowser, then use the shared revision-aware editor.
- **Test configuration**: detailed workflow to be decided. Distinguish deterministic offline schema/semantic validation from optional authorized live provider tests.
- **Configurator Settings**: user-local UI language, with other client preferences added when needed.
- **Exit**: never silently persist a host configuration draft.

Optional CLI arguments remain useful for scripting and direct path selection, but are not necessary for ordinary use.

## Already implemented in this increment

The Configurator now clears the interactive terminal once at startup, before its first menu or configuration diagnostics. It does not clear during file browsing, and the existing noninteractive `validate` command does not clear the screen. The Blackboard Showcase applies the same startup-only policy. Menu redesign and bilingual dictionaries are separate future work.
