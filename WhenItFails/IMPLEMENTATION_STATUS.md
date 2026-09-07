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
- The complete `WhenItFails.Tests` suite is verified GREEN at 970/970 tests.
- `ErrorCatalogRuntime` already stabilizes null responses from several injected dependencies.
- A new focused runtime contract now defines behavior when the injected `IErrorDescriptorService.FromId(...)` throws an ordinary exception.

## Latest committed steps

### 2026-09-07 — ErrorCatalogRuntime descriptor-service exception contract

Contract commit: `5775f368bac0d4d1bdd87d81c99296021d3ee541`

Added:

`WhenItFails.Tests/Services/ErrorCatalogRuntimeDescriptorServiceExceptionContractTests.FromId_WhenDescriptorServiceThrows_ReturnsStableFailure`

Contract:

```text
IErrorDescriptorService.FromId(...) => throws ordinary exception
                         ↓
Status: Failed
Code: WIF_DESCRIPTOR_SERVICE_FAILED
Message: The error descriptor service failed.
```

The injected descriptor service throws an `InvalidOperationException` containing sensitive diagnostic text. The runtime facade must not expose that raw text.

The fixture provides a valid current `ErrorCatalogContext`, so the test isolates only the descriptor-service invocation boundary. Initializer, built-in provider, and profile-selection dependencies are throwing sentinels and must not be invoked.

No production code changed in this step.

Current `ErrorCatalogRuntime.FromId(...)` directly calls `_descriptorService.FromId(...)` after obtaining a valid context, so this focused contract is expected to be RED with the original exception escaping.

### 2026-09-07 — verified ErrorDescriptorService resolver cancellation contract

Contract commit: `0027ba19d27dbe86a10f7dd39f0398f2678051f1`

Verified locally:

```text
WhenItFails.Tests
Failed:   0
Passed: 970
Skipped:  0
Total:  970
```

This confirms that `OperationCanceledException` thrown by the injected `IErrorDescriptorResolver` passes through `ErrorDescriptorService` as the exact original exception instance.

No production change was required.

### 2026-09-07 — centralized ErrorDescriptorService resolver exception boundary

Production fix commit: `c41256ca8efdf137bff5a2d7ab5287eb10862990`

`FromId(...)`, `FromName(...)`, and `FromCode(...)` delegate through one shared service boundary. Ordinary resolver exceptions become `ErrorDescriptorResolverFailed`; null responses remain handled independently.

## Verification state

- Complete verified continuation baseline: 970/970 tests GREEN.
- `ErrorDescriptorResolver` and `ErrorDescriptorService` ordinary-exception and cancellation behavior are verified for their current dependency boundaries.
- New `ErrorCatalogRuntime.FromId(...)` descriptor-service exception contract is committed and awaits focused local verification.
- Production `ErrorCatalogRuntime` remains unchanged until the focused RED state is observed.

## Recommended verification

Pull current `master` and run only the new runtime contract:

```powershell
dotnet test WhenItFails.Tests --filter "FullyQualifiedName~FromId_WhenDescriptorServiceThrows_ReturnsStableFailure"
```

Expected current result: RED with the original `InvalidOperationException` from the injected `IErrorDescriptorService.FromId(...)` escaping the runtime facade.

Preserve that focused failure output before changing production code.

## Next recommended step

If the focused runtime contract fails as expected, add the smallest exception boundary around the `ErrorCatalogRuntime.FromId(...)` descriptor-service invocation. Convert ordinary exceptions into `WIF_DESCRIPTOR_SERVICE_FAILED` while allowing `OperationCanceledException` to propagate.

Do not add `FromName(...)` / `FromCode(...)` symmetry or cancellation tests in the same production step. Verify the single contract first.

Avoid broader refactoring. Keep each step small, tested, documented here, and committed directly to `master`.
