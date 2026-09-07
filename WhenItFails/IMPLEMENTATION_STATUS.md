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
- `ErrorCatalogRuntime` already stabilizes null responses from several injected dependencies.
- `ErrorCatalogRuntime.FromId(...)` converts ordinary `IErrorDescriptorService.FromId(...)` exceptions into `WIF_DESCRIPTOR_SERVICE_FAILED` without exposing raw exception text.
- The complete `WhenItFails.Tests` suite is verified GREEN at 971/971 tests after the runtime `FromId(...)` descriptor-service exception fix.
- New runtime symmetry contracts now require the same descriptor-service ordinary-exception behavior for `FromName(...)` and `FromCode(...)`.

## Latest committed steps

### 2026-09-07 — ErrorCatalogRuntime descriptor-service exception symmetry contracts

Contract commit: `a2d381dff3caf86efd94853cd804693a87767c59`

Updated:

`WhenItFails.Tests/Services/ErrorCatalogRuntimeDescriptorServiceExceptionContractTests.cs`

Coverage now includes:

- `FromId_WhenDescriptorServiceThrows_ReturnsStableFailure`
- `FromName_WhenDescriptorServiceThrows_ReturnsStableFailure`
- `FromCode_WhenDescriptorServiceThrows_ReturnsStableFailure`

All three require:

```text
Status: Failed
Code: WIF_DESCRIPTOR_SERVICE_FAILED
Message: The error descriptor service failed.
```

Each descriptor-service entry point throws its own sensitive diagnostic text. The runtime facade must never expose that text.

No production code changed in this symmetry step. `FromName(...)` and `FromCode(...)` are therefore expected to be RED until the runtime descriptor-service boundary is centralized.

### 2026-09-07 — verified ErrorCatalogRuntime FromId descriptor-service exception fix

Production fix commit: `bd12d740814bb2ad622da69c90d58c276803db3f`

Verified locally:

```text
WhenItFails.Tests
Failed:   0
Passed: 971
Skipped:  0
Total:  971
```

This confirms that an ordinary exception from `IErrorDescriptorService.FromId(...)` becomes `WIF_DESCRIPTOR_SERVICE_FAILED` without exposing raw dependency exception text.

### 2026-09-07 — verified ErrorDescriptorService resolver cancellation contract

Contract commit: `0027ba19d27dbe86a10f7dd39f0398f2678051f1`

Verified locally at 970/970 tests GREEN. The exact original `OperationCanceledException` instance propagates through `ErrorDescriptorService`.

## Verification state

- Complete verified continuation baseline: 971/971 tests GREEN.
- `ErrorDescriptorResolver` and `ErrorDescriptorService` ordinary-exception and cancellation behavior are verified for their current dependency boundaries.
- `ErrorCatalogRuntime.FromId(...)` descriptor-service ordinary-exception behavior is verified GREEN.
- New `FromName(...)` and `FromCode(...)` runtime symmetry contracts are committed and await local verification.
- Production code is unchanged for the symmetry step.

## Recommended verification

Pull current `master` and run the runtime descriptor-service exception contract class:

```powershell
dotnet test WhenItFails.Tests --filter "FullyQualifiedName~ErrorCatalogRuntimeDescriptorServiceExceptionContractTests"
```

Expected current result:

- `FromId(...)`: GREEN
- `FromName(...)`: RED with the original descriptor-service exception escaping
- `FromCode(...)`: RED with the original descriptor-service exception escaping

Expected focused total: 1 passed / 2 failed.

Expected complete-suite count after both new symmetry tests, once fixed: 973 tests.

## Next recommended step

If the two new symmetry contracts fail as expected, centralize the runtime descriptor-service exception conversion so all three public entry points share one stable boundary without duplicating catch logic.

After 973/973 GREEN, add one focused cancellation contract proving that `OperationCanceledException` from `IErrorDescriptorService` still propagates as the exact original instance rather than becoming `WIF_DESCRIPTOR_SERVICE_FAILED`.

Avoid broader refactoring. Keep each step small, tested, documented here, and committed directly to `master`.
