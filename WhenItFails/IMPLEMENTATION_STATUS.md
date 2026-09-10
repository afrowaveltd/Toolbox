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
- A focused exact-instance cancellation contract is now committed for `IErrorCatalogFactory.Create(...)` and awaits local verification.

## 2026-09-10 — factory cancellation contract

Contract commit: `ae724530c67bfb02a427f46a9fe9401c763454c5`

Updated:

`WhenItFails.Tests/Catalog/ErrorCatalogProviderFactoryExceptionContractTests.cs`

Added:

`LoadFromFileAsync_WhenFactoryCancels_RethrowsSameOperationCanceledException`

Contract:

```text
IErrorCatalogFactory.Create(...)
    => throws a specific OperationCanceledException instance
                         ↓
rethrow the exact same OperationCanceledException instance
```

The test uses `Assert.Same(...)`, so factory cancellation cannot be wrapped, replaced, or converted into `WIF_ERROR_CATALOG_FACTORY_FAILED`.

No production code changed. The factory exception guard excludes `OperationCanceledException`, so this focused contract is expected to be GREEN.

## 2026-09-10 — 1001/1001 GREEN factory exception checkpoint

Checkpoint commit: `a62bb898a521593b00b2b335d11740449a5e1bd8`.
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

## Completed `ErrorCatalogProvider` dependency boundaries so far

- `IErrorCatalogLoader.LoadFromFileAsync(...)` — null response, ordinary exception normalization, exact cancellation propagation.
- `IErrorCatalogDocumentNormalizer.Normalize(...)` — null result, ordinary exception normalization, exact cancellation propagation.
- `IErrorCatalogValidator.Validate(...)` — null result, ordinary exception normalization, exact cancellation propagation.
- `IErrorCatalogFactory.Create(...)` — null result and ordinary exception normalization complete; exact cancellation contract is committed and awaiting GREEN.

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
4. `IErrorCatalogFactory.Create(...)` — exact-instance cancellation awaiting GREEN.

Existing malformed-result handling:

- null loader response → `WIF_ERROR_CATALOG_LOADER_RESPONSE_NULL`;
- null normalizer result → `WIF_ERROR_CATALOG_NORMALIZER_RESULT_NULL`;
- null validator result → `WIF_ERROR_CATALOG_VALIDATOR_RESULT_NULL`;
- null factory result → `WIF_ERROR_CATALOG_FACTORY_RESULT_NULL`.

## Verification state

- Clean continuation baseline: **1001/1001 GREEN, zero compiler warnings**.
- Factory null-result and ordinary-exception behavior are covered and locally verified.
- Exact-instance cancellation originating from the factory dependency is committed and awaits focused local verification.
- No production change is expected for the cancellation contract.
- Expected complete-suite count after the contract passes: **1002 tests**.

## Recommended verification

Pull current `master` and run the focused factory cancellation contract:

```powershell
dotnet test WhenItFails.Tests --filter "FullyQualifiedName~LoadFromFileAsync_WhenFactoryCancels_RethrowsSameOperationCanceledException"
```

Expected result: GREEN.

Then run the complete suite:

```powershell
dotnet test WhenItFails.Tests
```

Expected complete-suite result: **1002/1002 GREEN with zero compiler warnings**.

## Next recommended step

After **1002/1002 GREEN** is confirmed, close the `ErrorCatalogProvider` dependency boundary audit for the current scope.

Then perform fresh repository reconnaissance for the next normalization boundary before adding any new contract. In particular, search existing propagation/shape/cancellation/null-task contracts first so transparent boundaries are not accidentally normalized.

Keep changes small, tested, documented here, and committed directly to `master`.
