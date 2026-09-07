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
- Ordinary exceptions from all three `IErrorDefinitionResolver` entry points are converted through one shared boundary into `ErrorDefinitionResolverFailed` without exposing raw exception text.
- The complete `WhenItFails.Tests` suite is verified GREEN at 965/965 tests after centralizing the definition-resolver exception boundary.

## Latest committed steps

### 2026-09-07 — verified centralized definition-resolver exception boundary

Production fix commit: `8be4191d089446893a5686b693821a3c6da230f0`

Verified locally after pulling the centralized fix:

```text
WhenItFails.Tests
Failed:   0
Passed: 965
Skipped:  0
Total:  965
```

This confirms that `CreateById(...)`, `CreateByName(...)`, and `CreateByCode(...)` all convert ordinary `IErrorDefinitionResolver` exceptions into:

```text
Status: Failed
Code: ErrorDefinitionResolverFailed
Message: Error definition resolver failed.
```

without exposing the original dependency exception text.

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

Ordinary definition-resolver exceptions become `ErrorDefinitionResolverFailed`. The exception filter excludes `OperationCanceledException`, so cancellation continues to propagate naturally.

The helper catches only exceptions thrown while invoking the definition resolver. `CreateDescriptorResponse(...)` executes after the catch boundary, preserving the independent factory and descriptor-processing exception semantics.

### 2026-09-07 — verified RED symmetry state

Symmetry contract commit: `f2c9bad3e069a56f75b22f25a9cbcffbdba40a8f`

Before the centralized production fix the focused contract class produced:

```text
Failed: 2
Passed: 1
Skipped: 0
Total: 3
```

`CreateById(...)` was GREEN while `CreateByName(...)` and `CreateByCode(...)` leaked their original resolver exceptions.

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

The exact original `OperationCanceledException` instance propagates from the descriptor factory.

## Verification state

- Complete verified continuation baseline: 965/965 tests GREEN.
- Definition-resolver ordinary-exception behavior is symmetric and verified for ID, name, and numeric code paths.
- Production code uses one shared exception boundary for definition resolution.

## Recommended verification

The current committed baseline is already locally verified GREEN at 965/965 tests.

## Next recommended step

Add one focused definition-resolver cancellation contract proving that `OperationCanceledException` propagates as the exact original instance rather than becoming `ErrorDefinitionResolverFailed`.

Because all three entry points now share `ResolveAndCreateDescriptor(...)`, one focused public-path contract is sufficient to lock the centralized cancellation semantics before moving to the next distinct dependency boundary.

Avoid broader refactoring. Keep each step small, tested, documented here, and committed directly to `master`.
