# Implementation status

Last updated: 2026-09-17

This file is the continuation point for `WhenItFails` development. Git history contains the detailed chronological checkpoints; keep this file focused on the current verified state, established contracts, and next step.

## Current focus

Hardening dependency boundaries and malformed-context handling while preserving established public exception contracts.

## Current verified state

- Complete `WhenItFails.Tests` suite baseline: **1038/1038 GREEN**.
- `ErrorDescriptorResolver` and `ErrorDescriptorService` hardening are complete for the current scope.
- `ErrorCatalogProvider` and `CatalogProviderPipeline` dependency-boundary audits are complete for the current scope.
- `BuiltInErrorCatalogContextProvider` dependency-boundary audit is complete for the current scope.
- `ErrorCatalogInitializer` bootstrapper/context-provider ordinary-exception, cancellation, null-response and null-task behavior is complete for the current scope.
- `ErrorCatalogRuntime` initializer and both built-in-provider runtime paths are complete for null response, ordinary exception, null task and exact cancellation behavior.
- Direct `JsonsBootstrapper` → `IJsonsTemplateProvider.GetTemplateFiles(...)` boundary is complete for malformed results, ordinary-exception normalization and exact-instance cancellation propagation.
- `JsonCatalogDocumentWriter` serialization-failure temporary-file cleanup and deterministic pre-cancellation behavior are verified.
- `ErrorProfileSelectionService` → `IErrorProfileResolver.Resolve(...)` boundary is complete for null result, ordinary-exception normalization and exact-instance cancellation propagation.
- `ErrorProfileSelectionService` classifies null `ErrorCatalogDocument.Errors`, null error definitions, null `ErrorDefinition.Subcategories`, null `ErrorDefinition.Tags`, and all resolver-consumed profile collections (`IncludeOwners`, `IncludeCodeGroups`, `IncludeCategories`, `IncludeSubcategories`, `IncludeTags`, `ExcludeTags`, `IncludeErrors`, `ExcludeErrors`) as malformed input rather than resolver failure.
- The isolated null `ErrorDefinition.Categories` contract is locally confirmed RED and the smallest production guard is committed; focused/full GREEN verification is pending.

## 2026-09-17 — profile selection null error-categories fix

Contract commit:
`f34a2c0a3c90920771db962a7140ab881af53b2a`

Production guard commit:
`2872fce9108a85ebb76d0836c426df523ae1dbd9`

Baseline checkpoint commit:
`acccd3a341460dbfc7be98206fdec586709510aa`

Contract:

`ResolveByProfileName_WhenErrorCategoriesCollectionIsNull_ReturnsInvalidResponse`

`ErrorCatalogValidator` and `ErrorCatalogCrossValidator` define the stable malformed-error contract:

```text
Status: Invalid
Data: null
Code: ErrorCategoriesCollectionIsNull
Message: Error categories collection is null.
```

The isolated focused run confirmed the expected selection-service RED:

```text
Expected: Invalid
Actual:   Failed
```

The malformed state exists only in `ErrorCatalogDocument`; runtime `context.ErrorCatalog` is an independent valid empty catalog. The profile uses `IncludeCategories = ["NETWORK"]` while `PrimaryCategory = "OTHER"`, so `ErrorProfileResolver.MatchesCategory(...)` necessarily consumes the null `Categories` collection.

The null `ErrorDefinition.Categories` collection therefore reached `ErrorProfileResolver.Resolve(...)` and was normalized by the dependency exception boundary as `Failed / WIF_PROFILE_RESOLVER_FAILED`.

Production change in `WhenItFails/Resolution/ErrorProfileSelectionService.cs` is intentionally minimal: after rejecting null error definitions, the service now rejects any error definition whose `Categories` collection is null and reuses the established validator contract above.

No resolver exception, cancellation, profile lookup, profile collection, or unrelated response behavior was changed.

Expected complete-suite count after verification: **1039/1039 GREEN**.

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

## Recommended verification

Pull current `master` and run:

```powershell
dotnet test WhenItFails.Tests --filter "FullyQualifiedName~ResolveByProfileName_WhenErrorCategoriesCollectionIsNull_ReturnsInvalidResponse"
dotnet test WhenItFails.Tests
```

Expected results:

```text
Focused contract: GREEN
Complete suite: 1039/1039 GREEN
```

## Next recommended step

After **1039/1039 GREEN** is confirmed, record the checkpoint and perform fresh reconnaissance across remaining scalar-field and dependency boundaries rather than adding further collection guards speculatively.
