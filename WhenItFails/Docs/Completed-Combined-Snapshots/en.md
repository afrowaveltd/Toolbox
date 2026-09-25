# Completed combined catalog and status snapshot

Status: **additive pre-1.0 API candidate; six focused tests await local verification**.

## Purpose

The default `ErrorCatalogRuntime` implements the optional
`IErrorCatalogRuntimeCombinedObservationReader`. It exposes a single
`GetCompletedCombinedSnapshot()` operation for consumers that need a
**matched recorded runtime status and detached catalog data** in one
response. The original nine-method `IErrorCatalogRuntime` and existing
individual snapshot APIs remain unchanged.

```csharp
using Afrowave.Toolbox.WhenItFails.Interfaces;

if (runtime is IErrorCatalogRuntimeCombinedObservationReader reader)
{
    var response = reader.GetCompletedCombinedSnapshot();

    if (response.IsSuccess && response.Data is { } observation)
    {
        Console.WriteLine(
            $"{observation.StoreId}/{observation.Generation}, " +
            $"status #{observation.ActivationSequence}: " +
            $"{observation.Status.State}");
        Console.WriteLine(observation.Snapshot.Definitions.Count);
    }
}
```

The sealed, getter-only `ErrorCatalogCompletedCombinedSnapshot` carries
`StoreId`, `Generation`, `ActivationSequence`,
`ErrorCatalogRuntimeStatus Status` and
`ErrorCatalogCombinedSnapshot Snapshot`. The combined snapshot contains
detached error definitions, category catalog and recorded cross-validation
findings. It does **not** expose the live `ErrorCatalogContext` or
supporting owner/code-group/profile catalogs. The outer Essentials
`Response<T>` remains the ordinary mutable response envelope.

## One selected publication and consistency checks

The runtime first selects **one previously recorded completed status**
with its exact associated `ErrorCatalogContextPublication`. It checks
that this record is the current store publication and that its recorded
status is still current. It then copies all three catalog projections
from the context held in the **selected record**, not from a new
`GetCurrentContext()` or another independently selected publication.

After copying the data, it checks the current store publication again
against that same record, and verifies the selected completed observation
and runtime status have not changed. A mismatch returns Invalid with no
partially captured data. The second store read is a **consistency check**,
not another independent context selection.

`WIF_COMPLETED_COMBINED_UNAVAILABLE` means no completed observation was
recorded, `WIF_COMPLETED_COMBINED_STATUS_CHANGED` means the selected
status has changed, and
`WIF_COMPLETED_COMBINED_PUBLICATION_CHANGED` means the selected store
publication is no longer current. A configured store without the
optional publication reader returns NotSupported with
`WIF_COMPLETED_COMBINED_NOT_SUPPORTED`. Unexpected ordinary errors
produce `WIF_COMPLETED_COMBINED_FAILED` without exception text;
cancellation exceptions propagate. Missing source catalogs retain the
structured `WIF_COMBINED_SNAPSHOT_*` error codes.

## What this does NOT guarantee

This is a **checked, selected observation**, not a transaction or an
atomic store-and-status publication. An external writer can publish a
different record immediately *after* the final check. A caller must not
interpret the result as a durable assertion that its generation is still
current at a later instant.

The live context nested objects remain mutable. External in-place edits
during capture may yield inconsistent data despite an unchanged
publication generation; the read-only projection does not validate
or lock the live catalog. Applications should treat published contexts
as read-only and publish validated replacements.

The ordinary recorded cross-validation findings are captured from the
selected context; this method does not rerun validation against live
documents. A custom initializer that does not supply an exact owned
publication token retains the existing weaker association semantics.
Recovery with an optional publication reader selects the exact **existing**
record, but does not own a new write. Neither read-side consistency
checks nor one runtime's instance-local activation gate serialize
external writers.

No stable JSON wire schema or direct deserialization contract for these
getter-only projection types is promised. Package version 0.1.0 and
persisted catalog JSON formats are unchanged.

## Verification

`CompletedCombinedSnapshotContractTests` contains six focused cases:
a default reset capturing one matched publication and detached data,
previous-context recovery updating the status sequence without advancing
the generation, later same-reference publication rejection, an external
write during capture detected by the second store read, a missing
required category catalog, and additive public/legacy-store behavior.
