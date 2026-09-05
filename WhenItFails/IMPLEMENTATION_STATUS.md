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
- Contract coverage includes null issue collections, a null first issue, a null-leading collection followed by a valid issue, a whitespace-only issue code, and a whitespace-code issue followed by a later valid issue.
- The later-valid-issue contract is verified by the complete `WhenItFails.Tests` suite: 956/956 tests GREEN.
- The next distinct boundary under test is a broken `IErrorDefinitionResolver` implementation that returns `null` instead of `Response<ErrorDefinition>`.

## Latest committed steps

### 2026-09-05 — null definition-resolver response contract

Contract commit: `248daa2ba288ebb3e921169906a961dc20887c45`

Added:

`WhenItFails.Tests/Descriptors/ErrorDescriptorResolverNullResponseContractTests.CreateById_ShouldReturnStableInvalidResponse_WhenDefinitionResolverReturnsNull`

Contract:

```text
IErrorDefinitionResolver.FindById(...) => null
                         ↓
return stable Invalid Response<ErrorDescriptor>
code: ErrorDefinitionResolverReturnedNull
message: Error definition resolver returned a null response.
```

No production code was changed in this step.

The current `ErrorDescriptorResolver` dereferences the resolver response in `CreateDescriptorResponse(...)`, so this focused contract is expected to be RED with `NullReferenceException`. Preserve that failure before applying the smallest production fix.

### 2026-09-05 — verified later valid issue-code contract

Contract commit: `711c227a1704874a0b9a86547cbd55729c4a323b`

Verified locally on Windows:

```text
WhenItFails.Tests
Failed:   0
Passed: 956
Skipped:  0
Total:  956
```

This confirms that an earlier malformed whitespace-only issue code is skipped and the first later valid issue code is preserved without additional production changes.

### 2026-09-05 — verified whitespace resolver issue-code fix

Production fix commit: `bd0c48242f9c4ac1d03c63bcaf946ee9dbd7482a`

Verified locally on Windows after pulling the fix:

```text
WhenItFails.Tests
Failed:   0
Passed: 955
Skipped:  0
Total:  955
```

This confirms that malformed whitespace-only issue codes use the stable fallback without regressing the existing suite.

### 2026-09-04 — verified RED whitespace resolver issue-code contract

Contract commit: `2c1c938f75b701a455bde1c62c989f5c4bbd94d4`

Focused test:

`WhenItFails.Tests/Descriptors/ErrorDescriptorResolverFallbackContractTests.CreateById_ShouldUseFallbackCode_WhenFirstIssueCodeIsWhitespace`

Observed locally before the fix:

```text
Failed: 1
Passed: 0
Skipped: 0
Total: 1
```

Failure:

```text
System.ArgumentException:
The value cannot be an empty string or composed entirely of whitespace.
Parameter 'code'
```

The exception originated in `Essentials/Issues/IssueInfoFactory.Create(...)` after `ErrorDescriptorResolver` passed the whitespace code to `Response<ErrorDescriptor>.Fail(...)`.

## Verification state

- Verified continuation baseline before the new contract: complete `WhenItFails.Tests` suite GREEN, 956/956 passed.
- New null-definition-resolver-response contract is committed and awaits focused local verification.
- Production code remains unchanged until the focused RED/GREEN state is observed.

## Recommended verification

Pull current `master` and run only the new contract:

```powershell
dotnet test WhenItFails.Tests --filter "FullyQualifiedName~CreateById_ShouldReturnStableInvalidResponse_WhenDefinitionResolverReturnsNull"
```

Expected current result: RED with `NullReferenceException` from `ErrorDescriptorResolver.CreateDescriptorResponse(...)`.

After preserving the failure output, make the smallest production change necessary to convert a null definition-resolver response into the stable invalid response required by the contract.

Then rerun the focused test and the complete package suite.

## Next recommended step

Resolve only the null definition-resolver response boundary if the focused contract fails as expected. Do not broaden the change into unrelated validation or refactoring.

Once GREEN, consider whether the same centralized guard automatically covers `CreateByName(...)` and `CreateByCode(...)`; add focused symmetry contracts only if they provide meaningful regression protection.

Keep each step small, tested, documented here, and committed directly to `master`.
