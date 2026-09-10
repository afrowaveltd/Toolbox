# Implementation status

Last updated: 2026-09-10

This file is the continuation point for `WhenItFails` development. Update it after every implementation, test, catalog, or documentation change that alters the current state or recommended next step.

## Current focus

Hardening dependency boundaries while preserving established public exception contracts, including malformed async dependency results such as null `Task` values.

## Current state

- `ErrorDescriptorResolver` and `ErrorDescriptorService` hardening are complete for the current scope.
- `ErrorCatalogRuntime` dependency boundaries covered so far are complete for the current scope.
- `ErrorCatalogInitializer` bootstrapper/context-provider/context-store boundaries covered so far are complete for the current scope.
- `ErrorCatalogProvider` loader/normalizer/validator/factory boundary audit is complete for the current scope.
- `CatalogProviderPipeline` loader/normalizer/validator/payload-factory boundary audit is complete for the current scope.
- `ErrorCatalogContextProvider` is intentionally transparent for exceptions and null tasks from its five internal providers; existing propagation/shape/null-task contracts must not be replaced by normalization there.
- `BuiltInErrorCatalogContextProvider` template-provider and injected context-provider ordinary-exception/cancellation/null-response behavior is complete for the previously defined scope.
- The complete `WhenItFails.Tests` suite is locally verified **GREEN at 1014/1014 tests with zero compiler warnings**.

## 2026-09-10 — 1014/1014 GREEN BuiltInErrorCatalogContextProvider checkpoint

Checkpoint commit: this commit.
Context-provider cancellation contract commit: `59da25c7489c2d131d10a1285ed5715c240f0ca6`.
Context-provider ordinary-exception contract commit: `6913cd4d02769f42958128fc5471dba8fe72f3e8`.
Template-provider cancellation contract commit: `5344b2a111a005e3b53cb3d8ba7e4c8986648de1`.
Template-provider sanitization fix commit: `13429fd33d6c467409007d084b63711cd2e9c472`.
Template-provider ordinary-exception contract commit: `5910b04e8942384c5f0c7cdd0f1dadff6aff2aa6`.

Verified locally:

```text
WhenItFails.Tests
Failed:   0
Passed: 1014
Skipped:  0
Total:  1014
Compiler warnings: 0
```

`BuiltInErrorCatalogContextProvider` established behavior now includes:

- template provider null collection → `WIF_BUILT_IN_TEMPLATES_NULL`;
- template provider empty collection → `WIF_BUILT_IN_TEMPLATES_EMPTY`;
- malformed template entries → stable Invalid responses;
- template-provider ordinary exception → `WIF_BUILT_IN_CONTEXT_LOAD_FAILED` with sanitized message;
- template-provider exact `OperationCanceledException` instance propagates unchanged;
- injected context-provider null `Response` → `WIF_BUILT_IN_CONTEXT_PROVIDER_RESPONSE_NULL`;
- injected context-provider ordinary exception → `WIF_BUILT_IN_CONTEXT_LOAD_FAILED` with sanitized message;
- injected context-provider exact `OperationCanceledException` instance propagates unchanged;
- temporary workspace cleanup remains in `finally`.

Stable public ordinary-failure message:

```text
The bundled WhenItFails catalog context could not be loaded.
```

## Established transparent lower boundary — do not normalize

`ErrorCatalogContextProvider.LoadFromJsonsAsync(...)` intentionally preserves exceptions from its five internal catalog providers.

Relevant suites include:

- `WhenItFails.Tests/Catalog/ErrorCatalogContextProviderProviderExceptionPropagationTests.cs`
- `WhenItFails.Tests/Catalog/ErrorCatalogContextProviderExceptionShapeTests.cs`
- `WhenItFails.Tests/Catalog/ErrorCatalogContextProviderCancellationPropagationTests.cs`
- `WhenItFails.Tests/Catalog/ErrorCatalogContextProviderOwnerNullTaskTests.cs`
- `WhenItFails.Tests/Catalog/ErrorCatalogContextProviderProfileNullTaskTests.cs`

Null tasks at this lower transparent boundary are intentionally observable as `NullReferenceException`; do not normalize them there.

## Fresh reconnaissance after 1014 GREEN

A repository-wide search for remaining raw `exception.Message` use found `JsonCatalogDocumentLoader`, `JsonCatalogDocumentWriter`, `JsonsBootstrapper`, and Setter tooling.

Do **not** change `JsonCatalogDocumentLoader.InvalidJson` merely to sanitize the parser message: `Docs/Loading-and-Normalization/en.md` explicitly documents that the loader includes the JSON parser message in the structured failure response. That behavior is therefore an established public contract unless deliberately redesigned later with documentation and migration consideration.

The next smallest unverified edge is instead a malformed async dependency result at the normalizing `BuiltInErrorCatalogContextProvider` layer: the injected `IErrorCatalogContextProvider.LoadFromJsonsAsync(...)` may incorrectly return a null `Task`.

Current production behavior should already convert the resulting ordinary `NullReferenceException` into the stable sanitized built-in load failure because the `await` is inside the normalizing try/catch.

## Next recommended step

Add a focused contract to `BuiltInErrorCatalogContextProviderContextProviderExceptionContractTests.cs`:

```text
LoadAsync_WhenContextProviderReturnsNullTask_ReturnsStableFailure
```

The fake context provider should return `null!` from `LoadFromJsonsAsync(...)`.

Expected public result:

```text
Status: Failed
Data: null
Code: WIF_BUILT_IN_CONTEXT_LOAD_FAILED
Message: The bundled WhenItFails catalog context could not be loaded.
```

No production change is expected. If the focused contract is GREEN, run the complete suite; expected count will be **1015/1015**.

After that, perform fresh reconnaissance for other async dependency null-task gaps before selecting a new behavior change.

Keep changes small, tested, documented here, and committed directly to `master`.
