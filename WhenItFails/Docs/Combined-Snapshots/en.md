# Combined detached catalog snapshot

Status: **additive pre-1.0 API candidate; all eight test cases included in maintainer-confirmed 1286/1286 GREEN suite**.

## Scope

`GetCombinedSnapshot(this IErrorCatalogRuntime)` selects the active
`ErrorCatalogContext` with **one** `GetCurrentContext()` call and
builds three independent, detached read-only data projections from
that selected reference:

- `Definitions`: the main indexed error definitions as an
  `IReadOnlyList<ErrorDefinitionSnapshot>`.
- `CategoryCatalog`: an `ErrorCategoryCatalogSnapshot`, including
  detached category definitions, nested lists, mappings and metadata.
- `Validation`: an `ErrorCatalogValidationSnapshot`, with detached
  issue records and validity calculated from captured severities.

The returned `ErrorCatalogCombinedSnapshot` is sealed with getter-only
properties. It has no public constructor and does not expose any live
`ErrorCatalogContext`, mutable definition, source document or metadata bag.
Its outer `Response<T>` retains the ordinary mutable Essentials envelope.

This is an **additive extension**. The nine existing members of
`IErrorCatalogRuntime` and `GetCurrentContext()` remain unchanged.

## Example

```csharp
using Afrowave.Toolbox.WhenItFails.Runtime;

var response = runtime.GetCombinedSnapshot();

if (response.IsSuccess && response.Data is { } snapshot)
{
    Console.WriteLine($"Error definitions: {snapshot.Definitions.Count}");
    Console.WriteLine($"Categories: {snapshot.CategoryCatalog.Categories.Count}");
    Console.WriteLine($"Recorded validation: {snapshot.Validation.IsValid}");
}
```

Call the method after successful runtime initialization.

## Selected-reference consistency versus activation identity

All three projections are derived from **the same selected context
reference**. The method does not call the three existing individual
snapshot extensions, since each of those independently obtains its own
active context and might select a different activation.

If a replacement is published immediately after the combined method reads
the old context, the method continues capturing the old selected reference.
A subsequent invocation may return data from the newer context. Already
returned projections do not change when the active reference is replaced.

This selected-reference guarantee is **not** a transactional deep snapshot
when another caller mutates the *same published context* during the copy.
It also does not prove that the selected context's historic validation
findings still describe the latest in-place mutated documents.
Applications should treat published contexts and their nested objects as
read-only, and use the supported initialization/reset path to activate
validated replacements.

The original `GetCombinedSnapshot()` has no publication ID. A separate additive [publication-aware combined snapshot](../Published-Snapshots/en.md) captures the same three-part data along with the selected store publication's `StoreId` and `Generation` when the optional runtime/store capability exists; it is not an atomic runtime-status activation ID. One must
be assigned and owned by the runtime activation/publication lifecycle,
including retained-previous-context and reset semantics. A new identifier
generated for each call identifies a capture, **not** the active context
generation. The combined snapshot also does not include runtime status,
owners, code groups or profiles; it must not be described as a full
seven-field context copy.

## Error handling

An unsuccessful context response is forwarded without snapshot data.
A successful context lacking an error catalog, category catalog, or
cross-validation result produces Invalid with, respectively:

- `WIF_COMBINED_SNAPSHOT_ERROR_CATALOG_NULL`
- `WIF_COMBINED_SNAPSHOT_CATEGORY_CATALOG_NULL`
- `WIF_COMBINED_SNAPSHOT_VALIDATION_RESULT_NULL`

A null definition list produces
`WIF_COMBINED_SNAPSHOT_DEFINITIONS_NULL`, while a null context response
produces `WIF_COMBINED_SNAPSHOT_CONTEXT_RESPONSE_NULL`. Unexpected
ordinary capture exceptions produce `WIF_COMBINED_SNAPSHOT_FAILED`
without exposing exception details or partially captured data.
`OperationCanceledException` propagates.

No JSON wire schema, direct deserialization into these getter-only
projection types, or stable version-1.0 public API freeze is implied.

## Related projections

See [definition snapshots](../Definition-Snapshots/en.md),
[category catalog snapshots](../Category-Snapshots/en.md), and
[validation snapshots](../Validation-Snapshots/en.md) for their
independently callable counterparts and additional ownership caveats.
