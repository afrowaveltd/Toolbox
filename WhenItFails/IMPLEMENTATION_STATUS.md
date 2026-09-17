# Implementation status

Last updated: 2026-09-17

This file is the continuation point for `WhenItFails` development. Git history contains the detailed chronological checkpoints; keep this file focused on the current verified state, established contracts, and next step.

## Current focus

Hardening dependency boundaries and malformed-context handling while preserving established public exception contracts.

## Current verified state

- Complete `WhenItFails.Tests` suite baseline: **1035/1035 GREEN, zero compiler warnings**.
- `ErrorDescriptorResolver` and `ErrorDescriptorService` hardening are complete for the current scope.
- `ErrorCatalogProvider` and `CatalogProviderPipeline` dependency-boundary audits are complete for the current scope.
- `BuiltInErrorCatalogContextProvider` dependency-boundary audit is complete for the current scope.
- `ErrorCatalogInitializer` bootstrapper/context-provider ordinary-exception, cancellation, null-response and null-task behavior is complete for the current scope.
- `ErrorCatalogRuntime` initializer and both built-in-provider runtime paths are complete for null response, ordinary exception, null task and exact cancellation behavior.
- Direct `JsonsBootstrapper` → `IJsonsTemplateProvider.GetTemplateFiles(...)` boundary is complete for malformed results, ordinary-exception normalization and exact-instance cancellation propagation.
- `JsonCatalogDocumentWriter` serialization-failure temporary-file cleanup and deterministic pre-cancellation behavior are verified.
- `ErrorProfileSelectionService` → `IErrorProfileResolver.Resolve(...)` boundary is complete for null result, ordinary-exception normalization and exact-instance cancellation propagation.
- `ErrorProfileSelectionService` now classifies null `ErrorCatalogDocument.Errors` and all resolver-consumed profile collections (`IncludeOwners`, `IncludeCodeGroups`, `IncludeCategories`, `IncludeSubcategories`, `IncludeTags`, `ExcludeTags`, `IncludeErrors`, `ExcludeErrors`) as malformed input rather than resolver failure.
- The isolated null `ErrorDefinition` contract is locally confirmed RED and the smallest production guard is committed; focused/full GREEN verification is pending.

## 2026-09-17 — profile selection null error-definition fix

Original contract commit:
`ce50bfcd9dfd6fc09b9be32ca16b28eb06323cd7`

Fixture-isolation commit:
`e678281dfac0c83232de700f2dec431a8f01bcde`

Production guard commit:
`295c49c7f2bd39132f864f6f5d3eb77d07a333d9`

Baseline checkpoint commit:
`471f7366b55e927ec73afb436f6fbe3c10773fbf`

Contract:

`ResolveByProfileName_WhenErrorCatalogContainsNullDefinition_ReturnsInvalidResponse`

`ErrorCatalogValidator` defines the stable malformed-error contract:

```text
Status: Invalid
Data: null
Code: ErrorDefinitionIsNull
Message: Error catalog contains a null error definition.
```

The first local run was not relevant to the selection-service boundary because the fixture built the runtime `ErrorCatalog` from the malformed document; `ErrorCatalog.BuildIndexes()` threw before `ErrorProfileSelectionService` ran. The fixture was therefore isolated so the malformed state exists only in `ErrorCatalogDocument`, while `context.ErrorCatalog` is an independent valid empty runtime catalog.

The isolated focused run then confirmed the expected selection-service RED:

```text
Expected: Invalid
Actual:   Failed
```

The null `ErrorDefinition` reached `ErrorProfileResolver.Resolve(...)` and was normalized by the dependency exception boundary as `Failed / WIF_PROFILE_RESOLVER_FAILED`.

Production change in `WhenItFails/Resolution/ErrorProfileSelectionService.cs` is intentionally minimal: immediately after validating that `ErrorCatalogDocument.Errors` itself is non-null, the service now rejects any null error definition before invoking the resolver and reuses the existing validator contract above.

No resolver exception, cancellation, profile lookup, profile collection, or other response behavior was changed.

Expected complete-suite count after verification: **1036/1036 GREEN with zero compiler warnings**.

## 2026-09-15 — 1035/1035 GREEN malformed profile collection series complete

Final malformed-profile contract commit:
`66662fea33e04940e197b2f5550921cfe333b6a1`

Final production guard commit:
`40bf621b8925cc923460890dd0652a1070e2909f`

Checkpoint commit:
`471f7366b55e927ec73afb436f6fbe3c10773fbf`

Locally verified:

```text
WhenItFails.Tests
Failed:   0
Passed: 1035
Skipped:  0
Total:  1035
Compiler warnings: 0
```

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

- 1032/1032 — profile selection null include-tags classification complete.
- 1033/1033 — profile selection null exclude-tags classification complete.
- 1034/1034 — profile selection null include-errors classification complete.
- 1035/1035 — profile selection null exclude-errors classification complete; malformed resolver-consumed profile collection series closed.

## Reconnaissance notes

`ErrorProfileResolver` also consumes each non-null `ErrorDefinition`'s `Subcategories` and `Tags` collections. `ErrorCatalogValidator` already defines stable malformed contracts for these null collections:

```text
ErrorSubcategoriesCollectionIsNull
Error subcategories collection is null.

ErrorTagsCollectionIsNull
Error tags collection is null.
```

Continue one concrete contract at a time after the null-error-definition fix is verified GREEN.

## Recommended verification

Pull current `master` and run:

```powershell
dotnet test WhenItFails.Tests --filter "FullyQualifiedName~ResolveByProfileName_WhenErrorCatalogContainsNullDefinition_ReturnsInvalidResponse"
dotnet test WhenItFails.Tests
```

Expected results:

```text
Focused contract: GREEN
Complete suite: 1036/1036 GREEN
Compiler warnings: 0
```

## Next recommended step

After **1036/1036 GREEN** is confirmed, record the checkpoint and continue with one nested resolver-consumed error collection at a time, starting with `ErrorDefinition.Subcategories = null`.
