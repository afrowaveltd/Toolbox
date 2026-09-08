# Implementation status

Last updated: 2026-09-08

This file is the continuation point for `WhenItFails` development. Update it after every implementation, test, catalog, or documentation change that alters the current state or recommended next step.

## Current focus

Hardening runtime and service boundaries against malformed dependency behavior, raw exception leakage, and internally inconsistent responses.

## Current state

- `WhenItFails` provides structured error catalogs, runtime error resolution, profiles, diagnostics, initialization/recovery behavior, and project-local catalog handling.
- `ErrorDescriptorResolver` and `ErrorDescriptorService` exception/cancellation hardening blocks are complete for the current scope.
- `ErrorCatalogRuntime` descriptor-service ordinary-exception and cancellation behavior is complete for the current scope.
- `ErrorCatalogRuntime` profile-selection ordinary-exception and cancellation behavior is complete for the current scope.
- `ErrorCatalogRuntime` context-store `GetCurrent()` null-response, ordinary-exception, and cancellation behavior is complete for the current scope.
- `ErrorCatalogRuntime` initializer null-response, ordinary-exception, and cancellation behavior is complete for the current scope.
- Built-in catalog-provider null-response behavior is already covered for `ResetToDefaultsAsync()` and flexible initialization fallback.
- `ResetToDefaultsAsync()` converts ordinary `IBuiltInErrorCatalogContextProvider.LoadAsync(...)` exceptions into `WIF_BUILT_IN_CONTEXT_PROVIDER_FAILED` without exposing raw dependency exception text.
- The complete `WhenItFails.Tests` suite is verified GREEN at 981/981 tests.

## Latest committed steps

### 2026-09-08 — verified ErrorCatalogRuntime built-in provider exception fix

Production fix commit: `4d737d366c359c27bde1b0ee0eac93f30d1a61dd`

Contract commit: `29235c4985f76d45133ebc3b170783edb58ac079`

Verified locally:

```text
WhenItFails.Tests
Failed:   0
Passed: 981
Skipped:  0
Total:  981
```

This confirms that ordinary `IBuiltInErrorCatalogContextProvider.LoadAsync(...)` exceptions in `ResetToDefaultsAsync()` become:

```text
Status: Failed
Code: WIF_BUILT_IN_CONTEXT_PROVIDER_FAILED
Message: The bundled default catalog provider failed.
```

without exposing raw dependency exception text.

The existing null-response behavior remains unchanged as `WIF_BUILT_IN_CONTEXT_RESPONSE_NULL`.

### 2026-09-08 — verified RED built-in provider exception contract

Before the production fix, the focused contract failed with:

```text
System.InvalidOperationException:
Sensitive runtime built-in provider detail must not escape.
```

confirming the direct exception leak from `ResetToDefaultsAsync()`.

### 2026-09-08 — verified ErrorCatalogRuntime initializer cancellation contract

Contract commit: `ba3a1137bfcb25f64787b54bff7fada037285b7a`

Verified locally at 980/980 tests GREEN. The exact original `OperationCanceledException` instance from `IErrorCatalogInitializer.InitializeAsync(...)` propagates unchanged. The initializer boundary is complete for the current scope.

## Verification state

- Complete verified continuation baseline: 981/981 tests GREEN.
- Runtime descriptor-service, profile-selection, context-store `GetCurrent()`, and initializer exception/cancellation boundaries are complete for the current scope.
- Built-in provider null-response and ordinary-exception behavior through `ResetToDefaultsAsync()` are verified GREEN.
- `ResetToDefaultsAsync()` excludes `OperationCanceledException` from ordinary exception conversion, but that exact-instance cancellation behavior is not yet locked by a focused contract.

## Recommended verification

The next focused contract should prove that an `OperationCanceledException` from `IBuiltInErrorCatalogContextProvider.LoadAsync(...)` propagates as the exact original instance through `ResetToDefaultsAsync()` rather than becoming `WIF_BUILT_IN_CONTEXT_PROVIDER_FAILED`.

Expected complete-suite count after that new contract passes: 982 tests.

## Next recommended step

Add one focused cancellation contract for the built-in provider through `ResetToDefaultsAsync()` using `Assert.Same(...)` on the exact original `OperationCanceledException` instance.

No production change is expected because the current catch filter already excludes `OperationCanceledException`.

If cancellation passes, then add one focused symmetry contract for the flexible initialization fallback path because `CreateBuiltInFallbackResponseAsync(...)` has its own separate direct `_builtInContextProvider.LoadAsync(...)` invocation site.

Do not harden `_contextStore.Set(...)` in the same step. Keep each dependency boundary isolated, tested, documented here, and committed directly to `master`.
