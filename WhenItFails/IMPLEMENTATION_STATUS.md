# Implementation status

Last updated: 2026-09-09

This file is the continuation point for `WhenItFails` development. Update it after every implementation, test, catalog, or documentation change that alters the current state or recommended next step.

## Current focus

Hardening initialization dependency boundaries against malformed behavior, raw exception leakage, and cancellation corruption.

## Current state

- `ErrorDescriptorResolver` and `ErrorDescriptorService` exception/cancellation hardening are complete for the current scope.
- `ErrorCatalogRuntime` descriptor-service and profile-selection exception/cancellation boundaries are complete.
- `IErrorCatalogContextStore.GetCurrent()` null-response, ordinary-exception, and exact-instance cancellation behavior are complete.
- `IErrorCatalogInitializer.InitializeAsync(...)` as consumed by `ErrorCatalogRuntime` has null-response, ordinary-exception, and exact-instance cancellation contracts.
- `IBuiltInErrorCatalogContextProvider.LoadAsync(...)` is hardened for both explicit `ResetToDefaultsAsync()` and flexible initialization fallback paths.
- All currently known `IErrorCatalogContextStore.Set(...)` invocation sites are complete for the current scope: explicit reset, flexible fallback, and `ErrorCatalogInitializer` each have ordinary-exception and exact-instance cancellation contracts.
- The complete `WhenItFails.Tests` suite is locally verified **GREEN at 990/990 tests with zero compiler warnings**.
- A focused ordinary-exception contract is now committed for `IJsonsBootstrapper.EnsureWorkspaceAsync(...)` as invoked by `ErrorCatalogInitializer.InitializeAsync(...)` and awaits local RED verification.

## Latest committed steps

### 2026-09-09 — initializer bootstrapper ordinary-exception contract

Contract commit: `cfe6fc4caf24bcb3a4eda476f135b29f868043d6`

Added:

`WhenItFails.Tests/Initialization/ErrorCatalogInitializerBootstrapperExceptionContractTests.cs`

Test:

`InitializeAsync_WhenBootstrapperThrows_ReturnsStableFailure`

The fixture supplies a bootstrapper that throws an ordinary `InvalidOperationException` containing sensitive diagnostic text. A tracking context provider and a store containing a previous context verify that the initializer stops immediately at the bootstrapper boundary.

Required stable initializer contract:

```text
Status: Failed
Data: null
Code: WIF_INITIALIZER_BOOTSTRAPPER_FAILED
Message: The JSON workspace bootstrapper failed.
```

The raw exception text must not escape:

```text
Sensitive initializer bootstrapper detail must not escape.
```

The context provider must not be called, and the previous store context must remain unchanged.

No production code changed in this step. `ErrorCatalogInitializer.InitializeAsync(...)` currently awaits `_bootstrapper.EnsureWorkspaceAsync(...)` directly, so the focused contract is expected to be RED with the original exception escaping.

### 2026-09-09 — 990/990 GREEN store-boundary checkpoint

Checkpoint commit: `1c2a04bb1757d0f64098a55fa802993b1e80b84e`

Verified locally:

```text
WhenItFails.Tests
Failed:   0
Passed: 990
Skipped:  0
Total:  990
Compiler warnings: 0
```

The exact original `OperationCanceledException` instance thrown by the initializer context-store `Set(...)` invocation propagates unchanged. This completes all currently known context-store write boundaries for the current scope.

## Verification state

- Clean continuation baseline: **990/990 GREEN, zero compiler warnings**.
- All currently known context-store read/write boundaries in the active runtime/initializer scope are complete.
- `IJsonsBootstrapper.EnsureWorkspaceAsync(...)` null-response behavior is already covered by `WIF_INITIALIZER_BOOTSTRAPPER_RESPONSE_NULL`.
- The new bootstrapper ordinary-exception contract is committed and awaits focused local RED verification.
- Production bootstrapper invocation remains unchanged until RED is observed.
- Expected complete-suite count once the new contract eventually passes: **991 tests**.

## Recommended verification

Pull current `master` and run only the new contract:

```powershell
dotnet test WhenItFails.Tests --filter "FullyQualifiedName~InitializeAsync_WhenBootstrapperThrows_ReturnsStableFailure"
```

Expected current result: RED with the original exception text:

```text
Sensitive initializer bootstrapper detail must not escape.
```

## Next recommended step

If RED is confirmed, add the smallest exception boundary around only `_bootstrapper.EnsureWorkspaceAsync(...)` inside `ErrorCatalogInitializer.InitializeAsync(...)`.

Convert ordinary exceptions into:

```text
Status: Failed
Data: null
Code: WIF_INITIALIZER_BOOTSTRAPPER_FAILED
Message: The JSON workspace bootstrapper failed.
```

while allowing `OperationCanceledException` to propagate unchanged.

After ordinary-exception behavior is GREEN, add a separate exact-instance cancellation contract for the same bootstrapper invocation. Do not modify `IErrorCatalogContextProvider.LoadFromJsonsAsync(...)` in the same production step.

Keep changes small, tested, documented here, and committed directly to `master`.
