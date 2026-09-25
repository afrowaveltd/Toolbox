# Detached code group catalog snapshots

Status: **additive pre-1.0 API candidate; six focused tests await local verification**.

## Purpose and usage

`GetCodeGroupCatalogSnapshot(this IErrorCatalogRuntime)` in
`Afrowave.Toolbox.WhenItFails.Runtime` captures the active **supporting code
group catalog** without modifying the original runtime interface:

```csharp
using Afrowave.Toolbox.WhenItFails.Runtime;

var response = runtime.GetCodeGroupCatalogSnapshot();

if (response.IsSuccess && response.Data is { } catalog)
{
    foreach (ErrorCodeGroupDefinitionSnapshot group in catalog.CodeGroups)
    {
        Console.WriteLine(
            $"{group.CodePrefix}: {group.CodeFrom}..{group.CodeTo}");
    }
}
```

`ErrorCodeGroupCatalogSnapshot` is sealed and getter-only, capturing all
11 public catalog-document fields. The code groups, document tags, and
metadata are separately allocated read-only collections or dictionaries.
`ErrorCodeGroupDefinitionSnapshot` is sealed and getter-only, capturing
all ten current definition fields, including code prefix and numeric code
range, default categories/tags, default behavior mappings, and metadata.

The copied mapping dictionary preserves the source dictionary's key
comparer; metadata uses a separate case-insensitive dictionary. A
previously returned snapshot cannot be modified through its list or
dictionary interfaces, and later source mutations do not change its
captured values.

## Failure and consistency boundaries

The extension reads the current context **once**. An unsuccessful context
response is forwarded without snapshot data. A missing code group catalog
returns Invalid with `WIF_CODE_GROUP_SNAPSHOT_CATALOG_NULL`. Unexpected
ordinary capture errors produce `WIF_CODE_GROUP_SNAPSHOT_FAILED` with no
exception details or partially constructed data; cancellation propagates.

The outer Essentials `Response<T>` is still mutable. The detached
projection is **not** an atomic transaction against simultaneous
in-place mutation of the selected live context. Independently obtained
code group, owner, category or combined snapshots may select different
publications if activation occurs between calls. The existing combined
snapshot APIs are **not** silently extended with code groups; their
public data shape remains unchanged.

This pre-1.0 CLR projection does not promise a versioned JSON wire
format or direct deserialization into the getter-only snapshot type.
The published 0.1.0 package and persistent catalog JSON schemas are
unchanged by this source-development step.

## Verification

`ErrorCodeGroupCatalogSnapshotContractTests` adds six focused cases:
deep source detachment and read-only nested collections; selection of
one context per call and retention across replacement; uninitialized
runtime response; missing catalog; malformed nested data; and the sealed
getter-only public shape without modifying `IErrorCatalogRuntime`.
