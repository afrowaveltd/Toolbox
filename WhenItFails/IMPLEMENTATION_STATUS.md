# Implementation status

Last updated: 2026-09-07

This file is the continuation point for `WhenItFails` development. Update it after every implementation, test, catalog, or documentation change that alters the current state or recommended next step.

## Current focus

Hardening runtime and service boundaries against malformed dependency behavior, raw exception leakage, and internally inconsistent responses.

## Current state

- `WhenItFails` provides structured error catalogs, runtime error resolution, profiles, diagnostics, initialization/recovery behavior, and project-local catalog handling.
- `ErrorDescriptorResolver` now has stable contracts for failed/malformed definition responses, null dependency responses, null descriptor-factory results, ordinary dependency exceptions, and cancellation.
- Ordinary exceptions from `IErrorDefinitionResolver` are converted through one shared boundary into `ErrorDefinitionResolverFailed` without exposing raw exception text.
- `OperationCanceledException` from the definition resolver propagates as the exact original instance.
- Ordinary descriptor-factory exceptions become `ErrorDescriptorFactoryFailed`; descriptor-factory cancellation also propagates as the exact original instance.
- The complete `WhenItFails.Tests` suite is verified GREEN at 966/966 tests.
- The `ErrorDescriptorResolver` exception/cancellation hardening block is considered complete for the current scope.
- The next distinct boundary is `ErrorDescriptorService` calling an injected `IErrorDescriptorResolver`: null responses are already stabilized, but direct resolver exceptions are not yet covered by a service-level contract.

## Latest committed steps

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

This confirms that the shared definition-resolver boundary rethrows the exact original `OperationCanceledException` instance rather than converting or wrapping it.

No production change was required.

### 2026-09-07 — centralized definition-resolver exception boundary

Production fix commit: `8be4191d089446893a5686b693821a3c6da230f0`

`CreateById(...)`, `CreateByName(...)`, and `CreateByCode(...)` all delegate through one shared definition-resolution boundary. Ordinary resolver exceptions become:

```text
Status: Failed
Code: ErrorDefinitionResolverFailed
Message: Error definition resolver failed.
```

Raw dependency exception text is not exposed.

### 2026-09-07 — descriptor-factory exception/cancellation boundary

Production fix commit: `717cc33e26534aa30f1634505f5248b38ea4658c`

Ordinary descriptor-factory exceptions become `ErrorDescriptorFactoryFailed`; `OperationCanceledException` is rethrown.

## Verification state

- Complete verified continuation baseline: 966/966 tests GREEN.
- `ErrorDescriptorResolver` ordinary-exception and cancellation behavior is verified for both definition resolution and descriptor creation.
- No pending resolver-level production fix remains from the current hardening sequence.

## Recommended next step

Move to `ErrorDescriptorService`, which already converts a null `IErrorDescriptorResolver` response into:

```text
Status: Invalid
Code: ErrorDescriptorResolverReturnedNull
Message: Error descriptor resolver returned a null response.
```

Add one focused `FromId(...)` contract for an ordinary exception thrown directly by the injected `IErrorDescriptorResolver`. Require a stable failed response without exposing raw exception text, and verify RED before changing production code.

Do not add `FromName(...)` / `FromCode(...)` symmetry or cancellation behavior in the same test-first step.

Avoid broader refactoring. Keep each step small, tested, documented here, and committed directly to `master`.
