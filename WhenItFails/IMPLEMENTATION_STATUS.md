# Implementation status

Last updated: 2026-09-10

This file is the continuation point for `WhenItFails` development. Update it after every implementation, test, catalog, or documentation change that alters the current state or recommended next step.

## Current focus

Hardening dependency boundaries while preserving established public exception contracts.

## Current state

- `ErrorDescriptorResolver` and `ErrorDescriptorService` exception/cancellation hardening are complete for the current scope.
- `ErrorCatalogRuntime` descriptor-service and profile-selection exception/cancellation boundaries are complete.
- All currently known `IErrorCatalogContextStore` read/write boundaries in the active runtime/initializer scope are complete.
- `IJsonsBootstrapper.EnsureWorkspaceAsync(...)` is complete for the current scope.
- `IErrorCatalogContextProvider.LoadFromJsonsAsync(...)` as consumed by `ErrorCatalogInitializer` is complete for the current scope.
- `ErrorCatalogContextProvider` is intentionally a transparent orchestration boundary for exceptions thrown by its five internal catalog providers; do not normalize them there.
- `ErrorCatalogProvider` dependency-boundary audit is complete for the current scope.
- `CatalogProviderPipeline` dependency-boundary audit is complete for the current scope.
- The complete `WhenItFails.Tests` suite is locally verified **GREEN at 1010/1010 tests with zero compiler warnings**.
- Fresh reconnaissance identifies `BuiltInErrorCatalogContextProvider` as the next focused hardening target because its generic ordinary-exception response currently exposes `exception.Message` publicly.

## 2026-09-10 — 1010/1010 GREEN CatalogProviderPipeline checkpoint

Checkpoint commit: this commit.
Payload-factory cancellation contract commit: `8290c3be0eecb9d38a7a511ae9e8e3407ef9a4ea`.
Payload-factory production fix commit: `fe49871c054523932063935c7aeab62521a18a7c`.
Payload-factory ordinary-exception contract commit: `676dc55d19ceb04f5155445ceb929ea4acee16f3`.

Verified locally:

```text
WhenItFails.Tests
Failed:   0
Passed: 1010
Skipped:  0
Total:  1010
Compiler warnings: 0
```

`CatalogProviderPipeline` boundary is complete for the current scope:

- loader delegate — null response, ordinary exception normalization, exact cancellation propagation;
- normalizer delegate — null result, ordinary exception normalization, exact cancellation propagation;
- validator delegate — null result, ordinary exception normalization, exact cancellation propagation;
- payload-factory delegate — null result, ordinary exception normalization, exact cancellation propagation.

Stable pipeline ordinary-exception codes:

- `WIF_CATALOG_PIPELINE_LOADER_FAILED`
- `WIF_CATALOG_PIPELINE_NORMALIZER_FAILED`
- `WIF_CATALOG_PIPELINE_VALIDATOR_FAILED`
- `WIF_CATALOG_PIPELINE_PAYLOAD_FACTORY_FAILED`

## Completed `ErrorCatalogProvider` dependency boundaries

- `IErrorCatalogLoader.LoadFromFileAsync(...)` — null response, ordinary exception normalization, exact cancellation propagation.
- `IErrorCatalogDocumentNormalizer.Normalize(...)` — null result, ordinary exception normalization, exact cancellation propagation.
- `IErrorCatalogValidator.Validate(...)` — null result, ordinary exception normalization, exact cancellation propagation.
- `IErrorCatalogFactory.Create(...)` — null result, ordinary exception normalization, exact cancellation propagation.

## Established transparent boundary — do not normalize

`ErrorCatalogContextProvider.LoadFromJsonsAsync(...)` must preserve exceptions from its five provider dependencies.

Relevant suites:

- `WhenItFails.Tests/Catalog/ErrorCatalogContextProviderProviderExceptionPropagationTests.cs`
- `WhenItFails.Tests/Catalog/ErrorCatalogContextProviderExceptionShapeTests.cs`
- `WhenItFails.Tests/Catalog/ErrorCatalogContextProviderCancellationPropagationTests.cs`

## Next boundary reconnaissance — `BuiltInErrorCatalogContextProvider`

Production file:

`WhenItFails/Catalog/WhenItFails/Catalog/BuiltInErrorCatalogContextProvider.cs`

Dependencies:

1. `IJsonsTemplateProvider`
2. `IErrorCatalogContextProvider`

Existing behavior already covers:

- null template collection → `WIF_BUILT_IN_TEMPLATES_NULL`;
- empty template collection → `WIF_BUILT_IN_TEMPLATES_EMPTY`;
- malformed template entries → stable Invalid responses;
- null context-provider response → `WIF_BUILT_IN_CONTEXT_PROVIDER_RESPONSE_NULL`;
- cancellation → rethrown;
- ordinary exceptions → `WIF_BUILT_IN_CONTEXT_LOAD_FAILED`.

The ordinary-exception path currently builds the public message as:

```text
The bundled WhenItFails catalog context could not be loaded: {exception.Message}
```

This exposes dependency/internal exception text through the public response. Repository searches found no existing test contract requiring that exception detail to be preserved in the response message.

## Verification state

- Clean continuation baseline: **1010/1010 GREEN, zero compiler warnings**.
- `CatalogProviderPipeline` audit is closed for the current scope.
- No production change has yet been made to `BuiltInErrorCatalogContextProvider`.

## Next recommended step

Add one focused ordinary-exception contract for `IJsonsTemplateProvider.GetTemplateFiles(...)` as consumed by `BuiltInErrorCatalogContextProvider`.

Expected stable response:

```text
Status: Failed
Data: null
Code: WIF_BUILT_IN_CONTEXT_LOAD_FAILED
Message: The bundled WhenItFails catalog context could not be loaded.
```

The test should assert that a sensitive raw dependency exception message is not present in `Response.Message`.

If the focused test is RED as expected, make the smallest production change: keep the existing code and catch structure, but remove `exception.Message` from the public message. Preserve `OperationCanceledException` propagation unchanged.

Keep changes small, tested, documented here, and committed directly to `master`.
