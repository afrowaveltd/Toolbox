# Implementation status

Last updated: 2026-09-08

This file is the continuation point for `WhenItFails` development. Update it after every implementation, test, catalog, or documentation change that alters the current state or recommended next step.

## Current focus

Hardening runtime and service boundaries against malformed dependency behavior, raw exception leakage, and cancellation corruption.

## Current state

- `ErrorDescriptorResolver` and `ErrorDescriptorService` exception/cancellation hardening are complete for the current scope.
- `ErrorCatalogRuntime` descriptor-service and profile-selection exception/cancellation boundaries are complete.
- `IErrorCatalogContextStore.GetCurrent()` null-response, ordinary-exception, and exact-instance cancellation behavior are complete.
- `IErrorCatalogInitializer.InitializeAsync(...)` null-response, ordinary-exception, and exact-instance cancellation behavior are complete.
- `IBuiltInErrorCatalogContextProvider.LoadAsync(...)` is hardened for both explicit `ResetToDefaultsAsync()` and flexible initialization fallback paths.
- The complete `WhenItFails.Tests` suite is locally verified **GREEN at 986/986 tests with zero compiler warnings**.
- The explicit `ResetToDefaultsAsync()` `_contextStore.Set(...)` boundary is complete for the current scope.
- A focused ordinary-exception contract is now committed for the separate flexible-fallback `_contextStore.Set(...)` invocation and awaits local RED verification.
- `ErrorCatalogInitializer` store-write behavior remains intentionally untouched.

## Latest committed steps

### 2026-09-08 — flexible fallback context-store Set exception contract

Contract commit: `c0d2f39998cd8f316d75bce18dbf4e8079ff2475`

Updated:

`WhenItFails.Tests/Services/ErrorCatalogRuntimeContextStoreSetExceptionContractTests.cs`

Added:

`InitializeAsync_WhenFlexibleFallbackContextStoreSetThrows_ReturnsStableFallbackFailure`

The fixture forces the flexible recovery path by returning a failed configured initialization and no previous valid context. The built-in provider then returns a successful context, and the context store throws from `Set(...)`.

Established public wrapper required by the contract:

```text
Status: Failed
Data: null
Code: WIF_DEFAULT_FALLBACK_FAILED
Message: The configured error catalog failed and the bundled default catalog could not be activated.
```

The normalized store-write dependency failure must be retained in fallback metadata:

```text
WhenItFails.FallbackFailure.Code = WIF_CONTEXT_STORE_FAILED
WhenItFails.FallbackFailure.Status = Failed
WhenItFails.FallbackFailure.Message = The error catalog context store failed.
```

The configured initialization failure remains available as:

```text
WhenItFails.ProjectFailure.Code = CatalogDocumentsInvalid
```

The raw store exception text must not escape:

```text
Sensitive flexible fallback context store Set detail must not escape.
```

No production code changed in this step. `_contextStore.Set(fallbackResponse.Data)` inside `CreateBuiltInFallbackResponseAsync(...)` is still a direct unguarded call, so the focused contract is expected to be RED.

### 2026-09-08 — 986/986 GREEN reset store-write checkpoint

Checkpoint commit: `9a4d01e74056eab9fce0ce845267544d637d7baa`

Verified locally:

```text
WhenItFails.Tests
Failed:   0
Passed: 986
Skipped:  0
Total:  986
Compiler warnings: 0
```

The exact original `OperationCanceledException` instance from the explicit reset store-write path propagates unchanged. Together with the ordinary-exception contract, this completes that invocation site for the current scope.

## Verification state

- Clean continuation baseline: **986/986 GREEN, zero compiler warnings**.
- Explicit reset store-write boundary is complete.
- Flexible-fallback store-write ordinary-exception contract is committed and awaits focused local RED verification.
- Production flexible-fallback store-write code remains unchanged until RED is observed.
- Expected complete-suite count once the new contract eventually passes: **987 tests**.

## Recommended verification

Pull current `master` and run only the new contract:

```powershell
dotnet test WhenItFails.Tests --filter "FullyQualifiedName~InitializeAsync_WhenFlexibleFallbackContextStoreSetThrows_ReturnsStableFallbackFailure"
```

Expected current result: RED with the original exception text:

```text
Sensitive flexible fallback context store Set detail must not escape.
```

## Next recommended step

If the focused RED is confirmed, add the smallest exception boundary around only `_contextStore.Set(fallbackResponse.Data)` in `CreateBuiltInFallbackResponseAsync(...)`.

Convert ordinary store exceptions into an internal `Response<ErrorCatalogContext>.Fail(...)` using:

```text
WIF_CONTEXT_STORE_FAILED
The error catalog context store failed.
```

and pass that internal failure through the existing `CreateBuiltInFallbackFailureResponse(...)` so the established `WIF_DEFAULT_FALLBACK_FAILED` wrapper and metadata contract are preserved.

Allow `OperationCanceledException` to propagate unchanged. Do not modify `ErrorCatalogInitializer` in the same production step.

Avoid broader refactoring. Keep each step small, tested, documented here, and committed directly to `master`.