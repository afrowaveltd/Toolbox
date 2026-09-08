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
- The complete `WhenItFails.Tests` suite is verified GREEN at 975/975 tests.
- A focused profile-selection cancellation contract now requires the exact original `OperationCanceledException` instance to propagate unchanged.

## Latest committed steps

### 2026-09-08 — ErrorCatalogRuntime profile-selection cancellation contract

Contract commit: `15be30a6e03f333c9f2153124591aac79a82bf5b`

Updated:

`WhenItFails.Tests/Services/ErrorCatalogRuntimeProfileSelectionServiceExceptionContractTests.cs`

Added:

`ResolveProfile_WhenProfileSelectionServiceCancels_RethrowsSameOperationCanceledException`

Contract:

```text
IErrorProfileSelectionService.ResolveByProfileName(...)
    => OperationCanceledException instance
                         ↓
rethrow the exact same OperationCanceledException instance
```

The test uses `Assert.Same(...)`, so future refactoring cannot silently wrap cancellation, replace it with another cancellation exception, or convert it into `WIF_PROFILE_SELECTION_FAILED`.

The test fixture was lightly refactored through a shared `CreateRuntime(...)` helper; production code is unchanged.

The current `ResolveProfile(...)` catch filter excludes `OperationCanceledException`, so this focused contract is expected to be GREEN without a production fix.

### 2026-09-08 — verified ErrorCatalogRuntime profile-selection exception fix

Production fix commit: `67771ee0ea22c24d30adef93327535f3690d98af`

Verified locally:

```text
WhenItFails.Tests
Failed:   0
Passed: 975
Skipped:  0
Total:  975
```

This confirms that ordinary profile-selection exceptions become:

```text
Status: Failed
Code: WIF_PROFILE_SELECTION_FAILED
Message: The error profile selection service failed.
```

without exposing raw dependency exception text.

### 2026-09-07 — verified ErrorCatalogRuntime descriptor-service cancellation contract

Contract commit: `8ed6487106cbf69c95c421f83b4851b46c3c0eff`

Verified locally at 974/974 tests GREEN. The exact original `OperationCanceledException` instance propagates through the runtime descriptor-service boundary.

## Verification state

- Complete verified continuation baseline: 975/975 tests GREEN.
- Runtime descriptor-service ordinary-exception and cancellation behavior is verified and complete for the current scope.
- Runtime profile-selection ordinary-exception behavior is verified GREEN.
- Profile-selection cancellation contract is committed and awaits focused local verification.
- No production change is expected for the cancellation contract.

## Recommended verification

Pull current `master` and run the focused cancellation contract:

```powershell
dotnet test WhenItFails.Tests --filter "FullyQualifiedName~ResolveProfile_WhenProfileSelectionServiceCancels_RethrowsSameOperationCanceledException"
```

If green, run the complete suite:

```powershell
dotnet test WhenItFails.Tests
```

Expected complete-suite count: 976 tests.

## Next recommended step

After 976/976 GREEN is confirmed, consider the runtime profile-selection boundary complete for the current scope.

Then inspect the next distinct runtime dependency boundary, preferably `IErrorCatalogContextStore.GetCurrent()`. Existing null-response coverage should be confirmed first; if ordinary store exceptions are not yet structured, establish one focused test before changing production code.

Avoid broader refactoring. Keep each step small, tested, documented here, and committed directly to `master`.
