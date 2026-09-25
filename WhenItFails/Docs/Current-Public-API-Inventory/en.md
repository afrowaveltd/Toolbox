# Current exported API inventory and NuGet 0.1.0 comparison

Status: pre-1.0 review; source/package comparer counts measured, full test run and complete reports pending.

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

The inventory test writes a Markdown file only when the environment variable points to a file in an existing directory. The comparer records the actual loaded DLL paths and SHA-256 hashes, source/package exported type totals and type-delta lists, and source/package public member-entry totals and member-delta lists. Its temporary consumer directories are printed for inspection. Use `-Feed` when an explicit approved package source is required; if exact-package restore fails, record that failure rather than substituting a fresh local pack.

## Reading results

Source-only types and members may be nonbreaking additions; package-only entries need case-by-case review for possible removals or signature changes. An empty package-only list does not establish full ABI compatibility, nullable metadata, JSON compatibility or equivalent behavior.

Publicly exported implementation classes remain public CLR APIs even if they are classified as provisional for 1.0. Internal `CaptureFromContext` helper methods are not exposed. A complete operational snapshot omits some raw source JSON fields by design; do not call it a complete raw-document clone.

**Observed comparison:** source **148 types / 830 entries**, package **110 types / 611 entries**, source-only **38 types / 219 entries**, package-only **0 types / 0 entries**. The requested exact version is `[0.1.0]`; configured package feeds/cache were used and origin was not independently established. SHA-256 of the inspected source DLL: `E81504A484AD99D5C818C98D594CDB88A08651638984D1A92503CDC367B9F0DD`; package DLL: `379F7CF6FF99223ECF2F388AB6295A34D97F33A8BD8EB7F9A52152994347CE28`.

**Verification update:** the maintainer subsequently confirmed the complete **1445/1445 GREEN** suite, including the three inventory and three legacy-class contract additions; the separate focused-run count was not reported. A precompiled consumer binary smoke is now prepared but not yet executed. The maintainer subsequently supplied all **38 source-only type names** and a partial start of the member-entry diff. Their exhaustive group classification is in [added public types](../Added-Public-Types-Classification/en.md). A subsequent filtered diff identifies **14 entries on the two original concrete types**; the remaining **205 entries** belong to the 38 new exported types. See [original-type additions](../Original-Type-API-Additions/en.md). These entries include interface/kind declarations and are not all methods. The current compiled Markdown inventory is still pending. Do not declare 1.0 compatibility solely from the zero package-only census.

The 38 new exported types have now been classified by intended ownership in [the added-type classification](../Added-Public-Types-Classification/en.md). This is a source-excerpt-based review, not a 1.0 compatibility commitment.

For an additional limited compatibility check beyond the signature census, see the [precompiled NuGet 0.1.0 consumer DLL-swap smoke](../Published-Binary-Consumer-Smoke/en.md), currently awaiting local execution.
