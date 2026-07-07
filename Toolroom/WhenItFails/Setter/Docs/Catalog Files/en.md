# Catalog files

WhenItFails Setter works with a small JSON catalog package stored under:

```text
Jsons/WhenItFails
```

The package is intentionally split into several files so that error definitions, categories, numeric ranges, ownership, and profiles can evolve independently while still being validated together.

## Current catalog package

A normal workspace contains:

```text
errors.en.json
categories.en.json
code-groups.en.json
owners.en.json
profiles.json
```

These files form one logical workspace.

Do not treat one file as fully independent from the others.

## Why several files exist

The files have different responsibilities:

```text
errors.en.json
→ actual error definitions
```

```text
categories.en.json
→ category vocabulary
```

```text
code-groups.en.json
→ numeric code families and prefixes
```

```text
owners.en.json
→ ownership and responsibility ranges
```

```text
profiles.json
→ reusable views and presentation defaults
```

This separation keeps the main error catalog readable.

It also allows validation to check cross-file relationships.

## Shared catalog header fields

The catalog files commonly contain fields such as:

```text
schemaVersion
catalogId
catalogName
description
language
sourceCatalogId
sourceCatalogVersion
isShadowCopy
tags
```

Example shape:

```json
{
  "schemaVersion": "1.0",
  "catalogId": "afw.when-it-fails.categories",
  "catalogName": "Afrowave WhenItFails default category catalog",
  "description": "Default category catalog template for project-local customization.",
  "language": "en",
  "sourceCatalogId": "afw.when-it-fails.categories",
  "sourceCatalogVersion": "1.0",
  "isShadowCopy": true,
  "tags": [ "default", "categories", "shadow-copy-template" ]
}
```

The exact root collection differs by file.

## Schema version

Field:

```text
schemaVersion
```

Example:

```json
"schemaVersion": "1.0"
```

The schema version describes the catalog document shape.

Do not change it casually.

A schema version change should correspond to a deliberate compatibility decision.

## Catalog ID

Field:

```text
catalogId
```

Example:

```json
"catalogId": "afw.when-it-fails.errors"
```

The catalog ID identifies the catalog document.

It is not the same thing as an error ID.

## Catalog name

Field:

```text
catalogName
```

Example:

```json
"catalogName": "Afrowave WhenItFails default owner catalog"
```

The catalog name is shown in the workspace summary.

It should be understandable to humans.

## Description

Field:

```text
description
```

The description explains what the catalog is for.

It is human documentation inside the JSON file.

Avoid using descriptions as scriptable behavior.

## Language

Field:

```text
language
```

Typical current value:

```json
"language": "en"
```

Current text catalogs use English.

The filename may also include language:

```text
errors.en.json
categories.en.json
code-groups.en.json
owners.en.json
```

Profiles currently use:

```text
profiles.json
```

## Source catalog fields

Fields:

```text
sourceCatalogId
sourceCatalogVersion
isShadowCopy
```

These fields help describe whether a project-local catalog was copied or derived from a source template.

Example:

```json
"sourceCatalogId": "afw.when-it-fails.profiles",
"sourceCatalogVersion": "1.0",
"isShadowCopy": true
```

A project may customize a shadow-copy template locally.

## Catalog tags

Field:

```text
tags
```

Example:

```json
"tags": [ "default", "profiles", "shadow-copy-template" ]
```

Catalog-level tags describe the catalog document.

They are not the same thing as error-level tags.

## errors.en.json

The main error catalog contains known error definitions.

Root collection:

```text
errors
```

Each error definition may contain:

```text
id
code
name
owner
codePrefix
codeGroup
primaryCategory
categories
subcategories
title
message
defaultSeverity
developerHint
documentationKey
tags
metadata
```

## Error definition example

Conceptual example:

```json
{
  "id": "AFW_NET_0001",
  "code": 600001,
  "name": "NETWORKUNAVAILABLE",
  "owner": "AFW",
  "codePrefix": "NET",
  "codeGroup": "NETWORK",
  "primaryCategory": "NETWORK",
  "categories": [ "NETWORK" ],
  "subcategories": [ "CONNECTIVITY" ],
  "title": "Network unavailable",
  "message": "The network is unavailable.",
  "defaultSeverity": "Error",
  "developerHint": "Check connectivity, DNS, firewall, proxy, VPN, and host availability.",
  "documentationKey": "when-it-fails/errors/network/network-unavailable",
  "tags": [ "NETWORK", "USER_VISIBLE" ],
  "metadata": {}
}
```

The exact active catalog content may differ.

## Error ID

Field:

```text
id
```

Example:

```json
"id": "AFW_NET_0001"
```

The ID is a stable human-readable identifier.

Use it in documentation, tests, and manual lookups.

## Numeric code

Field:

```text
code
```

Example:

```json
"code": 600001
```

The numeric code should fit the intended owner and code-group ranges.

Numeric codes are useful for logs, support, interoperability, and cases where symbolic identifiers are inconvenient.

## Error name

Field:

```text
name
```

Example:

```json
"name": "NETWORKUNAVAILABLE"
```

The name is machine-friendly and developer-facing.

The `details` command can look up an error by this name.

## Owner reference

Field:

```text
owner
```

Example:

```json
"owner": "AFW"
```

The value should reference a known owner from:

```text
owners.en.json
```

## Code prefix reference

Field:

```text
codePrefix
```

Example:

```json
"codePrefix": "NET"
```

The prefix should match the intended code group from:

```text
code-groups.en.json
```

## Code group reference

Field:

```text
codeGroup
```

Example:

```json
"codeGroup": "NETWORK"
```

The value should reference a known code group from:

```text
code-groups.en.json
```

## Primary category reference

Field:

```text
primaryCategory
```

Example:

```json
"primaryCategory": "NETWORK"
```

The value should reference a known category from:

```text
categories.en.json
```

The dedicated `errors --category` filter currently checks the primary category.

## Additional categories

Field:

```text
categories
```

Example:

```json
"categories": [ "NETWORK", "EXTERNAL_SERVICE" ]
```

Use additional categories when an error naturally belongs to more than one logical area.

Do not use categories as random tags.

Use `tags` for flexible labels.

## Subcategories

Field:

```text
subcategories
```

Example:

```json
"subcategories": [ "CONNECTIVITY", "DNS" ]
```

Subcategories provide more detail than primary categories.

Use them consistently.

If a subcategory vocabulary becomes important, document it.

## Title, message, and hint

Fields:

```text
title
message
developerHint
```

These fields are documented in:

```text
Authoring Error Text
```

Short rule:

```text
title
→ names the problem
```

```text
message
→ explains what happened
```

```text
developerHint
→ suggests what to investigate
```

## Default severity

Field:

```text
defaultSeverity
```

Supported values:

```text
Trace
Debug
Information
Warning
Error
Critical
```

Setter normalizes supported severity input when using `set-severity`.

Manual JSON editing should use canonical casing.

## Documentation key

Field:

```text
documentationKey
```

Example:

```json
"documentationKey": "when-it-fails/errors/network/network-unavailable"
```

This points to extended guidance.

It does not have to be a URL.

## Error tags

Field:

```text
tags
```

Example:

```json
"tags": [ "NETWORK", "USER_VISIBLE" ]
```

Tags support searching, profile behavior, and future tooling.

Tags should be meaningful and stable.

## Metadata

Field:

```text
metadata
```

Example:

```json
"metadata": {}
```

Metadata is reserved for advanced structured data.

Do not use metadata as a dumping ground for text that belongs in title, message, hint, or documentation.

## categories.en.json

The category catalog defines the known category vocabulary.

Root collection:

```text
categories
```

A category may contain:

```text
name
displayName
description
aliases
parentCategories
defaultTags
defaultMappings
```

## Category example

```json
{
  "name": "NETWORK",
  "displayName": "Network",
  "description": "Errors related to connectivity, DNS, HTTP, proxies, VPN, timeouts, and remote endpoints.",
  "aliases": [ "HTTP", "CONNECTIVITY" ],
  "parentCategories": [],
  "defaultTags": [ "NETWORK" ],
  "defaultMappings": {
    "defaultSeverity": "Error"
  }
}
```

The exact active category definitions may differ.

## Category name

Field:

```text
name
```

Example:

```json
"name": "NETWORK"
```

This is the stable category identifier.

Error definitions should reference category names, not display names.

## Category display name

Field:

```text
displayName
```

Example:

```json
"displayName": "Network"
```

This is a human-readable label.

It can be nicer than the stable name.

## Category aliases

Field:

```text
aliases
```

Example:

```json
"aliases": [ "HTTP", "CONNECTIVITY" ]
```

Aliases can help describe related terminology.

Do not assume every command treats aliases as lookup keys unless documented.

## Parent categories

Field:

```text
parentCategories
```

Example:

```json
"parentCategories": []
```

This can represent category hierarchy.

Keep hierarchies simple and intentional.

## Default tags

Field:

```text
defaultTags
```

Example:

```json
"defaultTags": [ "VALIDATION", "USER_VISIBLE" ]
```

Default tags describe useful tags associated with a category.

They are not automatically a substitute for reviewing each error definition.

## Default mappings

Field:

```text
defaultMappings
```

Example:

```json
"defaultMappings": {
  "defaultSeverity": "Warning",
  "web.httpStatusCode": "400"
}
```

Mappings are string key-value pairs.

They may be used by consuming applications or future tooling.

## code-groups.en.json

The code-group catalog defines numeric code families.

Root collection:

```text
codeGroups
```

A code group may contain:

```text
name
displayName
codePrefix
codeFrom
codeTo
description
defaultCategories
defaultTags
defaultMappings
```

## Code-group example

```json
{
  "name": "NETWORK",
  "displayName": "Network",
  "codePrefix": "NET",
  "codeFrom": 600000,
  "codeTo": 699999,
  "description": "Network, HTTP, DNS, connectivity, and timeout errors.",
  "defaultCategories": [ "NETWORK" ],
  "defaultTags": [ "NETWORK" ],
  "defaultMappings": {}
}
```

## Code-group name

Field:

```text
name
```

Example:

```json
"name": "NETWORK"
```

Error definitions reference this value through:

```text
codeGroup
```

## Code prefix

Field:

```text
codePrefix
```

Example:

```json
"codePrefix": "NET"
```

Error definitions reference this value through:

```text
codePrefix
```

The prefix also appears in symbolic error IDs such as:

```text
AFW_NET_0001
```

## Numeric range

Fields:

```text
codeFrom
codeTo
```

Example:

```json
"codeFrom": 600000,
"codeTo": 699999
```

The numeric error code should be inside the intended code-group range.

## owners.en.json

The owner catalog defines responsible owners and their numeric ranges.

Root collection:

```text
owners
```

An owner may contain:

```text
name
displayName
description
codeFrom
codeTo
isBuiltIn
aliases
defaultMappings
```

## Owner example

```json
{
  "name": "APP",
  "displayName": "Application",
  "description": "Project-local application error definitions.",
  "codeFrom": 1000000,
  "codeTo": 1999999,
  "isBuiltIn": false,
  "aliases": [ "APPLICATION", "PROJECT" ],
  "defaultMappings": {
    "catalogRole": "application",
    "editable": "true"
  }
}
```

## Owner name

Field:

```text
name
```

Example:

```json
"name": "AFW"
```

Error definitions reference this value through:

```text
owner
```

## Built-in flag

Field:

```text
isBuiltIn
```

Example:

```json
"isBuiltIn": true
```

This describes whether an owner represents built-in catalog content.

It is informational and may also guide future tooling.

## Owner mappings

Owner mappings may express policy-like information.

Example:

```json
"defaultMappings": {
  "catalogRole": "application",
  "editable": "true"
}
```

Mappings are strings.

Consumers should parse them deliberately.

## profiles.json

The profile catalog defines reusable context-specific views and presentation defaults.

Root collection:

```text
profiles
```

A profile may contain:

```text
name
displayName
description
includeOwners
includeCodeGroups
includeCategories
includeSubcategories
includeTags
excludeTags
defaultMappings
```

Profiles are documented in detail in:

```text
Profiles
```

## Profile example

```json
{
  "name": "CLI",
  "displayName": "CLI",
  "description": "Default profile for command-line applications.",
  "includeOwners": [ "AFW", "APP", "PLUGIN", "USER" ],
  "includeCodeGroups": [],
  "includeCategories": [],
  "includeSubcategories": [],
  "includeTags": [],
  "excludeTags": [],
  "defaultMappings": {
    "cli.includeHints": "true",
    "cli.includeExitCode": "true",
    "cli.includeColors": "true"
  }
}
```

## Cross-file relationships

Important relationships include:

```text
error.owner
→ owners[].name
```

```text
error.codeGroup
→ codeGroups[].name
```

```text
error.codePrefix
→ codeGroups[].codePrefix
```

```text
error.primaryCategory
→ categories[].name
```

```text
error.categories[]
→ categories[].name
```

```text
profile.includeOwners[]
→ owners[].name
```

```text
profile.includeCodeGroups[]
→ codeGroups[].name
```

```text
profile.includeCategories[]
→ categories[].name
```

Validation exists because these files are connected.

## Manual editing rule

When manually editing catalog files:

```text
edit
→ validate
→ inspect summary or affected details
→ review Git diff
```

Do not edit several related files and postpone validation until much later.

Small validation loops are safer.

## Recommended edit order

When adding a new kind of error, prefer this order:

1. Confirm or add owner.
2. Confirm or add code group.
3. Confirm or add category.
4. Add error definition.
5. Update profiles if needed.
6. Validate.
7. Inspect summary.
8. Inspect the new error.
9. Review Git diff.

## Adding a new error

Before adding an error definition, decide:

```text
owner
code group
numeric code
code prefix
primary category
title
message
severity
developer hint
documentation key
tags
```

Then add the definition to:

```text
errors.en.json
```

Validate immediately afterward.

## Adding a new category

Add to:

```text
categories.en.json
```

Then update any error definitions or profiles that should use it.

Validate after the category change and after the related references are added.

## Adding a new code group

Add to:

```text
code-groups.en.json
```

Choose:

```text
name
codePrefix
codeFrom
codeTo
defaultCategories
defaultTags
```

Then add or update error definitions that use the new code group.

Validate to catch range and reference mistakes.

## Adding a new owner

Add to:

```text
owners.en.json
```

Choose:

```text
name
displayName
codeFrom
codeTo
isBuiltIn
aliases
defaultMappings
```

Then add or update error definitions that use the owner.

Validate to catch reference mistakes.

## Adding a new profile

Add to:

```text
profiles.json
```

Choose:

```text
name
displayName
description
includeOwners
includeCodeGroups
includeCategories
includeSubcategories
includeTags
excludeTags
defaultMappings
```

Then run:

```bash
dotnet run \
  --project Toolroom/WhenItFails/Setter \
  -- validate .

dotnet run \
  --project Toolroom/WhenItFails/Setter \
  -- summary .

dotnet run \
  --project Toolroom/WhenItFails/Setter \
  -- errors . --profile <PROFILE_NAME>
```

## JSON formatting

Setter writes JSON using indented formatting.

Manual edits should keep JSON readable.

Avoid unnecessary formatting-only churn.

If a file is reformatted, review the diff carefully before committing.

## Comments and trailing commas

The loading configuration may accept comments and trailing commas in some contexts.

However, catalog files should preferably remain normal portable JSON unless there is a deliberate project convention.

Strict tools such as `jq` may reject comments or trailing commas.

Validation is authoritative for Setter behavior.

## Stable names

Stable identifiers should not be renamed casually.

This includes:

```text
error id
error name
owner name
code group name
code prefix
category name
profile name
```

Renaming may break:

- documentation,
- tests,
- scripts,
- support references,
- runtime mappings,
- external consumers.

## Display names

Display names are easier to change than stable names, but they still affect users.

Use clear human-readable wording.

Avoid churn without a reason.

## Aliases

Aliases help bridge old names, alternate terminology, or related terms.

Use aliases deliberately.

Do not rely on aliases unless the command or runtime behavior explicitly supports them.

## Default mappings as policy

Several catalog files contain:

```text
defaultMappings
```

Mappings are string key-value pairs.

They may represent default behavior, presentation rules, or metadata for consumers.

Use predictable names such as:

```text
web.includeTraceId
cli.includeHints
production.includeStackTrace
catalogRole
editable
```

Avoid vague keys such as:

```text
flag1
x
thing
mode
```

## Validation checklist

After editing any catalog file, run:

```bash
dotnet run \
  --project Toolroom/WhenItFails/Setter \
  -- validate .
```

Then, depending on the change:

```bash
dotnet run \
  --project Toolroom/WhenItFails/Setter \
  -- summary .
```

```bash
dotnet run \
  --project Toolroom/WhenItFails/Setter \
  -- details . AFW_NET_0001
```

```bash
dotnet run \
  --project Toolroom/WhenItFails/Setter \
  -- errors . --profile WEB
```

## Git review checklist

Before committing catalog changes:

```bash
git diff --stat
git diff -- Jsons/WhenItFails
git diff --check
```

Confirm:

- only intended files changed,
- no unrelated formatting churn appeared,
- no backup files are being committed accidentally,
- JSON remains readable,
- validation passed,
- any changed profile behavior is intentional,
- any changed range or owner behavior is intentional.

## Common mistakes

Common catalog-file mistakes include:

- editing the wrong workspace,
- using display name instead of stable name,
- adding an error code outside the code-group range,
- using an unknown category,
- using an unknown owner,
- changing a stable ID unnecessarily,
- creating duplicate names,
- copying development mappings into production,
- committing timestamped backup files accidentally,
- reformatting whole files unintentionally.

## Current Setter edit scope

Current edit commands focus on selected fields in:

```text
errors.en.json
```

Current commands include:

```text
set-title
set-message
set-developer-hint
set-severity
set-documentation-key
```

Other catalog-file changes are currently manual JSON editing tasks.

Validate after manual edits.

## Future improvements

Possible future improvements include:

- catalog file schema documentation generated from models,
- JSON schema files,
- dedicated profile editing commands,
- category editing commands,
- code-group editing commands,
- owner editing commands,
- add-error command,
- rename-safe workflows,
- cross-reference explanation output,
- unused category detection,
- unused profile detection,
- migration tooling between schema versions.

These are future possibilities, not current guarantees.

## Related documentation

- [Glossary](../Glossary/en.md)
- [Profiles](../Profiles/en.md)
- [Validation](../Validation/en.md)
- [Workspace Summary](../Workspace%20Summary/en.md)
- [Browsing and Filtering Errors](../Browsing%20and%20Filtering%20Errors/en.md)
- [Inspecting Error Details](../Inspecting%20Error%20Details/en.md)
- [Authoring Error Text](../Authoring%20Error%20Text/en.md)
- [Safe Writes](../Safe%20Writes/en.md)
- [Backups and Recovery](../Backups%20and%20Recovery/en.md)

## Central principle

> A catalog file can be edited alone, but it can only be trusted as part of the validated workspace.
