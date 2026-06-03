# Release Notes

## Afrowave.Toolbox.Essentials 0.2.0

- Prepared the first NuGet-ready Essentials package publication.
- Added package icon metadata and packaged icon asset.
- Marked the release as documentation-ready with no functional API changes.

## Publishing

Build, test, and pack the release:

```bash
dotnet clean
dotnet restore
dotnet build Toolbox.sln -c Release --no-restore
dotnet test Toolbox.sln -c Release --no-build
dotnet pack Essentials/Essentials.csproj -c Release --no-build -o artifacts/packages
```

Publish after verifying the generated package:

```bash
dotnet nuget push artifacts/packages/Afrowave.Toolbox.Essentials.0.2.0.nupkg --source https://api.nuget.org/v3/index.json --api-key <NUGET_API_KEY>
```
