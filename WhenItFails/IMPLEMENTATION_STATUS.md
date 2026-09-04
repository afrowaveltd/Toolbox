# Implementation status

Last updated: 2026-09-04

This file is the continuation point for `WhenItFails` development. Update it after every implementation, test, catalog, or documentation change that alters the current state or recommended next step.

## Current focus

Hardening runtime and descriptor contracts against malformed or externally supplied `Response<T>` values, especially nullable or internally inconsistent issue collections.

## Current state

- `WhenItFails` provides structured error catalogs, runtime error resolution, profiles, diagnostics, initialization/recovery behavior, and project-local catalog handling.
- `ErrorDescriptorResolver` preserves the status of failed definition responses and produces a descriptor failure without invoking the descriptor factory.
- `ErrorDescriptorResolver.GetFirstIssueCode(...)` selects the first non-null issue and falls back to `ErrorDefinitionResolveFailed` when no usable issue exists.
- Contract coverage includes a failed resolver response whose first and only issue is `null`.
- Contract coverage also includes a failed resolver response whose first issue is `null` and whose later issue is valid; the valid issue code must be used.
- The latest contract-only change did not require a production-code modification.

## Latest committed steps

### 2026-09-04 — null-leading resolver issue contract

Commit: `c2aac2daa4a20220de1bf36ead5cbe1826e8b772`

Added:

`WhenItFails.Tests/Descriptors/ErrorDescriptorResolverNullFirstIssueContractTests.CreateById_ShouldUseFirstNonNullIssueCode_WhenFirstResolverIssueIsNull`

Contract:

```text
Issues = [null, validIssue]
              ↓
use validIssue.Code
```

The current resolver implementation already follows this contract by selecting the first non-null issue.

## Verification state

Verified locally on Windows against `master` on 2026-09-04:

```text
WhenItFails.Tests
Failed:   0
Passed: 954
Skipped:  0
Total:  954
```

The run targeted `net10.0` and completed successfully. The installed .NET 11 preview SDK emitted only the expected `NETSDK1057` preview-SDK informational message; it did not affect the test result.

The null-leading resolver issue contract is therefore fully verified GREEN together with the complete `WhenItFails.Tests` suite.

## Recommended verification

For every new narrow contract step:

1. run the focused new test first;
2. if green or after the minimal production fix, run the affected test class;
3. then run the complete package suite:

```powershell
dotnet test WhenItFails.Tests
```

## Next recommended step

Continue the same narrow hardening sequence around `ErrorDescriptorResolver` rather than broad refactoring.

Inspect the next malformed-response boundary not yet explicitly covered, add one focused contract test, observe RED/GREEN, and change production code only if the contract exposes a real failure.

Keep each step small, tested, documented here, and committed directly to `master`.
