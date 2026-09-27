# Public API inventory and 1.0 decision register

Status: **frozen first-stable 1.0 inventory; 126 exported types / 698 API entries, guarded by the exact exported-type manifest contract**.

The current support-level decision is recorded in [the 1.0 public API policy](../Public-API-1.0-Policy/en.md). This inventory remains evidence/input for that policy rather than a second competing classification.
Source of truth: current `afrowaveltd/Toolbox` GitHub `master`. The original pre-cleanup census was **148 exported types / 830 API entries**. Commits `6afd3f6f88c7d71097fd1813d8e0dc2d9d80b276` and `147b96310090c6cbbb5f2a5819b440026fb6062b` internalized 22 implementation/helper types in two reviewed batches. The post-cleanup source-only comparer measured the frozen **126 exported types / 698 API entries** surface, and the exact exported-type manifest plus the complete **1475/1475 GREEN** suite lock that first-stable baseline.

## 1. Application-facing stable contracts

- Entry points: `IErrorCatalogRuntime` and the four `AddWhenItFails(...)` overloads.
- Published configuration and error models: `WhenItFailsOptions`, `JsonsOptions`, `ErrorDefinition`, `ErrorDescriptor`, `ErrorCatalogContext`, and `ErrorCatalogRuntimeStatus`.
- Read-only lookup interface: `IErrorCatalog`. Its collection-returning methods expose a read-only **collection interface**, not deeply immutable `ErrorDefinition` instances.
- User-visible statuses and configuration values: `ErrorCatalogContextSource`, `ErrorCatalogRuntimeState`, `ErrorCatalogInitializationMode` and the `ResultStatus` type supplied by Essentials.
- `ErrorDescriptor<TAttachment>` and `ErrorDescriptorRequest` are supported public concrete model types. Their focused public-API/serialization contracts are part of the accepted 1.0 surface.

All previously verified contract tests are recorded in [the baseline review](../Public-API-Stability/en.md). A verified source declaration by itself is not a promise of future binary, source or serialization compatibility.

## 2. Transitive public models

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

These declarations were subsequently covered by focused public-API/default/nullability/serialization contracts as appropriate and accepted into the frozen 1.0 surface. Persistent catalog JSON compatibility remains governed separately by `schemaVersion: "1.0"`.

## 3. DI extension points

There are 31 interface source files in `WhenItFails/Interfaces/`. All 31 now have at least a first method/property-shape contract review in `WhenItFails.Tests/PublicApi`. The final built-in-provider group is confirmed in the complete 1218/1218 GREEN suite.

The final interface added to this baseline is `IBuiltInErrorCatalogContextProvider`. It exposes `LoadAsync(CancellationToken = default)` returning `Task<Response<ErrorCatalogContext>>`, and its source DI registration uses `TryAddSingleton`. `BuiltInCatalogContextProviderPublicApiContractTests` verifies the signature and prior-registration precedence without executing the provider's temporary filesystem workflow.

Existing DI override tests prove `TryAddSingleton` precedence for the covered interfaces. They **do not** prove that all third-party implementations preserve full recovery, cancellation, data isolation or validation semantics. Treat these interfaces as supported replacement seams under the documented contracts, not as permission to change arbitrary dependencies without compatibility review and tests.

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
4. The pre-cleanup census **148 / 830** is historical input only. Both visibility
   cleanup batches are verified. The authoritative first-stable census is
   **126 exported types / 698 API entries**, measured by the source-only comparer and
   protected by the exact exported-type manifest.
5. The unpublished local 0.1.0 artifact is optional historical diagnostics, not a
   first-release compatibility gate.
6. The complete release suite is maintainer-confirmed **1475/1475 GREEN**. The
   remaining exported types were reviewed against repository usage and accepted into
   the frozen first-stable surface.

Next release work is artifact identity, clean-build verification, tagging and first
publication; no additional public-surface cleanup is pending for 1.0.


## 6. Additive snapshot API review — 2026-09-25

The original runtime interface still declares exactly nine methods. Optional publication/activation/combined/supporting/full readers and the snapshot extension families remain separate public capabilities; internal capture helpers remain implementation details. Their earlier contracts were part of the **1473/1473 GREEN** pre-cleanup baseline and remain covered by the final **1475/1475 GREEN** release suite. See [snapshot capability boundaries](../Pre-1.0-Snapshot-Capability-Boundaries/en.md). The visibility cleanup does not alter those public interfaces or snapshot models.


## 7. Pre-cleanup export baseline and post-cleanup remeasurement

Immediately before the stable-surface cleanup, the current source measured **148 exported types / 830 API entries**. After both cleanup batches, the source-only comparer measured **126 exported types / 698 API entries**. That post-cleanup measurement is the frozen 1.0 baseline and is guarded by the exact exported-type manifest contract. Historical 0.1.0 package comparisons remain optional context only. See [the current inventory procedure](../Current-Public-API-Inventory/en.md).


## 8. Classification of the 38 source-only exported CLR types

The maintainer supplied the complete **38-name source-only type list** from the current package comparer. It comprises **2 store infrastructure interfaces, 5 optional runtime readers, 2 publication/activation infrastructure models, 19 detached catalog/validation/observation models and 10 additive snapshot extension classes**. See [complete classification](../Added-Public-Types-Classification/en.md). This section records the historical pre-cleanup classification that informed the two visibility batches. The resulting first-stable surface is now frozen at **126 exported types / 698 API entries** with the complete **1475/1475 GREEN** suite. Re-running the exact original 0.1.0 comparison remains optional historical diagnostics and is not a 1.0 release gate.


## 9. Source-only additions to existing concrete classes

A maintainer-supplied filtered comparer output identifies **14 API census entries on the two original classes**: `ErrorCatalogContextStore` (2 interface entries, 2 methods) and `ErrorCatalogRuntime` (5 interface entries, 5 methods). The other **205 entries** belong to the 38 new exported types; these counts describe census entries, not exclusively methods. The `LegacyConcretePublicationExpansionContractTests` protect original constructors and interface surfaces, while the exported-inventory tests protect the current assembly census. Both groups remain covered by the final maintainer-confirmed **1475/1475 GREEN** release suite. See [the 14-item inventory](../Original-Type-API-Additions/en.md).
