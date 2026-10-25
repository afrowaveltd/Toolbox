# Configurator launcher, localization and user preferences

**Status (2026-10-25): parameterless main menu, offline English/Czech localization and persistent user language preference implemented; network features remain future work.**

## Product boundary

The standalone Configurator is a lightweight client for managing UnderstandMe configurations outside AfuTrans. AfuTrans itself remains the planned all-in-one product with TUI (Terminal.GUI), GUI (Avalonia) and Web frontends. Their business logic should share the **specific genuinely identical reusable functions**, without imposing the same UI toolkit or prematurely turning all editor behavior into a monolithic shared library. A small UI-neutral NuGet package is appropriate if a repeatable common function appears.

Configurator does **not** require an AfuTrans installation, translation service or any remote connection. No mandatory CLI arguments are needed.

## Parameterless main menu

On interactive start, clear the active console once, read Configurator's own preferences and display:
1. Create configuration: user enters a new path; create a default in-memory draft without writing; edit, validate, confirm Save.
2. Edit configuration: existing JSON through the published Blackboard FileBrowser (`*.json` filter) or manual path entry; edit draft through the shared core settings store.
3. Test configuration (offline): select a JSON file and validate using the core schema and option descriptors, without saving or contacting a provider. This **does not** imply the actual translation provider is reachable.
4. Configurator settings: switch language between English and Czech and persist only the Configurator preference.
5. Exit: no implicit write.

Optional `config --config <path>` still jumps into an editor session; `validate --config <path>` remains an unmodified noninteractive CI/automation command. The editor asks before discarding unsaved changes. The TUI itself is not yet a full descriptor-driven provider/storage editor.

## Two hand-maintained embedded languages

The executable embeds `Resources/en.json` and `Resources/cs.json` using fixed manifest resource names. Each has stable key/value pairs, maintained by hand. `ConfiguratorStrings` uses a selected translation, then a built-in English fallback, and finally the key for unavailable messages. Tests ensure matching key sets, no empty translations, locale switching, and fallback.

The TUI menu, prompts, status messages and the **consumer-supplied FileBrowser labels** use the same resources. Neither the independent Blackboard FileBrowser package nor UnderstandMe core now depends on a Configurator language or resource file.

This is the safe bootstrapping layer for eventually localizing the tool **with UnderstandMe itself**. A future integration may delegate to core localization contracts, but repairing broken host configuration must always work fully offline.

### Known incomplete translation areas

The shared validator's English diagnostic messages are displayed unchanged, together with JSON property paths. This is deliberate: error identities are not yet available as localized message IDs. Full validation diagnostics localization needs a future structured error contract, not fragile string replacement. Errors from the operating system can also contain OS-localized or English text.

## Separate per-user app settings

`ConfiguratorPreferencesStore` manages only `SchemaVersion: 1` and `Language: "en"|"cs"` in the initial release; it neither reads nor modifies the edited host's `UnderstandMeSettings.json` as a side effect.

Default location:
- Linux: absolute `XDG_CONFIG_HOME` if set, otherwise user `~/.config`, then `afrowave/understandme-configurator/settings.json`.
- Windows: user `ApplicationData` directory, then `afrowave/understandme-configurator/settings.json`.

Tests inject a private preferences path. Loading when missing yields English in memory **without creating a file**. Invalid JSON, unsupported schema/locale or unknown settings fields are not normalized on load. In that case the Configurator still opens in English and displays an error; a subsequent write refuses to overwrite the invalid preexisting file.

The store validates the candidate, uses a same-directory temporary file flushed before a move, and checks the original SHA-256 revision before replacement; this is **not** a transactional compare-and-swap against arbitrary simultaneous writers. No user secret is stored by these first preferences.

## Future increments (not implemented)

- Use UnderstandMe's localization primitives for higher-level UI translation while retaining bundled offline fallback.
- Additional language packs, their formats, localization QA and integrity-checked optional download.
- User-controlled application update checks, with no network activity required at startup.
- Optional connection/remote integration tests with explicit timeouts and authorization, separate from deterministic offline validation.
- Expanded metadata-driven provider/storage forms, granular redacted diff, undo/redo, direct Ctrl+S/Ctrl+V and terminal/SSH manual smoke.

See the [Configurator README](../README.md) and [Configurator-TUI](../../../Docs/Configurator-TUI/en.md).
