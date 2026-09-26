# WhenItFails release tools

This directory contains release-candidate verification helpers for
`Afrowave.Toolbox.WhenItFails`.

## Candidate package smoke

Run from the Toolbox repository root:

```powershell
pwsh ./Toolroom/WhenItFails/Release/Test-CandidatePackage.ps1
```

The smoke builds a **new temporary candidate package** from current source, first verifies the generated Essentials dependency package (including its icon and project-specific README), then verifies the WhenItFails package contents and README, restores an isolated external consumer,
initializes a real project catalog workspace, resolves a descriptor, and exercises
explicit built-in reset/status behavior.

The default temporary package version is `1.0.0-rc.1`. The script uses the WhenItFails-specific MSBuild property
`WhenItFailsPackageVersionOverride`. In addition, the WhenItFails
`ProjectReference` removes parent version properties with
`GlobalPropertiesToRemove`, so the candidate package version cannot propagate
into the Essentials project build.
Override the candidate version without editing the project file:

```powershell
pwsh ./Toolroom/WhenItFails/Release/Test-CandidatePackage.ps1 -PackageVersion 1.0.0-rc.2
```

The smoke parses **all** Essentials dependency nodes in the generated nuspec
and requires them to remain on the Essentials 0.2.0 line. The smoke also redirects NuGet's global-packages folder to a workspace-local
directory and uses `--no-http-cache` for the external consumer restore. This
prevents an earlier candidate built with the same version from being reused
from `~/.nuget/packages` or the HTTP cache. On failure, the temporary workspace
is retained automatically for inspection.

The generated candidate is not the historical 0.1.0 compatibility baseline.
The original 0.1.0 `.nupkg` was never published and exists only as a
maintainer-held Windows artifact. Use the separate PublicApiComparer tools when that
original file is available.

See [WhenItFails 1.0 release checklist](../../../WhenItFails/Docs/Release-Checklist/en.md).
