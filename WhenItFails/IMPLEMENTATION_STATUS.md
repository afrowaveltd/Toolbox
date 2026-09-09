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
- Last clean locally verified baseline before the internal-provider experiment: **994/994 GREEN, zero compiler warnings**.
- `ErrorCatalogContextProvider` is intentionally a transparent orchestration boundary for exceptions thrown by its five internal catalog providers.
- Existing contract tests require provider exceptions to propagate unchanged rather than be normalized into `Response<T>` failures.

## Important regression finding

A new experimental contract attempted to normalize an exception from `IErrorCatalogProvider.LoadFromFileAsync(...)` into `WIF_ERROR_CATALOG_PROVIDER_FAILED`.

That production change caused seven existing tests to fail in the full suite. The failures proved that the established behavior is intentional and broad:

- direct synchronous provider exceptions propagate unchanged;
- faulted provider tasks propagate the original exception instance;
- different exception types such as `FormatException` are preserved;
- outer/inner exception references are preserved;
- `Exception.Data` entries are preserved;
- custom exception properties are preserved;
- later providers are not invoked after an earlier provider throws.

Relevant existing suites:

- `WhenItFails.Tests/Catalog/ErrorCatalogContextProviderProviderExceptionPropagationTests.cs`
- `WhenItFails.Tests/Catalog/ErrorCatalogContextProviderExceptionShapeTests.cs`

The full regression run after the attempted normalization reported:

```text
Total: 995
Passed: 988
Failed: 7
Skipped: 0
```

## Restoration

The temporary first-provider exception guard has been removed from:

`WhenItFails/Catalog/ErrorCatalogContextProvider.cs`

Restoration commits:

- `b211eda61ec9dc538b499fd9b67b4f9a412b1bab` — restore provider exception propagation;
- `f0912a656743c3bf8af272cdc186e96eb454f9ec` — restore the original `WIF_JSONS_PACKAGE_DIRECTORY_NAME_EMPTY` validation code after a mechanical full-file replacement typo.

The resulting production file blob is back to the exact pre-experiment SHA:

`79e298aafcdb07bd521ef7eff9d04d0f2e7e88af`

## Pending cleanup

The temporary contradictory test file still needs to be removed:

`WhenItFails.Tests/Catalog/ErrorCatalogContextProviderErrorCatalogProviderExceptionContractTests.cs`

After that removal, run the complete suite again. Expected count returns to **994 tests**.

## Next recommended step

Do not add normalization guards around the five internal provider calls in `ErrorCatalogContextProvider`; their exception pass-through behavior is an established contract.

After restoring **994/994 GREEN**, continue reconnaissance outside this transparent orchestration boundary for the next dependency call whose existing contracts actually permit normalization/hardening.

Keep changes small, tested, documented here, and committed directly to `master`.
