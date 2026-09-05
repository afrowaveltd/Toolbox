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
- The later-valid-issue contract is verified by the complete `WhenItFails.Tests` suite: 956/956 tests GREEN.
- The null definition-resolver response contract was verified RED with `NullReferenceException` before the production fix.
- `CreateDescriptorResponse(...)` now accepts a nullable definition response and converts `null` into a stable invalid descriptor response.

## Latest committed steps

### 2026-09-05 — null definition-resolver response fix

Production fix commit: `7e89e6abe0a92e60383f91faacb54f721b484f29`

Changed:

`WhenItFails/Descriptors/ErrorDescriptorResolver.cs`

`CreateDescriptorResponse(...)` now accepts `Response<ErrorDefinition>?` and guards `null` before any dereference.

A null resolver response now becomes:

```text
Status: Invalid
Code: ErrorDefinitionResolverReturnedNull
Message: Error definition resolver returned a null response.
```

The guard is centralized, so the same production behavior applies to `CreateById(...)`, `CreateByName(...)`, and `CreateByCode(...)` without duplicated checks.

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

### 2026-09-05 — verified whitespace resolver issue-code fix

Production fix commit: `bd0c48242f9c4ac1d03c63bcaf946ee9dbd7482a`

Verified locally after pulling the fix:

```text
WhenItFails.Tests
Failed:   0
Passed: 955
Skipped:  0
Total:  955
```

This confirms that malformed whitespace-only issue codes use the stable fallback without regressing the existing suite.

## Verification state

- Verified continuation baseline before the null-response contract: complete `WhenItFails.Tests` suite GREEN, 956/956 passed.
- Null definition-resolver response contract: verified RED before the production fix.
- Production null-response guard is committed and awaits focused local verification and then the complete `WhenItFails.Tests` suite.

## Recommended verification

Pull current `master` and run the focused contract:

```powershell
dotnet test WhenItFails.Tests --filter "FullyQualifiedName~CreateById_ShouldReturnStableInvalidResponse_WhenDefinitionResolverReturnsNull"
```

If green, run the complete package suite:

```powershell
dotnet test WhenItFails.Tests
```

Expected complete-suite count: 957 tests.

## Next recommended step

Once the centralized null-response guard is verified GREEN, add focused symmetry contracts for `CreateByName(...)` and `CreateByCode(...)` only if they meaningfully protect the centralized behavior from future refactoring.

After symmetry is locked, move to a genuinely different malformed-response boundary rather than continuing equivalent permutations.

Avoid broader refactoring. Keep each step small, tested, documented here, and committed directly to `master`.
