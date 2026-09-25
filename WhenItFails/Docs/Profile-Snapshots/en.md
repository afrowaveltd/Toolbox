# Detached profile catalog snapshots

Status: **additive pre-1.0 CLR API candidate; six focused tests included in maintainer-confirmed 1365/1365 GREEN suite**.

## Purpose and usage

The additive `GetProfileCatalogSnapshot(this IErrorCatalogRuntime)`
extension in `Afrowave.Toolbox.WhenItFails.Runtime` captures a
**detached, read-only view of the supporting profile catalog**:

```csharp
using Afrowave.Toolbox.WhenItFails.Runtime;

var response = runtime.GetProfileCatalogSnapshot();

if (response.IsSuccess && response.Data is { } catalog)
{
    foreach (ErrorProfileDefinitionSnapshot profile in catalog.Profiles)
    {
        Console.WriteLine($"{profile.Name}: {profile.Source}");
    }
}
```

`ErrorProfileCatalogSnapshot` is sealed and getter-only. It captures
all 11 public profile catalog document fields, including separately
allocated read-only document tags, metadata and profile definitions.
`ErrorProfileDefinitionSnapshot` captures all 14 public definition
fields: name, display name, optional description, source, all eight
include/exclude filters (owners, code groups, categories, subcategories,
tags, explicit error IDs, excluded tags and excluded error IDs), default
behavior mappings and metadata.

All lists and dictionaries are copied independently and exposed through
read-only wrappers. The mapping dictionary retains the source
dictionary's key comparer; metadata has independent case-insensitive
keys. No live `ErrorProfileDefinition`, `ErrorProfileCatalogDocument`
or source `MetadataBag` escapes through the returned data. Editing
the source after capture cannot alter a previously returned snapshot.

## Error handling and consistency limits

The extension selects a single active context via one
`GetCurrentContext()` call. An unsuccessful runtime response is
forwarded without snapshot data. A successful context without a profile
catalog returns Invalid with `WIF_PROFILE_SNAPSHOT_CATALOG_NULL`.
Malformed nested source values and ordinary capture errors return
`WIF_PROFILE_SNAPSHOT_FAILED` without exception details or partial
data. Cancellation exceptions propagate.

The outer Essentials `Response<T>` remains mutable. This read-only
projection is **not a transaction** against concurrent in-place
mutation of nested objects in the selected live context. Independently
calling owner, code group, profile or combined snapshot extensions may
select different context generations if another activation occurs
between calls.

The original `GetCombinedSnapshot()` and
`GetCompletedCombinedSnapshot()` keep their existing public
three-part data shape: main definitions, category catalog and recorded
validation. They do **not** silently include profiles, owners or code
groups. The separate additive [all-supporting-catalog view](../Supporting-Catalog-Snapshots/en.md)
selects one context reference and copies category, owner, code-group and
profile catalogs together, without changing those existing three-part types.

The pre-1.0 CLR projection does not establish a versioned JSON wire
contract or promise direct deserialization into its getter-only types.
The published 0.1.0 package and persisted catalog JSON schemas remain
unchanged.

## Verification

`ErrorProfileCatalogSnapshotContractTests` adds six focused cases:
complete filter/nested-data detachment and read-only wrappers; one
context read and retention after activation replacement; uninitialized
runtime response; missing catalog; malformed filter values; and
sealed getter-only public shape without changing the original
nine-method `IErrorCatalogRuntime` interface.
