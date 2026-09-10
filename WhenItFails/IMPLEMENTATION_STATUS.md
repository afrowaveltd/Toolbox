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
- A focused ordinary-exception contract is now committed for the pipeline loader delegate and awaits local RED verification.

## 2026-09-10 — catalog pipeline loader ordinary-exception contract

Contract commit: `2a5232c4c15b242be0cc1dfae40653d57964223e`

Added:

`WhenItFails.Tests/Catalog/CatalogProviderPipelineLoaderExceptionContractTests.cs`

Test:

`LoadNormalizeValidateAsync_WhenLoaderThrows_ReturnsStableFailure`

The loader delegate faults with:

```text
System.InvalidOperationException:
Sensitive catalog provider pipeline loader detail must not escape.
```

Required stable pipeline contract:

```text
Status: Failed
Data: null
Code: WIF_CATALOG_PIPELINE_LOADER_FAILED
Message: The catalog provider pipeline loader failed.
```

The normalizer, validator, and payload-factory delegates all throw if reached, so the contract also locks short-circuit behavior after loader failure.

No production code changed. `CatalogProviderPipeline.LoadNormalizeValidateAsync(...)` currently awaits the loader delegate directly, so this focused test is expected to be RED with the raw loader exception escaping.

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

- Clean continuation baseline before the new test: **1002/1002 GREEN, zero compiler warnings**.
- `ErrorCatalogProvider` dependency-boundary audit is closed for the current scope.
- New pipeline loader ordinary-exception contract is committed and awaits focused RED verification.
- Production `CatalogProviderPipeline` remains unchanged until RED is observed.
- Expected complete-suite count once the new contract eventually passes: **1003 tests**.

## Recommended verification

Pull current `master` and run only the new contract:

```powershell
dotnet test WhenItFails.Tests --filter "FullyQualifiedName~LoadNormalizeValidateAsync_WhenLoaderThrows_ReturnsStableFailure"
```

Expected current result: RED with the raw exception text:

```text
Sensitive catalog provider pipeline loader detail must not escape.
```

## Next recommended step

If RED is confirmed, add the smallest ordinary-exception guard around only the pipeline loader delegate await, excluding `OperationCanceledException` so exact-instance cancellation can be tested separately afterward.

Do not change normalizer, validator, or payload-factory exception behavior in the same production commit.

Keep changes small, tested, documented here, and committed directly to `master`.
