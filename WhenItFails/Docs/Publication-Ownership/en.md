# Exact context publication ownership

Status: **additive store-layer contract; six focused tests await local verification**.

## Why a successful write must return its own publication

`IErrorCatalogContextStore.Set(context)` intentionally returns `void`.
The existing publication reader can retrieve the *current* store record,
but an external writer can publish a newer record immediately after
`Set` returns. This also happens when both writers pass the **same
context object**: comparing context references cannot establish which
writer owns a particular `Generation`.

The default `ErrorCatalogContextStore` now implements the additive
`IErrorCatalogContextPublisher`:

```csharp
using Afrowave.Toolbox.WhenItFails.Catalog;
using Afrowave.Toolbox.WhenItFails.Interfaces;
using Afrowave.Toolbox.WhenItFails.Runtime;

IErrorCatalogContextStore store = /* resolved through DI */;

if (store is IErrorCatalogContextPublisher publisher)
{
    ErrorCatalogContextPublication owned =
        publisher.Publish(new ErrorCatalogContext());

    Console.WriteLine($"{owned.StoreId}/{owned.Generation}");
    // owned is the exact record created by this call, even if another
    // writer replaces the current record immediately after it returns.
}
```

`Publish` executes the same compare/exchange loop as the original store
`Set` and returns the **exact record that won the atomic exchange**.
A later `GetCurrentPublication()` returns the latest record, which may
be another writer's publication. Do not replace `Publish`'s return
value with that later read when recording ownership.

The original `IErrorCatalogContextStore` interface and `Set` signature
are unchanged. On the default store `Set` delegates to `Publish`,
discarding its return value. Both write paths advance the same
monotonically increasing generation counter; a successful re-publish of
the identical context object still creates a distinct record. A rejected
null publication does not advance the counter.

## Thread and compatibility boundaries

Concurrent default-store writers each receive their own successful
publication record with a distinct generation. Atomic publication
protects the record, **not** the live mutable context inside it.
`ErrorCatalogContextPublication.Context` must be treated as read-only
after publication.

This is a **store-level ownership capability**. It does **not** yet
prove that the existing `ErrorCatalogInitializer` or
`ErrorCatalogRuntime` uses that returned record. The initializer
currently invokes legacy `Set` and returns a payload without an
owned publication token. The runtime's optional completed-status
observation still matches a context reference to a subsequently
retrieved publication. Its same-reference ownership gap therefore
remains until the exact token is propagated through initialization,
fallback/reset and previous-context recovery.

Custom `IErrorCatalogContextStore` implementations need not implement
`IErrorCatalogContextPublisher`. A future higher-level ownership-aware
operation must report a defined weaker result when this optional
capability is absent; it must not invent a token from a timestamp,
GUID, reference hash or a later unowned store read.

## Verification and next step

`ExactContextPublicationOwnershipContractTests` adds six focused tests:
exact record identity, later same-reference replacement, legacy/new
write-path generation continuity, 64 concurrent writers receiving
distinct records, null-write preservation, and optional-interface
compatibility.

After local verification, bridge this record through the **default
initializer** to its successful payload and through the runtime's own
fallback/reset write paths. Record the owning write rather than
inferring it from context-reference equality. Preserve the existing
public contracts and explicitly define behavior for custom initializers
that cannot report an owned publication.
