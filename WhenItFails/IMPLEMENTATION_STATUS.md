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
- `ErrorDescriptorService` already stabilizes null responses from `IErrorDescriptorResolver`.
- A new focused contract now defines service behavior when the injected descriptor resolver throws an ordinary exception from `CreateById(...)`.

## Latest committed steps

### 2026-09-07 — ErrorDescriptorService resolver-exception contract

Contract commit: `42c47e9c84dab0e6c03da5d6777c9154cf5546f8`

Added:

`WhenItFails.Tests/Services/ErrorDescriptorServiceResolverExceptionContractTests.FromId_WhenResolverThrows_ReturnsStableFailure`

Contract:

```text
IErrorDescriptorResolver.CreateById(...) => throws ordinary exception
                         ↓
Status: Failed
Code: ErrorDescriptorResolverFailed
Message: Error descriptor resolver failed.
```

The injected resolver throws an `InvalidOperationException` containing sensitive diagnostic text. The service-level response must not expose that raw text.

No production code changed in this step.

The current `ErrorDescriptorService.FromId(...)` directly invokes the resolver before `EnsureResponse(...)`, so the focused contract is expected to be RED with the original exception escaping.

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

`CreateById(...)`, `CreateByName(...)`, and `CreateByCode(...)` all delegate through one shared definition-resolution boundary. Ordinary resolver exceptions become `ErrorDefinitionResolverFailed` without exposing raw dependency text.

### 2026-09-07 — descriptor-factory exception/cancellation boundary

Production fix commit: `717cc33e26534aa30f1634505f5248b38ea4658c`

Ordinary descriptor-factory exceptions become `ErrorDescriptorFactoryFailed`; `OperationCanceledException` is rethrown.

## Verification state

- Complete verified continuation baseline: 966/966 tests GREEN.
- `ErrorDescriptorResolver` ordinary-exception and cancellation behavior is verified for both definition resolution and descriptor creation.
- `ErrorDescriptorService` null-response behavior is already covered.
- New service resolver-exception contract is committed and awaits focused local verification.
- Production `ErrorDescriptorService` remains unchanged until the RED state is observed.

## Recommended verification

Pull current `master` and run only the new service contract:

```powershell
dotnet test WhenItFails.Tests --filter "FullyQualifiedName~FromId_WhenResolverThrows_ReturnsStableFailure"
```

Expected current result: RED with the original `InvalidOperationException` from the injected `IErrorDescriptorResolver.CreateById(...)` escaping `ErrorDescriptorService.FromId(...)`.

Preserve that focused failure output before changing production code.

## Next recommended step

If the focused service contract fails as expected, add the smallest exception boundary around the `FromId(...)` descriptor-resolver invocation. Convert ordinary exceptions into the stable `ErrorDescriptorResolverFailed` response while allowing `OperationCanceledException` to propagate.

Do not add `FromName(...)` / `FromCode(...)` symmetry or cancellation tests in the same production step. Verify the single contract first.

Avoid broader refactoring. Keep each step small, tested, documented here, and committed directly to `master`.
