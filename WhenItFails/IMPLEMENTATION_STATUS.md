# Implementation status

Last updated: 2026-09-17

This file is the continuation point for `WhenItFails` development. Git history contains the detailed chronological checkpoints; keep this file focused on the current verified state, established contracts, and next step.

## Current focus

Hardening dependency boundaries and malformed-context handling while preserving established public exception contracts.

## Current verified state

- Complete `WhenItFails.Tests` suite: **1038/1038 GREEN**.
- `ErrorDescriptorResolver` and `ErrorDescriptorService` hardening are complete for the current scope.
- `ErrorCatalogProvider` and `CatalogProviderPipeline` dependency-boundary audits are complete for the current scope.
- `BuiltInErrorCatalogContextProvider` dependency-boundary audit is complete for the current scope.
- `ErrorCatalogInitializer` bootstrapper/context-provider ordinary-exception, cancellation, null-response and null-task behavior is complete for the current scope.
- `ErrorCatalogRuntime` initializer and both built-in-provider runtime paths are complete for null response, ordinary exception, null task and exact cancellation behavior.
- Direct `JsonsBootstrapper` → `IJsonsTemplateProvider.GetTemplateFiles(...)` boundary is complete for malformed results, ordinary-exception normalization and exact-instance cancellation propagation.
- `JsonCatalogDocumentWriter` serialization-failure temporary-file cleanup and deterministic pre-cancellation behavior are verified.
- `ErrorProfileSelectionService` → `IErrorProfileResolver.Resolve(...)` boundary is complete for null result, ordinary-exception normalization and exact-instance cancellation propagation.
- `ErrorProfileSelectionService` classifies null `ErrorCatalogDocument.Errors`, null error definitions, null `ErrorDefinition.Subcategories`, null `ErrorDefinition.Tags`, and all resolver-consumed profile collections (`IncludeOwners`, `IncludeCodeGroups`, `IncludeCategories`, `IncludeSubcategories`, `IncludeTags`, `ExcludeTags`, `IncludeErrors`, `ExcludeErrors`) as malformed input rather than resolver failure.

## 2026-09-17 — 1038/1038 GREEN null error-tags checkpoint

Contract commit:
`8275c809bb21a9c9a7d9c46e5c66c01bf8507bc8`

Production guard commit:
`5e2ab50603ea623ff71a7dbef4d64336702c6981`

Previous checkpoint commit:
`2823891cec007edbc331370ce7f3cef97bd8420c`

Locally verified:

```text
WhenItFails.Tests
Failed:   0
Passed: 1038
Skipped:  0
Total:  1038
```

`ErrorProfileSelectionService.ResolveByProfileName(...)` rejects an `ErrorDefinition` whose `Tags` collection is null before invoking the resolver and reuses the established validator contract:

```text
Status: Invalid
Data: null
Code: ErrorTagsCollectionIsNull
Message: Error tags collection is null.
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

- 1035/1035 — profile selection null exclude-errors classification complete; malformed resolver-consumed profile collection series closed.
- 1036/1036 — profile selection null error-definition classification complete.
- 1037/1037 — profile selection null error-subcategories classification complete.
- 1038/1038 — profile selection null error-tags classification complete.

## Reconnaissance notes

Fresh resolver-shape reconnaissance found one remaining resolver-consumed `ErrorDefinition` collection not yet classified explicitly by `ErrorProfileSelectionService`:

- `ErrorDefinition.Categories`

`ErrorProfileResolver.MatchesCategory(...)` reads `error.Categories` whenever `IncludeCategories` is non-empty and the primary category does not match. `ErrorCatalogValidator` and `ErrorCatalogCrossValidator` already define the stable malformed contract:

```text
ErrorCategoriesCollectionIsNull
Error categories collection is null.
```

Any focused contract must make `PrimaryCategory` differ from the included category so resolver short-circuiting does not bypass the malformed `Categories` collection. Keep the malformed document isolated from runtime `ErrorCatalog` construction.

## Next recommended step

Add one focused `ErrorProfileSelectionService` contract for `ErrorDefinition.Categories = null`, with production intentionally unchanged until the focused RED run confirms the current response shape.
