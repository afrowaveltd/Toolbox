# Detached owner catalog snapshots

Status: **additive pre-1.0 API candidate; six focused tests await local verification**.

## Scope and usage

The `GetOwnerCatalogSnapshot(this IErrorCatalogRuntime)` extension in
`Afrowave.Toolbox.WhenItFails.Runtime` returns a detached copy of the
**supporting owner catalog**, without adding a method to the existing
nine-method runtime interface:

```csharp
using Afrowave.Toolbox.WhenItFails.Runtime;

var result = runtime.GetOwnerCatalogSnapshot();

if (result.IsSuccess && result.Data is { } catalog)
{
    foreach (ErrorOwnerDefinitionSnapshot owner in catalog.Owners)
    {
        Console.WriteLine(
            $"{owner.Name}: {owner.CodeFrom}..{owner.CodeTo}");
    }
}
```

`ErrorOwnerCatalogSnapshot` is sealed and getter-only, capturing all 11
public owner-document fields including independently allocated read-only
`Tags`, `Metadata` and `Owners`. Each
`ErrorOwnerDefinitionSnapshot` is sealed and getter-only and captures
all nine public owner-definition fields: name, display name, optional
description, numeric code range, built-in flag, aliases, default
mappings and metadata.

The copied `DefaultMappings` preserves the source dictionary's key
comparer, while metadata uses independent case-insensitive string keys.
Neither the live `ErrorOwnerCatalogDocument`, mutable owner definitions
nor any original `MetadataBag` instance is reachable through the
returned snapshot data. Changes to the source after capture do not
change a previously returned snapshot.

## Error handling and consistency

The extension calls `GetCurrentContext()` once. An unsuccessful runtime
response is forwarded without snapshot data; a successful context
missing its owner catalog yields Invalid with
`WIF_OWNER_SNAPSHOT_CATALOG_NULL`. Unexpected ordinary capture errors
yield `WIF_OWNER_SNAPSHOT_FAILED` with no exception text or partial
snapshot data. `OperationCanceledException` propagates.

The outer `Response<T>` retains the ordinary mutable Essentials
envelope. The captured data is detached but **not a transactional
snapshot** if other code modifies the same published context in place
while the copy is being created. An independent call to this owner
snapshot method and a separate completed combined snapshot can select
different context generations if another activation occurs between
them.

This first owner snapshot is a separate narrow view. The existing
`GetCombinedSnapshot()` and `GetCompletedCombinedSnapshot()` still
contain only main definitions, categories and recorded validation;
they do **not** automatically add owners or change their public shape.
A future version can explicitly design a complete catalog view using
one selected publication and ownership guarantees appropriate to
the additional supporting documents.

No versioned JSON wire schema, direct deserialization contract, or
published package upgrade is implied by this pre-1.0 CLR projection.

## Verification

`ErrorOwnerCatalogSnapshotContractTests` adds six focused tests:
all-field/nested-data isolation and read-only collections; one-context
capture and independence across activation; uninitialized response;
missing owner document; malformed nested source; and sealed getter-only
public shape without runtime interface changes.
