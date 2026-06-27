# Plain output

WhenItFails Setter normally uses rich Spectre.Console output designed for interactive terminal use.

Selected commands also support:

```text
--plain
```

Plain output removes Spectre.Console tables, colors, borders, and ANSI-oriented presentation.

It is intended for:

* shell scripts,
* redirected output,
* CI logs,
* text processing,
* comparisons,
* environments without rich terminal support.

## Supported commands

Plain output is currently supported by:

```text
errors
details
detail
```

Examples:

```bash
when-it-fails-setter errors . --plain
```

```bash
when-it-fails-setter details . AFW_NET_0001 --plain
```

The singular alias also supports it:

```bash
when-it-fails-setter detail . AFW_NET_0001 --plain
```

Other commands currently retain their normal console presentation.

## Rich output versus plain output

Rich output uses:

* Spectre.Console tables,
* borders,
* terminal-aware layout,
* colors,
* escaped markup,
* formatted headings.

Plain output uses:

* ordinary text,
* no color markup,
* no table borders,
* predictable line-oriented content,
* tab-separated rows for error lists,
* key-value lines for one error detail.

## Plain error list

Run:

```bash
when-it-fails-setter errors . --plain
```

Typical structure:

```text
WhenItFails Error Definitions
Workspace: Jsons/WhenItFails
Errors: 37 shown from 37

Code	Id	Name	Owner	Group	Category	Severity	Title
600001	AFW_NET_0001	NETWORKUNAVAILABLE	AFW	NETWORK	NETWORK	Error	Network unavailable
```

The output contains:

```text
human-readable header
workspace path
result count
optional filter summary
blank line
TSV column header
TSV data rows
```

## Important TSV distinction

The error rows are tab-separated, but the complete command output is not a headerless TSV file.

Before the TSV section, Setter writes metadata such as:

```text
WhenItFails Error Definitions
Workspace: ...
Errors: ...
Filters: ...
```

Therefore:

```bash
when-it-fails-setter errors . --plain > errors.tsv
```

creates a human-readable plain report containing a TSV section.

It is not currently a strict machine-only TSV export.

Scripts should account for the metadata lines.

## Error-list columns

The TSV header is:

```text
Code	Id	Name	Owner	Group	Category	Severity	Title
```

Column order is stable in the current implementation:

1. `Code`
2. `Id`
3. `Name`
4. `Owner`
5. `Group`
6. `Category`
7. `Severity`
8. `Title`

Example row:

```text
600001	AFW_NET_0001	NETWORKUNAVAILABLE	AFW	NETWORK	NETWORK	Error	Network unavailable
```

## Row ordering

Error rows are ordered by:

```text
numeric code
→ stable ID
```

Conceptually:

```csharp
errors
    .OrderBy(error => error.Code)
    .ThenBy(error => error.Id)
```

This provides predictable output for review and comparison.

## Filtered output

Plain output supports the same filters as rich output.

Example:

```bash
when-it-fails-setter errors . \
  --plain \
  --category NETWORK
```

Typical structure:

```text
WhenItFails Error Definitions
Workspace: Jsons/WhenItFails
Errors: 4 shown from 37
Filters: category=NETWORK

Code	Id	Name	Owner	Group	Category	Severity	Title
600001	AFW_NET_0001	NETWORKUNAVAILABLE	AFW	NETWORK	NETWORK	Error	Network unavailable
600002	AFW_NET_0002	HTTPREQUESTFAILED	AFW	NETWORK	NETWORK	Error	HTTP request failed
```

## Filter summary

Active filters are displayed in a comma-separated form.

Possible labels:

```text
owner
group
category
severity
profile
search
```

Example:

```text
Filters: owner=AFW, category=NETWORK, severity=Warning
```

When no filter is active, the `Filters:` line is omitted.

## Combined filters

Example:

```bash
when-it-fails-setter errors . \
  --plain \
  --owner AFW \
  --group NETWORK \
  --severity Error
```

The output includes:

```text
Filters: owner=AFW, group=NETWORK, severity=Error
```

Filters are applied before output formatting.

Rich and plain output therefore display the same selected error set.

## Empty result

A valid filter may return no errors.

Example:

```bash
when-it-fails-setter errors . \
  --plain \
  --category DOES_NOT_EXIST
```

The command may still succeed and show:

```text
Errors: 0 shown from 37
```

followed by the TSV header with no data rows.

An empty result is different from:

* invalid workspace,
* unknown profile,
* missing path,
* command failure.

## Unknown profile

A nonexistent profile is treated as an input error.

Example:

```bash
when-it-fails-setter errors . \
  --plain \
  --profile DOES_NOT_EXIST
```

The command returns a non-zero exit code and displays a validation-style error.

Plain list rendering does not occur because filtering cannot be completed.

## Extracting the TSV section

Because metadata appears before the table, a simple script may locate the header row.

Example using `awk`:

```bash
when-it-fails-setter errors . --plain |
awk '
  /^Code\tId\tName\tOwner\tGroup\tCategory\tSeverity\tTitle$/ {
    output = 1
  }

  output {
    print
  }
'
```

This prints only:

```text
Code	Id	Name	Owner	Group	Category	Severity	Title
...
```

## Skipping the TSV header

To obtain only data rows:

```bash
when-it-fails-setter errors . --plain |
awk '
  /^Code\tId\tName\tOwner\tGroup\tCategory\tSeverity\tTitle$/ {
    output = 1
    next
  }

  output {
    print
  }
'
```

## Selecting one column

Example: print stable IDs.

```bash
when-it-fails-setter errors . --plain |
awk -F '\t' '
  /^Code\tId\tName\tOwner\tGroup\tCategory\tSeverity\tTitle$/ {
    output = 1
    next
  }

  output {
    print $2
  }
'
```

Example result:

```text
AFW_NET_0001
AFW_NET_0002
AFW_DB_0001
```

## Filtering with shell tools

Example: show only critical rows from the plain table.

```bash
when-it-fails-setter errors . --plain |
awk -F '\t' '
  /^Code\tId\tName\tOwner\tGroup\tCategory\tSeverity\tTitle$/ {
    output = 1
    next
  }

  output && $7 == "Critical" {
    print
  }
'
```

Setter’s own:

```text
--severity Critical
```

is usually preferable because filtering happens before rendering.

## Plain error detail

Run:

```bash
when-it-fails-setter details . AFW_NET_0001 --plain
```

Typical output:

```text
WhenItFails Error Detail
Workspace: Jsons/WhenItFails

Code: 600001
Id: AFW_NET_0001
Name: NETWORKUNAVAILABLE
Title: Network unavailable
Message: The network is unavailable.
Severity: Error
Owner: AFW
Code prefix: NET
Code group: NETWORK
Primary category: NETWORK
Categories: NETWORK
Subcategories: CONNECTIVITY
Tags: NETWORK, USER_VISIBLE
Developer hint: Check connectivity, DNS, firewall, proxy, VPN, and host availability.
Documentation key: when-it-fails/errors/network/network-unavailable
```

## Detail field order

Plain detail currently prints fields in this order:

1. `Code`
2. `Id`
3. `Name`
4. `Title`
5. `Message`
6. `Severity`
7. `Owner`
8. `Code prefix`
9. `Code group`
10. `Primary category`
11. `Categories`
12. `Subcategories`
13. `Tags`
14. `Developer hint`
15. `Documentation key`

This order matches the conceptual identity and presentation structure of an error definition.

## Collection formatting

Collections are rendered as comma-separated values.

Example:

```text
Tags: NETWORK, USER_VISIBLE
```

Example:

```text
Subcategories: CONNECTIVITY, DNS
```

An empty collection is rendered as an empty value after the label.

## Nullable values

Optional values such as:

```text
Developer hint
Documentation key
```

are rendered as empty text when no value exists.

Example:

```text
Developer hint:
Documentation key:
```

Plain output does not print:

```text
null
```

for these fields.

## Parsing key-value detail output

A simple shell script may split on the first colon.

Example:

```bash
when-it-fails-setter details . AFW_NET_0001 --plain |
awk -F ': ' '
  $1 == "Id" {
    print $2
  }
'
```

However, values themselves may contain colons.

For example:

```text
Message: Remote service returned: access denied.
```

Therefore generic parsing should split only at the first field separator and should recognize known field names.

## Plain output is not JSON

Plain output is intended for readable text processing.

It is not:

* JSON,
* CSV,
* XML,
* a formal serialization contract,
* a replacement for reading the catalog files directly.

Applications requiring structured catalog data should use:

* the WhenItFails runtime API,
* the JSON catalogs,
* a future dedicated export command.

## Tabs inside values

The error list joins columns with tab characters.

Current output does not perform TSV quoting or escaping.

If a field itself contains a tab or newline, that value may disrupt row-based parsing.

Catalog authors should avoid tabs and embedded line breaks in fields commonly shown in list output, especially:

```text
Title
```

Human-facing messages and hints are better inspected through `details --plain` or the JSON catalog.

## Newlines inside detail values

Plain detail output writes each field using one `Console.WriteLine` call.

If a stored value contains embedded newline characters, it may span several physical output lines.

This is readable for humans but complicates automated parsing.

Plain output should therefore be treated as lightweight tooling output rather than a hardened interchange format.

## ANSI behavior

Plain output methods use ordinary:

```csharp
Console.WriteLine(...)
```

instead of Spectre.Console tables.

This means the normal list and detail output does not add:

* ANSI color sequences,
* Spectre markup,
* border characters,
* terminal width calculations.

Errors encountered before plain rendering may still be displayed through the shared validation console renderer.

Therefore a failed command is not guaranteed to use the same plain data layout as a successful command.

## Standard output and standard error

The current commands primarily write command output through the console presentation layer.

Scripts should primarily rely on:

```text
process exit code
+
expected output structure
```

rather than assuming every diagnostic is separated into a dedicated standard-error stream.

## Exit codes

Plain mode does not change command exit codes.

For `errors`:

```text
0
→ list displayed, including an empty valid result

1
→ missing path or unknown profile

2
→ workspace loading or validation failed
```

For `details`:

```text
0
→ error detail displayed

1
→ arguments missing or error not found

2
→ workspace loading or validation failed
```

Always check the exit code before parsing output.

## Safe scripting example

```bash
output_file="$(mktemp)"

if dotnet run \
  --project Toolroom/WhenItFails/Setter \
  -- errors . --plain --category NETWORK \
  > "$output_file"
then
  awk '
    /^Code\tId\tName\tOwner\tGroup\tCategory\tSeverity\tTitle$/ {
      output = 1
    }

    output {
      print
    }
  ' "$output_file"
else
  echo "Setter command failed." >&2
fi

rm -f "$output_file"
```

This avoids parsing a validation failure as though it were a successful table.

## CI use

Example validation and export sequence:

```bash
dotnet run \
  --project Toolroom/WhenItFails/Setter \
  -- validate .

dotnet run \
  --project Toolroom/WhenItFails/Setter \
  -- errors . --plain \
  > when-it-fails-errors.txt
```

The first command provides a clear validation gate.

The second produces a readable CI artifact.

## Comparing two workspaces

Generate reports:

```bash
when-it-fails-setter errors ./ProjectA --plain \
  > project-a-errors.txt

when-it-fails-setter errors ./ProjectB --plain \
  > project-b-errors.txt
```

Compare:

```bash
diff -u \
  project-a-errors.txt \
  project-b-errors.txt
```

Remember that workspace paths appear in the metadata and may differ even when the error rows are identical.

To compare only table content, extract the TSV sections first.

## Comparing only rows

```bash
extract_rows()
{
  awk '
    /^Code\tId\tName\tOwner\tGroup\tCategory\tSeverity\tTitle$/ {
      output = 1
    }

    output {
      print
    }
  '
}

when-it-fails-setter errors ./ProjectA --plain |
extract_rows > project-a-errors.tsv

when-it-fails-setter errors ./ProjectB --plain |
extract_rows > project-b-errors.tsv

diff -u \
  project-a-errors.tsv \
  project-b-errors.tsv
```

## Redirecting details

```bash
when-it-fails-setter details . \
  AFW_NET_0001 \
  --plain \
  > AFW_NET_0001.txt
```

This creates a readable single-error report suitable for:

* support tickets,
* review,
* debugging,
* documentation drafts,
* test artifacts.

Review sensitive fields before sharing the output.

## Sensitive information

Plain detail output may include:

```text
Message
Developer hint
Documentation key
Tags
```

Future catalog fields may contain additional diagnostic content.

Before publishing or attaching plain output externally, verify that it does not expose:

* internal hostnames,
* private paths,
* customer identifiers,
* credentials,
* tokens,
* implementation secrets.

Plain output removes formatting, not sensitive information.

## When to use rich output

Use the default rich output when:

* working interactively,
* visually scanning errors,
* reading long titles,
* using a capable terminal,
* inspecting one error manually.

Example:

```bash
when-it-fails-setter errors . --category NETWORK
```

## When to use plain output

Use `--plain` when:

* redirecting to a file,
* piping into text tools,
* creating CI artifacts,
* comparing results,
* working in a limited terminal,
* avoiding ANSI formatting.

## Future export formats

Possible future dedicated machine formats may include:

```text
--json
--json-lines
--csv
--tsv-only
```

Such formats would need explicit contracts for:

* escaping,
* metadata,
* null values,
* collections,
* schema versioning,
* standard output,
* standard error.

The current `--plain` switch should not silently promise those guarantees.

## Recommended practices

1. Check the command exit code before parsing.
2. Remember that `errors --plain` includes metadata before the TSV section.
3. Extract the TSV header and rows explicitly in scripts.
4. Do not assume TSV escaping for tabs or newlines inside values.
5. Use Setter filters instead of reproducing them unnecessarily in shell tools.
6. Treat detail output as line-oriented text, not formal serialization.
7. Validate the workspace before creating CI exports.
8. Review sensitive values before sharing output.
9. Use rich output for humans and plain output for lightweight automation.
10. Use runtime APIs or JSON catalogs for robust structured integration.

## Central principle

> Plain output removes terminal decoration; it does not turn a human-oriented command into a formal data-export protocol.
