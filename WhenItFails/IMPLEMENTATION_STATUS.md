# Implementation status

Last updated: 2026-09-06

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
- The complete `WhenItFails.Tests` suite is verified GREEN at 961/961 tests after the descriptor-factory exception fix.
- A focused regression contract now requires descriptor-factory cancellation to rethrow the exact original `OperationCanceledException` instance.

## Latest committed steps

### 2026-09-06 — descriptor-factory cancellation contract

Contract commit: `2bf741e404146a8686b483655d95d58ef357a5dd`

Added:

`ErrorDescriptorResolverFactoryExceptionContractTests.CreateById_ShouldRethrowOperationCanceledException_WhenDescriptorFactoryCancels`

Contract:

```text
IErrorDescriptorFactory.Create(...) => OperationCanceledException instance
                         ↓
rethrow the exact same OperationCanceledException instance
```

The test uses `Assert.Same(...)` so future refactoring cannot silently wrap cancellation in another exception or convert it into `ErrorDescriptorFactoryFailed`.

No production code was changed. The current exception boundary already has an explicit `catch (OperationCanceledException) { throw; }`, so this contract is expected to be GREEN.

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

### 2026-09-06 — verified RED descriptor-factory exception contract

Contract commit: `c4836bf8e7bef89a8059a112b94dfd3e9f16288c`

Observed locally before the production fix:

```text
Failed: 1
Passed: 0
Skipped: 0
Total: 1
```

Failure:

```text
System.InvalidOperationException:
Sensitive descriptor factory detail must not escape.
```

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

- Complete verified baseline before the new cancellation contract: 961/961 tests GREEN.
- Descriptor-factory ordinary exception contract is verified GREEN after the production fix.
- Descriptor-factory cancellation contract is committed and awaits focused local verification.
- No production change is expected for the cancellation step.

## Recommended verification

Pull current `master` and run the focused cancellation contract:

```powershell
dotnet test WhenItFails.Tests --filter "FullyQualifiedName~CreateById_ShouldRethrowOperationCanceledException_WhenDescriptorFactoryCancels"
```

If green, run the complete package suite:

```powershell
dotnet test WhenItFails.Tests
```

Expected complete-suite count: 962 tests.

## Next recommended step

Once the cancellation contract is verified GREEN, move to a genuinely different dependency boundary rather than adding more descriptor-factory exception permutations.

Inspect exception behavior of `IErrorDefinitionResolver` next: unlike descriptor-factory invocation, `CreateById(...)`, `CreateByName(...)`, and `CreateByCode(...)` currently call the definition resolver before entering `CreateDescriptorResponse(...)`, so an unexpected resolver exception may still escape raw. Establish one focused `CreateById(...)` contract first before considering symmetry.

Avoid broader refactoring. Keep each step small, tested, documented here, and committed directly to `master`.
