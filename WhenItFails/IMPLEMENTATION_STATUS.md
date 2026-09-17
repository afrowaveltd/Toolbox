# Implementation status

Last updated: 2026-09-17

This file is the continuation point for `WhenItFails` development. Git history contains the detailed chronological checkpoints; keep this file focused on the current verified state, established contracts, and next step.

## Current focus

Hardening dependency boundaries and malformed-context/configuration handling while preserving established public exception contracts.

## Current verified state

- Complete `WhenItFails.Tests` suite: **1040/1040 GREEN, zero compiler warnings**.
- `ErrorDescriptorResolver` and `ErrorDescriptorService` hardening are complete for the current scope.
- `ErrorCatalogProvider` and `CatalogProviderPipeline` dependency-boundary audits are complete for the current scope.
- `BuiltInErrorCatalogContextProvider` dependency-boundary audit is complete for the current scope.
- `ErrorCatalogInitializer` bootstrapper/context-provider ordinary-exception, cancellation, null-response and null-task behavior is complete for the current scope.
- `ErrorCatalogRuntime` initializer and both built-in-provider runtime paths are complete for null response, ordinary exception, null task and exact cancellation behavior.
- Direct `JsonsBootstrapper` → `IJsonsTemplateProvider.GetTemplateFiles(...)` boundary is complete for malformed results, ordinary-exception normalization and exact-instance cancellation propagation.
- `JsonCatalogDocumentWriter` serialization-failure temporary-file cleanup and deterministic pre-cancellation behavior are verified.
- `ErrorProfileSelectionService` → `IErrorProfileResolver.Resolve(...)` boundary is complete for null result, ordinary-exception normalization and exact-instance cancellation propagation.
- `ErrorProfileSelectionService` classifies all resolver-consumed nullable collections currently audited as malformed input rather than resolver failure.
- Scalar `ErrorDefinition` reconnaissance did not reveal an exception boundary; duplicating the full validator inside selection service remains intentionally deferred.
- `JsonsBootstrapper` now rejects a whitespace `PackageDirectoryName` before any filesystem operation, reusing the stable `ErrorCatalogContextProvider` contract.

## 2026-09-17 — 1040/1040 GREEN bootstrap whitespace package-directory checkpoint

Contract commit:
`e3ddf3e94be3e562382a1fcd7b27719fa29fd90d`

Production guard commit:
`c1ca5693f37bc129edde761b2452f4548edc2a0b`

Previous checkpoint commit:
`715659225484ff68d50a4c9d92185ba960587eda`

Locally verified:

```text
WhenItFails.Tests
Failed:   0
Passed: 1040
Skipped:  0
Total:  1040
Compiler warnings: 0
```

`JsonsBootstrapper.EnsureWorkspaceAsync(...)` rejects a whitespace `PackageDirectoryName` before entering the filesystem block and returns:

```text
Status: Invalid
Data: null
Code: WIF_JSONS_PACKAGE_DIRECTORY_NAME_EMPTY
Message: The package directory name cannot be empty.
```

The focused contract also verifies that the invalid configuration creates no root workspace directory.

The null `PackageDirectoryName` case remains intentionally unchanged and is the next concrete bootstrap-boundary contract.

## 2026-09-17 — 1039/1039 GREEN null error-categories checkpoint

Contract commit:
`f34a2c0a3c90920771db962a7140ab881af53b2a`

Production guard commit:
`2872fce9108a85ebb76d0836c426df523ae1dbd9`

Checkpoint commit:
`715659225484ff68d50a4c9d92185ba960587eda`

Locally verified:

```text
WhenItFails.Tests
Failed:   0
Passed: 1039
Skipped:  0
Total:  1039
```

This completed the current resolver-consumed collection-shape audit for `ErrorProfileSelectionService`.

## Established transparent lower boundary — do not normalize

`ErrorCatalogContextProvider.LoadFromJsonsAsync(...)` intentionally preserves exceptions and null-task behavior from its five internal catalog providers.

Relevant suites include:

- `WhenItFails.Tests/Catalog/ErrorCatalogContextProviderProviderExceptionPropagationTests.cs`
- `WhenItFails.Tests/Catalog/ErrorCatalogContextProviderExceptionShapeTests.cs`
- `WhenItFails.Tests/Catalog/ErrorCatalogContextProviderCancellationPropagationTests.cs`
- `WhenItFails.Tests/Catalog/ErrorCatalogContextProviderOwnerNullTaskTests.cs`
- `WhenItFails.Tests/Catalog/ErrorCatalogContextProviderProfileNullTaskTests.cs`

Do not replace those transparent contracts with normalization at that layer.

## Established documented behavior — preserve

`JsonCatalogDocumentLoader.InvalidJson` deliberately includes the JSON parser message. `WhenItFails/Docs/Loading-and-Normalization/en.md` documents this behavior; do not sanitize it as incidental hardening.

## Recent verified checkpoints

- 1037/1037 — profile selection null error-subcategories classification complete.
- 1038/1038 — profile selection null error-tags classification complete.
- 1039/1039 — profile selection null error-categories classification complete; resolver-consumed collection-shape audit closed.
- 1040/1040 — bootstrap whitespace package directory name rejected before filesystem mutation.

## Next recommended step

Add one focused `JsonsBootstrapper` contract for `PackageDirectoryName = null`, reusing the existing `ErrorCatalogContextProvider` contract:

```text
WIF_JSONS_PACKAGE_DIRECTORY_NAME_NULL
The package directory name cannot be null.
```

Require rejection before any filesystem side effect. Keep production unchanged until the focused run establishes the current behavior.
