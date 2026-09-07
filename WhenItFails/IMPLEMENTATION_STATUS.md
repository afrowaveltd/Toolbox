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
- The complete `WhenItFails.Tests` suite is verified GREEN at 970/970 tests before the new runtime descriptor-service exception contract.
- `ErrorCatalogRuntime` already stabilizes null responses from several injected dependencies.
- `ErrorCatalogRuntime.FromId(...)` now also converts ordinary `IErrorDescriptorService.FromId(...)` exceptions into a stable runtime failure without exposing the original exception text.

## Latest committed steps

### 2026-09-07 — ErrorCatalogRuntime descriptor-service exception fix for FromId

Production fix commit: `bd12d740814bb2ad622da69c90d58c276803db3f`

Changed:

`WhenItFails/Services/ErrorCatalogRuntime.cs`

`FromId(...)` now wraps only the injected `IErrorDescriptorService.FromId(...)` invocation in a narrow exception boundary.

Ordinary descriptor-service exceptions become:

```text
Status: Failed
Code: WIF_DESCRIPTOR_SERVICE_FAILED
Message: The error descriptor service failed.
```

The original exception message is deliberately not copied into the public runtime response.

The exception filter excludes `OperationCanceledException`, so cancellation continues to propagate naturally.

The existing null-response guard remains unchanged and still converts a null descriptor-service response into `WIF_DESCRIPTOR_SERVICE_RESPONSE_NULL`.

This production step is intentionally limited to `FromId(...)`. `FromName(...)` and `FromCode(...)` remain unchanged until the focused contract is verified GREEN.

### 2026-09-07 — verified RED ErrorCatalogRuntime descriptor-service exception contract

Contract commit: `5775f368bac0d4d1bdd87d81c99296021d3ee541`

Focused test:

`WhenItFails.Tests/Services/ErrorCatalogRuntimeDescriptorServiceExceptionContractTests.FromId_WhenDescriptorServiceThrows_ReturnsStableFailure`

Observed locally on Windows before the production fix:

```text
Failed: 1
Passed: 0
Skipped: 0
Total: 1
```

Failure:

```text
System.InvalidOperationException:
Sensitive runtime descriptor service detail must not escape.
```

The exception escaped directly from `IErrorDescriptorService.FromId(...)` through `ErrorCatalogRuntime.FromId(...)`, confirming the missing runtime-facade exception boundary and raw diagnostic-text leak.

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

- Complete verified continuation baseline before the new runtime contract: 970/970 tests GREEN.
- `ErrorDescriptorResolver` and `ErrorDescriptorService` ordinary-exception and cancellation behavior are verified for their current dependency boundaries.
- `ErrorCatalogRuntime.FromId(...)` descriptor-service exception contract is verified RED before the production fix.
- Production runtime `FromId(...)` exception boundary is committed and awaits focused local verification and then the complete `WhenItFails.Tests` suite.
- `FromName(...)` / `FromCode(...)` runtime descriptor-service exception symmetry has not yet been added.

## Recommended verification

Pull current `master` and run the focused runtime contract:

```powershell
dotnet test WhenItFails.Tests --filter "FullyQualifiedName~FromId_WhenDescriptorServiceThrows_ReturnsStableFailure"
```

If green, run the complete suite:

```powershell
dotnet test WhenItFails.Tests
```

Expected complete-suite count: 971 tests.

## Next recommended step

After 971/971 GREEN is confirmed, add focused `FromName(...)` and `FromCode(...)` descriptor-service exception symmetry contracts before centralizing the runtime-facade descriptor-service boundary.

After symmetry is verified, add one focused cancellation contract proving that `OperationCanceledException` from `IErrorDescriptorService` still propagates as the exact original instance rather than becoming `WIF_DESCRIPTOR_SERVICE_FAILED`.

Avoid broader refactoring. Keep each step small, tested, documented here, and committed directly to `master`.
