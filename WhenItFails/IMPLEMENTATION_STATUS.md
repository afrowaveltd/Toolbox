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
- The complete `WhenItFails.Tests` suite is verified GREEN at 973/973 tests after centralizing the runtime descriptor-service exception boundary.
- A focused runtime cancellation contract now requires the exact original `OperationCanceledException` instance from `IErrorDescriptorService` to propagate unchanged.

## Latest committed steps

### 2026-09-07 — ErrorCatalogRuntime descriptor-service cancellation contract

Contract commit: `8ed6487106cbf69c95c421f83b4851b46c3c0eff`

Updated:

`WhenItFails.Tests/Services/ErrorCatalogRuntimeDescriptorServiceExceptionContractTests.cs`

Added:

`FromId_WhenDescriptorServiceCancels_RethrowsSameOperationCanceledException`

Contract:

```text
IErrorDescriptorService.FromId(...) => OperationCanceledException instance
                         ↓
rethrow the exact same OperationCanceledException instance
```

The test uses `Assert.Same(...)`, so future refactoring cannot silently wrap cancellation, replace it with another cancellation exception, or convert it into `WIF_DESCRIPTOR_SERVICE_FAILED`.

The fixture still provides a valid current catalog context, so the test isolates only the descriptor-service invocation boundary.

No production code changed in this step. The shared runtime `ResolveDescriptor(...)` exception filter already excludes `OperationCanceledException`, so the focused contract is expected to be GREEN.

### 2026-09-07 — verified centralized ErrorCatalogRuntime descriptor-service boundary

Production fix commit: `8da3fef0140c081e9119b5097a7343bf24cfafa5`

Verified locally:

```text
WhenItFails.Tests
Failed:   0
Passed: 973
Skipped:  0
Total:  973
```

This confirms that `FromId(...)`, `FromName(...)`, and `FromCode(...)` all convert ordinary `IErrorDescriptorService` exceptions into:

```text
Status: Failed
Code: WIF_DESCRIPTOR_SERVICE_FAILED
Message: The error descriptor service failed.
```

without exposing raw descriptor-service exception text.

### 2026-09-07 — centralized ErrorCatalogRuntime descriptor-service exception boundary

Production fix commit: `8da3fef0140c081e9119b5097a7343bf24cfafa5`

`FromId(...)`, `FromName(...)`, and `FromCode(...)` delegate through one shared `ResolveDescriptor(...)` helper. Only the selected descriptor-service invocation is inside the exception boundary.

### 2026-09-07 — verified ErrorDescriptorService resolver cancellation contract

Contract commit: `0027ba19d27dbe86a10f7dd39f0398f2678051f1`

Verified locally at 970/970 tests GREEN. The exact original `OperationCanceledException` instance propagates through `ErrorDescriptorService`.

## Verification state

- Complete verified continuation baseline: 973/973 tests GREEN.
- `ErrorDescriptorResolver` and `ErrorDescriptorService` ordinary-exception and cancellation behavior are verified for their current dependency boundaries.
- `ErrorCatalogRuntime` descriptor-service ordinary-exception behavior is symmetric and verified for ID, name, and code paths.
- Runtime descriptor-service cancellation contract is committed and awaits focused local verification.
- No production change is expected for the cancellation contract.

## Recommended verification

Pull current `master` and run the focused runtime cancellation contract:

```powershell
dotnet test WhenItFails.Tests --filter "FullyQualifiedName~FromId_WhenDescriptorServiceCancels_RethrowsSameOperationCanceledException"
```

If green, run the complete suite:

```powershell
dotnet test WhenItFails.Tests
```

Expected complete-suite count: 974 tests.

## Next recommended step

After 974/974 GREEN is confirmed, consider the `ErrorCatalogRuntime` descriptor-service exception/cancellation boundary complete for the current scope and move to another injected dependency.

A strong next candidate is `IErrorProfileSelectionService.ResolveByProfileName(...)`: the runtime already guards a null response, but its ordinary-exception behavior should be inspected for a missing structured boundary. The context-store invocation is another candidate and should be checked against its existing null-response contracts before choosing.

Avoid broader refactoring. Keep each step small, tested, documented here, and committed directly to `master`.
