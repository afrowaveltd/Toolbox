# Profiles

Profiles are named selections and configuration layers over the WhenItFails error catalog.

They answer two separate questions:

1. Which errors are relevant for this application context?
2. Which presentation or handling defaults should normally be used?

Profiles are stored in:

```text
Jsons/WhenItFails/profiles.json
```

The profile catalog is loaded and validated together with the other WhenItFails catalogs.

## Profile fields

A profile may contain:

```text
name
displayName
description
source
includeOwners
includeCodeGroups
includeCategories
includeSubcategories
includeTags
includeErrors
excludeTags
excludeErrors
defaultMappings
metadata
```

`name` is the stable machine-facing identifier. `displayName` and `description` are human-facing text.

For automation, prefer the stable profile name.

## Selection semantics

Setter uses the shared runtime `ErrorProfileResolver` for:

```text
errors <path> --profile <profile-name>
```

This keeps CLI filtering aligned with runtime profile resolution.

### Include rules

The following collections are include selectors:

```text
includeOwners
includeCodeGroups
includeCategories
includeSubcategories
includeTags
includeErrors
```

All non-empty include collections participate in one combined OR expression.

An error is included when it matches at least one configured include selector.

Example:

```json
{
  "includeCategories": [ "NETWORK" ],
  "includeTags": [ "STORAGE" ]
}
```

This means:

```text
primary or additional category is NETWORK
OR
tag is STORAGE
```

It does not require both conditions.

If all include collections are empty, every catalog error is initially eligible.

### Exclude rules

The following collections are exclusion vetoes:

```text
excludeTags
excludeErrors
```

An error matching an exclusion rule is removed even when it also matches an include rule.

Explicitly excluded error IDs therefore have final priority.

### Catalog order

Resolved errors preserve their original catalog order.

## Included owners

`includeOwners` contains normalized owner names.

Example:

```json
"includeOwners": [ "AFW", "APP" ]
```

Setter commands:

```text
profile-add-owner
profile-remove-owner
```

## Included code groups

`includeCodeGroups` contains normalized code-group names.

Example:

```json
"includeCodeGroups": [ "NETWORK", "DATABASE" ]
```

Setter commands:

```text
profile-add-code-group
profile-remove-code-group
```

## Included categories

`includeCategories` matches both primary and additional error categories.

Example:

```json
"includeCategories": [ "NETWORK", "FILE_SYSTEM" ]
```

Setter commands:

```text
profile-add-category
profile-remove-category
```

## Included subcategories

`includeSubcategories` matches normalized error subcategories.

Example:

```json
"includeSubcategories": [ "DNS", "TIMEOUT" ]
```

Setter commands:

```text
profile-add-subcategory
profile-remove-subcategory
```

## Included tags

`includeTags` matches normalized error tags.

Example:

```json
"includeTags": [ "USER_VISIBLE" ]
```

Setter commands:

```text
profile-add-tag
profile-remove-tag
```

## Explicitly included errors

`includeErrors` contains stable normalized error IDs.

Example:

```json
"includeErrors": [ "AFW_NET_0001" ]
```

The CLI accepts an error ID, numeric code, or symbolic name and stores the canonical error ID.

Setter commands:

```text
profile-add-error
profile-remove-error
```

## Excluded tags

`excludeTags` removes errors carrying any listed tag.

Example:

```json
"excludeTags": [ "INTERNAL_ONLY", "DEBUG_ONLY" ]
```

Setter commands:

```text
profile-add-excluded-tag
profile-remove-excluded-tag
```

## Explicitly excluded errors

`excludeErrors` contains stable normalized error IDs.

Example:

```json
"excludeErrors": [ "AFW_NET_0001" ]
```

The CLI accepts an error ID, numeric code, or symbolic name and stores the canonical error ID.

Setter commands:

```text
profile-add-excluded-error
profile-remove-excluded-error
```

## Default mappings

`defaultMappings` stores string key-value pairs describing presentation or runtime defaults.

Example:

```json
"defaultMappings": {
  "WEB_PROBLEMDETAILS": "true",
  "WEB_INCLUDETRACEID": "true"
}
```

Setter normalizes mapping keys with `TextKeyNormalizer.NormalizeKey`. Punctuation groups become underscores and letters become uppercase.

Example:

```text
web.problemDetails
→ WEB_PROBLEMDETAILS
```

Values remain trimmed strings. Consumers should parse boolean-like or numeric values deliberately.

Setter commands:

```text
profile-set-default-mapping
profile-remove-default-mapping
```

## Metadata

`metadata` stores additional string key-value information that does not directly participate in profile resolution.

Example:

```json
"metadata": {
  "DOCUMENTATION_OWNER": "DiTa Team"
}
```

Metadata keys are normalized with the same project key normalizer. Values are trimmed strings.

Metadata is serialized as an ordinary JSON object through `MetadataBagJsonConverter`.

Setter commands:

```text
profile-set-metadata
profile-remove-metadata
```

## Creating and removing profiles

Create a project profile:

```bash
when-it-fails-setter add-profile . \
  DITA \
  "DiTa" \
  "Disk diagnostics profile"
```

Remove it:

```bash
when-it-fails-setter remove-profile . DITA
```

Profile names are normalized stable keys. Display names and descriptions remain human-readable.

## Editing profile text

Change the display name:

```bash
when-it-fails-setter set-profile-display-name . \
  DITA \
  "DiTa storage tools"
```

Set the description:

```bash
when-it-fails-setter set-profile-description . \
  DITA \
  "Profile for disk diagnostics and storage tools."
```

Pass an empty quoted string to clear the description.

## Inspecting profiles

List profiles:

```bash
when-it-fails-setter list-profiles .
```

Show one profile:

```bash
when-it-fails-setter show-profile . DITA
```

Plain output:

```bash
when-it-fails-setter show-profile . DITA --plain
```

The detailed view displays:

```text
owners
code groups
categories
subcategories
include tags
include errors
exclude tags
exclude errors
default mappings
metadata
```

## Filtering errors through a profile

```bash
when-it-fails-setter errors . --profile DITA
```

The profile may be selected by stable name or display name. Matching is case-insensitive.

Ordinary command filters run after profile resolution.

Example:

```bash
when-it-fails-setter errors . \
  --profile DITA \
  --severity Warning \
  --search disk
```

This first resolves the profile and then narrows that result by severity and search text.

## Default profiles

The bundled profile template currently includes:

```text
WEB
API
CLI
DESKTOP
SERVICE
DEVELOPMENT
PRODUCTION
```

These are starting points for project-local customization. They are not immutable framework policy.

## Recommended custom profile names

Use names that are:

```text
short
stable
uppercase
machine-friendly
```

Examples:

```text
DITA
DISK_TOOL
ADMIN_PORTAL
PUBLIC_API
WORKER
SUPPORT
```

Avoid temporary or conversational names that are likely to change.

## Safe editing behavior

Every profile write operation follows the same conservative pattern:

```text
load catalog
→ normalize
→ validate current catalog
→ locate profile and referenced catalog value
→ apply one change
→ validate edited catalog
→ write temporary file
→ create timestamped backup
→ replace profiles.json
```

No-op or invalid operations do not create backups.

Examples include:

```text
value already present
value not present during removal
unknown profile
unknown referenced error, owner, category, or code group
empty required argument
edited catalog fails validation
```

## Recommended workflow

```text
validate
→ show-profile
→ run one profile write command
→ show-profile
→ errors --profile
→ validate
→ git diff
→ commit
```

Example:

```bash
when-it-fails-setter validate .
when-it-fails-setter show-profile . DITA --plain
when-it-fails-setter profile-add-tag . DITA STORAGE
when-it-fails-setter errors . --profile DITA --plain
when-it-fails-setter validate .
git diff
```

## Review checklist

When reviewing a profile, verify:

- the stable name is intentional,
- the display name and description are clear,
- include selectors represent intended alternatives,
- exclusion rules are deliberate vetoes,
- explicit error IDs are canonical,
- default mappings are safe for the target context,
- metadata is descriptive rather than executable policy,
- `show-profile` displays the expected state,
- `errors --profile` returns a plausible set,
- workspace validation passes,
- the Git diff contains only the intended change.
