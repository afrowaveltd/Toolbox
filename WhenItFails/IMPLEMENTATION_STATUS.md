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
- `JsonCatalogDocumentWriter` performs verified best-effort cleanup of its generated temporary file when serialization fails and honors pre-cancellation before filesystem side effects.
- `ErrorProfileSelectionService` ordinary resolver-exception normalization is locally verified.
- The complete `WhenItFails.Tests` suite is locally verified **GREEN at 1025/1025 tests with zero compiler warnings**.

## 2026-09-15 — 1025/1025 GREEN profile resolver exception checkpoint

Checkpoint commit: this commit.
Contract commit: `3a3a81a7c816fd72d63c5f1450d07c675bb12288`.
Production fix commit: `7d739c3d9499951304da0d3463308668b7b67e19`.
Previous checkpoint commit: `46b0953f2b2e479be39a2a1098c668dcfd7d0575`.

Verified locally:

```text
WhenItFails.Tests
Failed:   0
Passed: 1025
Skipped:  0
Total:  1025
Compiler warnings: 0
```

`ErrorProfileSelectionService.ResolveByProfileName(...)` now explicitly normalizes ordinary exceptions from its injected `IErrorProfileResolver.Resolve(...)` boundary:

- ordinary resolver exception → `Failed` / `WIF_PROFILE_RESOLVER_FAILED`;
- message is stable: `The error profile resolver failed.`;
- raw dependency exception detail does not escape;
- existing null-result behavior remains `Invalid` / `WIF_PROFILE_RESOLVER_RESULT_NULL`;
- `OperationCanceledException` is excluded from normalization and still needs an explicit exact-instance contract.

## 2026-09-15 — profile selection resolver ordinary-exception fix

Contract commit: `3a3a81a7c816fd72d63c5f1450d07c675bb12288`.
Production fix commit: `7d739c3d9499951304da0d3463308668b7b67e19`.
Baseline checkpoint commit: `46b0953f2b2e479be39a2a1098c668dcfd7d0575`.

Focused RED was locally confirmed:

```text
ErrorProfileSelectionServiceResolverExceptionContractTests
Failed: 1
Passed: 0

System.InvalidOperationException:
Sensitive profile resolver detail must not escape.
```

The exception propagated directly from `IErrorProfileResolver.Resolve(...)`, confirming the missing normalization boundary.

Production change in `WhenItFails/Resolution/ErrorProfileSelectionService.cs` is intentionally narrow: only the injected resolver call is wrapped.

Ordinary resolver exceptions now return:

```text
Status: Failed
Data: null
Code: WIF_PROFILE_RESOLVER_FAILED
Message: The error profile resolver failed.
```

Raw dependency exception detail is not exposed. `OperationCanceledException` is explicitly excluded from normalization and continues to propagate unchanged. Existing malformed-input and `WIF_PROFILE_RESOLVER_RESULT_NULL` behavior remains unchanged.

## 2026-09-15 — profile selection resolver ordinary-exception contract

Contract commit: `3a3a81a7c816fd72d63c5f1450d07c675bb12288`.
Baseline checkpoint commit: `46b0953f2b2e479be39a2a1098c668dcfd7d0575`.

Added:

`WhenItFails.Tests/Resolution/ErrorProfileSelectionServiceResolverExceptionContractTests.cs`

Contract:

`ResolveByProfileName_WhenResolverThrows_ReturnsStableFailureWithoutExceptionDetail`

`ErrorProfileSelectionService` is already a normalizing public boundary: it validates malformed context/profile inputs and converts a null resolver result to `WIF_PROFILE_RESOLVER_RESULT_NULL`. The injected resolver ordinary-exception path therefore also returns a stable response rather than leaking dependency details.

Expected contract:

```text
Status: Failed
Data: null
Code: WIF_PROFILE_RESOLVER_FAILED
Message: The error profile resolver failed.
Raw dependency exception detail: absent
```

## 2026-09-15 — 1024/1024 GREEN writer pre-cancellation checkpoint

Checkpoint commit: `46b0953f2b2e479be39a2a1098c668dcfd7d0575`.
Cancellation contract commit: `2d861fdcf52b9bafb2e146bcf558bc55b2cae798`.
Previous checkpoint commit: `7dc4bdebbfc363c9075dde53eda80712f0509bb4`.

Verified locally:

```text
WhenItFails.Tests
Failed:   0
Passed: 1024
Skipped:  0
Total:  1024
Compiler warnings: 0
```

`JsonCatalogDocumentWriter.SaveToFileAsync(...)` has an explicit deterministic cancellation contract: a token cancelled before entry propagates `OperationCanceledException` before validation or filesystem work, leaving both the target directory and target file absent.

No production change was required. The writer area is closed for the current hardening scope; avoid timing-sensitive mid-write cancellation tests unless a concrete defect or requirement appears.

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

No production change was required because `SaveToFileAsync(...)` calls `cancellationToken.ThrowIfCancellationRequested()` before validation or filesystem operations.

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

Fresh reconnaissance after the 1024 checkpoint moved out of writer/loader/store areas. `ErrorProfileSelectionService` was selected because it already normalizes malformed inputs and a null result from its injected `IErrorProfileResolver`; ordinary exceptions are now normalized as well.

## Verification state

- Clean continuation baseline: **1025/1025 GREEN, zero compiler warnings**.
- Writer hardening is complete for the current scope.
- Profile resolver ordinary-exception boundary is locally verified.
- Next focused contract: exact supplied `OperationCanceledException` from `IErrorProfileResolver.Resolve(...)` must propagate unchanged.
- Expected suite count after adding that contract: **1026 tests**.

## Recommended verification

No pending verification at this checkpoint.

## Next recommended step

Add one exact-instance cancellation contract for `IErrorProfileResolver.Resolve(...)`. No production change is expected because the exception filter explicitly excludes `OperationCanceledException`. Run focused first, then the complete suite. If GREEN, record **1026/1026 GREEN** and resume fresh reconnaissance outside already hardened areas.

Keep changes small, tested, documented here, and committed directly to `master`.
