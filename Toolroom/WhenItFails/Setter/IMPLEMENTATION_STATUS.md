# Implementation status

Last updated: 2026-07-30

This file is the continuation point for `Toolroom/WhenItFails/Setter` development. Update it after every implementation, test, or documentation change that alters the current state or recommended next step.

## Current state

WhenItFails Setter is a mature .NET 10 command-line authoring and maintenance tool for the WhenItFails JSON catalog workspace under `Jsons/WhenItFails`.

The latest user-verified Setter test run reported **5,029 passed, 0 failed, 0 skipped**. The complete Setter suite is green.

## Runtime/public-API verification

Recently verified focused contracts:

- `ErrorCatalogContextProviderNullResponseTaskResultTests`: **5 green**.
- `ErrorCatalogContextProviderFailureFallbackTests`: **7 green**.
- `ErrorCatalogContextProviderCategorySourceMessageFallbackTests`: **2 green**.
- `ErrorCatalogContextProviderCodeGroupSourceEnvelopeTests`: **1 green**.
- `ErrorCatalogContextProviderOwnerSourceEnvelopeTests`: **1 green**.
- `ErrorCatalogContextProviderProfileSourceEnvelopeTests`: **1 green**.
- `ErrorCatalogContextProviderWhitespaceSourceMessageFallbackTests`: **1 green**.
- `ErrorCatalogContextProviderWhitespaceSourceIssueCodeFallbackTests`: **3 user-verified green**, covering empty and whitespace issue codes plus the combined malformed-code/malformed-message envelope.
- Current `ErrorCatalogContextProviderFailureIssueSeverityTests`: **1 test pending verification**.

The audit protects provider ordering and configured paths, short-circuiting, cancellation, exception identity and shape, null tasks, null responses, null payloads, cross-validation envelopes, provider-specific fallback details, source-message preservation, source-code preservation, first-issue selection, later-issue suppression, malformed code/message fallback, and original status preservation.

The full-source failure-envelope sequence is complete across error, category, code-group, owner, and profile boundaries.

The current response-shape contract requires:

- an error-provider failure with `ResultStatus.Failed`;
- a first source issue with a meaningful code but misleading `Warning` severity and its own message;
- preservation of the source issue code and source response message;
- synthesis of a distinct output issue with `Error` severity;
- no mutation or reuse of the source `IssueInfo` instance;
- exactly one output issue;
- no context or later provider execution.

Do not invent automatic `DefaultMappings` consumption.

## Focused verification

```powershell
dotnet test WhenItFails.Tests --filter FullyQualifiedName~ErrorCatalogContextProviderFailureIssueSeverityTests
```

Expected result: **1 green test**.

Primary Setter verification command:

```powershell
dotnet test Toolroom/WhenItFails/Setter.Tests
```

Before committing catalog changes, also run:

```powershell
dotnet run --project Toolroom/WhenItFails/Setter -- validate .
dotnet run --project Toolroom/WhenItFails/Setter -- check-doc-keys .
dotnet run --project Toolroom/WhenItFails/Setter -- check-doc-links .
git diff --check
```

## Documentation status

High-value Setter documentation synchronization is complete. Maintain `README.md`, `Readme/en.md`, and the existing English documents under `Docs/<topic>/en.md` in line with actual behavior.

Next documentation target: keep this file synchronized while the runtime/public-API audit continues; no separate documentation expansion is currently required.

## Current intentional boundaries

Setter currently does not provide automatic schema migration, multi-file atomic transactions, multi-process locking, backup-retention cleanup, complete localization lifecycle management, remote catalog synchronization, package publishing, a GUI or interactive TUI, complete source-code dependency discovery, or a full security audit and semantic duplicate detector.

## Working rules

- GitHub `master` is the source of truth.
- Keep each change narrow and intentional.
- Add or update tests immediately.
- Do not advance while the current slice is red.
- Update this file after every change.

## Recommended next step

Run `ErrorCatalogContextProviderFailureIssueSeverityTests`; the expected count is **1 green test**.

If green, record the verification and inspect exactly one adjacent synthesized-issue shape contract, such as suppression of source issue details, number, or metadata.

## Last completed change

The eighty-eighth runtime/public-API audit slice adds `ErrorCatalogContextProviderFailureIssueSeverityTests`. A failed error provider returns a source issue with a valid code, a source-only message, and `Warning` severity. `ErrorCatalogContextProvider` must create a distinct output issue, preserve the code and response-level message, normalize severity to `Error`, leave the source issue unchanged, emit exactly one issue, return no context, and invoke no later provider.

Commit:

```text
e6c2cba84ce5dbdfefe006b6e9dc570ef2116264
Normalize provider failure issue severity
```
