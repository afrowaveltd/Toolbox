# Implementation status

Last updated: 2026-09-07

This file is the continuation point for `WhenItFails` development. Update it after every implementation, test, catalog, or documentation change that alters the current state or recommended next step.

## Current focus

Hardening runtime and service boundaries against malformed dependency behavior, raw exception leakage, and internally inconsistent responses.

## Current state

- `WhenItFails` provides structured error catalogs, runtime error resolution, profiles, diagnostics, initialization/recovery behavior, and project-local catalog handling.
- `ErrorDescriptorResolver` has stable contracts for failed/malformed definition responses, null dependency responses, null descriptor-factory results, ordinary dependency exceptions, and cancellation.
- Ordinary exceptions from `IErrorDefinitionResolver` are converted through one shared boundary into `ErrorDefinitionResolverFailed` without exposing raw exception text.
- `OperationCanceledException` from the definition resolver propagates as the exact original instance.
- Ordinary descriptor-factory exceptions become `ErrorDescriptorFactoryFailed`; descriptor-factory cancellation also propagates as the exact original instance.
- The `ErrorDescriptorResolver` exception/cancellation hardening block is complete for the current scope.
- `ErrorDescriptorService` stabilizes null responses from `IErrorDescriptorResolver` and converts ordinary resolver exceptions through one shared boundary into `ErrorDescriptorResolverFailed`.
- `OperationCanceledException` from `IErrorDescriptorResolver` propagates through `ErrorDescriptorService` as the exact original instance.
- The complete `WhenItFails.Tests` suite is verified GREEN at 970/970 tests.
- The `ErrorDescriptorService` resolver exception/cancellation hardening block is complete for the current scope.
- The next distinct boundary under inspection is `ErrorCatalogRuntime` calling injected runtime services.

## Latest committed steps

### 2026-09-07 — verified ErrorDescriptorService resolver cancellation contract

Contract commit: `0027ba19d27dbe86a10f7dd39f0398f2678051f1`

Verified locally:

```text
WhenItFails.Tests
Failed:   0
Passed: 970
Skipped:  0
Total:  970
```

This confirms that `OperationCanceledException` thrown by the injected `IErrorDescriptorResolver` passes through the shared `ErrorDescriptorService.ResolveDescriptor(...)` boundary as the exact original exception instance.

No production change was required.

### 2026-09-07 — centralized ErrorDescriptorService resolver exception boundary

Production fix commit: `c41256ca8efdf137bff5a2d7ab5287eb10862990`

`FromId(...)`, `FromName(...)`, and `FromCode(...)` delegate through one shared `ResolveDescriptor(...)` boundary. Ordinary resolver exceptions become `ErrorDescriptorResolverFailed`, while null responses remain handled independently by `EnsureResponse(...)`.

### 2026-09-07 — centralized definition-resolver exception boundary

Production fix commit: `8be4191d089446893a5686b693821a3c6da230f0`

`ErrorDescriptorResolver` uses one shared boundary for ID, name, and code definition resolution. Ordinary exceptions become `ErrorDefinitionResolverFailed`; cancellation propagates.

## Verification state

- Complete verified continuation baseline: 970/970 tests GREEN.
- `ErrorDescriptorResolver` ordinary-exception and cancellation behavior is verified for both definition resolution and descriptor creation.
- `ErrorDescriptorService` null-response, ordinary-exception, and cancellation behavior is verified for its resolver dependency.
- Both resolver/service hardening blocks are complete for the current scope.
- `ErrorCatalogRuntime` already guards null responses from several injected dependencies, but its ordinary-exception behavior still needs focused review one dependency boundary at a time.

## Recommended verification

No verification is currently pending. The verified continuation baseline is:

```text
WhenItFails.Tests
Failed:   0
Passed: 970
Skipped:  0
Total:  970
```

## Next recommended step

Inspect `ErrorCatalogRuntime` one injected dependency at a time. Start with the synchronous descriptor-service call used by `FromId(...)`: the runtime already converts a null `IErrorDescriptorService` response into `WIF_DESCRIPTOR_SERVICE_RESPONSE_NULL`, but an ordinary exception from a custom/injected descriptor service currently escapes the runtime facade.

Establish one focused `FromId(...)` contract before making production changes. Keep cancellation separate and do not add name/code symmetry until the first contract is locally verified RED/GREEN.

Avoid broader refactoring. Keep each step small, tested, documented here, and committed directly to `master`.
