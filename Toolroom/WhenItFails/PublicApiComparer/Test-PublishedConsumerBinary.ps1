[CmdletBinding()]
param(
    [string]$Feed,
    [switch]$ExerciseInitialization,
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
    param([string]$Application, [string]$ExpectedWhenItFailsAssembly)

    $lines = @(& dotnet $Application)
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
$publishedResult = Invoke-Consumer $publishedApplication $publishedDll

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
$swappedResult = Invoke-Consumer $swappedApplication $swappedDll
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
    "Initialization and descriptor probe: $(if ($ExerciseInitialization) { 'enabled (explicit bundled defaults; FromName/FromId/FromCode)' } else { 'disabled (original pre-initialization path)' })"
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
    "Scope: one executable compiled once against package 0.1.0 and run again without rebuilding after replacing only WhenItFails.dll. It exercises original context-store/DI/pre-initialization calls$(if ($ExerciseInitialization) { ', plus explicit bundled-default activation, active status and the stable UNKNOWNERROR descriptor via name, ID and numeric code' } else { '' })."
    'The original .deps.json and other package dependencies remain unchanged. This narrow smoke is not exhaustive ABI, dependency-version, nullable, JSON, project-workspace initialization, recovery or behavioral compatibility testing.'
)
$report | Set-Content -LiteralPath $ReportPath -Encoding UTF8
Write-Host $(if ($ExerciseInitialization) { 'Binary initialization smoke: PASS (original package consumer and swapped source DLL).' } else { 'Binary smoke: PASS (original package consumer and swapped source DLL).' })
Write-Host "Report: $ReportPath"
Write-Host "Temporary consumers: $workspace"
