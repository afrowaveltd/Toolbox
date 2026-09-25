# Public runtime API

The main application-facing entry point of WhenItFails is:

```csharp
IErrorCatalogRuntime
```

It provides access to:

* runtime initialization,
* the active catalog context,
* runtime status,
* error resolution by ID,
* error resolution by name,
* error resolution by numeric code,
* profile-based error selection,
* explicit activation of bundled defaults.

The runtime owns the currently active catalog context so consuming applications do not need to pass the context manually between individual services.

## Registration

Register WhenItFails through dependency injection:

```csharp
using Afrowave.Toolbox.WhenItFails.Configuration;
using Afrowave.Toolbox.WhenItFails.Enums;
using Microsoft.Extensions.DependencyInjection;

services.AddWhenItFails(
    new WhenItFailsOptions
    {
        InitializationMode =
            ErrorCatalogInitializationMode.Flexible,

        HideRecoverableFailures = false,

        Jsons = new JsonsOptions
        {
            RootDirectory = "Jsons",
            PackageDirectoryName = "WhenItFails"
        }
    });
```

The runtime can then be resolved through:

```csharp
IErrorCatalogRuntime runtime =
    serviceProvider.GetRequiredService<IErrorCatalogRuntime>();
```

## Default registration

The package may also be registered with default options:

```csharp
services.AddWhenItFails();
```

The default behavior uses:

```text
Initialization mode: Flexible
Root directory: Jsons
Package directory: WhenItFails
```

## Configuration callback

Options may be configured through a callback:

```csharp
services.AddWhenItFails(
    options =>
    {
        options.InitializationMode =
            ErrorCatalogInitializationMode.Strict;

        options.Jsons.RootDirectory =
            "Configuration";

        options.Jsons.PackageDirectoryName =
            "Errors";
    });
```

## Configuration section

WhenItFails may also be configured from an application configuration section:

```csharp
services.AddWhenItFails(
    configuration.GetSection("WhenItFails"));
```

Example configuration:

```json
{
  "WhenItFails": {
    "InitializationMode": "Flexible",
    "HideRecoverableFailures": false,
    "Jsons": {
      "RootDirectory": "Jsons",
      "PackageDirectoryName": "WhenItFails",
      "ErrorCatalogFileName": "errors.en.json",
      "CategoryCatalogFileName": "categories.en.json",
      "CodeGroupCatalogFileName": "code-groups.en.json",
      "OwnerCatalogFileName": "owners.en.json",
      "ProfilesFileName": "profiles.json"
    }
  }
}
```

## Initialization

Initialize the runtime before resolving errors:

```csharp
Response<ErrorCatalogInitializationPayload> response =
    await runtime.InitializeAsync();
```

Initialization prepares the project workspace, loads all catalogs, validates the complete context, activates the resulting context, and records the runtime status.

A successful initialization payload contains:

```text
Bootstrap
Context
ContextSource
KeptPreviousContext
UsedFallback
IsDegraded
```

## Initialization with temporary workspace options

A specific initialization call may override the registered JSON workspace:

```csharp
JsonsOptions jsonsOptions = new()
{
    RootDirectory = "AlternativeJsons",
    PackageDirectoryName = "WhenItFails"
};

Response<ErrorCatalogInitializationPayload> response =
    await runtime.InitializeAsync(
        jsonsOptions);
```

This override applies to the initialization call.

It does not change the registered `WhenItFailsOptions` instance.

## Checking the initialization result

The exact `Response<T>` handling depends on the shared Afrowave Toolbox result API.

The important rule is:

```text
Do not use the runtime context until initialization has completed successfully
or completed through an accepted flexible recovery path.
```

After successful activation, the active runtime state may be inspected independently through:

```csharp
Response<ErrorCatalogRuntimeStatus> statusResponse =
    runtime.GetStatus();
```

## Active context

The currently active context is available through:

```csharp
Response<ErrorCatalogContext> contextResponse =
    runtime.GetCurrentContext();
```

Before the first successful activation, this method returns a failure response.

A failed later initialization does not automatically destroy a previously valid active context.

## Detached error definition snapshots

For a read-only **copy of the main indexed error definitions**, without exposing mutable `ErrorDefinition` or `MetadataBag` instances, use the additive runtime extension:

```csharp
using Afrowave.Toolbox.WhenItFails.Runtime;

Response<IReadOnlyList<ErrorDefinitionSnapshot>> snapshotResponse =
    runtime.GetErrorDefinitionSnapshots();
```

Each snapshot owns separate read-only copies of the definition's categories, subcategories, tags and metadata, as well as its captured scalar fields. This is **not** a complete deep snapshot of the active context: supporting catalogs, cross-validation results and runtime status are not included. Do not mutate the published context during capture. See [detached definition snapshots](../Definition-Snapshots/en.md) for ownership, failure handling and consistency limitations.

## Detached cross-validation findings

For a read-only copy of the active context's **recorded validation issues**, use the additive runtime extension:

```csharp
using Afrowave.Toolbox.WhenItFails.Runtime;

Response<ErrorCatalogValidationSnapshot> validationResponse =
    runtime.GetCrossValidationSnapshot();
```

Each issue is captured in a new getter-only projection, and `IsValid` is calculated from captured severities rather than from a live validation result. This does **not** revalidate supporting catalogs, guarantee a transactional capture during in-place mutation or synchronize with a separate call to `GetErrorDefinitionSnapshots()`. See [detached validation snapshots](../Validation-Snapshots/en.md) for the scope and limitations.

## Detached supporting category catalog

The additive `GetCategoryCatalogSnapshot()` extension returns a read-only, detached projection of the current category catalog, including copied category definitions, aliases, parent categories, default tags, mappings and metadata:

```csharp
using Afrowave.Toolbox.WhenItFails.Runtime;

Response<ErrorCategoryCatalogSnapshot> categoryResponse =
    runtime.GetCategoryCatalogSnapshot();
```

This category view is independently captured; it does **not** share a context-generation identifier with separate definition/validation snapshot calls. Refer to [category snapshot ownership and context identity](../Category-Snapshots/en.md). A combined capture and stable activation identity remain separate design steps.

## Combined detached catalog snapshot

To capture the main error definitions, supporting category catalog and recorded cross-validation findings from **one selected active context reference**, use the additive extension:

```csharp
using Afrowave.Toolbox.WhenItFails.Runtime;

Response<ErrorCatalogCombinedSnapshot> combinedResponse =
    runtime.GetCombinedSnapshot();
```

The combined result contains detached, getter-only projections and requires only one `GetCurrentContext()` call. It does **not** include owner/code-group/profile catalogs or runtime status, provide an activation-generation ID, revalidate already mutated documents, or create a transaction against concurrent in-place modification of the selected context. See [combined snapshot ownership](../Combined-Snapshots/en.md).

## Context publication identity (infrastructure)

The default `ErrorCatalogContextStore` also implements the **optional** `IErrorCatalogContextPublicationReader`. It atomically publishes a context reference alongside a `(StoreId, Generation)` pair; `Generation` advances with each successful store `Set`. Existing `IErrorCatalogContextStore` members are unchanged. This record is **not** a detached consumer snapshot: its `Context` is still the live mutable object.

This generation identifies **store publications**, not a synchronized context-plus-runtime-status activation. Normal initialization, reset, fallback and retained-previous-context recovery do not all change the context and status together. `GetCombinedSnapshot()` does not yet expose generation identity. See [context publication identity](../Context-Publication/en.md) for lifecycle and compatibility details.

## Publication-aware combined catalog snapshots

For a detached three-part catalog view with its **actual store publication identity**, use the additive `GetPublishedCombinedSnapshot()` extension:

```csharp
using Afrowave.Toolbox.WhenItFails.Runtime;

Response<ErrorCatalogPublishedCombinedSnapshot> response =
    runtime.GetPublishedCombinedSnapshot();
```

The default runtime implements the optional `IErrorCatalogRuntimePublicationReader`, which reads a single publication from its injected store when that store exposes the optional publication reader. Custom runtimes/stores without this capability return NotSupported instead of fabricated generations. `StoreId` and `Generation` identify the **selected store publication**, not an atomic pairing with runtime status. The original `GetCombinedSnapshot()` remains unchanged. See [publication-aware snapshot documentation](../Published-Snapshots/en.md).

## Completed activation status observation

The default runtime also implements the **optional** `IErrorCatalogRuntimeActivationReader`. `GetCompletedActivation()` returns a getter-only status observation containing the selected store publication's `StoreId` and `Generation`, a separate runtime-local `ActivationSequence`, and the corresponding recorded `ErrorCatalogRuntimeStatus`. Previous-context recovery can advance `ActivationSequence` without publishing a new generation. A custom store without publication support returns NotSupported; a context published without a completed matching status produces a non-success response instead of a fabricated pair.

This is **not** a fully atomic current context-and-status snapshot: readers can race with later external `Set` operations or overlapping runtime initializations. The existing `GetStatus()` and `GetPublishedCombinedSnapshot()` are still independent calls and must not be assumed to match this observation. See [activation status ownership and consistency limits](../Activation-Status/en.md).

## Exact context publication ownership

Infrastructure code that must identify **its own successful store write** can use the default store's optional `IErrorCatalogContextPublisher.Publish(context)`. Unlike calling `Set()` followed by a separate `GetCurrentPublication()`, `Publish()` returns the exact record that won the atomic write, even when a competing writer immediately replaces it with the **same** context reference. The original store interface is unchanged. The default initializer and runtime reset/fallback now propagate their exact owned write record to completed-status observation. Legacy/custom initializers without an owned token and previous-context recovery retain weaker reference-based association; this is still not a globally atomic context/status read. See [exact publication ownership](../Publication-Ownership/en.md).

## Previous-context publication selection

During flexible recovery, the default runtime now selects the previous context **and its existing publication identity in one read** when the store supports `IErrorCatalogContextPublicationReader`. This does not republish the context or increment its generation; the completed-status sequence advances independently. A later external publication, even of the same context object, causes the completed-activation reader to report changed publication instead of attributing the new generation to recovery. A legacy or unavailable optional reader preserves the original weaker recovery path. See [previous-context selection](../Recovery-Selection/en.md).

## Shared-store concurrency boundary

The default activation gate is instance-local. Different runtime instances using the same context store, and direct external calls to `Set`, can publish outside that gate. In particular, same-reference republishes cannot currently be attributed to the original runtime operation by reference comparison alone. See [shared-store concurrency](../Shared-Store-Concurrency/en.md). Neither the completed activation status reader nor separately called snapshot/status methods are a globally atomic transaction.

## Completed combined catalog and status snapshot

The default runtime implements the optional `IErrorCatalogRuntimeCombinedObservationReader`. Its `GetCompletedCombinedSnapshot()` captures the **recorded completed status and detached main definitions, category catalog and validation findings from one selected publication**. It checks the store publication again after copying the data and rejects a changed status or generation without returning a partial snapshot. Custom stores without publication-reader support return NotSupported.

This is a **checked observation**, not a transaction against external writes or in-place mutations of the live context. An external writer may publish again immediately after the final check. See [completed combined snapshots](../Completed-Combined-Snapshots/en.md) for scope, errors and consistency limits.

## Detached supporting owner catalog

The additive `GetOwnerCatalogSnapshot()` extension returns a detached,
getter-only owner catalog view, including owner code ranges, built-in flags,
aliases, default mappings and metadata:

```csharp
using Afrowave.Toolbox.WhenItFails.Runtime;

Response<ErrorOwnerCatalogSnapshot> ownerResponse =
    runtime.GetOwnerCatalogSnapshot();
```

It obtains its own active-context reference once. It is **not** included
automatically in `GetCombinedSnapshot()` or
`GetCompletedCombinedSnapshot()`, which retain their existing public
three-part shape. Separately called snapshots can select different
context generations. See [detached owner catalog snapshots](../Owner-Snapshots/en.md).

## Detached supporting code group catalog

The additive `GetCodeGroupCatalogSnapshot()` extension captures an
independent read-only view of the active code group catalog, including
numeric ranges, code prefixes, default categories/tags, mappings and
metadata:

```csharp
using Afrowave.Toolbox.WhenItFails.Runtime;

Response<ErrorCodeGroupCatalogSnapshot> codeGroups =
    runtime.GetCodeGroupCatalogSnapshot();
```

This method independently selects a context and does not silently extend
the existing combined snapshot APIs, which retain their existing public
shape. See [code group snapshots](../Code-Group-Snapshots/en.md).

## Runtime status

Retrieve the active status snapshot through:

```csharp
Response<ErrorCatalogRuntimeStatus> statusResponse =
    runtime.GetStatus();
```

The status identifies whether the active context came from:

```text
ProjectCatalog
PreviousContext
BuiltInDefaults
```

It also exposes the derived runtime state:

```text
ProjectCatalog
PreviousContextRecovery
BuiltInFallback
BuiltInDefaults
Unknown
```

Applications may use this information for:

* health checks,
* startup diagnostics,
* administration screens,
* logging,
* monitoring,
* degraded-mode warnings.

## Resolving an error by ID

Use:

```csharp
Response<ErrorDescriptor> response =
    runtime.FromId(
        "AFW_NET_0001");
```

The ID is the preferred stable symbolic identity when it is known.

## Resolving an error by name

Use:

```csharp
Response<ErrorDescriptor> response =
    runtime.FromName(
        "NETWORKUNAVAILABLE");
```

Names are useful when application code prefers readable symbolic identifiers.

Name lookup uses normalized catalog values.

## Resolving an error by numeric code

Use:

```csharp
Response<ErrorDescriptor> response =
    runtime.FromCode(
        600001);
```

Numeric codes are useful for:

* external integrations,
* logs,
* support workflows,
* persistence,
* protocol responses,
* compatibility with legacy systems.

## Error descriptors

Resolution produces an:

```csharp
ErrorDescriptor
```

An error descriptor represents one concrete runtime occurrence of a known catalog definition.

It contains stable catalog identity together with fields that may be enriched for a specific occurrence.

Important properties include:

```text
Id
Code
Name
Owner
CodePrefix
CodeGroup
PrimaryCategory
Categories
Subcategories
Title
Message
Severity
Detail
OperationName
ComponentName
SourceName
DeveloperHint
DocumentationKey
Tags
Metadata
Exception
```

## Definition and descriptor

An error definition describes a reusable error type.

An error descriptor describes one concrete occurrence.

Conceptually:

```text
ErrorDefinition
→ stable catalog template

ErrorDescriptor
→ runtime occurrence
```

For example, the catalog definition may say:

```text
Network is not available
```

A concrete descriptor may additionally contain:

```text
OperationName: DownloadPackage
ComponentName: PackageDownloader
SourceName: https://example.invalid/package
Detail: Connection timed out after 30 seconds.
```

## Enriching a descriptor

A resolved descriptor may be enriched with runtime details:

```csharp
Response<ErrorDescriptor> response =
    runtime.FromName(
        "NETWORKUNAVAILABLE");

if (response.Value is ErrorDescriptor descriptor)
{
    descriptor.OperationName =
        "DownloadPackage";

    descriptor.ComponentName =
        "PackageDownloader";

    descriptor.SourceName =
        remoteEndpoint;

    descriptor.Detail =
        "The remote endpoint did not respond within the configured timeout.";

    descriptor.Exception =
        exception;
}
```

The exact success check should follow the conventions of the shared `Response<T>` API used by the application.

## Exception handling

The original exception may be attached to:

```csharp
descriptor.Exception
```

The exception is intentionally excluded from normal JSON serialization.

Exceptions may contain:

* stack traces,
* file paths,
* secrets,
* connection strings,
* tokens,
* private implementation details.

Applications should never expose attached exceptions directly to untrusted users.

## Public and internal information

A descriptor may contain both user-facing and developer-facing information.

Typical public fields include:

```text
Id
Code
Title
Message
Severity
```

Typical internal fields include:

```text
DeveloperHint
Detail
OperationName
ComponentName
SourceName
Exception
Metadata
```

The final presentation layer is responsible for deciding which fields are safe for the current audience and environment.

## Resolving a profile

Use:

```csharp
Response<IReadOnlyList<ErrorDefinition>> response =
    runtime.ResolveProfile(
        "WEB");
```

A profile selects error definitions from the active context using owners, code groups, categories, subcategories, and tags.

Profiles are useful for creating environment-specific or application-specific views such as:

```text
WEB
API
CLI
DESKTOP
SERVICE
DEVELOPMENT
PRODUCTION
```

A custom project profile may also select a specialized domain, for example:

```text
DISK
FILESYSTEM
LOCALIZATION
NETWORKING
```

## Explicit reset to bundled defaults

Use:

```csharp
Response<ErrorCatalogInitializationPayload> response =
    await runtime.ResetToDefaultsAsync();
```

This operation intentionally activates the bundled Afrowave default context.

It does not:

* overwrite project files,
* delete project files,
* repair project files,
* merge defaults into project files.

An explicit reset is recorded as a normal built-in-default state, not as degraded fallback recovery.

## Cancellation

Initialization and reset operations accept a cancellation token:

```csharp
using CancellationTokenSource cancellationSource =
    new(
        TimeSpan.FromSeconds(30));

Response<ErrorCatalogInitializationPayload> response =
    await runtime.InitializeAsync(
        cancellationSource.Token);
```

Cancellation is not considered a recoverable catalog failure and must not be silently hidden.

## Recommended application startup

A typical startup sequence is:

```text
Register WhenItFails services
→ build service provider or application host
→ resolve IErrorCatalogRuntime
→ initialize runtime
→ inspect initialization result
→ inspect runtime status when needed
→ start normal application processing
```

Example:

```csharp
IErrorCatalogRuntime runtime =
    serviceProvider.GetRequiredService<IErrorCatalogRuntime>();

Response<ErrorCatalogInitializationPayload> initializationResponse =
    await runtime.InitializeAsync();

Response<ErrorCatalogRuntimeStatus> statusResponse =
    runtime.GetStatus();
```

The application should decide whether degraded flexible recovery is acceptable for its environment.

For example:

```text
Development
→ fallback may be acceptable with a warning

Production service
→ previous-context recovery may be acceptable temporarily

Security-sensitive startup
→ strict mode may be required
```

## Lifetime

The default dependency-injection registration uses singleton runtime services.

This means the active context and status are shared within the application service provider.

A successful later initialization replaces the active context atomically.

Consumers should request the current context from the runtime rather than storing old references indefinitely. `GetCurrentContext()` returns the **live, shared, mutable** `ErrorCatalogContext` instance. It is not a defensive copy, an immutable snapshot or an ownership transfer. Callers can currently replace its properties or modify objects and collections reachable through them; such changes can affect later runtime lookups and other consumers. Treat the returned context and its contained catalogs as read-only by convention. Do not edit the published context in place; build and validate a replacement through the supported initialization/reset flow.

A reference retained before a later successful activation still points to the old context; it is not automatically retargeted to the newly active instance. Creating a shallow copy of `ErrorCatalogContext` does not isolate the nested catalog objects.

The default indexed `ErrorCatalog` copies its source list membership but retains each mutable `ErrorDefinition` instance. Mutating a definition returned by `GetAll()`, `FindById()` or another lookup can change its displayed fields while the indexes still use keys captured at construction. Editing the source document's `Errors` list does not automatically reindex the active lookup catalog. Normalization creates new definition objects and tag lists, but the default normalizers retain the original document-level and per-definition `MetadataBag` references. Thus even code holding an earlier source document can mutate metadata reachable through a later normalized catalog; normalization is not a deep isolation boundary. Do not modify definitions, tag lists, metadata or source document collections belonging to a published context; build and activate a separate validated context instead.

## Thread safety

The store publishes and replaces the **context reference** atomically. The runtime status is separately published atomically. This lets readers obtain either the previous context reference or the new context reference during a successful replacement, assuming no one mutates a published context in place.

Atomic publication does **not** make the context, its catalog documents or contained definitions deeply immutable or thread-safe for arbitrary concurrent writes. In-place mutations by a publisher or consumer can be seen by other readers and can expose inconsistent combinations of fields. Treat active contexts as read-only after publication; perform updates by constructing and validating a separate context and activating it through the supported runtime flow.

`CrossValidationResult` records findings from the catalog state *at validation time*; it does not automatically revalidate supporting catalogs after a later mutation. Its `IsValid` property recomputes from its **mutable issue objects**, so editing an issue's severity can also change the reported validity. An earlier successful validation is not evidence that an in-place modified published context is still valid. Supporting profile and category definitions contain mutable nested collections and mappings; never use them as an implicit read-only snapshot.

## Failure behavior

Runtime methods return structured failure responses when:

* initialization has not completed,
* an error identity cannot be resolved,
* a profile cannot be resolved,
* catalog initialization fails,
* bundled defaults cannot be activated,
* invalid input is supplied.

Programming errors and internally impossible runtime states may still result in exceptions.

For example, the runtime rejects an internally inconsistent status snapshot rather than publishing it.

## Recommended usage principles

1. Initialize the runtime explicitly.
2. Treat error IDs and numeric codes as stable contracts.
3. Prefer catalog definitions over hard-coded messages.
4. Add occurrence-specific details to descriptors.
5. Do not expose exceptions directly to users.
6. Inspect runtime status when degraded recovery matters.
7. Use strict mode when fallback would be unsafe.
8. Use profiles for environment-specific selection.
9. Keep project catalog editing outside normal runtime operation.
10. Do not assume that fallback modifies project files.

## Design principle

The public runtime API follows one central idea:

> Applications should resolve known errors from one validated active context, while recovery, diagnostics, and catalog ownership remain explicit.
