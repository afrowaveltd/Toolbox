# Implementation status

Last updated: 2026-09-15

This file is the continuation point for `WhenItFails` development. Git history contains the detailed chronological checkpoints; keep this file focused on the current verified state, established contracts, and next step.

## Current focus

Hardening dependency boundaries and malformed-context handling while preserving established public exception contracts.

## Current verified state

- Complete `WhenItFails.Tests` suite: **1033/1033 GREEN, zero compiler warnings**.
- `ErrorDescriptorResolver` and `ErrorDescriptorService` hardening are complete for the current scope.
- `ErrorCatalogProvider` and `CatalogProviderPipeline` dependency-boundary audits are complete for the current scope.
- `BuiltInErrorCatalogContextProvider` dependency-boundary audit is complete for the current scope.
- `ErrorCatalogInitializer` bootstrapper/context-provider ordinary-exception, cancellation, null-response and null-task behavior is complete for the current scope.
- `ErrorCatalogRuntime` initializer and both built-in-provider runtime paths are complete for null response, ordinary exception, null task and exact cancellation behavior.
- Direct `JsonsBootstrapper` → `IJsonsTemplateProvider.GetTemplateFiles(...)` boundary is complete for malformed results, ordinary-exception normalization and exact-instance cancellation propagation.
- `JsonCatalogDocumentWriter` serialization-failure temporary-file cleanup and deterministic pre-cancellation behavior are verified.
- `ErrorProfileSelectionService` → `IErrorProfileResolver.Resolve(...)` boundary is complete for null result, ordinary-exception normalization and exact-instance cancellation propagation.
- `ErrorProfileSelectionService` classifies null `ErrorCatalogDocument.Errors`, `IncludeOwners`, `IncludeCodeGroups`, `IncludeCategories`, `IncludeSubcategories`, `IncludeTags`, and `ExcludeTags` as malformed input rather than resolver failure.

## 2026-09-15 — 1033/1033 GREEN null exclude-tags checkpoint

Malformed-profile contract commit:
`ed3d0519704d7e17c4e503804fcd02347f0ac6ab`

Production guard commit:
`ea00a602d69924c93e572a71c47fdad7ebb7bfdb`

Previous checkpoint commit:
`15c7112e6917cadaba6a001b21a1732e5cadedfb`

Locally verified:

```text
WhenItFails.Tests
Failed:   0
Passed: 1033
Skipped:  0
Total:  1033
Compiler warnings: 0
```

`ErrorProfileSelectionService.ResolveByProfileName(...)` now rejects a null `ErrorProfileDefinition.ExcludeTags` collection before invoking the resolver and reuses the established validator contract:

```text
Status: Invalid
Data: null
Code: ProfileExcludeTagsCollectionIsNull
Message: Profile exclude tags collection is null.
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

- 1030/1030 — profile selection null include-categories classification complete.
- 1031/1031 — profile selection null include-subcategories classification complete.
- 1032/1032 — profile selection null include-tags classification complete.
- 1033/1033 — profile selection null exclude-tags classification complete.

## Reconnaissance notes

Remaining resolver-consumed profile collections still without `ErrorProfileSelectionService` malformed-profile coverage:

- `IncludeErrors`
- `ExcludeErrors`

`ErrorProfileCatalogValidator` and `ErrorCatalogCrossValidator` already define stable codes/messages for these null collections. Continue one collection at a time so each RED/GREEN step stays explicit and reviewable.

## Next recommended step

Add one focused contract for `ErrorProfileDefinition.IncludeErrors = null` in `ErrorProfileSelectionService`. Expected stable malformed-profile response:

```text
Status: Invalid
Code: ProfileIncludeErrorsCollectionIsNull
Message: Profile include errors collection is null.
```

Production should remain unchanged until the focused RED run confirms the current misclassification as `Failed / WIF_PROFILE_RESOLVER_FAILED`.
