# WhenItFails release tools

This directory contains release-candidate verification helpers for
`Afrowave.Toolbox.WhenItFails`.

## Candidate package smoke

Run from the Toolbox repository root:

```powershell
pwsh ./Toolroom/WhenItFails/Release/Test-CandidatePackage.ps1
```

The smoke builds a **new temporary candidate package** from current source, verifies
its contents and project-specific README, restores an isolated external consumer,
initializes a real project catalog workspace, resolves a descriptor, and exercises
explicit built-in reset/status behavior.

The default temporary package version is `1.0.0-rc.1`. Override it without editing
the project file:

```powershell
pwsh ./Toolroom/WhenItFails/Release/Test-CandidatePackage.ps1 -PackageVersion 1.0.0-rc.2
```

The generated candidate is not the historical 0.1.0 compatibility baseline.
The original 0.1.0 `.nupkg` was never published and exists only as a
maintainer-held Windows artifact. Use the separate PublicApiComparer tools when that
original file is available.

See [WhenItFails 1.0 release checklist](../../../WhenItFails/Docs/Release-Checklist/en.md).
