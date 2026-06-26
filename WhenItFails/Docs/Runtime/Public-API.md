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

Consumers should request the current context from the runtime rather than storing stale copies indefinitely unless snapshot behavior is intentional.

## Thread safety

The runtime publishes the active context and status only after a complete successful activation.

Invalid or partially constructed snapshots are not exposed.

Status updates are atomic.

This allows concurrent consumers to observe either:

```text
the previous valid snapshot
or
the new valid snapshot
```

They should not observe an intermediate partially updated state.

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
