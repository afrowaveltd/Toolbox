# Classification of 38 added public types after 0.1.0

Review checkpoint: 2026-09-25. Scope: **the 38 source-only exported type names actually supplied by the maintainer** from the isolated NuGet 0.1.0 versus current source API comparison. This is a provisional 1.0 API classification, **not** a change of CLR visibility or a 1.0 compatibility guarantee.

## Observed comparison

| Measure | Current source-built DLL | NuGet consumer requesting `[0.1.0]` |
| --- | ---: | ---: |
| Exported CLR types | 148 | 110 |
| Public API census entries | 830 | 611 |

Source-only: **38 types, 219 API entries**. Package-only: **0 types, 0 API entries** within the comparer census. The maintainer supplied all 38 source-only type names but only the opening portion of the 219 source-only member-entry list. No current complete inventory Markdown report or full 1442-test confirmation has been supplied. Package feed override was not used, so original publishing provenance is not independently established.

## Classification by intended ownership

| Group | Types | Intended usage / compatibility review |
| --- | ---: | --- |
| Store infrastructure interfaces | 2 | Optional publication reading/writing seams; they return publication records that retain **live mutable context** references. Review ownership, concurrency and failure behavior before 1.0. |
| Optional runtime observation interfaces | 5 | Additional capabilities; custom `IErrorCatalogRuntime` implementations do **not** have to implement them. Review unavailable/unsupported semantics. |
| Publication and activation infrastructure models | 2 | Store-scoped `StoreId`/`Generation` versus runtime-local activation sequence/status; the context publication record is **not** a detached consumer snapshot. |
| Detached catalog, validation and observation models | 19 | Read-only copied operational projections and associated identity/status observations; review nullability, data isolation and selection consistency. |
| Additive snapshot extension classes | 10 | Public snapshot entry points, separate from the nine-method core runtime interface; review unsupported custom runtime behavior and capture boundaries. |
| **Total** | **38** | All names listed below; no unknown added type remains in the supplied type-level excerpt. |

### 1. Store infrastructure interfaces (2)
- `Afrowave.Toolbox.WhenItFails.Interfaces.IErrorCatalogContextPublicationReader`
- `Afrowave.Toolbox.WhenItFails.Interfaces.IErrorCatalogContextPublisher`

### 2. Optional runtime observation interfaces (5)
- `Afrowave.Toolbox.WhenItFails.Interfaces.IErrorCatalogRuntimeActivationReader`
- `Afrowave.Toolbox.WhenItFails.Interfaces.IErrorCatalogRuntimeCombinedObservationReader`
- `Afrowave.Toolbox.WhenItFails.Interfaces.IErrorCatalogRuntimeFullObservationReader`
- `Afrowave.Toolbox.WhenItFails.Interfaces.IErrorCatalogRuntimePublicationReader`
- `Afrowave.Toolbox.WhenItFails.Interfaces.IErrorCatalogRuntimeSupportingObservationReader`

### 3. Publication and activation infrastructure models (2)
- `Afrowave.Toolbox.WhenItFails.Runtime.ErrorCatalogActivationStatusSnapshot`
- `Afrowave.Toolbox.WhenItFails.Runtime.ErrorCatalogContextPublication`

### 4. Detached catalog, validation and observation models (19)
- `Afrowave.Toolbox.WhenItFails.Runtime.ErrorCatalogCombinedSnapshot`
- `Afrowave.Toolbox.WhenItFails.Runtime.ErrorCatalogCompletedCombinedSnapshot`
- `Afrowave.Toolbox.WhenItFails.Runtime.ErrorCatalogCompletedFullSnapshot`
- `Afrowave.Toolbox.WhenItFails.Runtime.ErrorCatalogCompletedSupportingCatalogsSnapshot`
- `Afrowave.Toolbox.WhenItFails.Runtime.ErrorCatalogFullSnapshot`
- `Afrowave.Toolbox.WhenItFails.Runtime.ErrorCatalogPublishedCombinedSnapshot`
- `Afrowave.Toolbox.WhenItFails.Runtime.ErrorCatalogPublishedSupportingCatalogsSnapshot`
- `Afrowave.Toolbox.WhenItFails.Runtime.ErrorCatalogValidationIssueSnapshot`
- `Afrowave.Toolbox.WhenItFails.Runtime.ErrorCatalogValidationSnapshot`
- `Afrowave.Toolbox.WhenItFails.Runtime.ErrorCategoryCatalogSnapshot`
- `Afrowave.Toolbox.WhenItFails.Runtime.ErrorCategoryDefinitionSnapshot`
- `Afrowave.Toolbox.WhenItFails.Runtime.ErrorCodeGroupCatalogSnapshot`
- `Afrowave.Toolbox.WhenItFails.Runtime.ErrorCodeGroupDefinitionSnapshot`
- `Afrowave.Toolbox.WhenItFails.Runtime.ErrorDefinitionSnapshot`
- `Afrowave.Toolbox.WhenItFails.Runtime.ErrorOwnerCatalogSnapshot`
- `Afrowave.Toolbox.WhenItFails.Runtime.ErrorOwnerDefinitionSnapshot`
- `Afrowave.Toolbox.WhenItFails.Runtime.ErrorProfileCatalogSnapshot`
- `Afrowave.Toolbox.WhenItFails.Runtime.ErrorProfileDefinitionSnapshot`
- `Afrowave.Toolbox.WhenItFails.Runtime.ErrorSupportingCatalogsSnapshot`

### 5. Additive snapshot extension classes (10)
- `Afrowave.Toolbox.WhenItFails.Runtime.ErrorCatalogCombinedSnapshotExtensions`
- `Afrowave.Toolbox.WhenItFails.Runtime.ErrorCatalogPublishedCombinedSnapshotExtensions`
- `Afrowave.Toolbox.WhenItFails.Runtime.ErrorCatalogPublishedSupportingCatalogsSnapshotExtensions`
- `Afrowave.Toolbox.WhenItFails.Runtime.ErrorCatalogSnapshotExtensions`
- `Afrowave.Toolbox.WhenItFails.Runtime.ErrorCatalogValidationSnapshotExtensions`
- `Afrowave.Toolbox.WhenItFails.Runtime.ErrorCategoryCatalogSnapshotExtensions`
- `Afrowave.Toolbox.WhenItFails.Runtime.ErrorCodeGroupCatalogSnapshotExtensions`
- `Afrowave.Toolbox.WhenItFails.Runtime.ErrorOwnerCatalogSnapshotExtensions`
- `Afrowave.Toolbox.WhenItFails.Runtime.ErrorProfileCatalogSnapshotExtensions`
- `Afrowave.Toolbox.WhenItFails.Runtime.ErrorSupportingCatalogsSnapshotExtensions`

## Compatibility boundaries and pending evidence

The absence of package-only types/signatures in this comparer is useful evidence for preserving the inspected public signature census. It is **not** proof of full ABI/source, nullable metadata, serialization, behavior or complete dependency compatibility. The 38 names **do** now have an existing-type API census follow-up: exactly **14** additions are on the two previously exported concrete types and **205** entries are on the 38 new types; see [original-type additions](../Original-Type-API-Additions/en.md).

Do not make these publicly exported types `internal` merely because their intended audience is infrastructure. Any future visibility restriction or 1.0 API freeze requires a separate compatibility and consumer-usage decision. The historic 110-type and 611-entry figures describe the requested 0.1.0 package consumer, not current source.

Remaining verification: confirm the four focused inventory tests and complete **1442/1442 GREEN** suite, then review the current compiled Markdown inventory and package-compatibility evidence. The most recent complete suite explicitly confirmed by the maintainer before this report was **1439/1439 GREEN**.

A later maintainer-supplied filtered API excerpt identifies **all 14** additions to existing concrete types; the remaining **205 census entries** belong to the 38 new exported types. See [original-type API additions](../Original-Type-API-Additions/en.md).
