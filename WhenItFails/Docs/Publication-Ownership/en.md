# Exact context publication ownership

Status: **additive store-layer contract; default initializer/runtime owned-write bridge included in maintainer-confirmed 1336/1336 GREEN suite; no-write recovery selection verification pending**.

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

The default `ErrorCatalogInitializer` now uses `Publish` when the
injected store supports it, retaining the **exact returned record** in an
internal-only `ErrorCatalogInitializationPayload.OwnedPublication` property.
The default runtime similarly uses `Publish` for its own explicit reset
and automatic fallback writes and passes the returned record into
`RecordStatus`. For these **owned write paths**, `RecordStatus` no longer
substitutes a later current-publication read with matching context
reference. If an external writer republishes even the identical context
before status recording, the subsequent `GetCompletedActivation()` check
rejects the newer record as `WIF_ACTIVATION_PUBLICATION_CHANGED` rather
than attributing it to the earlier write. The original status flow and
initialization result remain unchanged.

The owned-publication property is **internal**, not part of the public
payload surface, JSON output, or the published 0.1.0 package. For legacy
stores without `IErrorCatalogContextPublisher`, the initializer and
runtime still use the original `Set` path. Custom initializers that do not report an owned record still use a
**best-effort** current-publication reference match. For no-write
previous-context recovery, the default runtime now selects the existing
publication and its context in one read when the optional publication
reader is available. That record is retained separately as an internal
`SelectedPublication`: selecting an existing publication is **not**
owning a new write. Recovery on legacy or failing readers retains the
weaker best-effort path. See [previous-context selection](../Recovery-Selection/en.md).

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

After local verification, review an optional owned-initializer capability
for custom implementations and design a coherent combined status/data read.
Do not claim globally atomic data/status reads: external writers can
still replace the store after the identity check.
