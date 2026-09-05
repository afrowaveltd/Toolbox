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
- The complete `WhenItFails.Tests` suite is verified GREEN at 959/959 tests after the symmetry contracts.
- A broken `IErrorDescriptorFactory` implementation returning `null` is now guarded so the resolver cannot emit `Success` with a null descriptor payload.

## Latest committed steps

### 2026-09-05 — null descriptor-factory result fix

Production fix commit: `a784f6c0e0b11273c65b400cdb2a827b7721673e`

Changed:

`WhenItFails/Descriptors/ErrorDescriptorResolver.cs`

After a successful definition resolution, the descriptor returned by `IErrorDescriptorFactory.Create(...)` is now checked before constructing the outward response.

A null factory result now becomes:

```text
Status: Invalid
Code: ErrorDescriptorFactoryReturnedNull
Message: Error descriptor factory returned a null descriptor.
```

The change is intentionally narrow and preserves all existing definition-failure, null-definition, and issue-code behavior.

### 2026-09-05 — verified RED null descriptor-factory result contract

Contract commits:

- `a9b4f8852cb713a6a2a5a8f39c7de3eb56986bce`
- fixture correction: `f7cd0127d68362d94d8b7ee78d5b2eb65720d0f5`

Focused test:

`WhenItFails.Tests/Descriptors/ErrorDescriptorResolverNullFactoryResultContractTests.CreateById_ShouldReturnStableInvalidResponse_WhenDescriptorFactoryReturnsNull`

Observed locally on Linux before the production fix:

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

This confirms that the centralized null definition-resolver guard behaves consistently for `CreateById(...)`, `CreateByName(...)`, and `CreateByCode(...)` without further production changes.

## Verification state

- Verified continuation baseline: complete `WhenItFails.Tests` suite GREEN, 959/959 passed.
- Null descriptor-factory result contract: verified RED before the production fix.
- Production null-factory-result guard is committed and awaits focused local verification and then the complete `WhenItFails.Tests` suite.

## Recommended verification

Pull current `master` and run only the focused contract:

```powershell
dotnet test WhenItFails.Tests --filter "FullyQualifiedName~CreateById_ShouldReturnStableInvalidResponse_WhenDescriptorFactoryReturnsNull"
```

If green, run the complete package suite:

```powershell
dotnet test WhenItFails.Tests
```

Expected complete-suite count: 960 tests.

## Next recommended step

Once the null descriptor-factory result guard is verified GREEN, move to the next distinct dependency-contract failure rather than adding more null permutations.

A useful next candidate is exception behavior: define whether an exception thrown by `IErrorDescriptorFactory.Create(...)` should intentionally propagate or be converted into a stable failure response. Inspect existing project conventions first and add one focused contract only after the intended boundary is clear.

Avoid broader refactoring. Keep each step small, tested, documented here, and committed directly to `master`.
