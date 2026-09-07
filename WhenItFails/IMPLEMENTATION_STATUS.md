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

## Latest committed steps

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

This confirms that an ordinary exception from `IErrorDescriptorService.FromId(...)` becomes:

```text
Status: Failed
Code: WIF_DESCRIPTOR_SERVICE_FAILED
Message: The error descriptor service failed.
```

without exposing the original dependency exception text.

The exception filter excludes `OperationCanceledException`, and the existing null-response guard remains independent.

### 2026-09-07 — verified RED ErrorCatalogRuntime descriptor-service exception contract

Contract commit: `5775f368bac0d4d1bdd87d81c99296021d3ee541`

Before the production fix the focused `FromId(...)` contract failed with the raw `InvalidOperationException` text escaping the runtime facade.

### 2026-09-07 — verified ErrorDescriptorService resolver cancellation contract

Contract commit: `0027ba19d27dbe86a10f7dd39f0398f2678051f1`

Verified locally at 970/970 tests GREEN. The exact original `OperationCanceledException` instance propagates through `ErrorDescriptorService`.

## Verification state

- Complete verified continuation baseline: 971/971 tests GREEN.
- `ErrorDescriptorResolver` and `ErrorDescriptorService` ordinary-exception and cancellation behavior are verified for their current dependency boundaries.
- `ErrorCatalogRuntime.FromId(...)` descriptor-service ordinary-exception behavior is verified GREEN.
- `FromName(...)` / `FromCode(...)` runtime descriptor-service exception symmetry has not yet been added.

## Recommended verification

No verification is pending for the 971-test checkpoint.

## Next recommended step

Add focused `FromName(...)` and `FromCode(...)` descriptor-service exception symmetry contracts without changing production code. Expect `FromId(...)` to remain GREEN and the name/code paths to expose the original injected-service exceptions until the runtime facade boundary is centralized.

After symmetry is verified, centralize the runtime descriptor-service exception boundary carefully and then add one focused cancellation contract proving that `OperationCanceledException` still propagates as the exact original instance.

Avoid broader refactoring. Keep each step small, tested, documented here, and committed directly to `master`.
