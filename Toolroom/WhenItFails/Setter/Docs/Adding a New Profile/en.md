# Adding a New Profile

This guide explains how to add a new profile to the WhenItFails catalog.

Profiles live in:
A profile is a named context for selecting catalog entries and describing default presentation or behavior.

## Main principle

A good profile is:
Do not create a profile just because one temporary filter was useful once.

Create a profile when an application context is likely to be reused.

## What a profile is

A profile describes a reusable context.

Examples:
A profile can say:
It is not an error definition.

It is not a category.

It is not an owner.

## When to add a profile

Add a new profile when you need a stable reusable policy.

Good reasons:

- a public API needs different output than an admin UI,
- a CLI tool should include hints and exit-code information,
- a disk tool needs storage-specific warnings,
- support tooling needs extra diagnostics,
- production must hide details that development can show,
- a worker/service context needs retry and trace information.

Weak reasons:

- one temporary debug session,
- one local experiment,
- one user preference,
- avoiding a normal `errors --search` command,
- duplicating an existing profile with a different display name.

## Recommended workflow

Use this workflow:
Bash:
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
  -- summary .

dotnet run \
  --project Toolroom/WhenItFails/Setter \
  -- errors . --profile DISK_TOOL

git diff -- Jsons/WhenItFails/profiles.json
PowerShell:
dotnet run `
  --project Toolroom/WhenItFails/Setter `
  -- validate .

dotnet run `
  --project Toolroom/WhenItFails/Setter `
  -- summary .

# edit Jsons/WhenItFails/profiles.json

dotnet run `
  --project Toolroom/WhenItFails/Setter `
  -- validate .

dotnet run `
  --project Toolroom/WhenItFails/Setter `
  -- summary .

dotnet run `
  --project Toolroom/WhenItFails/Setter `
  -- errors . --profile DISK_TOOL

git diff -- Jsons/WhenItFails/profiles.json
## Profile shape

A typical profile contains:
{
  "name": "DISK_TOOL",
  "displayName": "Disk tool",
  "description": "Profile for disk diagnostics and storage tools.",
  "includeOwners": [ "AFW", "APP" ],
  "includeCodeGroups": [ "FILE_SYSTEM" ],
  "includeCategories": [ "FILE_SYSTEM" ],
  "includeSubcategories": [],
  "includeTags": [],
  "excludeTags": [ "DEBUG_ONLY" ],
  "defaultMappings": {
    "cli.includeHints": "true",
    "disk.includeDeviceIdentifier": "true",
    "disk.includeDestructiveWarning": "true"
  }
}
Use the active catalog vocabulary.

Do not copy an example without reviewing it.

## Profile name

The `name` field is the stable profile identifier.

Example:
"name": "DISK_TOOL"
A good profile name is:

- stable,
- uppercase if that is the catalog convention,
- machine-friendly,
- short enough to type,
- specific enough to understand,
- not tied to a temporary project name unless the profile is truly project-specific.

Good:
Weak:
## Display name

The `displayName` field is a human-readable label.

Example:
"displayName": "Disk tool"
A good display name is:

- readable,
- short,
- pleasant in terminal output,
- not overloaded with implementation details.

The display name can be nicer than the stable profile name.

## Description

The `description` field explains where the profile should be used.

Good:
"description": "Profile for public API responses where exception details must not be exposed."
Weak:
"description": "Stuff for API."
A profile description should answer:
## Included owners

The `includeOwners` array restricts the profile by owner.

Example:
"includeOwners": [ "AFW", "APP" ]
Use known owner names from:
An empty array means:
For current Setter browsing, included owners participate in `errors --profile` filtering.

## Included code groups

The `includeCodeGroups` array restricts the profile by code group.

Example:
"includeCodeGroups": [ "NETWORK", "DATABASE" ]
Use known code-group names from:
An empty array means:
For current Setter browsing, included code groups participate in `errors --profile` filtering.

## Included categories

The `includeCategories` array restricts the profile by primary category.

Example:
"includeCategories": [ "NETWORK", "VALIDATION" ]
Use known category names from:
An empty array means:
For current Setter browsing, included categories participate in `errors --profile` filtering.

## Included subcategories

The `includeSubcategories` array is available for more detailed filtering.

Example:
"includeSubcategories": [ "TIMEOUT" ]
Important current Setter note:
does not currently apply `includeSubcategories` in its simplified profile filter.

Use subcategories deliberately, but do not assume current Setter browsing fully enforces them.

## Included tags

The `includeTags` array describes tags the profile may want to include.

Example:
"includeTags": [ "USER_VISIBLE" ]
Important current Setter note:
does not currently apply `includeTags` in its simplified profile filter.

Runtime consumers may interpret tags more fully than Setter browsing currently does.

## Excluded tags

The `excludeTags` array describes tags the profile may want to hide.

Example:
"excludeTags": [ "INTERNAL_ONLY", "DEBUG_ONLY" ]
Important current Setter note:
does not currently apply `excludeTags` in its simplified profile filter.

Do not use Setter browsing alone as proof that production runtime filtering is complete.

## Default mappings

The `defaultMappings` object stores profile-specific defaults.

Example:
"defaultMappings": {
  "production.includeExceptionDetails": "false",
  "production.includeStackTrace": "false",
  "production.includeSensitiveMetadata": "false"
}
Mappings are string key-value pairs.

They may represent presentation behavior, diagnostics, safety policy, or runtime hints.

## Mapping key style

Use predictable mapping keys.

Recommended pattern:
Examples:
Avoid vague keys:
## Mapping values are strings

Current mappings store values as strings.

Example:
"cli.includeHints": "true"
This is a JSON string, not a JSON boolean.

Consumers should parse values deliberately.

Catalog authors should stay consistent with the existing format.

## Empty include arrays

For the current Setter profile filter:
When multiple include arrays are non-empty, they combine with logical AND.

Within one include array, values combine with logical OR.

Example:
"includeOwners": [ "AFW", "APP" ],
"includeCodeGroups": [ "NETWORK", "DATABASE" ],
"includeCategories": []
Conceptually means:
## Designing a production profile

Production profiles should be conservative.

Recommended mappings:
"production.includeExceptionDetails": "false"
"production.includeStackTrace": "false"
"production.includeSensitiveMetadata": "false"
Production profiles should normally exclude:
Before committing a production profile, check that it does not expose:

- stack traces,
- exception details,
- sensitive metadata,
- private paths,
- raw SQL,
- credentials,
- tokens,
- customer data.

## Designing a development profile

Development profiles may expose more detail.

Useful mappings:
"development.includeExceptionDetails": "true"
"development.includeStackTrace": "true"
"development.includeCatalogDiagnostics": "true"
Development behavior must not silently leak into production defaults.

Keep development and production profile names visibly separate.

## Designing a CLI profile

CLI profiles often care about:

- hints,
- exit codes,
- colors,
- concise messages,
- terminal-friendly output.

Example mappings:
"cli.includeHints": "true"
"cli.includeExitCode": "true"
"cli.includeColors": "true"
CLI does not automatically mean development.

A CLI can be production-facing.

## Designing a web or API profile

Web/API profiles often care about:

- trace IDs,
- safe messages,
- problem details,
- no exception details,
- no stack traces,
- HTTP status mappings.

Example mappings:
"web.problemDetails": "true"
"web.includeTraceId": "true"
"web.includeExceptionDetails": "false"
"web.includeStackTrace": "false"
## Designing a support profile

Support profiles may include additional diagnostics.

However, support-facing does not automatically mean safe to expose secrets.

A support profile should still avoid:

- credentials,
- tokens,
- raw customer data,
- private keys,
- unnecessary stack traces.

Support output should be useful, not reckless.

## Custom profile example

Example for a disk diagnostics tool:
{
  "name": "DISK_TOOL",
  "displayName": "Disk tool",
  "description": "Profile for disk diagnostics, storage checks, and user-safe device health messages.",
  "includeOwners": [ "AFW", "APP" ],
  "includeCodeGroups": [ "FILE_SYSTEM" ],
  "includeCategories": [ "FILE_SYSTEM" ],
  "includeSubcategories": [],
  "includeTags": [ "USER_VISIBLE" ],
  "excludeTags": [ "DEBUG_ONLY", "INTERNAL_ONLY" ],
  "defaultMappings": {
    "cli.includeHints": "true",
    "cli.includeExitCode": "true",
    "disk.includeDeviceIdentifier": "true",
    "disk.includeDestructiveWarning": "true",
    "production.includeStackTrace": "false"
  }
}
Review the exact code groups and categories available in your catalog before using this example.

## Validate after editing

Run:
dotnet run \
  --project Toolroom/WhenItFails/Setter \
  -- validate .
PowerShell:
dotnet run `
  --project Toolroom/WhenItFails/Setter `
  -- validate .
Expected exit code:
If validation fails, fix the profile before continuing.

## Inspect summary

Run:
dotnet run \
  --project Toolroom/WhenItFails/Setter \
  -- summary .
Review whether the new profile appears with the expected:

- name,
- display name,
- owners,
- code groups,
- categories.

## Test profile browsing

Run:
dotnet run \
  --project Toolroom/WhenItFails/Setter \
  -- errors . --profile DISK_TOOL
Plain output:
dotnet run \
  --project Toolroom/WhenItFails/Setter \
  -- errors . --profile DISK_TOOL --plain
Search within the profile:
dotnet run \
  --project Toolroom/WhenItFails/Setter \
  -- errors . --profile DISK_TOOL --search disk
Use the actual profile name.

## Test unknown profile behavior

Run a negative check:
dotnet run \
  --project Toolroom/WhenItFails/Setter \
  -- errors . --profile DOES_NOT_EXIST
Expected issue code:
Expected exit code:
PowerShell:
$LASTEXITCODE
## Review Git diff

Run:
git diff -- Jsons/WhenItFails/profiles.json
Then:
git diff --check
Confirm:

- only intended profile content changed,
- JSON formatting is clean,
- no unrelated profile was changed,
- production mappings remain safe,
- no backup or temporary files are included.

## Profile review checklist

Before commit, confirm:
## Common mistakes

Common profile mistakes include:

- using display names instead of stable names,
- creating duplicate profile names,
- using unknown owners,
- using unknown code groups,
- using unknown categories,
- assuming current Setter browsing applies include/exclude tags,
- exposing development details in production,
- adding vague mappings,
- making mappings booleans while the catalog uses strings,
- copying another profile without changing description,
- creating a profile that no application will use.

## Commit message

Good:
Weak:
## Related documentation

- [Profiles](../Profiles/en.md)
- [Catalog Files](../Catalog%20Files/en.md)
- [Catalog Author Checklist](../Catalog%20Author%20Checklist/en.md)
- [Validation](../Validation/en.md)
- [Workspace Summary](../Workspace%20Summary/en.md)
- [Browsing and Filtering Errors](../Browsing%20and%20Filtering%20Errors/en.md)
- [Glossary](../Glossary/en.md)

## Central principle

> Add a profile only when the catalog needs a stable named context, not merely because one temporary filter was convenient.