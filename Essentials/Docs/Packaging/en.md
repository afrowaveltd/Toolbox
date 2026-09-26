# Essentials packaging

Essentials package metadata is defined in `Essentials.csproj` and the shared
repository defaults in `Directory.Build.props`.

## Project-specific package files

The package includes:

- `Essentials/README.md` as `README.md`;
- the repository `LICENSE.txt`;
- generated XML documentation;
- `Assets/toolbox-essentials-icon.png` as
  `assets/toolbox-essentials-icon.png`.

The icon item uses a forward-slash project path and an explicit
`None Remove` + `None Include` pair. This is intentional: the Linux .NET 11
RC SDK did not expose the PNG as an updatable default `None` item during
packing, so `Update` silently left no pack item and NuGet raised `NU5046`.
Removing any implicit item first and then explicitly including the PNG avoids
duplicates while making the package input deterministic on Windows and Linux.

## Verification

Run:

```bash
dotnet pack Essentials/Essentials.csproj -c Release
```

For the WhenItFails release workflow, the candidate-package smoke also packs
Essentials into an isolated local feed before restoring the external consumer.

The 2026-09-26 Linux candidate-package smoke exposed the previous
Windows-specific `Assets\...` include: the package icon was missing and NuGet
reported `NU5046`. The first attempted cross-platform fix used `None Update`, but a Linux rerun
still produced `NU5046`. The project now explicitly removes any implicit PNG
item and includes the icon as a pack item with `PackagePath="assets/"`.
