# Precompiled NuGet 0.1.0 consumer: source DLL substitution smoke

Status: **maintainer-confirmed PASS for this targeted precompiled-consumer DLL-swap smoke (2026-09-25)**. Last maintainer-confirmed complete library test suite: **1445/1445 GREEN**.

## What this verifies

`Toolroom/WhenItFails/PublicApiComparer/Test-PublishedConsumerBinary.ps1` creates a temporary .NET 10 application, restores the exact requested `[0.1.0]` NuGet package plus `Microsoft.Extensions.DependencyInjection` 10.0.9, and compiles the application **only once** against that package. It runs the original executable, then copies its entire output directory and substitutes only `Afrowave.Toolbox.WhenItFails.dll` with the newly compiled source DLL. The exact same `Consumer.dll`, `.deps.json`, `.runtimeconfig.json` and package dependency files are retained. It runs the same consumer executable again **without rebuilding it**.

The consumer exercises the original `ErrorCatalogContextStore` constructor and `IErrorCatalogContextStore` members (`IsInitialized`, `Current`, `GetCurrent`, `Set`) plus `AddWhenItFails()` registration, runtime resolution through the original `IErrorCatalogRuntime`, and the pre-initialization `GetCurrentContext`/`GetStatus` results. It checks the actual loaded DLL location, unchanged consumer executable SHA-256, substituted DLL SHA-256, and identical expected result lines.

## Run from Toolbox root in PowerShell

Each line can be copied and executed separately; no multiline `try/finally` paste is required.

```powershell
cd D:\Toolbox
git pull --ff-only origin master
$reportDir = Join-Path $env:TEMP 'WhenItFails-api-review'; New-Item -ItemType Directory -Force -Path $reportDir | Out-Null
& .\Toolroom\WhenItFails\PublicApiComparer\Test-PublishedConsumerBinary.ps1 -ReportPath (Join-Path $reportDir 'WhenItFails-0.1.0-binary-smoke.md')
Get-Content (Join-Path $reportDir 'WhenItFails-0.1.0-binary-smoke.md') | Select-Object -First 20
```

An optional `-Feed '<verified package source>'` overrides the NuGet restore source. Without an override, configured NuGet sources/cache may satisfy restore; successful execution **does not prove the package's original publishing origin**. If exact-package restore fails, do not build a new package and call it 0.1.0; report the failure.

## Observed maintainer execution

On 2026-09-25, the maintainer ran the PowerShell tool against their updated
Toolbox checkout and reported the following terminal result:

```text
Build succeeded.
Binary smoke: PASS (original package consumer and swapped source DLL).
```

The result confirms the executable built against requested package `[0.1.0]`
ran the exercised legacy store/DI/runtime calls with the source DLL
substituted *without recompiling that executable*. The script performs
runtime loaded-assembly, consumer executable hash and substitution-hash
checks before emitting PASS. The generated Markdown report, run-specific
DLL hashes and package-feed provenance were not provided in the
conversation; do not invent or publish those details. The script uses
configured NuGet feeds/cache unless `-Feed` is set.

## Interpretation and limitations

A PASS proves only that this **particular precompiled application** ran its exercised old API path with the source DLL under the old package-consumer dependency graph and retained identical expected behavior. It is stronger evidence for that path than the earlier reflection signature census, but it is not a complete ABI verifier or a guarantee of compatibility for unrelated consumers, all transitive dependencies, error catalogs, initialization and recovery paths, nullable metadata, JSON or runtime performance.

A FAIL is actionable: keep the exception, process exit code, hashes and original/transplanted output paths. Failures may result from assembly load, dependency binding, missing members, different pre-initialization behavior or unrelated environment issues; diagnose before concluding the whole library is incompatible.

The script creates its working directory under the current user's temporary directory and writes a Markdown report only on full success. It does **not** modify the repository, publish packages, update the original compiled consumer, or alter any user project. The script itself does not add xUnit tests: the confirmed suite remains **1445/1445 GREEN**; this separate smoke has now passed.
