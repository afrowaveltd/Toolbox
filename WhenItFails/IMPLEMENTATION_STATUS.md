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
- Explicit `ResetToDefaultsAsync(...)` built-in-provider null-task behavior is locally verified.
- The complete `WhenItFails.Tests` suite is locally verified **GREEN at 1019/1019 tests with zero compiler warnings**.
- Flexible-fallback built-in-provider null-task behavior is the next focused runtime boundary.

## 2026-09-10 — 1019/1019 GREEN reset built-in-provider checkpoint

Checkpoint commit: this status commit.
Reset null-task contract commit: `533058b97684f411a40a5b7ea86ef68b3cdcfa6e`.
Previous baseline checkpoint commit: `bcbc9c6696fcf2a058c83c376fa83da2ce36f7bc`.

Verified locally:

```text
WhenItFails.Tests
Failed:   0
Passed: 1019
Skipped:  0
Total:  1019
Compiler warnings: 0
```

`ResetToDefaultsAsync(...)` now explicitly covers its `IBuiltInErrorCatalogContextProvider.LoadAsync(...)` boundary:

- null `Response` → `WIF_BUILT_IN_CONTEXT_RESPONSE_NULL`;
- ordinary exception → `WIF_BUILT_IN_CONTEXT_PROVIDER_FAILED`;
- null `Task` → `WIF_BUILT_IN_CONTEXT_PROVIDER_FAILED`;
- exact `OperationCanceledException` instance propagates unchanged.

No production change was required for the null-`Task` case because the provider await is already inside the ordinary-exception normalization guard.

## 2026-09-10 — runtime initializer null-task contract

Contract commit: `a8a9cb118e43eca67f4cea9d3d0291c4baa1e2a0`.
Baseline checkpoint commit: `c35fd30a260d82d32239ddf04851c6cd25ea5769`.

Updated:

`WhenItFails.Tests/Services/ErrorCatalogRuntimeInitializerExceptionContractTests.cs`

Added:

`InitializeAsync_WhenInitializerReturnsNullTask_ReturnsStableFailure`

The fake `IErrorCatalogInitializer.InitializeAsync(...)` returns `null!` instead of a `Task<Response<ErrorCatalogInitializationPayload>>`.

Required contract:

```text
Status: Failed
Data: null
Code: WIF_INITIALIZER_FAILED
Message: The error catalog initializer failed.
```

No production code changed. `ErrorCatalogRuntime.InitializeCoreAsync(...)` awaits the initializer inside its existing ordinary-exception normalization guard, so the null-task `NullReferenceException` normalizes to the established runtime initializer failure.

## 2026-09-10 — 1018/1018 GREEN runtime initializer checkpoint

Checkpoint commit: `bcbc9c6696fcf2a058c83c376fa83da2ce36f7bc`.
Runtime initializer null-task contract commit: `a8a9cb118e43eca67f4cea9d3d0291c4baa1e2a0`.
Previous status commit: `3f65365ce84601557ce78710ca28b64921db6af0`.

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

Runtime reconnaissance identified two `IBuiltInErrorCatalogContextProvider.LoadAsync(...)` await sites for explicit null-task coverage:

1. explicit reset via `ResetToDefaultsAsync(...)` — **verified GREEN at 1019/1019**;
2. flexible fallback via `CreateBuiltInFallbackResponseAsync(...)` — next focused contract.

The flexible fallback path already has ordinary-exception normalization, exact cancellation propagation and null-response coverage. No dedicated null-`Task` contract exists yet.

## Verification state

- Clean continuation baseline: **1019/1019 GREEN, zero compiler warnings**.
- Runtime initializer async dependency boundary is complete for the current scope.
- Explicit-reset built-in-provider async boundary is complete for the current scope.
- Flexible fallback built-in-provider null-task contract is the remaining runtime built-in-provider gap.
- No production change is expected because the fallback provider await is already inside its ordinary-exception normalization guard.
- Expected complete-suite count after the next contract passes: **1020/1020 GREEN with zero compiler warnings**.

## Next recommended step

Add one focused null-task contract to `ErrorCatalogRuntimeBuiltInContextProviderFlexibleFallbackExceptionContractTests.cs` requiring the same stable fallback shape as an ordinary provider exception, then run the focused test and complete suite.

Keep changes small, tested, documented here, and committed directly to `master`.
