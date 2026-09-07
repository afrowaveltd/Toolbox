# Implementation status

Last updated: 2026-09-07

This file is the continuation point for `WhenItFails` development. Update it after every implementation, test, catalog, or documentation change that alters the current state or recommended next step.

## Current focus

Hardening runtime and service boundaries against malformed dependency behavior, raw exception leakage, and internally inconsistent responses.

## Current state

- `WhenItFails` provides structured error catalogs, runtime error resolution, profiles, diagnostics, initialization/recovery behavior, and project-local catalog handling.
- `ErrorDescriptorResolver` and `ErrorDescriptorService` exception/cancellation hardening blocks are complete for the current scope.
- `ErrorCatalogRuntime` descriptor-service ordinary-exception and cancellation behavior is complete for the current scope.
- Complete verified continuation baseline before the profile-selection exception contract: 974/974 tests GREEN.
- Profile-selection null-response behavior is covered and returns `WIF_PROFILE_SELECTION_RESPONSE_NULL`.
- `ErrorCatalogRuntime.ResolveProfile(...)` now also converts ordinary `IErrorProfileSelectionService.ResolveByProfileName(...)` exceptions into a stable runtime failure without exposing raw exception text.
- `OperationCanceledException` is deliberately excluded from that conversion and is intended to propagate unchanged.

## Latest committed steps

### 2026-09-07 — ErrorCatalogRuntime profile-selection exception fix

Production fix commit: `67771ee0ea22c24d30adef93327535f3690d98af`

Changed:

`WhenItFails/Services/ErrorCatalogRuntime.cs`

`ResolveProfile(...)` now wraps only the injected `IErrorProfileSelectionService.ResolveByProfileName(...)` call in a narrow exception boundary.

Ordinary profile-selection exceptions become:

```text
Status: Failed
Code: WIF_PROFILE_SELECTION_FAILED
Message: The error profile selection service failed.
```

The original exception message is deliberately not copied into the public runtime response.

The exception filter excludes `OperationCanceledException`, so cancellation continues to propagate naturally.

The existing null-response guard is unchanged and still returns:

```text
Status: Invalid
Code: WIF_PROFILE_SELECTION_RESPONSE_NULL
Message: The error profile selection service returned a null response.
```

The production commit diff was checked after commit and contains only the intended `ResolveProfile(...)` exception boundary.

### 2026-09-07 — verified RED profile-selection exception contract

Contract commit: `c3909b03b3ad8dd0b8a2b200fef636a291fa0bb1`

Focused test:

`WhenItFails.Tests/Services/ErrorCatalogRuntimeProfileSelectionServiceExceptionContractTests.ResolveProfile_WhenProfileSelectionServiceThrows_ReturnsStableFailure`

Observed locally on Linux before the production fix:

```text
Failed: 1
Passed: 0
Skipped: 0
Total: 1
```

Failure:

```text
System.InvalidOperationException:
Sensitive runtime profile selection detail must not escape.
```

The exception escaped directly from `IErrorProfileSelectionService.ResolveByProfileName(...)` through `ErrorCatalogRuntime.ResolveProfile(...)`, confirming the missing runtime-facade exception boundary and raw diagnostic-text leak.

The existing NETSDK1057 preview message and the known CS8767 warning in `ErrorCatalogProviderNullFirstIssueContractTests.cs` are unrelated to this contract.

### 2026-09-07 — verified ErrorCatalogRuntime descriptor-service cancellation contract

Contract commit: `8ed6487106cbf69c95c421f83b4851b46c3c0eff`

Verified locally:

```text
WhenItFails.Tests
Failed:   0
Passed: 974
Skipped:  0
Total:  974
```

The exact original `OperationCanceledException` instance propagates through the runtime descriptor-service boundary.

## Verification state

- Verified continuation baseline: 974/974 tests GREEN before adding the profile-selection exception contract.
- Profile-selection exception contract is verified RED before the production fix.
- Production `ResolveProfile(...)` exception boundary is committed and awaits focused local verification.
- Expected complete-suite count after the new contract passes: 975 tests.

## Recommended verification

Pull current `master` and run the focused profile-selection contract:

```powershell
dotnet test WhenItFails.Tests --filter "FullyQualifiedName~ResolveProfile_WhenProfileSelectionServiceThrows_ReturnsStableFailure"
```

Expected result after the production fix: GREEN.

Then run the complete suite:

```powershell
dotnet test WhenItFails.Tests
```

Expected complete-suite result: 975/975 GREEN.

## Next recommended step

After 975/975 GREEN is confirmed, add one focused cancellation contract proving that an `OperationCanceledException` from `IErrorProfileSelectionService.ResolveByProfileName(...)` propagates as the exact original instance rather than becoming `WIF_PROFILE_SELECTION_FAILED`.

If that passes without production changes, consider the runtime profile-selection boundary complete for the current scope and move to the next distinct runtime dependency boundary, likely `IErrorCatalogContextStore.GetCurrent()` after checking its existing null-response coverage.

Avoid broader refactoring. Keep each step small, tested, documented here, and committed directly to `master`.
