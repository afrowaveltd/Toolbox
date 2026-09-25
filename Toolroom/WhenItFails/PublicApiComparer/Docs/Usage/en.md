# Public API comparer — usage

From Toolbox root in PowerShell:

```powershell
git pull --ff-only origin master
& .\Toolroom\WhenItFails\PublicApiComparer\Compare-PublicApi.ps1 -ReportPath (Join-Path $env:TEMP "WhenItFails-published-vs-source-api.md")
```

The script creates isolated temporary .NET 10 consumer projects. One uses a ProjectReference to the current source and the other pins PackageReference to the exact NuGet version `[0.1.0]`. Both are compiled and executed in separate processes; the report records the **actual loaded** DLL paths, SHA-256 digests and public signature differences.

Use `-Feed "<real published feed URL or directory>"` to force the published consumer to restore from the specified source. Without it, configured sources/cache may satisfy restore, and package provenance requires separate verification. The maintainer successfully restored exact [0.1.0] through configured sources/cache, but its original publication on nuget.org has **not** been independently established by this checkpoint. An **earlier checkpoint** reported 611 source and 611 package-consumer API entries with no reflected signature difference; both DLL hashes differed. Those figures are historical, not measurements of the current source. If restore fails, report that failure rather than substituting a fresh local `dotnet pack` output.

The updated report also contains source/package exported-type totals, package-only/source-only type lists and member-entry totals/differences. Package-only entries merit individual compatibility review; source-only additions are not automatically breaking. Actual current counts remain pending until local execution.

The temporary consumers remain under the printed temp directory for inspection. The script does not alter the repository's production code or package version. Published-only members are candidates for a compatibility review; source-only additions are not automatically breaking. This is a reflection signature census rather than a full ABI, nullable-reference, JSON or behavioral compatibility test.

The initial comparison belonged to the historical **1241/1241 GREEN** checkpoint. The updated type-aware comparer awaits local execution after the maintainer-confirmed **1439/1439 GREEN** checkpoint; three additional inventory tests are pending. Configured sources/cache do not independently establish the package's original publication provenance.
