# Inspecting error details

The `details` command displays one complete error definition from a validated WhenItFails workspace.

Alias:

```text
detail
```

Both commands behave identically.

## Command syntax

```text
details <path> <id|code|name> [--plain]
```

or:

```text
detail <path> <id|code|name> [--plain]
```

Example:

```bash
when-it-fails-setter details . AFW_NET_0001
```

From the Toolbox repository:

```bash
dotnet run \
  --project Toolroom/WhenItFails/Setter \
  -- details . AFW_NET_0001
```

## Accepted workspace paths

The path may point to:

* the project root,
* the `Jsons/WhenItFails` directory directly.

Examples:

```bash
when-it-fails-setter details . AFW_NET_0001
```

```bash
when-it-fails-setter details \
  ./Jsons/WhenItFails \
  AFW_NET_0001
```

## Lookup forms

An error definition may be selected by:

```text
stable ID
numeric code
symbolic name
```

### Lookup by stable ID

```bash
when-it-fails-setter details . \
  AFW_NET_0001
```

### Lookup by numeric code

```bash
when-it-fails-setter details . \
  600001
```

### Lookup by symbolic name

```bash
when-it-fails-setter details . \
  NETWORKUNAVAILABLE
```

ID and name matching are case-insensitive.

Numeric code lookup uses an integer comparison.

## Lookup order

When the lookup value can be parsed as an integer, Setter first searches by numeric code.

Conceptually:

```text
can lookup value be parsed as integer?
→ yes: search Code
→ not found: continue with Id and Name
→ no: search Id and Name
```

This allows ordinary numeric error codes to be used directly.

## Validation before lookup

The `details` command validates the complete workspace before displaying any error.

Sequence:

```text
resolve workspace
→ validate all catalogs
→ stop on validation failure
→ load normalized workspace summary
→ locate error
→ render detail
```

If workspace validation fails, no error detail is shown.

Expected exit code:

```text
2
```

## Why validation happens first

Displaying an error from an invalid workspace could be misleading.

For example:

* owner reference may be invalid,
* code group may not exist,
* category may be unknown,
* code may fall outside its group,
* duplicate identities may exist.

Setter therefore refuses to present the definition as authoritative until the complete workspace is valid.

## Fields displayed

The detail view displays:

```text
Code
Id
Name
Title
Message
Severity
Owner
Code prefix
Code group
Primary category
Categories
Subcategories
Tags
Developer hint
Documentation key
```

This gives a complete authoring view of one error definition.

## Example rich output

The default view uses Spectre.Console.

Conceptually:

```text
WhenItFails Error Detail
Workspace: Jsons/WhenItFails

Field                 Value
────────────────────────────────────────────────
Code                  600001
Id                    AFW_NET_0001
Name                  NETWORKUNAVAILABLE
Title                 Network unavailable
Message               The network is unavailable.
Severity              Error
Owner                 AFW
Code prefix           NET
Code group            NETWORK
Primary category      NETWORK
Categories            NETWORK
Subcategories         CONNECTIVITY
Tags                  NETWORK, USER_VISIBLE
Developer hint        Check connectivity...
Documentation key     when-it-fails/errors/...
```

The exact layout depends on terminal width.

## Plain output

Use:

```bash
when-it-fails-setter details . \
  AFW_NET_0001 \
  --plain
```

The alias also supports it:

```bash
when-it-fails-setter detail . \
  AFW_NET_0001 \
  --plain
```

## Plain output structure

Current plain output is line-oriented key/value text.

Example:

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

This is not JSON, YAML, or strict TSV.

It is intended for:

* shell scripts,
* redirected output,
* CI logs,
* human-readable text processing.

## Optional fields

`DeveloperHint` and `DocumentationKey` may be empty.

Plain output then prints:

```text
Developer hint:
Documentation key:
```

The labels remain present.

This gives the output a stable field order even when values are absent.

## Collection formatting

These fields are collections:

```text
Categories
Subcategories
Tags
```

They are joined into one plain text value.

Example:

```text
Tags: NETWORK, USER_VISIBLE
```

The exact separator is defined by the shared console table helper.

Scripts should not assume these lines are lossless structured serialization.

## Read-only behavior

The `details` command does not intentionally modify:

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
→ lookup
→ rendering
```

No save is attempted.

## Error not found

When no matching error exists, Setter returns:

```text
ErrorDefinitionNotFound
```

Example:

```bash
when-it-fails-setter details . \
  AFW_UNKNOWN_9999
```

Message conceptually:

```text
No error definition was found for 'AFW_UNKNOWN_9999'.
Search by Id, Code or Name.
```

Expected exit code:

```text
1
```

## Missing arguments

Example:

```bash
when-it-fails-setter details .
```

Expected issue code:

```text
MissingDetailsArguments
```

Expected syntax:

```text
details <path> <id|code|name>
```

Expected exit code:

```text
1
```

## Exit codes

```text
0
→ error definition displayed successfully
```

```text
1
→ required arguments missing or error not found
```

```text
2
→ workspace validation failed
```

Unexpected top-level failures may return:

```text
3
```

## `details` versus `errors`

Use `errors` for discovery:

```bash
when-it-fails-setter errors . \
  --search network
```

Use `details` for complete inspection:

```bash
when-it-fails-setter details . \
  AFW_NET_0001
```

Typical workflow:

```text
list or search
→ choose one error
→ inspect complete definition
```

## Recommended pre-edit workflow

Before changing a field:

```bash
when-it-fails-setter details . \
  AFW_NET_0001
```

Confirm:

* stable ID,
* numeric code,
* symbolic name,
* current field value,
* owner,
* group,
* category,
* severity.

Then edit:

```bash
when-it-fails-setter set-title . \
  AFW_NET_0001 \
  "Network is not available"
```

Then inspect again:

```bash
when-it-fails-setter details . \
  AFW_NET_0001
```

## Recommended post-edit workflow

```text
details before
→ edit
→ details after
→ validate workspace
→ review Git diff
```

Example:

```bash
when-it-fails-setter details . \
  AFW_NET_0001

when-it-fails-setter set-message . \
  AFW_NET_0001 \
  "The application could not reach the network."

when-it-fails-setter details . \
  AFW_NET_0001

when-it-fails-setter validate .

git diff -- \
  Jsons/WhenItFails/errors.en.json
```

## Inspecting by code

Numeric code lookup is useful when the value comes from:

* log output,
* API response,
* monitoring event,
* support ticket,
* runtime failure record.

Example:

```bash
when-it-fails-setter details . 600001
```

This avoids first translating the number into an ID.

## Inspecting by name

Symbolic names are convenient during development.

Example:

```bash
when-it-fails-setter details . \
  NETWORKUNAVAILABLE
```

Names are often easier to remember than numeric codes.

## Inspecting by ID

Stable IDs are usually the safest authoring lookup.

Example:

```bash
when-it-fails-setter details . \
  AFW_NET_0001
```

An ID explicitly identifies:

* owner or namespace prefix,
* code family,
* stable sequence.

For documentation and scripts, stable ID is generally preferable to title text.

## Titles are not lookup keys

This does not search by title:

```bash
when-it-fails-setter details . \
  "Network unavailable"
```

Supported lookup fields are only:

```text
Id
Code
Name
```

Use full-text search first:

```bash
when-it-fails-setter errors . \
  --search "Network unavailable"
```

Then pass the returned ID, code, or name to `details`.

## Quoting lookup values

Normal IDs and symbolic names do not require quotes:

```bash
when-it-fails-setter details . AFW_NET_0001
```

If a malformed or unusual lookup contains shell-sensitive characters, quote it.

However, valid IDs and names should normally remain simple symbolic values.

## `--plain` switch handling

The `--plain` switch is recognized case-insensitively.

These are equivalent:

```text
--plain
--Plain
--PLAIN
```

Canonical lowercase spelling is recommended.

The switch may appear after the required arguments.

Example:

```bash
when-it-fails-setter details . \
  AFW_NET_0001 \
  --plain
```

## Unknown extra arguments

The current command checks only:

* path,
* lookup value,
* presence of `--plain`.

Other extra arguments are not currently rejected by a strict parser.

Example:

```bash
when-it-fails-setter details . \
  AFW_NET_0001 \
  --something
```

may still display the error successfully.

Avoid relying on ignored arguments.

A future stricter parser may reject them.

## Plain output in scripts

Extract one field:

```bash
when-it-fails-setter details . \
  AFW_NET_0001 \
  --plain |
awk -F ': ' '
  $1 == "Severity" {
    print $2
  }
'
```

Extract the ID:

```bash
when-it-fails-setter details . \
  600001 \
  --plain |
awk -F ': ' '
  $1 == "Id" {
    print $2
  }
'
```

## Values containing colons

The plain format uses:

```text
Field: Value
```

A value itself may contain a colon.

Therefore splitting every line on every colon is unsafe.

Prefer splitting only on the first delimiter.

Example with shell parameter expansion:

```bash
line="$(
  when-it-fails-setter details . \
    AFW_NET_0001 \
    --plain |
  grep '^Message: '
)"

message="${line#Message: }"

printf '%s\n' "$message"
```

## Plain output is not a stable serialization format

The plain detail view is useful, but it is still presentation-oriented.

Do not treat it as a permanent machine API.

Potential future changes may include:

* new fields,
* changed labels,
* machine-readable JSON,
* explicit output format selection.

For robust automation, a dedicated JSON format would be preferable.

## Rich output and ANSI

The rich view may contain ANSI control sequences.

When redirecting output:

```bash
when-it-fails-setter details . \
  AFW_NET_0001 \
  --plain \
  > error-detail.txt
```

Plain mode is the safer choice.

Validation and failure paths may still use the shared validation renderer.

Therefore `--plain` does not currently guarantee ANSI-free output for every possible failure.

## Validation failure example

Suppose the catalog contains:

```json
"defaultSeverity": "Fatal"
```

Then:

```bash
when-it-fails-setter details . \
  AFW_NET_0001
```

does not display the requested error.

It first reports workspace validation failure and returns:

```text
2
```

Repair the workspace, validate, and retry.

## Wrong workspace

When the lookup unexpectedly fails:

```text
ErrorDefinitionNotFound
```

check:

```bash
pwd
```

and:

```bash
realpath Jsons/WhenItFails
```

The error may exist in another project workspace.

## Case sensitivity

ID and name lookup use case-insensitive comparison.

These are equivalent:

```text
AFW_NET_0001
afw_net_0001
```

and:

```text
NETWORKUNAVAILABLE
networkunavailable
```

Numeric codes are compared as integers.

File and directory paths remain subject to filesystem rules.

## Duplicate identities

The workspace validator should reject duplicate:

* IDs,
* names,
* numeric codes.

Therefore a successful `details` lookup should identify one unambiguous definition.

The command uses the first matching definition internally, but duplicate valid identities should not survive workspace validation.

## Practical diagnostic workflow

When a runtime log reports code `600001`:

```bash
when-it-fails-setter validate .

when-it-fails-setter details . \
  600001

echo "Exit code: $?"
```

Then inspect:

* title,
* message,
* severity,
* developer hint,
* documentation key,
* categories,
* tags.

This turns a raw numeric code into an actionable catalog definition.

## Comparing before and after

Capture plain output before editing:

```bash
when-it-fails-setter details . \
  AFW_NET_0001 \
  --plain \
  > /tmp/error-before.txt
```

Apply the edit:

```bash
when-it-fails-setter set-title . \
  AFW_NET_0001 \
  "Network is not available"
```

Capture after:

```bash
when-it-fails-setter details . \
  AFW_NET_0001 \
  --plain \
  > /tmp/error-after.txt
```

Compare:

```bash
diff -u \
  /tmp/error-before.txt \
  /tmp/error-after.txt
```

Then clean up:

```bash
rm -f \
  /tmp/error-before.txt \
  /tmp/error-after.txt
```

## Suggested smoke tests

Lookup by ID:

```bash
dotnet run \
  --project Toolroom/WhenItFails/Setter \
  -- details . AFW_NET_0001
```

Lookup by code:

```bash
dotnet run \
  --project Toolroom/WhenItFails/Setter \
  -- details . 600001
```

Lookup by name:

```bash
dotnet run \
  --project Toolroom/WhenItFails/Setter \
  -- detail . NETWORKUNAVAILABLE
```

Plain output:

```bash
dotnet run \
  --project Toolroom/WhenItFails/Setter \
  -- details . AFW_NET_0001 --plain
```

Not found:

```bash
dotnet run \
  --project Toolroom/WhenItFails/Setter \
  -- details . AFW_UNKNOWN_9999

echo "Exit code: $?"
```

Expected:

```text
Exit code: 1
```

## Future improvements

Possible future improvements include:

* JSON output,
* YAML output,
* field selection,
* direct documentation-link opening,
* profile membership display,
* source line information,
* explanation of cross-catalog references,
* strict rejection of unknown arguments,
* comparison between two definitions,
* history or backup inspection.

These are future possibilities, not current guarantees.

## Checklist

Before editing an error, confirm:

* workspace validation succeeds,
* lookup identifies the intended definition,
* ID, code, and name agree,
* current title and message are understood,
* owner and code group are correct,
* primary category is correct,
* severity is appropriate,
* tags and subcategories are expected,
* developer hint is useful,
* documentation key points to the intended topic.

## Related documentation

* [Browsing and Filtering Errors](../Browsing%20and%20Filtering%20Errors/en.md)
* [Editing Error Fields](../Editing%20Error%20Fields/en.md)
* [Plain Output](../Plain%20Output/en.md)
* [Validation](../Validation/en.md)
* [Troubleshooting](../Troubleshooting/en.md)
* [Commands](../Commands/en.md)

## Central principle

> Inspect the complete definition before changing one field, and inspect it again afterward.

