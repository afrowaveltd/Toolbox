# Configurator Tests

Offline xUnit and Spectre.Console.Testing coverage for UnderstandMe.Configurator (net10.0). Tests cover unsaved draft behavior, no filesystem writes until explicit save, shared validation, stale disk revision, invalid source recovery, preserving storage/provider registrations and virtual-terminal cancellation.

Run `dotnet test UnderstandMe/Tools/Configurator.Tests/UnderstandMe.Configurator.Tests.csproj`.

## 2026-10-25 additional coverage

The suite now also verifies parameterless Create/Edit/Test/Preferences navigation via Spectre.Console.Testing, bundled English/Czech key parity, language selection surviving restarts, corrupt app preference fallback, revision-aware own-settings writes, and offline validation without rewriting a selected host configuration. [CI #40231883535](https://github.com/afrowaveltd/Toolbox/actions/runs/40231883535) passed **14 Configurator tests**.
