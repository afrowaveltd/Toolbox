# Authoring error text

A good error definition separates:

```text
identity
→ what kind of failure this is
```

```text
title
→ short human-readable label
```

```text
message
→ what happened
```

```text
developer hint
→ what a developer should investigate
```

```text
severity
→ how serious the default condition is
```

```text
documentation key
→ where extended guidance belongs
```

These fields should complement one another rather than repeat the same sentence five times.

## Relevant fields

The current Setter edit commands modify:

```text
Title
Message
DeveloperHint
DefaultSeverity
DocumentationKey
```

Commands:

```text
set-title
set-message
set-developer-hint
set-severity
set-documentation-key
```

## Title

The title is a short human-readable name for the failure.

Example:

```text
Network unavailable
```

A title should answer:

```text
What kind of problem is this?
```

It should not attempt to explain the entire incident.

## Good title characteristics

A good title is:

* short,
* specific,
* stable,
* understandable without implementation detail,
* suitable for a table or dialog heading,
* different from the symbolic machine name.

Examples:

```text
Network unavailable
```

```text
Configuration value missing
```

```text
Database connection failed
```

```text
Unsupported file format
```

```text
Access denied
```

## Weak titles

Too vague:

```text
Error
```

```text
Something failed
```

```text
Problem
```

Too technical:

```text
SocketException in HttpClient.SendAsync
```

Too long:

```text
The application was unable to connect to the requested remote network service because the network was unavailable
```

Too procedural:

```text
Please check the network cable and try again
```

Procedural guidance belongs in the message, developer hint, or documentation.

## Title capitalization

Use sentence-style capitalization consistently.

Recommended:

```text
Network unavailable
```

rather than:

```text
Network Unavailable
```

unless the project deliberately adopts title case.

Acronyms may retain their normal casing:

```text
DNS lookup failed
```

```text
HTTP request rejected
```

```text
JSON document invalid
```

## Title punctuation

Titles normally do not need a final period.

Recommended:

```text
Network unavailable
```

Not usually:

```text
Network unavailable.
```

Question marks or exclamation marks should be rare.

## Title versus symbolic name

Example symbolic name:

```text
NETWORKUNAVAILABLE
```

Example title:

```text
Network unavailable
```

The symbolic name is machine-friendly and stable.

The title is human-readable.

Do not merely insert spaces into every symbolic name when better wording exists.

## Editing a title

```bash
when-it-fails-setter set-title . \
  AFW_NET_0001 \
  "Network is not available"
```

Inspect before and after:

```bash
when-it-fails-setter details . \
  AFW_NET_0001
```

## Message

The message is the default human-readable explanation of what happened.

Example:

```text
The network is unavailable.
```

A message should answer:

```text
What happened in this failure?
```

It may provide limited context, but it should remain reusable.

## Good message characteristics

A good message is:

* a complete sentence,
* clear to the intended audience,
* neutral in tone,
* reusable across occurrences,
* free of misleading certainty,
* useful without exposing internal implementation,
* safe to display in logs or user interfaces where intended.

Examples:

```text
The application could not reach the remote service.
```

```text
A required configuration value is missing.
```

```text
The supplied document is not valid JSON.
```

```text
The current user does not have permission to perform this operation.
```

## Avoid blame

Avoid messages such as:

```text
You entered an invalid value.
```

Prefer:

```text
The supplied value is not valid.
```

The neutral form is more accurate because the value may have come from:

* a user,
* an API,
* a configuration file,
* another service,
* generated data,
* an earlier application state.

## Avoid unnecessary apology

Messages normally do not need:

```text
Sorry, something went wrong.
```

The catalog should describe the failure clearly.

A user interface may add suitable conversational language separately.

## Avoid implementation leakage

Weak:

```text
HttpClient threw TaskCanceledException while awaiting SendAsync.
```

Better:

```text
The remote request did not complete within the allowed time.
```

Implementation details may belong in:

* diagnostics,
* exception data,
* developer hint,
* logs,
* documentation.

## Avoid claiming a cause that is not known

Weak:

```text
The server is offline.
```

when the application only knows that a connection failed.

Better:

```text
The remote service could not be reached.
```

Possible causes may include:

* server offline,
* DNS failure,
* firewall,
* proxy,
* VPN,
* routing,
* timeout,
* local network failure.

The default message should state what is known.

## Message punctuation

Messages should normally be complete sentences ending with punctuation.

Recommended:

```text
The network is unavailable.
```

rather than:

```text
Network unavailable
```

The shorter form is better suited to the title.

## Message versus title

Title:

```text
Network unavailable
```

Message:

```text
The application could not reach the network.
```

The title labels the condition.

The message explains the occurrence.

Avoid:

```text
Title:
Network unavailable
```

```text
Message:
Network unavailable
```

Exact duplication wastes one of the fields.

## Static default message

The catalog message is a default reusable message.

It should not contain occurrence-specific values unless the runtime has a defined placeholder system that safely supplies them.

Weak static text:

```text
Could not open /home/user/file.txt.
```

Better reusable default:

```text
The requested file could not be opened.
```

Occurrence-specific details can be added at runtime.

## Sensitive information

Do not place secrets or private values in default messages.

Avoid:

* passwords,
* access tokens,
* connection strings,
* private filesystem paths,
* personal data,
* customer identifiers,
* raw SQL containing sensitive data.

Catalog content is usually stored in source control and may be distributed with packages.

## Editing a message

```bash
when-it-fails-setter set-message . \
  AFW_NET_0001 \
  "The application could not reach the network."
```

## Developer hint

The developer hint is optional developer-focused guidance.

Example:

```text
Check connectivity, DNS, firewall, proxy, VPN, and remote endpoint availability.
```

It should answer:

```text
What should a developer or operator investigate next?
```

## Intended audience

A developer hint is primarily for:

* developers,
* operators,
* support engineers,
* system administrators,
* advanced diagnostics,
* maintainers.

It is not necessarily appropriate for direct display to an end user.

## Good developer hint characteristics

A good developer hint is:

* actionable,
* concise,
* technically useful,
* based on plausible causes,
* careful not to overstate certainty,
* independent of one local machine,
* free of secrets.

Examples:

```text
Check the configured endpoint, DNS resolution, proxy settings, VPN state, and network connectivity.
```

```text
Verify that the required configuration key exists and that the selected environment loads the expected configuration source.
```

```text
Inspect the inner exception and database logs, then verify credentials, server availability, and connection-string settings.
```

```text
Confirm that the file exists, that the process has read permission, and that no other process holds an incompatible lock.
```

## Weak developer hints

Too vague:

```text
Check the code.
```

```text
Fix the problem.
```

Too certain:

```text
The firewall blocked the request.
```

when that has not been established.

Too local:

```text
Ask Peter because his server does this sometimes.
```

Too dangerous:

```text
Disable all security and run as root.
```

Too verbose:

```text
A complete multi-page troubleshooting guide embedded in one JSON property.
```

Long guidance belongs in documentation.

## Hint versus message

Message:

```text
The remote service could not be reached.
```

Developer hint:

```text
Check the endpoint address, DNS, proxy, VPN, firewall, and service availability.
```

The message states the observed failure.

The hint suggests investigation.

## Hint versus documentation

The hint should be the first useful diagnostic step.

The documentation should contain:

* detailed troubleshooting,
* examples,
* platform differences,
* recovery procedures,
* related errors,
* architecture context.

A useful relationship is:

```text
developer hint
→ first diagnostic direction
```

```text
documentation key
→ complete explanation
```

## Editing a developer hint

```bash
when-it-fails-setter set-developer-hint . \
  AFW_NET_0001 \
  "Check the endpoint, DNS, proxy, VPN, firewall, and service availability."
```

## Current empty-value behavior

The Setter currently rejects whitespace-only developer hints.

Example:

```bash
when-it-fails-setter set-developer-hint . \
  AFW_NET_0001 \
  "   "
```

Expected issue:

```text
DeveloperHintIsEmpty
```

The command sets a non-empty value.

It is not currently a general remove-or-clear command.

## Default severity

`DefaultSeverity` expresses the normal seriousness of this error definition.

Supported values:

```text
Trace
Debug
Information
Warning
Error
Critical
```

The value is canonicalized case-insensitively.

Example input:

```text
warning
```

Stored value:

```text
Warning
```

## Severity is a default

The field is:

```text
DefaultSeverity
```

not:

```text
PermanentSeverity
```

A runtime occurrence may have additional context that justifies a different handling level.

The catalog defines the ordinary expected severity of the reusable error type.

## Trace

Use `Trace` for extremely detailed diagnostic events that are not normally useful outside deep investigation.

Typical characteristics:

* very high volume,
* fine-grained execution detail,
* usually disabled in production,
* not itself a user-facing failure.

A stable catalog error will rarely need `Trace`.

## Debug

Use `Debug` for developer-oriented diagnostic conditions.

Typical characteristics:

* useful during development,
* not normally operationally important,
* may describe an expected internal branch,
* should not trigger ordinary alerts.

## Information

Use `Information` when the condition is notable but not harmful.

Examples may include:

* optional fallback selected,
* operation skipped intentionally,
* non-error lifecycle event represented through the same result infrastructure.

Do not classify a genuine failed user operation as `Information` merely to reduce log noise.

## Warning

Use `Warning` when:

* the operation can continue,
* recovery occurred,
* degraded behavior is active,
* user attention may eventually be required,
* a risk exists but the main operation did not completely fail.

Examples:

```text
Preferred configuration source unavailable; fallback source used.
```

```text
Optional metadata could not be loaded.
```

## Error

Use `Error` when:

* the requested operation failed,
* the caller must handle failure,
* normal completion did not occur,
* the problem is significant but the whole application is not necessarily unusable.

This is the usual default for ordinary failed operations.

## Critical

Use `Critical` when:

* continued operation may be unsafe,
* a core service cannot function,
* data integrity is at immediate risk,
* the application cannot provide its essential purpose,
* urgent operator attention is justified.

Examples may include:

* unrecoverable corruption of essential state,
* inability to initialize a mandatory security component,
* failure threatening persistent data integrity.

Do not use `Critical` merely because an error is frustrating.

## Unsupported severity

This is invalid:

```text
Fatal
```

The supported highest value is:

```text
Critical
```

Example rejected command:

```bash
when-it-fails-setter set-severity . \
  AFW_NET_0001 \
  Fatal
```

Expected issue:

```text
UnsupportedSeverity
```

## Choosing severity by impact

Ask:

```text
Did the requested operation succeed?
```

```text
Can the application continue safely?
```

```text
Was recovery automatic?
```

```text
Is data integrity at risk?
```

```text
Does an operator need urgent attention?
```

A simple guide:

```text
diagnostic only
→ Trace or Debug
```

```text
notable successful condition
→ Information
```

```text
degraded but continued
→ Warning
```

```text
operation failed
→ Error
```

```text
system or data safety threatened
→ Critical
```

## Do not choose severity by emotion

Avoid:

```text
This bug is annoying, therefore Critical.
```

Severity should reflect operational impact, not developer frustration.

## Editing severity

```bash
when-it-fails-setter set-severity . \
  AFW_NET_0001 \
  Warning
```

After editing:

```bash
when-it-fails-setter details . \
  AFW_NET_0001
```

Then:

```bash
when-it-fails-setter validate .
```

## Documentation key

The documentation key is an optional stable reference to extended documentation.

Example:

```text
when-it-fails/errors/network/network-unavailable
```

It should answer:

```text
Where can more complete guidance for this error be found?
```

## Key versus URL

A documentation key does not have to be a complete URL.

A stable logical key is often preferable:

```text
when-it-fails/errors/network/network-unavailable
```

The consuming application can resolve it to:

* local documentation,
* web documentation,
* localized documentation,
* embedded help,
* an internal support portal.

## Good documentation key characteristics

A good key is:

* stable,
* predictable,
* lowercase where project convention requires it,
* independent of one deployment hostname,
* suitable for lookup,
* not tied to a temporary file location.

Examples:

```text
when-it-fails/errors/network/network-unavailable
```

```text
when-it-fails/errors/configuration/missing-value
```

```text
when-it-fails/errors/storage/read-failed
```

## Weak documentation keys

Machine-local path:

```text
C:\Users\John\Documents\error-help.md
```

Temporary development URL:

```text
http://localhost:5000/docs/error1
```

Unstable title-derived text:

```text
The Network Error Page Final Version 3
```

Secret internal token:

```text
https://docs.example.test/error?token=secret
```

## Stable hierarchy

A useful pattern is:

```text
product-or-package
/errors
/domain
/error-topic
```

Example:

```text
when-it-fails/errors/network/network-unavailable
```

The hierarchy should remain readable but should primarily function as a stable identifier.

## Documentation key versus error ID

Error ID:

```text
AFW_NET_0001
```

Documentation key:

```text
when-it-fails/errors/network/network-unavailable
```

The error ID identifies the definition.

The documentation key identifies extended guidance.

Several related errors could theoretically point to:

* separate documentation pages,
* one shared troubleshooting page,
* one domain overview.

## Editing a documentation key

```bash
when-it-fails-setter set-documentation-key . \
  AFW_NET_0001 \
  "when-it-fails/errors/network/network-unavailable"
```

## Current empty-value behavior

Whitespace-only documentation keys are rejected.

Expected issue:

```text
DocumentationKeyIsEmpty
```

The current command assigns a non-empty value.

It does not provide general remove behavior.

## Keep fields independent

Weak definition:

```text
Title:
Network error
```

```text
Message:
Network error
```

```text
Developer hint:
Network error
```

```text
Documentation key:
network-error
```

Better definition:

```text
Title:
Network unavailable
```

```text
Message:
The application could not reach the remote service.
```

```text
Developer hint:
Check the endpoint, DNS, proxy, VPN, firewall, and service availability.
```

```text
Documentation key:
when-it-fails/errors/network/network-unavailable
```

Each field now contributes different information.

## Example: missing configuration

```text
Name:
MISSINGCONFIGURATIONVALUE
```

```text
Title:
Configuration value missing
```

```text
Message:
A required configuration value is missing.
```

```text
Severity:
Error
```

```text
Developer hint:
Verify that the expected configuration source is loaded and that the required key exists for the selected environment.
```

```text
Documentation key:
when-it-fails/errors/configuration/missing-value
```

## Example: network fallback

```text
Name:
PREFERREDENDPOINTUNAVAILABLE
```

```text
Title:
Preferred endpoint unavailable
```

```text
Message:
The preferred endpoint could not be reached, so a fallback endpoint was selected.
```

```text
Severity:
Warning
```

```text
Developer hint:
Check the preferred endpoint address, DNS resolution, routing, proxy, VPN, and service health.
```

```text
Documentation key:
when-it-fails/errors/network/preferred-endpoint-unavailable
```

## Example: invalid JSON

```text
Name:
INVALIDJSONDOCUMENT
```

```text
Title:
JSON document invalid
```

```text
Message:
The supplied document is not valid JSON.
```

```text
Severity:
Error
```

```text
Developer hint:
Inspect the reported JSON path and parser details, then check quoting, commas, braces, brackets, and encoding.
```

```text
Documentation key:
when-it-fails/errors/data/invalid-json
```

## Example: access denied

```text
Name:
ACCESSDENIED
```

```text
Title:
Access denied
```

```text
Message:
The current process does not have permission to perform the requested operation.
```

```text
Severity:
Error
```

```text
Developer hint:
Check file or resource ownership, directory permissions, execution identity, mount options, and security policy.
```

```text
Documentation key:
when-it-fails/errors/security/access-denied
```

## Editing workflow

Recommended workflow:

```text
find definition
→ inspect complete detail
→ decide which field is wrong
→ change one field
→ inspect again
→ validate
→ review Git diff
```

Example:

```bash
when-it-fails-setter errors . \
  --search network

when-it-fails-setter details . \
  AFW_NET_0001

when-it-fails-setter set-message . \
  AFW_NET_0001 \
  "The application could not reach the remote service."

when-it-fails-setter details . \
  AFW_NET_0001

when-it-fails-setter validate .

git diff -- \
  Jsons/WhenItFails/errors.en.json
```

## Change one semantic idea at a time

Prefer one focused edit:

```text
correct message wording
```

then validate.

Avoid combining unrelated changes such as:

```text
rewrite title
change severity
change documentation hierarchy
rename categories
reassign owner
```

in one unreviewed step.

Focused edits are easier to:

* review,
* test,
* revert,
* explain,
* commit.

## Validate semantic consistency

After editing, ask:

```text
Does the title still match the message?
```

```text
Does the hint investigate plausible causes?
```

```text
Does severity match operational impact?
```

```text
Does the documentation key describe the same error?
```

```text
Does the symbolic name still accurately represent the definition?
```

Setter validates structure and known relationships.

It cannot fully determine whether English wording is semantically good.

Human review remains necessary.

## Avoid promises of recovery

Weak:

```text
The connection will be restored shortly.
```

Unless the system knows that.

Better:

```text
The connection is currently unavailable.
```

Do not promise:

* automatic recovery,
* data preservation,
* retry success,
* service availability,
* completion time,

unless the system can guarantee it.

## Avoid ambiguous pronouns

Weak:

```text
It could not be loaded.
```

Better:

```text
The configuration document could not be loaded.
```

Messages may appear without surrounding interface context.

## Prefer concrete nouns

Weak:

```text
The thing failed.
```

Better:

```text
The database connection failed.
```

Weak:

```text
This is unavailable.
```

Better:

```text
The requested service is unavailable.
```

## Avoid unnecessary internal names

Weak:

```text
FooRepositoryProviderFactory failed.
```

Better user-facing message:

```text
The data provider could not be initialized.
```

The internal component name may be added to diagnostics separately.

## Be careful with “invalid”

Use `invalid` when a rule was actually checked and failed.

Example:

```text
The supplied JSON document is invalid.
```

Do not use it merely because an operation failed for an unknown reason.

## Be careful with “not found”

Use `not found` when the search completed and no matching resource existed.

Do not use it when access was denied or the lookup could not be completed.

These are different conditions:

```text
File not found
```

```text
Access denied
```

```text
File lookup failed
```

## Be careful with timeout wording

A timeout proves only that the operation did not complete within the allowed interval.

Good:

```text
The remote request did not complete within the allowed time.
```

Too certain:

```text
The server is down.
```

## Reusable wording

Catalog text should normally avoid one-off context such as:

```text
The report John requested yesterday failed.
```

Prefer:

```text
The report could not be generated.
```

Occurrence-specific context belongs in runtime parameters or diagnostics.

## Localization readiness

Even when only English exists today, write text that can later be translated.

Prefer:

* complete sentences,
* clear subject and object,
* limited embedded formatting,
* no sentence fragments assembled dynamically,
* no concatenated grammatical pieces,
* no culture-specific jokes in operational errors.

Avoid constructing:

```text
"File " + name + " not " + action
```

A future localization system may require different word order.

## Placeholder readiness

When placeholders are introduced, they should represent complete values.

Example concept:

```text
The file '{fileName}' could not be opened.
```

Avoid using placeholders as grammatical fragments.

Placeholder safety should be validated by the localization tooling.

## Tone

Recommended tone:

```text
clear
neutral
factual
helpful
```

Avoid:

```text
sarcastic
accusatory
panicked
overly apologetic
```

The application may already be failing. The message should not add theatre.

## Security considerations

Error text can accidentally reveal:

* filesystem structure,
* usernames,
* service topology,
* internal hostnames,
* SQL,
* secrets,
* authentication state,
* customer information.

Default catalog text should remain safe for its intended display context.

Sensitive occurrence details should be controlled separately.

## Review checklist for title

* Is it short?
* Is it specific?
* Is it understandable?
* Does it avoid implementation details?
* Does it avoid a final period?
* Is it distinct from the full message?
* Does it still match the symbolic error identity?

## Review checklist for message

* Is it a complete sentence?
* Does it state what is known?
* Does it avoid blaming the user?
* Does it avoid unsupported assumptions?
* Is it reusable?
* Is it safe to display?
* Does it avoid secrets and local paths?
* Does it end with appropriate punctuation?

## Review checklist for developer hint

* Is it actionable?
* Does it suggest plausible checks?
* Does it avoid claiming an unproven cause?
* Is it useful to developers or operators?
* Is it concise enough for the catalog?
* Does detailed material belong in documentation instead?
* Is it free of secrets?

## Review checklist for severity

* Did the requested operation fail?
* Did automatic recovery succeed?
* Can the application continue safely?
* Is data integrity at risk?
* Is urgent operator action required?
* Is the value one of the supported canonical severities?

## Review checklist for documentation key

* Is it stable?
* Is it predictable?
* Is it independent of one machine?
* Does it avoid temporary hostnames?
* Does it avoid secrets?
* Does it point to documentation for the same error?
* Does it follow the project hierarchy?

## Current Setter limitations

Current edit commands do not yet provide:

* interactive text editing,
* localization variants,
* placeholder validation,
* explicit clearing of optional fields,
* multiline editor integration,
* semantic comparison,
* spelling or grammar checking,
* automatic documentation-key creation,
* batch text editing.

These are future possibilities, not current guarantees.

## Suggested smoke test

Create a disposable workspace:

```bash
rm -rf /tmp/when-it-fails-text-test

mkdir -p /tmp/when-it-fails-text-test

dotnet run \
  --project Toolroom/WhenItFails/Setter \
  -- init /tmp/when-it-fails-text-test
```

Inspect:

```bash
dotnet run \
  --project Toolroom/WhenItFails/Setter \
  -- details \
  /tmp/when-it-fails-text-test \
  AFW_NET_0001
```

Edit the message:

```bash
dotnet run \
  --project Toolroom/WhenItFails/Setter \
  -- set-message \
  /tmp/when-it-fails-text-test \
  AFW_NET_0001 \
  "The application could not reach the remote service."
```

Edit the hint:

```bash
dotnet run \
  --project Toolroom/WhenItFails/Setter \
  -- set-developer-hint \
  /tmp/when-it-fails-text-test \
  AFW_NET_0001 \
  "Check the endpoint, DNS, proxy, VPN, firewall, and service availability."
```

Validate:

```bash
dotnet run \
  --project Toolroom/WhenItFails/Setter \
  -- validate /tmp/when-it-fails-text-test
```

Inspect again:

```bash
dotnet run \
  --project Toolroom/WhenItFails/Setter \
  -- details \
  /tmp/when-it-fails-text-test \
  AFW_NET_0001
```

Clean up:

```bash
rm -rf /tmp/when-it-fails-text-test
```

## Related documentation

* [Editing Error Fields](../Editing%20Error%20Fields/en.md)
* [Setting Title](../Setting%20Title/en.md)
* [Inspecting Error Details](../Inspecting%20Error%20Details/en.md)
* [Safe Writes](../Safe%20Writes/en.md)
* [Validation](../Validation/en.md)
* [Browsing and Filtering Errors](../Browsing%20and%20Filtering%20Errors/en.md)

## Central principle

> The title names the problem, the message explains what happened, the hint guides investigation, the severity expresses impact, and the documentation key leads to deeper help.
