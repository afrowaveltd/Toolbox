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
- Ordinary exceptions from all three `IErrorDefinitionResolver` entry points are now converted through one shared boundary into `ErrorDefinitionResolverFailed` without exposing raw exception text.
- The complete `WhenItFails.Tests` suite is verified GREEN at 963/963 tests before the new definition-resolver symmetry contracts.
- The definition-resolver exception contract class currently contains three tests; before the symmetry fix local verification produced one GREEN (`CreateById`) and two RED (`CreateByName`, `CreateByCode`) results.

## Latest committed steps

### 2026-09-07 — centralized definition-resolver exception boundary

Production fix commit: `8be4191d089446893a5686b693821a3c6da230f0`

Changed:

`WhenItFails/Descriptors/ErrorDescriptorResolver.cs`

The three public entry points now delegate definition resolution through one helper:

```text
CreateById / CreateByName / CreateByCode
                ↓
ResolveAndCreateDescriptor(...)
                ↓
invoke only the selected IErrorDefinitionResolver method inside the exception boundary
                ↓
CreateDescriptorResponse(...)
```

Ordinary definition-resolver exceptions become:

```text
Status: Failed
Code: ErrorDefinitionResolverFailed
Message: Error definition resolver failed.
```

The exception filter excludes `OperationCanceledException`, so cancellation continues to propagate naturally.

The shared helper catches only exceptions thrown while invoking the definition resolver. `CreateDescriptorResponse(...)` executes after the catch boundary, preserving the existing independent factory and descriptor-processing exception semantics.

### 2026-09-07 — verified RED symmetry state

Symmetry contract commit: `f2c9bad3e069a56f75b22f25a9cbcffbdba40a8f`

Focused class:

`WhenItFails.Tests/Descriptors/ErrorDescriptorResolverDefinitionResolverExceptionContractTests`

Observed locally on Windows before the centralized production fix:

```text
Failed: 2
Passed: 1
Skipped: 0
Total: 3
```

Observed behavior:

- `CreateById_ShouldReturnStableFailure_WhenDefinitionResolverThrows`: GREEN
- `CreateByName_ShouldReturnStableFailure_WhenDefinitionResolverThrows`: RED with `Sensitive definition resolver name detail must not escape.`
- `CreateByCode_ShouldReturnStableFailure_WhenDefinitionResolverThrows`: RED with `Sensitive definition resolver code detail must not escape.`

This confirmed that only `CreateById(...)` had the required exception boundary and that raw dependency diagnostic text still escaped from the name/code paths.

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

- Verified continuation baseline before the two new symmetry tests: 963/963 tests GREEN.
- Symmetry tests reproduced the expected pre-fix state: 1 GREEN / 2 RED.
- Centralized definition-resolver exception boundary is committed and awaits local focused verification.
- Expected complete-suite count after both new symmetry tests: 965 tests.

## Recommended verification

Pull current `master` and run the focused contract class:

```powershell
dotnet test WhenItFails.Tests --filter "FullyQualifiedName~ErrorDescriptorResolverDefinitionResolverExceptionContractTests"
```

Expected result after the centralized fix: 3/3 GREEN.

Then run the complete suite:

```powershell
dotnet test WhenItFails.Tests
```

Expected complete-suite result: 965/965 GREEN.

## Next recommended step

After 965/965 GREEN is confirmed, add one focused definition-resolver cancellation contract proving that `OperationCanceledException` still propagates as the exact original instance rather than becoming `ErrorDefinitionResolverFailed`.

If that passes without production changes, move to the next distinct dependency boundary rather than adding more exception permutations.

Avoid broader refactoring. Keep each step small, tested, documented here, and committed directly to `master`.
