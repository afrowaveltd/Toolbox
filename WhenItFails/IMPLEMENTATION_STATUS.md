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
- The complete `WhenItFails.Tests` suite is locally verified **GREEN at 1012/1012 tests with zero compiler warnings** before the new built-in context-provider exception contract.
- A focused ordinary-exception contract for the injected `IErrorCatalogContextProvider` is committed and awaits local verification.

## 2026-09-10 — built-in context-provider ordinary-exception contract

Contract commit: `6913cd4d02769f42958128fc5471dba8fe72f3e8`.

Added:

`WhenItFails.Tests/Catalog/BuiltInErrorCatalogContextProviderContextProviderExceptionContractTests.cs`

Test:

`LoadAsync_WhenContextProviderThrows_ReturnsStableFailureWithoutExceptionDetail`

The test supplies a valid built-in template so execution reaches `IErrorCatalogContextProvider.LoadFromJsonsAsync(...)`. The injected dependency returns a faulted task containing:

```text
System.InvalidOperationException:
Sensitive built-in context provider detail must not escape.
```

Required public contract:

```text
Status: Failed
Data: null
Code: WIF_BUILT_IN_CONTEXT_LOAD_FAILED
Message: The bundled WhenItFails catalog context could not be loaded.
```

The raw dependency exception detail must not appear in the public response.

No production code changed. The existing sanitized outer catch in `BuiltInErrorCatalogContextProvider.LoadAsync(...)` should already satisfy this contract, so focused verification is expected to be GREEN.

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
2. `IErrorCatalogContextProvider` — ordinary-exception contract awaiting GREEN; exact cancellation next.

Established context-provider behavior:

- null response → `WIF_BUILT_IN_CONTEXT_PROVIDER_RESPONSE_NULL`;
- ordinary exceptions fall under the sanitized built-in load guard → `WIF_BUILT_IN_CONTEXT_LOAD_FAILED`;
- `OperationCanceledException` is explicitly rethrown;
- temporary-directory cleanup remains in `finally`.

## Context-provider dependency reconnaissance

Fresh repository searches found no `BuiltInErrorCatalogContextProvider` contract requiring exceptions from its injected `IErrorCatalogContextProvider.LoadFromJsonsAsync(...)` dependency to propagate unchanged.

The pass-through contracts on `ErrorCatalogContextProvider` itself apply one layer lower, to its own five internal provider dependencies, and do not conflict with normalization at `BuiltInErrorCatalogContextProvider`.

## Verification state

- Clean continuation baseline: **1012/1012 GREEN, zero compiler warnings**.
- `IJsonsTemplateProvider` boundary is complete for the current scope.
- New context-provider ordinary-exception contract is committed and awaits local verification.
- No production change is expected for this contract.
- Expected complete-suite count after it passes: **1013/1013 GREEN with zero compiler warnings**.

## Recommended verification

Pull current `master` and run:

```powershell
dotnet test WhenItFails.Tests --filter "FullyQualifiedName~LoadAsync_WhenContextProviderThrows_ReturnsStableFailureWithoutExceptionDetail"
dotnet test WhenItFails.Tests
```

Expected results:

```text
Focused contract: GREEN
Complete suite: 1013/1013 GREEN
Compiler warnings: 0
```

## Next recommended step

After **1013/1013 GREEN** is confirmed, record that checkpoint and add an exact-instance cancellation contract for `IErrorCatalogContextProvider.LoadFromJsonsAsync(...)` as consumed by `BuiltInErrorCatalogContextProvider`.

No production change should be needed because `BuiltInErrorCatalogContextProvider.LoadAsync(...)` already rethrows `OperationCanceledException`.

After cancellation is verified, consider both injected dependency boundaries of `BuiltInErrorCatalogContextProvider` complete and perform fresh reconnaissance for the next target.

Keep changes small, tested, documented here, and committed directly to `master`.
