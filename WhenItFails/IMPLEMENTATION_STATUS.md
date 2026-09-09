# Implementation status

Last updated: 2026-09-09

This file is the continuation point for `WhenItFails` development. Update it after every implementation, test, catalog, or documentation change that alters the current state or recommended next step.

## Current focus

Hardening initialization dependency boundaries against malformed behavior, raw exception leakage, and cancellation corruption.

## Current state

- `ErrorDescriptorResolver` and `ErrorDescriptorService` exception/cancellation hardening are complete for the current scope.
- `ErrorCatalogRuntime` descriptor-service and profile-selection exception/cancellation boundaries are complete.
- All currently known `IErrorCatalogContextStore` read/write boundaries in the active runtime/initializer scope are complete.
- `IJsonsBootstrapper.EnsureWorkspaceAsync(...)` is complete for the current scope: null-response, ordinary-exception, and exact-instance cancellation behavior are covered.
- The complete `WhenItFails.Tests` suite is locally verified **GREEN at 992/992 tests with zero compiler warnings** before the new context-provider exception contract.
- `IErrorCatalogContextProvider.LoadFromJsonsAsync(...)` null-response behavior is already covered.
- A focused ordinary-exception contract is now committed for the initializer context-provider invocation and awaits local RED verification.

## Latest committed steps

### 2026-09-09 — initializer context-provider ordinary-exception contract

Contract commit: `98a14a04feb702345eade7b1217b347e72910dda`

Added:

`WhenItFails.Tests/Initialization/ErrorCatalogInitializerContextProviderExceptionContractTests.cs`

Test:

`InitializeAsync_WhenContextProviderThrows_ReturnsStableFailure`

The fixture supplies a successful bootstrap response and then throws an ordinary `InvalidOperationException` from `IErrorCatalogContextProvider.LoadFromJsonsAsync(...)` containing sensitive diagnostic text.

Required stable initializer contract:

```text
Status: Failed
Data: null
Code: WIF_INITIALIZER_CONTEXT_PROVIDER_FAILED
Message: The error catalog context provider failed during initialization.
```

The raw provider exception text must not escape:

```text
Sensitive initializer context provider detail must not escape.
```

A previous context is preloaded into the test store. The contract requires that it remain unchanged, and the store fixture throws `Unexpected Set call.` if the initializer attempts to update it.

No production code changed in this step. `ErrorCatalogInitializer.InitializeAsync(...)` currently awaits `_contextProvider.LoadFromJsonsAsync(...)` directly, so the focused contract is expected to be RED with the original provider exception escaping.

### 2026-09-09 — 992/992 GREEN bootstrapper checkpoint

Checkpoint commit: `245b83c03db0fb9de7432565f1006c7f9524fdf3`

Bootstrapper cancellation contract commit: `dc7700c61dfc0165a374fafac8c98145be4ca8ad`

Verified locally:

```text
WhenItFails.Tests
Failed:   0
Passed: 992
Skipped:  0
Total:  992
Compiler warnings: 0
```

The exact original `OperationCanceledException` instance from `IJsonsBootstrapper.EnsureWorkspaceAsync(...)` propagates unchanged. Together with the existing null-response and ordinary-exception contracts, this completes the initializer bootstrapper boundary for the current scope.

## Verification state

- Clean continuation baseline: **992/992 GREEN, zero compiler warnings**.
- Bootstrapper boundary is complete for the current scope.
- Context-provider null-response behavior is covered by `WIF_INITIALIZER_CONTEXT_PROVIDER_RESPONSE_NULL`.
- Context-provider ordinary-exception contract is committed and awaits focused local RED verification.
- Production context-provider invocation remains unchanged until RED is observed.
- Expected complete-suite count once the new contract eventually passes: **993 tests**.

## Recommended verification

Pull current `master` and run only the new contract:

```powershell
dotnet test WhenItFails.Tests --filter "FullyQualifiedName~InitializeAsync_WhenContextProviderThrows_ReturnsStableFailure"
```

Expected current result: RED with the original exception text:

```text
Sensitive initializer context provider detail must not escape.
```

## Next recommended step

If RED is confirmed, add the smallest exception boundary around only `_contextProvider.LoadFromJsonsAsync(...)` inside `ErrorCatalogInitializer.InitializeAsync(...)`.

Convert ordinary exceptions into:

```text
Status: Failed
Data: null
Code: WIF_INITIALIZER_CONTEXT_PROVIDER_FAILED
Message: The error catalog context provider failed during initialization.
```

while allowing `OperationCanceledException` to propagate unchanged.

After ordinary-exception behavior is GREEN, add a separate exact-instance cancellation contract for the same context-provider invocation.

Keep changes small, tested, documented here, and committed directly to `master`.
