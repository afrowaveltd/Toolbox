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
- `JsonCatalogDocumentWriter` performs verified best-effort cleanup of its generated temporary file when serialization fails.
- The complete `WhenItFails.Tests` suite is locally verified **GREEN at 1023/1023 tests with zero compiler warnings** before the new writer cancellation contract.
- A focused pre-cancellation side-effect contract for `JsonCatalogDocumentWriter.SaveToFileAsync(...)` is committed and awaits local verification.

## 2026-09-15 — writer pre-cancellation side-effect contract

Contract commit: `2d861fdcf52b9bafb2e146bcf558bc55b2cae798`.
Baseline checkpoint commit: `7dc4bdebbfc363c9075dde53eda80712f0509bb4`.

Added:

`WhenItFails.Tests/Loading/JsonCatalogDocumentWriterCancellationContractTests.cs`

Contract:

`SaveToFileAsync_WhenTokenAlreadyCancelled_ThrowsWithoutFilesystemSideEffects`

A pre-cancelled token must propagate cancellation before any filesystem work begins. The contract therefore requires:

```text
OperationCanceledException is thrown
Target directory does not exist
Target file does not exist
```

No production change is expected because `SaveToFileAsync(...)` calls `cancellationToken.ThrowIfCancellationRequested()` before validation or filesystem operations.

Expected complete-suite count after verification: **1024/1024 GREEN with zero compiler warnings**.

## 2026-09-15 — 1023/1023 GREEN writer temporary-file cleanup checkpoint

Checkpoint commit: `7dc4bdebbfc363c9075dde53eda80712f0509bb4`.
Cleanup contract commit: `9a638c467dd435dcceb43f1c4e16887b71a8e828`.
Production cleanup fix commit: `424a6ea94df0a871b5c50fcaa8193142828fdfbc`.
Previous checkpoint commit: `e6008acd13a1b31e18847208d7ebd61d129f60de`.

Verified locally:

```text
WhenItFails.Tests
Failed:   0
Passed: 1023
Skipped:  0
Total:  1023
Compiler warnings: 0
```

The focused writer contract verifies that a `JsonException` raised after the generated temporary file is created preserves the established `Invalid` / `JsonSerializationFailed` public response and leaves no generated `.<target>.<guid>.tmp` file behind.

The production cleanup is best-effort and runs from `finally`, so cleanup failures cannot replace the original response or exception. Successful writes remain unchanged because the temporary path no longer exists after `File.Move(...)` succeeds.

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

The direct `JsonsBootstrapper` template-provider boundary explicitly covers:

- null template collection → existing `WIF_JSONS_TEMPLATE_COLLECTION_NULL` invalid response;
- null template item → existing `WIF_JSONS_TEMPLATE_ITEM_NULL` invalid response;
- ordinary provider exception → `Failed` / `WIF_JSONS_TEMPLATE_PROVIDER_FAILED` with stable sanitized message;
- exact supplied `OperationCanceledException` instance propagates unchanged.

No production change was required for cancellation because the provider exception filter already excludes `OperationCanceledException`.

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

Fresh reconnaissance after the 1023 checkpoint found no existing writer cancellation contract. The new pre-cancellation contract intentionally avoids timing-sensitive mid-write cancellation and is cross-platform: it verifies that cancellation is honored before any directory or temporary-file side effect.

## Verification state

- Clean continuation baseline: **1023/1023 GREEN, zero compiler warnings**.
- Writer serialization-failure cleanup contract and production fix are locally verified.
- Writer pre-cancellation contract commit: `2d861fdcf52b9bafb2e146bcf558bc55b2cae798`.
- No production change is expected.
- Expected complete-suite count after verification: **1024/1024 GREEN with zero compiler warnings**.

## Recommended verification

Pull current `master` and run:

```powershell
dotnet test WhenItFails.Tests --filter "FullyQualifiedName~SaveToFileAsync_WhenTokenAlreadyCancelled_ThrowsWithoutFilesystemSideEffects"
dotnet test WhenItFails.Tests
```

Expected results:

```text
Focused contract: GREEN
Complete suite: 1024/1024 GREEN
Compiler warnings: 0
```

## Next recommended step

After **1024/1024 GREEN** is confirmed, record the checkpoint and move out of the writer unless fresh evidence reveals another deterministic cross-platform gap. Continue reconnaissance for the next smallest uncovered core failure boundary rather than adding timing-sensitive cancellation tests.

Keep changes small, tested, documented here, and committed directly to `master`.
