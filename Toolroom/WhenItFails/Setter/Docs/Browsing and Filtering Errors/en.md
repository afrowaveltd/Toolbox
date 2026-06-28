# Browsing and filtering errors

The `errors` command lists error definitions from a validated WhenItFails workspace.

It supports:

* rich terminal output,
* plain script-friendly output,
* exact field filters,
* profile-based filtering,
* case-insensitive full-text search,
* combinations of several filters.

## Command syntax

```text
errors <path> [options]
```

Example:

```bash
when-it-fails-setter errors .
```

From the Toolbox repository:

```bash
dotnet run \
  --project Toolroom/WhenItFails/Setter \
  -- errors .
```

## Accepted workspace paths

The path may point to:

* the project root,
* the `Jsons/WhenItFails` directory directly.

Examples:

```bash
when-it-fails-setter errors .
```

```bash
when-it-fails-setter errors \
  ./Jsons/WhenItFails
```

## Validation before listing

The `errors` command validates the complete workspace before loading and displaying definitions.

Sequence:

```text
resolve workspace
→ validate all catalogs
→ stop on validation failure
→ load normalized summary
→ apply profile filter
→ apply field filters
→ apply search filter
→ render result
```

If workspace validation fails, the normal error table is not shown.

The command returns exit code:

```text
2
```

for validation failure.

## Available options

```text
--owner <value>
--group <value>
--code-group <value>
--category <value>
--severity <value>
--profile <value>
--search <text>
--plain
```

`--group` and `--code-group` are aliases.

## Listing all errors

```bash
when-it-fails-setter errors .
```

This displays every error definition in the validated catalog.

The output is ordered according to the view implementation.

Plain output currently presents rows ordered by numeric code and stable ID.

## Filtering by owner

```bash
when-it-fails-setter errors . \
  --owner AFW
```

Owner matching is:

* exact,
* case-insensitive.

These match:

```text
AFW
afw
Afw
```

This does not perform partial matching.

Example:

```text
--owner AF
```

does not match:

```text
AFW
```

Use `--search` for partial text matching.

## Filtering by code group

Using the short option:

```bash
when-it-fails-setter errors . \
  --group NETWORK
```

Using the explicit alias:

```bash
when-it-fails-setter errors . \
  --code-group NETWORK
```

Matching is:

* exact,
* case-insensitive,
* performed against the error definition’s `CodeGroup`.

## Filtering by category

```bash
when-it-fails-setter errors . \
  --category NETWORK
```

The category filter matches:

```text
PrimaryCategory
```

It does not independently search every entry in:

```text
Categories
Subcategories
```

For broader category-related searching, use:

```bash
when-it-fails-setter errors . \
  --search NETWORK
```

because full-text search checks primary category, categories, subcategories, and tags.

## Filtering by severity

```bash
when-it-fails-setter errors . \
  --severity Error
```

Matching is:

* exact,
* case-insensitive,
* performed against `DefaultSeverity`.

Supported catalog severity values are:

```text
Trace
Debug
Information
Warning
Error
Critical
```

Example:

```bash
when-it-fails-setter errors . \
  --severity warning
```

matches errors stored as:

```text
Warning
```

## Filtering by profile

```bash
when-it-fails-setter errors . \
  --profile WEB
```

A profile may be selected by:

* profile name,
* profile display name.

Matching is case-insensitive.

Example:

```text
Name:
WEB
```

```text
DisplayName:
Web application
```

Both may be used:

```bash
when-it-fails-setter errors . \
  --profile WEB
```

```bash
when-it-fails-setter errors . \
  --profile "Web application"
```

Quote display names containing spaces.

## Unknown profile

An unknown profile is treated as invalid command input.

Example:

```bash
when-it-fails-setter errors . \
  --profile DOES_NOT_EXIST
```

Expected issue code:

```text
UnknownProfileFilter
```

Expected exit code:

```text
1
```

This differs from an ordinary field filter that matches no errors.

## Empty ordinary filter result

Example:

```bash
when-it-fails-setter errors . \
  --owner DOES_NOT_EXIST
```

may produce zero rows while still returning:

```text
0
```

That means:

```text
the filter was valid
→ no definitions matched
```

An unknown profile is different because the selected profile definition itself does not exist.

## Current Setter profile semantics

The current `errors` command evaluates these profile fields:

```text
IncludeOwners
IncludeCodeGroups
IncludeCategories
```

For each profile field:

```text
empty include list
→ that dimension accepts all errors
```

```text
non-empty include list
→ error value must match one included value
```

The three dimensions are then combined with logical AND.

Conceptually:

```text
owner matches
AND
code group matches
AND
primary category matches
```

## Profile example

Suppose a profile contains:

```json
{
  "includeOwners": [
    "AFW"
  ],
  "includeCodeGroups": [
    "NETWORK"
  ],
  "includeCategories": [
    "NETWORK"
  ]
}
```

An error is included only when all three are true:

```text
Owner = AFW
CodeGroup = NETWORK
PrimaryCategory = NETWORK
```

An error owned by `AFW` but belonging to another group does not match.

## Multiple values inside one profile dimension

Values inside one include collection behave as logical OR.

Example:

```json
{
  "includeOwners": [
    "AFW",
    "DISK"
  ]
}
```

Owner matching is:

```text
AFW
OR
DISK
```

Across dimensions, matching remains AND:

```text
(owner is AFW OR DISK)
AND
(group matches included groups)
AND
(category matches included categories)
```

## Important runtime difference

The Setter `errors --profile` implementation is currently narrower than the full package runtime profile resolver.

The command currently evaluates only:

```text
IncludeOwners
IncludeCodeGroups
IncludeCategories
```

It does not currently apply the complete runtime profile behavior for:

* included tags,
* excluded error IDs,
* excluded tags,
* other runtime profile selection rules.

Therefore:

```text
Setter errors --profile
```

is useful for browsing, but it should not automatically be treated as a byte-for-byte equivalent of:

```text
ResolveProfile()
```

when a profile uses advanced include or exclusion fields.

This distinction should remain explicit until Setter delegates profile evaluation to the same shared runtime resolver.

## Full-text search

Syntax:

```bash
when-it-fails-setter errors . \
  --search timeout
```

Search is:

* case-insensitive,
* substring-based,
* applied across many error fields.

It is not an exact-match filter.

## Fields searched

The current search checks:

```text
Id
Name
Title
Message
DeveloperHint
DocumentationKey
Code
Owner
CodeGroup
PrimaryCategory
Categories
Subcategories
Tags
```

## Search by ID

```bash
when-it-fails-setter errors . \
  --search AFW_NET
```

This may find several errors whose IDs contain the fragment.

## Search by numeric code

```bash
when-it-fails-setter errors . \
  --search 600001
```

The numeric code is converted to text for searching.

Partial numeric fragments may also match.

Example:

```bash
when-it-fails-setter errors . \
  --search 600
```

may return several errors in the same numeric area.

## Search by title or message

```bash
when-it-fails-setter errors . \
  --search unavailable
```

This may match:

* title,
* message,
* developer hint,
* documentation key,
* tags,
* categories.

The command does not report which field produced the match.

Use `details` to inspect a returned definition.

## Search by tag

```bash
when-it-fails-setter errors . \
  --search USER_VISIBLE
```

Because tags participate in full-text search, this can be used as a practical tag filter.

However, it remains substring search rather than exact tag matching.

For example:

```text
VISIBLE
```

may also match:

```text
USER_VISIBLE
```

## Search by subcategory

```bash
when-it-fails-setter errors . \
  --search FALLBACK
```

This can match a subcategory even though no dedicated `--subcategory` option currently exists.

## Search is not regular-expression matching

The value is treated as ordinary text.

Example:

```bash
when-it-fails-setter errors . \
  --search "NET.*FAIL"
```

searches for the literal substring:

```text
NET.*FAIL
```

It does not execute a regular expression.

## Combining filters

Filters may be combined.

Example:

```bash
when-it-fails-setter errors . \
  --owner AFW \
  --group NETWORK \
  --severity Error
```

All active command-line filters are combined with logical AND.

Conceptually:

```text
owner matches
AND
group matches
AND
category matches
AND
severity matches
AND
search text matches
```

## Combining profile and ordinary filters

A profile may be combined with other filters.

Example:

```bash
when-it-fails-setter errors . \
  --profile WEB \
  --severity Error \
  --search timeout
```

Evaluation is conceptually:

```text
profile matches
AND
severity is Error
AND
search text contains timeout
```

The additional command-line filters narrow the profile result.

They do not expand it.

## Example narrowing workflow

Start broadly:

```bash
when-it-fails-setter errors . \
  --profile WEB
```

Then narrow:

```bash
when-it-fails-setter errors . \
  --profile WEB \
  --severity Error
```

Then search:

```bash
when-it-fails-setter errors . \
  --profile WEB \
  --severity Error \
  --search connection
```

## Option names are case-insensitive

Option switches are compared case-insensitively.

These are accepted equivalently:

```text
--plain
--PLAIN
--Plain
```

Canonical lowercase spelling is recommended for:

* readability,
* documentation consistency,
* shell scripts,
* future tooling.

## Option values

Option values are read from the argument immediately following the option.

Example:

```bash
when-it-fails-setter errors . \
  --owner AFW
```

The value of `--owner` is:

```text
AFW
```

## Missing option value

Example:

```bash
when-it-fails-setter errors . \
  --owner
```

The current parser returns no owner value.

This effectively means the owner filter is not applied.

Likewise:

```bash
when-it-fails-setter errors . \
  --owner \
  --severity Error
```

does not use `--severity` as the owner value because values beginning with `--` are rejected as option values.

At present, a missing ordinary filter value is not reported as a dedicated command error.

Scripts should therefore provide every expected value explicitly.

## Repeated option

When the same option appears more than once, the parser reads the first occurrence.

Example:

```bash
when-it-fails-setter errors . \
  --severity Warning \
  --severity Error
```

The effective severity is:

```text
Warning
```

Avoid repeated options because their intent is ambiguous.

## Unknown option

The current parser reads only known options and otherwise ignores unrecognized arguments.

Example:

```bash
when-it-fails-setter errors . \
  --severty Error
```

contains a typo.

The severity filter is not applied.

This may produce a successful but unexpectedly broad result.

Always inspect the displayed filter summary or result count.

A future stricter command parser may reject unknown options.

## Plain output

Use:

```bash
when-it-fails-setter errors . \
  --plain
```

Plain output is intended for:

* shell scripts,
* redirection,
* CI logs,
* text processing,
* external tools.

It is simpler than the rich Spectre.Console table.

## Plain output structure

The current plain output contains metadata before the tabular section.

Conceptually:

```text
WhenItFails Error Definitions
Workspace: ...
Errors: <shown> shown from <total>
Filters: ...

Code	Id	Name	Owner	Group	Category	Severity	Title
...
```

It is not a headerless TSV stream.

See the dedicated plain-output guide for extraction examples.

## Rich output

Without `--plain`, Setter uses a Spectre.Console table.

Example:

```bash
when-it-fails-setter errors . \
  --category NETWORK
```

The rich view is intended for interactive use.

Terminal width may affect layout.

## Read-only behavior

The `errors` command does not intentionally modify:

* catalog files,
* backups,
* temporary files,
* runtime state.

It performs:

```text
validation
→ loading
→ normalization in memory
→ filtering
→ rendering
```

No save is performed.

## Exit codes

```text
0
→ listing completed successfully
```

This includes a valid filter result containing zero rows.

```text
1
→ required path missing or selected profile unknown
```

```text
2
→ workspace validation failed
```

Unexpected top-level failures may return:

```text
3
```

## Missing path

Example:

```bash
when-it-fails-setter errors
```

Expected issue code:

```text
MissingErrorsPath
```

Expected syntax:

```text
errors <path>
```

Exit code:

```text
1
```

## Recommended interactive workflow

```text
validate workspace
→ list all errors
→ apply one broad filter
→ narrow with more filters
→ inspect one definition
```

Example:

```bash
when-it-fails-setter validate .

when-it-fails-setter errors .

when-it-fails-setter errors . \
  --group NETWORK

when-it-fails-setter errors . \
  --group NETWORK \
  --severity Error

when-it-fails-setter details . \
  AFW_NET_0001
```

## Recommended scripting workflow

```bash
set -euo pipefail

dotnet run \
  --project Toolroom/WhenItFails/Setter \
  -- errors . \
  --plain \
  --severity Error
```

For tabular extraction:

```bash
dotnet run \
  --project Toolroom/WhenItFails/Setter \
  -- errors . \
  --plain \
  --severity Error |
awk '
  /^Code\tId\tName\tOwner\tGroup\tCategory\tSeverity\tTitle$/ {
    output = 1
  }

  output {
    print
  }
'
```

Use `set -o pipefail` when a pipeline must preserve Setter failures.

## Counting filtered rows

Example:

```bash
dotnet run \
  --project Toolroom/WhenItFails/Setter \
  -- errors . \
  --plain \
  --severity Error |
awk '
  /^Code\tId\tName\tOwner\tGroup\tCategory\tSeverity\tTitle$/ {
    output = 1
    next
  }

  output && NF > 0 {
    count++
  }

  END {
    print count + 0
  }
'
```

This counts data rows after the header.

## Extracting IDs

```bash
dotnet run \
  --project Toolroom/WhenItFails/Setter \
  -- errors . \
  --plain \
  --group NETWORK |
awk -F '\t' '
  /^Code\tId\tName\tOwner\tGroup\tCategory\tSeverity\tTitle$/ {
    output = 1
    next
  }

  output && NF >= 2 {
    print $2
  }
'
```

## Filter troubleshooting

When the result is unexpectedly empty:

1. Remove all filters.
2. Add one filter at a time.
3. Confirm exact owner, group, category, and severity values.
4. Use `--search` to locate likely values.
5. Inspect the selected error with `details`.
6. Confirm the correct workspace path.
7. Validate the workspace.

Example:

```bash
when-it-fails-setter errors . \
  --search network
```

Then:

```bash
when-it-fails-setter details . \
  AFW_NET_0001
```

## Broad result troubleshooting

When the result is unexpectedly broad:

* check option spelling,
* check missing option values,
* check repeated options,
* confirm the profile exists,
* inspect the displayed filters,
* confirm quoting of values with spaces.

Example typo:

```text
--severty
```

Correct:

```text
--severity
```

## Quoting search text

Quote search values containing spaces:

```bash
when-it-fails-setter errors . \
  --search "remote service"
```

Without quotes, only the first word is read as the option value and later words become unrelated arguments.

## Quoting profile display names

```bash
when-it-fails-setter errors . \
  --profile "Web application"
```

Without quotes, the parser reads only:

```text
Web
```

which may produce:

```text
UnknownProfileFilter
```

## Exact filters versus search

Use exact filters when you know the structured value:

```bash
--owner AFW
--group NETWORK
--category NETWORK
--severity Error
```

Use search when:

* you know only part of the text,
* you want to search several fields,
* you want to find tags or subcategories,
* you do not know the exact structured value.

## Filter behavior summary

```text
--owner
→ exact Owner match
```

```text
--group / --code-group
→ exact CodeGroup match
```

```text
--category
→ exact PrimaryCategory match
```

```text
--severity
→ exact DefaultSeverity match
```

```text
--profile
→ current Setter profile include matching
```

```text
--search
→ case-insensitive substring across many fields
```

```text
multiple filters
→ logical AND
```

## Practical examples

### All network errors

```bash
when-it-fails-setter errors . \
  --group NETWORK
```

### Critical errors

```bash
when-it-fails-setter errors . \
  --severity Critical
```

### User-visible errors

```bash
when-it-fails-setter errors . \
  --search USER_VISIBLE
```

### Errors mentioning timeout

```bash
when-it-fails-setter errors . \
  --search timeout
```

### Network errors owned by AFW

```bash
when-it-fails-setter errors . \
  --owner AFW \
  --group NETWORK
```

### Error-severity network failures containing connection text

```bash
when-it-fails-setter errors . \
  --group NETWORK \
  --severity Error \
  --search connection
```

### Profile selection in plain output

```bash
when-it-fails-setter errors . \
  --profile WEB \
  --plain
```

## Future improvements

Possible future improvements include:

* strict rejection of unknown options,
* dedicated errors for missing option values,
* exact tag filters,
* subcategory filters,
* reusable shared runtime profile evaluation,
* machine-readable JSON output,
* explicit sorting options,
* pagination,
* output column selection,
* explanation of why a profile matched an error.

These are future possibilities, not current command guarantees.

## Checklist

Before relying on a filtered result, confirm:

* workspace validation succeeded,
* the correct workspace path was supplied,
* option names are spelled correctly,
* every option has a value,
* values with spaces are quoted,
* exact filters use canonical structured values,
* zero rows are distinguished from command failure,
* profile behavior is understood,
* advanced runtime profile exclusions are not assumed,
* scripts capture the exit code,
* plain-output metadata is handled correctly.

## Related documentation

* [Commands](../Commands/en.md)
* [Plain Output](../Plain%20Output/en.md)
* [Validation](../Validation/en.md)
* [Troubleshooting](../Troubleshooting/en.md)
* [Testing and CI](../Testing%20and%20CI/en.md)
* [Overview](../Overview/en.md)

## Central principle

> Use structured filters for exact catalog values, full-text search for discovery, and profiles for reusable broad selections.
