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
- The complete `WhenItFails.Tests` suite is locally verified **GREEN at 992/992 tests with zero compiler warnings** before the context-provider exception contract.
- `IErrorCatalogContextProvider.LoadFromJsonsAsync(...)` null-response behavior is already covered.
- The context-provider ordinary-exception contract is verified RED before the production fix.
- `ErrorCatalogInitializer.InitializeAsync(...)` now converts ordinary context-provider exceptions into `WIF_INITIALIZER_CONTEXT_PROVIDER_FAILED` without exposing raw dependency text.

## Latest committed steps

### 2026-09-09 — initializer context-provider ordinary-exception fix

Production fix commit: `f5485fd244b860514c0f683bee855e6cc6e7a69c`

Changed only the `_contextProvider.LoadFromJsonsAsync(...)` invocation inside `ErrorCatalogInitializer.InitializeAsync(...)`.

Ordinary exceptions are now converted to:

```text
Status: Failed
Data: null
Code: WIF_INITIALIZER_CONTEXT_PROVIDER_FAILED
Message: The error catalog context provider failed during initialization.
```

The catch filter excludes `OperationCanceledException`, so cancellation is still intended to propagate unchanged.

The production diff was checked and contains only the intended context-provider exception guard. Existing null-response, failed-response, payload-null, and context-store write logic remain unchanged.

### 2026-09-09 — verified RED initializer context-provider exception contract

Contract commit: `98a14a04feb702345eade7b1217b347e72910dda`

Focused test:

`WhenItFails.Tests/Initialization/ErrorCatalogInitializerContextProviderExceptionContractTests.InitializeAsync_WhenContextProviderThrows_ReturnsStableFailure`

Observed locally before the production fix:

```text
Failed: 1
Passed: 0
Skipped: 0
Total: 1
```

Failure:

```text
System.InvalidOperationException:
Sensitive initializer context provider detail must not escape.
```

The exception escaped directly from `IErrorCatalogContextProvider.LoadFromJsonsAsync(...)` through `ErrorCatalogInitializer.InitializeAsync(...)`, confirming the missing initializer context-provider boundary.

The store write path was not reached and the previous context remained protected by the test fixture.

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

The initializer bootstrapper boundary is complete for the current scope.

## Verification state

- Clean continuation baseline before the context-provider exception contract: **992/992 GREEN, zero compiler warnings**.
- Bootstrapper boundary is complete for the current scope.
- Context-provider null-response behavior is covered by `WIF_INITIALIZER_CONTEXT_PROVIDER_RESPONSE_NULL`.
- Context-provider ordinary-exception contract is verified RED before the production fix.
- Production context-provider guard is committed and awaits focused local GREEN verification.
- Expected complete-suite count after the new contract passes: **993 tests**.

## Recommended verification

Pull current `master` and run only the context-provider exception contract:

```powershell
dotnet test WhenItFails.Tests --filter "FullyQualifiedName~InitializeAsync_WhenContextProviderThrows_ReturnsStableFailure"
```

Expected result after the production fix: GREEN.

Then run the complete suite:

```powershell
dotnet test WhenItFails.Tests
```

Expected complete-suite result: **993/993 GREEN with zero compiler warnings**.

## Next recommended step

After 993/993 GREEN is confirmed, add one focused exact-instance cancellation contract for the same `IErrorCatalogContextProvider.LoadFromJsonsAsync(...)` invocation.

If that passes without production changes, consider the initializer context-provider boundary complete for the current scope.

Keep changes small, tested, documented here, and committed directly to `master`.
