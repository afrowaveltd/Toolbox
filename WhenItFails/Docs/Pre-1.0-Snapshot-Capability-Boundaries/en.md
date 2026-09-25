# Snapshot capability boundaries before 1.0

Status: **source-level pre-1.0 review; four focused tests pending verification**.

## Application-facing and optional entry points

The original `IErrorCatalogRuntime` retains exactly nine methods, including
two initialization overloads. New capabilities must not require third-party
implementations of that interface to add members.

The default runtime separately implements five optional single-method readers:

| Optional interface | Method | Contract |
| --- | --- | --- |
| `IErrorCatalogRuntimePublicationReader` | `GetCurrentPublication()` | Store-scoped identity and a **live mutable** context reference intended for infrastructure |
| `IErrorCatalogRuntimeActivationReader` | `GetCompletedActivation()` | Recorded completed activation status |
| `IErrorCatalogRuntimeCombinedObservationReader` | `GetCompletedCombinedSnapshot()` | Indexed definitions, category, recorded validation and matching status |
| `IErrorCatalogRuntimeSupportingObservationReader` | `GetCompletedSupportingCatalogsSnapshot()` | Four supporting catalogs and matching status |
| `IErrorCatalogRuntimeFullObservationReader` | `GetCompletedFullSnapshot()` | All six operational projections and matching status |

Custom runtimes may implement only the optional capabilities they support.
The consumer must test the interface and both response success and non-null
`Data`. A store without publication support may return NotSupported.

Six context-selected detached extension methods
(`GetCategoryCatalogSnapshot`, `GetOwnerCatalogSnapshot`,
`GetCodeGroupCatalogSnapshot`, `GetProfileCatalogSnapshot`,
`GetCombinedSnapshot`, `GetSupportingCatalogsSnapshot`) take
`IErrorCatalogRuntime`. The two publication-aware extensions
(`GetPublishedCombinedSnapshot`,
`GetPublishedSupportingCatalogsSnapshot`) take that same runtime but
require its optional publication reader. They do not invent publication IDs.

Internal `CaptureFromContext` helpers are **not** public entry points.
Conversely, publicly exported optional interfaces, extension methods,
snapshot models and concrete service classes remain real public CLR APIs
even when classified as provisional. Do not change their visibility without
a separate compatibility decision.

## Scope and remaining work

The full snapshot is a detached *operational* view, not a raw JSON document
clone or a transaction against concurrent in-place source mutation. The
historical exported-assembly report listed 110 types at an earlier source
checkpoint. Regenerate that report and compare against the published
package before claiming a current type count or freezing 1.0 compatibility.
Nullable annotations, JSON versioning, errors, and constructors of public
concrete classes require separately scoped decisions.

`SnapshotCapabilityBoundaryContractTests` adds four focused signature
tests for the core interface, five optional readers, six context extensions
and two publication-aware extensions. Last confirmed full suite:
**1435/1435 GREEN**; expected next full suite **1439/1439 GREEN**.
