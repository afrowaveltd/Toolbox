# Implementation status

Last updated: 2026-09-07

This file is the continuation point for `WhenItFails` development. Update it after every implementation, test, catalog, or documentation change that alters the current state or recommended next step.

## Current focus

Hardening runtime and service boundaries against malformed dependency behavior, raw exception leakage, and internally inconsistent responses.

## Current state

- `WhenItFails` provides structured error catalogs, runtime error resolution, profiles, diagnostics, initialization/recovery behavior, and project-local catalog handling.
- `ErrorDescriptorResolver` has stable contracts for failed/malformed definition responses, null dependency responses, null descriptor-factory results, ordinary dependency exceptions, and cancellation.
- Ordinary exceptions from `IErrorDefinitionResolver` are converted through one shared boundary into `ErrorDefinitionResolverFailed` without exposing raw exception text.
- `OperationCanceledException` from the definition resolver propagates as the exact original instance.
- Ordinary descriptor-factory exceptions become `ErrorDescriptorFactoryFailed`; descriptor-factory cancellation also propagates as the exact original instance.
- The `ErrorDescriptorResolver` exception/cancellation hardening block is considered complete for the current scope.
- `ErrorDescriptorService` stabilizes null responses from `IErrorDescriptorResolver`.
- Ordinary exceptions from all three `IErrorDescriptorResolver` entry points are converted through one shared service boundary into `ErrorDescriptorResolverFailed` without exposing raw exception text.
- The complete `WhenItFails.Tests` suite is verified GREEN at 969/969 tests after centralizing the service-level resolver exception boundary.

## Latest committed steps

### 2026-09-07 — verified centralized ErrorDescriptorService resolver exception boundary

Production fix commit: `c41256ca8efdf137bff5a2d7ab5287eb10862990`

Verified locally after pulling the centralized fix:

```text
WhenItFails.Tests
Failed:   0
Passed: 969
Skipped:  0
Total:  969
```

This confirms that `FromId(...)`, `FromName(...)`, and `FromCode(...)` all convert ordinary `IErrorDescriptorResolver` exceptions into:

```text
Status: Failed
Code: ErrorDescriptorResolverFailed
Message: Error descriptor resolver failed.
```

without exposing raw resolver exception text.

### 2026-09-07 — centralized ErrorDescriptorService resolver exception boundary

Production fix commit: `c41256ca8efdf137bff5a2d7ab5287eb10862990`

`FromId(...)`, `FromName(...)`, and `FromCode(...)` delegate through one shared `ResolveDescriptor(...)` boundary. Only the selected resolver invocation is inside the exception boundary; `EnsureResponse(...)` remains responsible for null resolver responses.

The exception filter excludes `OperationCanceledException`, so cancellation is intended to propagate naturally.

### 2026-09-07 — verified RED service symmetry state

Symmetry contract commit: `f69558276a3ae696ac6429b66343a9341acdc985`

Focused class before the centralized production fix:

```text
Failed: 2
Passed: 1
Skipped: 0
Total: 3
```

Observed behavior:

- `FromId_WhenResolverThrows_ReturnsStableFailure`: GREEN
- `FromName_WhenResolverThrows_ReturnsStableFailure`: RED with `Sensitive descriptor resolver name detail must not escape.`
- `FromCode_WhenResolverThrows_ReturnsStableFailure`: RED with `Sensitive descriptor resolver code detail must not escape.`

### 2026-09-07 — verified definition-resolver cancellation contract

Contract commit: `94edf27b427ebcba2f387d6526e6d949610d118f`

Verified locally:

```text
WhenItFails.Tests
Failed:   0
Passed: 966
Skipped:  0
Total:  966
```

The exact original `OperationCanceledException` instance propagates from the shared definition-resolver boundary.

## Verification state

- Complete verified continuation baseline: 969/969 tests GREEN.
- `ErrorDescriptorResolver` ordinary-exception and cancellation behavior is verified for both definition resolution and descriptor creation.
- `ErrorDescriptorService` null-response behavior is covered for ID, name, and code paths.
- `ErrorDescriptorService` ordinary resolver-exception behavior is symmetric and verified for ID, name, and code paths.
- Service-level resolver cancellation is not yet explicitly covered by a regression contract.

## Recommended verification

No verification is pending for the current production state. The complete suite is GREEN at 969/969 tests.

## Next recommended step

Add one focused `ErrorDescriptorService` cancellation contract proving that an `OperationCanceledException` thrown by the injected `IErrorDescriptorResolver` propagates as the exact original exception instance rather than becoming `ErrorDescriptorResolverFailed`.

The shared `ResolveDescriptor(...)` exception filter already excludes `OperationCanceledException`, so no production change is expected.

If that contract passes, move to the next distinct service/runtime dependency boundary rather than adding more resolver-exception permutations.

Avoid broader refactoring. Keep each step small, tested, documented here, and committed directly to `master`.
