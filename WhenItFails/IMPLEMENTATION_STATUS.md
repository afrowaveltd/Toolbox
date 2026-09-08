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
- The complete `WhenItFails.Tests` suite is verified GREEN at 982/982 tests before the flexible-fallback exception contract.
- Flexible initialization fallback has its own provider-load boundary. Ordinary provider exceptions are now normalized to `WIF_BUILT_IN_CONTEXT_PROVIDER_FAILED` before the existing fallback wrapper processes them.
- The flexible-fallback ordinary-exception contract is verified RED before the production fix and now awaits focused GREEN verification.

## Latest committed steps

### 2026-09-08 — flexible fallback built-in-provider exception fix

Production fix commit: `3a4abe0052d04b4112d9150a7a7e8d3cc749568b`

Changed:

`WhenItFails/Services/ErrorCatalogRuntime.cs`

Only the `_builtInContextProvider.LoadAsync(...)` invocation/await inside `CreateBuiltInFallbackResponseAsync(...)` is wrapped in a narrow exception boundary.

Ordinary provider exceptions are converted into an internal context response:

```text
Status: Failed
Code: WIF_BUILT_IN_CONTEXT_PROVIDER_FAILED
Message: The bundled default catalog provider failed.
```

That internal response is then handled by the existing `CreateBuiltInFallbackFailureResponse(...)`, preserving the established public wrapper:

```text
Status: Failed
Code: WIF_DEFAULT_FALLBACK_FAILED
Message: The configured error catalog failed and the bundled default catalog could not be activated.
```

and metadata:

```text
WhenItFails.FallbackFailure.Code = WIF_BUILT_IN_CONTEXT_PROVIDER_FAILED
WhenItFails.FallbackFailure.Status = Failed
WhenItFails.FallbackFailure.Message = The bundled default catalog provider failed.
```

The catch filter excludes `OperationCanceledException`, so cancellation is still intended to propagate unchanged.

The production diff was checked after commit and contains only the intended flexible-fallback provider-load boundary.

### 2026-09-08 — verified RED flexible fallback provider exception contract

Contract commit: `76de742808ee09d05fedda1ef9089b67e3a1ab90`

Focused test:

`WhenItFails.Tests/Services/ErrorCatalogRuntimeBuiltInContextProviderFlexibleFallbackExceptionContractTests.InitializeAsync_WhenFlexibleFallbackProviderThrows_ReturnsStableFallbackFailure`

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
Sensitive flexible fallback provider detail must not escape.
```

The exception escaped directly through `CreateBuiltInFallbackResponseAsync(...)`, confirming that the flexible fallback path was a distinct unguarded provider invocation site.

### 2026-09-08 — verified ErrorCatalogRuntime built-in provider cancellation contract

Contract commit: `7f7ef67223cc67614282f2a43bd0284bad4a55e1`

Verified locally at 982/982 tests GREEN. The exact original `OperationCanceledException` instance from `IBuiltInErrorCatalogContextProvider.LoadAsync(...)` propagates unchanged through `ResetToDefaultsAsync()`.

## Verification state

- Verified continuation baseline before the flexible-fallback exception contract: 982/982 tests GREEN.
- Runtime descriptor-service, profile-selection, context-store `GetCurrent()`, initializer, and explicit reset built-in-provider exception/cancellation boundaries are complete for the current scope.
- Flexible fallback null-response behavior is already verified and uses `WIF_DEFAULT_FALLBACK_FAILED` with `WhenItFails.FallbackFailure.*` metadata.
- Flexible fallback ordinary-exception contract is verified RED before the production fix.
- Production flexible-fallback provider exception boundary is committed and awaits focused local verification.
- Expected complete-suite count after the new contract passes: 983 tests.

## Recommended verification

Pull current `master` and run the focused flexible-fallback provider contract:

```powershell
dotnet test WhenItFails.Tests --filter "FullyQualifiedName~InitializeAsync_WhenFlexibleFallbackProviderThrows_ReturnsStableFallbackFailure"
```

Expected result after the production fix: GREEN.

Then run the complete suite:

```powershell
dotnet test WhenItFails.Tests
```

Expected complete-suite result: 983/983 GREEN.

## Next recommended step

After 983/983 GREEN is confirmed, add one focused cancellation contract proving that an `OperationCanceledException` from the flexible fallback provider invocation propagates as the exact original exception instance rather than becoming `WIF_BUILT_IN_CONTEXT_PROVIDER_FAILED` or `WIF_DEFAULT_FALLBACK_FAILED`.

If that passes without production changes, consider the built-in provider boundary complete for both explicit reset and flexible fallback paths, then inspect `_contextStore.Set(...)` as the next distinct runtime dependency boundary.

Avoid broader refactoring. Keep each step small, tested, documented here, and committed directly to `master`.