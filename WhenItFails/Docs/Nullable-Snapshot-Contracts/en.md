# Nullable contracts for detached snapshot consumers

Status: **pre-1.0 CLR API review; six focused reflection tests pending local verification**.

## What the annotations promise

The .NET 10 project builds with nullable reference types enabled. Its detached
snapshot models expose non-nullable, getter-only references for operational data
that is present in a successfully captured snapshot, including indexed
definitions, supporting catalog projections, captured validation and the
recorded runtime status. Nested read-only lists and string metadata/mappings
use non-nullable element, key and value annotations.

Optional document and definition strings retain `string?` metadata:
`Description`, `SourceCatalogId`, `SourceCatalogVersion`,
`DeveloperHint` and `DocumentationKey`, where applicable.
`ErrorCatalogRuntimeStatus.RecoveryReasonCode`,
`RecoveryMessage` and `RecoveryStatus` are optional recovery details,
not guaranteed on a normal activation.

The optional reader interfaces return a non-nullable `Response<T>` object.
That does **not** mean its `Data` is always present. The shared Essentials
envelope has a nullable `Data` payload and allows even `Ok(null)`.
Consumers must examine the result status **and** the payload:

```csharp
using Afrowave.Toolbox.WhenItFails.Interfaces;

if (runtime is IErrorCatalogRuntimeFullObservationReader reader)
{
    var response = reader.GetCompletedFullSnapshot();

    if (response.IsSuccess && response.Data is { } completed)
    {
        Console.WriteLine(completed.Snapshot.Definitions.Count);
        Console.WriteLine(completed.Status.State);
    }
    else
    {
        // Inspect response.Status and response.Issues as appropriate.
    }
}
```

A successful `GetCompletedFullSnapshot()` from the default runtime
supplies its payload after validating the required source components.
An unavailable activation, missing catalog, failed copy, or unsupported
publication reader returns a non-success response with no full-snapshot
payload. Do not replace that with a fabricated empty context.

## Source integrity is a different contract

C# non-nullable metadata is a *consumer-facing compile-time annotation*,
not runtime validation of an arbitrary external implementation or an
externally mutated live context. The published source context and its
nested documents are mutable and can be changed by external writers.
Do not interpret reflection-based nullability checks as a transaction or
as proof that unvalidated source scalar values cannot be null.

A successfully returned detached snapshot does not subsequently track
replacements of the active context. Its getter-only projection and
read-only copied collections do not imply deep immutability of the
outer Essentials response or the recorded `ErrorCatalogRuntimeStatus`
object's existing init-only members.

## Verification and compatibility

`SnapshotNullableContractTests` adds **six cases** checking reflected
nullable metadata for completed observations, complete/supporting data
projections, nested list/dictionary elements, optional catalog/definition
fields, optional reader responses and runtime recovery details. It
separately documents the existing Essentials `Ok(null)` behavior.

No existing public signature, nullability annotation, serialization
schema, or published NuGet 0.1.0 package is modified by this checkpoint.
Last maintainer-confirmed complete suite: **1429/1429 GREEN**.
Expected next complete suite after the six cases: **1435/1435 GREEN**.
