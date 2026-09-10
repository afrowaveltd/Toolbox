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
- The complete `WhenItFails.Tests` suite is locally verified **GREEN at 1008/1008 tests with zero compiler warnings** before the new payload-factory exception contract.
- A focused ordinary-exception contract is now committed for the pipeline payload-factory delegate and awaits local RED verification.

## 2026-09-10 — pipeline payload-factory ordinary-exception contract

Contract commit: `676dc55d19ceb04f5155445ceb929ea4acee16f3`.

Added:

`WhenItFails.Tests/Catalog/CatalogProviderPipelinePayloadFactoryExceptionContractTests.cs`

Test:

`LoadNormalizeValidateAsync_WhenPayloadFactoryThrows_ReturnsStableFailure`

The payload-factory delegate throws:

```text
System.InvalidOperationException:
Sensitive catalog provider pipeline payload factory detail must not escape.
```

Required stable pipeline contract:

```text
Status: Failed
Data: null
Code: WIF_CATALOG_PIPELINE_PAYLOAD_FACTORY_FAILED
Message: The catalog provider pipeline payload factory failed.
```

The raw sensitive exception text must not escape through `Response.Message`.

No production code changed. `CatalogProviderPipeline.LoadNormalizeValidateAsync(...)` currently invokes `createPayload(normalizedDocument, validationResult)` directly, so the focused test is expected to be RED with the raw payload-factory exception escaping.

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
4. payload-factory delegate — null result covered; ordinary-exception contract awaiting RED; exact cancellation later.

Stable malformed-result codes include:

- `WIF_CATALOG_PIPELINE_LOADER_RESPONSE_NULL`
- `WIF_CATALOG_PIPELINE_NORMALIZER_RESULT_NULL`
- `WIF_CATALOG_PIPELINE_VALIDATOR_RESULT_NULL`
- `WIF_CATALOG_PIPELINE_PAYLOAD_NULL`

Stable ordinary-exception codes currently implemented include:

- `WIF_CATALOG_PIPELINE_LOADER_FAILED`
- `WIF_CATALOG_PIPELINE_NORMALIZER_FAILED`
- `WIF_CATALOG_PIPELINE_VALIDATOR_FAILED`

## Payload-factory reconnaissance

Repository searches found no `CatalogProviderPipeline` payload-factory exception-shape/pass-through contract and no concrete category/code-group/owner/profile provider contract requiring payload-factory exceptions to propagate unchanged.

The existing null-result contract uses payload-factory terminology:

```text
Code: WIF_CATALOG_PIPELINE_PAYLOAD_NULL
Message: The catalog provider pipeline payload factory returned a null result.
```

No existing `WIF_CATALOG_PIPELINE_PAYLOAD_FACTORY_FAILED` code was found before the focused contract was introduced.

## Verification state

- Clean continuation baseline before the new test: **1008/1008 GREEN, zero compiler warnings**.
- Pipeline loader, normalizer and validator boundaries are complete for the current scope.
- Pipeline payload-factory null-result behavior is already covered.
- New payload-factory ordinary-exception contract is committed and awaits focused RED verification.
- Production payload-factory invocation remains unchanged until RED is observed.
- Expected complete-suite count once the new contract eventually passes: **1009 tests**.

## Recommended verification

Pull current `master` and run only the new payload-factory contract:

```powershell
dotnet test WhenItFails.Tests --filter "FullyQualifiedName~LoadNormalizeValidateAsync_WhenPayloadFactoryThrows_ReturnsStableFailure"
```

Expected current result: RED with the raw exception text:

```text
Sensitive catalog provider pipeline payload factory detail must not escape.
```

## Next recommended step

If RED is confirmed, add the smallest ordinary-exception guard around only `createPayload(normalizedDocument, validationResult)`, excluding `OperationCanceledException` so exact-instance cancellation can be tested separately afterward.

Keep the existing `WIF_CATALOG_PIPELINE_PAYLOAD_NULL` null-result branch unchanged.

Keep changes small, tested, documented here, and committed directly to `master`.
