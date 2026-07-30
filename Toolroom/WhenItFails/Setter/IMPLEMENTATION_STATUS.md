# Implementation status

Last updated: 2026-07-30

This file is the continuation point for `Toolroom/WhenItFails/Setter` development. Update it after every implementation, test, or documentation change that alters the current state or recommended next step.

## Current state

WhenItFails Setter is a mature .NET 10 command-line authoring and maintenance tool for the WhenItFails JSON catalog workspace under `Jsons/WhenItFails`.

The latest user-verified Setter test run reported **1,241 passed, 0 failed, 0 skipped**. The complete Setter suite is green.

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
- `ErrorCatalogContextProviderFailureIssueSeverityTests`: **2 user-verified green** for severity normalization, source issue immutability, and suppression of source-only issue fields.
- Current provider-response-metadata slice adds a third test to `ErrorCatalogContextProviderFailureIssueSeverityTests`; the expected count is **3 tests**.
- Setter CI repair pair — `ImplementationStatusDocumentationTests` and `SuggestDocumentationKeyBracketTitleTests`: **2 user-verified green**.
- Complete `Toolroom/WhenItFails/Setter.Tests` suite: **1,241 user-verified green, 0 failed, 0 skipped**.

The runtime/public-API audit protects provider ordering and configured paths, short-circuiting, cancellation, exception identity and shape, null tasks, null responses, null payloads, cross-validation envelopes, provider-specific fallbacks, source-message and source-code selection, malformed envelopes, first-issue selection, later-issue suppression, synthesized issue severity, source object immutability, and suppression of source-only failure fields.

The current response-shape contract additionally requires provider-response metadata not to cross into the synthesized `Response<ErrorCatalogContext>`. The output must have empty independent metadata, `Data == null`, `HasData == false`, and `IsFailure == true`, while the source response metadata remains unchanged.

Do not invent automatic `DefaultMappings` consumption.

## Current CI repair

GitHub Actions run `30545059089`, job `90879002972`, originally reported **1,239 passed and 2 failed** in the Setter test project.

Both exposed regressions are repaired and the complete local Setter suite is user-verified green:

- `ImplementationStatusDocumentationTests.Documentation_ProvidesCurrentContinuationPoint`: exact continuation-document headings and maintained documentation anchors are restored.
- `SuggestDocumentationKeyBracketTitleTests.ExecuteAsync_WithBracketedTitle_ShowsLiteralTitleAndCanonicalKey`: the test verifies the literal bracketed title without depending on ANSI decoration placement; production already used `Markup.Escape` correctly.

## Focused verification

The complete Setter suite is green:

```powershell
dotnet test Toolroom/WhenItFails/Setter.Tests
```

Latest user-verified result: **1,241 passed, 0 failed, 0 skipped**.

Run the current runtime/public-API focused command:

```powershell
dotnet test WhenItFails.Tests --filter FullyQualifiedName~ErrorCatalogContextProviderFailureIssueSeverityTests
```

Expected result: **3 green tests**.

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

Run `ErrorCatalogContextProviderFailureIssueSeverityTests`; the expected count is **3 green tests**.

If green, record verification and inspect exactly one adjacent synthesized-response contract outside provider-response metadata.

## Last completed change

The ninety-fourth runtime/public-API audit slice extends `ErrorCatalogContextProviderFailureIssueSeverityTests` with provider-response metadata coverage. A failed error provider returns meaningful response metadata and a warning source issue. `ErrorCatalogContextProvider` must synthesize a failed context response with no data, empty independent metadata, one normalized error issue, and unchanged source metadata. No production change is expected.

Commit:

```text
6683dad459916b26816f9ca80fb3226721c855fc
Suppress provider response metadata in context failures
```
