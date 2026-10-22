# Blackboard SpectreConsole FileBrowser Tests

This .NET 10 xUnit project tests the independently packaged FileBrowser library, currently covering file-type identification, per-instance extension/file-name overrides, accessible labels and icon rendering across terminal capability modes.

Run these tests with:

```bash
dotnet test Blackboard/SpectreConsole/FileBrowser.Tests/Afrowave.Toolbox.Blackboard.SpectreConsole.FileBrowser.Tests.csproj
```

The test project is part of `Toolbox.sln`. It does not require a running FTP service, graphical display or Spectre.Console input terminal. Future tests will cover the actual interactive picker, local filesystem navigation and remote extensions as they are implemented.

See [Implementation Status](IMPLEMENTATION_STATUS.md) and [File-type Icons](../FileBrowser/Docs/File-Type-Icons/en.md).
