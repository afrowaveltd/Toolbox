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
- Ordinary exceptions from all three `IErrorDescriptorResolver` entry points are now converted through one shared service boundary into `ErrorDescriptorResolverFailed` without exposing raw exception text.
- The complete `WhenItFails.Tests` suite is verified GREEN at 967/967 tests before the two service symmetry tests.
- Before the centralized service fix, the focused resolver-exception contract class produced one GREEN (`FromId`) and two RED (`FromName`, `FromCode`) results.

## Latest committed steps

### 2026-09-07 — centralized ErrorDescriptorService resolver exception boundary

Production fix commit: `c41256ca8efdf137bff5a2d7ab5287eb10862990`

Changed:

`WhenItFails/Services/ErrorDescriptorService.cs`

`FromId(...)`, `FromName(...)`, and `FromCode(...)` now delegate through one shared helper:

```text
FromId / FromName / FromCode
          ↓
ResolveDescriptor(...)
          ↓
invoke only the selected IErrorDescriptorResolver method inside the exception boundary
          ↓
EnsureResponse(...)
```

Ordinary resolver exceptions become:

```text
Status: Failed
Code: ErrorDescriptorResolverFailed
Message: Error descriptor resolver failed.
```

The original exception message is deliberately not copied into the public response.

The exception filter excludes `OperationCanceledException`, so cancellation continues to propagate naturally.

The existing `EnsureResponse(...)` null-response guard remains unchanged and still converts a null resolver response into `ErrorDescriptorResolverReturnedNull`.

### 2026-09-07 — verified RED service symmetry state

Symmetry contract commit: `f69558276a3ae696ac6429b66343a9341acdc985`

Focused class:

`WhenItFails.Tests/Services/ErrorDescriptorServiceResolverExceptionContractTests`

Observed locally on Windows before the centralized production fix:

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

This confirmed that only `FromId(...)` had the required service-level exception boundary and that raw resolver diagnostic text still escaped from the name/code paths.

### 2026-09-07 — verified ErrorDescriptorService FromId exception fix

Production fix commit: `5db0ebd6773afd21a50b83824e6d4a533ac5b377`

Verified locally:

```text
WhenItFails.Tests
Failed:   0
Passed: 967
Skipped:  0
Total:  967
```

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

- Verified continuation baseline before the two service symmetry tests: 967/967 tests GREEN.
- Service symmetry tests reproduced the expected pre-fix state: 1 GREEN / 2 RED.
- Centralized `ErrorDescriptorService` resolver-exception boundary is committed and awaits focused local verification.
- Expected complete-suite count after both new symmetry tests: 969 tests.

## Recommended verification

Pull current `master` and run the focused service contract class:

```powershell
dotnet test WhenItFails.Tests --filter "FullyQualifiedName~ErrorDescriptorServiceResolverExceptionContractTests"
```

Expected result after the centralized fix: 3/3 GREEN.

Then run the complete suite:

```powershell
dotnet test WhenItFails.Tests
```

Expected complete-suite result: 969/969 GREEN.

## Next recommended step

After 969/969 GREEN is confirmed, add one focused cancellation contract proving that `OperationCanceledException` from `IErrorDescriptorResolver` still propagates as the exact original instance rather than becoming `ErrorDescriptorResolverFailed`.

If that passes without production changes, move to the next distinct service/runtime dependency boundary rather than adding more resolver-exception permutations.

Avoid broader refactoring. Keep each step small, tested, documented here, and committed directly to `master`.
