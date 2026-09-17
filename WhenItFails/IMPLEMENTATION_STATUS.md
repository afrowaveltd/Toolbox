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
- The focused whitespace `RootDirectory` RED is locally confirmed and the smallest pre-filesystem production guard is committed; focused/full GREEN verification is pending.

## 2026-09-17 — bootstrap whitespace root-directory fix

Contract commit:
`b35339be52737211d29516a66db57a43b359e7f5`

Production guard commit:
`3f5fcceafac6c7662822c49c3810d4dadd1a77c6`

Baseline checkpoint commit:
`a203fdf8f601b51a9448241f10a838f21af7c299`

Contract:

`EnsureWorkspaceAsync_WhenRootDirectoryIsWhitespace_ReturnsInvalidWithoutCreatingWorkspace`

`ErrorCatalogContextProvider.ValidateJsonsOptions(...)` already defines the stable contract:

```text
Status: Invalid
Data: null
Code: WIF_JSONS_ROOT_DIRECTORY_EMPTY
Message: The JSON root directory cannot be empty.
```

The focused run confirmed the expected RED:

```text
Expected: Invalid
Actual:   Success
```

Before the fix, whitespace `RootDirectory` was normalized to an empty string. With the isolated absolute temporary `PackageDirectoryName` used by the test, bootstrap could therefore create/use that temporary directory and return `Success`.

Production change in `WhenItFails/Bootstrap/JsonsBootstrapper.cs` is intentionally narrow: a whitespace `RootDirectory` is now rejected immediately after the existing null-root guard and before package-directory validation or any filesystem operation.

Expected complete-suite result after verification: **1043/1043 GREEN, zero compiler warnings**.

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

Pull current `master` and run:

```powershell
dotnet test WhenItFails.Tests --filter "FullyQualifiedName~EnsureWorkspaceAsync_WhenRootDirectoryIsWhitespace_ReturnsInvalidWithoutCreatingWorkspace"
dotnet test WhenItFails.Tests
```

Expected results:

```text
Focused contract: GREEN
Complete suite: 1043/1043 GREEN
Compiler warnings: 0
```

Ignore `NETSDK1057` when counting compiler warnings; it is an SDK informational support-policy message.

## Next recommended step

After **1043/1043 GREEN, zero compiler warnings** is confirmed, record the checkpoint and continue the bootstrap `JsonsOptions` audit from the remaining filename/path contracts rather than adding broader validation without focused evidence.
