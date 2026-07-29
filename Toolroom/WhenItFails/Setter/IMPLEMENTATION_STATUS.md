# Implementation status

Last updated: 2026-07-29

This file is the continuation point for `Toolroom/WhenItFails/Setter` development. Update it after every implementation, test, or documentation change that alters the current state or recommended next step.

## Current state

WhenItFails Setter is a mature .NET 10 command-line authoring and maintenance tool for the WhenItFails JSON catalog workspace under `Jsons/WhenItFails`.

Implemented areas include:

- workspace initialization, validation, summary, and reference inspection;
- error listing, filtering, detail inspection, creation, removal, and focused field editing;
- tags, aliases, ownership, categories, code groups, and documentation keys;
- profile creation, selectors, mappings, metadata, and explanation output;
- safe writes with timestamped backups, backup listing, and restore operations;
- rich, plain, and JSON output with stable exit-code conventions;
- documentation-key validation and local Markdown link checking.

## Verification status

The latest user-verified Setter test run reported **5,029 passed, 0 failed, 0 skipped**. The complete Setter suite is green after restoring the implementation-status continuation-point contract.

- Setter complete suite: **5,029 tests user-verified green**.
- Runtime `ErrorProfileSelectionServiceTests`: **10 focused tests user-verified green**.
- Runtime `ErrorCatalogRuntimeProfileDisplayNameTests`: **1 focused test user-verified green**.
- Runtime `ErrorProfileResolverTests`: **19 focused tests user-verified green**, including mapping-selection invariance.
- Runtime `ErrorProfileCatalogProviderMetadataTests`: **1 focused test user-verified green**.
- Runtime `ErrorCatalogContextProfileMetadataIntegrationTests`: **1 focused test user-verified green**, including writer/loader round-trip, provider/context preservation, metadata values, and normalized mappings.
- Runtime `ErrorProfileDefinitionNormalizerTests`: **10 focused tests user-verified green** after independent metadata, mapping, and selector-list copy semantics.
- Runtime `ErrorProfileCatalogDocumentNormalizerTests`: **8 focused tests user-verified green** after independent catalog metadata, profile collection, profile instance, and tag copy semantics.
- Runtime `ErrorProfileCatalogProviderTests`: **10 focused tests user-verified green**, including loader-to-provider payload isolation.
- Runtime `ErrorCatalogContextProviderProfileReferenceTests`: **2 focused tests user-verified green** for direct reuse of validated provider outputs, a newly computed combined `CrossValidationResult`, and provider-local validation-result isolation.
- Runtime `ErrorCatalogContextProviderShortCircuitTests`: **5 focused tests user-verified green**, covering failure at every provider boundary.
- Runtime `ErrorCatalogContextProviderCallOrderTests`: **1 focused test user-verified green** for exact provider order and configured path routing.
- Runtime `ErrorCatalogContextProviderNullPayloadShortCircuitTests`: **5 focused tests user-verified green**, covering null payloads at every provider boundary.
- Runtime `ErrorCatalogContextProviderCancellationPropagationTests`: **4 focused tests user-verified green**, covering every inter-provider cancellation boundary.
- Runtime `ErrorCatalogContextProviderPostProfileCancellationTests`: **1 focused test user-verified green** for cancellation after the profile provider and before cross-validation.
- Runtime `ErrorCatalogContextProviderCrossValidationFailureContractTests`: **1 focused test user-verified green** after correcting the test's issue-type reference.
- Runtime `ErrorCatalogContextProviderCrossValidationWarningTests`: **3 focused tests user-verified green** for warning-only, information-only, mixed non-error context construction, deterministic issue preservation, and a clean outer success envelope.
- Current error-selection slice adds one focused `ErrorCatalogContextProviderCrossValidationErrorSelectionTests` contract and changes production selection from the first issue to the first error-severity issue. This test is not yet user-verified.

Focused verification command for the current slice:

```powershell
dotnet test WhenItFails.Tests --filter FullyQualifiedName~ErrorCatalogContextProviderCrossValidationErrorSelectionTests
```

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

## Documentation synchronization completed

High-value Setter documentation synchronization is complete. The maintained English documentation covers the implemented command surface, architecture, testing, safe writes, backups, automation, contribution workflow, and maintainer continuation rules.

Primary synchronized documents include:

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

## Runtime/public-API audit

Completed audit slices:

1. Runtime profile lookup accepts normalized `Name` or `DisplayName`, aligned with Setter.
2. Setter `errors --profile`, `show-profile`, and `explain-profile` share one normalized lookup path.
3. Command-level JSON contracts protect normalized selectors, exit codes, envelopes, failure fields, and no-backup behavior.
4. Profile explanation reports include matches and final exclusion vetoes consistently with runtime selection.
5. `DefaultMappings` remain consumer recommendations and do not affect `ErrorProfileResolver` selection.
6. Profile metadata survives JSON load, normalization, validation, provider payload creation, safe writer/loader round-trip, and final `ErrorCatalogContext` construction.
7. `MetadataBag` keys are case-insensitive only; separator normalization is intentionally not applied.
8. `ErrorProfileDefinitionNormalizer` creates an independent `MetadataBag` copy.
9. `DefaultMappings` are independently copied and normalized.
10. All profile selector lists are independently copied and normalized.
11. `ErrorProfileCatalogDocumentNormalizer` copies catalog metadata instead of sharing mutable state with the source document.
12. The normalized `Profiles` collection and each normalized profile instance are independent from the source catalog.
13. Catalog `Tags` are independently copied and normalized.
14. `ErrorProfileCatalogProvider` returns the normalized document copy rather than exposing the mutable document instance supplied by the loader.
15. `ErrorCatalogContextProvider` intentionally aggregates already normalized and validated provider outputs without a second deep copy.
16. The aggregation contract covers `ErrorCatalog`, `ErrorCatalogDocument`, category, code-group, owner, and profile documents.
17. The final context receives a newly computed cross-catalog validation result rather than reusing any provider-local validation result.
18. Failure of the first error-catalog provider prevents category, code-group, owner, and profile providers from running and preserves the source issue code.
19. Category-provider failure preserves the source issue and prevents code-group, owner, and profile providers from running.
20. Code-group-provider failure preserves the source issue and prevents owner and profile providers from running.
21. Owner-provider failure preserves the source issue and prevents the profile provider from running.
22. Profile-provider failure preserves the source issue and returns no partial `ErrorCatalogContext`.
23. Exact provider invocation order and matching `JsonsOptions` path routing are protected.
24. Null-payload handling short-circuits immediately at every provider boundary.
25. Cooperative cancellation is protected at all four inter-provider transitions.
26. Cancellation after the profile provider returns is observed before cross-validation and context construction.
27. Cross-validation failure returns an `Invalid` response and exposes no partial context.
28. Provider-local validation results are intentionally isolated after successful provider responses; final success or failure uses a fresh cross-validation result computed from the returned documents.
29. Warning-only cross-validation issues remain available in the final `CrossValidationResult` but do not prevent successful context construction.
30. Information-only cross-validation issues remain available in the final `CrossValidationResult`, preserve `IsValid == true`, and do not prevent successful context construction.
31. Mixed non-error behavior preserves deterministic information-then-warning ordering inside `CrossValidationResult`, while the successful outer `Response` keeps an empty `Issues` collection.
32. The outer success envelope retains `ResultStatus.Success` and no message when non-error cross-validation diagnostics exist.
33. The current slice fixes mixed-severity failure selection: if information or warning issues precede an error, the invalid outer response now uses the first error-severity issue rather than blindly using `Issues[0]`.

## Current intentional boundaries

Setter currently does not provide:

- automatic schema migration;
- a multi-file atomic transaction;
- a multi-process locking contract;
- automatic backup-retention cleanup;
- complete localization lifecycle management;
- remote catalog synchronization;
- package publishing;
- a GUI or interactive TUI;
- complete source-code dependency discovery;
- a full security audit or semantic duplicate detector.

These are boundaries or future candidates, not undocumented defects.

## Working rules

- GitHub `master` is the source of truth.
- Keep each change narrow and intentional.
- Add or update tests immediately with each implementation or documentation change.
- Run the affected focused test project after every commit-sized change.
- Do not advance while the current test slice is red.
- Keep README and `Docs/<topic>/en.md` aligned with actual behavior.
- Update this file after every change.

## Recommended next step

First verify `ErrorCatalogContextProviderCrossValidationErrorSelectionTests`; the expected count is **1 green test**.

Next documentation target: keep this file synchronized while the runtime/public-API audit continues; no separate documentation expansion is currently required.

If green, inspect one adjacent failure-selection contract only, preferably whether a warning preceding an error is also skipped correctly, without broadening the slice.

Do not invent automatic `DefaultMappings` consumption.

## Last completed change

The forty-third runtime/public-API audit slice found and fixed a mixed-severity failure-selection defect. `ErrorCatalogCrossValidator` preserves discovery order, so an informational issue from an earlier error definition can precede a later error-level issue. `ErrorCatalogContextProvider` previously used `Issues[0]` and could therefore return an `Invalid` response carrying an informational code and message. It now selects the first issue whose severity is `Error` and falls back to the generic cross-validation failure only if no such issue exists.

Commits in this change sequence:

```text
8de94e172ebcdc1bfc487f60b5547ee40440f658
Protect first error selection after non-error diagnostics

8263d22b2b5f6d7f7094d19e03c7c81941f0945b
Select first cross-validation error issue
```
