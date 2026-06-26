# Configuration

WhenItFails may be configured through dependency injection, application configuration, or explicit options supplied during initialization.

The main configuration type is:

```csharp
WhenItFailsOptions
```

It controls:

* initialization behavior,
* recoverable failure visibility,
* project-local JSON workspace paths.

## Main configuration structure

```csharp
public sealed class WhenItFailsOptions
{
    public JsonsOptions Jsons { get; set; } = new();

    public ErrorCatalogInitializationMode InitializationMode { get; set; }
        = ErrorCatalogInitializationMode.Flexible;

    public bool? HideRecoverableFailures { get; set; }
}
```

## Default configuration

The default configuration is equivalent to:

```csharp
new WhenItFailsOptions
{
    InitializationMode =
        ErrorCatalogInitializationMode.Flexible,

    HideRecoverableFailures = null,

    Jsons = new JsonsOptions
    {
        RootDirectory = "Jsons",
        PackageDirectoryName = "WhenItFails",
        ErrorCatalogFileName = "errors.en.json",
        CategoryCatalogFileName = "categories.en.json",
        CodeGroupCatalogFileName = "code-groups.en.json",
        OwnerCatalogFileName = "owners.en.json",
        ProfilesFileName = "profiles.json"
    }
};
```

This produces the default workspace:

```text
Jsons/
└── WhenItFails/
    ├── errors.en.json
    ├── categories.en.json
    ├── code-groups.en.json
    ├── owners.en.json
    └── profiles.json
```

## Dependency injection registration

### Default options

```csharp
services.AddWhenItFails();
```

### Options instance

```csharp
services.AddWhenItFails(
    new WhenItFailsOptions
    {
        InitializationMode =
            ErrorCatalogInitializationMode.Strict
    });
```

### Configuration callback

```csharp
services.AddWhenItFails(
    options =>
    {
        options.InitializationMode =
            ErrorCatalogInitializationMode.Flexible;

        options.HideRecoverableFailures =
            false;

        options.Jsons.RootDirectory =
            "Jsons";

        options.Jsons.PackageDirectoryName =
            "WhenItFails";
    });
```

### Configuration section

```csharp
services.AddWhenItFails(
    configuration.GetSection(
        "WhenItFails"));
```

Example `appsettings.json`:

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

## Initialization mode

Configuration property:

```csharp
WhenItFailsOptions.InitializationMode
```

Supported values:

```csharp
ErrorCatalogInitializationMode.Strict
ErrorCatalogInitializationMode.Flexible
```

## Strict mode

Strict mode requires the requested project-local catalog context to initialize successfully.

```text
Project catalog succeeds
→ activate project context

Project catalog fails
→ return failure
→ keep any previously valid context unchanged
→ do not activate bundled fallback
```

Strict mode is appropriate when using a different catalog context would be unsafe or misleading.

Typical examples include:

* security-sensitive services,
* regulated environments,
* applications requiring exact catalog versions,
* startup checks where fallback would hide deployment errors.

## Flexible mode

Flexible mode allows the runtime to recover from project catalog failure.

The recovery order is:

```text
Project catalog succeeds
→ activate project context

Project catalog fails and previous context exists
→ retain previous context

Project catalog fails and no previous context exists
→ activate bundled defaults

Bundled defaults also fail
→ return failure
```

Flexible mode favors availability while keeping recovery visible through runtime status and diagnostics.

## Hide recoverable failures

Configuration property:

```csharp
WhenItFailsOptions.HideRecoverableFailures
```

Possible values:

```text
null
true
false
```

### `null`

No explicit override was supplied.

The runtime uses its safe default behavior.

### `false`

Recoverable initialization failures remain visible in the normal result flow.

The application may still continue when a valid recovery context was activated or retained.

This is useful during development, diagnostics, and administration.

### `true`

Successfully recovered failures may be hidden from the normal public result flow.

This may apply only when the runtime recovered by:

* retaining a previously valid context,
* activating valid bundled defaults.

The following must never be hidden:

* unrecoverable failures,
* cancellation,
* invalid API usage,
* fatal runtime failures,
* failure to activate any valid context.

Recovery details remain available through runtime status and diagnostic metadata.

## JSON workspace configuration

The project-local workspace is configured through:

```csharp
JsonsOptions
```

The configuration type contains:

```csharp
public sealed class JsonsOptions
{
    public string RootDirectory { get; set; }
        = "Jsons";

    public string PackageDirectoryName { get; set; }
        = "WhenItFails";

    public string ErrorCatalogFileName { get; set; }
        = "errors.en.json";

    public string CategoryCatalogFileName { get; set; }
        = "categories.en.json";

    public string CodeGroupCatalogFileName { get; set; }
        = "code-groups.en.json";

    public string OwnerCatalogFileName { get; set; }
        = "owners.en.json";

    public string ProfilesFileName { get; set; }
        = "profiles.json";
}
```

## Root directory

Property:

```csharp
JsonsOptions.RootDirectory
```

Default:

```text
Jsons
```

This is the root directory containing JSON workspaces used by Afrowave Toolbox packages.

Example:

```csharp
options.Jsons.RootDirectory =
    "Configuration";
```

Result:

```text
Configuration/
└── WhenItFails/
```

The path may be relative or absolute, depending on the consuming application.

Relative paths are resolved according to the application's current working directory.

Applications should avoid relying on an uncertain working directory in production environments.

## Package directory name

Property:

```csharp
JsonsOptions.PackageDirectoryName
```

Default:

```text
WhenItFails
```

This directory separates WhenItFails catalogs from JSON data belonging to other Toolbox packages.

Example:

```csharp
options.Jsons.PackageDirectoryName =
    "Errors";
```

Result:

```text
Jsons/
└── Errors/
```

## Catalog file names

The individual catalog file names may be changed independently.

```csharp
options.Jsons.ErrorCatalogFileName =
    "application-errors.en.json";

options.Jsons.CategoryCatalogFileName =
    "application-categories.en.json";

options.Jsons.CodeGroupCatalogFileName =
    "application-code-groups.en.json";

options.Jsons.OwnerCatalogFileName =
    "application-owners.en.json";

options.Jsons.ProfilesFileName =
    "application-profiles.json";
```

Result:

```text
Jsons/
└── WhenItFails/
    ├── application-errors.en.json
    ├── application-categories.en.json
    ├── application-code-groups.en.json
    ├── application-owners.en.json
    └── application-profiles.json
```

Changing file names does not change the internal identity of the catalogs.

Catalog identity remains defined by fields such as:

```text
catalogId
schemaVersion
sourceCatalogId
sourceCatalogVersion
```

## Calculated paths

`JsonsOptions` provides calculated path properties.

### Package directory

```csharp
options.Jsons.PackageDirectoryPath
```

Equivalent to:

```csharp
Path.Combine(
    options.Jsons.RootDirectory,
    options.Jsons.PackageDirectoryName);
```

### Error catalog

```csharp
options.Jsons.ErrorCatalogFilePath
```

### Category catalog

```csharp
options.Jsons.CategoryCatalogFilePath
```

### Code-group catalog

```csharp
options.Jsons.CodeGroupCatalogFilePath
```

### Owner catalog

```csharp
options.Jsons.OwnerCatalogFilePath
```

### Profiles catalog

```csharp
options.Jsons.ProfilesFilePath
```

These calculated properties should be preferred over manually combining path segments elsewhere in application code.

## Per-call workspace override

The registered workspace may be overridden for one initialization call:

```csharp
JsonsOptions alternateWorkspace = new()
{
    RootDirectory =
        "AlternativeJsons",

    PackageDirectoryName =
        "WhenItFails"
};

Response<ErrorCatalogInitializationPayload> response =
    await runtime.InitializeAsync(
        alternateWorkspace);
```

This override applies only to the initialization request.

It does not replace or mutate the registered `WhenItFailsOptions`.

Typical uses include:

* testing an alternative catalog,
* administrative validation,
* migration checks,
* temporary staging workspaces,
* isolated integration tests.

## Configuration snapshot behavior

When options are registered, WhenItFails creates its own options snapshot.

This prevents later external mutation of the original options object from unexpectedly changing runtime configuration.

For example:

```csharp
WhenItFailsOptions options = new()
{
    InitializationMode =
        ErrorCatalogInitializationMode.Strict
};

services.AddWhenItFails(
    options);

options.InitializationMode =
    ErrorCatalogInitializationMode.Flexible;
```

The later mutation should not silently change the configuration already registered in the service collection.

Applications should treat registered options as startup configuration rather than as a live mutable control surface.

## Environment-specific configuration

Configuration may vary by environment.

Example development configuration:

```json
{
  "WhenItFails": {
    "InitializationMode": "Flexible",
    "HideRecoverableFailures": false
  }
}
```

Example production configuration:

```json
{
  "WhenItFails": {
    "InitializationMode": "Strict",
    "HideRecoverableFailures": false,
    "Jsons": {
      "RootDirectory": "/etc/afrowave/jsons",
      "PackageDirectoryName": "WhenItFails"
    }
  }
}
```

The correct policy depends on whether availability or exact catalog correctness is more important for the application.

## Relative and absolute paths

A relative root directory is convenient during development:

```text
Jsons
```

An absolute root directory may be safer for deployed services:

```text
/etc/afrowave/jsons
```

or:

```text
C:\ProgramData\Afrowave\Jsons
```

Applications should ensure that:

* the process can read the workspace,
* bootstrap has write permission when missing files may be created,
* deployment tools preserve project-owned catalogs,
* backups include the catalog workspace,
* the working directory cannot unexpectedly redirect relative paths.

## Read-only deployments

When the project workspace is intentionally read-only, all required catalog files should already exist before initialization.

Bootstrap cannot create missing files without write permission.

A read-only deployment may still work when:

* every required project catalog exists,
* all files are readable,
* all catalogs validate successfully.

Bundled defaults remain package resources and do not require write access to the project workspace when used directly as fallback or explicit defaults.

## Configuration and file ownership

Configuration chooses where catalogs are located.

It does not transfer ownership of existing files to the runtime.

Existing project-local catalog files remain user-managed data.

The runtime must not:

* overwrite them automatically,
* replace them during fallback,
* rewrite them because defaults changed,
* delete unknown project definitions,
* silently migrate their schema.

## Configuration validation

Invalid configuration may include:

* empty required file names,
* unusable directory paths,
* conflicting paths,
* inaccessible workspace locations,
* unsupported initialization mode values.

Configuration problems should be reported explicitly.

They must not be disguised as ordinary catalog validation failures when the failure is actually caused by an invalid workspace configuration.

## Recommended configuration principles

1. Use stable workspace paths in production.
2. Prefer absolute paths for long-running services.
3. Keep catalog files under version control where appropriate.
4. Do not mutate registered options after service registration.
5. Use strict mode when fallback would be unsafe.
6. Use flexible mode when continued availability is preferred.
7. Keep recoverable failures visible during development.
8. Do not use hidden failures as a substitute for monitoring.
9. Back up project-local catalogs.
10. Treat catalog location changes as deployment changes.
11. Validate alternative workspaces before activation.
12. Keep runtime configuration separate from catalog content.

## Central principle

Configuration in WhenItFails follows one rule:

> The application chooses where catalogs live and how recovery behaves, while the project remains the owner of its catalog data.
