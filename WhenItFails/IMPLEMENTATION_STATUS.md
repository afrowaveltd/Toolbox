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
- A focused contract for null `ErrorDefinition.Categories` is committed and awaits local RED verification.

## 2026-09-17 — profile selection null error-categories contract

Contract commit:
`f34a2c0a3c90920771db962a7140ab881af53b2a`

Baseline checkpoint commit:
`acccd3a341460dbfc7be98206fdec586709510aa`

Added:

`WhenItFails.Tests/Resolution/ErrorProfileSelectionServiceNullErrorCategoriesCollectionContractTests.cs`

Contract:

`ResolveByProfileName_WhenErrorCategoriesCollectionIsNull_ReturnsInvalidResponse`

`ErrorCatalogValidator` and `ErrorCatalogCrossValidator` already define the stable malformed-error contract:

```text
Status: Invalid
Code: ErrorCategoriesCollectionIsNull
Message: Error categories collection is null.
```

The fixture is deliberately isolated:

- malformed state exists only in `ErrorCatalogDocument` (`ErrorDefinition.Categories = null!`),
- runtime `context.ErrorCatalog` is an independent valid empty catalog,
- profile uses `IncludeCategories = ["NETWORK"]`,
- `PrimaryCategory = "OTHER"`, so the primary-category short-circuit cannot bypass the malformed `Categories` collection.

Production is intentionally unchanged before the focused run. Current `ErrorProfileResolver.MatchesCategory(...)` should attempt to enumerate the null `Categories` collection, causing an ordinary exception that `ErrorProfileSelectionService` normalizes as `Failed / WIF_PROFILE_RESOLVER_FAILED`.

Expected eventual complete-suite count after this contract passes: **1039/1039 GREEN**.

## 2026-09-17 — 1038/1038 GREEN null error-tags checkpoint

Contract commit:
`8275c809bb21a9c9a7d9c46e5c66c01bf8507bc8`

Production guard commit:
`5e2ab50603ea623ff71a7dbef4d64336702c6981`

Checkpoint commit:
`acccd3a341460dbfc7be98206fdec586709510aa`

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

- 1036/1036 — profile selection null error-definition classification complete.
- 1037/1037 — profile selection null error-subcategories classification complete.
- 1038/1038 — profile selection null error-tags classification complete.

## Reconnaissance notes

`ErrorDefinition.Categories` is the remaining collection directly consumed by `ErrorProfileResolver` that is not yet explicitly preclassified by `ErrorProfileSelectionService`. After this contract is resolved, perform fresh reconnaissance across scalar fields and other boundaries instead of assuming another collection guard is needed.

## Recommended verification

Pull current `master` and run only the focused contract:

```powershell
dotnet test WhenItFails.Tests --filter "FullyQualifiedName~ResolveByProfileName_WhenErrorCategoriesCollectionIsNull_ReturnsInvalidResponse"
```

Expected current result: **RED** with `Actual: Failed` rather than the expected `Invalid`.

## Next recommended step

If RED confirms the response-shape mismatch, add the smallest `ErrorProfileSelectionService` guard for null `ErrorDefinition.Categories`, reusing exactly:

```text
ErrorCategoriesCollectionIsNull
Error categories collection is null.
```

Then run focused and complete suites. Record **1039/1039 GREEN** before fresh reconnaissance.
