# Implementation status

Last updated: 2026-09-17

This file is the continuation point for `WhenItFails` development. Git history contains the detailed chronological checkpoints; keep this file focused on the current verified state, established contracts, and next step.

## Current focus

Hardening dependency boundaries and malformed-context handling while preserving established public exception contracts.

## Current verified state

- Complete `WhenItFails.Tests` suite: **1037/1037 GREEN**.
- `ErrorDescriptorResolver` and `ErrorDescriptorService` hardening are complete for the current scope.
- `ErrorCatalogProvider` and `CatalogProviderPipeline` dependency-boundary audits are complete for the current scope.
- `BuiltInErrorCatalogContextProvider` dependency-boundary audit is complete for the current scope.
- `ErrorCatalogInitializer` bootstrapper/context-provider ordinary-exception, cancellation, null-response and null-task behavior is complete for the current scope.
- `ErrorCatalogRuntime` initializer and both built-in-provider runtime paths are complete for null response, ordinary exception, null task and exact cancellation behavior.
- Direct `JsonsBootstrapper` → `IJsonsTemplateProvider.GetTemplateFiles(...)` boundary is complete for malformed results, ordinary-exception normalization and exact-instance cancellation propagation.
- `JsonCatalogDocumentWriter` serialization-failure temporary-file cleanup and deterministic pre-cancellation behavior are verified.
- `ErrorProfileSelectionService` → `IErrorProfileResolver.Resolve(...)` boundary is complete for null result, ordinary-exception normalization and exact-instance cancellation propagation.
- `ErrorProfileSelectionService` classifies null `ErrorCatalogDocument.Errors`, null error definitions, null `ErrorDefinition.Subcategories`, and all resolver-consumed profile collections (`IncludeOwners`, `IncludeCodeGroups`, `IncludeCategories`, `IncludeSubcategories`, `IncludeTags`, `ExcludeTags`, `IncludeErrors`, `ExcludeErrors`) as malformed input rather than resolver failure.

## 2026-09-17 — 1037/1037 GREEN null error-subcategories checkpoint

Contract commit:
`d994cf77ab602afbef1daa3645230202be8863c3`

Production guard commit:
`986743006a72549ea7c8be7cc15066682da11d86`

Previous checkpoint commit:
`bb833a5382315b88b054c90dca6316d24dd0ddda`

Locally verified:

```text
WhenItFails.Tests
Failed:   0
Passed: 1037
Skipped:  0
Total:  1037
```

`ErrorProfileSelectionService.ResolveByProfileName(...)` rejects an `ErrorDefinition` whose `Subcategories` collection is null before invoking the resolver and reuses the established validator contract:

```text
Status: Invalid
Data: null
Code: ErrorSubcategoriesCollectionIsNull
Message: Error subcategories collection is null.
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

## Reconnaissance notes

The remaining directly resolver-consumed nested error collection is `ErrorDefinition.Tags`. `ErrorCatalogValidator` defines the stable malformed contract:

```text
ErrorTagsCollectionIsNull
Error tags collection is null.
```

Keep malformed `ErrorCatalogDocument` state isolated from runtime `ErrorCatalog` construction. To force the resolver to consume a null `Tags` collection, use a non-empty profile `IncludeTags` filter.

## Next recommended step

Add one focused `ErrorProfileSelectionService` contract for `ErrorDefinition.Tags = null`. Production must remain unchanged until the focused run confirms the current behavior.
