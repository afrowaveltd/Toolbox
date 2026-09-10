# Implementation status

Last updated: 2026-09-10

This file is the continuation point for `WhenItFails` development. Update it after every implementation, test, catalog, or documentation change that alters the current state or recommended next step.

## Current focus

Hardening dependency boundaries while preserving established public exception contracts, including malformed async dependency results such as null `Task` values.

## Current state

- `ErrorDescriptorResolver` and `ErrorDescriptorService` hardening are complete for the current scope.
- `ErrorCatalogRuntime` dependency boundaries covered so far are complete for the current scope.
- `ErrorCatalogInitializer` bootstrapper/context-provider/context-store boundaries covered so far are complete for the current scope.
- `ErrorCatalogProvider` loader/normalizer/validator/factory boundary audit is complete for the current scope.
- `CatalogProviderPipeline` loader/normalizer/validator/payload-factory boundary audit is complete for the current scope.
- `ErrorCatalogContextProvider` is intentionally transparent for exceptions and null tasks from its five internal providers; existing propagation/shape/null-task contracts must not be replaced by normalization there.
- `BuiltInErrorCatalogContextProvider` template-provider and injected context-provider ordinary-exception/cancellation/null-response behavior is complete for the previously defined scope.
- The complete `WhenItFails.Tests` suite is locally verified **GREEN at 1014/1014 tests with zero compiler warnings** before the new null-task contract.
- A focused null-task contract for the injected `IErrorCatalogContextProvider` is committed and awaits local verification.

## 2026-09-10 — built-in context-provider null-task contract

Contract commit: `ba3a8533c7d33f352bb43d626fa032b2e906daac`.

Updated:

`WhenItFails.Tests/Catalog/BuiltInErrorCatalogContextProviderContextProviderExceptionContractTests.cs`

Added:

`LoadAsync_WhenContextProviderReturnsNullTask_ReturnsStableFailure`

The fake `IErrorCatalogContextProvider.LoadFromJsonsAsync(...)` returns `null!` instead of a `Task<Response<ErrorCatalogContext>>`.

Required public contract:

```text
Status: Failed
Data: null
Code: WIF_BUILT_IN_CONTEXT_LOAD_FAILED
Message: The bundled WhenItFails catalog context could not be loaded.
```

No production code changed. The null `Task` should cause an ordinary `NullReferenceException` at the `await`, which is inside the existing normalizing catch and therefore should become the stable sanitized built-in load failure.

## 2026-09-10 — 1014/1014 GREEN BuiltInErrorCatalogContextProvider checkpoint

Checkpoint commit: `0bbe72d86d885b2de89d66e8233fe15d0d406ea1`.
Context-provider cancellation contract commit: `59da25c7489c2d131d10a1285ed5715c240f0ca6`.
Context-provider ordinary-exception contract commit: `6913cd4d02769f42958128fc5471dba8fe72f3e8`.
Template-provider cancellation contract commit: `5344b2a111a005e3b53cb3d8ba7e4c8986648de1`.
Template-provider sanitization fix commit: `13429fd33d6c467409007d084b63711cd2e9c472`.
Template-provider ordinary-exception contract commit: `5910b04e8942384c5f0c7cdd0f1dadff6aff2aa6`.

Verified locally:

```text
WhenItFails.Tests
Failed:   0
Passed: 1014
Skipped:  0
Total:  1014
Compiler warnings: 0
```

`BuiltInErrorCatalogContextProvider` established behavior includes:

- template provider null collection → `WIF_BUILT_IN_TEMPLATES_NULL`;
- template provider empty collection → `WIF_BUILT_IN_TEMPLATES_EMPTY`;
- malformed template entries → stable Invalid responses;
- template-provider ordinary exception → `WIF_BUILT_IN_CONTEXT_LOAD_FAILED` with sanitized message;
- template-provider exact `OperationCanceledException` instance propagates unchanged;
- injected context-provider null `Response` → `WIF_BUILT_IN_CONTEXT_PROVIDER_RESPONSE_NULL`;
- injected context-provider ordinary exception → `WIF_BUILT_IN_CONTEXT_LOAD_FAILED` with sanitized message;
- injected context-provider exact `OperationCanceledException` instance propagates unchanged;
- temporary workspace cleanup remains in `finally`.

## Established transparent lower boundary — do not normalize

`ErrorCatalogContextProvider.LoadFromJsonsAsync(...)` intentionally preserves exceptions from its five internal catalog providers.

Relevant suites include:

- `WhenItFails.Tests/Catalog/ErrorCatalogContextProviderProviderExceptionPropagationTests.cs`
- `WhenItFails.Tests/Catalog/ErrorCatalogContextProviderExceptionShapeTests.cs`
- `WhenItFails.Tests/Catalog/ErrorCatalogContextProviderCancellationPropagationTests.cs`
- `WhenItFails.Tests/Catalog/ErrorCatalogContextProviderOwnerNullTaskTests.cs`
- `WhenItFails.Tests/Catalog/ErrorCatalogContextProviderProfileNullTaskTests.cs`

Null tasks at this lower transparent boundary are intentionally observable as `NullReferenceException`; do not normalize them there.

## Fresh reconnaissance notes

A repository-wide search for remaining raw `exception.Message` use found `JsonCatalogDocumentLoader`, `JsonCatalogDocumentWriter`, `JsonsBootstrapper`, and Setter tooling.

Do **not** change `JsonCatalogDocumentLoader.InvalidJson` merely to sanitize the parser message: `Docs/Loading-and-Normalization/en.md` explicitly documents that the loader includes the JSON parser message in the structured failure response. That is an established public contract unless deliberately redesigned later.

## Verification state

- Clean continuation baseline: **1014/1014 GREEN, zero compiler warnings**.
- New null-task contract is committed and awaiting focused local verification.
- No production change is expected.
- Expected complete-suite count after it passes: **1015/1015 GREEN with zero compiler warnings**.

## Recommended verification

Pull current `master` and run:

```powershell
dotnet test WhenItFails.Tests --filter "FullyQualifiedName~LoadAsync_WhenContextProviderReturnsNullTask_ReturnsStableFailure"
dotnet test WhenItFails.Tests
```

Expected results:

```text
Focused contract: GREEN
Complete suite: 1015/1015 GREEN
Compiler warnings: 0
```

## Next recommended step

After **1015/1015 GREEN** is confirmed, record the checkpoint and perform fresh reconnaissance for other async dependency null-task gaps before selecting a new behavior change.

Keep changes small, tested, documented here, and committed directly to `master`.
