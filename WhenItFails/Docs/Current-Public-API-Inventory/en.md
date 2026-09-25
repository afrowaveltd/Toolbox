# Current exported API inventory and NuGet 0.1.0 comparison

Status: pre-1.0 review; local report generation and current counts pending.

## Two different measurements

The older 110 exported types and 611 package/source API entries are historical checkpoints, not a count of the current source. The current inventory test inspects the compiled WhenItFails assembly with `Assembly.GetExportedTypes()`. The comparer builds two independent .NET 10 consumers, one referencing current source and the other restoring the exact package version `[0.1.0]`.

Neither a matching signature census nor the NuGet version requested in a restore independently establishes full binary, nullable, JSON or runtime compatibility. A restore through configured feeds/cache does not independently establish the package's original publication source.

## PowerShell procedure from Toolbox root

```powershell
cd D:\Toolbox
git pull --ff-only origin master

$reportDir = Join-Path $env:TEMP 'WhenItFails-api-review'
New-Item -ItemType Directory -Force -Path $reportDir | Out-Null
$env:AFROWAVE_WHENITFAILS_PUBLIC_API_REPORT = Join-Path $reportDir 'WhenItFails-current-public-api.md'
try {
    dotnet test .\WhenItFails.Tests\WhenItFails.Tests.csproj -c Release --filter 'FullyQualifiedName~ExportedAssemblyInventoryTests'
}
finally {
    Remove-Item Env:AFROWAVE_WHENITFAILS_PUBLIC_API_REPORT -ErrorAction SilentlyContinue
}

dotnet test .\WhenItFails.Tests\WhenItFails.Tests.csproj -c Release

& .\Toolroom\WhenItFails\PublicApiComparer\Compare-PublicApi.ps1 -ReportPath (Join-Path $reportDir 'WhenItFails-0.1.0-vs-source.md')
```

The inventory test writes a Markdown file only when the environment variable points to a file in an existing directory. The comparer records the actual loaded DLL paths and SHA-256 hashes, source/package exported type totals and type-delta lists, and source/package public member-entry totals and member-delta lists. Its temporary consumer directories are printed for inspection. Use `-Feed` when an explicit approved package source is required; if exact-package restore fails, record that failure rather than substituting a fresh local pack.

## Reading results

Source-only types and members may be nonbreaking additions; package-only entries need case-by-case review for possible removals or signature changes. An empty package-only list does not establish full ABI compatibility, nullable metadata, JSON compatibility or equivalent behavior.

Publicly exported implementation classes remain public CLR APIs even if they are classified as provisional for 1.0. Internal `CaptureFromContext` helper methods are not exposed. A complete operational snapshot omits some raw source JSON fields by design; do not call it a complete raw-document clone.

Three new inventory contract cases await local verification: four focused inventory tests and expected full **1442/1442 GREEN**. Last maintainer-confirmed full suite: **1439/1439 GREEN**. Record actual counts and comparison deltas with the source commit and package provenance when available before declaring any 1.0 compatibility guarantee.
