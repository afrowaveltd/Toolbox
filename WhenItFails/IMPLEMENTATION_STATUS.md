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

## Latest committed steps

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

- Complete verified baseline: 962/962 tests GREEN.
- Descriptor-factory ordinary exception and cancellation contracts are both verified GREEN.
- No production changes are pending verification.

## Recommended verification

No additional verification is required for the completed descriptor-factory exception boundary.

## Next recommended step

Move to a genuinely different dependency boundary: unexpected exceptions thrown by `IErrorDefinitionResolver`.

Start with one focused `CreateById(...)` contract. The current method calls `_definitionResolver.FindById(...)` before entering `CreateDescriptorResponse(...)`, so an ordinary resolver exception currently escapes raw.

Define a stable outward failure without exposing the dependency exception message. Preserve cancellation behavior for a separate follow-up contract rather than broadening this first step.

Avoid broader refactoring. Keep each step small, tested, documented here, and committed directly to `master`.
