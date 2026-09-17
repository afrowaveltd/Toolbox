# Implementation status

Last updated: 2026-09-17

This file is the continuation point for `WhenItFails` development. Git history contains the detailed chronological checkpoints; keep this file focused on the current verified state, established contracts, and next step.

## Current focus

Hardening dependency boundaries and malformed-context/configuration handling while preserving established public exception contracts.

## Current verified state

- Complete `WhenItFails.Tests` suite: **1043/1043 GREEN, zero compiler warnings**.
- The SDK emits `NETSDK1057` informational messages because the local SDK is `.NET 11.0.100-rc.1`; these are SDK support-policy messages, not compiler warnings from Toolbox code.
- `ErrorDescriptorResolver` and `ErrorDescriptorService` hardening are complete for the current scope.
- `ErrorCatalogProvider` and `CatalogProviderPipeline` dependency-boundary audits are complete for the current scope.
- `BuiltInErrorCatalogContextProvider` dependency-boundary audit is complete for the current scope.
- `ErrorCatalogInitializer` bootstrapper/context-provider ordinary-exception, cancellation, null-response and null-task behavior is complete for the current scope.
- `ErrorCatalogRuntime` initializer and both built-in-provider runtime paths are complete for null response, ordinary exception, null task and exact cancellation behavior.
- Direct `JsonsBootstrapper` → `IJsonsTemplateProvider.GetTemplateFiles(...)` boundary is complete for malformed results, ordinary-exception normalization and exact-instance cancellation propagation.
- `JsonCatalogDocumentWriter` serialization-failure temporary-file cleanup and deterministic pre-cancellation behavior are verified.
- `ErrorProfileSelectionService` → `IErrorProfileResolver.Resolve(...)` boundary is complete for null result, ordinary-exception normalization and exact-instance cancellation propagation.
- `ErrorProfileSelectionService` classifies all resolver-consumed nullable collections currently audited as malformed input rather than resolver failure.
- `JsonsBootstrapper` rejects null/whitespace `RootDirectory` and `PackageDirectoryName` before filesystem mutation.
- `PackageDirectoryName` nullable-flow cleanup remains verified with zero compiler warnings.

## 2026-09-17 — 1043/1043 GREEN bootstrap whitespace root-directory checkpoint

Contract commit:
`b35339be52737211d29516a66db57a43b359e7f5`

Production guard commit:
`3f5fcceafac6c7662822c49c3810d4dadd1a77c6`

Previous checkpoint commit:
`a203fdf8f601b51a9448241f10a838f21af7c299`

Locally verified:

```text
WhenItFails.Tests
Failed:   0
Passed: 1043
Skipped:  0
Total:  1043
Compiler warnings: 0
```

`JsonsBootstrapper.EnsureWorkspaceAsync(...)` rejects a whitespace `RootDirectory` before package-directory validation or any filesystem operation and returns:

```text
Status: Invalid
Data: null
Code: WIF_JSONS_ROOT_DIRECTORY_EMPTY
Message: The JSON root directory cannot be empty.
```

Together with the prior null-root and null/whitespace package-directory contracts, the bootstrap directory-option shape is now aligned with `ErrorCatalogContextProvider` for the current scope.

## Fresh reconnaissance — remaining `JsonsOptions` filename boundary

`JsonsOptions` also exposes five configurable catalog file names:

- `ErrorCatalogFileName`
- `CategoryCatalogFileName`
- `CodeGroupCatalogFileName`
- `OwnerCatalogFileName`
- `ProfilesFileName`

`ErrorCatalogContextProvider.ValidateJsonsOptions(...)` already defines distinct stable null/empty contracts for every one of these values. `JsonsBootstrapper` currently validates only the resulting `JsonsTemplateFile.TargetFileName` after the package directory has already been created and after `IJsonsTemplateProvider.GetTemplateFiles(...)` has been invoked.

This creates a concrete boundary mismatch: malformed caller configuration can cause filesystem mutation and provider execution before it is classified, and the default provider can convert an option-level problem into the more generic template-target contract.

The next focused contract is `ErrorCatalogFileName = null`, reusing exactly:

```text
WIF_JSONS_ERROR_CATALOG_FILE_NAME_NULL
The error catalog file name cannot be null.
```

The focused contract should require rejection before both filesystem mutation and template-provider invocation. Production must remain unchanged until the local RED establishes the current behavior.

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

- 1041/1041 — bootstrap null package directory name rejected before filesystem mutation.
- 1042/1042 — bootstrap null root directory rejected before filesystem mutation; nullable-flow cleanup verified with zero compiler warnings.
- 1043/1043 — bootstrap whitespace root directory rejected before filesystem mutation.

## Next recommended step

Add one focused `JsonsBootstrapper` contract for `ErrorCatalogFileName = null`. Require the stable option-level `WIF_JSONS_ERROR_CATALOG_FILE_NAME_NULL` response, no filesystem creation, and no template-provider invocation. Keep production unchanged until the focused run confirms the current behavior.