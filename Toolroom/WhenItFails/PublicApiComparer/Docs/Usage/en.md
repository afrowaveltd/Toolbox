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


## Optional bundled-default activation and descriptor resolution

Pass `-ExerciseInitialization` to the existing binary smoke script to compile the original package consumer **once** with a second test path before swapping its DLL. Both runs activate the isolated bundled defaults through `ResetToDefaultsAsync()`, read the non-degraded active status and resolve the historical `UNKNOWNERROR` definition by name, ID and numeric code, checking the descriptor identity and text. This mode does not create or overwrite a project-local catalog workspace; it does not test ordinary project `InitializeAsync()` or recovery. The maintainer subsequently confirmed **PASS for both original-package and swapped-source runs** in this opt-in mode; the earlier default pre-initialization smoke remains PASS as well.

```powershell
& .\Toolroom\WhenItFails\PublicApiComparer\Test-PublishedConsumerBinary.ps1 -ExerciseInitialization -ReportPath (Join-Path $env:TEMP 'WhenItFails-0.1.0-binary-initialization.md')
```

See [opt-in initialization smoke details](../../../../WhenItFails/Docs/Published-Binary-Consumer-Smoke/en.md).


## Optional isolated project catalog initialization

Use `-ExerciseProjectInitialization` to compile a disposable consumer once against package `[0.1.0]` and run that identical executable with the original package and swapped source DLL. Each run uses its **own empty temporary workspace**, calls `InitializeAsync(JsonsOptions)` twice, verifies five generated catalog files are not rewritten (SHA-256 before/after), and checks a non-degraded project activation and the `UNKNOWNERROR` descriptor by name, ID and code. This flag cannot be combined with `-ExerciseInitialization`. It does not write catalogs to the repository checkout. This extended mode was **confirmed PASS by the maintainer** for both original-package and swapped-source runs, following the earlier bundled-default PASS.

```powershell
& .\Toolroom\WhenItFails\PublicApiComparer\Test-PublishedConsumerBinary.ps1 -ExerciseProjectInitialization -ReportPath (Join-Path $env:TEMP 'WhenItFails-0.1.0-project-initialization.md')
```

See [project workspace smoke details](../../../../WhenItFails/Docs/Published-Binary-Consumer-Smoke/en.md).


## Optional malformed project catalog recovery

After the successful isolated project initialization check, use both `-ExerciseProjectInitialization` and `-ExerciseProjectRecovery`. The unchanged precompiled 0.1.0 consumer writes intentionally invalid JSON **only** to its disposable temporary error catalog, runs `InitializeAsync(JsonsOptions)` again, and verifies the previous valid context, degraded `PreviousContextRecovery` status, descriptor lookup, and unchanged hashes of all five project files (including the malformed one). The original package run and source DLL substitution run use distinct empty temp roots. **Maintainer-confirmed PASS** for original-package and source-DLL-substituted runs; this does not imply a broader ABI guarantee.

```powershell
& .\Toolroom\WhenItFails\PublicApiComparer\Test-PublishedConsumerBinary.ps1 -ExerciseProjectInitialization -ExerciseProjectRecovery -ReportPath (Join-Path $env:TEMP 'WhenItFails-0.1.0-project-recovery.md')
```

See [previous-context recovery smoke](../../../../WhenItFails/Docs/Published-Binary-Consumer-Smoke/en.md).


## Optional first-start built-in fallback from malformed project JSON

Use the separate `-ExerciseFirstStartFallback` switch. The unchanged consumer, compiled once against NuGet `[0.1.0]`, starts with no active context. In each of two separate private temp workspaces it creates an invalid `errors.en.json` **before its first** `InitializeAsync(JsonsOptions)`, checks the degraded `BuiltInFallback` state and the unchanged malformed file hash, then resolves `UNKNOWNERROR` by name, ID and code. The first execution uses the old package DLL; the second swaps only WhenItFails.dll for the new source build. Do not combine this switch with other optional probe flags. **Local verification pending**; earlier fallback-from-project-failure with a retained previous context is a different, already passing scenario.

```powershell
& .\Toolroom\WhenItFails\PublicApiComparer\Test-PublishedConsumerBinary.ps1 -ExerciseFirstStartFallback -ReportPath (Join-Path $env:TEMP 'WhenItFails-0.1.0-first-start-fallback.md')
```

See [first-start fallback smoke details](../../../../WhenItFails/Docs/Published-Binary-Consumer-Smoke/en.md).
