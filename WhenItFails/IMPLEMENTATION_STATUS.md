# Implementation status

Last updated: 2026-09-15

This file is the continuation point for `WhenItFails` development. Update it after every implementation, test, catalog, or documentation change that alters the current state or recommended next step.

## Current focus

Hardening dependency boundaries and failure cleanup while preserving established public exception contracts.

## Current state

- `ErrorDescriptorResolver` and `ErrorDescriptorService` hardening are complete for the current scope.
- `ErrorCatalogProvider` and `CatalogProviderPipeline` dependency-boundary audits are complete for the current scope.
- `ErrorCatalogContextProvider` is intentionally transparent for exceptions and null tasks from its internal providers; do not normalize those lower-layer contracts.
- `BuiltInErrorCatalogContextProvider` dependency-boundary audit is complete for the current scope, including injected context-provider null `Task` normalization.
- `ErrorCatalogInitializer` bootstrapper/context-provider ordinary-exception, cancellation, null-response and null-task behavior is complete for the current scope.
- `ErrorCatalogRuntime` initializer ordinary-exception, cancellation, null-response and null-task behavior is complete for the current scope.
- Both runtime `IBuiltInErrorCatalogContextProvider.LoadAsync(...)` boundaries — explicit reset and flexible fallback — are complete for null response, ordinary exception, null task and exact cancellation behavior in the current scope.
- Direct `JsonsBootstrapper` → `IJsonsTemplateProvider.GetTemplateFiles(...)` boundary is complete for malformed results, ordinary exception normalization and exact-instance cancellation propagation.
- The complete `WhenItFails.Tests` suite is locally verified **GREEN at 1022/1022 tests with zero compiler warnings** before the new writer cleanup contract.
- The focused `JsonCatalogDocumentWriter` temporary-file cleanup contract produced the expected RED: the public response remained correct, but a generated `.tmp` file was left behind.
- Production now performs best-effort cleanup of the generated temporary file in `finally`; local GREEN verification is pending.

## 2026-09-15 — writer temporary-file cleanup fix

Contract commit: `9a638c467dd435dcceb43f1c4e16887b71a8e828`.
Production fix commit: `424a6ea94df0a871b5c50fcaa8193142828fdfbc`.
Baseline checkpoint commit: `e6008acd13a1b31e18847208d7ebd61d129f60de`.

Focused RED was locally confirmed:

```text
JsonCatalogDocumentWriterTemporaryFileCleanupContractTests
Failed: 1
Passed: 0

Assert.Empty() Failure: Collection was not empty
```

The established public response already remained `Invalid` / `JsonSerializationFailed`; the failure was exclusively the generated temporary file remaining in the target directory.

Production change in `WhenItFails/Loading/JsonCatalogDocumentWriter.cs` is intentionally narrow:

- the generated temporary path is retained outside the `try` block;
- `finally` calls best-effort cleanup for that exact path;
- cleanup failures are suppressed so they can never replace the original operation result or exception;
- successful writes are unaffected because `File.Move(...)` removes the temporary path before `finally` runs;
- cancellation and existing response codes/messages are unchanged.

Expected complete-suite count after verification: **1023/1023 GREEN with zero compiler warnings**.

## 2026-09-15 — writer temporary-file cleanup contract

Contract commit: `9a638c467dd435dcceb43f1c4e16887b71a8e828`.
Baseline checkpoint commit: `e6008acd13a1b31e18847208d7ebd61d129f60de`.

Added:

`WhenItFails.Tests/Loading/JsonCatalogDocumentWriterTemporaryFileCleanupContractTests.cs`

Contract:

`SaveToFileAsync_WhenSerializationFails_DoesNotLeaveTemporaryFile`

The test uses a self-referencing document so `System.Text.Json` throws `JsonException` only after the safe-write temporary file has been created.

Required public response remains the established contract:

```text
Status: Invalid
Code: JsonSerializationFailed
Target file: not created
```

Additional safe-write requirement:

```text
No .errors.en.json.<guid>.tmp file remains in the target directory.
```

## 2026-09-15 — 1022/1022 GREEN bootstrap template-provider cancellation checkpoint

Checkpoint commit: `e6008acd13a1b31e18847208d7ebd61d129f60de`.
Cancellation contract commit: `21b03f20dbea7428f9568a5025ad7ebb183a12fe`.
Ordinary-exception production fix commit: `32816f57873266fbccbd90917bd00a86f5818db9`.
Previous checkpoint commit: `4ada29c6bbc2f73509fdf0238cc24a245951051c`.

Verified locally:

```text
WhenItFails.Tests
Failed:   0
Passed: 1022
Skipped:  0
Total:  1022
Compiler warnings: 0
```

The direct `JsonsBootstrapper` template-provider boundary now explicitly covers:

- null template collection → existing `WIF_JSONS_TEMPLATE_COLLECTION_NULL` invalid response;
- null template item → existing `WIF_JSONS_TEMPLATE_ITEM_NULL` invalid response;
- ordinary provider exception → `Failed` / `WIF_JSONS_TEMPLATE_PROVIDER_FAILED` with stable sanitized message;
- exact supplied `OperationCanceledException` instance propagates unchanged.

No production change was required for cancellation because the provider exception filter already excludes `OperationCanceledException`.

## 2026-09-15 — bootstrap template-provider cancellation contract

Contract commit: `21b03f20dbea7428f9568a5025ad7ebb183a12fe`.
Baseline checkpoint commit: `4ada29c6bbc2f73509fdf0238cc24a245951051c`.

Updated:

`WhenItFails.Tests/Bootstrap/JsonsBootstrapperTemplateProviderExceptionContractTests.cs`

Added:

`EnsureWorkspaceAsync_WhenTemplateProviderCancels_RethrowsSameOperationCanceledException`

The fake `IJsonsTemplateProvider.GetTemplateFiles(...)` throws a supplied `OperationCanceledException`. The contract requires the same exception instance to propagate unchanged:

```text
Assert.Same(cancellation, thrown)
```

No production code changed. The current `JsonsBootstrapper` provider catch explicitly excludes `OperationCanceledException`.

## 2026-09-15 — 1021/1021 GREEN bootstrap template-provider checkpoint

Checkpoint commit: `4ada29c6bbc2f73509fdf0238cc24a245951051c`.
Contract commit: `1da0988b5419d6af649c16c820bb92d63ab4dc8a`.
Production fix commit: `32816f57873266fbccbd90917bd00a86f5818db9`.
Previous baseline checkpoint commit: `383014695d776bbbbb45db31de5f050c8a09cde8`.

Verified locally:

```text
WhenItFails.Tests
Failed:   0
Passed: 1021
Skipped:  0
Total:  1021
Compiler warnings: 0
```

The direct `JsonsBootstrapper` template-provider boundary has verified ordinary-exception behavior:

- ordinary provider exception → `Failed` / `WIF_JSONS_TEMPLATE_PROVIDER_FAILED`;
- public message is stable: `The JSON template provider failed.`;
- raw dependency exception detail does not escape;
- malformed null collection/item results remain represented by their existing `WIF_JSONS_TEMPLATE_*` invalid responses.

## 2026-09-10 — bootstrap template-provider ordinary-exception fix

Contract commit: `1da0988b5419d6af649c16c820bb92d63ab4dc8a`.
Production fix commit: `32816f57873266fbccbd90917bd00a86f5818db9`.
Baseline checkpoint commit: `383014695d776bbbbb45db31de5f050c8a09cde8`.

Focused RED was locally confirmed:

```text
JsonsBootstrapperTemplateProviderExceptionContractTests
Failed: 1
Passed: 0

System.InvalidOperationException:
Sensitive JSON template provider detail must not escape.
```

The exception propagated directly from `IJsonsTemplateProvider.GetTemplateFiles(...)`, confirming the missing dependency-boundary normalization.

Production change in `WhenItFails/Bootstrap/JsonsBootstrapper.cs` is intentionally narrow: only the template-provider call is wrapped.

Ordinary provider exceptions now return:

```text
Status: Failed
Data: null
Code: WIF_JSONS_TEMPLATE_PROVIDER_FAILED
Message: The JSON template provider failed.
```

Raw exception detail is not exposed. `OperationCanceledException` is explicitly excluded from normalization and continues to propagate unchanged. Existing filesystem `UnauthorizedAccessException` and `IOException` handling outside the provider call remains unchanged.

## 2026-09-10 — 1020/1020 GREEN flexible-fallback checkpoint

Checkpoint commit: `383014695d776bbbbb45db31de5f050c8a09cde8`.
Flexible-fallback null-task contract commit: `27e37855c4ac6e12a5323810c67e47e2d4ebba0a`.
Previous status commit: `302847070ba712fe89fced815eeeb6a6da128c23`.

Verified locally:

```text
WhenItFails.Tests
Failed:   0
Passed: 1020
Skipped:  0
Total:  1020
Compiler warnings: 0
```

The flexible fallback path through `CreateBuiltInFallbackResponseAsync(...)` explicitly covers its `IBuiltInErrorCatalogContextProvider.LoadAsync(...)` boundary:

- null `Response` → fallback metadata `WIF_BUILT_IN_CONTEXT_RESPONSE_NULL` / `Invalid`;
- ordinary exception → fallback metadata `WIF_BUILT_IN_CONTEXT_PROVIDER_FAILED` / `Failed`;
- null `Task` → fallback metadata `WIF_BUILT_IN_CONTEXT_PROVIDER_FAILED` / `Failed`;
- exact `OperationCanceledException` instance propagates unchanged.

Together with the explicit reset coverage, both runtime uses of the built-in provider are complete for the current dependency-boundary scope.

## 2026-09-10 — 1019/1019 GREEN reset built-in-provider checkpoint

Checkpoint commit: `2bf35596529be9f27795ddc462f055b3a3ea883c`.
Reset null-task contract commit: `533058b97684f411a40a5b7ea86ef68b3cdcfa6e`.

Verified locally:

```text
WhenItFails.Tests
Failed:   0
Passed: 1019
Skipped:  0
Total:  1019
Compiler warnings: 0
```

`ResetToDefaultsAsync(...)` explicitly covers its `IBuiltInErrorCatalogContextProvider.LoadAsync(...)` boundary:

- null `Response` → `WIF_BUILT_IN_CONTEXT_RESPONSE_NULL`;
- ordinary exception → `WIF_BUILT_IN_CONTEXT_PROVIDER_FAILED`;
- null `Task` → `WIF_BUILT_IN_CONTEXT_PROVIDER_FAILED`;
- exact `OperationCanceledException` instance propagates unchanged.

## 2026-09-10 — 1018/1018 GREEN runtime initializer checkpoint

Checkpoint commit: `bcbc9c6696fcf2a058c83c376fa83da2ce36f7bc`.
Runtime initializer null-task contract commit: `a8a9cb118e43eca67f4cea9d3d0291c4baa1e2a0`.

Verified locally:

```text
WhenItFails.Tests
Failed:   0
Passed: 1018
Skipped:  0
Total:  1018
Compiler warnings: 0
```

`ErrorCatalogRuntime.InitializeCoreAsync(...)` explicitly covers its injected `IErrorCatalogInitializer` boundary:

- null `Response` → `WIF_INITIALIZER_RESPONSE_NULL`;
- ordinary exception → `WIF_INITIALIZER_FAILED`;
- null `Task` → `WIF_INITIALIZER_FAILED`;
- exact `OperationCanceledException` instance propagates unchanged.

## 2026-09-10 — 1017/1017 GREEN initializer checkpoint

Checkpoint commit: `c35fd30a260d82d32239ddf04851c6cd25ea5769`.
Initializer context-provider null-task contract commit: `40404b8e07fef0bb24e2f3fb5251cda03c99cd8b`.
Initializer bootstrapper null-task contract commit: `84f3fe36469882aedfed03507d857b267f89d1c1`.

Verified locally:

```text
WhenItFails.Tests
Failed:   0
Passed: 1017
Skipped:  0
Total:  1017
Compiler warnings: 0
```

`ErrorCatalogInitializer` explicitly covers both async dependency boundaries:

### `IJsonsBootstrapper.EnsureWorkspaceAsync(...)`

- null `Response` → `WIF_INITIALIZER_BOOTSTRAPPER_RESPONSE_NULL`;
- ordinary exception → `WIF_INITIALIZER_BOOTSTRAPPER_FAILED`;
- null `Task` → `WIF_INITIALIZER_BOOTSTRAPPER_FAILED`;
- exact `OperationCanceledException` instance propagates unchanged;
- downstream context provider does not run after bootstrapper failure;
- previously stored context is preserved.

### `IErrorCatalogContextProvider.LoadFromJsonsAsync(...)`

- null `Response` → `WIF_INITIALIZER_CONTEXT_PROVIDER_RESPONSE_NULL`;
- ordinary exception → `WIF_INITIALIZER_CONTEXT_PROVIDER_FAILED`;
- null `Task` → `WIF_INITIALIZER_CONTEXT_PROVIDER_FAILED`;
- exact `OperationCanceledException` instance propagates unchanged;
- context store is not replaced after failure;
- previously stored context is preserved.

## Established transparent lower boundary — do not normalize

`ErrorCatalogContextProvider.LoadFromJsonsAsync(...)` intentionally preserves exceptions and null-task behavior from its five internal catalog providers.

Relevant suites include:

- `WhenItFails.Tests/Catalog/ErrorCatalogContextProviderProviderExceptionPropagationTests.cs`
- `WhenItFails.Tests/Catalog/ErrorCatalogContextProviderExceptionShapeTests.cs`
- `WhenItFails.Tests/Catalog/ErrorCatalogContextProviderCancellationPropagationTests.cs`
- `WhenItFails.Tests/Catalog/ErrorCatalogContextProviderOwnerNullTaskTests.cs`
- `WhenItFails.Tests/Catalog/ErrorCatalogContextProviderProfileNullTaskTests.cs`

Do not replace those transparent contracts with normalization at that layer.

## Reconnaissance notes

`JsonCatalogDocumentLoader.InvalidJson` deliberately includes the parser message; `Docs/Loading-and-Normalization/en.md` documents that behavior. Do not sanitize it as an incidental hardening change.

Fresh reconnaissance after the 1022 checkpoint:

- `JsonCatalogDocumentWriter` invalid-file-path behavior is already explicitly covered by `JsonCatalogDocumentWriterInvalidFilePathContractTests`; do not duplicate that contract.
- `JsonCatalogDocumentWriter` describes a conservative safe-write workflow using a generated temporary file.
- The focused cleanup contract confirmed that serialization failure left the generated temporary file behind before the production fix.

## Verification state

- Clean continuation baseline: **1022/1022 GREEN, zero compiler warnings**.
- Writer cleanup contract commit: `9a638c467dd435dcceb43f1c4e16887b71a8e828`.
- Focused RED locally confirmed: correct `JsonSerializationFailed` response, stale `.tmp` file remained.
- Production cleanup fix commit: `424a6ea94df0a871b5c50fcaa8193142828fdfbc`.
- Focused and complete-suite GREEN verification are pending.
- Expected complete-suite count: **1023/1023 GREEN with zero compiler warnings**.

## Recommended verification

Pull current `master` and run:

```powershell
dotnet test WhenItFails.Tests --filter "FullyQualifiedName~SaveToFileAsync_WhenSerializationFails_DoesNotLeaveTemporaryFile"
dotnet test WhenItFails.Tests
```

Expected results:

```text
Focused contract: GREEN
Complete suite: 1023/1023 GREEN
Compiler warnings: 0
```

## Next recommended step

After **1023/1023 GREEN** is confirmed, record the checkpoint and perform fresh reconnaissance for the next smallest uncovered failure-cleanup or dependency boundary. Do not broaden the writer change until the full suite verifies the new `finally` cleanup behavior.

Keep changes small, tested, documented here, and committed directly to `master`.
