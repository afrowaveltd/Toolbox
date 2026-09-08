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
- A focused built-in-provider cancellation contract now requires the exact original `OperationCanceledException` instance to propagate unchanged through `ResetToDefaultsAsync()`.

## Latest committed steps

### 2026-09-08 — ErrorCatalogRuntime built-in provider cancellation contract

Contract commit: `7f7ef67223cc67614282f2a43bd0284bad4a55e1`

Updated:

`WhenItFails.Tests/Services/ErrorCatalogRuntimeBuiltInContextProviderExceptionContractTests.cs`

Added:

`ResetToDefaultsAsync_WhenBuiltInProviderCancels_RethrowsSameOperationCanceledException`

Contract:

```text
IBuiltInErrorCatalogContextProvider.LoadAsync(...)
    => faulted task carrying an OperationCanceledException instance
                         ↓
rethrow the exact same OperationCanceledException instance
```

The test uses `Assert.Same(...)`, so future refactoring cannot wrap cancellation, replace it with another cancellation exception, or convert it into `WIF_BUILT_IN_CONTEXT_PROVIDER_FAILED`.

The fixture now uses a shared `CreateRuntime(...)` helper; all unrelated dependencies remain throwing sentinels.

No production code changed in this step. The current `ResetToDefaultsAsync()` catch filter excludes `OperationCanceledException`, so this focused contract is expected to be GREEN.

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

This confirms that ordinary built-in-provider exceptions through `ResetToDefaultsAsync()` become:

```text
Status: Failed
Code: WIF_BUILT_IN_CONTEXT_PROVIDER_FAILED
Message: The bundled default catalog provider failed.
```

without exposing raw dependency exception text.

### 2026-09-08 — verified ErrorCatalogRuntime initializer cancellation contract

Contract commit: `ba3a1137bfcb25f64787b54bff7fada037285b7a`

Verified locally at 980/980 tests GREEN. The initializer boundary is complete for the current scope.

## Verification state

- Complete verified continuation baseline: 981/981 tests GREEN.
- Runtime descriptor-service, profile-selection, context-store `GetCurrent()`, and initializer exception/cancellation boundaries are complete for the current scope.
- Built-in provider null-response and ordinary-exception behavior through `ResetToDefaultsAsync()` are verified GREEN.
- Built-in-provider cancellation contract through `ResetToDefaultsAsync()` is committed and awaits focused local verification.
- No production change is expected for this cancellation contract.

## Recommended verification

Pull current `master` and run the focused cancellation contract:

```powershell
dotnet test WhenItFails.Tests --filter "FullyQualifiedName~ResetToDefaultsAsync_WhenBuiltInProviderCancels_RethrowsSameOperationCanceledException"
```

If green, run the complete suite:

```powershell
dotnet test WhenItFails.Tests
```

Expected complete-suite count: 982 tests.

## Next recommended step

After 982/982 GREEN is confirmed, consider the explicit `ResetToDefaultsAsync()` built-in-provider boundary complete for the current scope.

Then add one focused ordinary-exception symmetry contract for the flexible initialization fallback path because `CreateBuiltInFallbackResponseAsync(...)` contains a separate direct `_builtInContextProvider.LoadAsync(...)` invocation site that is not protected by the `ResetToDefaultsAsync()` boundary.

Do not harden `_contextStore.Set(...)` in the same step. Keep each dependency boundary isolated, tested, documented here, and committed directly to `master`.
