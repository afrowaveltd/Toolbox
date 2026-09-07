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
- `ErrorCatalogRuntime` stabilizes null descriptor-service responses and routes `FromId(...)`, `FromName(...)`, and `FromCode(...)` through one shared descriptor-service exception boundary.
- Ordinary `IErrorDescriptorService` exceptions become `WIF_DESCRIPTOR_SERVICE_FAILED` without exposing raw dependency text.
- `OperationCanceledException` from the descriptor service propagates through `ErrorCatalogRuntime` as the exact original exception instance.
- The complete `WhenItFails.Tests` suite is verified GREEN at 974/974 tests.
- The runtime descriptor-service exception/cancellation boundary is complete for the current scope.
- The next dependency under inspection is `IErrorProfileSelectionService.ResolveByProfileName(...)`; runtime already guards a null response, but ordinary-exception behavior is not yet covered.

## Latest committed steps

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

The exact original `OperationCanceledException` instance propagates through `ErrorCatalogRuntime` rather than being converted into `WIF_DESCRIPTOR_SERVICE_FAILED`.

No production change was required.

### 2026-09-07 — centralized ErrorCatalogRuntime descriptor-service exception boundary

Production fix commit: `8da3fef0140c081e9119b5097a7343bf24cfafa5`

`FromId(...)`, `FromName(...)`, and `FromCode(...)` delegate through one shared `ResolveDescriptor(...)` helper. Ordinary descriptor-service exceptions become `WIF_DESCRIPTOR_SERVICE_FAILED`; null responses remain a separate `WIF_DESCRIPTOR_SERVICE_RESPONSE_NULL` contract.

### 2026-09-07 — verified ErrorDescriptorService resolver cancellation contract

Contract commit: `0027ba19d27dbe86a10f7dd39f0398f2678051f1`

Verified locally at 970/970 tests GREEN. The exact original `OperationCanceledException` instance propagates through `ErrorDescriptorService`.

## Verification state

- Complete verified continuation baseline: 974/974 tests GREEN.
- `ErrorDescriptorResolver` and `ErrorDescriptorService` ordinary-exception and cancellation behavior are verified for their current dependency boundaries.
- `ErrorCatalogRuntime` descriptor-service ordinary-exception behavior is symmetric and verified for ID, name, and code paths.
- Runtime descriptor-service cancellation is verified and the exact original exception instance propagates.
- Existing profile-selection null-response behavior is covered by `ErrorCatalogRuntimeNullDownstreamResponseContractTests` and returns `WIF_PROFILE_SELECTION_RESPONSE_NULL`.
- No profile-selection ordinary-exception contract exists yet.

## Recommended verification

No verification is pending for the 974-test checkpoint.

## Next recommended step

Add one focused `ResolveProfile(...)` contract where `IErrorProfileSelectionService.ResolveByProfileName(...)` throws an ordinary exception. Require a stable failed runtime response without exposing the dependency exception text.

Use the runtime `WIF_` naming convention for the new contract:

```text
Status: Failed
Code: WIF_PROFILE_SELECTION_FAILED
Message: The error profile selection service failed.
```

Do not change production code until the focused RED state is observed. Preserve `OperationCanceledException` for a later dedicated cancellation contract.

Avoid broader refactoring. Keep each step small, tested, documented here, and committed directly to `master`.
