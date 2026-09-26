[CmdletBinding()]
param(
    [string]$PackageVersion = '1.0.0-rc.1',
    [string]$NuGetSource = 'https://api.nuget.org/v3/index.json',
    [string]$ReportPath = (Join-Path ([IO.Path]::GetTempPath()) 'WhenItFails-candidate-package-smoke.md'),
    [switch]$KeepWorkspace
)

Set-StrictMode -Version Latest
$ErrorActionPreference = 'Stop'

$root = (Resolve-Path (Join-Path $PSScriptRoot '../../..')).Path
$essentialsProject = (Resolve-Path (Join-Path $root 'Essentials/Essentials.csproj')).Path
$whenItFailsProject = (Resolve-Path (Join-Path $root 'WhenItFails/WhenItFails.csproj')).Path
$projectReadme = (Resolve-Path (Join-Path $root 'WhenItFails/README.md')).Path
$essentialsReadme = (Resolve-Path (Join-Path $root 'Essentials/README.md')).Path

$workspace = Join-Path ([IO.Path]::GetTempPath()) ('WIF-CandidatePackage-' + [guid]::NewGuid().ToString('N'))
$feed = Join-Path $workspace 'feed'
$consumerDir = Join-Path $workspace 'consumer'
$projectWorkspace = Join-Path $workspace 'project-jsons'
$essentialsExtractDir = Join-Path $workspace 'essentials-extract'
$extractDir = Join-Path $workspace 'package-extract'

New-Item -ItemType Directory -Force -Path $workspace, $feed, $consumerDir | Out-Null

function Invoke-Dotnet {
    param([string[]]$Arguments)

    & dotnet @Arguments
    if ($LASTEXITCODE -ne 0) {
        throw "dotnet failed (exit $LASTEXITCODE): $($Arguments -join ' ')"
    }
}

function Assert-FileExists {
    param([string]$Path)

    if (-not (Test-Path -LiteralPath $Path -PathType Leaf)) {
        throw "Expected file is missing: $Path"
    }
}

try {
    Invoke-Dotnet @(
        'pack',
        $essentialsProject,
        '-c', 'Release',
        '-o', $feed,
        '--nologo'
    )

    Invoke-Dotnet @(
        'pack',
        $whenItFailsProject,
        '-c', 'Release',
        '-o', $feed,
        '--nologo',
        "-p:WhenItFailsPackageVersionOverride=$PackageVersion"
    )

    $essentialsPackages = @(
        Get-ChildItem -LiteralPath $feed -File |
            Where-Object {
                $_.Name -like 'Afrowave.Toolbox.Essentials.*.nupkg' -and
                $_.Name -notlike '*.snupkg'
            }
    )

    if ($essentialsPackages.Count -ne 1) {
        throw "Expected exactly one Essentials nupkg, found $($essentialsPackages.Count)."
    }

    $essentialsPackage = $essentialsPackages[0]

    [System.IO.Compression.ZipFile]::ExtractToDirectory(
        $essentialsPackage.FullName,
        $essentialsExtractDir)

    $requiredEssentialsEntries = @(
        'README.md',
        'LICENSE.txt',
        'assets/toolbox-essentials-icon.png',
        'lib/net10.0/Afrowave.Toolbox.Essentials.dll',
        'lib/net10.0/Afrowave.Toolbox.Essentials.xml'
    )

    foreach ($entry in $requiredEssentialsEntries) {
        Assert-FileExists (Join-Path $essentialsExtractDir $entry)
    }

    $expectedEssentialsReadmeHash =
        (Get-FileHash -LiteralPath $essentialsReadme -Algorithm SHA256).Hash

    $packedEssentialsReadmeHash =
        (Get-FileHash -LiteralPath (Join-Path $essentialsExtractDir 'README.md') -Algorithm SHA256).Hash

    if ($expectedEssentialsReadmeHash -ne $packedEssentialsReadmeHash) {
        throw 'Essentials package README.md does not match Essentials/README.md.'
    }

    $candidatePackages = @(
        Get-ChildItem -LiteralPath $feed -File |
            Where-Object {
                $_.Name -like 'Afrowave.Toolbox.WhenItFails.*.nupkg' -and
                $_.Name -notlike '*.snupkg'
            }
    )

    if ($candidatePackages.Count -ne 1) {
        throw "Expected exactly one WhenItFails candidate nupkg, found $($candidatePackages.Count)."
    }

    $candidatePackage = $candidatePackages[0]

    [System.IO.Compression.ZipFile]::ExtractToDirectory(
        $candidatePackage.FullName,
        $extractDir)

    $requiredEntries = @(
        'README.md',
        'LICENSE.txt',
        'lib/net10.0/Afrowave.Toolbox.WhenItFails.dll',
        'lib/net10.0/Afrowave.Toolbox.WhenItFails.xml'
    )

    foreach ($entry in $requiredEntries) {
        Assert-FileExists (Join-Path $extractDir $entry)
    }

    $nuspecFiles = @(Get-ChildItem -LiteralPath $extractDir -Filter '*.nuspec' -File)

    if ($nuspecFiles.Count -ne 1) {
        throw "Expected exactly one WhenItFails nuspec, found $($nuspecFiles.Count)."
    }

    [xml]$nuspec = Get-Content -LiteralPath $nuspecFiles[0].FullName -Raw

    $essentialsDependency =
        $nuspec.SelectSingleNode(
            "//*[local-name()='dependency' and @id='Afrowave.Toolbox.Essentials']")

    if ($null -eq $essentialsDependency) {
        throw 'WhenItFails candidate nuspec has no Essentials dependency.'
    }

    $essentialsDependencyVersion =
        [string]$essentialsDependency.GetAttribute('version')

    if ($essentialsDependencyVersion -notmatch '0\.2\.0') {
        throw "WhenItFails candidate requires unexpected Essentials version: $essentialsDependencyVersion"
    }

    if ($essentialsDependencyVersion -match [regex]::Escape($PackageVersion)) {
        throw 'Candidate package version leaked into the Essentials dependency version.'
    }

    $expectedReadmeHash = (Get-FileHash -LiteralPath $projectReadme -Algorithm SHA256).Hash
    $packedReadmeHash = (Get-FileHash -LiteralPath (Join-Path $extractDir 'README.md') -Algorithm SHA256).Hash

    if ($expectedReadmeHash -ne $packedReadmeHash) {
        throw 'Candidate package README.md does not match WhenItFails/README.md.'
    }

    $consumerProject = Join-Path $consumerDir 'Consumer.csproj'
    $consumerProgram = Join-Path $consumerDir 'Program.cs'

    $projectXml = @"
<Project Sdk="Microsoft.NET.Sdk">
  <PropertyGroup>
    <OutputType>Exe</OutputType>
    <TargetFramework>net10.0</TargetFramework>
    <ImplicitUsings>enable</ImplicitUsings>
    <Nullable>enable</Nullable>
  </PropertyGroup>
  <ItemGroup>
    <PackageReference Include="Afrowave.Toolbox.WhenItFails" Version="[$PackageVersion]" />
    <PackageReference Include="Microsoft.Extensions.DependencyInjection" Version="10.0.9" />
  </ItemGroup>
</Project>
"@

    Set-Content -LiteralPath $consumerProject -Value $projectXml

    $program = @'
using Afrowave.Toolbox.WhenItFails.Configuration;
using Afrowave.Toolbox.WhenItFails.Enums;
using Afrowave.Toolbox.WhenItFails.Interfaces;
using Microsoft.Extensions.DependencyInjection;

if (args.Length != 1)
{
    throw new InvalidOperationException("Expected isolated JSON root path.");
}

JsonsOptions jsons = new()
{
    RootDirectory = args[0],
    PackageDirectoryName = "WhenItFails"
};

ServiceCollection services = new();
services.AddWhenItFails(
    new WhenItFailsOptions
    {
        InitializationMode = ErrorCatalogInitializationMode.Strict,
        Jsons = jsons
    });

using ServiceProvider provider = services.BuildServiceProvider(
    new ServiceProviderOptions
    {
        ValidateOnBuild = true,
        ValidateScopes = true
    });

IErrorCatalogRuntime runtime =
    provider.GetRequiredService<IErrorCatalogRuntime>();

var initialized = await runtime.InitializeAsync(jsons);

if (!initialized.IsSuccess || initialized.Data is null)
{
    throw new InvalidOperationException(
        "Candidate package project initialization failed.");
}

if (initialized.Data.ContextSource != ErrorCatalogContextSource.ProjectCatalog ||
    initialized.Data.IsDegraded)
{
    throw new InvalidOperationException(
        "Candidate package did not activate a clean project catalog.");
}

string[] expectedFiles =
[
    jsons.ErrorCatalogFilePath,
    jsons.CategoryCatalogFilePath,
    jsons.CodeGroupCatalogFilePath,
    jsons.OwnerCatalogFilePath,
    jsons.ProfilesFilePath
];

if (expectedFiles.Any(path => !File.Exists(path)))
{
    throw new InvalidOperationException(
        "Candidate package did not create all five project catalog files.");
}

var descriptor = runtime.FromId("AFW-GEN-0001");

if (!descriptor.IsSuccess ||
    descriptor.Data is null ||
    descriptor.Data.Id != "AFW_GEN_0001" ||
    descriptor.Data.Name != "UNKNOWNERROR")
{
    throw new InvalidOperationException(
        "Candidate package descriptor lookup failed.");
}

var reset = await runtime.ResetToDefaultsAsync();

if (!reset.IsSuccess ||
    reset.Data is null ||
    reset.Data.ContextSource != ErrorCatalogContextSource.BuiltInDefaults ||
    reset.Data.IsDegraded)
{
    throw new InvalidOperationException(
        "Candidate package explicit built-in reset failed.");
}

var status = runtime.GetStatus();

if (!status.IsSuccess ||
    status.Data is null ||
    status.Data.State != ErrorCatalogRuntimeState.BuiltInDefaults ||
    status.Data.IsDegraded)
{
    throw new InvalidOperationException(
        "Candidate package runtime status is inconsistent after reset.");
}

Console.WriteLine("CANDIDATE_PACKAGE_SMOKE=PASS");
Console.WriteLine($"PROJECT_FILES={expectedFiles.Length}");
Console.WriteLine($"DESCRIPTOR_ID={descriptor.Data.Id}");
Console.WriteLine($"DESCRIPTOR_NAME={descriptor.Data.Name}");
Console.WriteLine($"RUNTIME_STATE={status.Data.State}");
'@

    Set-Content -LiteralPath $consumerProgram -Value $program

    Invoke-Dotnet @(
        'restore',
        $consumerProject,
        '--source', $feed,
        '--source', $NuGetSource
    )

    Invoke-Dotnet @(
        'build',
        $consumerProject,
        '-c', 'Release',
        '--no-restore',
        '--nologo'
    )

    $consumerOutput = @(
        & dotnet run --project $consumerProject -c Release --no-build --no-restore -- $projectWorkspace
    )

    if ($LASTEXITCODE -ne 0) {
        throw 'Candidate package consumer execution failed.'
    }

    if ($consumerOutput -notcontains 'CANDIDATE_PACKAGE_SMOKE=PASS') {
        throw 'Candidate package consumer did not report PASS.'
    }

    $packageHash = (Get-FileHash -LiteralPath $candidatePackage.FullName -Algorithm SHA256).Hash
    $dllHash = (Get-FileHash -LiteralPath (Join-Path $extractDir 'lib/net10.0/Afrowave.Toolbox.WhenItFails.dll') -Algorithm SHA256).Hash

    $reportParent = Split-Path -Parent $ReportPath
    if ([string]::IsNullOrWhiteSpace($reportParent)) {
        $reportParent = (Get-Location).Path
    }

    New-Item -ItemType Directory -Force -Path $reportParent | Out-Null

    $report = @(
        '# WhenItFails candidate package smoke'
        ''
        'Result: **PASS**'
        ''
        "- Candidate package version: ``$PackageVersion``"
        "- Candidate nupkg SHA-256: ``$packageHash``"
        "- Candidate DLL SHA-256: ``$dllHash``"
        "- Essentials package: ``$($essentialsPackage.Name)``"
        '- Essentials icon/package entries: **present**'
        "- Essentials README matches ``Essentials/README.md``: **yes**"
        "- Package README matches ``WhenItFails/README.md``: **yes**"
        '- Required package entries: **present**'
        '- External consumer restore/build: **PASS**'
        '- Strict project initialization and five-file bootstrap: **PASS**'
        '- Descriptor lookup: **PASS**'
        '- Explicit built-in reset and runtime status: **PASS**'
        ''
        '## Consumer output'
        ''
        '```text'
        $consumerOutput
        '```'
        ''
        'This smoke validates the newly built candidate package only. It is not a'
        'substitute for the separate compatibility comparison against the original'
        'maintainer-held 0.1.0 reference nupkg.'
    )

    Set-Content -LiteralPath $ReportPath -Value $report

    Write-Host 'Candidate package smoke: PASS'
    Write-Host "Report: $ReportPath"
    Write-Host "Candidate package: $($candidatePackage.FullName)"

    if ($KeepWorkspace) {
        Write-Host "Workspace retained: $workspace"
    }
}
finally {
    if (-not $KeepWorkspace -and (Test-Path -LiteralPath $workspace)) {
        Remove-Item -LiteralPath $workspace -Recurse -Force -ErrorAction SilentlyContinue
    }
}
