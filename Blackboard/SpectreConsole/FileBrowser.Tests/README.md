# Blackboard SpectreConsole FileBrowser Tests

This .NET 10 xUnit project tests the independently packaged FileBrowser library, currently covering file-type identification, per-instance extension/file-name overrides, accessible labels and icon rendering across terminal capability modes.

Run these tests with:

```bash
dotnet test Blackboard/SpectreConsole/FileBrowser.Tests/Afrowave.Toolbox.Blackboard.SpectreConsole.FileBrowser.Tests.csproj
```

The test project is part of `Toolbox.sln`. It does not require a running FTP service, graphical display or Spectre.Console input terminal. Future tests will cover the actual interactive picker, local filesystem navigation and remote extensions as they are implemented.

See [Implementation Status](IMPLEMENTATION_STATUS.md) and [File-type Icons](../FileBrowser/Docs/File-Type-Icons/en.md).

## New navigation presentation tests

`FileBrowserBrowsingPresentationTests` adds coverage for known local folders on Linux, Windows (including a non-C: drive), and macOS, unknown lookalikes, special icon fallbacks, per-type/extension JSON filtering, directory preservation, exact one-space file indentation and escaped terminal control characters. No external filesystem, terminal emulator or FTP server is needed.

## Recursive search and tree tests

`FileBrowserTreeSearchTests` adds verification for lazy folder expansion, multiple identical-named files in separate branches, path-relative result rows and pruned trees, wildcard case sensitivity, explicit result selection, unchanged navigation on miss/cancel, search safety budgets, permission-error reporting, source-qualified identities, cycle avoidance, and local filesystem browsing without working-directory mutation. See [Tree and Recursive Search](../FileBrowser/Docs/Tree-and-Recursive-Search/en.md).
