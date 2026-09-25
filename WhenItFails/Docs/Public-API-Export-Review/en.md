# Compiled public API review — WhenItFails 0.1.0

Status: **provisional classification for 1.0 planning, not a frozen compatibility guarantee**.
Review date: 2026-09-25.
Source: maintainer-generated `WhenItFails-public-api.md` from `ExportedAssemblyInventoryTests` on the source-built `Afrowave.Toolbox.WhenItFails` assembly, reporting assembly version `0.1.0.0` and **110 exported types**. The report lists *declared* public members of exported types. It is not a metadata/ABI diff against the published NuGet package.

The report was supplied for review after commit `955f5f897be96ec41e77ea34ab3ec482043ccf06`. The maintainer confirmed the inventory test and complete **1236/1236 GREEN** `WhenItFails.Tests` suite after commit `955f5f897be96ec41e77ea34ab3ec482043ccf06`. The source-built report still does **not** establish binary identity with the separately published NuGet package.

## 1. Classification: separate promises from visibility

The three categories below describe *intended use* and possible 1.0 policy, **not** the current CLR accessibility. All named concrete classes in this review are already publicly exported in 0.1.0. A label such as “public implementation detail” does not make a public type internal or permit breaking existing consumers without assessing compatibility.

### A. Application-facing API — stable-contract candidates

- `Microsoft.Extensions.DependencyInjection.WhenItFailsServiceCollectionExtensions.AddWhenItFails(...)`: all four public DI overloads.
- `IErrorCatalogRuntime` and its returned `Response<T>` values; `IErrorCatalog` for indexed lookup.
- Configuration: `WhenItFailsOptions`, `JsonsOptions`, `ErrorCatalogInitializationMode`.
- Core models: `ErrorDefinition`, `ErrorDescriptor`, `ErrorCatalogContext`, `ErrorCatalogRuntimeStatus`, `ErrorCatalogInitializationPayload`, and `ErrorCatalogContextSource` / `ErrorCatalogRuntimeState`.
- Public objects reachable via these signatures: five catalog documents; category/owner/code-group/profile definitions; bootstrap payload and file result; provider payloads; validation result/issue/severity and `JsonsTemplateFile` when returned by supported extension interfaces.
- Shared Essentials types exposed in signatures (notably `Response<T>`, `ResultStatus`, `MetadataBag`) have **separate** compatibility ownership in Essentials; WhenItFails must avoid inadvertently imposing inconsistent contracts on them.

The first public-signature and JSON/default baselines for these models have focused tests. Still open: precise 1.0 JSON migration policy, nullable-reference metadata guarantees, mutability/isolation, and which fields are required in a successfully produced payload.

**Active-context decision remains unresolved.** `GetCurrentContext()` returns the shared mutable context currently held in `ErrorCatalogContextStore`; atomically publishing the reference does not provide a deep immutable snapshot. Do not describe it as one. Decide whether to document shared mutability explicitly or add a separate read-only/snapshot-facing API with a compatibility plan.

### B. DI extension-point candidates

The 31 interface source files in `WhenItFails/Interfaces/` have a first public-shape review. Besides the application-facing `IErrorCatalogRuntime` and lookup `IErrorCatalog`, examples of supported **registration** seams are:

- Bootstrap, template, initialization and context: `IJsonsTemplateProvider`, `IJsonsBootstrapper`, `IErrorCatalogInitializer`, `IErrorCatalogContextProvider`, `IBuiltInErrorCatalogContextProvider`, `IErrorCatalogContextStore`.
- Main and specialized loader/provider/validator families, `IErrorCatalogFactory`, `IErrorCatalogDocumentNormalizer`.
- Definition/descriptor/profile resolution and descriptor creation services.

The default registrations use `TryAddSingleton` for the covered replacements, and the focused tests verify precedence of earlier custom DI registrations. This establishes **replaceability**, not behavioral equivalence of arbitrary replacements. Before calling an interface a fully guaranteed 1.0 extension contract, document cancellation, error normalization, null-return handling, ownership and lifecycle requirements for external implementers.

### C. Standalone public utility candidates — real Toolroom use verified

These exported concrete types have directly callable public APIs that merit a **supported utility** decision, rather than automatic classification as private implementation:

| Public type | Evidence from exported assembly | Repository consumer / review consequence |
| --- | --- | --- |
| `JsonCatalogDocumentWriter` | Public parameterless constructor; generic `SaveToFileAsync<TDocument>(TDocument, string, CancellationToken)` | Setter directly constructs and invokes it in workspace-editing files. Preserving its entry point matters to current Toolroom source compatibility; safe-write semantics have dedicated tests. |
| `DocumentationKeyGenerator` | Public constructor; `Generate(string, string, IEnumerable<string>)` and static `ToSegment(string)` | Setter directly constructs it in documentation-key suggestions and add-error workflows. Review/retain publicly documented use before visibility changes. |
| `DocumentationKeyFormat` | Public static `IsCanonical(string)` | Setter's commands and planning helpers reference it. |
| `ErrorCatalogCrossValidator` | Public constructor; `Validate(... five catalog documents, optional profileCatalog)` | Setter's workspace validator invokes it. |
| `JsonCatalogDocumentLoader` | Public generic `LoadFromFileAsync<TDocument>` | The five public JSON loader implementations directly depend on it; inspect direct external/Toolroom use before making its concrete API narrower. |
| `TextKeyNormalizer` | Public static `NormalizeDisplayName(string)` and `NormalizeKey(string)` | Used broadly within WhenItFails. External usage is **not established** by the source-search sample; decide whether it is documented utility API or merely an exported implementation helper. |

The search above verifies **repository** consumers, not the absence of additional external NuGet consumers. A published `public` type should not be made `internal` on the assumption that a code search found no outside users.

### D. Public concrete implementations — implementation-detail candidates

`ErrorCatalogRuntime`, `ErrorCatalogContextStore`, `ErrorCatalogContextProvider`, `BuiltInErrorCatalogContextProvider`, `ErrorCatalog`, `ErrorCatalogFactory`, `JsonsBootstrapper`, the default loaders/providers/validators, the resolver/factory implementations and normalizer classes are all publicly exported with public constructors or methods.

Their interfaces can remain the primary extension contract without freezing **every concrete constructor** for 1.0. However, some are intentionally instantiated directly by tests, tools or consumers. Review actual caller use and constructor dependencies case by case before renaming, hiding or altering a concrete type. Prefer adding a supported facade/overload and deprecating an old entry point to a silent accessibility-breaking change.

## 2. Additional gaps surfaced by the compiled report

- `ErrorDescriptor<TAttachment>` and `ErrorDescriptorRequest` are public models with dedicated tests, but were outside the original eight-model entry-point list. Decide whether they are explicitly supported convenience API, and review their inherited/error-attachment/nullable behavior.
- Exported enum integer values (`ErrorCatalogContextSource`, `ErrorCatalogInitializationMode`, `ErrorCatalogRuntimeState`, `ErrorCatalogValidationSeverity`) affect compatibility independently of method signatures; check the published values before changing any values or ordering.
- Public generic `JsonCatalogDocumentLoader.LoadFromFileAsync<TDocument>` and `JsonCatalogDocumentWriter.SaveToFileAsync<TDocument>` deserve separate publicly supported generic method and failure-contract decisions; provider-interface tests do not pin those generic signatures.
- Public `init`/getter-only properties (notably `ErrorCatalogRuntimeStatus`) differ from mutable `get; set;` models; retain that distinction in API comparison.
- The inventory's `TypeName` formatter **omits nullable-reference annotations, method generic constraints and most custom attributes**. It is therefore a human-readable type/member census, not a complete binary/API compatibility fingerprint. Existing focused tests for nullability and JSON property names remain necessary.

## 4. Setter utility contract baseline (verification pending)

The `SetterUtilityPublicApiContractTests` group contains five focused tests for four publicly exported types with confirmed Setter usage: `JsonCatalogDocumentWriter`, `DocumentationKeyGenerator`, `DocumentationKeyFormat`, and `ErrorCatalogCrossValidator`. It records their public constructors/method shapes (including the writer's generic class constraint and optional token, and cross-validator's optional profile parameter). A narrow smoke test exercises them without service registration or a workspace. Dedicated behavior suites still own backup, cancellation, complete documentation key formatting and cross-catalog validation behavior. No production code or visibility changes were made. The maintainer confirmed five focused tests GREEN and observed three xUnit2031 warnings. The test-only fix uses `Assert.Single(collection, predicate)`; the maintainer subsequently confirmed **1241/1241 GREEN** complete suite with zero warnings.

## 5. Isolated published-versus-source comparison workflow (verification pending)

[PublicApiComparer](../../Toolroom/WhenItFails/PublicApiComparer/Docs/Usage/en.md) builds independent temporary consumers against the current project and the exact published NuGet `[0.1.0]` package. Separate reflection inspections record actual DLL paths, SHA-256 hashes and public signature differences. Real feed provenance and the comparison result remain pending; no production API has changed.

## 6. Actual package-consumer comparison (611 vs 611 entries)

The maintainer ran the isolated comparison on 2026-09-25. Both temporary consumers restored and built successfully. The report shows **611 public API entries on each side, zero package-only entries and zero source-only entries**. The package consumer requested exact `[0.1.0]`; no `-Feed` override was used, so the result reflects an artifact resolved from configured sources/cache, with original publication provenance unverified.

The compared DLL SHA-256 digests differ: source-built `587AED89A427E184CEB465073201EB7CE986AE2219C964ACFFBF1C349EAE05EF`, package-consumer `379F7CF6FF99223ECF2F388AB6295A34D97F33A8BD8EB7F9A52152994347CE28`. This establishes matching signatures **within the comparer’s reflected census**, not identical binary content, complete ABI compatibility, nullability/JSON equivalence or identical runtime behavior. The temporary user-specific paths from the report are intentionally not copied into repository documentation.

The comparer added no library tests: last confirmed complete suite remains **1241/1241 GREEN with zero warnings**. Both consumer builds succeeded; the displayed `NETSDK1057` lines are informational preview-SDK notices.

## 7. Standalone JSON loader public contract (1244/1244 GREEN)

`JsonCatalogDocumentLoader` is already exported in 0.1.0 and called by the default typed loader implementations. `JsonCatalogDocumentLoaderPublicApiContractTests` now targets its public parameterless constructor, generic `class`-constrained `LoadFromFileAsync<TDocument>` signature with optional token, direct use without DI, and pre-cancelled token propagation before filesystem access. Detailed I/O and JSON cases are covered elsewhere. The maintainer confirmed three new focused tests and the complete **1244/1244 GREEN** suite; no production API changes were made.

## 8. Auxiliary descriptor model baseline (verification pending)

`ErrorDescriptorRequest` and `ErrorDescriptor<TAttachment>` now have three focused public API tests in `DescriptorAuxiliaryModelsPublicApiContractTests` for request shape/nullability, generic inheritance and explicit `attachment` JSON output with inherited `Exception` ignored. Existing dedicated descriptor tests cover defaults and attachment assignment. No 1.0 JSON naming guarantee is inferred for `ErrorDescriptorRequest`, which has no explicit `JsonPropertyName` attributes. Local test confirmation is pending; production code is unchanged.

## 3. Release/compatibility decisions still needed

1. The latest maintainer-confirmed full library suite is **1244/1244 GREEN** (the last explicit zero-warning confirmation was at 1241/1241). The independently executed package/source comparer reports matching public-signature census (611/611), not full binary identity.
2. Confirm **publishing-feed provenance** if a claim specifically requires nuget.org origin: the successful package restore used configured sources/cache. Keep this separate from the 611/611 API match.
3. Define the supported scope for the standalone writer/documentation utilities (and their external and Toolroom consumers) before the next public-constructor baseline. Do not bulk-test all exported constructors as if they were all promised stable.
4. Decide mutable active-context exposure and public JSON schema/version guarantees.
5. Document stable consumer contracts, supported extension points and public implementation details separately, then create selected cross-version API regression checks. Avoid an indiscriminate `110`-type count assertion, since deliberate nonbreaking additions should remain possible.

No code, visibility, runtime behavior or package version changes were made in this review.
