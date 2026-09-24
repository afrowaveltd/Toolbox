# Public API stability — core entry points

This document records the pre-1.0 public API review of the application-facing entry points. It is a review baseline, not a declaration that version 1.0 has shipped.

## Verified external package baseline

The separately restored NuGet package version 0.1.0 has been exercised by an external .NET 10 consumer through registration, service resolution, initialization, status inspection, and error descriptor resolution. A separate reflection inspection of that consumer recorded the signatures below.

## Stable-contract candidates

- `Afrowave.Toolbox.WhenItFails.Interfaces.IErrorCatalogRuntime`: nine declared methods, including both initialization overloads, reset, current context, status, three descriptor lookup methods, and profile resolution. Its existing `CancellationToken` arguments remain optional. Adding interface members is a compatibility decision because third-party implementations may exist.
- `Microsoft.Extensions.DependencyInjection.WhenItFailsServiceCollectionExtensions.AddWhenItFails`: four overloads accepting `IServiceCollection` alone or with `WhenItFailsOptions`, `IConfigurationSection`, or `Action<WhenItFailsOptions>`. Each overload returns `IServiceCollection`.

The source-level contract tests are in `WhenItFails.Tests/PublicApi/CoreEntryPointPublicApiContractTests.cs`. They check method counts, parameter and return types, optional cancellation-token parameters, and the DI extension-method shape. These tests do not replace the external NuGet consumer check.

## Still under review

`ErrorDescriptor`, `ErrorDefinition`, `ErrorCatalogContext`, `ErrorCatalogRuntimeStatus`, `WhenItFailsOptions`, and `JsonsOptions` are public data/configuration models. Their current construction, mutability, JSON names, nullability annotations, and relevant behavior require separate review before their full 1.0 guarantees are set. In particular, an exposed mutable `ErrorCatalogContext` must not be mistaken for an immutable runtime snapshot.

No production visibility, names, signatures, or runtime behavior have been changed by this checkpoint.

## Verification

Run the two focused contract tests and then the complete `WhenItFails.Tests` suite. Record actual results in `WhenItFails/IMPLEMENTATION_STATUS.md` only after the maintainer confirms them.
