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
- `ErrorCatalogContextProviderFailureIssueSeverityTests`: **4 user-verified green** for severity normalization, source issue immutability, suppression of source-only issue fields, suppression of provider-response metadata, and the distinct `NotFound` flag contract.
- `ErrorCatalogContextProviderSuccessWithWarningsTests`: **2 user-verified green** confirming preservation of successful provider warnings, aggregation completeness, and provider-order stability; a third informational-diagnostic case is pending.
- Setter CI repair pair — `ImplementationStatusDocumentationTests` and `SuggestDocumentationKeyBracketTitleTests`: **2 user-verified green**.
- Complete `Toolroom/WhenItFails/Setter.Tests` suite: **1,241 user-verified green, 0 failed, 0 skipped**.

The runtime/public-API audit protects provider ordering and configured paths, short-circuiting, cancellation, exception identity and shape, null tasks, null responses, null payloads, cross-validation envelopes, provider-specific fallbacks, source-message and source-code selection, malformed envelopes, first-issue selection, later-issue suppression, synthesized issue severity, source object immutability, suppression of source-only failure fields, suppression of provider-response metadata, status-derived response flags, and preservation of diagnostics from successful providers.

`ResultStatus.NotFound` is intentionally context-neutral rather than automatically successful or failed. It may represent a successful lookup that found no matching item, or a wider operation that cannot complete because a required item was not found. The status therefore remains a distinct non-success category: `IsNotFound == true`, `IsSuccess == false`, and `IsFailure == false`. Severity and surrounding operation context communicate whether the overall outcome is problematic.

The successful-provider composition contract collects issues from all five successful catalog responses in provider order. Informational issues must survive composition without changing plain `Success` or making `HasWarnings` true. Warning-or-higher diagnostics must produce `SuccessWithWarnings`. In both cases the fully validated non-null context and original issue order are preserved. Failure short-circuit and cross-validation behavior are unchanged.

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

Run the expanded successful-provider diagnostic suite:

```powershell
dotnet test WhenItFails.Tests --filter FullyQualifiedName~ErrorCatalogContextProviderSuccessWithWarningsTests
```

Expected result: **3 tests**. The new informational case may expose an over-broad status promotion if every preserved issue currently forces `SuccessWithWarnings`.

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

Run `ErrorCatalogContextProviderSuccessWithWarningsTests`; the expected count is **3 tests**. Do not proceed until the informational-diagnostic status contract is verified.

## Last completed change

The one-hundred-first runtime/public-API audit slice records **2 user-verified green tests** for successful-provider warning aggregation and adds one focused informational-diagnostic case. A successful provider may attach an `Information` issue; the composed response must preserve it while remaining plain `Success` with `HasWarnings == false`. This distinguishes retained diagnostics from actual warning status and intentionally tests whether the current production aggregation promotes every issue too broadly.

Commits:

```text
24dd625047033cc8a285f07552421eeed23b5ab7
Preserve provider warnings in catalog context

a1ce12c505031b4a4780d6951c6a3d048885779a
Verify warning aggregation order

72434329977adcc3b8d406f593a249b3687076d2
Distinguish informational provider diagnostics
```
