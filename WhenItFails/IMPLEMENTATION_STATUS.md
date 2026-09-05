# Implementation status

Last updated: 2026-09-05

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
- The null descriptor-factory result guard is now verified by the complete `WhenItFails.Tests` suite: 960/960 tests GREEN.

## Latest committed steps

### 2026-09-05 — verified null descriptor-factory result guard

Production fix commit: `a784f6c0e0b11273c65b400cdb2a827b7721673e`

Verified locally after pulling the fix:

```text
WhenItFails.Tests
Failed:   0
Passed: 960
Skipped:  0
Total:  960
```

This confirms that `IErrorDescriptorFactory.Create(...) => null` is converted into:

```text
Status: Invalid
Code: ErrorDescriptorFactoryReturnedNull
Message: Error descriptor factory returned a null descriptor.
```

without regressing the existing suite.

### 2026-09-05 — verified RED null descriptor-factory result contract

Contract commits:

- `a9b4f8852cb713a6a2a5a8f39c7de3eb56986bce`
- fixture correction: `f7cd0127d68362d94d8b7ee78d5b2eb65720d0f5`

Focused test:

`WhenItFails.Tests/Descriptors/ErrorDescriptorResolverNullFactoryResultContractTests.CreateById_ShouldReturnStableInvalidResponse_WhenDescriptorFactoryReturnsNull`

Observed locally before the production fix:

```text
Failed: 1
Passed: 0
Skipped: 0
Total: 1
```

Failure:

```text
Assert.False() Failure
Expected: False
Actual:   True
```

This confirmed that the previous resolver passed the null descriptor into `Response<ErrorDescriptor>.Ok(...)`, producing an invalid `Success + null Data` response instead of rejecting the broken factory result.

### 2026-09-05 — verified null-response symmetry contracts

Symmetry contract commit: `8a2df4bef092fff2f98c93f327579c4b5dc9ba20`

Verified locally:

```text
WhenItFails.Tests
Failed:   0
Passed: 959
Skipped:  0
Total:  959
```

## Verification state

- Verified continuation baseline: complete `WhenItFails.Tests` suite GREEN, 960/960 passed.
- Null definition-resolver response behavior is verified.
- Malformed/null issue-code behavior is verified.
- Null descriptor-factory result behavior is verified.

## Recommended next step

Inspect the next distinct dependency-contract failure: an exception thrown by `IErrorDescriptorFactory.Create(...)`.

Project conventions favor structured `Response` failures for runtime/dependency failures and explicitly warn against exposing raw exception text to users. Cancellation is the documented exception to that rule, but descriptor creation is synchronous and has no cancellation contract.

Add one focused contract requiring a thrown descriptor-factory exception to become a stable failed response without leaking `exception.Message`.

Avoid broader refactoring. Keep each step small, tested, documented here, and committed directly to `master`.
