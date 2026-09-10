# Implementation status

Last updated: 2026-09-10

This file is the continuation point for `WhenItFails` development. Update it after every implementation, test, catalog, or documentation change that alters the current state or recommended next step.

## Current focus

Hardening dependency boundaries while preserving established public exception contracts, including malformed async dependency results such as null `Task` values.

## Current state

- `ErrorDescriptorResolver` and `ErrorDescriptorService` hardening are complete for the current scope.
- `ErrorCatalogProvider` and `CatalogProviderPipeline` dependency-boundary audits are complete for the current scope.
- `ErrorCatalogContextProvider` is intentionally transparent for exceptions and null tasks from its internal providers; do not normalize those lower-layer contracts.
- `BuiltInErrorCatalogContextProvider` dependency-boundary audit is complete for the current scope, including injected context-provider null `Task` normalization.
- `ErrorCatalogInitializer` bootstrapper/context-provider ordinary-exception, cancellation, null-response and null-task behavior is complete for the current scope.
- `ErrorCatalogRuntime` initializer ordinary-exception, cancellation, null-response and null-task behavior is complete for the current scope.
- Both runtime `IBuiltInErrorCatalogContextProvider.LoadAsync(...)` boundaries — explicit reset and flexible fallback — are complete for null response, ordinary exception, null task and exact cancellation behavior in the current scope.
- The complete `WhenItFails.Tests` suite is locally verified **GREEN at 1020/1020 tests with zero compiler warnings** before the new bootstrap template-provider exception contract.
- A focused ordinary-exception contract for direct `JsonsBootstrapper` → `IJsonsTemplateProvider.GetTemplateFiles(...)` is committed and awaits local RED verification.

## 2026-09-10 — bootstrap template-provider ordinary-exception contract

Contract commit: `1da0988b5419d6af649c16c820bb92d63ab4dc8a`.
Baseline checkpoint commit: `383014695d776bbbbb45db31de5f050c8a09cde8`.

Added:

`WhenItFails.Tests/Bootstrap/JsonsBootstrapperTemplateProviderExceptionContractTests.cs`

Contract:

`EnsureWorkspaceAsync_WhenTemplateProviderThrows_ReturnsStableFailureWithoutExceptionDetail`

The fake `IJsonsTemplateProvider.GetTemplateFiles(...)` throws an `InvalidOperationException` containing sensitive implementation detail.

Required response:

```text
Status: Failed
Data: null
Code: WIF_JSONS_TEMPLATE_PROVIDER_FAILED
Message: The JSON template provider failed.
```

The raw dependency exception detail must not appear in the public response.

Production is intentionally unchanged before the RED run. Current `JsonsBootstrapper` catches filesystem `UnauthorizedAccessException` and `IOException`, but does not currently normalize an ordinary exception thrown by the injected template provider, so the focused test is expected to fail by propagating the `InvalidOperationException`.

If the expected RED is confirmed, make the smallest production change around `IJsonsTemplateProvider.GetTemplateFiles(...)`: normalize ordinary exceptions to `WIF_JSONS_TEMPLATE_PROVIDER_FAILED`, while preserving `OperationCanceledException` propagation unchanged. Add exact-instance cancellation as a separate follow-up contract after the ordinary-exception fix is verified.

## 2026-09-10 — 1020/1020 GREEN flexible-fallback checkpoint

Checkpoint commit: `383014695d776bbbbb45db31de5f050c8a09cde8`.
Flexible-fallback null-task contract commit: `27e37855c4ac6e12a5323810c67e47e2d4ebba0a`.
Previous status commit: `302847070ba712fe89fced815eeeb6a6da128c23`.

Verified locally:

```text
WhenItFails.Tests
Failed:   0
Passed: 1020
Skipped:  0
Total:  1020
Compiler warnings: 0
```

The flexible fallback path through `CreateBuiltInFallbackResponseAsync(...)` explicitly covers its `IBuiltInErrorCatalogContextProvider.LoadAsync(...)` boundary:

- null `Response` → fallback metadata `WIF_BUILT_IN_CONTEXT_RESPONSE_NULL` / `Invalid`;
- ordinary exception → fallback metadata `WIF_BUILT_IN_CONTEXT_PROVIDER_FAILED` / `Failed`;
- null `Task` → fallback metadata `WIF_BUILT_IN_CONTEXT_PROVIDER_FAILED` / `Failed`;
- exact `OperationCanceledException` instance propagates unchanged.

Together with the explicit reset coverage, both runtime uses of the built-in provider are complete for the current dependency-boundary scope.

## 2026-09-10 — 1019/1019 GREEN reset built-in-provider checkpoint

Checkpoint commit: `2bf35596529be9f27795ddc462f055b3a3ea883c`.
Reset null-task contract commit: `533058b97684f411a40a5b7ea86ef68b3cdcfa6e`.

Verified locally:

```text
WhenItFails.Tests
Failed:   0
Passed: 1019
Skipped:  0
Total:  1019
Compiler warnings: 0
```

`ResetToDefaultsAsync(...)` explicitly covers its `IBuiltInErrorCatalogContextProvider.LoadAsync(...)` boundary:

- null `Response` → `WIF_BUILT_IN_CONTEXT_RESPONSE_NULL`;
- ordinary exception → `WIF_BUILT_IN_CONTEXT_PROVIDER_FAILED`;
- null `Task` → `WIF_BUILT_IN_CONTEXT_PROVIDER_FAILED`;
- exact `OperationCanceledException` instance propagates unchanged.

## 2026-09-10 — 1018/1018 GREEN runtime initializer checkpoint

Checkpoint commit: `bcbc9c6696fcf2a058c83c376fa83da2ce36f7bc`.
Runtime initializer null-task contract commit: `a8a9cb118e43eca67f4cea9d3d0291c4baa1e2a0`.

Verified locally:

```text
WhenItFails.Tests
Failed:   0
Passed: 1018
Skipped:  0
Total:  1018
Compiler warnings: 0
```

`ErrorCatalogRuntime.InitializeCoreAsync(...)` explicitly covers its injected `IErrorCatalogInitializer` boundary:

- null `Response` → `WIF_INITIALIZER_RESPONSE_NULL`;
- ordinary exception → `WIF_INITIALIZER_FAILED`;
- null `Task` → `WIF_INITIALIZER_FAILED`;
- exact `OperationCanceledException` instance propagates unchanged.

## 2026-09-10 — 1017/1017 GREEN initializer checkpoint

Checkpoint commit: `c35fd30a260d82d32239ddf04851c6cd25ea5769`.
Initializer context-provider null-task contract commit: `40404b8e07fef0bb24e2f3fb5251cda03c99cd8b`.
Initializer bootstrapper null-task contract commit: `84f3fe36469882aedfed03507d857b267f89d1c1`.

Verified locally:

```text
WhenItFails.Tests
Failed:   0
Passed: 1017
Skipped:  0
Total:  1017
Compiler warnings: 0
```

`ErrorCatalogInitializer` explicitly covers both async dependency boundaries:

### `IJsonsBootstrapper.EnsureWorkspaceAsync(...)`

- null `Response` → `WIF_INITIALIZER_BOOTSTRAPPER_RESPONSE_NULL`;
- ordinary exception → `WIF_INITIALIZER_BOOTSTRAPPER_FAILED`;
- null `Task` → `WIF_INITIALIZER_BOOTSTRAPPER_FAILED`;
- exact `OperationCanceledException` instance propagates unchanged;
- downstream context provider does not run after bootstrapper failure;
- previously stored context is preserved.

### `IErrorCatalogContextProvider.LoadFromJsonsAsync(...)`

- null `Response` → `WIF_INITIALIZER_CONTEXT_PROVIDER_RESPONSE_NULL`;
- ordinary exception → `WIF_INITIALIZER_CONTEXT_PROVIDER_FAILED`;
- null `Task` → `WIF_INITIALIZER_CONTEXT_PROVIDER_FAILED`;
- exact `OperationCanceledException` instance propagates unchanged;
- context store is not replaced after failure;
- previously stored context is preserved.

## Established transparent lower boundary — do not normalize

`ErrorCatalogContextProvider.LoadFromJsonsAsync(...)` intentionally preserves exceptions and null-task behavior from its five internal catalog providers.

Relevant suites include:

- `WhenItFails.Tests/Catalog/ErrorCatalogContextProviderProviderExceptionPropagationTests.cs`
- `WhenItFails.Tests/Catalog/ErrorCatalogContextProviderExceptionShapeTests.cs`
- `WhenItFails.Tests/Catalog/ErrorCatalogContextProviderCancellationPropagationTests.cs`
- `WhenItFails.Tests/Catalog/ErrorCatalogContextProviderOwnerNullTaskTests.cs`
- `WhenItFails.Tests/Catalog/ErrorCatalogContextProviderProfileNullTaskTests.cs`

Do not replace those transparent contracts with normalization at that layer.

## Reconnaissance notes

`JsonCatalogDocumentLoader.InvalidJson` deliberately includes the parser message; `Docs/Loading-and-Normalization/en.md` documents that behavior. Do not sanitize it as an incidental hardening change.

Fresh reconnaissance after the 1020 checkpoint moved outside the completed runtime/initializer/built-in-provider boundaries. `JsonsBootstrapper` consumes `IJsonsTemplateProvider.GetTemplateFiles(...)` directly. Existing contracts already normalize malformed provider results such as null collections/items. Searches found no established provider-exception propagation contract and no documentation requiring such propagation.

## Verification state

- Clean continuation baseline: **1020/1020 GREEN, zero compiler warnings**.
- Runtime initializer and built-in-provider async dependency boundaries are complete for the current scope.
- Bootstrap template-provider ordinary-exception contract is committed and awaiting expected RED.
- Production is unchanged.
- Expected complete-suite count after the new test eventually passes: **1021/1021 GREEN with zero compiler warnings**.

## Recommended verification

Pull current `master` and run:

```powershell
dotnet test WhenItFails.Tests --filter "FullyQualifiedName~EnsureWorkspaceAsync_WhenTemplateProviderThrows_ReturnsStableFailureWithoutExceptionDetail"
```

Expected current result: RED because the provider's `InvalidOperationException` propagates instead of being converted to a `Response`.

## Next recommended step

If the focused test fails by propagating the injected provider exception, add the smallest production normalization for ordinary provider exceptions only. Preserve cancellation propagation. Then rerun the focused test and the complete suite before adding the separate exact-instance cancellation contract.

Keep changes small, tested, documented here, and committed directly to `master`.
