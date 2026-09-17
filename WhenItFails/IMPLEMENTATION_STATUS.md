# Implementation status

Last updated: 2026-09-17

This file is the continuation point for `WhenItFails` development. Git history contains the detailed chronological checkpoints; keep this file focused on the current verified state, established contracts, and next step.

## Current focus

Hardening dependency boundaries and malformed-context/configuration handling while preserving established public exception contracts.

## Current verified state

- Complete `WhenItFails.Tests` suite baseline: **1042/1042 GREEN, zero compiler warnings**.
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
- `JsonsBootstrapper` rejects null/whitespace `PackageDirectoryName` and null `RootDirectory` before filesystem mutation.
- `PackageDirectoryName` nullable-flow cleanup is locally verified with zero compiler warnings.
- A focused contract for whitespace `RootDirectory` is committed and awaits local RED verification; production is intentionally unchanged for this case.

## 2026-09-17 — bootstrap whitespace root-directory contract

Contract commit:
`b35339be52737211d29516a66db57a43b359e7f5`

Baseline checkpoint commit:
`a203fdf8f601b51a9448241f10a838f21af7c299`

Added:

`WhenItFails.Tests/Bootstrap/JsonsBootstrapperWhitespaceRootDirectoryContractTests.cs`

Contract:

`EnsureWorkspaceAsync_WhenRootDirectoryIsWhitespace_ReturnsInvalidWithoutCreatingWorkspace`

`ErrorCatalogContextProvider.ValidateJsonsOptions(...)` already defines the stable contract:

```text
Status: Invalid
Data: null
Code: WIF_JSONS_ROOT_DIRECTORY_EMPTY
Message: The JSON root directory cannot be empty.
```

The fixture uses an isolated absolute temporary path as `PackageDirectoryName`. With current production behavior, whitespace `RootDirectory` is normalized to an empty string and `Path.Combine("", absolutePackagePath)` resolves to that isolated temporary path. The focused test can therefore observe any filesystem mutation safely outside the repository.

The contract requires the absolute temporary package path to remain absent.

Production is intentionally unchanged before the focused run. With an empty template provider, current behavior is expected to create/use the isolated temporary package directory and return `Success`.

Expected current focused result:

```text
Expected: Invalid
Actual:   Success
```

Expected eventual complete-suite count after this contract passes: **1043/1043 GREEN, zero compiler warnings**.

## 2026-09-17 — 1042/1042 GREEN clean bootstrap null root-directory checkpoint

Root-directory contract commit:
`157118d9ec08a879da8fc364c03b3e4f3b260ec6`

Root-directory production guard commit:
`f30cd7cea9ff3e9f1a5e7b4ac2ff29b26fe3fe5d`

Nullable-flow cleanup commit:
`8f27377ff6a156eee0f3930fd6ec7ec715ac4ef7`

Checkpoint commit:
`a203fdf8f601b51a9448241f10a838f21af7c299`

Locally verified:

```text
WhenItFails.Tests
Failed:   0
Passed: 1042
Skipped:  0
Total:  1042
Compiler warnings: 0
```

`NETSDK1057` may still appear and remains informational rather than a Toolbox compiler warning.

The root-null contract returns:

```text
Status: Invalid
Data: null
Code: WIF_JSONS_ROOT_DIRECTORY_NULL
Message: The JSON root directory cannot be null.
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

## Established documented behavior — preserve

`JsonCatalogDocumentLoader.InvalidJson` deliberately includes the JSON parser message. `WhenItFails/Docs/Loading-and-Normalization/en.md` documents this behavior; do not sanitize it as incidental hardening.

## Recent verified checkpoints

- 1040/1040 — bootstrap whitespace package directory name rejected before filesystem mutation.
- 1041/1041 — bootstrap null package directory name rejected before filesystem mutation.
- 1042/1042 — bootstrap null root directory rejected before filesystem mutation; nullable-flow cleanup verified with zero compiler warnings.

## Recommended verification

Pull current `master` and run only the focused whitespace-root contract:

```powershell
dotnet test WhenItFails.Tests --filter "FullyQualifiedName~EnsureWorkspaceAsync_WhenRootDirectoryIsWhitespace_ReturnsInvalidWithoutCreatingWorkspace"
```

Expected current result: **RED** with `Actual: Success` rather than the expected `Invalid`.

## Next recommended step

If RED confirms the response-shape mismatch, add the smallest `JsonsBootstrapper` precondition for whitespace `RootDirectory`, reusing exactly:

```text
WIF_JSONS_ROOT_DIRECTORY_EMPTY
The JSON root directory cannot be empty.
```

The guard must execute before any filesystem operation. Then run focused and complete suites before expanding the `JsonsOptions` bootstrap audit further.
