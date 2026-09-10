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
- Explicit `ResetToDefaultsAsync(...)` built-in-provider async boundary is locally verified complete.
- The complete `WhenItFails.Tests` suite is locally verified **GREEN at 1019/1019 tests with zero compiler warnings** before the new flexible-fallback null-task contract.
- A focused null-task contract for the flexible fallback `IBuiltInErrorCatalogContextProvider.LoadAsync(...)` dependency is committed and awaits local verification.

## 2026-09-10 — flexible fallback built-in-provider null-task contract

Contract commit: `27e37855c4ac6e12a5323810c67e47e2d4ebba0a`.
Baseline checkpoint commit: `2bf35596529be9f27795ddc462f055b3a3ea883c`.

Updated:

`WhenItFails.Tests/Services/ErrorCatalogRuntimeBuiltInContextProviderFlexibleFallbackExceptionContractTests.cs`

Added:

`InitializeAsync_WhenFlexibleFallbackProviderReturnsNullTask_ReturnsStableFallbackFailure`

The fake `IBuiltInErrorCatalogContextProvider.LoadAsync(...)` returns `null!` while `ErrorCatalogRuntime` is in `Flexible` initialization mode and the configured project initializer has already failed.

Required public fallback shape:

```text
Status: Failed
Data: null
Code: WIF_DEFAULT_FALLBACK_FAILED
Message: The configured error catalog failed and the bundled default catalog could not be activated.
```

Required fallback metadata:

```text
WhenItFails.ProjectFailure.Code = CatalogDocumentsInvalid
WhenItFails.FallbackFailure.Code = WIF_BUILT_IN_CONTEXT_PROVIDER_FAILED
WhenItFails.FallbackFailure.Status = Failed
WhenItFails.FallbackFailure.Message = The bundled default catalog provider failed.
```

This is intentionally distinct from a completed task whose `Response` value is null; that case remains represented inside fallback metadata as `WIF_BUILT_IN_CONTEXT_RESPONSE_NULL` with `Invalid` status.

No production code changed. The fallback provider await is already inside the ordinary-exception normalization guard, so a null-task `NullReferenceException` is expected to normalize to the established built-in provider failure and then be wrapped in the stable default-fallback response. `OperationCanceledException` remains excluded and propagates unchanged.

## 2026-09-10 — 1019/1019 GREEN reset built-in-provider checkpoint

Checkpoint commit: `2bf35596529be9f27795ddc462f055b3a3ea883c`.
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

`ResetToDefaultsAsync(...)` explicitly covers its `IBuiltInErrorCatalogContextProvider.LoadAsync(...)` boundary:

- null `Response` → `WIF_BUILT_IN_CONTEXT_RESPONSE_NULL`;
- ordinary exception → `WIF_BUILT_IN_CONTEXT_PROVIDER_FAILED`;
- null `Task` → `WIF_BUILT_IN_CONTEXT_PROVIDER_FAILED`;
- exact `OperationCanceledException` instance propagates unchanged.

No production change was required for the null-`Task` case because the provider await is already inside the ordinary-exception normalization guard.

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
2. flexible fallback via `CreateBuiltInFallbackResponseAsync(...)` — contract committed and awaiting GREEN.

The flexible fallback path already had ordinary-exception normalization, exact cancellation propagation and null-response coverage before the new null-task contract.

## Verification state

- Clean continuation baseline: **1019/1019 GREEN, zero compiler warnings**.
- Runtime initializer async dependency boundary is complete for the current scope.
- Explicit-reset built-in-provider async boundary is complete for the current scope.
- Flexible-fallback built-in-provider null-task contract is committed and awaiting focused local verification.
- No production change is expected.
- Expected complete-suite count after it passes: **1020/1020 GREEN with zero compiler warnings**.
- After GREEN, both runtime `IBuiltInErrorCatalogContextProvider.LoadAsync(...)` await sites are complete for null response, ordinary exception, null task and exact cancellation behavior in the current scope.

## Recommended verification

Pull current `master` and run:

```powershell
dotnet test WhenItFails.Tests --filter "FullyQualifiedName~InitializeAsync_WhenFlexibleFallbackProviderReturnsNullTask_ReturnsStableFallbackFailure"
dotnet test WhenItFails.Tests
```

Expected results:

```text
Focused contract: GREEN
Complete suite: 1020/1020 GREEN
Compiler warnings: 0
```

## Next recommended step

After **1020/1020 GREEN** is confirmed, record the checkpoint and perform fresh repository reconnaissance before selecting the next smallest uncovered dependency boundary. Search existing propagation/shape/cancellation/null-task contracts first and do not change an established transparent boundary.

Keep changes small, tested, documented here, and committed directly to `master`.
