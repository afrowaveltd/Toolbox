# Markdown.Cache tests

Focused xUnit regression tests for verified content storage, corruption and failed replacements.

Run:

```bash
dotnet test Blackboard/SpectreConsole/Markdown.Cache.Tests/Afrowave.Toolbox.Blackboard.SpectreConsole.Markdown.Cache.Tests.csproj
```

The suite does not yet cover simultaneous processes, symlink attacks, disk-full conditions, abrupt process death or HTTP conditional refresh.

[Cache implementation](../Markdown.Cache/README.md).

Additional tests cover concurrent independent instances, repairing a damaged content-addressed file, Linux symlink rejection and default per-user location. These are regression *specifications* until run on the target .NET 10 SDK; total ten tests authored.

The HTTP adapter has eight additional fake-handler tests for conditional `304`, new `200`, stale offline fallback, rejected unsafe body, `404` without cache, URL restrictions, Last-Modified persistence and unexpected `304`. Total authored tests: **18**. These are not yet recorded as passed on .NET 10.

A separate `GitHubReadmeSourceTests.cs` now covers preferred README API requests and raw media type, owner/repo casing aliases, conditional cached reuse, offline fallback, missing README errors, corrupt network bodies, unsafe input and unexpected 304. Each test uses an in-memory HTTP handler (no network). **Not yet verified green in CI**.
