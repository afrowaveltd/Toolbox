# Implementation status

Last updated: 2026-09-09

This file is the continuation point for `WhenItFails` development. Update it after every implementation, test, catalog, or documentation change that alters the current state or recommended next step.

## Current focus

Hardening initialization and runtime dependency boundaries against malformed behavior, raw exception leakage, and cancellation corruption.

## Current state

- `ErrorDescriptorResolver` and `ErrorDescriptorService` exception/cancellation hardening are complete for the current scope.
- `ErrorCatalogRuntime` descriptor-service and profile-selection exception/cancellation boundaries are complete.
- `IErrorCatalogContextStore.GetCurrent()` null-response, ordinary-exception, and exact-instance cancellation behavior are complete.
- `IErrorCatalogInitializer.InitializeAsync(...)` as consumed by `ErrorCatalogRuntime` has null-response, ordinary-exception, and exact-instance cancellation contracts.
- `IBuiltInErrorCatalogContextProvider.LoadAsync(...)` is hardened for both explicit `ResetToDefaultsAsync()` and flexible initialization fallback paths.
- Both `ErrorCatalogRuntime` `_contextStore.Set(...)` invocation sites are complete for the current scope: explicit reset and flexible fallback each have ordinary-exception and exact-instance cancellation contracts.
- The complete `WhenItFails.Tests` suite is locally verified **GREEN at 988/988 tests with zero compiler warnings**.
- The remaining distinct store-write boundary is `_contextStore.Set(...)` inside `ErrorCatalogInitializer.InitializeAsync(...)`.
- A focused ordinary-exception contract is now committed for that initializer store-write boundary and awaits local RED verification.

## Latest committed steps

### 2026-09-09 — initializer context-store Set exception contract

Contract commit: `71ee95f3621f13eed1cd01f0bdcc0767b98d1754`

Added:

`WhenItFails.Tests/Initialization/ErrorCatalogInitializerContextStoreSetExceptionContractTests.cs`

Test:

`InitializeAsync_WhenContextStoreSetThrows_ReturnsStableFailure`

The fixture supplies a successful bootstrapper and a successful context provider, then throws an ordinary `InvalidOperationException` from `IErrorCatalogContextStore.Set(...)` containing sensitive diagnostic text.

Required public initializer contract:

```text
Status: Failed
Data: null
Code: WIF_CONTEXT_STORE_FAILED
Message: The error catalog context store failed.
```

The raw dependency exception text must not escape:

```text
Sensitive initializer context store Set detail must not escape.
```

No production code changed in this step. `ErrorCatalogInitializer.InitializeAsync(...)` still invokes `_contextStore.Set(contextResponse.Data)` directly, so the focused contract is expected to be RED with the original exception escaping.

### 2026-09-09 — 988/988 GREEN runtime store-write checkpoint

Checkpoint commit: `522d9b54326f51c7eb11ca1c8bc34bc17b948395`

Verified locally:

```text
WhenItFails.Tests
Failed:   0
Passed: 988
Skipped:  0
Total:  988
Compiler warnings: 0
```

Both runtime store-write invocation sites are complete for the current scope.

## Verification state

- Clean continuation baseline: **988/988 GREEN, zero compiler warnings**.
- Both `ErrorCatalogRuntime` store-write boundaries are complete.
- New `ErrorCatalogInitializer` store-write ordinary-exception contract is committed and awaits focused local RED verification.
- Production initializer code remains unchanged until RED is observed.
- Expected complete-suite count once the new contract eventually passes: **989 tests**.

## Recommended verification

Pull current `master` and run only the new contract:

```powershell
dotnet test WhenItFails.Tests --filter "FullyQualifiedName~InitializeAsync_WhenContextStoreSetThrows_ReturnsStableFailure"
```

Expected current result: RED with the original exception text:

```text
Sensitive initializer context store Set detail must not escape.
```

## Next recommended step

If RED is confirmed, add the smallest exception boundary around only `_contextStore.Set(contextResponse.Data)` inside `ErrorCatalogInitializer.InitializeAsync(...)`.

Convert ordinary exceptions into:

```text
Status: Failed
Data: null
Code: WIF_CONTEXT_STORE_FAILED
Message: The error catalog context store failed.
```

while allowing `OperationCanceledException` to propagate unchanged.

After the ordinary-exception behavior is GREEN, add a separate exact-instance cancellation contract for the same initializer store-write invocation.

Do not broaden this step to bootstrapper or context-provider exception behavior. Keep changes small, tested, documented here, and committed directly to `master`.