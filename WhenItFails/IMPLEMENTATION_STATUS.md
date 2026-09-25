# Implementation status

Last updated: 2026-09-25

This file is the continuation point for `WhenItFails` development. Git history contains the detailed chronological checkpoints; keep this file focused on the current verified state, established contracts, and next step.

## Current focus

Core hardening and concrete class-level coverage audits are complete for the current scope. NuGet archive, dependency restore, embedded-template consumption and full external runtime initialization/resolution in a separate .NET 10 consumer are verified. Next: complete the public API stability review and define the exact stable 1.0 scope.

## 2026-09-24 — core public API entry-point contract (1159/1159 GREEN)

- Externally restored NuGet package 0.1.0 was inspected from a separate consumer after the successful runtime smoke test at `5e41e4b3`.
- The first API stability group covers the nine declared `IErrorCatalogRuntime` methods and four `AddWhenItFails` overloads, including optional cancellation tokens and the DI extension namespace.
- Added `WhenItFails.Tests/PublicApi/CoreEntryPointPublicApiContractTests.cs` (two focused contract tests) and `Docs/Public-API-Stability/en.md` for the initial 1.0-scope review.
- The other six public models remain under review; no public API or runtime behavior was changed.
- **Verified locally by maintainer:** both focused contract tests and the complete **1159/1159 GREEN** suite after commit `f9065aee1322993942b6ae0ac50aecac3dcbb3b2`.
- Next: audit `ErrorDescriptor` and `ErrorDefinition` public C# shape, JSON property names, defaults, and mutable collection expectations.

## 2026-09-24 — ErrorDescriptor / ErrorDefinition contract baseline (1163/1163 GREEN)

- Added `WhenItFails.Tests/PublicApi/ErrorModelPublicApiContractTests.cs` with four focused tests for the published C# property shapes, JSON field names, runtime-only exception exclusion, default values, per-instance mutable collections and `MetadataBag` JSON round trips.
- `ErrorDescriptor` is a publicly constructible unsealed runtime occurrence model with 21 declared properties; `ErrorDefinition` is a publicly constructible sealed catalog model with 16 declared properties.
- These tests snapshot the existing 0.1.0 behavior for pre-1.0 review; they do not change or declare a permanent 1.0 guarantee for every existing property.
- **Verified locally by maintainer:** four focused model contract tests and complete **1163/1163 GREEN** suite after commit `a6eaba33dad832f7a85d7168fbf0ff431979791c`.
- Next: audit `ErrorCatalogContext` mutable shared references and `ErrorCatalogRuntimeStatus` computed state/consistency contracts.

## 2026-09-24 — runtime context/status public API baseline (1165/1165 GREEN)

- Added `WhenItFails.Tests/PublicApi/RuntimeStatePublicApiContractTests.cs` with two focused reflection tests: `ErrorCatalogContext` seven mutable public properties and `ErrorCatalogRuntimeStatus` nine `init` properties plus two computed getter-only properties.
- Audited `ErrorCatalogContextStore`: it swaps the active context reference atomically but returns the same mutable context instance on reads. An application can mutate a returned catalog context; neither thread-safe reference publication nor a context getter implies a deep immutable snapshot.
- `ErrorCatalogRuntimeStatus` publishes a separate instance via a volatile reference; its `State` and `IsConsistent` are computed from the init-only fields, whose semantic cases are already tested under `WhenItFails.Tests/Runtime/ErrorCatalogRuntimeStatusTests.cs`.
- No production behavior, source or API visibility changed. Before 1.0, decide and document whether active context mutation is a supported extension mechanism or a boundary to restrict with a compatibility plan.
- **Verified locally by maintainer:** two focused tests and complete **1165/1165 GREEN** suite after commit `efa89777656a99cdd39900d361d9c69b53fa1da7`.
- Next: audit `WhenItFailsOptions` and `JsonsOptions` property shape, default paths and DI registration snapshots.

## 2026-09-24 — public configuration API baseline (1169/1169 GREEN)

- Added `WhenItFails.Tests/PublicApi/ConfigurationPublicApiContractTests.cs` with four focused tests for the existing `WhenItFailsOptions` / `JsonsOptions` property and constructor shape, default values, live computed paths, and deep options snapshot at explicit DI registration.
- `WhenItFailsOptions` exposes three get/set properties. `JsonsOptions` exposes seven get/set configuration values and six getter-only computed paths.
- Computed paths use platform-sensitive `Path.Combine` and reflect current option values; **path construction alone is not validation**. Existing bootstrapper guards remain the validation boundary.
- Explicit `AddWhenItFails(WhenItFailsOptions)` takes an independent snapshot of the supplied option object and nested `JsonsOptions` fields at registration time. The registered singleton itself remains mutable; no guarantee of later immutability is implied.
- This completes the initial eight-type API shape baseline, but DOES NOT settle the active catalog context mutability decision, full nullability/JSON/versioning policy, or review of extension points and other public implementation types.
- No production code changed. **Verified locally by maintainer:** four focused tests and full suite **1169/1169 GREEN** after commit `8f8870ab6978d3ca2389638e520f328a0fd97ecc`.
- Next: review DI-replaceable extension interfaces and classify stable contract vs extension point vs implementation detail.

## 2026-09-24 — first DI extension-point API contracts (1173/1173 GREEN)

- Added `WhenItFails.Tests/PublicApi/FirstExtensionPointPublicApiContractTests.cs` with four focused tests covering `IJsonsTemplateProvider`, `IErrorCatalogContextProvider`, and `IErrorDescriptorService` (exact declared method shapes and optional catalog-provider cancellation token), plus pre-registered custom implementation preservation via `AddWhenItFails()`.
- Confirmed in the existing DI implementation that these services are registered using `TryAddSingleton`. Pre-registering a custom implementation is supported by this registration mechanism, but the tests do not claim that arbitrary custom behaviors are automatically validated or safe.
- First classification: these three public interfaces are **DI extension-point candidates**, while `IErrorCatalogRuntime` remains the application-facing stable-contract candidate. Complete extension-point and implementation-detail inventory is still pending.
- The local `ErrorCatalogContext` mutability decision remains open; extension tests do not imply immutability.
- No production behavior or visibility changed. **Verified locally by maintainer:** four focused extension-point tests and complete **1173/1173 GREEN** suite after commit `053dc387d926e9be3956dd2c99268fc4681c648f`.
- Next: audit `IErrorCatalogInitializer`, `IErrorCatalogContextStore`, and `IJsonsBootstrapper` signatures and DI replacement contracts.

## 2026-09-24 — second DI extension-point API contracts (1177/1177 GREEN)

- Added `WhenItFails.Tests/PublicApi/SecondExtensionPointPublicApiContractTests.cs` with four focused tests for `IErrorCatalogInitializer`, `IErrorCatalogContextStore` and `IJsonsBootstrapper` signatures and pre-registered DI implementation precedence.
- The initializer and bootstrapper each expose one async method taking `JsonsOptions` and an optional `CancellationToken`, returning their typed `Response<T>` in a task.
- The context-store interface exposes getter-only `IsInitialized` and nullable `Current`, and `GetCurrent()` and `Set(ErrorCatalogContext)`. Reflection checks verify CLR shape; nullable reference annotations and error/recovery behavior remain separate audit work.
- Each default service uses `TryAddSingleton`; a prior custom singleton remains registered and the default runtime service graph can still be constructed with scope/build validation. These are registration-shape tests, not endorsement of arbitrary custom service semantics.
- **No production code or visibility changed. Verified locally by maintainer:** four focused tests and complete **1177/1177 GREEN** suite after commit `b61b5bd38534e10e4063d6b9ac383591165bd355`.
- Next: catalog source/loading and factory/provider DI interfaces.

## 2026-09-24 — main catalog pipeline extension-point API contracts (1181/1181 GREEN)

- Added `WhenItFails.Tests/PublicApi/CatalogPipelineExtensionPointPublicApiContractTests.cs` with four tests for `IErrorCatalogLoader`, `IErrorCatalogFactory`, and `IErrorCatalogProvider` (one declared method each), including optional file-loading cancellation tokens.
- Loader returns `Task<Response<ErrorCatalogDocument>>`; factory converts `ErrorCatalogDocument` to `IErrorCatalog` synchronously; provider returns `Task<Response<ErrorCatalogProviderPayload>>`.
- Verified source DI registrations use `TryAddSingleton`; tests register three test-only implementations first and assert service identity plus validation of the remaining default runtime graph.
- These interfaces are candidates for supported DI extension points, subject to separate behavior and exception/cancellation contract review. The test-only factory intentionally throws if invoked; this suite checks registration rather than a full custom pipeline run.
- `IErrorCatalog` itself (ten lookup methods) remains for a separate dedicated interface baseline.
- **Verified locally by maintainer:** four focused tests and complete **1181/1181 GREEN** suite after commit `14831c34701a2b7326e859ffbdb35e40e753b8a1`. Production code and visibility unchanged.
- Next: audit the ten-method `IErrorCatalog` lookup interface and distinguish single-result nullable lookups from collection-returning lookups.

## 2026-09-24 — IErrorCatalog lookup public API baseline (1184/1184 GREEN)

- Added `WhenItFails.Tests/PublicApi/ErrorCatalogLookupPublicApiContractTests.cs` with three focused tests: ten exact lookup interface method signatures; nullability metadata for single-result vs non-null collection returns; and a public `IErrorCatalogFactory` → `IErrorCatalog` smoke test.
- `GetAll()` and six classification searches return `IReadOnlyList<ErrorDefinition>`; `FindById`, `FindByCode`, `FindByName` return `ErrorDefinition?`. This classification does not imply that returned `ErrorDefinition` instances are immutable.
- Existing `WhenItFails.Tests/Catalog/ErrorCatalogTests.cs` already covers normalization, positive/negative lookups, and category/tag semantics; the new API tests intentionally avoid duplicating its comprehensive behavior coverage.
- `IErrorCatalog` is a public consumer lookup contract and a return type of `IErrorCatalogFactory`, not a directly registered DI service. Third-party factory implementations can supply another implementation subject to the contract.
- Production code unchanged. **Verified locally by maintainer:** three focused tests and complete **1184/1184 GREEN** suite after commit `76161e5cff0e3e1c70e63940cdc7fb3e3f80b850`.
- Next: audit main catalog normalizer and validator interface contracts and DI replacement.

## 2026-09-24 — catalog normalization and validation public API baseline (1188/1188 GREEN)

- Added `WhenItFails.Tests/PublicApi/CatalogNormalizationValidationPublicApiContractTests.cs` with four focused tests for `IErrorCatalogDocumentNormalizer` and `IErrorCatalogValidator` method shapes and nullable annotations, DI registration precedence, and the established differences in default null-input behavior.
- `Normalize(ErrorCatalogDocument)` returns a non-null `ErrorCatalogDocument`; the default normalizer throws `ArgumentNullException` for null input. `Validate(ErrorCatalogDocument?)` returns a non-null `ErrorCatalogValidationResult`; the default validator reports a `CatalogDocumentIsNull` validation issue for null.
- The default document normalizer creates a new document but currently keeps the source `MetadataBag` reference, as already asserted by `WhenItFails.Tests/Normalization/ErrorCatalogDocumentNormalizerTests.cs`. **Do not claim deep-copy isolation** of all fields.
- Both defaults use `TryAddSingleton`; pre-registered custom implementations are expected to remain in DI. These are interface/registration and narrow boundary tests, not a full acceptance suite for third-party implementations.
- Production source unchanged. **Verified locally by maintainer:** four focused tests and complete **1188/1188 GREEN** suite after commit `dd15485158df460dccd7c5e191063465d46631c7`.
- Next: review specialist category, owner, code-group and profile catalog loader interfaces and DI replacement.

## 2026-09-24 — specialized catalog loader public API baseline (1193/1193 GREEN)

- Added `WhenItFails.Tests/PublicApi/SpecializedCatalogLoaderPublicApiContractTests.cs` with five focused tests: signatures of the category, owner, code-group and profile catalog loaders, and one combined DI override test for their pre-registered custom implementations.
- Each public loader exposes exactly one `LoadFromFileAsync(string, CancellationToken = default)` method returning a `Task<Response<TCatalogDocument>>` for its own specialized catalog document type.
- Their default DI registrations use `TryAddSingleton`. The custom implementations in this suite are registration-only test stubs; no file access, full alternate loader behavior, or cancellation-path execution is implied.
- The specialized provider and validator interfaces remain for separate focused groups. All production APIs and implementations remain unchanged.
- **Verified locally by maintainer:** all five focused tests and complete **1193/1193 GREEN** suite after commit `be7066854c56fb37e33432eeee2ae7b453841621`.
- Next: audit the four specialized catalog providers and DI replacement.

## 2026-09-24 — specialized catalog provider public API baseline (1198/1198 GREEN)

- Added `WhenItFails.Tests/PublicApi/SpecializedCatalogProviderPublicApiContractTests.cs` with five focused tests: exact signatures of the category, owner, code-group and profile catalog providers, and one combined DI override test for pre-registered custom implementations.
- Each public provider exposes one `LoadFromFileAsync(string filePath, CancellationToken cancellationToken = default)` method. Its response payload is the corresponding `ErrorCategoryCatalogProviderPayload`, `ErrorOwnerCatalogProviderPayload`, `ErrorCodeGroupCatalogProviderPayload` or `ErrorProfileCatalogProviderPayload`, wrapped in `Task<Response<T>>`.
- Their default DI registrations use `TryAddSingleton`. The custom implementations in this suite are registration-only test stubs; no filesystem access or end-to-end custom provider behavior is exercised.
- Specialized validator interfaces remain for the next focused group. No production implementation or public visibility changed.
- **Verified locally by maintainer:** all five focused tests and complete **1198/1198 GREEN** suite after final test correction commit `51ad75815a8d2ae30efa3daaf0a00d1d33019000`.
- Next: audit four specialized catalog validator interfaces and DI replacement.

## 2026-09-24 — specialized catalog validator public API baseline (1203/1203 GREEN)

- Added `WhenItFails.Tests/PublicApi/SpecializedCatalogValidatorPublicApiContractTests.cs` with five focused tests: exact signatures and nullable input/non-null output annotations for the category, owner, code-group and profile catalog validators, plus custom DI implementation precedence for all four services.
- Each specialized validator exposes `ErrorCatalogValidationResult Validate(TCatalogDocument? document)`. The document type differs by catalog family; `null` is permitted by the interface annotation. The test-only validators only verify registration and signature shape, not correctness of validation results.
- All four defaults use `TryAddSingleton`; pre-registered replacements remain selected when `AddWhenItFails()` is invoked. The test validates DI graph construction without executing a custom pipeline.
- The four specialized loader, provider, and validator groups have now each received their first public API baseline, subject to local verification for this validator group. Future work: remaining resolver/descriptor interfaces and explicit classification of stable contracts vs implementation details.
- Production source unchanged. **Verified locally by maintainer:** all five focused tests and complete **1203/1203 GREEN** suite after commit `a5029ce927c3d6394fd5a975fcdab41315e57188`.
- Next: audit definition-resolution and descriptor factory/resolver interface contracts and DI replacement.

## 2026-09-24 — definition and descriptor extension-point public API baseline (1208/1208 GREEN)

- Added `WhenItFails.Tests/PublicApi/DefinitionAndDescriptorExtensionPointPublicApiContractTests.cs` with five focused tests: exact method shapes for `IErrorDefinitionResolver` (three methods), `IErrorDescriptorFactory` (one method), and `IErrorDescriptorResolver` (three methods), context/factory nullability annotations, and custom DI implementation precedence.
- Definition resolution returns `Response<ErrorDefinition>`; descriptor resolution returns `Response<ErrorDescriptor>`; descriptor factory converts `ErrorDefinition` directly to `ErrorDescriptor`. The resolver context input is nullable in all six lookup methods, and factory definition input/output are non-nullable in the public C# annotation contract.
- Defaults are registered via `TryAddSingleton`; test-only custom implementations must remain resolved after `AddWhenItFails()` while the remaining service graph validates. This does not establish the full behavior of arbitrary replacement implementations.
- Existing default resolver/factory behavior tests remain responsible for runtime correctness; the present checkpoint changes no production source or public visibility.
- **Verified locally by maintainer:** five focused tests and complete **1208/1208 GREEN** suite after commit `e9d2936858714b9168501be04dff39a76098b7a8`.
- Next: audit profile resolver and profile-selection interface contracts and DI replacement.
- Next after verification: review profile resolver and profile-selection extension interfaces, then inventory implementation details and define the stable 1.0 surface.

## 2026-09-24 — profile resolution extension-point public API baseline (1212/1212 GREEN)

- Added `WhenItFails.Tests/PublicApi/ProfileResolutionExtensionPointPublicApiContractTests.cs` with four focused tests for the two remaining profile resolver interfaces: method signatures, nullable-reference annotations, and pre-registered custom DI implementation precedence.
- `IErrorProfileResolver.Resolve(ErrorCatalogDocument, ErrorProfileDefinition)` returns `IReadOnlyList<ErrorDefinition>` directly; its inputs and returned collection are non-nullable in the public C# contract.
- `IErrorProfileSelectionService.ResolveByProfileName(ErrorCatalogContext?, string)` returns `Response<IReadOnlyList<ErrorDefinition>>`. Only the context parameter is nullable; this interface operates at the loaded-context layer rather than the document/profile layer.
- Both defaults use `TryAddSingleton`. The test-only replacements validate registration precedence, not equivalent full profile selection semantics.
- No production API or behavior changed. **Verified locally by maintainer:** four focused tests and complete **1212/1212 GREEN** suite after commit `34ef9cf3f16cad2650c4df0ec5e74fb89956ca64`.
- Next: inventory the remaining public concrete types, dependent DTOs and interfaces; classify the stable 1.0 API vs supported extension points vs public implementation details, then decide active context mutability/versioning policy.

## 2026-09-24 — public API inventory and 1.0 decision register (source audit, no new tests)

- Added `Docs/Public-API-Inventory/en.md`, linked from project README and baseline review. GitHub tree at `9f4fab423b898b8f30f584eeedbff54f97f0cadc` contains 114 production C# files under `WhenItFails/`, including 31 interface source files; this is a *source-file* count, not an exported-type count.
- The initial source audit identifies transitive public payloads, documents, definitions, validation models, bootstrap types and enums referenced by already covered signatures; these must be reviewed before 1.0. Thirty interfaces have first-shape test coverage; `IBuiltInErrorCatalogContextProvider` remains to be reviewed.
- Distinguish stable consumer candidates, DI extension-point candidates, public concrete implementations/utilities requiring compatibility decisions, and verified internal helpers. Do not hide or rename currently public types without usage/compatibility analysis.
- Open: active shared-context mutability, JSON/nullability/versioning, concrete-class consumers (including Toolroom), and full exported-assembly inventory.
- **No tests added and no production source changed.** Last maintainer-confirmed complete suite remains **1212/1212 GREEN** after commit `34ef9cf3f16cad2650c4df0ec5e74fb89956ca64`; this documentation checkpoint itself has not been locally test-verified.
- Next focused test group: `ErrorCatalogInitializationPayload`, `JsonsBootstrapPayload`, `JsonsBootstrapFileResult`, then the remaining built-in provider interface.

## 2026-09-24 — initialization and bootstrap payload API baseline (1216/1216 GREEN)

- Added `WhenItFails.Tests/PublicApi/InitializationAndBootstrapPayloadPublicApiContractTests.cs` with four focused tests of `ErrorCatalogInitializationPayload`, `JsonsBootstrapPayload` and `JsonsBootstrapFileResult`: exact public property types and accessor shape, defaults, nullable annotations, independent mutable `Files` lists and computed degraded state.
- `ErrorCatalogInitializationPayload.Bootstrap` and `Context` are annotated non-nullable but default to null via `null!` until the producer populates them. `IsDegraded` is a getter-only logical OR of `KeptPreviousContext` and `UsedFallback`.
- `JsonsBootstrapPayload.Files` is a getter-only reference to a mutable `List<JsonsBootstrapFileResult>`, initialized independently per payload instance; do not mistake it for an immutable snapshot.
- Documentation updated in the API inventory and baseline. No production source changed. **Verified locally by maintainer:** all four focused tests and complete **1216/1216 GREEN** suite after commit `a0393dc147511c0be218ae7d9ae55874729da3c0`.
- Next after verification: `IBuiltInErrorCatalogContextProvider` and the remaining dependent model/JSON versioning inventory.

## 2026-09-24 — built-in catalog context provider public API baseline (1218/1218 GREEN)

- Added `WhenItFails.Tests/PublicApi/BuiltInCatalogContextProviderPublicApiContractTests.cs` with two focused tests for the final individually unreviewed interface, `IBuiltInErrorCatalogContextProvider`.
- The interface exposes exactly one `LoadAsync(CancellationToken cancellationToken = default)` method returning `Task<Response<ErrorCatalogContext>>`; the token remains optional.
- Source DI registration uses `TryAddSingleton<IBuiltInErrorCatalogContextProvider, BuiltInErrorCatalogContextProvider>()`. The test verifies that a prior custom registration remains selected and that the remaining WhenItFails service graph validates.
- The test intentionally does not execute the default built-in provider or create its temporary filesystem workspace; its existing behavioral tests remain responsible for loading, validation, cleanup, exception normalization and cancellation behavior.
- All **31 interface source files** under `WhenItFails/Interfaces/` now have at least a first method/property-shape review in the public API contract suite.
- No production code changed. **Verified locally by maintainer:** both focused tests and complete **1218/1218 GREEN** suite after commit `ed0075da64a66657caf2e91d7d298cd92161ea01`.
- Next: continue transitive data/JSON model review, beginning with the five catalog document types and their definition models.

## 2026-09-24 — catalog document public API/JSON baseline (1222/1222 GREEN)

- Added `WhenItFails.Tests/PublicApi/CatalogDocumentPublicApiContractTests.cs` with four focused tests covering all five public catalog document types: `ErrorCatalogDocument`, `ErrorCategoryCatalogDocument`, `ErrorOwnerCatalogDocument`, `ErrorCodeGroupCatalogDocument` and `ErrorProfileCatalogDocument`.
- Each document has the same ten catalog-header properties plus one typed content collection. The tests lock the current CLR property/accessor shape, explicit `JsonPropertyName` field names, default values, nullable-reference annotations and independently allocated `Tags`, `Metadata` and typed content collections.
- Current defaults are `SchemaVersion = "1.0"`, empty catalog id/name, `Language = "en"`, null optional description/source fields, `IsShadowCopy = false`, and empty per-instance collections/metadata.
- The JSON content fields remain `errors`, `categories`, `owners`, `codeGroups` and `profiles` respectively. These tests snapshot the present pre-1.0 schema; they do not yet define a policy for future schema-version migration.
- No production code changed. **Verified locally by maintainer:** all four focused tests and complete **1222/1222 GREEN** suite after commit `643c4681e23988641e575b2d28901d05b726c527`.
- Next: audit the four supporting definition models (`ErrorCategoryDefinition`, `ErrorOwnerDefinition`, `ErrorCodeGroupDefinition`, `ErrorProfileDefinition`) including JSON names/defaults/collection isolation.

## 2026-09-24 — supporting definition model public API/JSON baseline (1225/1225 GREEN)

- Added `WhenItFails.Tests/PublicApi/SupportingDefinitionPublicApiContractTests.cs` with three focused tests covering `ErrorCategoryDefinition`, `ErrorOwnerDefinition`, `ErrorCodeGroupDefinition` and `ErrorProfileDefinition`.
- The new tests snapshot each model's exact public property/accessor shape, all explicit `JsonPropertyName` names, and nullable-reference annotations. The models intentionally have different property counts and semantics, so the contract data records each shape independently instead of assuming one common schema.
- Existing `WhenItFails.Tests/DefinitionContracts/*DefinitionContractTests.cs` already verify safe constructor defaults and per-instance mutable container isolation for all four models; those behavioral checks are intentionally not duplicated here.
- This checkpoint treats the existing JSON names as compatibility-sensitive inputs to the 1.0 review but does not yet establish a general future JSON schema migration policy.
- No production code changed. **Verified locally by maintainer:** three focused tests and complete **1225/1225 GREEN** suite after commit `f04c00a03ea91822453d42c6de453672c5b0f2ac`.
- Next: audit the main and four specialized provider payload models, then validation result/issue/severity and `JsonsTemplateFile`.

## 2026-09-25 — provider payload public API baseline (1228/1228 GREEN)

- Added `WhenItFails.Tests/PublicApi/CatalogProviderPayloadPublicApiContractTests.cs` with three focused tests covering all five public catalog provider payload types.
- `ErrorCatalogProviderPayload` publishes `IErrorCatalog Catalog`, `ErrorCatalogDocument Document`, and `ErrorCatalogValidationResult ValidationResult`; four specialized payloads each publish their corresponding typed `Document` and a `ValidationResult`. All declared properties are public get/set.
- The reference properties carry non-nullable C# annotations but currently start as `null!` on a manually constructed empty payload. Tests check both the annotations and actual empty-constructor state. A successful provider response must populate its payload; these tests do not claim that any incomplete instance is valid.
- Assigned document and validation-result references are retained as supplied; payloads are not documented as deep copies or immutable snapshots. The existing models have no explicit JSON property-name attributes; **do not claim a separately versioned JSON wire contract for provider payloads** from this CLR-shape baseline.
- No production code changed. **Verified locally by maintainer:** all three focused tests and complete **1228/1228 GREEN** suite after commit `17016cabb03aacccc408f3591fc95f846255230e`.
- Next: audit `ErrorCatalogValidationResult`, `ErrorCatalogValidationIssue` and `ErrorCatalogValidationSeverity`, then the public template model.

## 2026-09-25 — validation result/issue/severity public API baseline (1232/1232 GREEN)

- Added `WhenItFails.Tests/PublicApi/ValidationModelsPublicApiContractTests.cs` with four focused tests for `ErrorCatalogValidationResult`, `ErrorCatalogValidationIssue`, and `ErrorCatalogValidationSeverity`.
- Public shape: result exposes getter-only `IReadOnlyList<ErrorCatalogValidationIssue> Issues` and computed getter-only `bool IsValid`; its four public methods are `AddIssue`, `AddError`, `AddWarning`, and `AddInformation`. The three severity-specific methods retain their three optional nullable detail parameters.
- Issue model exposes six public get/set properties, with nullable `ErrorId`, `ErrorName`, and `Path`. The severity enum has exactly three values: `Information = 0`, `Warning = 1`, and `Error = 2`.
- Existing `Validation/*Tests.cs` and `Enums/ErrorCatalogValidationSeverityContractTests.cs` already cover defaults, issue insertion, live list view, validity for different severity levels and numeric enum values; the new behavioral test adds recomputation of `IsValid` when a retained issue changes severity after insertion. The result must not be documented as an immutable snapshot.
- No production code changed. **Verified locally by maintainer:** all four focused tests and complete **1232/1232 GREEN** suite after commit `fb3fc4c650709853041aafad69e6ea5e0993108e`.
- Next: review `JsonsTemplateFile` public model, then exported assembly surface and concrete implementation stability decisions.

## 2026-09-25 — JsonsTemplateFile public API baseline (1235/1235 GREEN)

- Added `WhenItFails.Tests/PublicApi/JsonsTemplateFilePublicApiContractTests.cs` with three focused tests covering the public template model's CLR type/property shape, non-nullable string annotations, empty-string defaults, independent assignments and the typed `IJsonsTemplateProvider.GetTemplateFiles(JsonsOptions)` return type.
- `JsonsTemplateFile` has three public get/set string properties (`Name`, `TargetFileName`, `Content`) and a public parameterless constructor. There are no explicit `JsonPropertyName` attributes; this checkpoint establishes a CLR contract, **not** a separately versioned JSON wire schema for bundled-template objects.
- The template-provider shape was already covered in the first DI extension-point group; the third test explicitly ties its collection element type to this reviewed public model.
- No production code changed. **Verified locally by maintainer:** all three focused tests and complete **1235/1235 GREEN** suite after commit `d2e376800dbc39671cb083d695266ddc3e5361fd`.
- Next: assembly-level exported public API inventory (including publicly visible constructors, methods, dependent models and concrete implementation classes); classify stable contracts/extension points/implementation details before deciding any 1.0 visibility or compatibility changes.

## 2026-09-25 — compiled exported API inventory test (verification pending)

- Added `WhenItFails.Tests/PublicApi/ExportedAssemblyInventoryTests.cs` with one focused test using `typeof(IErrorCatalogRuntime).Assembly.GetExportedTypes()` to inspect the **compiled** WhenItFails assembly rather than counting source files. It verifies entry-point presence and exported type visibility.
- When `AFROWAVE_WHENITFAILS_PUBLIC_API_REPORT` is explicitly set to a file in an existing directory, the test writes a sorted Markdown inventory of exported types and their declared public constructors, methods, properties (including get/set/init), events, fields, enum numeric values, base types and interfaces. It prints exported type/interface/class/enum counts through xUnit output. Without that environment variable it does not write files.
- This is a first compiled-assembly inventory, **not a full ABI or nullability/attribute compatibility verifier**. The existing focused model tests cover nullable annotations and JSON names. Compare the exported source build against an external NuGet 0.1.0 consumer before any visibility or compatibility changes.
- No production code changed. **Verification pending:** one focused inventory test with an explicit report path and the complete suite; last maintainer-confirmed full suite **1235/1235 GREEN**. Expected total if the new test passes: **1236/1236 GREEN**.
- Next: obtain and review the generated report, categorize public concrete types and decide the 1.0 compatibility policy. Do not invent exported-type counts before running reflection.

## 2026-09-25 — reviewed compiled exported API report (110 exported types)

- Received and reviewed the maintainer-generated Markdown inventory from `ExportedAssemblyInventoryTests`: source-built `Afrowave.Toolbox.WhenItFails`, assembly version `0.1.0.0`, **110 exported types** and publicly declared constructors/members. This proves report export occurred, not that the complete new suite passed or that this DLL is byte-identical to the separately published NuGet package.
- Added `Docs/Public-API-Export-Review/en.md` with provisional 1.0 classification: application-facing stable-contract candidates; replaceable DI extension points; public standalone utilities used by Setter (`JsonCatalogDocumentWriter`, `DocumentationKeyGenerator`, `DocumentationKeyFormat`, `ErrorCatalogCrossValidator`); and exported concrete implementation candidates requiring individual compatibility decisions.
- Public auxiliary models `ErrorDescriptor<TAttachment>` and `ErrorDescriptorRequest` and standalone generic document I/O signatures require explicit 1.0 scope decisions. The inventory omits nullable-reference annotations, most custom attributes and generic constraints: do not use it as a complete ABI baseline.
- **Verified locally by maintainer:** inventory test and complete **1236/1236 GREEN** suite after commit `955f5f897be96ec41e77ea34ab3ec482043ccf06`; the report contains 110 exported types. No production code or public visibility changed.
- Next: verify `WhenItFails.Tests` full suite; review actual exported report against package 0.1.0 and the Setter utility dependency set, then select focused utility compatibility tests and decide active-context mutability policy.

## 2026-09-25 — Setter standalone utility public API baseline (1241/1241 GREEN, zero warnings)

- Added `WhenItFails.Tests/PublicApi/SetterUtilityPublicApiContractTests.cs` with five focused tests for `JsonCatalogDocumentWriter`, `DocumentationKeyGenerator`, `DocumentationKeyFormat`, and `ErrorCatalogCrossValidator`.
- The tests snapshot standalone public construction and precise member signatures, including the generic `SaveToFileAsync<TDocument>` class constraint, the optional cancellation token, the generator's static `ToSegment`, the static `IsCanonical`, and the cross-validator's five typed catalog parameters with an optional profile catalog.
- The narrow end-to-end smoke verifies that these tools can be called without DI: a canonical key is generated and validated, a null primary document gives a structured validation issue, and an empty writer path yields a structured invalid response before any filesystem write. Existing dedicated suites remain responsible for normal file writes/backups, cancellation and comprehensive key/cross-catalog semantics.
- These are public **standalone utility candidates with confirmed Setter consumers**, not unreviewed internal details. No production visibility or behavior changed. Published NuGet 0.1.0 comparison and full 1.0 compatibility policy are still pending.
- **Maintainer verified the five focused tests GREEN (5/5)** after `7f4875eeda318a43bd04aa0dcab3383f87ed6752`; the build reported three xUnit2031 analyzer warnings on `Assert.Single(collection.Where(predicate))`.
- Replaced all three with the supported `Assert.Single(collection, predicate)` overload. This changes test syntax only; no production code or test expectations changed. **Verified locally by maintainer after fix `6ecaa40b7c26261d8e253c4fb463d2c7b01e333b`: all five focused tests GREEN, complete **1241/1241 GREEN**, and zero build warnings.**
- Next: review source-built vs published NuGet 0.1.0 public API, then the independently exported `JsonCatalogDocumentLoader` and auxiliary descriptor models; decide active-context mutability policy.

## 2026-09-25 — isolated published NuGet 0.1.0 versus source API comparison (verification pending)

- Added `Toolroom/WhenItFails/PublicApiComparer/Compare-PublicApi.ps1` with README.md, Docs/Usage/en.md and IMPLEMENTATION_STATUS.md. It generates two independent temporary net10.0 consumers: a current-source ProjectReference and an exact published NuGet `[0.1.0]` PackageReference.
- Each consumer inspects its actually loaded DLL in a separate process. The Markdown report records both resolved DLL paths, SHA-256 hashes and public signature differences; it distinguishes published-only from source-only entries.
- Actual published-feed availability/provenance and script execution are **not yet verified**. A local NuGet cache may not prove package origin; do not substitute a new local build when published restore fails.
- No production code or tests changed. Last maintainer-confirmed full suite **1241/1241 GREEN with zero warnings**. Reflection comparison is not a full ABI, nullable-reference, JSON or behavior guarantee.
- Next: run the comparison against the actual published package and review its report before changing any public visibility.

## 2026-09-25 — source-built versus NuGet-resolved 0.1.0 API comparison (611/611 signatures matched)

- Maintainer ran `Toolroom/WhenItFails/PublicApiComparer/Compare-PublicApi.ps1` on Windows, with successful restores, Release builds and both isolated consumer executions. The generated Markdown report compares **611 current-source** and **611 package-consumer** API entries: **0 package-only** and **0 source-only**.
- The exact `[0.1.0]` PackageReference was requested. No `-Feed` override was supplied; the report says “configured sources and cache; check provenance”. It establishes **API-census equality for the two DLLs that actually loaded**, not origin from nuget.org.
- Source DLL SHA-256: `587AED89A427E184CEB465073201EB7CE986AE2219C964ACFFBF1C349EAE05EF`. Package-consumer DLL SHA-256: `379F7CF6FF99223ECF2F388AB6295A34D97F33A8BD8EB7F9A52152994347CE28`. The bytes differ despite matched reflected signatures; do not claim byte identity, exact binary equivalence, full ABI, nullable-annotation, JSON or runtime compatibility.
- Both consumers built successfully; their build output included informational `NETSDK1057` (preview SDK), not compiler warnings. No new complete `WhenItFails.Tests` suite was run in this step: the last confirmed test baseline remains **1241/1241 GREEN, zero warnings**.
- Source report contains local temporary user paths; record only hashes, counts and provenances in repository documentation. No production code, published package or API visibility changed.
- Next: narrowly baseline `JsonCatalogDocumentLoader` as a standalone public utility (signature and no-DI invalid-path/cancellation entry points), then examine auxiliary descriptor models and context mutability/versioning policy.

## 2026-09-25 — standalone JsonCatalogDocumentLoader public API baseline (1244/1244 GREEN)

- Added `WhenItFails.Tests/PublicApi/JsonCatalogDocumentLoaderPublicApiContractTests.cs` with three focused tests for the independently public `JsonCatalogDocumentLoader` utility.
- Its public parameterless constructor and one generic instance method `LoadFromFileAsync<TDocument>(string filePath, CancellationToken cancellationToken = default)` are checked, including the `class` generic constraint, exact `Task<Response<TDocument>>` return, and optional token metadata.
- The narrow no-DI smoke asserts structured `FilePathIsEmpty` on an empty path without filesystem writes; a pre-cancelled token must throw `OperationCanceledException` before the empty-path check. The existing dedicated loader tests continue to cover reading valid/invalid JSON, missing/directory paths and file I/O semantics; no duplicate broad filesystem suite was added.
- This is a **public standalone utility** baseline, not permission to narrow its already exported 0.1.0 entry point. The isolated source/package comparer recorded 611 matching reflected API entries, but full 1.0 compatibility scope, feed provenance and runtime/serialization compatibility remain separate.
- No production code changed. **Verified locally by maintainer:** all three focused tests and complete **1244/1244 GREEN** suite after commit `e6b33575ad15065e1df72567133d64a00ede5d48`. The maintainer did not separately report warning count in this checkpoint.
- Next: audit independently exported `ErrorDescriptorRequest` and `ErrorDescriptor<TAttachment>` CLR/JSON properties and their documented consumer surface, then address active-context mutability.

## 2026-09-25 — auxiliary descriptor public API/JSON baseline (1247/1247 GREEN)

- Added `WhenItFails.Tests/PublicApi/DescriptorAuxiliaryModelsPublicApiContractTests.cs` with three focused tests for public `ErrorDescriptorRequest` and `ErrorDescriptor<TAttachment>`.
- `ErrorDescriptorRequest` is sealed, publicly constructible, and has eight public get/set optional properties: seven nullable strings and nullable `int? Code`. Its public C# nullability is explicitly tested. It has no explicit `JsonPropertyName` attributes; this does not establish a separate fixed JSON wire naming policy for the request.
- `ErrorDescriptor<TAttachment>` is sealed, publicly constructible, inherits `ErrorDescriptor`, adds one public get/set generic `TAttachment? Attachment`, and explicitly serializes that member as `"attachment"`. The test checks an integer attachment in serialized JSON while confirming that the inherited runtime-only `Exception` is excluded under its `JsonIgnore` attribute.
- Existing `WhenItFails.Tests/Descriptors/ErrorDescriptorRequestContractTests.cs` and `ErrorDescriptorOfTContractTests.cs` already cover constructor defaults and assigned reference/value attachments, so those cases are not duplicated.
- No production source, visibility or package version changed. **Verified locally by maintainer:** all three focused tests and complete **1247/1247 GREEN** after commit `0d1c37e2eb4954ccea9d2ab1a6685ee5589d439a`; warning count was not separately reported.
- Next: establish and document active-context shared-reference behavior and decide a compatible read-only consumer direction.

## 2026-09-25 — shared active-context reference boundary (1250/1250 GREEN)

- Added `WhenItFails.Tests/PublicApi/ActiveContextSharedReferenceContractTests.cs` (three focused tests). They record: a reader's mutation of a returned context is visible to later readers; the publisher's mutation of the stored instance is visible after publication; and replacing the active context leaves a previously returned reference pointing at the old instance.
- Source audit: `ErrorCatalogContextStore.Set` atomically publishes the caller's reference with `Interlocked.Exchange`; `Current` and `GetCurrent` use `Volatile.Read` and return that same mutable object. `IErrorCatalogRuntime.GetCurrentContext()` forwards this response. Atomic reference replacement does not protect the context's mutable properties, nested catalog documents, definitions or collections from concurrent in-place modification.
- Compatibility direction: preserve the existing public `GetCurrentContext(): Response<ErrorCatalogContext>` signature from 0.1.0. Document a read-only-by-convention consumer rule and use the supported initialization/reset flow to activate validated replacements. Do not silently replace the existing response with a shallow copy or claim deep immutability. A future *additive* safe read-only/deep-snapshot API needs a separate ownership/schema design before being promised for 1.0.
- Updated `WhenItFails/README.md`, `Docs/Runtime/Public-API.md`, `Docs/Public-API-Stability/en.md` and `Docs/Public-API-Export-Review/en.md` to distinguish atomic reference publication from deep immutability and to record the 1247/1247 verified checkpoint.
- **Verified locally by maintainer:** three focused tests and complete **1250/1250 GREEN** suite after shared-reference test commit `77826ab331da83f180cfa7ad6edb95d53efb4c47`. Warning count was not separately reported. No production code, public signatures, package version or persisted JSON schema changed.
- Next: audit the nested context documents and indexed error definitions for mutation leaks when designing a distinct safe consumer-view API.

## 2026-09-25 — indexed catalog mutable-definition boundary (1253/1253 GREEN)

- Audited `ErrorCatalogFactory` and `ErrorCatalog`: the factory passes `ErrorCatalogDocument.Errors` to the catalog; the catalog copies the source list membership into a read-only list, but retains the **same mutable `ErrorDefinition` instances** in that list and its once-built indexes.
- Added `WhenItFails.Tests/PublicApi/IndexedCatalogMutableDefinitionBoundaryTests.cs` with three narrow tests: (1) mutating a source definition's ID/title is visible through the original index key while the new key is not indexed; (2) mutating `Tags` after indexing leaves the original tag key mapped to the changed definition, without indexing a new tag; (3) adding an item to the source document's `Errors` list does not extend an existing indexed catalog, while edits to an original item's fields remain visible.
- This is a pre-1.0 **hazard and ownership baseline**, not a recommendation to mutate active definitions or a decision to retain stale-index behavior in a future safe view. A safe consumer projection must account for the identity/collection indexes and deep mutability of their stored objects.
- Updated `WhenItFails/Docs/Public-API-Stability/en.md`, `Docs/Runtime/Public-API.md`, and `Docs/Public-API-Export-Review/en.md`; no production code, public signatures, NuGet version or JSON schema changed.
- **Verified locally by maintainer:** complete **1253/1253 GREEN** suite (0 failed, 0 skipped), successful build with no compiler warnings reported after commit `b0d9ceb09180d5ba2e72448648cc70d05f5711bc`; `NETSDK1057` is an informational preview SDK notice. The three new tests are included in the passing complete suite; focused run was not separately reported.
- Next: examine whether normalized catalog document and `IErrorCatalog` share definition/metadata references before designing any separate safe-context API.

## 2026-09-25 — normalized document/index ownership boundary (1256/1256 GREEN)

- Added `WhenItFails.Tests/PublicApi/NormalizedCatalogReferenceOwnershipTests.cs` with three focused cross-layer tests (normalization -> factory -> lookup): (1) normalized definitions are independent of source identity/title, yet the indexed definition is the normalized document's same object; (2) editing the normalized document/lookup definition is visible through the other reference while name indexes retain their original key; (3) normalized document/definition tag lists are distinct from source lists, but document/definition `MetadataBag` references remain aliased through normalization and catalog construction.
- Source audit: `ErrorCatalogDocumentNormalizer` allocates a document and invokes `ErrorDefinitionNormalizer` for new definition objects; both normalizers reuse source `MetadataBag` references. `ErrorCatalogFactory`/`ErrorCatalog` copy only the source definition-list membership, not definitions or their metadata. This is a pre-1.0 isolation/ownership baseline, not a requirement to preserve these aliases in a future new safe view.
- Updated `WhenItFails/Docs/Public-API-Stability/en.md` and `Docs/Runtime/Public-API.md`. No production code, public signatures, NuGet package version or JSON schema changed.
- **Verified locally by maintainer:** all tests GREEN, complete suite **1256/1256 GREEN** after commit `bfdf9b0b3d94978e50cf00bfe6e465f92835aaba`. Focused test output and warning count were not separately reported.
- Next: audit cross-validation result and supporting catalog documents' nested mutability/ownership before defining any additive safe-context API.

## 2026-09-25 — supporting catalog mutation and validation freshness (1259/1259 GREEN)

- Added `WhenItFails.Tests/PublicApi/SupportingCatalogLiveStateBoundaryTests.cs` with three focused tests: (1) a result from `ErrorCatalogCrossValidator` reflects validation-time category relationships and does not revalidate when the category name changes; (2) `ErrorCatalogContext.CrossValidationResult` in the live store shares its mutable issue instances across readers, allowing severity changes to flip `IsValid`; (3) an active profile definition exposes live `IncludeTags`, `DefaultMappings` and `Metadata` reachable from the same shared context reference.
- Source audit: `ErrorCatalogValidationResult.IsValid` evaluates the *currently stored issue severities* each time, not the current supporting catalog documents. Context and supporting documents remain mutable after publication. These observations are an ownership/freshness hazard baseline, **not** a new immutable/snapshot API or an endorsement of in-place mutation.
- Updated `WhenItFails/Docs/Public-API-Stability/en.md` and `Docs/Runtime/Public-API.md` to distinguish validation-time findings from later mutable state. No production code, public API signature, package version or JSON schema changed.
- **Verified locally by maintainer:** complete **1259/1259 GREEN** suite after commit `24b5d675830fbb8b57961c4e789c0dbf4736fb94`; focused output and compiler warning count were not separately reported.
- Next: provide an independently detached, additive first-stage main-definition projection without modifying `GetCurrentContext()` or the nine-method runtime interface.

## 2026-09-25 — additive detached definition snapshot API (1263/1263 GREEN)

- Added `WhenItFails/Runtime/ErrorDefinitionSnapshot.cs`, a sealed read-only data projection of the 16 fields of one `ErrorDefinition`. It copies scalar values, category/subcategory/tag lists into new read-only collections, and metadata into a new case-insensitive read-only dictionary. It does not expose the live `ErrorDefinition` or `MetadataBag`.
- Added `WhenItFails/Runtime/ErrorCatalogSnapshotExtensions.cs`: `GetErrorDefinitionSnapshots(this IErrorCatalogRuntime)` creates a new read-only list of detached definition snapshots from the active indexed catalog. It forwards unsuccessful context status/issues, handles missing context/catalog/list with stable invalid responses, and normalizes ordinary capture exceptions to a stable failure without exception details; cancellation exceptions propagate.
- Existing `GetCurrentContext()` and all nine `IErrorCatalogRuntime` methods remain unchanged. This new method is an **extension**, not a new interface member. The projection is scoped to main error definitions; it is not a deep context snapshot and cannot guarantee a transaction while live source objects are concurrently modified.
- Added `WhenItFails.Tests/PublicApi/ErrorDefinitionSnapshotContractTests.cs` with four focused tests for full read-only copy and metadata/list detachment, independence across activation, preserved uninitialized response, and missing-catalog failure. No changes to catalog normalization/indexing behavior, existing public signatures, persisted JSON schema or current package version.
- Updated `WhenItFails/README.md`, `Docs/Runtime/Public-API.md`, `Docs/Public-API-Stability/en.md`; created `Docs/Definition-Snapshots/en.md` documenting scope, caveats, usage and open 1.0 API decisions.
- **Verified locally by maintainer:** all tests GREEN, complete suite **1263/1263 GREEN** after commit `82650bc55092f6ae758c0a6e13ab35d609840094`. Focused run and compiler warning count were not separately reported. No published package version changed.
- Next: review CLR nullability/serialization shape and failure boundary of the new detached projection before planning separately versioned supporting-catalog and validation views.

## 2026-09-25 — detached snapshot public CLR and JSON surface (1267/1267 GREEN)

- Added `WhenItFails.Tests/PublicApi/ErrorDefinitionSnapshotPublicSurfaceTests.cs` with four focused tests: (1) the new `ErrorDefinitionSnapshot` has 16 getter-only public properties, no public constructor, and expected nullable-reference metadata; (2) `GetErrorDefinitionSnapshots` is an additive public static extension method without adding an `IErrorCatalogRuntime` interface method; (3) default `System.Text.Json` serialization of a detached snapshot retains captured list/metadata values even after source mutation; and (4) a malformed source definition with null metadata is normalized to a stable `WIF_ERROR_DEFINITION_SNAPSHOT_FAILED` without exposing exception details.
- Updated `WhenItFails/Docs/Definition-Snapshots/en.md` and `Docs/Public-API-Stability/en.md`: current default JSON serialization is a **pre-1.0 observation, not a versioned wire-schema promise or a claim of direct deserialization**. The public data projection is read-only and detached; the outer Essentials `Response<T>` envelope remains mutable.
- No production source, existing public method, package version or catalog JSON schema changed in this step. **Verified locally by maintainer:** all tests GREEN, complete **1267/1267 GREEN** after commit `8cf6ef76c363587f6455d1f178708020d99081fa`. Focused test output and compiler warning count were not separately reported.
- Next: independently design a safe, detached supporting-catalog and validation view without exposing mutable raw documents, or promising an atomic deep snapshot while live in-place writers remain possible.

## 2026-09-25 — additive detached cross-validation snapshot (1272/1272 GREEN)

- Added `WhenItFails/Runtime/ErrorCatalogValidationIssueSnapshot.cs`, a sealed getter-only projection of the six issue fields (`Severity`, `Code`, `Message`, `ErrorId`, `ErrorName`, `Path`), without retaining the mutable source issue.
- Added `WhenItFails/Runtime/ErrorCatalogValidationSnapshot.cs`, a sealed getter-only validation projection with an independent read-only list of issue snapshots. `IsValid` is calculated from **captured** issue severities, so subsequent edits of live source issues do not change an existing snapshot.
- Added `WhenItFails/Runtime/ErrorCatalogValidationSnapshotExtensions.cs`: `GetCrossValidationSnapshot(this IErrorCatalogRuntime)` is an **additive extension** returning the active context's previously recorded findings. It does not change the nine-method runtime interface, call the validator again, or provide a transactional capture during concurrent in-place mutations. An invalid runtime response propagates without data; missing context/validation and ordinary capture exceptions produce stable codes without exception detail; cancellation propagates.
- Added `WhenItFails.Tests/PublicApi/ErrorCatalogValidationSnapshotContractTests.cs` with **five** focused tests for issue/value detachment and live-mutation isolation, empty/error validity, uninitialized-context response, missing validation and getter-only public shapes. Updated root README, `Docs/Runtime/Public-API.md`, `Docs/Public-API-Stability/en.md`, and created `Docs/Validation-Snapshots/en.md` (English) covering scope, freshness and ownership limitations.
- **Verified locally by maintainer:** all tests GREEN, complete suite **1272/1272 GREEN** after commit `cb7ddf92311c0e82083185d0d134cdf9ca18891c`. Focused test output and compiler warning count were not separately reported. No published package version, existing public interface methods, source JSON schema or catalog behavior changed.
- Next: start a detached supporting-catalog projection and specify a coherent ownership/identity boundary, explicitly distinguishing independently captured views from a single context generation.

## 2026-09-25 — detached category catalog snapshot and context identity boundary (1278/1278 GREEN)

- Added `WhenItFails/Runtime/ErrorCategoryDefinitionSnapshot.cs` (eight getter-only fields) and `WhenItFails/Runtime/ErrorCategoryCatalogSnapshot.cs` (11 getter-only document fields). Nested aliases, parent categories, default tags, document tags, default mappings and document/definition metadata are copied into separately allocated read-only collections/dictionaries; metadata retains case-insensitive key lookup and default mappings retain their source dictionary comparer. Source definitions, documents and `MetadataBag` instances do not escape through the projected data.
- Added `WhenItFails/Runtime/ErrorCategoryCatalogSnapshotExtensions.cs` with the additive `GetCategoryCatalogSnapshot(this IErrorCatalogRuntime)` method. The method reads the active context once, captures a detached category catalog, forwards uninitialized context failures, rejects absent catalog with `WIF_CATEGORY_SNAPSHOT_CATALOG_NULL`, and normalizes ordinary capture errors with `WIF_CATEGORY_SNAPSHOT_FAILED` without exception detail. Cancellation exceptions propagate. The nine-method runtime interface and existing `GetCurrentContext()` behavior are unchanged.
- Added six focused tests in `WhenItFails.Tests/PublicApi/ErrorCategoryCatalogSnapshotContractTests.cs`: detachment of all nested collection/reference types; retention across context replacement with a single context read per capture; uninitialized failure; missing category; malformed nested source failure; and sealed getter-only CLR shape. Added `WhenItFails/Docs/Category-Snapshots/en.md` and updated README, `Docs/Runtime/Public-API.md`, and `Docs/Public-API-Stability/en.md`.
- **Context identity decision pending:** independently obtained category/definition/validation snapshots may come from different activations. A future combined snapshot must derive them from one selected active-context reference; a stable activation-generation ID additionally needs runtime publication ownership, not a random ID generated per capture. Neither one-read capture nor atomic reference replacement guarantees a transaction against concurrent in-place mutation of that context.
- **Verified locally by maintainer:** all tests GREEN, complete **1278/1278 GREEN** after commit `6d14bddfb70bb2db43d0e0d3ac5e2059e1ebaef1`. Focused run details and compiler warning count were not separately reported. No change to the existing package version or persistent JSON schema.
- Next: design a combined capture of main definitions, recorded validation and category catalog from one context selection; specify activation identity/lifecycle separately before claiming cross-call generation coherence.

## 2026-09-25 — combined selected-reference snapshot (1286/1286 GREEN)

- Added `WhenItFails/Runtime/ErrorCatalogCombinedSnapshot.cs`, a sealed getter-only projection with three properties: `Definitions` (detached `ErrorDefinitionSnapshot` list), `CategoryCatalog` (detached `ErrorCategoryCatalogSnapshot`), and `Validation` (detached `ErrorCatalogValidationSnapshot`). It intentionally excludes owner/code-group/profile catalogs, independent runtime status, and a stable activation-generation ID.
- Added `WhenItFails/Runtime/ErrorCatalogCombinedSnapshotExtensions.cs`: the additive `GetCombinedSnapshot(this IErrorCatalogRuntime)` calls `GetCurrentContext()` once, selects **one context reference**, checks all three required source components and projects them without invoking independently reading snapshot extensions. Missing context/required catalog/recorded validation produces stable Invalid codes without partial data; ordinary capture failures produce `WIF_COMBINED_SNAPSHOT_FAILED` without exception details; cancellation propagates. The existing nine-method runtime interface and `GetCurrentContext()` behavior remain unchanged.
- Added `WhenItFails.Tests/PublicApi/ErrorCatalogCombinedSnapshotContractTests.cs`: five `[Fact]` tests and one three-case `[Theory]` = **eight test cases** for nested definition/category/issue detachment, one-read selection while the runtime changes contexts, uninitialized status forwarding, absent required components, malformed source, and getter-only CLR shape/interface compatibility.
- Created `WhenItFails/Docs/Combined-Snapshots/en.md` and updated root README, `Docs/Runtime/Public-API.md` and `Docs/Public-API-Stability/en.md` to distinguish **single selected-reference consistency** from both a transaction against in-place mutation and a durable activation-generation ID. No owner/code-group/profile snapshot, runtime status pairing, published package version or persisted JSON schema changed.
- **Verified locally by maintainer:** complete **1286/1286 GREEN** suite after commit `e3527b703740181e2a6212eac3d3a5faa02a6bbd`. Focused run details and compiler warning count were not separately reported.
- Next: establish publication identity at the context store boundary; analyze reinitialization/fallback/retained-previous-context semantics without treating per-call capture IDs as context generations.

## 2026-09-25 — atomic context publication identity (1293/1293 GREEN)

- Source audit: the default `ErrorCatalogInitializer` calls `IErrorCatalogContextStore.Set` before the runtime records status after normal initialization; explicit reset and automatic built-in fallback also publish a context before recording status. Retained-previous-context recovery records status **without** calling `Set`. Therefore a store publication ID is not automatically a synchronized context-and-runtime-status activation ID.
- Added `WhenItFails/Runtime/ErrorCatalogContextPublication.cs`: an immutable three-property publication record (`StoreId: Guid`, `Generation: long`, `Context: ErrorCatalogContext`). `Context` remains **live and mutable**; this low-level record is not a safe consumer snapshot. Store IDs are per store instance and generations start at 1 within that store.
- Added optional `WhenItFails/Interfaces/IErrorCatalogContextPublicationReader.cs`, implemented by the default `ErrorCatalogContextStore` without modifying `IErrorCatalogContextStore` or its existing public members. `GetCurrentPublication()` atomically obtains the current context+generation record, preserving the established pre-initialization invalid response.
- Changed only the default store's internal publication slot: `Set` uses a `CompareExchange` loop over the entire immutable publication record, incrementing generation in successful publication order even under concurrent writers. Publishing an identical context reference still increments generation. `GetCurrent()`, `Current` and `IsInitialized` preserve their existing behavior, now derived from the record.
- Added seven focused tests in `WhenItFails.Tests/PublicApi/ContextPublicationGenerationContractTests.cs`: uninitialized reads, sequential publication and retained old record, repeated identical reference, rejected null Set, concurrent publication order, independent store scopes and additive CLR/interface shape.
- Created `WhenItFails/Docs/Context-Publication/en.md`; updated the WhenItFails README, `Docs/Runtime/Public-API.md` and `Docs/Public-API-Stability/en.md`. **Verified locally by maintainer:** complete **1293/1293 GREEN** suite after commit `df27488d292e85f9e20077e533bc021bc3990e9a`. Focused run details and compiler warning count were not separately reported. Existing runtime interface, published NuGet version and persisted catalog JSON schema unchanged.
- Next: surface the store publication ID on a separate combined detached snapshot for the default runtime without fabricating IDs for custom stores, and separately design status/context synchronization and retained-previous-context recovery semantics.

## 2026-09-25 — publication-aware combined snapshot (verification pending)

- Added optional `WhenItFails/Interfaces/IErrorCatalogRuntimePublicationReader.cs` without adding any member to the existing nine-method `IErrorCatalogRuntime`. The default `ErrorCatalogRuntime` implements it by delegating `GetCurrentPublication()` to its injected context store when that store implements `IErrorCatalogContextPublicationReader`. Unsupported custom stores return NotSupported with `WIF_CONTEXT_PUBLICATION_NOT_SUPPORTED`; ordinary exceptions are normalized, cancellation propagates.
- Added sealed getter-only `WhenItFails/Runtime/ErrorCatalogPublishedCombinedSnapshot.cs` with `StoreId`, `Generation` and `Snapshot: ErrorCatalogCombinedSnapshot`. The returned consumer data contains detached main definitions, categories and recorded validation; it does **not** expose the live context publication record.
- Added additive `WhenItFails/Runtime/ErrorCatalogPublishedCombinedSnapshotExtensions.cs`: `GetPublishedCombinedSnapshot(this IErrorCatalogRuntime)` reads a **single actual** publication from the optional runtime capability and derives detached data from the context inside that selected record. It does not call `GetCurrentContext()` or `GetStatus()`, and never fabricates generation IDs for custom runtimes/stores. Unsupported capability, uninitialized publication, null responses and missing source components produce structured non-success results without partial snapshot data.
- Refactored existing `ErrorCatalogCombinedSnapshotExtensions.GetCombinedSnapshot()` to share an internal `CaptureFromContext()` projection helper. Existing public signature, single-context-read behavior, missing-component codes, and detached data shape are preserved. The store publication ID remains distinct from any synchronized runtime status/activation identifier; no status pairing or guarantee against in-place mutations is claimed.
- Added seven focused tests in `WhenItFails.Tests/PublicApi/PublishedCombinedSnapshotContractTests.cs`: real default runtime/store identity (even with unavailable runtime status), one-publication selection across replacement, unsupported custom runtime/store, uninitialized store, absent required catalog and getter-only public surface. Added `WhenItFails/Docs/Published-Snapshots/en.md` and updated README and runtime, combined snapshot, publication identity and public API review documentation.
- **Verified locally by maintainer:** the first focused run stopped before test execution with two CS7036 errors because `Response<T>.NotSupported` requires a `data` argument. Added `data: null` in `ErrorCatalogRuntime.GetCurrentPublication()` and `ErrorCatalogPublishedCombinedSnapshotExtensions.GetPublishedCombinedSnapshot()` (commits `a519d6d3fbfc8f43c905d1d1f8eb77f43cf43f59` and `dd9b365bdad84f651a6da71f5c279b002784be3a`). After the correction, the maintainer confirmed all tests GREEN, complete **1300/1300 GREEN** suite. Focused output and compiler warning count were not separately reported. Existing runtime/store interface members, package version and persisted catalog JSON schemas are unchanged.
- Next: verify focused/full suite, then audit the runtime status/context publication window and retained-previous-context recovery as a separate synchronization contract before exposing any activation identity.

## 2026-09-25 — runtime status / publication lifecycle boundary (1306/1306 GREEN)

- Audited `ErrorCatalogInitializer`, `ErrorCatalogRuntime.InitializeCoreAsync`, `ResetToDefaultsAsync`, previous-context recovery and built-in fallback against the real default publication-aware store. A successful project initialization calls `Set` inside the initializer **before** `ErrorCatalogRuntime.RecordStatus` runs. Reset/fallback also publish before recording status. Flexible recovery with an existing context calls `RecordStatus` **without** calling `Set`, so the status can change with a fixed generation.
- Added `WhenItFails.Tests/PublicApi/ContextPublicationStatusLifecycleContractTests.cs` containing **six** focused tests: successful project publication/status, strict-failure preservation, flexible previous-context recovery with the same publication and a new status, built-in fallback then explicit reset (two generations), failed explicit reset preservation, and a deterministic callback capturing the newly published context **before** the runtime updates the previous status. The last test deliberately demonstrates a possible mismatched context/status read; it is not a timing-dependent concurrency assertion.
- Updated `WhenItFails/Docs/Context-Publication/en.md` with the six lifecycle behaviors and the required design distinction. Updated `Docs/Public-API-Stability/en.md` and `Docs/Published-Snapshots/en.md` with the maintainer-confirmed **1300/1300 GREEN** checkpoint after the earlier two-argument `NotSupported(data: null)` correction.
- **Verified locally by maintainer:** complete **1306/1306 GREEN** suite after commit `eb11bffc37ff7b1a5a3b6345ce56fb51cbef7352`. Focused run details and compiler warning count were not separately reported. No production code, public interfaces, published package version, or persisted catalog JSON schemas changed in this step.
- Next: confirm focused and full test suite, then design a separate activation/status association mechanism that does not infer status from the store generation alone and does not falsely claim an atomic read while custom stores or direct external `Set` calls can update the publication independently.

## 2026-09-25 — completed runtime activation status observation (1314/1314 GREEN)

- Added sealed getter-only `WhenItFails/Runtime/ErrorCatalogActivationStatusSnapshot.cs`: the public projection carries the real `StoreId`, `Generation`, a separate runtime-local `ActivationSequence`, and the recorded `ErrorCatalogRuntimeStatus`. The public data exposes no live catalog context.
- Added optional `WhenItFails/Interfaces/IErrorCatalogRuntimeActivationReader.cs` and implemented `GetCompletedActivation()` in the default `ErrorCatalogRuntime`. Existing nine-method `IErrorCatalogRuntime`, its constructor and `GetStatus()` remain unchanged. Custom stores lacking publication identity return NotSupported with a required `data: null` response argument. The publication check explicitly matches a non-null successful response before comparing the selected publication record (`27c99770e1c6f8e88a4e42e0543a8bf4e3c7495d`).
- After `RecordStatus` validates and publishes the legacy status, the runtime attempts an optional completed observation only when its store publication contains the exact payload context reference. The observation records the selected immutable publication record together with that status, incrementing a runtime-local sequence on successful matches. Its reader checks that the recorded status remains current and the observed store publication is still the selected record; absent, pending or changed matches produce structured Invalid results instead of fabricated identity. Optional observation failure must not change the existing initialization, reset or recovery outcome.
- Flexible previous-context recovery retains the store publication while generating a new status and incrementing `ActivationSequence`. Strict failure and failed reset leave both unchanged. A direct external `Set`, including one publishing the same context reference again, invalidates the selected completed observation when its publication record no longer matches.
- Added `WhenItFails.Tests/PublicApi/CompletedActivationStatusContractTests.cs` with **eight** focused test cases: uninitialized/legacy store, two successful activations, recovery without a new generation, external same-reference republish, publication-before-status window, failed strict initialization/reset, fallback followed by explicit reset, and getter-only/additive public surface.
- Created `WhenItFails/Docs/Activation-Status/en.md` and updated README, `Docs/Runtime/Public-API.md`, `Docs/Context-Publication/en.md` and `Docs/Public-API-Stability/en.md`. **Verified locally by maintainer:** complete **1314/1314 GREEN** suite after commit `195c45fb99447c19716eb641b820beacb1bf70ec`. Focused run details and compiler warning count were not separately reported. Published package version and persisted catalog JSON schemas unchanged.
- **Important limit:** this API is a selected completed status/publication **observation**, not an atomic live context/status transaction. A direct external store write immediately after validation or overlapping runtime initialization (particularly re-publishing the same object) can invalidate strict event ownership; separate `GetStatus()` and published combined-snapshot calls are never automatically paired. Next: after verification, define lifecycle serialization or an owned activation protocol before exposing stronger multi-view coherence.

## 2026-09-25 — serialize default runtime activations (1319/1319 GREEN)

- Added a private instance-local `SemaphoreSlim(1,1)` gate to `WhenItFails/Services/ErrorCatalogRuntime.cs`, acquired with `WaitAsync(cancellationToken)` by both `InitializeAsync` overloads (through `InitializeCoreAsync`) and `ResetToDefaultsAsync`. Existing activation logic remains in private core methods; the public entry points release the gate in `finally`, including when the inner operation fails or propagates cancellation. This preserves the nine-method `IErrorCatalogRuntime` interface and all original entry-point signatures while preventing two activations on the **same** default runtime instance from overtaking each other's publication/status recording.
- Added five deterministic focused tests in `WhenItFails.Tests/PublicApi/RuntimeActivationSerializationContractTests.cs` with `TaskCompletionSource` barriers: two concurrent initializations; reset before initialize; initialize before reset; cancellation while initialization is queued; and strict failure releasing the gate for the next initialization. The tests check real default-store publication generation and completed activation status sequence rather than relying on artificial sleeps. All new tests and regression suite **await maintainer verification**.
- Updated `WhenItFails/Docs/Activation-Status/en.md`, `Docs/Runtime/Initialization-and-Recovery.md`, and `Docs/Public-API-Stability/en.md` to record the instance-local, cancellable, **non-reentrant** nature of the gate. Ordinary context/descriptor/status/snapshot readers do **not** acquire this gate. Direct external `Set` calls, custom runtimes and distinct runtime instances sharing a store remain outside its scope; context publication and status still occur in separate writes, so the read-side publication-before-status window remains.
- **Verified locally by maintainer:** all tests GREEN, complete **1319/1319 GREEN** after commit `11d82973005c1d330baecf0bf0ba9bdeaa8a6bad`. Focused test output and compiler warning count were not separately reported. Existing public interface signatures, published 0.1.0 package version and persisted catalog JSON schemas unchanged. Production operation ordering is intentionally changed only for concurrent activations on one default runtime instance.
- Next: verify focused/full suite; then audit direct-store writes/competing runtime instances and design an owned publication protocol or a coherent combined read before promising strict activation-event identity across all writers.

## 2026-09-25 — shared-store multi-runtime publication audit (1324/1324 GREEN)

- Added `WhenItFails.Tests/PublicApi/SharedStoreRuntimePublicationBoundaryTests.cs` with **five** deterministic tests using the real default publication-aware store and controlled initializer doubles: (1) a second runtime publishing a different context invalidates the first runtime's selected completed observation; (2) a paused first runtime can complete its status after a second runtime publishes a different context because the activation gate is instance-local; (3) a direct external `Set` of a different context between publication and status prevents a matching completed observation; (4) a strict failure on another runtime preserves the previous publication and status observation; (5) two runtime instances can report different local project/recovery statuses for the **same** unchanged `(StoreId, Generation)` pair.
- Source review identified an **unresolved same-reference ownership defect**: an external writer can call `Set` on the exact same context object after another runtime operation's publication but before its `RecordStatus`. The current reference-equality match can attribute the later externally owned generation to the first runtime operation. The default instance-local gate does not cover other runtimes/direct store writers; the current `GetCompletedActivation()` cannot promise strict event ownership or a globally atomic data/status transaction.
- Created `WhenItFails/Docs/Shared-Store-Concurrency/en.md` with precise lifecycle and ownership limitations, plus updated README, `Docs/Activation-Status/en.md`, `Docs/Runtime/Public-API.md` and `Docs/Public-API-Stability/en.md`. This is a contract/hazard audit, **not** a new ownership implementation. No production source, public API signatures, published package version or persisted JSON schemas changed.
- **Verified locally by maintainer:** all tests GREEN, complete **1324/1324 GREEN** suite after commit `077e77101b45ee379b92225a6c05a8d83c3aaf9c`. Focused output and compiler warning count were not separately reported.
- Next: verify focused/full tests and design a real publication-ownership token returned by the exact successful store write, with a backward-compatible optional publisher and a clear bridge to default initializer/runtime status; retain an explicit weaker contract for legacy stores and independently writing runtimes. Do **not** infer ownership from object reference identity alone.

## 2026-09-25 — exact atomic store publication ownership (1330/1330 GREEN)

- Added `WhenItFails/Interfaces/IErrorCatalogContextPublisher.cs`, an additive optional contract `Publish(ErrorCatalogContext): ErrorCatalogContextPublication` that returns **the exact immutable publication record created by this successful write**. It exposes a live context reference to infrastructure callers; it is not a detached consumer projection.
- The default `ErrorCatalogContextStore` now implements this optional publisher. `Publish` returns the record that won the atomic compare/exchange, rather than reading the current record after the write; concurrent writers cannot retarget the returned record. Legacy `Set` retains its existing void signature and delegates to `Publish`, discarding the return value. Both write paths advance the same store-scoped generation. Publishing the **same** context instance twice still yields distinct publication records; null writes do not publish.
- Added `WhenItFails.Tests/PublicApi/ExactContextPublicationOwnershipContractTests.cs` with **six** focused tests: exact successful record, later same-reference republish, legacy/new write sequence, 64 concurrent publishers with distinct generations, null rejection and unchanged legacy interface/optional publisher CLR shape. Created `WhenItFails/Docs/Publication-Ownership/en.md` and updated README, `Docs/Context-Publication/en.md`, `Docs/Shared-Store-Concurrency/en.md`, `Docs/Runtime/Public-API.md` and `Docs/Public-API-Stability/en.md` (English).
- **Verified locally by maintainer:** all tests GREEN, complete **1330/1330 GREEN** suite after commit `5633ef3a887be82713a60560db3f983500c6a9cb`. Focused output and compiler warning count were not separately reported. The new optional interface and default-store behavior are additive; original `IErrorCatalogContextStore` members, published package version and persisted JSON schema are unchanged.
- **Important:** this solves **store-level** publication ownership only. The default initializer currently calls legacy `Set` and its payload has no owned-publication field; the runtime still infers completed-observation ownership through reference equality. A competing same-reference writer can therefore still create a wrongly attributed runtime observation until the exact token is explicitly carried from each owning write to the status-completion path.
- Next: verify focused/full suite, then add an optional, backwards-compatible bridge from default initializer and runtime's own reset/fallback publications to `RecordStatus`. Define a weaker explicit result for legacy/custom initializers and stores lacking a true owned publication record; never treat a later publication read as proof of write ownership.

## 2026-09-25 — bridge exact owned publication to default activation paths (1336/1336 GREEN)

- The default `WhenItFails/Initialization/ErrorCatalogInitializer.cs` now calls optional `IErrorCatalogContextPublisher.Publish(context)` and captures **that successful write's record**. On legacy stores without the optional publisher it still invokes `Set(context)`. Its successful `ErrorCatalogInitializationPayload` retains the record in a new **internal-only** `OwnedPublication` property; the property is not part of the public payload API or default JSON serialization.
- The default `WhenItFails/Services/ErrorCatalogRuntime.cs` uses the same optional publisher for its own explicit reset and automatic built-in fallback writes, passing each returned exact record through the corresponding payload. `RecordStatus` uses `payload.OwnedPublication` when present instead of rereading and potentially attributing a different writer's same-reference publication. If the current store publication has since changed, `GetCompletedActivation()` returns `WIF_ACTIVATION_PUBLICATION_CHANGED`, not a falsely matched status/generation. Previous-context recovery and custom initializer payloads without an owned record retain a documented **best-effort** reference-based association; the strict ownership claim applies only to the default owned write paths.
- Added **six** focused tests in `WhenItFails.Tests/PublicApi/OwnedPublicationActivationBridgeContractTests.cs`: exact default initializer token with non-public/non-JSON payload shape, ordinary default initializer/runtime success, deterministic same-reference republish after the default initializer write, the same scenario for explicit reset and automatic fallback, and legacy `Set`-only initializer/runtime compatibility. The interfering store returns the original winning record while independently publishing the same context again, reproducing the previously misattributed generation without timing-dependent races.
- Updated `WhenItFails/Docs/Publication-Ownership/en.md`, `Docs/Activation-Status/en.md`, `Docs/Shared-Store-Concurrency/en.md`, `Docs/Runtime/Public-API.md` and `Docs/Public-API-Stability/en.md` to explain owned versus best-effort paths and the remaining non-atomic reader/status window.
- **Verified locally by maintainer:** full **1336/1336 GREEN** suite after commit `1af510a8989322d20841a3dd16e36538f6c6e1f5`. Focused test output and compiler-warning count were not separately reported. Published NuGet version, original public interfaces, public initialization payload surface and persisted JSON schemas are unchanged.
- Next: confirm focused/full suite; then design exact **selection** of a prior store publication for previous-context recovery and an optional custom-initializer ownership contract, without inventing identities for stores lacking the capability. Also examine competing writers that replace a selected publication after status is recorded before designing a globally coherent data/status read.

## 2026-09-25 — exact previous-context publication selection (1341/1341 GREEN)

- Changed the default `ErrorCatalogRuntime.InitializeCoreLockedAsync` flexible recovery path: when the store implements `IErrorCatalogContextPublicationReader`, it selects **the context and its existing publication record in a single `GetCurrentPublication()` read**. `CreatePreviousContextRecoveryResponse` retains the exact selected record in a new **internal-only**, non-JSON `ErrorCatalogInitializationPayload.SelectedPublication` property. Unlike `OwnedPublication`, this is an already existing publication selected for **no-write** recovery; it does not advance the generation.
- `RecordStatus` now uses `payload.OwnedPublication ?? payload.SelectedPublication` for exact write/selection paths, never replacing either with a later read that might belong to another writer. After a competing same-reference or different-context publication, `GetCompletedActivation()` rejects the displaced selected publication with `WIF_ACTIVATION_PUBLICATION_CHANGED` rather than attributing the newer generation to the recovery status. An unavailable, failing or throwing optional reader preserves the original legacy `GetCurrent()` recovery path and its **weaker reference-based association**. The optional reader's ordinary exception is caught without altering recovery outcome; cancellation propagates.
- Added `WhenItFails.Tests/PublicApi/PreviousContextPublicationSelectionContractTests.cs` with **five** focused cases: no-write recovery from a prior project activation (same generation, advanced runtime status sequence and internal/non-JSON selected identity), same-reference external republish after selection, different-context external republish after selection, legacy store compatibility and optional reader exception fallback. The two interfering-writer cases use deterministic read-boundary hooks rather than timing-dependent concurrency assertions.
- Created `WhenItFails/Docs/Recovery-Selection/en.md` and updated README, `Docs/Activation-Status/en.md`, `Docs/Publication-Ownership/en.md`, `Docs/Shared-Store-Concurrency/en.md`, `Docs/Runtime/Public-API.md` and `Docs/Public-API-Stability/en.md`. **Verified locally by maintainer:** all tests GREEN, complete **1341/1341 GREEN** after commit `54a526d9be6eef63de34c3fa49eabfe35e10518e`. Focused output and compiler warning count were not separately reported. Existing public interfaces/signatures, public payload JSON shape, published 0.1.0 package and persisted catalog schemas unchanged.
- **Remaining limits:** recovery selection does not take ownership of the store or block external writers; an earlier selected context can be replaced before the recovery response is returned. `GetCompletedActivation()` checks the current publication at read time, but an external writer can still replace it immediately afterwards. A coherent, detached combined status/catalog read and stronger custom-initializer ownership semantics remain separate design steps.
- Next: verify focused/full suite; then review an additive runtime reader that captures matched completed status and combined detached catalog data from one selected publication record without a second independent context/status selection, while rejecting changed/missing generations and retaining explicit non-transactional external-mutation limits.

## 2026-09-25 — matched completed status and detached combined catalog (1347/1347 GREEN)

- Added `WhenItFails/Runtime/ErrorCatalogCompletedCombinedSnapshot.cs`, a sealed getter-only projection carrying `StoreId`, `Generation`, runtime-local `ActivationSequence`, the recorded `ErrorCatalogRuntimeStatus` and an `ErrorCatalogCombinedSnapshot` (detached main definitions, categories and recorded validation). Its data does not expose the live `ErrorCatalogContext`; the outer Essentials `Response<T>` retains its existing mutable semantics.
- Added optional `WhenItFails/Interfaces/IErrorCatalogRuntimeCombinedObservationReader.cs` and implemented `GetCompletedCombinedSnapshot()` on the default runtime. The nine-method `IErrorCatalogRuntime` remains unchanged. The method selects one previously recorded `CompletedActivation` and its **exact associated publication record**, verifies its legacy status and current store identity, copies all three catalog projections from that **same selected context**, and rechecks the store publication and selected status before returning. The second publication read only checks the selected record; it does not independently select new source data. Changed status/publication, missing completion, unsupported store, invalid component or ordinary capture exceptions produce structured non-success responses with no partially returned data.
- Added **six** focused cases in `WhenItFails.Tests/PublicApi/CompletedCombinedSnapshotContractTests.cs`: matching default reset and detached data, previous-context recovery with unchanged generation/advanced status sequence, external same-reference republish rejection, deterministic external store write detected by the second publication read, missing required category failure and optional getter-only/legacy-store compatibility. Created `WhenItFails/Docs/Completed-Combined-Snapshots/en.md` and updated README, `Docs/Runtime/Public-API.md`, `Docs/Activation-Status/en.md`, `Docs/Combined-Snapshots/en.md` and `Docs/Public-API-Stability/en.md`.
- **Verified locally by maintainer:** the initial run stopped with CS7036 because the test helper needed `builtIn`; adding `new FixedBuiltInProvider(Context("UNUSED-FALLBACK"))` (commit `1d35823508fc62e8560ad74d1ce13f166c99e4f9`) resolved the build failure. The maintainer subsequently confirmed all tests GREEN, complete **1347/1347 GREEN** after commit `1f87c43de877f6626ccaf2fda2107ebf5ccaca71`. Focused output and compiler-warning count were not separately reported. No production code or public API was changed by the correction; published package version and persisted catalog JSON schemas unchanged.
- **Consistency limits:** this is a **checked observation**, not an atomic transaction or a lock on external writers; a new publication can occur immediately after the final check, and live nested context objects may still be mutated during copy without changing generation. Existing custom initializer paths lacking owned publication tokens retain the previously documented weaker association. No current-state guarantee is promised for later use of a returned snapshot.
- Next: verify focused/full suite and compiler warnings; then plan an optional stronger lifecycle-owned, read-consistent publication/status protocol if applications require transaction-like semantics, or continue detached snapshot coverage of remaining supporting catalogs without making unsupported atomicity claims.

## 2026-09-25 — detached owner catalog snapshot (1353/1353 GREEN)

- Added `WhenItFails/Runtime/ErrorOwnerDefinitionSnapshot.cs` (all **nine** owner-definition fields) and `WhenItFails/Runtime/ErrorOwnerCatalogSnapshot.cs` (all **11** owner-catalog-document fields). Each type is sealed and getter-only. Owner definitions, aliases, catalog tags, default mappings and document/definition metadata are copied into separately allocated read-only collections/dictionaries, not shared with mutable source objects. Default mappings retain the source dictionary comparer and metadata retains case-insensitive keys.
- Added `WhenItFails/Runtime/ErrorOwnerCatalogSnapshotExtensions.cs` with additive `GetOwnerCatalogSnapshot(this IErrorCatalogRuntime)`. It selects a single active context response and returns the detached supporting owner catalog, forwarding unavailable runtime responses without data and handling missing owner catalogs and malformed source with stable error codes (`WIF_OWNER_SNAPSHOT_CATALOG_NULL`, `WIF_OWNER_SNAPSHOT_FAILED`) without exception details or partial data. It does not change `IErrorCatalogRuntime` or either existing combined snapshot's data shape.
- Added `WhenItFails.Tests/PublicApi/ErrorOwnerCatalogSnapshotContractTests.cs` with **six** focused cases: deep detachment and getter-only nested lists/dictionaries, one context read and independence after replacement, uninitialized response, missing owner catalog, malformed nested aliases, and sealed/additive public CLR shape.
- Added English `WhenItFails/Docs/Owner-Snapshots/en.md`; updated `WhenItFails/README.md`, `Docs/Runtime/Public-API.md` and `Docs/Public-API-Stability/en.md`. Independent owner and combined snapshot calls are **not** claimed to select the same generation; copying cannot prevent concurrent in-place mutation of an active context. Published 0.1.0 package and persisted catalog JSON schemas remain unchanged.
- **Verified locally by maintainer:** complete **1353/1353 GREEN** suite after commit `297975a2b3adf08a3c9922664201ec03a5b2d156`. Focused output and compiler warning count were not separately reported.
- Next: confirm focused/full suite, then separately implement detached code-group and profile catalog views before considering an explicitly versioned full-context snapshot with generation and status semantics.

## 2026-09-25 — detached supporting code group catalog snapshot (1359/1359 GREEN)

- Added `WhenItFails/Runtime/ErrorCodeGroupDefinitionSnapshot.cs` (all **ten** code group definition fields) and `WhenItFails/Runtime/ErrorCodeGroupCatalogSnapshot.cs` (all **11** code group catalog document fields). Both are sealed and getter-only; tags, default categories, code groups, mappings and metadata are copied into independently allocated read-only collections/dictionaries. Mapping keys retain the source comparer and metadata keys remain case-insensitive.
- Added `WhenItFails/Runtime/ErrorCodeGroupCatalogSnapshotExtensions.cs` with additive `GetCodeGroupCatalogSnapshot(this IErrorCatalogRuntime)`. It selects one context, forwards uninitialized failure without data, rejects missing code group catalog with `WIF_CODE_GROUP_SNAPSHOT_CATALOG_NULL`, and normalizes malformed source with `WIF_CODE_GROUP_SNAPSHOT_FAILED` without exposing exception details or partial data. Cancellation propagates. No existing runtime interface methods or combined snapshot data shape changed.
- Added `WhenItFails.Tests/PublicApi/ErrorCodeGroupCatalogSnapshotContractTests.cs` with **six** focused tests: deep detachment and read-only nested collections, one context read and replacement isolation, uninitialized response, missing document, malformed nested categories and getter-only/additive public shape. Added English `WhenItFails/Docs/Code-Group-Snapshots/en.md`; updated README, `Docs/Runtime/Public-API.md` and `Docs/Public-API-Stability/en.md`.
- **Verified locally by maintainer:** complete **1359/1359 GREEN**, without compiler warnings or errors, after commit `14fbc489a8b28e15f2bae6bddc64490c9af3bb6c`. Focused test output was not separately reported. Published 0.1.0 NuGet package and persistent catalog JSON schemas unchanged.
- Next: verify focused/full suite. Then implement a separately versioned detached profile catalog projection (including all its nested collections), before discussing any optional full-context view from a single selected publication. No atomic snapshot against external in-place mutation is claimed.

## 2026-09-25 — detached supporting profile catalog snapshot (1365/1365 GREEN)

- Added `WhenItFails/Runtime/ErrorProfileDefinitionSnapshot.cs` capturing all **14** public definition fields, including all eight independent include/exclude filter lists, source, mappings and metadata. Added `WhenItFails/Runtime/ErrorProfileCatalogSnapshot.cs` capturing all **11** public catalog-document fields. Both projections are sealed and getter-only, with separately allocated read-only filter lists, document tags, profile definitions, mappings and metadata. Default mappings retain their original dictionary key comparer; metadata remains case-insensitive.
- Added `WhenItFails/Runtime/ErrorProfileCatalogSnapshotExtensions.cs`: additive `GetProfileCatalogSnapshot(this IErrorCatalogRuntime)` selects a single active-context response, forwards unsuccessful context results without data, returns Invalid on missing profile catalog (`WIF_PROFILE_SNAPSHOT_CATALOG_NULL`), and converts malformed nested values or ordinary capture exceptions into `WIF_PROFILE_SNAPSHOT_FAILED` without exception details or partial result data. Cancellation propagates. The original runtime interface and existing combined snapshot shapes remain unchanged.
- Added `WhenItFails.Tests/PublicApi/ErrorProfileCatalogSnapshotContractTests.cs` with **six** focused cases: every filter and nested collection detached and read-only, one active-context read per call and retained prior values after replacement, uninitialized response, missing document, malformed nested filters, and sealed/getter-only CLR surface without altering `IErrorCatalogRuntime`.
- Created English `WhenItFails/Docs/Profile-Snapshots/en.md`; updated `WhenItFails/README.md`, `Docs/Runtime/Public-API.md` and `Docs/Public-API-Stability/en.md`. This finishes the independently callable detached projections for category, owner, code group and profile supporting catalogs. Separate calls can still select different context generations; no atomicity against concurrent in-place mutation or stable JSON wire contract is claimed.
- **Verified locally by maintainer:** six focused cases and full **1365/1365 GREEN**, zero errors (warning count not separately reported for this checkpoint). Source development does not change the published NuGet 0.1.0 version or persistent catalog JSON schemas.
- Next: confirm focused/full suite. Then design an additive, explicitly scoped combined *all-supporting-catalog* capture from one selected publication with full failure/ownership semantics, without silently changing existing three-part combined snapshot types. Decide separately whether a versioned transport DTO is warranted.

## 2026-09-25 — all-supporting-catalog detached capture (1377/1377 GREEN)

- Added `ErrorSupportingCatalogsSnapshot` (sealed, getter-only) and additive `GetSupportingCatalogsSnapshot(this IErrorCatalogRuntime)`. The extension selects the active context once and copies the four supporting category, owner, code-group and profile catalogs using their existing detached projections. No main definitions, validation findings, publication generation or runtime status are implied.
- Missing catalogs return catalog-specific Invalid responses without partial data; null context responses, unsuccessful context forwarding, stable capture failure and cancellation propagation follow established snapshot contracts. Concurrent in-place mutation of the selected context remains unsupported.
- Added `WhenItFails.Tests/PublicApi/ErrorSupportingCatalogsSnapshotContractTests.cs` with **12 theory-expanded focused cases** and English `Docs/Supporting-Catalog-Snapshots/en.md`; updated README, runtime API, public API review and profile docs. Existing combined snapshot and runtime interface shapes are unchanged. Published NuGet 0.1.0 and persisted catalog JSON schemas are unchanged.
- **Verified locally by maintainer:** full **1377/1377 GREEN** with no reported errors; focused test and compiler-warning counts were not separately reported for this checkpoint.
- Next: confirm focused/full suite locally, then decide whether a publication-aware all-catalog read or separately versioned transport DTO is needed. Do not infer a generation or atomic status from this single-context-reference capture.

## 2026-09-25 — publication-aware supporting catalogs (1392/1392 GREEN)

- Refactored the existing context-only supporting projection to reuse an internal `CaptureFromContext` helper without a second runtime read. Added sealed getter-only `ErrorCatalogPublishedSupportingCatalogsSnapshot` and additive `GetPublishedSupportingCatalogsSnapshot(this IErrorCatalogRuntime)`.
- The new extension selects one actual store publication through the optional `IErrorCatalogRuntimePublicationReader` and captures all four supporting catalog documents from that record. It uses the real `StoreId` and `Generation` without a separate context/status read, synthetic identity or change to existing combined snapshot types.
- Added `WhenItFails.Tests/PublicApi/PublishedSupportingCatalogsSnapshotContractTests.cs` with **15 theory-expanded cases** for identity, replacement, optional support, failure forwarding, four absent documents, nested-data failure, cancellation and public surface. Added English `Docs/Published-Supporting-Catalog-Snapshots/en.md` and updated README and runtime/public API documentation.
- This is a publication-identified data capture, **not** an atomic status pairing or a transaction against external in-place mutation. Published NuGet 0.1.0 and catalog JSON schemas remain unchanged.
- **Verified locally by maintainer:** complete **1392/1392 GREEN**; no failures reported. Focused-run and compiler-warning counts were not separately reported. Next: completed-activation-aware supporting catalog view with independent status/publication ownership tests.

## 2026-09-25 — completed activation with four supporting catalogs (1407/1407 GREEN)

- Added optional `IErrorCatalogRuntimeSupportingObservationReader` to the default runtime and sealed getter-only `ErrorCatalogCompletedSupportingCatalogsSnapshot` with actual `StoreId`, `Generation`, runtime-local `ActivationSequence`, corresponding recorded status and all four detached supporting catalog projections. Existing `IErrorCatalogRuntime`, `IErrorCatalogRuntimeCombinedObservationReader`, and three-part combined snapshot types remain unchanged.
- The method selects one recorded completed activation and its exact publication, checks publication/status before capture, reuses `ErrorSupportingCatalogsSnapshotExtensions.CaptureFromContext`, then rechecks publication and status. A same-reference republish or status-only previous-context recovery during capture invalidates the observation instead of mixing identities.
- Added `WhenItFails.Tests/PublicApi/CompletedSupportingCatalogsSnapshotContractTests.cs` with **15 theory-expanded cases**; created English `Docs/Completed-Supporting-Catalog-Snapshots/en.md`; updated README, runtime API and public API stability docs. Missing catalogs preserve established supporting-snapshot failure codes; ordinary outer exceptions are normalized and cancellation propagates.
- **Verified locally by maintainer:** complete **1407/1407 GREEN**; focused run and compiler-warning count not separately reported. Published package 0.1.0 and persisted catalog JSON schemas remain unchanged.
- Next: confirm focused/full suite and investigate any failure before extending the API further. The result is a checked observation, not a transaction against external in-place mutation or later store writes.

## 2026-09-25 — completed full operational catalog snapshot (1426/1426 GREEN)

- Added optional `IErrorCatalogRuntimeFullObservationReader` and sealed getter-only `ErrorCatalogCompletedFullSnapshot` plus `ErrorCatalogFullSnapshot`. The latter combines main indexed definitions, four supporting catalogs and recorded cross-validation findings from one selected publication, with actual `StoreId`, `Generation`, `ActivationSequence` and recorded runtime status.
- Reused the established combined and supporting capture paths from the same selected context. The supporting helper now accepts an internal pre-captured category projection to avoid copying the same category source twice. The runtime checks the selected publication and completed status before and after copying, rejecting changes without exposing partial snapshots. Earlier API shapes remain unchanged.
- Added `WhenItFails.Tests/PublicApi/CompletedFullSnapshotContractTests.cs` with **19 theory-expanded cases**; created English `Docs/Completed-Full-Snapshots/en.md`; updated README, runtime API, public API stability and completed supporting docs.
- **Scope:** complete detached operational indexed catalog view, not a raw `ErrorCatalogDocument` JSON clone, not a transaction against external in-place mutation and not a guarantee that no later publication occurs. No published NuGet 0.1.0 or persisted JSON schema change.
- **Verified locally by maintainer:** complete **1426/1426 GREEN**. Individual focused-test and compiler-warning counts were not separately reported. Next: strengthen complete-snapshot successive-activation and error-boundary contracts before deciding the 1.0 CLR API scope.

## 2026-09-25 — full-snapshot successive-activation and publication-race contracts (1429/1429 GREEN)

- Added three focused cases to `WhenItFails.Tests/PublicApi/CompletedFullSnapshotContractTests.cs`: two consecutive successful reset activations advance actual store `Generation` and runtime `ActivationSequence`, with all six captured views of the earlier activation remaining detached; an external replacement before the first publication read is rejected; and re-publication of the *same context object during capture* is detected by the second publication read.
- The deterministic `InterferingStore` test double now supports a first-publication-read hook alongside its existing second-read hook. Added a sequenced built-in provider for two successive successful resets. No production behavior or public API shape changed.
- **Verified locally by maintainer:** complete **1429/1429 GREEN** after three additional cases (22 focused cases in this class); focused-run and compiler-warning counts were not separately reported. Next: review nullable annotations and explicit stable 1.0 public API scope before further additive expansion.

## 2026-09-25 — snapshot nullable annotation contract review (1435/1435 GREEN)

- Added `WhenItFails.Tests/PublicApi/SnapshotNullableContractTests.cs` with **six** focused reflection tests for compiled C# nullable metadata: completed status/snapshot references, all six complete operational data projections, nested list and dictionary elements, optional document/definition fields, optional reader response envelopes, and runtime recovery details.
- Confirmed documented distinction between non-nullable `Response<T>` return values and its nullable `Data` payload (including the shared Essentials `Ok(null)` behavior). No production API, existing nullable annotations, serialization schemas or published package 0.1.0 were changed.
- Added English `Docs/Nullable-Snapshot-Contracts/en.md`; updated README, runtime API, public API stability notes and completed full snapshot documentation. Reflection checks protect compile-time consumer contracts; they do not claim a transaction against external in-place mutation of published contexts.
- **Verified locally by maintainer:** complete **1435/1435 GREEN** after six nullable-contract cases; focused-run and compiler-warning counts were not separately reported. Next: review pre-1.0 consumer-facing, optional and implementation-only contract boundaries without declaring version 1.0 released.

## 2026-09-25 — pre-1.0 optional snapshot capability boundary review (1439/1439 GREEN)

- Added `WhenItFails.Tests/PublicApi/SnapshotCapabilityBoundaryContractTests.cs` with four focused reflection tests: unchanged nine-method `IErrorCatalogRuntime`, five independent single-method optional readers, six context-only and two publication-aware additive extensions.
- Documented the distinction between live context publication records, detached consumer snapshot models and internal `CaptureFromContext` helpers. Existing public CLR signatures are unchanged; do not classify currently exported concrete types as private implementation details.
- Added English `Docs/Pre-1.0-Snapshot-Capability-Boundaries/en.md`; updated README and public API inventory/stability review.
- **Verified locally by maintainer:** full **1439/1439 GREEN** after the four capability-boundary tests; compiler warning count was not separately reported. Published NuGet 0.1.0, persistent JSON schemas and production code unchanged. Next: regenerate the current exported-type inventory and compare with 0.1.0 before 1.0 compatibility commitments.

## 2026-09-25 — package/source API comparison measured (1442 suite confirmation pending)

- Added three cases to `WhenItFails.Tests/PublicApi/ExportedAssemblyInventoryTests.cs`: coverage of new optional readers and snapshot families in the exported assembly, internal capture helper non-exposure, and complete deterministic current-type Markdown reporting without hardcoding the obsolete 110-type checkpoint.
- Extended `Toolroom/WhenItFails/PublicApiComparer/Compare-PublicApi.ps1` to include source/package exported type totals and type-level differences alongside existing member-level differences and the loaded DLL paths/hashes. Both API enumerations use the same case-sensitive sorting; the package consumer still requests exact `[0.1.0]` (feed provenance requires separate verification).
- Added English `Docs/Current-Public-API-Inventory/en.md` with the local report-generation procedure. Older 110-type/611-member figures are historical, not the current compiled count.
- **Maintainer-provided comparer result (2026-09-25):** source-built DLL **148 exported types / 830 API entries**; exact-requested NuGet `[0.1.0]` consumer **110 exported types / 611 API entries**. Package-only types **0**, source-only types **38**; package-only API entries **0**, source-only entries **219**. The reflected census shows no package-only signatures, but does not establish full binary, nullable, JSON or behavioral compatibility.
- **Reported DLL SHA-256:** source `E81504A484AD99D5C818C98D594CDB88A08651638984D1A92503CDC367B9F0DD`; package consumer `379F7CF6FF99223ECF2F388AB6295A34D97F33A8BD8EB7F9A52152994347CE28`. Feed override was **not** used: the package came from configured sources/cache and original publishing provenance was not independently verified. Do not store user-specific temporary file paths in repository documents.
- **Verification still pending:** the pasted comparison excerpt does **not** include focused inventory test output or confirmation of complete **1442/1442 GREEN**; last explicitly confirmed complete suite is **1439/1439 GREEN**. The comparison shows actual type/member totals only; full lists and inventory Markdown are not yet committed. No production code or published package changes.

## 2026-09-25 — classification of 38 new exported CLR types (source-only type excerpt reviewed)

- Maintainer supplied all **38 source-only exported type names** from the 0.1.0 comparer. Classified exhaustively in English `Docs/Added-Public-Types-Classification/en.md`: **2** optional store infrastructure interfaces, **5** optional runtime observation interfaces, **2** publication/activation infrastructure models, **19** detached data/observation models, and **10** additive snapshot extension classes. Count verified: 2 + 5 + 2 + 19 + 10 = 38.
- This classification changes no CLR visibility, production code or 0.1.0 package. Publicly exported infrastructure types remain public; existing nine-method `IErrorCatalogRuntime` does not gain methods. Publication records contain live context references, unlike detached catalog projections.
- The maintainer's comparer census remains **148 source / 110 package types**, **830 source / 611 package API entries**, **0 package-only types/entries**, **38 source-only types**, **219 source-only API entries**. The supplied excerpt shows *all type names* but not all 219 added member entries; do not infer that all added members belong exclusively to new types.
- **Verification outstanding:** inventory focused **4/4** and complete **1442/1442 GREEN** not yet explicitly confirmed. Last confirmed complete suite **1439/1439 GREEN**. Next: inspect remaining 219-entry member diff and record full inventory test results before considering 1.0 API freeze.

## 2026-09-25 — 14 additive public entries in original concrete classes (1445/1445 GREEN)

- Maintainer supplied the complete source-only **existing-type API diff**: 14 of 219 additional API census entries belong to `ErrorCatalogContextStore` (2 new interface and 2 method entries) and `ErrorCatalogRuntime` (5 new interface and 5 method entries). All remaining **205 entries** belong to the **38 newly exported types**; an API census entry need not be a method.
- Added English `Docs/Original-Type-API-Additions/en.md`, listing all 14 entries and explicit compatibility boundaries. Existing core interfaces and constructors are preserved; optional publication/observation interfaces are additive. The source/package comparer reports **0 package-only types/entries**, not full ABI or behavior equivalence.
- Added `WhenItFails.Tests/PublicApi/LegacyConcretePublicationExpansionContractTests.cs` with **3** focused tests of the existing store/runtime constructors, interface surfaces and added optional methods; no production code or public signatures changed.
- **Verified locally by maintainer:** full **1445/1445 GREEN**, including three inventory and three legacy-concrete API contract additions; the individual focused-run and compiler-warning counts were not separately reported. The full current Markdown inventory has not been supplied. Next: test an executable built against the actual NuGet 0.1.0 package with the source-built DLL substituted without rebuilding the executable.

## 2026-09-25 — precompiled NuGet 0.1.0 consumer binary smoke (PASS confirmed)

- Added `Toolroom/WhenItFails/PublicApiComparer/Test-PublishedConsumerBinary.ps1`. It builds one disposable .NET 10 consumer against exact requested NuGet `[0.1.0]`, exercises legacy context-store and DI/runtime pre-initialization calls, builds the current source DLL separately, and then runs the **same previously compiled consumer executable** after swapping only its WhenItFails DLL. It confirms the original application hash, loaded assembly path, substituted DLL hash and equal expected behavior; preserves the original dependency and runtimeconfig files.
- Added English `Docs/Published-Binary-Consumer-Smoke/en.md` and tool usage instructions. This is a narrow smoke of one old application, **not** a full ABI, JSON/nullable or behavioral compatibility guarantee. A configured-feed/cache restore does not verify nuget.org publishing provenance.
- **Verified locally by maintainer:** source and original-package consumer builds completed, and the script reported `Binary smoke: PASS (original package consumer and swapped source DLL).` The original .NET 10 consumer compiled against requested package `[0.1.0]` executed the covered legacy store/DI/runtime path again after replacing only WhenItFails.dll, without recompiling that executable. The script checks loaded DLL paths, consumer binary hash, substituted source DLL hash and result parity before reporting PASS.
- Report was written to the maintainer's temporary `WhenItFails-0.1.0-binary-smoke.md` path; its full contents/current DLL hashes were **not supplied**, so no run-specific hash or original-feed provenance is asserted here. Configured NuGet sources/cache are used unless `-Feed` is supplied. This is a **targeted binary smoke**, not full ABI/behavior/JSON/nullable compatibility. Confirmed full library test suite remains **1445/1445 GREEN**; no new xUnit tests or production changes. Next: extend the precompiled-consumer smoke to another legacy execution path, with exact loaded-assembly and no-rebuild checks.

## 2026-09-25 — precompiled 0.1.0 consumer: bundled activation and descriptor probe (PASS confirmed)

- Extended `Toolroom/WhenItFails/PublicApiComparer/Test-PublishedConsumerBinary.ps1` with optional `-ExerciseInitialization`. The original default smoke is unchanged. In opt-in mode, the disposable application is **still compiled exactly once against requested NuGet [0.1.0]**, then runs unchanged against the original package DLL and a copied output directory with only WhenItFails.dll replaced by the current source build.
- The optional probe explicitly activates isolated bundled catalogs through `ResetToDefaultsAsync()`, confirms valid non-degraded `BuiltInDefaults` status and active context, then uses the **original** `IErrorCatalogRuntime` methods `FromName("UNKNOWNERROR")`, `FromId("AFW_GEN_0001")` and `FromCode(100001)` to verify the known historical descriptor identity and title/message. Hash, loaded-assembly and fixed-result checks remain in the script.
- Updated English `Docs/Published-Binary-Consumer-Smoke/en.md`, Toolroom README and usage docs. **Locally verified by maintainer:** opt-in report returned PASS for both original 0.1.0 consumer execution and the unchanged consumer with the current DLL substituted; descriptor lookup via name/ID/code and explicit bundled activation passed in both runs. No xUnit or production changes; last maintainer-confirmed full suite **1445/1445 GREEN**. Original consumer DLL SHA-256: `379F7CF6FF99223ECF2F388AB6295A34D97F33A8BD8EB7F9A52152994347CE28`; swapped source DLL SHA-256: `C30205E4D42FB63EAB540063F8FF4BCD138A2A602F4A0942509C26B8FF4004F2`; unchanged consumer SHA-256: `586F81041EA77AE853CB198784D753B991FA3980C1DCF51A95B4F850DD02022F`. Package origin not independently verified; report uses configured sources/cache. This opt-in probe is not a project-workspace `InitializeAsync()`, automatic recovery or comprehensive binary compatibility test. Next: independently validate project workspace initialization in an isolated temp directory without modifying the user's source checkout, then consider recovery coverage.

## 2026-09-25 — isolated project-workspace initialization of precompiled 0.1.0 consumer (PASS confirmed)

- Added opt-in, mutually exclusive `-ExerciseProjectInitialization` to `Toolroom/WhenItFails/PublicApiComparer/Test-PublishedConsumerBinary.ps1`. It compiles the disposable consumer once against exact requested NuGet `[0.1.0]`, executes it with the package DLL, and re-executes the unchanged executable with only WhenItFails.dll replaced by the source build. Each run receives its **own previously empty temporary project workspace**, outside the source checkout.
- On each run, `IErrorCatalogRuntime.InitializeAsync(JsonsOptions)` initializes project-local JSONs and records a non-degraded `ProjectCatalog` state, all five catalog files must exist, and the historical `UNKNOWNERROR` descriptor must resolve by name, ID and numeric code with matching identity/title/message. Repeating initialization verifies that all five existing files have unchanged SHA-256 hashes. Loaded assembly, executable hash and substituted DLL hash checks remain in place.
- Updated English `Docs/Published-Binary-Consumer-Smoke/en.md` and Toolroom usage/README. **Verified locally by maintainer:** the project-workspace mode reported `Binary project initialization smoke: PASS (original package consumer and swapped source DLL).` Both executions completed the initial and repeated project initialization checks; the script verified five unchanged file hashes, matched descriptor lookups, the loaded DLL and unchanged consumer binary. The full report and run-specific hashes were not supplied; package feed provenance remains unverified. No production or xUnit test changes: complete `WhenItFails.Tests` remains **1445/1445 GREEN**. Next: test isolated malformed-project previous-context recovery while keeping the original application binary and source checkout unchanged.

## 2026-09-25 — precompiled 0.1.0 consumer malformed project / previous-context recovery (PASS confirmed)

- Extended `Toolroom/WhenItFails/PublicApiComparer/Test-PublishedConsumerBinary.ps1` with opt-in `-ExerciseProjectRecovery` requiring `-ExerciseProjectInitialization`. Both executions use the unchanged consumer compiled once against exact requested package `[0.1.0]`, running separately against the package DLL and current source-built DLL in two isolated temporary workspaces outside the checkout.
- The disposable consumer activates valid project catalogs, confirms the previous context and descriptor, corrupts only its temporary `errors.en.json`, then retries `InitializeAsync(JsonsOptions)` in Flexible mode. It requires an active `PreviousContextRecovery` status and the **same context reference**, unchanged original descriptor and unchanged SHA-256 of all five files after the failed initialization. No project JSON repair or fallback publication is allowed in this tested previous-context path.
- Added English recovery procedure to `Docs/Published-Binary-Consumer-Smoke/en.md` and updated Toolroom usage/README. **Verified locally by maintainer:** the recovery mode returned `Binary project recovery smoke: PASS (original package consumer and swapped source DLL).` The original and substituted runs passed all checks of retained context, degraded recovery, descriptor and five unchanged JSON file hashes. The full report and run-specific hashes were not supplied; NuGet source provenance remains unverified. No production or xUnit changes; maintainer-confirmed full suite remains **1445/1445 GREEN**. Next: test the first-start built-in fallback for an already malformed project catalog, with no previously valid active context.

## 2026-09-25 — isolated first-start malformed project built-in fallback of precompiled 0.1.0 consumer (PASS confirmed)

- Added mutually exclusive `-ExerciseFirstStartFallback` to `Toolroom/WhenItFails/PublicApiComparer/Test-PublishedConsumerBinary.ps1`. The disposable application is compiled once against requested exact package `[0.1.0]`, run first with that package and then without recompiling with only WhenItFails.dll swapped to the current source build. Both runs receive separate new temporary project roots outside the repository.
- The consumer starts with no active runtime context, creates an invalid `errors.en.json` inside its private temporary workspace **before the first project initialization**, then calls original `IErrorCatalogRuntime.InitializeAsync(JsonsOptions)`. In Flexible mode it must activate bundled defaults, report `BuiltInFallback` / degraded / UsedFallback with no retained previous context, preserve the malformed file content and SHA-256, and resolve the historical `UNKNOWNERROR` descriptor consistently via name, ID and code.
- Existing loaded-assembly, original executable hash, substituted DLL hash and exact result-parity checks remain active. Updated English `Docs/Published-Binary-Consumer-Smoke/en.md` and Toolroom README/usage. **Verified locally by maintainer:** the first-start fallback mode printed `Binary first-start fallback smoke: PASS (original package consumer and swapped source DLL).` Both runs passed their isolated malformed-file retention and bundled-fallback/descriptor checks. The run report and specific DLL hashes were not supplied; source feed provenance remains unverified. Five smoke scenarios now have confirmed PASS. No production or xUnit test changes; complete `WhenItFails.Tests` remains **1445/1445 GREEN**. Next: verify first-start Strict mode rejects the invalid project catalog without activating a context or rewriting JSON, using the same unchanged original consumer.

## 2026-09-25 — precompiled 0.1.0 consumer strict first-start rejection (PASS confirmed)

- Extended `Toolroom/WhenItFails/PublicApiComparer/Test-PublishedConsumerBinary.ps1` with mutually exclusive `-ExerciseStrictFirstStart`. The disposable .NET 10 consumer is configured with `WhenItFailsOptions.InitializationMode = Strict` **before a single compilation against exact requested package [0.1.0]**. It then runs against the original package DLL and without rebuilding against the current source-built DLL substituted into an output copy, each using its own temporary project root outside the source checkout.
- The disposable consumer begins without an active runtime context, places intentionally malformed `errors.en.json` in its isolated workspace, calls original `InitializeAsync(JsonsOptions)`, and requires a non-success response with no payload, no active context/status, no descriptor available from the legacy name/ID/code methods, and byte-identical invalid JSON with unchanged SHA-256. The existing loaded DLL, unchanged consumer executable and substitution-hash/result-parity checks remain enabled.
- Added English strict-mode procedure to `Docs/Published-Binary-Consumer-Smoke/en.md` and Toolroom usage/README. **Verified locally by maintainer:** the strict first-start run printed `Binary strict first-start smoke: PASS (original package consumer and swapped source DLL).` Both executions passed rejected-initialization, no-context/no-fallback and byte-preservation checks. Full report and run-specific hashes were not supplied; NuGet original-feed provenance is unverified. Six smoke modes now have confirmed PASS. No production or xUnit changes; confirmed suite remains **1445/1445 GREEN**. Next: test strict-mode failed reinitialization after an already valid project activation, preserving its active context, recorded status and user-managed JSON.

## 2026-09-25 — precompiled 0.1.0 consumer strict reinitialization after healthy project activation (PASS confirmed)

- Added mutually exclusive `-ExerciseStrictReinitialization` to `Toolroom/WhenItFails/PublicApiComparer/Test-PublishedConsumerBinary.ps1`. The original consumer is configured in Strict mode **before its single compilation against exact requested [0.1.0]**, then run with the package DLL and again without recompilation after replacing only WhenItFails.dll with the source build. The two runs use distinct disposable project roots outside the checkout; all prior smoke modes remain available.
- Each run initializes a valid project workspace twice, checks project `UNKNOWNERROR` descriptor and original file hashes, then corrupts only its own temporary `errors.en.json` and attempts Strict reinitialization. The rejection must provide no success/data payload; the previously active context and recorded `ProjectCatalog` status must retain their object identities without degraded/fallback flags. All five existing file hashes, including the malformed JSON, must remain unchanged. Legacy `FromName`, `FromId` and `FromCode` must continue resolving the original descriptor.
- Updated English `Docs/Published-Binary-Consumer-Smoke/en.md` and Toolroom usage/README. **Locally confirmed by maintainer:** the strict reinitialization mode reported `Binary strict reinitialization smoke: PASS (original package consumer and swapped source DLL).` Both executions passed the retained original context/status identity, unchanged five JSON file hashes and descriptor checks; seven binary smoke scenarios are now confirmed PASS. No production API or xUnit changes; last confirmed complete suite **1445/1445 GREEN**. Next: review cancellation paths against NuGet 0.1.0 before implementing the next isolated precompiled-consumer scenario; retain no-rebuild, no-repository-writes and diagnostic boundaries.

## Current verified state

- Complete `WhenItFails.Tests` suite: **1445/1445 GREEN**, confirmed locally by maintainer after inventory and original-concrete API contract additions (compiler-warning count not separately reported for this checkpoint).
- The SDK emits `NETSDK1057` informational messages because the local SDK is `.NET 11.0.100-rc.1`; these are SDK support-policy messages, not compiler warnings from Toolbox code.
- `ErrorDescriptorResolver` and `ErrorDescriptorService` hardening are complete for the current scope.
- `ErrorCatalogProvider` and `CatalogProviderPipeline` dependency-boundary audits are complete for the current scope.
- `BuiltInErrorCatalogContextProvider` dependency-boundary audit is complete for the current scope.
- `ErrorCatalogInitializer` bootstrapper/context-provider ordinary-exception, cancellation, null-response and null-task behavior is complete for the current scope.
- `ErrorCatalogRuntime` initializer and both built-in-provider runtime paths are complete for null response, ordinary exception, null task and exact cancellation behavior.
- Direct `JsonsBootstrapper` → `IJsonsTemplateProvider.GetTemplateFiles(...)` invocation boundary is complete for malformed direct results, ordinary-exception normalization and exact-instance cancellation propagation; deferred failures while consuming the returned collection are under audit.
- `JsonCatalogDocumentWriter` serialization-failure temporary-file cleanup and deterministic pre-cancellation behavior are verified.
- `JsonCatalogDocumentWriter` current safe-write scope is complete; `AccessDenied` remains intentionally unforced because a deterministic cross-platform permission failure would require a filesystem seam or OS-specific test setup.
- `JsonErrorCodeGroupCatalogLoader` concrete coverage is locally verified GREEN in the complete **1143/1143** suite.
- `JsonErrorOwnerCatalogLoader` concrete coverage is locally verified GREEN in the complete **1145/1145** suite.
- Specialized JSON catalog loader baseline coverage is complete for the current scope; generic invalid-path, malformed-JSON and cancellation behavior remains centralized in `JsonCatalogDocumentLoader` tests rather than duplicated per wrapper.
- `ErrorCategoryDefinitionNormalizer` null-input contract is locally verified GREEN in the complete **1146/1146** suite.
- `ErrorCategoryDefinitionNormalizer` basic-field normalization contract is locally verified GREEN in the complete **1147/1147** suite.
- `ErrorCategoryDefinitionNormalizer` mutable collection/mapping isolation contract is locally verified GREEN in the complete **1148/1148** suite.
- `ErrorCategoryDefinitionNormalizer` metadata isolation fix is locally verified GREEN in the complete **1149/1149** suite.
- `ErrorCodeGroupDefinitionNormalizer` null-input contract is locally verified GREEN in the complete **1150/1150** suite.
- `ErrorCodeGroupDefinitionNormalizer` basic-field normalization contract is locally verified GREEN in the complete **1151/1151** suite.
- `ErrorCodeGroupDefinitionNormalizer` mutable collection/mapping isolation contract is locally verified GREEN in the complete **1152/1152** suite.
- `ErrorCodeGroupDefinitionNormalizer` metadata isolation fix is locally verified GREEN in the complete **1153/1153** suite.
- `ErrorOwnerDefinitionNormalizer` null-input contract is locally verified GREEN in the complete **1154/1154** suite.
- `ErrorOwnerDefinitionNormalizer` basic-field normalization contract is locally verified GREEN in the complete **1155/1155** suite.
- `ErrorOwnerDefinitionNormalizer` mutable alias/mapping isolation contract is locally verified GREEN in the complete **1156/1156** suite.
- `ErrorOwnerDefinitionNormalizer` metadata isolation fix is locally verified GREEN in the complete **1157/1157** suite.
- Normalization layer direct coverage is complete for the current scope: all 12 production normalization files have corresponding focused tests.
- Concrete core class-level coverage audit is complete for the current scope. Remaining filename-audit mismatches were interfaces, already-covered bootstrap DTO/value contracts, `TextKeyNormalizer` under its historical test filename, `ErrorCatalogInitializationMode` covered by configuration enum-value contracts, and `CatalogValidationHelper` behavior covered through public validator tests.
- Writer nested-directory creation contract is locally verified GREEN in the complete **1141/1141** suite.
- Writer extensionless-target backup file-name contract is locally verified GREEN in the complete **1140/1140** suite.
- Writer backup file-name shape contract is locally verified GREEN in the complete **1139/1139** suite.
- Writer existing-target I/O failure contract is locally verified GREEN in the complete **1138/1138** suite.
- Writer first-save success-message contract is locally verified GREEN in the complete **1137/1137** suite.
- Writer backup success-message contract is locally verified GREEN in the complete **1136/1136** suite.
- Writer first-save no-backup/no-temp success contract is locally verified GREEN in the complete **1135/1135** suite.
- Writer successful-replace temporary-file cleanup contract is locally verified GREEN in the complete **1134/1134** suite.
- Writer exact-byte backup preservation contract is locally verified GREEN in the complete **1133/1133** suite.
- Writer surrounding-whitespace path normalization contract is locally verified GREEN in the complete **1132/1132** suite.
- Writer null file-path contract is locally verified GREEN in the complete **1131/1131** suite.
- Writer whitespace-only file-path contract is locally verified GREEN in the complete **1130/1130** suite.
- Writer no-directory-path classification contract is locally verified GREEN in the complete **1129/1129** suite.
- Writer null-document entry contract is locally verified GREEN in the complete **1128/1128** suite.
- `ErrorProfileSelectionService` → `IErrorProfileResolver.Resolve(...)` boundary is complete for null result, ordinary-exception normalization and exact-instance cancellation propagation.
- `ErrorProfileSelectionService` classifies all resolver-consumed nullable collections currently audited as malformed input rather than resolver failure.
- `JsonsBootstrapper` rejects null/whitespace `RootDirectory` and `PackageDirectoryName` before filesystem mutation.
- `ErrorCatalogFileName = null` is locally verified GREEN and is rejected before template-provider invocation or filesystem mutation.
- `ErrorCatalogFileName` null/whitespace contracts are locally verified GREEN and reject malformed caller configuration before provider invocation or filesystem mutation.
- `CategoryCatalogFileName = null` is locally verified GREEN as part of the 1046-test suite.
- `CategoryCatalogFileName` null/whitespace contracts are locally verified GREEN and reject malformed caller configuration before provider invocation or filesystem mutation.
- `CodeGroupCatalogFileName = null` is locally verified GREEN and is rejected before template-provider invocation or filesystem mutation.
- `CodeGroupCatalogFileName` null/whitespace contracts are locally verified GREEN; invalid values are rejected before provider invocation or filesystem mutation.
- `OwnerCatalogFileName = null` is locally verified GREEN and is rejected before template-provider invocation or filesystem mutation.
- `OwnerCatalogFileName` null/whitespace contracts are locally verified GREEN; invalid values are rejected before provider invocation or filesystem mutation.
- `ProfilesFileName = null` is locally verified GREEN; invalid configuration is rejected before provider invocation or filesystem mutation.
- All five `JsonsOptions` catalog filename fields have locally verified null/whitespace GREEN contracts in `JsonsBootstrapper`, rejecting malformed input before filesystem mutation and template-provider invocation.
- The package-directory escape contract is locally verified GREEN; `JsonsBootstrapper` rejects a package path outside the configured root before filesystem mutation and template-provider invocation.
- A positive regression for valid nested `PackageDirectoryName` values is locally verified GREEN; legitimate nested package directories remain supported.
- The escaping `ErrorCatalogFileName` caller-configuration contract is locally verified GREEN; the bootstrapper rejects that invalid filename before filesystem mutation and template-provider invocation.
- The escaping `CategoryCatalogFileName` caller-configuration contract is locally verified GREEN; invalid filename paths are rejected before filesystem mutation and template-provider invocation.
- The escaping `CodeGroupCatalogFileName` caller-configuration contract is locally verified GREEN; invalid filename paths are rejected before filesystem mutation and template-provider invocation.
- The escaping `OwnerCatalogFileName` caller-configuration contract is locally verified GREEN; invalid filename paths are rejected before filesystem mutation and template-provider invocation.
- The escaping `ProfilesFileName` caller-configuration contract is locally verified GREEN; invalid filename paths are rejected before filesystem mutation and template-provider invocation.
- All five caller-configured catalog filename containment guards are locally verified GREEN.
- Malformed `RootDirectory` caller-configuration contract is locally verified GREEN; syntactically invalid root paths are normalized to a stable `Invalid` response before filesystem mutation or template-provider invocation.
- Malformed `PackageDirectoryName` caller-configuration contract is locally verified GREEN; syntactically invalid package directory names are normalized to a stable `Invalid` response before filesystem mutation or template-provider invocation.
- Malformed `ErrorCatalogFileName` caller-configuration contract is locally verified GREEN; syntactically invalid error catalog filenames are normalized to a stable `Invalid` response before filesystem mutation or template-provider invocation.
- Malformed `CategoryCatalogFileName` caller-configuration contract is locally verified GREEN; syntactically invalid category catalog filenames are normalized to a stable `Invalid` response before filesystem mutation or template-provider invocation.
- Malformed `CodeGroupCatalogFileName` caller-configuration contract is locally verified GREEN; syntactically invalid code-group catalog filenames are normalized to a stable `Invalid` response before filesystem mutation or template-provider invocation.
- Malformed `OwnerCatalogFileName` caller-configuration contract is locally verified GREEN; syntactically invalid owner catalog filenames are normalized to a stable `Invalid` response before filesystem mutation or template-provider invocation.
- Malformed `ProfilesFileName` caller-configuration contract is locally verified GREEN; syntactically invalid profile catalog filenames are normalized to a stable `Invalid` response before filesystem mutation or template-provider invocation.
- Malformed template-provider `TargetFileName` contract is locally verified GREEN; syntactically invalid provider target filenames are normalized to stable `Invalid` while preserving null, whitespace, and outside-package contracts.
- Valid nested template-target creation contract is locally verified GREEN; validated nested targets create missing parent directories while preserving existing files.
- Null template-name provider-output contract is locally verified GREEN; null logical template names are rejected before target validation and file creation.
- Whitespace template-name provider-output contract is locally verified GREEN; whitespace-only logical names are rejected before target validation and file creation.
- Template collection enumeration-exception contract is locally verified GREEN; ordinary deferred collection failures are normalized to the stable provider-failure response without leaking provider detail.
- Template collection enumeration-cancellation regression contract is locally verified GREEN; exact-instance `OperationCanceledException` propagation is preserved during returned-collection enumeration.
- Later-null-template-item no-partial-write contract is locally verified GREEN; the full materialized template snapshot is validated before any template file write.
- Directory-only template-target contract is locally verified GREEN; directory-only provider targets are rejected during provider-output validation before filesystem mutation.
- Directory-only `ErrorCatalogFileName` caller-configuration contract is locally verified GREEN; directory-only error catalog filenames are rejected before provider invocation and filesystem mutation.
- Directory-only `CategoryCatalogFileName` caller-configuration contract is locally verified GREEN; directory-only category catalog filenames are rejected before provider invocation and filesystem mutation.
- Directory-only `CodeGroupCatalogFileName` caller-configuration contract is locally verified GREEN; directory-only code-group catalog filenames are rejected before provider invocation and filesystem mutation.
- Directory-only `OwnerCatalogFileName` caller-configuration contract is locally verified GREEN; directory-only owner catalog filenames are rejected before provider invocation and filesystem mutation.
- Directory-only `ProfilesFileName` caller-configuration contract is locally verified GREEN; all five caller-configured catalog filenames reject directory-only targets before provider invocation or filesystem mutation.
- Existing-directory provider-target contract is locally verified GREEN; provider targets resolving to existing directories are rejected during full-snapshot validation before any template write.
- Existing-directory `ErrorCatalogFileName` caller-configuration contract is locally verified GREEN; caller configuration resolving to an existing directory is rejected before provider invocation.
- Existing-directory `CategoryCatalogFileName` caller-configuration contract is locally verified GREEN; caller configuration resolving to an existing directory is rejected before provider invocation.
- Existing-directory `CodeGroupCatalogFileName` caller-configuration contract is locally verified GREEN; caller configuration resolving to an existing directory is rejected before provider invocation.
- Existing-directory `OwnerCatalogFileName` caller-configuration contract is locally verified GREEN; caller configuration resolving to an existing directory is rejected before provider invocation.
- Existing-directory `ProfilesFileName` caller-configuration contract is locally verified GREEN; all five caller-configured catalog filenames now reject paths resolving to existing directories before provider invocation.
- Provider `TargetFileName = "."` semantic-directory contract is locally verified GREEN; the package-directory target returns the invalid-target code without changing the shared containment helper.
- Caller `ErrorCatalogFileName = "."` semantic-directory contract is locally verified GREEN; a package-directory target is now classified as an invalid caller filename without changing the shared containment helper.
- Caller `CategoryCatalogFileName = "."` semantic-directory contract is locally verified GREEN; package-directory targets are classified as invalid caller filenames without changing the shared containment helper.
- Caller `CodeGroupCatalogFileName = "."` semantic-directory contract is locally verified GREEN; the package-directory target returns a code-group-specific invalid filename code.
- Caller `OwnerCatalogFileName = "."` semantic-directory contract is locally verified GREEN; the package-directory target returns the owner-specific invalid filename code.
- Caller `ProfilesFileName = "."` semantic-directory contract is locally verified GREEN; all five caller catalog filename fields classify package-directory targets as invalid filenames.
- Provider-target existing-file parent contract is locally verified GREEN; an existing regular-file ancestor is rejected during full-snapshot validation before template writes.
- Caller error-catalog existing-file parent contract is locally verified GREEN; its existing regular-file ancestor is rejected before template-provider invocation.
- Caller category-catalog existing-file parent contract is locally verified GREEN; an existing regular-file ancestor is rejected before template-provider invocation.
- Caller code-group existing-file parent contract is locally verified GREEN; an existing regular-file ancestor is rejected before template-provider invocation.
- Caller owner-catalog existing-file parent contract is locally verified GREEN; an existing regular-file ancestor is rejected before template-provider invocation.
- Caller profiles existing-file parent contract is locally verified GREEN; all five caller-configured catalog filename fields now reject existing regular-file ancestors before provider invocation.
- Existing-file package-directory-path contract is locally verified GREEN; a regular file occupying the resolved package path is rejected before workspace creation or provider invocation.
- Existing-file root-directory-path contract is locally verified GREEN; a regular file occupying the configured root path is rejected before package containment or provider invocation.
- Root-directory existing-file-parent contract is locally verified GREEN; existing regular-file ancestors of the configured root are rejected before package containment or provider invocation.
- Package-directory existing-file-parent contract is locally verified GREEN; existing regular-file ancestors between the package directory and configured root are rejected before workspace creation or provider invocation.
- Package-directory current-directory semantic contract is locally verified GREEN; package paths resolving exactly to the configured root are classified as invalid names rather than outside-root escapes.
- Nested current-directory template-target contract is locally verified GREEN; terminal current-directory segments are rejected during full-snapshot validation before template writes.
- Nested current-directory `ErrorCatalogFileName` contract is locally verified GREEN; terminal current-directory segments are rejected before workspace creation or template-provider invocation.
- Nested current-directory `CategoryCatalogFileName` contract is locally verified GREEN; terminal current-directory segments are rejected before workspace creation or template-provider invocation.
- Nested current-directory `CodeGroupCatalogFileName` contract is locally verified GREEN; terminal current-directory segments are rejected before workspace creation or template-provider invocation.
- Nested current-directory `OwnerCatalogFileName` contract is locally verified GREEN; terminal current-directory segments are rejected before workspace creation or template-provider invocation.
- Nested current-directory `ProfilesFileName` contract is locally verified GREEN; all five caller-configured catalog filename fields now reject terminal current-directory segments before workspace creation or template-provider invocation.
- Contained terminal parent-directory provider-target contract is locally verified GREEN; contained terminal `..` targets are rejected during full-snapshot validation while true escapes retain outside-package classification.
- Contained terminal parent-directory `ErrorCatalogFileName` contract is locally verified GREEN; contained terminal `..` values are rejected before workspace creation or template-provider invocation while true escapes retain outside-package classification.
- Contained terminal parent-directory `CategoryCatalogFileName` contract is locally verified GREEN; contained terminal `..` values are rejected before workspace creation or template-provider invocation while true escapes retain outside-package classification.
- Contained terminal parent-directory `CodeGroupCatalogFileName` contract is locally verified GREEN; contained terminal `..` values are rejected before workspace creation or template-provider invocation while true escapes retain outside-package classification.
- Contained terminal parent-directory `OwnerCatalogFileName` contract is locally verified GREEN; contained terminal `..` values are rejected before workspace creation or template-provider invocation while true escapes retain outside-package classification.
- Contained terminal parent-directory `ProfilesFileName` contract is locally verified GREEN; all five caller-configured catalog filename fields now reject contained terminal `..` values before workspace creation or template-provider invocation while true escapes retain outside-package classification.
- Duplicate canonical provider-target contract is locally verified GREEN; canonical aliases are rejected before writes with platform-appropriate path comparison.
- Provider snapshot file/directory target-conflict contract is locally verified GREEN; prospective file-vs-directory conflicts are rejected before the write loop.
- Reverse-order provider target-conflict regression is locally verified GREEN; snapshot file/directory conflict detection is order-independent.
- Later-null-template-content no-partial-write regression is locally verified GREEN; complete provider snapshot validation reaches every item's content before the write loop.
- `JsonCatalogDocumentLoader` existing-directory path contract is locally verified GREEN; directory paths return `Invalid` / `FilePathIsDirectory` instead of `NotFound`.
- `JsonCatalogDocumentLoader` existing-file-parent path guard is verified GREEN in the complete 1121-test suite; the separate focused-filter result was not reported.
- `JsonCatalogDocumentLoader` JSON-null-document classification regression is locally verified GREEN in the full suite; a syntactically valid JSON `null` returns `EmptyCatalogDocument` rather than `InvalidJson`.
- `JsonCatalogDocumentWriter` existing-directory target contract is verified GREEN in the complete 1123-test suite. The separate focused result was not reported.
- `JsonCatalogDocumentWriter` existing-file parent target contract is locally verified GREEN in the full 1124-test suite; its ancestor-path guard rejects file parents before directory or temporary-file creation.
- Existing-catalog serialization-failure preservation regression is locally verified GREEN in the complete suite; the original file remains unchanged with no extra backup or temporary file.
- Writer unsupported-type serialization exception contract is locally verified GREEN in the complete 1126-test suite; `NotSupportedException` from serializer becomes `Invalid` / `JsonSerializationFailed` with temporary-file cleanup.
- Writer mid-serialization cancellation preservation regression is locally verified GREEN in the clean **1127/1127** suite after duplicate coverage removal.

## 2026-09-23 — category definition normalizer compile fix

Compile fix commit: `e8d918fd464d96746a89a8ca0fcbcc3d6fcf961d`

The new basic-field test referenced `ErrorCategoryDefinition` without importing `Afrowave.Toolbox.WhenItFails.Definitions`.

The missing using directive is now added.

No production code changed. Focused/full verification remains pending.

## 2026-09-23 — Blazor presentation integration candidate

Design direction:

WhenItFails remains a toolbox of composable error-handling capabilities rather than one monolithic framework.

A future Blazor integration should therefore live in a separate package, provisionally:

`Afrowave.Toolbox.WhenItFails.Blazor`

Suggested responsibilities:

- render user-safe error views from `ErrorDescriptor`;
- render optional developer-oriented diagnostic views;
- provide reusable components for common states such as validation failure, unavailable service, permission failure and unexpected error;
- support profile/mapping-driven presentation policy;
- keep exception details, stack traces and sensitive metadata hidden by default;
- allow explicit developer-mode diagnostics;
- avoid introducing Blazor dependencies into the core `Afrowave.Toolbox.WhenItFails` package;
- compose naturally with the planned ASP.NET Core Problem Details integration, while remaining usable for interactive Blazor UI scenarios where an HTTP Problem Details payload is not the final presentation surface.

This is a future integration package candidate, not a blocker for completing the current core runtime audit.

## 2026-09-23 — ASP.NET Core Problem Details integration added to implementation plan

Design document commit:
`ddde1f3e916142b18c60a36e4bb71defa7b72e69`

Added:
`WhenItFails/Docs/ASP.NET Core Integration/en.md`

Decision:

- add first-class ASP.NET Core Problem Details support;
- keep ASP.NET Core dependencies out of the core `Afrowave.Toolbox.WhenItFails` package;
- prefer a separate integration assembly/package, provisionally `Afrowave.Toolbox.WhenItFails.AspNetCore`;
- map `ErrorDescriptor` to ASP.NET Core `ProblemDetails`;
- preserve explicit HTTP policy through web mappings such as `web.httpStatusCode`;
- do not infer HTTP status solely from severity;
- expose stable public identifiers through safe Problem Details extensions;
- suppress exception, stack trace, developer hints and other internal details by default;
- integrate with Minimal APIs, controllers and the standard `IProblemDetailsService` pipeline in staged follow-up work;
- evaluate `ValidationProblemDetails` only after defining a proper structured field-validation contract.

This is considered a strong candidate for the first stable WhenItFails release because the existing WEB/API profile mappings already anticipate web-specific presentation behavior.

## 2026-09-23 — external NuGet runtime end-to-end smoke test GREEN

Maintainer confirmed the isolated external `net10.0` consumer project with:

- `Afrowave.Toolbox.WhenItFails 0.1.0`,
- explicit `Microsoft.Extensions.DependencyInjection 10.0.9`,
- transitive `Afrowave.Toolbox.Essentials 0.2.0`,
- Microsoft.Extensions dependencies at `10.0.9`.

Observed runtime output:

```text
Initialization: Success
Runtime status: Success
Error resolution: Success
Resolved: AFW_GEN_0001 - Unknown error
SUCCESS: External WhenItFails runtime smoke test.
```

This verifies the packaged public path outside the Toolbox repository:

```text
NuGet restore
→ consumer build
→ AddWhenItFails registration
→ ServiceProvider creation
→ IErrorCatalogRuntime resolution
→ InitializeAsync
→ GetStatus
→ FromId
→ ErrorDescriptor
```

The earlier consumer compile failure was caused by the test application lacking the full `Microsoft.Extensions.DependencyInjection` implementation package and by an incorrect sample reference to `Response<T>.Value`; both were consumer-test issues, not package defects.

Packaging, embedded resources, external restore, external build and external runtime execution are now verified for the current `0.1.0` package.

Next: complete the 1.0 public API stability review, classify intended extension interfaces versus implementation exposure, capture a reproducible API baseline, and define the exact stable 1.0 scope before changing package version or publishing.

## 2026-09-23 — external runtime consumer smoke test compile corrections

External consumer smoke compilation failed before runtime execution because its sample used `ServiceProvider` / `BuildServiceProvider()` without an explicit `Microsoft.Extensions.DependencyInjection` implementation package (the WhenItFails NuGet package declares DI Abstractions, not the implementation) and referenced nonexistent `Response<ErrorDescriptor>.Value`.

Verified source contracts: `Essentials/Results/ResponseOfT.cs` exposes `Data` and `IsSuccess`, and existing `WhenItFails.Tests/Integration/ErrorCatalogRuntimeIntegrationTests.cs` uses `Data`, `IsSuccess` and the full DI implementation. `AFW_GEN_0001` is present in the authoritative catalog.

Corrective consumer-only actions: add `Microsoft.Extensions.DependencyInjection 10.0.9` explicitly to the isolated console project; use `descriptor.Data` and check `initialization.IsSuccess`, `status.IsSuccess` and `descriptor.IsSuccess`. Run the repaired consumer sample before reporting the runtime end-to-end gate as GREEN. No production change is indicated by the current compiler errors.

## 2026-09-23 — packaged primary API signatures verified

The maintainer used the isolated NuGet consumer to reflect the public constructors, properties and declared methods of `IErrorCatalogRuntime`, `ErrorDescriptor`, `WhenItFailsOptions` and `JsonsOptions` from `Afrowave.Toolbox.WhenItFails 0.1.0`.

Observed: `IErrorCatalogRuntime` has 9 declared methods (two `InitializeAsync` overloads, `ResetToDefaultsAsync`, `GetCurrentContext`, `GetStatus`, `FromId`, `FromName`, `FromCode`, `ResolveProfile`). `ErrorDescriptor` has a public parameterless constructor and 23 public properties including mutable lists, `MetadataBag` and `Exception`. `WhenItFailsOptions` has 3 public properties; `JsonsOptions` has 13 (7 settable configuration fields plus 6 derived paths).

Source inspection confirms `HideRecoverableFailures` is intentionally `bool?`; null means no explicit override, and the safe default applies. `JsonsOptions` derived paths are get-only and computed from configurable root/package/file names. `ErrorDescriptor.Exception` is marked `[JsonIgnore]`; that alone does NOT make the whole descriptor safe for untrusted JSON/API responses because other diagnostic fields are serialized.

No source or public API changes made in this checkpoint. Next: verify descriptor isolation/mapping and public API usage in an external consumer against the documented runtime methods before declaring stable 1.0 contracts. Retain the API inventory as a baseline rather than immediately hiding concrete types.

## 2026-09-23 — packaged public type inventory collected

The maintainer ran an isolated NuGet consumer against `Afrowave.Toolbox.WhenItFails 0.1.0` and enumerated `typeof(DefaultJsonsTemplateProvider).Assembly.GetExportedTypes()`. The inventory contains **110 exported public types** in the packaged DLL (including the `Microsoft.Extensions.DependencyInjection.WhenItFailsServiceCollectionExtensions` type). The output lists type names and reflection `TypeAttributes`, not public constructors, methods, properties, events, parameter/return types or accessibility of nested members.

Initial categories for the 1.0 review:
- application surface: `IErrorCatalogRuntime`, `WhenItFailsOptions`, `JsonsOptions`, `ErrorDescriptor`, context/status and other public response/definition DTOs;
- intended extension contracts: loader, provider, validator, normalizer, resolver, bootstrap and context-store interfaces;
- concrete implementation types: loaders, normalizers, validators, providers, runtime services and helpers, whose public status must be reviewed individually rather than mechanically changed to `internal`;
- integration registration: `WhenItFailsServiceCollectionExtensions`.

This is an inventory checkpoint, not an assertion that all 110 types are supported stable contracts. Before a 1.0 compatibility promise, obtain public member signatures for the main application surface and inspect public concrete types used by DI and existing consumers. Capture a reproducible API baseline without loading the repository build-output DLL into a long-lived PowerShell session.

Next small slice: inspect public `IErrorCatalogRuntime` methods and `ErrorDescriptor` / configuration DTO shapes in the packaged DLL, compare them with docs and existing contracts, then record any concrete issues before production changes.

## 2026-09-23 — public API stabilization audit started

The Release solution build is GREEN, and the package/isolated-consumer embedded-template smoke gates have passed.

Initial review of `Interfaces/IErrorCatalogRuntime.cs`, `Docs/Runtime/Public-API.md` and `WhenItFails.csproj`:

- The documented high-level runtime API includes initialization (registered options and per-call workspace override), reset to built-in defaults, active context and runtime status, resolution by ID/name/numeric code, and profile selection.
- Current package remains `0.1.0`, targets `net10.0`, depends on Essentials `0.2.0`; ASP.NET Core and Blazor dependencies are not part of the core package.
- Before committing to 1.0 compatibility, inventory all *publicly accessible* types/members, including implementation classes and public return types, rather than assuming the single runtime interface is the whole supported API.
- Classify the API inventory into explicitly stable consumer contracts, public extension/customization points, and implementation exposure requiring an intentional compatibility decision; avoid blanket visibility changes prior to usage/backward-compatibility review.
- Compare documented examples against the current public signatures, error identities and runtime behavior; do not infer that docs compile merely from successful solution build.
- Follow with an isolated external consumer end-to-end runtime initialization and error-resolution smoke test, API baseline capture, and explicit 1.0 scope/release approval. Planned ASP.NET Core ProblemDetails and Blazor adapters remain separate packages and must not introduce web/UI dependencies into core.

Next incremental action: capture an externally observable public type/method baseline from the *built NuGet DLL* in an isolated process (do not load the repository's output DLL into a long-lived PowerShell process, which previously locked the Release build output). Record and review the resulting inventory before making API changes.

## 2026-09-23 — full solution Release build GREEN

The maintainer reran `dotnet build Toolbox.sln -c Release` after closing the PowerShell session that had previously loaded and locked `WhenItFails/bin/Release/net10.0/Afrowave.Toolbox.WhenItFails.dll`.

The complete solution built successfully: 7 projects, 0 warnings, 0 errors, 13.95 s. SDK `NETSDK1057` messages were informational notices from the installed .NET 11 RC SDK; the target framework remains `net10.0`.

The earlier `MSB3026/MSB3027/MSB3021` file-lock failure was environmental, not a source-code regression; no production change was required.

Release build, core/Setter regression suites, catalog/documentation validation, NuGet manifest/resource inspection, and external consumer embedded-template smoke checks are now verified. Next: public API compatibility/scope audit and a final consumer runtime end-to-end check before a stable release.

## 2026-09-23 — isolated NuGet consumer embedded-catalog smoke test GREEN

Maintainer ran an external `net10.0` console application installed with `Afrowave.Toolbox.WhenItFails 0.1.0` via NuGet (with `Afrowave.Toolbox.Essentials 0.2.0` as a transitive dependency). The application instantiated `DefaultJsonsTemplateProvider`, called `GetTemplateFiles(new JsonsOptions())`, checked for exactly five templates and successfully parsed each template's content with `System.Text.Json.JsonDocument.Parse`.

Observed output: GREEN for error, category, code-group, owner, and profiles catalogs, followed by `SUCCESS: All five embedded catalogs loaded and parsed.`

The NuGet package-creation, manifest/payload, isolated restore and embedded-template smoke gates are verified. Full application-level runtime resolution/recovery in the isolated consumer and the independent `dotnet build Toolbox.sln -c Release` output remain unverified.

Next: audit stable public API/package release scope. Avoid treating the 0.1.0 package as published or assuming the full 1.0 release gate has passed.

## 2026-09-23 — external NuGet consumer restore verified

Maintainer confirmed an isolated `net10.0` console project outside Toolbox successfully restored `Afrowave.Toolbox.WhenItFails 0.1.0` from the temporary local package feed. Resolved transitively: `Afrowave.Toolbox.Essentials 0.2.0` and Microsoft.Extensions packages `10.0.9` (Configuration, Configuration.Abstractions, Configuration.Binder, DependencyInjection.Abstractions and Primitives).

The first restore command was malformed by specifying `--source` twice; the corrected single-source-list invocation succeeded. This is a command-line issue, not a package defect.

Clean consumer build output has not been separately confirmed. Next: run an actual isolated consumer smoke test invoking `DefaultJsonsTemplateProvider.GetTemplateFiles(new JsonsOptions())` and parse its five embedded JSON templates, then verify consumer build and execution.

## 2026-09-23 — NuGet manifest and embedded resource verification

Maintainer inspected `Afrowave.Toolbox.WhenItFails.0.1.0.nupkg`:

- `Afrowave.Toolbox.WhenItFails.nuspec` declares `net10.0` and the dependency `Afrowave.Toolbox.Essentials` version `0.2.0`, plus three `Microsoft.Extensions.*` dependencies at `10.0.9`.
- The archive contains `lib/net10.0/Afrowave.Toolbox.WhenItFails.dll`, the XML API documentation, README and license.
- The release DLL manifest contains all five expected embedded resources: `errors.en.json`, `categories.en.json`, `code-groups.en.json`, `owners.en.json` and `profiles.json`, with the `Afrowave.Toolbox.WhenItFails.Bootstrap.Templates.` prefix.

Artifact structure, package dependency declaration and manifest-resource *presence* are verified. Runtime resource consumption and installation from a clean external consumer remain unverified. A standalone Release solution build result was not provided.

Next: pack `Essentials/Essentials.csproj` in Release, then install `Afrowave.Toolbox.WhenItFails` from a local folder containing both packages into a separate .NET 10 test console project; verify restore, build and default-template retrieval. Check the test output before marking the consumer gate GREEN.

## 2026-09-23 — NuGet archive structure inspected

The maintainer listed the contents of `Afrowave.Toolbox.WhenItFails.0.1.0.nupkg`. The archive contains `Afrowave.Toolbox.WhenItFails.nuspec`, `LICENSE.txt`, `README.md`, `lib/net10.0/Afrowave.Toolbox.WhenItFails.dll`, `lib/net10.0/Afrowave.Toolbox.WhenItFails.xml`, plus standard NuGet metadata entries.

This verifies the expected assembly and accompanying documentation/license files are present. Five default JSON catalogs are compiled as `EmbeddedResource`, so their absence as standalone archive entries is expected; manifest resource names and usability still need verification. The nuspec dependency list and clean consumer restore/build also remain pending.

Next: inspect the nuspec and assembly manifest resources, then run an isolated package-consumer smoke test. Do not claim package readiness merely because the archive was created.

## 2026-09-23 — Release NuGet artifact creation verified

Maintainer confirmed that `WhenItFails/bin/Release/Afrowave.Toolbox.WhenItFails.0.1.0.nupkg` exists (93,441 bytes, observed 2026-09-23 14:31 local time).

This verifies artifact creation only. The standalone `dotnet build Toolbox.sln -c Release` result and `dotnet pack` warnings were not included in the reported output. Package archive contents, NuGet dependency metadata, embedded resource names and a clean consumer restore/build remain to be verified before marking the packaging gate complete.

Next: inspect the archive using PowerShell `System.IO.Compression.ZipFile` and inspect the nuspec plus a clean temporary consumer project. Preserve the 0.1.0 version until release readiness is explicitly approved.

## 2026-09-23 — catalog and documentation release gates

Confirmed locally by the maintainer after the fresh core **1157/1157** and Setter **1241/1241** regression checkpoints:

- `dotnet run --project Toolroom/WhenItFails/Setter -- validate .`: PASS; source `Jsons/WhenItFails`, 0 errors, 0 warnings, 0 information.
- `dotnet run --project Toolroom/WhenItFails/Setter -- check-doc-links .`: PASS; 45 Markdown files and 424 local links checked, no broken links.
- `dotnet run --project Toolroom/WhenItFails/Setter -- check-doc-keys .`: PASS; all 49 errors have unique, non-empty, canonical documentation keys.

A standalone `dotnet build Toolbox.sln` result was not included in this report, so whole-solution build remains unconfirmed.

Next gate: `dotnet pack WhenItFails/WhenItFails.csproj -c Release` and inspect the generated NuGet package for successful packaging, README/license placement, required dependency metadata and embedded default catalogs. Do not mark pack/payload as verified before inspecting the output. Then review public API stability and establish explicit 1.0 versus later-adapter scope.

## 2026-09-23 — Setter regression baseline refreshed

Locally confirmed by the maintainer on current master:

```text
Toolroom/WhenItFails/Setter.Tests
Total:   1241
Passed:  1241
Failed:  0
Skipped: 0
Duration: 15.7 s
Build:   successful in 22.3 s
```

The Setter regression baseline is therefore current again and matches the previously recorded count.

Next release-readiness gate: project build plus Setter catalog/documentation validation commands.

## 2026-09-23 — core coverage audit completion

Verified core baseline: **1157/1157 GREEN**.

The post-normalization concrete production audit found no remaining genuine class-level coverage gap requiring a new focused unit test.

Filename mismatches were reviewed rather than treated mechanically:

- `DefaultJsonsTemplateProvider` is directly covered by `DefaultJsonTemplateProviderTests.cs`;
- `JsonsBootstrapFileResult` and `JsonsTemplateFile` are covered by bootstrap value/payload contracts and integration tests;
- `ErrorCatalogInitializationMode` is protected by existing configuration numeric-value contracts;
- `TextKeyNormalizer` is covered by the historically named `TestKeyNormalizerTests.cs`;
- `CatalogValidationHelper` behavior is exercised through public category/owner/profile/error validator contracts, including normalized duplicate detection and header warning/error semantics;
- interface-only files do not require artificial implementation tests.

Do not add tests solely to make filenames line up. Future core tests should continue to be added only for a concrete contract, regression, uncovered behavior or proven failure.

Setter regression suite is freshly verified on current master: **1241/1241 GREEN**. Next release-readiness gate: build the relevant projects and run Setter validation/documentation checks.

## 2026-09-23 — project audit checkpoint

Current locally verified core baseline: **1151/1151 GREEN**.

Repository audit summary:

- main `WhenItFails` library: 114 production C# files;
- `WhenItFails.Tests`: 364 C# test files;
- main project documentation: 13 Markdown topic documents plus root README/status;
- current authoritative project catalogs: 49 errors, 11 categories, 10 code groups, 4 owners and 7 profiles;
- no `TODO`, `FIXME` or `NotImplementedException` markers were found under `WhenItFails`;
- runtime initialization, strict/flexible recovery, active-context publication, status, resolution by ID/name/code, profiles, bootstrap, loading, validation, descriptors and DI all have implemented production paths and test coverage;
- JSON writer and specialized JSON loader audits are complete for the current scope;
- definition-normalizer direct coverage is still being completed: category normalizer is hardened including metadata isolation; code-group normalizer is in progress; owner normalizer remains to be audited directly;
- several csproj placeholder areas remain intentionally unimplemented or empty: `Codes`, `Customization`, `Mapping`, `Exceptions`, `Exporting`, `Serialization`, `Storage`, and the physical `Profiles` folder. These represent future broader-scope capabilities rather than missing core runtime wiring;
- `DependencyInjection` is implemented despite the historical folder placeholder entry;
- package version remains `0.1.0`, and README explicitly states that the public API/catalog structure may still evolve before the first stable release.

For a practical first stable runtime release, remaining work should prioritize completing direct normalizer audits, synchronizing status/docs, running full cross-platform/release gates, and making an explicit decision about which placeholder areas belong to 1.0 versus later releases.

## 2026-09-23 — owner definition normalizer metadata isolation fix

Focused RED confirmed locally by the maintainer:

```text
Normalize_ShouldCopyMetadataWithoutSharingMutableState
Assert.NotSame() Failure: Values are the same instance
Focused suite: 4 total, 3 passed, 1 failed
```

Production fix commit:
`a8461c5e74e7039ddea34cafc700eb61a9abdb65`

Updated:
`WhenItFails/Normalization/ErrorOwnerDefinitionNormalizer.cs`

The normalizer now creates an independent `MetadataBag` copy using the same established pattern as the category and code-group normalizers.

No other normalization behavior changed.

**Focused/full local verification completed; the complete suite is 1157/1157 GREEN.**

## 2026-09-23 — owner definition normalizer metadata isolation contract

Test commit: `d9bbc8169c2b4c10faa1e7ae75b43f76754a54ec`

Baseline: **1156/1156 GREEN**, confirmed locally by the maintainer before this contract was introduced.

Updated:
`WhenItFails.Tests/Normalization/ErrorOwnerDefinitionNormalizerTests.cs`

Contract:
`Normalize_ShouldCopyMetadataWithoutSharingMutableState`

The contract requires an independent `MetadataBag` copy for the normalized owner definition. Mutating normalized metadata must not change the source definition.

Current production assigns `Metadata = definition.Metadata`, so this focused contract is expected to be RED before a narrow implementation fix.

## 2026-09-23 — owner definition normalizer mutable isolation contract

Test commit: `8bb694bcf562f67e01bbaab4f8255bda23e6d769`

Baseline: **1155/1155 GREEN**, confirmed locally by the maintainer before this contract was introduced.

Updated:
`WhenItFails.Tests/Normalization/ErrorOwnerDefinitionNormalizerTests.cs`

Contract:
`Normalize_ShouldCopyAliasesAndMappingsWithoutSharingMutableState`

The contract verifies that normalized aliases and default mappings are independent mutable objects. Mutating the normalized result must not change the source owner definition.

Metadata remains intentionally excluded and will be audited separately.

No production change was made.

**Focused/full local verification completed; the complete suite is 1156/1156 GREEN.**

## 2026-09-23 — owner definition normalizer basic-field contract

Test commit: `3c37367d683466c9df57e37b03adc6cacd560a6d`

Baseline: **1154/1154 GREEN**, confirmed locally by the maintainer before this contract was introduced.

Updated:
`WhenItFails.Tests/Normalization/ErrorOwnerDefinitionNormalizerTests.cs`

Contract:
`Normalize_ShouldNormalizeBasicFields`

The contract verifies normalization and preservation of:

- owner key,
- display name and description,
- numeric owner range,
- built-in flag,
- aliases,
- default mapping keys and values,
- duplicate normalized alias values.

Metadata remains intentionally excluded from this contract and will be audited separately.

No production change was made.

**Focused/full local verification completed; the complete suite is 1155/1155 GREEN.**

## 2026-09-23 — owner definition normalizer null-input contract

Test commit: `4cd932e7eff037fe4974cb9c6dbdaf351d4d1a3b`

Baseline: **1153/1153 GREEN**, confirmed locally by the maintainer before this contract was introduced.

Added:
`WhenItFails.Tests/Normalization/ErrorOwnerDefinitionNormalizerTests.cs`

Contract:
`Normalize_ShouldThrowArgumentNullException_WhenDefinitionIsNull`

This starts direct coverage of `ErrorOwnerDefinitionNormalizer`, which previously had no tests referencing the class.

No production change was made.

**Focused/full local verification completed; the complete suite is 1154/1154 GREEN.**

## 2026-09-23 — code-group definition normalizer metadata isolation fix

Focused RED confirmed locally by the maintainer:

```text
Normalize_ShouldCopyMetadataWithoutSharingMutableState
Assert.NotSame() Failure: Values are the same instance
Focused suite: 4 total, 3 passed, 1 failed
```

Production fix commit:
`8c50d6b49155dfcde87c55d035a98e026ece6097`

Updated:
`WhenItFails/Normalization/ErrorCodeGroupDefinitionNormalizer.cs`

The normalizer now creates an independent `MetadataBag` copy using the same established pattern as the category and profile normalizers.

No other normalization behavior changed.

**Focused/full local verification completed; the complete suite is 1153/1153 GREEN.**

## 2026-09-23 — code-group definition normalizer metadata isolation contract

Test commit: `594423a0d766e4111e83082086eab56fa1be6b6b`

Baseline: **1152/1152 GREEN**, confirmed locally by the maintainer before this contract was introduced.

Updated:
`WhenItFails.Tests/Normalization/ErrorCodeGroupDefinitionNormalizerTests.cs`

Contract:
`Normalize_ShouldCopyMetadataWithoutSharingMutableState`

The contract requires an independent `MetadataBag` copy for the normalized code-group definition. Mutating normalized metadata must not change the source definition.

Current production assigns `Metadata = definition.Metadata`, so this focused contract is expected to be RED before a narrow implementation fix.

## 2026-09-23 — code-group definition normalizer mutable isolation contract

Test commit: `ceca60548b2ea5b795a21a350f0a5769e55d76bb`

Baseline: **1151/1151 GREEN**, confirmed locally by the maintainer before this contract was introduced.

Updated:
`WhenItFails.Tests/Normalization/ErrorCodeGroupDefinitionNormalizerTests.cs`

Contract:
`Normalize_ShouldCopyCollectionsAndMappingsWithoutSharingMutableState`

The contract verifies that normalized default categories, default tags and default mappings are independent mutable objects. Mutating the normalized result must not change the source definition.

Metadata remains intentionally excluded from this contract and will be audited separately because the current implementation assigns it directly.

No production change was made.

**Focused/full local verification completed; the complete suite is 1152/1152 GREEN.**

## 2026-09-23 — code-group definition normalizer basic-field contract

Test commit: `1ff7d9904b1e18381d63a16663516cc88af3920f`

Baseline: **1150/1150 GREEN**, confirmed locally by the maintainer before this contract was introduced.

Updated:
`WhenItFails.Tests/Normalization/ErrorCodeGroupDefinitionNormalizerTests.cs`

Contract:
`Normalize_ShouldNormalizeBasicFields`

The contract verifies normalization of:

- code-group key,
- display name and description,
- code prefix,
- numeric range preservation,
- default categories and tags,
- default mapping keys and values,
- duplicate normalized list values.

No production change was made.

**Focused/full local verification completed; the complete suite is 1151/1151 GREEN.**

## 2026-09-23 — code-group definition normalizer null-input contract

Test commit: `db1da183e6885ca14fbc2217ebcebdc8ca8cbdf3`

Baseline: **1149/1149 GREEN**, confirmed locally by the maintainer before this contract was introduced.

Added:
`WhenItFails.Tests/Normalization/ErrorCodeGroupDefinitionNormalizerTests.cs`

Contract:
`Normalize_ShouldThrowArgumentNullException_WhenDefinitionIsNull`

Required behavior:

```text
Input definition: null
Result: throws ArgumentNullException
```

This starts direct coverage of `ErrorCodeGroupDefinitionNormalizer`, which previously had no tests referencing the class.

No production change was made.

**Focused/full local verification completed; the complete suite is 1150/1150 GREEN.**

## 2026-09-23 — category definition normalizer metadata isolation fix

Focused RED confirmed locally by the maintainer:

```text
Normalize_ShouldCopyMetadataWithoutSharingMutableState
Assert.NotSame() Failure: Values are the same instance
Complete suite: 1149 total, 1148 passed, 1 failed
```

Production fix commit:
`2d75aee252b13310afdf2a411fba757f7ef80adb`

Updated:
`WhenItFails/Normalization/ErrorCategoryDefinitionNormalizer.cs`

The normalizer now creates an independent `MetadataBag` copy using the same established pattern as `ErrorProfileDefinitionNormalizer`.

No other normalization behavior changed.

**Focused/full local verification completed; the complete suite is 1149/1149 GREEN.**

## 2026-09-23 — category definition normalizer metadata isolation contract

Test commit: `02a456933e926697e1dc91308950bb6d7821ef10`

Baseline: **1148/1148 GREEN**, confirmed locally by the maintainer before this contract was introduced.

Updated:
`WhenItFails.Tests/Normalization/ErrorCategoryDefinitionNormalizerTests.cs`

Contract:
`Normalize_ShouldCopyMetadataWithoutSharingMutableState`

The contract mirrors the already-established `ErrorProfileDefinitionNormalizer` behavior: a normalized definition must receive an independent `MetadataBag` copy. Mutating normalized metadata must not change the source definition.

Current production assigns `Metadata = definition.Metadata`, so this focused contract is expected to be RED before a narrow implementation fix.

## 2026-09-23 — category definition normalizer mutable isolation contract

Test commit: `b51f47931405aee86b6fdb4d8f054a7c7b657269`

Baseline: **1147/1147 GREEN**, confirmed locally by the maintainer before this contract was introduced.

Updated:
`WhenItFails.Tests/Normalization/ErrorCategoryDefinitionNormalizerTests.cs`

Contract:
`Normalize_ShouldCopyCollectionsAndMappingsWithoutSharingMutableState`

The contract verifies that normalized aliases, parent categories, default tags and default mappings are independent mutable objects. Mutating the normalized result must not change the original definition.

Metadata is intentionally excluded from this contract and will be audited separately because the current implementation assigns it directly.

No production change was made.

**Focused/full local verification completed; the complete suite is 1148/1148 GREEN.**

## 2026-09-23 — category definition normalizer basic-field contract

Test commit: `b54f0a1977542309e5f0256c0340a99039add10c`

Baseline: **1146/1146 GREEN**, confirmed locally by the maintainer before this contract was introduced.

Updated:
`WhenItFails.Tests/Normalization/ErrorCategoryDefinitionNormalizerTests.cs`

Contract:
`Normalize_ShouldNormalizeBasicFields`

The contract verifies normalization of:

- category key and display text,
- nullable description text,
- aliases,
- parent categories,
- default tags,
- default mapping keys and values,
- duplicate normalized list values.

No production change was made.

**Focused/full local verification completed; the complete suite is 1147/1147 GREEN.**

## 2026-09-23 — category definition normalizer null-input contract

Test commit: `1ab326e1cc9db72780aadab1dc61a4c3ded59c91`

Baseline: **1145/1145 GREEN**, confirmed locally by the maintainer before this contract was introduced.

Added:
`WhenItFails.Tests/Normalization/ErrorCategoryDefinitionNormalizerTests.cs`

Contract:
`Normalize_ShouldThrowArgumentNullException_WhenDefinitionIsNull`

Required behavior:

```text
Input definition: null
Result: throws ArgumentNullException
```

This begins direct coverage of `ErrorCategoryDefinitionNormalizer`, which previously had no tests referencing the class.

No production change was made.

**Focused/full local verification completed; the complete suite is 1146/1146 GREEN.**

## 2026-09-23 — JSON owner catalog loader baseline coverage

Test commit: `b7346eeacace4d96bfc3ed52f4de93214877dd22`

Baseline: **1143/1143 GREEN**, confirmed locally by the maintainer before this coverage was introduced.

Updated:
`WhenItFails.Tests/Loading/JsonErrorOwnerCatalogLoaderTests.cs`

The previous file was only an empty placeholder in the wrong namespace.

Added contracts:

- `Constructor_ShouldThrowArgumentNullException_WhenDocumentLoaderIsNull`
- `LoadFromFileAsync_ShouldLoadOwnerCatalogDocument`

The load contract verifies a real JSON owner catalog including identity, numeric range, built-in flag, aliases and default mappings.

No production change was made.

**Focused/full local verification completed; the complete suite is 1145/1145 GREEN.**

## 2026-09-23 — JSON code-group catalog loader baseline coverage

Test commit: `e2235533ae0ae077b2414ef3337ff8bab43d185a`

Baseline: **1141/1141 GREEN**, confirmed locally by the maintainer before this coverage was introduced.

Updated:
`WhenItFails.Tests/Loading/JsonErrorCodeGroupCatalogLoaderTests.cs`

The previous file was only an empty placeholder in the wrong namespace.

Added contracts:

- `Constructor_ShouldThrowArgumentNullException_WhenDocumentLoaderIsNull`
- `LoadFromFileAsync_ShouldLoadCodeGroupCatalogDocument`

The load contract verifies a real JSON code-group catalog including identity, numeric range, prefix, default categories/tags and default mappings.

No production change was made.

**Focused/full local verification completed; the complete suite is 1143/1143 GREEN.**

## 2026-09-23 — writer nested-directory creation contract

Test commit: `49a9fd8cdcb12f9ff9c95754622d5f7fff6a1ab7`

Baseline: **1140/1140 GREEN**, confirmed locally by the maintainer before this contract was introduced.

Updated:
`WhenItFails.Tests/Loading/JsonCatalogDocumentWriterTests.cs`

Contract:
`SaveToFileAsync_WhenNestedDirectoryDoesNotExist_CreatesDirectoryAndTarget`

The target path points into a missing nested directory tree.

Required behavior:

```text
Response: Success
Nested directory: created
Target file: created
Serialized catalog content: present
```

This verifies the positive `Directory.CreateDirectory(...)` path and complements the existing invalid-parent and existing-file-parent guards.

No production change was made.

**Focused/full local verification completed; the complete suite is 1141/1141 GREEN.**

## 2026-09-23 — writer extensionless-target backup file-name contract

Test commit: `406229ddee0998121d778e4ecc7d26e799277a33`

Baseline: **1139/1139 GREEN**, confirmed locally by the maintainer before this contract was introduced.

Updated:
`WhenItFails.Tests/Loading/JsonCatalogDocumentWriterTests.cs`

Contract:
`SaveToFileAsync_WhenExtensionlessTargetAlreadyExists_BackupFileNameHasStableShape`

The writer replaces an existing target named simply `errors`, with no extension.

Required backup file-name shape:

```text
errors.yyyyMMdd-HHmmss-fff-<32 lowercase hex chars>.bak
```

This exercises the empty-extension branch of `CreateBackupFilePath(...)` and ensures no spurious trailing dot or synthetic extension is introduced.

No production change was made.

**Focused/full local verification completed; the complete suite is 1140/1140 GREEN.**

## 2026-09-23 — writer backup file-name shape contract

Test commit: `6e4f37ad3797c0b07c1106f1f7d95566ec77c298`

Baseline: **1138/1138 GREEN**, confirmed locally by the maintainer before this contract was introduced.

Updated:
`WhenItFails.Tests/Loading/JsonCatalogDocumentWriterTests.cs`

Contract:
`SaveToFileAsync_WhenTargetAlreadyExists_BackupFileNameHasStableShape`

The writer replaces `errors.en.json` and creates one backup.

Required backup file-name shape:

```text
errors.en.yyyyMMdd-HHmmss-fff-<32 lowercase hex chars>.bak.json
```

The timestamp and GUID remain variable, but the externally visible naming convention is fixed. This is relevant because the backup path is returned to callers in the success response.

No production change was made.

**Focused/full local verification completed; the complete suite is 1139/1139 GREEN.**

## 2026-09-23 — writer existing-target I/O failure assertion-order fix

Test fix commit: `08d8de034652aafeeb28538f89b2790be14440c0`

The focused run reached the intended writer failure path, but the test attempted to read the target while its own `FileShare.None` lock was still active, causing the test assertion phase itself to throw `IOException`.

The lock scope now covers only the writer invocation and response assertions. Target-byte preservation and temporary/backup cleanup assertions run after the lock is released.

No production code changed. Focused/full verification remains pending.

## 2026-09-23 — writer existing-target I/O failure compile fix

Compile fix commit: `be897b94f3594717be37e4a1396881d1ca78e81d`

The initial contract used the non-existent enum member `ResultStatus.Failure`. The actual Essentials enum member is `ResultStatus.Failed`.

No production code changed. Focused/full verification remains pending.

## 2026-09-23 — writer existing-target I/O failure contract

Test commit: `336a9159c52ed3911acdcead3a1d8c7a83e879d7`

Baseline: **1137/1137 GREEN**, confirmed locally by the maintainer before this contract was introduced.

Added:
`WhenItFails.Tests/Loading/JsonCatalogDocumentWriterInputOutputErrorContractTests.cs`

Contract:
`SaveToFileAsync_WhenExistingTargetIsLocked_ReturnsInputOutputErrorAndPreservesTarget`

The existing target is opened with `FileShare.None` while the writer attempts a safe replace. Serialization to the temporary file can complete, but backup creation from the locked target should fail with an I/O error.

Required behavior:

```text
Status: Failure
Code: InputOutputError
Original target bytes: unchanged
Backup files: none
Temporary files: none after cleanup
```

This exercises the writer's `IOException` normalization and verifies safe cleanup after a failure that occurs after temporary serialization but before target replacement.

No production change was made.

**Focused/full local verification completed; the complete suite is 1138/1138 GREEN.**

## 2026-09-23 — writer first-save success-message contract

Test commit: `44c454612dc7353664eae7a8d7484928a967b97c`

Baseline: **1136/1136 GREEN**, confirmed locally by the maintainer before this contract was introduced.

Updated:
`WhenItFails.Tests/Loading/JsonCatalogDocumentWriterTests.cs`

Contract:
`SaveToFileAsync_WhenTargetDoesNotExist_ResponseMessageContainsOnlyTargetPath`

The writer saves a valid document to a target that does not yet exist.

Required behavior:

```text
Response: Success
Message: JSON catalog file was saved: <target>
Message contains Backup:: false
```

This completes the public success-message shape for both first-save and replace-with-backup branches.

No production change was made.

**Focused/full local verification completed; the complete suite is 1137/1137 GREEN.**

## 2026-09-23 — writer backup success-message contract

Test commit: `9eb406b3801d623c367234023fcaf70a179f40fb`

Baseline: **1135/1135 GREEN**, confirmed locally by the maintainer before this contract was introduced.

Updated:
`WhenItFails.Tests/Loading/JsonCatalogDocumentWriterTests.cs`

Contract:
`SaveToFileAsync_WhenTargetAlreadyExists_ResponseMessageIncludesActualBackupPath`

The writer replaces an existing target and creates exactly one backup.

Required behavior:

```text
Response: Success
Message: JSON catalog file was saved: <target>. Backup: <actual backup path>
Backup path in message: exactly the path of the file created on disk
```

This fixes the success-message shape at the safe-write boundary and ensures callers receive the real backup location rather than a derived or stale value.

No production change was made.

**Focused/full local verification completed; the complete suite is 1136/1136 GREEN.**

## 2026-09-23 — writer first-save no-backup/no-temp success contract

Test commit: `38e0fa0b3451c8b177d35b33e5c01ebd80272a60`

Baseline: **1134/1134 GREEN**, confirmed locally by the maintainer before this contract was introduced.

Updated:
`WhenItFails.Tests/Loading/JsonCatalogDocumentWriterTests.cs`

Contract:
`SaveToFileAsync_WhenTargetDoesNotExist_LeavesOnlyTargetWithoutBackupOrTemporaryFile`

The writer saves a valid document to a target that does not yet exist.

Required behavior:

```text
Response: Success
Target: exists
Backup files: none
Temporary files: none
Files remaining in directory: exactly one target
```

This complements the successful replace contract by covering the first-write branch where no backup should ever be created.

No production change was made.

**Focused/full local verification completed; the complete suite is 1135/1135 GREEN.**

## 2026-09-23 — writer successful-replace temporary-file cleanup contract

Test commit: `f306a905acfad2d2c430e8564d8793666ea3af3c`

Baseline: **1133/1133 GREEN**, confirmed locally by the maintainer before this contract was introduced.

Updated:
`WhenItFails.Tests/Loading/JsonCatalogDocumentWriterTests.cs`

Contract:
`SaveToFileAsync_WhenExistingTargetIsReplaced_DoesNotLeaveTemporaryFile`

The writer replaces an existing target successfully.

Required behavior:

```text
Response: Success
Target: exists
Backup count: exactly one
Temporary files: none
Files remaining in directory: exactly target + backup
```

This closes the successful safe-write cleanup side alongside the already verified serialization-failure and cancellation cleanup paths.

No production change was made.

**Focused/full local verification completed; the complete suite is 1134/1134 GREEN.**

## 2026-09-23 — writer exact-byte backup preservation contract

Test commit: `9b1ab4c851754e3e06f5ef43afc90c4335a939a9`

Baseline: **1132/1132 GREEN**, confirmed locally by the maintainer before this contract was introduced.

Updated:
`WhenItFails.Tests/Loading/JsonCatalogDocumentWriterTests.cs`

Contract:
`SaveToFileAsync_WhenTargetAlreadyExists_BackupPreservesOriginalBytesExactly`

The existing target is written with a deliberately distinctive byte sequence, including a UTF-8 BOM and CRLF bytes, before the writer replaces it with a newly serialized document.

Required behavior:

```text
Response: Success
Backup count: exactly one
Backup bytes: exactly identical to the original target bytes
Current target bytes: different from the original bytes
```

This protects the safe-write guarantee at the backup boundary: the backup must be a true byte-for-byte copy of the previous file, not merely semantically equivalent JSON.

No production change was made.

**Focused/full local verification completed; the complete suite is 1133/1133 GREEN.**

## 2026-09-23 — writer surrounding-whitespace path normalization contract

Test commit: `876c58aa18a0479dd8f9a18c359a460adc1adc98`

Baseline: **1131/1131 GREEN**, confirmed locally by the maintainer before this contract was introduced.

Updated:
`WhenItFails.Tests/Loading/JsonCatalogDocumentWriterTests.cs`

Contract:
`SaveToFileAsync_WhenFilePathHasSurroundingWhitespace_WritesToTrimmedPath`

The writer receives a valid target path padded with leading and trailing spaces.

Required behavior:

```text
Response: Success
Actual target: trimmed path exists
Padded path: absent
Serialized content: written to trimmed target
```

No production change was made. The existing `filePath.Trim()` normalization is expected to satisfy the contract.

**Focused/full local verification completed; the complete suite is 1132/1132 GREEN.**

## 2026-09-23 — writer null file-path contract

Test commit: `1ddaf745a50b9b021b69da7c235fc65a164bb8cf`

Baseline: **1130/1130 GREEN**, confirmed locally by the maintainer before this contract was introduced.

Updated:
`WhenItFails.Tests/Loading/JsonCatalogDocumentWriterTests.cs`

Contract:
`SaveToFileAsync_WhenFilePathIsNull_ReturnsInvalid`

The writer receives a valid document and a null file path.

Required behavior:

```text
Status: Invalid
Code: FilePathIsEmpty
Message: JSON catalog file path is empty.
```

No production change was made. The existing `string.IsNullOrWhiteSpace(filePath)` guard is expected to classify null consistently with empty and whitespace-only paths.

**Focused/full local verification completed; the complete suite is 1131/1131 GREEN.**

## 2026-09-23 — writer whitespace-only file-path contract

Test commit: `95522c786f2220840723e6b4b3f2eb8f8cd1dad4`

Baseline: **1129/1129 GREEN**, confirmed locally by the maintainer before this contract was introduced.

Updated:
`WhenItFails.Tests/Loading/JsonCatalogDocumentWriterTests.cs`

Contract:
`SaveToFileAsync_WhenFilePathIsWhitespace_ReturnsInvalid`

The writer receives a valid document and a whitespace-only file path.

Required behavior:

```text
Status: Invalid
Code: FilePathIsEmpty
Message: JSON catalog file path is empty.
```

No production change was made. The current writer uses `string.IsNullOrWhiteSpace(filePath)`, so the test is expected to pass before any filesystem operation.

**Focused/full local verification completed; the complete suite is 1130/1130 GREEN.**

## 2026-09-23 — writer no-directory-path classification contract

Test commit: `940cff3f6c0de0e3dc0aecf11665e019c379bb90`

Baseline: **1128/1128 GREEN**, confirmed locally by the maintainer before this contract was introduced.

Updated:
`WhenItFails.Tests/Loading/JsonCatalogDocumentWriterDirectoryPathContractTests.cs`

Contract:
`SaveToFileAsync_WhenFilePathHasNoDirectory_ReturnsInvalidWithoutFilesystemSideEffects`

The writer receives a valid document and a filename-only target such as `errors-<guid>.json`, so `Path.GetDirectoryName(...)` cannot resolve a directory.

Required behavior:

```text
Status: Invalid
Code: DirectoryPathIsEmpty
Message: JSON catalog directory path could not be resolved from: <fileName>
Target file: absent
Filesystem side effects: none
```

No production change was made.

**Focused/full local verification completed; the complete suite is 1129/1129 GREEN.**

## 2026-09-23 — writer null-document entry contract

Test commit: `fdd0cb801999656340e0052b8b0d68c2e1d7bf1f`

Baseline: **1127/1127 GREEN**, confirmed locally by the maintainer before this contract was introduced.

Added:
`WhenItFails.Tests/Loading/JsonCatalogDocumentWriterNullDocumentContractTests.cs`

Contract:
`SaveToFileAsync_WhenDocumentIsNull_ThrowsBeforeFilesystemSideEffects`

The writer receives a null catalog document and a target path whose parent directory does not yet exist.

Required behavior:

```text
Outcome: ArgumentNullException
ParamName: document
Target file: absent
Target directory: absent
Filesystem side effects: none
```

No production change was made. The current writer performs `ArgumentNullException.ThrowIfNull(document)` before validating or mutating the target path, so the test is expected to pass.

**Focused/full local verification completed; the complete suite is 1128/1128 GREEN.**

## 2026-09-23 — writer mid-serialization cancellation preservation regression

Test commit: `fe6cee4da834f8e1d5ec3718ce9280d27620790b`

Baseline: **1126/1126 GREEN**, confirmed locally by the maintainer before this regression was introduced.

Added:
`WhenItFails.Tests/Loading/JsonCatalogDocumentWriterMidSerializationCancellationContractTests.cs`

Contract:
`SaveToFileAsync_WhenCancelledDuringSerialization_PreservesExistingTargetWithoutBackupOrTemporaryFile`

The writer receives an uncancelled token and an existing catalog containing a known original byte sequence. During JSON serialization a document property getter triggers cancellation and throws `OperationCanceledException` from the same token. This exercises the cancellation path after temporary-file creation rather than the previously covered pre-cancelled entry guard.

Required behavior:

```text
Outcome: OperationCanceledException propagates
Existing catalog: identical original bytes
Temporary files: none
Backup files: none
Remaining files: only the original catalog
```

No production change was made. The current writer rethrows `OperationCanceledException`, runs the existing temporary-file cleanup in `finally` and creates backups only after successful serialization; this test is expected to pass.

**The contract is locally verified GREEN in the clean 1127/1127 suite after duplicate coverage removal in commit `dc16d5c14d55afa2d82e93413b4b0191ce11e9a9`.**

## 2026-09-23 — 1126/1126 GREEN unsupported-type JSON writer checkpoint

Contract commit: `88aaf76be4567731bef7f78939043d92cec38371`

Production fix commit: `335f036b33d121648f416b761dd80ead3c60beb2`

Documentation commit: `3692ef551d2573605d6d6b3cef13d99cbd51b7d5`

Locally confirmed by the maintainer:

```text
WhenItFails.Tests: 1126/1126 GREEN
```

Unsupported-type serialization now returns the stable error response, and its temporary-file cleanup is covered. The focused-filter result was not separately reported.

## 2026-09-23 — writer unsupported-type serialization exception contract

Contract commit: `88aaf76be4567731bef7f78939043d92cec38371`

Baseline: **1125/1125 GREEN**, locally confirmed by the maintainer before adding this contract.

Added:
`WhenItFails.Tests/Loading/JsonCatalogDocumentWriterUnsupportedTypeContractTests.cs`

Contract:
`SaveToFileAsync_WhenDocumentContainsUnsupportedType_ReturnsSerializationFailureWithoutTemporaryFile`

The test passes a document with a `System.Type` property. `System.Text.Json` does not support serializing `System.Type` values and can throw `NotSupportedException`, distinct from the cyclic-reference `JsonException` previously tested.

Required response:

```text
Status: Invalid
Code: JsonSerializationFailed
Message prefix: JSON catalog document serialization failed.
Target file: absent
Temporary and backup files: absent
```

The writer should preserve its stable public serialization-error contract and cleanup guarantee rather than allow a serializer `NotSupportedException` to escape.

The focused test confirmed the expected RED on Windows: `System.Text.Json` raised an uncaught `System.NotSupportedException` for `System.Type` at `$.Unsupported`. The exception propagated out of `SaveToFileAsync` rather than producing a `Response`.

Production fix commit: `335f036b33d121648f416b761dd80ead3c60beb2`

Documentation commit: `3692ef551d2573605d6d6b3cef13d99cbd51b7d5`

The writer now catches `NotSupportedException` only around the document-to-temporary-file serialization operation and returns `Invalid` / `JsonSerializationFailed` with the established message prefix. The existing outer `finally` still deletes any temporary file. Cancellation, `JsonException`, genuine I/O and access failures retain their separate handling.

**Complete-suite 1126/1126 GREEN is locally confirmed; the focused result was not separately reported.**

## 2026-09-23 — 1125/1125 GREEN existing-catalog preservation checkpoint

Regression commits: `157961a99102beefc004cb1e8304c796ac300fe2`, `44b5626703c237f9ea43456e341bc79c57d63d5e`

Locally confirmed by the maintainer:

```text
WhenItFails.Tests: 1125/1125 GREEN
```

The original catalog is preserved when new JSON serialization fails; no extra backup or temporary file remains. The focused-filter result was not separately reported.

## 2026-09-23 — existing-catalog serialization failure preservation regression

Test commit: `157961a99102beefc004cb1e8304c796ac300fe2`

Test assertion cleanup commit: `44b5626703c237f9ea43456e341bc79c57d63d5e`

Baseline: **1124/1124 GREEN**, confirmed locally by the maintainer before this regression was introduced.

Added:
`WhenItFails.Tests/Loading/JsonCatalogDocumentWriterExistingTargetSerializationFailureContractTests.cs`

Contract:
`SaveToFileAsync_WhenSerializationFailsForExistingTarget_PreservesOriginalWithoutBackupOrTemporaryFile`

The target `errors.en.json` already exists and contains a known byte sequence. The new document deliberately contains a self-reference so JSON serialization fails.

Expected:

```text
Status: Invalid
Code: JsonSerializationFailed
Existing target: identical original bytes
Temporary files: none
Backup files: none
Remaining files: only the original catalog
```

This covers safe-write preservation of an existing catalog when new serialization fails before the backup/replacement stage. It is distinct from the existing no-target cleanup contract and from the successful backup contract.

No production changes were made. The existing writer serializes to a temporary file, cleans it up on error, and only then creates backups/replaces the target; this regression is expected to pass.

**Complete-suite 1125/1125 GREEN is locally confirmed; the focused result was not separately reported.**

## 2026-09-23 — 1124/1124 GREEN writer existing-file parent checkpoint

Contract commit: `0f494ad52a8c48bb7f809024594038ccda5bb576`

Production fix commit: `fdb7d045599974c3772cc16d4a67ec79cb931fc8`

Documentation commit: `f516f126a81cc284eee278f557f5ebc169ab333e`

Locally confirmed by the maintainer:

```text
WhenItFails.Tests: 1124/1124 GREEN
```

The focused test result was not separately reported; the full suite includes the existing-file-parent contract. Invalid writer parent paths now fail before filesystem mutation.

## 2026-09-23 — writer existing-file parent target contract

Contract commit: `0f494ad52a8c48bb7f809024594038ccda5bb576`

Baseline: **1123/1123 GREEN**, confirmed locally by the maintainer before this contract was introduced.

Added:
`WhenItFails.Tests/Loading/JsonCatalogDocumentWriterFileParentContractTests.cs`

Contract:
`SaveToFileAsync_WhenTargetParentIsExistingFile_ReturnsInvalidWithoutFilesystemMutation`

The requested destination is `blocked/catalog.json`, where the `blocked` parent already exists as a regular file. This is an invalid destination path, not a normal disk I/O failure.

Required response:

```text
Status: Invalid
Code: FilePathParentIsFile
Message: JSON catalog file path has an existing file as a parent.
```

The existing parent file must remain untouched, no target file may appear, and no temporary file or backup may be created. The contract checks that only the original blocking file remains in the test root.

The focused contract confirmed the expected RED on Windows:

```text
Expected: Invalid
Actual:   Failed
```

The writer attempted `Directory.CreateDirectory(directoryPath)` while an existing regular file occupied the required parent component, classifying the invalid destination as a general failure.

Production fix commit: `fdb7d045599974c3772cc16d4a67ec79cb931fc8`

Documentation commit: `f516f126a81cc284eee278f557f5ebc169ab333e`

After the existing-directory destination guard, the writer now checks each canonical parent component for a regular file before `Directory.CreateDirectory(...)` or any temporary-file or backup write. A file ancestor yields the stable `Invalid` / `FilePathParentIsFile` contract while real disk I/O errors keep their existing response path.

**Complete-suite 1124/1124 GREEN is confirmed locally; the focused result was not separately reported.**

## 2026-09-23 — 1123/1123 GREEN writer existing-directory checkpoint

Contract commit: `b13539eb8d38906e2e697786dd4ce4a8d0a84c9b`

Production fix commit: `88111438a9dea5337a04bcf00afff35b8d845091`

Documentation commit: `7cf5d20c015da1508bc90fe89c1e9867647b273a`

Locally confirmed by the maintainer:

```text
WhenItFails.Tests: 1123/1123 GREEN
```

The writer rejects an existing-directory destination before creating a temporary file or backup. The focused result was not separately supplied.

## 2026-09-23 — writer existing-directory target contract

Contract commit: `b13539eb8d38906e2e697786dd4ce4a8d0a84c9b`

Baseline: **1122/1122 GREEN**, confirmed locally by the maintainer before this new contract.

Added:
`WhenItFails.Tests/Loading/JsonCatalogDocumentWriterDirectoryPathContractTests.cs`

Contract:
`SaveToFileAsync_WhenTargetPathIsExistingDirectory_ReturnsInvalidWithoutTemporaryFiles`

The target path already exists as a directory called `catalog.json`. The writer must reject the destination as invalid before generating a temporary file or backup.

Required response:

```text
Status: Invalid
Code: FilePathIsDirectory
Message: JSON catalog file path points to a directory.
```

The target directory must remain present and empty; no `*.tmp` or `*.bak*` files may remain in the parent directory.

The focused contract confirmed the expected RED on Windows:

```text
Expected: Invalid
Actual:   Failed
```

The writer created a temporary file and reached `File.Move(...)` without validating the target's directory type; the operating-system failure was reported as `Failed` rather than `Invalid`.

Production fix commit: `88111438a9dea5337a04bcf00afff35b8d845091`

Documentation commit: `7cf5d20c015da1508bc90fe89c1e9867647b273a`

The writer now checks `Directory.Exists(normalizedFilePath)` before creating the target parent directory or any temporary file. Existing-directory targets return `Invalid` / `FilePathIsDirectory`, while real writer I/O failures retain their existing classification.

**Complete-suite 1123/1123 GREEN confirmed locally; the focused result was not separately reported.**

## 2026-09-23 — 1122/1122 GREEN JSON-null loader checkpoint

Regression commit: `342cc18fc79c2c55ddee085535502a82f63d980a`

Locally confirmed by the maintainer:

```text
WhenItFails.Tests: 1122/1122 GREEN
```

The existing document-is-null response is preserved. The focused test result was not separately supplied.

## 2026-09-23 — loader JSON-null document classification regression

Regression commit: `342cc18fc79c2c55ddee085535502a82f63d980a`

Baseline: **1121/1121 GREEN**, confirmed locally by the maintainer before this regression was introduced.

Added: `WhenItFails.Tests/Loading/JsonCatalogDocumentLoaderNullJsonDocumentContractTests.cs`

Contract:
`LoadFromFileAsync_WhenJsonDocumentIsNull_ReturnsEmptyCatalogDocumentInsteadOfInvalidJson`

The file exists and contains the syntactically valid JSON literal `null`. Deserialization produces no catalog document, which is different from invalid JSON syntax.

Expected response:

```text
Status: Invalid
Data: null
Code: EmptyCatalogDocument
Message: JSON catalog file was loaded, but the catalog document is empty.
```

No production changes were made. The current loader already contains the `document is null` check following deserialization; this regression verifies the documented classification through the public loader API.

**Complete-suite 1122/1122 GREEN is confirmed locally; the focused result was not reported separately.**

## 2026-09-23 — 1121/1121 GREEN loader file-parent checkpoint

Contract commit: `20189c13a577ea7ff75380c2bbafa9cfe1ad2985`

Production fix commit: `dc2cc4184c87975383b7c2cd57709d3e0989e06b`

Documentation commit: `c68ef5aca735f32cb43a9959fe9650603b7f3664`

Locally confirmed by the maintainer on Windows:

```text
WhenItFails.Tests: 1121 total, 1121 passed, 0 failed, 0 skipped
```

The complete suite includes the existing-file-parent contract. The focused filter result was not separately supplied. The existing parent file remains unchanged and a missing catalog retains its own classification.

## 2026-09-23 — catalog document loader existing-file parent contract

Contract commit: `20189c13a577ea7ff75380c2bbafa9cfe1ad2985`

Baseline: **1120/1120 GREEN**, confirmed locally by the maintainer before this contract was introduced.

Added: `WhenItFails.Tests/Loading/JsonCatalogDocumentLoaderFileParentContractTests.cs`

Contract:
`LoadFromFileAsync_WhenFilePathParentIsExistingFile_ReturnsInvalidBeforeFileOpen`

The caller supplies `blocked/catalog.json`, where `blocked` already exists as a regular file. The catalog cannot be opened at that path because its parent component is not a directory.

Required response:

```text
Status: Invalid
Data: null
Code: FilePathParentIsFile
Message: JSON catalog file path has an existing file as a parent.
```

The blocking file must remain unchanged and no catalog file may be created. A genuinely missing catalog retains the established `NotFound` / `FileNotFound` contract.

At the previous checkpoint, the loader only checked whether the final path was a directory and then used `File.Exists(catalogFilePath)`. With a regular file occupying a parent path, both checks returned false and the path was classified as `NotFound`. The maintainer subsequently reported "all green" without a focused result or matched-test count. The available `master` source at that point still lacked the ancestor guard, so that report **cannot establish that this new focused contract passed**; do not mark 1121/1121 verified yet. A focused RED result was not separately supplied.

Production fix commit: `dc2cc4184c87975383b7c2cd57709d3e0989e06b`

Documentation commit: `c68ef5aca735f32cb43a9959fe9650603b7f3664`

The loader now checks canonical parent components for an existing regular file before the missing-file branch and returns the `Invalid` / `FilePathParentIsFile` contract. Existing-directory, genuine missing-file and parser-error classifications remain separately handled.

**Complete-suite 1121/1121 GREEN is confirmed locally on Windows. The focused-filter result was not separately reported.**

## 2026-09-23 — 1120/1120 GREEN catalog loader directory-path checkpoint

Contract commit: `848d8356cd37b79eae1f359760aae84ad38868c6`

Production fix commit: `b66dea9b5942813a5336524ab922e1342e718e59`

Documentation commit: `d542131e467b9a4c1e08f19f5fa6aef9ca472646`

Locally confirmed by the maintainer:

```text
Focused contract: 1/1 GREEN
WhenItFails.Tests: 1120/1120 GREEN
```

Existing directory paths supplied as catalog-file paths are now classified as invalid, without changing the genuine missing-file contract.

## 2026-09-22 — catalog document loader directory-path contract

Contract commit:
`848d8356cd37b79eae1f359760aae84ad38868c6`

Baseline: **1119/1119 GREEN**, confirmed locally by the maintainer before this contract was introduced.

Added:
`WhenItFails.Tests/Loading/JsonCatalogDocumentLoaderDirectoryPathContractTests.cs`

Contract:
`LoadFromFileAsync_WhenFilePathPointsToDirectory_ReturnsInvalidDirectoryPath`

The supplied path exists as a directory. This is not a missing resource and should not be classified as `NotFound`.

Required response:

```text
Status: Invalid
Data: null
Code: FilePathIsDirectory
Message: JSON catalog file path points to a directory.
```

The focused contract confirmed the expected RED on Windows:

```text
Expected: Invalid
Actual:   NotFound
```

The loader treated an existing directory as a missing file because `File.Exists(directoryPath)` returns false.

Production fix commit:
`b66dea9b5942813a5336524ab922e1342e718e59`

Documentation commit:
`d542131e467b9a4c1e08f19f5fa6aef9ca472646`

`JsonCatalogDocumentLoader` now checks `Directory.Exists(...)` before the missing-file branch and returns:

```text
Status: Invalid
Code: FilePathIsDirectory
Message: JSON catalog file path points to a directory.
```

The existing `FileNotFound` contract remains unchanged for paths that genuinely do not exist.

**Focused GREEN and complete-suite 1120/1120 GREEN are pending local verification.**

## 2026-09-22 — 1119/1119 GREEN full-snapshot content-validation checkpoint

Regression commit: `d605eaf9e3503271c7b3e37a9caac988f95b774d`

Locally confirmed by the maintainer:

```text
Focused regression: 1/1 GREEN
WhenItFails.Tests: 1119/1119 GREEN
```

The provider snapshot is fully validated through each item's content before any template file is written. The current bootstrap provider-snapshot audit is complete for materialization failures/cancellation, malformed items, per-item fields, canonical duplicate targets, structural file/directory conflicts, and no-partial-write validation failures.

## 2026-09-22 — later null template-content no-partial-write regression

Regression commit:
`d605eaf9e3503271c7b3e37a9caac988f95b774d`

Baseline: **1118/1118 GREEN**, confirmed locally by the maintainer before this regression was introduced.

Added:
`WhenItFails.Tests/Bootstrap/JsonsBootstrapperLaterNullTemplateContentContractTests.cs`

Contract:
`EnsureWorkspaceAsync_WhenLaterTemplateContentIsNull_ReturnsInvalidWithoutPartialWrites`

The provider returns a valid first template followed by a second template whose name and target are valid but whose `Content` is null.

Required response:

```text
Status: Invalid
Data: null
Code: WIF_JSONS_TEMPLATE_CONTENT_NULL
Message: The JSON template provider returned a template with null content.
```

The package workspace may already exist, but `first.json` must not be written. This verifies that full-snapshot validation extends through the final validated field of every item before the write loop begins.

No production change was made. Current two-phase snapshot validation is expected to satisfy this regression.

**Focused GREEN and complete-suite 1119/1119 GREEN are pending local verification.**

## 2026-09-22 — 1118/1118 GREEN order-independent target-conflict checkpoint

Regression commit: `0fe83d28d95703be905ed27201ae6a8d52901747`

Locally confirmed by the maintainer:

```text
Focused regression: 1/1 GREEN
WhenItFails.Tests: 1118/1118 GREEN
```

The provider snapshot rejects the file/directory target relationship in both encounter orders before any template write.

## 2026-09-22 — reverse-order provider target-conflict regression

Regression commit:
`0fe83d28d95703be905ed27201ae6a8d52901747`

Baseline: **1117/1117 GREEN**, confirmed locally by the maintainer before this regression was introduced.

Added contract:
`EnsureWorkspaceAsync_WhenChildTemplatePrecedesParentFileTarget_ReturnsInvalidWithoutPartialWrites`

Provider order:

```text
Nested/item.json
Nested
```

Expected response remains:

```text
Status: Invalid
Data: null
Code: WIF_JSONS_TEMPLATE_TARGET_FILE_NAME_CONFLICT
Message: The JSON template provider returned conflicting target file paths.
```

Neither target may be written. No production change was made; this regression verifies that snapshot conflict detection is order-independent.

**Focused GREEN and complete-suite 1118/1118 GREEN are pending local verification.**

## 2026-09-22 — 1117/1117 GREEN provider target-relationship checkpoint

Contract commit: `5fca0f000da3b6687889b43976c7c2ae5ab4f368`

Production fix commit: `c6be6abaedb729958339814d6ae2e9798a286ed8`

Documentation commit: `c02f4e1766fb20ce771e8405cf67fbc5cba995cb`

Locally confirmed by the maintainer:

```text
Focused contract: 1/1 GREEN
WhenItFails.Tests: 1117/1117 GREEN
```

A provider snapshot can no longer contain a target file whose path another target requires as a directory. The conflict is detected before any template write.

## 2026-09-22 — provider snapshot file/directory target-conflict contract

Contract commit:
`5fca0f000da3b6687889b43976c7c2ae5ab4f368`

Baseline: **1116/1116 GREEN**, confirmed locally by the maintainer before this contract was introduced.

Added:
`WhenItFails.Tests/Bootstrap/JsonsBootstrapperConflictingTemplateTargetRelationshipContractTests.cs`

Contract:
`EnsureWorkspaceAsync_WhenTemplateTargetIsParentFileOfAnotherTarget_ReturnsInvalidWithoutPartialWrites`

The provider returns two individually valid targets:

```text
Nested
Nested/item.json
```

The first identifies a file. The second requires that same path to be a directory. Neither target is invalid in isolation; the conflict exists only at the snapshot relationship level.

Required response:

```text
Status: Invalid
Data: null
Code: WIF_JSONS_TEMPLATE_TARGET_FILE_NAME_CONFLICT
Message: The JSON template provider returned conflicting target file paths.
```

The package workspace may already exist because provider validation happens after workspace creation, but neither `Nested` nor `Nested/item.json` may be created. This protects the full-snapshot no-partial-write guarantee.

The focused contract confirmed the expected RED on Windows:

```text
Expected: Invalid
Actual:   Failed
```

The first target was written as file `Nested`; the later child target then failed when bootstrap attempted to use that same path as a directory.

Production fix commit:
`c6be6abaedb729958339814d6ae2e9798a286ed8`

Documentation commit:
`c02f4e1766fb20ce771e8405cf67fbc5cba995cb`

Snapshot validation now tracks both canonical target-file paths and directory paths required by prospective targets. It rejects either relationship:

```text
Nested
Nested/item.json
```

or the reverse encounter order before the write loop with:

```text
Status: Invalid
Code: WIF_JSONS_TEMPLATE_TARGET_FILE_NAME_CONFLICT
Message: The JSON template provider returned conflicting target file paths.
```

The relationship sets use the same platform-aware comparer as canonical duplicate detection.

**Focused GREEN and complete-suite 1117/1117 GREEN are pending local verification.**

## 2026-09-22 — 1116/1116 GREEN canonical provider-target uniqueness checkpoint

Contract commit: `e0c1703cf23fb6a971e8961f3776d1f1c59ce6e0`

Production fix commit: `60b583e513853349e43ddf4cbb731f9845a459e1`

Compile-fix commit: `09e980651c45c10d3e9688b4aa111e8fc56897bb`

Documentation commit: `f823c6bc451952412dfe382b5c340602637fbd83`

Locally confirmed by the maintainer:

```text
Focused contract: 1/1 GREEN
WhenItFails.Tests: 1116/1116 GREEN
```

Canonical aliases in the provider snapshot are now rejected before the write loop. Path uniqueness is case-insensitive on Windows and ordinal on non-Windows systems.

## 2026-09-22 — duplicate canonical provider-target contract

Contract commit:
`e0c1703cf23fb6a971e8961f3776d1f1c59ce6e0`

Baseline: **1115/1115 GREEN**, confirmed locally by the maintainer before this contract was introduced.

Added:
`WhenItFails.Tests/Bootstrap/JsonsBootstrapperDuplicateCanonicalTemplateTargetContractTests.cs`

Contract:
`EnsureWorkspaceAsync_WhenTemplateTargetsResolveToSameFile_ReturnsInvalidWithoutPartialWrites`

The provider returns two individually valid target names:

```text
Nested/item.json
Nested/./item.json
```

Their lexical forms differ, but canonical resolution identifies the same target file. Provider output must be unambiguous before writes begin.

Required response:

```text
Status: Invalid
Data: null
Code: WIF_JSONS_TEMPLATE_TARGET_FILE_NAME_DUPLICATE
Message: The JSON template provider returned multiple templates for the same target file.
```

The package workspace may already exist because provider validation occurs after workspace creation, but `Nested` and `item.json` must not be created. This preserves the full-snapshot no-partial-write contract.

The focused contract confirmed the expected RED on Windows:

```text
Expected: Invalid
Actual:   Success
```

The provider snapshot accepted both aliases, wrote the first target, treated the second as the same already-existing file, and returned success.

Production fix commit:
`60b583e513853349e43ddf4cbb731f9845a459e1`

Documentation commit:
`f823c6bc451952412dfe382b5c340602637fbd83`

Snapshot validation now tracks full canonical target paths before the write loop. Duplicate canonical targets return:

```text
Status: Invalid
Code: WIF_JSONS_TEMPLATE_TARGET_FILE_NAME_DUPLICATE
Message: The JSON template provider returned multiple templates for the same target file.
```

Path uniqueness follows platform semantics: `StringComparer.OrdinalIgnoreCase` on Windows and `StringComparer.Ordinal` elsewhere. Existing per-target invalid/outside-package contracts remain unchanged, and duplicate aliases are rejected before any nested directory or template file is created.

Local verification after the first production fix was blocked before test execution by compiler error:

```text
CS0136: fullTemplateTargetFilePath cannot be declared in this scope because the name is already used in an enclosing local scope.
```

Compile-fix commit:
`09e980651c45c10d3e9688b4aa111e8fc56897bb`

The new canonical-path local is now named `canonicalTemplateTargetFilePath`; behavior is unchanged.

**Focused GREEN and complete-suite 1116/1116 GREEN are pending local verification.**

## 2026-09-22 — 1115/1115 GREEN completed contained parent-directory filename checkpoint

Contract commit: `34f6375cb44573899379b59bfdc4f9c94eea1bb5`

Production fix commit: `44a0679f0df8e850b6063ae479d9e00a0fb8d77a`

Documentation commit: `6283075f4859aeb678108f6fc342a5766330352c`

Locally confirmed by the maintainer:

```text
Focused contract: 1/1 GREEN
WhenItFails.Tests: 1115/1115 GREEN
```

Provider targets plus all five caller-configured catalog filename fields now reject contained terminal parent-directory semantics such as `Nested/Sub/..`, while genuine escapes retain their outside-package classification.

## 2026-09-22 — contained terminal parent-directory profiles filename contract

Contract commit:
`34f6375cb44573899379b59bfdc4f9c94eea1bb5`

Baseline: **1114/1114 GREEN**, confirmed locally by the maintainer before this contract was introduced.

Added:
`WhenItFails.Tests/Bootstrap/JsonsBootstrapperNestedParentDirectoryProfilesFileNameContractTests.cs`

Contract:
`EnsureWorkspaceAsync_WhenProfilesFileNameEndsWithParentDirectorySegment_ReturnsInvalidBeforeProviderOrFilesystem`

Caller configuration sets:

```csharp
ProfilesFileName = Path.Combine(
    "Nested",
    "Sub",
    "..")
```

Canonical resolution stays inside the package and resolves to the `Nested` directory itself. This is not an outside-package escape, but it cannot represent a profile-catalog file.

Required response:

```text
Status: Invalid
Data: null
Code: WIF_JSONS_PROFILE_CATALOG_FILE_NAME_INVALID
Message: The profile catalog file name is invalid.
```

The template provider must not be invoked and the root/package workspace must remain uncreated.

The focused contract confirmed the expected RED on Windows:

```text
Expected: Invalid
Actual:   Success
```

The contained semantic-directory profiles filename passed caller validation and reached successful bootstrap completion.

Production fix commit:
`44a0679f0df8e850b6063ae479d9e00a0fb8d77a`

Documentation commit:
`6283075f4859aeb678108f6fc342a5766330352c`

After successful containment, `ProfilesFileName` now rejects a normalized terminal parent-directory segment:

```text
Status: Invalid
Code: WIF_JSONS_PROFILE_CATALOG_FILE_NAME_INVALID
Message: The profile catalog file name is invalid.
```

The guard deliberately runs only after containment succeeds. True parent-directory escapes therefore retain the profile-specific outside-package classification, while contained semantic-directory filenames are rejected before workspace creation or template-provider invocation.

With this change, provider targets plus all five caller-configured catalog filename fields now cover contained terminal parent-directory semantics.

**Focused GREEN and complete-suite 1115/1115 GREEN are pending local verification.**

## 2026-09-22 — 1114/1114 GREEN contained parent-directory owner-catalog checkpoint

Contract commit: `2116a75b36c7ad5296c5e0c6112ed9e8153eae49`

Production fix commit: `27b345c7001ec05476f58b85ae0c15aae278f10e`

Documentation commit: `074b95ce7ee0c7c7ccd0fd0dfee7220a6bba2b99`

Locally confirmed by the maintainer:

```text
Focused contract: 1/1 GREEN
WhenItFails.Tests: 1114/1114 GREEN
```

Contained terminal parent-directory owner-catalog filenames such as `Nested/Sub/..` are rejected before workspace creation or template-provider invocation, while true parent-directory escapes retain the outside-package contract.

## 2026-09-22 — contained terminal parent-directory owner-catalog filename contract

Contract commit:
`2116a75b36c7ad5296c5e0c6112ed9e8153eae49`

Baseline: **1113/1113 GREEN**, confirmed locally by the maintainer before this contract was introduced.

Added:
`WhenItFails.Tests/Bootstrap/JsonsBootstrapperNestedParentDirectoryOwnerCatalogFileNameContractTests.cs`

Contract:
`EnsureWorkspaceAsync_WhenOwnerCatalogFileNameEndsWithParentDirectorySegment_ReturnsInvalidBeforeProviderOrFilesystem`

Caller configuration sets:

```csharp
OwnerCatalogFileName = Path.Combine(
    "Nested",
    "Sub",
    "..")
```

Canonical resolution stays inside the package and resolves to the `Nested` directory itself. This is not an outside-package escape, but it cannot represent an owner-catalog file.

Required response:

```text
Status: Invalid
Data: null
Code: WIF_JSONS_OWNER_CATALOG_FILE_NAME_INVALID
Message: The owner catalog file name is invalid.
```

The template provider must not be invoked and the root/package workspace must remain uncreated.

The focused contract confirmed the expected RED on Windows:

```text
Expected: Invalid
Actual:   Success
```

The contained semantic-directory owner filename passed caller validation and reached successful bootstrap completion.

Production fix commit:
`27b345c7001ec05476f58b85ae0c15aae278f10e`

Documentation commit:
`074b95ce7ee0c7c7ccd0fd0dfee7220a6bba2b99`

After successful containment, `OwnerCatalogFileName` now rejects a normalized terminal parent-directory segment:

```text
Status: Invalid
Code: WIF_JSONS_OWNER_CATALOG_FILE_NAME_INVALID
Message: The owner catalog file name is invalid.
```

The guard deliberately runs only after containment succeeds. True parent-directory escapes therefore retain the owner-specific outside-package classification, while contained semantic-directory filenames are rejected before workspace creation or template-provider invocation.

**Focused GREEN and complete-suite 1114/1114 GREEN are pending local verification.**

## 2026-09-22 — 1113/1113 GREEN contained parent-directory code-group checkpoint

Contract commit: `c6d3e816e886724974ff9d5110cc2dba020cd69f`

Production fix commit: `e1d9211eb6a2950d1fe6dad59b73c65d5cdeedf8`

Documentation commit: `c1573b9efc2f2011ae4ca775152fa8a5d7c5f9ef`

Locally confirmed by the maintainer:

```text
Focused contract: 1/1 GREEN
WhenItFails.Tests: 1113/1113 GREEN
```

Contained terminal parent-directory code-group catalog filenames such as `Nested/Sub/..` are rejected before workspace creation or template-provider invocation, while true parent-directory escapes retain the outside-package contract.

## 2026-09-22 — contained terminal parent-directory code-group catalog filename contract

Contract commit:
`c6d3e816e886724974ff9d5110cc2dba020cd69f`

Baseline: **1112/1112 GREEN**, confirmed locally by the maintainer before this contract was introduced.

Added:
`WhenItFails.Tests/Bootstrap/JsonsBootstrapperNestedParentDirectoryCodeGroupCatalogFileNameContractTests.cs`

Contract:
`EnsureWorkspaceAsync_WhenCodeGroupCatalogFileNameEndsWithParentDirectorySegment_ReturnsInvalidBeforeProviderOrFilesystem`

Caller configuration sets:

```csharp
CodeGroupCatalogFileName = Path.Combine(
    "Nested",
    "Sub",
    "..")
```

Canonical resolution stays inside the package and resolves to the `Nested` directory itself. This is not an outside-package escape, but it cannot represent a code-group catalog file.

Required response:

```text
Status: Invalid
Data: null
Code: WIF_JSONS_CODE_GROUP_CATALOG_FILE_NAME_INVALID
Message: The code group catalog file name is invalid.
```

The template provider must not be invoked and the root/package workspace must remain uncreated.

The focused contract confirmed the expected RED on Windows:

```text
Expected: Invalid
Actual:   Success
```

The contained semantic-directory code-group filename passed caller validation and reached successful bootstrap completion.

Production fix commit:
`e1d9211eb6a2950d1fe6dad59b73c65d5cdeedf8`

Documentation commit:
`c1573b9efc2f2011ae4ca775152fa8a5d7c5f9ef`

After successful containment, `CodeGroupCatalogFileName` now rejects a normalized terminal parent-directory segment:

```text
Status: Invalid
Code: WIF_JSONS_CODE_GROUP_CATALOG_FILE_NAME_INVALID
Message: The code group catalog file name is invalid.
```

The guard deliberately runs only after containment succeeds. True parent-directory escapes therefore retain the code-group-specific outside-package classification, while contained semantic-directory filenames are rejected before workspace creation or template-provider invocation.

**Focused GREEN and complete-suite 1113/1113 GREEN are pending local verification.**

## 2026-09-22 — 1112/1112 GREEN contained parent-directory category-catalog checkpoint

Contract commit: `98a84ce3adfb9160bb79dd13713ba4d6a558e199`

Production fix commit: `f986fd77a019b8f31caf3c58b0a9b4918c14bf4f`

Documentation commit: `9c46e4c150a70f6a28d1eda7b7c82cdb4910b830`

Locally confirmed by the maintainer:

```text
Focused contract: 1/1 GREEN
WhenItFails.Tests: 1112/1112 GREEN
```

Contained terminal parent-directory category-catalog filenames such as `Nested/Sub/..` are rejected before workspace creation or template-provider invocation, while true parent-directory escapes retain the outside-package contract.

## 2026-09-22 — contained terminal parent-directory category-catalog filename contract

Contract commit:
`98a84ce3adfb9160bb79dd13713ba4d6a558e199`

Baseline: **1111/1111 GREEN**, confirmed locally by the maintainer before this contract was introduced.

Added:
`WhenItFails.Tests/Bootstrap/JsonsBootstrapperNestedParentDirectoryCategoryCatalogFileNameContractTests.cs`

Contract:
`EnsureWorkspaceAsync_WhenCategoryCatalogFileNameEndsWithParentDirectorySegment_ReturnsInvalidBeforeProviderOrFilesystem`

Caller configuration sets:

```csharp
CategoryCatalogFileName = Path.Combine(
    "Nested",
    "Sub",
    "..")
```

Canonical resolution stays inside the package and resolves to the `Nested` directory itself. This is not an outside-package escape, but it cannot represent a category-catalog file.

Required response:

```text
Status: Invalid
Data: null
Code: WIF_JSONS_CATEGORY_CATALOG_FILE_NAME_INVALID
Message: The category catalog file name is invalid.
```

The template provider must not be invoked and the root/package workspace must remain uncreated.

The focused contract confirmed the expected RED on Windows:

```text
Expected: Invalid
Actual:   Success
```

The contained semantic-directory category-catalog filename passed caller validation and reached successful bootstrap completion.

Production fix commit:
`f986fd77a019b8f31caf3c58b0a9b4918c14bf4f`

Documentation commit:
`9c46e4c150a70f6a28d1eda7b7c82cdb4910b830`

After successful containment, `CategoryCatalogFileName` now rejects a normalized terminal parent-directory segment:

```text
Status: Invalid
Code: WIF_JSONS_CATEGORY_CATALOG_FILE_NAME_INVALID
Message: The category catalog file name is invalid.
```

The guard deliberately runs only after containment succeeds. True parent-directory escapes therefore retain the category-specific outside-package classification, while contained semantic-directory filenames are rejected before workspace creation or template-provider invocation.

**Focused GREEN and complete-suite 1112/1112 GREEN are pending local verification.**

## 2026-09-22 — 1111/1111 GREEN contained parent-directory error-catalog checkpoint

Contract commit: `6e1ab5d8c6fe2e50d79ea0892fa52c5f40bf8092`

Production fix commit: `39df8860aaeba07b0632d0bbb5b277ffbed04a57`

Documentation commit: `b60b7f10976a2cef31dac900a39804f84d1b5eab`

Locally confirmed by the maintainer:

```text
Focused contract: 1/1 GREEN
WhenItFails.Tests: 1111/1111 GREEN
```

Contained terminal parent-directory error-catalog filenames such as `Nested/Sub/..` are rejected before workspace creation or template-provider invocation, while true parent-directory escapes retain the outside-package contract.

## 2026-09-22 — contained terminal parent-directory error-catalog filename contract

Contract commit:
`6e1ab5d8c6fe2e50d79ea0892fa52c5f40bf8092`

Baseline: **1110/1110 GREEN**, confirmed locally by the maintainer before this contract was introduced.

Added:
`WhenItFails.Tests/Bootstrap/JsonsBootstrapperNestedParentDirectoryErrorCatalogFileNameContractTests.cs`

Contract:
`EnsureWorkspaceAsync_WhenErrorCatalogFileNameEndsWithParentDirectorySegment_ReturnsInvalidBeforeProviderOrFilesystem`

Caller configuration sets:

```csharp
ErrorCatalogFileName = Path.Combine(
    "Nested",
    "Sub",
    "..")
```

Canonical resolution stays inside the package and resolves to the `Nested` directory itself. This is not an outside-package escape, but it cannot represent an error-catalog file.

Required response:

```text
Status: Invalid
Data: null
Code: WIF_JSONS_ERROR_CATALOG_FILE_NAME_INVALID
Message: The error catalog file name is invalid.
```

The template provider must not be invoked and the root/package workspace must remain uncreated.

The focused contract confirmed the expected RED on Windows:

```text
Expected: Invalid
Actual:   Success
```

The contained semantic-directory error-catalog filename passed caller validation and reached successful bootstrap completion.

Production fix commit:
`39df8860aaeba07b0632d0bbb5b277ffbed04a57`

Documentation commit:
`b60b7f10976a2cef31dac900a39804f84d1b5eab`

After successful containment, `ErrorCatalogFileName` now rejects a normalized terminal parent-directory segment:

```text
Status: Invalid
Code: WIF_JSONS_ERROR_CATALOG_FILE_NAME_INVALID
Message: The error catalog file name is invalid.
```

The guard deliberately runs only after containment succeeds. True parent-directory escapes therefore retain `WIF_JSONS_ERROR_CATALOG_FILE_NAME_OUTSIDE_PACKAGE`, while contained semantic-directory filenames are rejected before workspace creation or template-provider invocation.

**Focused GREEN and complete-suite 1111/1111 GREEN are pending local verification.**

## 2026-09-22 — 1110/1110 GREEN contained parent-directory provider-target checkpoint

Contract commit: `db0b391f21eac8ee5768e666915b4cbbd573e754`

Production fix commit: `b232973c77ff56870ff28f2a21155a0e21a583df`

Documentation commit: `13b33905fa0b952fef459f7315aee24d7552e7c2`

Locally confirmed by the maintainer:

```text
Focused contract: 1/1 GREEN
WhenItFails.Tests: 1110/1110 GREEN
```

Contained terminal parent-directory provider targets such as `Nested/Sub/..` are rejected before template writes, while true parent-directory escapes retain the outside-package contract.

## 2026-09-22 — contained terminal parent-directory provider-target contract

Contract commit:
`db0b391f21eac8ee5768e666915b4cbbd573e754`

Baseline: **1109/1109 GREEN**, confirmed locally by the maintainer before this contract was introduced.

Added:
`WhenItFails.Tests/Bootstrap/JsonsBootstrapperNestedParentDirectoryTemplateTargetContractTests.cs`

Contract:
`EnsureWorkspaceAsync_WhenLaterTemplateTargetEndsWithParentDirectorySegment_ReturnsInvalidWithoutPartialWrites`

The provider returns two templates. The first target is valid `first.json`. The later target is:

```csharp
TargetFileName = Path.Combine("Nested", "Sub", "..")
```

Canonical resolution stays inside the package and resolves to the `Nested` directory itself. This is not an outside-package escape, but it cannot represent a template file.

Required response:

```text
Status: Invalid
Data: null
Code: WIF_JSONS_TEMPLATE_TARGET_FILE_NAME_INVALID
Message: The JSON template provider returned a template with an invalid target file name.
```

The complete provider snapshot must be rejected before writes begin: `first.json` must remain absent and `Nested` must not be created.

The focused contract confirmed the expected RED on Windows:

```text
Expected: Invalid
Actual:   Failed
```

The contained semantic-directory target passed full-snapshot validation and reached filesystem creation, where it was normalized as a generic failure.

Production fix commit:
`b232973c77ff56870ff28f2a21155a0e21a583df`

Documentation commit:
`13b33905fa0b952fef459f7315aee24d7552e7c2`

After containment succeeds, provider snapshot validation now rejects a normalized target whose final segment is `..`:

```text
Status: Invalid
Code: WIF_JSONS_TEMPLATE_TARGET_FILE_NAME_INVALID
Message: The JSON template provider returned a template with an invalid target file name.
```

The guard deliberately runs only after successful containment. A true parent-directory escape outside the package therefore retains `WIF_JSONS_TEMPLATE_TARGET_FILE_NAME_OUTSIDE_PACKAGE`, while contained semantic-directory targets are rejected before any template write begins.

**Focused GREEN and complete-suite 1110/1110 GREEN are pending local verification.**

## 2026-09-22 — 1109/1109 GREEN completed nested current-directory filename checkpoint

Contract commit: `08e1399ca69652f39a57aa54fe357b20645b818c`

Production fix commit: `2adf8c71ca48c38e5db2d220b46ed7270b532e49`

Documentation commit: `260cdc212e0a16342188dae68fae61cfd02fff74`

Locally confirmed by the maintainer:

```text
Focused contract: 1/1 GREEN
WhenItFails.Tests: 1109/1109 GREEN
```

The provider target plus all five caller-configured catalog filename fields now reject terminal current-directory segments such as `Nested/.` before write/provider boundaries appropriate to each contract.

## 2026-09-22 — nested current-directory profiles filename contract

Contract commit:
`08e1399ca69652f39a57aa54fe357b20645b818c`

Baseline: **1108/1108 GREEN**, confirmed locally by the maintainer before this contract was introduced.

Added:
`WhenItFails.Tests/Bootstrap/JsonsBootstrapperNestedCurrentDirectoryProfilesFileNameContractTests.cs`

Contract:
`EnsureWorkspaceAsync_WhenProfilesFileNameEndsWithCurrentDirectorySegment_ReturnsInvalidBeforeProviderOrFilesystem`

Caller configuration sets:

```csharp
ProfilesFileName = Path.Combine("Nested", ".")
```

The value is lexically contained inside the package, but it resolves to the `Nested` directory itself rather than a file within that directory. Because `Nested` does not yet exist, an existing-directory check alone cannot recognize this invalid filename.

Required response:

```text
Status: Invalid
Data: null
Code: WIF_JSONS_PROFILE_CATALOG_FILE_NAME_INVALID
Message: The profile catalog file name is invalid.
```

The tracking template provider must not be invoked, and no root/package workspace directory may be created.

The focused contract confirmed the expected RED on Windows:

```text
Expected: Invalid
Actual:   Success
```

The nested semantic-directory profiles filename passed caller validation and reached successful bootstrap completion.

Production fix commit:
`2adf8c71ca48c38e5db2d220b46ed7270b532e49`

Documentation commit:
`260cdc212e0a16342188dae68fae61cfd02fff74`

After normalization and before containment/provider invocation, `ProfilesFileName` now rejects a final current-directory segment such as `Nested/.`:

```text
Status: Invalid
Code: WIF_JSONS_PROFILE_CATALOG_FILE_NAME_INVALID
Message: The profile catalog file name is invalid.
```

All five caller-configured catalog filename fields now enforce the same terminal-current-directory semantic-directory rule. Valid nested filenames, package-directory equality classification, outside-package handling, existing-directory detection, and file-parent guards remain unchanged.

**Focused GREEN and complete-suite 1109/1109 GREEN are pending local verification.**

## 2026-09-22 — 1108/1108 GREEN nested owner current-directory checkpoint

Contract commit: `defecc110cfb71b91b02691011b2c655e6db4405`

Production fix commit: `29ad1525aeb33de2d6b2e6c14246c55c1429c6e0`

Documentation commit: `717243ce858ae72ca56bde491ebee9b223b21567`

Locally confirmed by the maintainer:

```text
Focused contract: 1/1 GREEN
WhenItFails.Tests: 1108/1108 GREEN
```

Nested semantic-directory owner-catalog filenames such as `Nested/.` are rejected before workspace creation or template-provider invocation.

## 2026-09-22 — nested current-directory owner-catalog filename contract

Contract commit:
`defecc110cfb71b91b02691011b2c655e6db4405`

Baseline: **1107/1107 GREEN**, confirmed locally by the maintainer before this contract was introduced.

Added:
`WhenItFails.Tests/Bootstrap/JsonsBootstrapperNestedCurrentDirectoryOwnerCatalogFileNameContractTests.cs`

Contract:
`EnsureWorkspaceAsync_WhenOwnerCatalogFileNameEndsWithCurrentDirectorySegment_ReturnsInvalidBeforeProviderOrFilesystem`

Caller configuration sets:

```csharp
OwnerCatalogFileName = Path.Combine("Nested", ".")
```

The value is lexically contained inside the package but semantically resolves to the `Nested` directory itself rather than a file beneath it. Because `Nested` does not yet exist, the existing-directory guard cannot identify the problem.

Required response:

```text
Status: Invalid
Data: null
Code: WIF_JSONS_OWNER_CATALOG_FILE_NAME_INVALID
Message: The owner catalog file name is invalid.
```

The template provider must not be invoked and the root/package workspace must remain uncreated.

The focused contract confirmed the expected RED on Windows:

```text
Expected: Invalid
Actual:   Success
```

The nested semantic-directory owner filename passed caller validation and reached successful bootstrap completion.

Production fix commit:
`29ad1525aeb33de2d6b2e6c14246c55c1429c6e0`

Documentation commit:
`717243ce858ae72ca56bde491ebee9b223b21567`

After normalization and before containment/provider invocation, `OwnerCatalogFileName` now rejects a final current-directory segment such as `Nested/.`:

```text
Status: Invalid
Code: WIF_JSONS_OWNER_CATALOG_FILE_NAME_INVALID
Message: The owner catalog file name is invalid.
```

Valid nested owner filenames, package-directory equality classification, outside-package handling, existing-directory detection, and file-parent guards remain unchanged.

**Focused GREEN and complete-suite 1108/1108 GREEN are pending local verification.**

## 2026-09-22 — 1107/1107 GREEN nested code-group current-directory checkpoint

Contract commit: `79ef20b37308f01f275f35db308e665fc30ef207`

Production fix commit: `216411d884f82a56af9e3dd126aec68e25f26c91`

Documentation commit: `251f94eb4181819d42ae82930f21cca8657b892e`

Locally confirmed by the maintainer:

```text
Focused contract: 1/1 GREEN
WhenItFails.Tests: 1107/1107 GREEN
```

Nested semantic-directory code-group catalog filenames such as `Nested/.` are now rejected before workspace creation or template-provider invocation.

## 2026-09-22 — nested current-directory code-group catalog filename contract

Contract commit:
`79ef20b37308f01f275f35db308e665fc30ef207`

Baseline: **1106/1106 GREEN**, confirmed locally by the maintainer before this contract was introduced.

Added:
`WhenItFails.Tests/Bootstrap/JsonsBootstrapperNestedCurrentDirectoryCodeGroupCatalogFileNameContractTests.cs`

Contract:
`EnsureWorkspaceAsync_WhenCodeGroupCatalogFileNameEndsWithCurrentDirectorySegment_ReturnsInvalidBeforeProviderOrFilesystem`

Caller configuration sets:

```csharp
CodeGroupCatalogFileName = Path.Combine("Nested", ".")
```

The value is lexically contained inside the package but semantically resolves to the `Nested` directory itself rather than a file beneath it. Because `Nested` does not yet exist, the existing-directory guard cannot identify the problem.

Required response:

```text
Status: Invalid
Data: null
Code: WIF_JSONS_CODE_GROUP_CATALOG_FILE_NAME_INVALID
Message: The code group catalog file name is invalid.
```

The template provider must not be invoked and the root/package workspace must remain uncreated.

The focused contract confirmed the expected RED on Windows:

```text
Expected: Invalid
Actual:   Success
```

The nested semantic-directory code-group filename passed caller validation and reached successful bootstrap completion.

Production fix commit:
`216411d884f82a56af9e3dd126aec68e25f26c91`

Documentation commit:
`251f94eb4181819d42ae82930f21cca8657b892e`

After normalization and before containment/provider invocation, `CodeGroupCatalogFileName` now rejects a final current-directory segment such as `Nested/.`:

```text
Status: Invalid
Code: WIF_JSONS_CODE_GROUP_CATALOG_FILE_NAME_INVALID
Message: The code group catalog file name is invalid.
```

Valid nested code-group filenames, package-directory equality classification, outside-package handling, existing-directory detection, and file-parent guards remain unchanged.

**Focused GREEN and complete-suite 1107/1107 GREEN are pending local verification.**

## 2026-09-22 — 1106/1106 GREEN nested category-catalog current-directory checkpoint

Contract commit: `dc2802122b0053dc575240c2d0b165ec392f5b55`

Production fix commit: `513e53c4c6f0a63281e31505349d87c154f777ea`

Documentation commit: `1cebfcad76060e0d4ca68ab580272726abd985c2`

Locally confirmed by the maintainer:

```text
Focused contract: 1/1 GREEN
WhenItFails.Tests: 1106/1106 GREEN
```

Nested semantic-directory category-catalog filenames such as `Nested/.` are now rejected before workspace creation or template-provider invocation.

## 2026-09-22 — nested current-directory category-catalog filename contract

Contract commit:
`dc2802122b0053dc575240c2d0b165ec392f5b55`

Baseline: **1105/1105 GREEN**, confirmed locally by the maintainer before this contract was introduced.

Added:
`WhenItFails.Tests/Bootstrap/JsonsBootstrapperNestedCurrentDirectoryCategoryCatalogFileNameContractTests.cs`

Contract:
`EnsureWorkspaceAsync_WhenCategoryCatalogFileNameEndsWithCurrentDirectorySegment_ReturnsInvalidBeforeProviderOrFilesystem`

Caller configuration sets:

```csharp
CategoryCatalogFileName = Path.Combine("Nested", ".")
```

The value is lexically contained inside the package but semantically resolves to the `Nested` directory itself rather than a file beneath it. Because `Nested` does not yet exist, the existing-directory guard cannot identify the problem.

Required response:

```text
Status: Invalid
Data: null
Code: WIF_JSONS_CATEGORY_CATALOG_FILE_NAME_INVALID
Message: The category catalog file name is invalid.
```

The template provider must not be invoked and the root/package workspace must remain uncreated.

The focused contract confirmed the expected RED on Windows:

```text
Expected: Invalid
Actual:   Success
```

The nested semantic-directory category filename passed caller validation and reached successful bootstrap completion.

Production fix commit:
`513e53c4c6f0a63281e31505349d87c154f777ea`

Documentation commit:
`1cebfcad76060e0d4ca68ab580272726abd985c2`

After normalization and before containment/provider invocation, `CategoryCatalogFileName` now rejects a final current-directory segment such as `Nested/.`:

```text
Status: Invalid
Code: WIF_JSONS_CATEGORY_CATALOG_FILE_NAME_INVALID
Message: The category catalog file name is invalid.
```

Valid nested category filenames, package-directory equality classification, outside-package handling, existing-directory detection, and file-parent guards remain unchanged.

**Focused GREEN and complete-suite 1106/1106 GREEN are pending local verification.**

## 2026-09-22 — 1105/1105 GREEN nested error-catalog current-directory checkpoint

Contract commit: `022a094b8c69b4cdadcb4650e0a11819eeca250e`

Production fix commit: `e33f61441d5f16c6cccd2b95a67f01fb304e5eb6`

Documentation commit: `3bd6bce684d4e6d962c820b02681c61d92fbfd1b`

Locally confirmed by the maintainer:

```text
Focused contract: 1/1 GREEN
WhenItFails.Tests: 1105/1105 GREEN
```

Nested semantic-directory error-catalog filenames such as `Nested/.` are now rejected before workspace creation or template-provider invocation.

## 2026-09-22 — nested current-directory error-catalog filename contract

Contract commit:
`022a094b8c69b4cdadcb4650e0a11819eeca250e`

Baseline: **1104/1104 GREEN**, confirmed locally by the maintainer before this contract was introduced.

Added:
`WhenItFails.Tests/Bootstrap/JsonsBootstrapperNestedCurrentDirectoryErrorCatalogFileNameContractTests.cs`

Contract:
`EnsureWorkspaceAsync_WhenErrorCatalogFileNameEndsWithCurrentDirectorySegment_ReturnsInvalidBeforeProviderOrFilesystem`

Caller configuration sets:

```csharp
ErrorCatalogFileName = Path.Combine("Nested", ".")
```

The value is lexically contained inside the package. Because `Nested` does not yet exist, `Directory.Exists(...)` cannot identify the semantic-directory target. Canonical resolution, however, identifies the configured target as the `Nested` directory itself rather than a file beneath it.

Required response:

```text
Status: Invalid
Data: null
Code: WIF_JSONS_ERROR_CATALOG_FILE_NAME_INVALID
Message: The error catalog file name is invalid.
```

The template provider must not be invoked and the root/package workspace must remain uncreated.

The focused contract confirmed the expected RED on Windows:

```text
Expected: Invalid
Actual:   Success
```

The nested semantic-directory filename passed caller validation, the workspace was created, and the template provider was invoked. This confirmed that the missing guard was in caller configuration validation rather than filesystem failure normalization.

Production fix commit:
`e33f61441d5f16c6cccd2b95a67f01fb304e5eb6`

Documentation commit:
`3bd6bce684d4e6d962c820b02681c61d92fbfd1b`

After normalization and before containment/provider invocation, `ErrorCatalogFileName` now rejects a final current-directory segment such as `Nested/.`:

```text
Status: Invalid
Code: WIF_JSONS_ERROR_CATALOG_FILE_NAME_INVALID
Message: The error catalog file name is invalid.
```

Valid nested filenames, package-directory equality classification, outside-package handling, existing-directory detection, and file-parent guards remain unchanged.

**Focused GREEN and complete-suite 1105/1105 GREEN are pending local verification.**

## 2026-09-22 — 1104/1104 GREEN nested current-directory template-target checkpoint

Contract commit: `2cbcb2772ee61af450ba4b6d995bb242692a05ca`

Production fix commit: `4e885a5334b78503e77bbe1dce910bc628a9964a`

Documentation commit: `0f3ac729e99f26080e609a6f3f82071e539563ea`

Locally confirmed by the maintainer:

```text
Focused contract: 1/1 GREEN
WhenItFails.Tests: 1104/1104 GREEN
```

Nested semantic-directory provider targets such as `Nested/.` are now rejected during full-snapshot validation before any template write begins.

## 2026-09-22 — nested current-directory template-target contract

Contract commit:
`2cbcb2772ee61af450ba4b6d995bb242692a05ca`

Baseline: **1103/1103 GREEN**, confirmed locally by the maintainer before this contract was introduced.

Added:
`WhenItFails.Tests/Bootstrap/JsonsBootstrapperNestedCurrentDirectoryTemplateTargetContractTests.cs`

Contract:
`EnsureWorkspaceAsync_WhenLaterTemplateTargetEndsWithCurrentDirectorySegment_ReturnsInvalidWithoutPartialWrites`

The provider returns two templates. The first target is a valid `first.json`. The second target is:

```csharp
TargetFileName = Path.Combine("Nested", ".")
```

The second path is lexically contained inside the package and `Nested` does not yet exist. Canonical resolution, however, identifies the target as the `Nested` directory itself rather than a file beneath it.

Required response:

```text
Status: Invalid
Data: null
Code: WIF_JSONS_TEMPLATE_TARGET_FILE_NAME_INVALID
Message: The JSON template provider returned a template with an invalid target file name.
```

The complete provider snapshot must be rejected before writes begin: `first.json` must remain absent and the `Nested` directory must not be created.

The focused contract confirmed the expected RED on Windows:

```text
Expected: Invalid
Actual:   Failed
```

The nested semantic-directory target passed full-snapshot validation and reached filesystem creation, where it was normalized as a generic failure. This also meant the invalid target was being detected too late to uphold the intended no-partial-write validation boundary.

Production fix commit:
`4e885a5334b78503e77bbe1dce910bc628a9964a`

Documentation commit:
`0f3ac729e99f26080e609a6f3f82071e539563ea`

During provider snapshot validation, bootstrap now rejects a normalized target whose final path segment is `.` before containment/write processing:

```text
Status: Invalid
Code: WIF_JSONS_TEMPLATE_TARGET_FILE_NAME_INVALID
Message: The JSON template provider returned a template with an invalid target file name.
```

Valid nested file targets remain supported. The later invalid target is rejected before any template write begins, preserving the no-partial-write guarantee.

**Focused GREEN and complete-suite 1104/1104 GREEN are pending local verification.**

## 2026-09-22 — 1103/1103 GREEN package root-target checkpoint

Contract commit: `1a6368ee9b3580806e76dbbd4d0fa298015cf9b4`

Production fix commit: `ff45147b74495140f1a1aa0ad7d8061ff3ab427b`

Documentation commit: `f3c667fb07f57ac2d0ef43eab6618b0adad69854`

Locally confirmed by the maintainer:

```text
Focused contract: 1/1 GREEN
WhenItFails.Tests: 1103/1103 GREEN
```

Package-directory equality with `RootDirectory` now has stable invalid-name classification while true outside-root paths retain the escape-specific contract.

## 2026-09-22 — package-directory current-directory semantic contract

Contract commit:
`1a6368ee9b3580806e76dbbd4d0fa298015cf9b4`

Baseline: **1102/1102 GREEN**, confirmed locally by the maintainer before this contract was introduced.

Added:
`WhenItFails.Tests/Bootstrap/JsonsBootstrapperCurrentDirectoryPackageDirectoryNameContractTests.cs`

Contract:
`EnsureWorkspaceAsync_WhenPackageDirectoryNameResolvesToRoot_ReturnsInvalidBeforeProviderOrFilesystem`

Caller configuration sets:

```csharp
PackageDirectoryName = "."
```

After normalization and canonical path resolution, the package directory resolves exactly to `RootDirectory`. This is not a path escape outside the configured root; it is semantically invalid package-directory configuration because the package directory name does not identify a child directory.

Required response:

```text
Status: Invalid
Data: null
Code: WIF_JSONS_PACKAGE_DIRECTORY_NAME_INVALID
Message: The package directory name is invalid.
```

The template provider must not be invoked and the root directory must remain uncreated.

The focused contract confirmed the expected RED on Windows:

```text
Expected: WIF_JSONS_PACKAGE_DIRECTORY_NAME_INVALID
Actual:   WIF_JSONS_PACKAGE_DIRECTORY_NAME_OUTSIDE_ROOT
```

The response status was already `Invalid`; only the issue classification was wrong. The shared containment helper correctly rejects equality because it requires a child path, but equality with `RootDirectory` is semantically different from a true escape.

Production fix commit:
`ff45147b74495140f1a1aa0ad7d8061ff3ab427b`

Documentation commit:
`f3c667fb07f57ac2d0ef43eab6618b0adad69854`

When package containment returns false, bootstrap now compares canonical root and package paths. Exact equality returns:

```text
Status: Invalid
Code: WIF_JSONS_PACKAGE_DIRECTORY_NAME_INVALID
Message: The package directory name is invalid.
```

Only a non-equal path outside the root returns `WIF_JSONS_PACKAGE_DIRECTORY_NAME_OUTSIDE_ROOT`. The shared strict containment helper, true escape behavior, nested package paths, and filesystem guards remain unchanged.

**Focused GREEN and complete-suite 1103/1103 GREEN are pending local verification.**

## 2026-09-22 — 1102/1102 GREEN package file-parent checkpoint

Contract commit: `6d7e970bb612328d8d91cc27650bfead2a0a6c45`

Production fix commit: `3634b11b63a69446d118ace6c68e213321c99526`

Documentation commit: `09897c076ed538a95e51e102ab52bce5c29b8df5`

Locally confirmed by the maintainer:

```text
Focused contract: 1/1 GREEN
WhenItFails.Tests: 1102/1102 GREEN
```

Nested package-directory paths now reject regular-file ancestors inside the configured root before workspace creation or template-provider invocation.

## 2026-09-22 — 1101/1101 GREEN root file-parent checkpoint

Contract commit: `53206e6f2745897df1160bc1db38beeef7d8f773`

Production fix commit: `aa7a7d666eb15b2a81d16a81116286e09db75466`

Documentation commit: `579d5fa922488f6069958658ad494ec9f8cc7c75`

Locally confirmed by the maintainer:

```text
Focused contract: 1/1 GREEN
WhenItFails.Tests: 1101/1101 GREEN
```

The configured root and its existing parent chain now reject regular-file collisions before package containment, workspace creation, or template-provider invocation.

## 2026-09-22 — package-directory existing-file-parent contract

Contract commit:
`6d7e970bb612328d8d91cc27650bfead2a0a6c45`

Baseline: **1101/1101 GREEN**, confirmed locally by the maintainer before this contract was introduced.

Added:
`WhenItFails.Tests/Bootstrap/JsonsBootstrapperFileParentPackageDirectoryContractTests.cs`

Contract:
`EnsureWorkspaceAsync_WhenPackageDirectoryParentIsExistingFile_ReturnsInvalidBeforeProvider`

The configured root already exists and contains a regular file `Packages` with contents `Keep me.`. Caller configuration sets:

```csharp
PackageDirectoryName = Path.Combine("Packages", "WhenItFails")
```

The nested package path is syntactically valid and remains inside the root, but its parent `Packages` is a regular file and cannot contain the requested package directory.

Required response:

```text
Status: Invalid
Data: null
Code: WIF_JSONS_PACKAGE_DIRECTORY_NAME_INVALID
Message: The package directory name is invalid.
```

The template provider must not be invoked. The existing parent file must remain unchanged and no package directory may be created.

The focused contract confirmed the expected RED on Windows:

```text
Expected: Invalid
Actual:   Failed
```

The exact package-directory path did not exist, but its parent `Packages` was a regular file. Package containment therefore succeeded and the collision reached directory creation, where the outer I/O catch normalized it as a generic failure.

Production fix commit:
`3634b11b63a69446d118ace6c68e213321c99526`

Documentation commit:
`09897c076ed538a95e51e102ab52bce5c29b8df5`

After package containment and the exact package-file guard, bootstrap now walks the canonical package parent chain up to (but excluding) the configured root. Any existing regular-file ancestor returns:

```text
Status: Invalid
Code: WIF_JSONS_PACKAGE_DIRECTORY_NAME_INVALID
Message: The package directory name is invalid.
```

The validation runs before workspace creation or template-provider invocation. Valid nested package paths, valid existing package directories, exact package-file handling, and root-path classification remain unchanged.

**Focused GREEN and complete-suite 1102/1102 GREEN are pending local verification.**

## 2026-09-22 — root-directory existing-file-parent contract

Contract commit:
`53206e6f2745897df1160bc1db38beeef7d8f773`

Baseline: **1100/1100 GREEN**, confirmed locally by the maintainer before this contract was introduced.

Added:
`WhenItFails.Tests/Bootstrap/JsonsBootstrapperFileParentRootDirectoryContractTests.cs`

Contract:
`EnsureWorkspaceAsync_WhenRootDirectoryParentIsExistingFile_ReturnsInvalidBeforeProvider`

A unique temporary test directory contains an existing regular file `Blocked` with contents `Keep me.`. Caller configuration sets:

```csharp
RootDirectory = Path.Combine(existingParentFilePath, "Jsons"),
PackageDirectoryName = "WhenItFails"
```

The configured root is syntactically valid and does not itself exist as a file or directory. Its parent `Blocked`, however, is a regular file and therefore cannot contain the requested root directory.

Required response:

```text
Status: Invalid
Data: null
Code: WIF_JSONS_ROOT_DIRECTORY_INVALID
Message: The JSON root directory path is invalid.
```

The template provider must not be invoked. The existing parent file must remain a regular file with unchanged contents, and neither the root directory nor package directory may be created.

The focused contract confirmed the expected RED on Windows:

```text
Expected: Invalid
Actual:   Failed
```

The configured root itself did not exist, but its parent `Blocked` was a regular file. Root syntax and lexical containment therefore succeeded, and the collision reached directory creation where the outer I/O catch normalized it as a generic failure.

Production fix commit:
`aa7a7d666eb15b2a81d16a81116286e09db75466`

Documentation commit:
`579d5fa922488f6069958658ad494ec9f8cc7c75`

After root syntax validation and the exact-root-file guard, bootstrap now walks the canonical parent chain of `RootDirectory`. Any existing regular-file ancestor returns:

```text
Status: Invalid
Code: WIF_JSONS_ROOT_DIRECTORY_INVALID
Message: The JSON root directory path is invalid.
```

The validation runs before package containment, workspace creation, or template-provider invocation. Valid missing-root paths and valid existing directories remain unchanged, and the original parent file is preserved.

**Focused GREEN and complete-suite 1101/1101 GREEN are pending local verification.**

## 2026-09-22 — 1100/1100 GREEN existing-file root-directory checkpoint

Contract commit: `e691a6405e116dc5eca951d915041866c00f5376`

Production fix commit: `40ec16aed6b63e9580dd32f593a98e454770a38d`

Documentation commit: `595cedd3195531d6134dd86870ce4befc5c578cc`

Locally confirmed by the maintainer:

```text
Focused contract: 1/1 GREEN
WhenItFails.Tests: 1100/1100 GREEN
```

The configured root path now distinguishes an existing regular file from a valid or missing directory before package containment, filesystem mutation, or template-provider invocation.

## 2026-09-22 — existing-file root-directory-path contract

Contract commit:
`e691a6405e116dc5eca951d915041866c00f5376`

Baseline: **1099/1099 GREEN**, confirmed locally by the maintainer before this contract was introduced.

Added:
`WhenItFails.Tests/Bootstrap/JsonsBootstrapperExistingFileRootDirectoryContractTests.cs`

Contract:
`EnsureWorkspaceAsync_WhenRootDirectoryPathIsExistingFile_ReturnsInvalidBeforeProvider`

A unique temporary test directory contains a regular file named `Jsons` with contents `Keep me.`. Caller configuration sets that file path as:

```csharp
RootDirectory = rootDirectory,
PackageDirectoryName = "WhenItFails"
```

The configured root path is syntactically valid, but it cannot represent the JSON workspace root because it is already occupied by a regular file.

Required response:

```text
Status: Invalid
Data: null
Code: WIF_JSONS_ROOT_DIRECTORY_INVALID
Message: The JSON root directory path is invalid.
```

The tracking template provider must not be invoked. The existing root file must remain a file with unchanged contents, and no package directory may be created beneath it.

The focused contract confirmed the expected RED on Windows:

```text
Expected: Invalid
Actual:   Failed
```

The configured root path was syntactically valid but occupied by a regular file. Lexical containment therefore succeeded and the collision reached directory creation, where the outer I/O catch normalized it as a generic failure.

Production fix commit:
`40ec16aed6b63e9580dd32f593a98e454770a38d`

Documentation commit:
`595cedd3195531d6134dd86870ce4befc5c578cc`

After root syntax validation, bootstrap now rejects an existing regular file at `RootDirectory` before package containment, workspace creation, or template-provider invocation:

```text
Status: Invalid
Code: WIF_JSONS_ROOT_DIRECTORY_INVALID
Message: The JSON root directory path is invalid.
```

Missing root directories remain valid for later workspace creation; valid existing directories remain unchanged; the original colliding file is preserved.

**Focused GREEN and complete-suite 1100/1100 GREEN are pending local verification.**

## 2026-09-22 — 1099/1099 GREEN existing-file package-directory checkpoint

Contract commit: `1471d0a940390a06070a69b99b52c9e879739ac8`

Production fix commit: `3b75438d332ad03e7aa6bface3e0fd6554d94c2c`

Documentation commit: `49b935b6cfc483909c19f7d36ae23a4cbc254374`

Locally confirmed by the maintainer:

```text
Focused contract: 1/1 GREEN
WhenItFails.Tests: 1099/1099 GREEN
```

The resolved package-directory path now distinguishes an existing regular file from an existing directory before filesystem mutation or template-provider invocation.

## 2026-09-22 — existing-file package-directory-path contract

Contract commit:
`1471d0a940390a06070a69b99b52c9e879739ac8`

Baseline: **1098/1098 GREEN**, confirmed locally by the maintainer before this contract was introduced.

Added:
`WhenItFails.Tests/Bootstrap/JsonsBootstrapperExistingFilePackageDirectoryContractTests.cs`

Contract:
`EnsureWorkspaceAsync_WhenPackageDirectoryPathIsExistingFile_ReturnsInvalidBeforeProvider`

The JSON root directory already exists and contains a regular file named `WhenItFails` with contents `Keep me.`. Caller configuration sets:

```csharp
RootDirectory = rootDirectory,
PackageDirectoryName = "WhenItFails"
```

The resolved package-directory path is lexically valid and remains inside the configured root, but it cannot represent a directory because that path is already occupied by a regular file.

Required response:

```text
Status: Invalid
Data: null
Code: WIF_JSONS_PACKAGE_DIRECTORY_NAME_INVALID
Message: The package directory name is invalid.
```

The tracking template provider must not be invoked. The existing file must remain a file, retain its original contents, and must not be replaced or mutated.

The focused contract confirmed the expected RED on Windows:

```text
Expected: Invalid
Actual:   Failed
```

The resolved package-directory path was occupied by a regular file. Because `Directory.Exists(packageDirectoryPath)` is false for a file, the previous implementation reached `Directory.CreateDirectory(packageDirectoryPath)` and the outer I/O catch normalized the collision as a generic failure.

Production fix commit:
`3b75438d332ad03e7aa6bface3e0fd6554d94c2c`

Documentation commit:
`49b935b6cfc483909c19f7d36ae23a4cbc254374`

After package containment succeeds and `packageDirectoryPath` is resolved, bootstrap now rejects an existing regular file at that exact path before any caller filename validation, workspace creation, or template-provider invocation:

```text
Status: Invalid
Code: WIF_JSONS_PACKAGE_DIRECTORY_NAME_INVALID
Message: The package directory name is invalid.
```

Existing valid directories still remain untouched, missing directories can still be created, and the original colliding file is preserved.

**Focused GREEN and complete-suite 1099/1099 GREEN are pending local verification.**

## 2026-09-22 — 1098/1098 GREEN caller profiles file-parent checkpoint

Contract commit: `3185dc5b222d8c71e6352df5984660ba86af788e`

Production fix commit: `7f9f6f4e2d424c49274460832460386890039e6f`

Documentation commit: `fe34b6c5e6bd3070eceebd098a3a96cb41e1f0ef`

Locally confirmed by the maintainer:

```text
Focused contract: 1/1 GREEN
WhenItFails.Tests: 1098/1098 GREEN
```

The existing-file ancestor boundary is now covered consistently for `ErrorCatalogFileName`, `CategoryCatalogFileName`, `CodeGroupCatalogFileName`, `OwnerCatalogFileName`, and `ProfilesFileName`.

## 2026-09-22 — caller profiles existing-file parent contract

Contract commit:
`3185dc5b222d8c71e6352df5984660ba86af788e`

Baseline: **1097/1097 GREEN**, confirmed locally by the maintainer before this contract was introduced.

Added:
`WhenItFails.Tests/Bootstrap/JsonsBootstrapperFileParentProfilesFileNameContractTests.cs`

Contract:
`EnsureWorkspaceAsync_WhenProfilesFileNameParentIsExistingFile_ReturnsInvalidBeforeProvider`

The package workspace contains an existing regular file `Nested` with contents `Keep me.`. Caller configuration sets:

```csharp
ProfilesFileName = Path.Combine("Nested", "profiles.json")
```

The target remains lexically inside the package and does not itself resolve to an existing directory. Its parent `Nested`, however, is a regular file and cannot serve as a directory.

Required response:

```text
Status: Invalid
Data: null
Code: WIF_JSONS_PROFILE_CATALOG_FILE_NAME_INVALID
Message: The profile catalog file name is invalid.
```

The tracking template provider must not be invoked; the nested target must remain absent; and `Nested` must remain an unchanged regular file.

The focused contract confirmed the expected RED on Windows:

```text
Expected: Invalid
Actual:   Success
```

The profile filename passed lexical containment despite its parent `Nested` being an existing regular file. An empty tracking provider allowed bootstrap to return success.

Production fix commit:
`7f9f6f4e2d424c49274460832460386890039e6f`

Documentation commit:
`fe34b6c5e6bd3070eceebd098a3a96cb41e1f0ef`

After containment and existing-directory validation of `ProfilesFileName`, the bootstrapper now checks canonical parent paths inside the package up to (but excluding) the package directory. An existing regular-file ancestor returns:

```text
Status: Invalid
Code: WIF_JSONS_PROFILE_CATALOG_FILE_NAME_INVALID
Message: The profile catalog file name is invalid.
```

The validation runs before provider invocation and leaves the original file untouched. Valid nested filenames, true outside-package paths, provider-target validation, and the established profile error classifications remain unchanged.

**Focused GREEN and complete-suite 1098/1098 GREEN are pending local verification.**

## 2026-09-22 — 1097/1097 GREEN caller owner catalog file-parent checkpoint

Contract commit: `20f3f4f343a84386db0c73e99229031cfffae78e`

Production fix commit: `2e5e498fabf67f1150b985d7a1507aa68747ba8a`

Documentation commit: `f9500de7c202fc036838bacebf4d7cda5bb6a5f0`

Locally confirmed by the maintainer:

```text
WhenItFails.Tests
Failed:   0
Passed: 1097
Total:  1097
```

Caller `OwnerCatalogFileName` paths with existing regular-file ancestors inside the package now return the owner-specific invalid response before template-provider invocation, preserving the ancestor file.

Final caller-configured field for this boundary: `ProfilesFileName`.

## 2026-09-22 — caller owner catalog existing-file parent contract

Contract commit:
`20f3f4f343a84386db0c73e99229031cfffae78e`

Baseline: **1096/1096 GREEN**, confirmed locally by the maintainer before the new contract.

Added:
`WhenItFails.Tests/Bootstrap/JsonsBootstrapperFileParentOwnerCatalogFileNameContractTests.cs`

Contract:
`EnsureWorkspaceAsync_WhenOwnerCatalogFileNameParentIsExistingFile_ReturnsInvalidBeforeProvider`

The package workspace contains an existing regular file `Nested` with contents `Keep me.`. Caller configuration sets:

```csharp
OwnerCatalogFileName = Path.Combine("Nested", "owners.json")
```

The target is inside the package and does not itself resolve to an existing directory. However, its parent `Nested` is a regular file, not a usable directory.

Require:

```text
Status: Invalid
Data: null
Code: WIF_JSONS_OWNER_CATALOG_FILE_NAME_INVALID
Message: The owner catalog file name is invalid.
```

The tracking template provider must not be called; the nested target must remain absent; and `Nested` must remain an unchanged regular file.

Current production checks the owner filename for containment and a terminal existing-directory target, but does not yet validate existing-file ancestors. An empty tracking provider can therefore allow bootstrap to return `Success`.

The focused contract confirmed the expected RED on Windows:

```text
Expected: Invalid
Actual:   Success
```

The owner catalog filename passed lexical containment despite its parent `Nested` being an existing regular file. An empty tracking provider allowed bootstrap to return success.

Production fix commit:
`2e5e498fabf67f1150b985d7a1507aa68747ba8a`

Documentation commit:
`f9500de7c202fc036838bacebf4d7cda5bb6a5f0`

After containment and existing-directory validation of `OwnerCatalogFileName`, the bootstrapper now checks canonical parent paths inside the package up to (but excluding) the package directory. An existing regular-file ancestor returns:

```text
Status: Invalid
Code: WIF_JSONS_OWNER_CATALOG_FILE_NAME_INVALID
Message: The owner catalog file name is invalid.
```

The validation runs before provider invocation and leaves the original file untouched. The other caller fields, actual outside-package paths, and the shared containment helper remain unchanged.

**Complete-suite 1097/1097 GREEN was subsequently confirmed locally by the maintainer.** The latest confirmation did not separately report the focused result, compiler-warning count, or operating system.

## 2026-09-22 — 1096/1096 GREEN caller code group file-parent checkpoint

Contract commit: `bd8ca8b8683a4a2d28a5a3e6036dbad416b484f3`

Production fix commit: `680d6140cb7fab2c238f2e7ae61f49c50bd25045`

Documentation commit: `c4e13a06b4192de531ab72f5f9312558d7be7916`

Locally confirmed by the maintainer:

```text
WhenItFails.Tests
Failed:   0
Passed: 1096
Total:  1096
```

Caller `CodeGroupCatalogFileName` paths with existing regular-file ancestors inside the package now return the code-group-specific invalid response before template-provider invocation, leaving the ancestor file untouched.

Next caller-configured field: `OwnerCatalogFileName`.

## 2026-09-22 — caller code group catalog existing-file parent contract

Contract commit:
`bd8ca8b8683a4a2d28a5a3e6036dbad416b484f3`

Baseline: **1095/1095 GREEN**, confirmed locally by the maintainer before this contract was introduced.

Added:
`WhenItFails.Tests/Bootstrap/JsonsBootstrapperFileParentCodeGroupCatalogFileNameContractTests.cs`

Contract:
`EnsureWorkspaceAsync_WhenCodeGroupCatalogFileNameParentIsExistingFile_ReturnsInvalidBeforeProvider`

The package workspace already contains a regular file `Nested` with contents `Keep me.`. Caller configuration uses:

```csharp
CodeGroupCatalogFileName = Path.Combine("Nested", "code-groups.json")
```

The target is lexically inside the package, but its parent `Nested` is a regular file rather than a directory. The target itself does not resolve to an existing directory.

Required response:

```text
Status: Invalid
Data: null
Code: WIF_JSONS_CODE_GROUP_CATALOG_FILE_NAME_INVALID
Message: The code group catalog file name is invalid.
```

The tracking template provider must not be invoked. The nested target must remain absent, and `Nested` must remain a regular file with unchanged contents.

Current production checks code-group filename containment and whether its terminal target is an existing directory, but does not yet validate existing regular-file ancestors. With an empty tracking provider, bootstrap can return `Success`.

The focused contract confirmed the expected RED on Windows:

```text
Expected: Invalid
Actual:   Success
```

The code-group catalog filename passed lexical containment despite its `Nested` parent being an existing regular file; the tracking provider returned no templates and bootstrap reported success.

Production fix commit:
`680d6140cb7fab2c238f2e7ae61f49c50bd25045`

Documentation commit:
`c4e13a06b4192de531ab72f5f9312558d7be7916`

After containment and existing-directory validation of `CodeGroupCatalogFileName`, `JsonsBootstrapper` now checks canonical parent paths inside the package up to (but excluding) the package directory. Any existing regular-file ancestor returns:

```text
Status: Invalid
Code: WIF_JSONS_CODE_GROUP_CATALOG_FILE_NAME_INVALID
Message: The code group catalog file name is invalid.
```

The check runs before workspace creation or template-provider invocation and does not modify the existing parent file. Actual escape paths retain the outside-package response; other caller fields are unchanged.

**Complete-suite 1096/1096 GREEN was subsequently confirmed locally by the maintainer.** The latest confirmation did not separately report the focused result, compiler-warning count, or operating system.

## 2026-09-22 — 1095/1095 GREEN caller category catalog file-parent checkpoint

Contract commit: `389e5487c03a9e498b8f0bd106358167d55c1b08`

Production fix commit: `f83293b1acebc1dc613995cdd5545c1b81b3e0b2`

Documentation commit: `3feac61a6a071bc6e0a0e4c1b6ea34f1200efe7a`

Locally confirmed by the maintainer:

```text
WhenItFails.Tests
Failed:   0
Passed: 1095
Total:  1095
```

Caller `CategoryCatalogFileName` paths with existing regular-file ancestors inside the package now return the category-specific invalid response before template-provider invocation. The original ancestor file remains untouched.

Next caller-configured field: `CodeGroupCatalogFileName`.

## 2026-09-22 — caller category catalog existing-file parent contract

Contract commit:
`389e5487c03a9e498b8f0bd106358167d55c1b08`

Baseline: **1094/1094 GREEN**, confirmed locally by the maintainer before the new contract.

Added:
`WhenItFails.Tests/Bootstrap/JsonsBootstrapperFileParentCategoryCatalogFileNameContractTests.cs`

Contract:
`EnsureWorkspaceAsync_WhenCategoryCatalogFileNameParentIsExistingFile_ReturnsInvalidBeforeProvider`

The package workspace contains an existing regular file `Nested` with contents `Keep me.`. Caller configuration sets:

```csharp
CategoryCatalogFileName = Path.Combine("Nested", "categories.json")
```

The target is lexically inside the package and does not itself resolve to an existing directory. Its parent `Nested`, however, is a regular file and cannot be used as a directory.

Required response:

```text
Status: Invalid
Data: null
Code: WIF_JSONS_CATEGORY_CATALOG_FILE_NAME_INVALID
Message: The category catalog file name is invalid.
```

The tracking template provider must not be called, the nested target must remain absent, and `Nested` must remain an unchanged regular file.

Current production checks the category filename for containment and an existing-directory terminal target, but does not validate existing-file ancestors. A tracking provider returning no templates can therefore allow bootstrap to return `Success`.

The focused contract confirmed the expected RED on Windows:

```text
Expected: Invalid
Actual:   Success
```

The category catalog filename passed lexical containment despite its `Nested` parent being an existing regular file; the tracking provider returned no templates and bootstrap reported success.

Production fix commit:
`f83293b1acebc1dc613995cdd5545c1b81b3e0b2`

Documentation commit:
`3feac61a6a071bc6e0a0e4c1b6ea34f1200efe7a`

After containment and existing-directory validation of `CategoryCatalogFileName`, `JsonsBootstrapper` now checks canonical parent paths inside the package up to (but excluding) the package directory. Any existing regular-file ancestor returns:

```text
Status: Invalid
Code: WIF_JSONS_CATEGORY_CATALOG_FILE_NAME_INVALID
Message: The category catalog file name is invalid.
```

The check runs before workspace creation or template-provider invocation and does not modify the existing parent file. Actual escape paths retain the outside-package response; other caller fields are unchanged.

**Complete-suite 1095/1095 GREEN was subsequently confirmed locally by the maintainer.** The latest confirmation did not separately report the focused result, compiler-warning count, or operating system.

## 2026-09-22 — 1094/1094 GREEN caller error catalog file-parent checkpoint

Contract commit: `764002ee53b8acf22e1426a17183fc0581c88a63`

Production fix commit: `55f2f2cc3dd7ea4216fd13fd309e9b1fc80893ac`

Documentation commit: `48d8e237f2a327d4db132b25ada833031d189cd5`

Locally confirmed by the maintainer:

```text
WhenItFails.Tests
Failed:   0
Passed: 1094
Total:  1094
```

Caller `ErrorCatalogFileName` paths with existing regular-file ancestors inside the package are rejected as invalid configuration before template-provider invocation. The existing ancestor file remains untouched. The analogous provider-target contract is already GREEN.

Next separate field: `CategoryCatalogFileName`.

## 2026-09-22 — caller error catalog existing-file parent contract

Contract commit:
`764002ee53b8acf22e1426a17183fc0581c88a63`

Baseline: **1093/1093 GREEN**, confirmed locally by the maintainer before the new contract.

Added:
`WhenItFails.Tests/Bootstrap/JsonsBootstrapperFileParentErrorCatalogFileNameContractTests.cs`

Contract:
`EnsureWorkspaceAsync_WhenErrorCatalogFileNameParentIsExistingFile_ReturnsInvalidBeforeProvider`

The package workspace contains a regular file `Nested` with contents `Keep me.`. Caller configuration sets:

```csharp
ErrorCatalogFileName = Path.Combine("Nested", "errors.json")
```

The target is lexically inside the package, and its terminal target is not an existing directory. However, its parent `Nested` is a regular file rather than a usable directory.

Required response:

```text
Status: Invalid
Data: null
Code: WIF_JSONS_ERROR_CATALOG_FILE_NAME_INVALID
Message: The error catalog file name is invalid.
```

The tracking template provider must not be called, the target must remain absent, and `Nested` must remain an unchanged regular file.

Current production validates caller error-catalog containment and the target's existing-directory state, but not whether its parent paths are existing files. A tracking provider returning no templates can therefore allow bootstrap to report `Success`.

The focused contract confirmed the expected RED on Windows:

```text
Expected: Invalid
Actual:   Success
```

The caller's nested error catalog filename passed lexical containment, and the tracking provider could return success despite its parent being an existing regular file.

Production fix commit:
`55f2f2cc3dd7ea4216fd13fd309e9b1fc80893ac`

Documentation commit:
`48d8e237f2a327d4db132b25ada833031d189cd5`

After containment and existing-directory validation of `ErrorCatalogFileName`, `JsonsBootstrapper` now checks each canonical parent path inside the package up to (but excluding) the package directory. Any existing regular-file ancestor returns:

```text
Status: Invalid
Code: WIF_JSONS_ERROR_CATALOG_FILE_NAME_INVALID
Message: The error catalog file name is invalid.
```

This guard executes before workspace creation and template-provider invocation; it leaves the original file untouched. Actual escape paths keep the outside-package response, and the other caller filename fields are unchanged.

**Complete-suite 1094/1094 GREEN was subsequently confirmed locally by the maintainer.** The latest confirmation did not separately report the focused result, compiler-warning count, or operating system.

## 2026-09-22 — 1093/1093 GREEN provider file-parent checkpoint

Contract commit: `a6dd14f6a497bda3c1cf8d4308b4fcd9c0d8e4ca`

Production fix commit: `8578aa7ef35bbd12d6f5dadf9358dc7c18261606`

Documentation commit: `1d0d339e87b86efa7492d0913cd14468a38c27d0`

Locally confirmed by the maintainer:

```text
WhenItFails.Tests
Failed:   0
Passed: 1093
Total:  1093
```

Provider targets with existing regular-file parent paths inside the package are now rejected during full-snapshot validation before template writes. Existing files are preserved, and the separate valid nested-target creation contract remains covered.

Next distinct boundary: a caller-configured nested catalog filename with an existing regular file at one of its parent paths should be rejected before template-provider invocation.

## 2026-09-22 — provider target with existing file parent contract

Contract commit:
`a6dd14f6a497bda3c1cf8d4308b4fcd9c0d8e4ca`

Baseline: **1092/1092 GREEN**, confirmed locally by the maintainer before this contract was introduced.

Added:
`WhenItFails.Tests/Bootstrap/JsonsBootstrapperFileParentTemplateTargetContractTests.cs`

Contract:
`EnsureWorkspaceAsync_WhenLaterTemplateTargetParentIsExistingFile_ReturnsInvalidWithoutPartialWrites`

The existing package workspace contains a regular file `Nested` with the contents `Keep me.`. The provider returns two templates: a valid `first.json` followed by a nested target `Nested/child.json`. The second target remains lexically inside the package and does not itself exist as a directory, but its parent cannot be used as a directory.

Required response:

```text
Status: Invalid
Data: null
Code: WIF_JSONS_TEMPLATE_TARGET_FILE_NAME_INVALID
Message: The JSON template provider returned a template with an invalid target file name.
```

The full provider snapshot must be validated before any template write: `first.json` and `Nested/child.json` must not exist afterward, and `Nested` must remain a regular file with unchanged contents.

Current production checks whether a provider target is itself an existing directory, but does not reject parent paths that are existing regular files. The later target is expected to fail during filesystem writes after the first template has been created.

The focused contract confirmed the expected RED on Windows:

```text
Expected: Invalid
Actual:   Failed
```

The first template was eligible for writing, but the later nested target used `Nested` as a directory even though that path was an existing regular file. It was only rejected during filesystem I/O.

Production fix commit:
`8578aa7ef35bbd12d6f5dadf9358dc7c18261606`

Documentation commit:
`1d0d339e87b86efa7492d0913cd14468a38c27d0`

During validation of the complete materialized provider snapshot, `JsonsBootstrapper` now traverses the canonical parent paths for each contained template target up to the package directory. If any parent inside the package is an existing regular file, the bootstrapper returns:

```text
Status: Invalid
Code: WIF_JSONS_TEMPLATE_TARGET_FILE_NAME_INVALID
Message: The JSON template provider returned a template with an invalid target file name.
```

The guard executes before writing template files, protecting earlier template targets and the existing regular-file parent for this deterministic invalid configuration. It does not add a transaction or prevent unrelated concurrent filesystem changes.

**Complete-suite 1093/1093 GREEN was subsequently confirmed locally by the maintainer.** The latest confirmation did not separately report the focused result, compiler-warning count, or operating system.

## 2026-09-22 — 1092/1092 GREEN five-field current-directory checkpoint

Final contract commit: `51f428cfa517c754c539e6b9b33c0b2c0beb2376`

Final production fix commit: `a39fcb881f5b6f0cabbf642a1dc794d067a6e7b3`

Final documentation commit: `4310b612e029250096a3ccdd0c5e2d8117339329`

Locally confirmed by the maintainer:

```text
WhenItFails.Tests
Failed:   0
Passed: 1092
Total:  1092
```

The provider target and all five caller-configured catalog filename fields now classify paths that resolve exactly to the package directory as invalid filename targets, rather than escape paths. True paths outside the package keep their separate outside-package errors; the shared containment helper remains unchanged.

Next distinct boundary: provider target whose parent path already exists as a regular file. A later nested template could otherwise fail only during file creation, after earlier templates have been written.

## 2026-09-22 — caller current-directory profiles filename contract

Contract commit:
`51f428cfa517c754c539e6b9b33c0b2c0beb2376`

Baseline: **1091/1091 GREEN**, confirmed locally by the maintainer before this contract was introduced.

Added:
`WhenItFails.Tests/Bootstrap/JsonsBootstrapperCurrentDirectoryProfilesFileNameContractTests.cs`

Contract:
`EnsureWorkspaceAsync_WhenProfilesFileNameResolvesToPackageDirectory_ReturnsInvalidBeforeProviderOrFilesystem`

Caller configuration:

```csharp
ProfilesFileName = "."
```

The value resolves exactly to the package directory, so it does not identify a catalog file and is not a path escaping the package.

Required response:

```text
Status: Invalid
Data: null
Code: WIF_JSONS_PROFILE_CATALOG_FILE_NAME_INVALID
Message: The profile catalog file name is invalid.
```

The tracking template provider must not be invoked, and the initially nonexistent workspace root must remain absent.

Current profile filename containment reports false when its target resolves to the package directory itself and classifies that input as `WIF_JSONS_PROFILE_CATALOG_FILE_NAME_OUTSIDE_PACKAGE`. The other four caller filename fields have already received their separate classification fixes.

The focused contract confirmed the expected RED on Windows:

```text
Expected: WIF_JSONS_PROFILE_CATALOG_FILE_NAME_INVALID
Actual:   WIF_JSONS_PROFILE_CATALOG_FILE_NAME_OUTSIDE_PACKAGE
```

The response status was already `Invalid`; only the error classification was wrong. `ProfilesFileName = "."` resolves exactly to the package directory, not outside it.

Production fix commit:
`a39fcb881f5b6f0cabbf642a1dc794d067a6e7b3`

Documentation commit:
`4310b612e029250096a3ccdd0c5e2d8117339329`

When profile filename containment fails but its canonical target equals the canonical package directory, the bootstrapper now returns:

```text
Status: Invalid
Code: WIF_JSONS_PROFILE_CATALOG_FILE_NAME_INVALID
Message: The profile catalog file name is invalid.
```

True escapes still return `WIF_JSONS_PROFILE_CATALOG_FILE_NAME_OUTSIDE_PACKAGE`. The shared containment helper and the other caller fields remain unchanged. All five caller-configured filename fields now include this classification in production.

**Complete-suite 1092/1092 GREEN was subsequently confirmed locally by the maintainer.** The latest confirmation did not separately report the focused result, compiler-warning count, or operating system.

## 2026-09-22 — 1091/1091 GREEN caller current-directory owner checkpoint

Contract commit: `96e2ee67c829b5dfb290bffc2c96529bf32d72e1`

Production fix commit: `d24e62b0fd3404a3ea17acaea450643226e9125e`

Documentation commit: `8b2e4b994e47d8627cf544eaa89b1979508a4687`

Locally confirmed by the maintainer:

```text
WhenItFails.Tests
Failed:   0
Passed: 1091
Total:  1091
```

`OwnerCatalogFileName = "."` now returns the owner-specific invalid filename code before provider invocation or workspace mutation. Actual escapes still return `WIF_JSONS_OWNER_CATALOG_FILE_NAME_OUTSIDE_PACKAGE`.

Final caller-configured field for this boundary: `ProfilesFileName`.

## 2026-09-22 — caller current-directory owner catalog filename contract

Contract commit:
`96e2ee67c829b5dfb290bffc2c96529bf32d72e1`

Baseline: **1090/1090 GREEN**, confirmed locally by the maintainer before this contract was introduced.

Added:
`WhenItFails.Tests/Bootstrap/JsonsBootstrapperCurrentDirectoryOwnerCatalogFileNameContractTests.cs`

Contract:
`EnsureWorkspaceAsync_WhenOwnerCatalogFileNameResolvesToPackageDirectory_ReturnsInvalidBeforeProviderOrFilesystem`

Caller configuration:

```csharp
OwnerCatalogFileName = "."
```

The value resolves exactly to the package directory and does not identify a catalog file. It does not escape the package.

Required response:

```text
Status: Invalid
Data: null
Code: WIF_JSONS_OWNER_CATALOG_FILE_NAME_INVALID
Message: The owner catalog file name is invalid.
```

The tracking template provider must not be invoked; the initially nonexistent workspace root must remain absent.

Current owner containment returns false for the package-directory target and classifies it as `WIF_JSONS_OWNER_CATALOG_FILE_NAME_OUTSIDE_PACKAGE` instead of an invalid filename.

The focused contract confirmed the expected RED on Windows:

```text
Expected: WIF_JSONS_OWNER_CATALOG_FILE_NAME_INVALID
Actual:   WIF_JSONS_OWNER_CATALOG_FILE_NAME_OUTSIDE_PACKAGE
```

The response status was already `Invalid`; only the error classification was wrong. `OwnerCatalogFileName = "."` resolves exactly to the package directory, not outside it.

Production fix commit:
`d24e62b0fd3404a3ea17acaea450643226e9125e`

Documentation commit:
`8b2e4b994e47d8627cf544eaa89b1979508a4687`

When owner filename containment fails but the canonical target equals the canonical package directory, the bootstrapper now returns:

```text
Status: Invalid
Code: WIF_JSONS_OWNER_CATALOG_FILE_NAME_INVALID
Message: The owner catalog file name is invalid.
```

True escapes still return `WIF_JSONS_OWNER_CATALOG_FILE_NAME_OUTSIDE_PACKAGE`. Other caller fields and the shared containment helper remain unchanged.

**Complete-suite 1091/1091 GREEN was subsequently confirmed locally by the maintainer.** The latest confirmation did not separately report the focused result, compiler-warning count, or operating system.

## 2026-09-22 — 1090/1090 GREEN caller current-directory code group checkpoint

Contract commit: `81c6375a4f8e1cc6898fe9aaabaefb720b894800`

Production fix commit: `24a21904c1dc0f2ad7952cbef186e4cc99fa9341`

Documentation commit: `a3c3b8e7c0a5d5b13c11997999e1db5f744bdea0`

Locally confirmed by the maintainer:

```text
WhenItFails.Tests
Failed:   0
Passed: 1090
Total:  1090
```

`CodeGroupCatalogFileName = "."` now returns the code-group-specific invalid filename code before provider invocation or workspace mutation. Actual escapes still return `WIF_JSONS_CODE_GROUP_CATALOG_FILE_NAME_OUTSIDE_PACKAGE`.

Next caller-configured field: `OwnerCatalogFileName`.

## 2026-09-22 — caller current-directory code group catalog filename contract

Contract commit:
`81c6375a4f8e1cc6898fe9aaabaefb720b894800`

Baseline: **1089/1089 GREEN**, confirmed locally by the maintainer before this contract was introduced.

Added:
`WhenItFails.Tests/Bootstrap/JsonsBootstrapperCurrentDirectoryCodeGroupCatalogFileNameContractTests.cs`

Contract:
`EnsureWorkspaceAsync_WhenCodeGroupCatalogFileNameResolvesToPackageDirectory_ReturnsInvalidBeforeProviderOrFilesystem`

Caller configuration:

```csharp
CodeGroupCatalogFileName = "."
```

The target resolves exactly to the package directory and does not identify a catalog file. This is not a path escaping the package.

Required response:

```text
Status: Invalid
Data: null
Code: WIF_JSONS_CODE_GROUP_CATALOG_FILE_NAME_INVALID
Message: The code group catalog file name is invalid.
```

The tracking template provider must not be invoked and the initially nonexistent workspace root must remain absent.

Current code-group containment rejects the package-directory target as `WIF_JSONS_CODE_GROUP_CATALOG_FILE_NAME_OUTSIDE_PACKAGE`. The other already fixed caller fields are not being changed by this test.

The focused contract confirmed the expected RED on Windows:

```text
Expected: WIF_JSONS_CODE_GROUP_CATALOG_FILE_NAME_INVALID
Actual:   WIF_JSONS_CODE_GROUP_CATALOG_FILE_NAME_OUTSIDE_PACKAGE
```

The response status was already `Invalid`; the code group filename was incorrectly classified as escaping the package despite resolving exactly to the package directory.

Production fix commit:
`24a21904c1dc0f2ad7952cbef186e4cc99fa9341`

Documentation commit:
`a3c3b8e7c0a5d5b13c11997999e1db5f744bdea0`

After the shared containment check reports false, `JsonsBootstrapper` compares the canonical code group target against the canonical package directory. Equality now returns:

```text
Status: Invalid
Code: WIF_JSONS_CODE_GROUP_CATALOG_FILE_NAME_INVALID
Message: The code group catalog file name is invalid.
```

True escapes still return `WIF_JSONS_CODE_GROUP_CATALOG_FILE_NAME_OUTSIDE_PACKAGE`. Other caller fields and the shared containment helper remain unchanged.

**Complete-suite 1090/1090 GREEN was subsequently confirmed locally by the maintainer.** The latest confirmation did not separately report the focused result, compiler-warning count, or operating system.

## 2026-09-22 — 1089/1089 GREEN caller current-directory category checkpoint

Contract commit: `864595065ad7f3c95fa7d954e62a3aa094b2ff34`

Production fix commit: `9a76620d14f92e3b62f4186a21800efddb864464`

Documentation commit: `323f44cf69d09196c920d3c350446b1027cb33bd`

Locally confirmed by the maintainer:

```text
WhenItFails.Tests
Failed:   0
Passed: 1089
Total:  1089
```

`CategoryCatalogFileName = "."` now returns the category-specific invalid filename code before provider invocation or workspace mutation. Actual escapes still return `WIF_JSONS_CATEGORY_CATALOG_FILE_NAME_OUTSIDE_PACKAGE`.

Next caller-configured field: `CodeGroupCatalogFileName`.

## 2026-09-22 — caller current-directory category catalog filename contract

Contract commit:
`864595065ad7f3c95fa7d954e62a3aa094b2ff34`

Baseline: **1088/1088 GREEN**, confirmed locally by the maintainer before this contract was introduced.

Added:
`WhenItFails.Tests/Bootstrap/JsonsBootstrapperCurrentDirectoryCategoryCatalogFileNameContractTests.cs`

Contract:
`EnsureWorkspaceAsync_WhenCategoryCatalogFileNameResolvesToPackageDirectory_ReturnsInvalidBeforeProviderOrFilesystem`

Caller configuration:

```csharp
CategoryCatalogFileName = "."
```

This value resolves exactly to the package directory, not outside it, and cannot identify a catalog file.

Require:

```text
Status: Invalid
Data: null
Code: WIF_JSONS_CATEGORY_CATALOG_FILE_NAME_INVALID
Message: The category catalog file name is invalid.
```

The tracking template provider must not be invoked; the initially nonexistent workspace root must remain absent.

Existing caller category containment is expected to report false when the target resolves to the package directory itself. Unlike the already fixed error catalog field, the category field does not yet distinguish this semantic-directory value from an actual escape. Focused RED is expected due to the `WIF_JSONS_CATEGORY_CATALOG_FILE_NAME_OUTSIDE_PACKAGE` code.

The focused contract confirmed the expected RED on Windows:

```text
Expected: WIF_JSONS_CATEGORY_CATALOG_FILE_NAME_INVALID
Actual:   WIF_JSONS_CATEGORY_CATALOG_FILE_NAME_OUTSIDE_PACKAGE
```

The response status was already `Invalid`; only the caller-specific error classification was wrong. The target `"." ` resolves exactly to the package directory rather than escaping it.

Production fix commit:
`9a76620d14f92e3b62f4186a21800efddb864464`

Documentation commit:
`323f44cf69d09196c920d3c350446b1027cb33bd`

If the category filename fails containment but its canonical path equals the canonical package directory path, bootstrap now returns:

```text
Status: Invalid
Code: WIF_JSONS_CATEGORY_CATALOG_FILE_NAME_INVALID
Message: The category catalog file name is invalid.
```

True escapes still return `WIF_JSONS_CATEGORY_CATALOG_FILE_NAME_OUTSIDE_PACKAGE`. Shared containment logic, package-directory semantics, and the other caller fields were not changed.

**Complete-suite 1089/1089 GREEN was subsequently confirmed locally by the maintainer.** The latest confirmation did not separately report the focused result, compiler-warning count, or operating system.

## 2026-09-22 — 1088/1088 GREEN caller current-directory error filename checkpoint

Contract commit: `bf1e0e9fb853359d95f9f030882ea368847d0d70`

Production fix commit: `209505b4e79beb25571088c882a82ee55b31f420`

Documentation commit: `4b431d1d0b132fbcf0f891cb61f9de5ad0b9a70a`

Locally confirmed by the maintainer:

```text
WhenItFails.Tests
Failed:   0
Passed: 1088
Total:  1088
```

`ErrorCatalogFileName = "."` now returns the field-specific invalid filename code before provider invocation or filesystem mutation. Actual escapes continue to return `WIF_JSONS_ERROR_CATALOG_FILE_NAME_OUTSIDE_PACKAGE`.

Next caller-configured field to audit separately: `CategoryCatalogFileName`.

## 2026-09-22 — caller current-directory error filename contract

Contract commit:
`bf1e0e9fb853359d95f9f030882ea368847d0d70`

Baseline: **1087/1087 GREEN**, confirmed locally by the maintainer before the contract was introduced.

Added:
`WhenItFails.Tests/Bootstrap/JsonsBootstrapperCurrentDirectoryErrorCatalogFileNameContractTests.cs`

Contract:
`EnsureWorkspaceAsync_WhenErrorCatalogFileNameResolvesToPackageDirectory_ReturnsInvalidBeforeProviderOrFilesystem`

Caller configuration:

```csharp
ErrorCatalogFileName = "."
```

The value resolves exactly to the package directory itself. It does not escape the package but cannot identify a catalog file.

Required result:

```text
Status: Invalid
Data: null
Code: WIF_JSONS_ERROR_CATALOG_FILE_NAME_INVALID
Message: The error catalog file name is invalid.
```

The tracking provider must not be invoked and the initially nonexistent workspace root must remain absent.

The existing shared containment helper reports false when a path resolves to the directory itself; current caller validation is therefore expected to return `WIF_JSONS_ERROR_CATALOG_FILE_NAME_OUTSIDE_PACKAGE`, which has the wrong classification for this input.

The focused contract confirmed the expected RED on Windows:

```text
Expected: WIF_JSONS_ERROR_CATALOG_FILE_NAME_INVALID
Actual:   WIF_JSONS_ERROR_CATALOG_FILE_NAME_OUTSIDE_PACKAGE
```

The response status was already `Invalid`; only the error classification was wrong. The value `"." ` resolves exactly to the package directory, not outside it.

Production fix commit:
`209505b4e79beb25571088c882a82ee55b31f420`

Documentation commit:
`4b431d1d0b132fbcf0f891cb61f9de5ad0b9a70a`

If the caller error catalog filename fails containment but its canonical path equals the canonical package directory path, the bootstrapper returns:

```text
Status: Invalid
Code: WIF_JSONS_ERROR_CATALOG_FILE_NAME_INVALID
Message: The error catalog file name is invalid.
```

Actual escapes retain `WIF_JSONS_ERROR_CATALOG_FILE_NAME_OUTSIDE_PACKAGE`. The shared containment helper, package-directory behavior, and the other four caller filename fields were not changed.

**Complete-suite 1088/1088 GREEN was subsequently confirmed locally by the maintainer.** The latest confirmation did not separately report the focused result, compiler-warning count, or operating system.

## 2026-09-22 — 1087/1087 GREEN provider current-directory checkpoint

Contract commit: `c4367fd76d4d1f8e28314e72b5e6f337e53e090b`

Production fix commit: `cc01f3aa25d640bcc1fdc5e0ca3b35ae3e6eb7b2`

Documentation commit: `4d4a0461f4ef716210897a59d0710ed4e71dedb4`

Locally confirmed by the maintainer:

```text
WhenItFails.Tests
Failed:   0
Passed: 1087
Total:  1087
```

Provider `TargetFileName = "."` now returns the invalid-target code rather than outside-package. This fixes error classification without modifying shared containment behavior or the distinct escape-path contract.

Next audit: caller-configured `ErrorCatalogFileName = "."` resolves to the package directory and should receive the caller-specific invalid filename code before any provider invocation or workspace creation.

## 2026-09-21 — provider current-directory target contract

Contract commit:
`c4367fd76d4d1f8e28314e72b5e6f337e53e090b`

Baseline: **1086/1086 GREEN**, confirmed locally by the maintainer before this contract was introduced.

Added:
`WhenItFails.Tests/Bootstrap/JsonsBootstrapperCurrentDirectoryTemplateTargetContractTests.cs`

Contract:
`EnsureWorkspaceAsync_WhenLaterTemplateTargetResolvesToPackageDirectory_ReturnsInvalidWithoutPartialWrites`

The provider returns a valid `first.json` followed by:

```csharp
TargetFileName = "."
```

The target does not escape the package; it resolves exactly to the package directory and therefore does not identify a file.

Require:

```text
Status: Invalid
Data: null
Code: WIF_JSONS_TEMPLATE_TARGET_FILE_NAME_INVALID
Message: The JSON template provider returned a template with an invalid target file name.
```

Because the full provider snapshot is validated before template writes, `first.json` must not be created.

Current containment logic compares the resolved target against a package-directory prefix. A target resolving exactly to the package directory does not start with that prefix and is therefore currently expected to be classified as `WIF_JSONS_TEMPLATE_TARGET_FILE_NAME_OUTSIDE_PACKAGE` instead of the required invalid-target contract.

The focused contract confirmed the expected RED on Windows:

```text
Expected: WIF_JSONS_TEMPLATE_TARGET_FILE_NAME_INVALID
Actual:   WIF_JSONS_TEMPLATE_TARGET_FILE_NAME_OUTSIDE_PACKAGE
```

The response status was already `Invalid`; the defect was classification. `TargetFileName = "."` resolves exactly to the package directory, so it is a directory target rather than an escaping path.

Production fix commit:
`cc01f3aa25d640bcc1fdc5e0ca3b35ae3e6eb7b2`

Documentation commit:
`4d4a0461f4ef716210897a59d0710ed4e71dedb4`

The provider-target path now gets a narrow equality check only when the shared containment helper reports false. If the canonical target path equals the canonical package directory path, the bootstrapper returns:

```text
Status: Invalid
Code: WIF_JSONS_TEMPLATE_TARGET_FILE_NAME_INVALID
Message: The JSON template provider returned a template with an invalid target file name.
```

True escapes such as parent-directory targets remain `WIF_JSONS_TEMPLATE_TARGET_FILE_NAME_OUTSIDE_PACKAGE`. The shared `IsPathInsideDirectory` helper and `PackageDirectoryName` semantics were not changed.

**Complete-suite 1087/1087 GREEN was subsequently confirmed locally by the maintainer.** The latest confirmation did not separately report the focused result, compiler-warning count, or operating system.

## 2026-09-21 — 1086/1086 GREEN five-field existing-directory checkpoint

Final contract commit: `eeb55feb66f53e4be9fb5bed739ef7c14f44d4c0`

Final production fix commit: `d8c1cb78c7eaf362fd7693c972e645bc30571177`

Final documentation commit: `bd09568f490a88c5bd1a230223c5d55b67d1e8f5`

Locally confirmed by the maintainer:

```text
WhenItFails.Tests
Failed:   0
Passed: 1086
Total:  1086
```

All five caller-configured catalog filename fields now reject values whose resolved paths already exist as directories before template-provider invocation, with their field-specific stable invalid codes and messages. The earlier provider-target existing-directory contract remains separate and GREEN.

Next distinct audit: semantic directory targets such as `TargetFileName = "."`, which currently resolve to the package directory itself and are classified by containment before the existing-directory guard can run.

## 2026-09-21 — existing-directory profiles filename contract

Contract commit:
`eeb55feb66f53e4be9fb5bed739ef7c14f44d4c0`

Baseline: **1085/1085 GREEN**, confirmed locally by the maintainer before this contract was introduced.

Added:
`WhenItFails.Tests/Bootstrap/JsonsBootstrapperExistingDirectoryProfilesFileNameContractTests.cs`

Contract:
`EnsureWorkspaceAsync_WhenProfilesFileNameResolvesToExistingDirectory_ReturnsInvalidBeforeProvider`

The package workspace already contains a `Nested` directory with `preserve.txt`, while caller configuration uses:

```csharp
ProfilesFileName = "Nested"
```

The value is lexically valid, contained in the package, and does not end with a directory separator, but resolves to a directory rather than a file.

Require:

```text
Status: Invalid
Data: null
Code: WIF_JSONS_PROFILE_CATALOG_FILE_NAME_INVALID
Message: The profile catalog file name is invalid.
```

The template provider must not be invoked, and the existing directory and its contents must remain unchanged.

Current production has existing-directory guards for the other four caller-configured catalog filenames; `ProfilesFileName` still stops after lexical containment, so focused RED is expected.

The focused contract confirmed the expected RED on Windows:

```text
Expected: Invalid
Actual:   Success
```

The lexically valid contained profile filename resolved to an existing directory, the tracking provider was reachable, and bootstrap returned success instead of rejecting caller configuration.

Production fix commit:
`d8c1cb78c7eaf362fd7693c972e645bc30571177`

Documentation commit:
`bd09568f490a88c5bd1a230223c5d55b67d1e8f5`

After the existing containment check, `JsonsBootstrapper` now resolves `ProfilesFileName` inside the package workspace and rejects it when that path already exists as a directory:

```text
Status: Invalid
Code: WIF_JSONS_PROFILE_CATALOG_FILE_NAME_INVALID
Message: The profile catalog file name is invalid.
```

The guard runs before template-provider invocation. Existing directory contents remain untouched, and the existing null, whitespace, malformed-path, trailing-separator, and outside-package contracts remain separate.

All five caller-configured catalog filename fields now contain the existing-directory guard in production.

**Complete-suite 1086/1086 GREEN was subsequently confirmed locally by the maintainer.** The latest confirmation did not separately report the focused result, compiler-warning count, or operating system.

## 2026-09-21 — 1085/1085 GREEN existing-directory owner filename checkpoint

Contract commit: `40ac5516b7c4c0f6d1c8d711f64f99ff55a23d35`

Production fix commit: `ce3748f1c843ba42051fee82585bbbefa3528411`

Documentation commit: `ae8324f8dd80a32d2cef9f14f59425ef5215cd9c`

Locally confirmed by the maintainer:

```text
WhenItFails.Tests
Failed:   0
Passed: 1085
Total:  1085
```

`OwnerCatalogFileName` values resolving to existing directories are now rejected as caller configuration before provider invocation. Existing directory contents remain unchanged.

Final caller-configured field in this series: `ProfilesFileName`.

## 2026-09-21 — existing-directory owner catalog filename contract

Contract commit:
`40ac5516b7c4c0f6d1c8d711f64f99ff55a23d35`

Baseline: **1084/1084 GREEN**, confirmed locally by the maintainer before this contract was introduced.

Added:
`WhenItFails.Tests/Bootstrap/JsonsBootstrapperExistingDirectoryOwnerCatalogFileNameContractTests.cs`

Contract:
`EnsureWorkspaceAsync_WhenOwnerCatalogFileNameResolvesToExistingDirectory_ReturnsInvalidBeforeProvider`

The package workspace already contains a `Nested` directory with `preserve.txt`, while caller configuration uses:

```csharp
OwnerCatalogFileName = "Nested"
```

The value is lexically valid, contained in the package, and does not end with a directory separator, but resolves to a directory rather than a file.

Require:

```text
Status: Invalid
Data: null
Code: WIF_JSONS_OWNER_CATALOG_FILE_NAME_INVALID
Message: The owner catalog file name is invalid.
```

The template provider must not be invoked, and the existing directory and its contents must remain unchanged.

Current production has existing-directory guards for the error, category, and code-group fields only; the owner field still stops after lexical containment, so focused RED is expected.

The focused contract confirmed the expected RED on Windows:

```text
Expected: Invalid
Actual:   Success
```

The lexically valid contained owner filename resolved to an existing directory, the tracking provider was reachable, and bootstrap returned success instead of rejecting caller configuration.

Production fix commit:
`ce3748f1c843ba42051fee82585bbbefa3528411`

Documentation commit:
`ae8324f8dd80a32d2cef9f14f59425ef5215cd9c`

After the existing containment check, `JsonsBootstrapper` now resolves `OwnerCatalogFileName` inside the package workspace and rejects it when that path already exists as a directory:

```text
Status: Invalid
Code: WIF_JSONS_OWNER_CATALOG_FILE_NAME_INVALID
Message: The owner catalog file name is invalid.
```

The guard runs before template-provider invocation. Existing directory contents remain untouched, and the existing null, whitespace, malformed-path, trailing-separator, and outside-package contracts remain separate.

**Complete-suite 1085/1085 GREEN was subsequently confirmed locally by the maintainer.** The latest confirmation did not separately report the focused result, compiler-warning count, or operating system.

## 2026-09-21 — 1084/1084 GREEN existing-directory code group filename checkpoint

Contract commit: `541630e28b68109f5f9ad2bb4b216f794ace985d`

Production fix commit: `43ca724ee46dfe8aae71d16860bafd51810f9dad`

Documentation commit: `38c9a8ba5b65f52bb810ca43299d63dd898306eb`

Locally confirmed by the maintainer:

```text
WhenItFails.Tests
Failed:   0
Passed: 1084
Total:  1084
```

`CodeGroupCatalogFileName` values resolving to existing directories are now rejected as caller configuration before provider invocation. Existing directory contents remain unchanged.

Next caller-configured field: `OwnerCatalogFileName`.

## 2026-09-21 — existing-directory code group catalog filename contract

Contract commit:
`541630e28b68109f5f9ad2bb4b216f794ace985d`

Baseline: **1083/1083 GREEN**, confirmed locally by the maintainer before this contract was introduced.

Added:
`WhenItFails.Tests/Bootstrap/JsonsBootstrapperExistingDirectoryCodeGroupCatalogFileNameContractTests.cs`

Contract:
`EnsureWorkspaceAsync_WhenCodeGroupCatalogFileNameResolvesToExistingDirectory_ReturnsInvalidBeforeProvider`

The package workspace already contains a `Nested` directory with `preserve.txt`, while caller configuration uses:

```csharp
CodeGroupCatalogFileName = "Nested"
```

The value is lexically valid, contained in the package, and does not end with a directory separator, but resolves to a directory rather than a file.

Require:

```text
Status: Invalid
Data: null
Code: WIF_JSONS_CODE_GROUP_CATALOG_FILE_NAME_INVALID
Message: The code group catalog file name is invalid.
```

The template provider must not be invoked, and the existing directory and its contents must remain unchanged.

Current production has existing-directory guards for the error and category fields only; the code-group field still stops after lexical containment, so focused RED is expected.

The focused contract confirmed the expected RED on Windows:

```text
Expected: Invalid
Actual:   Success
```

The lexically valid contained code-group filename resolved to an existing directory, the tracking provider was reachable, and bootstrap returned success instead of rejecting caller configuration.

Production fix commit:
`43ca724ee46dfe8aae71d16860bafd51810f9dad`

Documentation commit:
`38c9a8ba5b65f52bb810ca43299d63dd898306eb`

After the existing containment check, `JsonsBootstrapper` now resolves `CodeGroupCatalogFileName` inside the package workspace and rejects it when that path already exists as a directory:

```text
Status: Invalid
Code: WIF_JSONS_CODE_GROUP_CATALOG_FILE_NAME_INVALID
Message: The code group catalog file name is invalid.
```

The guard runs before template-provider invocation. Existing directory contents remain untouched, and the existing null, whitespace, malformed-path, trailing-separator, and outside-package contracts remain separate.

**Complete-suite 1084/1084 GREEN was subsequently confirmed locally by the maintainer.** The latest confirmation did not separately report the focused result, compiler-warning count, or operating system.

## 2026-09-21 — 1083/1083 GREEN existing-directory category filename checkpoint

Contract commit: `043634a61d6f7de7d91c665db81191d39b3f9614`

Production fix commit: `dacff0d269f327600906262ce25d206521db2393`

Documentation commit: `1c9852d025a18a3f4a4297af43a06445ebe82453`

Locally confirmed by the maintainer:

```text
WhenItFails.Tests
Failed:   0
Passed: 1083
Total:  1083
```

`CategoryCatalogFileName` values resolving to existing directories are now rejected as caller configuration before provider invocation. Existing directory contents remain unchanged.

Next caller-configured field: `CodeGroupCatalogFileName`.

## 2026-09-21 — existing-directory category catalog filename contract

Contract commit:
`043634a61d6f7de7d91c665db81191d39b3f9614`

Baseline: **1082/1082 GREEN**, confirmed locally by the maintainer before this contract was introduced.

Added:
`WhenItFails.Tests/Bootstrap/JsonsBootstrapperExistingDirectoryCategoryCatalogFileNameContractTests.cs`

Contract:
`EnsureWorkspaceAsync_WhenCategoryCatalogFileNameResolvesToExistingDirectory_ReturnsInvalidBeforeProvider`

The package workspace already contains a `Nested` directory with `preserve.txt`, while caller configuration uses:

```csharp
CategoryCatalogFileName = "Nested"
```

The value is lexically valid, contained in the package, and does not end with a directory separator, but resolves to a directory rather than a file.

Require:

```text
Status: Invalid
Data: null
Code: WIF_JSONS_CATEGORY_CATALOG_FILE_NAME_INVALID
Message: The category catalog file name is invalid.
```

The template provider must not be invoked, and the existing directory and its contents must remain unchanged.

Current production has the existing-directory guard only for `ErrorCatalogFileName`; the category field still stops after lexical containment, so focused RED is expected.

The focused contract confirmed the expected RED on Windows:

```text
Expected: Invalid
Actual:   Success
```

The lexically valid contained category filename resolved to an existing directory, the tracking provider was reachable, and bootstrap returned success instead of rejecting caller configuration.

Production fix commit:
`dacff0d269f327600906262ce25d206521db2393`

Documentation commit:
`1c9852d025a18a3f4a4297af43a06445ebe82453`

After the existing containment check, `JsonsBootstrapper` now resolves `CategoryCatalogFileName` inside the package workspace and rejects it when that path already exists as a directory:

```text
Status: Invalid
Code: WIF_JSONS_CATEGORY_CATALOG_FILE_NAME_INVALID
Message: The category catalog file name is invalid.
```

The guard runs before template-provider invocation. Existing directory contents remain untouched, and the existing null, whitespace, malformed-path, trailing-separator, and outside-package contracts remain separate.

**Complete-suite 1083/1083 GREEN was subsequently confirmed locally by the maintainer.** The latest confirmation did not separately report the focused result, compiler-warning count, or operating system.

## 2026-09-21 — 1082/1082 GREEN existing-directory error filename checkpoint

Contract commit: `39e90ff34020818a7f1c7bccb3ee27249f18619a`

Production fix commit: `deb62864b575195fc0d8ce63dcafe80ed109ef1b`

Documentation commit: `bf12036cf2d32a20e766a4e6740bfb8903aff5ef`

Locally confirmed by the maintainer:

```text
WhenItFails.Tests
Failed:   0
Passed: 1082
Total:  1082
```

`ErrorCatalogFileName` values resolving to existing directories are now rejected as caller configuration before provider invocation. Existing directory contents remain unchanged.

Next caller-configured field: `CategoryCatalogFileName`.

## 2026-09-21 — existing-directory error catalog filename contract

Contract commit:
`39e90ff34020818a7f1c7bccb3ee27249f18619a`

Baseline: **1081/1081 GREEN**, confirmed locally by the maintainer before this contract was introduced.

Added:
`WhenItFails.Tests/Bootstrap/JsonsBootstrapperExistingDirectoryErrorCatalogFileNameContractTests.cs`

Contract:
`EnsureWorkspaceAsync_WhenErrorCatalogFileNameResolvesToExistingDirectory_ReturnsInvalidBeforeProvider`

The package workspace already contains a `Nested` directory with `preserve.txt`, while caller configuration uses:

```csharp
ErrorCatalogFileName = "Nested"
```

The value is lexically valid, contained in the package, and does not end with a directory separator, but it resolves to a directory rather than a file.

Require:

```text
Status: Invalid
Data: null
Code: WIF_JSONS_ERROR_CATALOG_FILE_NAME_INVALID
Message: The error catalog file name is invalid.
```

The template provider must not be invoked, and the existing directory and its contents must remain unchanged.

Current production validates lexical containment but does not check whether the caller-configured target already resolves to a directory. With the tracking provider returning no templates, current behavior is expected to complete successfully.

The focused contract confirmed the expected RED on Windows:

```text
Expected: Invalid
Actual:   Success
```

The lexically valid contained caller filename resolved to an existing directory, the tracking provider was reachable, and bootstrap returned success instead of rejecting caller configuration.

Production fix commit:
`deb62864b575195fc0d8ce63dcafe80ed109ef1b`

Documentation commit:
`bf12036cf2d32a20e766a4e6740bfb8903aff5ef`

After the existing containment check, `JsonsBootstrapper` now resolves `ErrorCatalogFileName` inside the package workspace and rejects it when that path already exists as a directory:

```text
Status: Invalid
Code: WIF_JSONS_ERROR_CATALOG_FILE_NAME_INVALID
Message: The error catalog file name is invalid.
```

The guard runs before template-provider invocation. Existing directory contents remain untouched, and the existing null, whitespace, malformed-path, trailing-separator, and outside-package contracts remain separate.

**Complete-suite 1082/1082 GREEN was subsequently confirmed locally by the maintainer.** The latest confirmation did not separately report the focused result, compiler-warning count, or operating system.

## 2026-09-21 — 1081/1081 GREEN existing-directory provider-target checkpoint

Contract commit: `fe1c1ab2b8886f0c3cf2af3584eaa7c5885ace33`

Production fix commit: `3567953e5d83613b41f667251f83bad154cfe615`

Documentation commit: `afc99aab299391255953217fd48a298aef46a847`

Locally confirmed by the maintainer:

```text
WhenItFails.Tests
Failed:   0
Passed: 1081
Total:  1081
```

Provider targets resolving to existing directories are rejected during full-snapshot validation before any template write. Existing directory contents remain untouched and valid nested file targets remain supported.

Next caller-configuration audit: distinguish a file-name option that lexically looks valid but resolves to an already existing directory before invoking the provider.

## 2026-09-21 — existing-directory template target contract

Contract commit:
`fe1c1ab2b8886f0c3cf2af3584eaa7c5885ace33`

Baseline: **1080/1080 GREEN**, locally confirmed by the maintainer before this new test.

Added:
`WhenItFails.Tests/Bootstrap/JsonsBootstrapperExistingDirectoryTemplateTargetContractTests.cs`

Contract:
`EnsureWorkspaceAsync_WhenLaterTemplateTargetIsExistingDirectory_ReturnsInvalidWithoutPartialWrites`

The workspace contains an existing `Nested` directory with `preserve.txt`. The provider returns two templates: a valid `first.json` followed by `TargetFileName = "Nested"` (no trailing separator). The latter is syntactically acceptable and inside the package lexically, but names a directory rather than a file.

Required response:

```text
Status: Invalid
Data: null
Code: WIF_JSONS_TEMPLATE_TARGET_FILE_NAME_INVALID
Message: The JSON template provider returned a template with an invalid target file name.
```

Require that `first.json` is not written and the pre-existing directory and its contents remain unchanged. This checks validation of the entire provider snapshot before file writes.

Current production rejects directory-only names with trailing separators but does not check whether a separator-free target already resolves to a directory. The second write is expected to fail as a filesystem error, after the first template was created.

The focused contract confirmed the expected RED on Windows:

```text
Expected: Invalid
Actual:   Failed
```

The later provider target named an existing directory. Current production classified it only at the write phase, after the first valid template could already be written.

Production fix commit:
`3567953e5d83613b41f667251f83bad154cfe615`

Documentation commit:
`afc99aab299391255953217fd48a298aef46a847`

During full-snapshot validation, `JsonsBootstrapper` now checks whether each contained, normalized provider target resolves to an existing directory and rejects it before any template files are written:

```text
Status: Invalid
Code: WIF_JSONS_TEMPLATE_TARGET_FILE_NAME_INVALID
Message: The JSON template provider returned a template with an invalid target file name.
```

Existing directories and their files remain untouched, and valid nested file targets remain supported. This is a validation-time safeguard, not a transactional guarantee against unrelated I/O failures or concurrent filesystem changes.

**Complete-suite 1081/1081 GREEN was subsequently confirmed locally by the maintainer.** The latest confirmation did not separately report the focused result, compiler-warning count, or operating system.

## 2026-09-21 — 1080/1080 GREEN five-field directory-only checkpoint

Contract commit: `306b7be4148dc73a268376dd5c36fdc36fb5d1b2`

Production fix commit: `e74fff6c9624e93e857fba49769694b3c9b14622`

Documentation commit: `85969b3a56ea1336c432e95da84c312404a99076`

Locally confirmed by the maintainer:

```text
WhenItFails.Tests
Failed:   0
Passed: 1080
Total:  1080
```

Directory-only caller filenames are now rejected for all five catalog fields before invoking the provider or mutating the workspace. Previously verified null, whitespace, malformed-syntax, containment, and valid nested-target contracts remain covered by the complete suite.

Next boundary to audit: a provider target that syntactically looks like a file, but resolves to an existing directory in the workspace.

## 2026-09-21 — directory-only profiles filename contract

Contract commit:
`306b7be4148dc73a268376dd5c36fdc36fb5d1b2`

Baseline: **1079/1079 GREEN**, confirmed locally by the maintainer before this contract was introduced.

Added:
`WhenItFails.Tests/Bootstrap/JsonsBootstrapperDirectoryProfilesFileNameContractTests.cs`

Contract:
`EnsureWorkspaceAsync_WhenProfilesFileNameEndsWithDirectorySeparator_ReturnsInvalidBeforeProviderOrFilesystem`

Caller configuration uses:

```csharp
ProfilesFileName =
    "Nested" + Path.DirectorySeparatorChar
```

Require:

```text
Status: Invalid
Data: null
Code: WIF_JSONS_PROFILE_CATALOG_FILE_NAME_INVALID
Message: The profile catalog file name is invalid.
```

The template provider must not be invoked and the workspace root must not be created.

Current caller validation checks containment but does not yet reject a directory-only profile catalog target, so focused RED is expected.

The focused contract confirmed the expected RED on Windows:

```text
Expected: Invalid
Actual:   Success
```

The caller-configured directory-only profile filename passed early validation and allowed bootstrap to complete successfully.

Production fix commit:
`e74fff6c9624e93e857fba49769694b3c9b14622`

Documentation commit:
`85969b3a56ea1336c432e95da84c312404a99076`

`JsonsBootstrapper` now normalizes `ProfilesFileName` and rejects values ending with a directory separator before containment, provider invocation, or filesystem mutation:

```text
Status: Invalid
Code: WIF_JSONS_PROFILE_CATALOG_FILE_NAME_INVALID
Message: The profile catalog file name is invalid.
```

Its existing malformed-path and outside-package contracts remain separate. All five caller-configured filename fields now contain the directory-only guard in production, but full-suite verification of the last guard is still pending.

**Complete-suite 1080/1080 GREEN was subsequently confirmed locally by the maintainer.** The latest confirmation did not separately report the focused result, compiler-warning count, or operating system.

## 2026-09-21 — 1079/1079 GREEN directory-only owner filename checkpoint

Contract commit: `2c7abb14f8d442a51bf377ef2e51d6e4dabf4f8d`

Production fix commit: `90b10457c57abe87af8465037f6fa9409ba8af3e`

Documentation commit: `82f54c23d63287a6928c8c48f3d204314f5a59c8`

Locally confirmed by the maintainer:

```text
WhenItFails.Tests
Failed:   0
Passed: 1079
Total:  1079
```

Directory-only `OwnerCatalogFileName` values are now rejected as caller configuration before provider invocation or filesystem mutation.

Final caller-configured field in this group: `ProfilesFileName`.

## 2026-09-21 — directory-only owner catalog filename contract

Contract commit:
`2c7abb14f8d442a51bf377ef2e51d6e4dabf4f8d`

Baseline: **1078/1078 GREEN**, confirmed locally by the maintainer before this contract was introduced.

Added:
`WhenItFails.Tests/Bootstrap/JsonsBootstrapperDirectoryOwnerCatalogFileNameContractTests.cs`

Contract:
`EnsureWorkspaceAsync_WhenOwnerCatalogFileNameEndsWithDirectorySeparator_ReturnsInvalidBeforeProviderOrFilesystem`

Caller configuration uses:

```csharp
OwnerCatalogFileName =
    "Nested" + Path.DirectorySeparatorChar
```

Require:

```text
Status: Invalid
Data: null
Code: WIF_JSONS_OWNER_CATALOG_FILE_NAME_INVALID
Message: The owner catalog file name is invalid.
```

The template provider must not be invoked and the workspace root must not be created.

Current caller validation checks containment but does not yet reject a directory-only owner catalog target, so focused RED is expected.

The focused contract confirmed the expected RED on Windows:

```text
Expected: Invalid
Actual:   Success
```

The caller-configured directory-only owner catalog filename passed early validation and allowed bootstrap to complete successfully.

Production fix commit:
`90b10457c57abe87af8465037f6fa9409ba8af3e`

Documentation commit:
`82f54c23d63287a6928c8c48f3d204314f5a59c8`

`JsonsBootstrapper` now normalizes `OwnerCatalogFileName` and rejects values ending with a directory separator before containment, provider invocation, or filesystem mutation:

```text
Status: Invalid
Code: WIF_JSONS_OWNER_CATALOG_FILE_NAME_INVALID
Message: The owner catalog file name is invalid.
```

Its existing malformed-path and outside-package contracts remain separate.

**Complete-suite 1079/1079 GREEN was subsequently confirmed locally by the maintainer.** The latest confirmation did not separately report the focused result, compiler-warning count, or operating system.

## 2026-09-21 — 1078/1078 GREEN directory-only code group filename checkpoint

Contract commit: `b6a5edd311c0fa912664e9efbb437d72112148f7`

Production fix commit: `4cf9fd93aa2a7eeeddc45e0b80c0ed7e5894ccd1`

Documentation commit: `5722d1f59596d28afca83bae5f291b20ec2265df`

Locally confirmed by the maintainer:

```text
WhenItFails.Tests
Failed:   0
Passed: 1078
Total:  1078
```

Directory-only `CodeGroupCatalogFileName` values are now rejected as caller configuration before provider invocation or filesystem mutation.

Next caller-configured field: `OwnerCatalogFileName`.

## 2026-09-21 — directory-only code group catalog filename contract

Contract commit:
`b6a5edd311c0fa912664e9efbb437d72112148f7`

Baseline: **1077/1077 GREEN**, confirmed locally by the maintainer before this contract was introduced.

Added:
`WhenItFails.Tests/Bootstrap/JsonsBootstrapperDirectoryCodeGroupCatalogFileNameContractTests.cs`

Contract:
`EnsureWorkspaceAsync_WhenCodeGroupCatalogFileNameEndsWithDirectorySeparator_ReturnsInvalidBeforeProviderOrFilesystem`

Caller configuration uses:

```csharp
CodeGroupCatalogFileName =
    "Nested" + Path.DirectorySeparatorChar
```

Require:

```text
Status: Invalid
Data: null
Code: WIF_JSONS_CODE_GROUP_CATALOG_FILE_NAME_INVALID
Message: The code group catalog file name is invalid.
```

The template provider must not be invoked and the workspace root must not be created.

Current caller validation checks containment but does not yet reject a directory-only code-group catalog target, so focused RED is expected.

The focused contract confirmed the expected RED on Windows:

```text
Expected: Invalid
Actual:   Success
```

The caller-configured directory-only code-group catalog filename passed early validation and allowed bootstrap to complete successfully.

Production fix commit:
`4cf9fd93aa2a7eeeddc45e0b80c0ed7e5894ccd1`

Documentation commit:
`5722d1f59596d28afca83bae5f291b20ec2265df`

`JsonsBootstrapper` now normalizes `CodeGroupCatalogFileName` and rejects values ending with a directory separator before containment, provider invocation, or filesystem mutation:

```text
Status: Invalid
Code: WIF_JSONS_CODE_GROUP_CATALOG_FILE_NAME_INVALID
Message: The code group catalog file name is invalid.
```

Its existing malformed-path and outside-package contracts remain separate.

**Complete-suite 1078/1078 GREEN was subsequently confirmed locally by the maintainer.** The latest confirmation did not separately report the focused result, compiler-warning count, or operating system.

## 2026-09-21 — 1077/1077 GREEN directory-only category filename checkpoint

Contract commit: `73fd638f4f4d7c9906e0ed4b7f13d41ba81bc418`

Production fix commit: `cb605b2628ed9dd55e16bd3a89e9bf580f87afda`

Documentation commit: `eb126168b4e5c7a794c0a3bbc07158723c21e1a8`

Locally confirmed by the maintainer:

```text
WhenItFails.Tests
Failed:   0
Passed: 1077
Total:  1077
```

Directory-only `CategoryCatalogFileName` values are now rejected as caller configuration before provider invocation or filesystem mutation.

Next caller-configured field: `CodeGroupCatalogFileName`.

## 2026-09-21 — directory-only category catalog filename contract

Contract commit:
`73fd638f4f4d7c9906e0ed4b7f13d41ba81bc418`

Baseline: **1076/1076 GREEN**, confirmed locally by the maintainer before this contract was introduced.

Added:
`WhenItFails.Tests/Bootstrap/JsonsBootstrapperDirectoryCategoryCatalogFileNameContractTests.cs`

Contract:
`EnsureWorkspaceAsync_WhenCategoryCatalogFileNameEndsWithDirectorySeparator_ReturnsInvalidBeforeProviderOrFilesystem`

Caller configuration uses:

```csharp
CategoryCatalogFileName =
    "Nested" + Path.DirectorySeparatorChar
```

Require:

```text
Status: Invalid
Data: null
Code: WIF_JSONS_CATEGORY_CATALOG_FILE_NAME_INVALID
Message: The category catalog file name is invalid.
```

The template provider must not be invoked and the workspace root must not be created.

Current caller validation checks containment but does not yet reject a directory-only category catalog target, so focused RED is expected.

The focused contract confirmed the expected RED on Windows:

```text
Expected: Invalid
Actual:   Success
```

The caller-configured directory-only category catalog filename passed early validation and allowed bootstrap to complete successfully.

Production fix commit:
`cb605b2628ed9dd55e16bd3a89e9bf580f87afda`

Documentation commit:
`eb126168b4e5c7a794c0a3bbc07158723c21e1a8`

`JsonsBootstrapper` now normalizes `CategoryCatalogFileName` and rejects values ending with a directory separator before containment, provider invocation, or filesystem mutation:

```text
Status: Invalid
Code: WIF_JSONS_CATEGORY_CATALOG_FILE_NAME_INVALID
Message: The category catalog file name is invalid.
```

Its existing malformed-path and outside-package contracts remain separate.

**Complete-suite 1077/1077 GREEN was subsequently confirmed locally by the maintainer.** The latest confirmation did not separately report the focused result, compiler-warning count, or operating system.

## 2026-09-21 — 1076/1076 GREEN directory-only error filename checkpoint

Contract commit: `10e5ade814b41bd78ac5cdd15235386e836f1a00`

Production fix commit: `65fdd4baf0579b4fda6822c1b456c1ec438a9111`

Documentation commit: `a5c4aea3d66ef31c1858e9095fc652c2679e4c2d`

Locally confirmed by the maintainer:

```text
WhenItFails.Tests
Failed:   0
Passed: 1076
Total:  1076
```

Directory-only `ErrorCatalogFileName` values are now rejected as caller configuration before provider invocation or filesystem mutation.

Next caller-configured field: `CategoryCatalogFileName`.

## 2026-09-21 — directory-only error catalog filename contract

Contract commit:
`10e5ade814b41bd78ac5cdd15235386e836f1a00`

Baseline: **1075/1075 GREEN**, confirmed locally by the maintainer before this contract was introduced.

Added:
`WhenItFails.Tests/Bootstrap/JsonsBootstrapperDirectoryErrorCatalogFileNameContractTests.cs`

Contract:
`EnsureWorkspaceAsync_WhenErrorCatalogFileNameEndsWithDirectorySeparator_ReturnsInvalidBeforeProviderOrFilesystem`

Caller configuration uses:

```csharp
ErrorCatalogFileName =
    "Nested" + Path.DirectorySeparatorChar
```

Require:

```text
Status: Invalid
Data: null
Code: WIF_JSONS_ERROR_CATALOG_FILE_NAME_INVALID
Message: The error catalog file name is invalid.
```

The template provider must not be invoked and the workspace root must not be created.

This keeps malformed caller configuration distinct from malformed provider output. Current caller validation checks containment but not whether the configured catalog target denotes a file, so this value is expected to pass early validation.

The focused contract confirmed the expected RED on Windows:

```text
Expected: Invalid
Actual:   Success
```

The caller-configured directory-only error catalog filename passed early validation and allowed bootstrap to complete successfully.

Production fix commit:
`65fdd4baf0579b4fda6822c1b456c1ec438a9111`

Documentation commit:
`a5c4aea3d66ef31c1858e9095fc652c2679e4c2d`

`JsonsBootstrapper` now normalizes `ErrorCatalogFileName` and rejects values ending with a directory separator before containment, provider invocation, or filesystem mutation:

```text
Status: Invalid
Code: WIF_JSONS_ERROR_CATALOG_FILE_NAME_INVALID
Message: The error catalog file name is invalid.
```

The existing outside-package and malformed-path contracts remain separate.

**Complete-suite 1076/1076 GREEN was subsequently confirmed locally by the maintainer.** The latest confirmation did not separately report the focused result, compiler-warning count, or operating system.

## 2026-09-21 — 1075/1075 GREEN directory-only provider-target checkpoint

Contract commit: `521e6390293d464dd7cda6fe2d2f2834099e112c`

Production fix commit: `481c91eab264de58a55599dbd542db309e077b77`

Documentation commit: `fede769c4fd70a621f91565aa04edb0e3dc537b5`

Locally confirmed by the maintainer:

```text
WhenItFails.Tests
Failed:   0
Passed: 1075
Total:  1075
```

Directory-only provider targets are rejected as stable invalid provider output before nested-directory creation or file writes. Valid nested file targets remain supported.

Next caller-configuration audit: apply the same file-target requirement to caller-configured catalog filenames before provider invocation and filesystem mutation.

## 2026-09-21 — directory-only template target contract

Contract commit:
`521e6390293d464dd7cda6fe2d2f2834099e112c`

Baseline: **1074/1074 GREEN**, confirmed locally by the maintainer before this contract was introduced.

Added:
`WhenItFails.Tests/Bootstrap/JsonsBootstrapperDirectoryTemplateTargetFileNameContractTests.cs`

Contract:
`EnsureWorkspaceAsync_WhenTemplateTargetEndsWithDirectorySeparator_ReturnsInvalidBeforeCreatingNestedDirectory`

The provider returns a target constructed as:

```csharp
"Nested" + Path.DirectorySeparatorChar
```

This path resolves inside the package workspace, so containment alone accepts it, but it denotes a directory path rather than a file target.

Require:

```text
Status: Invalid
Data: null
Code: WIF_JSONS_TEMPLATE_TARGET_FILE_NAME_INVALID
Message: The JSON template provider returned a template with an invalid target file name.
```

The nested directory must not be created.

The focused contract confirmed the expected RED on Windows:

```text
Expected: Invalid
Actual:   Failed
```

The directory-only target passed containment and was classified only after the filesystem write path failed.

Production fix commit:
`481c91eab264de58a55599dbd542db309e077b77`

Documentation commit:
`fede769c4fd70a621f91565aa04edb0e3dc537b5`

`WhenItFails/Docs/Bootstrap/en.md` now documents that template targets must identify files, not directory-only paths ending with a directory separator.

`JsonsBootstrapper` now normalizes the provider target during the full-snapshot validation phase and rejects values ending with a directory separator before containment/write processing:

```text
Status: Invalid
Code: WIF_JSONS_TEMPLATE_TARGET_FILE_NAME_INVALID
Message: The JSON template provider returned a template with an invalid target file name.
```

Valid nested file targets such as `Nested/errors.en.json` remain supported.

**Complete-suite 1075/1075 GREEN was subsequently confirmed locally by the maintainer.** The latest confirmation did not separately report the focused result, compiler-warning count, or operating system.

## 2026-09-21 — 1074/1074 GREEN full template-snapshot validation checkpoint

Contract commit: `22afb9d9aba703f015adadc22495a8a839b67d5c`

Production fix commit: `2f7759369ca0aab51e9c46205628195cbf36b244`

Documentation commit: `c6ed1c36b1e092a3df3c2fff8f966cd048fb69ec`

Locally confirmed by the maintainer:

```text
WhenItFails.Tests
Failed:   0
Passed: 1074
Total:  1074
```

The latest confirmation did not separately report focused test output, compiler-warning count, or operating system.

The complete materialized provider snapshot is now validated before the first template file write, preventing malformed later items from leaving earlier provider files partially created.

Next provider-target audit: distinguish a valid nested file target from a directory-only target that ends with a directory separator.

## 2026-09-21 — later null template item no-partial-write contract

Contract commit:
`22afb9d9aba703f015adadc22495a8a839b67d5c`

Baseline: **1073/1073 GREEN**, confirmed locally by the maintainer before this contract was introduced.

Added:
`WhenItFails.Tests/Bootstrap/JsonsBootstrapperLaterNullTemplateItemContractTests.cs`

Contract:
`EnsureWorkspaceAsync_WhenLaterTemplateItemIsNull_ReturnsInvalidBeforeWritingAnyTemplateFiles`

The provider returns a materialized collection containing:

1. a valid first template targeting `first.json`,
2. a later `null` template item.

Require the existing malformed-item response:

```text
Status: Invalid
Data: null
Code: WIF_JSONS_TEMPLATE_ITEM_NULL
Message: The JSON template provider returned a null template item.
```

and require that `first.json` is not written.

This verifies that a malformed provider snapshot is validated as a whole before any template file mutation. Current production validates and writes in the same loop, so the first valid template is expected to be created before the later null item is discovered.

The focused contract confirmed the expected RED on Windows: the bootstrap correctly returned the existing `WIF_JSONS_TEMPLATE_ITEM_NULL` response, but `first.json` had already been created before the later null item was discovered.

Production fix commit:
`2f7759369ca0aab51e9c46205628195cbf36b244`

Documentation commit:
`c6ed1c36b1e092a3df3c2fff8f966cd048fb69ec`

`WhenItFails/Docs/Bootstrap/en.md` now documents full-snapshot validation before the first template file write and the resulting no-partial-write guarantee for malformed provider output.

Template processing now uses two phases:

```text
materialize provider collection
→ validate every template item
→ only then write template files
```

All existing provider-item error codes and messages remain unchanged. Cancellation checks remain present during validation and again before each file write.

**Complete-suite 1074/1074 GREEN was subsequently confirmed locally by the maintainer.** The latest confirmation did not separately report the focused result, compiler-warning count, or operating system.

## 2026-09-21 — 1073/1073 GREEN template enumeration-cancellation checkpoint

Contract commit: `62c0ece1813790bfcb586e1a046bf7bff89b444c`

No production change was required.

Locally confirmed by the maintainer:

```text
WhenItFails.Tests
Failed:   0
Passed: 1073
Total:  1073
```

The exact `OperationCanceledException` instance thrown while enumerating the returned template collection propagates unchanged, and no template file is written.

Next provider-output hardening target: validate the entire materialized template snapshot before any template file write so a malformed later item cannot leave an earlier valid item partially written.

## 2026-09-21 — template collection enumeration cancellation contract

Contract commit:
`62c0ece1813790bfcb586e1a046bf7bff89b444c`

Baseline: **1072/1072 GREEN**, confirmed locally by the maintainer before this regression contract was introduced.

Added:
`WhenItFails.Tests/Bootstrap/JsonsBootstrapperTemplateCollectionEnumerationCancellationContractTests.cs`

Contract:
`EnsureWorkspaceAsync_WhenTemplateCollectionEnumerationCancels_RethrowsSameOperationCanceledException`

The provider call succeeds and returns an `IReadOnlyList<JsonsTemplateFile>`, but the returned collection throws a specific `OperationCanceledException` instance from `GetEnumerator()`.

Require:

- the same exception instance to propagate unchanged,
- no conversion into `WIF_JSONS_TEMPLATE_PROVIDER_FAILED`,
- no template file write.

The current collection-materialization catch filter already excludes `OperationCanceledException`, so this is expected to be a focused **GREEN regression contract** with no production change.

**Focused and complete-suite GREEN were subsequently confirmed locally by the maintainer; total suite: 1073/1073.**

## 2026-09-21 — 1072/1072 GREEN template enumeration-failure checkpoint

Contract commit: `6abb4d38ecae08d2cf4d495907671855304b08fd`

Production fix commit: `f26676980a5654efb31f5e7ac0012942f9daa613`

Documentation commit: `8f8ef9583dfc9e713381fcbddbf0d8febfd9be6a`

Locally confirmed by the maintainer:

```text
WhenItFails.Tests
Failed:   0
Passed: 1072
Total:  1072
```

The latest confirmation did not separately report focused test output, compiler-warning count, or operating system.

Ordinary exceptions thrown while consuming the returned template collection now normalize to `WIF_JSONS_TEMPLATE_PROVIDER_FAILED` without exposing provider detail. Collection materialization also prevents partial template writes caused by a later enumeration failure.

Next regression contract: exact-instance `OperationCanceledException` propagation when cancellation is thrown during returned-collection enumeration.

## 2026-09-21 — template collection enumeration exception contract

Contract commit:
`6abb4d38ecae08d2cf4d495907671855304b08fd`

Baseline: **1071/1071 GREEN**, confirmed locally by the maintainer before this test was introduced.

Added:
`WhenItFails.Tests/Bootstrap/JsonsBootstrapperTemplateCollectionEnumerationExceptionContractTests.cs`

Contract:
`EnsureWorkspaceAsync_WhenTemplateCollectionEnumerationThrows_ReturnsStableProviderFailureWithoutExceptionDetail`

The provider call itself succeeds and returns an `IReadOnlyList<JsonsTemplateFile>`, but the returned collection throws `InvalidOperationException` from `GetEnumerator()`.

Require:

```text
Status: Failed
Data: null
Code: WIF_JSONS_TEMPLATE_PROVIDER_FAILED
Message: The JSON template provider failed.
```

The provider/collection exception detail must not escape into either the response message or issue message.

This is distinct from the already verified direct `GetTemplateFiles(...)` exception contract: the current `try/catch` ends before the returned collection is enumerated, so deferred provider-output failures can currently escape.

The focused contract confirmed the expected RED on Windows:

```text
System.InvalidOperationException:
Sensitive template collection enumeration detail must not escape.
```

The exception escaped from the provider collection's `GetEnumerator()` after `GetTemplateFiles(...)` itself had already returned successfully.

Production fix commit:
`f26676980a5654efb31f5e7ac0012942f9daa613`

Documentation commit:
`8f8ef9583dfc9e713381fcbddbf0d8febfd9be6a`

`WhenItFails/Docs/Bootstrap/en.md` now documents collection-consumption failure normalization and preservation of cancellation semantics.

The returned template collection is now materialized into a snapshot inside a narrow provider-boundary `try/catch`. Ordinary exceptions raised while consuming the collection map to:

```text
Status: Failed
Code: WIF_JSONS_TEMPLATE_PROVIDER_FAILED
Message: The JSON template provider failed.
```

`OperationCanceledException` remains excluded from normalization and continues to propagate unchanged. Template items are processed only after successful collection materialization, so an enumeration failure cannot cause partial template writes.

**Complete-suite 1072/1072 GREEN was subsequently confirmed locally by the maintainer.** The latest confirmation did not separately report the focused result, compiler-warning count, or operating system.

## 2026-09-21 — 1071/1071 GREEN whitespace template-name checkpoint

Contract commit: `b82283c81eef5af862194f5b5dde8547d8d542a7`

Production fix commit: `ded57fb513b614a5f0b3c5a0d3675400e86a2327`

Documentation commit: `e058dae8d0afce8fbf70e81f37a66864b08fe218`

Locally confirmed by the maintainer:

```text
WhenItFails.Tests
Failed:   0
Passed: 1071
Total:  1071
```

The latest confirmation did not separately report focused test output, compiler-warning count, or operating system.

Null and whitespace-only logical template names are now rejected before target validation and before any template file write.

Next provider boundary audit: failures that occur while enumerating the collection returned by `IJsonsTemplateProvider.GetTemplateFiles(...)`, after the provider call itself has already succeeded.

## 2026-09-21 — whitespace template name provider-output contract

Contract commit:
`b82283c81eef5af862194f5b5dde8547d8d542a7`

Baseline: **1070/1070 GREEN**, confirmed locally by the maintainer before this test was introduced.

Added:
`WhenItFails.Tests/Bootstrap/JsonsBootstrapperWhitespaceTemplateNameContractTests.cs`

Contract:
`EnsureWorkspaceAsync_WhenTemplateNameIsWhitespace_ReturnsInvalidBeforeWritingTemplateFile`

A malformed provider returns:

```csharp
Name = "   "
TargetFileName = "errors.en.json"
Content = "{}"
```

Require:

```text
Status: Invalid
Data: null
Code: WIF_JSONS_TEMPLATE_NAME_EMPTY
Message: The JSON template provider returned a template with an empty name.
```

The target file must not be written.

The focused contract confirmed the expected RED on Windows:

```text
Expected: Invalid
Actual:   Success
```

Current production created the template file and returned `Success` despite `Name = "   "`.

Production fix commit:
`ded57fb513b614a5f0b3c5a0d3675400e86a2327`

Documentation commit:
`e058dae8d0afce8fbf70e81f37a66864b08fe218`

`JsonsBootstrapper` now rejects whitespace-only logical names before target validation and before writing the target file:

```text
Status: Invalid
Code: WIF_JSONS_TEMPLATE_NAME_EMPTY
Message: The JSON template provider returned a template with an empty name.
```

`WhenItFails/Docs/Bootstrap/en.md` now documents both null and whitespace-only logical template names as invalid provider output.

**Complete-suite 1071/1071 GREEN was subsequently confirmed locally by the maintainer.** The latest confirmation did not separately report the focused result, compiler-warning count, or operating system.

## 2026-09-21 — 1070/1070 GREEN null template-name checkpoint

Contract commit: `d2ca307b856d0fb973f9f3f738a667617fb0c760`

Production fix commit: `25942620071fcbb28de37739096b167b46c93bf8`

Documentation commit: `1b5479baf216d1cc6a9f2892ae4a10aed7a57bd2`

Locally confirmed by the maintainer:

```text
WhenItFails.Tests
Failed:   0
Passed: 1070
Total:  1070
```

The latest confirmation did not separately report focused test output, compiler-warning count, or operating system.

Null logical template names are now rejected before target validation and before any template file write.

Next provider-output boundary: whitespace-only logical template names.

## 2026-09-21 — null template name provider-output contract

Contract commit:
`d2ca307b856d0fb973f9f3f738a667617fb0c760`

Baseline: **1069/1069 GREEN**, confirmed locally by the maintainer before this test was introduced.

Added:
`WhenItFails.Tests/Bootstrap/JsonsBootstrapperNullTemplateNameContractTests.cs`

Contract:
`EnsureWorkspaceAsync_WhenTemplateNameIsNull_ReturnsInvalidBeforeWritingTemplateFile`

A malformed provider returns:

```csharp
Name = null
TargetFileName = "errors.en.json"
Content = "{}"
```

Require:

```text
Status: Invalid
Data: null
Code: WIF_JSONS_TEMPLATE_NAME_NULL
Message: The JSON template provider returned a template with a null name.
```

The target file must not be written.

This closes a distinct provider-output gap: `JsonsTemplateFile.Name` and `JsonsBootstrapFileResult.Name` are public non-nullable strings, but current bootstrap code does not validate the logical name before creating the file and copying the value into the result.

The focused contract confirmed the expected RED on Windows:

```text
Expected: Invalid
Actual:   Success
```

Current production created the template file and returned `Success` despite `Name = null`.

Production fix commit:
`25942620071fcbb28de37739096b167b46c93bf8`

Documentation commit:
`1b5479baf216d1cc6a9f2892ae4a10aed7a57bd2`

`JsonsBootstrapper` now rejects a null logical template name before target validation and before writing the target file:

```text
Status: Invalid
Code: WIF_JSONS_TEMPLATE_NAME_NULL
Message: The JSON template provider returned a template with a null name.
```

`WhenItFails/Docs/Bootstrap/en.md` now documents logical template names as provider output and records that null names are rejected before template writes.

**Complete-suite 1070/1070 GREEN was subsequently confirmed locally by the maintainer.** The latest confirmation did not separately report the focused result, compiler-warning count, or operating system.

## 2026-09-21 — 1069/1069 GREEN nested template-target checkpoint

Contract commit: `60b790fa37b57e23d77b865e766a7d64b4fa8043`

Production fix commit: `eab18d989552581caeb24e1ad5cc7177747101a3`

Documentation commit: `cbba2875f8c0afac217ce891d739055fbc8b06be`

Locally confirmed by the maintainer:

```text
WhenItFails.Tests
Failed:   0
Passed: 1069
Total:  1069
```

The latest confirmation did not separately report focused test output, compiler-warning count, or operating system.

Valid nested provider targets are now supported: missing parent directories are created only for missing validated targets, while existing files remain preserved and skipped.

Next provider-output audit target: null logical template `Name`, which is copied into the public non-nullable `JsonsBootstrapFileResult.Name`.

## 2026-09-21 — valid nested template target contract

Contract commit:
`60b790fa37b57e23d77b865e766a7d64b4fa8043`

Baseline: **1068/1068 GREEN**, confirmed locally by the maintainer before this test was introduced.

Added:
`WhenItFails.Tests/Bootstrap/JsonsBootstrapperNestedTemplateTargetFileNameContractTests.cs`

Contract:
`EnsureWorkspaceAsync_WhenTemplateTargetIsNestedInsidePackage_CreatesParentDirectoryAndFile`

The provider returns a valid target inside the package workspace:

```text
Nested/errors.en.json
```

The contract requires successful bootstrap, creation of the missing `Nested` parent directory, creation of the target file with the provider content, and a successful `JsonsBootstrapFileResult`.

This is a positive path contract, not malformed-input normalization. The existing containment guard already classifies this target as inside the package. Current `EnsureTemplateFileAsync` writes directly to the nested target but does not create its parent directory, so the current implementation is expected to return `JsonsWorkspaceInputOutputError` through the outer `IOException` normalization.

The focused contract confirmed RED on Windows: the bootstrap response was not successful because the nested target parent directory did not exist and the write degraded into the existing workspace I/O failure path.

Production fix commit:
`eab18d989552581caeb24e1ad5cc7177747101a3`

Documentation commit:
`cbba2875f8c0afac217ce891d739055fbc8b06be`

`WhenItFails/Docs/Bootstrap/en.md` now documents valid nested template targets, automatic creation of missing parent directories, containment inside the package workspace, and preservation of existing files.

`EnsureTemplateFileAsync` now creates the missing parent directory for a validated target only when the target file itself does not already exist. Existing files still return immediately as `Skipped` and are never overwritten.

**Complete-suite 1069/1069 GREEN was subsequently confirmed locally by the maintainer.** The latest confirmation did not separately report the focused result, compiler-warning count, or operating system.

## 2026-09-21 — 1068/1068 GREEN malformed provider-target checkpoint

Contract commit: `aea40cc601b04f13fc9f69355e3d4a85cc52bbfb`

Production fix commit: `11409034e05e891863f7f17bc4d517ff2a79edfc`

Locally confirmed by the maintainer:

```text
WhenItFails.Tests
Failed:   0
Passed: 1068
Total:  1068
```

The latest confirmation did not separately report focused test output, compiler-warning count, or operating system.

Malformed provider target syntax now returns stable `Invalid` without changing the established null, whitespace, or outside-package target contracts.

Next audit target: valid nested template targets inside the package workspace. Containment permits them, but `EnsureTemplateFileAsync` currently writes directly to the nested target without creating its parent directory.

## 2026-09-21 — malformed template target filename path contract

Contract commit:
`aea40cc601b04f13fc9f69355e3d4a85cc52bbfb`

Baseline: **1067/1067 GREEN**, confirmed locally by the maintainer before this test was introduced.

Added:
`WhenItFails.Tests/Bootstrap/JsonsBootstrapperInvalidTemplateTargetFileNamePathContractTests.cs`

Contract:
`EnsureWorkspaceAsync_WhenTemplateTargetFileNameContainsNullCharacter_ReturnsInvalidBeforeWritingTemplateFiles`

The template provider returns an invalid first target:

```text
errors\0.en.json
```

followed by a valid sentinel target. Require:

```text
Status: Invalid
Data: null
Code: WIF_JSONS_TEMPLATE_TARGET_FILE_NAME_INVALID
Message: The JSON template provider returned a template with an invalid target file name.
```

The contract also requires that the later sentinel template file is not written. Unlike caller-configuration validation, the package workspace may already exist at this provider-output boundary because provider invocation occurs after workspace creation.

This remains distinct from the existing `WIF_JSONS_TEMPLATE_TARGET_FILE_NAME_OUTSIDE_PACKAGE` containment contract.

The focused contract confirmed the expected RED on Windows:

```text
System.ArgumentException: Null character in path.
```

The exception escaped from `Path.GetFullPath(...)` inside `IsPathInsideDirectory(...)` after workspace creation but before any template file was written.

Production fix commit:
`11409034e05e891863f7f17bc4d517ff2a79edfc`

Template-target containment evaluation is now isolated in a narrow `try/catch (ArgumentException)`. Malformed provider target syntax maps to:

```text
Status: Invalid
Code: WIF_JSONS_TEMPLATE_TARGET_FILE_NAME_INVALID
Message: The JSON template provider returned a template with an invalid target file name.
```

The existing null, whitespace, and outside-package target contracts remain unchanged.

**Complete-suite 1068/1068 GREEN was subsequently confirmed locally by the maintainer.** The latest confirmation did not separately report the focused result, compiler-warning count, or operating system.

## 2026-09-21 — 1067/1067 GREEN malformed caller-path checkpoint

Contract commit: `514339b43269b7d25584a851acfc7138fe15d48a`

Production fix commit: `32357c6033b8be4c6ba8247baf39d73f5c21b385`

Locally confirmed by the maintainer:

```text
WhenItFails.Tests
Failed:   0
Passed: 1067
Total:  1067
```

The latest confirmation did not separately report focused test output, compiler-warning count, or operating system.

Malformed-path syntax handling is now locally verified GREEN for:

- `RootDirectory`
- `PackageDirectoryName`
- `ErrorCatalogFileName`
- `CategoryCatalogFileName`
- `CodeGroupCatalogFileName`
- `OwnerCatalogFileName`
- `ProfilesFileName`

The next distinct boundary is malformed `TargetFileName` returned by `IJsonsTemplateProvider`; preserve the existing provider-target containment contract separately.

## 2026-09-21 — malformed profile-catalog filename path contract

Contract commit:
`514339b43269b7d25584a851acfc7138fe15d48a`

Baseline: **1066/1066 GREEN**, confirmed locally by the maintainer before this test was introduced.

Added:
`WhenItFails.Tests/Bootstrap/JsonsBootstrapperInvalidProfilesFileNamePathContractTests.cs`

Contract:
`EnsureWorkspaceAsync_WhenProfilesFileNameContainsNullCharacter_ReturnsInvalidBeforeProviderOrFilesystem`

With `ProfilesFileName = "profiles\0.en.json"`, require:

```text
Status: Invalid
Data: null
Code: WIF_JSONS_PROFILE_CATALOG_FILE_NAME_INVALID
Message: The profile catalog file name is invalid.
```

The contract also requires no template-provider invocation and no workspace-root creation.

The focused contract confirmed the expected RED on Windows:

```text
System.ArgumentException: Null character in path.
```

The exception escaped from `Path.GetFullPath(...)` inside `IsPathInsideDirectory(...)` before filesystem mutation or template-provider invocation.

Production fix commit:
`32357c6033b8be4c6ba8247baf39d73f5c21b385`

The profile-catalog containment evaluation is now isolated in a narrow `try/catch (ArgumentException)`. Malformed profile-catalog filename syntax maps to:

```text
Status: Invalid
Code: WIF_JSONS_PROFILE_CATALOG_FILE_NAME_INVALID
Message: The profile catalog file name is invalid.
```

A syntactically valid filename outside the package continues to use the separate `WIF_JSONS_PROFILE_CATALOG_FILE_NAME_OUTSIDE_PACKAGE` contract.

**Complete-suite 1067/1067 GREEN was subsequently confirmed locally by the maintainer.** The latest confirmation did not separately report the focused result, compiler-warning count, or operating system.

## 2026-09-21 — 1066/1066 GREEN malformed owner-catalog filename checkpoint

Contract commit: `b0084b060adb8234848c567f9018b004f0af1939`

Production fix commit: `a2157487af81894a62c8989339a93c516661a8cb`

Locally confirmed by the maintainer:

```text
WhenItFails.Tests
Failed:   0
Passed: 1066
Total:  1066
```

The latest confirmation did not separately report focused test output, compiler-warning count, or operating system. A syntactically malformed `OwnerCatalogFileName` now returns stable `Invalid` instead of leaking `ArgumentException`.

Next malformed-path boundary: `ProfilesFileName`.

## 2026-09-21 — malformed owner-catalog filename path contract

Contract commit:
`b0084b060adb8234848c567f9018b004f0af1939`

Baseline: **1065/1065 GREEN**, confirmed locally by the maintainer before this test was introduced.

Added:
`WhenItFails.Tests/Bootstrap/JsonsBootstrapperInvalidOwnerCatalogFileNamePathContractTests.cs`

Contract:
`EnsureWorkspaceAsync_WhenOwnerCatalogFileNameContainsNullCharacter_ReturnsInvalidBeforeProviderOrFilesystem`

With `OwnerCatalogFileName = "owners\0.en.json"`, require:

```text
Status: Invalid
Data: null
Code: WIF_JSONS_OWNER_CATALOG_FILE_NAME_INVALID
Message: The owner catalog file name is invalid.
```

The contract also requires no template-provider invocation and no workspace-root creation.

The focused contract confirmed the expected RED on Windows:

```text
System.ArgumentException: Null character in path.
```

The exception escaped from `Path.GetFullPath(...)` inside `IsPathInsideDirectory(...)` before filesystem mutation or template-provider invocation.

Production fix commit:
`a2157487af81894a62c8989339a93c516661a8cb`

The owner-catalog containment evaluation is now isolated in a narrow `try/catch (ArgumentException)`. Malformed owner-catalog filename syntax maps to:

```text
Status: Invalid
Code: WIF_JSONS_OWNER_CATALOG_FILE_NAME_INVALID
Message: The owner catalog file name is invalid.
```

A syntactically valid filename outside the package continues to use the separate `WIF_JSONS_OWNER_CATALOG_FILE_NAME_OUTSIDE_PACKAGE` contract.

**Complete-suite 1066/1066 GREEN was subsequently confirmed locally by the maintainer.** The latest confirmation did not separately report the focused result, compiler-warning count, or operating system.

## 2026-09-21 — 1065/1065 GREEN malformed code-group filename checkpoint

Contract commit: `7efc266266f15437f6c533814e8648d29d51e48d`

Production fix commit: `d1013def0656dde8493e907ec9bd6545b0cabbcd`

Locally confirmed by the maintainer:

```text
WhenItFails.Tests
Failed:   0
Passed: 1065
Total:  1065
```

The latest confirmation did not separately report focused test output, compiler-warning count, or operating system. A syntactically malformed `CodeGroupCatalogFileName` now returns stable `Invalid` instead of leaking `ArgumentException`.

Next malformed-path boundary: `OwnerCatalogFileName`.

## 2026-09-21 — malformed code-group-catalog filename path contract

Contract commit:
`7efc266266f15437f6c533814e8648d29d51e48d`

Baseline: **1064/1064 GREEN**, confirmed locally by the maintainer before this test was introduced.

Added:
`WhenItFails.Tests/Bootstrap/JsonsBootstrapperInvalidCodeGroupCatalogFileNamePathContractTests.cs`

Contract:
`EnsureWorkspaceAsync_WhenCodeGroupCatalogFileNameContainsNullCharacter_ReturnsInvalidBeforeProviderOrFilesystem`

With `CodeGroupCatalogFileName = "code-groups\0.en.json"`, require:

```text
Status: Invalid
Data: null
Code: WIF_JSONS_CODE_GROUP_CATALOG_FILE_NAME_INVALID
Message: The code group catalog file name is invalid.
```

The contract also requires no template-provider invocation and no workspace-root creation.

The focused contract confirmed the expected RED on Windows:

```text
System.ArgumentException: Null character in path.
```

The exception escaped from `Path.GetFullPath(...)` inside `IsPathInsideDirectory(...)` before filesystem mutation or template-provider invocation.

Production fix commit:
`d1013def0656dde8493e907ec9bd6545b0cabbcd`

The code-group catalog containment evaluation is now isolated in a narrow `try/catch (ArgumentException)`. Malformed code-group catalog filename syntax maps to:

```text
Status: Invalid
Code: WIF_JSONS_CODE_GROUP_CATALOG_FILE_NAME_INVALID
Message: The code group catalog file name is invalid.
```

A syntactically valid filename outside the package continues to use the separate `WIF_JSONS_CODE_GROUP_CATALOG_FILE_NAME_OUTSIDE_PACKAGE` contract.

**Complete-suite 1065/1065 GREEN was subsequently confirmed locally by the maintainer.** The latest confirmation did not separately report the focused result, compiler-warning count, or operating system.

## 2026-09-21 — 1064/1064 GREEN malformed category-catalog filename checkpoint

Contract commit: `f061fddb3aff140ecf40e41fe28ec538efef0c45`

Production fix commit: `a7e4add893b84b74347a6bd21e5c563a635b9c5c`

Locally confirmed by the maintainer:

```text
WhenItFails.Tests
Failed:   0
Passed: 1064
Total:  1064
```

The latest confirmation did not separately report focused test output, compiler-warning count, or operating system. A syntactically malformed `CategoryCatalogFileName` now returns stable `Invalid` instead of leaking `ArgumentException`.

Next malformed-path boundary: `CodeGroupCatalogFileName`.

## 2026-09-21 — malformed category-catalog filename path contract

Contract commit:
`f061fddb3aff140ecf40e41fe28ec538efef0c45`

Baseline: **1063/1063 GREEN**, confirmed locally by the maintainer before this test was introduced.

Added:
`WhenItFails.Tests/Bootstrap/JsonsBootstrapperInvalidCategoryCatalogFileNamePathContractTests.cs`

Contract:
`EnsureWorkspaceAsync_WhenCategoryCatalogFileNameContainsNullCharacter_ReturnsInvalidBeforeProviderOrFilesystem`

With `CategoryCatalogFileName = "categories\0.en.json"`, require:

```text
Status: Invalid
Data: null
Code: WIF_JSONS_CATEGORY_CATALOG_FILE_NAME_INVALID
Message: The category catalog file name is invalid.
```

The contract also requires no template-provider invocation and no workspace-root creation.

The focused contract confirmed the expected RED on Windows:

```text
System.ArgumentException: Null character in path.
```

The exception escaped from `Path.GetFullPath(...)` inside `IsPathInsideDirectory(...)` before filesystem mutation or template-provider invocation.

Production fix commit:
`a7e4add893b84b74347a6bd21e5c563a635b9c5c`

The category-catalog containment evaluation is now isolated in a narrow `try/catch (ArgumentException)`. Malformed category-catalog filename syntax maps to:

```text
Status: Invalid
Code: WIF_JSONS_CATEGORY_CATALOG_FILE_NAME_INVALID
Message: The category catalog file name is invalid.
```

A syntactically valid filename outside the package continues to use the separate `WIF_JSONS_CATEGORY_CATALOG_FILE_NAME_OUTSIDE_PACKAGE` contract.

**Complete-suite 1064/1064 GREEN was subsequently confirmed locally by the maintainer.** The latest confirmation did not separately report the focused result, compiler-warning count, or operating system.

## 2026-09-21 — 1063/1063 GREEN malformed error-catalog filename checkpoint

Contract commit: `bfdfefdb9e84b0574ff4fe233c104df481191996`

Production fix commit: `e3ea09bcfa07a4e6bfed38d84025abec121dbf7f`

Locally confirmed by the maintainer:

```text
WhenItFails.Tests
Failed:   0
Passed: 1063
Total:  1063
```

The latest confirmation did not separately report focused test output, compiler-warning count, or operating system. A syntactically malformed `ErrorCatalogFileName` now returns stable `Invalid` instead of leaking `ArgumentException`.

Next malformed-path boundary: `CategoryCatalogFileName`.

## 2026-09-21 — malformed error-catalog filename path contract

Contract commit:
`bfdfefdb9e84b0574ff4fe233c104df481191996`

Baseline: **1062/1062 GREEN**, confirmed locally by the maintainer before this test was introduced.

Added:
`WhenItFails.Tests/Bootstrap/JsonsBootstrapperInvalidErrorCatalogFileNamePathContractTests.cs`

Contract:
`EnsureWorkspaceAsync_WhenErrorCatalogFileNameContainsNullCharacter_ReturnsInvalidBeforeProviderOrFilesystem`

With `ErrorCatalogFileName = "errors\0.en.json"`, require:

```text
Status: Invalid
Data: null
Code: WIF_JSONS_ERROR_CATALOG_FILE_NAME_INVALID
Message: The error catalog file name is invalid.
```

The contract also requires no template-provider invocation and no workspace-root creation.

The focused contract confirmed the expected RED on Windows:

```text
System.ArgumentException: Null character in path.
```

The exception escaped from `Path.GetFullPath(...)` inside `IsPathInsideDirectory(...)` before any filesystem mutation or template-provider invocation.

Production fix commit:
`e3ea09bcfa07a4e6bfed38d84025abec121dbf7f`

The error-catalog containment evaluation is now isolated in a narrow `try/catch (ArgumentException)`. Malformed error-catalog filename syntax maps to:

```text
Status: Invalid
Code: WIF_JSONS_ERROR_CATALOG_FILE_NAME_INVALID
Message: The error catalog file name is invalid.
```

A syntactically valid filename outside the package continues to use the separate `WIF_JSONS_ERROR_CATALOG_FILE_NAME_OUTSIDE_PACKAGE` contract.

**Complete-suite 1063/1063 GREEN was subsequently confirmed locally by the maintainer.** The latest confirmation did not separately report the focused result, compiler-warning count, or operating system.

## 2026-09-21 — 1062/1062 GREEN malformed package-directory checkpoint

Contract commit: `8c63d3aee47df25d809d72c559efab16f4a32b87`

Production fix commit: `0200ee5b600ccce8f9fa16551fd2b29a04f3658a`

Locally confirmed by the maintainer:

```text
WhenItFails.Tests
Failed:   0
Passed: 1062
Total:  1062
```

The latest confirmation did not separately report focused test output, compiler-warning count, or operating system. A syntactically malformed `PackageDirectoryName` now returns stable `Invalid` instead of leaking `ArgumentException`.

Next malformed-path boundary: `ErrorCatalogFileName`.

## 2026-09-21 — malformed package-directory-name path contract

Contract commit:
`8c63d3aee47df25d809d72c559efab16f4a32b87`

Baseline: **1061/1061 GREEN**, confirmed locally by the maintainer before this test was introduced.

Added:
`WhenItFails.Tests/Bootstrap/JsonsBootstrapperInvalidPackageDirectoryNamePathContractTests.cs`

Contract:
`EnsureWorkspaceAsync_WhenPackageDirectoryNameContainsNullCharacter_ReturnsInvalidBeforeProviderOrFilesystem`

With `PackageDirectoryName = "When\0ItFails"`, require:

```text
Status: Invalid
Data: null
Code: WIF_JSONS_PACKAGE_DIRECTORY_NAME_INVALID
Message: The package directory name is invalid.
```

The contract also requires no template-provider invocation and no workspace-root creation.

The focused contract confirmed the expected RED on Windows:

```text
System.ArgumentException: Null character in path.
```

The exception escaped from `Path.GetFullPath(...)` inside `IsPathInsideDirectory(...)` before any filesystem mutation or template-provider invocation.

Production fix commit:
`0200ee5b600ccce8f9fa16551fd2b29a04f3658a`

The package containment evaluation is now isolated in a narrow `try/catch (ArgumentException)`. Malformed package-directory syntax maps to:

```text
Status: Invalid
Code: WIF_JSONS_PACKAGE_DIRECTORY_NAME_INVALID
Message: The package directory name is invalid.
```

A syntactically valid package path outside the root continues to use the separate `WIF_JSONS_PACKAGE_DIRECTORY_NAME_OUTSIDE_ROOT` contract.

**Complete-suite 1062/1062 GREEN was subsequently confirmed locally by the maintainer.** The latest confirmation did not separately report the focused result, compiler-warning count, or operating system.

## 2026-09-21 — 1061/1061 GREEN malformed root-directory checkpoint

Contract commit: `3cff3ca67386f04afbc8ea65d9a5c8380086518c`

Production fix commit: `bbdbfe3ad7479593153ebda6ebab060c0793a7bb`

Locally confirmed by the maintainer:

```text
WhenItFails.Tests
Failed:   0
Passed: 1061
Total:  1061
```

The latest confirmation did not separately report focused test output, compiler-warning count, or operating system. A syntactically malformed `RootDirectory` now returns stable `Invalid` instead of leaking `ArgumentException`.

Next caller-controlled malformed-path boundary: `PackageDirectoryName`.

## 2026-09-21 — malformed root-directory path contract

Contract commit:
`3cff3ca67386f04afbc8ea65d9a5c8380086518c`

Baseline: **1060/1060 GREEN**, confirmed locally by the maintainer before this test was introduced.

Added:
`WhenItFails.Tests/Bootstrap/JsonsBootstrapperInvalidRootDirectoryPathContractTests.cs`

Contract:
`EnsureWorkspaceAsync_WhenRootDirectoryContainsNullCharacter_ReturnsInvalidBeforeProviderOrFilesystem`

With a `RootDirectory` containing a null character, require:

```text
Status: Invalid
Data: null
Code: WIF_JSONS_ROOT_DIRECTORY_INVALID
Message: The JSON root directory path is invalid.
```

The contract also requires no template-provider invocation and no creation of the safe temporary parent directory. This aligns the bootstrap caller-configuration boundary with the existing loader/writer malformed-path behavior, while keeping bootstrap-specific error codes.

The focused contract confirmed the expected RED on Windows:

```text
System.ArgumentException: Null character in path.
```

The exception escaped from `Path.GetFullPath(...)` inside `IsPathInsideDirectory(...)` before any filesystem mutation or template-provider invocation.

Production fix commit:
`bbdbfe3ad7479593153ebda6ebab060c0793a7bb`

`JsonsBootstrapper` now validates the normalized `RootDirectory` with `Path.GetFullPath(...)` before containment evaluation and maps `ArgumentException` to:

```text
Status: Invalid
Code: WIF_JSONS_ROOT_DIRECTORY_INVALID
Message: The JSON root directory path is invalid.
```

No broader exception normalization was introduced.

**Complete-suite 1061/1061 GREEN was subsequently confirmed locally by the maintainer.** The latest confirmation did not separately report the focused result, compiler-warning count, or operating system.

## 2026-09-21 — 1060/1060 GREEN complete catalog-filename containment checkpoint

Contract commit: `f20a7041a37b9e0af2761d5817392076cd3fc242`

Production guard commit: `8182490e51f5d7e393e5efbcebed0d1014c4f439`

Locally confirmed by the maintainer:

```text
WhenItFails.Tests
Failed:   0
Passed: 1060
Total:  1060
```

The latest confirmation did not separately report focused test output, compiler-warning count, or operating system. All five caller-configured catalog filename fields now have locally verified null, whitespace, and package-containment guards before filesystem mutation and template-provider invocation.

Next audit target: syntactically malformed caller paths. `JsonCatalogDocumentLoader` and `JsonCatalogDocumentWriter` already normalize malformed paths to `Invalid`, while `JsonsBootstrapper` can currently allow `Path.GetFullPath(...)` argument failures to escape.

## 2026-09-21 — escaping profiles filename caller-configuration contract

Contract commit:
`f20a7041a37b9e0af2761d5817392076cd3fc242`

Baseline: **1059/1059 GREEN**, confirmed locally by the maintainer before this test was introduced.

Added:
`WhenItFails.Tests/Bootstrap/JsonsBootstrapperEscapingProfilesFileNameContractTests.cs`

Contract:
`EnsureWorkspaceAsync_WhenProfilesFileNameEscapesPackage_ReturnsInvalidBeforeProviderOrFilesystem`

With `ProfilesFileName = "../escaped.json"` and a tracking template provider returning no files, require:

```text
Status: Invalid
Data: null
Code: WIF_JSONS_PROFILE_CATALOG_FILE_NAME_OUTSIDE_PACKAGE
Message: The profile catalog file name must stay inside the package directory.
```

The contract also asserts no provider invocation, no workspace-root creation, and no escaped file. This is a proposed caller-configuration error code distinct from the established provider-target containment contract.

The focused test confirmed the expected RED on Windows:

```text
Expected: Invalid
Actual:   Success
```

Production guard commit:
`8182490e51f5d7e393e5efbcebed0d1014c4f439`

The bootstrapper now calls the existing `IsPathInsideDirectory` helper for caller-configured `ProfilesFileName` immediately after the owner-catalog filename check, before filesystem mutation or template-provider invocation. The established provider-target containment contract remains unchanged.

**Complete-suite 1060/1060 GREEN was subsequently confirmed locally by the maintainer.** The latest confirmation did not separately report the focused result, compiler-warning count, or operating system.

## 2026-09-21 — 1059/1059 GREEN escaping owner-catalog filename checkpoint

Contract commit: `ea94faccdab5ff65ae410c62d662cb37662fc8e6`

Production guard commit: `cbef653c0be0e1d4fa368d7bbe1f1f0fe04f344f`

Locally confirmed by the maintainer:

```text
WhenItFails.Tests
Failed:   0
Passed: 1059
Total:  1059
```

The latest confirmation did not separately report focused test output, compiler-warning count, or operating system. Caller-configured `OwnerCatalogFileName = "../escaped.json"` now returns `Invalid` before workspace creation or provider invocation. Next focused boundary: `ProfilesFileName` escaping its package directory.

## 2026-09-21 — escaping owner-catalog filename caller-configuration contract

Contract commit:
`ea94faccdab5ff65ae410c62d662cb37662fc8e6`

Baseline: **1058/1058 GREEN**, confirmed locally by the maintainer before this test was introduced.

Added:
`WhenItFails.Tests/Bootstrap/JsonsBootstrapperEscapingOwnerCatalogFileNameContractTests.cs`

Contract:
`EnsureWorkspaceAsync_WhenOwnerCatalogFileNameEscapesPackage_ReturnsInvalidBeforeProviderOrFilesystem`

With `OwnerCatalogFileName = "../escaped.json"` and a tracking template provider returning no files, require:

```text
Status: Invalid
Data: null
Code: WIF_JSONS_OWNER_CATALOG_FILE_NAME_OUTSIDE_PACKAGE
Message: The owner catalog file name must stay inside the package directory.
```

The contract also asserts no provider invocation, no workspace-root creation, and no escaped file. This is a proposed caller-configuration error code distinct from the established provider-target error contract.

The focused test confirmed the expected RED on Windows:

```text
Expected: Invalid
Actual:   Success
```

Production guard commit:
`cbef653c0be0e1d4fa368d7bbe1f1f0fe04f344f`

The bootstrapper now calls the existing `IsPathInsideDirectory` helper for caller-configured `OwnerCatalogFileName` immediately after the code-group catalog filename check, before filesystem mutation or template-provider invocation. The existing provider-target contract remains unchanged.

**Complete-suite 1059/1059 GREEN was subsequently confirmed locally by the maintainer.** The latest confirmation did not separately report the focused result, compiler-warning count, or operating system.

## 2026-09-21 — 1058/1058 GREEN escaping code-group-catalog filename checkpoint

Contract commit: `b95f1015af3e839fbc67f42294ed8a15c79c3a88`

Production guard commit: `afa88ba450f99da6aa66ba5178d712c31f65f8a0`

Locally confirmed by the maintainer:

```text
WhenItFails.Tests
Failed:   0
Passed: 1058
Total:  1058
```

The latest confirmation did not separately report focused test output, compiler-warning count, or operating system. Caller-configured `CodeGroupCatalogFileName = "../escaped.json"` now returns `Invalid` before workspace creation or provider invocation. Next boundary: `OwnerCatalogFileName` path escaping its package directory.

## 2026-09-21 — escaping code-group-catalog filename caller-configuration contract

Contract commit:
`b95f1015af3e839fbc67f42294ed8a15c79c3a88`

Baseline: **1057/1057 GREEN**, confirmed locally by the maintainer before this test was introduced.

Added:
`WhenItFails.Tests/Bootstrap/JsonsBootstrapperEscapingCodeGroupCatalogFileNameContractTests.cs`

Contract:
`EnsureWorkspaceAsync_WhenCodeGroupCatalogFileNameEscapesPackage_ReturnsInvalidBeforeProviderOrFilesystem`

With `CodeGroupCatalogFileName = "../escaped.json"` and a tracking template provider returning no files, require:

```text
Status: Invalid
Data: null
Code: WIF_JSONS_CODE_GROUP_CATALOG_FILE_NAME_OUTSIDE_PACKAGE
Message: The code group catalog file name must stay inside the package directory.
```

The contract also asserts no provider invocation, no workspace-root creation, and no escaped file. This is a proposed caller-configuration error code distinct from the existing provider-target error contract.

The focused contract confirmed the expected RED on Windows:

```text
Expected: Invalid
Actual:   Success
```

Production guard commit:
`afa88ba450f99da6aa66ba5178d712c31f65f8a0`

The bootstrapper now calls the existing `IsPathInsideDirectory` helper for caller-configured `CodeGroupCatalogFileName` immediately after the category-catalog filename check, before any filesystem mutation or template-provider invocation. Existing provider-target behavior remains unchanged.

**Complete-suite 1058/1058 GREEN was subsequently confirmed locally by the maintainer.** The latest confirmation did not separately report the focused result, compiler-warning count, or operating system.

## 2026-09-21 — 1057/1057 GREEN escaping category-catalog filename checkpoint

Contract commit: `0bee29d224d3edd467cc88e6746d4bb80dbde66c`

Production guard commit: `80645d303b55529df54a6d41ed06130b3783acd6`

Locally confirmed by the maintainer:

```text
WhenItFails.Tests
Failed:   0
Passed: 1057
Total:  1057
```

The latest confirmation did not separately report the focused test result, compiler-warning count, or operating system. Caller-configured `CategoryCatalogFileName = "../escaped.json"` now returns `Invalid` before workspace creation and provider invocation. Next focused boundary: `CodeGroupCatalogFileName` escaping its package directory.

## 2026-09-21 — escaping category-catalog filename caller-configuration contract

Contract commit:
`0bee29d224d3edd467cc88e6746d4bb80dbde66c`

Baseline: **1056/1056 GREEN**, confirmed locally by the maintainer before this test was added.

Added:
`WhenItFails.Tests/Bootstrap/JsonsBootstrapperEscapingCategoryCatalogFileNameContractTests.cs`

Contract:
`EnsureWorkspaceAsync_WhenCategoryCatalogFileNameEscapesPackage_ReturnsInvalidBeforeProviderOrFilesystem`

With `CategoryCatalogFileName = "../escaped.json"` and a tracking template provider returning no template files, require:

```text
Status: Invalid
Data: null
Code: WIF_JSONS_CATEGORY_CATALOG_FILE_NAME_OUTSIDE_PACKAGE
Message: The category catalog file name must stay inside the package directory.
```

The test also requires no provider invocation, no workspace-root creation, and no escaped file. This is a proposed caller-configuration code distinct from the existing provider-target error contract.

The focused test confirmed the expected RED on Windows:

```text
Expected: Invalid
Actual:   Success
```

Production guard commit:
`80645d303b55529df54a6d41ed06130b3783acd6`

The bootstrapper now calls the existing `IsPathInsideDirectory` helper for caller-configured `CategoryCatalogFileName` immediately after the existing error-catalog filename check, before any filesystem mutation or template-provider invocation. The existing provider-target error contract remains unchanged.

**Complete-suite 1057/1057 GREEN was subsequently confirmed locally by the maintainer.** The latest confirmation did not separately report the focused result, compiler-warning count, or operating system.

## 2026-09-21 — 1056/1056 GREEN escaping error-catalog filename checkpoint

Contract commit: `0ce2dd1e5f8b00a3b12be8f281b9b6a1e54987b8`

Production guard commit: `92a4f351641cba172bf66e3121f5065c1a27f151`

Locally confirmed by the maintainer:

```text
WhenItFails.Tests
Failed:   0
Passed: 1056
Total:  1056
```

The latest confirmation did not separately report the focused result, compiler-warning count, or operating system. Caller-configured `ErrorCatalogFileName = "../escaped.json"` now returns `Invalid` before workspace creation and provider invocation. Next focused boundary: caller-configured `CategoryCatalogFileName` escaping its package directory.

## 2026-09-21 — escaping error-catalog filename caller-configuration contract

Contract commit:
`0ce2dd1e5f8b00a3b12be8f281b9b6a1e54987b8`

Baseline: **1055/1055 GREEN**, confirmed locally by the maintainer before this test was added.

Added:
`WhenItFails.Tests/Bootstrap/JsonsBootstrapperEscapingErrorCatalogFileNameContractTests.cs`

Contract:
`EnsureWorkspaceAsync_WhenErrorCatalogFileNameEscapesPackage_ReturnsInvalidBeforeProviderOrFilesystem`

With `ErrorCatalogFileName = "../escaped.json"` and an empty tracking template provider, require:

```text
Status: Invalid
Data: null
Code: WIF_JSONS_ERROR_CATALOG_FILE_NAME_OUTSIDE_PACKAGE
Message: The error catalog file name must stay inside the package directory.
```

Also require no template-provider invocation, no workspace-root creation and no escaped file. This is a newly proposed caller-configuration error contract; the established `WIF_JSONS_TEMPLATE_TARGET_FILE_NAME_OUTSIDE_PACKAGE` provider-target contract remains distinct.

The focused contract confirmed the expected RED on Windows:

```text
Expected: Invalid
Actual:   Success
```

Production guard commit:
`92a4f351641cba172bf66e3121f5065c1a27f151`

After validating package-directory containment and computing its path, the bootstrapper calls the existing `IsPathInsideDirectory` helper on caller-configured `ErrorCatalogFileName` before any `Directory.Exists`, `Directory.CreateDirectory`, or template-provider call. The existing provider-target error contract remains unchanged.

**Full-suite 1056/1056 GREEN was subsequently confirmed locally by the maintainer.** The latest confirmation did not separately report focused test output, compiler-warning count, or operating system.

## 2026-09-21 — 1055/1055 GREEN nested package-directory checkpoint

Contract commit: `8fa2d9409256756e38e690225a1bcec05a1b023f`

Locally confirmed by the maintainer:

```text
WhenItFails.Tests
Failed:   0
Passed: 1055
Total:  1055
```

The latest confirmation did not separately report the focused result, compiler-warning count, or operating system. Legitimate nested package-directory configuration remains accepted with the root-containment guard. No production changes were needed.

Next: reject an escaping catalog filename supplied directly through caller options, before filesystem mutation or template-provider invocation.

## 2026-09-21 — valid nested package-directory regression contract

Contract commit:
`8fa2d9409256756e38e690225a1bcec05a1b023f`

Baseline: **1054/1054 GREEN**, confirmed locally by the maintainer before adding the regression.

Added:
`WhenItFails.Tests/Bootstrap/JsonsBootstrapperNestedPackageDirectoryNameContractTests.cs`

Contract:
`EnsureWorkspaceAsync_WhenPackageDirectoryNameIsNestedInsideRoot_CreatesWorkspace`

With `PackageDirectoryName = Path.Combine("Packages", "WhenItFails")` under a unique temporary `RootDirectory`, the bootstrapper must return `Success`, report the nested package directory as newly created, invoke the tracking template provider, and create that directory inside the root. The tracking provider returns an empty template list so the contract stays focused on directory containment and creation.

**Full-suite 1055/1055 GREEN was subsequently confirmed locally by the maintainer.** The latest confirmation did not separately report the focused result. No production change was made for this regression.

## 2026-09-21 — 1054/1054 GREEN package-directory escape checkpoint

Contract commit: `ccd5cdf9dbede67f95b0817f266d4445e17597c5`

Production guard commit: `0379b4ccb314264dab6069c3322a2188c28cfbbc`

Locally confirmed by the maintainer:

```text
WhenItFails.Tests
Failed:   0
Passed: 1054
Total:  1054
```

The latest confirmation did not specify operating system or compiler-warning count. The containment guard now rejects `PackageDirectoryName = "../escaped"` before filesystem mutation or template-provider invocation. Next: protect the legitimate nested-package case from regressions.

## 2026-09-21 — bootstrap package-directory escape contract

Contract commit: `ccd5cdf9dbede67f95b0817f266d4445e17597c5`

Baseline: **1053/1053 GREEN**, confirmed locally by the maintainer before this test.

Added:
`WhenItFails.Tests/Bootstrap/JsonsBootstrapperEscapingPackageDirectoryNameContractTests.cs`

Contract:
`EnsureWorkspaceAsync_WhenPackageDirectoryNameEscapesRoot_ReturnsInvalidBeforeProviderOrFilesystem`

Configured input: `PackageDirectoryName = "../escaped"` with a unique temporary `RootDirectory = <temp>/Jsons`. The test requires an invalid response with:

```text
Status: Invalid
Data: null
Code: WIF_JSONS_PACKAGE_DIRECTORY_NAME_OUTSIDE_ROOT
Message: The package directory name must stay inside the JSON root directory.
```

It also requires the template provider not to run and no root, escaped sibling directory, or unique temporary parent to be created. This is a new proposed bootstrap-specific contract, not a pre-existing error code of `ErrorCatalogContextProvider`.

The focused contract confirmed the expected RED on Windows:

```text
Expected: Invalid
Actual:   Success
```

Production guard commit:
`0379b4ccb314264dab6069c3322a2188c28cfbbc`

The guard calls the existing `IsPathInsideDirectory` helper on the normalized JSON root and package-directory name, inside the existing filesystem `try` block but before `Directory.Exists`, `Directory.CreateDirectory`, or template-provider invocation. It rejects a path resolving outside the root without changing the existing template target containment logic.

**Full-suite 1054/1054 GREEN was subsequently confirmed locally by the maintainer; the latest confirmation did not specify operating system or separate focused test output.**

## 2026-09-21 — 1053/1053 GREEN bootstrap whitespace profiles-file-name checkpoint

Contract commit: `aae834ba2fbac7b8729a64a108f08940ab07b452`

Production guard commit: `4318b3a9337f8b9cf40ab08e5cc733023a1d34ce`

Locally confirmed by the maintainer:

```text
WhenItFails.Tests
Failed:   0
Passed: 1053
Total:  1053
```

The compiler-warning count was not reported separately. All five catalog filename options now reject null and whitespace before workspace creation and template-provider invocation. Next focus: a package-directory path that escapes the configured JSON root.

## 2026-09-20 — bootstrap whitespace profiles-file-name contract

Contract commit:
`aae834ba2fbac7b8729a64a108f08940ab07b452`

Baseline: **1052/1052 GREEN**, confirmed locally by the maintainer before this test was introduced.

Added:
`WhenItFails.Tests/Bootstrap/JsonsBootstrapperWhitespaceProfilesFileNameContractTests.cs`

Contract:
`EnsureWorkspaceAsync_WhenProfilesFileNameIsWhitespace_ReturnsInvalidBeforeProviderOrFilesystem`

Expected stable response, matching the established `ErrorCatalogContextProvider` options contract:

```text
Status: Invalid
Data: null
Code: WIF_JSONS_PROFILE_CATALOG_FILE_NAME_EMPTY
Message: The profile catalog file name cannot be empty.
```

The test also requires no template-provider invocation and no workspace-root creation.

The focused run confirmed the expected RED:

```text
Expected: Invalid
Actual:   Success
```

Before the fix, whitespace `ProfilesFileName` reached workspace creation and template-provider invocation.

Production guard commit:
`4318b3a9337f8b9cf40ab08e5cc733023a1d34ce`

The narrow whitespace guard runs immediately after the existing null guard, before filesystem mutation and template-provider invocation.

**Focused and full-suite 1053/1053 GREEN were subsequently confirmed locally by the maintainer.**

## 2026-09-20 — 1052/1052 GREEN bootstrap null profiles-file-name checkpoint

Contract commit: `c7cf48d775df9c8136184481ff6e50cb7cfbcc98`

Production guard commit: `738b3677a5c9fbfbc46f64463f02fc13506ddcb5`

Locally confirmed by the maintainer:

```text
WhenItFails.Tests
Failed:   0
Passed: 1052
Total:  1052
```

The compiler-warning count was not reported separately. Null `ProfilesFileName` is rejected before workspace creation and template-provider invocation. Next contract: whitespace `ProfilesFileName`.

## 2026-09-20 — bootstrap null profiles-file-name contract

Contract commit:
`c7cf48d775df9c8136184481ff6e50cb7cfbcc98`

Baseline: **1051/1051 GREEN**, confirmed locally by the maintainer before this test was introduced.

Added:
`WhenItFails.Tests/Bootstrap/JsonsBootstrapperNullProfilesFileNameContractTests.cs`

Contract:
`EnsureWorkspaceAsync_WhenProfilesFileNameIsNull_ReturnsInvalidBeforeProviderOrFilesystem`

Expected stable response, matching the established `ErrorCatalogContextProvider` options contract:

```text
Status: Invalid
Data: null
Code: WIF_JSONS_PROFILE_CATALOG_FILE_NAME_NULL
Message: The profile catalog file name cannot be null.
```

The test also requires no template-provider invocation and no workspace-root creation.

The focused test confirmed the expected RED:

```text
Expected: Invalid
Actual:   Success
```

Before the fix, null `ProfilesFileName` passed through to workspace creation and template-provider invocation.

Production guard commit:
`738b3677a5c9fbfbc46f64463f02fc13506ddcb5`

The narrow null guard runs before filesystem mutation and template-provider invocation, after the established owner-catalog guards.

**Focused and full-suite 1052/1052 GREEN were subsequently confirmed locally by the maintainer.**

## 2026-09-20 — 1051/1051 GREEN bootstrap whitespace owner-catalog checkpoint

Contract commit: `9f30c19cbd8319a3f9b45d6dad74ace89918400e`

Production guard commit: `8a666aba9959233dc55bb45aaf93e9da9279f3df`

Locally confirmed by the maintainer:

```text
WhenItFails.Tests
Failed:   0
Passed: 1051
Total:  1051
```

The compiler-warning count was not reported separately. Both null and whitespace `OwnerCatalogFileName` are rejected before workspace creation and template-provider invocation. Next contract: null `ProfilesFileName`.

## 2026-09-20 — bootstrap whitespace owner-catalog-file-name contract

Contract commit:
`9f30c19cbd8319a3f9b45d6dad74ace89918400e`

Baseline: **1050/1050 GREEN**, confirmed locally by the maintainer before the new test was added.

Added:
`WhenItFails.Tests/Bootstrap/JsonsBootstrapperWhitespaceOwnerCatalogFileNameContractTests.cs`

Contract:
`EnsureWorkspaceAsync_WhenOwnerCatalogFileNameIsWhitespace_ReturnsInvalidBeforeProviderOrFilesystem`

Expected stable response, matching the established `ErrorCatalogContextProvider` option-level contract:

```text
Status: Invalid
Data: null
Code: WIF_JSONS_OWNER_CATALOG_FILE_NAME_EMPTY
Message: The owner catalog file name cannot be empty.
```

The test additionally requires that the template provider is not called and the workspace root is not created.

The focused test confirmed the expected RED:

```text
Expected: Invalid
Actual:   Success
```

Before the fix, whitespace `OwnerCatalogFileName` reached workspace creation and template-provider invocation.

Production guard commit:
`8a666aba9959233dc55bb45aaf93e9da9279f3df`

The narrow whitespace guard is placed immediately after the existing null guard, before filesystem mutation and template-provider invocation.

**Focused and full-suite 1051/1051 GREEN were subsequently confirmed locally by the maintainer.**

## 2026-09-20 — 1050/1050 GREEN bootstrap null owner-catalog checkpoint

Contract commit: `d72f135ea9e122e9be94e06545ce24e806cdde0b`

Production guard commit: `d0993d4c6cea0589a145986e87f27e5bea29ffdc`

Locally confirmed by the maintainer:

```text
WhenItFails.Tests
Failed:   0
Passed: 1050
Total:  1050
```

The compiler-warning count was not reported separately. Null `OwnerCatalogFileName` is rejected before workspace creation and template-provider invocation. Next: whitespace `OwnerCatalogFileName`.

## 2026-09-20 — bootstrap null owner-catalog-file-name contract

Contract commit:
`d72f135ea9e122e9be94e06545ce24e806cdde0b`

Baseline: **1049/1049 GREEN**, confirmed locally by the maintainer before this test was introduced.

Added:
`WhenItFails.Tests/Bootstrap/JsonsBootstrapperNullOwnerCatalogFileNameContractTests.cs`

Contract:
`EnsureWorkspaceAsync_WhenOwnerCatalogFileNameIsNull_ReturnsInvalidBeforeProviderOrFilesystem`

Expected stable response, matching the established `ErrorCatalogContextProvider` option contract:

```text
Status: Invalid
Data: null
Code: WIF_JSONS_OWNER_CATALOG_FILE_NAME_NULL
Message: The owner catalog file name cannot be null.
```

The contract additionally requires no template-provider invocation and no workspace-root creation.

The focused test confirmed the expected RED:

```text
Expected: Invalid
Actual:   Success
```

Before the fix, the invalid option passed into workspace creation and template-provider invocation.

Production guard commit:
`d0993d4c6cea0589a145986e87f27e5bea29ffdc`

The guard rejects null `OwnerCatalogFileName` before workspace creation or template-provider invocation.

**Focused and full-suite 1050/1050 GREEN were subsequently confirmed locally by the maintainer.**

## 2026-09-20 — 1049/1049 GREEN bootstrap whitespace code-group-catalog checkpoint

Contract commit: `2cc5ad94aae216c221456a4710bcca1c9472b16f`

Production guard commit: `6adbfb0b0b66e648bce2ef5a075cc066cd5fb944`

Locally confirmed by the maintainer:

```text
WhenItFails.Tests
Failed:   0
Passed: 1049
Total:  1049
```

The compiler-warning count was not reported separately. Null and whitespace `CodeGroupCatalogFileName` options are rejected before workspace creation and template-provider invocation. Next: null `OwnerCatalogFileName`.

## 2026-09-20 — bootstrap whitespace code-group-catalog-file-name contract

Contract commit:
`2cc5ad94aae216c221456a4710bcca1c9472b16f`

Baseline: **1048/1048 GREEN**, locally confirmed by the maintainer before this test was added.

Added:
`WhenItFails.Tests/Bootstrap/JsonsBootstrapperWhitespaceCodeGroupCatalogFileNameContractTests.cs`

Contract:
`EnsureWorkspaceAsync_WhenCodeGroupCatalogFileNameIsWhitespace_ReturnsInvalidBeforeProviderOrFilesystem`

Expected stable response, matching the established `ErrorCatalogContextProvider` option-level contract:

```text
Status: Invalid
Data: null
Code: WIF_JSONS_CODE_GROUP_CATALOG_FILE_NAME_EMPTY
Message: The code group catalog file name cannot be empty.
```

The test additionally checks that the template provider was not called and the workspace root was not created.

The focused run confirmed the expected RED:

```text
Expected: Invalid
Actual:   Success
```

The null guard alone allowed whitespace configuration through to workspace creation and the tracking template provider.

Production guard commit:
`6adbfb0b0b66e648bce2ef5a075cc066cd5fb944`

The new whitespace guard runs immediately after the existing null guard and before workspace creation or template-provider invocation.

**Focused and full-suite 1049/1049 GREEN were subsequently confirmed locally by the maintainer.**

## 2026-09-20 — 1048/1048 GREEN bootstrap null code-group-catalog checkpoint

Contract commit:
`01f9d68fd9fb975d59f6f1f0032408f0e6bf6b4e`

Production guard commit:
`50c39e1e4ec5349afe58005738b8dd30ed65402e`

Locally confirmed by the maintainer:

```text
WhenItFails.Tests
Failed:   0
Passed: 1048
Total:  1048
```

The compiler-warning count was not separately reported. Null `CodeGroupCatalogFileName` is rejected before workspace creation or template-provider invocation. Next contract: whitespace `CodeGroupCatalogFileName`.

## 2026-09-20 — bootstrap null code-group-catalog-file-name contract

Contract commit:
`01f9d68fd9fb975d59f6f1f0032408f0e6bf6b4e`

Baseline: **1047/1047 GREEN**, confirmed locally before this test was introduced.

Added `WhenItFails.Tests/Bootstrap/JsonsBootstrapperNullCodeGroupCatalogFileNameContractTests.cs` with contract
`EnsureWorkspaceAsync_WhenCodeGroupCatalogFileNameIsNull_ReturnsInvalidBeforeProviderOrFilesystem`.

Expected stable response:

```text
Status: Invalid
Data: null
Code: WIF_JSONS_CODE_GROUP_CATALOG_FILE_NAME_NULL
Message: The code group catalog file name cannot be null.
```

The contract additionally requires that the template provider is not invoked and the workspace root is not created.

The focused run confirmed the expected RED: `Expected: Invalid`, `Actual: Success`. Before the guard, malformed configuration could reach workspace creation and the tracking template provider.

Production guard commit:
`50c39e1e4ec5349afe58005738b8dd30ed65402e`

The guard rejects null `CodeGroupCatalogFileName` immediately after the category-file-name guards and before filesystem mutation or template-provider invocation.

**Focused GREEN and complete-suite 1048/1048 GREEN are pending maintainer verification.**

## 2026-09-20 — 1047/1047 GREEN bootstrap whitespace category-catalog checkpoint

Contract commit:
`9cac09bc34ec17ea4b5b969e3c08528e64651f1d`

Production guard commit:
`a7d60a23a485433361e92af14745c0b6fdb8c19d`

Locally confirmed by the maintainer:

```text
WhenItFails.Tests
Failed:   0
Passed: 1047
Total:  1047
```

The compiler-warning count was not reported separately. Both null and whitespace `CategoryCatalogFileName` inputs are rejected before workspace creation and template-provider invocation. Next contract: null `CodeGroupCatalogFileName`.

## 2026-09-20 — bootstrap whitespace category-catalog-file-name contract

Contract commit:
`9cac09bc34ec17ea4b5b969e3c08528e64651f1d`

Production guard commit:
`a7d60a23a485433361e92af14745c0b6fdb8c19d`

Previous locally confirmed checkpoint: **1046/1046 GREEN**.

Added contract:
`EnsureWorkspaceAsync_WhenCategoryCatalogFileNameIsWhitespace_ReturnsInvalidBeforeProviderOrFilesystem`

The focused run confirmed RED before the production fix:

```text
Expected: Invalid
Actual:   Success
```

The narrow guard rejects whitespace `CategoryCatalogFileName` after the existing null check and before filesystem mutation or template-provider invocation. Expected stable response:

```text
Status: Invalid
Data: null
Code: WIF_JSONS_CATEGORY_CATALOG_FILE_NAME_EMPTY
Message: The category catalog file name cannot be empty.
```

The contract also verifies that the template provider is not called and the workspace root is not created.

**Focused GREEN and complete-suite 1047/1047 GREEN are pending maintainer verification.**

## 2026-09-17 — bootstrap null error-catalog-file-name fix

Contract commit:
`91bba6f1c1cc11c2f7e6b12c5814a2afbb0ef64d`

Production guard commit:
`e3502565d5139132ce16947e7b0fbcb63e235e29`

Baseline checkpoint commit:
`7eaf2eda992ce59632dc086319e2a8c246858b8c`

Contract:

`EnsureWorkspaceAsync_WhenErrorCatalogFileNameIsNull_ReturnsInvalidBeforeProviderOrFilesystem`

`ErrorCatalogContextProvider.ValidateJsonsOptions(...)` defines the stable option-level contract:

```text
Status: Invalid
Data: null
Code: WIF_JSONS_ERROR_CATALOG_FILE_NAME_NULL
Message: The error catalog file name cannot be null.
```

The focused run confirmed the expected RED:

```text
Expected: Invalid
Actual:   Success
```

Before the fix, `JsonsBootstrapper` did not validate `ErrorCatalogFileName` before preparing the workspace. With the focused tracking provider, the malformed option therefore allowed workspace creation, provider invocation and a final `Success` response.

The production change is intentionally narrow: `options.ErrorCatalogFileName is null` is rejected after the already-established root/package option guards and before entering the filesystem block or invoking `IJsonsTemplateProvider.GetTemplateFiles(...)`.

The focused contract also protects both ordering guarantees:

```text
template provider invoked: false
workspace root created: false
```

Expected complete-suite result after verification: **1044/1044 GREEN, zero compiler warnings**.

## 2026-09-19 — bootstrap whitespace error-catalog-file-name contract

Contract commit:
`e58599d05993dac8af142a27325fa283b9a37e89`

Baseline checkpoint commit:
`b90d3c4563acae874f50c5b7aafad66ec216ade0`

Added:

`WhenItFails.Tests/Bootstrap/JsonsBootstrapperWhitespaceErrorCatalogFileNameContractTests.cs`

Contract:

`EnsureWorkspaceAsync_WhenErrorCatalogFileNameIsWhitespace_ReturnsInvalidBeforeProviderOrFilesystem`

Stable option-level response:

```text
Status: Invalid
Data: null
Code: WIF_JSONS_ERROR_CATALOG_FILE_NAME_EMPTY
Message: The error catalog file name cannot be empty.
```

The focused contract also requires:

```text
template provider invoked: false
workspace root created: false
```

Current production validates only the null form. A whitespace value is therefore expected to pass into the filesystem/provider path and, with the tracking provider returning an empty collection, end as `Success`.

The focused run confirmed the expected RED:

```text
Expected: Invalid
Actual:   Success
```

Before the fix, whitespace `ErrorCatalogFileName` passed through the option boundary, allowing workspace creation and template-provider invocation before returning `Success` with the tracking provider.

Production guard commit:
`6d60d321ea2e746e41b3ec0fe6dcb4c821727ab1`

The production change is intentionally narrow: whitespace `ErrorCatalogFileName` is rejected immediately after the existing null guard and before entering the filesystem block or invoking the template provider.

Expected complete-suite result after verification: **1045/1045 GREEN**.

## 2026-09-19 — bootstrap null category-catalog-file-name contract

Contract commit:
`0e55002be070cfbd237b3661b2464e3138c1521f`

Baseline checkpoint commit:
`e1e6a178d0f20e6e0be36c965f6eda6de23878ca`

Added:

`WhenItFails.Tests/Bootstrap/JsonsBootstrapperNullCategoryCatalogFileNameContractTests.cs`

Contract:

`EnsureWorkspaceAsync_WhenCategoryCatalogFileNameIsNull_ReturnsInvalidBeforeProviderOrFilesystem`

Stable option-level response:

```text
Status: Invalid
Data: null
Code: WIF_JSONS_CATEGORY_CATALOG_FILE_NAME_NULL
Message: The category catalog file name cannot be null.
```

The focused contract also requires:

```text
template provider invoked: false
workspace root created: false
```

Current production does not validate `CategoryCatalogFileName` before entering the filesystem/provider path. With the tracking provider returning an empty collection, current behavior is expected to create the workspace, invoke the provider and return `Success`.

The focused run confirmed the expected RED:

```text
Expected: Invalid
Actual:   Success
```

Before the fix, `CategoryCatalogFileName = null` passed through the option boundary, allowing workspace creation and template-provider invocation before returning `Success` with the tracking provider.

Production guard commit:
`75b2579c499002f1b073b7abf9cbf2f377cec196`

The production change is intentionally narrow: null `CategoryCatalogFileName` is rejected immediately after the established `ErrorCatalogFileName` guards and before entering the filesystem block or invoking the template provider.

Expected complete-suite result after verification: **1046/1046 GREEN**.

## 2026-09-19 — 1045/1045 GREEN bootstrap whitespace error-catalog-file-name checkpoint

Contract commit:
`e58599d05993dac8af142a27325fa283b9a37e89`

Production guard commit:
`6d60d321ea2e746e41b3ec0fe6dcb4c821727ab1`

Previous checkpoint commit:
`b90d3c4563acae874f50c5b7aafad66ec216ade0`

Locally verified:

```text
WhenItFails.Tests
Failed:   0
Passed: 1045
Skipped:  0
Total:  1045
```

The warning count was not separately included in the latest confirmation, so this checkpoint records the test result only.

`JsonsBootstrapper.EnsureWorkspaceAsync(...)` now rejects whitespace `ErrorCatalogFileName` before any filesystem mutation or template-provider invocation and returns:

```text
Status: Invalid
Data: null
Code: WIF_JSONS_ERROR_CATALOG_FILE_NAME_EMPTY
Message: The error catalog file name cannot be empty.
```

Together with the prior null contract, `ErrorCatalogFileName` option handling is complete for the current null/empty scope.

## 2026-09-19 — 1044/1044 GREEN bootstrap null error-catalog-file-name checkpoint

Contract commit:
`91bba6f1c1cc11c2f7e6b12c5814a2afbb0ef64d`

Production guard commit:
`e3502565d5139132ce16947e7b0fbcb63e235e29`

Previous checkpoint commit:
`7eaf2eda992ce59632dc086319e2a8c246858b8c`

Locally verified:

```text
WhenItFails.Tests
Failed:   0
Passed: 1044
Skipped:  0
Total:  1044
```

The warning count was not separately included in the latest confirmation, so this checkpoint records the test result only.

`JsonsBootstrapper.EnsureWorkspaceAsync(...)` now rejects `ErrorCatalogFileName = null` before any filesystem mutation or template-provider invocation and returns:

```text
Status: Invalid
Data: null
Code: WIF_JSONS_ERROR_CATALOG_FILE_NAME_NULL
Message: The error catalog file name cannot be null.
```

## 2026-09-17 — 1043/1043 GREEN bootstrap whitespace root-directory checkpoint

Contract commit:
`b35339be52737211d29516a66db57a43b359e7f5`

Production guard commit:
`3f5fcceafac6c7662822c49c3810d4dadd1a77c6`

Checkpoint commit:
`7eaf2eda992ce59632dc086319e2a8c246858b8c`

Locally verified:

```text
WhenItFails.Tests
Failed:   0
Passed: 1043
Skipped:  0
Total:  1043
Compiler warnings: 0
```

`JsonsBootstrapper.EnsureWorkspaceAsync(...)` rejects null/whitespace `RootDirectory` and `PackageDirectoryName` before filesystem mutation, aligning the directory-option shape with `ErrorCatalogContextProvider` for the current scope.

## Completed `JsonsOptions` filename boundary

The five configurable catalog filename options (`ErrorCatalogFileName`, `CategoryCatalogFileName`, `CodeGroupCatalogFileName`, `OwnerCatalogFileName`, `ProfilesFileName`) now have locally verified null/whitespace contracts in `JsonsBootstrapper`, aligned with `ErrorCatalogContextProvider` and enforced before provider invocation and filesystem mutation.

## Next configuration boundary

The package-directory containment guard and positive nested-directory regression are verified. For template outputs, `JsonsBootstrapper` already rejects a provider-supplied target escaping its package directory, but does so only after workspace creation and the provider call.

The five catalog filename options are validated early for null/whitespace but not yet for an escaping relative path. A tracking template provider returning no files can therefore allow an invalid caller-configured filename such as `ErrorCatalogFileName = "../escaped.json"` to produce a successful bootstrap. Test this caller-configuration boundary before making a narrow production change. Preserve the existing separate provider-target error contract.

## Established transparent lower boundary — do not normalize

`ErrorCatalogContextProvider.LoadFromJsonsAsync(...)` intentionally preserves exceptions and null-task behavior from its five internal catalog providers.

Relevant suites include:

- `WhenItFails.Tests/Catalog/ErrorCatalogContextProviderProviderExceptionPropagationTests.cs`
- `WhenItFails.Tests/Catalog/ErrorCatalogContextProviderExceptionShapeTests.cs`
- `WhenItFails.Tests/Catalog/ErrorCatalogContextProviderCancellationPropagationTests.cs`
- `WhenItFails.Tests/Catalog/ErrorCatalogContextProviderOwnerNullTaskTests.cs`
- `WhenItFails.Tests/Catalog/ErrorCatalogContextProviderProfileNullTaskTests.cs`

Do not replace those transparent contracts with normalization at that layer.

## Established documented behavior — preserve

`JsonCatalogDocumentLoader.InvalidJson` deliberately includes the JSON parser message. `WhenItFails/Docs/Loading-and-Normalization/en.md` documents this behavior; do not sanitize it as incidental hardening.

## Recent verified checkpoints

- 1041/1041 — bootstrap null package directory name rejected before filesystem mutation.
- 1042/1042 — bootstrap null root directory rejected before filesystem mutation; nullable-flow cleanup verified with zero compiler warnings.
- 1043/1043 — bootstrap whitespace root directory rejected before filesystem mutation.

## Recommended verification

Pull current `master` and run:

```powershell
dotnet test WhenItFails.Tests --filter "FullyQualifiedName~SaveToFileAsync_WhenCancelledDuringSerialization_PreservesExistingTargetWithoutBackupOrTemporaryFile"
dotnet test WhenItFails.Tests
```

Expected: **one focused GREEN** and **1127/1127 GREEN** for the complete suite. A zero-test filter match is not a valid checkpoint.

## Next recommended step

After **1127/1127 GREEN** is confirmed locally, record the writer cancellation-preservation checkpoint and continue auditing a distinct safe-write boundary. Preserve the established cancellation propagation and backup/no-overwrite guarantees.