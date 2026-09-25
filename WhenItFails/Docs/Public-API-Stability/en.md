# Public API stability — core entry points

This document records the pre-1.0 public API review of the application-facing entry points. It is a review baseline, not a declaration that version 1.0 has shipped.

## Verified external package baseline

The separately restored NuGet package version 0.1.0 has been exercised by an external .NET 10 consumer through registration, service resolution, initialization, status inspection, and error descriptor resolution. A separate reflection inspection of that consumer recorded the signatures below.

## Stable-contract candidates

- `Afrowave.Toolbox.WhenItFails.Interfaces.IErrorCatalogRuntime`: nine declared methods, including both initialization overloads, reset, current context, status, three descriptor lookup methods, and profile resolution. Its existing `CancellationToken` arguments remain optional. Adding interface members is a compatibility decision because third-party implementations may exist.
- `Microsoft.Extensions.DependencyInjection.WhenItFailsServiceCollectionExtensions.AddWhenItFails`: four overloads accepting `IServiceCollection` alone or with `WhenItFailsOptions`, `IConfigurationSection`, or `Action<WhenItFailsOptions>`. Each overload returns `IServiceCollection`.

The source-level contract tests are in `WhenItFails.Tests/PublicApi/CoreEntryPointPublicApiContractTests.cs`. They check method counts, parameter and return types, optional cancellation-token parameters, and the DI extension-method shape. These tests do not replace the external NuGet consumer check.

## Error data model baseline (1163/1163 GREEN)

The next contract snapshot covers `ErrorDescriptor` (21 declared public properties; unsealed) and `ErrorDefinition` (16 declared public properties; sealed). Both have a public parameterless constructor and mutable model properties. String identity fields are initialized to empty strings, severity defaults to `Error`, and collections plus `MetadataBag` are initialized per instance.

Their JSON property names are explicit and case-sensitive. `ErrorDescriptor.Severity` serializes as `severity`, while `ErrorDefinition.DefaultSeverity` serializes as `defaultSeverity`. `ErrorDescriptor.Exception` is intentionally excluded from JSON; `MetadataBag` serializes as a plain JSON object and is round-trippable through its converter.

The focused review tests are in `WhenItFails.Tests/PublicApi/ErrorModelPublicApiContractTests.cs`. The maintainer confirmed all four focused tests and the complete 1163/1163 suite GREEN. The shape snapshot is not a blanket assertion that every mutable detail of these models is frozen for 1.0.

## Context and runtime status baseline (1165/1165 GREEN)

`ErrorCatalogContext` exposes seven public get/set properties including the runtime `IErrorCatalog`, normalized documents and cross-validation result. The context store publishes a reference atomically and gives callers that **same mutable object**; it does not deep-clone or enforce immutable catalog content. Treat `GetCurrentContext()` as access to shared active state, **not** a deep immutable snapshot. Mutating it may affect subsequent resolution. This is a pre-1.0 design boundary to decide explicitly, not a recommendation to mutate a live context.

`ErrorCatalogRuntimeStatus` has nine public `init` properties and two getter-only computed properties, `State` and `IsConsistent`. Its own fields are not publicly settable after initialization; it is recorded as a new status instance when activation succeeds. Its semantic state combinations already have dedicated runtime tests. The focused API shape tests live in `WhenItFails.Tests/PublicApi/RuntimeStatePublicApiContractTests.cs`.

Neither status nor context is declared to have a frozen JSON wire format by this baseline. The maintainer confirmed both focused tests and the complete 1165/1165 suite GREEN; no production changes have been made.

## Configuration baseline (1169/1169 GREEN)

`WhenItFailsOptions` has three public get/set properties (`Jsons`, `InitializationMode`, `HideRecoverableFailures`). Defaults: a separately created `JsonsOptions` per instance, initialization mode `Flexible`, and nullable recovery-hiding override `null`. `JsonsOptions` has seven public get/set path inputs, defaulting to the `Jsons/WhenItFails` workspace and its five published JSON filenames, plus six getter-only computed paths.

Computed paths call `Path.Combine` on their current inputs, respecting host-platform separators. The property getters construct paths; they do **not** validate their safety or existence. Validation is performed at the relevant workspace/bootstrap boundaries.

The explicit `AddWhenItFails(WhenItFailsOptions)` DI overload copies the outer options and all seven nested JSON path inputs into an independent registration-time snapshot. Subsequent mutations of the *source* options do not change that snapshot. The registered options object itself is still mutable; the snapshot is not a deep-immutable runtime configuration guarantee.

The four focused tests in `WhenItFails.Tests/PublicApi/ConfigurationPublicApiContractTests.cs` cover model shape, defaults, path recalculation, and the explicit-options DI snapshot. The maintainer confirmed all four focused tests and the complete 1169/1169 suite GREEN.

## First DI extension-point group (1173/1173 GREEN)

Three public interfaces are candidates for supported third-party extension points: `IJsonsTemplateProvider` (`GetTemplateFiles(JsonsOptions)`), `IErrorCatalogContextProvider` (`LoadFromJsonsAsync(JsonsOptions, CancellationToken = default)`), and `IErrorDescriptorService` (`FromId`, `FromName`, `FromCode`, each taking `ErrorCatalogContext?` and an identifier). The DI entry point currently registers default implementations with `TryAddSingleton`, allowing an earlier registration to take precedence.

`WhenItFails.Tests/PublicApi/FirstExtensionPointPublicApiContractTests.cs` snapshots the exact declared method signatures and checks that custom implementations registered before `AddWhenItFails()` remain the resolved services. This tests the **registration and signature** boundaries, not full semantic compatibility or error normalization of custom services. A public interface being replaceable does not itself imply that every implementation class is a supported extension API. The maintainer confirmed all four focused tests and the complete 1173/1173 suite GREEN.

## Second DI extension-point group (1177/1177 GREEN)

`IErrorCatalogInitializer` exposes `InitializeAsync(JsonsOptions, CancellationToken = default)` returning `Task<Response<ErrorCatalogInitializationPayload>>`. `IJsonsBootstrapper` exposes `EnsureWorkspaceAsync(JsonsOptions, CancellationToken = default)` returning `Task<Response<JsonsBootstrapPayload>>`. `IErrorCatalogContextStore` exposes getter-only `IsInitialized` and nullable `Current`, plus `GetCurrent()` and `Set(ErrorCatalogContext)`.

These services are registered using `TryAddSingleton` and can be replaced by a prior registration. `WhenItFails.Tests/PublicApi/SecondExtensionPointPublicApiContractTests.cs` checks signatures, optional token parameters, getter-only store properties, and that a test-only custom implementation of each interface survives DI registration with scope/build validation. It does **not** test a custom implementation's full runtime behavior, cancellation semantics, or deep immutability of a stored context. Those remain separate behavioral/compatibility questions. Maintainer confirmed all four focused tests and the complete 1177/1177 suite GREEN.

## Main catalog pipeline DI extension points (1181/1181 GREEN)

`IErrorCatalogLoader` exposes `LoadFromFileAsync(string, CancellationToken = default)` returning `Task<Response<ErrorCatalogDocument>>`. `IErrorCatalogFactory` exposes `Create(ErrorCatalogDocument)` returning the lookup interface `IErrorCatalog`. `IErrorCatalogProvider` exposes `LoadFromFileAsync(string, CancellationToken = default)` returning `Task<Response<ErrorCatalogProviderPayload>>`.

Each has exactly one declared method. The default DI registrations use `TryAddSingleton`. The four focused tests in `WhenItFails.Tests/PublicApi/CatalogPipelineExtensionPointPublicApiContractTests.cs` check the method signatures, optional cancellation tokens, and pre-registered custom implementation precedence with DI graph validation. They do not exercise a custom catalog pipeline end to end: the test-only factory intentionally throws if called. This baseline is not a promise that every alternative implementation handles filesystem failures or cancellation correctly.

`IErrorCatalog` is the separate indexed lookup contract and will receive its own shape review. No production implementation or public visibility changes were made. The maintainer confirmed all four focused tests and the complete 1181/1181 suite GREEN.

## Indexed lookup contract: IErrorCatalog (1184/1184 GREEN)

`IErrorCatalog` has ten declared public methods. Three single-item lookups (`FindById(string)`, `FindByCode(int)`, and `FindByName(string)`) return nullable `ErrorDefinition?`. `GetAll()` and the six classification searches (`FindByOwner`, `FindByCodePrefix`, `FindByCodeGroup`, `FindByCategory`, `FindBySubcategory`, `FindByTag`) return non-null `IReadOnlyList<ErrorDefinition>`. The read-only collection interface does not imply that the definitions it contains are deeply immutable.

The focused tests in `WhenItFails.Tests/PublicApi/ErrorCatalogLookupPublicApiContractTests.cs` snapshot the exact interface shape and its nullable-reference return annotations, then smoke-test the public `IErrorCatalogFactory` → `IErrorCatalog` path. Normalization, positive/negative filtering, category overlap, and tag lookup semantics already have detailed coverage in `WhenItFails.Tests/Catalog/ErrorCatalogTests.cs`.

`IErrorCatalog` is a stable application-facing lookup-contract candidate and the return type of a DI-replaceable factory; it is not directly registered as a service by `AddWhenItFails()`. The maintainer confirmed all three focused tests and the complete 1184/1184 suite GREEN. These tests do not freeze the concrete catalog implementation's constructor or internal indexes.

## Main catalog normalization and validation extension points (1188/1188 GREEN)

`IErrorCatalogDocumentNormalizer` has one method, `Normalize(ErrorCatalogDocument)` returning `ErrorCatalogDocument`. Its input and return annotations are non-nullable. The default implementation rejects a null document with `ArgumentNullException`, builds a new normalized document, and currently retains the original `MetadataBag` reference. **It must not be described as a guaranteed deep copy**.

`IErrorCatalogValidator` has one method, `Validate(ErrorCatalogDocument?)` returning non-null `ErrorCatalogValidationResult`. Null input is a supported validation case: the default implementation returns an invalid result containing an issue with code `CatalogDocumentIsNull`, instead of throwing due solely to the null document.

The four focused tests in `WhenItFails.Tests/PublicApi/CatalogNormalizationValidationPublicApiContractTests.cs` check exact interface methods, nullable-reference annotations, the default null-input distinction and pre-registered custom DI service precedence with service-graph validation. Detailed normalization and document-validation behavior already has dedicated tests. The maintainer confirmed all four focused tests and the complete 1188/1188 suite GREEN; no production code or public visibility has been changed.

## Specialized catalog loaders (1193/1193 GREEN)

The category, owner, code-group and profile catalog loaders implement four separate interfaces: `IErrorCategoryCatalogLoader`, `IErrorOwnerCatalogLoader`, `IErrorCodeGroupCatalogLoader` and `IErrorProfileCatalogLoader`. Each has one declared `LoadFromFileAsync(string, CancellationToken = default)` method returning `Task<Response<TCatalogDocument>>` with the corresponding category, owner, code-group or profile document type.

`WhenItFails.Tests/PublicApi/SpecializedCatalogLoaderPublicApiContractTests.cs` checks these public shapes and that four custom implementations registered before `AddWhenItFails()` remain selected by DI, including service-graph validation. These are registration and shape tests, not a claim of complete behavioral or cancellation equivalence for arbitrary replacement loaders. Specialized provider and validator interfaces remain to be reviewed separately. The maintainer confirmed all five focused tests and the complete 1193/1193 suite GREEN; no production code changed.

## Specialized catalog providers (1198/1198 GREEN)

The category, owner, code-group and profile catalog providers implement `IErrorCategoryCatalogProvider`, `IErrorOwnerCatalogProvider`, `IErrorCodeGroupCatalogProvider`, and `IErrorProfileCatalogProvider`. Each interface declares one `LoadFromFileAsync(string filePath, CancellationToken cancellationToken = default)` method returning `Task<Response<TPayload>>` for its own specialized `Error...CatalogProviderPayload` type.

`WhenItFails.Tests/PublicApi/SpecializedCatalogProviderPublicApiContractTests.cs` verifies exact CLR method and parameter shape, optional cancellation token and the corresponding payload type. One DI test confirms that four custom implementations registered before `AddWhenItFails()` remain selected with service-graph validation enabled. This is not a full behavioral test of replacement providers; existing provider tests remain responsible for default loading, normalization, validation and failure handling. The specialized validator group is next. The maintainer confirmed all five focused tests and the complete 1198/1198 suite GREEN after commit `51ad75815a8d2ae30efa3daaf0a00d1d33019000`; production code is unchanged.

## Specialized catalog validators (1203/1203 GREEN)

The category, owner, code-group and profile validators use four public interfaces: `IErrorCategoryCatalogValidator`, `IErrorOwnerCatalogValidator`, `IErrorCodeGroupCatalogValidator` and `IErrorProfileCatalogValidator`. Each declares one synchronous `ErrorCatalogValidationResult Validate(TCatalogDocument? document)` method. The input is annotated nullable and the return value non-nullable; the concrete document type differs for each catalog family.

`WhenItFails.Tests/PublicApi/SpecializedCatalogValidatorPublicApiContractTests.cs` checks all four exact public signatures and their nullable-reference annotations, plus preservation of pre-registered custom validator implementations in `AddWhenItFails()` with DI graph validation enabled. This verifies replaceability and public API shape, not the semantic correctness of third-party validators. The specialized loader, provider and validator groups now each have an API baseline, but full 1.0 stability scope and concrete implementation visibility decisions remain open. The maintainer confirmed all five focused tests and the complete 1203/1203 suite GREEN; production code remains unchanged.

## Definition and descriptor extension points (1208/1208 GREEN)

`IErrorDefinitionResolver` exposes `FindById`, `FindByName` and `FindByCode` returning `Response<ErrorDefinition>`. `IErrorDescriptorResolver` exposes `CreateById`, `CreateByName` and `CreateByCode` returning `Response<ErrorDescriptor>`. All six resolution methods accept nullable `ErrorCatalogContext?` and an ID/name string or numeric code. `IErrorDescriptorFactory` exposes `ErrorDescriptor Create(ErrorDefinition definition)` with non-nullable input and output annotations.

All three are DI-replaceable via `TryAddSingleton` registrations. `WhenItFails.Tests/PublicApi/DefinitionAndDescriptorExtensionPointPublicApiContractTests.cs` checks the seven exact public method signatures, relevant nullability annotations, and precedence of three pre-registered test-only implementations with service-graph validation. The test stubs do not exercise a fully working replacement resolution pipeline. The stable 1.0 behavior and concrete class visibility require further review; no production API has changed, and the maintainer confirmed all five focused tests and complete 1208/1208 suite GREEN.

## Profile resolution extension points (1212/1212 GREEN)

`IErrorProfileResolver.Resolve(ErrorCatalogDocument, ErrorProfileDefinition)` returns a non-null `IReadOnlyList<ErrorDefinition>` from non-null catalog/profile inputs. `IErrorProfileSelectionService.ResolveByProfileName(ErrorCatalogContext?, string)` returns a non-null `Response<IReadOnlyList<ErrorDefinition>>`; the catalog context input is annotated nullable. The two interfaces serve different layers: direct document/profile resolution versus lookup of a profile by name in an already loaded context.

Both default services are registered with `TryAddSingleton`. `WhenItFails.Tests/PublicApi/ProfileResolutionExtensionPointPublicApiContractTests.cs` checks their exact public signatures, relevant nullability annotations and preservation of pre-registered custom implementations with DI graph validation. These registration and shape tests do not establish behavioral equivalence of arbitrary custom profile resolvers. The remaining work for the 1.0 API review is inventory and classification of the other public types, dependent model surfaces and the shared active context mutation boundary. The maintainer confirmed all four focused tests and complete 1212/1212 suite GREEN; production code is unchanged.

## Public API inventory (source-audit checkpoint)

[Public API inventory and 1.0 decision register](../Public-API-Inventory/en.md) identifies transitive public models, the one remaining unreviewed built-in-provider interface, and concrete implementation/utility surfaces that need explicit compatibility decisions. This inventory adds no tests and does not change the existing 1212/1212 maintainer-confirmed suite baseline.

## Initialization and bootstrap payload models (1216/1216 GREEN)

Four focused contract tests in `WhenItFails.Tests/PublicApi/InitializationAndBootstrapPayloadPublicApiContractTests.cs` review `ErrorCatalogInitializationPayload`, `JsonsBootstrapPayload`, and `JsonsBootstrapFileResult`: constructor/property shape, defaults, nullable annotations, mutable per-instance file result lists, and derived degraded status. `Bootstrap` and `Context` in the initialization payload have non-nullable annotations but default to null until the producer assigns them; consumers must not interpret a manually constructed empty payload as a completed initialization. The maintainer confirmed all four focused tests and the complete 1216/1216 suite GREEN; production code is unchanged.

## Built-in catalog context provider (1218/1218 GREEN)

`IBuiltInErrorCatalogContextProvider` is the last interface in the current 31-file interface inventory to receive an individual baseline. It exposes one `LoadAsync(CancellationToken = default)` method returning `Task<Response<ErrorCatalogContext>>`. Its default implementation is registered through `TryAddSingleton`, so a prior application registration can replace it.

`WhenItFails.Tests/PublicApi/BuiltInCatalogContextProviderPublicApiContractTests.cs` verifies exact method shape and DI precedence without executing the default provider's temporary-workspace behavior. That behavior remains covered by its dedicated tests. With this addition, every interface source file under `WhenItFails/Interfaces/` has a first public-API shape review. The maintainer confirmed both focused tests and the complete 1218/1218 suite GREEN; production code remains unchanged.

## Catalog document JSON models (1222/1222 GREEN)

The five public catalog document types share a common catalog header and differ only in their typed content collection: `errors`, `categories`, `owners`, `codeGroups` or `profiles`. `WhenItFails.Tests/PublicApi/CatalogDocumentPublicApiContractTests.cs` snapshots their 11-property CLR shape, explicit JSON field names, constructor defaults, nullable annotations and per-instance mutable `Tags`, `Metadata` and content collections.

This baseline treats the existing JSON names and defaults as compatibility-sensitive inputs to the 1.0 decision, but it does not yet define a schema migration policy. In particular, a mutable collection property is not an immutable snapshot merely because a loader/provider returns the containing document. The maintainer confirmed all four focused tests and the complete 1222/1222 suite GREEN; production code is unchanged.

## Supporting definition JSON models (1225/1225 GREEN)

`ErrorCategoryDefinition`, `ErrorOwnerDefinition`, `ErrorCodeGroupDefinition` and `ErrorProfileDefinition` are transitive public JSON models used by the catalog documents and profile resolution API. `WhenItFails.Tests/PublicApi/SupportingDefinitionPublicApiContractTests.cs` records each model's exact public property types/accessors, explicit JSON field names and nullable-reference annotations.

Constructor defaults and independent mutable collections/dictionaries/metadata are already covered by the dedicated `WhenItFails.Tests/DefinitionContracts/*DefinitionContractTests.cs` tests, so the public-API suite deliberately does not repeat them. The maintainer confirmed all three new API tests and complete 1225/1225 suite GREEN; production code remains unchanged.

## Catalog provider payload models (1228/1228 GREEN)

The main `ErrorCatalogProviderPayload` has three publicly mutable properties: `Catalog: IErrorCatalog`, `Document: ErrorCatalogDocument`, and `ValidationResult: ErrorCatalogValidationResult`. The four specialized payloads each expose a corresponding typed `Document` and a `ValidationResult`. All five have a public parameterless constructor.

`WhenItFails.Tests/PublicApi/CatalogProviderPayloadPublicApiContractTests.cs` checks the exact property/accessor shape, non-nullable annotations, the present `null!` constructor state and reference-preserving assignment of document/validation fields. Non-nullable annotations do not make an empty, manually constructed payload valid; consumers should use the populated payload returned by a successful provider. The payload models currently do not declare explicit JSON property-name attributes, so this group is a CLR API baseline rather than a separate JSON wire-schema commitment. The maintainer confirmed all three focused tests and complete 1228/1228 suite GREEN; production code is unchanged.

## Catalog validation result/issue/severity models (1232/1232 GREEN)

`ErrorCatalogValidationResult` exposes getter-only `Issues: IReadOnlyList<ErrorCatalogValidationIssue>` and computed `IsValid: bool`, plus `AddIssue`, `AddError`, `AddWarning`, and `AddInformation`. The severity-specific helper methods each accept a code, message and three optional nullable details. `ErrorCatalogValidationIssue` has six get/set properties; `ErrorId`, `ErrorName`, and `Path` are annotated nullable. The `ErrorCatalogValidationSeverity` enum currently has exactly `Information = 0`, `Warning = 1`, `Error = 2`.

`WhenItFails.Tests/PublicApi/ValidationModelsPublicApiContractTests.cs` checks public CLR shape and nullability, exact enum values and recomputation of validity when an existing issue's severity changes. Existing tests already cover insertion, defaults, standard validity cases and the live issue-list view. Although `Issues` is exposed as an `IReadOnlyList`, its existing contents are mutable and `IsValid` is calculated from their current severity. **Do not treat validation results as immutable snapshots**. The maintainer confirmed all four focused tests and complete 1232/1232 suite GREEN; production code is unchanged.

## Bundled JSON template file model (1235/1235 GREEN)

`JsonsTemplateFile` is the public item model returned in `IJsonsTemplateProvider.GetTemplateFiles(JsonsOptions)`. Its current CLR contract consists of a public parameterless constructor and three public get/set non-nullable string properties: `Name`, `TargetFileName`, and `Content`, each initialized to `string.Empty`.

`WhenItFails.Tests/PublicApi/JsonsTemplateFilePublicApiContractTests.cs` checks this shape, defaults, independent assignments and the provider interface's typed collection return. The model does not declare explicit `JsonPropertyName` attributes; this review **does not** promise a separately versioned JSON wire schema for template file objects. The maintainer confirmed all three focused tests and the complete 1235/1235 suite GREEN; production code is unchanged.

## Compiled exported assembly inventory (verification pending)

`WhenItFails.Tests/PublicApi/ExportedAssemblyInventoryTests.cs` uses `Assembly.GetExportedTypes()` on the compiled WhenItFails project DLL. It can write a sorted Markdown inventory of the actually exported types, publicly declared constructors, methods, properties (including `init`), fields, events, enum numeric values, and base/interface relationships when `AFROWAVE_WHENITFAILS_PUBLIC_API_REPORT` is set. Without that environment variable, the test only checks the assembly and prints summary counts.

The report does not include all CLR metadata (for example complete generic constraints, nullability and custom attributes), and does not by itself establish cross-version binary compatibility, externally published NuGet contents, JSON schema stability or runtime behavior. The detailed contract tests already created remain authoritative for those focused facets. Do not make concrete-class visibility changes until this report and existing consumer usage are reviewed. The maintainer supplied the generated report: **110 exported types** in source-built assembly version **0.1.0.0**. The report generation is observed, but complete-suite verification after the new inventory test is still pending. See [Compiled public API review](../Public-API-Export-Review/en.md) for provisional 1.0 scope and concrete utility consumers.

## Setter standalone utility API baseline (1241/1241 GREEN; zero warnings)

`WhenItFails.Tests/PublicApi/SetterUtilityPublicApiContractTests.cs` adds five focused tests for standalone public `JsonCatalogDocumentWriter`, `DocumentationKeyGenerator`, `DocumentationKeyFormat`, and `ErrorCatalogCrossValidator`. The tests capture their declared public method/constructor shape and a no-DI, no-workspace smoke path. Setter consumes these types directly; they are **public utility candidates**, not automatically internal implementation details. Detailed functional behavior remains in their existing dedicated test suites. The maintainer confirmed all five focused tests GREEN, but reported three xUnit2031 analyzer warnings. They were corrected by using `Assert.Single(collection, predicate)` in all three locations. The maintainer subsequently confirmed a warning-free build, all five focused tests GREEN and complete **1241/1241 GREEN** suite after correction commit `6ecaa40b7c26261d8e253c4fb463d2c7b01e333b`; production code is unchanged.

## Standalone generic JSON loader (1244/1244 GREEN)

`JsonCatalogDocumentLoader` is publicly constructible and exposes one generic instance method, `LoadFromFileAsync<TDocument>(string filePath, CancellationToken cancellationToken = default)`, where `TDocument : class`, returning `Task<Response<TDocument>>`. The public contract permits direct use without DI. A cancelled token is checked before path validation in the current implementation.

`WhenItFails.Tests/PublicApi/JsonCatalogDocumentLoaderPublicApiContractTests.cs` adds three focused tests for the public CLR shape, no-DI empty-path response, and pre-cancelled token propagation without filesystem access. Existing `WhenItFails.Tests/Loading/JsonCatalogDocumentLoader*Tests.cs` cover file/JSON behavior; this new group intentionally does not duplicate them. The maintainer confirmed all three focused tests and complete **1244/1244 GREEN** suite. No production source, public visibility or distributed package has changed.

## Auxiliary descriptor models (1247/1247 GREEN)

`ErrorDescriptorRequest` is a public sealed model with eight mutable optional properties: seven `string?` values and nullable `int? Code`. Its C# type and nullability are part of the reviewed public surface; the model does not declare explicit JSON property-name attributes, so no separately versioned request JSON naming convention is promised here.

`ErrorDescriptor<TAttachment>` is a public sealed subclass of `ErrorDescriptor`, with one additional `TAttachment? Attachment` get/set property explicitly marked `[JsonPropertyName("attachment")]`. The inherited `Exception` is marked `[JsonIgnore]` to keep runtime exception objects out of JSON. `WhenItFails.Tests/PublicApi/DescriptorAuxiliaryModelsPublicApiContractTests.cs` snapshots the request's public shape/nullability, generic inheritance/property/attribute shape and a concrete typed JSON serialization case. Constructor defaults and attachment assignment semantics are already covered in dedicated descriptor tests. The maintainer confirmed three focused tests and the complete **1247/1247 GREEN** suite after commit `0d1c37e2eb4954ccea9d2ab1a6685ee5589d439a`; production code is unchanged.

## Active context: shared-reference contract (1250/1250 GREEN)

`ErrorCatalogContextStore.Set` atomically publishes the caller-supplied `ErrorCatalogContext` reference; `Current` and `GetCurrent()` return the **same instance**, not a defensive copy. `IErrorCatalogRuntime.GetCurrentContext()` forwards the context returned by the store. Both the publishing caller and anyone holding a returned context can modify its seven mutable properties and the objects reachable through them. Atomic reference replacement does not prevent in-place mutation or guarantee a consistent deep snapshot. A previously returned reference remains bound to the old context after replacement.

`ActiveContextSharedReferenceContractTests` adds three focused tests covering reader mutation visibility, publisher mutation visibility, and replacement without retargeting an earlier reference. These are regression observations of the existing 0.1.0 behavior, not an endorsement of concurrent mutation or a commitment to freeze mutable ownership as the 1.0 design. The maintainer confirmed three focused tests and complete **1250/1250 GREEN** suite. Warning count was not separately reported. No public signatures, production runtime behavior or package version changed.

**Compatibility direction:** preserve the existing `GetCurrentContext(): Response<ErrorCatalogContext>` signature for 0.1.0 consumers. Document the returned object as live and shared and instruct callers not to mutate an active context. If a safe consumer-facing view is needed for 1.0, evaluate an **additive** read-only/deep-snapshot API separately, with explicit ownership and nested collection/definition isolation; do not silently change this method to a shallow copy or claim that an interface typed as `IReadOnlyList<T>` implies deeply immutable items.

## Indexed catalog and nested definition ownership (1253/1253 GREEN)

The default `ErrorCatalogFactory` constructs `ErrorCatalog` from `ErrorCatalogDocument.Errors`. Its constructor copies list **membership** with `Array.AsReadOnly(errors.ToArray())`, but it does not clone the contained `ErrorDefinition` objects. Its identity and classification dictionaries index those same instances once at construction. An existing definition can therefore be mutated via the input document or a lookup result while the indexes retain their original keys; changing an `Id` or `Tags` after indexing does not rebuild the index. Adding an entry to the source document's `Errors` list does not add it to the existing indexed catalog, although mutations of earlier entries remain visible. `IReadOnlyList<ErrorDefinition>` only prevents direct edits to the returned collection interface, not to its elements.

`IndexedCatalogMutableDefinitionBoundaryTests` records these three distinct behaviors as a **pre-1.0 ownership hazard audit**, not a recommendation to mutate indexed definitions or a promise to preserve stale-index behavior in a future safe view. The safe-view design must decide whether to copy definitions and nested mutable values, and whether it may expose a lookup catalog that shares mutable source definitions. No production code, public signature or JSON schema changed. The maintainer confirmed the complete **1253/1253 GREEN** suite (zero failed or skipped); the three new tests are included in that full run. No compiler warnings were reported.

## Normalized document -> indexed catalog ownership (1256/1256 GREEN)

The default `ErrorCatalogDocumentNormalizer` creates a new document with normalized, newly allocated `ErrorDefinition` objects and tag lists, but reuses the **source document's `MetadataBag`**; `ErrorDefinitionNormalizer` likewise reuses each source definition's `MetadataBag`. `ErrorCatalogFactory` then copies the normalized document's error-list membership but retains those normalized definition instances in lookup indexes. Consequently, mutating the original definition's ID/title after normalization does not change the normalized indexed definition, while changing an indexed definition through the normalized document or lookup result remains visible through the other path and can leave lookup keys stale. Source metadata mutations remain visible across normalization and subsequent indexing despite the independent definition objects.

Three focused tests in `NormalizedCatalogReferenceOwnershipTests` record those concrete aliasing boundaries without modifying production code or promising that a future 1.0 safe-context view will retain them. They complement rather than replace existing unit tests for the normalizers' direct metadata aliases. The maintainer confirmed all tests GREEN, complete suite **1256/1256 GREEN**. Focused output and warning count were not separately reported. Before designing a safe view, separately review validation result and supporting catalog documents; shallow copying the main context alone cannot isolate nested metadata or indexed definition objects.

## Supporting catalogs and validation freshness (1259/1259 GREEN)

The default cross-validator produces an `ErrorCatalogValidationResult` from the input document state at validation time. It does not retain subscriptions or rerun validation if a category/profile document is subsequently mutated. The `IsValid` getter recomputes from the *stored mutable issue list*, not from the current supporting catalogs. A previously valid result may therefore remain `true` after the source catalog becomes invalid; conversely, mutating a published issue's `Severity` can flip `IsValid` without changing any catalog. `ErrorCatalogContext.CrossValidationResult` exposes the same mutable result instance when published in the default context store.

Supporting category/profile documents and their nested definitions expose mutable lists, dictionaries and metadata. In particular, `ErrorProfileCatalogDocument.Profiles` exposes `ErrorProfileDefinition` instances whose `IncludeTags`, `DefaultMappings` and `Metadata` can be modified by a reader of a published context. `SupportingCatalogLiveStateBoundaryTests` captures these three behaviors with focused tests: stale cross-validation after category mutation, mutable issue severity shared across readers, and nested profile collection/mapping/metadata aliases. The maintainer confirmed complete **1259/1259 GREEN**. The individual focused output and warning count were not separately reported. These tests document current ownership hazards rather than guaranteeing a future safe view preserves them; production code and the published API are unchanged.

## Detached main definition snapshot (1263/1263 GREEN)

An additive extension `GetErrorDefinitionSnapshots(this IErrorCatalogRuntime)` was introduced under `Afrowave.Toolbox.WhenItFails.Runtime`, with an independent sealed `ErrorDefinitionSnapshot` containing getter-only representations of the current 16 definition fields. The extension does **not** add any method to the existing nine-method runtime interface, nor does it change `GetCurrentContext()`. The returned list and all included category, subcategory, tag and metadata collections are privately copied and read-only; the projection does not retain `ErrorDefinition` or `MetadataBag` from the live catalog. A previously returned snapshot remains independent of later source edits or context replacement. The ordinary `Response<T>` envelope still has its existing mutable Essentials contract.

This is a **narrow main-definition projection**, not a deep snapshot of `ErrorCatalogContext`: supporting catalogs, validation findings and runtime status remain outside its scope. Capture is not transactional with respect to concurrent in-place mutation of live objects, and does not revalidate or rebuild the source indexes. `ErrorDefinitionSnapshotContractTests` adds four focused regression tests for detached lists/metadata and read-only accessors, independence across reactivation, invalid context failure propagation, and missing-catalog detection. The maintainer confirmed **1263/1263 GREEN** for the full suite; compiler warning count and focused run were not separately reported. See [detached definition snapshots](../Definition-Snapshots/en.md). The two new public types are pre-1.0 API candidates, not a finalized version-1.0 compatibility promise.

## Detached definition snapshot CLR/JSON boundary (1267/1267 GREEN)

`ErrorDefinitionSnapshotPublicSurfaceTests` introduces four focused checks covering its 16 getter-only properties, nullable-reference metadata and non-public construction; the additive static extension signature without altering `IErrorCatalogRuntime`; observed `System.Text.Json` serialization of an already captured detached snapshot; and stable failure without exception detail when source metadata is unexpectedly null. JSON property names observed under the default serializer are a **baseline, not a versioned 1.0 wire-schema guarantee**. No claim of direct JSON deserialization into the getter-only projection is made. The data projection excludes active context objects; the `Response<T>` envelope is still the ordinary mutable Essentials model. No production code, existing public signatures, package version or catalog JSON schema changed. The maintainer confirmed all tests GREEN, complete **1267/1267 GREEN**. Focused output and compiler warning count were not separately reported. See [definition snapshot documentation](../Definition-Snapshots/en.md).

## Detached cross-validation projection (1272/1272 GREEN)

The additive `ErrorCatalogValidationSnapshotExtensions.GetCrossValidationSnapshot(this IErrorCatalogRuntime)` returns a detached `ErrorCatalogValidationSnapshot` without extending the existing runtime interface. The projection copies each `ErrorCatalogValidationIssue` into a sealed, getter-only `ErrorCatalogValidationIssueSnapshot` and exposes an independent read-only issue list. `IsValid` is captured from copied issue severities and cannot change after live issue mutation. This captures **existing validation findings**, not a new validation of subsequently mutated source catalogs. It cannot guarantee atomic consistency if external code concurrently edits the live issue graph during capture; separate definition and validation snapshot calls are not one coherent multi-catalog transaction. `Response<T>` remains the ordinary mutable Essentials envelope.

Five focused contract tests in `ErrorCatalogValidationSnapshotContractTests` cover detached issue/value ownership, empty vs error validity, uninitialized runtime response propagation, missing result failure and getter-only projection shapes. The maintainer confirmed all tests GREEN, complete **1272/1272 GREEN**. Focused output and compiler warning count were not separately reported. See [validation snapshot documentation](../Validation-Snapshots/en.md). No changes to existing runtime interface methods, published package version or JSON schema.

## Detached supporting category projection (1278/1278 GREEN)

Added `ErrorCategoryCatalogSnapshot` (all 11 current category-document fields), `ErrorCategoryDefinitionSnapshot` (all eight current category-definition fields), and the additive `GetCategoryCatalogSnapshot(this IErrorCatalogRuntime)` extension. The snapshot types are sealed and getter-only; nested tags, category definitions, aliases, parent categories, default tags, mappings and metadata are copied into independent read-only collections. No live supporting document, definition or `MetadataBag` is exposed through the returned data. The extension takes a single context response and handles unavailable context, missing category catalog and ordinary capture failures without leaking source objects or exception detail. Six focused contract tests in `ErrorCategoryCatalogSnapshotContractTests` and the full **1278/1278 GREEN** suite were confirmed locally by the maintainer. Focused output and compiler warning count were not separately reported. There is no change to the nine-method `IErrorCatalogRuntime` interface or the existing published 0.1.0 package.

**Context identity boundary:** independently called category, definition and validation snapshot extensions may capture different context activations. A proposed combined capture would derive several projections from one selected context reference, but would not by itself provide a stable activation-generation ID or a transaction against in-place mutation. A real generation ID must be assigned by the runtime's activation/publishing lifecycle, not synthesized independently for each snapshot. See [category snapshot documentation](../Category-Snapshots/en.md). A combined capture and activation identity remain future design steps, not guarantees of this method.

## Combined selected-reference snapshot (1286/1286 GREEN)

`ErrorCatalogCombinedSnapshotExtensions.GetCombinedSnapshot(this IErrorCatalogRuntime)` is an additive extension that calls `GetCurrentContext()` **once** and derives the existing detached definition, category and recorded-validation projections from the **same selected context reference**. Its sealed, getter-only `ErrorCatalogCombinedSnapshot` has three properties: `Definitions`, `CategoryCatalog`, and `Validation`. It does not add a runtime-interface method, alter the existing individual snapshot methods or publish a context-generation ID. Missing required source components produce stable Invalid failures without partial snapshot data; ordinary capture exceptions produce a stable Failed result without exception text. `ErrorCatalogCombinedSnapshotContractTests` adds **eight test cases** (five facts and one three-case theory) for deep data detachment, one-read context selection across replacement, uninitialized response, missing components, malformed nested data, and public CLR shape. The maintainer confirmed the complete **1286/1286 GREEN** suite. Focused run details and compiler warning count were not separately reported.

**Selected-reference consistency is not activation-generation identity.** Captures of the same context reference can still differ if third-party callers mutate that published context in place, and a separate `GetStatus()` read is not atomically paired with the combined capture. Owners, code groups, profiles, and runtime status are not included. A durable activation-generation ID requires separate lifecycle design. See [combined snapshot documentation](../Combined-Snapshots/en.md). The published 0.1.0 package and persistent JSON schemas remain unchanged.

## Store-scoped context publication identity (1293/1293 GREEN)

The default `ErrorCatalogContextStore` now implements the optional `IErrorCatalogContextPublicationReader`, which returns an immutable `ErrorCatalogContextPublication` record containing `StoreId`, a monotonically increasing `Generation` and the **live** `Context` reference. `Set` uses an atomic compare/exchange loop over the whole record, preserving publication order with concurrent writers. Its old `IErrorCatalogContextStore` interface and methods remain unchanged. Every successful `Set` advances generation, including publishing the same reference twice; failed `Set(null)` does not. Distinct store instances have independent generation scopes and store IDs. The new record is an **infrastructure boundary, not a safe consumer projection**. Seven focused tests in `ContextPublicationGenerationContractTests` cover pre-initialization, sequential/repeated publications, failed Set, concurrent writes, independent stores and public/interface shape. The maintainer confirmed the complete **1293/1293 GREEN** suite. Focused test output and compiler warning count were not separately reported.

**Runtime activation identity is still open:** the initializer publishes before runtime status is recorded; automatic fallback/reset publish new contexts, while retained-previous-context recovery changes status without replacing the context. Direct store callers can publish independently of runtime status. The store publication identity therefore cannot yet be claimed to be an atomic context-and-status activation identity, and existing `GetCombinedSnapshot()` does not expose it. Any future integration needs an explicit consistent context/status read and a custom-store compatibility policy. See [publication identity documentation](../Context-Publication/en.md). Package version and persisted catalog JSON schema remain unchanged.

## Publication-aware detached combined snapshot (1300/1300 GREEN)

Added optional `IErrorCatalogRuntimePublicationReader` to the default `ErrorCatalogRuntime`, forwarding `GetCurrentPublication()` to the injected store only if it implements the optional store reader; unsupported custom stores return NotSupported. Added getter-only `ErrorCatalogPublishedCombinedSnapshot` (`StoreId`, `Generation`, detached `Snapshot`) and the additive `GetPublishedCombinedSnapshot(this IErrorCatalogRuntime)` extension. The latter reads exactly one **real store publication** and copies the existing three-part combined projection from its selected context; it never invokes a second context or status read and never invents generation IDs for unsupported custom runtimes/stores. The original `GetCombinedSnapshot()` was refactored to share an internal projection helper but retains its public signature and observed failure codes.

Seven focused tests in `PublishedCombinedSnapshotContractTests` cover the real default runtime/store path, selection across replacement, unsupported custom runtime/store, uninitialized response, missing category and getter-only public shape. The first build reported two CS7036 errors because `Response<T>.NotSupported` requires `data`; both call sites were corrected with `data: null`. The maintainer subsequently confirmed the complete **1300/1300 GREEN** suite. Focused output and compiler warning count were not separately reported. Documented in [publication-aware snapshots](../Published-Snapshots/en.md). This is a **store publication ID, not a synchronized runtime status/activation ID**. No existing runtime/store interface methods, published package version or persisted catalog schema changed.

## Context publication versus runtime status lifecycle (1306/1306 GREEN)

Six focused tests in `ContextPublicationStatusLifecycleContractTests` use the real publication-aware store and a controlled initializer/provider to record: normal project activation, strict failure preservation, flexible previous-context recovery without a new publication, built-in fallback followed by explicit reset, failed reset preservation, and a deterministic point **after a new context publication but before the runtime writes its corresponding status**. That interval can expose new catalog generation with prior status. Conversely, previous-context recovery changes status while generation remains unchanged. This is a pre-1.0 **consistency hazard baseline**, not a new synchronized activation API. No production code, public signature or JSON schema changed in this step. The maintainer confirmed the complete **1306/1306 GREEN** suite. Focused run output and compiler warning count were not separately reported. See [publication identity documentation](../Context-Publication/en.md).

## Completed runtime status observation (1314/1314 GREEN)

Added `ErrorCatalogActivationStatusSnapshot` and optional `IErrorCatalogRuntimeActivationReader.GetCompletedActivation()` to the default runtime without extending `IErrorCatalogRuntime`. Each matched completed status record retains a real store publication and the corresponding recorded status. The public getter-only projection exposes `StoreId`, `Generation`, a separate runtime-local `ActivationSequence`, and `Status`, but no live context. Normal activation advances both counters; flexible previous-context recovery advances only the status-observation sequence. Failed strict initialization/reset leave the preceding completion intact. A direct external `Set` invalidates the prior observation when the selected publication has changed, including republishing the same context reference. Legacy custom stores return NotSupported instead of synthetic identity. Eight focused cases in `CompletedActivationStatusContractTests` are included in the maintainer-confirmed complete **1314/1314 GREEN** suite. Focused output and compiler warning count were not separately reported. See [completed activation status documentation](../Activation-Status/en.md).

**Limits:** The associated status and publication are a selected, completed **observation**, not a continuously synchronized context/status transaction; separate `GetStatus()` and `GetPublishedCombinedSnapshot()` calls remain unpaired. External writes immediately after the check and overlapping initializations (especially re-publishing the same context object) require a future lifecycle ownership/serialization design for any stronger guarantee. Existing public interfaces, NuGet package version and persistent catalog JSON schema are unchanged.

## Serialized activation on one default runtime (verification pending)

The default `ErrorCatalogRuntime` now uses a private instance-local `SemaphoreSlim` gate shared by both `InitializeAsync` overloads and `ResetToDefaultsAsync`. Each operation holds the gate through its underlying initializer/provider, possible context publication, recovery and status recording, releasing it in `finally`. A queued request honors cancellation without entering the initializer or provider. Five focused cases in `RuntimeActivationSerializationContractTests` check concurrent initialization ordering, reset-before-initialize, initialize-before-reset, cancellation of a queued initialization, and release after a strict initialization failure. Expected complete **1319/1319 GREEN** if all pass; local verification pending. This changes runtime **operation ordering** but not existing public interface signatures, package version or JSON schema.

**Scope:** readers do not take the gate; direct store writers, custom runtimes and other instances sharing the same store are not covered. The context publication-before-status observation window still exists, and independent reads do not become a transaction. A stronger guarantee across all writers requires an owned store publication protocol; this step only prevents operations on one default runtime instance from overtaking one another. The gate is not reentrant; initializer/provider implementations must not synchronously or asynchronously await activation of the same runtime instance while holding it. See [initialization and recovery](../Runtime/Initialization-and-Recovery.md) and [activation status](../Activation-Status/en.md).

## Still under review

The initial eight-type public API baseline is covered. The shape and ownership of a **complete** safe context view, nullable annotations, and the distinction between documented stable contracts and implementation details remain open before 1.0.

The already published 0.1.0 NuGet package has not been changed by this source-development checkpoint. The current source includes additive public API candidates and the instance-local activation ordering change described above.

## Verification

The maintainer confirmed the first two entry-point contract tests and complete 1159/1159 suite GREEN after commit `f9065aee1322993942b6ae0ac50aecac3dcbb3b2`, and the next four error-model tests and complete 1163/1163 suite GREEN after commit `a6eaba33dad832f7a85d7168fbf0ff431979791c`.
