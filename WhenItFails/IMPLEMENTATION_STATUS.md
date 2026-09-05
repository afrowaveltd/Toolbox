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
- The null descriptor-factory result guard is verified by the complete `WhenItFails.Tests` suite: 960/960 tests GREEN.
- A new focused contract now defines behavior for an exception thrown by `IErrorDescriptorFactory.Create(...)`.

## Latest committed steps

### 2026-09-05 — descriptor-factory exception contract

Contract commit: `c4836bf8e7bef89a8059a112b94dfd3e9f16288c`

Added:

`WhenItFails.Tests/Descriptors/ErrorDescriptorResolverFactoryExceptionContractTests.CreateById_ShouldReturnStableFailure_WhenDescriptorFactoryThrows`

Contract:

```text
IErrorDefinitionResolver => successful non-null ErrorDefinition
IErrorDescriptorFactory.Create(...) => throws
                         ↓
Status: Failed
Code: ErrorDescriptorFactoryFailed
Message: Error descriptor factory failed.
```

The factory stub throws an exception whose message contains sensitive diagnostic text. The outward contract deliberately requires a stable public message instead of propagating that raw exception text.

No production code was changed in this step.

The current resolver invokes `_descriptorFactory.Create(...)` without an exception boundary, so this focused contract is expected to be RED with the original `InvalidOperationException` escaping.

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

## Verification state

- Verified continuation baseline: complete `WhenItFails.Tests` suite GREEN, 960/960 passed.
- Descriptor-factory exception contract is committed and awaits focused local verification.
- Production code remains unchanged until the focused RED/GREEN state is observed.

## Recommended verification

Pull current `master` and run only the new contract:

```powershell
dotnet test WhenItFails.Tests --filter "FullyQualifiedName~CreateById_ShouldReturnStableFailure_WhenDescriptorFactoryThrows"
```

Expected current result: RED with the `InvalidOperationException` from the test factory escaping `ErrorDescriptorResolver.CreateDescriptorResponse(...)`.

Preserve that focused failure output before changing production code.

## Next recommended step

If the focused contract fails as expected, add the smallest exception boundary around descriptor-factory invocation and return the stable `Failed` response required by the contract. Do not broaden the catch to definition resolution or unrelated code paths in the same step.

Avoid broader refactoring. Keep each step small, tested, documented here, and committed directly to `master`.
