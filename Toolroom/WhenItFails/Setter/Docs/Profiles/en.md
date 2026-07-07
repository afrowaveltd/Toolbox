# Profiles

Profiles are named views of the WhenItFails error catalog.

They allow a project to describe which errors and presentation settings are useful for a particular application context, such as:

- web applications,
- APIs,
- command-line tools,
- desktop applications,
- background services,
- development,
- production.

A profile is not an error definition.

A profile is a reusable selection and configuration layer over the catalog.

## Profile catalog file

Profiles are stored in:

```text
Jsons/WhenItFails/profiles.json
```

The file is part of the WhenItFails JSON catalog package and is validated together with the other catalog files.

## Current default profiles

The current default profile catalog contains these profiles:

```text
WEB
API
CLI
DESKTOP
SERVICE
DEVELOPMENT
PRODUCTION
```

Each profile has:

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

## Profile name

The `name` field is the stable profile identifier.

Example:

```json
"name": "WEB"
```

Use the profile name when scripting or documenting stable behavior.

Example:

```bash
dotnet run \
  --project Toolroom/WhenItFails/Setter \
  -- errors . --profile WEB
```

## Display name

The `displayName` field is human-readable.

Example:

```json
"displayName": "Web"
```

Setter can currently find a profile by either:

```text
name
displayName
```

Matching is case-insensitive.

Example:

```bash
dotnet run \
  --project Toolroom/WhenItFails/Setter \
  -- errors . --profile Web
```

For automation, prefer the stable profile name.

## Description

The `description` field explains the intended use of the profile.

Example:

```json
"description": "Default profile for web applications with user-safe error presentation."
```

Descriptions are for humans.

They should not be parsed by scripts.

## Included owners

The `includeOwners` collection limits the profile to selected owners.

Example:

```json
"includeOwners": [ "AFW", "APP", "PLUGIN", "USER" ]
```

An empty collection means:

```text
do not restrict by owner
```

In the current Setter `errors --profile` command, included owners participate in profile filtering.

## Included code groups

The `includeCodeGroups` collection limits the profile to selected code groups.

Example:

```json
"includeCodeGroups": [ "NETWORK", "DATABASE" ]
```

An empty collection means:

```text
do not restrict by code group
```

In the current Setter `errors --profile` command, included code groups participate in profile filtering.

## Included categories

The `includeCategories` collection limits the profile to selected primary categories.

Example:

```json
"includeCategories": [
  "GENERAL",
  "CONFIGURATION",
  "VALIDATION",
  "SECURITY",
  "NETWORK"
]
```

An empty collection means:

```text
do not restrict by category
```

In the current Setter `errors --profile` command, included categories participate in profile filtering.

## Included subcategories

The `includeSubcategories` collection is available in the profile document.

Example:

```json
"includeSubcategories": []
```

It is intended for more detailed profile selection.

Important current Setter note:

```text
errors --profile
```

does not currently use `includeSubcategories` in its simplified profile filtering.

Treat subcategory-aware profile behavior as runtime or future Setter behavior unless the implementation is updated.

## Included tags

The `includeTags` collection describes tags that a profile may want to include.

Example:

```json
"includeTags": [ "USER_VISIBLE" ]
```

Important current Setter note:

```text
errors --profile
```

does not currently use `includeTags` in its simplified profile filtering.

This matters because default profiles such as `WEB`, `API`, and `DESKTOP` include `USER_VISIBLE`, but the current Setter profile list filter is based only on owners, code groups, and categories.

## Excluded tags

The `excludeTags` collection describes tags that a profile may want to hide.

Example:

```json
"excludeTags": [ "INTERNAL_ONLY", "DEBUG_ONLY" ]
```

Important current Setter note:

```text
errors --profile
```

does not currently use `excludeTags` in its simplified profile filtering.

Do not assume that `errors --profile PRODUCTION` perfectly represents the complete runtime production filter.

## Default mappings

The `defaultMappings` object stores profile-specific presentation or behavior settings.

Examples:

```json
"defaultMappings": {
  "web.problemDetails": "true",
  "web.includeTraceId": "true",
  "web.includeExceptionDetails": "false",
  "web.includeStackTrace": "false"
}
```

Mappings are string key-value pairs.

They allow a profile to say not only:

```text
which errors are relevant
```

but also:

```text
how errors should normally be presented or handled
```

## Mapping examples

Web profile mappings:

```json
"web.problemDetails": "true"
```

```json
"web.includeTraceId": "true"
```

```json
"web.includeExceptionDetails": "false"
```

CLI profile mappings:

```json
"cli.includeHints": "true"
```

```json
"cli.includeExitCode": "true"
```

```json
"cli.includeColors": "true"
```

Production profile mappings:

```json
"production.includeExceptionDetails": "false"
```

```json
"production.includeStackTrace": "false"
```

```json
"production.includeSensitiveMetadata": "false"
```

## Profile filtering in Setter

The current Setter `errors --profile` command uses a simplified profile match.

It currently evaluates:

```text
includeOwners
includeCodeGroups
includeCategories
```

It does not currently evaluate:

```text
includeSubcategories
includeTags
excludeTags
defaultMappings
```

This distinction is important.

The Setter list view is useful for quick catalog browsing, but it should not be treated as a byte-for-byte runtime profile resolver unless the command is changed to delegate to the same runtime profile logic.

## Empty include collections

For the current Setter profile filter:

```text
empty includeOwners
→ accept all owners
```

```text
empty includeCodeGroups
→ accept all code groups
```

```text
empty includeCategories
→ accept all primary categories
```

If more than one include dimension is non-empty, the dimensions combine with logical AND.

Within one dimension, values combine with logical OR.

Example:

```json
"includeOwners": [ "AFW", "APP" ],
"includeCodeGroups": [ "NETWORK", "DATABASE" ],
"includeCategories": []
```

Means conceptually:

```text
(owner is AFW or APP)
AND
(code group is NETWORK or DATABASE)
AND
(any primary category)
```

## WEB profile

The `WEB` profile is intended for web applications with user-safe error presentation.

Typical concerns:

- safe user-facing messages,
- trace IDs,
- Problem Details style output,
- hiding exception details,
- hiding stack traces.

The default `WEB` profile includes common application owners and common user-relevant categories.

It also includes the `USER_VISIBLE` tag and excludes internal/debug tags for runtime-oriented presentation settings.

## API profile

The `API` profile is intended for HTTP APIs and service endpoints.

Typical concerns:

- machine-readable HTTP responses,
- trace IDs,
- safe error body content,
- no stack traces,
- no exception details in normal responses.

Its mappings are similar to web-style problem-details behavior.

## CLI profile

The `CLI` profile is intended for command-line applications.

Typical concerns:

- readable terminal output,
- useful hints,
- exit-code visibility,
- colors when appropriate.

Example mappings:

```json
"cli.includeHints": "true"
```

```json
"cli.includeExitCode": "true"
```

```json
"cli.includeColors": "true"
```

The `CLI` profile currently has broad include collections, which means it is not heavily restricted by owner, code group, or category in the current Setter list filter.

## DESKTOP profile

The `DESKTOP` profile is intended for desktop applications.

Typical concerns:

- safe dialogs,
- optional details button,
- hiding exception details by default,
- user-visible descriptions.

Example mappings:

```json
"desktop.showDialog": "true"
```

```json
"desktop.includeDetailsButton": "true"
```

```json
"desktop.includeExceptionDetails": "false"
```

## SERVICE profile

The `SERVICE` profile is intended for background services, workers, daemons, scheduled jobs, and hosted services.

Typical concerns:

- trace IDs,
- retry information,
- operator diagnostics,
- avoiding exception details in unsafe output.

Example mappings:

```json
"service.includeTraceId": "true"
```

```json
"service.includeRetryInformation": "true"
```

```json
"service.includeExceptionDetails": "false"
```

## DEVELOPMENT profile

The `DEVELOPMENT` profile is intended for development and debugging.

Typical concerns:

- exception details,
- stack traces,
- catalog diagnostics,
- faster local troubleshooting.

Example mappings:

```json
"development.includeExceptionDetails": "true"
```

```json
"development.includeStackTrace": "true"
```

```json
"development.includeCatalogDiagnostics": "true"
```

Development behavior is not production behavior.

Do not copy development mappings into production profiles without deliberate review.

## PRODUCTION profile

The `PRODUCTION` profile is intended for production-safe error presentation.

Typical concerns:

- no exception details,
- no stack traces,
- no sensitive metadata,
- no debug-only output.

Example mappings:

```json
"production.includeExceptionDetails": "false"
```

```json
"production.includeStackTrace": "false"
```

```json
"production.includeSensitiveMetadata": "false"
```

Production profiles should be conservative.

## Shadow-copy template

The default profile catalog currently contains:

```json
"isShadowCopy": true
```

and tags such as:

```json
"default"
```

```json
"profiles"
```

```json
"shadow-copy-template"
```

This expresses that the profile catalog can act as a default template for project-local customization.

Project-local catalogs may evolve from this template.

## Custom project profiles

A project can add its own profiles.

Example:

```json
{
  "name": "DISK_TOOL",
  "displayName": "Disk tool",
  "description": "Profile for disk diagnostics and storage tools.",
  "includeOwners": [ "AFW", "APP" ],
  "includeCodeGroups": [ "STORAGE", "FILE_SYSTEM" ],
  "includeCategories": [ "STORAGE", "FILE_SYSTEM" ],
  "includeSubcategories": [],
  "includeTags": [],
  "excludeTags": [ "DEBUG_ONLY" ],
  "defaultMappings": {
    "cli.includeHints": "true",
    "disk.includeDeviceIdentifier": "true",
    "disk.includeDestructiveWarning": "true"
  }
}
```

The exact profile depends on the catalog vocabulary available in the workspace.

Always validate after adding or changing a profile.

## Naming custom profiles

Recommended profile names:

```text
uppercase
short
stable
machine-friendly
```

Examples:

```text
DISK_TOOL
ADMIN_PORTAL
PUBLIC_API
WORKER
SUPPORT
```

Avoid:

```text
My temporary profile final 2
```

```text
profile-for-today
```

```text
test please delete
```

Profile names may become part of scripts, tests, documentation, and support procedures.

## Display names for custom profiles

Display names should be pleasant for humans.

Examples:

```text
Disk tool
Admin portal
Public API
Worker
Support
```

The display name can change more safely than the profile name, but avoid unnecessary churn.

## Profile descriptions

A profile description should answer:

```text
Where should this profile be used?
```

Good:

```text
Profile for public API responses where exception details must not be exposed.
```

Weak:

```text
Stuff for API.
```

## Profile mappings as policy

Mappings can represent profile policy.

Examples:

```json
"production.includeStackTrace": "false"
```

```json
"web.includeExceptionDetails": "false"
```

```json
"cli.includeHints": "true"
```

Treat these values as policy defaults.

A consuming application may still decide exactly how to apply them.

## Boolean-like mapping values

Current mappings store values as strings.

Example:

```json
"web.includeTraceId": "true"
```

Do not assume these are JSON booleans.

They are string values unless the model changes.

Consumers should parse them deliberately.

## Mapping key naming

Use predictable names.

Recommended pattern:

```text
context.settingName
```

Examples:

```text
web.includeTraceId
cli.includeHints
production.includeStackTrace
service.includeRetryInformation
```

Avoid vague keys:

```text
flag1
thing
show
x
```

## Selecting a profile in Setter

List errors for a profile:

```bash
dotnet run \
  --project Toolroom/WhenItFails/Setter \
  -- errors . --profile WEB
```

Plain output:

```bash
dotnet run \
  --project Toolroom/WhenItFails/Setter \
  -- errors . --profile WEB --plain
```

Combine with other filters:

```bash
dotnet run \
  --project Toolroom/WhenItFails/Setter \
  -- errors . \
  --profile WEB \
  --severity Error \
  --search network
```

Profile filtering combines with ordinary filters.

## Unknown profile

Example:

```bash
dotnet run \
  --project Toolroom/WhenItFails/Setter \
  -- errors . --profile DOES_NOT_EXIST
```

Expected issue code:

```text
UnknownProfileFilter
```

Expected exit code:

```text
1
```

## Reviewing profile effects

Use summary first:

```bash
dotnet run \
  --project Toolroom/WhenItFails/Setter \
  -- summary .
```

The summary shows each profile with:

```text
Profile
Display name
Owners
Code groups
Categories
```

Then list errors for a specific profile:

```bash
dotnet run \
  --project Toolroom/WhenItFails/Setter \
  -- errors . --profile WEB
```

## Profile review checklist

When reviewing a profile, ask:

- Is the profile name stable?
- Is the display name clear?
- Does the description state the target context?
- Are included owners intentional?
- Are included code groups intentional?
- Are included categories intentional?
- Are tag rules documented and understood?
- Are default mappings safe for the target context?
- Is production output conservative?
- Is development output clearly not production output?
- Does validation pass?
- Does `summary` show the expected include dimensions?
- Does `errors --profile` return a plausible list?

## Safe profile editing

Recommended workflow:

```text
validate
→ summary
→ edit profiles.json
→ validate
→ summary
→ errors --profile <name>
→ review Git diff
```

Example:

```bash
dotnet run \
  --project Toolroom/WhenItFails/Setter \
  -- validate .

dotnet run \
  --project Toolroom/WhenItFails/Setter \
  -- summary .

# edit Jsons/WhenItFails/profiles.json

dotnet run \
  --project Toolroom/WhenItFails/Setter \
  -- validate .

dotnet run \
  --project Toolroom/WhenItFails/Setter \
  -- errors . --profile WEB

git diff -- Jsons/WhenItFails/profiles.json
```

## Current Setter limitation

Setter currently does not provide dedicated commands such as:

```text
set-profile-description
add-profile
remove-profile
set-profile-mapping
```

Profile editing is currently a manual JSON editing task.

Validate immediately after manual edits.

## Manual editing caution

When editing `profiles.json` manually:

- keep valid JSON,
- preserve expected property names,
- avoid duplicate profile names,
- use stable casing,
- check string arrays carefully,
- avoid accidental whole-file formatting churn,
- validate the complete workspace afterward.

## When to create a new profile

Create a new profile when an application context has a stable and reusable policy.

Good reasons:

- public API differs from internal admin UI,
- CLI tools should include hints but not web problem details,
- production should hide data that development can show,
- a disk diagnostic tool needs storage-specific behavior,
- support tooling needs additional catalog diagnostics.

Poor reasons:

- one temporary debug session,
- one user preference,
- one experimental local script,
- avoiding a proper command-line filter,
- duplicating another profile with a different display name only.

## Profile versus filter

Use a filter for one-time exploration:

```bash
dotnet run \
  --project Toolroom/WhenItFails/Setter \
  -- errors . --severity Warning --search network
```

Use a profile for reusable context:

```bash
dotnet run \
  --project Toolroom/WhenItFails/Setter \
  -- errors . --profile WEB
```

A profile is a named contract.

A filter is an ad-hoc query.

## Profile versus category

A category classifies an error.

A profile selects errors for a context.

Example:

```text
NETWORK
```

may be a category.

```text
WEB
```

may be a profile.

A web application may include network errors, validation errors, security errors, and serialization errors.

Therefore a profile is usually broader than one category.

## Profile versus owner

An owner describes responsibility.

A profile describes usage context.

Example:

```text
AFW
```

may own an error.

```text
CLI
```

may include that error because it is relevant to command-line applications.

Do not use owners as substitutes for profiles.

## Profile versus tag

A tag is a flexible label on an error definition.

A profile may include or exclude tags.

Example:

```text
USER_VISIBLE
```

may be a tag.

```text
PRODUCTION
```

may be a profile that excludes internal-only and debug-only tags.

## Production safety

Production profiles should usually avoid exposing:

- stack traces,
- exception details,
- sensitive metadata,
- internal hostnames,
- credentials,
- private paths,
- raw SQL,
- debug-only diagnostic payloads.

Use production mappings conservatively.

## Development convenience

Development profiles may expose additional detail to make debugging faster.

However, development settings should not leak into production defaults.

Keep development and production profiles separate and visibly named.

## Testing profile changes

Suggested tests after changing `profiles.json`:

```bash
dotnet run \
  --project Toolroom/WhenItFails/Setter \
  -- validate .
```

```bash
dotnet run \
  --project Toolroom/WhenItFails/Setter \
  -- summary .
```

```bash
dotnet run \
  --project Toolroom/WhenItFails/Setter \
  -- errors . --profile WEB
```

```bash
dotnet run \
  --project Toolroom/WhenItFails/Setter \
  -- errors . --profile PRODUCTION
```

```bash
dotnet run \
  --project Toolroom/WhenItFails/Setter \
  -- errors . --profile DOES_NOT_EXIST

echo "Exit code: $?"
```

Expected unknown-profile exit code:

```text
1
```

## Future improvements

Possible future Setter improvements include:

- profile listing command,
- profile detail command,
- dedicated profile editing commands,
- runtime-equivalent profile filtering,
- tag-aware Setter profile browsing,
- profile diff view,
- profile validation explanations,
- JSON output for profile views,
- profile export and import,
- portable profile packages,
- profile templates.

These are future possibilities, not current guarantees.

## Related documentation

- [Browsing and Filtering Errors](../Browsing%20and%20Filtering%20Errors/en.md)
- [Workspace Summary](../Workspace%20Summary/en.md)
- [Validation](../Validation/en.md)
- [Authoring Error Text](../Authoring%20Error%20Text/en.md)
- [Command Quick Reference](../Command%20Quick%20Reference/en.md)
- [Documentation Map](../Documentation%20Map/en.md)
- [Glossary](../Glossary/en.md)

## Central principle

> A profile is a named context: it says which catalog entries and presentation defaults make sense for a particular kind of application or environment.
