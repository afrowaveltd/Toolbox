# Public API stability — core entry points

This document records the pre-1.0 public API review of the application-facing entry points. It is a review baseline, not a declaration that version 1.0 has shipped.

## Verified external package baseline

The separately restored NuGet package version 0.1.0 has been exercised by an external .NET 10 consumer through registration, service resolution, initialization, status inspection, and error descriptor resolution. A separate reflection inspection of that consumer recorded the signatures below.

## Stable-contract candidates

- `Afrowave.Toolbox.WhenItFails.Interfaces.IErrorCatalogRuntime`: nine declared methods, including both initialization overloads, reset, current context, status, three descriptor lookup methods, and profile resolution. Its existing `CancellationToken` arguments remain optional. Adding interface members is a compatibility decision because third-party implementations may exist.
- `Microsoft.Extensions.DependencyInjection.WhenItFailsServiceCollectionExtensions.AddWhenItFails`: four overloads accepting `IServiceCollection` alone or with `WhenItFailsOptions`, `IConfigurationSection`, or `Action<WhenItFailsOptions>`. Each overload returns `IServiceCollection`.

The source-level contract tests are in `WhenItFails.Tests/PublicApi/CoreEntryPointPublicApiContractTests.cs`. They check method counts, parameter and return types, optional cancellation-token parameters, and the DI extension-method shape. These tests do not replace the external NuGet consumer check.

## Error data model baseline (1163/1163 GREEN)

The next contract snapshot covers `ErrorDescriptor` (21 declared public properties; unsealed) and `ErrorDefinition` (16 declared public properties; sealed). Both have a public parameterless constructor and mutable model properties. String identity fields are initialized to empty strings, severity defaults to `Error`, and collections plus `MetadataBag` are initialized per instance.

Their JSON property names are explicit and case-sensitive. `ErrorDescriptor.Severity` serializes as `severity`, while `ErrorDefinition.DefaultSeverity` serializes as `defaultSeverity`. `ErrorDescriptor.Exception` is intentionally excluded from JSON; `MetadataBag` serializes as a plain JSON object and is round-trippable through its converter.

The focused review tests are in `WhenItFails.Tests/PublicApi/ErrorModelPublicApiContractTests.cs`. The maintainer confirmed all four focused tests and the complete 1163/1163 suite GREEN. The shape snapshot is not a blanket assertion that every mutable detail of these models is frozen for 1.0.

## Context and runtime status baseline (verification pending)

`ErrorCatalogContext` exposes seven public get/set properties including the runtime `IErrorCatalog`, normalized documents and cross-validation result. The context store publishes a reference atomically and gives callers that **same mutable object**; it does not deep-clone or enforce immutable catalog content. Treat `GetCurrentContext()` as access to shared active state, **not** a deep immutable snapshot. Mutating it may affect subsequent resolution. This is a pre-1.0 design boundary to decide explicitly, not a recommendation to mutate a live context.

`ErrorCatalogRuntimeStatus` has nine public `init` properties and two getter-only computed properties, `State` and `IsConsistent`. Its own fields are not publicly settable after initialization; it is recorded as a new status instance when activation succeeds. Its semantic state combinations already have dedicated runtime tests. The focused API shape tests live in `WhenItFails.Tests/PublicApi/RuntimeStatePublicApiContractTests.cs`.

Neither status nor context is declared to have a frozen JSON wire format by this baseline. The tests are pending maintainer verification, and no production changes have been made.

## Still under review

`WhenItFailsOptions` and `JsonsOptions` still need a separate configuration contract review. The context mutability decision above remains open before 1.0.

No production visibility, names, signatures, or runtime behavior have been changed by this checkpoint.

## Verification

The maintainer confirmed the first two entry-point contract tests and complete 1159/1159 suite GREEN after commit `f9065aee1322993942b6ae0ac50aecac3dcbb3b2`, and the next four error-model tests and complete 1163/1163 suite GREEN after commit `a6eaba33dad832f7a85d7168fbf0ff431979791c`.
