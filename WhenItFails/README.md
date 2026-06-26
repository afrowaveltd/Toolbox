# Afrowave.Toolbox.WhenItFails

`Afrowave.Toolbox.WhenItFails` provides structured error catalogs, runtime error resolution, profiles, diagnostics, and controlled recovery behavior for .NET applications.

The package is designed for applications that need more than exception messages scattered through source code. Error definitions are stored in project-local JSON catalogs and loaded into a validated runtime context.

## Main goals

* keep error definitions outside application code,
* provide stable error identifiers and numeric codes,
* group errors by categories, owners, code ranges, and profiles,
* resolve errors into consistent descriptors,
* support project-specific catalog customization,
* preserve application availability when catalog initialization fails,
* expose recovery and runtime state without hiding diagnostics,
* never overwrite project catalogs silently.

## Default workspace

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

Bundled catalogs are used as read-only templates and as optional runtime defaults.

Existing project files are never overwritten automatically.

## Registration

Register the package through dependency injection:

```csharp
using Afrowave.Toolbox.WhenItFails.Configuration;
using Afrowave.Toolbox.WhenItFails.Enums;
using Microsoft.Extensions.DependencyInjection;

services.AddWhenItFails(
    new WhenItFailsOptions
    {
        InitializationMode =
            ErrorCatalogInitializationMode.Flexible,

        Jsons = new JsonsOptions
        {
            RootDirectory = "Jsons",
            PackageDirectoryName = "WhenItFails"
        }
    });
```

The main runtime entry point is:

```csharp
IErrorCatalogRuntime
```

It provides initialization, current context access, runtime status information, error resolution, and profile selection.

## Initialization

```csharp
Response<ErrorCatalogInitializationPayload> response =
    await runtime.InitializeAsync();
```

The runtime supports two initialization modes:

* `Strict`
* `Flexible`

Strict initialization fails when the requested project catalog cannot be activated.

Flexible initialization may retain a previously valid context or activate bundled defaults when the project catalog cannot be loaded.

Recovery never overwrites or repairs project JSON files automatically.

## Documentation

- [Getting started](Docs/Getting-Started/en.md)
- [Design philosophy](Docs/Philosophy/en.md)
- [Configuration](Docs/Configuration/en.md)
- [Bootstrap and project workspace](Docs/Bootstrap/en.md)
- [Catalogs](Docs/Catalogs/en.md)
- [Profiles](Docs/Profiles/en.md)
- [Error descriptors](Docs/Descriptors/en.md)
- [Validation](Docs/Validation/en.md)
- [Public runtime API](Docs/Runtime/Public-API.md)
- [Initialization and recovery](Docs/Runtime/Initialization-and-Recovery.md)

## Runtime status

The currently active runtime state can be inspected through:

```csharp
Response<ErrorCatalogRuntimeStatus> response =
    runtime.GetStatus();
```

The status describes:

* the active context source,
* whether recovery mode is active,
* whether a previous context was retained,
* whether bundled defaults were used as fallback,
* the reason for recovery,
* the activation timestamp,
* the configured project workspace path.

## Explicit reset

The active runtime can be switched to bundled defaults explicitly:

```csharp
Response<ErrorCatalogInitializationPayload> response =
    await runtime.ResetToDefaultsAsync();
```

This operation changes the active in-memory context only.

It does not overwrite, delete, or modify project-local catalog files.

## Project status

The package is under active development.

The public API and catalog structure may still evolve before the first stable release.
