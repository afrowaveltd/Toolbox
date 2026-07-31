# Implementation status

Last updated: 2026-07-31

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
- `ErrorCatalogContextProviderSuccessWithWarningsTests`: **5 user-verified green** confirming warning preservation, provider-order aggregation, informational diagnostics, mixed-severity preservation, and defensive handling of a runtime-null `Issues` collection.
- `ErrorCatalogContextProviderNullIssueElementTests`: **1 user-verified green** confirming that runtime-null issue elements are filtered without losing surrounding valid diagnostics.
- `ErrorCatalogContextProviderNullFailureIssuesTests`: **1 user-verified green** confirming that a runtime-null failure `Issues` collection uses the provider fallback envelope without invoking later providers.
- `ErrorCatalogContextProviderNullFailureIssueElementTests`: **1 user-verified green** confirming that failure mapping skips runtime-null issue elements and preserves the first valid source issue code.
- `ErrorCatalogContextProviderCategoryNullFailureIssuesTests`: **1 user-verified green** for a runtime-null failure collection at the category-provider position; a second null-element-plus-valid-source-code case is pending.
- Setter CI repair pair — `ImplementationStatusDocumentationTests` and `SuggestDocumentationKeyBracketTitleTests`: **2 user-verified green**.
- Complete `Toolroom/WhenItFails/Setter.Tests` suite: **1,241 user-verified green, 0 failed, 0 skipped**.

The runtime/public-API audit protects provider ordering and configured paths, short-circuiting, cancellation, exception identity and shape, null tasks, null responses, null payloads, cross-validation envelopes, provider-specific fallbacks, source-message and source-code selection, malformed envelopes, first-issue selection, later-issue suppression, synthesized issue severity, source object immutability, suppression of source-only failure fields, suppression of provider-response metadata, status-derived response flags, and preservation of diagnostics from successful providers.

`ResultStatus.NotFound` is intentionally context-neutral rather than automatically successful or failed. It may represent a successful lookup that found no matching item, or a wider operation that cannot complete because a required item was not found. The status therefore remains a distinct non-success category: `IsNotFound == true`, `IsSuccess == false`, and `IsFailure == false`. Severity and surrounding operation context communicate whether the overall outcome is problematic.

The successful-provider composition contract collects issues from all five successful catalog responses in provider order. Informational issues survive composition while the final response remains plain `Success` with `HasWarnings == false`. Only an aggregated issue whose severity is `Warning` or higher promotes the final response to `SuccessWithWarnings`. Mixed lower- and warning-level diagnostics remain present and ordered while the warning determines the final status. Runtime-null `Issues` collections are treated as empty across all five provider positions. Runtime-null elements inside a non-null collection are filtered during aggregation, while surrounding valid diagnostics retain their original order and identity. In every case the fully validated non-null context is preserved.

The failure-envelope contract treats both a runtime-null `Issues` collection and runtime-null elements inside a non-null collection defensively. Failure mapping preserves source status and message, selects the first valid source issue code when available, otherwise uses the provider-specific fallback code, and keeps later providers short-circuited. The same generic behavior must hold at every provider position, not only for the first error-catalog provider.

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

Run the expanded category-provider malformed-failure suite:

```powershell
dotnet test WhenItFails.Tests --filter FullyQualifiedName~ErrorCatalogContextProviderCategoryNullFailureIssuesTests
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

Run `ErrorCatalogContextProviderCategoryNullFailureIssuesTests`; the expected count is **2 green tests**. Do not proceed until both null-collection and null-element behavior are verified at the category-provider position.

## Last completed change

The one-hundred-twelfth runtime/public-API audit slice records the category-provider null-collection case as **1 user-verified green test** and adds a second generic-position verification. The error-catalog provider succeeds, then the category provider returns `Failed` with a source message and `Issues = [null, validIssue]`. Failure mapping must skip the malformed element, preserve the first valid source code and source message, retain `Failed`, and short-circuit all later providers. No production change is expected because the shared generic failure mapper is already corrected.

Commits:

```text
dfcc324a9e038ef78997e8d9a0c041e5cac659b0
Verify category null failure issues fallback

fe8636534a3ddc7e0de95efffb2129b0118a1335
Verify category null failure issue elements
```
