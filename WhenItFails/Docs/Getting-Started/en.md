# Getting started

This guide shows the shortest practical path from package registration to resolving a structured error.

## 1. Register WhenItFails

```csharp
using Afrowave.Toolbox.WhenItFails.Configuration;
using Afrowave.Toolbox.WhenItFails.Enums;
using Microsoft.Extensions.DependencyInjection;

ServiceCollection services = new();

services.AddWhenItFails(
    new WhenItFailsOptions
    {
        InitializationMode =
            ErrorCatalogInitializationMode.Flexible,

        HideRecoverableFailures =
            false
    });
```

The default project-local workspace is:

```text
Jsons/
└── WhenItFails/
    ├── errors.en.json
    ├── categories.en.json
    ├── code-groups.en.json
    ├── owners.en.json
    └── profiles.json
```

Missing files may be created from bundled templates.

Existing project files are not overwritten automatically.

## 2. Build the service provider

```csharp
using ServiceProvider serviceProvider =
    services.BuildServiceProvider();
```

## 3. Resolve the runtime

```csharp
using Afrowave.Toolbox.WhenItFails.Interfaces;

IErrorCatalogRuntime runtime =
    serviceProvider.GetRequiredService<IErrorCatalogRuntime>();
```

## 4. Initialize the runtime

```csharp
var initializationResponse =
    await runtime.InitializeAsync();
```

Initialization prepares the workspace, loads the catalogs, validates the complete catalog context, and activates it.

The runtime should be initialized before resolving errors.

## 5. Inspect the active runtime state

```csharp
var statusResponse =
    runtime.GetStatus();
```

The status can indicate one of these active states:

```text
ProjectCatalog
PreviousContextRecovery
BuiltInFallback
BuiltInDefaults
Unknown
```

In flexible mode, initialization may still produce a valid active context through recovery.

Applications that care about degraded operation should inspect the runtime status.

## 6. Resolve an error by name

```csharp
var errorResponse =
    runtime.FromName(
        "NETWORKUNAVAILABLE");
```

The runtime may also resolve errors by stable ID:

```csharp
var errorResponse =
    runtime.FromId(
        "AFW_NET_0001");
```

or by numeric code:

```csharp
var errorResponse =
    runtime.FromCode(
        600001);
```

## 7. Add occurrence-specific information

A resolved error descriptor may be enriched with details describing the concrete failure.

```csharp
if (errorResponse.Value is ErrorDescriptor descriptor)
{
    descriptor.OperationName =
        "DownloadPackage";

    descriptor.ComponentName =
        "PackageDownloader";

    descriptor.SourceName =
        "https://example.invalid/package";

    descriptor.Detail =
        "The remote endpoint did not respond within the configured timeout.";
}
```

The catalog definition remains unchanged.

Only the runtime occurrence is enriched.

## 8. Attach an exception

```csharp
try
{
    await DownloadPackageAsync();
}
catch (Exception exception)
{
    var errorResponse =
        runtime.FromName(
            "NETWORKUNAVAILABLE");

    if (errorResponse.Value is ErrorDescriptor descriptor)
    {
        descriptor.Exception =
            exception;

        descriptor.OperationName =
            "DownloadPackage";

        descriptor.ComponentName =
            "PackageDownloader";
    }
}
```

Attached exceptions are excluded from ordinary descriptor JSON serialization.

They may contain sensitive information and should not be exposed directly to users.

## 9. Resolve a profile

```csharp
var profileResponse =
    runtime.ResolveProfile(
        "WEB");
```

Profiles select definitions from the active catalog using owners, code groups, categories, subcategories, and tags.

Typical built-in profiles include:

```text
WEB
API
CLI
DESKTOP
SERVICE
DEVELOPMENT
PRODUCTION
```

## 10. Reset to bundled defaults

The active context may be switched explicitly to bundled defaults:

```csharp
var resetResponse =
    await runtime.ResetToDefaultsAsync();
```

This changes the active in-memory context only.

It does not overwrite or modify project-local catalog files.

## Complete minimal example

```csharp
using Afrowave.Toolbox.WhenItFails.Configuration;
using Afrowave.Toolbox.WhenItFails.Descriptors;
using Afrowave.Toolbox.WhenItFails.Enums;
using Afrowave.Toolbox.WhenItFails.Interfaces;
using Microsoft.Extensions.DependencyInjection;

ServiceCollection services = new();

services.AddWhenItFails(
    new WhenItFailsOptions
    {
        InitializationMode =
            ErrorCatalogInitializationMode.Flexible,

        HideRecoverableFailures =
            false
    });

using ServiceProvider serviceProvider =
    services.BuildServiceProvider();

IErrorCatalogRuntime runtime =
    serviceProvider.GetRequiredService<IErrorCatalogRuntime>();

var initializationResponse =
    await runtime.InitializeAsync();

var statusResponse =
    runtime.GetStatus();

var errorResponse =
    runtime.FromName(
        "NETWORKUNAVAILABLE");

if (errorResponse.Value is ErrorDescriptor descriptor)
{
    descriptor.OperationName =
        "DownloadPackage";

    descriptor.ComponentName =
        "PackageDownloader";

    descriptor.Detail =
        "The remote endpoint could not be reached.";
}
```

## Recommended next reading

* [Configuration](../Configuration/en.md)
* [Catalogs](../Catalogs/en.md)
* [Public runtime API](../Runtime/Public-API.md)
* [Initialization and recovery](../Runtime/Initialization-and-Recovery.md)
* [Design philosophy](../Philosophy/en.md)

## Central principle

> Initialize one validated catalog context, resolve stable error definitions, and add only occurrence-specific details at runtime.
