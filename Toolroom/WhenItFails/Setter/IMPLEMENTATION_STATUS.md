# Implementation status

Last updated: 2026-08-08

This file is the continuation point for `Toolroom/WhenItFails/Setter` development. Update it after every implementation, test, catalog, or documentation change that alters the current state or recommended next step.

## Current state

WhenItFails Setter is a mature .NET 10 command-line authoring and maintenance tool for the project-local catalogs under `Jsons/WhenItFails`.

The `AFW_THM_0012` slice is complete. The runtime/public-API audit now protects bootstrap DTOs, validation contracts, stable enum values, descriptor and definition models, provider payloads, catalog context and initialization payloads, `CatalogProviderPipeline`, `ErrorCatalog`, `ErrorCatalogFactory`, `ErrorDefinitionResolver`, `ErrorDescriptorFactory`, `ErrorDescriptorResolver`, `ErrorDescriptorService`, `ErrorCatalogRuntime`, `ErrorProfileSelectionService`, `ErrorCatalogContextProvider`, `ErrorCatalogProvider`, `BuiltInErrorCatalogContextProvider`, `ErrorCatalogInitializer`, and `JsonsBootstrapper` malformed dependency-result boundaries. The shared catalog provider pipeline also rejects runtime-null loader responses, normalizer results, validator results, and payload-factory results, protecting the category, code-group, owner, and profile providers through one common contract. `ErrorProfileResolver` normalization is now directly contract-tested across separator, whitespace, casing, include, and exclusion/veto paths.

The current user-verified complete regression baselines are fully green:

- complete `Toolroom/WhenItFails/Setter.Tests`: **1,241 passed, 0 failed, 0 skipped**;
- complete `WhenItFails.Tests`: **780 passed, 0 failed, 0 skipped**.

## Verification status

The latest user-verified Setter test run:

```powershell
dotnet test Toolroom/WhenItFails/Setter.Tests
```

Result: **1,241 passed, 0 failed, 0 skipped**.

The latest complete core test run:

```powershell
dotnet test WhenItFails.Tests
```

Result: **780 passed, 0 failed, 0 skipped**.

Completed runtime/public-API focused checkpoints include:

- bootstrap value and payload contracts: **7 passed**;
- validation issue, result, and severity contracts: **15 passed**;
- catalog context source and runtime state contracts: **8 passed**;
- descriptor contract family: **7 passed**;
- definition and catalog-document contract family: **16 passed**;
- provider-payload contract family: **10 passed**;
- `ErrorCatalogContextContractTests`: **2 passed**;
- `CatalogProviderPipelineTests`: **10 passed**;
- `CatalogProviderPipelineNullResultContractTests`: **4 passed**;
- `ErrorCatalogInitializationPayloadContractTests`: **5 passed**;
- `ErrorCatalogSnapshotContractTests`: **4 passed**;
- `ErrorCatalogEmptyLookupContractTests`: **2 passed**;
- `ErrorCatalogNonPositiveCodeContractTests`: **2 passed**;
- `ErrorCatalogDuplicateSingleValueKeyContractTests`: **3 passed**;
- `ErrorCatalogMultiValueIndexContractTests`: **2 passed**;
- `ErrorCatalogMultiValueNormalizationContractTests`: **6 passed**;
- `ErrorCatalogUnusableSingleValueLookupContractTests`: **6 passed**;
- `ErrorCatalogFactoryContractTests`: **2 passed**;
- `ErrorDefinitionResolverInputBoundaryContractTests`: **5 passed**;
- `ErrorDefinitionResolverResponseShapeContractTests`: **3 passed**;
- `ErrorDescriptorFactoryContractTests`: **3 passed**;
- `ErrorDescriptorFactoryNullBoundaryContractTests`: **2 passed**;
- `ErrorDescriptorResolverFallbackContractTests`: **2 passed**;
- `ErrorDescriptorServiceNullResponseContractTests`: **3 passed**;
- `ErrorCatalogRuntimeNullContextPayloadContractTests`: **4 passed**;
- `ErrorCatalogRuntimeNullDownstreamResponseContractTests`: **4 passed**;
- `ErrorCatalogRuntimeNullInitializationDependencyResponseContractTests`: **2 passed**;
- `ErrorCatalogRuntimeNullContextStoreResponseContractTests`: **5 passed**;
- `ErrorCatalogRuntimeNullFlexibleFallbackResponseContractTests`: **1 passed**;
- `ErrorCatalogRuntimeNullInitializationPayloadContractTests`: **1 passed**;
- `ErrorCatalogRuntimeInvalidInitializationPayloadContractTests`: **2 passed**;
- `ErrorProfileSelectionServiceNullResolverResultContractTests`: **1 passed**;
- `ErrorCatalogContextProviderNullDependencyResponseContractTests`: **5 passed**;
- `ErrorCatalogProviderNullDependencyResultContractTests`: **4 passed**;
- `BuiltInErrorCatalogContextProviderNullContextResponseContractTests`: **1 passed**;
- `ErrorCatalogInitializerNullDependencyResponseContractTests`: **2 passed**;
- `JsonsBootstrapperNullTemplateResultContractTests`: **2 passed**;
- `ErrorProfileResolverNormalizationContractTests`: **8 passed**.

The catalog contracts protect source-sequence snapshots, ordering, exact object identity, read-only collection exposure, safe empty and unusable lookups, non-positive numeric indexing, deterministic duplicate-key handling, multi-value ordering and de-duplication, and consistent normalization. The factory creates a usable empty catalog and snapshots `document.Errors` at creation time. The definition resolver returns stable responses for invalid, not-found, and successful paths. The descriptor factory safely maps and owns runtime values. The descriptor resolver preserves source failure status and uses stable fallbacks for malformed failures. The descriptor service converts runtime-null resolver responses into stable `Invalid` responses. The runtime rejects context-store `Success` responses with null context data, normalizes a runtime-null context-store response to `WIF_CONTEXT_STORE_RESPONSE_NULL`, converts runtime-null descriptor/profile responses into stable `Invalid` responses, converts runtime-null initializer or built-in provider responses into stable failures, preserves the existing `WIF_DEFAULT_FALLBACK_FAILED` recovery contract when the automatic flexible fallback provider itself returns runtime null, rejects initializer `Success` responses whose initialization payload is null as `WIF_INITIALIZATION_PAYLOAD_NULL`, and rejects successful payloads whose required `Bootstrap` or `Context` value is null as `WIF_INITIALIZATION_BOOTSTRAP_NULL` or `WIF_INITIALIZATION_CONTEXT_NULL`. `ErrorProfileSelectionService` rejects a runtime-null `IErrorProfileResolver.Resolve()` result as `WIF_PROFILE_RESOLVER_RESULT_NULL` instead of returning `Ok(null)`. `ErrorCatalogContextProvider` rejects runtime-null responses from all five composed catalog providers with provider-specific stable `Invalid` codes rather than dereferencing null. `ErrorCatalogProvider` rejects runtime-null loader responses, normalizer results, validator results, and factory results with layer-specific stable `Invalid` responses instead of throwing or returning a malformed success payload. `CatalogProviderPipeline` applies the same protection generically to its loader, normalizer, validator, and payload-factory steps. `BuiltInErrorCatalogContextProvider` rejects a runtime-null response from its delegated `IErrorCatalogContextProvider` as `WIF_BUILT_IN_CONTEXT_PROVIDER_RESPONSE_NULL` instead of allowing a null public response to escape. `ErrorCatalogInitializer` rejects runtime-null responses from the JSON bootstrapper and catalog context provider as `WIF_INITIALIZER_BOOTSTRAPPER_RESPONSE_NULL` and `WIF_INITIALIZER_CONTEXT_PROVIDER_RESPONSE_NULL`, while leaving the existing context store unchanged. `JsonsBootstrapper` now rejects a runtime-null template collection and null template items as `WIF_JSONS_TEMPLATE_COLLECTION_NULL` and `WIF_JSONS_TEMPLATE_ITEM_NULL` instead of throwing `NullReferenceException`. `ErrorProfileResolver` applies the same `TextKeyNormalizer` semantics to both profile filters and error-definition values; focused normalization tests confirm separator, whitespace, casing, include, and exclusion/veto consistency.

Numeric compatibility is directly protected for validation severity, catalog context source, and runtime state. `ErrorCatalogInitializationMode` remains protected by the existing numeric-value theory in `WhenItFailsOptionsTests`.

Other verified gates for the completed thermal slice:

- focused `ThermalFallbackProtectionActionUnverifiedCatalogTests`: **1 passed**;
- catalog validation after `AFW_THM_0012`: user-verified green;
- documentation-key validation: user-verified green;
- Markdown-link validation: user-verified green;
- `git diff --check`: user-verified clean.

The bundled reference catalog contains **17 categories**, **10 code groups**, **5 profiles**, and **37 bundled reference errors**. The project-local authoritative catalog additionally contains twelve thermal definitions through `AFW_THM_0012`.

## Thermal catalog state

The thermal domain uses category `THERMAL`, code group `THERMAL`, prefix `THM`, and range `1000000–1099999`.

The current thermal contracts run from `AFW_THM_0001` through `AFW_THM_0012`. `AFW_THM_0011` represents confirmed failure of a distinct approved fallback. `AFW_THM_0012` represents a distinct approved fallback that was initiated but whose required result remains unverified.

## Documentation synchronization completed

The English documentation remains synchronized in the project root and topic-based `Docs` folders. Only `en.md` localized documents are maintained manually at this stage.

Current synchronized documentation includes:

- `README.md` and `Readme/en.md`;
- `Docs/Overview/en.md`;
- `Docs/Commands/en.md`;
- `Docs/Known Limitations/en.md`;
- `Docs/Roadmap and Future Work/en.md`;
- `Docs/Getting-Started/en.md`;
- `Docs/FAQ/en.md`;
- `Docs/Testing and CI/en.md`;
- `Docs/Reviewing Catalog Changes/en.md`;
- `Docs/Catalog Author Checklist/en.md`;
- `WhenItFails/Docs/Bootstrap/en.md`;
- `WhenItFails/Docs/Thermal Errors/en.md`.

## Current intentional boundaries

Setter currently does not provide automatic schema migration, multi-file atomic transactions, multi-process locking, automatic translation generation, remote catalog synchronization, package publishing, a GUI, complete source-code dependency discovery, or automatic runtime behavior for humorous message variants.

## Working rules

- GitHub `master` is the source of truth.
- Keep each change narrow and intentional.
- Add or update tests immediately.
- Do not advance while the current slice is red.
- Update this file after every change.
- Prefer one file plus its directly related test, then commit.
- Match command examples to the user's current PowerShell environment.
- Use the user's `to-clipboard` helper whenever a long local output or file content is needed.

## Recommended next step

Continue the runtime/public-API audit beyond bootstrap initialization and profile-resolution normalization. Prefer uncovered public services or stateful composition boundaries with meaningful failure/status semantics; avoid duplicating already-green context-store invariants or synthetic null permutations already structurally protected upstream.

Prefer one narrow contract with a clear public response shape. Preserve the complete **780-test** core baseline until the next full regression run.

## Last completed change

`ErrorProfileResolverNormalizationContractTests` passed all **8 focused tests**. Profile include and exclusion/veto filters now have direct regression coverage proving the same separator, whitespace, casing, and key normalization semantics used elsewhere in the catalog API.