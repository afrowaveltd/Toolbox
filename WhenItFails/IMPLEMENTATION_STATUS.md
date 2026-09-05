# Implementation status

Last updated: 2026-09-05

This file is the continuation point for `WhenItFails` development. Update it after every implementation, test, catalog, or documentation change that alters the current state or recommended next step.

## Current focus

Hardening runtime and descriptor contracts against malformed or externally supplied `Response<T>` values, especially null responses and internally inconsistent issue collections.

## Current state

- `WhenItFails` provides structured error catalogs, runtime error resolution, profiles, diagnostics, initialization/recovery behavior, and project-local catalog handling.
- `ErrorDescriptorResolver` preserves the status of failed definition responses and produces a descriptor failure without invoking the descriptor factory.
- `ErrorDescriptorResolver.GetFirstIssueCode(...)` selects the first issue whose object is non-null and whose `Code` is not null, empty, or whitespace-only.
- If no usable issue code exists, the resolver falls back to `ErrorDefinitionResolveFailed`.
- Contract coverage includes null issue collections, null issue entries, malformed issue codes, and a resolver implementation that returns a null `Response<ErrorDefinition>`.
- The centralized null definition-resolver response guard is verified by the complete `WhenItFails.Tests` suite: 957/957 tests GREEN.
- Null-response symmetry contracts now cover `CreateById(...)`, `CreateByName(...)`, and `CreateByCode(...)`.

## Latest committed steps

### 2026-09-05 — null definition-resolver response symmetry contracts

Contract commit: `8a2df4bef092fff2f98c93f327579c4b5dc9ba20`

Added focused symmetry coverage:

- `CreateByName_ShouldReturnStableInvalidResponse_WhenDefinitionResolverReturnsNull`
- `CreateByCode_ShouldReturnStableInvalidResponse_WhenDefinitionResolverReturnsNull`

Together with the existing `CreateById(...)` contract, all three public resolver entry points now require the same stable behavior:

```text
IErrorDefinitionResolver => null
             ↓
Status: Invalid
Code: ErrorDefinitionResolverReturnedNull
Message: Error definition resolver returned a null response.
```

The tests share one assertion helper and one null-returning resolver stub. No production code was changed because the guard is centralized in `CreateDescriptorResponse(...)`.

### 2026-09-05 — verified null definition-resolver response fix

Production fix commit: `7e89e6abe0a92e60383f91faacb54f721b484f29`

Verified locally after pulling the fix:

```text
WhenItFails.Tests
Failed:   0
Passed: 957
Skipped:  0
Total:  957
```

This confirms that a null `Response<ErrorDefinition>` is converted into the stable invalid descriptor response without regressing the existing suite.

### 2026-09-05 — verified RED null definition-resolver response contract

Contract commit: `248daa2ba288ebb3e921169906a961dc20887c45`

Focused test:

`WhenItFails.Tests/Descriptors/ErrorDescriptorResolverNullResponseContractTests.CreateById_ShouldReturnStableInvalidResponse_WhenDefinitionResolverReturnsNull`

Observed locally on Linux before the production fix:

```text
Failed: 1
Passed: 0
Skipped: 0
Total: 1
```

Failure:

```text
System.NullReferenceException
at ErrorDescriptorResolver.CreateDescriptorResponse(...): line 67
```

This confirmed that the previous implementation dereferenced the null definition response before validating it.

### 2026-09-05 — verified later valid issue-code contract

Contract commit: `711c227a1704874a0b9a86547cbd55729c4a323b`

Verified locally:

```text
WhenItFails.Tests
Failed:   0
Passed: 956
Skipped:  0
Total:  956
```

This confirms that an earlier malformed whitespace-only issue code is skipped and the first later valid issue code is preserved without additional production changes.

## Verification state

- Verified continuation baseline before symmetry coverage: complete `WhenItFails.Tests` suite GREEN, 957/957 passed.
- `CreateById(...)` null-response contract is verified GREEN after the production fix.
- New `CreateByName(...)` and `CreateByCode(...)` symmetry contracts are committed and await local verification.
- Production code remains unchanged for the symmetry step.

## Recommended verification

Pull current `master` and run the complete null-response contract class:

```powershell
dotnet test WhenItFails.Tests --filter "FullyQualifiedName~ErrorDescriptorResolverNullResponseContractTests"
```

If green, run the complete package suite:

```powershell
dotnet test WhenItFails.Tests
```

Expected complete-suite count after the two new symmetry contracts: 959 tests.

## Next recommended step

Once the symmetry contracts are verified GREEN, move away from null-response permutations and inspect a genuinely different malformed-response boundary.

A strong next candidate is the successful-definition path where `IErrorDescriptorFactory.Create(...)` itself returns `null` despite its non-nullable contract. The current resolver immediately passes that value into `Response<ErrorDescriptor>.Ok(...)`; verify whether Essentials rejects or accidentally accepts that malformed success payload, then define the stable WhenItFails behavior with one focused contract.

Avoid broader refactoring. Keep each step small, tested, documented here, and committed directly to `master`.
