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
- Template-provider ordinary-exception sanitization is locally verified GREEN.
- The complete `WhenItFails.Tests` suite is locally verified **GREEN at 1011/1011 tests with zero compiler warnings** before the new template-provider cancellation contract.
- A focused exact-instance cancellation contract for `IJsonsTemplateProvider.GetTemplateFiles(...)` is committed and awaits local verification.

## 2026-09-10 — built-in template-provider cancellation contract

Contract commit: `5344b2a111a005e3b53cb3d8ba7e4c8986648de1`.

Updated:

`WhenItFails.Tests/Catalog/BuiltInErrorCatalogContextProviderTemplateProviderExceptionContractTests.cs`

Added:

`LoadAsync_WhenTemplateProviderCancels_RethrowsSameOperationCanceledException`

Contract:

```text
template provider
    => throws a specific OperationCanceledException instance
                         ↓
rethrow the exact same OperationCanceledException instance
```

The test uses `Assert.Same(...)`. The context provider throws if reached, so cancellation must short-circuit before downstream loading.

No production code changed. `BuiltInErrorCatalogContextProvider.LoadAsync(...)` already has a dedicated `catch (OperationCanceledException) { throw; }`, so this focused contract is expected to be GREEN.

## 2026-09-10 — 1011/1011 GREEN built-in template-provider checkpoint

Checkpoint commit: `c4c4f0b82d53aed8ca7208a58fce473be5787062`.
Template-provider sanitization fix commit: `13429fd33d6c467409007d084b63711cd2e9c472`.
Template-provider ordinary-exception contract commit: `5910b04e8942384c5f0c7cdd0f1dadff6aff2aa6`.

Verified locally:

```text
WhenItFails.Tests
Failed:   0
Passed: 1011
Skipped:  0
Total:  1011
Compiler warnings: 0
```

Template-provider ordinary exceptions are normalized to:

```text
Status: Failed
Data: null
Code: WIF_BUILT_IN_CONTEXT_LOAD_FAILED
Message: The bundled WhenItFails catalog context could not be loaded.
```

The raw dependency `exception.Message` no longer escapes through the public response.

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

1. `IJsonsTemplateProvider`
2. `IErrorCatalogContextProvider`

Established malformed-result behavior:

- null template collection → `WIF_BUILT_IN_TEMPLATES_NULL`;
- empty template collection → `WIF_BUILT_IN_TEMPLATES_EMPTY`;
- malformed template entries → stable Invalid responses;
- null context-provider response → `WIF_BUILT_IN_CONTEXT_PROVIDER_RESPONSE_NULL`.

Current exception behavior:

- ordinary exceptions map to stable sanitized `WIF_BUILT_IN_CONTEXT_LOAD_FAILED`;
- `OperationCanceledException` is rethrown;
- temporary-directory cleanup remains in `finally`.

## Verification state

- Clean continuation baseline: **1011/1011 GREEN, zero compiler warnings**.
- Template-provider ordinary-exception sanitization is locally verified.
- Exact-instance template-provider cancellation contract is committed and awaits focused verification.
- No production change is expected for this cancellation contract.
- Expected complete-suite count after it passes: **1012 tests**.

## Recommended verification

Pull current `master` and run:

```powershell
dotnet test WhenItFails.Tests --filter "FullyQualifiedName~LoadAsync_WhenTemplateProviderCancels_RethrowsSameOperationCanceledException"
```

Expected result: GREEN.

Then run:

```powershell
dotnet test WhenItFails.Tests
```

Expected complete-suite result: **1012/1012 GREEN with zero compiler warnings**.

## Next recommended step

After **1012/1012 GREEN** is confirmed, consider the `IJsonsTemplateProvider` dependency boundary complete for the current scope.

Then inspect the `IErrorCatalogContextProvider.LoadFromJsonsAsync(...)` dependency inside `BuiltInErrorCatalogContextProvider` separately, searching existing propagation/shape/cancellation contracts before adding an ordinary-exception sanitization contract.

Keep changes small, tested, documented here, and committed directly to `master`.
