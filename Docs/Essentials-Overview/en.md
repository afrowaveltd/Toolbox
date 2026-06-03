# Afrowave.Toolbox.Essentials Overview

`Afrowave.Toolbox.Essentials` is a small foundation library for common application infrastructure.

It is intended to sit at the bottom of the Toolbox package stack and provide shared building blocks that higher packages can use without pulling in heavy dependencies.

## Project areas

The package is organized into these main areas:

```text
Essentials/
  Diagnostics/
  Enums/
  Extensions/
  Guards/
  Interfaces/
  Issues/
  Metadata/
  Results/
  ValueObjects/
```

## Diagnostics

The diagnostics model is useful when a result needs source locations, spans, hints, and richer contextual information.

Main types:

- `DiagnosticInfo`
- `DiagnosticLocation`
- `DiagnosticSpan`
- `DiagnosticHint`
- diagnostic factory classes
- diagnostic collection factories

See [Diagnostics](../Diagnostics/en.md).

## Enums

The package defines shared enums for common states and categories:

- `DataFormat`
- `IssueSeverity`
- `OperationRiskLevel`
- `OperationStatus`
- `ResultStatus`
- `SortDirection`

See [Enums](../Enums/en.md).

## Extensions

The extension methods provide small convenience helpers for:

- enums
- results and responses
- issues
- diagnostics
- metadata
- value objects
- common `IHas...` interfaces

See [Extensions](../Extensions/en.md).

## Guards

The `Guard` class provides simple validation helpers for common argument checks.

See [Guards](../Guards/en.md).

## Interfaces

The interfaces define small contracts such as `IHasCode`, `IHasMessage`, `IHasMetadata`, `IResult`, `IResponse`, and `IResponse<T>`.

See [Interfaces](../Interfaces/en.md).

## Issues

Issues represent warnings, errors, and other problem reports with stable codes, messages, severity levels, optional numbers, details, and metadata.

See [Issues](../Issues/en.md).

## Metadata

`MetadataBag` is a mutable, case-insensitive string key-value container used by results, issues, and diagnostics.

See [Metadata](../Metadata/en.md).

## Results and responses

The result model provides a consistent way to return operation status, messages, issues, metadata, and optional data.

Main types:

- `Result`
- `Response`
- `Response<T>`

See [Results and Responses](../Results-and-Responses/en.md).

## Value objects

The value object types wrap common validated strings:

- `CultureCode`
- `ProviderName`
- `ProfileName`

See [Value Objects](../Value-Objects/en.md).

## Design contract

The most important cross-cutting rules are:

- public helpers validate null root objects
- invalid required strings are rejected
- `MetadataBag` values cannot be null
- metadata is copied by result and response copy/conversion methods
- issue collections are exposed as read-only lists
- null issue collections are normalized by copy/conversion methods where appropriate
