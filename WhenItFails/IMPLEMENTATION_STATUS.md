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
- All currently known `IErrorCatalogContextStore.Set(...)` invocation sites are complete for the current scope.
- `IJsonsBootstrapper.EnsureWorkspaceAsync(...)` null-response and ordinary-exception behavior are covered.
- The complete `WhenItFails.Tests` suite is locally verified **GREEN at 991/991 tests with zero compiler warnings**.
- Bootstrapper exact-instance cancellation remains the next focused contract.

## Latest committed steps

### 2026-09-09 — 991/991 GREEN bootstrapper exception checkpoint

Production fix commit: `c4951648527c5fa123f64714e2f87e1c0352e125`

Ordinary-exception contract commit: `cfe6fc4caf24bcb3a4eda476f135b29f868043d6`

Verified locally after the production fix:

```text
WhenItFails.Tests
Failed:   0
Passed: 991
Skipped:  0
Total:  991
Compiler warnings: 0
```

The initializer bootstrapper ordinary exception is converted to:

```text
Status: Failed
Data: null
Code: WIF_INITIALIZER_BOOTSTRAPPER_FAILED
Message: The JSON workspace bootstrapper failed.
```

The raw dependency exception text does not escape, the context provider is not invoked, and the previous context remains unchanged.

### 2026-09-09 — initializer bootstrapper ordinary-exception fix

Production fix commit: `c4951648527c5fa123f64714e2f87e1c0352e125`

Changed only `_bootstrapper.EnsureWorkspaceAsync(...)` inside `ErrorCatalogInitializer.InitializeAsync(...)`.

The catch filter excludes `OperationCanceledException`, so cancellation is intended to propagate unchanged.

### 2026-09-09 — verified RED initializer bootstrapper exception contract

Contract commit: `cfe6fc4caf24bcb3a4eda476f135b29f868043d6`

Observed before the production fix:

```text
System.InvalidOperationException:
Sensitive initializer bootstrapper detail must not escape.
```

This confirmed the missing bootstrapper dependency boundary.

## Verification state

- Clean continuation baseline: **991/991 GREEN, zero compiler warnings**.
- All currently known context-store read/write boundaries in the active runtime/initializer scope are complete.
- Bootstrapper null-response and ordinary-exception behavior are verified.
- Bootstrapper cancellation behavior has not yet been locked by an exact-instance contract.
- No production change is expected for the cancellation contract because the current catch filter excludes `OperationCanceledException`.
- Expected complete-suite count after the cancellation contract passes: **992 tests**.

## Recommended verification

After the cancellation contract is committed, run its focused test first, then the complete suite.

Expected complete-suite result: **992/992 GREEN with zero compiler warnings**.

## Next recommended step

Add one focused exact-instance cancellation contract for `IJsonsBootstrapper.EnsureWorkspaceAsync(...)` as invoked by `ErrorCatalogInitializer.InitializeAsync(...)`.

If that passes without production changes, consider the initializer bootstrapper boundary complete for the current scope. Then move separately to `IErrorCatalogContextProvider.LoadFromJsonsAsync(...)`: ordinary exception first, cancellation second.

Do not combine bootstrapper and context-provider hardening in one production step.

Keep changes small, tested, documented here, and committed directly to `master`.
