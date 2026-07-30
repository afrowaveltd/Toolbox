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
- `ErrorCatalogContextProviderWhitespaceSourceIssueCodeFallbackTests`: **1 user-verified green** after the malformed-code production guard fix.
- Current expansion converts that test into a theory for empty-string and whitespace-only first source issue codes; the expected count is **2 tests**.

The audit protects provider ordering and paths, short-circuiting, cancellation, exception identity and shape, null tasks, null responses, null payloads, cross-validation envelopes, provider-specific fallback details, source-message preservation, source-code preservation, first-issue selection, later-issue suppression, whitespace-only message fallback, and malformed first-issue-code fallback.

The full-source failure-envelope sequence is complete across error, category, code-group, owner, and profile boundaries.

The malformed-source-code guard requires:

- an error-provider failure with `ResultStatus.Failed`;
- a non-empty source response message;
- a first source issue whose code is empty or whitespace-only;
- use of `ErrorCatalogContextErrorCatalogLoadFailed` instead of exposing the malformed code;
- preservation of the source message in both the outer response and output issue;
- exactly one output issue;
- no promotion of a later source issue code;
- no context or later provider execution.

The production guard also handles null through `string.IsNullOrWhiteSpace`; no null test is added because `IssueInfo.Code` is a non-nullable public contract and forcing null would add artificial nullable noise.

Do not invent automatic `DefaultMappings` consumption.

## Focused verification

```powershell
dotnet test WhenItFails.Tests --filter FullyQualifiedName~ErrorCatalogContextProviderWhitespaceSourceIssueCodeFallbackTests
```

Expected result: **2 green tests**.

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

Run `ErrorCatalogContextProviderWhitespaceSourceIssueCodeFallbackTests`; the expected count is **2 green tests**.

If green, record the verification and inspect exactly one adjacent malformed-source contract outside this slice.

## Last completed change

The eighty-sixth runtime/public-API audit slice expands `ErrorCatalogContextProviderWhitespaceSourceIssueCodeFallbackTests` from one fact into a two-case theory. Both empty-string and whitespace-only first source issue codes must use `ErrorCatalogContextErrorCatalogLoadFailed`, preserve the source response message and status, emit exactly one issue, retain first-issue selection semantics, and avoid all later provider execution. No production change is required beyond the already committed malformed-code guard.

Commit:

```text
d19d6f57b4726ca7f4bdbcd51c673913e058636d
Cover empty and whitespace source issue codes
```
