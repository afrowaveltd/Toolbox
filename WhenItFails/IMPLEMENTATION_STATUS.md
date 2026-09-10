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
- Pipeline loader boundary is complete for the current scope.
- Pipeline normalizer ordinary-exception normalization is locally verified GREEN.
- The complete `WhenItFails.Tests` suite is locally verified **GREEN at 1005/1005 tests with zero compiler warnings** before the new normalizer cancellation contract.
- A focused exact-instance cancellation contract is now committed for the pipeline normalizer delegate and awaits local verification.

## 2026-09-10 — pipeline normalizer cancellation contract

Contract commit: `87d7502f8e49a46d1d3a5af89619d5c7b6d7a45b`.

Updated:

`WhenItFails.Tests/Catalog/CatalogProviderPipelineNormalizerExceptionContractTests.cs`

Added:

`LoadNormalizeValidateAsync_WhenNormalizerCancels_RethrowsSameOperationCanceledException`

Contract:

```text
normalizer delegate
    => throws a specific OperationCanceledException instance
                         ↓
rethrow the exact same OperationCanceledException instance
```

The test uses `Assert.Same(...)`. Validator and payload-factory delegates throw if reached, so cancellation must also short-circuit the remainder of the pipeline.

No production code changed. The normalizer exception guard excludes `OperationCanceledException`, so this focused contract is expected to be GREEN.

## 2026-09-10 — 1005/1005 GREEN pipeline normalizer exception checkpoint

Checkpoint commit: `a541d0578ba24b26d49288de693d07be7d44c517`.
Production fix commit: `2f25dd58a3644ee2e87685aaf2c5f909cea9adb4`.
Ordinary-exception contract commit: `81db615ca52e878f2583b6167df14a7d85124be2`.

Verified locally:

```text
WhenItFails.Tests
Failed:   0
Passed: 1005
Skipped:  0
Total:  1005
Compiler warnings: 0
```

Pipeline normalizer ordinary exceptions normalize to:

```text
Status: Failed
Data: null
Code: WIF_CATALOG_PIPELINE_NORMALIZER_FAILED
Message: The catalog provider pipeline normalizer failed.
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

1. loader delegate — complete for current scope.
2. normalizer delegate — null result and ordinary exception covered; exact cancellation awaiting GREEN.
3. validator delegate — null result covered; exception semantics not yet hardened.
4. payload-factory delegate — null result covered; exception semantics not yet hardened.

Stable malformed-result codes already include:

- `WIF_CATALOG_PIPELINE_LOADER_RESPONSE_NULL`
- `WIF_CATALOG_PIPELINE_NORMALIZER_RESULT_NULL`
- `WIF_CATALOG_PIPELINE_VALIDATOR_RESULT_NULL`
- `WIF_CATALOG_PIPELINE_PAYLOAD_NULL`

Repository reconnaissance found no `CatalogProviderPipeline` exception-shape/pass-through contract and no concrete category/code-group/owner/profile provider contract requiring normalizer exceptions to propagate unchanged.

## Verification state

- Clean continuation baseline: **1005/1005 GREEN, zero compiler warnings**.
- Pipeline loader boundary is complete for the current scope.
- Pipeline normalizer null-result and ordinary-exception behavior are locally verified.
- Exact-instance pipeline normalizer cancellation contract is committed and awaits focused local verification.
- No production change is expected for this cancellation contract.
- Expected complete-suite count after it passes: **1006 tests**.

## Recommended verification

Pull current `master` and run:

```powershell
dotnet test WhenItFails.Tests --filter "FullyQualifiedName~LoadNormalizeValidateAsync_WhenNormalizerCancels_RethrowsSameOperationCanceledException"
```

Expected result: GREEN.

Then run:

```powershell
dotnet test WhenItFails.Tests
```

Expected complete-suite result: **1006/1006 GREEN with zero compiler warnings**.

## Next recommended step

After **1006/1006 GREEN** is confirmed, consider the `CatalogProviderPipeline` normalizer boundary complete for the current scope.

Then inspect the validator delegate separately and search existing propagation/shape/cancellation contracts before introducing an ordinary-exception contract.

Do not change validator or payload-factory exception behavior until its own focused contract is established.

Keep changes small, tested, documented here, and committed directly to `master`.
