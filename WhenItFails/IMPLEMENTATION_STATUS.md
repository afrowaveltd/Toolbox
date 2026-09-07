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
- `ErrorCatalogRuntime` stabilizes null descriptor-service responses and now routes `FromId(...)`, `FromName(...)`, and `FromCode(...)` through one shared descriptor-service exception boundary.
- Ordinary `IErrorDescriptorService` exceptions now become `WIF_DESCRIPTOR_SERVICE_FAILED` without exposing raw dependency text.
- The complete `WhenItFails.Tests` suite is verified GREEN at 971/971 tests before the two runtime symmetry tests.
- Before the centralized runtime fix, the focused descriptor-service exception contract class produced one GREEN (`FromId`) and two RED (`FromName`, `FromCode`) results.

## Latest committed steps

### 2026-09-07 — centralized ErrorCatalogRuntime descriptor-service exception boundary

Production fix commit: `8da3fef0140c081e9119b5097a7343bf24cfafa5`

Changed:

`WhenItFails/Services/ErrorCatalogRuntime.cs`

`FromId(...)`, `FromName(...)`, and `FromCode(...)` now delegate through one shared helper:

```text
FromId / FromName / FromCode
          ↓
validate current context
          ↓
ResolveDescriptor(...)
          ↓
invoke only the selected IErrorDescriptorService method inside the exception boundary
          ↓
null-response guard
```

Ordinary descriptor-service exceptions become:

```text
Status: Failed
Code: WIF_DESCRIPTOR_SERVICE_FAILED
Message: The error descriptor service failed.
```

The original exception message is deliberately not copied into the public runtime response.

The exception filter excludes `OperationCanceledException`, so cancellation continues to propagate naturally.

The existing null-response behavior remains unchanged and still returns:

```text
Status: Invalid
Code: WIF_DESCRIPTOR_SERVICE_RESPONSE_NULL
Message: The error descriptor service returned a null response.
```

The production diff was checked after commit and contains only the three descriptor entry points plus the new shared helper.

### 2026-09-07 — verified RED runtime descriptor-service symmetry state

Symmetry contract commit: `a2d381dff3caf86efd94853cd804693a87767c59`

Focused class:

`WhenItFails.Tests/Services/ErrorCatalogRuntimeDescriptorServiceExceptionContractTests`

Observed locally on Windows before the centralized production fix:

```text
Failed: 2
Passed: 1
Skipped: 0
Total: 3
```

Observed behavior:

- `FromId_WhenDescriptorServiceThrows_ReturnsStableFailure`: GREEN
- `FromName_WhenDescriptorServiceThrows_ReturnsStableFailure`: RED with `Sensitive runtime descriptor service name detail must not escape.`
- `FromCode_WhenDescriptorServiceThrows_ReturnsStableFailure`: RED with `Sensitive runtime descriptor service code detail must not escape.`

This confirmed that only `FromId(...)` had the required runtime-facade exception boundary and that raw descriptor-service diagnostic text still escaped from the name/code paths.

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

### 2026-09-07 — verified ErrorDescriptorService resolver cancellation contract

Contract commit: `0027ba19d27dbe86a10f7dd39f0398f2678051f1`

Verified locally at 970/970 tests GREEN. The exact original `OperationCanceledException` instance propagates through `ErrorDescriptorService`.

## Verification state

- Verified continuation baseline before the two runtime symmetry tests: 971/971 tests GREEN.
- Runtime symmetry tests reproduced the expected pre-fix state: 1 GREEN / 2 RED.
- Centralized `ErrorCatalogRuntime` descriptor-service exception boundary is committed and awaits focused local verification.
- Expected complete-suite count after both new symmetry tests: 973 tests.

## Recommended verification

Pull current `master` and run the focused runtime descriptor-service exception contract class:

```powershell
dotnet test WhenItFails.Tests --filter "FullyQualifiedName~ErrorCatalogRuntimeDescriptorServiceExceptionContractTests"
```

Expected result after the centralized fix: 3/3 GREEN.

Then run the complete suite:

```powershell
dotnet test WhenItFails.Tests
```

Expected complete-suite result: 973/973 GREEN.

## Next recommended step

After 973/973 GREEN is confirmed, add one focused cancellation contract proving that `OperationCanceledException` from `IErrorDescriptorService` still propagates as the exact original instance rather than becoming `WIF_DESCRIPTOR_SERVICE_FAILED`.

If that passes without production changes, move to the next distinct `ErrorCatalogRuntime` dependency boundary. A strong next candidate is the profile-selection service or context-store invocation, depending on existing contract coverage.

Avoid broader refactoring. Keep each step small, tested, documented here, and committed directly to `master`.
