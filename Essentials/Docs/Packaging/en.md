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

The icon item uses a forward-slash project path and MSBuild `None Update`
metadata. This is intentional: the SDK already discovers the PNG as a default
`None` item, and using `Update` avoids duplicate-item behavior while keeping
the path valid on both Windows and Linux.

## Verification

Run:

```bash
dotnet pack Essentials/Essentials.csproj -c Release
```

For the WhenItFails release workflow, the candidate-package smoke also packs
Essentials into an isolated local feed before restoring the external consumer.

The 2026-09-26 Linux candidate-package smoke exposed the previous
Windows-specific `Assets\...` include: the package icon was missing and NuGet
reported `NU5046`. The project now uses the cross-platform path above.
