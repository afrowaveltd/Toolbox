# Detached error definition snapshots

Status: **additive pre-1.0 API candidate; first four focused tests and complete 1263/1263 suite confirmed GREEN**. Additional public-shape/JSON verification pending.

## Scope

`IErrorCatalogRuntime.GetErrorDefinitionSnapshots()` is an extension method in
`Afrowave.Toolbox.WhenItFails.Runtime`. It does **not** add a method to
`IErrorCatalogRuntime` or change the existing
`GetCurrentContext(): Response<ErrorCatalogContext>` contract.

The method captures the **main indexed error definitions only**. The response
data is an `IReadOnlyList<ErrorDefinitionSnapshot>`, backed by a separate
read-only collection. Each sealed snapshot exposes all 16 current definition
fields as getter-only properties. Categories, subcategories and tags are
copied into separate read-only collections. Metadata is copied into a separate
read-only dictionary with case-insensitive string keys. No `ErrorDefinition`,
`MetadataBag`, writable collection, or original lookup index is exposed by
the response data.

This **does not** create a deep snapshot of `ErrorCatalogContext`. The
supporting owner/category/code-group/profile documents, cross-validation
result and runtime status are outside this API's scope. The enclosing
`Response<T>` remains the ordinary mutable Essentials response; its status,
message and issues are not claimed to be a deeply immutable result object.

## Usage

```csharp
using Afrowave.Toolbox.WhenItFails.Interfaces;
using Afrowave.Toolbox.WhenItFails.Runtime;

IErrorCatalogRuntime runtime = /* resolved through DI */;
var response = runtime.GetErrorDefinitionSnapshots();

if (response.IsSuccess && response.Data is not null)
{
    foreach (ErrorDefinitionSnapshot definition in response.Data)
    {
        Console.WriteLine($"{definition.Id}: {definition.Message}");
    }
}
```

Call this after successful initialization. A failed context response is
forwarded without exposing a live catalog through the returned data. A
successful response missing the required catalog or its definition list
returns a stable Invalid result. Exceptions from the capture boundary are
converted into a stable failure without including exception text;
`OperationCanceledException` is propagated.

## Public CLR and JSON boundaries

`ErrorDefinitionSnapshot` is a public sealed projection without a public constructor or public setters. Its 16 getter-only properties preserve the current fields of `ErrorDefinition`; `DeveloperHint` and `DocumentationKey` are nullable strings, while the other string fields and the collection/dictionary properties are non-nullable in C# metadata. `GetErrorDefinitionSnapshots` is one public static extension method on `IErrorCatalogRuntime`; no new method is added to the runtime interface.

Default `System.Text.Json` serialization of a captured snapshot produces readable data properties, including arrays for category/subcategory/tag lists and an object for string metadata. This is an **observed, pre-1.0 output shape**, not a fixed wire format or a promise of direct `JsonSerializer.Deserialize<ErrorDefinitionSnapshot>` support: the type has no public deserialization constructor. If a stable wire DTO is needed, design and version it separately from this read-only CLR projection. Its ordinary `Response<T>` envelope retains the established Essentials mutability and serialization contract.

## Ownership and concurrency

The returned **data projection** is independent of later changes to source
definition properties, lists and metadata. A snapshot already returned to
the caller is not retargeted on a later runtime reinitialization; request a
fresh snapshot to observe newly activated definitions.

Capture reads the current context reference once, then copies its
definitions. It is not a lock or a transaction. If another thread mutates
the *same live context or its nested collections during capture*, an
inconsistent capture or failed response is possible. The supported usage
rule remains: do not mutate a published context in place; build and
validate a replacement and activate it through the runtime workflow.
Atomic context-reference replacement and a read-only projection are
different guarantees.

The snapshot preserves the fields found in `IErrorCatalog.GetAll()`; it
does not recalculate index keys, revalidate definitions, or repair a
previously mutated/stale catalog.

## Compatibility and next steps

This is an additive convenience method with no change to the nine-method
`IErrorCatalogRuntime` interface or the existing 0.1.0 package. It is
**not** a statement that a full read-only catalog-context interface, JSON
schema, or version 1.0 compatibility policy has been completed.

Before promising a full immutable consumer context, separately design
snapshots of supporting definitions/catalogs, validation diagnostics,
runtime status, consistency/version identity and the ownership of all
nested collections and metadata. In particular, do not expose a
`ReadOnlyCollection<ErrorDefinition>` as a supposedly safe result:
its element instances are still mutable.
