# Essentials implementation status

Last updated: 2026-09-26

## Current scope

- Target framework: `net10.0`.
- Package ID: `Afrowave.Toolbox.Essentials`.
- Current project version metadata: `0.2.0`.
- Core areas include Results/Responses, issues, diagnostics, metadata, guards,
  interfaces, extensions, enums and value objects.
- Tests live in `Essentials.Tests`.

## 2026-09-26 — cross-platform package icon and project documentation

- A Linux release-candidate smoke exposed a packaging-only failure:
  `NU5046` reported that `assets/toolbox-essentials-icon.png` was missing.
- The source icon exists at
  `Essentials/Assets/toolbox-essentials-icon.png`.
- `Essentials.csproj` previously used the Windows-specific
  `Assets\toolbox-essentials-icon.png` include. The item now uses
  `None Update="Assets/toolbox-essentials-icon.png"` and
  `PackagePath="assets/"`, preserving the declared
  `PackageIcon=assets/toolbox-essentials-icon.png` path on Windows and Linux.
- Added the required project-level `README.md` plus
  `Docs/Overview/en.md` and `Docs/Packaging/en.md`.
- No Essentials runtime API or behavior changed.
- The WhenItFails candidate-package smoke now explicitly opens the generated Essentials `.nupkg` and verifies `README.md`, `LICENSE.txt`, the package icon, DLL and XML documentation, so this Linux-only packaging regression is pinned by the release workflow.

## Verification pending

Run:

```bash
dotnet test Essentials.Tests/Essentials.Tests.csproj -c Release
dotnet pack Essentials/Essentials.csproj -c Release
```

The broader WhenItFails candidate-package smoke should then be rerun because it
packs Essentials as the first dependency artifact.
