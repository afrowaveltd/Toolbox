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
- The null `ErrorDefinition.Subcategories` RED is locally confirmed and the smallest production guard is committed; focused/full GREEN verification is pending.

## 2026-09-17 — profile selection null error-subcategories fix

Contract commit:
`d994cf77ab602afbef1daa3645230202be8863c3`

Production guard commit:
`986743006a72549ea7c8be7cc15066682da11d86`

Baseline checkpoint commit:
`bb833a5382315b88b054c90dca6316d24dd0ddda`

Contract:

`ResolveByProfileName_WhenErrorSubcategoriesCollectionIsNull_ReturnsInvalidResponse`

`ErrorCatalogValidator` defines the stable malformed-error contract:

```text
Status: Invalid
Data: null
Code: ErrorSubcategoriesCollectionIsNull
Message: Error subcategories collection is null.
```

The isolated focused run confirmed the expected selection-service RED:

```text
Expected: Invalid
Actual:   Failed
```

The malformed state exists only in `ErrorCatalogDocument`; runtime `context.ErrorCatalog` is an independent valid empty catalog, and the profile uses a non-empty `IncludeSubcategories` filter so the resolver necessarily consumes the null collection.

The null `ErrorDefinition.Subcategories` collection reached `ErrorProfileResolver.Resolve(...)` and was normalized by the dependency exception boundary as `Failed / WIF_PROFILE_RESOLVER_FAILED`.

Production change in `WhenItFails/Resolution/ErrorProfileSelectionService.cs` is intentionally minimal: after rejecting null error definitions, the service now rejects any error definition whose `Subcategories` collection is null and reuses the established validator contract above.

No resolver exception, cancellation, profile lookup, profile collection, or unrelated response behavior was changed.

Expected complete-suite count after verification: **1037/1037 GREEN with zero compiler warnings**.

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

Pull current `master` and run:

```powershell
dotnet test WhenItFails.Tests --filter "FullyQualifiedName~ResolveByProfileName_WhenErrorSubcategoriesCollectionIsNull_ReturnsInvalidResponse"
dotnet test WhenItFails.Tests
```

Expected results:

```text
Focused contract: GREEN
Complete suite: 1037/1037 GREEN
Compiler warnings: 0
```

## Next recommended step

After **1037/1037 GREEN** is confirmed, record the checkpoint and continue with `ErrorDefinition.Tags = null`, reusing the stable validator contract `ErrorTagsCollectionIsNull / Error tags collection is null.`.
