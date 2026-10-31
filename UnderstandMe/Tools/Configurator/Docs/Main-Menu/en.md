# Configurator interactive main menu — first implementation

**Implemented 2026-10-25.** A parameterless TUI launch displays a five-action menu, localized by the Configurator's own per-user UI setting. Selecting actions uses internal typed enum identifiers rather than matching translated display labels.

The menu and editor use `IAnsiConsole` injected at the boundary to permit virtual-terminal tests. Interactive startup and subsequent returns to the main or editor menus clear the screen; readable results are acknowledged before clearing. A new or loaded configuration is represented by `ConfiguratorSession`, which uses existing validated UnderstandMe settings APIs; no additional editor implementation is packaged as a shared NuGet yet.

## Workflows

- **Create:** type the destination path; refuse an already existing file; open an unsaved default draft and enter the editor. Save always requires review and user confirmation.
- **Edit:** choose Browse (published Blackboard FileBrowser, initial `*.json` mask) or manual path, load only valid schemas and enter the editor. Escape/back refuses to discard dirty changes without confirmation.
- **Test:** choose Browse/manual path, read and validate using the same `UnderstandMeSettingsJsonStore` as normal configuration editing; report errors without saving. No network providers are contacted.
- **Configurator settings:** select `English` or `Čeština`, persist only the separate per-user preferences and refresh the next menu immediately.
- **Exit:** return from the tool; no implicit host settings writes.

`--config` remains an optional direct jump into the editor; `validate` is a noninteractive path with no console clear and no app-preference dependency.

## Verification

Virtual-console tests cover keyboard menu navigation, the real FileBrowser path selection, returning to main from direct-open editor, offline testing without file mutation, English/Czech switch surviving app restart, broken preference fallback, and English/Czech embedded key parity. See [GitHub Actions #40231883535](https://github.com/afrowaveltd/Toolbox/actions/runs/40231883535): **SUCCESS, 14 Configurator tests passed**, alongside the full solution workflow.

The visible menu is a first slice. It does not yet implement all proposed CRUD metadata-driven provider forms or live connection checks.

## Screen lifecycle and readable results (2026-10-25)

The main menu clears the terminal before each **return from an action**, and the editing menu clears before each display. Initial startup still clears prior shell output. Clearing is owned by Configurator's screen transitions; the independently packaged Blackboard FileBrowser does not clear on directory navigation, and noninteractive `validate` has no terminal side effects.

Summary, validation and save results must not disappear immediately. The app presents an offline localized **Return to menu** action (Enter/Esc) that gives the user control over when results may be cleared. Ordinary prompts for changing scalar values can return straight to the refreshed editor menu; when a value is malformed, the error is acknowledged first. Failure to open/create settings is acknowledged before returning to the main menu. Canceling a file picker without diagnostics returns directly to a clean menu.

The virtual-console test suite checks ANSI clear-screen positions, repeated editor menu transitions, cancellation, corrupted JSON and preservation of files when navigating. [GitHub Actions #40233060410](https://github.com/afrowaveltd/Toolbox/actions/runs/40233060410) passed 18/18 Configurator tests.

## 2026-10-25 — optional live diagnostics

Test configuration first performs the existing offline structural check. Its result screen now offers either Return or explicit **Test provider connections (live)**. Enabled LibreTranslate providers are tested with bounded GET `/languages`; a failed response is advisory and does not affect Save. The editor also exposes the same live action on a draft, alongside metadata-driven provider and storage registration editors. See [Live Connection Diagnostics](../Live-Connection-Diagnostics/en.md) and [Provider/Storage Editor](../Provider-Storage-Editor/en.md).

## 2026-10-29 — built-in offline Help

The main menu and editing screen now include Help. Help topics are selected by stable IDs and displayed from English Markdown embedded resources; labels are localized. Translation-provider and dictionary-storage registration menus also have a contextual Help entry. Navigation through help does not change draft settings. See [Offline Help](../Offline-Help/en.md). No network documentation updater or general-purpose Markdown AST is implemented in this increment.
