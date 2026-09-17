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
- Direct `JsonsBootstrapper` → `IJsonsTemplateProvider.GetTemplateFiles(...)` boundary is complete for malformed provider results, ordinary-exception normalization and exact-instance cancellation propagation.
- `JsonCatalogDocumentWriter` serialization-failure temporary-file cleanup and deterministic pre-cancellation behavior are verified.
- `ErrorProfileSelectionService` → `IErrorProfileResolver.Resolve(...)` boundary is complete for null result, ordinary-exception normalization and exact-instance cancellation propagation.
- `ErrorProfileSelectionService` classifies all resolver-consumed nullable collections currently audited as malformed input rather than resolver failure.
- `JsonsBootstrapper` rejects null/whitespace `RootDirectory` and `PackageDirectoryName` before filesystem mutation.
- A focused bootstrap contract for `ErrorCatalogFileName = null` is committed and awaits local RED verification; production is intentionally unchanged for this case.

## 2026-09-17 — bootstrap null error-catalog-file-name contract

Contract commit:
`91bba6f1c1cc11c2f7e6b12c5814a2afbb0ef64d`

Baseline checkpoint commit:
`7eaf2eda992ce59632dc086319e2a8c246858b8c`

Added:

`WhenItFails.Tests/Bootstrap/JsonsBootstrapperNullErrorCatalogFileNameContractTests.cs`

Contract:

`EnsureWorkspaceAsync_WhenErrorCatalogFileNameIsNull_ReturnsInvalidBeforeProviderOrFilesystem`

`ErrorCatalogContextProvider.ValidateJsonsOptions(...)` already defines the stable option-level contract:

```text
Status: Invalid
Data: null
Code: WIF_JSONS_ERROR_CATALOG_FILE_NAME_NULL
Message: The error catalog file name cannot be null.
```

The focused bootstrap contract additionally requires both boundary guarantees:

```text
template provider invoked: false
workspace root created: false
```

The test uses an isolated temporary root and a tracking template provider that returns an empty template collection. Current `JsonsBootstrapper` creates the package workspace before invoking the provider and does not validate `ErrorCatalogFileName` itself. Therefore current behavior is expected to call the provider, create the workspace, and return `Success`.

Expected focused RED:

```text
Expected: Invalid
Actual:   Success
```

Production remains unchanged until this RED is locally confirmed.

Expected eventual complete-suite count after the contract passes: **1044/1044 GREEN, zero compiler warnings**.

## 2026-09-17 — 1043/1043 GREEN bootstrap whitespace root-directory checkpoint

Contract commit:
`b35339be52737211d29516a66db57a43b359e7f5`

Production guard commit:
`3f5fcceafac6c7662822c49c3810d4dadd1a77c6`

Checkpoint commit:
`7eaf2eda992ce59632dc086319e2a8c246858b8c`

Locally verified:

```text
WhenItFails.Tests
Failed:   0
Passed: 1043
Skipped:  0
Total:  1043
Compiler warnings: 0
```

`JsonsBootstrapper.EnsureWorkspaceAsync(...)` rejects null/whitespace `RootDirectory` and `PackageDirectoryName` before filesystem mutation, aligning the directory-option shape with `ErrorCatalogContextProvider` for the current scope.

## Remaining `JsonsOptions` filename boundary

The remaining configurable catalog file names are:

- `ErrorCatalogFileName`
- `CategoryCatalogFileName`
- `CodeGroupCatalogFileName`
- `OwnerCatalogFileName`
- `ProfilesFileName`

`ErrorCatalogContextProvider` already defines distinct stable null/empty contracts for each. `JsonsBootstrapper` currently validates only template output after workspace creation and provider invocation, so this audit proceeds one focused caller-configuration contract at a time.

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

## Recommended verification

Pull current `master` and run only the focused null-error-catalog-file-name contract:

```powershell
dotnet test WhenItFails.Tests --filter "FullyQualifiedName~EnsureWorkspaceAsync_WhenErrorCatalogFileNameIsNull_ReturnsInvalidBeforeProviderOrFilesystem"
```

Expected current result: **RED** with `Actual: Success` rather than the expected `Invalid`.

## Next recommended step

If RED confirms the mismatch, add the smallest pre-provider/pre-filesystem `ErrorCatalogFileName is null` guard using exactly `WIF_JSONS_ERROR_CATALOG_FILE_NAME_NULL`. Then run focused and complete suites before testing the adjacent whitespace form.