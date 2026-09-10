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
- The complete `WhenItFails.Tests` suite is locally verified **GREEN at 1017/1017 tests with zero compiler warnings** before the new runtime initializer null-task contract.
- A focused null-task contract for `ErrorCatalogRuntime`'s injected `IErrorCatalogInitializer` is committed and awaits local verification.

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

No production code changed. `ErrorCatalogRuntime.InitializeCoreAsync(...)` awaits the initializer inside its existing ordinary-exception normalization guard, so the null-task `NullReferenceException` is expected to normalize to the established runtime initializer failure.

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

Fresh reconnaissance outside initializer/built-in boundaries identified `ErrorCatalogRuntime.InitializeCoreAsync(...)` as the next smallest async dependency gap. Existing runtime initializer tests already covered ordinary exceptions and exact cancellation, while null `Response` behavior was covered separately. No dedicated null-`Task` contract was found before `a8a9cb118e43eca67f4cea9d3d0291c4baa1e2a0`.

## Verification state

- Clean continuation baseline: **1017/1017 GREEN, zero compiler warnings**.
- `ErrorCatalogInitializer` async dependency null-task audit is complete for the current scope.
- `BuiltInErrorCatalogContextProvider` async dependency null-task audit is complete for the current scope.
- Runtime initializer null-task contract is committed and awaiting focused local verification.
- No production change is expected.
- Expected complete-suite count after it passes: **1018/1018 GREEN with zero compiler warnings**.

## Recommended verification

Pull current `master` and run:

```powershell
dotnet test WhenItFails.Tests --filter "FullyQualifiedName~InitializeAsync_WhenInitializerReturnsNullTask_ReturnsStableFailure"
dotnet test WhenItFails.Tests
```

Expected results:

```text
Focused contract: GREEN
Complete suite: 1018/1018 GREEN
Compiler warnings: 0
```

## Next recommended step

After **1018/1018 GREEN** is confirmed, record the checkpoint and continue the runtime async dependency audit. The next candidates are the two `IBuiltInErrorCatalogContextProvider.LoadAsync(...)` awaits used by explicit reset and flexible fallback; search existing exception/cancellation/null-response/null-task contracts before adding a new one.

Keep changes small, tested, documented here, and committed directly to `master`.
