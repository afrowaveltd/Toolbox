# Implementation status

Last updated: 2026-09-10

This file is the continuation point for `WhenItFails` development. Update it after every implementation, test, catalog, or documentation change that alters the current state or recommended next step.

## Current focus

Hardening dependency boundaries while preserving established public exception contracts, including malformed async dependency results such as null `Task` values.

## Current state

- `ErrorDescriptorResolver` and `ErrorDescriptorService` hardening are complete for the current scope.
- `ErrorCatalogRuntime` dependency boundaries covered so far are complete for the current scope.
- `ErrorCatalogProvider` and `CatalogProviderPipeline` dependency-boundary audits are complete for the current scope.
- `ErrorCatalogContextProvider` is intentionally transparent for exceptions and null tasks from its internal providers; do not normalize those lower-layer contracts.
- `BuiltInErrorCatalogContextProvider` template-provider and injected context-provider ordinary-exception/cancellation/null-response/null-task behavior is complete for the current scope.
- The complete `WhenItFails.Tests` suite is locally verified **GREEN at 1015/1015 tests with zero compiler warnings**.
- The next focused target is malformed async dependency behavior in `ErrorCatalogInitializer`.

## 2026-09-10 — 1015/1015 GREEN built-in context-provider null-task checkpoint

Checkpoint commit: this commit.
Null-task contract commit: `ba3a8533c7d33f352bb43d626fa032b2e906daac`.
Built-in provider checkpoint before this contract: `0bbe72d86d885b2de89d66e8233fe15d0d406ea1`.

Verified locally:

```text
WhenItFails.Tests
Failed:   0
Passed: 1015
Skipped:  0
Total:  1015
Compiler warnings: 0
```

`BuiltInErrorCatalogContextProvider` now has explicit contracts for:

- template provider null/empty collections and malformed entries;
- template-provider ordinary exception normalization;
- template-provider exact cancellation propagation;
- injected context-provider null `Response`;
- injected context-provider ordinary exception normalization;
- injected context-provider exact cancellation propagation;
- injected context-provider null `Task` normalization;
- temporary workspace cleanup through `finally`.

A null `Task` from the injected context provider normalizes to:

```text
Status: Failed
Data: null
Code: WIF_BUILT_IN_CONTEXT_LOAD_FAILED
Message: The bundled WhenItFails catalog context could not be loaded.
```

## Established transparent lower boundary — do not normalize

`ErrorCatalogContextProvider.LoadFromJsonsAsync(...)` intentionally preserves exceptions and null-task behavior from its five internal catalog providers.

Relevant suites include:

- `WhenItFails.Tests/Catalog/ErrorCatalogContextProviderProviderExceptionPropagationTests.cs`
- `WhenItFails.Tests/Catalog/ErrorCatalogContextProviderExceptionShapeTests.cs`
- `WhenItFails.Tests/Catalog/ErrorCatalogContextProviderCancellationPropagationTests.cs`
- `WhenItFails.Tests/Catalog/ErrorCatalogContextProviderOwnerNullTaskTests.cs`
- `WhenItFails.Tests/Catalog/ErrorCatalogContextProviderProfileNullTaskTests.cs`

Do not replace those transparent contracts with normalization at that layer.

## Fresh reconnaissance notes

A repository-wide search for raw `exception.Message` use found `JsonCatalogDocumentLoader`, `JsonCatalogDocumentWriter`, `JsonsBootstrapper`, and Setter tooling.

Do **not** change `JsonCatalogDocumentLoader.InvalidJson` merely to sanitize the parser message: `Docs/Loading-and-Normalization/en.md` explicitly documents that the loader includes the JSON parser message in the structured failure response.

Fresh null-task reconnaissance found no dedicated `ErrorCatalogInitializer` null-task contract for `IJsonsBootstrapper.EnsureWorkspaceAsync(...)` or `IErrorCatalogContextProvider.LoadFromJsonsAsync(...)`.

Current `ErrorCatalogInitializer` production code awaits both dependencies inside ordinary-exception normalization guards, so a null `Task` should become the same stable failure as an ordinary dependency exception.

## Verification state

- Clean continuation baseline: **1015/1015 GREEN, zero compiler warnings**.
- `BuiltInErrorCatalogContextProvider` audit is complete for the current scope.
- No new test has been added after this checkpoint yet.

## Next recommended step

Add one focused null-task contract for `ErrorCatalogInitializer`'s `IJsonsBootstrapper.EnsureWorkspaceAsync(...)` dependency.

Expected behavior:

```text
Status: Failed
Data: null
Code: WIF_INITIALIZER_BOOTSTRAPPER_FAILED
Message: The JSON workspace bootstrapper failed.
```

The downstream context provider must not run and the previously stored context must remain untouched.

No production change is expected because the `await` is already inside the initializer's ordinary-exception normalization guard.

After that contract is GREEN, run the complete suite before testing the initializer context-provider null-task case separately.

Keep changes small, tested, documented here, and committed directly to `master`.
