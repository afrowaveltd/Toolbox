# Development, documentation and verification

## Source of truth and atomic progress

Work directly with [afrowaveltd/Toolbox](https://github.com/afrowaveltd/Toolbox). Commit each self-contained implementation and its tests/docs; keep each project `IMPLEMENTATION_STATUS.md` current so another contributor can resume without reconstructing the conversation. GitHub workflow results are verification evidence only for the exact commit/run they checked.

## Documentation conventions

- Root `README.md`: short English family entry point and basic usage.
- `Docs/<Topic>/en.md`: detailed English topic documentation; translated variants will be generated later.
- `IMPLEMENTATION_STATUS.md`: tested behavior, commit, blockers, remaining work and latest CI evidence.
- Never copy planned architecture into a current API reference as if the code already exists.
- Prefer repository-relative Markdown links and check them when adding or moving documentation.
- Small code paths should have appropriate XML API comments; documentation should not drift from current public members.

## Recommended validation

```bash
dotnet restore Toolbox.sln
dotnet build Toolbox.sln -c Release
dotnet test Toolbox.sln -c Release
```

For parser/UI changes, additionally run the focused Markdown, Showcase, Configurator and FileBrowser test projects. Windows and Linux terminals have different path-wrapping, Unicode and ANSI behavior; targeted Windows CI supplements the Linux solution job.

```bash
dotnet test ExchangeHouse/Markdown.Tests/Afrowave.Toolbox.ExchangeHouse.Markdown.Tests.csproj -c Release
dotnet test Blackboard/SpectreConsole/Markdown.Tests/Afrowave.Toolbox.Blackboard.SpectreConsole.Markdown.Tests.csproj -c Release
dotnet test UnderstandMe/Tools/Configurator.Tests/UnderstandMe.Configurator.Tests.csproj -c Release
```

Check [GitHub Actions](https://github.com/afrowaveltd/Toolbox/actions) for real results. A checked build is not necessarily a package publication, and virtual-console tests do not substitute for manual Windows/Debian or SSH/tmux smoke checks where relevant.

## Dependencies and compatibility

.NET 10 is the current primary framework. Keep runtime-facing libraries independent of demos and consumers; avoid forcing Spectre.Console into UI-neutral format/parsing components. Verify reverse dependencies before changing public APIs or document models. Keep tested, stable public packages separate from experimental modules.

## Continuation guide

Start with the [root status](../../IMPLEMENTATION_STATUS.md), then the owning project's status, README and topic docs; inspect source/tests and latest CI before implementing the next step. If a project has no status file, add one based on actual source inspection and record what remains unverified.

[Architecture](../Architecture/en.md) · [Release status](../Release-Status/en.md).

## Repository-wide local Markdown link checks

Run `python3 Toolroom/Documentation/Check-Markdown-Links.py` for a quick relative file-path audit across the repository's Markdown docs. It checks file targets rather than external sites or heading anchors. A dedicated CI job runs this checker on push/PR. See [usage](../../Toolroom/Documentation/Docs/Usage/en.md).
