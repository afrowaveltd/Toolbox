# Public API inventory and 1.0 decision register

Status: **pre-1.0 inventory; classification candidates, not a published 1.0 compatibility promise**.

The current support-level decision is recorded in [the candidate 1.0 public API policy](../Public-API-1.0-Policy/en.md). This inventory remains evidence/input for that policy rather than a second competing classification.
Source of truth: current `afrowaveltd/Toolbox` GitHub `master`, with the latest maintainer-confirmed complete suite at **1473/1473 GREEN**. The refreshed compiled assembly inventory reports **148 exported CLR types**. Historical source-file counts and the earlier 110-type inventory remain useful chronology, but they are not the current API census.

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

## 4. Public concrete types vs internal implementation

The following are **public concrete implementation examples requiring an explicit compatibility decision**: `ErrorCatalogRuntime`, `ErrorCatalogContextStore`, `ErrorCatalogContextProvider`, `ErrorCatalogFactory`, `ErrorCatalog`, `JsonsBootstrapper`, specialized JSON loaders/providers/validators, normalization classes, and `ErrorDescriptorFactory` / resolver classes. Their interface may be stable while their **constructor, subclassing and concrete-type surface** is not automatically guaranteed.

Separate public utility candidates include `JsonCatalogDocumentWriter`, `DocumentationKeyGenerator`, `DocumentationKeyFormat` and `TextKeyNormalizer`. Their existing uses inside Toolbox (including Toolroom) and any consumer-facing documentation must be checked **before** considering a visibility change or treating them as implementation-only.

Verified internal helper declarations include `CatalogProviderPipeline`, `DefinitionNormalizationHelper` and `CatalogValidationHelper`. A `public` method on an `internal` containing type is **not** an externally accessible public type/member contract.

No concrete public class is being hidden or renamed in this inventory checkpoint. Restricting existing public visibility may break consumers compiled against the existing local 0.1.0 reference package artifact, even though that artifact was never published to a feed.

## 5. Current 1.0 decisions and remaining gate

1. `GetCurrentContext()` intentionally retains the original live, mutable context reference for compatibility. Consumers treat it as read-only by convention. The additive detached snapshot/observation family now provides the safer read path without expanding the original nine-method `IErrorCatalogRuntime` interface.
2. Persistent project catalog JSON is explicitly versioned separately from CLR compatibility. The stable 1.0 catalog baseline is `schemaVersion: "1.0"`; other non-empty versions are rejected with `UnsupportedSchemaVersion`. Snapshot DTOs remain CLR projections, not a promised versioned JSON wire format.
3. Public concrete implementations remain **public implementation surface** under the candidate 1.0 policy. Their visibility and already relied-upon constructors are not narrowed casually; interfaces remain the preferred dependency boundary for new code.
4. The current compiled assembly inventory is refreshed and still reports **148 exported types**. The remaining binary/API gate is the exact comparison against the original maintainer-held 0.1.0 `.nupkg`; a rebuilt replacement must not be used.
5. The transitive public payload/document/definition/validation/template contracts and the later snapshot/nullability contracts are all included in the current maintainer-confirmed **1473/1473 GREEN** suite. Earlier per-checkpoint counts remain historical evidence rather than the current verification state.

No production code changes were made as part of this inventory reconciliation.


## 6. Additive snapshot API review — 2026-09-25

The original runtime interface still declares exactly nine methods. Optional publication/activation/combined/supporting/full readers and the snapshot extension families remain separate public capabilities; internal capture helpers remain implementation details. The focused boundary contracts are included in the current **1473/1473 GREEN** suite. The refreshed compiled inventory reports **148 exported types**. See [snapshot capability boundaries](../Pre-1.0-Snapshot-Capability-Boundaries/en.md). The exact original-0.1.0 binary comparison remains the freeze gate.


## 7. Current export refreshed; exact 0.1.0 comparison pending

The compiled-assembly inventory tests cover newer snapshot exports, internal capture-helper non-exposure and deterministic complete type reporting. A freshly generated current-master report has now been supplied and still reports **148 exported source types**, so no exported-type-count drift is observed. `PublicApiComparer` separately measures source/package exported-type and member-entry differences against the exact requested `[0.1.0]` package. The last successful historical comparer measured **148 source / 110 package types**, **830 source / 611 package entries**, **38 source-only / 0 package-only types**, and **219 source-only / 0 package-only entries**. Those member counts remain historical until the exact original artifact is available for refresh. See [the current inventory procedure](../Current-Public-API-Inventory/en.md).


## 8. Classification of the 38 source-only exported CLR types

The maintainer supplied the complete **38-name source-only type list** from the current package comparer. It comprises **2 store infrastructure interfaces, 5 optional runtime readers, 2 publication/activation infrastructure models, 19 detached catalog/validation/observation models and 10 additive snapshot extension classes**. See [complete classification](../Added-Public-Types-Classification/en.md). This is a provisional usage classification, not a visibility change by itself. The ownership groups are now reflected in the candidate 1.0 policy, the refreshed current inventory remains at **148 exported types**, and the complete suite is **1473/1473 GREEN**. The exact member-entry delta still requires the original 0.1.0 artifact for refresh.


## 9. Source-only additions to existing concrete classes

A maintainer-supplied filtered comparer output identifies **14 API census entries on the two original classes**: `ErrorCatalogContextStore` (2 interface entries, 2 methods) and `ErrorCatalogRuntime` (5 interface entries, 5 methods). The other **205 entries** belong to the 38 new exported types; these counts describe census entries, not exclusively methods. The `LegacyConcretePublicationExpansionContractTests` protect original constructors and interface surfaces, while the exported-inventory tests protect the current assembly census. Both groups are included in the maintainer-confirmed **1473/1473 GREEN** complete suite. See [the 14-item inventory](../Original-Type-API-Additions/en.md).
