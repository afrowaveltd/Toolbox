#requires -Version 5.1
<#
.SYNOPSIS
Manual, opt-in MdView HDD benchmark on Windows.
.DESCRIPTION
By default uses H:\MDViewTest. Verifies the test volume is not on the
same physical disk as the Windows system drive and refuses reparse-point
directories. Never formats disks, clears caches, or overwrites fixtures.
.EXAMPLE
.\run.ps1 -Mode all -SizeMiB 8 -Seeks 10
.EXAMPLE
.\run.ps1 -Mode prepare
#>
[CmdletBinding()]
param(
    [Parameter(Position = 0)]
    [ValidateSet('all', 'prepare', 'generate', 'measure', 'index')]
    [string] $Mode = 'all',
    [Parameter(Position = 1)]
    [string] $Root,
    [ValidateRange(8, 8192)]
    [int] $SizeMiB = 2048,
    [ValidateRange(1, 2000)]
    [int] $Seeks = 250,
    [ValidateRange(1, 4)]
    [int] $Passes = 1,
    [ValidateRange(80, 4096)]
    [int] $Width = 120,
    [ValidateRange(8, 128)]
    [int] $Rows = 48,
    [ValidateRange(40, 4096)]
    [int] $MaxWidth = 100,
    [int] $Seed = 20261115,
    [ValidateRange(1, 10)]
    [int] $ProgressStep = 10,
    [switch] $TraceJournalIo,
    [switch] $LongTables
)

Set-StrictMode -Version Latest
$ErrorActionPreference = 'Stop'

if (-not [System.Runtime.InteropServices.RuntimeInformation]::IsOSPlatform(
        [System.Runtime.InteropServices.OSPlatform]::Windows)) {
    throw 'run.ps1 is the Windows launcher. Use run.sh on Linux.'
}
if (-not (Get-Command dotnet -ErrorAction SilentlyContinue)) {
    throw '.NET 10 SDK is required (dotnet command not found).'
}
if ([string]::IsNullOrWhiteSpace($Root)) {
    $Root = if ([string]::IsNullOrWhiteSpace($env:MDVIEW_BENCH_ROOT)) {
        'H:\MDViewTest'
    } else {
        $env:MDVIEW_BENCH_ROOT
    }
}
if ($MaxWidth -gt $Width) {
    throw '-MaxWidth cannot exceed -Width.'
}

function Assert-OrdinaryDirectoryPath([string] $Path) {
    $driveRoot = [System.IO.Path]::GetPathRoot($Path)
    $cursor = $driveRoot
    foreach ($part in ($Path.Substring($driveRoot.Length) -split '\\')) {
        if ([string]::IsNullOrWhiteSpace($part)) { continue }
        $cursor = Join-Path -Path $cursor -ChildPath $part
        if (Test-Path -LiteralPath $cursor) {
            $item = Get-Item -LiteralPath $cursor -Force
            if (($item.Attributes -band [System.IO.FileAttributes]::ReparsePoint) -ne 0) {
                throw "Refusing junction, symbolic link or mount point: $cursor"
            }
            if (-not $item.PSIsContainer) {
                throw "Expected a directory, found a file: $cursor"
            }
        }
    }
}

$rootPath = [System.IO.Path]::GetFullPath($Root)
$driveRoot = [System.IO.Path]::GetPathRoot($rootPath)
if ($rootPath.TrimEnd([char]'\') -eq $driveRoot.TrimEnd([char]'\')) {
    throw 'Refusing to use a volume root directly as the benchmark directory.'
}
if ($driveRoot -notmatch '^[A-Za-z]:\\$') {
    throw 'The test must use a local drive letter on a separate physical disk.'
}
$testLetter = $driveRoot.Substring(0, 1).ToUpperInvariant()
$osRoot = [System.IO.Path]::GetPathRoot($env:SystemRoot)
if (-not $osRoot -or $osRoot -notmatch '^[A-Za-z]:\\$') {
    throw 'Could not determine the Windows system drive safely.'
}
$osLetter = $osRoot.Substring(0, 1).ToUpperInvariant()
if ($testLetter -eq $osLetter) {
    throw ("Refusing the Windows system volume " + $osLetter + ":")
}
if (-not (Test-Path -LiteralPath $driveRoot -PathType Container)) {
    throw "Drive $driveRoot does not exist or is not accessible."
}

# Verify physical-disk separation, not just drive-letter separation.
# Get-Partition also rejects network shares and unsupported volume types.
$testPartition = Get-Partition -DriveLetter $testLetter -ErrorAction Stop
$osPartition = Get-Partition -DriveLetter $osLetter -ErrorAction Stop
if ($testPartition.DiskNumber -eq $osPartition.DiskNumber) {
    throw 'Test volume and Windows system volume are on the same physical disk.'
}
$testDisk = Get-Disk -Number $testPartition.DiskNumber -ErrorAction Stop
if ($testDisk.IsOffline -or $testDisk.IsReadOnly) {
    throw 'The test disk is offline or read-only.'
}
$volume = Get-Volume -DriveLetter $testLetter -ErrorAction Stop
if (-not $volume.FileSystem -or $volume.SizeRemaining -le 0) {
    throw 'The target volume must have a mounted, writable filesystem.'
}

$sizeForReserve = if ($Mode -eq 'prepare') {
    [Math]::Max(2048, $SizeMiB)
} else {
    $SizeMiB
}
$needed = ([long]$sizeForReserve * 6L + 1024L) * 1048576L
$free = [long]$volume.SizeRemaining
if ($free -lt $needed) {
    throw "Insufficient free space on $driveRoot. Have $free bytes; require $needed bytes."
}

$folders = @(
    $rootPath,
    (Join-Path $rootPath 'data'),
    (Join-Path $rootPath 'tmp'),
    (Join-Path $rootPath 'logs')
)
foreach ($folder in $folders) {
    Assert-OrdinaryDirectoryPath $folder
    if ($folder -ne (Join-Path $rootPath 'logs') -and
        -not (Test-Path -LiteralPath $folder -PathType Container)) {
        [void](New-Item -ItemType Directory -Path $folder -Force)
    }
    Assert-OrdinaryDirectoryPath $folder
}

$env:TEMP = Join-Path $rootPath 'tmp'
$env:TMP = $env:TEMP
$env:TMPDIR = $env:TEMP
$env:MDVIEW_BENCH_AVAILABLE_BYTES = [string]$free
$env:DOTNET_CLI_TELEMETRY_OPTOUT = '1'
$stamp = [DateTime]::UtcNow.ToString('yyyyMMddTHHmmssZ')
$systemLog = Join-Path $rootPath "logs\system-$stamp-$PID.txt"
$systemInfo = @(
    "UTC: $([DateTime]::UtcNow.ToString('O'))",
    "Mode: $Mode",
    "Benchmark root: $rootPath",
    "Temporary root: $env:TEMP",
    "Test disk number: $($testPartition.DiskNumber)",
    "System disk number: $($osPartition.DiskNumber)",
    "Available bytes: $free",
    "Required conservative reserve bytes: $needed",
    "OS: $([System.Environment]::OSVersion.VersionString)",
    "CPU cores (logical): $([Environment]::ProcessorCount)",
    '--- Disk ---',
    ($testDisk | Format-List Number, FriendlyName, BusType, Size, IsReadOnly | Out-String),
    '--- Volume ---',
    ($volume | Format-List DriveLetter, FileSystem, Size, SizeRemaining | Out-String),
    '--- dotnet ---',
    (& dotnet --version | Out-String)
)
# Keep all system-report text in RAM until the benchmark has finished.
# Writing a report on H: during journal/snapshot measurement can
# perturb slow HDD scheduling and cache behavior.
Write-Host ("Verified separate test disk: " + $testLetter + ": (disk " + $testPartition.DiskNumber + ")")
Write-Host "Benchmark files and .NET temporary data: $rootPath"

$project = Join-Path $PSScriptRoot 'MdView.Benchmarks.csproj'
$binary = Join-Path $PSScriptRoot 'bin\Release\net10.0\MdView.Benchmarks.dll'
& dotnet build $project -c Release --nologo
if ($LASTEXITCODE -ne 0) {
    throw "Benchmark project build failed (exit code $LASTEXITCODE)."
}
$benchmarkArguments = @(
    $Mode, $rootPath,
    '--size-mib', [string]$SizeMiB,
    '--seeks', [string]$Seeks,
    '--passes', [string]$Passes,
    '--width', [string]$Width,
    '--rows', [string]$Rows,
    '--max-width', [string]$MaxWidth,
    '--seed', [string]$Seed,
    '--progress-step', [string]$ProgressStep
)
if ($LongTables) { $benchmarkArguments += '--long-tables' }
if ($TraceJournalIo) { $benchmarkArguments += '--trace-journal-io' }
$benchmarkExitCode = 1
try {
    & dotnet $binary @benchmarkArguments
    $benchmarkExitCode = $LASTEXITCODE
} finally {
    # Export platform diagnostics only AFTER the measured process ends,
    # even if that process failed. Never overwrite an older system report.
    $logsFolder = Join-Path $rootPath 'logs'
    Assert-OrdinaryDirectoryPath $logsFolder
    if (-not (Test-Path -LiteralPath $logsFolder -PathType Container)) {
        [void](New-Item -ItemType Directory -Path $logsFolder)
    }
    Assert-OrdinaryDirectoryPath $logsFolder
    $systemInfo | Out-File -LiteralPath $systemLog -Encoding UTF8 -NoClobber
    Write-Host "System information: $systemLog"
}
exit $benchmarkExitCode
