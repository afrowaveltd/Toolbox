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
- `ErrorCatalogContextProviderWhitespaceSourceMessageFallbackTests`: **1 user-verified green**.
- Current `ErrorCatalogContextProviderWhitespaceSourceIssueCodeFallbackTests`: **1 test pending verification**.

The audit protects provider ordering and paths, short-circuiting, cancellation, exception identity and shape, null tasks, null responses, null payloads, cross-validation envelopes, provider-specific fallback details, source-message preservation, source-code preservation, first-issue selection, later-issue suppression, and whitespace-only message fallback.

The full-source failure-envelope sequence is complete across error, category, code-group, owner, and profile boundaries.

The new malformed-source-code contract requires:

- an error-provider failure with `ResultStatus.Failed`;
- a non-empty source response message;
- a first source issue whose code is whitespace-only;
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

Expected result: **1 green test**. The current implementation may expose a missing guard and produce one focused failure; if so, fix only `CreateFailedContextResponse` and rerun this test.

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

First verify `ErrorCatalogContextProviderWhitespaceSourceIssueCodeFallbackTests`; the expected count is **1 test**.

If it fails because the output issue code remains whitespace, update `CreateFailedContextResponse` so a null, empty, or whitespace first source issue code uses the provider-specific fallback code. Do not select a later source issue.

## Last completed change

The eighty-fourth runtime/public-API audit slice adds `ErrorCatalogContextProviderWhitespaceSourceIssueCodeFallbackTests`. The error provider returns `ResultStatus.Failed`, a meaningful response message, and two source issues: the first has a whitespace-only code and the second has a valid code. The context provider must retain first-issue selection semantics while replacing the malformed first code with `ErrorCatalogContextErrorCatalogLoadFailed`, preserve the source message, emit one issue, return no context, and invoke no later provider.

Commit:

```text
80609d830106e68043d5ac2af7dd7a22695ba79b
Protect whitespace-only source issue codes
```
