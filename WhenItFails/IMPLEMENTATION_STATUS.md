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
- The next distinct boundary under test is a broken `IErrorDescriptorFactory` implementation that returns `null` despite its non-nullable contract.

## Latest committed steps

### 2026-09-05 — null descriptor-factory result contract

Contract commits:

- `a9b4f8852cb713a6a2a5a8f39c7de3eb56986bce`
- fixture correction: `f7cd0127d68362d94d8b7ee78d5b2eb65720d0f5`

Added:

`WhenItFails.Tests/Descriptors/ErrorDescriptorResolverNullFactoryResultContractTests.CreateById_ShouldReturnStableInvalidResponse_WhenDescriptorFactoryReturnsNull`

Contract:

```text
IErrorDefinitionResolver => successful non-null ErrorDefinition
IErrorDescriptorFactory.Create(...) => null
                         ↓
return stable Invalid Response<ErrorDescriptor>
code: ErrorDescriptorFactoryReturnedNull
message: Error descriptor factory returned a null descriptor.
```

No production code was changed in this step.

`Response<T>.Ok(T? data)` in Essentials accepts nullable data and creates a successful response even when `data` is `null`. Therefore the current resolver is expected to produce a malformed `Success + null Data` response when the factory violates its contract. The new focused test should expose that behavior.

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

### 2026-09-05 — null definition-resolver response fix

Production fix commit: `7e89e6abe0a92e60383f91faacb54f721b484f29`

`CreateDescriptorResponse(...)` accepts `Response<ErrorDefinition>?` and converts a null resolver response into:

```text
Status: Invalid
Code: ErrorDefinitionResolverReturnedNull
Message: Error definition resolver returned a null response.
```

## Verification state

- Verified continuation baseline: complete `WhenItFails.Tests` suite GREEN, 959/959 passed.
- New null descriptor-factory result contract is committed and awaits focused local verification.
- Production code remains unchanged until the focused RED/GREEN state is observed.

## Recommended verification

Pull current `master` and run only the new contract:

```powershell
dotnet test WhenItFails.Tests --filter "FullyQualifiedName~CreateById_ShouldReturnStableInvalidResponse_WhenDescriptorFactoryReturnsNull"
```

Expected current result: RED because the resolver currently passes the null descriptor into `Response<ErrorDescriptor>.Ok(...)`, which accepts null data and returns a successful response.

Preserve the focused failure output before changing production code.

After the smallest production fix is committed, rerun the focused test and then the complete `WhenItFails.Tests` suite.

## Next recommended step

Resolve only the null descriptor-factory result boundary if the focused contract fails as expected.

Once GREEN, inspect the next distinct dependency-contract failure. Prefer meaningful boundaries such as exceptions or malformed successful results rather than additional permutations of already centralized null guards.

Avoid broader refactoring. Keep each step small, tested, documented here, and committed directly to `master`.
