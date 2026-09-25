# Detached cross-validation snapshots

Status: **additive pre-1.0 API candidate; local verification pending**.

## Purpose and usage

The existing `IErrorCatalogRuntime.GetCurrentContext()` returns a shared,
mutable active context. Its `CrossValidationResult` contains mutable issues;
changing an issue's severity changes its live `IsValid`. Additionally,
validation is performed at the time the context is built and is **not**
automatically rerun after later mutation of the source catalog documents.

For a detached record of those validation findings:

```csharp
using Afrowave.Toolbox.WhenItFails.Runtime;

var response = runtime.GetCrossValidationSnapshot();

if (response.IsSuccess && response.Data is { } snapshot)
{
    Console.WriteLine($"Validated at capture: {snapshot.IsValid}");

    foreach (ErrorCatalogValidationIssueSnapshot issue in snapshot.Issues)
    {
        Console.WriteLine($"{issue.Code}: {issue.Message}");
    }
}
```

This is an **additive extension** in
`Afrowave.Toolbox.WhenItFails.Runtime`; no method is added to the
nine-method `IErrorCatalogRuntime` interface. The existing
`GetCurrentContext()` behavior is unchanged.

## Captured data and ownership

`ErrorCatalogValidationSnapshot` is sealed with getter-only `Issues`
and `IsValid` properties. Its issues are held in a privately allocated
read-only collection of `ErrorCatalogValidationIssueSnapshot`. Each issue
snapshot is sealed, has six getter-only properties, and captures
`Severity`, `Code`, `Message`, `ErrorId`, `ErrorName` and `Path`.
No live `ErrorCatalogValidationIssue`, source validation result or
mutable issue collection is returned as response data.

`IsValid` is computed from the **captured issue severities**, so later
edits to live issues, including severity changes, do not affect an
already returned snapshot. Repeated calls create new snapshots and can
return different findings if the active context or its issues changed.

This API reports *historical findings recorded in the active context*,
not a fresh validation of its current mutable catalogs. An earlier
valid result does not prove a catalog remains valid after in-place edits.
The operation is also **not transactional** if external code concurrently
mutates the live issue list or its instances during capture. The
recommended contract remains: do not mutate a published context in place.

## Failures and limits

Call after successful initialization. An unsuccessful context response
is forwarded with no snapshot data. A successful response missing
`CrossValidationResult` returns Invalid with
`WIF_VALIDATION_SNAPSHOT_RESULT_NULL`. Unexpected ordinary capture
errors are returned as a stable failure without exception details, while
`OperationCanceledException` propagates.

The ordinary outer `Response<T>` is still the mutable Essentials
response type. This snapshot does not include the main error
definitions, supporting catalogs, runtime status or a versioned JSON
schema. Its getter-only CLR shape does not imply direct
`JsonSerializer.Deserialize<T>` support; design a versioned wire DTO
separately if a stable exchange format is needed.

See [detached main error definitions](../Definition-Snapshots/en.md)
for the separate `GetErrorDefinitionSnapshots()` projection. Each
extension obtains a context independently; two separate calls are
**not** a single coherent multi-catalog snapshot.
