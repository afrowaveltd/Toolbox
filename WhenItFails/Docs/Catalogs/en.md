# Error catalogs

WhenItFails stores error definitions and their supporting metadata in a group of project-local JSON catalogs.

The catalogs are designed to be readable, versionable, portable, and editable without recompiling the consuming application.

The default workspace is:

```text
Jsons/
└── WhenItFails/
    ├── errors.en.json
    ├── categories.en.json
    ├── code-groups.en.json
    ├── owners.en.json
    └── profiles.json
```

Together, these files form one complete runtime catalog context.

## Catalog overview

| Catalog               | Purpose                                                     |
| --------------------- | ----------------------------------------------------------- |
| `errors.en.json`      | Defines individual errors                                   |
| `categories.en.json`  | Defines semantic error categories                           |
| `code-groups.en.json` | Defines numeric code ranges and prefixes                    |
| `owners.en.json`      | Defines ownership boundaries and code ranges                |
| `profiles.json`       | Defines reusable error selections and presentation defaults |

The error catalog is the central catalog.

The remaining catalogs describe, classify, constrain, and select the error definitions.

## Common catalog metadata

Each catalog document contains common identifying metadata such as:

```json
{
  "schemaVersion": "1.0",
  "catalogId": "afw.when-it-fails.errors",
  "catalogName": "Afrowave WhenItFails default error catalog",
  "description": "Default error catalog template for project-local customization.",
  "language": "en",
  "sourceCatalogId": "afw.when-it-fails.errors",
  "sourceCatalogVersion": "1.0",
  "isShadowCopy": true,
  "tags": []
}
```

### `schemaVersion`

Identifies the structure of the JSON document.

Schema versioning allows loaders and maintenance tools to detect unsupported or outdated catalog formats.

### `catalogId`

Provides a stable identity for the catalog document.

It should not be changed casually after the catalog is distributed or referenced by other tools.

### `catalogName`

Provides a human-readable catalog name.

Unlike `catalogId`, it may be changed without altering the catalog identity.

### `language`

Identifies the language used for human-readable catalog content.

The default catalogs currently use:

```text
en
```

### `sourceCatalogId`

Identifies the catalog from which the project copy originated.

### `sourceCatalogVersion`

Records the source catalog version used to create the project copy.

### `isShadowCopy`

Indicates that the catalog is a project-local copy derived from a bundled source template.

A shadow copy is editable by the project.

It is not the bundled package resource itself.

## Error catalog

File:

```text
errors.en.json
```

The error catalog contains the individual error definitions used by applications.

A simplified error definition looks like this:

```json
{
  "id": "AFW_NET_0001",
  "code": 600001,
  "name": "NETWORKUNAVAILABLE",
  "owner": "AFW",
  "codePrefix": "NET",
  "codeGroup": "NETWORK",
  "primaryCategory": "NETWORK",
  "categories": [
    "NETWORK"
  ],
  "subcategories": [
    "CONNECTIVITY"
  ],
  "title": "Network is not available",
  "message": "The network is currently unavailable.",
  "defaultSeverity": "Error",
  "developerHint": "Check network connectivity, DNS, proxy, VPN, and remote endpoint availability.",
  "documentationKey": "when-it-fails/errors/network/network-unavailable",
  "tags": [
    "NETWORK",
    "USER_VISIBLE"
  ],
  "metadata": {
    "Count": 0,
    "IsEmpty": true,
    "Items": {}
  }
}
```

## Stable error identity

An error may be resolved by several values:

```text
id
name
code
```

Each serves a different purpose.

### `id`

Example:

```text
AFW_NET_0001
```

The ID is the primary symbolic identity of the error.

It combines ownership, group prefix, and a sequence number.

IDs should remain stable after publication.

### `name`

Example:

```text
NETWORKUNAVAILABLE
```

The name is a readable symbolic identifier intended for code, diagnostics, and authoring tools.

Names are normalized and should be unique within the active catalog context.

### `code`

Example:

```text
600001
```

The numeric code provides a stable machine-readable identifier.

Codes may be useful for:

* logs,
* external protocols,
* support communication,
* telemetry,
* database storage,
* compatibility with systems that prefer numeric values.

Published numeric codes should not be reused for unrelated errors.

## Error ownership

The `owner` property references an entry in:

```text
owners.en.json
```

Example:

```json
"owner": "AFW"
```

Ownership identifies who controls and maintains the definition.

It also constrains the permitted numeric code range.

## Code groups

The following error properties reference a code-group definition:

```json
"codePrefix": "NET",
"codeGroup": "NETWORK"
```

The code group determines:

* the semantic group name,
* the symbolic prefix,
* the permitted numeric code range,
* default categories,
* default tags,
* default mappings.

The error code must fit the range assigned to its code group.

## Categories

The central category properties are:

```json
"primaryCategory": "NETWORK",
"categories": [
  "NETWORK",
  "EXTERNAL_SERVICE"
],
"subcategories": [
  "CONNECTIVITY"
]
```

### `primaryCategory`

Defines the main semantic classification of the error.

### `categories`

Contains all major categories associated with the error.

An error may belong to more than one category.

### `subcategories`

Provides finer project-specific classification.

Subcategories are useful when a full top-level category would be unnecessarily broad.

## Human-readable fields

### `title`

A short human-readable label.

Example:

```text
Network is not available
```

### `message`

The default explanatory message.

The message should be safe and useful for its intended audience.

Sensitive technical details should not be exposed through a public message.

### `developerHint`

Provides technical guidance for developers, operators, or support personnel.

The developer hint may contain information unsuitable for end-user presentation.

### `documentationKey`

Provides a stable logical reference to extended documentation.

It is not required to be a direct physical file path or URL.

## Severity

The `defaultSeverity` property defines the default severity of the error.

Example:

```json
"defaultSeverity": "Error"
```

A category, profile, mapping layer, or consuming application may later adapt how that severity is presented.

## Tags

Tags provide lightweight classification without requiring a new strongly structured catalog.

Examples include:

```text
USER_VISIBLE
INTERNAL_ONLY
DEBUG_ONLY
RETRYABLE
NETWORK
FALLBACK
```

Tags are commonly used by profiles and presentation layers.

Tags should have clear and stable meanings within a project.

## Metadata

The `metadata` object allows additional structured values to be associated with a catalog or definition.

Metadata is intended for extensibility.

Core identity and validation rules should not depend on arbitrary metadata when a dedicated property would be more appropriate.

## Category catalog

File:

```text
categories.en.json
```

The category catalog defines semantic classifications shared by multiple errors.

A category definition may contain:

```json
{
  "name": "NETWORK",
  "displayName": "Network",
  "description": "Errors related to connectivity, DNS, HTTP, proxies, VPN, timeouts, and remote endpoints.",
  "aliases": [
    "NETWORKING",
    "COMMUNICATION",
    "HTTP"
  ],
  "parentCategories": [],
  "defaultTags": [
    "NETWORK"
  ],
  "defaultMappings": {
    "defaultSeverity": "Error",
    "web.httpStatusCode": "503"
  }
}
```

## Category identity

The stable category identity is:

```text
name
```

Example:

```text
NETWORK
```

`displayName` is intended for user interfaces and documentation.

Aliases allow alternate names to resolve to the same category.

## Parent categories

A category may reference one or more broader categories:

```json
"parentCategories": [
  "NETWORK"
]
```

For example, `EXTERNAL_SERVICE` may be treated as a specialization of `NETWORK`.

Parent relationships must reference existing categories.

Circular relationships should not be introduced.

## Category defaults

Categories may provide defaults such as:

```text
defaultTags
defaultMappings
```

These values describe typical behavior for errors in the category.

An explicit value on an error definition remains more specific than a category-level default.

## Code-group catalog

File:

```text
code-groups.en.json
```

Code groups divide the numeric error namespace into meaningful ranges.

Example:

```json
{
  "name": "NETWORK",
  "displayName": "Network",
  "codePrefix": "NET",
  "codeFrom": 600000,
  "codeTo": 699999,
  "description": "Network, HTTP, DNS, connectivity, and timeout errors.",
  "defaultCategories": [
    "NETWORK"
  ],
  "defaultTags": [
    "NETWORK"
  ],
  "defaultMappings": {}
}
```

A code group defines:

* a stable group name,
* a symbolic prefix,
* an inclusive numeric range,
* default categories,
* default tags,
* optional mappings.

Code ranges should not overlap unless the catalog model explicitly permits the overlap.

An error referencing a code group must use a code inside that group's range.

## Default built-in code groups

The default Afrowave code groups are:

| Group              | Prefix |           Range |
| ------------------ | -----: | --------------: |
| `GENERAL`          |  `GEN` | `100000–199999` |
| `CONFIGURATION`    |  `CFG` | `200000–299999` |
| `VALIDATION`       |  `VAL` | `300000–399999` |
| `FILE_SYSTEM`      |   `IO` | `400000–499999` |
| `SECURITY`         |  `SEC` | `500000–599999` |
| `NETWORK`          |  `NET` | `600000–699999` |
| `DATABASE`         |   `DB` | `700000–799999` |
| `EXTERNAL_SERVICE` |  `EXT` | `800000–899999` |
| `SERIALIZATION`    |  `FMT` | `900000–999999` |

These ranges describe the built-in Afrowave namespace.

Projects may define additional ranges within the boundaries permitted by their owner definitions.

## Owner catalog

File:

```text
owners.en.json
```

Owners divide responsibility and numeric namespaces between Afrowave, applications, plugins, and local user definitions.

Example:

```json
{
  "name": "APP",
  "displayName": "Application",
  "description": "Project-local application error definitions.",
  "codeFrom": 1000000,
  "codeTo": 1999999,
  "isBuiltIn": false,
  "aliases": [
    "APPLICATION",
    "PROJECT"
  ],
  "defaultMappings": {
    "catalogRole": "application",
    "editable": "true"
  }
}
```

An owner definition provides:

* a stable owner name,
* display information,
* an inclusive numeric code range,
* built-in or project-owned status,
* aliases,
* default mappings.

## Default owner ranges

| Owner    | Purpose                            |             Range |
| -------- | ---------------------------------- | ----------------: |
| `AFW`    | Built-in Afrowave definitions      |        `0–999999` |
| `APP`    | Project application definitions    | `1000000–1999999` |
| `PLUGIN` | Plugin and extension definitions   | `2000000–2999999` |
| `USER`   | Local and experimental definitions | `9000000–9999999` |

The owner range and code-group range describe different dimensions.

The owner determines who controls the definition.

The code group determines what kind of error it is.

A valid numeric code must satisfy the catalog rules for both.

## Profiles catalog

File:

```text
profiles.json
```

Profiles define reusable selections of errors and default presentation behavior.

A profile does not duplicate error definitions.

It selects definitions from the active catalog context.

Example:

```json
{
  "name": "WEB",
  "displayName": "Web",
  "description": "Default profile for web applications with user-safe error presentation.",
  "includeOwners": [
    "AFW",
    "APP",
    "PLUGIN",
    "USER"
  ],
  "includeCodeGroups": [],
  "includeCategories": [
    "GENERAL",
    "CONFIGURATION",
    "VALIDATION",
    "FILE_SYSTEM",
    "SECURITY",
    "NETWORK",
    "DATABASE",
    "EXTERNAL_SERVICE",
    "SERIALIZATION"
  ],
  "includeSubcategories": [],
  "includeTags": [
    "USER_VISIBLE"
  ],
  "excludeTags": [
    "INTERNAL_ONLY",
    "DEBUG_ONLY"
  ],
  "defaultMappings": {
    "web.problemDetails": "true",
    "web.includeTraceId": "true",
    "web.includeExceptionDetails": "false",
    "web.includeStackTrace": "false"
  }
}
```

Profiles may select errors by:

```text
owner
code group
category
subcategory
tag
```

Profiles may also exclude errors by tag.

## Profile selection

A profile normally behaves as a filter over the active error catalog.

Empty include collections mean that the profile does not restrict selection by that particular dimension.

For example:

```json
"includeCodeGroups": []
```

does not necessarily mean that no code groups are permitted.

It means that code groups are not being used as an additional include restriction.

Exclusion rules are applied to remove definitions that should not be exposed in the selected environment.

## Default mappings

Catalog entries may contain:

```json
"defaultMappings": {}
```

Mappings provide contextual defaults for consumers.

Examples include:

```text
web.httpStatusCode
web.problemDetails
web.includeTraceId
cli.includeExitCode
desktop.showDialog
service.includeRetryInformation
production.includeStackTrace
```

Mappings are stored as extensible key-value pairs.

They allow presentation and platform behavior to evolve without adding a strongly typed property for every integration.

## Cross-catalog references

Catalog files are not independent.

Common references include:

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
```

The runtime validates these relationships before activating a context.

A catalog context is activated only when the complete set of documents is valid.

## Normalization

Catalog values may be normalized before validation and runtime use.

Normalization provides consistent handling of values such as:

* symbolic names,
* tags,
* aliases,
* prefixes,
* empty collections,
* optional text values.

Authors should still write canonical values in project files.

Normalization is not a substitute for clear catalog authoring.

## Validation

Validation checks individual documents and relationships between documents.

Typical validation concerns include:

* missing required identifiers,
* duplicate IDs,
* duplicate names,
* duplicate numeric codes,
* invalid code ranges,
* unresolved owners,
* unresolved code groups,
* unresolved categories,
* mismatched code prefixes,
* invalid profile references,
* contradictory or malformed values.

A context that fails validation is not activated.

## Source of truth

The project-local files are the source of truth for the project runtime.

Bundled package catalogs are:

* read-only resources,
* bootstrap templates,
* explicit defaults,
* optional fallback contexts.

They are not automatically merged into project files during normal runtime operation.

## Bootstrap behavior

During bootstrap, missing project files may be created from bundled templates.

Existing project files are never overwritten automatically.

The bootstrap process therefore follows this rule:

```text
missing file
→ create project copy

existing file
→ preserve exactly as owned by the project
```

## Runtime safety

The runtime may fall back to a valid previous context or bundled defaults, depending on the configured initialization mode.

Fallback changes the active in-memory context only.

It does not:

* repair invalid JSON,
* replace project files,
* delete project definitions,
* merge new package defaults into project catalogs,
* silently migrate schema versions.

Those operations belong to explicit authoring, migration, or maintenance tools.

## Authoring recommendations

When extending project catalogs:

1. Use stable symbolic names.
2. Keep published IDs and numeric codes unchanged.
3. Allocate codes inside the correct owner range.
4. Allocate codes inside the correct code-group range.
5. Reference existing owners, groups, and categories.
6. Use aliases only for intentional compatibility.
7. Keep public messages free of secrets and internal details.
8. Place technical guidance in `developerHint`.
9. Use tags consistently.
10. Validate the complete workspace after every change.

## Design principle

The catalog model follows one central idea:

> Error identity, classification, ownership, presentation, and selection are related, but they are not the same thing.

Keeping those concerns in separate catalogs makes the system easier to extend, validate, localize, and maintain.
