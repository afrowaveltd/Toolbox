# Essentials implementation status

Last updated: 2026-09-27

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
  `Assets\toolbox-essentials-icon.png` include. The first Linux fix switched
  to `None Update="Assets/toolbox-essentials-icon.png"`, but a second smoke
  still produced `NU5046`, proving that the PNG was not present in the
  evaluated default `None` set during pack. The project now uses an explicit
  `None Remove` + `None Include="Assets/toolbox-essentials-icon.png"` with
  `PackagePath="assets/"`. This avoids duplicate items while making the icon
  package input deterministic across Windows and Linux.
- Added the required project-level `README.md` plus
  `Docs/Overview/en.md` and `Docs/Packaging/en.md`.
- No Essentials runtime API or behavior changed.
- The WhenItFails candidate-package smoke now explicitly opens the generated Essentials `.nupkg` and verifies `README.md`, `LICENSE.txt`, the package icon, DLL and XML documentation, so this Linux-only packaging regression is pinned by the release workflow.

## 2026-09-27 — explicit project README package item after NU5039

- The next Linux `dotnet pack Essentials/Essentials.csproj -c Release` advanced beyond the previous icon failure, then stopped with NuGet `NU5039`: `PackageReadmeFile=README.md` was declared but the file was absent from the package.
- Root cause: shared `Directory.Build.props` attempted `None Update="$(MSBuildProjectDirectory)/README.md"`; this is too early/import-order-dependent for a reliable project-local pack item.
- Shared packaging now provides only a repository-root README fallback for projects with no local README.
- `Essentials.csproj` explicitly uses `None Remove="README.md"` followed by `None Include="README.md" Pack="true" PackagePath="\\"`.
- `WhenItFails.csproj` uses the same explicit project-owned README rule.
- No Essentials runtime API or behavior changed.

## 2026-09-27 — package verification PASS

- The full WhenItFails candidate-package smoke successfully packed Essentials 0.2.0 on Linux after the icon/README fixes.
- The smoke opened the generated Essentials `.nupkg` and verified `README.md`, `LICENSE.txt`, `assets/toolbox-essentials-icon.png`, the net10.0 DLL and generated XML documentation.
- The same package was consumed transitively by a clean external WhenItFails consumer from an isolated local feed/global-packages directory.
- Packaging verification is therefore **PASS** for the current Linux source tree.

## 2026-09-27 — complete local verification PASS

- Maintainer reran `Essentials.Tests` on Linux after the packaging fixes: **PASS**, no test errors reported.
- The candidate-package smoke remains **PASS** and verifies the generated Essentials 0.2.0 package contents plus clean transitive consumption by an external WhenItFails consumer.
- Markdown-link validation for the current Toolbox documentation also completed without errors.
- No Essentials runtime API or behavior changed in this packaging/documentation hardening sequence.

## 2026-09-29 — repository line-ending policy hardened

- Root `.gitattributes` now enforces **LF** for cross-platform text in both index and working tree; only native `*.bat` / `*.cmd` scripts use CRLF.
- Common binary formats are explicitly marked binary so line-ending normalization cannot touch them.
- This removes Windows/Linux CRLF-only worktree noise from release preparation and keeps PowerShell scripts cross-platform with LF endings.
- No runtime, public API, package metadata, or frozen release artifact changed.

## 2026-09-29 — whole-Toolbox Windows regression GREEN

- Maintainer pulled current GitHub `master` on Windows and completed the whole Toolbox build/test suite: **5914/5914 GREEN**.
- Since the WhenItFails 1.0 release commit, repository changes are documentation/status-only; Essentials runtime/package source has not changed in that interval.
- This provides an additional cross-platform repository-wide GREEN checkpoint before the first GitHub Packages publication.

## 2026-09-27 — first private publication prerequisite for WhenItFails

- GitHub Packages was selected as the private NuGet registry for the Toolbox release family.
- `Afrowave.Toolbox.WhenItFails 1.0.0` declares `Afrowave.Toolbox.Essentials 0.2.0` as its minimum package dependency, so Essentials **must be published first** for a clean feed-only consumer restore to succeed.
- The retained final WhenItFails package-smoke workspace contains the exact Essentials 0.2.0 nupkg/snupkg built from release source commit `6face1742de35fbde665a9075c2e06649c171d7d`; that exact package was consumed transitively by the isolated PASS consumer.
- Next publication preparation step: capture the exact Essentials nupkg/snupkg hashes and package repository metadata from the retained workspace, freeze those artifacts, create a versioned Essentials release tag for the matching source commit, then publish Essentials 0.2.0 before WhenItFails 1.0.0.

## Next step

No additional Essentials blocker is open for the current WhenItFails release-readiness work. Continue with the WhenItFails 1.0 API/release review; the historical 0.1.0 compatibility artifact is unrelated to Essentials runtime verification.
