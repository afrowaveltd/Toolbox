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
- The next distinct boundary under test is an ordinary exception thrown by `IErrorDefinitionResolver.FindById(...)`.

## Latest committed steps

### 2026-09-07 — definition-resolver exception contract

Contract commit: `a5516eab9fe5e829be6eabf2ce8f1256c52edabd`

Added:

`WhenItFails.Tests/Descriptors/ErrorDescriptorResolverDefinitionResolverExceptionContractTests.CreateById_ShouldReturnStableFailure_WhenDefinitionResolverThrows`

Contract:

```text
IErrorDefinitionResolver.FindById(...) => throws ordinary exception
                         ↓
Status: Failed
Code: ErrorDefinitionResolverFailed
Message: Error definition resolver failed.
```

The resolver stub throws an exception containing sensitive diagnostic text. The outward contract deliberately requires a stable public message and issue rather than exposing the original exception text.

The descriptor factory is a throwing sentinel and must not run when definition resolution itself throws.

No production code was changed in this step.

The current `CreateById(...)` calls `_definitionResolver.FindById(...)` before entering `CreateDescriptorResponse(...)`, so this focused contract is expected to be RED with the original `InvalidOperationException` escaping.

### 2026-09-07 — verified descriptor-factory cancellation contract

Contract commit: `2bf741e404146a8686b483655d95d58ef357a5dd`

Verified locally on Linux:

```text
WhenItFails.Tests
Failed:   0
Passed: 962
Skipped:  0
Total:  962
```

This confirms that `OperationCanceledException` thrown by `IErrorDescriptorFactory.Create(...)` is rethrown as the exact original exception instance rather than converted into `ErrorDescriptorFactoryFailed` or wrapped in another exception.

No production change was required for the cancellation contract.

### 2026-09-06 — verified descriptor-factory exception fix

Production fix commit: `717cc33e26534aa30f1634505f5248b38ea4658c`

Verified locally on Linux after pulling the fix:

```text
WhenItFails.Tests
Failed:   0
Passed: 961
Skipped:  0
Total:  961
```

This confirms that an ordinary exception thrown by `IErrorDescriptorFactory.Create(...)` is converted into:

```text
Status: Failed
Code: ErrorDescriptorFactoryFailed
Message: Error descriptor factory failed.
```

The original exception message is not exposed by the resolver.

### 2026-09-05 — verified null descriptor-factory result guard

Production fix commit: `a784f6c0e0b11273c65b400cdb2a827b7721673e`

Verified locally:

```text
WhenItFails.Tests
Failed:   0
Passed: 960
Skipped:  0
Total:  960
```

## Verification state

- Complete verified baseline before the new definition-resolver exception contract: 962/962 tests GREEN.
- Descriptor-factory ordinary exception and cancellation contracts are both verified GREEN.
- Definition-resolver ordinary exception contract is committed and awaits focused local verification.
- Production code remains unchanged until the focused RED/GREEN state is observed.

## Recommended verification

Pull current `master` and run only the new contract:

```powershell
dotnet test WhenItFails.Tests --filter "FullyQualifiedName~CreateById_ShouldReturnStableFailure_WhenDefinitionResolverThrows"
```

Expected current result: RED with the `InvalidOperationException` from the test definition resolver escaping `ErrorDescriptorResolver.CreateById(...)`.

Preserve that focused failure output before changing production code.

## Next recommended step

If the focused contract fails as expected, add the smallest exception boundary around definition-resolver invocation for `CreateById(...)` and return the stable `Failed` response required by the contract.

Do not add `CreateByName(...)` / `CreateByCode(...)` symmetry or cancellation handling in the same production step. Verify the single contract first, then centralize carefully only if the shape remains clear.

Avoid broader refactoring. Keep each step small, tested, documented here, and committed directly to `master`.
