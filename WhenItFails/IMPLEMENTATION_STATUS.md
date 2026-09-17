# Implementation status

Last updated: 2026-09-17

This file is the continuation point for `WhenItFails` development. Git history contains the detailed chronological checkpoints; keep this file focused on the current verified state, established contracts, and next step.

## Current focus

Hardening dependency boundaries and malformed-context/configuration handling while preserving established public exception contracts.

## Current verified state

- Complete `WhenItFails.Tests` suite: **1041/1041 GREEN, zero compiler warnings**.
- The SDK emitted `NETSDK1057` informational messages because the local SDK is `.NET 11.0.100-rc.1`; these are SDK support-policy messages, not compiler warnings from the Toolbox code.
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
- `JsonsBootstrapper` now rejects both whitespace and null `PackageDirectoryName` before any filesystem operation, reusing the stable `ErrorCatalogContextProvider` contracts.

## 2026-09-17 — 1041/1041 GREEN bootstrap null package-directory checkpoint

Contract commit:
`d4d4cea2e00f3ba5a7968afa8eedeb99c8a4c99a`

Production guard commit:
`de85b3c460448d0dbe255dd3663e79cc05e4db4f`

Previous checkpoint commit:
`b8e2b7161584936d4b7c570db7c07d4a746397e4`

Locally verified:

```text
WhenItFails.Tests
Failed:   0
Passed: 1041
Skipped:  0
Total:  1041
Compiler warnings: 0
```

The local SDK emitted `NETSDK1057` messages for `.NET 11.0.100-rc.1`; they are informational SDK support-policy messages and do not change the zero-warning code checkpoint.

`JsonsBootstrapper.EnsureWorkspaceAsync(...)` rejects `PackageDirectoryName = null` before entering the filesystem block and returns:

```text
Status: Invalid
Data: null
Code: WIF_JSONS_PACKAGE_DIRECTORY_NAME_NULL
Message: The package directory name cannot be null.
```

The focused contract also verifies that invalid configuration creates no root workspace directory.

## 2026-09-17 — 1040/1040 GREEN bootstrap whitespace package-directory checkpoint

Contract commit:
`e3ddf3e94be3e562382a1fcd7b27719fa29fd90d`

Production guard commit:
`c1ca5693f37bc129edde761b2452f4548edc2a0b`

Checkpoint commit:
`b8e2b7161584936d4b7c570db7c07d4a746397e4`

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

The focused contract also verifies that invalid configuration creates no root workspace directory.

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

- 1039/1039 — profile selection null error-categories classification complete; resolver-consumed collection-shape audit closed.
- 1040/1040 — bootstrap whitespace package directory name rejected before filesystem mutation.
- 1041/1041 — bootstrap null package directory name rejected before filesystem mutation.

## Next recommended step

Continue the `JsonsOptions` bootstrap-boundary audit one contract at a time. The next concrete case is `RootDirectory = null`, for which `ErrorCatalogContextProvider` already defines the stable contract:

```text
WIF_JSONS_ROOT_DIRECTORY_NULL
The JSON root directory cannot be null.
```

Use an isolated absolute temporary package path in the focused test so current erroneous behavior can be observed safely without creating a relative directory in the repository. Keep production unchanged until the focused run establishes the current response and side-effect shape.
