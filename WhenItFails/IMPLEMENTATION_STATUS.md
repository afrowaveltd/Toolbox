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
- Pipeline loader and normalizer boundaries are complete for the current scope.
- Clean locally verified baseline before the new validator contract: **1006/1006 GREEN, zero compiler warnings**.
- Pipeline validator ordinary-exception RED is confirmed locally and the minimal production guard is now committed; local GREEN verification is next.

## 2026-09-10 — pipeline validator ordinary-exception fix

Contract commit: `25f0e8fe345d89e2e226aecbaaf4f3767a4a8010`.
Production fix commit: `c99bc586ff6f8f6e82e4a0b119ec3137ee92d792`.

Test:

`WhenItFails.Tests/Catalog/CatalogProviderPipelineValidatorExceptionContractTests.cs`

`LoadNormalizeValidateAsync_WhenValidatorThrows_ReturnsStableFailure`

Observed focused RED before the production fix:

```text
System.InvalidOperationException:
Sensitive catalog provider pipeline validator detail must not escape.
```

The exception escaped through `CatalogProviderPipeline.LoadNormalizeValidateAsync(...)` from the direct `validate(normalizedDocument)` invocation, confirming the missing validator exception boundary.

Production now guards only the validator delegate:

```text
ordinary validator exception
    => Status: Failed
       Data: null
       Code: WIF_CATALOG_PIPELINE_VALIDATOR_FAILED
       Message: The catalog provider pipeline validator failed.
```

`OperationCanceledException` is explicitly excluded from the catch so cancellation can continue to propagate unchanged. Payload-factory behavior was not changed.

Expected next result: the focused validator exception contract GREEN, followed by full suite **1007/1007 GREEN with zero compiler warnings**.

## 2026-09-10 — 1006/1006 GREEN pipeline normalizer boundary checkpoint

Checkpoint commit: `157ce20f93d6a02500c30df352dc57b023e7df15`.
Normalizer cancellation contract commit: `87d7502f8e49a46d1d3a5af89619d5c7b6d7a45b`.
Normalizer production fix commit: `2f25dd58a3644ee2e87685aaf2c5f909cea9adb4`.
Normalizer ordinary-exception contract commit: `81db615ca52e878f2583b6167df14a7d85124be2`.

Verified locally:

```text
WhenItFails.Tests
Failed:   0
Passed: 1006
Skipped:  0
Total:  1006
Compiler warnings: 0
```

Pipeline normalizer boundary is complete for the current scope:

- null result → `WIF_CATALOG_PIPELINE_NORMALIZER_RESULT_NULL`;
- ordinary exception → `WIF_CATALOG_PIPELINE_NORMALIZER_FAILED`;
- exact `OperationCanceledException` instance propagates unchanged.

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
2. normalizer delegate — complete for current scope.
3. validator delegate — null result covered; ordinary-exception production guard committed and awaiting GREEN; exact cancellation next.
4. payload-factory delegate — null result covered; exception semantics not yet hardened.

Stable malformed-result codes include:

- `WIF_CATALOG_PIPELINE_LOADER_RESPONSE_NULL`
- `WIF_CATALOG_PIPELINE_NORMALIZER_RESULT_NULL`
- `WIF_CATALOG_PIPELINE_VALIDATOR_RESULT_NULL`
- `WIF_CATALOG_PIPELINE_PAYLOAD_NULL`

Stable ordinary-exception codes currently include:

- `WIF_CATALOG_PIPELINE_LOADER_FAILED`
- `WIF_CATALOG_PIPELINE_NORMALIZER_FAILED`
- `WIF_CATALOG_PIPELINE_VALIDATOR_FAILED`

## Validator reconnaissance

Repository searches found no `CatalogProviderPipeline` validator exception-shape/pass-through contract and no concrete category/code-group/owner/profile provider contract requiring validator exceptions to propagate unchanged.

No existing `WIF_CATALOG_PIPELINE_VALIDATOR_FAILED` code was found before the focused validator contract was introduced.

## Verification state

- Clean continuation baseline before the validator test: **1006/1006 GREEN, zero compiler warnings**.
- Focused validator ordinary-exception RED is confirmed.
- Minimal production fix is committed.
- Expected complete-suite count after GREEN verification: **1007 tests**.

## Recommended verification

Pull current `master` and run:

```powershell
dotnet test WhenItFails.Tests --filter "FullyQualifiedName~LoadNormalizeValidateAsync_WhenValidatorThrows_ReturnsStableFailure"
```

Expected result: GREEN.

Then run:

```powershell
dotnet test WhenItFails.Tests
```

Expected complete-suite result: **1007/1007 GREEN with zero compiler warnings**.

## Next recommended step

After **1007/1007 GREEN** is confirmed, add a focused exact-instance cancellation contract for the pipeline validator delegate. It should use a specific `OperationCanceledException` instance and `Assert.Same(...)` and ensure the payload factory is not reached.

No production change is expected for that cancellation contract because the validator guard explicitly excludes `OperationCanceledException`.

Do not change payload-factory exception behavior until the validator boundary is complete.

Keep changes small, tested, documented here, and committed directly to `master`.
