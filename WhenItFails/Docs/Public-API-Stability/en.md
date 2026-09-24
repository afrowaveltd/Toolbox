# Public API stability — core entry points

This document records the pre-1.0 public API review of the application-facing entry points. It is a review baseline, not a declaration that version 1.0 has shipped.

## Verified external package baseline

The separately restored NuGet package version 0.1.0 has been exercised by an external .NET 10 consumer through registration, service resolution, initialization, status inspection, and error descriptor resolution. A separate reflection inspection of that consumer recorded the signatures below.

## Stable-contract candidates

- `Afrowave.Toolbox.WhenItFails.Interfaces.IErrorCatalogRuntime`: nine declared methods, including both initialization overloads, reset, current context, status, three descriptor lookup methods, and profile resolution. Its existing `CancellationToken` arguments remain optional. Adding interface members is a compatibility decision because third-party implementations may exist.
- `Microsoft.Extensions.DependencyInjection.WhenItFailsServiceCollectionExtensions.AddWhenItFails`: four overloads accepting `IServiceCollection` alone or with `WhenItFailsOptions`, `IConfigurationSection`, or `Action<WhenItFailsOptions>`. Each overload returns `IServiceCollection`.

The source-level contract tests are in `WhenItFails.Tests/PublicApi/CoreEntryPointPublicApiContractTests.cs`. They check method counts, parameter and return types, optional cancellation-token parameters, and the DI extension-method shape. These tests do not replace the external NuGet consumer check.

## Error data model baseline (verification pending)

The next contract snapshot covers `ErrorDescriptor` (21 declared public properties; unsealed) and `ErrorDefinition` (16 declared public properties; sealed). Both have a public parameterless constructor and mutable model properties. String identity fields are initialized to empty strings, severity defaults to `Error`, and collections plus `MetadataBag` are initialized per instance.

Their JSON property names are explicit and case-sensitive. `ErrorDescriptor.Severity` serializes as `severity`, while `ErrorDefinition.DefaultSeverity` serializes as `defaultSeverity`. `ErrorDescriptor.Exception` is intentionally excluded from JSON; `MetadataBag` serializes as a plain JSON object and is round-trippable through its converter.

The focused review tests are in `WhenItFails.Tests/PublicApi/ErrorModelPublicApiContractTests.cs`. They are awaiting local verification. The shape snapshot is not a blanket assertion that every mutable detail of these models is frozen for 1.0.

## Still under review

`ErrorCatalogContext`, `ErrorCatalogRuntimeStatus`, `WhenItFailsOptions`, and `JsonsOptions` are public data/configuration models. Their current construction, mutability, JSON names, nullability annotations, and relevant behavior require separate review before their full 1.0 guarantees are set. In particular, an exposed mutable `ErrorCatalogContext` must not be mistaken for an immutable runtime snapshot.

No production visibility, names, signatures, or runtime behavior have been changed by this checkpoint.

## Verification

The maintainer confirmed both focused tests GREEN and the complete `WhenItFails.Tests` suite **1159/1159 GREEN** after commit `f9065aee1322993942b6ae0ac50aecac3dcbb3b2`.
