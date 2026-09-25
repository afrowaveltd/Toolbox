# Completed full operational catalog and activation snapshot

Status: **additive pre-1.0 CLR API candidate; 19 cases included in maintainer-confirmed 1426/1426 GREEN; three additional race/activation tests pending verification**.

## Scope and usage

The optional `IErrorCatalogRuntimeFullObservationReader`, implemented by the
default `ErrorCatalogRuntime`, exposes a single checked operation:

```csharp
using Afrowave.Toolbox.WhenItFails.Interfaces;

if (runtime is IErrorCatalogRuntimeFullObservationReader reader)
{
    var response = reader.GetCompletedFullSnapshot();

    if (response.IsSuccess && response.Data is { } completed)
    {
        Console.WriteLine(
            $"{completed.StoreId}/{completed.Generation}, " +
            $"activation #{completed.ActivationSequence}: " +
            completed.Status.State);
        Console.WriteLine(completed.Snapshot.Definitions.Count);
        Console.WriteLine(completed.Snapshot.ProfileCatalog.Profiles.Count);
    }
}
```

The sealed, getter-only `ErrorCatalogCompletedFullSnapshot` captures
`StoreId`, `Generation`, `ActivationSequence`, the status recorded
for that activation, and one `ErrorCatalogFullSnapshot`.
The full operational data view contains six independently detached
components selected from **one existing context publication**:

- `Definitions`: main indexed error definitions, captured as
  `IReadOnlyList<ErrorDefinitionSnapshot>`;
- `CategoryCatalog`: category document and nested definitions;
- `OwnerCatalog`: owner document and nested definitions;
- `CodeGroupCatalog`: code-group document and nested definitions;
- `ProfileCatalog`: profile document, definitions and all eight filters;
- `Validation`: the recorded cross-validation findings.

The implementation reuses the existing detached combined and supporting
catalog projections, including the **same already captured category
snapshot object**, rather than copying category data twice.
No live `ErrorCatalogContext`, catalog document or mutable nested
definition escapes through these data projections. The outer Essentials
`Response<T>` envelope remains mutable.

This is a **complete operational catalog projection**, not a bit-for-bit
copy of all seven properties on `ErrorCatalogContext`: it does not expose
the original mutable `ErrorCatalogDocument`, the implementation of
`IErrorCatalog` or a newly validated source document. In particular,
main document-level JSON metadata not represented by indexed definitions
is outside this projection. No JSON wire protocol or direct
deserialization contract is promised.

## Selected completed activation and concurrency

The runtime selects its previously recorded completed activation, which
already associates one exact store publication record with runtime status
and a runtime-local activation sequence. It verifies status and
publication identity before copying, captures both the combined and
supporting data from **that same selected context**, then verifies
publication identity and completed status again.

A same-reference republish advances the store generation and is rejected
rather than attributed to the old activation. Previous-context recovery
can advance `ActivationSequence` without advancing `Generation`; a
status change during capture is rejected independently of generation.

This is a checked observation, **not** a globally atomic context-plus-status
transaction. A writer can publish after the final check, and concurrent
in-place mutation of nested objects in the selected live context is not
serialized. Treat published contexts as read-only and activate validated
replacement contexts instead. Recorded validation is copied, not rerun.

## Failure and compatibility contracts

Stores without the optional publication reader return NotSupported with
`WIF_COMPLETED_FULL_NOT_SUPPORTED`. Before the first completed activation,
the result is Invalid (`WIF_COMPLETED_FULL_UNAVAILABLE`). A mismatched
publication returns `WIF_COMPLETED_FULL_PUBLICATION_CHANGED`, while a
mismatched recorded activation or status returns
`WIF_COMPLETED_FULL_STATUS_CHANGED`.

Missing main/catalog/validation data preserves the established
`WIF_COMBINED_SNAPSHOT_*_NULL` Invalid codes; missing owner, code-group
or profile catalogs preserves the corresponding
`WIF_SUPPORTING_SNAPSHOT_*_NULL` codes. Malformed nested data returns
the established `WIF_COMBINED_SNAPSHOT_FAILED` or
`WIF_SUPPORTING_SNAPSHOT_FAILED` without partial snapshot data.
Unexpected ordinary outer exceptions return
`WIF_COMPLETED_FULL_FAILED` without exception details, and
`OperationCanceledException` propagates.

All original runtime and three-part combined snapshot public shapes
remain unchanged. The package 0.1.0 and persistent JSON catalog schemas
are not changed by this source checkpoint.

## Verification

`CompletedFullSnapshotContractTests` contains **19 theory-expanded cases**:
matched status and all six detached views, recovery without republishing,
same-reference republishing, simultaneous publication/status changes during
capture, uninitialized/legacy behavior, missing source components,
malformed nested documents, cancellation and stable public CLR shape.

The initial 19 cases were included in maintainer-confirmed **1426/1426 GREEN**.
Three additional focused tests, included in maintainer-confirmed **1429/1429 GREEN**, protect successive successful activations,
replacement before the first publication read, and a same-reference
republication during capture. The first snapshot must retain its own
six detached views and associated recorded status after the next
successful activation; a changed publication record must be rejected
even if the live context object is the same reference.

**Verified:** 22/22 focused cases were included in the maintainer-confirmed
**1429/1429 GREEN** complete suite. Six further nullable-contract tests are
pending verification; see [snapshot nullable contracts](../Nullable-Snapshot-Contracts/en.md).
