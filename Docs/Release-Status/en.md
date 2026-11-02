# Toolbox package publication and release status

**Updated 2026-10-31.** The package versions below come from current project files. The maintainer has explicitly confirmed that **three packages are already published on NuGet.org**. Direct independent NuGet gallery verification of every listing was unavailable during this documentation pass; that does not negate the maintainer's publication confirmation.

| Package ID | Project version | Publication |
|---|---|---|
| `Afrowave.Toolbox.Essentials` | `0.2.0` | **Published** (maintainer-confirmed) |
| `Afrowave.Toolbox.WhenItFails` | `1.0.0` | **Published** (maintainer-confirmed) |
| `Afrowave.Toolbox.Blackboard.SpectreConsole.FileBrowser` | `0.1.0-preview.1` | **Published** (maintainer-confirmed; prior publication screenshot) |
| `Afrowave.Toolbox.ExchangeHouse.Markdown` | `0.1.0-preview.1` | Source implementation, **not reported as published** |
| `Afrowave.Toolbox.Blackboard.SpectreConsole.Markdown` | `0.1.0-preview.1` | Source implementation, **not reported as published** |

NuGet package pages:

- [Essentials 0.2.0](https://www.nuget.org/packages/Afrowave.Toolbox.Essentials/0.2.0)
- [WhenItFails 1.0.0](https://www.nuget.org/packages/Afrowave.Toolbox.WhenItFails/1.0.0)
- [FileBrowser 0.1.0-preview.1](https://www.nuget.org/packages/Afrowave.Toolbox.Blackboard.SpectreConsole.FileBrowser/0.1.0-preview.1)

## Agreed NuGet-first preview publication policy (2026-11-02)

A reusable component should become a NuGet package **as soon as it is
independently useful, documented, tested and verified installable**. We do
not wait for a stable major release: frequent SemVer prereleases
(`0.1.0-preview.1`, `0.1.0-preview.2`, ...), including dozens over the
project's lifetime, are intentional. Downstream projects consume package IDs
rather than changing source paths throughout many repositories.

This is a release **policy**, not evidence of a newly published package.
Package managers can offer prerelease updates, but users must enable
prerelease inclusion or reference a specific prerelease version. Stable-only
consumers do not necessarily see newer previews. Stable NuGet packages cannot
depend on prerelease packages. Publish each distinct package/version only
once and verify an independent restore before marking the release published.

The full checklist and dependency rules are documented in
[NuGet release policy](../NuGet-Release-Policy/en.md).

## Version is not publication

A project file's `<Version>` indicates source package metadata, **not** a public registry listing. Not every Toolbox project is a NuGet library: Showcase, Configurator, FileBrowser.Sample and Setter are executables, while test projects are not release packages.

A published `.nupkg` is immutable for a given ID and version. Source README corrections do not change the already-shipped README inside an older package: fix the next version's package content and document any difference. This is particularly relevant to the first FileBrowser preview README.

## Release evidence and checks

- Latest relevant full cross-platform functional CI: [GitHub Actions #41244480220](https://github.com/afrowaveltd/Toolbox/actions/runs/41244480220).
- CI success means the checked source compiled and its specified tests passed; it does not itself certify NuGet publication, package indexing or public-feed restore.
- For a release, verify package IDs/versions, package contents (DLL/XML docs/README/LICENSE), dependency graph, source tag, archive hashes, independent consumer restore and target OS smoke. Record the exact artifact's provenance in its component status file.
- WhenItFails has a separate [1.0 release checklist](../../WhenItFails/Docs/Release-Checklist/en.md); Essentials has [packaging notes](../../Essentials/Docs/Packaging/en.md); FileBrowser has its [publication notes](../../Blackboard/SpectreConsole/FileBrowser/README.md).

[Main documentation index](../Documentation/en.md).
