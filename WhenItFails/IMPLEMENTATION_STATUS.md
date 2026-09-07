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
- A focused service cancellation contract now requires the shared service boundary to rethrow the exact original `OperationCanceledException` instance.

## Latest committed steps

### 2026-09-07 — ErrorDescriptorService resolver cancellation contract

Contract commit: `0027ba19d27dbe86a10f7dd39f0398f2678051f1`

Updated:

`WhenItFails.Tests/Services/ErrorDescriptorServiceResolverExceptionContractTests.cs`

Added:

`FromId_WhenResolverCancels_RethrowsSameOperationCanceledException`

Contract:

```text
IErrorDescriptorResolver.CreateById(...) => OperationCanceledException instance
                         ↓
rethrow the exact same OperationCanceledException instance
```

The test uses `Assert.Same(...)`, so future refactoring cannot silently wrap cancellation, replace it with another cancellation exception, or convert it into `ErrorDescriptorResolverFailed`.

No production code changed in this step. The shared `ResolveDescriptor(...)` exception filter already excludes `OperationCanceledException`, so the focused contract is expected to be GREEN.

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
- Service-level resolver cancellation contract is committed and awaits focused local verification.
- No production change is expected for the cancellation contract.

## Recommended verification

Pull current `master` and run the focused service cancellation contract:

```powershell
dotnet test WhenItFails.Tests --filter "FullyQualifiedName~FromId_WhenResolverCancels_RethrowsSameOperationCanceledException"
```

If green, run the complete suite:

```powershell
dotnet test WhenItFails.Tests
```

Expected complete-suite count: 970 tests.

## Next recommended step

After 970/970 GREEN is confirmed, consider the `ErrorDescriptorService` resolver boundary complete for the current scope and move to the next distinct service/runtime dependency boundary rather than adding more exception permutations.

Inspect `ErrorCatalogRuntime` next for an injected service/provider/store call that can throw outside an existing structured-response boundary. Establish one focused contract before making production changes.

Avoid broader refactoring. Keep each step small, tested, documented here, and committed directly to `master`.
