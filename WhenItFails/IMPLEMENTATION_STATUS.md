# Implementation status

Last updated: 2026-09-15

This file is the continuation point for `WhenItFails` development. Git history contains the detailed chronological checkpoints; keep this file focused on the current verified state, established contracts, and next step.

## Current focus

Hardening dependency boundaries and malformed-context handling while preserving established public exception contracts.

## Current verified state

- Complete `WhenItFails.Tests` suite: **1031/1031 GREEN, zero compiler warnings**.
- `ErrorDescriptorResolver` and `ErrorDescriptorService` hardening are complete for the current scope.
- `ErrorCatalogProvider` and `CatalogProviderPipeline` dependency-boundary audits are complete for the current scope.
- `BuiltInErrorCatalogContextProvider` dependency-boundary audit is complete for the current scope.
- `ErrorCatalogInitializer` bootstrapper/context-provider ordinary-exception, cancellation, null-response and null-task behavior is complete for the current scope.
- `ErrorCatalogRuntime` initializer and both built-in-provider runtime paths are complete for null response, ordinary exception, null task and exact cancellation behavior.
- Direct `JsonsBootstrapper` → `IJsonsTemplateProvider.GetTemplateFiles(...)` boundary is complete for malformed results, ordinary-exception normalization and exact-instance cancellation propagation.
- `JsonCatalogDocumentWriter` serialization-failure temporary-file cleanup and deterministic pre-cancellation behavior are verified.
- `ErrorProfileSelectionService` → `IErrorProfileResolver.Resolve(...)` boundary is complete for null result, ordinary-exception normalization and exact-instance cancellation propagation.
- `ErrorProfileSelectionService` classifies null `ErrorCatalogDocument.Errors`, `IncludeOwners`, `IncludeCodeGroups`, `IncludeCategories`, and `IncludeSubcategories` as malformed input rather than resolver failure.

## 2026-09-15 — 1031/1031 GREEN null include-subcategories checkpoint

Malformed-profile contract commit:
`f796cfda94f8cf357cf91fa3e7491010a232df54`

Production guard commit:
`5005b7f243335bc0819078a6690870d469e25276`

Previous checkpoint commit:
`0ec11bb829dcb0a6b5eab2460a46e9c6de1f2a4c`

Locally verified:

```text
WhenItFails.Tests
Failed:   0
Passed: 1031
Skipped:  0
Total:  1031
Compiler warnings: 0
```

`ErrorProfileSelectionService.ResolveByProfileName(...)` now rejects a null `ErrorProfileDefinition.IncludeSubcategories` collection before invoking the resolver and reuses the established validator contract:

```text
Status: Invalid
Data: null
Code: ProfileIncludeSubcategoriesCollectionIsNull
Message: Profile include subcategories collection is null.
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

- 1028/1028 — profile selection null include-owners classification complete.
- 1029/1029 — profile selection null include-code-groups classification complete.
- 1030/1030 — profile selection null include-categories classification complete.
- 1031/1031 — profile selection null include-subcategories classification complete.

## Reconnaissance notes

Remaining resolver-consumed profile collections still without `ErrorProfileSelectionService` malformed-profile coverage:

- `IncludeTags`
- `ExcludeTags`
- `IncludeErrors`
- `ExcludeErrors`

`ErrorProfileCatalogValidator` already defines stable codes/messages for each null collection. Continue one collection at a time so each RED/GREEN step stays explicit and reviewable.

## Next recommended step

Add one focused contract for `ErrorProfileDefinition.IncludeTags = null` in `ErrorProfileSelectionService`. Expected stable malformed-profile response:

```text
Status: Invalid
Code: ProfileIncludeTagsCollectionIsNull
Message: Profile include tags collection is null.
```

Production should remain unchanged until the focused RED run confirms the current misclassification as `Failed / WIF_PROFILE_RESOLVER_FAILED`.
