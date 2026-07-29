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
- Runtime `ErrorCatalogContextProviderCrossValidationErrorSelectionTests`: **2 focused tests user-verified green** for selecting the first error after earlier information or warning diagnostics.
- Runtime `ErrorCatalogContextProviderInputOrderingTests`: **9 focused tests user-verified green** for method-entry ordering, constructor guards, deterministic multi-null precedence, and successful construction without provider execution.
- Runtime `ErrorCatalogContextProviderProviderExceptionPropagationTests`: **4 focused tests user-verified green** for transparent exception propagation at the error, category, code-group, and owner provider boundaries.
- Current profile-boundary exception slice adds a fifth focused test to `ErrorCatalogContextProviderProviderExceptionPropagationTests`; the next successful focused run is expected to report **5 tests**.

Focused verification command for the current slice:

```powershell
dotnet test WhenItFails.Tests --filter FullyQualifiedName~ErrorCatalogContextProviderProviderExceptionPropagationTests
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
33. Mixed-severity failure selection uses the first error-severity issue when information diagnostics precede it.
34. The same failure-selection contract is protected when warning diagnostics precede the first error.
35. A pre-cancelled token is observed before null `JsonsOptions` validation, and no provider is invoked.
36. With a live token, null `JsonsOptions` produces `ArgumentNullException` with parameter name `options` before any provider invocation.
37. Constructor dependency validation protects exact `ArgumentNullException.ParamName` values for all five providers.
38. When multiple dependencies are null, the first declared constructor parameter is reported.
39. Five valid dependencies construct the context provider without invoking provider work.
40. An exception thrown by the first provider escapes unchanged and prevents all later provider calls rather than being converted into a `Response` failure.
41. Exception transparency at the category boundary preserves the exact thrown instance and prevents code-group, owner, and profile provider calls.
42. Exception transparency at the code-group boundary preserves the exact thrown instance and prevents owner and profile provider calls.
43. Exception transparency at the owner boundary preserves the exact thrown instance and prevents profile provider execution.
44. The current slice completes provider exception transparency at the profile boundary: the first four providers succeed, the profile provider's exact exception escapes unchanged, and no `ErrorCatalogContext` or response envelope is created.

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

First verify `ErrorCatalogContextProviderProviderExceptionPropagationTests`; the expected count is **5 green tests**.

Next documentation target: keep this file synchronized while the runtime/public-API audit continues; no separate documentation expansion is currently required.

If green, the provider-exception boundary sequence is complete. Inspect one adjacent exception-shape contract only, preferably whether a non-`InvalidOperationException` subtype is also propagated unchanged rather than being normalized.

Do not invent automatic `DefaultMappings` consumption.

## Last completed change

The fifty-fourth runtime/public-API audit slice completes exception transparency across all five provider boundaries. After the error, category, code-group, and owner providers return valid payloads, an exception thrown by the profile provider is propagated as the same exception instance. The context provider does not translate it into a `Response` failure and therefore constructs no partial or final `ErrorCatalogContext`.

Commit in this change sequence:

```text
23c5749e15a5327dfd6493ec6b0a9a798163dbf8
Protect profile provider exception propagation
```
