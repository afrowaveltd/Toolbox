# Implementation status

Last updated: 2026-09-05

This file is the continuation point for `WhenItFails` development. Update it after every implementation, test, catalog, or documentation change that alters the current state or recommended next step.

## Current focus

Hardening runtime and descriptor contracts against malformed or externally supplied `Response<T>` values, especially nullable or internally inconsistent issue collections.

## Current state

- `WhenItFails` provides structured error catalogs, runtime error resolution, profiles, diagnostics, initialization/recovery behavior, and project-local catalog handling.
- `ErrorDescriptorResolver` preserves the status of failed definition responses and produces a descriptor failure without invoking the descriptor factory.
- `ErrorDescriptorResolver.GetFirstIssueCode(...)` selects the first issue whose object is non-null and whose `Code` is not null, empty, or whitespace-only.
- If no usable issue code exists, the resolver falls back to `ErrorDefinitionResolveFailed`.
- Contract coverage includes null issue collections, a null first issue, a null-leading collection followed by a valid issue, and a whitespace-only issue code.
- The whitespace-only issue-code contract was verified RED before the production fix, proving that the previous implementation propagated whitespace into `Response<T>.Fail(...)` and triggered `ArgumentException` in `IssueInfoFactory`.
- The production fix is now verified by the complete `WhenItFails.Tests` suite: 955/955 tests GREEN.

## Latest committed steps

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

This confirms that malformed whitespace-only issue codes now use the stable fallback without regressing the existing suite.

### 2026-09-04 — whitespace resolver issue-code fix

Changed:

`WhenItFails/Descriptors/ErrorDescriptorResolver.cs`

`GetFirstIssueCode(...)` now ignores issue entries whose `Code` is null, empty, or whitespace-only and returns the first usable code instead. If none exists, it uses the stable fallback code.

The change is intentionally narrow and does not alter descriptor creation, response status preservation, or message fallback behavior.

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

### 2026-09-04 — verified null-leading resolver issue contract

Commit containing the contract: `c2aac2daa4a20220de1bf36ead5cbe1826e8b772`

Verified locally on Windows:

```text
WhenItFails.Tests
Failed:   0
Passed: 954
Skipped:  0
Total:  954
```

The installed .NET 11 preview SDK emitted only the expected `NETSDK1057` preview-SDK informational message; it did not affect the test result.

## Verification state

- Complete `WhenItFails.Tests` suite GREEN: 955/955 passed.
- Whitespace-code contract: verified RED before the production fix and GREEN after the fix as part of the complete suite.
- Current production implementation is the verified continuation baseline.

## Next recommended step

Add the next narrow contract for a malformed collection where an unusable issue code is followed by a later valid issue code. The contract should confirm that the resolver skips the unusable code and preserves the first later usable code.

Expected behavior:

```text
Issues = [whitespaceCodeIssue, validIssue]
                         ↓
use validIssue.Code
```

Production code should remain unchanged unless the focused contract exposes a real failure.

Avoid broader refactoring. Keep each step small, tested, documented here, and committed directly to `master`.
