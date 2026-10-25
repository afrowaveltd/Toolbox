# UnderstandMe Configurator

Standalone **.NET 10** offline TUI application for creating, editing and validating UnderstandMe JSON configuration files. Uses Spectre.Console and the published [Blackboard FileBrowser 0.1.0-preview.1](https://www.nuget.org/packages/Afrowave.Toolbox.Blackboard.SpectreConsole.FileBrowser/0.1.0-preview.1). The UnderstandMe core library is UI-independent.

## Run

From the Toolbox repository root:

```bash
dotnet run --project UnderstandMe/Tools/Configurator
dotnet run --project UnderstandMe/Tools/Configurator -- config --config /path/to/UnderstandMeSettings.json
dotnet run --project UnderstandMe/Tools/Configurator -- validate --config /path/to/UnderstandMeSettings.json
dotnet test UnderstandMe/Tools/Configurator.Tests/UnderstandMe.Configurator.Tests.csproj
```

**No command-line arguments are required for interactive use.** The app clears the terminal at launch and on subsequent menu returns, and presents this first-level menu:

| Action | Current behavior |
| --- | --- |
| Create configuration | Enter a new file path, create an unsaved in-RAM default, edit, validate and explicitly confirm Save |
| Edit configuration | Browse existing JSON files with Blackboard FileBrowser or type a path, then edit a RAM draft |
| Test configuration (offline) | Browse/type an existing path and run the **shared strict JSON/schema validation** without writing or contacting providers |
| Configurator settings | Choose English or Czech; persist the UI preference independently from edited host configuration |
| Exit | Quit without saving; the editor asks before discarding an unsaved draft |

The editor implements summary, default source-language and worker-buffer edits, plus descriptor-driven provider/storage registration and option editing. It delegates authoritative validation and checked-revision saving to `UnderstandMeSettingsJsonStore`. Unknown configuration fields, unknown provider types, unsupported schemas and stale file revisions fail safely; the original file is not silently rewritten. Direct `--config` opens the editor before returning to the main menu. Noninteractive `validate` returns 0 for valid settings, 1 for missing/invalid files and 2 for malformed CLI arguments.

## UI language and independent preferences

Two **bundled, manually maintained** resource dictionaries are compiled into the executable: [English](Resources/en.json) and [Czech](Resources/cs.json). Both the Configurator UI and the embedded FileBrowser's labels are localized. English remains the offline bootstrap fallback; neither startup nor translation requires a running UnderstandMe host or network.

The app stores its own versioned preferences separately from `UnderstandMeSettings.json`:
- Linux: `$XDG_CONFIG_HOME/afrowave/understandme-configurator/settings.json` when XDG_CONFIG_HOME is absolute; otherwise `~/.config/afrowave/understandme-configurator/settings.json`
- Windows: `%APPDATA%/afrowave/understandme-configurator/settings.json`

The file is created **only after explicitly changing the UI language**. Invalid/unsupported preferences are not overwritten or auto-migrated; startup falls back to English. Writes are same-directory temporary-file replacements with a SHA-256 revision check (not a cross-process transaction).

## Current boundaries and next work

**Test configuration** currently means offline structural/semantic validation, **not** a network connectivity test or live provider diagnostic. Core validator messages/JSON paths remain in English; the Configurator's own captions, prompts and FileBrowser labels are bilingual. Full metadata-driven storage/provider editing, extensive redacted diffs, direct Ctrl+S / Ctrl+V, language package downloads, remote-target connectors and opt-in app update checks are future increments.

As discussed, AfuTrans TUI (Terminal.GUI), GUI (Avalonia) and Web will eventually share **only genuinely reusable functions** with this Configurator, preferably via small UI-neutral libraries/NuGet packages when actual duplication emerges. Do not couple Configurator to AfuTrans or create a generic shared library prematurely.

See [Localization and Preferences](Docs/Localization-and-Preferences/en.md), [First Integration](Docs/First-Integration/en.md), and [implementation status](IMPLEMENTATION_STATUS.md).

## Clean menu transitions (2026-10-25)

The Configurator clears its interactive terminal on startup **and whenever it returns to the main menu or refreshes the editor menu**. FileBrowser itself is unchanged and is not cleared during individual folder navigation. Editing and return-to-menu actions do not leave a stack of earlier menus on the screen.

Results that need to be read (configuration summary, validation findings, save confirmation/errors or user-preference write errors) remain visible until you choose **Return to menu** using Enter or Esc; only then does the application clear the screen and render the next menu. The acknowledgement prompt is manually localized in `en` and `cs`. The noninteractive `validate` command does not clear output or require acknowledgement.

[GitHub Actions #40233060410](https://github.com/afrowaveltd/Toolbox/actions/runs/40233060410) passed the initial change with **18/18 Configurator tests**, including targeted menu-clear and diagnostic-acknowledgement regressions.

## Provider and storage editing (2026-10-25)

While editing a configuration, use **Translation providers** or **Dictionary storages** to list/add/edit/remove registrations based on the existing UnderstandMe core `SettingsDescriptorRegistry`. Built-in `LibreTranslate` and `FileJson` descriptors define the available options, defaults, constraints, choice lists, and types. Provider Enabled, storage SourceLanguage and dictionaryless SetValueFromKey are editable. Changes remain in the draft until explicit, revision-checked Save; full-document dirty tracking also detects component changes. Secret references remain redacted in the menu and are never shown as input defaults. Additional third-party descriptors are not yet registered by this tool.

## Offline validation and live connection testing (2026-10-25)

**Test configuration** still validates the file offline first; it now also offers an **optional live provider connectivity check**. You can run the same check from the editor against unsaved changes. Enabled LibreTranslate registrations receive **GET /languages**, with a bounded timeout; HTTP 2xx + any parseable JSON value indicates the endpoint **responded**, not that a translation or authenticated API request would succeed. Disabled/unsupported providers are skipped; unreachable servers report diagnostic failures. No API keys, translation text or cookies are sent. **Connection failures never block Save** and no probe is run implicitly during Save or startup. Full details and limits: [Live Connection Diagnostics](Docs/Live-Connection-Diagnostics/en.md) and [Provider/Storage Editor](Docs/Provider-Storage-Editor/en.md).
