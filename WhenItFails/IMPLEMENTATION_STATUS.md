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

## 2026-09-17 — 1036/1036 GREEN null error-definition checkpoint

Contract commit:
`ce50bfcd9dfd6fc09b9be32ca16b28eb06323cd7`

Fixture-isolation commit:
`e678281dfac0c83232de700f2dec431a8f01bcde`

Production guard commit:
`295c49c7f2bd39132f864f6f5d3eb77d07a333d9`

Previous checkpoint commit:
`471f7366b55e927ec73afb436f6fbe3c10773fbf`

Locally verified:

```text
WhenItFails.Tests
Failed:   0
Passed: 1036
Skipped:  0
Total:  1036
Compiler warnings: 0
```

`ErrorProfileSelectionService.ResolveByProfileName(...)` rejects a null `ErrorDefinition` before invoking the resolver and reuses the established `ErrorCatalogValidator` contract:

```text
Status: Invalid
Data: null
Code: ErrorDefinitionIsNull
Message: Error catalog contains a null error definition.
```

The initial focused test fixture accidentally constructed the runtime `ErrorCatalog` from malformed data and therefore failed in `ErrorCatalog.BuildIndexes()` before reaching the selection service. The isolated fixture keeps malformed data only in `ErrorCatalogDocument` and uses an independent valid runtime catalog; that focused RED then correctly demonstrated `Failed` vs `Invalid` before the production fix.

## 2026-09-15 — 1035/1035 GREEN malformed profile collection series complete

Final malformed-profile contract commit:
`66662fea33e04940e197b2f5550921cfe333b6a1`

Final production guard commit:
`40bf621b8925cc923460890dd0652a1070e2909f`

Checkpoint commit:
`471f7366b55e927ec73afb436f6fbe3c10773fbf`

This closes the malformed resolver-consumed profile collection series.

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

- 1033/1033 — profile selection null exclude-tags classification complete.
- 1034/1034 — profile selection null include-errors classification complete.
- 1035/1035 — profile selection null exclude-errors classification complete; malformed resolver-consumed profile collection series closed.
- 1036/1036 — profile selection null error-definition classification complete.

## Reconnaissance notes

`ErrorProfileResolver` consumes each non-null `ErrorDefinition`'s `Subcategories` and `Tags` collections. `ErrorCatalogValidator` already defines stable malformed contracts:

```text
ErrorSubcategoriesCollectionIsNull
Error subcategories collection is null.

ErrorTagsCollectionIsNull
Error tags collection is null.
```

For a meaningful null-`Subcategories` resolver test, the profile must contain a non-empty `IncludeSubcategories` filter; otherwise resolver short-circuiting does not enumerate the malformed collection. Keep the malformed `ErrorCatalogDocument` isolated from the runtime `ErrorCatalog`, which is intended to contain already validated definitions.

## Next recommended step

Add one focused `ErrorProfileSelectionService` contract for `ErrorDefinition.Subcategories = null`, with a non-empty `IncludeSubcategories` filter and an independent valid runtime `ErrorCatalog`. Production should remain unchanged until the focused run establishes the current behavior.
