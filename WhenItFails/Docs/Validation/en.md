# Validation

WhenItFails validates catalog documents before they become part of an active runtime context.

Validation protects stable identity, numeric codes, ownership, classification, references, and runtime predictability.

A catalog that can be parsed as JSON is not automatically a valid catalog.

The complete validation process must also determine whether the data is internally consistent and safe to activate.

## Validation goals

Validation is designed to detect problems such as:

* missing required values,
* duplicate identifiers,
* duplicate numeric codes,
* duplicate symbolic names,
* invalid severity values,
* malformed error IDs,
* inconsistent owner or code-prefix information,
* invalid code ranges,
* unresolved cross-catalog references,
* malformed profile filters,
* contradictory catalog data.

A context containing validation errors must not become active.

## Validation levels

Validation occurs at several levels:

```text
document structure
→ individual definitions
→ duplicate detection
→ cross-catalog relationships
→ complete context validation
```

A single catalog may be structurally valid while still being incompatible with the remaining catalogs.

For example, an error may reference:

```text
owner: APP
codeGroup: NETWORK
primaryCategory: NETWORK
```

Each value may be syntactically valid, but the complete context is invalid when the referenced owner, code group, or category does not exist.

## Validation result

The main validation result type is:

```csharp
ErrorCatalogValidationResult
```

It contains:

```text
Issues
IsValid
```

`Issues` contains every validation issue collected during the validation pass.

`IsValid` is `false` when at least one issue has error severity.

Warnings and informational issues do not by themselves make the result invalid.

Conceptually:

```text
no Error issues
→ IsValid = true

one or more Error issues
→ IsValid = false
```

## Validation issue severity

Validation issues may have one of these severities:

```text
Information
Warning
Error
```

### Information

An informational issue describes something useful to know but does not indicate invalid catalog data.

### Warning

A warning identifies suspicious, incomplete, or discouraged data that does not necessarily prevent activation.

Examples include:

* missing human-readable catalog name,
* missing language metadata,
* empty catalog,
* missing error title,
* duplicate category or tag inside one definition,
* empty optional collection value.

### Error

An error identifies a condition that makes the catalog unsafe or ambiguous.

Examples include:

* missing stable ID,
* invalid numeric code,
* duplicate error ID,
* duplicate error code,
* duplicate symbolic name,
* missing owner,
* missing code group,
* missing primary category,
* missing message,
* unsupported severity,
* unresolved catalog reference.

## Validation issue structure

A validation issue may contain:

```text
Severity
Code
Message
ErrorId
ErrorName
Path
```

Example:

```text
Severity: Error
Code: DuplicateErrorCode
Message: Duplicate error code '600001'.
ErrorId: AFW_NET_0002
ErrorName: NETWORKTIMEOUT
Path: errors[12].code
```

The issue code provides a stable machine-friendly identifier.

The message provides a human-readable explanation.

The optional error ID and name identify the affected definition.

The path points to the location in the source document.

## Stable issue codes

Validation issue codes should be treated as machine-readable diagnostics.

Examples include:

```text
CatalogDocumentIsNull
MissingSchemaVersion
MissingCatalogId
MissingCatalogName
MissingCatalogLanguage
CatalogContainsNoErrors
MissingErrorId
InvalidErrorCode
MissingErrorName
MissingErrorOwner
MissingErrorCodePrefix
MissingErrorCodeGroup
MissingErrorPrimaryCategory
MissingErrorTitle
MissingErrorMessage
MissingDefaultSeverity
UnknownDefaultSeverity
DuplicateErrorId
DuplicateErrorCode
DuplicateErrorName
ErrorIdDoesNotMatchOwnerAndCodePrefix
```

Applications and tools may use these codes to:

* group validation output,
* provide targeted help,
* link to documentation,
* filter expected warnings,
* support automated repair suggestions,
* produce CI reports.

Tools should not parse the human-readable message when a stable issue code is available.

## Document header validation

The error catalog header requires:

```text
schemaVersion
catalogId
```

Missing values are validation errors.

The following values are recommended but not required for validity:

```text
catalogName
language
```

Missing recommended values produce warnings.

Example invalid header:

```json
{
  "schemaVersion": "",
  "catalogId": "",
  "catalogName": "",
  "language": ""
}
```

Expected result:

```text
MissingSchemaVersion
MissingCatalogId
MissingCatalogName
MissingCatalogLanguage
```

The first two are errors.

The remaining two are warnings.

## Empty catalogs

An error catalog containing no error definitions produces a warning:

```text
CatalogContainsNoErrors
```

An empty catalog is not automatically invalid.

This allows intentional project stages such as:

* a newly initialized workspace,
* a specialized package with no definitions yet,
* a temporary authoring state,
* a catalog populated later by explicit tools.

Applications should still decide whether an empty active catalog is meaningful for their environment.

## Required error identity

Every error definition requires:

```text
Id
Code
Name
Owner
CodePrefix
CodeGroup
PrimaryCategory
Message
DefaultSeverity
```

A missing required value produces an error.

The title is recommended but currently produces a warning when missing.

## Error ID

The stable error ID must be present.

Example:

```text
AFW_NET_0001
```

The ID must also match the declared owner and code prefix.

For:

```json
{
  "id": "AFW_NET_0001",
  "owner": "AFW",
  "codePrefix": "NET"
}
```

the normalized ID is expected to begin with:

```text
AFW_NET_
```

Example invalid definition:

```json
{
  "id": "APP_NET_0001",
  "owner": "AFW",
  "codePrefix": "NET"
}
```

This produces:

```text
ErrorIdDoesNotMatchOwnerAndCodePrefix
```

The suffix format may evolve independently, but the owner and prefix portion must remain consistent.

## Numeric code

The numeric code must be greater than zero.

Example invalid value:

```json
{
  "code": 0
}
```

This produces:

```text
InvalidErrorCode
```

Numeric code validation later also includes compatibility with owner and code-group ranges during complete context validation.

## Symbolic name

Every definition requires a symbolic name.

Example:

```text
NETWORKUNAVAILABLE
```

The symbolic name should be:

* stable,
* machine-friendly,
* readable,
* independent of presentation language.

Human-readable messages should not be used as substitutes for symbolic identity.

## Required classification

Every definition requires:

```text
Owner
CodePrefix
CodeGroup
PrimaryCategory
```

These values connect the definition to the supporting catalogs.

A missing value is immediately invalid even before cross-catalog reference validation begins.

## Human-readable message

Every error definition requires a message.

Example:

```text
The network is currently unavailable.
```

A missing message produces:

```text
MissingErrorMessage
```

The title is recommended but not currently required for validity.

A missing title produces:

```text
MissingErrorTitle
```

with warning severity.

## Supported severities

The default severity must use one of the supported values:

```text
Trace
Debug
Information
Warning
Error
Critical
```

Matching is case-insensitive during validation.

Example invalid value:

```json
{
  "defaultSeverity": "Fatal"
}
```

This produces:

```text
UnknownDefaultSeverity
```

Projects that need additional presentation levels should normally map from the supported core severities rather than inventing incompatible catalog values.

## Duplicate stable identities

The error catalog requires uniqueness for:

```text
Id
Code
Name
```

Duplicate checks use normalized or case-insensitive comparison where appropriate.

### Duplicate ID

Example:

```json
[
  {
    "id": "AFW_NET_0001"
  },
  {
    "id": "afw_net_0001"
  }
]
```

These values are treated as duplicates.

The issue code is:

```text
DuplicateErrorId
```

### Duplicate numeric code

Example:

```json
[
  {
    "code": 600001
  },
  {
    "code": 600001
  }
]
```

The issue code is:

```text
DuplicateErrorCode
```

### Duplicate symbolic name

Example:

```json
[
  {
    "name": "NETWORKUNAVAILABLE"
  },
  {
    "name": "networkunavailable"
  }
]
```

The issue code is:

```text
DuplicateErrorName
```

Stable identities must remain unambiguous after normalization.

## Collection validation

The validator checks string collections such as:

```text
Categories
Subcategories
Tags
```

Problems detected inside collections include:

* empty values,
* duplicate normalized values.

Example:

```json
{
  "tags": [
    "NETWORK",
    "",
    "network"
  ]
}
```

This may produce warnings for:

```text
EmptyTag
DuplicateTag
```

Similar issue-code patterns apply to categories and subcategories:

```text
EmptyCategory
DuplicateCategory
EmptySubcategory
DuplicateSubcategory
```

These issues are warnings because normalization may still produce a usable definition, but catalog authors should clean them up.

## Normalized comparison

Duplicate detection uses normalized symbolic values.

This means that superficial formatting differences should not create separate identities.

Examples that may normalize to the same value include:

```text
NETWORK
network
Network
```

Authors should still use canonical formatting, normally uppercase symbolic names, to keep catalog files readable and predictable.

## Cross-catalog validation

The complete catalog context contains:

```text
errors
categories
code groups
owners
profiles
```

Cross-catalog validation ensures that references between these documents are valid.

Typical relationships include:

```text
error.owner
→ owner.name

error.codeGroup
→ codeGroup.name

error.codePrefix
→ codeGroup.codePrefix

error.primaryCategory
→ category.name

error.categories[]
→ category.name

category.parentCategories[]
→ category.name

profile.includeOwners[]
→ owner.name

profile.includeCodeGroups[]
→ codeGroup.name

profile.includeCategories[]
→ category.name

profile.includeErrors[]
→ error.id

profile.excludeErrors[]
→ error.id
```

An unresolved reference makes the complete context invalid.

## Owner-range validation

An error code must be compatible with the range assigned to its owner.

Example owner:

```json
{
  "name": "APP",
  "codeFrom": 1000000,
  "codeTo": 1999999
}
```

An `APP` error using code `600001` would violate the owner range.

Ownership and numeric allocation must agree.

## Code-group range validation

An error code must also be compatible with the selected code group.

Example code group:

```json
{
  "name": "NETWORK",
  "codePrefix": "NET",
  "codeFrom": 600000,
  "codeTo": 699999
}
```

A network error using code `700001` would violate the group range.

The code must satisfy all relevant catalog constraints.

## Code-prefix consistency

An error definition declares both:

```text
CodeGroup
CodePrefix
```

The prefix must match the prefix assigned to the referenced code group.

Example valid combination:

```text
CodeGroup: NETWORK
CodePrefix: NET
```

Example invalid combination:

```text
CodeGroup: NETWORK
CodePrefix: DB
```

This mismatch must be rejected because the symbolic identity and numeric family would disagree.

## Category validation

The primary category and all additional categories must reference existing category definitions.

Example invalid reference:

```json
{
  "primaryCategory": "SOMETHING_UNKNOWN"
}
```

This must not be silently converted into a new category.

New categories must be declared explicitly in the category catalog.

## Parent-category validation

Category parent references must resolve to existing categories.

Circular parent relationships should be rejected or reported by complete context validation.

A category hierarchy must not become ambiguous or infinitely recursive.

## Profile validation

Profiles must reference valid catalog entities.

Profile validation may check:

* missing profile name,
* duplicate profile names,
* unresolved owners,
* unresolved code groups,
* unresolved categories,
* unresolved explicit error IDs,
* malformed include or exclude collections,
* duplicate normalized values.

Profiles are part of the active context and must validate together with the catalogs they query.

## Errors versus warnings

A useful authoring rule is:

```text
Error
→ context must not be activated

Warning
→ context may be activated, but should be reviewed

Information
→ context is valid; diagnostic note only
```

Warnings should not be ignored indefinitely merely because they do not block activation.

Repeated warnings often indicate:

* incomplete authoring,
* untidy generated data,
* lost metadata,
* accidental duplicates,
* migration leftovers.

## Validation and normalization

Normalization occurs before or alongside parts of validation.

Normalization may:

* canonicalize symbolic keys,
* remove superficial formatting differences,
* initialize empty collections,
* normalize optional text,
* standardize tags and aliases.

Normalization must not silently repair semantic contradictions.

For example, normalization may make:

```text
network
```

comparable with:

```text
NETWORK
```

but it must not invent a missing owner or choose a new numeric code.

## Validation does not rewrite source files

Validation inspects catalog data.

It does not automatically:

* rewrite invalid JSON,
* remove duplicates,
* allocate codes,
* rename identifiers,
* repair references,
* migrate schemas,
* overwrite project files.

Repairs belong to explicit authoring and maintenance tools.

The runtime must remain a validator and consumer, not an invisible editor.

## Runtime activation

The runtime activates only a complete validated context.

Conceptually:

```text
load all documents
→ normalize
→ validate each document
→ validate relationships
→ create context
→ publish context and status
```

If validation fails:

```text
do not publish partial context
→ apply configured strict or flexible recovery policy
```

## Previous context safety

A failed validation attempt must not corrupt the currently active valid context.

The application should continue observing either:

```text
the previous complete valid context
```

or, when permitted:

```text
a complete bundled fallback context
```

Never the invalid candidate context.

## Validation in authoring tools

Authoring tools should display validation issues using:

```text
severity
issue code
message
path
error ID
error name
```

Good output should allow the user to answer:

```text
What is wrong?
Where is it wrong?
Which definition is affected?
Does it block activation?
How can it be repaired?
```

Example:

```text
ERROR DuplicateErrorCode
errors[12].code

Duplicate error code '600001'.

Error:
AFW_NET_0002 / NETWORKTIMEOUT
```

## Validation in CI

Catalog validation is suitable for continuous integration.

A CI validation step may:

```text
load project catalogs
→ normalize
→ validate complete context
→ fail build on Error issues
→ report Warning issues
```

This catches catalog problems before deployment.

Recommended CI policy:

```text
Errors
→ fail the job

Warnings
→ display clearly

Selected warnings
→ optionally treat as errors for strict projects
```

## Validation after editing

The complete workspace should be validated after every authoring operation.

This includes changes to:

* errors,
* categories,
* code groups,
* owners,
* profiles,
* schema metadata,
* numeric ranges,
* aliases,
* mappings.

Editing one catalog may invalidate references in another.

Validation should therefore target the complete workspace, not only the file that was edited.

## Validation after import

Imported definitions or profiles must be validated before they become project data.

Import should not assume that a package is compatible merely because it contains valid JSON.

The importer should check:

* schema compatibility,
* duplicate identities,
* numeric collisions,
* missing references,
* owner-range compatibility,
* code-group compatibility,
* profile references,
* project policy.

## Validation and localization

Localized catalogs must preserve stable identity and structural relationships.

Localization may change:

```text
catalogName
description
title
message
developerHint
displayName
```

Localization must not casually change:

```text
catalogId
error ID
numeric code
symbolic name
owner
code group
category identity
profile identity
```

Validation should ensure localized variants remain structurally compatible with their source catalog where such compatibility is required.

## Recommended validation rules

1. Validate the complete workspace, not only one file.
2. Fail activation on every error-severity issue.
3. Keep warnings visible to authors and operators.
4. Use issue codes for automation.
5. Use issue paths for precise diagnostics.
6. Preserve the previous valid runtime context after failure.
7. Never publish a partially validated context.
8. Never repair project files silently.
9. Validate imported data before merging it.
10. Validate after every authoring change.
11. Treat stable identity collisions as serious errors.
12. Keep normalization separate from semantic repair.
13. Make validation output readable for both humans and tools.
14. Run validation in CI before deployment.

## Central principle

> Parsing proves that a document can be read; validation proves that the complete catalog context can be trusted.
