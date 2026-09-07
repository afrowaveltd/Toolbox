# Implementation status

Last updated: 2026-09-07

This file is the continuation point for `WhenItFails` development. Update it after every implementation, test, catalog, or documentation change that alters the current state or recommended next step.

## Current focus

Hardening runtime and descriptor contracts against malformed dependency behavior and internally inconsistent success/failure responses.

## Current state

- `WhenItFails` provides structured error catalogs, runtime error resolution, profiles, diagnostics, initialization/recovery behavior, and project-local catalog handling.
- `ErrorDescriptorResolver` preserves the status of failed definition responses and produces a descriptor failure without invoking the descriptor factory.
- `ErrorDescriptorResolver.GetFirstIssueCode(...)` selects the first issue whose object is non-null and whose `Code` is not null, empty, or whitespace-only.
- If no usable issue code exists, the resolver falls back to `ErrorDefinitionResolveFailed`.
- Null `Response<ErrorDefinition>` values returned by `IErrorDefinitionResolver` are converted into a stable invalid descriptor response.
- Null-response symmetry is covered for `CreateById(...)`, `CreateByName(...)`, and `CreateByCode(...)`.
- A broken `IErrorDescriptorFactory` implementation returning `null` is guarded so the resolver cannot emit `Success` with a null descriptor payload.
- Descriptor-factory ordinary exceptions become a stable failed response without exposing the original exception message.
- `OperationCanceledException` is explicitly rethrown by the descriptor-factory exception boundary.
- The descriptor-factory cancellation contract is verified by the complete `WhenItFails.Tests` suite: 962/962 tests GREEN.
- `CreateById(...)` now has a narrow exception boundary around `IErrorDefinitionResolver.FindById(...)` for ordinary dependency exceptions.

## Latest committed steps

### 2026-09-07 — definition-resolver exception fix for CreateById

Production fix commit: `a9db9668d4fc08632277c253e4dff1ed17dcfcf1`

Changed:

`WhenItFails/Descriptors/ErrorDescriptorResolver.cs`

`CreateById(...)` now catches ordinary exceptions thrown by `IErrorDefinitionResolver.FindById(...)` and converts them into:

```text
Status: Failed
Code: ErrorDefinitionResolverFailed
Message: Error definition resolver failed.
```

The original exception message is deliberately not copied into the public response.

The catch uses an exception filter that excludes `OperationCanceledException`, so cancellation continues to propagate naturally.

This change is intentionally limited to `CreateById(...)`; `CreateByName(...)` and `CreateByCode(...)` remain unchanged until the focused contract is verified GREEN.

### 2026-09-07 — verified RED definition-resolver exception contract

Contract commit: `a5516eab9fe5e829be6eabf2ce8f1256c52edabd`

Focused test:

`WhenItFails.Tests/Descriptors/ErrorDescriptorResolverDefinitionResolverExceptionContractTests.CreateById_ShouldReturnStableFailure_WhenDefinitionResolverThrows`

Observed locally on Windows before the production fix:

```text
Failed: 1
Passed: 0
Skipped: 0
Total: 1
```

Failure:

```text
System.InvalidOperationException:
Sensitive definition resolver detail must not escape.
```

The exception escaped directly from `IErrorDefinitionResolver.FindById(...)` through `ErrorDescriptorResolver.CreateById(...)`, confirming the missing dependency boundary and the raw diagnostic-text leak.

### 2026-09-07 — verified descriptor-factory cancellation contract

Contract commit: `2bf741e404146a8686b483655d95d58ef357a5dd`

Verified locally:

```text
WhenItFails.Tests
Failed:   0
Passed: 962
Skipped:  0
Total:  962
```

This confirms that `OperationCanceledException` thrown by `IErrorDescriptorFactory.Create(...)` is rethrown as the exact original exception instance rather than converted into `ErrorDescriptorFactoryFailed` or wrapped in another exception.

## Verification state

- Complete verified baseline before the definition-resolver exception contract: 962/962 tests GREEN.
- Definition-resolver ordinary exception contract: verified RED before the production fix.
- Production `CreateById(...)` exception boundary is committed and awaits focused local verification and then the complete `WhenItFails.Tests` suite.
- `CreateByName(...)` / `CreateByCode(...)` symmetry is not yet implemented.

## Recommended verification

Pull current `master` and run the focused contract:

```powershell
dotnet test WhenItFails.Tests --filter "FullyQualifiedName~CreateById_ShouldReturnStableFailure_WhenDefinitionResolverThrows"
```

If green, run the complete package suite:

```powershell
dotnet test WhenItFails.Tests
```

Expected complete-suite count: 963 tests.

## Next recommended step

After 963/963 GREEN is confirmed, add focused symmetry contracts for `CreateByName(...)` and `CreateByCode(...)` before centralizing the definition-resolver exception boundary.

After symmetry is verified, add one cancellation contract proving that an `OperationCanceledException` from the definition resolver still propagates rather than becoming `ErrorDefinitionResolverFailed`.

Avoid broader refactoring. Keep each step small, tested, documented here, and committed directly to `master`.
