# Public API inventory and 1.0 decision register

Status: **pre-1.0 inventory; classification candidates, not a published 1.0 compatibility promise**.
Source of truth: `afrowaveltd/Toolbox` GitHub `master` at `9f4fab423b898b8f30f584eeedbff54f97f0cadc` (the 1212/1212 GREEN checkpoint).
The Git tree at this checkpoint is complete (`truncated=false`): 114 production `.cs` source files under `WhenItFails/`, including 31 files in `Interfaces/`. The number of files **is not** the count of public CLR types. The groups below record verified declarations and transitive public dependencies; this is not yet an exhaustive assembly-level API report.

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

No concrete public class is being hidden or renamed in this inventory checkpoint. Restricting existing public visibility may break users of the already published 0.1.0 package.

## 5. Open 1.0 decisions and next steps

1. Decide whether `GetCurrentContext()` intentionally exposes a **shared mutable active context** or should offer a separate read-only/snapshot projection. The current `ErrorCatalogContextStore` publishes/swaps a reference atomically but returns the same mutable object. Do not represent it as an immutable snapshot.
2. Determine JSON and nullability versioning guarantees for the transitive documents, payloads, profile definitions, errors and status enums. Keep schema evolution separate from C# API compatibility.
3. For public concrete classes, identify genuine consumer/tooling use before any `public` → `internal` change. Interface signatures alone do not prove concrete constructors are unused.
4. Perform an assembly-level exported-type/member inventory on the packaged binary and compare it against this source map before freezing the 1.0 surface. This source review is **not** that binary compatibility test.
5. Focused public API contract tests for `ErrorCatalogInitializationPayload`, `JsonsBootstrapPayload`, and `JsonsBootstrapFileResult` were added in `WhenItFails.Tests/PublicApi/InitializationAndBootstrapPayloadPublicApiContractTests.cs` and are **1216/1216 GREEN** in the complete suite. The five catalog document models have focused CLR/JSON/default/nullability contract tests confirmed in the complete **1222/1222 GREEN** suite. The four supporting definition models now have focused property/JSON/nullability tests in `SupportingDefinitionPublicApiContractTests`, confirmed in the complete **1225/1225 GREEN** suite. Their existing `DefinitionContracts` tests already cover defaults and mutable-container isolation. The five catalog provider payloads now have focused CLR property/nullability/default/reference tests in `CatalogProviderPayloadPublicApiContractTests` (complete **1228/1228 GREEN** suite). Validation result/issue/severity now have focused CLR/nullability/enum/liveness contract tests in `ValidationModelsPublicApiContractTests` (complete **1232/1232 GREEN** suite). Next, review the bundled-template model and exported assembly surface.

No production code changes were made as part of this inventory.
