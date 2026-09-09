# Implementation status

Last updated: 2026-09-09

This file is the continuation point for `WhenItFails` development. Update it after every implementation, test, catalog, or documentation change that alters the current state or recommended next step.

## Current focus

Hardening dependency boundaries while preserving established public exception contracts.

## Current state

- `ErrorDescriptorResolver` and `ErrorDescriptorService` exception/cancellation hardening are complete for the current scope.
- `ErrorCatalogRuntime` descriptor-service and profile-selection exception/cancellation boundaries are complete.
- All currently known `IErrorCatalogContextStore` read/write boundaries in the active runtime/initializer scope are complete.
- `IJsonsBootstrapper.EnsureWorkspaceAsync(...)` is complete for the current scope.
- `IErrorCatalogContextProvider.LoadFromJsonsAsync(...)` as consumed by `ErrorCatalogInitializer` is complete for the current scope.
- Last clean locally verified baseline: **994/994 GREEN, zero compiler warnings**.
- `ErrorCatalogContextProvider` is intentionally a transparent orchestration boundary for exceptions thrown by its five internal catalog providers.
- Existing contract tests require provider exceptions to propagate unchanged rather than be normalized into `Response<T>` failures.

## Important regression finding

A temporary experiment attempted to normalize an exception from `IErrorCatalogProvider.LoadFromFileAsync(...)` into `WIF_ERROR_CATALOG_PROVIDER_FAILED`.

The full suite immediately exposed the conflict:

```text
Total: 995
Passed: 988
Failed: 7
Skipped: 0
```

The failing pre-existing tests prove that exception transparency is deliberate and broad. They require preservation of:

- direct synchronous provider exceptions;
- faulted provider task exception identity;
- concrete exception type;
- outer/inner exception references;
- `Exception.Data` entries;
- custom exception properties;
- short-circuiting so later providers are not invoked.

Relevant established suites:

- `WhenItFails.Tests/Catalog/ErrorCatalogContextProviderProviderExceptionPropagationTests.cs`
- `WhenItFails.Tests/Catalog/ErrorCatalogContextProviderExceptionShapeTests.cs`

These tests also cover all five provider positions for ordinary exception propagation and cancellation behavior for the first provider.

## Recovery completed

The experimental production guard was removed and `ErrorCatalogContextProvider.cs` is back to the exact pre-experiment blob:

`79e298aafcdb07bd521ef7eff9d04d0f2e7e88af`

Recovery commits:

- `b211eda61ec9dc538b499fd9b67b4f9a412b1bab` — restore provider exception propagation;
- `f0912a656743c3bf8af272cdc186e96eb454f9ec` — restore the original package-directory validation code after a mechanical full-file replacement typo;
- `9d316fdaa1759459e218191552382c7951d2980d` — remove the contradictory temporary normalization test.

The temporary test count increase is gone, so the expected complete-suite count is again **994 tests**.

## Verification state

- Production behavior is restored to the established provider pass-through contract.
- Contradictory experimental test has been removed.
- No intended earlier initializer/runtime hardening was reverted.
- Local full-suite recovery verification is now required.

## Recommended verification

Pull current `master` and run:

```powershell
dotnet test WhenItFails.Tests
```

Expected result:

```text
Failed:   0
Passed: 994
Skipped:  0
Total:  994
Compiler warnings: 0
```

## Next recommended step

After **994/994 GREEN** is reconfirmed, treat all five internal provider exception calls in `ErrorCatalogContextProvider` as an already-defined transparent boundary and do not add normalization guards there.

Continue reconnaissance outside this boundary for the next dependency call whose existing contracts permit normalization/hardening. Before adding any new exception contract, first search for existing propagation/shape tests to avoid changing an established semantic contract.

Keep changes small, tested, documented here, and committed directly to `master`.
