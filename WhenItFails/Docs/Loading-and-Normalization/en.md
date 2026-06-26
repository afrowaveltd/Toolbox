# Loading and normalization

WhenItFails loads project catalogs from JSON files and then normalizes selected values before validation and runtime use.

These stages have different responsibilities:

```text
Loading
→ read and deserialize source documents

Normalization
→ create consistent runtime representations

Validation
→ decide whether the resulting catalog context can be trusted
```

Loading does not prove that a catalog is valid.

Normalization does not repair semantic mistakes.

Validation remains the stage that accepts or rejects the complete context.

## Loading pipeline

The catalog loading sequence is conceptually:

```text
locate configured file
→ open file for reading
→ deserialize JSON document
→ return structured loading result
→ normalize loaded document
→ validate document and cross-catalog relationships
```

Each catalog type has its own loader, while the common JSON reading behavior is shared.

## Shared JSON loader

The common loader is:

```csharp
JsonCatalogDocumentLoader
```

Concrete catalog loaders use it to deserialize their document types.

For example:

```csharp
JsonErrorCatalogLoader
```

loads:

```csharp
ErrorCatalogDocument
```

through the shared generic loader.

This keeps file handling and JSON behavior consistent across:

* error catalogs,
* category catalogs,
* code-group catalogs,
* owner catalogs,
* profile catalogs.

## Loading an error catalog

The error catalog loader exposes:

```csharp
Task<Response<ErrorCatalogDocument>> LoadFromFileAsync(
    string filePath,
    CancellationToken cancellationToken = default);
```

A successful response contains the deserialized document.

A failed response contains a stable code and human-readable explanation.

## File-path handling

The supplied file path is checked before the file is opened.

A null, empty, or whitespace-only path produces:

```text
FilePathIsEmpty
```

Example:

```csharp
await loader.LoadFromFileAsync(
    "   ");
```

The path is trimmed before use.

For example:

```text
"  Jsons/WhenItFails/errors.en.json  "
```

becomes:

```text
Jsons/WhenItFails/errors.en.json
```

Trimming is convenience only.

It does not make an otherwise invalid path valid.

## Missing files

When the configured file does not exist, loading returns a not-found response.

Failure code:

```text
FileNotFound
```

Example message:

```text
JSON catalog file was not found:
Jsons/WhenItFails/errors.en.json
```

A missing file is different from invalid JSON.

This distinction is important for diagnostics and recovery policy.

## Empty deserialized document

A file may exist and be readable but deserialize to no document.

This produces:

```text
EmptyCatalogDocument
```

This means the file did not produce a usable catalog object.

It is not the same as a valid catalog containing an empty `errors` collection.

For example:

```json
null
```

may deserialize to no document.

By contrast:

```json
{
  "schemaVersion": "1.0",
  "catalogId": "example",
  "errors": []
}
```

is a document that can proceed to validation.

## Invalid JSON

Malformed JSON produces:

```text
InvalidJson
```

Typical causes include:

* missing comma,
* unmatched brace,
* invalid string quoting,
* malformed number,
* truncated file,
* invalid token.

Example invalid JSON:

```json
{
  "schemaVersion": "1.0",
  "catalogId": "example"
  "errors": []
}
```

The loader includes the JSON parser message in the structured failure response.

## Access denied

When the process cannot read the file, loading returns:

```text
AccessDenied
```

Typical causes include:

* insufficient file permissions,
* denied directory traversal,
* restricted service account,
* locked-down container volume,
* operating-system policy.

This is an environment or deployment failure, not catalog validation failure.

## Input/output failure

General file-system reading failures produce:

```text
InputOutputError
```

Typical causes include:

* disk failure,
* unavailable mount,
* disconnected network share,
* file-system corruption,
* transient device error,
* storage interruption.

Loading preserves the distinction between malformed catalog content and infrastructure failure.

## Cancellation

Catalog loading accepts a cancellation token.

Cancellation is checked before file processing begins and during asynchronous deserialization.

Cancellation is rethrown as cancellation.

It is not converted into:

```text
InvalidJson
```

or:

```text
InputOutputError
```

This allows application shutdown and timeout policy to remain explicit.

## JSON parsing behavior

The shared JSON loader uses consistent serializer behavior.

### Case-insensitive property names

JSON property-name matching is case-insensitive.

For example, the loader may recognize both:

```json
{
  "catalogId": "example"
}
```

and:

```json
{
  "CatalogId": "example"
}
```

Authors should still use the canonical property names shown in the documentation and generated templates.

Case-insensitive loading is compatibility assistance, not a reason to use inconsistent formatting.

## Comments

JSON comments are skipped during reading.

This allows project files to contain comments accepted by the configured loader.

Example:

```json
{
  // Project-specific error catalog
  "schemaVersion": "1.0",
  "catalogId": "app.errors",
  "errors": []
}
```

Projects should remember that not every external JSON tool accepts comments.

Catalogs intended for broad interchange may prefer strict comment-free JSON.

## Trailing commas

Trailing commas are accepted.

Example:

```json
{
  "schemaVersion": "1.0",
  "catalogId": "app.errors",
  "errors": [],
}
```

Again, this is loader convenience.

Portable JSON intended for unrelated tools may avoid trailing commas.

## Loading is read-only

The loading stage reads project files.

It does not:

* change formatting,
* remove comments,
* rewrite property names,
* save normalized values,
* update schema versions,
* repair malformed JSON,
* add missing fields,
* replace project files.

The source file remains exactly as owned by the project.

## Why normalization exists

Human-authored catalogs may contain superficial formatting differences.

Examples:

```text
network
NETWORK
Network
network-error
Network error
```

Some values are intended to behave as stable symbolic keys.

Normalization converts those values into a consistent runtime form so comparison and lookup are predictable.

## Key normalization

The shared key normalizer is:

```csharp
TextKeyNormalizer
```

Its main operation is:

```csharp
NormalizeKey(...)
```

Key normalization follows these rules:

```text
trim surrounding whitespace
→ convert letters to invariant uppercase
→ replace each group of non-alphanumeric characters with one underscore
→ remove leading and trailing underscores
```

## Key-normalization examples

```text
Network error
→ NETWORK_ERROR
```

```text
network-error
→ NETWORK_ERROR
```

```text
network.error
→ NETWORK_ERROR
```

```text
HTTP 404
→ HTTP_404
```

```text
  APP / STORAGE  
→ APP_STORAGE
```

```text
___network___
→ NETWORK
```

Null or whitespace-only input becomes:

```text
empty string
```

Validation later decides whether an empty normalized value is permitted.

## Display-text normalization

Human-facing values use a different operation:

```csharp
NormalizeDisplayName(...)
```

Display-text normalization:

```text
trims surrounding whitespace
preserves letter casing
does not convert punctuation into underscores
```

Example:

```text
"  Network is not available  "
```

becomes:

```text
"Network is not available"
```

It does not become:

```text
NETWORK_IS_NOT_AVAILABLE
```

This distinction prevents machine-key rules from damaging user-facing text.

## Identity fields

Error-definition normalization treats these as stable keys:

```text
Id
Name
Owner
CodePrefix
CodeGroup
PrimaryCategory
Categories
Subcategories
Tags
```

Example input:

```json
{
  "id": " afw-net-0001 ",
  "name": " network unavailable ",
  "owner": " afw ",
  "codePrefix": " net ",
  "codeGroup": " network ",
  "primaryCategory": " network "
}
```

Normalized runtime values become conceptually:

```text
Id: AFW_NET_0001
Name: NETWORK_UNAVAILABLE
Owner: AFW
CodePrefix: NET
CodeGroup: NETWORK
PrimaryCategory: NETWORK
```

The exact source JSON is not rewritten.

## Human-facing fields

These fields are treated as display text:

```text
Title
Message
DefaultSeverity
DeveloperHint
DocumentationKey
```

They are trimmed without arbitrary uppercasing.

Example:

```json
{
  "title": "  Network is not available  ",
  "message": "  The network is currently unavailable.  "
}
```

becomes in the normalized runtime copy:

```text
Title: Network is not available
Message: The network is currently unavailable.
```

## Normalized copies

Normalization creates normalized copies rather than mutating the original source object in place.

For an error definition, the normalized copy preserves:

```text
numeric code
metadata
human meaning
```

while canonicalizing selected keys and trimming display fields.

This design makes the transformation explicit and easier to test.

## Collection normalization

String collections such as:

```text
Categories
Subcategories
Tags
```

are normalized item by item.

Example:

```json
{
  "tags": [
    " user visible ",
    "network-error",
    " Retryable "
  ]
}
```

becomes conceptually:

```text
USER_VISIBLE
NETWORK_ERROR
RETRYABLE
```

Normalization does not by itself guarantee uniqueness.

Validation still detects duplicate normalized values.

## Duplicate values after normalization

Two source values may appear different but normalize to the same key.

Example:

```json
{
  "tags": [
    "NETWORK ERROR",
    "network-error"
  ]
}
```

Both normalize to:

```text
NETWORK_ERROR
```

Validation may then report:

```text
DuplicateTag
```

This is why duplicate detection must happen after or with awareness of normalization.

## Normalization and stable identity

Normalization makes matching tolerant of superficial formatting differences.

It does not mean that authors should treat stable identity casually.

Once a published error uses:

```text
AFW_NET_0001
```

authors should continue writing exactly that canonical form.

Depending on normalization to reinterpret arbitrary spelling changes makes catalog review harder and may hide accidental renaming.

## Normalization is not translation

Normalization never translates text between languages.

It does not turn:

```text
Síť není dostupná
```

into:

```text
Network is not available
```

Localization belongs to language-specific catalog content and translation tooling.

Normalization only standardizes representation.

## Normalization is not semantic repair

Normalization may convert:

```text
network-error
```

to:

```text
NETWORK_ERROR
```

It must not invent:

* a missing owner,
* a missing error code,
* a missing category,
* a new profile,
* a corrected code range,
* a replacement stable ID,
* a relationship between unrelated values.

For example:

```json
{
  "owner": "",
  "codeGroup": "NETWORK"
}
```

remains semantically invalid after normalization.

Validation must reject it.

## Normalization and ID validation

Suppose the source definition contains:

```json
{
  "id": "afw-net-0001",
  "owner": "afw",
  "codePrefix": "net"
}
```

Normalization produces:

```text
Id: AFW_NET_0001
Owner: AFW
CodePrefix: NET
```

ID-structure validation can then reliably check whether the ID begins with:

```text
AFW_NET_
```

Without normalization, harmless case and separator differences would complicate comparison.

## Normalization and lookups

Runtime lookups may accept raw or normalized forms.

For example, resolving a symbolic name may normalize:

```text
network unavailable
```

to:

```text
NETWORK_UNAVAILABLE
```

before comparison.

Catalog authors should still document and use the canonical stored form.

User-friendly lookup does not replace stable API contracts.

## Normalization across catalog types

The same principles apply to supporting catalogs.

Typical key-like values include:

```text
category names
category aliases
parent categories
code-group names
code prefixes
owner names
owner aliases
profile names
profile filter values
tags
explicit error IDs
```

Typical display values include:

```text
catalog names
display names
descriptions
titles
messages
developer hints
```

This keeps identity comparison consistent across the complete context.

## Source JSON versus runtime representation

The project source file may contain:

```json
{
  "name": "network-error",
  "displayName": "  Network error  "
}
```

The runtime representation may contain:

```text
Name: NETWORK_ERROR
DisplayName: Network error
```

This is intentional.

The source remains human-editable.

The runtime receives canonical keys.

## Authoring-tool behavior

Authoring tools may choose to write canonical normalized values back to project files, but only as an explicit editing operation.

For example, Setter may eventually offer:

```text
normalize workspace
preview changes
show diff
confirm write
create backup
apply normalization
```

The runtime loading and normalization pipeline itself must not save those changes silently.

## Loading failures versus validation failures

These failure classes should remain distinct.

### Loading failure

The document could not be obtained as an object.

Examples:

```text
FilePathIsEmpty
FileNotFound
InvalidJson
EmptyCatalogDocument
AccessDenied
InputOutputError
```

### Validation failure

The document was loaded, but its content violates catalog rules.

Examples:

```text
MissingCatalogId
DuplicateErrorCode
MissingErrorOwner
UnknownDefaultSeverity
UnresolvedCategory
```

This distinction improves:

* startup diagnostics,
* recovery behavior,
* maintenance tooling,
* monitoring,
* user guidance.

## Recommended diagnostic order

When initialization fails, investigate in this order:

```text
1. Was the configured path valid?
2. Did the file exist?
3. Could the process read it?
4. Was the JSON syntactically valid?
5. Did it deserialize into a document?
6. What did normalization produce?
7. Did document validation pass?
8. Did cross-catalog validation pass?
```

This avoids treating every failure as “bad JSON,” the universal cupboard where lazy diagnostics go to hide.

## Security considerations

JSON loading reads project-controlled files into application memory.

Projects should:

* restrict unauthorized writes,
* validate all loaded content,
* avoid loading catalogs from untrusted temporary locations,
* review imported catalogs,
* protect paths from unintended redirection,
* treat large or malicious JSON as untrusted input where relevant.

Case-insensitive property names, comments, and trailing commas improve usability but do not reduce the need for validation.

## Performance considerations

Catalogs are normally loaded during explicit runtime initialization rather than repeatedly for every error resolution.

A typical lifecycle is:

```text
load once
→ normalize once
→ validate once
→ build active context
→ resolve many errors from memory
```

Applications should not reread catalog files for every call to:

```text
FromId
FromName
FromCode
ResolveProfile
```

The active context exists to avoid that cost and inconsistency.

## Reload behavior

A later initialization may load the project workspace again and attempt to create a new complete context.

The candidate context is not published until:

```text
all required files load
+
normalization completes
+
all validation succeeds
+
context creation succeeds
```

Until then, the previous active context remains untouched.

## Recommended authoring practices

1. Use canonical property names.
2. Use canonical uppercase symbolic keys.
3. Keep human-facing text naturally cased.
4. Avoid relying on comments when broad JSON interoperability matters.
5. Avoid relying on trailing commas when external tools are strict.
6. Treat loading and validation errors differently.
7. Do not expect normalization to repair missing relationships.
8. Validate normalized collisions such as duplicate tags or names.
9. Keep runtime loading read-only.
10. Use explicit authoring tools to rewrite source files.
11. Protect catalog paths from unauthorized modification.
12. Load and validate once, then resolve from the active context.

## Central principle

> Loading turns files into documents, normalization turns flexible text into consistent runtime values, and validation decides whether the result deserves trust.
