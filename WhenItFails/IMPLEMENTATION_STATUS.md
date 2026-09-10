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
- Pipeline loader, normalizer and validator boundaries are complete for the current scope.
- Pipeline payload-factory null-result and ordinary-exception behavior are locally verified GREEN.
- The complete `WhenItFails.Tests` suite is locally verified **GREEN at 1009/1009 tests with zero compiler warnings** before the new payload-factory cancellation contract.
- A focused exact-instance cancellation contract is now committed for the payload-factory delegate and awaits local verification.

## 2026-09-10 — pipeline payload-factory cancellation contract

Contract commit: `8290c3be0eecb9d38a7a511ae9e8e3407ef9a4ea`.

Updated:

`WhenItFails.Tests/Catalog/CatalogProviderPipelinePayloadFactoryExceptionContractTests.cs`

Added:

`LoadNormalizeValidateAsync_WhenPayloadFactoryCancels_RethrowsSameOperationCanceledException`

Contract:

```text
payload-factory delegate
    => throws a specific OperationCanceledException instance
                         ↓
rethrow the exact same OperationCanceledException instance
```

The test uses `Assert.Same(...)`.

No production code changed. The payload-factory exception guard excludes `OperationCanceledException`, so this focused contract is expected to be GREEN.

## 2026-09-10 — 1009/1009 GREEN pipeline payload-factory exception checkpoint

Checkpoint commit: `80aea0106589e58b5fd27c928e0ce6714c44492d`.
Payload-factory production fix commit: `fe49871c054523932063935c7aeab62521a18a7c`.
Payload-factory ordinary-exception contract commit: `676dc55d19ceb04f5155445ceb929ea4acee16f3`.

Verified locally:

```text
WhenItFails.Tests
Failed:   0
Passed: 1009
Skipped:  0
Total:  1009
Compiler warnings: 0
```

Pipeline payload-factory ordinary exceptions normalize to:

```text
Status: Failed
Data: null
Code: WIF_CATALOG_PIPELINE_PAYLOAD_FACTORY_FAILED
Message: The catalog provider pipeline payload factory failed.
```

Existing null-payload behavior remains:

```text
Code: WIF_CATALOG_PIPELINE_PAYLOAD_NULL
Message: The catalog provider pipeline payload factory returned a null result.
```

## 2026-09-10 — 1008/1008 GREEN pipeline validator boundary checkpoint

Checkpoint commit: `a8daffefc2b8ffb04ce639c859f951b639d1f9b6`.
Validator cancellation contract commit: `0af207af6fc957e55f498e8e669dc371b3313541`.
Validator production fix commit: `c99bc586ff6f8f6e82e4a0b119ec3137ee92d792`.
Validator ordinary-exception contract commit: `25f0e8fe345d89e2e226aecbaaf4f3767a4a8010`.

Verified locally:

```text
WhenItFails.Tests
Failed:   0
Passed: 1008
Skipped:  0
Total:  1008
Compiler warnings: 0
```

Pipeline validator boundary is complete for the current scope:

- null result → `WIF_CATALOG_PIPELINE_VALIDATOR_RESULT_NULL`;
- ordinary exception → `WIF_CATALOG_PIPELINE_VALIDATOR_FAILED`;
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
3. validator delegate — complete for current scope.
4. payload-factory delegate — null result and ordinary exception complete; exact cancellation awaiting GREEN.

Stable malformed-result codes include:

- `WIF_CATALOG_PIPELINE_LOADER_RESPONSE_NULL`
- `WIF_CATALOG_PIPELINE_NORMALIZER_RESULT_NULL`
- `WIF_CATALOG_PIPELINE_VALIDATOR_RESULT_NULL`
- `WIF_CATALOG_PIPELINE_PAYLOAD_NULL`

Stable ordinary-exception codes implemented include:

- `WIF_CATALOG_PIPELINE_LOADER_FAILED`
- `WIF_CATALOG_PIPELINE_NORMALIZER_FAILED`
- `WIF_CATALOG_PIPELINE_VALIDATOR_FAILED`
- `WIF_CATALOG_PIPELINE_PAYLOAD_FACTORY_FAILED`

## Verification state

- Clean continuation baseline: **1009/1009 GREEN, zero compiler warnings**.
- Pipeline loader, normalizer and validator boundaries are complete for the current scope.
- Pipeline payload-factory null-result and ordinary-exception behavior are locally verified.
- Exact-instance payload-factory cancellation contract is committed and awaits focused local verification.
- No production change is expected for this cancellation contract.
- Expected complete-suite count after it passes: **1010 tests**.

## Recommended verification

Pull current `master` and run:

```powershell
dotnet test WhenItFails.Tests --filter "FullyQualifiedName~LoadNormalizeValidateAsync_WhenPayloadFactoryCancels_RethrowsSameOperationCanceledException"
```

Expected result: GREEN.

Then run:

```powershell
dotnet test WhenItFails.Tests
```

Expected complete-suite result: **1010/1010 GREEN with zero compiler warnings**.

## Next recommended step

After **1010/1010 GREEN** is confirmed, consider the entire `CatalogProviderPipeline` dependency-boundary audit complete for the current scope.

Record the final checkpoint, then perform fresh repository reconnaissance before selecting the next normalization or transparent boundary. Search existing propagation, exception-shape, cancellation, null-task and malformed-result contracts before introducing any new behavior.

Keep changes small, tested, documented here, and committed directly to `master`.
