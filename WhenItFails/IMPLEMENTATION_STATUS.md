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
- The complete `WhenItFails.Tests` suite is locally verified **GREEN at 990/990 tests with zero compiler warnings** before the bootstrapper exception contract.
- `IJsonsBootstrapper.EnsureWorkspaceAsync(...)` null-response behavior is covered by `WIF_INITIALIZER_BOOTSTRAPPER_RESPONSE_NULL`.
- The initializer bootstrapper ordinary-exception contract is verified RED before the production fix.
- `ErrorCatalogInitializer.InitializeAsync(...)` now converts ordinary bootstrapper exceptions into `WIF_INITIALIZER_BOOTSTRAPPER_FAILED` without exposing raw dependency text.

## Latest committed steps

### 2026-09-09 — initializer bootstrapper ordinary-exception fix

Production fix commit: `c4951648527c5fa123f64714e2f87e1c0352e125`

Changed only the `_bootstrapper.EnsureWorkspaceAsync(...)` invocation inside `ErrorCatalogInitializer.InitializeAsync(...)`.

Ordinary exceptions are now converted to:

```text
Status: Failed
Data: null
Code: WIF_INITIALIZER_BOOTSTRAPPER_FAILED
Message: The JSON workspace bootstrapper failed.
```

The catch filter excludes `OperationCanceledException`, so cancellation is still intended to propagate unchanged.

The production diff was checked and contains only the intended bootstrapper exception guard. Existing null-response, failed-response, payload-null, context-provider, and store-write logic remain unchanged.

### 2026-09-09 — verified RED initializer bootstrapper exception contract

Contract commit: `cfe6fc4caf24bcb3a4eda476f135b29f868043d6`

Focused test:

`WhenItFails.Tests/Initialization/ErrorCatalogInitializerBootstrapperExceptionContractTests.InitializeAsync_WhenBootstrapperThrows_ReturnsStableFailure`

Observed locally before the production fix:

```text
Failed: 1
Passed: 0
Skipped: 0
Total: 1
```

Failure:

```text
System.InvalidOperationException:
Sensitive initializer bootstrapper detail must not escape.
```

The exception escaped directly from `IJsonsBootstrapper.EnsureWorkspaceAsync(...)` through `ErrorCatalogInitializer.InitializeAsync(...)`, confirming the missing initializer bootstrapper boundary.

The context-provider path was not reached.

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

All currently known context-store read/write boundaries in the active runtime/initializer scope are complete.

## Verification state

- Clean continuation baseline before the bootstrapper exception contract: **990/990 GREEN, zero compiler warnings**.
- All currently known context-store read/write boundaries in the active runtime/initializer scope are complete.
- Bootstrapper null-response behavior is already covered.
- Bootstrapper ordinary-exception contract is verified RED before the production fix.
- Production bootstrapper guard is committed and awaits focused local GREEN verification.
- Expected complete-suite count after the new contract passes: **991 tests**.

## Recommended verification

Pull current `master` and run only the bootstrapper exception contract:

```powershell
dotnet test WhenItFails.Tests --filter "FullyQualifiedName~InitializeAsync_WhenBootstrapperThrows_ReturnsStableFailure"
```

Expected result after the production fix: GREEN.

Then run the complete suite:

```powershell
dotnet test WhenItFails.Tests
```

Expected complete-suite result: **991/991 GREEN with zero compiler warnings**.

## Next recommended step

After 991/991 GREEN is confirmed, add one focused exact-instance cancellation contract for the same `IJsonsBootstrapper.EnsureWorkspaceAsync(...)` invocation.

If that passes without production changes, consider the initializer bootstrapper boundary complete for the current scope. Then move separately to `IErrorCatalogContextProvider.LoadFromJsonsAsync(...)`: ordinary exception first, cancellation second.

Do not combine bootstrapper and context-provider hardening in one production step.

Keep changes small, tested, documented here, and committed directly to `master`.
