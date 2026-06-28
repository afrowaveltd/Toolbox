# Workspace validation

WhenItFails Setter validates the complete project-local catalog workspace before it is inspected, summarized, or used for authoring.

The standard workspace contains:

```text
Jsons/
└── WhenItFails/
    ├── errors.en.json
    ├── categories.en.json
    ├── code-groups.en.json
    ├── owners.en.json
    └── profiles.json
```

Validation is deliberately broader than checking whether each file contains syntactically valid JSON.

It verifies:

```text
file loading
→ normalization
→ document validation
→ cross-catalog validation
```

## Command

```text
validate <path>
```

Example from a published executable:

```bash
when-it-fails-setter validate .
```

Example from the Toolbox repository:

```bash
dotnet run \
  --project Toolroom/WhenItFails/Setter \
  -- validate .
```

## Accepted paths

The supplied path may point to:

* a project root,
* the `Jsons/WhenItFails` directory directly.

Examples:

```text
.
./MyProject
./MyProject/Jsons/WhenItFails
```

Setter resolves both forms through the shared workspace path resolver.

## Validation sequence

The workspace validator follows this sequence:

```text
resolve workspace paths
→ load error catalog
→ normalize error catalog
→ validate error catalog
→ load category catalog
→ normalize category catalog
→ validate category catalog
→ load code-group catalog
→ normalize code-group catalog
→ validate code-group catalog
→ load owner catalog
→ normalize owner catalog
→ validate owner catalog
→ load profile catalog
→ normalize profile catalog
→ validate profile catalog
→ run cross-catalog validation
→ combine all issues
```

All discovered issues are collected into one combined validation result.

## Catalog loading

Each catalog is loaded through its own JSON loader.

Current loaders include:

```text
JsonErrorCatalogLoader
JsonErrorCategoryCatalogLoader
JsonErrorCodeGroupCatalogLoader
JsonErrorOwnerCatalogLoader
JsonErrorProfileCatalogLoader
```

Loading may fail because of:

* missing file,
* denied access,
* malformed JSON,
* incompatible document structure,
* input/output error,
* deserialization failure.

When a loader reports structured issues, Setter copies them into the combined workspace result.

When no structured issue is available, Setter uses a catalog-specific fallback code.

Examples:

```text
ErrorCatalogLoadFailed
CategoryCatalogLoadFailed
CodeGroupCatalogLoadFailed
OwnerCatalogLoadFailed
ProfileCatalogLoadFailed
```

## Normalization

A successfully loaded catalog is normalized before validation.

Current normalizers include:

```text
ErrorCatalogDocumentNormalizer
ErrorCategoryCatalogDocumentNormalizer
ErrorCodeGroupCatalogDocumentNormalizer
ErrorOwnerCatalogDocumentNormalizer
ErrorProfileCatalogDocumentNormalizer
```

Normalization makes values consistent for comparison and validation.

Depending on the catalog, this may include:

* trimming display values,
* canonicalizing symbolic keys,
* normalizing names,
* normalizing categories,
* normalizing tags,
* normalizing owner and group references.

Validation therefore evaluates normalized semantic values rather than relying on accidental whitespace or casing differences.

Normalization performed during validation is in memory only.

The `validate` command does not save normalized documents.

## Per-catalog validation

Each normalized document is passed to its corresponding validator.

Current validators include:

```text
ErrorCatalogValidator
ErrorCategoryCatalogValidator
ErrorCodeGroupCatalogValidator
ErrorOwnerCatalogValidator
ErrorProfileCatalogValidator
```

A document with validation errors is not passed into cross-catalog validation.

Its issues are still included in the combined result.

## Error catalog validation

The error catalog validator checks error definitions and document-level constraints.

Typical checks include:

* required document fields,
* required error fields,
* valid stable IDs,
* valid symbolic names,
* valid numeric codes,
* supported severities,
* duplicate IDs,
* duplicate names,
* duplicate numeric codes,
* owner values,
* code prefixes,
* code groups,
* primary categories,
* normalized collections.

Supported severity values are:

```text
Trace
Debug
Information
Warning
Error
Critical
```

Unsupported severity values make the error catalog invalid.

## Category catalog validation

The category catalog defines the allowed classification vocabulary.

Validation may include:

* required category fields,
* unique category names,
* normalized symbolic keys,
* valid display names,
* duplicate detection,
* structural consistency.

Error definitions may later reference these categories during cross-validation.

## Code-group catalog validation

Code groups describe numeric allocations and symbolic grouping.

Validation may include:

* required fields,
* unique names,
* unique prefixes,
* valid minimum and maximum code values,
* non-reversed ranges,
* overlapping or conflicting allocations,
* owner relationships,
* normalized group keys.

Code-group validity is essential before checking whether error codes belong to their declared ranges.

## Owner catalog validation

The owner catalog defines valid ownership identities.

Validation may include:

* required owner fields,
* unique names,
* normalized symbolic keys,
* display-name consistency,
* duplicate detection.

Owner definitions are later used to verify error and code-group references.

## Profile catalog validation

Profiles describe reusable selections of error definitions.

Validation may include:

* required profile fields,
* unique profile names,
* display-name consistency,
* include filters,
* exclusion filters,
* explicitly referenced error IDs,
* normalized owner, category, group, and tag values.

Profiles do not alter the underlying error catalog.

They define a view or selection over it.

## Cross-catalog validation

Cross-validation runs only when all five catalogs:

* loaded successfully,
* normalized successfully,
* passed their individual validators.

This is important because cross-validation requires trustworthy document structures.

The cross-validator receives:

```text
error catalog
owner catalog
code-group catalog
category catalog
profile catalog
```

It checks relationships between them.

Typical relationships include:

* every error owner exists,
* every code group exists,
* code-group ownership is valid,
* error code belongs to the expected range,
* error prefix matches its group,
* primary category exists,
* referenced categories exist,
* profile owners exist,
* profile groups exist,
* profile categories exist,
* explicitly referenced profile error IDs exist.

## Why cross-validation may be skipped

Suppose `owners.en.json` cannot be loaded.

Setter can still report:

* owner catalog loading errors,
* error catalog validation errors,
* category catalog validation errors,
* code-group validation errors,
* profile validation errors.

It cannot safely determine whether error owner references are valid because the owner catalog is unavailable.

Cross-validation is therefore skipped until every required document is independently valid.

This avoids producing misleading secondary errors from incomplete input.

## Combined result

All loading, document-validation, and cross-validation issues are collected into one:

```text
ErrorCatalogValidationResult
```

This allows one command run to report several independent problems.

The user does not need to fix only the first error and rerun repeatedly unless later validation depends on that repair.

## Issue severities

Validation issues may have three practical levels:

```text
Information
Warning
Error
```

### Information

Describes something worth noting but not inherently invalid.

### Warning

Indicates a suspicious, discouraged, or potentially problematic condition.

A warning should be reviewed, but it does not necessarily make the workspace invalid.

### Error

Indicates that the workspace cannot be considered valid.

Errors produce a failing validation result.

## Structured issue content

Each issue may include:

* stable issue code,
* severity,
* human-readable message,
* details,
* source path.

Example conceptually:

```text
Code: UnknownOwner
Severity: Error
Path: errors.en.json
Message: Error definition references an owner that does not exist.
```

Stable codes are more useful for tests and automation than matching complete human-readable messages.

## Loader issue mapping

Loader responses use the shared Afrowave issue infrastructure.

Setter maps their severity into the combined validation result.

Conceptually:

```text
IssueSeverity.Error or higher
→ validation error

IssueSeverity.Warning
→ validation warning

lower severity
→ validation information
```

When an issue contains both a message and details, Setter combines them for display.

## Successful validation

A workspace is valid when the combined result contains no errors.

Warnings and informational issues may still be shown.

Typical command result:

```text
validation completed
errors: 0
warnings: 0
information: 0
```

The exact presentation depends on the shared console validation renderer.

## Failed validation

Validation fails when one or more errors are present.

Possible causes include:

* catalog file missing,
* malformed JSON,
* duplicate error ID,
* duplicate numeric code,
* unsupported severity,
* unknown owner,
* unknown category,
* code outside allocated range,
* profile referencing nonexistent values.

Setter displays all collected issues and returns a non-zero exit code.

## Exit codes

For the `validate` command:

```text
0
→ workspace is valid
```

```text
1
→ required path argument missing or invalid
```

```text
2
→ loading or validation errors found
```

Unexpected top-level application failures may return:

```text
3
```

Scripts should use the exit code as the primary success signal.

## Validation is read-only

The `validate` command does not intentionally modify:

* JSON catalogs,
* backup files,
* temporary files,
* runtime context,
* project configuration.

Normalization is performed only in memory.

Validation does not:

* repair files,
* rewrite keys,
* fill missing values,
* replace invalid catalogs,
* copy bundled defaults,
* renumber codes.

## Validation versus initialization

`init` and `validate` have different responsibilities.

```text
init
→ create missing directories and files
```

```text
validate
→ inspect existing workspace correctness
```

`init` preserves existing files even when they are invalid.

It does not replace an invalid project catalog with a bundled template.

Recommended first-run sequence:

```bash
when-it-fails-setter init .
when-it-fails-setter validate .
```

## Validation versus runtime initialization

Setter validation is an authoring operation.

Runtime initialization is an application-startup operation.

Setter validation:

```text
reports workspace problems
→ does not activate runtime context
→ does not perform recovery fallback
```

Runtime initialization:

```text
loads a usable context
→ may use strict or flexible policy
→ may preserve previous context
→ may activate bundled fallback
```

A catalog may therefore fail Setter validation while a flexibly configured application continues running from an older or bundled context.

That does not make the project catalog valid.

## Validation before editing

Always validate before the first edit in a session.

```bash
when-it-fails-setter validate .
```

This establishes the initial state.

Without a pre-edit validation, a later failure may be incorrectly blamed on the new change even though the workspace was already invalid.

Recommended sequence:

```text
validate
→ inspect target
→ edit
→ validate again
```

## Validation after editing

Setter field-edit commands validate the edited error catalog before saving.

However, that does not replace complete workspace validation.

After any successful edit, run:

```bash
when-it-fails-setter validate .
```

This checks all catalogs and their relationships.

## Validation before summary and browsing

Commands such as:

```text
summary
inspect
errors
details
detail
```

validate the workspace before presenting data.

This prevents the tool from presenting an apparently authoritative view built from invalid catalogs.

A validation failure stops those commands before their normal view is shown.

## Validation in CI

A basic CI validation step:

```bash
dotnet run \
  --project Toolroom/WhenItFails/Setter \
  -- validate .
```

A shell example:

```bash
if ! dotnet run \
  --project Toolroom/WhenItFails/Setter \
  -- validate .
then
  echo "WhenItFails workspace validation failed." >&2
  exit 1
fi
```

## Recommended CI position

Validation should run after:

```text
repository checkout
→ dependency restore
→ build
```

and before:

```text
packaging
→ deployment
→ publishing catalog artifacts
```

Example:

```bash
dotnet restore
dotnet build --no-restore

dotnet run \
  --project Toolroom/WhenItFails/Setter \
  -- validate .

dotnet test --no-build
```

The exact order may vary, but invalid catalogs should block release.

## Diagnosing validation failures

Recommended process:

```text
read issue code
→ inspect source path
→ inspect related catalog entry
→ fix one logical problem
→ validate again
```

Do not blindly edit every file mentioned in cascading errors.

A missing owner definition, for example, may cause several errors in error definitions and code groups.

Fixing the owner catalog may resolve all of them.

## Missing catalog

When a required file is missing:

```bash
when-it-fails-setter init .
```

may create it from the bundled template.

Remember:

* only missing files are created,
* existing files are not overwritten.

Then validate again.

## Malformed JSON

For malformed JSON:

1. Open the file reported by the issue.
2. Check commas, quotes, brackets, and braces.
3. Use a JSON-aware editor or formatter.
4. Preserve the intended data.
5. Run validation again.

Setter accepts comments and trailing commas during loading where configured by the shared JSON options, but the catalog should still remain clear and maintainable.

## Duplicate identity

For duplicate:

* IDs,
* names,
* numeric codes,
* owner names,
* category names,
* group names,
* profile names,

determine which entry is authoritative.

Do not renumber or rename blindly.

Stable identities may already be referenced by:

* application code,
* logs,
* documentation,
* monitoring,
* customer support records.

## Unknown reference

For an unknown owner, group, category, or profile reference:

```text
check spelling
→ check normalization
→ check referenced catalog
→ decide whether definition or reference is wrong
```

Do not automatically create a new definition merely to silence validation.

The reference itself may contain a typo.

## Code range errors

When an error code falls outside its declared group:

* inspect the error code,
* inspect its code group,
* inspect group minimum and maximum,
* inspect prefix and owner,
* determine whether the error or allocation is wrong.

Numeric codes should be treated as stable contracts.

## Profile errors

A profile may reference:

* owners,
* code groups,
* categories,
* tags,
* explicit error IDs.

An unknown explicit error ID often indicates:

* renamed error,
* deleted error,
* typo,
* profile copied from another workspace.

Review the profile’s intent before changing it.

## Warnings

Warnings should not be ignored simply because the exit code is zero.

A warning may indicate:

* unusual but legal structure,
* incomplete descriptive data,
* discouraged naming,
* compatibility concern,
* future migration risk.

CI policy may choose to treat warnings more strictly, but Setter itself distinguishes them from errors.

## Validation output and automation

The validation renderer is designed primarily for humans.

Automation should rely on:

```text
exit code
+
stable issue codes where exposed
```

Avoid parsing complete formatted sentences as a long-term integration contract.

A future dedicated machine-readable validation format may provide stronger guarantees.

## Cancellation

Validation accepts a cancellation token internally.

Cancellation is propagated rather than converted into an ordinary validation error.

A cancelled validation should not be interpreted as a successful or failed workspace assessment.

It means the assessment was incomplete.

## Performance

Validation loads and validates all five catalogs.

This is intentional.

Correctness and complete relationship checking are more important than saving a small amount of authoring-time I/O.

For typical catalog sizes, complete validation should remain inexpensive enough to run:

* before edits,
* after edits,
* during builds,
* in CI.

## Recommended workflow

```bash
when-it-fails-setter validate .

when-it-fails-setter summary .

when-it-fails-setter details . AFW_NET_0001

when-it-fails-setter set-message . \
  AFW_NET_0001 \
  "The application could not reach the remote service."

when-it-fails-setter validate .

git diff -- Jsons/WhenItFails
```

## Validation checklist

Before committing catalog changes, confirm:

* every required catalog exists,
* JSON loads successfully,
* no duplicate stable identities exist,
* all severities are supported,
* every owner reference resolves,
* every code-group reference resolves,
* numeric codes belong to valid ranges,
* every category reference resolves,
* profiles reference valid definitions,
* full workspace validation returns exit code `0`,
* warnings have been reviewed,
* the Git diff contains only intended changes.

## Related documentation

* [Getting started](../Getting-Started/en.md)
* [Command reference](../Commands/en.md)
* [Overview](../Overview/en.md)
* [Editing error fields](../Editing%20Error%20Fields/en.md)
* [Safe writes](../Safe%20Writes/en.md)

## Central principle

> A workspace is valid only when each catalog is valid on its own and all catalogs agree with one another.
