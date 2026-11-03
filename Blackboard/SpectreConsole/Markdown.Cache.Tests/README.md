# Markdown.Cache tests

Focused xUnit regression tests for verified content storage, corruption and failed replacements.

Run:

```bash
dotnet test Blackboard/SpectreConsole/Markdown.Cache.Tests/Afrowave.Toolbox.Blackboard.SpectreConsole.Markdown.Cache.Tests.csproj
```

The suite does not yet cover simultaneous processes, symlink attacks, disk-full conditions, abrupt process death or HTTP conditional refresh.

[Cache implementation](../Markdown.Cache/README.md).
