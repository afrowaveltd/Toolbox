# Implementation status

Last updated: 2026-08-01

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
- `ErrorCatalogContextProviderNullFailureIssueElementTests`: **3 user-verified green** confirming null skipping, preservation of the first valid source code, preservation of first-actual-issue semantics when that issue has a blank code, and fallback behavior when every collection element is null.
- `ErrorCatalogContextProviderCategoryNullFailureIssuesTests`: **2 user-verified green** confirming both runtime-null failure collections and runtime-null issue elements at the category-provider position.
- `ErrorCatalogContextProviderCrossValidationWarningTests`: **5 user-verified green** confirming nested warning/information storage and strict separation of provider diagnostics from cross-validation diagnostics.
- `ErrorCatalogContextProviderProviderWarningBeforeCrossValidationErrorTests`: **4 user-verified green** confirming that a final cross-validation error replaces the complete earlier provider envelope, including issues, metadata, and message.
- `ErrorCatalogContextProviderSuccessfulEnvelopeSuppressionTests`: **2 user-verified green** confirming both plain `Success` and `SuccessWithWarnings` composition preserve provider issues while suppressing provider message and metadata.
- `ErrorCatalogContextProviderSuccessfulStatusNormalizationTests`: **3 user-verified green** confirming downward normalization from informational-only diagnostics, upward promotion from a real warning, and successful composition with a preserved `Error` diagnostic.
- `ErrorCatalogContextProviderEmptyWarningEnvelopeNormalizationTests`: **3 user-verified green** confirming empty, runtime-null, and null-only `SuccessWithWarnings` issue collections all normalize to plain `Success`.
- `ErrorCatalogContextProviderNullInnerPayloadTests`: **1 user-verified green** for `Document == null`; the first two-test run then failed exactly as expected for `Catalog == null`, and production now contains the matching guard pending rerun.
- Setter CI repair pair — `ImplementationStatusDocumentationTests` and `SuggestDocumentationKeyBracketTitleTests`: **2 user-verified green**.
- Complete `Toolroom/WhenItFails/Setter.Tests` suite: **1,241 user-verified green, 0 failed, 0 skipped**.

The runtime/public-API audit protects provider ordering and configured paths, short-circuiting, cancellation, exception identity and shape, null tasks, null responses, null payloads, cross-validation envelopes, provider-specific fallbacks, source-message and source-code selection, malformed envelopes, first-issue selection, later-issue suppression, synthesized issue severity, source object immutability, suppression of source-only failure fields, suppression of provider-response metadata, status-derived response flags, preservation of diagnostics from successful providers, and separation of provider diagnostics from cross-validation diagnostics.

`ResultStatus.NotFound` is intentionally context-neutral rather than automatically successful or failed. It may represent a successful lookup that found no matching item, or a wider operation that cannot complete because a required item was not found. The status therefore remains a distinct non-success category: `IsNotFound == true`, `IsSuccess == false`, and `IsFailure == false`. Severity and surrounding operation context communicate whether the overall outcome is problematic.

The successful-provider composition contract collects issues from all five successful catalog responses in provider order. Informational issues survive composition while the final response remains plain `Success` with `HasWarnings == false`. Any aggregated provider issue whose severity is `Warning` or higher promotes the final response to `SuccessWithWarnings`. Mixed lower- and warning-level provider diagnostics remain present and ordered while the highest relevant severity determines warning state. Runtime-null `Issues` collections are treated as empty across all five provider positions. Runtime-null elements inside a non-null collection are filtered during aggregation, while surrounding valid diagnostics retain their original order and identity. In every case the fully validated non-null context is preserved. Provider response messages and metadata describe the individual load operations rather than the newly composed context response, so they remain suppressed while provider issues are preserved. Final successful status is normalized from the aggregated issue severities rather than copied from any single provider envelope. Informational-only diagnostics normalize an inconsistent `SuccessWithWarnings` declaration to `Success`; a real warning promotes an inconsistent plain `Success` declaration to `SuccessWithWarnings`; and an explicitly successful provider response carrying an `Error` diagnostic remains a successful context composition represented as `SuccessWithWarnings`. Empty, runtime-null, and null-only warning-bearing issue collections are user-verified to normalize to plain `Success`.

Cross-validation diagnostics form a separate layer. Non-error cross-validation issues remain inside `ErrorCatalogContext.CrossValidationResult` and do not themselves populate outer `Response.Issues` or promote outer status. Provider warnings and information remain in the outer response while non-error cross-validation diagnostics remain solely inside the valid context, without duplication or loss. When cross-validation produces an error, no valid context exists; the final outer response becomes a fresh `Invalid` envelope with exactly the selected cross-validation error and omits the complete earlier provider envelope. `HasWarnings` nevertheless remains true because Essentials treats an attached `Error` as warning-or-higher severity; this flag does not imply that any provider warning survived.

The failure-envelope contract treats both a runtime-null `Issues` collection and runtime-null elements inside a non-null collection defensively. Failure mapping preserves source status and message, selects the first actual source issue after null elements, and uses its code only when non-blank. A blank code on that first actual issue triggers the provider-specific fallback rather than promoting a later issue code. A non-null collection containing no actual issues at all behaves exactly like an empty collection and produces the same fallback envelope. Later providers remain short-circuited.

A non-null provider payload can still violate its public contract internally. `ErrorCatalogProviderPayload.Document == null` is user-verified to return the existing `ErrorCatalogContextPayloadIsNull` invalid envelope before any later provider is called. The complementary `Catalog == null` test then demonstrated that production still called the category provider. `ErrorCatalogContextProvider` now checks `Data`, `Catalog`, and `Document` together immediately after the successful error-catalog response. Both missing required members therefore use the same invalid envelope and short-circuit point; the new runtime-catalog guard still requires user verification.

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

Rerun the repaired null inner-payload contract:

```powershell
dotnet test WhenItFails.Tests --filter FullyQualifiedName~ErrorCatalogContextProviderNullInnerPayloadTests
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

Rerun `ErrorCatalogContextProviderNullInnerPayloadTests`; the expected count is **2 green tests**. Do not proceed until both required error-catalog payload members, `Document` and `Catalog`, are confirmed to return `Invalid` with `ErrorCatalogContextPayloadIsNull`, keep `Data == null`, and prevent every later provider from being called.

## Last completed change

The one-hundred-thirty-third runtime/public-API audit slice records a two-test focused run with **1 green and 1 expected red**. The existing `Document == null` guard remained green, while `Catalog == null` still allowed the category provider to run. Production now extends the same early payload-integrity check to `errorCatalogResponse.Data.Catalog`. The repair is committed and the two-test class remains the verification gate before further work.

Commits:

```text
fca00ebcbbdfcdf76b170fcd7ca4741fc6348b8c
Verify null runtime catalog inner payload

05a6f3a7b8a8757ab0d4827f95108633302d57c8
Reject null error catalogs before composition
```
