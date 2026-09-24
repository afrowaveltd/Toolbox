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

## Still under review

The initial eight-type public API baseline is covered. The active-context mutability decision, nullable annotations, and the distinction between documented stable contracts and implementation details remain open before 1.0.

No production visibility, names, signatures, or runtime behavior have been changed by this checkpoint.

## Verification

The maintainer confirmed the first two entry-point contract tests and complete 1159/1159 suite GREEN after commit `f9065aee1322993942b6ae0ac50aecac3dcbb3b2`, and the next four error-model tests and complete 1163/1163 suite GREEN after commit `a6eaba33dad832f7a85d7168fbf0ff431979791c`.
