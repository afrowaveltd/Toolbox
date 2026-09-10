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
- The complete `WhenItFails.Tests` suite is locally verified **GREEN at 1010/1010 tests with zero compiler warnings** before the new built-in template-provider exception contract.
- `BuiltInErrorCatalogContextProvider` is the current focused hardening target.
- The focused template-provider exception-message contract produced the expected RED because the raw dependency exception detail was appended to the public response message.
- A minimal production sanitization fix is committed and awaits focused + full-suite verification.

## 2026-09-10 — built-in template-provider exception-message fix

Contract commit: `5910b04e8942384c5f0c7cdd0f1dadff6aff2aa6`.
Production fix commit: `13429fd33d6c467409007d084b63711cd2e9c472`.

Test:

`LoadAsync_WhenTemplateProviderThrows_ReturnsStableFailureWithoutExceptionDetail`

Observed locally before the production fix:

```text
RED
Expected:
The bundled WhenItFails catalog context could not be loaded.

Actual:
The bundled WhenItFails catalog context could not be loaded: Sensitive built-in template provider detail must not escape.
```

The status and issue code were already correct. The failure was specifically the public message shape.

Production behavior now keeps the existing stable failure code/status but removes the raw dependency exception detail:

```text
Status: Failed
Data: null
Code: WIF_BUILT_IN_CONTEXT_LOAD_FAILED
Message: The bundled WhenItFails catalog context could not be loaded.
```

`OperationCanceledException` is still rethrown unchanged and the temporary-directory cleanup remains in `finally`.

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

- `OperationCanceledException` is rethrown;
- ordinary exceptions map to `WIF_BUILT_IN_CONTEXT_LOAD_FAILED`;
- public ordinary-exception message is now sanitized and no longer includes `exception.Message`.

Repository reconnaissance found no existing contract requiring raw ordinary-exception detail to remain public.

## Verification state

- Clean continuation baseline before this contract: **1010/1010 GREEN, zero compiler warnings**.
- Focused template-provider exception-message contract produced the expected RED.
- Production sanitization fix commit `13429fd33d6c467409007d084b63711cd2e9c472` is committed and awaits local verification.
- Expected complete-suite count after the fix passes: **1011/1011 GREEN with zero compiler warnings**.

## Recommended verification

Pull current `master` and run:

```powershell
dotnet test WhenItFails.Tests --filter "FullyQualifiedName~LoadAsync_WhenTemplateProviderThrows_ReturnsStableFailureWithoutExceptionDetail"
dotnet test WhenItFails.Tests
```

Expected results:

```text
Focused contract: GREEN
Complete suite: 1011/1011 GREEN
Compiler warnings: 0
```

## Next recommended step

After **1011/1011 GREEN** is confirmed, record that checkpoint and add an exact-instance cancellation contract for an `OperationCanceledException` thrown by `IJsonsTemplateProvider.GetTemplateFiles(...)`.

No production change should be needed because `BuiltInErrorCatalogContextProvider.LoadAsync(...)` already rethrows `OperationCanceledException`.

After template-provider cancellation is verified, inspect the `IErrorCatalogContextProvider.LoadFromJsonsAsync(...)` dependency inside `BuiltInErrorCatalogContextProvider` separately, including ordinary exception sanitization and exact cancellation behavior.

Keep changes small, tested, documented here, and committed directly to `master`.
