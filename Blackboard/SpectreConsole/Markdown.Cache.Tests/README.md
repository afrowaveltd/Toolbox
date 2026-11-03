# Markdown.Cache tests

Focused xUnit regression tests for verified content storage, corruption and failed replacements.

Run:

```bash
dotnet test Blackboard/SpectreConsole/Markdown.Cache.Tests/Afrowave.Toolbox.Blackboard.SpectreConsole.Markdown.Cache.Tests.csproj
```

The suite does not yet cover simultaneous processes, symlink attacks, disk-full conditions, abrupt process death or HTTP conditional refresh.

[Cache implementation](../Markdown.Cache/README.md).

Additional tests cover concurrent independent instances, repairing a damaged content-addressed file, Linux symlink rejection and default per-user location. These are regression *specifications* until run on the target .NET 10 SDK; total ten tests authored.
