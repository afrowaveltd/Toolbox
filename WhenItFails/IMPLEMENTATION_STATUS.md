# Implementation status

Last updated: 2026-09-04

This file is the continuation point for `WhenItFails` development. Update it after every implementation, test, catalog, or documentation change that alters the current state or recommended next step.

## Current focus

Hardening runtime and descriptor contracts against malformed or externally supplied `Response<T>` values, especially nullable or internally inconsistent issue collections.

## Current state

- `WhenItFails` provides structured error catalogs, runtime error resolution, profiles, diagnostics, initialization/recovery behavior, and project-local catalog handling.
- `ErrorDescriptorResolver` preserves the status of failed definition responses and produces a descriptor failure without invoking the descriptor factory.
- `ErrorDescriptorResolver.GetFirstIssueCode(...)` currently selects the first non-null issue and falls back to `ErrorDefinitionResolveFailed` only when no usable issue object exists or its `Code` is `null`.
- `IssueInfo.Code` is a non-null string whose default value is `string.Empty`, so malformed responses can contain a non-null issue with an empty or whitespace-only code.
- Contract coverage includes null issue collections, a null first issue, and a null-leading collection followed by a valid issue.
- A new focused contract now requires a whitespace-only first issue code to use the stable fallback code instead of propagating whitespace.

## Latest committed steps

### 2026-09-04 — whitespace resolver issue-code contract

Commit: `2c1c938f75b701a455bde1c62c989f5c4bbd94d4`

Added:

`WhenItFails.Tests/Descriptors/ErrorDescriptorResolverFallbackContractTests.CreateById_ShouldUseFallbackCode_WhenFirstIssueCodeIsWhitespace`

Contract:

```text
first issue exists
first issue.Code = whitespace
              ↓
use ErrorDefinitionResolveFailed
```

No production code was changed in this step. Based on the current implementation, this focused contract is expected to expose the next malformed-response boundary and should be verified locally before changing production code.

### 2026-09-04 — verified null-leading resolver issue contract

Commit containing the contract: `c2aac2daa4a20220de1bf36ead5cbe1826e8b772`

Verified locally on Windows against `master`:

```text
WhenItFails.Tests
Failed:   0
Passed: 954
Skipped:  0
Total:  954
```

The run targeted `net10.0` and completed successfully. The installed .NET 11 preview SDK emitted only the expected `NETSDK1057` preview-SDK informational message; it did not affect the test result.

## Verification state

- Baseline before the new whitespace-code contract: complete `WhenItFails.Tests` suite GREEN, 954/954 passed.
- New whitespace-code contract committed and awaiting focused local verification.
- Do not modify production code until the focused test result is known.

## Recommended verification

Run only the new contract first:

```powershell
dotnet test WhenItFails.Tests --filter "FullyQualifiedName~CreateById_ShouldUseFallbackCode_WhenFirstIssueCodeIsWhitespace"
```

If RED as expected, preserve the failure output and make the smallest production change in `ErrorDescriptorResolver.GetFirstIssueCode(...)` necessary to treat null/empty/whitespace issue codes as unusable.

After the fix, rerun the focused test, then:

```powershell
dotnet test WhenItFails.Tests --filter "FullyQualifiedName~ErrorDescriptorResolverFallbackContractTests"
dotnet test WhenItFails.Tests
```

## Next recommended step

Resolve only the whitespace-code contract if it fails. Avoid broader refactoring.

Once the focused test and full suite are GREEN again, inspect the next malformed-response boundary and repeat the same narrow test-first sequence.

Keep each step small, tested, documented here, and committed directly to `master`.
