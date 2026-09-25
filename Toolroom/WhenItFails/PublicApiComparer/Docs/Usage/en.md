# Public API comparer — usage

From Toolbox root in PowerShell:

```powershell
git pull --ff-only origin master
& .\Toolroom\WhenItFails\PublicApiComparer\Compare-PublicApi.ps1 -ReportPath (Join-Path $env:TEMP "WhenItFails-published-vs-source-api.md")
```

The script creates isolated temporary .NET 10 consumer projects. One uses a ProjectReference to the current source and the other pins PackageReference to the exact NuGet version `[0.1.0]`. Both are compiled and executed in separate processes; the report records the **actual loaded** DLL paths, SHA-256 digests and public signature differences.

Use `-Feed "<real published feed URL or directory>"` to force the published consumer to restore from the specified source. Without it, configured sources/cache may satisfy restore, and package provenance requires separate verification. The package's availability on nuget.org has **not** been established by this checkpoint. If restore fails, report that failure rather than substituting a fresh local `dotnet pack` output.

The temporary consumers remain under the printed temp directory for inspection. The script does not alter the repository's production code or package version. Published-only members are candidates for a compatibility review; source-only additions are not automatically breaking. This is a reflection signature census rather than a full ABI, nullable-reference, JSON or behavioral compatibility test.

Last maintainer-confirmed full WhenItFails test suite before this script: **1241/1241 GREEN, zero warnings**. The script has not yet been run against the actual published package.
