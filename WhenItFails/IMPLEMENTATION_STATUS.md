# Implementation status

Last updated: 2026-09-07

This file is the continuation point for `WhenItFails` development. Update it after every implementation, test, catalog, or documentation change that alters the current state or recommended next step.

## Current focus

Hardening runtime and service boundaries against malformed dependency behavior, raw exception leakage, and internally inconsistent responses.

## Current state

- `WhenItFails` provides structured error catalogs, runtime error resolution, profiles, diagnostics, initialization/recovery behavior, and project-local catalog handling.
- `ErrorDescriptorResolver` has stable contracts for failed/malformed definition responses, null dependency responses, null descriptor-factory results, ordinary dependency exceptions, and cancellation.
- `ErrorDescriptorService` stabilizes null responses from `IErrorDescriptorResolver`, converts ordinary resolver exceptions through one shared boundary into `ErrorDescriptorResolverFailed`, and propagates cancellation unchanged.
- The `ErrorDescriptorResolver` and `ErrorDescriptorService` exception/cancellation hardening blocks are complete for the current scope.
- `ErrorCatalogRuntime` descriptor-service ordinary-exception and cancellation behavior is complete for the current scope.
- The complete `WhenItFails.Tests` suite is verified GREEN at 974/974 tests.
- Existing profile-selection null-response behavior is covered and returns `WIF_PROFILE_SELECTION_RESPONSE_NULL`.
- A new focused runtime contract now defines ordinary-exception behavior for `IErrorProfileSelectionService.ResolveByProfileName(...)`.

## Latest committed steps

### 2026-09-07 — ErrorCatalogRuntime profile-selection exception contract

Contract commit: `c3909b03b3ad8dd0b8a2b200fef636a291fa0bb1`

Added:

`WhenItFails.Tests/Services/ErrorCatalogRuntimeProfileSelectionServiceExceptionContractTests.ResolveProfile_WhenProfileSelectionServiceThrows_ReturnsStableFailure`

Contract:

```text
IErrorProfileSelectionService.ResolveByProfileName(...) => throws ordinary exception
                         ↓
Status: Failed
Code: WIF_PROFILE_SELECTION_FAILED
Message: The error profile selection service failed.
```

The injected profile-selection service throws an `InvalidOperationException` containing sensitive diagnostic text. The runtime facade must not expose that raw text.

The fixture supplies a valid current catalog context and throwing sentinels for unrelated dependencies, so the test isolates only the profile-selection invocation boundary.

No production code changed in this step.

Current `ErrorCatalogRuntime.ResolveProfile(...)` directly invokes the profile-selection service before its existing null-response guard, so this focused contract is expected to be RED with the original exception escaping.

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

The exact original `OperationCanceledException` instance propagates through `ErrorCatalogRuntime`; no production change was required.

### 2026-09-07 — centralized ErrorCatalogRuntime descriptor-service exception boundary

Production fix commit: `8da3fef0140c081e9119b5097a7343bf24cfafa5`

`FromId(...)`, `FromName(...)`, and `FromCode(...)` share one `ResolveDescriptor(...)` helper. Ordinary descriptor-service exceptions become `WIF_DESCRIPTOR_SERVICE_FAILED`; null responses remain independent.

## Verification state

- Complete verified continuation baseline: 974/974 tests GREEN.
- Runtime descriptor-service ordinary-exception and cancellation behavior is verified and complete for the current scope.
- Profile-selection null-response behavior is already covered.
- New profile-selection ordinary-exception contract is committed and awaits focused local verification.
- Production `ErrorCatalogRuntime.ResolveProfile(...)` remains unchanged until the RED state is observed.

## Recommended verification

Pull current `master` and run only the new profile-selection contract:

```powershell
dotnet test WhenItFails.Tests --filter "FullyQualifiedName~ResolveProfile_WhenProfileSelectionServiceThrows_ReturnsStableFailure"
```

Expected current result: RED with the original `InvalidOperationException` text `Sensitive runtime profile selection detail must not escape.` escaping `ErrorCatalogRuntime.ResolveProfile(...)`.

Preserve that focused failure output before changing production code.

## Next recommended step

If the focused profile-selection contract fails as expected, add the smallest exception boundary around the `ResolveProfile(...)` profile-selection-service invocation. Convert ordinary exceptions into `WIF_PROFILE_SELECTION_FAILED` while allowing `OperationCanceledException` to propagate unchanged.

Do not add cancellation or other dependency contracts in the same production step. Verify the single contract first.

Avoid broader refactoring. Keep each step small, tested, documented here, and committed directly to `master`.
