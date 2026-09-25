# Completed activation with detached supporting catalogs

Status: **additive pre-1.0 CLR API candidate; 15 focused cases included in maintainer-confirmed 1407/1407 GREEN suite**.

## Purpose and usage

The default `ErrorCatalogRuntime` implements the new optional
`IErrorCatalogRuntimeSupportingObservationReader`, without adding
members to the existing nine-method `IErrorCatalogRuntime` or to
`IErrorCatalogRuntimeCombinedObservationReader`:

```csharp
using Afrowave.Toolbox.WhenItFails.Interfaces;

if (runtime is IErrorCatalogRuntimeSupportingObservationReader reader)
{
    var response = reader.GetCompletedSupportingCatalogsSnapshot();

    if (response.IsSuccess && response.Data is { } observation)
    {
        Console.WriteLine(
            $"{observation.StoreId}/{observation.Generation}, " +
            $"activation #{observation.ActivationSequence}: " +
            $"{observation.Status.State}");
        Console.WriteLine(observation.Snapshot.ProfileCatalog.Profiles.Count);
    }
}
```

The sealed, getter-only
`ErrorCatalogCompletedSupportingCatalogsSnapshot` contains
`StoreId`, `Generation`, `ActivationSequence`, recorded
`ErrorCatalogRuntimeStatus Status` and a detached
`ErrorSupportingCatalogsSnapshot Snapshot`. The nested snapshot
contains the category, owner, code-group and profile catalog
projections. It exposes no live context or mutable catalog documents.
The Essentials `Response<T>` envelope is still mutable.

## Selected activation and consistency checks

The default runtime selects its **previously recorded** completed
activation, which already associates a runtime-local sequence and status
with one exact store publication record. It checks that the associated
status is current and that the selected record is the current store
publication. The four catalog projections are captured from the
context held by **that record**, without a separate context selection.

After copying, the runtime rereads the store publication as a
**consistency check against the same selected record**, and verifies
that both the recorded completed activation and status are still
current. A mismatch produces Invalid without a partial snapshot.
A second publication of the very same context object still changes
the publication record and is rejected.

A flexible recovery that retains the previous context can increment
`ActivationSequence` and change the recorded runtime status while
leaving `Generation` unchanged. The second status check rejects a
mismatched activation even if both publication reads return the same
record.

## Errors and unsupported configurations

A store without `IErrorCatalogContextPublicationReader` returns
NotSupported (`WIF_COMPLETED_SUPPORTING_NOT_SUPPORTED`); no
fabricated publication identity is permitted. Before any completed
activation, the call returns Invalid
(`WIF_COMPLETED_SUPPORTING_UNAVAILABLE`). A status mismatch uses
`WIF_COMPLETED_SUPPORTING_STATUS_CHANGED`; a publication mismatch
uses `WIF_COMPLETED_SUPPORTING_PUBLICATION_CHANGED`.

Missing supporting documents preserve existing
`WIF_SUPPORTING_SNAPSHOT_*_NULL` Invalid codes. Malformed nested
catalog values preserve `WIF_SUPPORTING_SNAPSHOT_FAILED` without
partial data. An ordinary exception at the outer runtime read boundary
returns `WIF_COMPLETED_SUPPORTING_FAILED`, omitting exception text.
`OperationCanceledException` propagates.

## Limitations

This is a **checked observation**, not an atomic joint status-and-store
write. An external writer can publish after the final check. External
in-place mutation of an already published context's nested objects
during capture is not transactional and can yield inconsistent
data with an unchanged `Generation`. Treat published context data
as read-only; activate validated replacements instead.

This projection deliberately excludes main indexed error definitions
and recorded cross-validation findings. The existing three-part
`GetCompletedCombinedSnapshot()`, `GetCombinedSnapshot()` and
`GetPublishedCombinedSnapshot()` retain their existing public shapes.
The package 0.1.0 and persisted catalog JSON schemas remain unchanged.
No versioned JSON wire contract or 1.0 public API freeze is implied.

## Verification

`CompletedSupportingCatalogsSnapshotContractTests` contains **15
theory-expanded cases**: matched reset/status and four detached
catalogs; retained-previous-context recovery; same-reference
republishing; external publication and status change during copying;
pre-activation and legacy-store handling; four missing-catalog cases;
malformed nested values; ordinary reader exception; exact cancellation;
and optional public API shape.

Expected full-suite checkpoint after local verification:
**1407/1407 GREEN**.

The [completed full operational snapshot](../Completed-Full-Snapshots/en.md)
also includes the main indexed definitions and recorded validation results
while preserving publication and completed-status consistency checks.
