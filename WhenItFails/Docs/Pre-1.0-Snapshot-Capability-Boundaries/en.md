# Snapshot capability boundaries before 1.0

Status: **historical pre-1.0 boundary review; its focused contracts are retained in the frozen 1.0 surface and the maintainer-confirmed 1475/1475 GREEN release suite**.

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

## Historical scope and resolved follow-up

The full snapshot is a detached *operational* view, not a raw JSON document
clone or a transaction against concurrent in-place source mutation. The earlier
110-type report and the later 148/830 pre-cleanup census are historical evidence.
The final first-stable surface was subsequently reviewed, reduced and frozen at
**126 exported types / 698 API entries**. Nullable contracts, persistent catalog
schema versioning, error behavior and supported public construction boundaries were
resolved by focused contracts and the final 1.0 API policy.

`SnapshotCapabilityBoundaryContractTests` adds four focused signature
tests for the core interface, five optional readers, six context extensions
and two publication-aware extensions. These tests remain included in the final
maintainer-confirmed **1475/1475 GREEN** release suite.
