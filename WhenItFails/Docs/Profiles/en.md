# Profiles

Profiles define reusable views over the active error catalog.

A profile does not duplicate error definitions and does not create a separate catalog.

It selects existing error definitions according to:

* owners,
* code groups,
* categories,
* subcategories,
* tags,
* explicit error identifiers.

Profiles may also define presentation or integration defaults through mappings.

## Profile definition

A profile is represented by:

```csharp
ErrorProfileDefinition
```

Its main properties are:

```text
Name
DisplayName
Description
Source
IncludeOwners
IncludeCodeGroups
IncludeCategories
IncludeSubcategories
IncludeTags
IncludeErrors
ExcludeTags
ExcludeErrors
DefaultMappings
Metadata
```

## Example profile

```json
{
  "name": "WEB",
  "displayName": "Web",
  "description": "Errors suitable for web application presentation.",
  "source": "BuiltIn",
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
  "includeErrors": [],
  "excludeTags": [
    "INTERNAL_ONLY",
    "DEBUG_ONLY"
  ],
  "excludeErrors": [],
  "defaultMappings": {
    "web.problemDetails": "true",
    "web.includeTraceId": "true",
    "web.includeExceptionDetails": "false",
    "web.includeStackTrace": "false"
  },
  "metadata": {
    "Count": 0,
    "IsEmpty": true,
    "Items": {}
  }
}
```

## Profile identity

The stable profile identity is:

```text
name
```

Example:

```text
WEB
```

Profile names are normalized during loading and resolution.

Applications should still use clear canonical names in project files.

`displayName` is intended for human-readable interfaces.

## Profile source

The `source` property describes where the profile originated.

Typical values include:

```text
BuiltIn
Project
User
Imported
```

The value is intentionally stored as a string so projects and tools may introduce additional source types without changing the core model.

The source is descriptive metadata.

It does not by itself grant authority or change selection behavior.

## Include filters

A profile may include errors through:

```text
IncludeOwners
IncludeCodeGroups
IncludeCategories
IncludeSubcategories
IncludeTags
IncludeErrors
```

Include filters are combined using OR logic.

This means that an error is included when it matches at least one active include filter.

Conceptually:

```text
owner matches
OR
code group matches
OR
category matches
OR
subcategory matches
OR
tag matches
OR
explicit error ID matches
```

Example:

```json
{
  "includeCategories": [
    "NETWORK"
  ],
  "includeTags": [
    "RETRYABLE"
  ]
}
```

This profile includes:

* every error in the `NETWORK` category,
* every error tagged `RETRYABLE`,
* every error matching both.

It does not require an error to be both network-related and retryable.

## Empty include filters

An empty include collection means that the profile does not restrict selection by that dimension.

Example:

```json
{
  "includeOwners": [],
  "includeCodeGroups": [],
  "includeCategories": [],
  "includeSubcategories": [],
  "includeTags": [],
  "includeErrors": []
}
```

When every include collection is empty, all errors are initially eligible.

Exclusion filters may still remove some of them.

This behavior is useful for broad profiles such as:

```text
CLI
SERVICE
DEVELOPMENT
PRODUCTION
```

## Owner selection

Use:

```json
{
  "includeOwners": [
    "APP",
    "PLUGIN"
  ]
}
```

This selects errors owned by either `APP` or `PLUGIN`.

Owner selection is useful when a project wants to expose only:

* application errors,
* plugin errors,
* built-in errors,
* user-defined errors.

## Code-group selection

Use:

```json
{
  "includeCodeGroups": [
    "NETWORK",
    "FILE_SYSTEM"
  ]
}
```

This selects errors belonging to either code group.

Code groups represent numeric and symbolic error families.

## Category selection

Use:

```json
{
  "includeCategories": [
    "NETWORK",
    "EXTERNAL_SERVICE"
  ]
}
```

Category matching checks both:

```text
PrimaryCategory
Categories[]
```

An error matches when its primary category or any additional category appears in the profile.

## Subcategory selection

Use:

```json
{
  "includeSubcategories": [
    "CONNECTIVITY",
    "TIMEOUT"
  ]
}
```

This selects errors whose `Subcategories` collection contains at least one requested value.

Subcategories are useful for project-specific specialization without creating a new top-level category for every scenario.

## Tag selection

Use:

```json
{
  "includeTags": [
    "USER_VISIBLE",
    "RETRYABLE"
  ]
}
```

An error matches when it contains at least one included tag.

Because include filters use OR logic, this selects errors tagged either:

```text
USER_VISIBLE
or
RETRYABLE
```

## Explicit error inclusion

Use:

```json
{
  "includeErrors": [
    "AFW_NET_0001",
    "APP_IO_0007"
  ]
}
```

Explicit inclusion uses stable error identifiers.

It is useful when a profile needs a small precise set that does not fit naturally into category or tag filters.

Published stable IDs should be preferred over names or human-readable messages.

## Exclusion filters

A profile may exclude errors through:

```text
ExcludeTags
ExcludeErrors
```

Exclusions act as vetoes.

An error that matches an exclusion is removed even when it matches one or more include filters.

Conceptually:

```text
included by profile
+
not explicitly excluded
+
does not contain an excluded tag
```

## Excluded tags

Use:

```json
{
  "excludeTags": [
    "INTERNAL_ONLY",
    "DEBUG_ONLY"
  ]
}
```

Any error containing at least one excluded tag is removed.

This is useful for production-safe or user-safe profiles.

Example:

```json
{
  "includeCategories": [
    "NETWORK"
  ],
  "excludeTags": [
    "INTERNAL_ONLY"
  ]
}
```

This selects network errors except those marked `INTERNAL_ONLY`.

## Explicit error exclusion

Use:

```json
{
  "excludeErrors": [
    "AFW_GEN_0001"
  ]
}
```

Explicit exclusions use stable error identifiers.

They have final priority.

An explicitly excluded error remains excluded even when it matches:

* an included owner,
* an included code group,
* an included category,
* an included subcategory,
* an included tag,
* an explicit include ID.

Example:

```json
{
  "includeErrors": [
    "AFW_NET_0001"
  ],
  "excludeErrors": [
    "AFW_NET_0001"
  ]
}
```

The error is excluded.

## Selection order

Profile resolution follows this logical order:

```text
1. Check explicit error exclusion
2. Check excluded tags
3. Determine whether include filters exist
4. If no include filters exist, accept the error
5. Otherwise accept when any include filter matches
6. Preserve original catalog order
```

The resolver does not reorder selected errors.

The resulting collection keeps the order from the source error catalog.

## Matching normalization

Profile filters are normalized before matching.

Matching is case-insensitive after normalization.

For example, values such as:

```text
network
NETWORK
Network
```

may resolve to the same canonical key.

Authors should nevertheless use canonical uppercase symbolic names in catalog files for readability and consistency.

## Default mappings

Profiles may provide default mappings.

Example:

```json
{
  "defaultMappings": {
    "web.problemDetails": "true",
    "web.includeTraceId": "true",
    "web.includeExceptionDetails": "false"
  }
}
```

Mappings describe recommended behavior for consumers.

They may control:

* web presentation,
* CLI output,
* desktop dialogs,
* logging,
* tracing,
* retry information,
* exception visibility,
* environment-specific diagnostics.

Mappings do not directly modify error definitions.

The consuming presentation or integration layer decides how to interpret them.

## Built-in profiles

The default profile catalog currently includes profiles such as:

```text
WEB
API
CLI
DESKTOP
SERVICE
DEVELOPMENT
PRODUCTION
```

These provide common starting points.

Projects are free to modify project-local copies or add their own profiles.

Bundled profiles remain read-only package resources.

## Custom project profiles

Projects may define domain-specific profiles.

Example:

```json
{
  "name": "DITA",
  "displayName": "Disk and storage",
  "description": "Errors used by disk testing, filesystem, storage and hardware diagnostics.",
  "source": "Project",
  "includeOwners": [
    "AFW",
    "APP"
  ],
  "includeCodeGroups": [
    "FILE_SYSTEM"
  ],
  "includeCategories": [
    "FILE_SYSTEM"
  ],
  "includeSubcategories": [
    "DISK",
    "FILESYSTEM",
    "STORAGE",
    "SMART"
  ],
  "includeTags": [
    "DISK",
    "STORAGE"
  ],
  "includeErrors": [],
  "excludeTags": [
    "DEBUG_ONLY"
  ],
  "excludeErrors": [],
  "defaultMappings": {
    "dita.includeDeviceIdentity": "true",
    "dita.includeSmartSummary": "true",
    "dita.includeRawException": "false"
  },
  "metadata": {
    "Count": 0,
    "IsEmpty": true,
    "Items": {}
  }
}
```

Because include filters use OR logic, this profile selects any error matching at least one of the supplied owners, groups, categories, subcategories or tags.

A narrowly targeted profile should therefore avoid broad include filters that unintentionally select unrelated errors.

## Broad filters require care

Consider:

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

This does not mean:

```text
AFW-owned network errors only
```

It means:

```text
all AFW-owned errors
OR
all network errors
```

To produce a narrow intersection-style selection, the current profile model may require:

* more specific tags,
* more specific subcategories,
* explicit error IDs,
* carefully designed ownership boundaries,
* a future advanced selection mechanism.

This OR behavior should be understood before creating custom profiles.

## Resolving a profile

Use:

```csharp
Response<IReadOnlyList<ErrorDefinition>> response =
    runtime.ResolveProfile(
        "WEB");
```

The profile is resolved against the currently active catalog context.

Changing the active context may therefore change the definitions selected by the same profile name.

## Missing profile

When the requested profile cannot be resolved, the runtime returns a structured failure response.

Applications should not silently treat a missing profile as an empty successful selection.

A missing profile usually indicates:

* a spelling mistake,
* an invalid deployment,
* a missing project file,
* an unexpected active context,
* a failed catalog update.

## Profile validation

Profile validation checks concerns such as:

* missing profile name,
* duplicate profile names,
* invalid owner references,
* invalid code-group references,
* invalid category references,
* invalid explicit error IDs,
* contradictory or malformed values.

The complete catalog context must validate before activation.

## Profiles and environment policy

Profiles may describe both selection and recommended behavior.

Examples:

```text
WEB
→ user-safe web errors

DEVELOPMENT
→ detailed diagnostics

PRODUCTION
→ suppress sensitive information

SERVICE
→ include tracing and retry information

CLI
→ include hints and exit-code mappings
```

The profile itself does not enforce security.

The consuming layer must apply the mappings and decide what information is safe to expose.

## Profiles are portable

Project profiles are stored in JSON and may be:

* committed to version control,
* copied between projects,
* exported,
* imported,
* reviewed,
* validated in CI,
* packaged with metadata and documentation.

A portable profile package may eventually contain:

```text
profile JSON
metadata
version
compatibility information
README
optional referenced definitions
```

Import and export must remain explicit operations.

Runtime initialization must not silently import or merge external profiles.

## Authoring recommendations

1. Use a stable uppercase profile name.
2. Give the profile a clear human-readable display name.
3. Document its intended audience and environment.
4. Remember that include filters use OR logic.
5. Keep broad include filters intentional.
6. Use excluded tags for safety boundaries.
7. Use explicit error exclusions sparingly.
8. Prefer stable error IDs for precise selection.
9. Keep mappings namespaced by consumer or platform.
10. Validate the complete catalog after every change.
11. Keep profiles free of duplicated full error definitions.
12. Treat imported profiles as reviewed project data.

## Central principle

> A profile is a reusable query and policy recommendation over one active catalog, not a second copy of that catalog.
