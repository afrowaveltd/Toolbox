# Implementation status

Last updated: 2026-09-15

This file is the continuation point for `WhenItFails` development. Git history contains the detailed chronological checkpoints; keep this file focused on the current verified state, established contracts, and next step.

## Current focus

Hardening dependency boundaries and malformed-context handling while preserving established public exception contracts.

## Current verified state

- Complete `WhenItFails.Tests` suite baseline: **1034/1034 GREEN, zero compiler warnings**.
- `ErrorDescriptorResolver` and `ErrorDescriptorService` hardening are complete for the current scope.
- `ErrorCatalogProvider` and `CatalogProviderPipeline` dependency-boundary audits are complete for the current scope.
- `BuiltInErrorCatalogContextProvider` dependency-boundary audit is complete for the current scope.
- `ErrorCatalogInitializer` bootstrapper/context-provider ordinary-exception, cancellation, null-response and null-task behavior is complete for the current scope.
- `ErrorCatalogRuntime` initializer and both built-in-provider runtime paths are complete for null response, ordinary exception, null task and exact cancellation behavior.
- Direct `JsonsBootstrapper` → `IJsonsTemplateProvider.GetTemplateFiles(...)` boundary is complete for malformed results, ordinary-exception normalization and exact-instance cancellation propagation.
- `JsonCatalogDocumentWriter` serialization-failure temporary-file cleanup and deterministic pre-cancellation behavior are verified.
- `ErrorProfileSelectionService` → `IErrorProfileResolver.Resolve(...)` boundary is complete for null result, ordinary-exception normalization and exact-instance cancellation propagation.
- `ErrorProfileSelectionService` classifies null `ErrorCatalogDocument.Errors`, `IncludeOwners`, `IncludeCodeGroups`, `IncludeCategories`, `IncludeSubcategories`, `IncludeTags`, `ExcludeTags`, and `IncludeErrors` as malformed input rather than resolver failure.

## 2026-09-15 — 1034/1034 GREEN null include-errors checkpoint

Malformed-profile contract commit:
`e21f859aab8154769d088d434fb28ae932529b73`

Production guard commit:
`375a3b2a495c989a9157f12a906decfe45f34746`

Previous checkpoint commit:
`9e876a9f4edb5d51a1bb3157b9597802da43f87c`

Locally verified:

```text
WhenItFails.Tests
Failed:   0
Passed: 1034
Skipped:  0
Total:  1034
Compiler warnings: 0
```

`ErrorProfileSelectionService.ResolveByProfileName(...)` rejects a null `ErrorProfileDefinition.IncludeErrors` collection before invoking the resolver and reuses the established validator/cross-validator contract:

```text
Status: Invalid
Data: null
Code: ProfileIncludeErrorsCollectionIsNull
Message: Profile include errors collection is null.
```

## Established transparent lower boundary — do not normalize

`ErrorCatalogContextProvider.LoadFromJsonsAsync(...)` intentionally preserves exceptions and null-task behavior from its five internal catalog providers.

Relevant suites include:

- `WhenItFails.Tests/Catalog/ErrorCatalogContextProviderProviderExceptionPropagationTests.cs`
- `WhenItFails.Tests/Catalog/ErrorCatalogContextProviderExceptionShapeTests.cs`
- `WhenItFails.Tests/Catalog/ErrorCatalogContextProviderCancellationPropagationTests.cs`
- `WhenItFails.Tests/Catalog/ErrorCatalogContextProviderOwnerNullTaskTests.cs`
- `WhenItFails.Tests/Catalog/ErrorCatalogContextProviderProfileNullTaskTests.cs`

Do not replace those transparent contracts with normalization at that layer.

## Established documented behavior — preserve

`JsonCatalogDocumentLoader.InvalidJson` deliberately includes the JSON parser message. `WhenItFails/Docs/Loading-and-Normalization/en.md` documents this behavior; do not sanitize it as incidental hardening.

## Recent verified checkpoints

- 1031/1031 — profile selection null include-subcategories classification complete.
- 1032/1032 — profile selection null include-tags classification complete.
- 1033/1033 — profile selection null exclude-tags classification complete.
- 1034/1034 — profile selection null include-errors classification complete.

## Reconnaissance notes

The only remaining resolver-consumed profile collection still without `ErrorProfileSelectionService` malformed-profile coverage is:

- `ExcludeErrors`

`ErrorProfileCatalogValidator` and `ErrorCatalogCrossValidator` already define the stable contract:

```text
ProfileExcludeErrorsCollectionIsNull
Profile exclude errors collection is null.
```

## Next recommended step

Add one focused contract for `ErrorProfileDefinition.ExcludeErrors = null` in `ErrorProfileSelectionService`. Production should remain unchanged until the focused RED run confirms the current misclassification as `Failed / WIF_PROFILE_RESOLVER_FAILED`.
