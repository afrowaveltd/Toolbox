# Implementation status

Last updated: 2026-09-17

This file is the continuation point for `WhenItFails` development. Git history contains the detailed chronological checkpoints; keep this file focused on the current verified state, established contracts, and next step.

## Current focus

Hardening dependency boundaries and malformed-context handling while preserving established public exception contracts.

## Current verified state

- Complete `WhenItFails.Tests` suite baseline: **1036/1036 GREEN, zero compiler warnings**.
- `ErrorDescriptorResolver` and `ErrorDescriptorService` hardening are complete for the current scope.
- `ErrorCatalogProvider` and `CatalogProviderPipeline` dependency-boundary audits are complete for the current scope.
- `BuiltInErrorCatalogContextProvider` dependency-boundary audit is complete for the current scope.
- `ErrorCatalogInitializer` bootstrapper/context-provider ordinary-exception, cancellation, null-response and null-task behavior is complete for the current scope.
- `ErrorCatalogRuntime` initializer and both built-in-provider runtime paths are complete for null response, ordinary exception, null task and exact cancellation behavior.
- Direct `JsonsBootstrapper` → `IJsonsTemplateProvider.GetTemplateFiles(...)` boundary is complete for malformed results, ordinary-exception normalization and exact-instance cancellation propagation.
- `JsonCatalogDocumentWriter` serialization-failure temporary-file cleanup and deterministic pre-cancellation behavior are verified.
- `ErrorProfileSelectionService` → `IErrorProfileResolver.Resolve(...)` boundary is complete for null result, ordinary-exception normalization and exact-instance cancellation propagation.
- `ErrorProfileSelectionService` classifies null `ErrorCatalogDocument.Errors`, null error definitions, and all resolver-consumed profile collections (`IncludeOwners`, `IncludeCodeGroups`, `IncludeCategories`, `IncludeSubcategories`, `IncludeTags`, `ExcludeTags`, `IncludeErrors`, `ExcludeErrors`) as malformed input rather than resolver failure.
- A focused contract for null `ErrorDefinition.Subcategories` is committed and awaits local RED verification.

## 2026-09-17 — profile selection null error-subcategories contract

Contract commit:
`d994cf77ab602afbef1daa3645230202be8863c3`

Baseline checkpoint commit:
`bb833a5382315b88b054c90dca6316d24dd0ddda`

Added:

`WhenItFails.Tests/Resolution/ErrorProfileSelectionServiceNullErrorSubcategoriesCollectionContractTests.cs`

Contract:

`ResolveByProfileName_WhenErrorSubcategoriesCollectionIsNull_ReturnsInvalidResponse`

`ErrorCatalogValidator` already defines the stable malformed-error contract:

```text
Status: Invalid
Code: ErrorSubcategoriesCollectionIsNull
Message: Error subcategories collection is null.
```

The fixture is deliberately isolated:

- malformed state exists only in `ErrorCatalogDocument` (`ErrorDefinition.Subcategories = null!`),
- runtime `context.ErrorCatalog` is an independent valid empty catalog,
- the profile uses `IncludeSubcategories = ["NETWORK"]` so resolver short-circuiting cannot bypass the malformed collection.

Production is intentionally unchanged before the focused run. Current `ErrorProfileResolver` should enumerate the null `Subcategories` collection when applying the non-empty subcategory filter, causing an ordinary exception that `ErrorProfileSelectionService` normalizes as `Failed / WIF_PROFILE_RESOLVER_FAILED`.

Expected eventual complete-suite count after this contract passes: **1037/1037 GREEN with zero compiler warnings**.

## 2026-09-17 — 1036/1036 GREEN null error-definition checkpoint

Contract commit:
`ce50bfcd9dfd6fc09b9be32ca16b28eb06323cd7`

Fixture-isolation commit:
`e678281dfac0c83232de700f2dec431a8f01bcde`

Production guard commit:
`295c49c7f2bd39132f864f6f5d3eb77d07a333d9`

Checkpoint commit:
`bb833a5382315b88b054c90dca6316d24dd0ddda`

Locally verified:

```text
WhenItFails.Tests
Failed:   0
Passed: 1036
Skipped:  0
Total:  1036
Compiler warnings: 0
```

`ErrorProfileSelectionService.ResolveByProfileName(...)` rejects a null `ErrorDefinition` before invoking the resolver and reuses the established validator contract:

```text
Status: Invalid
Data: null
Code: ErrorDefinitionIsNull
Message: Error catalog contains a null error definition.
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

- 1034/1034 — profile selection null include-errors classification complete.
- 1035/1035 — profile selection null exclude-errors classification complete; malformed resolver-consumed profile collection series closed.
- 1036/1036 — profile selection null error-definition classification complete.

## Reconnaissance notes

After `Subcategories`, `ErrorProfileResolver` also consumes each `ErrorDefinition.Tags` collection. `ErrorCatalogValidator` defines the stable malformed tag contract:

```text
ErrorTagsCollectionIsNull
Error tags collection is null.
```

Continue one concrete contract at a time and keep malformed documents isolated from runtime `ErrorCatalog` construction.

## Recommended verification

Pull current `master` and run only the focused contract:

```powershell
dotnet test WhenItFails.Tests --filter "FullyQualifiedName~ResolveByProfileName_WhenErrorSubcategoriesCollectionIsNull_ReturnsInvalidResponse"
```

Expected current result: **RED** with `Actual: Failed` rather than the expected `Invalid`.

## Next recommended step

If RED confirms the response-shape mismatch, add the smallest `ErrorProfileSelectionService` guard for null `ErrorDefinition.Subcategories`, reusing exactly:

```text
ErrorSubcategoriesCollectionIsNull
Error subcategories collection is null.
```

Then run focused and complete suites before proceeding to `ErrorDefinition.Tags = null`.
