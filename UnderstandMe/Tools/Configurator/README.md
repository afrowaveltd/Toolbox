# UnderstandMe Configurator

First executable **offline TUI settings editor** for UnderstandMe (`net10.0`), consuming the published [Blackboard FileBrowser](https://www.nuget.org/packages/Afrowave.Toolbox.Blackboard.SpectreConsole.FileBrowser/0.1.0-preview.1) from NuGet, separate from the core library.

## Try it

Run from the Toolbox repository root:

```bash
dotnet run --project UnderstandMe/Tools/Configurator
dotnet run --project UnderstandMe/Tools/Configurator -- config --config /path/to/UnderstandMeSettings.json
dotnet run --project UnderstandMe/Tools/Configurator -- validate --config /path/to/UnderstandMeSettings.json
dotnet test UnderstandMe/Tools/Configurator.Tests/UnderstandMe.Configurator.Tests.csproj
```

Without an explicit `--config`, the editor loads an existing `UnderstandMeSettings.json` in the current working directory, or shows a menu to browse `*.json` using FileBrowser, open a path or create a new *unsaved* default configuration.

The first editor supports displaying a non-sensitive summary, editing **DefaultSourceLanguage** and **DefaultWorkerBufferCapacity**, validation of the complete RAM draft and **Review & Save** with confirmation. Actual persistence delegates to the core `UnderstandMeSettingsJsonStore` and its SHA-256 revision check. Exit and cancellation never implicitly save. Bad JSON and unsupported schemas are not rewritten.

The headless `validate` command returns exit code 0 on valid settings, 1 on invalid/missing settings and 2 on malformed CLI syntax. No network call or translation service is started.

**First-slice limitations:** This is not yet the full metadata-driven provider/storage editor. Ctrl+S / Ctrl+V shortcuts, rich field-by-field redacted diff, undo/redo, multi-location discovery, live SSH resizing and localized UI are future stages; their visible menu actions remain available for saving and validation. No code is added to the UnderstandMe core for Spectre.

## Terminal display and planned preferences

Interactive startup clears the console **once**, before loading a config file and rendering the first menu. Headless `validate` output is never cleared.

**Planned, not yet implemented:** a parameterless main menu (Create / Edit / Test / Configurator Settings / Exit), per-user Configurator preferences distinct from edited settings, initially bundled manual `en` and `cs` translations with English offline fallback, and later optional language-pack downloads and version checks. See [Localization and Preferences](Docs/Localization-and-Preferences/en.md).
