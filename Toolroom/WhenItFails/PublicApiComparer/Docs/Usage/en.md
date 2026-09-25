# Public API comparer — usage

From Toolbox root in PowerShell:

```powershell
git pull --ff-only origin master
& .\Toolroom\WhenItFails\PublicApiComparer\Compare-PublicApi.ps1 -ReportPath (Join-Path $env:TEMP "WhenItFails-published-vs-source-api.md")
```

The script creates isolated temporary .NET 10 consumer projects. One uses a ProjectReference to the current source and the other pins PackageReference to the exact NuGet version `[0.1.0]`. Both are compiled and executed in separate processes; the report records the **actual loaded** DLL paths, SHA-256 digests and public signature differences.

Use `-Feed "<real published feed URL or directory>"` to force the published consumer to restore from the specified source. Without it, configured sources/cache may satisfy restore, and package provenance requires separate verification. The maintainer successfully restored exact [0.1.0] through configured sources/cache, but its original publication on nuget.org has **not** been independently established by this checkpoint. An **earlier checkpoint** reported 611 source and 611 package-consumer API entries with no reflected signature difference; both DLL hashes differed. Those figures are historical, not measurements of the current source. If restore fails, report that failure rather than substituting a fresh local `dotnet pack` output.

The updated report also contains source/package exported-type totals, package-only/source-only type lists and member-entry totals/differences. Package-only entries merit individual compatibility review; source-only additions are not automatically breaking. A maintainer-run current comparison now reports **148 source / 110 package exported types**, **830 source / 611 package API entries**, **38 source-only / 0 package-only types**, and **219 source-only / 0 package-only entries**; the complete individual entries have not been supplied.

The temporary consumers remain under the printed temp directory for inspection. The script does not alter the repository's production code or package version. Published-only members are candidates for a compatibility review; source-only additions are not automatically breaking. This is a reflection signature census rather than a full ABI, nullable-reference, JSON or behavioral compatibility test.

The initial comparison belonged to the historical **1241/1241 GREEN** checkpoint. The updated type-aware comparer was executed after the maintainer-confirmed **1439/1439 GREEN** checkpoint; the source and package DLL hashes are recorded in the current inventory documentation. Three additional inventory tests and the expected **1442/1442 GREEN** full-suite result have not yet been explicitly confirmed. Configured sources/cache do not independently establish the package's original publication provenance.


## Running an unchanged 0.1.0 consumer with the source-built DLL

The separate `Test-PublishedConsumerBinary.ps1` script builds a .NET 10 consumer against exact requested NuGet `[0.1.0]`, executes it with the original package DLL, copies its output, replaces only WhenItFails.dll in the copy, and executes the **same consumer binary without rebuilding**. It verifies the actual loaded assembly path, unchanged executable SHA-256, source DLL hash and the original store/DI/runtime pre-initialization contract results. Execute from Toolbox root as a single PowerShell command:

```powershell
& .\Toolroom\WhenItFails\PublicApiComparer\Test-PublishedConsumerBinary.ps1 -ReportPath (Join-Path $env:TEMP 'WhenItFails-0.1.0-binary-smoke.md')
```

The maintainer reported **PASS on 2026-09-25** for the package-consumer execution and the same executable after source DLL substitution; the script adds no xUnit tests. The observed PASS is evidence only for the exercised old consumer and its retained dependency graph, not a full ABI/behavior guarantee. Use `-Feed` to select a specific package source, otherwise configured sources/cache are used. See [binary smoke scope and steps](../../../../WhenItFails/Docs/Published-Binary-Consumer-Smoke/en.md).
