# Public API inventory and 1.0 decision register

Status: **pre-1.0 inventory; classification candidates, not a published 1.0 compatibility promise**.

The current support-level decision is recorded in [the candidate 1.0 public API policy](../Public-API-1.0-Policy/en.md). This inventory remains evidence/input for that policy rather than a second competing classification.
Source of truth: current `afrowaveltd/Toolbox` GitHub `master`. The original pre-cleanup census was **148 exported types / 830 API entries**. Commit `6afd3f6f88c7d71097fd1813d8e0dc2d9d80b276` internalized 17 DI-only default implementation classes and its resulting complete suite is maintainer-confirmed **1474/1474 GREEN**. Commit `147b96310090c6cbbb5f2a5819b440026fb6062b` now applies a second 5-type helper cleanup; its verification and the final post-cleanup exported/API counts are pending.

## 1. Application-facing stable-contract candidates

- Entry points: `IErrorCatalogRuntime` and the four `AddWhenItFails(...)` overloads.
- Published configuration and error models: `WhenItFailsOptions`, `JsonsOptions`, `ErrorDefinition`, `ErrorDescriptor`, `ErrorCatalogContext`, and `ErrorCatalogRuntimeStatus`.
- Read-only lookup interface: `IErrorCatalog`. Its collection-returning methods expose a read-only **collection interface**, not deeply immutable `ErrorDefinition` instances.
- User-visible statuses and configuration values: `ErrorCatalogContextSource`, `ErrorCatalogRuntimeState`, `ErrorCatalogInitializationMode` and the `ResultStatus` type supplied by Essentials.
- `ErrorDescriptor<TAttachment>` and `ErrorDescriptorRequest` are separately public concrete model types; review actual consumer use and JSON annotations before classifying them as stable API or optional convenience models.

All previously verified contract tests are recorded in [the baseline review](../Public-API-Stability/en.md). A verified source declaration by itself is not a promise of future binary, source or serialization compatibility.

## 2. Transitive public models: explicit review required

These types appear in public entry-point, extension-interface or public-model signatures. They **cannot be dismissed as private implementation details** solely because they live below `Bootstrap`, `Catalog`, `Definitions` or `Validation`.

| Public type(s) | Exposed by | Review focus |
| --- | --- | --- |
| `ErrorCatalogInitializationPayload` | `IErrorCatalogRuntime.InitializeAsync` / `ResetToDefaultsAsync`; `IErrorCatalogInitializer` | Context/source/fallback and bootstrap object properties, defaults, nullability |
| `JsonsBootstrapPayload`, `JsonsBootstrapFileResult` | Initialization payload / `IJsonsBootstrapper` | Mutable `Files` collection, path/status properties |
| `ErrorCatalogDocument`, `ErrorCategoryCatalogDocument`, `ErrorOwnerCatalogDocument`, `ErrorCodeGroupCatalogDocument`, `ErrorProfileCatalogDocument` | Public loaders, providers, normalizers, validators, context | JSON schema, property names/defaults and mutable collections |
| `ErrorCategoryDefinition`, `ErrorOwnerDefinition`, `ErrorCodeGroupDefinition`, `ErrorProfileDefinition` | Catalog documents and `IErrorProfileResolver` | Field meanings, JSON versioning, copied vs shared metadata |
| `ErrorCatalogProviderPayload`, four specialized `Error...CatalogProviderPayload` types | Five provider interfaces | Document and validation result properties; main payload also exposes `IErrorCatalog` |
| `ErrorCatalogValidationResult`, `ErrorCatalogValidationIssue`, `ErrorCatalogValidationSeverity` | Validators, providers, context | Issue list mutation, severity values, validity semantics |
| `JsonsTemplateFile` | `IJsonsTemplateProvider` | Template name/path/content contract |

These declarations were checked against GitHub source at the inventory checkpoint. Prioritize `ErrorCatalogInitializationPayload` and bootstrap payloads in the next **small contract-test group**, then the document/definition schema and provider/validation result types. Do not freeze all properties in one unreviewed bulk reflection test.

## 3. DI extension points

There are 31 interface source files in `WhenItFails/Interfaces/`. All 31 now have at least a first method/property-shape contract review in `WhenItFails.Tests/PublicApi`. The final built-in-provider group is confirmed in the complete 1218/1218 GREEN suite.

The final interface added to this baseline is `IBuiltInErrorCatalogContextProvider`. It exposes `LoadAsync(CancellationToken = default)` returning `Task<Response<ErrorCatalogContext>>`, and its source DI registration uses `TryAddSingleton`. `BuiltInCatalogContextProviderPublicApiContractTests` verifies the signature and prior-registration precedence without executing the provider's temporary filesystem workflow.

Existing DI override tests prove `TryAddSingleton` precedence for the covered interfaces. They **do not** prove that all third-party implementations preserve full recovery, cancellation, data isolation or validation semantics. Treat the candidate classification as supported replaceability under review, not as permission to change arbitrary dependencies without contract tests.

## 4. Intentional implementation visibility before 1.0

The first stable-surface cleanup internalizes 17 default orchestration classes that
are supplied through public DI interfaces rather than intended for direct consumer
construction:

- `BuiltInErrorCatalogContextProvider`, `ErrorCatalog`,
  `ErrorCatalogContextProvider`, `ErrorCatalogFactory`, `ErrorCatalogProvider`;
- the four specialized catalog providers;
- `ErrorDefinitionResolver`, `ErrorCatalogInitializer`,
  `ErrorProfileSelectionService`;
- `ErrorDescriptorFactory`, `ErrorDescriptorResolver`,
  `ErrorDescriptorService`;
- `ErrorCatalogContextStore` and `ErrorCatalogRuntime`.

The second cleanup in `147b9631` additionally internalizes
`JsonCatalogDocumentLoader`, `ErrorDefinitionNormalizer`,
`ErrorCategoryDefinitionNormalizer`, `ErrorCodeGroupDefinitionNormalizer` and
`ErrorOwnerDefinitionNormalizer`. Public typed loaders and document normalizers
retain their parameterless constructors; only their helper-injection constructors
become internal.

This is an intentional pre-1.0 breaking cleanup. There are no external consumers and
the package has never been published, so the historical local 0.1.0 binary is not a
compatibility target.

The cleanup deliberately **does not** bulk-hide lower-level types used directly by
Toolbox Setter. `ErrorProfileResolver`, selected JSON loaders/normalizers,
`TextKeyNormalizer`, `JsonCatalogDocumentWriter`,
`DocumentationKeyGenerator`, `DocumentationKeyFormat` and
`ErrorCatalogCrossValidator` remain subject to explicit tooling/API review.

Verified internal helper declarations also include `CatalogProviderPipeline`,
`DefinitionNormalizationHelper` and `CatalogValidationHelper`. A public member on
an internal containing type is not exported CLR API.

## 5. Current 1.0 decisions and remaining gate

1. `GetCurrentContext()` intentionally returns the live shared context; consumers
   treat it as read-only by convention and use detached snapshots when isolation is
   required.
2. Persistent project catalog JSON is versioned separately from CLR compatibility.
   The stable catalog baseline is `schemaVersion: "1.0"`; unsupported non-empty
   versions are rejected.
3. Public interfaces remain the supported DI replacement seams; their default
   orchestration implementations are internal unless direct construction is
   intentionally supported.
4. The pre-cleanup census **148 / 830** is historical input only. The first 17-type
   cleanup is test-verified; the second 5-type helper cleanup is now committed. The
   authoritative final type/member counts must come from the compiled inventory and
   `-SourceOnly` comparer after the second batch is GREEN.
5. The unpublished local 0.1.0 artifact is optional historical diagnostics, not a
   first-release compatibility gate.
6. The complete suite is confirmed **1474/1474 GREEN** for the first cleanup. The
   second cleanup changes existing tests but adds no new test, so the expected suite
   total remains **1474**; verification is pending.

Next: verify the second cleanup, measure the resulting public surface, then review the
remaining directly constructed tooling/low-level types before freezing 1.0.


## 6. Additive snapshot API review — 2026-09-25

The original runtime interface still declares exactly nine methods. Optional publication/activation/combined/supporting/full readers and the snapshot extension families remain separate public capabilities; internal capture helpers remain implementation details. Their previously verified contracts are part of the **1473/1473 GREEN** pre-cleanup baseline. See [snapshot capability boundaries](../Pre-1.0-Snapshot-Capability-Boundaries/en.md). The new visibility cleanup does not alter those public interfaces or snapshot models; post-cleanup verification is pending.


## 7. Pre-cleanup export baseline and post-cleanup remeasurement

Immediately before the stable-surface cleanup, the current source measured **148 exported types / 830 API entries**. Commit `6afd3f6f` internalizes 17 default implementation types, so that census is now the **pre-cleanup reference** rather than the intended 1.0 baseline. Rerun both the compiled exported-assembly inventory and the `-SourceOnly` comparer after the new suite is GREEN; record their actual resulting counts rather than deriving API entries arithmetically. Historical 0.1.0 package comparisons remain optional context only. See [the current inventory procedure](../Current-Public-API-Inventory/en.md).


## 8. Classification of the 38 source-only exported CLR types

The maintainer supplied the complete **38-name source-only type list** from the current package comparer. It comprises **2 store infrastructure interfaces, 5 optional runtime readers, 2 publication/activation infrastructure models, 19 detached catalog/validation/observation models and 10 additive snapshot extension classes**. See [complete classification](../Added-Public-Types-Classification/en.md). This is a provisional usage classification, not a visibility change by itself. The ownership groups are now reflected in the candidate 1.0 policy, the refreshed current inventory remains at **148 exported types**, and the complete suite is **1473/1473 GREEN**. The exact member-entry delta still requires the original 0.1.0 artifact for refresh.


## 9. Source-only additions to existing concrete classes

A maintainer-supplied filtered comparer output identifies **14 API census entries on the two original classes**: `ErrorCatalogContextStore` (2 interface entries, 2 methods) and `ErrorCatalogRuntime` (5 interface entries, 5 methods). The other **205 entries** belong to the 38 new exported types; these counts describe census entries, not exclusively methods. The `LegacyConcretePublicationExpansionContractTests` protect original constructors and interface surfaces, while the exported-inventory tests protect the current assembly census. Both groups are included in the maintainer-confirmed **1473/1473 GREEN** complete suite. See [the 14-item inventory](../Original-Type-API-Additions/en.md).
