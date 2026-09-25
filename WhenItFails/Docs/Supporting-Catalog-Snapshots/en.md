# Detached all-supporting-catalog snapshot

Status: **additive pre-1.0 CLR API candidate; 12 focused test cases pending maintainer verification**.

## Scope

`GetSupportingCatalogsSnapshot(this IErrorCatalogRuntime)` selects one active
`ErrorCatalogContext` with a single `GetCurrentContext()` call, then captures
four independent, detached, getter-only projections from that reference:

- `CategoryCatalog`: `ErrorCategoryCatalogSnapshot`.
- `OwnerCatalog`: `ErrorOwnerCatalogSnapshot`.
- `CodeGroupCatalog`: `ErrorCodeGroupCatalogSnapshot`.
- `ProfileCatalog`: `ErrorProfileCatalogSnapshot`.

Every projection uses the established independent copies of nested definitions,
tags, filter lists, mappings and metadata. The sealed outer
`ErrorSupportingCatalogsSnapshot` has four getter-only properties and no
public constructor. It exposes neither live context nor mutable catalog
documents. The enclosing Essentials `Response<T>` remains mutable.

## Usage

```csharp
using Afrowave.Toolbox.WhenItFails.Runtime;

Response<ErrorSupportingCatalogsSnapshot> response =
    runtime.GetSupportingCatalogsSnapshot();

if (response.IsSuccess && response.Data is { } supporting)
{
    Console.WriteLine(supporting.CategoryCatalog.Categories.Count);
    Console.WriteLine(supporting.OwnerCatalog.Owners.Count);
    Console.WriteLine(supporting.CodeGroupCatalog.CodeGroups.Count);
    Console.WriteLine(supporting.ProfileCatalog.Profiles.Count);
}
```

Call after the runtime has an initialized active context.

## Boundaries and error handling

Unlike four separate snapshot calls, this extension selects **one** active
context reference before copying any catalogs. A later active-context
replacement cannot change a previously returned snapshot. This is **not**
a transaction against concurrent *in-place* mutation of nested documents
inside the selected live context. It does not identify a store publication
generation or pair the result with runtime status.

This view intentionally excludes main error definitions and recorded
cross-validation findings. Existing `GetCombinedSnapshot()`,
`GetPublishedCombinedSnapshot()` and
`GetCompletedCombinedSnapshot()` retain their existing three-part
data shape; none is silently extended.

Unsuccessful context responses are forwarded without snapshot data.
Null context responses return Invalid
(`WIF_SUPPORTING_SNAPSHOT_CONTEXT_RESPONSE_NULL`). A successful
context missing one of the four catalogs returns Invalid with
`WIF_SUPPORTING_SNAPSHOT_CATEGORY_CATALOG_NULL`,
`WIF_SUPPORTING_SNAPSHOT_OWNER_CATALOG_NULL`,
`WIF_SUPPORTING_SNAPSHOT_CODE_GROUP_CATALOG_NULL` or
`WIF_SUPPORTING_SNAPSHOT_PROFILE_CATALOG_NULL`, respectively.
Malformed nested data or other ordinary capture errors return Failed
(`WIF_SUPPORTING_SNAPSHOT_FAILED`) without exception details or
partial data. `OperationCanceledException` propagates.

This pre-1.0 CLR projection is not a versioned JSON wire protocol,
a promise of direct deserialization into getter-only DTOs or an update
to the persistent catalog JSON schemas. The published 0.1.0 NuGet
package is unchanged.

## Verification

`ErrorSupportingCatalogsSnapshotContractTests` contains 12 theory-expanded
cases covering deep detachment, single-context selection and replacement,
uninitialized and null responses, four absent catalogs, malformed nested
values, ordinary getter exceptions, exact cancellation propagation and the
getter-only/additive public API surface. Full suite expected: **1377 tests**
once locally confirmed.
