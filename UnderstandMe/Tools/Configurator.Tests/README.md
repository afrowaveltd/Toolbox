# Configurator Tests

Offline xUnit and Spectre.Console.Testing coverage for UnderstandMe.Configurator (net10.0). Tests cover unsaved draft behavior, no filesystem writes until explicit save, shared validation, stale disk revision, invalid source recovery, preserving storage/provider registrations and virtual-terminal cancellation.

Run `dotnet test UnderstandMe/Tools/Configurator.Tests/UnderstandMe.Configurator.Tests.csproj`.

## 2026-10-25 additional coverage

The suite now also verifies parameterless Create/Edit/Test/Preferences navigation via Spectre.Console.Testing, bundled English/Czech key parity, language selection surviving restarts, corrupt app preference fallback, revision-aware own-settings writes, and offline validation without rewriting a selected host configuration. [CI #40231883535](https://github.com/afrowaveltd/Toolbox/actions/runs/40231883535) passed **14 Configurator tests**.

## Component and connectivity regressions (2026-10-25)

The tests also cover descriptor-backed LibreTranslate/FileJson registration/option edits, safe redaction of API key references in design, no implicit writes, revision conflicts, live provider probes with injected in-memory HTTP handlers, JSON/HTTP failure cases and an explicitly selected online test from the TUI. No external translation endpoint is contacted by the test suite.

`dotnet test UnderstandMe/Tools/Configurator.Tests/UnderstandMe.Configurator.Tests.csproj`
