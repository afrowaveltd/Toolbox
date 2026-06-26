# Error descriptors

An `ErrorDescriptor` represents one concrete runtime occurrence of a known error.

It combines stable information copied from an error definition with details that belong only to the current failure.

The central distinction is:

```text
ErrorDefinition
→ reusable catalog definition

ErrorDescriptor
→ one concrete runtime occurrence
```

The definition answers:

```text
What kind of error is this?
```

The descriptor answers:

```text
What happened this time?
```

## Descriptor structure

The main descriptor type is:

```csharp
ErrorDescriptor
```

It contains these properties:

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

## Stable identity

The following values describe the stable identity copied from the catalog definition:

```text
Id
Code
Name
Owner
CodePrefix
CodeGroup
```

These values identify the reusable error type.

They should not normally be changed after the descriptor has been resolved from the catalog.

## ID

Property:

```csharp
descriptor.Id
```

Example:

```text
AFW_NET_0001
```

The ID is the primary stable symbolic identifier.

It is intended for:

* source code,
* logs,
* diagnostics,
* support communication,
* documentation references,
* external integrations.

Applications should not derive error identity from human-readable messages.

## Numeric code

Property:

```csharp
descriptor.Code
```

Example:

```text
600001
```

Numeric codes are useful when integrating with systems that prefer or require numeric identifiers.

Typical uses include:

* log storage,
* telemetry,
* database records,
* protocol responses,
* support systems,
* legacy integrations.

A published numeric code should not later be reused for an unrelated failure.

## Name

Property:

```csharp
descriptor.Name
```

Example:

```text
NETWORKUNAVAILABLE
```

The name is a machine-friendly readable identifier.

It is suitable for code, administration tools, filtering, and diagnostics.

## Owner

Property:

```csharp
descriptor.Owner
```

Example:

```text
AFW
```

The owner identifies who controls and maintains the underlying definition.

Typical owners may include:

```text
AFW
APP
PLUGIN
USER
```

## Code prefix and code group

Properties:

```csharp
descriptor.CodePrefix
descriptor.CodeGroup
```

Example:

```text
CodePrefix: NET
CodeGroup: NETWORK
```

The code prefix forms part of the symbolic identity.

The code group identifies the numeric and semantic family to which the error belongs.

## Classification

A descriptor carries the classification copied from the definition:

```text
PrimaryCategory
Categories
Subcategories
Tags
```

These fields allow consumers to classify, filter, map, and present errors without parsing messages.

## Primary category

Property:

```csharp
descriptor.PrimaryCategory
```

Example:

```text
NETWORK
```

The primary category describes the main logical type of the failure.

## Additional categories

Property:

```csharp
descriptor.Categories
```

Example:

```csharp
descriptor.Categories =
[
    "NETWORK",
    "EXTERNAL_SERVICE"
];
```

An error may belong to several related categories.

## Subcategories

Property:

```csharp
descriptor.Subcategories
```

Example:

```csharp
descriptor.Subcategories =
[
    "CONNECTIVITY",
    "TIMEOUT"
];
```

Subcategories provide finer project-specific classification.

## Tags

Property:

```csharp
descriptor.Tags
```

Example:

```csharp
descriptor.Tags =
[
    "NETWORK",
    "USER_VISIBLE",
    "RETRYABLE"
];
```

Tags support flexible filtering and presentation behavior.

Typical tags may include:

```text
USER_VISIBLE
INTERNAL_ONLY
DEBUG_ONLY
RETRYABLE
FALLBACK
SECURITY
NETWORK
```

Tags are useful, but they should not replace core identity or strongly validated catalog fields.

## Human-readable information

A descriptor carries several human-readable fields:

```text
Title
Message
DeveloperHint
DocumentationKey
```

## Title

Property:

```csharp
descriptor.Title
```

Example:

```text
Network is not available
```

The title should be short and suitable for:

* dialogs,
* headings,
* logs,
* summaries,
* administration screens.

## Message

Property:

```csharp
descriptor.Message
```

Example:

```text
The network is currently unavailable.
```

The message is the default explanation of the error.

It should describe the logical failure without exposing secrets or unnecessary implementation details.

## Developer hint

Property:

```csharp
descriptor.DeveloperHint
```

Example:

```text
Check connectivity, DNS, proxy, VPN, and remote endpoint availability.
```

The developer hint is intended for:

* developers,
* administrators,
* operators,
* support personnel.

It may contain technical guidance that is not appropriate for ordinary end users.

## Documentation key

Property:

```csharp
descriptor.DocumentationKey
```

Example:

```text
when-it-fails/errors/network/network-unavailable
```

The documentation key provides a stable logical reference to extended documentation.

It does not have to be a physical URL or file path.

A consuming application may translate the key into:

* a web page,
* a local help topic,
* a documentation file,
* an administration portal route,
* a support knowledge-base entry.

## Severity

Property:

```csharp
descriptor.Severity
```

Example:

```text
Error
```

The descriptor normally starts with the default severity from the catalog definition.

The severity may be adjusted for a particular runtime occurrence when the context justifies it.

For example, the same definition might represent:

```text
Warning
```

during a recoverable background retry, but:

```text
Error
```

when the same failure prevents application startup.

Severity changes should remain intentional and observable.

## Runtime-specific detail

The following properties describe the concrete occurrence:

```text
Detail
OperationName
ComponentName
SourceName
Metadata
Exception
```

These values should not normally be written back into the reusable catalog definition.

## Detail

Property:

```csharp
descriptor.Detail
```

Example:

```text
The remote endpoint did not respond within 30 seconds.
```

`Detail` should explain what was specific about this occurrence.

The general error type belongs in `Message`.

The concrete circumstances belong in `Detail`.

Bad example:

```text
Network error.
```

This adds no useful information beyond the definition.

Better example:

```text
The request to api.example.com timed out after 30 seconds.
```

## Operation name

Property:

```csharp
descriptor.OperationName
```

Example:

```text
DownloadPackage
```

The operation name identifies what the application was trying to do.

It should describe the logical operation rather than only the low-level method name when possible.

Examples:

```text
LoadConfiguration
SaveUserProfile
DownloadPackage
OpenDatabase
ValidateCatalog
SendNotification
```

## Component name

Property:

```csharp
descriptor.ComponentName
```

Example:

```text
PackageDownloader
```

The component name identifies which subsystem, service, class, worker, or module reported the error.

Examples:

```text
JsonConfigurationLoader
ErrorCatalogRuntime
PackageDownloader
DatabaseConnectionFactory
TranslationWorker
```

## Source name

Property:

```csharp
descriptor.SourceName
```

Example:

```text
appsettings.json
```

The source name identifies the resource related to the failure.

Examples:

```text
appsettings.json
Jsons/WhenItFails/errors.en.json
api.example.com
PrimaryDatabase
Disk /dev/sdb
User profile 42
```

Source names may contain sensitive information.

Presentation layers should decide whether they are safe for the current audience.

## Metadata

Property:

```csharp
descriptor.Metadata
```

Metadata provides extensible structured information for advanced scenarios.

Possible values might include:

```text
trace ID
correlation ID
retry count
HTTP method
HTTP endpoint
database name
device serial
file path
tenant ID
worker name
elapsed time
```

Metadata should be used for information that does not justify a dedicated strongly typed property.

Core identity must not be hidden inside arbitrary metadata.

## Exception

Property:

```csharp
descriptor.Exception
```

The original exception related to the occurrence may be attached to the descriptor.

Example:

```csharp
descriptor.Exception =
    exception;
```

The exception is excluded from ordinary JSON serialization.

This is intentional because exceptions may contain:

* stack traces,
* local file paths,
* user names,
* connection strings,
* credentials,
* tokens,
* SQL statements,
* private implementation details.

An attached exception is diagnostic evidence.

It is not the stable identity of the error.

## Exceptions are not definitions

Different exception types may represent the same logical error.

For example:

```text
HttpRequestException
SocketException
TaskCanceledException
```

may all result in:

```text
NETWORKUNAVAILABLE
```

depending on the runtime context.

Likewise, the same exception type may correspond to different logical errors in different operations.

For that reason, WhenItFails does not use exception types as the primary catalog identity.

## Resolving a descriptor

Descriptors may be resolved through the runtime by:

```text
ID
name
numeric code
```

Examples:

```csharp
var byId =
    runtime.FromId(
        "AFW_NET_0001");
```

```csharp
var byName =
    runtime.FromName(
        "NETWORKUNAVAILABLE");
```

```csharp
var byCode =
    runtime.FromCode(
        600001);
```

All three approaches resolve a catalog definition and create a runtime descriptor.

## Enriching a resolved descriptor

A typical occurrence may be enriched like this:

```csharp
var response =
    runtime.FromName(
        "NETWORKUNAVAILABLE");

if (response.Value is ErrorDescriptor descriptor)
{
    descriptor.OperationName =
        "DownloadPackage";

    descriptor.ComponentName =
        "PackageDownloader";

    descriptor.SourceName =
        "https://packages.example.com/toolbox";

    descriptor.Detail =
        "The remote endpoint did not respond within 30 seconds.";

    descriptor.Exception =
        exception;
}
```

Only occurrence-specific fields should be changed.

Stable identity copied from the catalog should normally remain unchanged.

## Example occurrence

Catalog definition:

```text
ID: AFW_NET_0001
Name: NETWORKUNAVAILABLE
Title: Network is not available
Message: The network is currently unavailable.
Severity: Error
```

Concrete descriptor:

```text
ID: AFW_NET_0001
Name: NETWORKUNAVAILABLE
Title: Network is not available
Message: The network is currently unavailable.
Severity: Error
OperationName: DownloadPackage
ComponentName: PackageDownloader
SourceName: packages.example.com
Detail: The remote endpoint timed out after 30 seconds.
Exception: TaskCanceledException
```

The definition remains reusable.

The descriptor explains what happened in this one case.

## User-facing presentation

A user-facing presentation may use:

```text
Title
Message
Severity
ID
Code
```

Example:

```text
Network is not available

The package could not be downloaded because the network is currently
unavailable.

Error ID: AFW_NET_0001
Code: 600001
```

The application may include the stable ID and code so the user can report the exact problem to support.

## Developer-facing presentation

A developer or administrator view may additionally include:

```text
Detail
OperationName
ComponentName
SourceName
DeveloperHint
DocumentationKey
Metadata
Exception
```

Example:

```text
Error ID: AFW_NET_0001
Code: 600001
Operation: DownloadPackage
Component: PackageDownloader
Source: packages.example.com
Detail: Timeout after 30 seconds
Hint: Check connectivity, DNS, proxy, VPN, and endpoint availability.
```

## Production-safe presentation

Production presentation should normally avoid exposing:

```text
Exception
stack trace
internal file paths
connection strings
tokens
private metadata
implementation names
```

A production profile may recommend mappings such as:

```text
production.includeExceptionDetails = false
production.includeStackTrace = false
production.includeSensitiveMetadata = false
```

The final responsibility remains with the consuming presentation layer.

## Serialization

Most descriptor properties are serializable.

The attached exception is ignored during JSON serialization.

Consumers should still review serialized descriptor content before exposing it publicly, because properties such as:

```text
Detail
SourceName
Metadata
DeveloperHint
```

may contain sensitive values even without the exception.

Serialization safety is therefore broader than simply excluding the exception.

## Mutability

`ErrorDescriptor` is mutable so applications can add runtime details after resolution.

That flexibility should be used carefully.

Recommended rule:

```text
Stable catalog identity
→ preserve

Occurrence-specific context
→ enrich
```

Applications should avoid changing:

```text
Id
Code
Name
Owner
CodePrefix
CodeGroup
```

unless they are deliberately constructing or transforming a descriptor outside the normal catalog runtime.

## Descriptor lifetime

A descriptor represents one occurrence.

It should not normally be reused for unrelated failures.

Bad pattern:

```text
resolve one descriptor
→ mutate it repeatedly
→ use it for many different incidents
```

Preferred pattern:

```text
resolve a fresh descriptor
→ enrich it for one occurrence
→ log, return, display or store it
```

This prevents runtime details from leaking between unrelated errors.

## Concurrency

A descriptor should generally be treated as owned by the operation that created it.

Because it is mutable, sharing the same descriptor instance between concurrent operations may produce inconsistent details.

Each independent failure should receive its own descriptor instance.

## Logging

Structured logging should prefer descriptor fields over message parsing.

Useful log fields include:

```text
ErrorId
ErrorCode
ErrorName
Owner
CodeGroup
PrimaryCategory
Severity
OperationName
ComponentName
SourceName
Tags
TraceId
```

The human-readable message may still be logged, but it should not be the only structured information available.

## Persistence

When storing descriptors, applications should decide whether they need:

```text
the complete occurrence
or
only stable identity plus selected runtime details
```

A compact persistent record may contain:

```text
ErrorId
ErrorCode
OccurredAtUtc
OperationName
ComponentName
Detail
TraceId
```

The catalog may later provide the current title, message, hint, and documentation.

However, systems requiring exact historical wording may also store a snapshot of the descriptor.

## Localization

Stable identity should remain independent of language.

Localized presentation may replace:

```text
Title
Message
DeveloperHint
```

while preserving:

```text
Id
Code
Name
Owner
CodeGroup
Categories
Tags
```

A descriptor should therefore be understood as structured identity plus presentation and occurrence data, not as a single immutable text message.

## Recommended usage rules

1. Resolve descriptors from the active catalog context.
2. Preserve stable identity fields.
3. Add only occurrence-specific details at runtime.
4. Create a fresh descriptor for each independent failure.
5. Treat attached exceptions as sensitive diagnostic evidence.
6. Do not expose developer hints blindly to users.
7. Review detail, source, and metadata before public serialization.
8. Use structured fields for logging and monitoring.
9. Keep the general message in the definition.
10. Keep the concrete circumstances in the descriptor.
11. Do not use messages as machine identity.
12. Do not share mutable descriptor instances between unrelated operations.

## Central principle

> A definition explains what an error means; a descriptor records what happened this time.
