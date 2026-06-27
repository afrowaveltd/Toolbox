# Editing error fields

WhenItFails Setter can safely update selected fields of one error definition in `errors.en.json`.

Current editable fields are:

| Command                 | Field              |
| ----------------------- | ------------------ |
| `set-title`             | `Title`            |
| `set-message`           | `Message`          |
| `set-developer-hint`    | `DeveloperHint`    |
| `set-severity`          | `DefaultSeverity`  |
| `set-documentation-key` | `DocumentationKey` |

These commands intentionally edit presentation and diagnostic fields only.

They do not currently change stable identity fields such as:

```text
Id
Code
Name
Owner
CodePrefix
CodeGroup
PrimaryCategory
```

## General command shape

All edit commands follow this form:

```text
when-it-fails-setter <command> <path> <id|code|name> <new value>
```

When running through the Toolbox repository:

```bash
dotnet run \
  --project Toolroom/WhenItFails/Setter \
  -- <command> <path> <id|code|name> <new value>
```

Example:

```bash
dotnet run \
  --project Toolroom/WhenItFails/Setter \
  -- set-title . AFW_NET_0001 \
  "Network is not available"
```

## Workspace path

The `<path>` argument may point to:

* a project root containing `Jsons/WhenItFails`,
* the `Jsons/WhenItFails` directory itself.

Examples:

```text
.
./MyProject
./MyProject/Jsons/WhenItFails
```

## Error lookup

The target error may be selected by:

* stable ID,
* numeric code,
* symbolic name.

Examples:

```text
AFW_NET_0001
600001
NETWORKUNAVAILABLE
```

Lookup by ID and name is case-insensitive.

Examples:

```bash
when-it-fails-setter set-title . \
  AFW_NET_0001 \
  "Network is not available"
```

```bash
when-it-fails-setter set-title . \
  600001 \
  "Network is not available"
```

```bash
when-it-fails-setter set-title . \
  NETWORKUNAVAILABLE \
  "Network is not available"
```

All three examples target the same error definition when the catalog contains the corresponding identity.

## Recommended workflow

Before editing:

```text
validate
→ locate
→ inspect
```

After editing:

```text
inspect
→ validate
→ review Git diff
```

Complete example:

```bash
when-it-fails-setter validate .

when-it-fails-setter details . AFW_NET_0001

when-it-fails-setter set-title . \
  AFW_NET_0001 \
  "Network is not available"

when-it-fails-setter details . AFW_NET_0001

when-it-fails-setter validate .

git diff -- Jsons/WhenItFails/errors.en.json
```

## Why inspection matters

An error can be selected by several identifiers.

Before changing it, verify:

```text
Id
Code
Name
Owner
CodeGroup
PrimaryCategory
```

This reduces the risk of editing the wrong definition when several errors have similar names or titles.

## Text argument handling

For text commands, everything after the lookup value is joined with spaces.

Example:

```bash
when-it-fails-setter set-message . \
  AFW_NET_0001 \
  The network is currently unavailable.
```

is interpreted as:

```text
The network is currently unavailable.
```

Quoting remains strongly recommended:

```bash
when-it-fails-setter set-message . \
  AFW_NET_0001 \
  "The network is currently unavailable."
```

Quoting avoids shell interpretation problems involving:

* punctuation,
* repeated spaces,
* wildcard characters,
* parentheses,
* redirects,
* variable expansion.

## Whitespace normalization

Text values are trimmed before storage.

Input:

```text
"  Network is not available  "
```

Stored value:

```text
Network is not available
```

Whitespace-only values are rejected.

## Empty values

The following are invalid:

```bash
when-it-fails-setter set-title . AFW_NET_0001 ""
```

```bash
when-it-fails-setter set-message . AFW_NET_0001 "   "
```

Each field has its own failure code.

Possible codes include:

```text
TitleIsEmpty
MessageIsEmpty
DeveloperHintIsEmpty
DocumentationKeyIsEmpty
SeverityIsEmpty
```

The original catalog remains unchanged.

# Editing title

## Command

```text
set-title <path> <id|code|name> <title>
```

Example:

```bash
when-it-fails-setter set-title . \
  AFW_NET_0001 \
  "Network is not available"
```

## Purpose

`Title` is a short human-facing label.

Good titles are:

* concise,
* specific,
* naturally capitalized,
* understandable without internal implementation knowledge,
* suitable for tables, dialogs, logs, and summaries.

Good example:

```text
Network is not available
```

Less useful example:

```text
Error
```

Overly technical example:

```text
SocketException thrown by HTTP transport
```

The detailed technical explanation belongs elsewhere.

## Title versus message

Title:

```text
Network is not available
```

Message:

```text
The application could not reach the remote service.
```

The title identifies the problem.

The message explains it.

# Editing message

## Command

```text
set-message <path> <id|code|name> <message>
```

Example:

```bash
when-it-fails-setter set-message . \
  AFW_NET_0001 \
  "The application could not reach the remote service."
```

## Purpose

`Message` is the primary human-facing description of the failure.

A useful message should answer:

```text
What happened?
```

It may also briefly indicate:

```text
What can the user do next?
```

Example:

```text
The application could not reach the remote service. Check the network connection and try again.
```

## Message guidance

Prefer:

* clear language,
* complete sentences,
* neutral tone,
* actionable wording where appropriate,
* no blame,
* no unnecessary technical detail.

Avoid:

```text
Something went wrong.
```

Avoid exposing raw implementation details:

```text
System.Net.Http.HttpRequestException at line 417.
```

Avoid secrets or private environment data:

```text
Connection to internal-host-17 using token abc123 failed.
```

## Message versus developer hint

Message:

```text
The application could not reach the remote service.
```

Developer hint:

```text
Check DNS resolution, proxy settings, VPN state, firewall rules, and endpoint health.
```

The message is suitable for ordinary users.

The developer hint is technical diagnostic guidance.

# Editing developer hint

## Command

```text
set-developer-hint <path> <id|code|name> <developer-hint>
```

Example:

```bash
when-it-fails-setter set-developer-hint . \
  AFW_NET_0001 \
  "Check DNS, proxy, VPN, firewall rules, and endpoint availability."
```

## Purpose

`DeveloperHint` helps developers, operators, and support staff diagnose the problem.

A useful hint may contain:

* likely causes,
* recommended checks,
* relevant subsystems,
* recovery suggestions,
* configuration areas to inspect.

Example:

```text
Check DNS resolution, proxy configuration, VPN state, firewall rules, and remote endpoint health.
```

## Developer hint is not a stack trace

Avoid storing:

* complete stack traces,
* temporary exception text,
* request-specific values,
* machine-specific paths,
* credentials,
* tokens,
* customer data.

Those belong to runtime diagnostics, not the stable catalog definition.

## Optional but non-empty

The model allows a developer hint to be optional.

However, the current Setter command updates it only to a non-empty value.

The current command does not provide a dedicated clear/remove operation.

To remove an existing optional value, a future explicit command such as:

```text
clear-developer-hint
```

would be safer than treating an empty string as an ordinary edit.

# Editing severity

## Command

```text
set-severity <path> <id|code|name> <severity>
```

Example:

```bash
when-it-fails-setter set-severity . \
  AFW_NET_0001 \
  Warning
```

## Supported values

```text
Trace
Debug
Information
Warning
Error
Critical
```

Input matching is case-insensitive.

Example input:

```text
warning
```

Stored value:

```text
Warning
```

Unsupported values are rejected.

Example invalid input:

```text
Fatal
```

Failure code:

```text
UnsupportedSeverity
```

## Severity meanings

### Trace

Very detailed diagnostic information.

Use for low-level execution details that are normally disabled in production.

### Debug

Developer-oriented diagnostic information useful during investigation.

### Information

Normal significant application behavior that is not a problem.

### Warning

A problem or unusual condition occurred, but the application can continue.

### Error

An operation failed and needs attention, but the entire application is not necessarily unusable.

### Critical

A severe failure threatens application availability, integrity, or safe operation.

## Severity is behavioral

Changing severity can affect:

* logs,
* filters,
* dashboards,
* alerts,
* monitoring thresholds,
* profile selection,
* user-interface presentation,
* escalation policy.

Therefore severity edits should be reviewed as behavioral changes, not cosmetic wording changes.

Example commit message:

```text
Lower network unavailable severity to Warning
```

is better than:

```text
Update JSON
```

# Editing documentation key

## Command

```text
set-documentation-key <path> <id|code|name> <documentation-key>
```

Example:

```bash
when-it-fails-setter set-documentation-key . \
  AFW_NET_0001 \
  "when-it-fails/errors/network/network-unavailable"
```

## Purpose

`DocumentationKey` connects an error definition to external explanatory content.

Possible consumers include:

* documentation websites,
* local help systems,
* administration interfaces,
* support tools,
* troubleshooting pages,
* desktop or web UI links.

## Key versus URL

A documentation key does not need to be a full URL.

Recommended:

```text
when-it-fails/errors/network/network-unavailable
```

Less portable:

```text
https://docs.example.com/v1/errors/network-unavailable.html
```

A stable key allows each application to decide how to resolve it.

For example:

```text
documentation key
→ application documentation resolver
→ localized or environment-specific URL
```

## Recommended key properties

A documentation key should be:

* stable,
* machine-friendly,
* human-reviewable,
* independent of host name,
* independent of deployment environment,
* suitable for localization routing.

Avoid temporary values such as:

```text
new-page-2
test-link
todo
```

## Optional but non-empty

`DocumentationKey` may be optional in the model.

The current Setter command only assigns a non-empty value.

It does not currently provide a dedicated clear operation.

# Safe-write behavior

All edit commands use the same broad workflow:

```text
load catalog
→ normalize document
→ locate error
→ remember old value
→ apply new value in memory
→ validate edited catalog
→ serialize to temporary file
→ create timestamped backup
→ replace active file
```

For the complete behavior, see:

```text
Safe Writes
```

The active file is not directly truncated and rewritten from the beginning.

## Backup example

A successful edit may create:

```text
errors.en.20260627-095820-480.bak.json
```

The backup contains the original catalog before the edit.

## Validation failure

When the edited catalog is invalid:

```text
old value restored in memory
→ save not attempted
→ original file remains unchanged
```

Failure code:

```text
EditedErrorCatalogIsInvalid
```

## Save failure

When serialization, backup creation, or replacement fails:

```text
save response fails
→ command returns non-zero exit code
→ original or backup should be inspected
```

Possible failure codes include:

```text
AccessDenied
InputOutputError
JsonSerializationFailed
```

# Success output

A successful text edit prints:

```text
Updated title: AFW_NET_0001
New title: Network is not available
Error title changed from 'Network unavailable' to 'Network is not available'.
```

Other commands use the corresponding field names.

For example:

```text
Updated message: AFW_NET_0001
New message: The application could not reach the remote service.
```

Severity output reports the canonical stored severity.

## Same old and new value

The current editor does not treat an unchanged value as an error.

Example:

```bash
when-it-fails-setter set-title . \
  AFW_NET_0001 \
  "Network unavailable"
```

may still run the save workflow even when the existing title is already identical.

This can create a backup and rewrite the normalized document without a semantic change.

Recommended practice:

```text
inspect before editing
```

A future optimization may detect no-op edits and skip saving.

# Current editable scope

Setter currently allows changes to:

```text
Title
Message
DeveloperHint
DefaultSeverity
DocumentationKey
```

It does not currently expose commands for:

```text
Id
Code
Name
Owner
CodePrefix
CodeGroup
PrimaryCategory
Categories
Subcategories
Tags
Metadata
```

This separation is intentional.

Changing identity and classification fields can affect:

* stable API contracts,
* code ranges,
* owner allocation,
* profile resolution,
* cross-catalog references,
* application compatibility.

Such operations need richer guidance and stronger validation than a simple field setter.

# One field per command

Each command edits exactly one field.

Advantages:

* clear intent,
* focused Git diff,
* simple command syntax,
* easy testing,
* isolated failure,
* straightforward audit history.

Example:

```bash
when-it-fails-setter set-title . \
  AFW_NET_0001 \
  "Network is not available"

when-it-fails-setter set-message . \
  AFW_NET_0001 \
  "The application could not reach the remote service."
```

Both commands create their own safe-write cycle and backup.

## Multiple related edits

For several related fields:

```text
inspect
→ edit title
→ edit message
→ edit developer hint
→ validate
→ review diff
```

Example:

```bash
when-it-fails-setter details . AFW_NET_0001

when-it-fails-setter set-title . \
  AFW_NET_0001 \
  "Network is not available"

when-it-fails-setter set-message . \
  AFW_NET_0001 \
  "The application could not reach the remote service."

when-it-fails-setter set-developer-hint . \
  AFW_NET_0001 \
  "Check DNS, proxy, VPN, firewall rules, and endpoint health."

when-it-fails-setter validate .

git diff -- Jsons/WhenItFails/errors.en.json
```

This produces several backups.

That is expected under the current one-command-one-write design.

# Review after editing

After any edit:

```bash
when-it-fails-setter details . AFW_NET_0001
```

Then validate:

```bash
when-it-fails-setter validate .
```

Then review the source change:

```bash
git diff -- Jsons/WhenItFails/errors.en.json
```

Confirm:

* the correct error changed,
* only intended semantic values changed,
* severity is correct,
* no sensitive data was introduced,
* stable identity remains unchanged,
* any formatting normalization is understood.

# Exit codes

Write commands generally use:

```text
0
→ field updated successfully

1
→ required command arguments missing or invalid

2
→ lookup, validation, backup, or save failed
```

Unexpected application exceptions may return:

```text
3
```

Scripts should check the exit code before assuming the catalog was updated.

# Shell scripting example

```bash
if dotnet run \
  --project Toolroom/WhenItFails/Setter \
  -- set-severity . AFW_NET_0001 Warning
then
  dotnet run \
    --project Toolroom/WhenItFails/Setter \
    -- validate .
else
  echo "Severity update failed." >&2
  exit 1
fi
```

# Writing quality checklist

## Title

* Is it concise?
* Does it identify the failure?
* Is it naturally capitalized?
* Can it fit comfortably in a table or dialog?

## Message

* Does it clearly explain what happened?
* Is it suitable for the intended audience?
* Is it actionable where possible?
* Does it avoid raw implementation detail?

## Developer hint

* Does it help investigation?
* Does it name useful checks?
* Does it avoid secrets and request-specific values?
* Is it stable enough to live in a catalog?

## Severity

* Does it reflect actual operational impact?
* Could it trigger unnecessary alerts?
* Could it hide an important failure?
* Is the change reviewed as behavior?

## Documentation key

* Is it stable?
* Is it machine-friendly?
* Is it independent of environment?
* Can a resolver map it to localized documentation?

# Common mistakes

## Editing without inspecting

Risk:

```text
wrong error selected
```

Prevention:

```bash
when-it-fails-setter details . <lookup>
```

## Using title as a complete message

Poor title:

```text
The application could not connect to the network because the configured DNS server did not respond and the user should verify all network settings.
```

Better:

```text
Network is not available
```

Place the explanation in `Message`.

## Putting technical diagnostics into the message

Poor message:

```text
SocketException 11001 from Dns.GetHostEntryAsync.
```

Better message:

```text
The remote service could not be reached.
```

Developer hint:

```text
Check DNS resolution and endpoint configuration.
```

## Using unsupported severity

Invalid:

```text
Fatal
```

Use:

```text
Critical
```

## Storing a full URL as documentation identity

Possible, but less portable:

```text
https://production.example.com/docs/error-600001
```

Prefer a stable key:

```text
errors/network/network-unavailable
```

## Skipping full validation

The editor validates the edited error document, but complete workspace validation should still follow every edit.

Run:

```bash
when-it-fails-setter validate .
```

# Related documentation

* [Getting started](../Getting-Started/en.md)
* [Command reference](../Commands/en.md)
* [Setting title](../Setting%20Title/en.md)
* [Safe writes](../Safe%20Writes/en.md)
* [Plain output](../Plain%20Output/en.md)

# Central principle

> Inspect the error, edit one explicit field, validate the complete workspace, and review the resulting source change.
