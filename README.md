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

## Community-first service and transparent infrastructure funding

Afrowave's hosted services aim to give everyone the **maximum feasible access free from launch and permanently**, with no personal enrichment, artificial paywalls or paid queue-jumping inside shared community capacity. Optional donations should fund verifiable real services (e.g. LibreTranslate VPS or AI provider credits), with privacy-safe last-cent reconciliation of received funds, fees, actual invoices, usable capacity and unspent balances. These hosting values impose **no payment dependency on Toolbox libraries**. See [Afrowave Community Access and Transparent Funding](Docs/Afrowave-Community-Access-and-Transparent-Funding/en.md).

### Simple, verifiable community expense display

Future community funding transparency will show **real gross contributions, one actual transfer/processing fee, net available funds, the full amount actually paid for each service (tax included), and unspent reserves**—not an overwhelming VAT/tax breakdown. Detailed accounting evidence remains available for internal reconciliation and any required compliance. This is a planned principle only; donations and financial monitoring are **not being implemented before useful services exist**. See [Community Funding](Docs/Afrowave-Community-Access-and-Transparent-Funding/en.md).

### Optional future bank-verified public funds view

When services are running and voluntary support is useful, Afrowave intends to prefer a **dedicated bank account with officially supported public read-only transaction and balance data**. Any eventual public finance page should display bank-sourced incoming/outgoing postings, bank balance and last verified update alongside actual cost/purpose reconciliation—never bank credentials, write access or sensitive donor details. Bank data freshness and its direct public privacy implications must be clear. **Not implemented or accepting donations yet.** See [Community Funding](Docs/Afrowave-Community-Access-and-Transparent-Funding/en.md).

## Planned UI component family: Blackboard

[Blackboard](Blackboard/README.md) is the documented proposed home for reusable keyboard-first TUI controls, starting with a Spectre.Console file browser. **No Blackboard runtime package or tests exist yet.** The suggested complementary future GUI family is called *Whiteboard* (name provisional), with Avalonia as a potential adapter. Complete utilities will continue to live under Toolroom, while UI component libraries remain independently reusable.
