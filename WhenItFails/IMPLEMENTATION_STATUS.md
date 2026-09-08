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
- The complete `WhenItFails.Tests` suite is verified GREEN at 980/980 tests before the built-in-provider exception contract.
- Built-in catalog-provider null-response behavior is already covered for `ResetToDefaultsAsync()` and flexible initialization fallback.
- `ResetToDefaultsAsync()` now converts ordinary `IBuiltInErrorCatalogContextProvider.LoadAsync(...)` exceptions into `WIF_BUILT_IN_CONTEXT_PROVIDER_FAILED` without exposing raw dependency exception text.
- `OperationCanceledException` is deliberately excluded from that conversion and is intended to propagate unchanged.

## Latest committed steps

### 2026-09-08 — ErrorCatalogRuntime built-in provider exception fix

Production fix commit: `4d737d366c359c27bde1b0ee0eac93f30d1a61dd`

Changed:

`WhenItFails/Services/ErrorCatalogRuntime.cs`

Only the `_builtInContextProvider.LoadAsync(...)` invocation/await in `ResetToDefaultsAsync()` is wrapped in a narrow exception boundary.

Ordinary provider exceptions now become:

```text
Status: Failed
Code: WIF_BUILT_IN_CONTEXT_PROVIDER_FAILED
Message: The bundled default catalog provider failed.
```

The original exception message is not copied into the public runtime response.

The catch filter excludes `OperationCanceledException`, so cancellation continues to propagate naturally.

The existing null-response contract remains outside the exception boundary and unchanged:

```text
Status: Invalid
Code: WIF_BUILT_IN_CONTEXT_RESPONSE_NULL
Message: The bundled default catalog provider returned a null response.
```

The production diff was checked after commit and contains only the intended `ResetToDefaultsAsync()` provider-load boundary.

### 2026-09-08 — verified RED built-in provider exception contract

Contract commit: `29235c4985f76d45133ebc3b170783edb58ac079`

Focused test:

`WhenItFails.Tests/Services/ErrorCatalogRuntimeBuiltInContextProviderExceptionContractTests.ResetToDefaultsAsync_WhenBuiltInProviderThrows_ReturnsStableFailure`

Observed locally on Windows before the production fix:

```text
Failed: 1
Passed: 0
Skipped: 0
Total: 1
```

Failure:

```text
System.InvalidOperationException:
Sensitive runtime built-in provider detail must not escape.
```

The faulted task exception escaped directly through `ErrorCatalogRuntime.ResetToDefaultsAsync(...)`, confirming the missing runtime-facade provider boundary.

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

- Verified continuation baseline before the new built-in-provider contract: 980/980 tests GREEN.
- Runtime descriptor-service, profile-selection, context-store `GetCurrent()`, and initializer exception/cancellation boundaries are complete for the current scope.
- Built-in provider null-response behavior is already covered.
- Built-in-provider ordinary-exception contract is verified RED before the production fix.
- Production `ResetToDefaultsAsync()` provider exception boundary is committed and awaits focused local verification.
- Expected complete-suite count after the new contract passes: 981 tests.

## Recommended verification

Pull current `master` and run the focused built-in-provider contract:

```powershell
dotnet test WhenItFails.Tests --filter "FullyQualifiedName~ResetToDefaultsAsync_WhenBuiltInProviderThrows_ReturnsStableFailure"
```

Expected result after the production fix: GREEN.

Then run the complete suite:

```powershell
dotnet test WhenItFails.Tests
```

Expected complete-suite result: 981/981 GREEN.

## Next recommended step

After 981/981 GREEN is confirmed, add one focused cancellation contract proving that an `OperationCanceledException` from `IBuiltInErrorCatalogContextProvider.LoadAsync(...)` propagates as the exact original instance through `ResetToDefaultsAsync()` rather than becoming `WIF_BUILT_IN_CONTEXT_PROVIDER_FAILED`.

If that passes without production changes, add one focused symmetry contract for the flexible initialization fallback path because `CreateBuiltInFallbackResponseAsync(...)` currently has its own direct provider await and is a distinct invocation site.

Do not harden `_contextStore.Set(...)` in the same step. Keep each dependency boundary isolated, tested, documented here, and committed directly to `master`.
