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
- The shared internal `CatalogProviderPipeline` is the current normalization boundary under audit.
- Pipeline loader ordinary-exception normalization is locally verified GREEN.
- The complete `WhenItFails.Tests` suite is locally verified **GREEN at 1003/1003 tests with zero compiler warnings** before the new loader cancellation contract.
- A focused exact-instance cancellation contract is now committed for the pipeline loader delegate and awaits local verification.

## 2026-09-10 — pipeline loader cancellation contract

Contract commit: `7ef0585dfabc02edfb52b84298a11f6887586b2b`.

Updated:

`WhenItFails.Tests/Catalog/CatalogProviderPipelineLoaderExceptionContractTests.cs`

Added:

`LoadNormalizeValidateAsync_WhenLoaderCancels_RethrowsSameOperationCanceledException`

Contract:

```text
loader delegate
    => faults with a specific OperationCanceledException instance
                         ↓
rethrow the exact same OperationCanceledException instance
```

The test uses `Assert.Same(...)`. Normalizer, validator, and payload-factory delegates throw if reached, so cancellation must also short-circuit the remainder of the pipeline.

No production code changed. The loader exception guard excludes `OperationCanceledException`, so this focused contract is expected to be GREEN.

## 2026-09-10 — 1003/1003 GREEN pipeline loader exception checkpoint

Checkpoint commit: `8430b202f6b96445591f49406738beca29de3da9`.
Pipeline loader production fix commit: `42e2e74cbf43adbc62fbaa9616c33b9db9bcb835`.
Pipeline loader ordinary-exception contract commit: `2a5232c4c15b242be0cc1dfae40653d57964223e`.

Verified locally:

```text
WhenItFails.Tests
Failed:   0
Passed: 1003
Skipped:  0
Total:  1003
Compiler warnings: 0
```

Ordinary loader exceptions normalize to:

```text
Status: Failed
Data: null
Code: WIF_CATALOG_PIPELINE_LOADER_FAILED
Message: The catalog provider pipeline loader failed.
```

## Completed `ErrorCatalogProvider` dependency boundaries

- `IErrorCatalogLoader.LoadFromFileAsync(...)` — null response, ordinary exception normalization, exact cancellation propagation.
- `IErrorCatalogDocumentNormalizer.Normalize(...)` — null result, ordinary exception normalization, exact cancellation propagation.
- `IErrorCatalogValidator.Validate(...)` — null result, ordinary exception normalization, exact cancellation propagation.
- `IErrorCatalogFactory.Create(...)` — null result, ordinary exception normalization, exact cancellation propagation.

## Established transparent boundary — do not normalize

`ErrorCatalogContextProvider.LoadFromJsonsAsync(...)` must preserve exceptions from its five provider dependencies.

Relevant suites:

- `WhenItFails.Tests/Catalog/ErrorCatalogContextProviderProviderExceptionPropagationTests.cs`
- `WhenItFails.Tests/Catalog/ErrorCatalogContextProviderExceptionShapeTests.cs`
- `WhenItFails.Tests/Catalog/ErrorCatalogContextProviderCancellationPropagationTests.cs`

## `CatalogProviderPipeline` boundary map

`CatalogProviderPipeline.LoadNormalizeValidateAsync(...)` is used by:

1. `ErrorCategoryCatalogProvider`
2. `ErrorCodeGroupCatalogProvider`
3. `ErrorOwnerCatalogProvider`
4. `ErrorProfileCatalogProvider`

Current phases:

1. loader delegate — null response and ordinary exception covered; exact cancellation awaiting GREEN.
2. normalizer delegate — null result covered; exception semantics not yet hardened.
3. validator delegate — null result covered; exception semantics not yet hardened.
4. payload-factory delegate — null result covered; exception semantics not yet hardened.

Stable malformed-result codes already include:

- `WIF_CATALOG_PIPELINE_LOADER_RESPONSE_NULL`
- `WIF_CATALOG_PIPELINE_NORMALIZER_RESULT_NULL`
- `WIF_CATALOG_PIPELINE_VALIDATOR_RESULT_NULL`
- `WIF_CATALOG_PIPELINE_PAYLOAD_NULL`

Repository searches found no `CatalogProviderPipeline` exception-shape/pass-through contract and no concrete category/code-group/owner/profile provider contract requiring internal pipeline exceptions to propagate unchanged.

## Verification state

- Clean continuation baseline: **1003/1003 GREEN, zero compiler warnings**.
- Pipeline loader ordinary-exception behavior is locally verified.
- Exact-instance pipeline loader cancellation contract is committed and awaits focused local verification.
- No production change is expected for this cancellation contract.
- Expected complete-suite count after it passes: **1004 tests**.

## Recommended verification

Pull current `master` and run:

```powershell
dotnet test WhenItFails.Tests --filter "FullyQualifiedName~LoadNormalizeValidateAsync_WhenLoaderCancels_RethrowsSameOperationCanceledException"
```

Expected result: GREEN.

Then run:

```powershell
dotnet test WhenItFails.Tests
```

Expected complete-suite result: **1004/1004 GREEN with zero compiler warnings**.

## Next recommended step

After **1004/1004 GREEN** is confirmed, consider the `CatalogProviderPipeline` loader boundary complete for the current scope.

Then inspect the normalizer delegate separately and search existing propagation/shape/cancellation contracts before introducing an ordinary-exception contract.

Keep changes small, tested, documented here, and committed directly to `master`.
