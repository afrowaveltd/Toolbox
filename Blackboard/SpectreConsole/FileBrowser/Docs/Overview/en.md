# FileBrowser: independently packageable TUI file/folder picker

Status: **design only**, 2026-10-22.

The FileBrowser component will become its own .NET project and NuGet package in `Blackboard/SpectreConsole/FileBrowser`. It is not the entire Blackboard or Spectre.Console toolkit.

## Scope

- Allow applications to select files or folders, optionally filter by filename/extension, navigate parent directories and platform roots and cancel explicitly.
- Work inside SSH terminals and local consoles without changing the host process's working directory or overwriting application-specific state.
- Show navigation and help shortcuts, supply reliable English labels, and allow arbitrary clients to override text for localization.
- Return a typed selection/cancellation outcome; error handling belongs in the component while schema validation and file writes belong to the consumer.
- Reference only the UI/framework dependencies necessary for this picker. It must not reference UnderstandMe or other Blackboard controls.

## Consumers

The future UnderstandMe Configurator will use this package to select `UnderstandMeSettings.json`; it will own its validation and save workflow. Other programs can use the same picker without installing or invoking any translation code. Blackboard may later contain a separate `Samples` app showing the picker in action; it must not be a dependency of this package.

## Release requirements

The first actual source change should include a self-contained `.csproj`, automated tests, documentation of public APIs (English XML comments), the updated [implementation status](../IMPLEMENTATION_STATUS.md), plus integration in Toolbox CI. Validate behavior before declaring a release.
