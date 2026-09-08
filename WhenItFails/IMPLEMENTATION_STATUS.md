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
- Profile-selection null-response behavior remains covered and returns `WIF_PROFILE_SELECTION_RESPONSE_NULL`.
- The complete `WhenItFails.Tests` suite is verified GREEN at 976/976 tests.
- Existing context-store null-response coverage includes `GetCurrentContext()`, all three descriptor entry points, and `ResolveProfile(...)`.

## Latest committed steps

### 2026-09-08 — verified ErrorCatalogRuntime profile-selection cancellation contract

Contract commit: `15be30a6e03f333c9f2153124591aac79a82bf5b`

Verified locally:

```text
WhenItFails.Tests
Failed:   0
Passed: 976
Skipped:  0
Total:  976
```

This confirms that an `OperationCanceledException` thrown by `IErrorProfileSelectionService.ResolveByProfileName(...)` propagates through `ErrorCatalogRuntime.ResolveProfile(...)` as the exact original exception instance.

No production change was required because the existing profile-selection catch filter already excludes `OperationCanceledException`.

### 2026-09-08 — ErrorCatalogRuntime profile-selection exception fix

Production fix commit: `67771ee0ea22c24d30adef93327535f3690d98af`

Ordinary profile-selection exceptions become:

```text
Status: Failed
Code: WIF_PROFILE_SELECTION_FAILED
Message: The error profile selection service failed.
```

without exposing raw dependency exception text.

### 2026-09-08 — verified ErrorCatalogRuntime descriptor-service cancellation contract

Contract commit: `8ed6487106cbf69c95c421f83b4851b46c3c0eff`

Verified locally at 974/974 tests GREEN. The exact original `OperationCanceledException` instance propagates through the runtime descriptor-service boundary.

## Verification state

- Complete verified continuation baseline: 976/976 tests GREEN.
- Runtime descriptor-service ordinary-exception and cancellation behavior is complete for the current scope.
- Runtime profile-selection ordinary-exception and cancellation behavior is complete for the current scope.
- Context-store null-response behavior is already covered across the runtime facade.
- No context-store ordinary-exception contract has yet been established.

## Recommended verification

No verification is pending for the 976-test checkpoint.

## Next recommended step

Add one focused contract for `IErrorCatalogContextStore.GetCurrent()` throwing an ordinary exception, using `ErrorCatalogRuntime.GetCurrentContext()` as the narrowest public path.

Proposed stable runtime contract:

```text
Status: Failed
Code: WIF_CONTEXT_STORE_FAILED
Message: The error catalog context store failed.
```

The raw dependency exception text must not escape. Do not change production code until the focused RED state is observed.

After the ordinary-exception contract is fixed and verified, consider a focused cancellation contract before expanding coverage to the other runtime entry points that share `GetCurrentContextResponse()`.

Avoid broader refactoring. Keep each step small, tested, documented here, and committed directly to `master`.
