# Current exported API inventory and historical 0.1.0 comparison

Status: pre-1.0 first-stable surface cleanup. Complete **1474/1474 GREEN** suite confirmed after two visibility batches. The fresh post-cleanup source-only census is **126 exported types / 698 API entries** from source DLL SHA-256 `A468E68DEF8F703F452EEA97B6125797E15FEF96D1E5930422321170ECFADA7A`. Historical 0.1.0 package comparison is optional diagnostics only.

## 2026-09-27 post-cleanup 1.0 candidate census

After both intentional visibility cleanup batches, the maintainer reran
`Compare-PublicApi.ps1 -SourceOnly` against current master.

Measured current source surface:

- **126 exported types**;
- **698 comparer API entries**;
- source DLL SHA-256:
  `A468E68DEF8F703F452EEA97B6125797E15FEF96D1E5930422321170ECFADA7A`;
- complete WhenItFails suite: **1474/1474 GREEN**.

Compared with the pre-cleanup 148/830 census, the first-stable cleanup removed
**22 exported types and 132 API entries**. The complete exported-type list from this
report is now the candidate 1.0 type manifest. Remaining concrete exports were checked
against repository usage: the typed loaders, validators, catalog/document normalizers,
bootstrap/template implementations, profile resolver, writer, documentation helpers,
cross-validator and key normalizer all have intentional Toolbox Setter/release-tooling
consumers. Snapshot/observation types and DI interfaces are intentional supported
advanced/extension contracts.

The next freeze step is an exact exported-type manifest contract so later accidental
visibility changes fail tests before release.

## Two different measurements

The historical 110 exported types and 611 API entries describe the package-era checkpoint, not the current source. The maintainer's September 25, 2026 comparison now reports 148 exported source types and 830 source API entries against the package consumer's 110 exported types and 611 entries. The current inventory test inspects the compiled WhenItFails assembly with `Assembly.GetExportedTypes()`. The comparer builds two independent .NET 10 consumers, one referencing current source and the other restoring the exact package version `[0.1.0]`.

Neither a matching signature census nor the NuGet version requested in a restore independently establishes full binary, nullable, JSON or runtime compatibility. A restore through configured feeds/cache does not independently establish the package's original publication source.

## PowerShell procedure from Toolbox root

```powershell
cd D:\Toolbox
git pull --ff-only origin master

$reportDir = Join-Path $env:TEMP 'WhenItFails-api-review'
New-Item -ItemType Directory -Force -Path $reportDir | Out-Null
$env:AFROWAVE_WHENITFAILS_PUBLIC_API_REPORT = Join-Path $reportDir 'WhenItFails-current-public-api.md'
dotnet test .\WhenItFails.Tests\WhenItFails.Tests.csproj -c Release --filter 'FullyQualifiedName~ExportedAssemblyInventoryTests'
Remove-Item Env:AFROWAVE_WHENITFAILS_PUBLIC_API_REPORT -ErrorAction SilentlyContinue

dotnet test .\WhenItFails.Tests\WhenItFails.Tests.csproj -c Release

& .\Toolroom\WhenItFails\PublicApiComparer\Compare-PublicApi.ps1 -ReportPath (Join-Path $reportDir 'WhenItFails-0.1.0-vs-source.md')
```

## Source-side comparer census without the historical package

The compiled Markdown inventory and the comparer census answer different questions.
To refresh the comparer-style **source API-entry count** without the original 0.1.0
package, run:

```bash
git pull --ff-only origin master

report="${TMPDIR:-/tmp}/WhenItFails-current-source-api.md"
pwsh ./Toolroom/WhenItFails/PublicApiComparer/Compare-PublicApi.ps1 \
  -SourceOnly \
  -ReportPath "$report"

cat "$report"
```

This mode uses the same `API|...` reflection census as the source half of the full
comparison. It records the current source DLL hash, exported-type count and API-entry
count, but deliberately performs no package-side compatibility comparison.

## Linux compiled-assembly inventory procedure

The current-source inventory does **not** require the historical 0.1.0 package and can
therefore be refreshed independently on Linux:

```bash
git pull --ff-only origin master

report_dir="${TMPDIR:-/tmp}/WhenItFails-api-review"
mkdir -p "$report_dir"
export AFROWAVE_WHENITFAILS_PUBLIC_API_REPORT="$report_dir/WhenItFails-current-public-api.md"

dotnet test WhenItFails.Tests/WhenItFails.Tests.csproj -c Release \
  --filter "FullyQualifiedName~ExportedAssemblyInventoryTests"

unset AFROWAVE_WHENITFAILS_PUBLIC_API_REPORT

cat "$report_dir/WhenItFails-current-public-api.md"
```

This produces the current exported-type/member census only. That is the authoritative
measurement needed for the first stable 1.0 surface review. Historical 0.1.0 package
comparison is optional and is not required to freeze the first published release.

The inventory test writes a Markdown file only when the environment variable points to a file in an existing directory. The comparer records the actual loaded DLL paths and SHA-256 hashes, source/package exported type totals and type-delta lists, and source/package public member-entry totals and member-delta lists. Its temporary consumer directories are printed for inspection. Use `-Feed` when an explicit approved package source is required; if exact-package restore fails, record that failure rather than substituting a fresh local pack.

## 2026-09-26 Linux artifact-refresh restore result

The maintainer reran the comparer from Linux after the complete
**1473/1473 GREEN** suite. The current-source consumer restored and built, but
the exact package consumer failed during restore with `NU1101` because the
available source was `nuget.org` and
`Afrowave.Toolbox.WhenItFails [0.1.0]` was not present there.

This is an **expected local-artifact availability result**, not a library or
API compatibility failure. The 0.1.0 package was never published; the Linux
machine simply does not have the maintainer-held Windows artifact. The comparer
correctly stopped instead of substituting a freshly packed current DLL. A valid
refresh requires copying or otherwise exposing that exact 0.1.0 `.nupkg` to a
directory available on the current machine and rerunning with
`-Feed "<artifact-directory>"`.

## Reading results

For the first stable 1.0 release, review the **current source census itself**: every
remaining export should be intentional application API, DI extension point,
transitive model, snapshot/observation contract, or supported tooling utility.
Historical package-only/source-only deltas remain useful archaeology but do not
constrain the unpublished first release.

Publicly exported implementation classes remain public CLR APIs even if they are classified as provisional for 1.0. Internal `CaptureFromContext` helper methods are not exposed. A complete operational snapshot omits some raw source JSON fields by design; do not call it a complete raw-document clone.

**Observed comparison:** source **148 types / 830 entries**, package **110 types / 611 entries**, source-only **38 types / 219 entries**, package-only **0 types / 0 entries**. The requested exact version is `[0.1.0]`. This package was **never published to nuget.org or another package feed**; it is a maintainer-held local reference artifact created on Windows. The earlier successful comparison resolved that local artifact through the maintainer's configured source/cache. SHA-256 of the inspected source DLL: `E81504A484AD99D5C818C98D594CDB88A08651638984D1A92503CDC367B9F0DD`; package DLL: `379F7CF6FF99223ECF2F388AB6295A34D97F33A8BD8EB7F9A52152994347CE28`.

**Verification update:** the maintainer confirmed the complete **1473/1473 GREEN** suite and supplied a freshly generated compiled Markdown inventory from current master. That artifact reports assembly version `0.1.0.0` and **148 exported types**, matching the previous current-source type count. The new `-SourceOnly` comparer mode was then run on current master using the same reflection inspector as the source half of the full comparer and freshly measured **148 exported types / 830 API entries**, with a warning-free successful consumer build. This confirms no current source-side type or comparer-entry count drift. The package-side **110 types / 611 entries** and resulting deltas remain historical until the exact original 0.1.0 artifact is rerun through the full comparer. The three inventory and three legacy-class contract additions were already included in the earlier confirmed **1445/1445 GREEN** checkpoint; the separate focused-run count for this refresh was not supplied. A separate precompiled consumer binary smoke previously **passed**, running the same original package-built executable against the source-built DLL without recompilation. The maintainer supplied all **38 source-only type names** and a partial start of the member-entry diff. Their exhaustive group classification is in [added public types](../Added-Public-Types-Classification/en.md). A subsequent filtered diff identifies **14 entries on the two original concrete types**; the remaining **205 entries** belong to the 38 new exported types. See [original-type additions](../Original-Type-API-Additions/en.md). These entries include interface/kind declarations and are not all methods. The fresh compiled type inventory is complete; only the exact source/package comparer refresh remains pending. Do not declare 1.0 compatibility solely from the zero package-only historical census.

The 38 new exported types have now been classified by intended ownership in [the added-type classification](../Added-Public-Types-Classification/en.md). This is a source-excerpt-based review, not a 1.0 compatibility commitment.

For an additional limited compatibility check beyond the signature census, see the [precompiled NuGet 0.1.0 consumer DLL-swap smoke](../Published-Binary-Consumer-Smoke/en.md), maintainer-confirmed PASS for its targeted legacy execution path.
