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

## Optional bundled-default initialization and descriptor compatibility probe

The same script now accepts `-ExerciseInitialization`. This opt-in mode
injects additional code into the **original 0.1.0 consumer source before its
single compilation**, preserving the previous default smoke behavior.
Both executions of that same compiled executable then exercise:

- `IErrorCatalogRuntime.ResetToDefaultsAsync()` to activate validated,
  isolated bundled defaults without creating or overwriting project-managed
  `Jsons/WhenItFails` files;
- successful `GetCurrentContext()` and `GetStatus()` with
  `BuiltInDefaults` and no degraded recovery state;
- `FromName("UNKNOWNERROR")`, `FromId("AFW_GEN_0001")` and
  `FromCode(100001)`, comparing the known bundled descriptor's ID, name,
  numeric code, title and message from the historical 0.1.0 template;
- an unchanged consumer executable and original dependency graph, with
  only WhenItFails.dll substituted in the second run.

From the Toolbox root, each PowerShell command below is a single line:

```powershell
git pull --ff-only origin master
& .\Toolroom\WhenItFails\PublicApiComparer\Test-PublishedConsumerBinary.ps1 -ExerciseInitialization -ReportPath (Join-Path $env:TEMP 'WhenItFails-0.1.0-binary-initialization.md')
```

The intended success marker is
`Binary initialization smoke: PASS (original package consumer and swapped source DLL).`
The Markdown report records whether this optional probe was enabled,
both expected result markers and the actual executable/source/package hashes.
**Confirmed by maintainer:** the opt-in mode passed with the original
package consumer and its unchanged executable after DLL substitution.
The reported result in both runs was
`RESULT|PASS|STORE|DI|UNINITIALIZED_RUNTIME|BUILTIN_DEFAULTS|DESCRIPTOR`.
The unchanged consumer SHA-256 was
`586F81041EA77AE853CB198784D753B991FA3980C1DCF51A95B4F850DD02022F`;
the original package DLL SHA-256 was
`379F7CF6FF99223ECF2F388AB6295A34D97F33A8BD8EB7F9A52152994347CE28`
and the swapped source DLL SHA-256 was
`C30205E4D42FB63EAB540063F8FF4BCD138A2A602F4A0942509C26B8FF4004F2`.
NuGet sources/cache satisfied the exact `[0.1.0]` request; original
publishing provenance has not been independently verified.

This opt-in probe deliberately avoids default project-workspace
`InitializeAsync()`, automatic recovery, user-managed JSON writes and
full descriptor serialization. Those paths need separate isolated,
precompiled-consumer tests before claiming compatibility for them.

## Optional isolated project-workspace initialization probe (PASS confirmed)

The existing precompiled consumer script also accepts
`-ExerciseProjectInitialization`; this flag is mutually exclusive
with `-ExerciseInitialization`. The original consumer is compiled
**once against the requested exact NuGet [0.1.0]**, runs against the
package DLL, and is then run **without recompilation** against the
source-built replacement. Each execution receives a **different,
previously empty, script-generated temporary directory** as the
project `JsonsOptions.RootDirectory`. The tool does not initialize
catalogs in the source checkout or overwrite user project files.

The original `IErrorCatalogRuntime.InitializeAsync(JsonsOptions)`
method creates and activates the project JSON catalogs; the probe
checks non-degraded `ProjectCatalog` status, all five expected files,
and the original `UNKNOWNERROR` descriptor resolved by name, ID
and numeric code. Calling the same initialization again must succeed
**without rewriting existing catalog files**: SHA-256 checks cover
all five files before and after the second initialization. The
consumer executable and its `.deps.json` and dependency copies
remain unchanged across the original and substituted DLL runs;
the script checks actual loaded assembly identity and hashes.

From the Toolbox root, execute these two **single-line** commands:

```powershell
git pull --ff-only origin master
& .\Toolroom\WhenItFails\PublicApiComparer\Test-PublishedConsumerBinary.ps1 -ExerciseProjectInitialization -ReportPath (Join-Path $env:TEMP 'WhenItFails-0.1.0-project-initialization.md')
```

Expected on success:
`Binary project initialization smoke: PASS (original package consumer and swapped source DLL).`
**Confirmed by maintainer:** the original package consumer and the unchanged
consumer with the source DLL both returned PASS. The script verified project
initialization, existing-file hashes and descriptor lookups in two isolated
workspaces. Recovery after malformed JSON is tested separately below; neither
scenario proves full ABI, cancellation or wire-format compatibility.

## Optional malformed-project previous-context recovery (PASS confirmed)

`-ExerciseProjectRecovery` extends `-ExerciseProjectInitialization`
and **must be used together with it**. The same precompiled consumer first
loads a valid project catalog into an isolated temporary workspace and
checks all five files and descriptor lookups. It then deliberately replaces
only its disposable `errors.en.json` with invalid JSON and repeats
`InitializeAsync(JsonsOptions)`. Flexible initialization must retain the
**same previously active context reference**, report a degraded
`PreviousContextRecovery` state, continue resolving the original
`UNKNOWNERROR` descriptor, and **not modify or repair** any of the five
project files. File content is checked using SHA-256 both before and
after attempted recovery, including the malformed error catalog.

As with the earlier modes, the application is compiled **once against
requested NuGet [0.1.0]** and run without rebuilding with the package and
the source DLL, in two independent temporary workspaces. No malformed
file is written to the source checkout or a user's real project catalogs.

Run each command separately from Toolbox root:

```powershell
git pull --ff-only origin master
& .\Toolroom\WhenItFails\PublicApiComparer\Test-PublishedConsumerBinary.ps1 -ExerciseProjectInitialization -ExerciseProjectRecovery -ReportPath (Join-Path $env:TEMP 'WhenItFails-0.1.0-project-recovery.md')
```

Expected on success:
`Binary project recovery smoke: PASS (original package consumer and swapped source DLL).`
**Maintainer-confirmed PASS:** both package and swapped-source runs completed the recovery checks. This probe tests previous-context recovery
after a previously valid project activation, not first-start built-in
fallback, strict-mode failures, concurrent mutation or arbitrary malformed
catalog content. No additional xUnit cases or production changes are
included in this checkpoint.

## Optional first-start malformed-project built-in fallback (pending)

`-ExerciseFirstStartFallback` is a separate, mutually exclusive mode.
In each of the two isolated temporary workspaces, the disposable
**original 0.1.0 consumer executable** begins with an uninitialized
runtime, writes invalid JSON to its temporary `errors.en.json` and
calls `InitializeAsync(JsonsOptions)` for the first time. Because no
previously valid context exists, Flexible initialization should
activate the bundled defaults rather than retain a previous context.
The test checks the degraded `BuiltInFallback` state, `UsedFallback`,
a usable active context, and `UNKNOWNERROR` resolution through
`FromName`, `FromId`, and `FromCode`. The original malformed
user-managed file must remain byte-for-byte unchanged (content and
SHA-256) after the fallback.

The consumer is compiled once against requested package `[0.1.0]`,
then run with the package DLL and again **without recompilation**
with only the source-built WhenItFails.dll substituted. Each run
gets a different new temporary root; no existing project files in
the repository or user's workspace are touched. The script retains
its executable/DLL hash and actual loaded-assembly checks.

Run each PowerShell command on a separate line:

```powershell
git pull --ff-only origin master
& .\Toolroom\WhenItFails\PublicApiComparer\Test-PublishedConsumerBinary.ps1 -ExerciseFirstStartFallback -ReportPath (Join-Path $env:TEMP 'WhenItFails-0.1.0-first-start-fallback.md')
```

Expected marker on successful local verification:
`Binary first-start fallback smoke: PASS (original package consumer and swapped source DLL).`

**Maintainer-confirmed PASS.** Both runs completed the first-start fallback checks. This scenario
does not test strict-mode rejection, cancellation, validation of
arbitrary malformed catalog types or exhaustive binary compatibility.

## Optional strict first-start malformed-project rejection (PASS confirmed)

The separate `-ExerciseStrictFirstStart` mode configures the original
consumer with `WhenItFailsOptions.InitializationMode = Strict` **before
its one-and-only compilation against requested package [0.1.0]**.
The test runs that unchanged binary first against the package DLL and
then against the source-built WhenItFails DLL swapped into a copy of
the original output. Each run receives a distinct absent temporary root.

The consumer places intentionally malformed `errors.en.json` in its
temporary project catalog directory **before first initialization**.
Unlike Flexible mode, Strict must reject the invalid catalog without
publishing a context, activating bundled defaults, recording an active
status or exposing the bundled `UNKNOWNERROR` descriptor through
`FromName`, `FromId` or `FromCode`. The invalid JSON must remain
unchanged in content and SHA-256. The script retains its checks of
the loaded DLL path, executable hash, source DLL hash and result parity.

Run one command per PowerShell paste from Toolbox root:

```powershell
git pull --ff-only origin master
& .\Toolroom\WhenItFails\PublicApiComparer\Test-PublishedConsumerBinary.ps1 -ExerciseStrictFirstStart -ReportPath (Join-Path $env:TEMP 'WhenItFails-0.1.0-strict-first-start.md')
```

Expected final message:
`Binary strict first-start smoke: PASS (original package consumer and swapped source DLL).`

**Maintainer-confirmed PASS:** the original-package and substituted-source-DLL executions both completed strict first-start checks. All six current smoke modes have confirmed PASS.
This path only checks Strict first-start rejection; retaining a previously
valid context after a strict reinitialization failure is a separate test.
The experiment does not establish complete binary, behavioral, nullable
or JSON compatibility.

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
