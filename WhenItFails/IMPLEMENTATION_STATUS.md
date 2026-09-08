# Implementation status

Last updated: 2026-09-08

This file is the continuation point for `WhenItFails` development. Update it after every implementation, test, catalog, or documentation change that alters the current state or recommended next step.

## Current focus

Hardening runtime and service boundaries against malformed dependency behavior, raw exception leakage, and internally inconsistent responses.

## Current state

- `WhenItFails` provides structured error catalogs, runtime error resolution, profiles, diagnostics, initialization/recovery behavior, and project-local catalog handling.
- `ErrorDescriptorResolver` and `ErrorDescriptorService` exception/cancellation hardening blocks are complete for the current scope.
- `ErrorCatalogRuntime` descriptor-service ordinary-exception and cancellation behavior is complete for the current scope.
- Profile-selection null-response behavior is covered and returns `WIF_PROFILE_SELECTION_RESPONSE_NULL`.
- `ErrorCatalogRuntime.ResolveProfile(...)` converts ordinary `IErrorProfileSelectionService.ResolveByProfileName(...)` exceptions into `WIF_PROFILE_SELECTION_FAILED` without exposing raw exception text.
- `OperationCanceledException` is deliberately excluded from that conversion and is intended to propagate unchanged.
- The complete `WhenItFails.Tests` suite is verified GREEN at 975/975 tests.

## Latest committed steps

### 2026-09-08 — verified ErrorCatalogRuntime profile-selection exception fix

Production fix commit: `67771ee0ea22c24d30adef93327535f3690d98af`

Verified locally after pulling the fix:

```text
WhenItFails.Tests
Failed:   0
Passed: 975
Skipped:  0
Total:  975
```

This confirms that an ordinary exception from `IErrorProfileSelectionService.ResolveByProfileName(...)` becomes:

```text
Status: Failed
Code: WIF_PROFILE_SELECTION_FAILED
Message: The error profile selection service failed.
```

without exposing the original dependency exception text.

The existing null-response behavior remains unchanged.

### 2026-09-07 — ErrorCatalogRuntime profile-selection exception fix

Production fix commit: `67771ee0ea22c24d30adef93327535f3690d98af`

`ResolveProfile(...)` wraps only the injected profile-selection-service invocation in a narrow exception boundary. `OperationCanceledException` is excluded from the catch filter.

### 2026-09-07 — verified RED profile-selection exception contract

Contract commit: `c3909b03b3ad8dd0b8a2b200fef636a291fa0bb1`

Before the production fix the focused contract failed with the raw `InvalidOperationException` text `Sensitive runtime profile selection detail must not escape.` escaping `ErrorCatalogRuntime.ResolveProfile(...)`.

### 2026-09-07 — verified ErrorCatalogRuntime descriptor-service cancellation contract

Contract commit: `8ed6487106cbf69c95c421f83b4851b46c3c0eff`

Verified locally at 974/974 tests GREEN. The exact original `OperationCanceledException` instance propagates through the runtime descriptor-service boundary.

## Verification state

- Complete verified continuation baseline: 975/975 tests GREEN.
- Runtime descriptor-service ordinary-exception and cancellation behavior is verified and complete for the current scope.
- Runtime profile-selection ordinary-exception behavior is verified GREEN.
- Profile-selection cancellation behavior is not yet explicitly locked by a contract.

## Recommended verification

No verification is pending for the 975-test checkpoint.

## Next recommended step

Add one focused cancellation contract proving that an `OperationCanceledException` from `IErrorProfileSelectionService.ResolveByProfileName(...)` propagates as the exact original instance rather than becoming `WIF_PROFILE_SELECTION_FAILED`.

If that passes without production changes, consider the runtime profile-selection boundary complete for the current scope and move to the next distinct runtime dependency boundary, likely `IErrorCatalogContextStore.GetCurrent()` after checking its existing null-response and malformed-response coverage.

Avoid broader refactoring. Keep each step small, tested, documented here, and committed directly to `master`.
