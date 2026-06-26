# Bootstrap and project workspace

WhenItFails uses a project-local JSON workspace for catalogs.

The bootstrap process prepares this workspace before catalog loading begins.

Its responsibilities are deliberately limited:

* create the package directory when it is missing,
* create missing catalog files from bundled templates,
* preserve every existing project file,
* report what was created and what was skipped.

Bootstrap is not catalog validation, migration, repair, or synchronization.

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

The location is controlled by:

```csharp
JsonsOptions
```

Default values:

```csharp
new JsonsOptions
{
    RootDirectory = "Jsons",
    PackageDirectoryName = "WhenItFails",
    ErrorCatalogFileName = "errors.en.json",
    CategoryCatalogFileName = "categories.en.json",
    CodeGroupCatalogFileName = "code-groups.en.json",
    OwnerCatalogFileName = "owners.en.json",
    ProfilesFileName = "profiles.json"
};
```

## Bootstrap service

The workspace is prepared by:

```csharp
IJsonsBootstrapper
```

The default implementation is:

```csharp
JsonsBootstrapper
```

Its main operation is:

```csharp
Task<Response<JsonsBootstrapPayload>> EnsureWorkspaceAsync(
    JsonsOptions options,
    CancellationToken cancellationToken = default);
```

The runtime normally invokes this operation as part of catalog initialization.

## Bootstrap sequence

The bootstrap process follows this sequence:

```text
normalize configured paths
→ check package directory
→ create package directory when missing
→ obtain bundled template files
→ inspect each target file
→ create only missing files
→ preserve existing files
→ return bootstrap report
```

Bootstrap does not load or validate catalog contents.

That happens later in the initialization pipeline.

## Directory creation

The package directory is derived from:

```text
RootDirectory
+
PackageDirectoryName
```

Example:

```csharp
JsonsOptions options = new()
{
    RootDirectory = "Jsons",
    PackageDirectoryName = "WhenItFails"
};
```

Result:

```text
Jsons/WhenItFails
```

When the directory does not exist, bootstrap creates it.

When it already exists, bootstrap keeps it unchanged.

## Path normalization

Configured path values are trimmed before use.

For example:

```text
"  Jsons  "
```

is normalized to:

```text
Jsons
```

Whitespace-only values become empty strings.

Applications should still supply valid explicit paths.

Path normalization is convenience, not configuration repair.

## Template provider

Bootstrap obtains bundled template files through:

```csharp
IJsonsTemplateProvider
```

The default provider is:

```csharp
DefaultJsonsTemplateProvider
```

The provider supplies the contents and target file names for the standard catalogs.

Bundled templates are package-owned read-only resources.

The project copies created from them become project-owned files.

## Missing file behavior

When a target catalog file does not exist:

```text
missing file
→ write bundled template
→ mark file as Created
```

Example result:

```text
Name: Errors
TargetFilePath: Jsons/WhenItFails/errors.en.json
AlreadyExisted: false
Created: true
Skipped: false
Message: File was created from template.
```

The exact template content depends on the package version.

Once created, the project copy is no longer automatically synchronized with future bundled template changes.

## Existing file behavior

When a target file already exists:

```text
existing file
→ do not open for replacement
→ do not compare with template
→ do not merge
→ do not overwrite
→ mark file as Skipped
```

Example result:

```text
Name: Errors
TargetFilePath: Jsons/WhenItFails/errors.en.json
AlreadyExisted: true
Created: false
Skipped: true
Message: File already exists and was not overwritten.
```

This behavior is one of the central safety guarantees of WhenItFails.

## No automatic synchronization

Bootstrap does not attempt to keep project catalogs identical to bundled templates.

It does not:

* compare catalog versions,
* merge newly added built-in definitions,
* remove obsolete project definitions,
* update descriptions or messages,
* replace schema metadata,
* reconcile profile changes,
* rewrite aliases,
* change code ranges.

A project catalog may intentionally differ from the bundled source.

Automatic synchronization would risk destroying project-specific work.

## Shadow copies

Project files created from bundled templates may identify themselves as shadow copies.

Typical metadata includes:

```json
{
  "sourceCatalogId": "afw.when-it-fails.errors",
  "sourceCatalogVersion": "1.0",
  "isShadowCopy": true
}
```

This metadata records origin.

It does not mean that the file remains automatically linked to the bundled resource.

The project copy is editable and independently versioned by the project.

## Bootstrap payload

A successful bootstrap returns:

```csharp
JsonsBootstrapPayload
```

The payload describes:

```text
RootDirectory
PackageDirectoryPath
PackageDirectoryAlreadyExisted
PackageDirectoryCreated
Files
```

This information is useful for:

* startup diagnostics,
* administration screens,
* logs,
* tests,
* installer feedback,
* first-run user messages.

## Directory status

The payload distinguishes between:

```text
PackageDirectoryAlreadyExisted
```

and:

```text
PackageDirectoryCreated
```

These values describe what happened during the current bootstrap operation.

Typical combinations:

```text
AlreadyExisted: true
Created: false
```

or:

```text
AlreadyExisted: false
Created: true
```

## File results

Each template file produces a:

```csharp
JsonsBootstrapFileResult
```

A result describes:

```text
Name
TargetFilePath
AlreadyExisted
Created
Skipped
Message
```

This makes bootstrap behavior observable without requiring the caller to inspect the file system again.

## Partial workspace preparation

Bootstrap processes template files one by one.

If some files already exist and others do not, the resulting workspace may contain both preserved and newly created files.

Example:

```text
errors.en.json
→ already existed, preserved

categories.en.json
→ already existed, preserved

code-groups.en.json
→ missing, created

owners.en.json
→ missing, created

profiles.json
→ already existed, preserved
```

This is expected behavior.

The complete workspace is validated only after bootstrap finishes.

## Bootstrap does not imply valid catalogs

Successful bootstrap means:

```text
the directory exists
+
all required target files exist or were preserved
```

It does not mean:

```text
all JSON is valid
all references are valid
all catalogs are compatible
the context can be activated
```

The following later stages still need to succeed:

```text
load
→ normalize
→ validate documents
→ validate cross-catalog relationships
→ create context
→ activate context
```

## Invalid existing files

When an existing catalog file contains invalid JSON or invalid catalog data, bootstrap still preserves it.

This is intentional.

Bootstrap does not decide that project data is disposable merely because it is invalid.

The later loading or validation phase reports the failure.

Depending on initialization mode, the runtime may then:

* return failure,
* retain a previous valid context,
* activate bundled defaults as fallback.

The invalid project file remains available for diagnosis and repair.

## Access denied

When the process cannot access the workspace, bootstrap returns a structured failure.

Failure code:

```text
JsonsWorkspaceAccessDenied
```

Typical causes include:

* missing directory permissions,
* read-only parent directory,
* denied file creation,
* operating-system access policy,
* container volume permissions,
* service-account restrictions.

The failure message includes the underlying access error message.

## Input/output failure

General file-system failures return:

```text
JsonsWorkspaceInputOutputError
```

Typical causes include:

* disk I/O failure,
* unavailable mount,
* invalid storage state,
* interrupted network share,
* path-related I/O problem,
* file-system corruption,
* exhausted storage.

Bootstrap does not hide these failures.

## Cancellation

Bootstrap accepts a cancellation token.

Cancellation is checked:

* before workspace processing begins,
* before each template file is processed,
* during asynchronous file writing.

Cancellation is not converted into an ordinary bootstrap failure response.

It remains cancellation and must not be hidden as recoverable catalog failure.

## Read-only deployments

A read-only deployment can work when:

* the package directory already exists,
* every required catalog file already exists,
* the process can read those files,
* all catalogs validate successfully.

Bootstrap attempts to create only missing directories and files.

Therefore:

```text
complete existing workspace
+
read access
→ bootstrap can preserve and continue
```

but:

```text
missing file
+
no write access
→ bootstrap fails
```

Production deployment should ensure the workspace is complete before switching it to read-only mode.

## First-run behavior

On first run, bootstrap commonly performs:

```text
create Jsons/WhenItFails
→ create all default catalog files
→ report created files
```

The newly created files are project-local starting points.

The project may then:

* commit them to version control,
* edit them,
* add application errors,
* create custom profiles,
* change mappings,
* add owners or categories,
* validate them through Setter or CI.

## Later-run behavior

On later runs, bootstrap commonly performs:

```text
find existing directory
→ find existing files
→ skip all files
→ continue to loading and validation
```

The process is therefore idempotent with respect to already existing project files.

Repeated bootstrap calls should not repeatedly modify the workspace.

## Bootstrap and package upgrades

A package upgrade may contain newer bundled templates.

Bootstrap does not automatically apply them to an existing project workspace.

This prevents surprise changes but also means that projects must explicitly review template updates.

A future migration or comparison tool may help with:

* template diffing,
* schema migration,
* optional merge proposals,
* new built-in definition discovery,
* profile comparison,
* compatibility checks.

Such operations must remain explicit.

## Runtime bootstrap versus Setter initialization

The runtime bootstrap prepares the minimum required workspace for normal application initialization.

The Setter may provide richer authoring-oriented operations such as:

* workspace inspection,
* explicit initialization,
* validation,
* backups,
* editing,
* summaries,
* import and export,
* future migration support.

These tools may share concepts, but their responsibilities remain different.

The runtime must remain conservative and non-destructive.

## Source control

Project-local catalogs are good candidates for version control.

Recommended practice:

```text
bootstrap workspace once
→ review generated files
→ commit project copies
→ edit through normal development workflow
→ validate in CI
```

Benefits include:

* visible catalog history,
* code review,
* rollback,
* branch-specific definitions,
* deployment reproducibility,
* auditability.

## Backups

Bootstrap does not create backups before skipping or creating files because it never overwrites existing project files.

Authoring tools should create backups before explicit destructive or replacement operations.

Runtime bootstrap and authoring backup policy should not be confused.

## Concurrency

Applications should avoid running multiple independent bootstrap operations against the same new workspace at exactly the same time.

Although existing files are preserved, simultaneous first-run creation may still produce file-system races.

Recommended startup architecture:

```text
one application initialization owner
→ bootstrap once
→ activate runtime
→ allow concurrent runtime consumers
```

Multi-process deployments should provision shared catalog workspaces before starting all instances where possible.

## Security considerations

Catalog workspaces may affect application behavior.

Projects should protect them against unauthorized modification.

Recommended controls include:

* appropriate file permissions,
* controlled deployment ownership,
* version-control review,
* integrity monitoring where required,
* restricted write access in production,
* validation before activation.

Bootstrap creates files using the permissions and identity of the running process.

It does not configure operating-system security policy.

## Recommended bootstrap practices

1. Keep project catalogs under version control.
2. Review generated files after first bootstrap.
3. Use stable workspace paths in production.
4. Provision complete files before enabling read-only mode.
5. Do not expect package upgrades to modify project copies.
6. Use explicit tools for migration and merging.
7. Treat access failures as deployment problems.
8. Validate the complete workspace after bootstrap.
9. Avoid concurrent first-run bootstrap against one directory.
10. Protect production catalogs from unauthorized writes.
11. Preserve bootstrap diagnostics in startup logs.
12. Remember that successful bootstrap does not guarantee valid catalogs.

## Central principle

> Bootstrap may create what is missing, but it must never replace what the project already owns.
