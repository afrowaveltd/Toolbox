# Afrowave.Toolbox

Common foundation for the Afrowave.Toolbox package family.

## NuGet packaging baseline

The repository uses central configuration in Directory.Build.props, which sets net10.0, nullable, implicit usings, default metadata, symbol packages, and includes README.md + LICENSE.txt in each NuGet package.

## What to fill in for each new NuGet project

In a specific .csproj, keep only package-specific values: PackageId, Description, PackageTags, Version, and optionally AssemblyName/RootNamespace.

## Creating a package

dotnet pack Essentials/Essentials.csproj -c Release
