# Implementation status

Last updated: 2026-09-08

This file is the continuation point for `WhenItFails` development. Update it after every implementation, test, catalog, or documentation change that alters the current state or recommended next step.

## Current focus

Hardening runtime and service boundaries against malformed dependency behavior, raw exception leakage, and internally inconsistent responses.

## Current state

- `WhenItFails` provides structured error catalogs, runtime error resolution, profiles, diagnostics, initialization/recovery behavior, and project-local catalog handling.
- `ErrorDescriptorResolver` and `ErrorDescriptorService` exception/cancellation hardening blocks are complete for the current scope.
- `ErrorCatalogRuntime` descriptor-service ordinary-exception and cancellation behavior is complete for the current scope.
- `ErrorCatalogRuntime` profile-selection ordinary-exception and cancellation behavior is complete for the current scope.
- `ErrorCatalogRuntime` context-store `GetCurrent()` null-response, ordinary-exception, and cancellation behavior is complete for the current scope.
- `ErrorCatalogRuntime` initializer null-response, ordinary-exception, and cancellation behavior is complete for the current scope.
- The explicit `ResetToDefaultsAsync()` built-in-provider null-response, ordinary-exception, and cancellation boundary is complete for the current scope.
- Flexible initialization fallback has its own provider-load boundary. Null-response and ordinary-exception behavior are verified GREEN.
- The complete `WhenItFails.Tests` suite is verified GREEN at 983/983 tests.
- A focused flexible-fallback cancellation contract now requires the exact original `OperationCanceledException` instance to propagate unchanged.

## Latest committed steps

### 2026-09-08 — flexible fallback built-in-provider cancellation contract

Contract commit: `6d8e045df726ed3c1c367407323e1b4d3bd91375`

Updated:

`WhenItFails.Tests/Services/ErrorCatalogRuntimeBuiltInContextProviderFlexibleFallbackExceptionContractTests.cs`

Added:

`InitializeAsync_WhenFlexibleFallbackProviderCancels_RethrowsSameOperationCanceledException`

Contract:

```text
IBuiltInErrorCatalogContextProvider.LoadAsync(...)
    => faulted task carrying an OperationCanceledException instance
                         ↓
rethrow the exact same OperationCanceledException instance
```

The test forces the flexible fallback path by using a failed configured initialization and an empty previous-context store. It then injects a provider that faults with a specific `OperationCanceledException` instance.

The test uses `Assert.Same(...)`, so cancellation cannot be wrapped, replaced, normalized to `WIF_BUILT_IN_CONTEXT_PROVIDER_FAILED`, or converted into `WIF_DEFAULT_FALLBACK_FAILED`.

The fixture was lightly refactored through a shared `CreateRuntime(...)` helper. `EmptyContextStore.Set(...)` remains a throwing sentinel, so the test cannot pass by accidentally progressing beyond provider loading.

No production code changed in this step. The current flexible-fallback provider catch filter excludes `OperationCanceledException`, so the focused contract is expected to be GREEN.

### 2026-09-08 — verified flexible fallback built-in-provider exception fix

Production fix commit: `3a4abe0052d04b4112d9150a7a7e8d3cc749568b`

Contract commit: `76de742808ee09d05fedda1ef9089b67e3a1ab90`

Verified locally:

```text
WhenItFails.Tests
Failed:   0
Passed: 983
Skipped:  0
Total:  983
```

Ordinary provider exceptions are normalized without leaking raw dependency text while preserving the established `WIF_DEFAULT_FALLBACK_FAILED` wrapper and `WhenItFails.FallbackFailure.*` metadata.

## Verification state

- Complete verified continuation baseline: 983/983 tests GREEN.
- Runtime descriptor-service, profile-selection, context-store `GetCurrent()`, initializer, and explicit reset built-in-provider exception/cancellation boundaries are complete for the current scope.
- Flexible fallback null-response and ordinary-exception behavior are verified GREEN.
- Flexible fallback exact-instance cancellation contract is committed and awaits focused local verification.
- No production change is expected for this cancellation contract.
- Expected complete-suite count after the new contract passes: 984 tests.

## Recommended verification

Pull current `master` and run the focused flexible-fallback cancellation contract:

```powershell
dotnet test WhenItFails.Tests --filter "FullyQualifiedName~InitializeAsync_WhenFlexibleFallbackProviderCancels_RethrowsSameOperationCanceledException"
```

Expected result: GREEN.

Then run the complete suite:

```powershell
dotnet test WhenItFails.Tests
```

Expected complete-suite result: 984/984 GREEN.

## Next recommended step

After 984/984 GREEN is confirmed, consider the built-in provider boundary complete for both explicit reset and flexible fallback paths.

Then inspect `_contextStore.Set(...)` as the next distinct runtime dependency boundary. Keep ordinary-exception and cancellation behavior separate and do not broaden the change into unrelated initialization logic.

Avoid broader refactoring. Keep each step small, tested, documented here, and committed directly to `master`.