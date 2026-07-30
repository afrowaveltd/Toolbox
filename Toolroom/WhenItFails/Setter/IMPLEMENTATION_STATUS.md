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
- `ErrorCatalogContextProviderWhitespaceSourceIssueCodeFallbackTests`: **2 user-verified green** for empty-string and whitespace-only first source issue codes.
- Current combined malformed-envelope slice adds a third test to `ErrorCatalogContextProviderWhitespaceSourceIssueCodeFallbackTests`; the next successful run is expected to report **3 tests**.

The audit protects provider ordering and paths, short-circuiting, cancellation, exception identity and shape, null tasks, null responses, null payloads, cross-validation envelopes, provider-specific fallback details, source-message preservation, source-code preservation, first-issue selection, later-issue suppression, whitespace-only message fallback, and malformed first-issue-code fallback.

The full-source failure-envelope sequence is complete across error, category, code-group, owner, and profile boundaries.

The malformed-source guard now has focused coverage for:

- empty first source issue code;
- whitespace-only first source issue code;
- simultaneous whitespace-only source code and response message;
- provider-specific fallback code and fallback message selection;
- preservation of the original failure status;
- exactly one output issue;
- no promotion of a later valid source issue;
- no context or later provider execution.

The production guard also handles null through `string.IsNullOrWhiteSpace`; no null test is added because `IssueInfo.Code` is a non-nullable public contract and forcing null would add artificial nullable noise.

Do not invent automatic `DefaultMappings` consumption.

## Focused verification

```powershell
dotnet test WhenItFails.Tests --filter FullyQualifiedName~ErrorCatalogContextProviderWhitespaceSourceIssueCodeFallbackTests
```

Expected result: **3 green tests**.

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

Run `ErrorCatalogContextProviderWhitespaceSourceIssueCodeFallbackTests`; the expected count is **3 green tests**.

If green, the malformed code/message fallback interaction is complete. Inspect exactly one adjacent response-shape contract outside this slice.

## Last completed change

The eighty-seventh runtime/public-API audit slice adds a combined malformed-envelope test. The error provider returns `ResultStatus.Failed`, a whitespace-only response message, and two source issues whose first code is whitespace-only while the second code is valid. `ErrorCatalogContextProvider` must select both documented error-catalog fallbacks, preserve `Failed`, emit exactly one issue, avoid promoting the later valid issue, return no context, and invoke no later provider.

Commit:

```text
118e982c34852c53eef50018ab06a2a9fbb72165
Protect combined malformed failure fallbacks
```
