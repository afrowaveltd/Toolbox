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
- `ErrorDescriptorService` stabilizes null responses from `IErrorDescriptorResolver`.
- `ErrorDescriptorService.FromId(...)` now also converts ordinary resolver exceptions into a stable failed response without exposing the original exception text.

## Latest committed steps

### 2026-09-07 — ErrorDescriptorService resolver-exception fix for FromId

Production fix commit: `5db0ebd6773afd21a50b83824e6d4a533ac5b377`

Changed:

`WhenItFails/Services/ErrorDescriptorService.cs`

`FromId(...)` now wraps only the injected `IErrorDescriptorResolver.CreateById(...)` call in a narrow exception boundary.

Ordinary resolver exceptions become:

```text
Status: Failed
Code: ErrorDescriptorResolverFailed
Message: Error descriptor resolver failed.
```

The original exception message is deliberately not copied into the public response.

The exception filter excludes `OperationCanceledException`, so cancellation continues to propagate naturally.

The existing `EnsureResponse(...)` null-response guard remains unchanged and still converts a null resolver response into `ErrorDescriptorResolverReturnedNull`.

This production step is intentionally limited to `FromId(...)`. `FromName(...)` and `FromCode(...)` remain unchanged until the focused contract is verified GREEN.

### 2026-09-07 — verified RED ErrorDescriptorService resolver-exception contract

Contract commit: `42c47e9c84dab0e6c03da5d6777c9154cf5546f8`

Focused test:

`WhenItFails.Tests/Services/ErrorDescriptorServiceResolverExceptionContractTests.FromId_WhenResolverThrows_ReturnsStableFailure`

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
Sensitive descriptor resolver detail must not escape.
```

The exception escaped directly from `IErrorDescriptorResolver.CreateById(...)` through `ErrorDescriptorService.FromId(...)`, confirming the missing service-level exception boundary and raw diagnostic-text leak.

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
- Service resolver-exception contract is verified RED before the production fix.
- Production `FromId(...)` exception boundary is committed and awaits focused local verification and then the complete `WhenItFails.Tests` suite.
- `FromName(...)` / `FromCode(...)` resolver-exception symmetry has not yet been added.

## Recommended verification

Pull current `master` and run the focused service contract:

```powershell
dotnet test WhenItFails.Tests --filter "FullyQualifiedName~FromId_WhenResolverThrows_ReturnsStableFailure"
```

If green, run the complete suite:

```powershell
dotnet test WhenItFails.Tests
```

Expected complete-suite count: 967 tests.

## Next recommended step

After 967/967 GREEN is confirmed, add focused `FromName(...)` and `FromCode(...)` symmetry contracts before centralizing the service-level descriptor-resolver exception boundary.

After symmetry is verified, add one focused cancellation contract proving that an `OperationCanceledException` from `IErrorDescriptorResolver` still propagates rather than becoming `ErrorDescriptorResolverFailed`.

Avoid broader refactoring. Keep each step small, tested, documented here, and committed directly to `master`.
