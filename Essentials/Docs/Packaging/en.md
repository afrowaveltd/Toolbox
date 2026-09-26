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

Both project-specific package files now use explicit project-level item
declarations. `README.md` is removed from any implicit item set and then
re-included with `Pack="true"` at the package root. The icon uses the same
`None Remove` + `None Include` pattern with a forward-slash project path. This is intentional: the Linux .NET 11
RC SDK did not expose the PNG as an updatable default `None` item during
packing, so `Update` silently left no pack item and NuGet raised `NU5046`.
Removing any implicit item first and then explicitly including the file avoids
duplicates while making the package inputs deterministic on Windows and Linux.

The shared `Directory.Build.props` now packs the repository-root README only
as a fallback for packable projects that do **not** have a project-local
`README.md`. Projects that own a README package it explicitly in their
`.csproj`; this avoids relying on `None Update` timing in
`Directory.Build.props`.

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

On 2026-09-27 the next Linux pack advanced past the icon check and then failed
with `NU5039`: declared package readme `README.md` was missing. The shared
props file had attempted to `Update` the project README before that item was
reliably available in the evaluated pack item set. The shared update was
removed; both Essentials and WhenItFails now explicitly package their own
README from the project file.
