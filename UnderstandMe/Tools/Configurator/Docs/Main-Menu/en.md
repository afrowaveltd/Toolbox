# Configurator interactive main menu — first implementation

**Implemented 2026-10-25.** A parameterless TUI launch displays a five-action menu, localized by the Configurator's own per-user UI setting. Selecting actions uses internal typed enum identifiers rather than matching translated display labels.

The menu and editor use `IAnsiConsole` injected at the boundary to permit virtual-terminal tests. Only the initial entry clears the screen. A new or loaded configuration is represented by `ConfiguratorSession`, which uses existing validated UnderstandMe settings APIs; no additional editor implementation is packaged as a shared NuGet yet.

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
