# Implementation status

Last updated: 2026-09-10

This file is the continuation point for `WhenItFails` development. Update it after every implementation, test, catalog, or documentation change that alters the current state or recommended next step.

## Current focus

Hardening dependency boundaries while preserving established public exception contracts.

## Current state

- `ErrorDescriptorResolver` and `ErrorDescriptorService` exception/cancellation hardening are complete for the current scope.
- `ErrorCatalogRuntime` descriptor-service and profile-selection exception/cancellation boundaries are complete.
- All currently known `IErrorCatalogContextStore` read/write boundaries in the active runtime/initializer scope are complete.
- `IJsonsBootstrapper.EnsureWorkspaceAsync(...)` is complete for the current scope.
- `IErrorCatalogContextProvider.LoadFromJsonsAsync(...)` as consumed by `ErrorCatalogInitializer` is complete for the current scope.
- `ErrorCatalogContextProvider` is intentionally a transparent orchestration boundary for exceptions thrown by its five internal catalog providers; do not normalize them there.
- `IErrorCatalogLoader.LoadFromFileAsync(...)` is complete for the current scope.
- `IErrorCatalogDocumentNormalizer.Normalize(...)` is complete for the current scope.
- `IErrorCatalogValidator.Validate(...)` is complete for the current scope.
- `IErrorCatalogFactory.Create(...)` ordinary-exception normalization is locally verified GREEN.
- The complete `WhenItFails.Tests` suite is locally verified **GREEN at 1001/1001 tests with zero compiler warnings** before the factory cancellation contract.
- The only remaining contract in the current `ErrorCatalogProvider` dependency audit is exact-instance factory cancellation propagation.

## 2026-09-10 — 1001/1001 GREEN factory exception checkpoint

Checkpoint commit: this commit.

Factory production fix commit: `51444e6cb6be6df360d3861f4b251d1c1b8b8c88`.
Factory ordinary-exception contract commit: `da315a5d826177cdca2c3e773a42bd2f7b84db4a`.

Verified locally:

```text
WhenItFails.Tests
Failed:   0
Passed: 1001
Skipped:  0
Total:  1001
Compiler warnings: 0
```

`IErrorCatalogFactory.Create(...)` ordinary exceptions normalize to:

```text
Status: Failed
Data: null
Code: WIF_ERROR_CATALOG_FACTORY_FAILED
Message: The error catalog factory failed.
```

The factory guard excludes `OperationCanceledException`, so exact-instance cancellation is expected to propagate unchanged and will be locked by a separate contract.

## Completed `ErrorCatalogProvider` dependency boundaries so far

- `IErrorCatalogLoader.LoadFromFileAsync(...)` — null response, ordinary exception normalization, exact cancellation propagation.
- `IErrorCatalogDocumentNormalizer.Normalize(...)` — null result, ordinary exception normalization, exact cancellation propagation.
- `IErrorCatalogValidator.Validate(...)` — null result, ordinary exception normalization, exact cancellation propagation.
- `IErrorCatalogFactory.Create(...)` — null result and ordinary exception normalization complete; exact cancellation is the final pending contract.

## Established transparent boundary — do not normalize

`ErrorCatalogContextProvider.LoadFromJsonsAsync(...)` must preserve exceptions from its five provider dependencies.

Pre-existing tests require preservation of synchronous and faulted-task exception identity, exception type, inner exception references, `Exception.Data`, custom properties, cancellation information, and short-circuit behavior.

Relevant suites:

- `WhenItFails.Tests/Catalog/ErrorCatalogContextProviderProviderExceptionPropagationTests.cs`
- `WhenItFails.Tests/Catalog/ErrorCatalogContextProviderExceptionShapeTests.cs`
- `WhenItFails.Tests/Catalog/ErrorCatalogContextProviderCancellationPropagationTests.cs`

## `ErrorCatalogProvider` boundary map

`ErrorCatalogProvider.LoadFromFileAsync(...)` composes:

1. `IErrorCatalogLoader.LoadFromFileAsync(...)` — complete for current scope.
2. `IErrorCatalogDocumentNormalizer.Normalize(...)` — complete for current scope.
3. `IErrorCatalogValidator.Validate(...)` — complete for current scope.
4. `IErrorCatalogFactory.Create(...)` — ordinary exception verified; exact-instance cancellation pending.

Existing malformed-result handling:

- null loader response → `WIF_ERROR_CATALOG_LOADER_RESPONSE_NULL`;
- null normalizer result → `WIF_ERROR_CATALOG_NORMALIZER_RESULT_NULL`;
- null validator result → `WIF_ERROR_CATALOG_VALIDATOR_RESULT_NULL`;
- null factory result → `WIF_ERROR_CATALOG_FACTORY_RESULT_NULL`.

## Verification state

- Clean continuation baseline: **1001/1001 GREEN, zero compiler warnings**.
- Factory null-result and ordinary-exception behavior are covered and locally verified.
- No production change is expected for the exact-instance cancellation contract.
- Expected complete-suite count after the final factory cancellation contract passes: **1002 tests**.

## Next recommended step

Add one focused exact-instance cancellation contract for `IErrorCatalogFactory.Create(...)` using `Assert.Same(...)`.

If it passes without production changes, close the `ErrorCatalogProvider` dependency boundary audit for the current scope and perform fresh reconnaissance before selecting the next normalization boundary.

Keep changes small, tested, documented here, and committed directly to `master`.
