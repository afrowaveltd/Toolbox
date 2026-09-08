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
- The complete `WhenItFails.Tests` suite is verified GREEN at 982/982 tests.
- Flexible initialization fallback has a separate direct `_builtInContextProvider.LoadAsync(...)` invocation and now has a focused ordinary-exception contract awaiting RED verification.

## Latest committed steps

### 2026-09-08 — flexible fallback built-in-provider exception contract

Contract commit: `76de742808ee09d05fedda1ef9089b67e3a1ab90`

Added:

`WhenItFails.Tests/Services/ErrorCatalogRuntimeBuiltInContextProviderFlexibleFallbackExceptionContractTests.InitializeAsync_WhenFlexibleFallbackProviderThrows_ReturnsStableFallbackFailure`

The test forces configured initialization to fail, provides no previous context, and makes `IBuiltInErrorCatalogContextProvider.LoadAsync(...)` return a faulted task with sensitive raw exception text.

The public flexible-fallback wrapper must remain:

```text
Status: Failed
Code: WIF_DEFAULT_FALLBACK_FAILED
Message: The configured error catalog failed and the bundled default catalog could not be activated.
```

The normalized provider failure must be preserved in metadata:

```text
WhenItFails.FallbackFailure.Code = WIF_BUILT_IN_CONTEXT_PROVIDER_FAILED
WhenItFails.FallbackFailure.Status = Failed
WhenItFails.FallbackFailure.Message = The bundled default catalog provider failed.
```

The original provider exception text must not escape.

`EmptyContextStore.Set(...)` is a throwing sentinel, proving the test cannot pass by accidentally advancing beyond provider loading.

No production code changed in this step. `CreateBuiltInFallbackResponseAsync(...)` still directly awaits `_builtInContextProvider.LoadAsync(...)`, so this contract is expected to be RED with the raw exception escaping.

### 2026-09-08 — verified ErrorCatalogRuntime built-in provider cancellation contract

Contract commit: `7f7ef67223cc67614282f2a43bd0284bad4a55e1`

Verified locally:

```text
WhenItFails.Tests
Failed:   0
Passed: 982
Skipped:  0
Total:  982
```

The exact original `OperationCanceledException` instance from `IBuiltInErrorCatalogContextProvider.LoadAsync(...)` propagates unchanged through `ResetToDefaultsAsync()`.

Together with the ordinary-exception and null-response contracts, this completes the explicit reset provider boundary for the current scope.

## Verification state

- Complete verified continuation baseline: 982/982 tests GREEN.
- Runtime descriptor-service, profile-selection, context-store `GetCurrent()`, initializer, and explicit reset built-in-provider exception/cancellation boundaries are complete for the current scope.
- Flexible fallback null-response behavior is already verified and uses `WIF_DEFAULT_FALLBACK_FAILED` with `WhenItFails.FallbackFailure.*` metadata.
- Flexible fallback ordinary-exception contract is committed and awaits focused local verification.
- Production `CreateBuiltInFallbackResponseAsync(...)` remains unchanged until the RED state is observed.
- Expected complete-suite count once the new contract eventually passes: 983 tests.

## Recommended verification

Pull current `master` and run only the new flexible-fallback provider contract:

```powershell
dotnet test WhenItFails.Tests --filter "FullyQualifiedName~InitializeAsync_WhenFlexibleFallbackProviderThrows_ReturnsStableFallbackFailure"
```

Expected current result: RED with the original exception text:

```text
Sensitive flexible fallback provider detail must not escape.
```

## Next recommended step

If the focused contract fails as expected, add the smallest exception boundary around the `_builtInContextProvider.LoadAsync(...)` invocation/await in `CreateBuiltInFallbackResponseAsync(...)`.

Convert ordinary provider exceptions into an internal `Response<ErrorCatalogContext>.Fail(...)` using:

```text
WIF_BUILT_IN_CONTEXT_PROVIDER_FAILED
The bundled default catalog provider failed.
```

and then let the existing `CreateBuiltInFallbackFailureResponse(...)` preserve the established `WIF_DEFAULT_FALLBACK_FAILED` wrapper and metadata contract.

Allow `OperationCanceledException` to propagate unchanged.

Do not harden `_contextStore.Set(...)` or add cancellation in the same production step. First verify the single ordinary-exception contract.

Avoid broader refactoring. Keep each step small, tested, documented here, and committed directly to `master`.