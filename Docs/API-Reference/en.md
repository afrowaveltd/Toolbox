# API Reference

This document is a compact public API reference for `Afrowave.Toolbox.Essentials`.

It is not a generated API dump. It summarizes the public surface that is most important for users of the package.

## Namespaces

Main namespaces:

- `Afrowave.Toolbox.Essentials.Diagnostics`
- `Afrowave.Toolbox.Essentials.Enums`
- `Afrowave.Toolbox.Essentials.Extensions`
- `Afrowave.Toolbox.Essentials.Guards`
- `Afrowave.Toolbox.Essentials.Interfaces`
- `Afrowave.Toolbox.Essentials.Issues`
- `Afrowave.Toolbox.Essentials.Metadata`
- `Afrowave.Toolbox.Essentials.Results`
- `Afrowave.Toolbox.Essentials.ValueObjects`

## Results

Types:

- `Result`
- `Response`
- `Response<T>`

Interfaces:

- `IResult`
- `IResponse`
- `IResponse<T>`

Important members:

- `Status`
- `Message`
- `Issues`
- `Metadata`
- `IsSuccess`
- `IsFailure`
- `HasWarnings`
- `HasData`
- `Data`

Important factories:

- `Ok`
- `OkWithWarnings`
- `Fail`
- `Invalid`
- `NotFound`
- `Partial`
- `NotSupported`
- `Cancelled`
- `FromIssue`
- `FromIssues`

Important copy helpers:

- `WithMessage`
- `WithStatus`
- `WithData`
- `WithIssues`
- `WithMetadata`
- `AddIssue`
- `AddIssues`
- `AddMetadata`

## Issues

Types:

- `IssueInfo`
- `IssueInfoFactory`
- `IssueInfoListFactory`

Important members:

- `Code`
- `Number`
- `Message`
- `Details`
- `Severity`
- `Metadata`

Important factory methods:

- `Create`
- `Information`
- `Warning`
- `Error`
- `Critical`
- `Fatal`

## Metadata

Types:

- `MetadataBag`
- `MetadataBagFactory`

Important members:

- `Count`
- `IsEmpty`
- `Items`
- indexer `this[string key]`
- `Set`
- `TryGet`
- `Remove`
- `Clear`

Factory methods:

- `Empty`
- `From`
- `CopyFrom`
- `CopyWith`

## Diagnostics

Types:

- `DiagnosticInfo`
- `DiagnosticLocation`
- `DiagnosticSpan`
- `DiagnosticHint`
- `DiagnosticHintKind`

Factories:

- `DiagnosticInfoFactory`
- `DiagnosticInfoListFactory`
- `DiagnosticLocationFactory`
- `DiagnosticSpanFactory`
- `DiagnosticSpanListFactory`
- `DiagnosticHintFactory`

## Guards

Type:

- `Guard`

Methods:

- `NotNull`
- `NotNullOrWhiteSpace`
- `NotNegative`
- `Positive`
- `InRange`

## Enums

Types:

- `DataFormat`
- `IssueSeverity`
- `OperationRiskLevel`
- `OperationStatus`
- `ResultStatus`
- `SortDirection`

## Value objects

Types:

- `CultureCode`
- `ProviderName`
- `ProfileName`

Common members:

- `Value`
- `From`
- implicit conversion to `string`
- explicit conversion from `string`

`CultureCode` additionally has:

- `IsNeutral`
- `IsSpecific`
- `GetNeutralPart`

## Common interfaces

Interfaces include:

- `IHasCode`
- `IHasCultureCode`
- `IHasDataFormat`
- `IHasDescription`
- `IHasDetails`
- `IHasDiagnostics`
- `IHasId<TId>`
- `IHasIssues`
- `IHasMessage`
- `IHasMetadata`
- `IHasName`
- `IHasNumber`
- `IHasProfileName`
- `IHasProviderName`
- `IHasRiskLevel`
- `IHasSeverity`
- `IHasStatus<TStatus>`

## Important behavior contract

- null root objects usually throw `ArgumentNullException`
- invalid required strings usually throw `ArgumentException`
- metadata values cannot be null
- result and response metadata is copied by copy/conversion APIs
- result and response issue collections are normalized when null
- the public generic response type is `Response<T>`, not `ResponseOfT<T>`
