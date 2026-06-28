# Workspace summary

The `summary` command displays a read-only overview of a validated WhenItFails workspace.

Alias:

```text
inspect
```

Both commands behave identically.

## Command syntax

```text
summary <path>
```

or:

```text
inspect <path>
```

Example:

```bash
when-it-fails-setter summary .
```

From the Toolbox repository:

```bash
dotnet run \
  --project Toolroom/WhenItFails/Setter \
  -- summary .
```

Using the alias:

```bash
dotnet run \
  --project Toolroom/WhenItFails/Setter \
  -- inspect .
```

## Accepted workspace paths

The supplied path may point to:

* a project root,
* the `Jsons/WhenItFails` directory directly.

Examples:

```bash
when-it-fails-setter summary .
```

```bash
when-it-fails-setter summary \
  ./Jsons/WhenItFails
```

## Validation before summary

The complete workspace is validated before the summary is loaded.

Sequence:

```text
resolve workspace path
→ validate all catalogs
→ stop on validation failure
→ load normalized catalogs
→ build summary model
→ render tables
```

If validation fails, Setter does not show a partial or potentially misleading summary.

Expected exit code:

```text
2
```

## What the summary shows

The current summary view includes:

```text
workspace display path
package directory path
catalog overview
owner table
code-group table
profile table
primary-category counts
```

It is intended as a fast structural overview rather than a full catalog dump.

## Workspace path

The header shows:

```text
Workspace
```

This is the human-readable display path derived from the input path.

Example:

```text
Workspace: Jsons/WhenItFails
```

When the package directory itself is supplied, the display value may be shortened to:

```text
WhenItFails
```

## Package directory

The summary also shows the resolved physical package path:

```text
Directory
```

Example:

```text
Directory: /home/user/projects/Toolbox/Jsons/WhenItFails
```

This is useful when several repositories or workspaces exist on the same machine.

## Catalog overview

The first table summarizes all five catalogs.

Columns:

```text
Catalog
Name
Items
Language
```

Current catalog rows:

```text
Errors
Categories
Code groups
Owners
Profiles
```

Example conceptually:

```text
Catalog       Name                                Items   Language
Errors        Afrowave default error catalog      37      en
Categories    Afrowave category catalog           10      en
Code groups   Afrowave code-group catalog         9       en
Owners        Afrowave owner catalog              5       en
Profiles      Afrowave profile catalog             3       en
```

The exact values depend on the workspace.

## Catalog item counts

The summary calculates:

```text
ErrorCount
CategoryCount
CodeGroupCount
OwnerCount
ProfileCount
```

These counts come from the normalized loaded documents.

They are useful for:

* checking that expected catalogs were loaded,
* spotting accidentally empty files,
* comparing workspaces,
* reviewing large catalog changes,
* confirming template initialization.

A changed count is not automatically an error, but it should be explainable.

## Catalog names

The `Name` column displays each catalog document’s `CatalogName`.

This helps distinguish:

* bundled defaults,
* project-specific catalogs,
* copied or customized workspaces,
* language-specific variants.

The summary does not infer meaning from the name.

It displays the stored normalized value.

## Language values

The catalog overview shows the `Language` value of each document.

Typical value:

```text
en
```

The summary does not require all future catalogs to use the same language merely because they are shown together.

Cross-catalog validation determines whether the workspace structure is acceptable.

## Owner table

The owner table contains:

```text
Owner
Display name
Range
Built-in
```

Example conceptually:

```text
Owner   Display name       Range             Built-in
AFW     Afrowave           100000 - 199999   yes
NET     Networking         600000 - 699999   no
```

The exact owners and ranges depend on the workspace.

## Owner ordering

Owners are ordered by:

```text
CodeFrom
```

in ascending order.

This makes numeric allocation easier to inspect.

The table is not ordered alphabetically by owner name.

## Owner range

The range is displayed as:

```text
CodeFrom - CodeTo
```

Example:

```text
100000 - 199999
```

This gives a quick view of the numeric space allocated to an owner.

Detailed overlap and validity checks belong to validation.

## Built-in owner flag

The `Built-in` column displays:

```text
yes
```

or:

```text
no
```

The rich view uses different terminal styles for the two values.

This flag is informational.

It does not make an owner immutable by itself.

## Code-group table

The code-group table contains:

```text
Code group
Prefix
Display name
Range
```

Example conceptually:

```text
Code group   Prefix   Display name   Range
GENERAL      GEN      General        100000 - 109999
NETWORK      NET      Network        600000 - 609999
```

## Code-group ordering

Code groups are ordered by:

```text
CodeFrom
```

in ascending order.

This makes numeric allocation easier to review.

## Code-group prefix

The prefix is the symbolic code-family identifier.

Example:

```text
NET
```

An error may then use an ID such as:

```text
AFW_NET_0001
```

Validation checks whether IDs, prefixes, groups, owners, and numeric ranges agree.

The summary only presents the configured values.

## Code-group range

The range is displayed as:

```text
CodeFrom - CodeTo
```

Use this table to quickly answer questions such as:

* which group owns a numeric region,
* whether two groups appear suspiciously close,
* where a new error code should probably belong.

Always rely on full validation for correctness.

## Profile table

The profile table contains:

```text
Profile
Display name
Owners
Code groups
Categories
```

Example conceptually:

```text
Profile   Display name       Owners   Code groups   Categories
WEB       Web application    AFW      NETWORK       NETWORK
```

## Profile ordering

Profiles are ordered alphabetically by:

```text
Name
```

This differs from owners and code groups, which are ordered numerically.

## Included profile dimensions

The summary currently displays:

```text
IncludeOwners
IncludeCodeGroups
IncludeCategories
```

Collections are joined into readable text.

Example:

```text
AFW, DISK
```

## Important profile limitation

The summary table does not currently display every possible profile field.

It focuses on:

```text
included owners
included code groups
included categories
```

It may not show:

* included tags,
* excluded error IDs,
* excluded tags,
* future profile fields.

Therefore the summary is an overview, not a complete profile serialization.

Inspect `profiles.json` or use runtime profile documentation when advanced profile behavior matters.

## Primary-category table

The final table contains:

```text
Primary category
Errors
```

It groups errors by:

```text
PrimaryCategory
```

and counts how many errors belong to each group.

Example:

```text
Primary category   Errors
GENERAL            12
NETWORK             8
STORAGE             6
SECURITY             4
```

## Category ordering

Primary-category groups are ordered by:

1. descending error count,
2. category name alphabetically.

This places the largest categories first.

## Empty primary category

When an error has an empty or whitespace-only primary category, the summary displays:

```text
(empty)
```

A valid workspace will normally prevent unexpected empty required values, but the view still handles the case defensively.

## Summary versus validation

Use:

```bash
when-it-fails-setter validate .
```

to answer:

```text
Is this workspace valid?
```

Use:

```bash
when-it-fails-setter summary .
```

to answer:

```text
What is in this valid workspace?
```

The summary is not a replacement for validation.

It depends on validation succeeding first.

## Summary versus errors

Use `summary` for workspace structure:

```text
catalog counts
owners
code groups
profiles
category distribution
```

Use `errors` for individual definitions:

```text
IDs
codes
names
titles
severity
filters
search
```

Typical workflow:

```text
summary
→ understand workspace structure
→ errors
→ locate definitions
→ details
→ inspect one definition
```

## Summary versus details

`summary` shows broad structure.

`details` shows one complete error definition.

Example:

```bash
when-it-fails-setter summary .
```

Then:

```bash
when-it-fails-setter details . \
  AFW_NET_0001
```

## Read-only behavior

The summary command does not intentionally modify:

* catalog files,
* backups,
* temporary files,
* runtime context,
* project configuration.

It performs:

```text
validation
→ loading
→ normalization in memory
→ aggregation
→ rendering
```

No save is performed.

## Rich output only

The current summary command does not support:

```text
--plain
```

It always uses the rich Spectre.Console view.

This means the output is primarily intended for interactive terminal use.

Terminal width may affect table layout.

## Unknown extra arguments

The current command uses only:

```text
command
path
```

Additional arguments are not processed by a strict parser.

Example:

```bash
when-it-fails-setter summary . \
  --plain
```

does not activate a plain mode because none exists.

Avoid passing unsupported arguments.

A future stricter parser may reject them explicitly.

## Missing path

Example:

```bash
when-it-fails-setter summary
```

Expected issue code:

```text
MissingSummaryPath
```

Expected syntax:

```text
summary <path>
```

Expected exit code:

```text
1
```

The same applies to:

```bash
when-it-fails-setter inspect
```

## Exit codes

```text
0
→ summary displayed successfully
```

```text
1
→ required workspace path missing
```

```text
2
→ workspace validation failed
```

Unexpected top-level failures may return:

```text
3
```

## Typical first inspection

After initialization:

```bash
when-it-fails-setter init .
when-it-fails-setter validate .
when-it-fails-setter summary .
```

This confirms:

* all expected catalogs exist,
* the workspace is valid,
* catalog counts are plausible,
* owner ranges are visible,
* code groups are visible,
* profiles are visible,
* category distribution is plausible.

## Comparing two workspaces

Run summary against each project:

```bash
when-it-fails-setter summary \
  ./AppOne
```

```bash
when-it-fails-setter summary \
  ./AppTwo
```

Compare:

* item counts,
* owner ranges,
* code groups,
* profile lists,
* category distribution.

The command does not currently produce a machine-readable diff.

## Detecting accidental catalog shrinkage

Suppose the previous summary showed:

```text
Errors: 37
```

and after an edit it shows:

```text
Errors: 12
```

Validation may still succeed if the remaining document is structurally valid.

The count change should still be investigated.

Possible causes:

* accidental deletion,
* wrong workspace path,
* incomplete import,
* copied template,
* merge conflict resolution,
* editing the wrong catalog.

## Detecting wrong workspace

The summary header shows both:

```text
Workspace
Directory
```

Use them to confirm the intended project was loaded.

When the counts or owners look unfamiliar, check:

```bash
pwd
realpath Jsons/WhenItFails
```

Then rerun summary with an absolute path.

## Detecting unexpected profile changes

The profile table provides a quick view of:

* profile names,
* display names,
* included owners,
* included code groups,
* included categories.

A missing or newly added profile is immediately visible.

For advanced exclusions or tag behavior, inspect the JSON directly.

## Detecting allocation problems visually

Validation catches formal range problems.

The summary table additionally helps a human notice patterns such as:

* unexpectedly large gaps,
* surprising owner ordering,
* unusual code-group ranges,
* duplicated-looking display names,
* a group placed under the wrong numeric family.

Human review and validation complement each other.

## Terminal width

The tables are configured as wide terminal tables.

For the best result:

* use a reasonably wide terminal,
* maximize the terminal window,
* reduce font size if needed,
* avoid narrow split panes.

Very long catalog names or profile collections may wrap.

## Redirecting summary output

The summary uses rich output.

Redirecting it may preserve ANSI sequences depending on terminal and Spectre.Console behavior.

Example:

```bash
when-it-fails-setter summary . \
  > summary.txt
```

For permanent machine-readable reporting, a future JSON or plain mode would be preferable.

## Suggested smoke tests

Summary command:

```bash
dotnet run \
  --project Toolroom/WhenItFails/Setter \
  -- summary .
```

Alias:

```bash
dotnet run \
  --project Toolroom/WhenItFails/Setter \
  -- inspect .
```

Direct package directory:

```bash
dotnet run \
  --project Toolroom/WhenItFails/Setter \
  -- summary \
  ./Jsons/WhenItFails
```

Missing path:

```bash
dotnet run \
  --project Toolroom/WhenItFails/Setter \
  -- summary

echo "Exit code: $?"
```

Expected:

```text
Exit code: 1
```

## Practical review checklist

After running summary, confirm:

* resolved directory is correct,
* error count is plausible,
* category count is plausible,
* code-group count is plausible,
* owner count is plausible,
* profile count is plausible,
* catalog languages are expected,
* owner ranges appear intentional,
* code-group prefixes and ranges appear intentional,
* profile include dimensions appear intentional,
* primary-category distribution appears plausible.

## Future improvements

Possible future improvements include:

* `--plain` output,
* JSON output,
* category catalog table,
* severity distribution,
* owner usage counts,
* code-group usage counts,
* unused owner detection,
* unused category detection,
* unused profile detection,
* advanced profile-field display,
* comparison between two workspaces,
* summary snapshots for CI,
* sorting options,
* compact mode.

These are future possibilities, not current guarantees.

## Related documentation

* [Getting Started](../Getting-Started/en.md)
* [Workspace Paths and Initialization](../Workspace%20Paths%20and%20Initialization/en.md)
* [Validation](../Validation/en.md)
* [Browsing and Filtering Errors](../Browsing%20and%20Filtering%20Errors/en.md)
* [Inspecting Error Details](../Inspecting%20Error%20Details/en.md)
* [Troubleshooting](../Troubleshooting/en.md)
* [Commands](../Commands/en.md)

## Central principle

> Validation tells you whether the workspace is correct; summary tells you what that valid workspace contains.
