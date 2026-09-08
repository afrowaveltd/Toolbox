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
- The complete `WhenItFails.Tests` suite is verified GREEN at 980/980 tests.
- Built-in catalog-provider null-response behavior is already covered for `ResetToDefaultsAsync()` and flexible initialization fallback.
- A focused ordinary-exception contract now defines runtime behavior for `IBuiltInErrorCatalogContextProvider.LoadAsync(...)` through `ResetToDefaultsAsync()`.

## Latest committed steps

### 2026-09-08 — ErrorCatalogRuntime built-in provider exception contract

Contract commit: `29235c4985f76d45133ebc3b170783edb58ac079`

Added:

`WhenItFails.Tests/Services/ErrorCatalogRuntimeBuiltInContextProviderExceptionContractTests.ResetToDefaultsAsync_WhenBuiltInProviderThrows_ReturnsStableFailure`

Contract:

```text
IBuiltInErrorCatalogContextProvider.LoadAsync(...)
    => faulted task with ordinary exception
                         ↓
Status: Failed
Code: WIF_BUILT_IN_CONTEXT_PROVIDER_FAILED
Message: The bundled default catalog provider failed.
```

The injected provider returns a faulted task containing an `InvalidOperationException` with sensitive diagnostic text. The runtime facade must not expose that raw text.

All unrelated dependencies are throwing sentinels. `UnusedContextStore.Set(...)` also throws, proving the test cannot pass by accidentally advancing beyond the provider load boundary.

No production code changed in this step.

Current `ResetToDefaultsAsync()` directly awaits `_builtInContextProvider.LoadAsync(...)`, so this focused contract is expected to be RED with the original exception escaping.

### 2026-09-08 — verified ErrorCatalogRuntime initializer cancellation contract

Contract commit: `ba3a1137bfcb25f64787b54bff7fada037285b7a`

Verified locally:

```text
WhenItFails.Tests
Failed:   0
Passed: 980
Skipped:  0
Total:  980
```

The exact original `OperationCanceledException` instance from `IErrorCatalogInitializer.InitializeAsync(...)` propagates unchanged. The initializer boundary is complete for the current scope.

## Verification state

- Complete verified continuation baseline: 980/980 tests GREEN.
- Runtime descriptor-service, profile-selection, context-store `GetCurrent()`, and initializer exception/cancellation boundaries are complete for the current scope.
- Built-in provider null-response behavior is already covered.
- New built-in-provider ordinary-exception contract is committed and awaits focused local verification.
- Production `ResetToDefaultsAsync()` remains unchanged until the RED state is observed.
- Expected complete-suite count once this contract eventually passes: 981 tests.

## Recommended verification

Pull current `master` and run only the new built-in-provider exception contract:

```powershell
dotnet test WhenItFails.Tests --filter "FullyQualifiedName~ResetToDefaultsAsync_WhenBuiltInProviderThrows_ReturnsStableFailure"
```

Expected current result: RED with the original exception text:

```text
Sensitive runtime built-in provider detail must not escape.
```

## Next recommended step

If the focused contract fails as expected, add the smallest exception boundary around the `_builtInContextProvider.LoadAsync(...)` invocation/await in `ResetToDefaultsAsync()`.

Convert ordinary exceptions into:

```text
Status: Failed
Code: WIF_BUILT_IN_CONTEXT_PROVIDER_FAILED
Message: The bundled default catalog provider failed.
```

while allowing `OperationCanceledException` to propagate unchanged and preserving the existing `WIF_BUILT_IN_CONTEXT_RESPONSE_NULL` contract.

Do not change the flexible fallback path in the same production step. First verify the explicit reset path, then add one focused symmetry contract for the fallback path if needed.

Avoid broader refactoring. Keep each step small, tested, documented here, and committed directly to `master`.
