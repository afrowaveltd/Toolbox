# Implementation status

Last updated: 2026-09-07

This file is the continuation point for `WhenItFails` development. Update it after every implementation, test, catalog, or documentation change that alters the current state or recommended next step.

## Current focus

Hardening runtime and descriptor contracts against malformed dependency behavior and internally inconsistent success/failure responses.

## Current state

- `WhenItFails` provides structured error catalogs, runtime error resolution, profiles, diagnostics, initialization/recovery behavior, and project-local catalog handling.
- `ErrorDescriptorResolver` preserves failed definition-response status and does not invoke the descriptor factory when definition resolution fails.
- Null definition-resolver responses, malformed issue collections/codes, null descriptor-factory results, and descriptor-factory exceptions have stable outward contracts.
- Ordinary descriptor-factory exceptions become `ErrorDescriptorFactoryFailed` without exposing raw exception text.
- Descriptor-factory `OperationCanceledException` is rethrown as the exact original exception instance.
- Ordinary exceptions from all three `IErrorDefinitionResolver` entry points are converted through one shared boundary into `ErrorDefinitionResolverFailed` without exposing raw exception text.
- The complete `WhenItFails.Tests` suite is verified GREEN at 965/965 tests after centralizing the definition-resolver exception boundary.
- A focused cancellation contract now requires the shared definition-resolver boundary to rethrow the exact original `OperationCanceledException` instance.

## Latest committed steps

### 2026-09-07 — definition-resolver cancellation contract

Contract commit: `94edf27b427ebcba2f387d6526e6d949610d118f`

Updated:

`WhenItFails.Tests/Descriptors/ErrorDescriptorResolverDefinitionResolverExceptionContractTests.cs`

Added:

`CreateById_ShouldRethrowOperationCanceledException_WhenDefinitionResolverCancels`

Contract:

```text
IErrorDefinitionResolver.FindById(...) => OperationCanceledException instance
                         ↓
rethrow the exact same OperationCanceledException instance
```

The test uses `Assert.Same(...)`, so future refactoring cannot silently wrap cancellation or convert it into `ErrorDefinitionResolverFailed`.

The descriptor factory is a throwing sentinel and must not run when definition resolution cancels.

No production code changed in this step. The shared `ResolveAndCreateDescriptor(...)` exception filter already excludes `OperationCanceledException`, so the focused contract is expected to be GREEN.

### 2026-09-07 — verified centralized definition-resolver exception boundary

Production fix commit: `8be4191d089446893a5686b693821a3c6da230f0`

Verified locally after pulling the centralized fix:

```text
WhenItFails.Tests
Failed:   0
Passed: 965
Skipped:  0
Total:  965
```

This confirms that `CreateById(...)`, `CreateByName(...)`, and `CreateByCode(...)` all convert ordinary `IErrorDefinitionResolver` exceptions into:

```text
Status: Failed
Code: ErrorDefinitionResolverFailed
Message: Error definition resolver failed.
```

without exposing the original dependency exception text.

### 2026-09-07 — centralized definition-resolver exception boundary

Production fix commit: `8be4191d089446893a5686b693821a3c6da230f0`

The three public entry points delegate through `ResolveAndCreateDescriptor(...)`. Only the selected definition-resolver invocation is inside the exception boundary; descriptor processing remains outside it.

### 2026-09-07 — verified descriptor-factory cancellation contract

Contract commit: `2bf741e404146a8686b483655d95d58ef357a5dd`

The exact original `OperationCanceledException` instance propagates from the descriptor factory.

## Verification state

- Complete verified continuation baseline: 965/965 tests GREEN.
- Definition-resolver ordinary-exception behavior is symmetric and verified for ID, name, and numeric code paths.
- Definition-resolver cancellation contract is committed and awaits focused local verification.
- No production change is expected for the cancellation contract.

## Recommended verification

Pull current `master` and run the focused cancellation contract:

```powershell
dotnet test WhenItFails.Tests --filter "FullyQualifiedName~CreateById_ShouldRethrowOperationCanceledException_WhenDefinitionResolverCancels"
```

If green, run the complete suite:

```powershell
dotnet test WhenItFails.Tests
```

Expected complete-suite count: 966 tests.

## Next recommended step

After 966/966 GREEN is confirmed, move to the next distinct dependency boundary instead of adding more definition-resolver exception permutations.

Inspect another core resolver/runtime boundary where an injected dependency can return malformed data, `null`, or throw outside an existing structured-response boundary. Prefer one focused contract at a time.

Avoid broader refactoring. Keep each step small, tested, documented here, and committed directly to `master`.
