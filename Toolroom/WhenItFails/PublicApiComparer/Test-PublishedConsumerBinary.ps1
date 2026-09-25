[CmdletBinding()]
param(
    [string]$Feed,
    [switch]$ExerciseInitialization,
    [switch]$ExerciseProjectInitialization,
    [switch]$ExerciseProjectRecovery,
    [switch]$ExerciseFirstStartFallback,
    [string]$ReportPath = (Join-Path ([IO.Path]::GetTempPath()) 'WhenItFails-0.1.0-binary-smoke.md')
)

Set-StrictMode -Version Latest
$ErrorActionPreference = 'Stop'

$root = (Resolve-Path (Join-Path $PSScriptRoot '../../..')).Path
$whenItFailsProject = (Resolve-Path (Join-Path $root 'WhenItFails/WhenItFails.csproj')).Path
$workspace = Join-Path ([IO.Path]::GetTempPath()) ('WIF-BinarySmoke-' + [guid]::NewGuid().ToString('N'))
$consumerDir = Join-Path $workspace 'PublishedConsumer'
$swappedDir = Join-Path $workspace 'SwappedConsumer'
New-Item -ItemType Directory -Force -Path $consumerDir, $swappedDir | Out-Null

$projectXml = @'
<Project Sdk="Microsoft.NET.Sdk">
  <PropertyGroup>
    <OutputType>Exe</OutputType>
    <TargetFramework>net10.0</TargetFramework>
    <ImplicitUsings>enable</ImplicitUsings>
    <Nullable>enable</Nullable>
  </PropertyGroup>
  <ItemGroup>
    <PackageReference Include="Afrowave.Toolbox.WhenItFails" Version="[0.1.0]" />
    <PackageReference Include="Microsoft.Extensions.DependencyInjection" Version="10.0.9" />
  </ItemGroup>
</Project>
'@

$consumerSource = @'
using Afrowave.Toolbox.WhenItFails.Catalog;
using Afrowave.Toolbox.WhenItFails.Interfaces;
using Afrowave.Toolbox.WhenItFails.Services;
using Microsoft.Extensions.DependencyInjection;

IErrorCatalogContextStore store = new ErrorCatalogContextStore();
if (store.IsInitialized || store.Current is not null || store.GetCurrent().IsSuccess)
    throw new InvalidOperationException("A fresh store must be uninitialized.");

var context = new ErrorCatalogContext();
store.Set(context);
if (!store.IsInitialized ||
    !ReferenceEquals(store.Current, context) ||
    !ReferenceEquals(store.GetCurrent().Data, context))
    throw new InvalidOperationException("Legacy store publication/read contract changed.");

IServiceCollection services = new ServiceCollection();
if (!ReferenceEquals(services.AddWhenItFails(), services))
    throw new InvalidOperationException("DI registration must return the same collection.");

using ServiceProvider provider = services.BuildServiceProvider();
IErrorCatalogRuntime runtime = provider.GetRequiredService<IErrorCatalogRuntime>();
if (runtime.GetCurrentContext().IsSuccess || runtime.GetStatus().IsSuccess)
    throw new InvalidOperationException("A fresh runtime must not expose an active context.");

Console.WriteLine("LOADED|" + typeof(ErrorCatalogContextStore).Assembly.Location);
Console.WriteLine("RESULT|PASS|STORE|DI|UNINITIALIZED_RUNTIME");
'@

if ($ExerciseInitialization -and $ExerciseProjectInitialization) {
    throw 'Choose only one optional consumer probe at a time.'
}
if ($ExerciseProjectRecovery -and -not $ExerciseProjectInitialization) {
    throw 'Project recovery requires -ExerciseProjectInitialization.'
}
if ($ExerciseFirstStartFallback -and ($ExerciseInitialization -or $ExerciseProjectInitialization -or $ExerciseProjectRecovery)) {
    throw 'First-start fallback is a separate optional consumer probe.'
}

# This optional probe is inserted into the very same consumer source BEFORE
# its one-and-only compilation against the original 0.1.0 package.
# Explicit reset uses isolated bundled defaults; it does not create or
# overwrite a project-local Jsons/WhenItFails workspace.
$expectedResult = 'RESULT|PASS|STORE|DI|UNINITIALIZED_RUNTIME'
if ($ExerciseInitialization) {
    $probe = @'
var reset = runtime.ResetToDefaultsAsync().GetAwaiter().GetResult();
if (!reset.IsSuccess || reset.Data?.Context is null)
    throw new InvalidOperationException(
        "The legacy explicit bundled-default initialization failed: " + reset.Message);

var active = runtime.GetCurrentContext();
var status = runtime.GetStatus();
if (!active.IsSuccess || active.Data is null ||
    !status.IsSuccess || status.Data is null ||
    status.Data.State != Afrowave.Toolbox.WhenItFails.Enums.ErrorCatalogRuntimeState.BuiltInDefaults ||
    status.Data.IsDegraded)
    throw new InvalidOperationException("The explicit bundled-default activation state is invalid.");

var byName = runtime.FromName("UNKNOWNERROR");
var byId = runtime.FromId("AFW_GEN_0001");
var byCode = runtime.FromCode(100001);
if (!byName.IsSuccess || !byId.IsSuccess || !byCode.IsSuccess ||
    byName.Data is null || byId.Data is null || byCode.Data is null)
    throw new InvalidOperationException("Legacy descriptor lookup failed after bundled-default activation.");

foreach (var descriptor in new[] { byName.Data, byId.Data, byCode.Data })
{
    if (descriptor.Id != "AFW_GEN_0001" ||
        descriptor.Name != "UNKNOWNERROR" ||
        descriptor.Code != 100001 ||
        descriptor.Title != "Unknown error" ||
        descriptor.Message != "An unknown error occurred.")
        throw new InvalidOperationException(
            "Legacy descriptor identity or catalog text changed after activation.");
}
'@
    $marker = 'Console.WriteLine("LOADED|" + typeof(ErrorCatalogContextStore).Assembly.Location);'
    if (-not $consumerSource.Contains($marker)) {
        throw 'The original consumer output marker is missing.'
    }
    $consumerSource = $consumerSource.Replace($marker,
        $probe + [Environment]::NewLine + $marker)
    $oldResult = 'Console.WriteLine("RESULT|PASS|STORE|DI|UNINITIALIZED_RUNTIME");'
    $newResult = 'Console.WriteLine("RESULT|PASS|STORE|DI|UNINITIALIZED_RUNTIME|BUILTIN_DEFAULTS|DESCRIPTOR");'
    if (-not $consumerSource.Contains($oldResult)) {
        throw 'The original consumer result marker is missing.'
    }
    $consumerSource = $consumerSource.Replace($oldResult, $newResult)
    $expectedResult = 'RESULT|PASS|STORE|DI|UNINITIALIZED_RUNTIME|BUILTIN_DEFAULTS|DESCRIPTOR'
}

if ($ExerciseProjectInitialization) {
    $probe = @'
if (args.Length != 1 || string.IsNullOrWhiteSpace(args[0]))
    throw new InvalidOperationException("An isolated project workspace root is required.");

var workspaceOptions = new Afrowave.Toolbox.WhenItFails.Configuration.JsonsOptions
{
    RootDirectory = args[0]
};
var projectCatalogFiles = new[]
{
    workspaceOptions.ErrorCatalogFilePath,
    workspaceOptions.CategoryCatalogFilePath,
    workspaceOptions.CodeGroupCatalogFilePath,
    workspaceOptions.OwnerCatalogFilePath,
    workspaceOptions.ProfilesFilePath
};
if (projectCatalogFiles.Any(File.Exists))
    throw new InvalidOperationException("The isolated project workspace must start empty.");

var initialized = runtime.InitializeAsync(workspaceOptions).GetAwaiter().GetResult();
if (!initialized.IsSuccess || initialized.Data?.Context is null)
    throw new InvalidOperationException(
        "The legacy project catalog initialization failed: " + initialized.Message);

var statusAfterInit = runtime.GetStatus();
var contextAfterInit = runtime.GetCurrentContext();
if (!statusAfterInit.IsSuccess || statusAfterInit.Data is null ||
    statusAfterInit.Data.State !=
        Afrowave.Toolbox.WhenItFails.Enums.ErrorCatalogRuntimeState.ProjectCatalog ||
    statusAfterInit.Data.IsDegraded ||
    !contextAfterInit.IsSuccess || contextAfterInit.Data is null)
    throw new InvalidOperationException("The isolated project catalog did not activate normally.");

if (projectCatalogFiles.Any(file => !File.Exists(file)))
    throw new InvalidOperationException("The initializer did not create all five expected project catalog files.");

string[] initialHashes = projectCatalogFiles
    .Select(file => Convert.ToHexString(
        System.Security.Cryptography.SHA256.HashData(File.ReadAllBytes(file))))
    .ToArray();

var byName = runtime.FromName("UNKNOWNERROR");
var byId = runtime.FromId("AFW_GEN_0001");
var byCode = runtime.FromCode(100001);
if (!byName.IsSuccess || !byId.IsSuccess || !byCode.IsSuccess ||
    byName.Data is null || byId.Data is null || byCode.Data is null)
    throw new InvalidOperationException("Legacy descriptor resolution from project catalogs failed.");

foreach (var descriptor in new[] { byName.Data, byId.Data, byCode.Data })
{
    if (descriptor.Id != "AFW_GEN_0001" ||
        descriptor.Name != "UNKNOWNERROR" ||
        descriptor.Code != 100001 ||
        descriptor.Title != "Unknown error" ||
        descriptor.Message != "An unknown error occurred.")
        throw new InvalidOperationException(
            "Legacy project catalog descriptor identity or text changed.");
}

// A second ordinary initialization must retain existing project files.
// It may rebuild the in-memory runtime context but must not rewrite the files.
var repeated = runtime.InitializeAsync(workspaceOptions).GetAwaiter().GetResult();
if (!repeated.IsSuccess || repeated.Data?.Context is null ||
    !runtime.GetStatus().IsSuccess ||
    runtime.GetStatus().Data?.State !=
        Afrowave.Toolbox.WhenItFails.Enums.ErrorCatalogRuntimeState.ProjectCatalog)
    throw new InvalidOperationException("Reinitialization of the existing project workspace failed.");

string[] repeatedHashes = projectCatalogFiles
    .Select(file => Convert.ToHexString(
        System.Security.Cryptography.SHA256.HashData(File.ReadAllBytes(file))))
    .ToArray();
if (!initialHashes.SequenceEqual(repeatedHashes, StringComparer.Ordinal))
    throw new InvalidOperationException("Reinitialization unexpectedly rewrote a project catalog file.");
'@
    if ($ExerciseProjectRecovery) {
        $recoveryProbe = @'
var retainedBeforeFailure = runtime.GetCurrentContext().Data;
if (retainedBeforeFailure is null)
    throw new InvalidOperationException("No active project context exists before recovery.");

const string invalidProjectCatalog = "{ this is intentionally malformed JSON";
File.WriteAllText(workspaceOptions.ErrorCatalogFilePath, invalidProjectCatalog,
    new System.Text.UTF8Encoding(false));

string[] hashesBeforeRecovery = projectCatalogFiles
    .Select(file => Convert.ToHexString(
        System.Security.Cryptography.SHA256.HashData(File.ReadAllBytes(file))))
    .ToArray();

var recovered = runtime.InitializeAsync(workspaceOptions).GetAwaiter().GetResult();
if (!recovered.IsSuccess || recovered.Data?.Context is null ||
    !recovered.Data.KeptPreviousContext || recovered.Data.UsedFallback ||
    recovered.Data.ContextSource !=
        Afrowave.Toolbox.WhenItFails.Enums.ErrorCatalogContextSource.PreviousContext)
    throw new InvalidOperationException(
        "The existing context was not retained after malformed project JSON.");

var retainedAfterFailure = runtime.GetCurrentContext();
var recoveredStatus = runtime.GetStatus();
if (!retainedAfterFailure.IsSuccess ||
    !ReferenceEquals(retainedBeforeFailure, retainedAfterFailure.Data) ||
    !recoveredStatus.IsSuccess || recoveredStatus.Data is null ||
    recoveredStatus.Data.State !=
        Afrowave.Toolbox.WhenItFails.Enums.ErrorCatalogRuntimeState.PreviousContextRecovery ||
    !recoveredStatus.Data.IsDegraded ||
    !recoveredStatus.Data.KeptPreviousContext ||
    recoveredStatus.Data.UsedFallback)
    throw new InvalidOperationException(
        "The degraded previous-context runtime state is inconsistent.");

string[] hashesAfterRecovery = projectCatalogFiles
    .Select(file => Convert.ToHexString(
        System.Security.Cryptography.SHA256.HashData(File.ReadAllBytes(file))))
    .ToArray();
if (!hashesBeforeRecovery.SequenceEqual(hashesAfterRecovery, StringComparer.Ordinal) ||
    File.ReadAllText(workspaceOptions.ErrorCatalogFilePath) != invalidProjectCatalog)
    throw new InvalidOperationException(
        "Recovery rewrote a user-managed malformed JSON catalog.");

var retainedDescriptor = runtime.FromId("AFW_GEN_0001");
if (!retainedDescriptor.IsSuccess ||
    retainedDescriptor.Data?.Name != "UNKNOWNERROR" ||
    retainedDescriptor.Data.Code != 100001 ||
    retainedDescriptor.Data.Title != "Unknown error" ||
    retainedDescriptor.Data.Message != "An unknown error occurred.")
    throw new InvalidOperationException(
        "The previously valid descriptor was lost after project recovery.");
'@
        $probe += [Environment]::NewLine + $recoveryProbe
    }
    $insertionMarker = 'Console.WriteLine("LOADED|" + typeof(ErrorCatalogContextStore).Assembly.Location);'
    if (-not $consumerSource.Contains($insertionMarker)) {
        throw 'The original consumer output marker is missing.'
    }
    $consumerSource = $consumerSource.Replace($insertionMarker,
        $probe + [Environment]::NewLine + $insertionMarker)
    $oldResult = 'Console.WriteLine("RESULT|PASS|STORE|DI|UNINITIALIZED_RUNTIME");'
    $newResult = 'Console.WriteLine("RESULT|PASS|STORE|DI|UNINITIALIZED_RUNTIME|PROJECT_INITIALIZATION|DESCRIPTOR|NO_FILE_REWRITE");'
    if (-not $consumerSource.Contains($oldResult)) {
        throw 'The original consumer result marker is missing.'
    }
    $consumerSource = $consumerSource.Replace($oldResult, $newResult)
    if ($ExerciseProjectRecovery) {
        $consumerSource = $consumerSource.Replace(
            'Console.WriteLine("RESULT|PASS|STORE|DI|UNINITIALIZED_RUNTIME|PROJECT_INITIALIZATION|DESCRIPTOR|NO_FILE_REWRITE");',
            'Console.WriteLine("RESULT|PASS|STORE|DI|UNINITIALIZED_RUNTIME|PROJECT_INITIALIZATION|DESCRIPTOR|NO_FILE_REWRITE|PREVIOUS_CONTEXT_RECOVERY");')
        $expectedResult = 'RESULT|PASS|STORE|DI|UNINITIALIZED_RUNTIME|PROJECT_INITIALIZATION|DESCRIPTOR|NO_FILE_REWRITE|PREVIOUS_CONTEXT_RECOVERY'
    }
    else {
        $expectedResult = 'RESULT|PASS|STORE|DI|UNINITIALIZED_RUNTIME|PROJECT_INITIALIZATION|DESCRIPTOR|NO_FILE_REWRITE'
    }
}

if ($ExerciseFirstStartFallback) {
    $probe = @'
if (args.Length != 1 || string.IsNullOrWhiteSpace(args[0]))
    throw new InvalidOperationException("An isolated first-start workspace root is required.");

if (runtime.GetCurrentContext().IsSuccess || runtime.GetStatus().IsSuccess)
    throw new InvalidOperationException("First-start fallback must have no previously active context.");

var fallbackOptions = new Afrowave.Toolbox.WhenItFails.Configuration.JsonsOptions
{
    RootDirectory = args[0]
};
if (Directory.Exists(fallbackOptions.RootDirectory))
    throw new InvalidOperationException("The isolated first-start workspace root is not empty.");

Directory.CreateDirectory(fallbackOptions.PackageDirectoryPath);
const string malformedCatalog = "{ this is intentionally malformed JSON on first startup";
File.WriteAllText(fallbackOptions.ErrorCatalogFilePath, malformedCatalog,
    new System.Text.UTF8Encoding(false));
string malformedHash = Convert.ToHexString(
    System.Security.Cryptography.SHA256.HashData(
        File.ReadAllBytes(fallbackOptions.ErrorCatalogFilePath)));

var initialization = runtime.InitializeAsync(fallbackOptions).GetAwaiter().GetResult();
if (!initialization.IsSuccess || initialization.Data?.Context is null ||
    initialization.Data.KeptPreviousContext || !initialization.Data.UsedFallback ||
    initialization.Data.ContextSource !=
        Afrowave.Toolbox.WhenItFails.Enums.ErrorCatalogContextSource.BuiltInDefaults)
    throw new InvalidOperationException(
        "Flexible first-start initialization did not activate bundled fallback.");

var active = runtime.GetCurrentContext();
var status = runtime.GetStatus();
if (!active.IsSuccess ||
    !ReferenceEquals(active.Data, initialization.Data.Context) ||
    !status.IsSuccess || status.Data is null ||
    status.Data.State !=
        Afrowave.Toolbox.WhenItFails.Enums.ErrorCatalogRuntimeState.BuiltInFallback ||
    !status.Data.IsDegraded || status.Data.KeptPreviousContext ||
    !status.Data.UsedFallback)
    throw new InvalidOperationException(
        "First-start built-in fallback state or selected context is inconsistent.");

if (!File.Exists(fallbackOptions.ErrorCatalogFilePath) ||
    File.ReadAllText(fallbackOptions.ErrorCatalogFilePath) != malformedCatalog ||
    Convert.ToHexString(System.Security.Cryptography.SHA256.HashData(
        File.ReadAllBytes(fallbackOptions.ErrorCatalogFilePath))) != malformedHash)
    throw new InvalidOperationException(
        "First-start fallback rewrote or replaced the malformed project JSON.");

var byName = runtime.FromName("UNKNOWNERROR");
var byId = runtime.FromId("AFW_GEN_0001");
var byCode = runtime.FromCode(100001);
if (!byName.IsSuccess || !byId.IsSuccess || !byCode.IsSuccess ||
    byName.Data is null || byId.Data is null || byCode.Data is null)
    throw new InvalidOperationException("Bundled fallback descriptor lookup failed.");

foreach (var descriptor in new[] { byName.Data, byId.Data, byCode.Data })
{
    if (descriptor.Id != "AFW_GEN_0001" ||
        descriptor.Name != "UNKNOWNERROR" ||
        descriptor.Code != 100001 ||
        descriptor.Title != "Unknown error" ||
        descriptor.Message != "An unknown error occurred.")
        throw new InvalidOperationException(
            "First-start fallback descriptor identity or catalog text changed.");
}
'@
    $insertionMarker = 'Console.WriteLine("LOADED|" + typeof(ErrorCatalogContextStore).Assembly.Location);'
    if (-not $consumerSource.Contains($insertionMarker)) {
        throw 'The original consumer output marker is missing.'
    }
    $consumerSource = $consumerSource.Replace($insertionMarker,
        $probe + [Environment]::NewLine + $insertionMarker)
    $oldResult = 'Console.WriteLine("RESULT|PASS|STORE|DI|UNINITIALIZED_RUNTIME");'
    $newResult = 'Console.WriteLine("RESULT|PASS|STORE|DI|UNINITIALIZED_RUNTIME|FIRST_START_FALLBACK|DESCRIPTOR|NO_JSON_REWRITE");'
    if (-not $consumerSource.Contains($oldResult)) {
        throw 'The original consumer result marker is missing.'
    }
    $consumerSource = $consumerSource.Replace($oldResult, $newResult)
    $expectedResult = 'RESULT|PASS|STORE|DI|UNINITIALIZED_RUNTIME|FIRST_START_FALLBACK|DESCRIPTOR|NO_JSON_REWRITE'
}

$consumerProject = Join-Path $consumerDir 'Consumer.csproj'
[IO.File]::WriteAllText($consumerProject, $projectXml,
    [Text.UTF8Encoding]::new($false))
[IO.File]::WriteAllText((Join-Path $consumerDir 'Program.cs'), $consumerSource,
    [Text.UTF8Encoding]::new($false))

function Invoke-Dotnet {
    param([string[]]$Arguments)
    & dotnet @Arguments
    if ($LASTEXITCODE -ne 0) {
        throw "dotnet failed (exit $LASTEXITCODE): $($Arguments -join ' ')"
    }
}

function Invoke-Consumer {
    param([string]$Application, [string]$ExpectedWhenItFailsAssembly, [string]$WorkspaceRoot)

    $lines = @(& dotnet $Application $WorkspaceRoot)
    if ($LASTEXITCODE -ne 0) {
        throw "Consumer execution failed (exit $LASTEXITCODE): $Application"
    }

    $loaded = @($lines | Where-Object { $_ -like 'LOADED|*' })
    $result = @($lines | Where-Object { $_ -like 'RESULT|*' })
    if ($loaded.Count -ne 1 -or $result.Count -ne 1) {
        throw 'Consumer did not emit exactly one assembly location and result.'
    }

    $actual = $loaded[0].Substring('LOADED|'.Length)
    if (-not [string]::Equals(
        [IO.Path]::GetFullPath($actual),
        [IO.Path]::GetFullPath($ExpectedWhenItFailsAssembly),
        [StringComparison]::OrdinalIgnoreCase)) {
        throw "The executable loaded a different WhenItFails assembly: $actual"
    }

    if ($result[0] -ne $expectedResult) {
        throw "Consumer contract result is unexpected: $($result[0])"
    }

    return $result[0]
}

$restore = @('restore', $consumerProject)
if ($Feed) { $restore += @('--source', $Feed) }
Invoke-Dotnet $restore

# Compile the consumer ONCE against exactly the requested package.
Invoke-Dotnet @('build', $consumerProject, '-c', 'Release', '--no-restore')
$publishedBin = Join-Path $consumerDir 'bin/Release/net10.0'
$publishedApplication = Join-Path $publishedBin 'Consumer.dll'
$publishedDll = Join-Path $publishedBin 'Afrowave.Toolbox.WhenItFails.dll'
if (-not (Test-Path -LiteralPath $publishedApplication -PathType Leaf) -or
    -not (Test-Path -LiteralPath $publishedDll -PathType Leaf)) {
    throw 'Package-built consumer or its package DLL is missing.'
}

$publishedHash = (Get-FileHash -LiteralPath $publishedDll -Algorithm SHA256).Hash
$consumerHash = (Get-FileHash -LiteralPath $publishedApplication -Algorithm SHA256).Hash
$publishedWorkspaceRoot = Join-Path $workspace 'ProjectWorkspaceOriginal'
$publishedResult = Invoke-Consumer $publishedApplication $publishedDll $publishedWorkspaceRoot

# Independently build source DLL, but do not rebuild the package consumer.
Invoke-Dotnet @('build', $whenItFailsProject, '-c', 'Release')
$sourceDll = Join-Path $root 'WhenItFails/bin/Release/net10.0/Afrowave.Toolbox.WhenItFails.dll'
if (-not (Test-Path -LiteralPath $sourceDll -PathType Leaf)) {
    throw "The source-built WhenItFails DLL is missing: $sourceDll"
}
$sourceHash = (Get-FileHash -LiteralPath $sourceDll -Algorithm SHA256).Hash
if ($sourceHash -eq $publishedHash) {
    throw 'Package and source DLL hashes are identical; no distinct swap was tested.'
}

# Copy the complete original output, then replace ONLY WhenItFails.dll.
# Its .deps.json, Essentials package dependency, other dependencies, and
# executable remain exactly those of the original package-built consumer.
Copy-Item -Path (Join-Path $publishedBin '*') -Destination $swappedDir -Recurse -Force
$swappedApplication = Join-Path $swappedDir 'Consumer.dll'
$swappedDll = Join-Path $swappedDir 'Afrowave.Toolbox.WhenItFails.dll'
Copy-Item -LiteralPath $sourceDll -Destination $swappedDll -Force
if ((Get-FileHash -LiteralPath $swappedApplication -Algorithm SHA256).Hash -ne $consumerHash) {
    throw 'The consumer executable changed during the swap.'
}
if ((Get-FileHash -LiteralPath $swappedDll -Algorithm SHA256).Hash -ne $sourceHash) {
    throw 'The swapped WhenItFails DLL does not match the source build.'
}
$swappedWorkspaceRoot = Join-Path $workspace 'ProjectWorkspaceSwapped'
$swappedResult = Invoke-Consumer $swappedApplication $swappedDll $swappedWorkspaceRoot
if ($swappedResult -ne $publishedResult) {
    throw 'The package consumer produced different contract results after the DLL swap.'
}

$reportParent = Split-Path -Parent $ReportPath
if (-not (Test-Path -LiteralPath $reportParent -PathType Container)) {
    throw "Report directory does not exist: $reportParent"
}

$report = @(
    '# WhenItFails 0.1.0 precompiled-consumer binary smoke'
    ''
    'Result: PASS — both executions completed with the same expected legacy contract result.'
    "Consumer probe: $(if ($ExerciseFirstStartFallback) { 'first-start invalid project JSON; bundled fallback; malformed file unchanged; three descriptor lookups' } elseif ($ExerciseProjectRecovery) { 'project catalogs then malformed JSON and previous-context recovery, with all five files preserved' } elseif ($ExerciseProjectInitialization) { 'project catalogs (isolated workspace; two InitializeAsync calls; five files preserved; descriptor lookup)' } elseif ($ExerciseInitialization) { 'bundled defaults (explicit reset; descriptor lookup)' } else { 'original pre-initialization path' })"
    "Requested package: Afrowave.Toolbox.WhenItFails [0.1.0]"
    "Feed override: $(if ($Feed) { $Feed } else { 'configured NuGet sources/cache; publishing provenance unverified' })"
    "Package consumer executable SHA-256 (unchanged): $consumerHash"
    "Package WhenItFails DLL SHA-256: $publishedHash"
    "Source-built WhenItFails DLL SHA-256 (swapped): $sourceHash"
    "Package consumer executable: $publishedApplication"
    "Swapped consumer executable: $swappedApplication"
    "Package run: $publishedResult"
    "Swapped run: $swappedResult"
    ''
    "Scope: an executable compiled only once against requested package 0.1.0 and re-run without rebuilding after replacing only WhenItFails.dll. Legacy store/DI/pre-initialization calls$(if ($ExerciseFirstStartFallback) { ', first-start malformed project JSON with built-in fallback, unchanged user-managed file, and UNKNOWNERROR descriptor lookup in two isolated temp roots' } elseif ($ExerciseProjectRecovery) { ', valid project initialization and malformed-JSON previous-context recovery using separate temp roots, catalog-file integrity and retained descriptor lookup' } elseif ($ExerciseProjectInitialization) { ', project workspace initialization/reinitialization in two separate temp roots, five catalog-file hashes and UNKNOWNERROR descriptor lookup' } elseif ($ExerciseInitialization) { ', explicit bundled-default activation and UNKNOWNERROR descriptor lookup' } else { '' })."
    'The original .deps.json and other package dependencies remain unchanged. This narrow smoke is not exhaustive ABI, dependency-version, nullable, JSON, project-workspace initialization, recovery or behavioral compatibility testing.'
)
$report | Set-Content -LiteralPath $ReportPath -Encoding UTF8
Write-Host $(if ($ExerciseFirstStartFallback) { 'Binary first-start fallback smoke: PASS (original package consumer and swapped source DLL).' } elseif ($ExerciseProjectRecovery) { 'Binary project recovery smoke: PASS (original package consumer and swapped source DLL).' } elseif ($ExerciseProjectInitialization) { 'Binary project initialization smoke: PASS (original package consumer and swapped source DLL).' } elseif ($ExerciseInitialization) { 'Binary initialization smoke: PASS (original package consumer and swapped source DLL).' } else { 'Binary smoke: PASS (original package consumer and swapped source DLL).' })
Write-Host "Report: $ReportPath"
Write-Host "Temporary consumers: $workspace"
