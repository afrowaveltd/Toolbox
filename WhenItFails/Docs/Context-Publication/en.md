# Context publication identity

Status: **additive store-layer contract; seven focused tests included in maintainer-confirmed 1293/1293 GREEN suite**.

## What a generation means

The default `ErrorCatalogContextStore` now publishes one immutable
`ErrorCatalogContextPublication` record containing three getter-only values:

- `StoreId` is a nonempty `Guid` assigned once per store instance.
- `Generation` is a positive `long`, starting at 1 and advancing by
  one on every **successful** `Set(context)` call in that store.
- `Context` is the **live, mutable** context reference supplied to
  `Set`; publication does not deep-copy the context or nested catalogs.

A `(StoreId, Generation)` pair distinguishes publications within the
runtime's store lifetime. A generation number alone is **not** globally
unique: separate stores each start at 1. The identifier is not persisted
across process restarts or intended as a durable database key.

`Set` uses an atomic compare/exchange loop to update the **entire
publication record**. This preserves successful publication order even
under concurrent writers; reading the record requires one volatile
read. A previous publication record remains unchanged when a new one
is installed. Re-publishing the **same context reference** is still a
new publication with a new generation. A rejected null `Set` does
not change the current publication.

## Exact successful write ownership

The default context store now also implements the optional `IErrorCatalogContextPublisher`. `Publish(context)` returns the **exact** `ErrorCatalogContextPublication` that won this call's atomic compare/exchange; the legacy `Set(context)` delegates to the same write path and keeps its original void signature. A later `GetCurrentPublication()` can already refer to another writer, even one that republishes the same context object. Ownership must be based on the returned record, not reference equality. See [exact publication ownership](../Publication-Ownership/en.md). The existing default initializer/runtime status flow does not yet propagate this new token and therefore has not gained strict activation ownership from this store-only change.

## Optional infrastructure interface

`ErrorCatalogContextStore` implements both the original
`IErrorCatalogContextStore` and the additive
`IErrorCatalogContextPublicationReader`. The original interface,
`Current`, `GetCurrent()`, `IsInitialized`, and `Set` signatures
are unchanged. The additional read method returns a
`Response<ErrorCatalogContextPublication>` and preserves the
pre-initialization `ErrorCatalogContextNotInitialized` error.

```csharp
using Afrowave.Toolbox.WhenItFails.Interfaces;
using Afrowave.Toolbox.WhenItFails.Services;

IErrorCatalogContextStore store = new ErrorCatalogContextStore();

if (store is IErrorCatalogContextPublicationReader reader)
{
    var response = reader.GetCurrentPublication();
    if (response.IsSuccess && response.Data is { } published)
    {
        Console.WriteLine($"{published.StoreId}/{published.Generation}");
        // published.Context is LIVE; do not mutate it.
    }
}
```

This is a **low-level runtime/infrastructure contract**, not the
safe application-facing data view. The publication record exposes the
live context reference; use the detached definition, category,
validation or combined snapshot APIs for read-only consumer data.

A custom `IErrorCatalogContextStore` implementation is **not**
required to implement the optional reader. Callers must not fabricate
a generation from a timestamp, context hash, or per-call GUID when
the capability is absent.

## Publication versus runtime status

Source review of the current activation paths shows:

- Normal project initialization calls `Set` inside
  `ErrorCatalogInitializer` and only then records runtime status in
  `ErrorCatalogRuntime`.
- Explicit reset and automatic built-in fallback call `Set` when a
  new built-in context is successfully loaded, then record status.
- Failed initialization that retains the previous context records a
  recovery status **without** calling `Set`. Its previously published
  context generation therefore remains unchanged.
- `Set` calls made directly by external code or other store
  consumers are counted as publications whether or not the runtime
  creates a corresponding status record.

**Publication generation is not yet an atomic context-and-status
activation token.** `GetStatus()` and `GetCurrentPublication()` are
independent reads. A reader can observe a newly published context
before its status has been recorded, or a later status with another
publication. A retained-previous-context recovery changes the status
but not the context generation. Do not infer status identity or
recovery lifecycle from a generation number alone.

The original `GetCombinedSnapshot()` still uses one
`GetCurrentContext()` read and does not expose publication identity.
The separate additive [publication-aware combined snapshot](../Published-Snapshots/en.md)
uses the default runtime's optional publication-reader capability to copy
three detached projections from **one selected store publication** and
reports its actual `StoreId` and `Generation`. Custom runtimes and stores
can opt in; unsupported implementations return NotSupported without
fabricating an ID. This does **not** solve the status-update window or
create a synchronized runtime activation/status contract.

## Runtime status and publication lifecycle regression tests

`ContextPublicationStatusLifecycleContractTests` records **six** specific
default-runtime behaviors using the real `ErrorCatalogContextStore` and
controlled initializer/provider test doubles:

1. A successful project initialization publishes a context and later
   records project status.
2. A failed **strict** reinitialization retains the previous publication
   **and** the previously recorded runtime status.
3. A failed **flexible** reinitialization with a previous context retains
   the same publication record and generation, but records a **new**
   `PreviousContextRecovery` status.
4. A first-initialization failure recovered with bundled defaults publishes
   generation 1 with `BuiltInFallback` status; a subsequent explicit reset
   publishes generation 2 with `BuiltInDefaults` status.
5. A failed explicit reset preserves the active publication and status.
6. A deterministic callback after the initializer calls `Set`, but
   before it returns to `ErrorCatalogRuntime`, can observe the **new**
   publication with the **previous** recorded status. This demonstrates
   an observable ordering window without relying on timing or a
   probabilistic multithreaded race.

The six tests document the **existing lifecycle**. They do not prove that
status and context are atomically paired; in fact, the final test
demonstrates the opposite.

For a future combined activation state, a design must address both
the publication-before-status window and the independent status change
during previous-context recovery. Merely double-reading the store
generation around `GetStatus()` cannot prove correct pairing: the
generation can stay unchanged while recovery status changes. Direct
calls to `Set` by other code can also publish a context without any
runtime status event. Any stronger API must define publication ownership,
the status association, and the behavior of custom context stores.

## Completed status observations

The additive [completed activation status observation](../Activation-Status/en.md) records a selected context publication alongside one finished runtime status update. It uses a separate runtime-local sequence because previous-context recovery updates status without changing the store generation. The reader rejects a publication that no longer matches the recorded status event. This is **not** the final atomic activation protocol: external store writers and concurrent initializations can change the live publication independently, and a separately acquired combined snapshot/status is not automatically paired.

## Thread safety limits

Atomic publication protects the **record** (context reference plus
generation), not fields or collections inside the context. A
concurrently edited live document can still produce inconsistent
values during a snapshot copy, even when the record's generation is
stable. Published contexts must be treated as read-only in application
code. The ordinary `Response<T>` envelope remains mutable.
