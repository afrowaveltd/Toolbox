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

## Latest committed steps

### 2026-09-07 — verified centralized ErrorCatalogRuntime descriptor-service boundary

Production fix commit: `8da3fef0140c081e9119b5097a7343bf24cfafa5`

Verified locally after pulling the centralized fix:

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

The shared runtime boundary still keeps null-response handling independent and excludes `OperationCanceledException` from ordinary failure conversion.

### 2026-09-07 — centralized ErrorCatalogRuntime descriptor-service exception boundary

Production fix commit: `8da3fef0140c081e9119b5097a7343bf24cfafa5`

`FromId(...)`, `FromName(...)`, and `FromCode(...)` delegate through one shared `ResolveDescriptor(...)` helper. Only the selected descriptor-service invocation is inside the exception boundary.

### 2026-09-07 — verified RED runtime descriptor-service symmetry state

Symmetry contract commit: `a2d381dff3caf86efd94853cd804693a87767c59`

Before the centralized fix, the focused contract class produced:

```text
Failed: 2
Passed: 1
Skipped: 0
Total: 3
```

with raw resolver text escaping from the name/code paths.

### 2026-09-07 — verified ErrorDescriptorService resolver cancellation contract

Contract commit: `0027ba19d27dbe86a10f7dd39f0398f2678051f1`

Verified locally at 970/970 tests GREEN. The exact original `OperationCanceledException` instance propagates through `ErrorDescriptorService`.

## Verification state

- Complete verified continuation baseline: 973/973 tests GREEN.
- `ErrorDescriptorResolver` and `ErrorDescriptorService` ordinary-exception and cancellation behavior are verified for their current dependency boundaries.
- `ErrorCatalogRuntime` descriptor-service ordinary-exception behavior is symmetric and verified for ID, name, and code paths.
- Runtime descriptor-service cancellation behavior is not yet explicitly locked by a focused contract.

## Recommended verification

No verification is pending for the 973-test checkpoint.

## Next recommended step

Add one focused cancellation contract proving that `OperationCanceledException` from `IErrorDescriptorService` propagates as the exact original instance rather than becoming `WIF_DESCRIPTOR_SERVICE_FAILED`.

No production change is expected because the shared `ResolveDescriptor(...)` exception filter already excludes `OperationCanceledException`.

If that contract passes, move to the next distinct `ErrorCatalogRuntime` dependency boundary. A strong next candidate is the profile-selection service or context-store invocation, depending on existing contract coverage.

Avoid broader refactoring. Keep each step small, tested, documented here, and committed directly to `master`.
