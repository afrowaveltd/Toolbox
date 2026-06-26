# Troubleshooting

This guide helps diagnose common WhenItFails initialization, loading, validation, and runtime problems.

The recommended investigation order is:

```text
configuration
→ workspace
→ file loading
→ normalization
→ validation
→ runtime status
→ recovery state
```

## Runtime is not initialized

Typical symptoms:

```text
GetCurrentContext fails
GetStatus fails
FromId fails
FromName fails
FromCode fails
ResolveProfile fails
```

Cause:

```text
InitializeAsync was not completed successfully
and no valid context was activated.
```

Check that startup includes:

```csharp
var initializationResponse =
    await runtime.InitializeAsync();
```

Then inspect the returned response before using the runtime.

## Runtime status is unavailable

When no context has ever been activated, `GetStatus()` returns a failure.

Typical failure code:

```text
WIF_RUNTIME_STATUS_UNAVAILABLE
```

This is expected before the first successful initialization or explicit reset.

It does not necessarily indicate an internal runtime defect.

## Project workspace cannot be created

Typical failure codes:

```text
JsonsWorkspaceAccessDenied
JsonsWorkspaceInputOutputError
```

Check:

* directory permissions,
* parent-directory permissions,
* service-account identity,
* container volume configuration,
* free disk space,
* file-system availability,
* network share availability,
* configured root path.

Example configuration:

```csharp
options.Jsons.RootDirectory =
    "/etc/afrowave/jsons";
```

The running process must be able to create missing directories and files unless the workspace is already complete.

## Read-only workspace fails on first run

A read-only workspace works only when every required file already exists.

Required default files:

```text
errors.en.json
categories.en.json
code-groups.en.json
owners.en.json
profiles.json
```

When one is missing, bootstrap attempts to create it.

Without write permission, initialization fails.

Recommended deployment sequence:

```text
create and validate workspace
→ deploy complete files
→ make workspace read-only
→ start application
```

## Catalog file was not found

Typical failure code:

```text
FileNotFound
```

Check:

* `RootDirectory`,
* `PackageDirectoryName`,
* configured file names,
* current working directory,
* relative versus absolute paths,
* deployment packaging,
* file-name casing on Linux.

Example expected path:

```text
Jsons/WhenItFails/errors.en.json
```

A relative path is resolved from the process working directory, which may differ between development, tests, services, and containers.

## Configured file path is empty

Typical failure code:

```text
FilePathIsEmpty
```

Check custom `JsonsOptions` values.

Example invalid configuration:

```csharp
options.Jsons.ErrorCatalogFileName =
    "   ";
```

Use explicit non-empty file names.

## Catalog contains invalid JSON

Typical failure code:

```text
InvalidJson
```

Check for:

* missing commas,
* unmatched braces,
* broken string quotes,
* truncated files,
* invalid escape sequences,
* malformed values.

The loader accepts comments and trailing commas, but the rest of the JSON must still be syntactically valid.

Validate the file with Setter or another JSON-aware editor.

## Catalog document is empty

Typical failure code:

```text
EmptyCatalogDocument
```

This may happen when the file contains:

```json
null
```

or another value that does not produce a catalog object.

This is different from a valid catalog containing an empty collection.

Valid empty example:

```json
{
  "schemaVersion": "1.0",
  "catalogId": "app.errors",
  "catalogName": "Application errors",
  "language": "en",
  "errors": []
}
```

## Catalog loads but validation fails

This means:

```text
the file exists
+
the JSON is readable
+
the document was deserialized
+
the content violates catalog rules
```

Inspect validation issues rather than treating the problem as malformed JSON.

Common issue codes include:

```text
MissingSchemaVersion
MissingCatalogId
MissingErrorId
InvalidErrorCode
MissingErrorName
MissingErrorOwner
MissingErrorCodePrefix
MissingErrorCodeGroup
MissingErrorPrimaryCategory
MissingErrorMessage
MissingDefaultSeverity
UnknownDefaultSeverity
DuplicateErrorId
DuplicateErrorCode
DuplicateErrorName
ErrorIdDoesNotMatchOwnerAndCodePrefix
```

## Duplicate error ID

Typical issue:

```text
DuplicateErrorId
```

Remember that comparison is normalized and case-insensitive.

These may collide:

```text
AFW_NET_0001
afw-net-0001
Afw Net 0001
```

Use one stable canonical ID.

## Duplicate error name

Typical issue:

```text
DuplicateErrorName
```

These may normalize to the same value:

```text
NETWORKUNAVAILABLE
network unavailable
network-unavailable
```

Use one canonical symbolic name.

## Duplicate numeric code

Typical issue:

```text
DuplicateErrorCode
```

Every numeric code must remain unique within the active error catalog.

Do not reuse a published code for a new unrelated error.

## Error ID does not match owner and prefix

Typical issue:

```text
ErrorIdDoesNotMatchOwnerAndCodePrefix
```

Example invalid combination:

```json
{
  "id": "APP_NET_0001",
  "owner": "AFW",
  "codePrefix": "NET"
}
```

Expected ID prefix:

```text
AFW_NET_
```

Correct either:

* the ID,
* the owner,
* the code prefix.

Do not patch only the validation symptom without checking the intended ownership.

## Unknown severity

Typical issue:

```text
UnknownDefaultSeverity
```

Supported values are:

```text
Trace
Debug
Information
Warning
Error
Critical
```

Example invalid value:

```json
"defaultSeverity": "Fatal"
```

Use one supported core severity and map it later if a platform needs different terminology.

## Owner reference is invalid

Typical cause:

```text
error.owner references an owner that does not exist
```

Check:

```text
errors.en.json
→ owner

owners.en.json
→ name
```

Example:

```json
"owner": "APP"
```

requires an owner definition named:

```text
APP
```

## Code group reference is invalid

Check:

```text
error.codeGroup
→ codeGroup.name
```

and:

```text
error.codePrefix
→ codeGroup.codePrefix
```

Example valid pair:

```text
CodeGroup: NETWORK
CodePrefix: NET
```

Example invalid pair:

```text
CodeGroup: NETWORK
CodePrefix: DB
```

## Numeric code is outside owner range

Check the owner definition.

Example:

```json
{
  "name": "APP",
  "codeFrom": 1000000,
  "codeTo": 1999999
}
```

An `APP` error should use a code inside that range.

A code may be valid for a code group but still invalid for its owner.

Both constraints matter.

## Numeric code is outside code-group range

Check the code-group definition.

Example:

```json
{
  "name": "NETWORK",
  "codeFrom": 600000,
  "codeTo": 699999
}
```

A network error using:

```text
700001
```

does not belong to that range.

## Category reference is invalid

Check:

```text
PrimaryCategory
Categories[]
```

against:

```text
categories.en.json
```

A referenced category must exist explicitly.

The runtime does not create missing categories automatically.

## Profile returns too many errors

Remember that include filters use OR logic.

Example:

```json
{
  "includeOwners": [
    "AFW"
  ],
  "includeCategories": [
    "NETWORK"
  ]
}
```

This means:

```text
all AFW errors
OR
all NETWORK errors
```

It does not mean:

```text
AFW-owned NETWORK errors only
```

Use narrower tags, subcategories, or explicit IDs when needed.

## Profile returns no errors

Check:

* profile name,
* active context,
* include filters,
* excluded tags,
* explicit exclusions,
* normalized spelling,
* referenced owners, groups, and categories.

Remember:

```text
ExcludeErrors
→ final veto
```

and:

```text
ExcludeTags
→ veto before inclusion
```

An error explicitly excluded cannot be restored by an include rule.

## Profile cannot be found

Check:

* profile spelling,
* `profiles.json`,
* active catalog context,
* whether fallback defaults are active,
* whether the profile exists only in the project catalog,
* normalization of the profile name.

A project profile may disappear from lookup when the runtime is currently using bundled fallback defaults.

## Error cannot be resolved by name

Check the canonical normalized name.

Example catalog name:

```text
NETWORKUNAVAILABLE
```

Possible input:

```text
network unavailable
```

may normalize correctly, but applications should still prefer the documented canonical name.

Also verify that the active context is the expected one.

## Error cannot be resolved by ID

Check:

* exact intended ID,
* owner,
* code prefix,
* active context source,
* whether the project catalog or bundled defaults are active.

A project-specific error will not exist when the runtime is using only bundled defaults.

## Error cannot be resolved by numeric code

Check:

* code uniqueness,
* active context,
* owner range,
* code-group range,
* whether the code was changed during catalog editing.

Numeric codes should be treated as stable contracts after publication.

## Flexible mode activated previous context

Runtime state:

```text
PreviousContextRecovery
```

This means:

```text
new project initialization failed
+
a previous valid context already existed
+
the runtime kept the previous context
```

The application remains operational, but the latest project changes were not activated.

Inspect:

```text
RecoveryReasonCode
RecoveryStatus
RecoveryMessage
```

Then validate the updated workspace separately.

## Flexible mode activated bundled fallback

Runtime state:

```text
BuiltInFallback
```

This means:

```text
project initialization failed
+
no previous valid context existed
+
bundled defaults were activated
```

Project-specific definitions and profiles are not active.

Do not assume that successful error resolution means the requested project catalog loaded correctly.

Check runtime status.

## Runtime uses built-in defaults without degradation

Runtime state:

```text
BuiltInDefaults
```

This normally means bundled defaults were activated intentionally through:

```csharp
await runtime.ResetToDefaultsAsync();
```

This is not fallback recovery.

It is an explicit caller decision.

## Strict mode failed but old context still works

This is expected.

In strict mode:

```text
new project initialization fails
→ failure is returned
→ previous valid context remains unchanged
```

The failed attempt must not destroy the previously active context.

Inspect the initialization response and do not confuse retained internal state with successful strict initialization.

## Recoverable failure seems hidden

Check:

```csharp
WhenItFailsOptions.HideRecoverableFailures
```

When set to `true`, a successfully recovered failure may be hidden from the normal public result flow.

Recovery diagnostics should still remain available through runtime status or response metadata.

Never rely only on the apparent success of initialization.

Inspect:

```csharp
runtime.GetStatus();
```

## Cancellation appears as an exception

This is expected.

Cancellation is not converted into an ordinary catalog failure.

Typical causes:

* application shutdown,
* caller cancellation,
* timeout cancellation,
* cancelled file operation.

Handle `OperationCanceledException` according to application policy.

Do not report cancellation as invalid JSON or failed validation.

## Existing files were not updated after package upgrade

This is expected bootstrap behavior.

Bootstrap:

```text
creates missing files
preserves existing files
never overwrites project files
```

A package upgrade may contain newer bundled templates, but project copies remain unchanged.

Use an explicit comparison or migration workflow to review template changes.

## Invalid project file was not repaired automatically

This is also expected.

Runtime initialization does not:

* repair JSON,
* remove duplicates,
* renumber errors,
* replace missing references,
* migrate schemas,
* rewrite project files.

Use Setter or another explicit authoring tool.

The runtime remains intentionally conservative.

## Normalization changed the runtime key

Example source:

```text
network-error
```

Normalized runtime value:

```text
NETWORK_ERROR
```

This does not mean the source file was rewritten.

Normalization affects the runtime representation only.

Use canonical values in project files to avoid confusion.

## Display text changed only by trimming

Human-facing text preserves casing.

Example:

```text
"  Network is not available  "
```

becomes:

```text
"Network is not available"
```

It should not become:

```text
NETWORK_IS_NOT_AVAILABLE
```

When display text appears uppercased or otherwise transformed, inspect the authoring or presentation layer rather than the core display-text normalizer.

## Descriptor leaks old runtime details

A descriptor is mutable and represents one occurrence.

Do not reuse one descriptor instance for unrelated failures.

Bad pattern:

```text
resolve once
→ mutate repeatedly
→ share between operations
```

Preferred pattern:

```text
resolve fresh descriptor
→ enrich for one occurrence
→ log or return
→ discard
```

## Sensitive information appears in output

Even though `Exception` is excluded from normal JSON serialization, other fields may still contain sensitive values:

```text
Detail
SourceName
DeveloperHint
Metadata
```

Review the final serialized or displayed output.

Production profiles may recommend hiding sensitive fields, but the consuming presentation layer must enforce that policy.

## Diagnostic checklist

When startup fails, collect:

```text
configured workspace path
initialization mode
initialization response status
failure code
failure message
runtime status
context source
runtime state
recovery reason
catalog file paths
validation issues
active package version
```

Useful runtime calls:

```csharp
var initializationResponse =
    await runtime.InitializeAsync();

var statusResponse =
    runtime.GetStatus();

var contextResponse =
    runtime.GetCurrentContext();
```

## Recommended investigation order

```text
1. Confirm configuration
2. Confirm resolved file paths
3. Confirm directory and file permissions
4. Confirm required files exist
5. Confirm JSON syntax
6. Confirm documents deserialize
7. Review validation issues
8. Review cross-catalog references
9. Inspect runtime status
10. Confirm expected context source
11. Confirm whether recovery occurred
12. Test resolution against the active context
```

## What not to do

Avoid these emergency fixes:

```text
delete project catalogs and retry
copy bundled defaults over project files
renumber errors randomly
change stable IDs without review
hide all recoverable failures
ignore degraded runtime status
expose raw exceptions to users
```

These actions may make the immediate symptom disappear while damaging catalog identity or project-owned data.

## Central principle

> Diagnose the stage that failed before changing the data: path, loading, normalization, validation, activation, and recovery are different problems.
