# Context publication identity

Status: **additive store-layer contract; seven focused tests awaiting local verification**.

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

The existing `GetCombinedSnapshot()` currently uses one
`GetCurrentContext()` read, **not** the optional publication reader,
and therefore does not yet expose `StoreId` or `Generation`.
Integration must avoid breaking custom runtime/store implementations
and must define how to handle the status-update window before
promising an activation-generation contract.

## Thread safety limits

Atomic publication protects the **record** (context reference plus
generation), not fields or collections inside the context. A
concurrently edited live document can still produce inconsistent
values during a snapshot copy, even when the record's generation is
stable. Published contexts must be treated as read-only in application
code. The ordinary `Response<T>` envelope remains mutable.
