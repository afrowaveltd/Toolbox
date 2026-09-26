# Afrowave.Toolbox.Essentials

`Afrowave.Toolbox.Essentials` is the foundational package shared by the
Afrowave Toolbox family. It targets .NET 10 and provides small, reusable
contracts and value types that higher-level packages can depend on without
pulling in application-specific behavior.

## Main areas

- `Result`, `Response` and generic response/result models;
- issue and diagnostic models;
- metadata containers and JSON support;
- common interfaces and guards;
- enum and utility extensions;
- value objects such as culture, profile and provider names.

## Package

Current package metadata version: `0.2.0`.

Build:

```bash
dotnet build Essentials/Essentials.csproj -c Release
```

Test:

```bash
dotnet test Essentials.Tests/Essentials.Tests.csproj -c Release
```

Pack:

```bash
dotnet pack Essentials/Essentials.csproj -c Release
```

The package contains its project-specific README, the repository license,
XML documentation and the Essentials package icon.

## Documentation

- [Overview](Docs/Overview/en.md)
- [Packaging](Docs/Packaging/en.md)

## Repository

Source of truth:
`https://github.com/afrowaveltd/Toolbox`
