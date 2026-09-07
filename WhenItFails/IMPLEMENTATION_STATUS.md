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
- `ErrorDescriptorService.FromId(...)` converts ordinary resolver exceptions into `ErrorDescriptorResolverFailed` without exposing raw exception text.
- The complete `WhenItFails.Tests` suite is verified GREEN at 967/967 tests after the `FromId(...)` service exception fix.
- New service symmetry contracts now require the same ordinary-exception behavior for `FromName(...)` and `FromCode(...)`.

## Latest committed steps

### 2026-09-07 — ErrorDescriptorService resolver-exception symmetry contracts

Contract commit: `f69558276a3ae696ac6429b66343a9341acdc985`

Updated:

`WhenItFails.Tests/Services/ErrorDescriptorServiceResolverExceptionContractTests.cs`

Coverage now includes:

- `FromId_WhenResolverThrows_ReturnsStableFailure`
- `FromName_WhenResolverThrows_ReturnsStableFailure`
- `FromCode_WhenResolverThrows_ReturnsStableFailure`

All three require:

```text
Status: Failed
Code: ErrorDescriptorResolverFailed
Message: Error descriptor resolver failed.
```

Each resolver entry point throws its own sensitive diagnostic text. The service response must never expose that text.

No production code changed in this symmetry step. `FromName(...)` and `FromCode(...)` are therefore expected to be RED until the service exception boundary is implemented or centralized.

### 2026-09-07 — verified ErrorDescriptorService FromId exception fix

Production fix commit: `5db0ebd6773afd21a50b83824e6d4a533ac5b377`

Verified locally after pulling the fix:

```text
WhenItFails.Tests
Failed:   0
Passed: 967
Skipped:  0
Total:  967
```

This confirms that an ordinary exception from `IErrorDescriptorResolver.CreateById(...)` becomes:

```text
Status: Failed
Code: ErrorDescriptorResolverFailed
Message: Error descriptor resolver failed.
```

without exposing the original dependency exception text.

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

- Complete verified continuation baseline: 967/967 tests GREEN.
- `ErrorDescriptorResolver` ordinary-exception and cancellation behavior is verified for both definition resolution and descriptor creation.
- `ErrorDescriptorService` null-response behavior is covered for ID, name, and code paths.
- `ErrorDescriptorService.FromId(...)` ordinary resolver-exception behavior is verified GREEN.
- New `FromName(...)` and `FromCode(...)` symmetry contracts are committed and await local verification.
- Production code is unchanged for the symmetry step.

## Recommended verification

Pull current `master` and run the service resolver-exception contract class:

```powershell
dotnet test WhenItFails.Tests --filter "FullyQualifiedName~ErrorDescriptorServiceResolverExceptionContractTests"
```

Expected current result:

- `FromId(...)`: GREEN
- `FromName(...)`: RED with the original resolver exception escaping
- `FromCode(...)`: RED with the original resolver exception escaping

Expected focused total: 1 passed / 2 failed.

After the smallest production fix, rerun this focused class and then the complete suite.

Expected complete-suite count after both new symmetry tests: 969 tests.

## Next recommended step

If the two new symmetry contracts fail as expected, centralize the service-level descriptor-resolver exception conversion so all three public entry points share one stable boundary without duplicating catch logic.

After 969/969 GREEN, add one focused cancellation contract proving that `OperationCanceledException` from `IErrorDescriptorResolver` still propagates as the exact original instance rather than becoming `ErrorDescriptorResolverFailed`.

Avoid broader refactoring. Keep each step small, tested, documented here, and committed directly to `master`.
