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
- The complete `WhenItFails.Tests` suite is verified GREEN at 982/982 tests.
- Flexible initialization fallback still contains a separate direct `_builtInContextProvider.LoadAsync(...)` invocation and is the next boundary under test.

## Latest committed steps

### 2026-09-08 — verified ErrorCatalogRuntime built-in provider cancellation contract

Contract commit: `7f7ef67223cc67614282f2a43bd0284bad4a55e1`

Verified locally:

```text
WhenItFails.Tests
Failed:   0
Passed: 982
Skipped:  0
Total:  982
```

The exact original `OperationCanceledException` instance from `IBuiltInErrorCatalogContextProvider.LoadAsync(...)` propagates unchanged through `ResetToDefaultsAsync()`.

Together with the ordinary-exception and null-response contracts, this completes the explicit reset provider boundary for the current scope.

### 2026-09-08 — ErrorCatalogRuntime built-in provider exception fix

Production fix commit: `4d737d366c359c27bde1b0ee0eac93f30d1a61dd`

Ordinary provider exceptions through `ResetToDefaultsAsync()` become:

```text
Status: Failed
Code: WIF_BUILT_IN_CONTEXT_PROVIDER_FAILED
Message: The bundled default catalog provider failed.
```

without exposing raw dependency exception text.

## Verification state

- Complete verified continuation baseline: 982/982 tests GREEN.
- Runtime descriptor-service, profile-selection, context-store `GetCurrent()`, initializer, and explicit reset built-in-provider exception/cancellation boundaries are complete for the current scope.
- Flexible initialization fallback null-response behavior is already covered and preserves provider failure details in `WhenItFails.FallbackFailure.*` metadata.
- Flexible fallback ordinary-exception behavior has not yet been protected or contract-tested.

## Recommended verification

No pending verification for the current committed baseline. The complete suite is verified GREEN at 982/982 tests.

## Next recommended step

Add one focused ordinary-exception contract for the flexible initialization fallback path.

The contract should force configured initialization to fail, provide no previous context, and make `IBuiltInErrorCatalogContextProvider.LoadAsync(...)` throw an ordinary exception.

The flexible fallback wrapper should remain the public top-level contract:

```text
Status: Failed
Code: WIF_DEFAULT_FALLBACK_FAILED
```

while provider diagnostics are normalized into metadata:

```text
WhenItFails.FallbackFailure.Code = WIF_BUILT_IN_CONTEXT_PROVIDER_FAILED
WhenItFails.FallbackFailure.Status = Failed
WhenItFails.FallbackFailure.Message = The bundled default catalog provider failed.
```

The raw provider exception text must not escape.

Do not harden `_contextStore.Set(...)` or add cancellation in the same step. First observe the focused RED state for this distinct provider invocation site.

Avoid broader refactoring. Keep each step small, tested, documented here, and committed directly to `master`.