# Implementation status

Last updated: 2026-09-07

This file is the continuation point for `WhenItFails` development. Update it after every implementation, test, catalog, or documentation change that alters the current state or recommended next step.

## Current focus

Hardening runtime and descriptor contracts against malformed dependency behavior and internally inconsistent success/failure responses.

## Current state

- `WhenItFails` provides structured error catalogs, runtime error resolution, profiles, diagnostics, initialization/recovery behavior, and project-local catalog handling.
- `ErrorDescriptorResolver` preserves failed definition-response status and does not invoke the descriptor factory when definition resolution fails.
- Null definition-resolver responses, malformed issue collections/codes, null descriptor-factory results, and descriptor-factory exceptions have stable outward contracts.
- Ordinary descriptor-factory exceptions become `ErrorDescriptorFactoryFailed` without exposing raw exception text.
- Descriptor-factory `OperationCanceledException` is rethrown as the exact original exception instance.
- `CreateById(...)` has a narrow ordinary-exception boundary around `IErrorDefinitionResolver.FindById(...)` and returns `ErrorDefinitionResolverFailed` without exposing raw exception text.
- The complete `WhenItFails.Tests` suite is verified GREEN at 963/963 tests after the `CreateById(...)` definition-resolver exception fix.
- New symmetry contracts now require the same stable behavior for `CreateByName(...)` and `CreateByCode(...)`.

## Latest committed steps

### 2026-09-07 — definition-resolver exception symmetry contracts

Contract commit: `f2c9bad3e069a56f75b22f25a9cbcffbdba40a8f`

Updated:

`WhenItFails.Tests/Descriptors/ErrorDescriptorResolverDefinitionResolverExceptionContractTests.cs`

Coverage now includes:

- `CreateById_ShouldReturnStableFailure_WhenDefinitionResolverThrows`
- `CreateByName_ShouldReturnStableFailure_WhenDefinitionResolverThrows`
- `CreateByCode_ShouldReturnStableFailure_WhenDefinitionResolverThrows`

All three require:

```text
Status: Failed
Code: ErrorDefinitionResolverFailed
Message: Error definition resolver failed.
```

Each definition-resolver entry point throws its own sensitive diagnostic text. The outward response must never expose that text, and the descriptor factory must not run.

No production code changed in this symmetry step. `CreateByName(...)` and `CreateByCode(...)` are therefore expected to be RED until their exception boundary is implemented or centralized.

### 2026-09-07 — verified CreateById definition-resolver exception fix

Production fix commit: `a9db9668d4fc08632277c253e4dff1ed17dcfcf1`

Verified locally:

```text
WhenItFails.Tests
Failed:   0
Passed: 963
Skipped:  0
Total:  963
```

This confirms that an ordinary exception from `IErrorDefinitionResolver.FindById(...)` becomes:

```text
Status: Failed
Code: ErrorDefinitionResolverFailed
Message: Error definition resolver failed.
```

without exposing the original exception message.

### 2026-09-07 — verified RED CreateById definition-resolver exception contract

Contract commit: `a5516eab9fe5e829be6eabf2ce8f1256c52edabd`

Observed before the production fix:

```text
System.InvalidOperationException:
Sensitive definition resolver detail must not escape.
```

The exception escaped directly from `FindById(...)`, proving the missing boundary.

### 2026-09-07 — verified descriptor-factory cancellation contract

Contract commit: `2bf741e404146a8686b483655d95d58ef357a5dd`

Verified locally:

```text
WhenItFails.Tests
Failed:   0
Passed: 962
Skipped:  0
Total:  962
```

The exact original `OperationCanceledException` instance propagates.

## Verification state

- Verified continuation baseline: 963/963 tests GREEN.
- `CreateById(...)` ordinary definition-resolver exception contract is GREEN.
- New `CreateByName(...)` and `CreateByCode(...)` symmetry contracts are committed and await local verification.
- Production code is unchanged for the symmetry step.

## Recommended verification

Pull current `master` and run the definition-resolver exception contract class:

```powershell
dotnet test WhenItFails.Tests --filter "FullyQualifiedName~ErrorDescriptorResolverDefinitionResolverExceptionContractTests"
```

Expected current result:

- `CreateById(...)`: GREEN
- `CreateByName(...)`: RED with the original resolver exception escaping
- `CreateByCode(...)`: RED with the original resolver exception escaping

After the smallest production fix, rerun this focused class and then the complete suite.

Expected complete-suite count after both new symmetry tests: 965 tests.

## Next recommended step

If the two new symmetry contracts fail as expected, centralize the ordinary definition-resolver exception conversion carefully so all three public entry points share one stable boundary without duplicating catch logic.

After 965/965 GREEN, add one focused definition-resolver cancellation contract proving that `OperationCanceledException` still propagates rather than becoming `ErrorDefinitionResolverFailed`.

Avoid broader refactoring. Keep each step small, tested, documented here, and committed directly to `master`.
