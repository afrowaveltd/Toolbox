# Implementation status

Last updated: 2026-07-30

This file is the continuation point for `Toolroom/WhenItFails/Setter` development. Update it after every implementation, test, or documentation change that alters the current state or recommended next step.

## Current state

WhenItFails Setter is a mature .NET 10 command-line authoring and maintenance tool for the WhenItFails JSON catalog workspace under `Jsons/WhenItFails`.

The latest user-verified Setter test run reported **5,029 passed, 0 failed, 0 skipped**. The complete Setter suite is green.

## Runtime/public-API verification

The latest user-verified Setter test run remains the source anchor for the complete Setter suite.

Recently verified focused contracts:

- `ErrorCatalogContextProviderNullResponseTaskResultTests`: **5 green**.
- `ErrorCatalogContextProviderFailureFallbackTests`: **7 green**.
- `ErrorCatalogContextProviderCategorySourceMessageFallbackTests`: **2 green**.
- `ErrorCatalogContextProviderCodeGroupSourceEnvelopeTests`: **1 green**.
- `ErrorCatalogContextProviderOwnerSourceEnvelopeTests`: **1 green**.
- `ErrorCatalogContextProviderProfileSourceEnvelopeTests`: **1 green**.
- `ErrorCatalogContextProviderWhitespaceSourceMessageFallbackTests`: **1 green**.
- `ErrorCatalogContextProviderWhitespaceSourceIssueCodeFallbackTests`: initially **1 red**, exposing a missing malformed-code guard; production fix is committed and awaiting rerun.

The audit protects provider ordering and paths, short-circuiting, cancellation, exception identity and shape, null tasks, null responses, null payloads, cross-validation envelopes, provider-specific fallback details, source-message preservation, source-code preservation, first-issue selection, later-issue suppression, whitespace-only message fallback, and malformed first-issue-code fallback.

The full-source failure-envelope sequence is complete across error, category, code-group, owner, and profile boundaries.

The malformed-source-code production fix now requires:

- an error-provider failure with `ResultStatus.Failed`;
- a non-empty source response message;
- a first source issue whose code is null, empty, or whitespace-only;
- use of `ErrorCatalogContextErrorCatalogLoadFailed` instead of exposing the malformed code;
- preservation of the source message in both the outer response and output issue;
- exactly one output issue;
- no promotion of a later source issue code;
- no context or later provider execution.

Do not invent automatic `DefaultMappings` consumption.

## Focused verification

```powershell
dotnet test WhenItFails.Tests --filter FullyQualifiedName~ErrorCatalogContextProviderWhitespaceSourceIssueCodeFallbackTests
```

Expected result after commit `15c67cf7ace0974384fb23b8fe07b3fef272fd42`: **1 green test**.

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

Rerun `ErrorCatalogContextProviderWhitespaceSourceIssueCodeFallbackTests`; the expected count is **1 green test**.

If green, record the verification and inspect exactly one adjacent malformed-source contract outside this slice.

## Last completed change

The eighty-fifth runtime/public-API audit slice fixes the malformed first-source-issue-code path in `ErrorCatalogContextProvider.CreateFailedContextResponse`. The helper now reads the first source issue code, validates it with `string.IsNullOrWhiteSpace`, and uses the provider-specific fallback code when the source code is null, empty, or whitespace. It retains first-issue selection semantics, does not promote a later source issue, preserves the source response message and status, and prevents the lower-level `IssueInfoFactory` validation exception.

Commit:

```text
15c67cf7ace0974384fb23b8fe07b3fef272fd42
Guard malformed provider failure issue codes
```
