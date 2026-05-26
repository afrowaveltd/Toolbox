# Afrowave.Toolbox

Spolecny zaklad pro knihovny rodiny Afrowave.Toolbox.

## NuGet packaging baseline

Repo pouziva centralni konfiguraci v Directory.Build.props, ktera nastavuje net10.0, nullable, implicit usings, vychozi metadata, symbol balicky a pridani README.md + LICENSE.txt do kazdeho NuGet balicku.

## Co vyplnit v kazdem novem NuGet projektu

V konkretnim csproj ponechte jen balickove specificke hodnoty: PackageId, Description, PackageTags, Version a volitelne AssemblyName/RootNamespace.

## Vytvoreni balicku

dotnet pack Essentials/Essentials.csproj -c Release
