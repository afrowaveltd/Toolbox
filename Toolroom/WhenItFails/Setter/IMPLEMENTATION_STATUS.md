# Implementation status

Last updated: 2026-07-30

This file is the continuation point for `Toolroom/WhenItFails/Setter` development. Update it after every implementation, test, or documentation change that alters the current state or recommended next step.

## Current state

WhenItFails Setter is a mature .NET 10 command-line authoring and maintenance tool for the WhenItFails JSON catalog workspace under `Jsons/WhenItFails`.

The latest user-verified Setter test run reported **5,029 passed, 0 failed, 0 skipped**. The complete Setter suite is green.

## Verification status

Recently verified focused contracts:

- `ErrorCatalogContextProviderNullResponseTaskResultTests`: **5 green**.
- `ErrorCatalogContextProviderFailureFallbackTests`: **7 green**.
- `ErrorCatalogContextProviderCategorySourceMessageFallbackTests`: **2 green**.
- `ErrorCatalogContextProviderCodeGroupSourceEnvelopeTests`: **1 green**.
- `ErrorCatalogContextProviderOwnerSourceEnvelopeTests`: **1 green**.
- `ErrorCatalogContextProviderProfileSourceEnvelopeTests`: **1 green**.
- `ErrorCatalogContextProviderWhitespaceSourceMessageFallbackTests`: **1 green**.
- `ErrorCatalogContextProviderWhitespaceSourceIssueCodeFallbackTests`: **3 green**.
- `ErrorCatalogContextProviderFailureIssueSeverityTests`: **1 user-verified green** for severity normalization and source issue immutability.
- Current source-only-field slice adds a second test to `ErrorCatalogContextProviderFailureIssueSeverityTests`; the expected count is **2 tests**.

The runtime/public-API audit protects provider ordering and configured paths, short-circuiting, cancellation, exception identity and shape, null tasks, null responses, null payloads, cross-validation envelopes, provider-specific fallbacks, source-message and source-code selection, malformed envelopes, first-issue selection, later-issue suppression, synthesized issue severity, and source object immutability.

The current response-shape contract requires the synthesized failure issue to preserve only the selected code and response-level message, normalize severity to `Error`, and suppress source-only `Number`, `Details`, and `Metadata`. The source `IssueInfo` and its metadata bag must remain unchanged.

Do not invent automatic `DefaultMappings` consumption.

## Current CI repair

GitHub Actions run `30545059089`, job `90879002972`, reported **1,239 passed and 2 failed** in the Setter test project.

The two failures were:

- `ImplementationStatusDocumentationTests.Documentation_ProvidesCurrentContinuationPoint`, because this file had lost exact headings and documentation anchors required by its contract. All required anchors are restored in this change.
- `SuggestDocumentationKeyBracketTitleTests.ExecuteAsync_WithBracketedTitle_ShowsLiteralTitleAndCanonicalKey`, because the assertion depended on ANSI decoration placement around the rich-output `Title:` label. Production already escapes the title through `Markup.Escape`; the test now asserts the literal bracketed title independently of terminal decoration and is user-verified green.

## Focused verification

Run the two repaired Setter tests first:

```powershell
dotnet test Toolroom/WhenItFails/Setter.Tests --filter "FullyQualifiedName~ImplementationStatusDocumentationTests|FullyQualifiedName~SuggestDocumentationKeyBracketTitleTests"
```

Expected result: **2 green tests**.

Then run the complete Setter suite:

```powershell
dotnet test Toolroom/WhenItFails/Setter.Tests
```

Also retain the current runtime/public-API focused command:

```powershell
dotnet test WhenItFails.Tests --filter FullyQualifiedName~ErrorCatalogContextProviderFailureIssueSeverityTests
```

Expected result: **2 green tests**.

Before committing catalog changes, also run:

```powershell
dotnet run --project Toolroom/WhenItFails/Setter -- validate .
dotnet run --project Toolroom/WhenItFails/Setter -- check-doc-keys .
dotnet run --project Toolroom/WhenItFails/Setter -- check-doc-links .
git diff --check
```

## Documentation synchronization completed

High-value Setter documentation synchronization is complete. Maintained English documentation includes:

- `README.md` and `Readme/en.md`;
- `Docs/Overview/en.md`;
- `Docs/Commands/en.md` and `Docs/Command Quick Reference/en.md`;
- `Docs/Getting-Started/en.md`;
- `Docs/FAQ/en.md`;
- `Docs/Known Limitations/en.md`;
- `Docs/Roadmap and Future Work/en.md`;
- `Docs/Testing and CI/en.md`;
- `Docs/Reviewing Catalog Changes/en.md`;
- `Docs/Safe Writes/en.md` and `Docs/Backups and Recovery/en.md`;
- `Docs/Architecture Overview/en.md`;
- `Docs/Contributing to Setter/en.md`;
- `Docs/Maintainer Notes/en.md`.

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

First rerun the two repaired Setter tests; the expected count is **2 green tests**.

If green, run the complete Setter suite. Do not resume the runtime/public-API audit until the full Setter suite is green again.

## Last completed change

The ninety-first maintenance slice restores every exact continuation-document anchor required by `ImplementationStatusDocumentationTests`, including `## Verification status`, `## Documentation synchronization completed`, and the maintained documentation path list. The bracket-title test is already user-verified green. The remaining focused verification target is the documentation contract, followed by the complete Setter suite.

Commit in this repair sequence:

```text
13d80b6e4d7890f77b52f205da8de00863c92c44
Make bracket title assertion ANSI-safe
```
