# Implementation status

Last updated: 2026-09-20

This file is the continuation point for `WhenItFails` development. Git history contains the detailed chronological checkpoints; keep this file focused on the current verified state, established contracts, and next step.

## Current focus

Hardening dependency boundaries and malformed-context/configuration handling while preserving established public exception contracts.

## Current verified state

- Complete `WhenItFails.Tests` suite: **1047/1047 GREEN**, confirmed locally by the maintainer after the whitespace `CategoryCatalogFileName` production guard. The compiler-warning count was not separately reported for that full-suite checkpoint.
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
- `ErrorCatalogFileName = null` is locally verified GREEN and is rejected before template-provider invocation or filesystem mutation.
- `ErrorCatalogFileName` null/whitespace contracts are locally verified GREEN and reject malformed caller configuration before provider invocation or filesystem mutation.
- `CategoryCatalogFileName = null` is locally verified GREEN as part of the 1046-test suite.
- `CategoryCatalogFileName` null/whitespace contracts are locally verified GREEN and reject malformed caller configuration before provider invocation or filesystem mutation.

## 2026-09-20 — bootstrap null code-group-catalog-file-name contract

Contract commit:
`01f9d68fd9fb975d59f6f1f0032408f0e6bf6b4e`

Baseline: **1047/1047 GREEN**, confirmed locally before this test was introduced.

Added `WhenItFails.Tests/Bootstrap/JsonsBootstrapperNullCodeGroupCatalogFileNameContractTests.cs` with contract
`EnsureWorkspaceAsync_WhenCodeGroupCatalogFileNameIsNull_ReturnsInvalidBeforeProviderOrFilesystem`.

Expected stable response:

```text
Status: Invalid
Data: null
Code: WIF_JSONS_CODE_GROUP_CATALOG_FILE_NAME_NULL
Message: The code group catalog file name cannot be null.
```

The contract additionally requires that the template provider is not invoked and the workspace root is not created.

The current `JsonsBootstrapper` implementation does not yet guard `CodeGroupCatalogFileName = null`. Focused RED verification is pending; no production change has been made for this case.

## 2026-09-20 — 1047/1047 GREEN bootstrap whitespace category-catalog checkpoint

Contract commit:
`9cac09bc34ec17ea4b5b969e3c08528e64651f1d`

Production guard commit:
`a7d60a23a485433361e92af14745c0b6fdb8c19d`

Locally confirmed by the maintainer:

```text
WhenItFails.Tests
Failed:   0
Passed: 1047
Total:  1047
```

The compiler-warning count was not reported separately. Both null and whitespace `CategoryCatalogFileName` inputs are rejected before workspace creation and template-provider invocation. Next contract: null `CodeGroupCatalogFileName`.

## 2026-09-20 — bootstrap whitespace category-catalog-file-name contract

Contract commit:
`9cac09bc34ec17ea4b5b969e3c08528e64651f1d`

Production guard commit:
`a7d60a23a485433361e92af14745c0b6fdb8c19d`

Previous locally confirmed checkpoint: **1046/1046 GREEN**.

Added contract:
`EnsureWorkspaceAsync_WhenCategoryCatalogFileNameIsWhitespace_ReturnsInvalidBeforeProviderOrFilesystem`

The focused run confirmed RED before the production fix:

```text
Expected: Invalid
Actual:   Success
```

The narrow guard rejects whitespace `CategoryCatalogFileName` after the existing null check and before filesystem mutation or template-provider invocation. Expected stable response:

```text
Status: Invalid
Data: null
Code: WIF_JSONS_CATEGORY_CATALOG_FILE_NAME_EMPTY
Message: The category catalog file name cannot be empty.
```

The contract also verifies that the template provider is not called and the workspace root is not created.

**Focused GREEN and complete-suite 1047/1047 GREEN are pending maintainer verification.**

## 2026-09-17 — bootstrap null error-catalog-file-name fix

Contract commit:
`91bba6f1c1cc11c2f7e6b12c5814a2afbb0ef64d`

Production guard commit:
`e3502565d5139132ce16947e7b0fbcb63e235e29`

Baseline checkpoint commit:
`7eaf2eda992ce59632dc086319e2a8c246858b8c`

Contract:

`EnsureWorkspaceAsync_WhenErrorCatalogFileNameIsNull_ReturnsInvalidBeforeProviderOrFilesystem`

`ErrorCatalogContextProvider.ValidateJsonsOptions(...)` defines the stable option-level contract:

```text
Status: Invalid
Data: null
Code: WIF_JSONS_ERROR_CATALOG_FILE_NAME_NULL
Message: The error catalog file name cannot be null.
```

The focused run confirmed the expected RED:

```text
Expected: Invalid
Actual:   Success
```

Before the fix, `JsonsBootstrapper` did not validate `ErrorCatalogFileName` before preparing the workspace. With the focused tracking provider, the malformed option therefore allowed workspace creation, provider invocation and a final `Success` response.

The production change is intentionally narrow: `options.ErrorCatalogFileName is null` is rejected after the already-established root/package option guards and before entering the filesystem block or invoking `IJsonsTemplateProvider.GetTemplateFiles(...)`.

The focused contract also protects both ordering guarantees:

```text
template provider invoked: false
workspace root created: false
```

Expected complete-suite result after verification: **1044/1044 GREEN, zero compiler warnings**.

## 2026-09-19 — bootstrap whitespace error-catalog-file-name contract

Contract commit:
`e58599d05993dac8af142a27325fa283b9a37e89`

Baseline checkpoint commit:
`b90d3c4563acae874f50c5b7aafad66ec216ade0`

Added:

`WhenItFails.Tests/Bootstrap/JsonsBootstrapperWhitespaceErrorCatalogFileNameContractTests.cs`

Contract:

`EnsureWorkspaceAsync_WhenErrorCatalogFileNameIsWhitespace_ReturnsInvalidBeforeProviderOrFilesystem`

Stable option-level response:

```text
Status: Invalid
Data: null
Code: WIF_JSONS_ERROR_CATALOG_FILE_NAME_EMPTY
Message: The error catalog file name cannot be empty.
```

The focused contract also requires:

```text
template provider invoked: false
workspace root created: false
```

Current production validates only the null form. A whitespace value is therefore expected to pass into the filesystem/provider path and, with the tracking provider returning an empty collection, end as `Success`.

The focused run confirmed the expected RED:

```text
Expected: Invalid
Actual:   Success
```

Before the fix, whitespace `ErrorCatalogFileName` passed through the option boundary, allowing workspace creation and template-provider invocation before returning `Success` with the tracking provider.

Production guard commit:
`6d60d321ea2e746e41b3ec0fe6dcb4c821727ab1`

The production change is intentionally narrow: whitespace `ErrorCatalogFileName` is rejected immediately after the existing null guard and before entering the filesystem block or invoking the template provider.

Expected complete-suite result after verification: **1045/1045 GREEN**.

## 2026-09-19 — bootstrap null category-catalog-file-name contract

Contract commit:
`0e55002be070cfbd237b3661b2464e3138c1521f`

Baseline checkpoint commit:
`e1e6a178d0f20e6e0be36c965f6eda6de23878ca`

Added:

`WhenItFails.Tests/Bootstrap/JsonsBootstrapperNullCategoryCatalogFileNameContractTests.cs`

Contract:

`EnsureWorkspaceAsync_WhenCategoryCatalogFileNameIsNull_ReturnsInvalidBeforeProviderOrFilesystem`

Stable option-level response:

```text
Status: Invalid
Data: null
Code: WIF_JSONS_CATEGORY_CATALOG_FILE_NAME_NULL
Message: The category catalog file name cannot be null.
```

The focused contract also requires:

```text
template provider invoked: false
workspace root created: false
```

Current production does not validate `CategoryCatalogFileName` before entering the filesystem/provider path. With the tracking provider returning an empty collection, current behavior is expected to create the workspace, invoke the provider and return `Success`.

The focused run confirmed the expected RED:

```text
Expected: Invalid
Actual:   Success
```

Before the fix, `CategoryCatalogFileName = null` passed through the option boundary, allowing workspace creation and template-provider invocation before returning `Success` with the tracking provider.

Production guard commit:
`75b2579c499002f1b073b7abf9cbf2f377cec196`

The production change is intentionally narrow: null `CategoryCatalogFileName` is rejected immediately after the established `ErrorCatalogFileName` guards and before entering the filesystem block or invoking the template provider.

Expected complete-suite result after verification: **1046/1046 GREEN**.

## 2026-09-19 — 1045/1045 GREEN bootstrap whitespace error-catalog-file-name checkpoint

Contract commit:
`e58599d05993dac8af142a27325fa283b9a37e89`

Production guard commit:
`6d60d321ea2e746e41b3ec0fe6dcb4c821727ab1`

Previous checkpoint commit:
`b90d3c4563acae874f50c5b7aafad66ec216ade0`

Locally verified:

```text
WhenItFails.Tests
Failed:   0
Passed: 1045
Skipped:  0
Total:  1045
```

The warning count was not separately included in the latest confirmation, so this checkpoint records the test result only.

`JsonsBootstrapper.EnsureWorkspaceAsync(...)` now rejects whitespace `ErrorCatalogFileName` before any filesystem mutation or template-provider invocation and returns:

```text
Status: Invalid
Data: null
Code: WIF_JSONS_ERROR_CATALOG_FILE_NAME_EMPTY
Message: The error catalog file name cannot be empty.
```

Together with the prior null contract, `ErrorCatalogFileName` option handling is complete for the current null/empty scope.

## 2026-09-19 — 1044/1044 GREEN bootstrap null error-catalog-file-name checkpoint

Contract commit:
`91bba6f1c1cc11c2f7e6b12c5814a2afbb0ef64d`

Production guard commit:
`e3502565d5139132ce16947e7b0fbcb63e235e29`

Previous checkpoint commit:
`7eaf2eda992ce59632dc086319e2a8c246858b8c`

Locally verified:

```text
WhenItFails.Tests
Failed:   0
Passed: 1044
Skipped:  0
Total:  1044
```

The warning count was not separately included in the latest confirmation, so this checkpoint records the test result only.

`JsonsBootstrapper.EnsureWorkspaceAsync(...)` now rejects `ErrorCatalogFileName = null` before any filesystem mutation or template-provider invocation and returns:

```text
Status: Invalid
Data: null
Code: WIF_JSONS_ERROR_CATALOG_FILE_NAME_NULL
Message: The error catalog file name cannot be null.
```

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

The configurable catalog file names are:

- `ErrorCatalogFileName`
- `CategoryCatalogFileName`
- `CodeGroupCatalogFileName`
- `OwnerCatalogFileName`
- `ProfilesFileName`

`ErrorCatalogContextProvider` already defines distinct stable null/empty contracts for each. `JsonsBootstrapper` is being aligned one focused caller-configuration contract at a time, always before provider invocation and filesystem mutation.

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

Pull current `master` and run:

```powershell
dotnet test WhenItFails.Tests --filter "FullyQualifiedName~EnsureWorkspaceAsync_WhenCodeGroupCatalogFileNameIsNull_ReturnsInvalidBeforeProviderOrFilesystem"
```

Expected result at this stage: **one focused RED**, with `Expected: Invalid` and `Actual: Success`. Confirm this before adding the production guard.

## Next recommended step

After the focused RED is confirmed, add the smallest pre-provider/pre-filesystem null `CodeGroupCatalogFileName` guard, then run the focused test and complete suite (expected total: 1048 tests).