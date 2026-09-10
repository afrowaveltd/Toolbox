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
- `ErrorCatalogProvider` dependency-boundary audit is complete for the current scope.
- The complete `WhenItFails.Tests` suite is locally verified **GREEN at 1002/1002 tests with zero compiler warnings** before the new pipeline loader exception contract.
- The shared internal `CatalogProviderPipeline` is the current normalization boundary under audit.
- The pipeline loader ordinary-exception contract is verified RED and the smallest production guard is committed; local GREEN verification is pending.

## 2026-09-10 — catalog pipeline loader ordinary-exception fix

Production fix commit: `42e2e74cbf43adbc62fbaa9616c33b9db9bcb835`

Changed only the `await loadAsync(filePath, cancellationToken)` invocation inside `CatalogProviderPipeline.LoadNormalizeValidateAsync(...)`.

Ordinary loader exceptions are now converted to:

```text
Status: Failed
Data: null
Code: WIF_CATALOG_PIPELINE_LOADER_FAILED
Message: The catalog provider pipeline loader failed.
```

The catch filter excludes `OperationCanceledException`, so cancellation originating from the loader delegate continues to propagate unchanged.

The production commit diff was checked and contains only the intended loader exception guard. Existing null-response handling and the normalizer, validator, and payload-factory phases remain unchanged.

## 2026-09-10 — verified RED catalog pipeline loader contract

Contract commit: `2a5232c4c15b242be0cc1dfae40653d57964223e`

Focused test:

`WhenItFails.Tests.Catalog.CatalogProviderPipelineLoaderExceptionContractTests.LoadNormalizeValidateAsync_WhenLoaderThrows_ReturnsStableFailure`

Observed locally before the production fix:

```text
Failed: 1
Passed: 0
Skipped: 0
Total: 1
```

Failure:

```text
System.InvalidOperationException:
Sensitive catalog provider pipeline loader detail must not escape.
```

The exception escaped directly from the loader delegate through `CatalogProviderPipeline.LoadNormalizeValidateAsync(...)`, confirming the missing loader exception boundary.

The normalizer, validator, and payload-factory delegates in the test all throw if reached, so the contract also locks short-circuit behavior after loader failure.

## 2026-09-10 — 1002/1002 GREEN `ErrorCatalogProvider` checkpoint

Checkpoint commit: `3e0c543ffd1ebc043e5b1b237cb526778f4a58b0`.

Factory cancellation contract commit: `ae724530c67bfb02a427f46a9fe9401c763454c5`.
Factory production fix commit: `51444e6cb6be6df360d3861f4b251d1c1b8b8c88`.
Factory ordinary-exception contract commit: `da315a5d826177cdca2c3e773a42bd2f7b84db4a`.

Verified locally:

```text
WhenItFails.Tests
Failed:   0
Passed: 1002
Skipped:  0
Total:  1002
Compiler warnings: 0
```

The exact `OperationCanceledException` instance thrown by `IErrorCatalogFactory.Create(...)` propagates unchanged. Together with the previously verified loader, normalizer, and validator contracts, this closes the `ErrorCatalogProvider` dependency-boundary audit for the current scope.

## Completed `ErrorCatalogProvider` dependency boundaries

- `IErrorCatalogLoader.LoadFromFileAsync(...)`
  - null response → `WIF_ERROR_CATALOG_LOADER_RESPONSE_NULL`;
  - ordinary exception → `WIF_ERROR_CATALOG_LOADER_FAILED`;
  - exact cancellation propagates unchanged.
- `IErrorCatalogDocumentNormalizer.Normalize(...)`
  - null result → `WIF_ERROR_CATALOG_NORMALIZER_RESULT_NULL`;
  - ordinary exception → `WIF_ERROR_CATALOG_NORMALIZER_FAILED`;
  - exact cancellation propagates unchanged.
- `IErrorCatalogValidator.Validate(...)`
  - null result → `WIF_ERROR_CATALOG_VALIDATOR_RESULT_NULL`;
  - ordinary exception → `WIF_ERROR_CATALOG_VALIDATOR_FAILED`;
  - exact cancellation propagates unchanged.
- `IErrorCatalogFactory.Create(...)`
  - null result → `WIF_ERROR_CATALOG_FACTORY_RESULT_NULL`;
  - ordinary exception → `WIF_ERROR_CATALOG_FACTORY_FAILED`;
  - exact cancellation propagates unchanged.

## Established transparent boundary — do not normalize

`ErrorCatalogContextProvider.LoadFromJsonsAsync(...)` must preserve exceptions from its five provider dependencies.

Pre-existing tests require preservation of synchronous and faulted-task exception identity, exception type, inner exception references, `Exception.Data`, custom properties, cancellation information, and short-circuit behavior.

Relevant suites:

- `WhenItFails.Tests/Catalog/ErrorCatalogContextProviderProviderExceptionPropagationTests.cs`
- `WhenItFails.Tests/Catalog/ErrorCatalogContextProviderExceptionShapeTests.cs`
- `WhenItFails.Tests/Catalog/ErrorCatalogContextProviderCancellationPropagationTests.cs`

## `CatalogProviderPipeline` reconnaissance

`CatalogProviderPipeline.LoadNormalizeValidateAsync(...)` is used by:

1. `ErrorCategoryCatalogProvider`
2. `ErrorCodeGroupCatalogProvider`
3. `ErrorOwnerCatalogProvider`
4. `ErrorProfileCatalogProvider`

Existing stable malformed-result codes include:

- `WIF_CATALOG_PIPELINE_LOADER_RESPONSE_NULL`
- `WIF_CATALOG_PIPELINE_NORMALIZER_RESULT_NULL`
- `WIF_CATALOG_PIPELINE_VALIDATOR_RESULT_NULL`
- `WIF_CATALOG_PIPELINE_PAYLOAD_NULL`

Repository searches found no `CatalogProviderPipeline` exception-shape/pass-through contract and no concrete category/code-group/owner/profile provider contract requiring internal loader exceptions to propagate unchanged. Existing `ErrorCatalogContextProvider` propagation tests concern exceptions thrown by the provider dependency itself and remain a separate outer-boundary contract.

## Verification state

- Clean continuation baseline before the new pipeline test: **1002/1002 GREEN, zero compiler warnings**.
- `ErrorCatalogProvider` dependency-boundary audit is closed for the current scope.
- Pipeline loader ordinary-exception contract is verified RED before the production fix.
- Production loader guard is committed and awaits focused local GREEN verification.
- Expected complete-suite count after the contract passes: **1003 tests**.

## Recommended verification

Pull current `master` and run only the pipeline loader exception contract:

```powershell
dotnet test WhenItFails.Tests --filter "FullyQualifiedName~LoadNormalizeValidateAsync_WhenLoaderThrows_ReturnsStableFailure"
```

Expected result after the production fix: GREEN.

Then run the complete suite:

```powershell
dotnet test WhenItFails.Tests
```

Expected complete-suite result: **1003/1003 GREEN with zero compiler warnings**.

## Next recommended step

After **1003/1003 GREEN** is confirmed, add a separate exact-instance cancellation contract for the `CatalogProviderPipeline` loader delegate.

If that passes without production changes, consider the pipeline loader boundary complete for the current scope and inspect the pipeline normalizer delegate separately, again checking existing propagation/shape contracts before adding any new RED test.

Do not change normalizer, validator, or payload-factory exception behavior in the same production commit.

Keep changes small, tested, documented here, and committed directly to `master`.
