# Publication-aware detached supporting catalog snapshots

Status: **additive pre-1.0 CLR API candidate; 15 focused theory-expanded cases included in maintainer-confirmed 1392/1392 GREEN suite**.

## Purpose

`GetPublishedSupportingCatalogsSnapshot(this IErrorCatalogRuntime)`
selects exactly one existing context publication and returns its actual
store-scoped `StoreId` and `Generation` along with a detached copy
of all four supporting catalogs:

- categories;
- owners;
- code groups;
- profiles.

The returned `ErrorCatalogPublishedSupportingCatalogsSnapshot` has
getter-only `StoreId`, `Generation` and `Snapshot` properties. The
`Snapshot` is the existing `ErrorSupportingCatalogsSnapshot`,
with four independently copied read-only catalog projections.

```csharp
using Afrowave.Toolbox.WhenItFails.Runtime;

Response<ErrorCatalogPublishedSupportingCatalogsSnapshot> result =
    runtime.GetPublishedSupportingCatalogsSnapshot();

if (result.IsSuccess && result.Data is { } published)
{
    Console.WriteLine(
        $"{published.StoreId}/{published.Generation}: " +
        $"{published.Snapshot.ProfileCatalog.Profiles.Count} profiles");
}
```

## Ownership and compatibility

The optional `IErrorCatalogRuntimePublicationReader` selects an actual
publication record through one `GetCurrentPublication()` call. The
extension does **not** re-read the live context through
`GetCurrentContext()`, re-read the publication after copying or
query `GetStatus()`. The selected record's identity remains associated
with its own context even if a newer publication appears before
copying finishes. The result need not describe the *latest* publication
by the time the caller receives it.

`StoreId` identifies one store instance, and `Generation` counts
successful writes within that store. This pair does **not** identify
a completed runtime activation, nor does it synchronize a runtime
status with a catalog. It is not a durable across-process identity.
External in-place mutation of already published nested documents
during capture is **not** a supported transaction.

The existing `GetSupportingCatalogsSnapshot()` remains available for
runtimes without publication support. The original three-part
`GetCombinedSnapshot()`, `GetPublishedCombinedSnapshot()` and
`GetCompletedCombinedSnapshot()` remain unchanged. This new snapshot
does not add main error definitions, cross-validation findings or
runtime status; no JSON wire format or 1.0 API freeze is implied.

## Failure handling

A runtime without the optional publication reader returns NotSupported
(`WIF_PUBLISHED_SUPPORTING_SNAPSHOT_NOT_SUPPORTED`). The default runtime
with a legacy store forwards its existing
`WIF_CONTEXT_PUBLICATION_NOT_SUPPORTED` response. Uninitialized
publication responses are forwarded without data. A null response
produces `WIF_PUBLISHED_SUPPORTING_SNAPSHOT_PUBLICATION_RESPONSE_NULL`;
success without a publication yields
`WIF_PUBLISHED_SUPPORTING_SNAPSHOT_PUBLICATION_NULL`. Missing
catalogs reuse the established `WIF_SUPPORTING_SNAPSHOT_*_NULL`
codes, and malformed nested data reuse `WIF_SUPPORTING_SNAPSHOT_FAILED`.
Ordinary publication-reader or outer capture failures produce
`WIF_PUBLISHED_SUPPORTING_SNAPSHOT_FAILED` without exception details
or partial data. `OperationCanceledException` propagates.

The published 0.1.0 NuGet package and persistent JSON catalog
schema are not changed by this source checkpoint.

## Verification

`PublishedSupportingCatalogsSnapshotContractTests` adds 15
theory-expanded cases: default-runtime actual store identity without
status, one publication read across replacement, both unavailable
capability paths, uninitialized and null responses, success without
publication, all four absent catalogs, malformed nested data,
ordinary reader exception, exact cancellation, and public API shape.

Full suite expected after local maintainer verification:
**1392/1392 GREEN**.

The separate [completed supporting snapshot](../Completed-Supporting-Catalog-Snapshots/en.md)
also pairs these four catalogs with a recorded completed activation status,
with pre/post publication and status consistency checks.
