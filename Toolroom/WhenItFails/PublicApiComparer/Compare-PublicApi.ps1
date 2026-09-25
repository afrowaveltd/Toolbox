[CmdletBinding()]
param(
    [string]$Feed,
    [string]$ReportPath = (Join-Path ([IO.Path]::GetTempPath()) 'WhenItFails-published-vs-source-api.md')
)

Set-StrictMode -Version Latest
$ErrorActionPreference = 'Stop'

$root = (Resolve-Path (Join-Path $PSScriptRoot '../../..')).Path
$project = (Resolve-Path (Join-Path $root 'WhenItFails/WhenItFails.csproj')).Path
$escapedProject = [System.Security.SecurityElement]::Escape($project)
$workspace = Join-Path ([IO.Path]::GetTempPath()) ('WIF-ApiCompare-' + [guid]::NewGuid().ToString('N'))
$sourceDir = Join-Path $workspace 'Source'
$packageDir = Join-Path $workspace 'Published'
New-Item -ItemType Directory -Force -Path $sourceDir, $packageDir | Out-Null

$head = @'
<Project Sdk="Microsoft.NET.Sdk">
  <PropertyGroup>
    <OutputType>Exe</OutputType>
    <TargetFramework>net10.0</TargetFramework>
    <ImplicitUsings>enable</ImplicitUsings>
    <Nullable>enable</Nullable>
  </PropertyGroup>
  <ItemGroup>
'@
$tail = @'
  </ItemGroup>
</Project>
'@

Set-Content -Path (Join-Path $sourceDir 'Consumer.csproj') -Value (
    $head + [Environment]::NewLine +
    '    <ProjectReference Include="' + $escapedProject + '" />' +
    [Environment]::NewLine + $tail)
Set-Content -Path (Join-Path $packageDir 'Consumer.csproj') -Value (
    $head + [Environment]::NewLine +
    '    <PackageReference Include="Afrowave.Toolbox.WhenItFails" Version="[0.1.0]" />' +
    [Environment]::NewLine + $tail)

$inspector = @'
using System.Reflection;
using Afrowave.Toolbox.WhenItFails.Interfaces;

Assembly assembly = typeof(IErrorCatalogRuntime).Assembly;
const BindingFlags flags = BindingFlags.Public | BindingFlags.Instance |
    BindingFlags.Static | BindingFlags.DeclaredOnly;

Console.WriteLine("DLL|" + assembly.Location);
Console.WriteLine("VERSION|" + assembly.GetName().Version);
foreach (Type type in assembly.GetExportedTypes()
             .OrderBy(t => t.FullName, StringComparer.Ordinal))
{
    string name = type.FullName ?? type.Name;
    Console.WriteLine($"API|{name}|kind|{(type.IsInterface ? "interface" : type.IsEnum ? "enum" : type.IsValueType ? "struct" : type.IsSealed ? "sealed" : "class")}");
    if (type.BaseType is { } parent && parent != typeof(object))
        Console.WriteLine($"API|{name}|base|{parent.FullName}");
    foreach (Type implemented in type.GetInterfaces())
        Console.WriteLine($"API|{name}|interface|{implemented.FullName}");
    if (type.IsEnum)
    {
        foreach (FieldInfo field in type.GetFields(
                     BindingFlags.Public | BindingFlags.Static | BindingFlags.DeclaredOnly))
            Console.WriteLine($"API|{name}|enum|{field.Name}={field.GetRawConstantValue()}");
        continue;
    }
    foreach (ConstructorInfo ctor in type.GetConstructors(flags))
        Console.WriteLine($"API|{name}|ctor|{ctor}");
    foreach (MethodInfo method in type.GetMethods(flags))
    {
        if (method.IsSpecialName && (method.Name.StartsWith("get_") ||
            method.Name.StartsWith("set_") || method.Name.StartsWith("add_") ||
            method.Name.StartsWith("remove_")))
            continue;
        string parameters = string.Join(",", method.GetParameters()
            .Select(p => p.ParameterType.FullName + ":" + p.Name + ":" + p.IsOptional));
        Console.WriteLine($"API|{name}|method|{method}|{parameters}|static={method.IsStatic}");
    }
    foreach (PropertyInfo property in type.GetProperties(flags))
    {
        MethodInfo? setter = property.SetMethod;
        string accessors = (property.GetMethod?.IsPublic == true ? "get;" : "") +
            (setter?.IsPublic == true
                ? setter.ReturnParameter.GetRequiredCustomModifiers()
                    .Contains(typeof(System.Runtime.CompilerServices.IsExternalInit))
                    ? "init;" : "set;" : "");
        Console.WriteLine($"API|{name}|property|{property.PropertyType.FullName} {property.Name}|{accessors}");
    }
    foreach (FieldInfo field in type.GetFields(flags))
        Console.WriteLine($"API|{name}|field|{field.FieldType.FullName} {field.Name}");
    foreach (EventInfo item in type.GetEvents(flags))
        Console.WriteLine($"API|{name}|event|{item.EventHandlerType?.FullName} {item.Name}");
}
'@

Set-Content -Path (Join-Path $sourceDir 'Program.cs') -Value $inspector
Set-Content -Path (Join-Path $packageDir 'Program.cs') -Value $inspector

function Invoke-Dotnet {
    param([string[]]$Arguments)
    & dotnet @Arguments
    if ($LASTEXITCODE -ne 0) {
        throw "dotnet failed (exit $LASTEXITCODE): $($Arguments -join ' ')"
    }
}

Invoke-Dotnet @('restore', (Join-Path $sourceDir 'Consumer.csproj'))
$restore = @('restore', (Join-Path $packageDir 'Consumer.csproj'))
if ($Feed) { $restore += @('--source', $Feed) }
Invoke-Dotnet $restore
Invoke-Dotnet @('build', (Join-Path $sourceDir 'Consumer.csproj'), '-c', 'Release', '--no-restore')
Invoke-Dotnet @('build', (Join-Path $packageDir 'Consumer.csproj'), '-c', 'Release', '--no-restore')

$sourceOutput = @(& dotnet run --project (Join-Path $sourceDir 'Consumer.csproj') -c Release --no-build --no-restore)
if ($LASTEXITCODE -ne 0) { throw 'Source inspector failed.' }
$packageOutput = @(& dotnet run --project (Join-Path $packageDir 'Consumer.csproj') -c Release --no-build --no-restore)
if ($LASTEXITCODE -ne 0) { throw 'Published package inspector failed.' }

$sourceDll = (($sourceOutput | Where-Object { $_ -like 'DLL|*' } | Select-Object -First 1) -replace '^DLL[|]', '')
$packageDll = (($packageOutput | Where-Object { $_ -like 'DLL|*' } | Select-Object -First 1) -replace '^DLL[|]', '')
if (-not (Test-Path -LiteralPath $sourceDll -PathType Leaf) -or
    -not (Test-Path -LiteralPath $packageDll -PathType Leaf)) {
    throw 'Inspected assembly paths are missing.'
}

$sourceApi = @($sourceOutput | Where-Object { $_ -like 'API|*' } | Sort-Object -Unique -CaseSensitive)
$packageApi = @($packageOutput | Where-Object { $_ -like 'API|*' } | Sort-Object -Unique)
if ($sourceApi.Count -eq 0 -or $packageApi.Count -eq 0) {
    throw 'One inspector did not return public API entries.'
}
$diff = @(Compare-Object -ReferenceObject $packageApi -DifferenceObject $sourceApi -CaseSensitive)
$missing = @($diff | Where-Object { $_.SideIndicator -eq '<=' } | Select-Object -ExpandProperty InputObject)
$added = @($diff | Where-Object { $_.SideIndicator -eq '=>' } | Select-Object -ExpandProperty InputObject)

$lines = @(
    '# WhenItFails 0.1.0 package vs current source API'
    ''
    "Source DLL: $sourceDll"
    "Source SHA-256: $((Get-FileHash -LiteralPath $sourceDll -Algorithm SHA256).Hash)"
    "Published DLL: $packageDll"
    "Published SHA-256: $((Get-FileHash -LiteralPath $packageDll -Algorithm SHA256).Hash)"
    "Requested package version: [0.1.0]"
    "Feed override: $(if ($Feed) { $Feed } else { 'configured sources and cache; check provenance' })"
    "Source API entries: $($sourceApi.Count)"
    "Published API entries: $($packageApi.Count)"
    "Published-only entries: $($missing.Count)"
    "Source-only entries: $($added.Count)"
    ''
    '## Published-only entries'
    ''
)
if ($missing.Count -eq 0) { $lines += 'None.' } else {
    $lines += @($missing | ForEach-Object { '- ' + $_ })
}
$lines += @('', '## Source-only entries', '')
if ($added.Count -eq 0) { $lines += 'None.' } else {
    $lines += @($added | ForEach-Object { '- ' + $_ })
}
$lines += @(
    ''
    'Signature census only: not a complete binary, nullability, JSON or runtime compatibility guarantee.'
)

$parent = Split-Path -Parent $ReportPath
if (-not (Test-Path -LiteralPath $parent -PathType Container)) {
    throw "Report directory does not exist: $parent"
}
$lines | Set-Content -Path $ReportPath -Encoding UTF8
Write-Host "Report: $ReportPath"
Write-Host "Published-only: $($missing.Count); source-only: $($added.Count)"
Write-Host "Temporary consumers: $workspace"
