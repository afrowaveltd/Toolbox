# Afrowave.Toolbox

Afrowave.Toolbox is a .NET 10 package family that provides small, reusable building blocks for Afrowave applications and related tools.

This repository currently contains the `Afrowave.Toolbox.Essentials` package, which provides foundational contracts, result and response models, issue handling, metadata helpers, diagnostics, value objects, enums, guards, and extension methods.

## Current package

### Afrowave.Toolbox.Essentials

`Afrowave.Toolbox.Essentials` is the base package for common application infrastructure.

It includes:

- result and response models
- issue and diagnostic models
- metadata containers
- common interfaces
- enum helper extensions
- value objects such as culture codes and provider names
- guard helpers
- null-safe and consistency-focused extension methods

## Design principles

The Essentials package is intentionally small and dependency-light.

Core rules used across the package:

- public helper methods validate null root objects and invalid arguments
- nullable model properties are handled defensively where appropriate
- `MetadataBag` is mutable, so copying APIs create metadata copies instead of sharing mutable references
- issue lists are treated as read-only snapshots or read-only references
- empty factories return safe empty instances
- behavior is covered by unit tests

## Documentation

Documentation is available in the `Docs` folder:

- [Documentation Index](Docs/Documentation/en.md)
- [Getting Started](Docs/Getting-Started/en.md)
- [Essentials Overview](Docs/Essentials-Overview/en.md)
- [Results and Responses](Docs/Results-and-Responses/en.md)
- [Issues](Docs/Issues/en.md)
- [Metadata](Docs/Metadata/en.md)
- [Diagnostics](Docs/Diagnostics/en.md)
- [Extensions](Docs/Extensions/en.md)
- [Interfaces](Docs/Interfaces/en.md)
- [Enums](Docs/Enums/en.md)
- [Guards](Docs/Guards/en.md)
- [Value Objects](Docs/Value-Objects/en.md)
- [API Reference](Docs/API-Reference/en.md)

## Target framework

The current Toolbox generation targets:

```text
net10.0
```

Older compatibility targets are intentionally not part of this generation. The goal is to keep the package clean, modern, and free from historical compatibility constraints.

## Packaging

The repository uses central package configuration in `Directory.Build.props`.

Project files should usually contain only package-specific values such as:

- `PackageId`
- `Description`
- `PackageTags`
- `Version`
- optionally `AssemblyName`
- optionally `RootNamespace`

To create a release package:

```bash
dotnet pack Essentials/Essentials.csproj -c Release
```

## Testing

Run all tests with:

```bash
dotnet test
```

Before release, prefer a clean build:

```bash
dotnet clean
dotnet build
dotnet test
```

## Status

This package is under active development. Public behavior is being documented and stabilized step by step.
