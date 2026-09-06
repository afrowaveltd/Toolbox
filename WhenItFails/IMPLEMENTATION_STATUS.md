# Implementation status

Last updated: 2026-09-06

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
- A broken `IErrorDescriptorFactory` implementation returning `null` is guarded so the resolver cannot emit `Success` with a null descriptor payload.
- The null descriptor-factory result guard is verified by the complete `WhenItFails.Tests` suite: 960/960 tests GREEN.
- Descriptor-factory exceptions now have a narrow exception boundary: ordinary exceptions become a stable failed response, while `OperationCanceledException` is rethrown.

## Latest committed steps

### 2026-09-06 — descriptor-factory exception fix

Production fix commit: `717cc33e26534aa30f1634505f5248b38ea4658c`

Changed:

`WhenItFails/Descriptors/ErrorDescriptorResolver.cs`

The call to `IErrorDescriptorFactory.Create(...)` is now wrapped by the smallest possible exception boundary.

Ordinary factory exceptions become:

```text
Status: Failed
Code: ErrorDescriptorFactoryFailed
Message: Error descriptor factory failed.
```

The original exception message is deliberately not copied into the public response.

`OperationCanceledException` is explicitly rethrown so cancellation is not converted into an ordinary failure response.

### 2026-09-06 — verified RED descriptor-factory exception contract

Contract commit: `c4836bf8e7bef89a8059a112b94dfd3e9f16288c`

Focused test:

`WhenItFails.Tests/Descriptors/ErrorDescriptorResolverFactoryExceptionContractTests.CreateById_ShouldReturnStableFailure_WhenDescriptorFactoryThrows`

Observed locally on Linux before the production fix:

```text
Failed: 1
Passed: 0
Skipped: 0
Total: 1
```

Failure:

```text
System.InvalidOperationException:
Sensitive descriptor factory detail must not escape.
```

The exception escaped from `IErrorDescriptorFactory.Create(...)` through `ErrorDescriptorResolver.CreateDescriptorResponse(...)`, confirming the missing exception boundary and demonstrating that raw internal diagnostic text could escape the resolver.

### 2026-09-05 — verified null descriptor-factory result guard

Production fix commit: `a784f6c0e0b11273c65b400cdb2a827b7721673e`

Verified locally after pulling the fix:

```text
WhenItFails.Tests
Failed:   0
Passed: 960
Skipped:  0
Total:  960
```

This confirms that `IErrorDescriptorFactory.Create(...) => null` is converted into:

```text
Status: Invalid
Code: ErrorDescriptorFactoryReturnedNull
Message: Error descriptor factory returned a null descriptor.
```

without regressing the existing suite.

### 2026-09-05 — verified RED null descriptor-factory result contract

Contract commits:

- `a9b4f8852cb713a6a2a5a8f39c7de3eb56986bce`
- fixture correction: `f7cd0127d68362d94d8b7ee78d5b2eb65720d0f5`

Focused test:

`WhenItFails.Tests/Descriptors/ErrorDescriptorResolverNullFactoryResultContractTests.CreateById_ShouldReturnStableInvalidResponse_WhenDescriptorFactoryReturnsNull`

Observed locally before the production fix:

```text
Failed: 1
Passed: 0
Skipped: 0
Total: 1
```

Failure:

```text
Assert.False() Failure
Expected: False
Actual:   True
```

## Verification state

- Verified continuation baseline before the exception contract: complete `WhenItFails.Tests` suite GREEN, 960/960 passed.
- Descriptor-factory exception contract: verified RED before the production fix.
- Production exception boundary is committed and awaits focused local verification and then the complete `WhenItFails.Tests` suite.

## Recommended verification

Pull current `master` and run only the focused contract:

```powershell
dotnet test WhenItFails.Tests --filter "FullyQualifiedName~CreateById_ShouldReturnStableFailure_WhenDescriptorFactoryThrows"
```

If green, run the complete package suite:

```powershell
dotnet test WhenItFails.Tests
```

Expected complete-suite count: 961 tests.

## Next recommended step

After the descriptor-factory exception contract is verified GREEN, add one focused cancellation contract proving that `OperationCanceledException` from the factory is intentionally rethrown rather than converted into `ErrorDescriptorFactoryFailed`.

If that is GREEN without production changes, move to the next distinct dependency boundary instead of adding more factory-exception permutations.

Avoid broader refactoring. Keep each step small, tested, documented here, and committed directly to `master`.
