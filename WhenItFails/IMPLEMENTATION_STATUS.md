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
- The explicit `ResetToDefaultsAsync()` built-in-provider null-response, ordinary-exception, and cancellation boundary is complete for the current scope.
- Flexible initialization fallback has its own provider-load boundary. Null-response and ordinary-exception behavior are now verified.
- The complete `WhenItFails.Tests` suite is verified GREEN at 983/983 tests.

## Latest committed steps

### 2026-09-08 — verified flexible fallback built-in-provider exception fix

Production fix commit: `3a4abe0052d04b4112d9150a7a7e8d3cc749568b`

Contract commit: `76de742808ee09d05fedda1ef9089b67e3a1ab90`

Verified locally:

```text
WhenItFails.Tests
Failed:   0
Passed: 983
Skipped:  0
Total:  983
```

Ordinary provider exceptions from `CreateBuiltInFallbackResponseAsync(...)` are normalized to:

```text
Status: Failed
Code: WIF_BUILT_IN_CONTEXT_PROVIDER_FAILED
Message: The bundled default catalog provider failed.
```

and then wrapped by the established flexible fallback contract:

```text
Status: Failed
Code: WIF_DEFAULT_FALLBACK_FAILED
Message: The configured error catalog failed and the bundled default catalog could not be activated.
```

with provider diagnostics preserved only through normalized metadata:

```text
WhenItFails.FallbackFailure.Code = WIF_BUILT_IN_CONTEXT_PROVIDER_FAILED
WhenItFails.FallbackFailure.Status = Failed
WhenItFails.FallbackFailure.Message = The bundled default catalog provider failed.
```

The sensitive raw dependency exception text does not escape.

### 2026-09-08 — verified explicit reset built-in-provider cancellation contract

Contract commit: `7f7ef67223cc67614282f2a43bd0284bad4a55e1`

Verified locally at 982/982 tests GREEN. The exact original `OperationCanceledException` instance from `IBuiltInErrorCatalogContextProvider.LoadAsync(...)` propagates unchanged through `ResetToDefaultsAsync()`.

## Verification state

- Complete verified continuation baseline: 983/983 tests GREEN.
- Runtime descriptor-service, profile-selection, context-store `GetCurrent()`, initializer, and explicit reset built-in-provider exception/cancellation boundaries are complete for the current scope.
- Flexible fallback null-response and ordinary-exception behavior are verified GREEN.
- The flexible fallback provider catch filter excludes `OperationCanceledException`, but exact-instance cancellation behavior is not yet locked by a focused contract.

## Recommended verification

The 983-test baseline is verified GREEN. No production change is pending.

## Next recommended step

Add one focused cancellation contract proving that an `OperationCanceledException` from the flexible fallback provider invocation propagates as the exact original exception instance rather than becoming `WIF_BUILT_IN_CONTEXT_PROVIDER_FAILED` or `WIF_DEFAULT_FALLBACK_FAILED`.

If that passes without production changes, consider the built-in provider boundary complete for both explicit reset and flexible fallback paths, then inspect `_contextStore.Set(...)` as the next distinct runtime dependency boundary.

Avoid broader refactoring. Keep each step small, tested, documented here, and committed directly to `master`.