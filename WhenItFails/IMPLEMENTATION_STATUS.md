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
- `BuiltInErrorCatalogContextProvider` is the current focused hardening target.
- The `IJsonsTemplateProvider` dependency boundary is complete for the current scope.
- The injected `IErrorCatalogContextProvider` ordinary-exception contract is locally verified GREEN.
- The complete `WhenItFails.Tests` suite is locally verified **GREEN at 1013/1013 tests with zero compiler warnings**.

## 2026-09-10 — 1013/1013 GREEN built-in context-provider checkpoint

Checkpoint commit: this commit.
Context-provider ordinary-exception contract commit: `6913cd4d02769f42958128fc5471dba8fe72f3e8`.

Verified locally:

```text
WhenItFails.Tests
Failed:   0
Passed: 1013
Skipped:  0
Total:  1013
Compiler warnings: 0
```

The injected `IErrorCatalogContextProvider.LoadFromJsonsAsync(...)` ordinary-exception contract is verified:

```text
Status: Failed
Data: null
Code: WIF_BUILT_IN_CONTEXT_LOAD_FAILED
Message: The bundled WhenItFails catalog context could not be loaded.
```

The raw dependency exception detail does not escape through the public response.

`OperationCanceledException` is explicitly rethrown by `BuiltInErrorCatalogContextProvider.LoadAsync(...)`, so the next focused contract is exact-instance cancellation from the injected context provider.

## 2026-09-10 — 1012/1012 GREEN built-in template-provider boundary checkpoint

Checkpoint commit: `2ba14fe0cdfb56278c8e763f9672d3e619d82841`.
Template-provider cancellation contract commit: `5344b2a111a005e3b53cb3d8ba7e4c8986648de1`.
Template-provider sanitization fix commit: `13429fd33d6c467409007d084b63711cd2e9c472`.
Template-provider ordinary-exception contract commit: `5910b04e8942384c5f0c7cdd0f1dadff6aff2aa6`.

Verified locally:

```text
WhenItFails.Tests
Failed:   0
Passed: 1012
Skipped:  0
Total:  1012
Compiler warnings: 0
```

`IJsonsTemplateProvider.GetTemplateFiles(...)` boundary is complete for the current scope:

- null collection → `WIF_BUILT_IN_TEMPLATES_NULL`;
- empty collection → `WIF_BUILT_IN_TEMPLATES_EMPTY`;
- ordinary exception → stable sanitized `WIF_BUILT_IN_CONTEXT_LOAD_FAILED`;
- exact `OperationCanceledException` instance propagates unchanged.

## 2026-09-10 — 1010/1010 GREEN CatalogProviderPipeline checkpoint

Checkpoint commit: `1198abf789e7ac715aa36b5c77c764c7256bed1f`.
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

## `BuiltInErrorCatalogContextProvider` boundary map

Production file:

`WhenItFails/Catalog/WhenItFails/Catalog/BuiltInErrorCatalogContextProvider.cs`

Dependencies:

1. `IJsonsTemplateProvider` — complete for current scope.
2. `IErrorCatalogContextProvider` — null response and ordinary exception covered; exact cancellation next.

Established context-provider behavior:

- null response → `WIF_BUILT_IN_CONTEXT_PROVIDER_RESPONSE_NULL`;
- ordinary exception → stable sanitized `WIF_BUILT_IN_CONTEXT_LOAD_FAILED`;
- `OperationCanceledException` is explicitly rethrown;
- temporary-directory cleanup remains in `finally`.

## Verification state

- Clean continuation baseline: **1013/1013 GREEN, zero compiler warnings**.
- Template-provider boundary is complete for the current scope.
- Context-provider ordinary-exception behavior is locally verified.
- No exact-instance context-provider cancellation contract has been committed yet at this checkpoint.

## Next recommended step

Add a focused exact-instance cancellation contract for `IErrorCatalogContextProvider.LoadFromJsonsAsync(...)` as consumed by `BuiltInErrorCatalogContextProvider`.

The fake dependency should return a faulted task containing a specific `OperationCanceledException` instance and the test should use `Assert.Same(...)`.

No production change is expected.

After that contract is GREEN, consider both injected dependency boundaries of `BuiltInErrorCatalogContextProvider` complete and perform fresh repository reconnaissance for the next target.

Keep changes small, tested, documented here, and committed directly to `master`.
