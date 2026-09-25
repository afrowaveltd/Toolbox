# Publication-aware combined snapshots

Status: **additive pre-1.0 API candidate; local verification pending**.

## Purpose

The existing `GetCombinedSnapshot()` captures detached error definitions,
categories and recorded validation findings from one selected context
reference, but it does not identify the selected store publication.

`GetPublishedCombinedSnapshot(this IErrorCatalogRuntime)` adds an optional
capability that captures the same detached three-part data projection and
associates it with the **actual** `StoreId` and `Generation` returned by
the context store:

```csharp
using Afrowave.Toolbox.WhenItFails.Runtime;

var response = runtime.GetPublishedCombinedSnapshot();

if (response.IsSuccess && response.Data is { } published)
{
    Console.WriteLine(
        $"{published.StoreId}/{published.Generation}: " +
        $"{published.Snapshot.Definitions.Count} errors");
}
```

The returned `ErrorCatalogPublishedCombinedSnapshot` is sealed and
getter-only. Its `Snapshot` property holds the detached
`ErrorCatalogCombinedSnapshot`, with `Definitions`, `CategoryCatalog`
and `Validation`. The returned data exposes **no live
`ErrorCatalogContext` reference**. The outer Essentials
`Response<T>` remains mutable.

## Optional runtime capability

The default `ErrorCatalogRuntime` implements the additive
`IErrorCatalogRuntimePublicationReader` without changing the original
nine-method `IErrorCatalogRuntime` interface. Its
`GetCurrentPublication()` delegates to the **same injected context
store**, when that store implements
`IErrorCatalogContextPublicationReader`.

A custom runtime can opt in by implementing the optional runtime reader
with an equivalent real publication contract. A custom store that does
not expose publication identity returns NotSupported from the default
runtime. A custom runtime that does not implement the optional reader
also returns NotSupported from the extension. Neither path guesses
`StoreId`, `Generation`, timestamps or per-call GUIDs. The
original `GetCombinedSnapshot()` remains available to all runtimes.

## One selected publication

The new extension performs exactly **one**
`GetCurrentPublication()` call and does not subsequently call
`GetCurrentContext()` or `GetStatus()`. It copies the three data
projections from the `Context` reference held in that selected record.
When a newer publication occurs during capture, the reported identity
still belongs to the selected record and its context, not the newer
one. An already returned detached snapshot remains independent of
later source mutations or replacements.

`Generation` counts successful store `Set` publications. It is
monotonic **within one `StoreId`**; the pair is not a persistent
cross-process identifier. Re-publishing the same context reference
counts as a new publication, but does not isolate that mutable context.
The capture cannot be a transaction if external code changes the
selected live context in place during copying.

**This is not a synchronized runtime activation/status snapshot.**
Normal initialization publishes the context before recording runtime
status; retained-previous-context recovery changes status without
publishing a new context. The published combined projection deliberately
does **not** include or infer `ErrorCatalogRuntimeStatus`, recovery
state, or an activation-event ID. Use `GetStatus()` separately for
diagnostics without assuming it was read atomically with the catalog.

## Failures and compatibility

Missing optional runtime/store support returns NotSupported with,
respectively, `WIF_PUBLISHED_SNAPSHOT_NOT_SUPPORTED` or
`WIF_CONTEXT_PUBLICATION_NOT_SUPPORTED`. Uninitialized publication
responses are forwarded without data. A successful publication
response with null data returns Invalid with
`WIF_PUBLISHED_SNAPSHOT_PUBLICATION_NULL`. Ordinary capture failures
produce `WIF_PUBLISHED_SNAPSHOT_FAILED` without exception details;
cancellation propagates. Missing main catalog, category catalog or
recorded validation uses the existing `WIF_COMBINED_SNAPSHOT_*`
Invalid codes, also without partial data.

This adds a public optional interface, one public snapshot type and
one extension method, but does not change existing runtime/store
interface methods or constructors, the published 0.1.0 NuGet package,
or the persisted catalog JSON schemas. The output is a pre-1.0 CLR
projection, **not a versioned JSON wire contract**.

See [context publication identity](../Context-Publication/en.md) for
store-scoped identity and [combined snapshots](../Combined-Snapshots/en.md)
for the three captured data components.
