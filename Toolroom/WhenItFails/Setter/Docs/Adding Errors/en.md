# Adding errors

Use `add-error` to create one complete error definition safely inside a validated WhenItFails workspace.

## Syntax

```text
add-error <path> <owner> <group> <category> <name> <title> <message> [severity] [--json]
```

The path may point to the project root or directly to `Jsons/WhenItFails`.

Quote values that contain spaces.

## Example

```powershell
dotnet run --project Toolroom/WhenItFails/Setter -- \
  add-error . AFW NETWORK NETWORK \
  "Connection interrupted" \
  "Connection interrupted" \
  "The remote connection was interrupted." \
  Warning
```

## Values resolved by Setter

Setter resolves and validates:

- the owner by name or alias,
- the code group by name or prefix,
- the primary category by name or alias,
- the next available numeric code and structured ID,
- the normalized machine-friendly error name,
- canonical severity casing.

The command refuses to save when any required reference is unknown, an identity is already in use, the name already exists, or the resulting catalog is invalid.

## Generated documentation key

`add-error` automatically creates a documentation key using this shape:

```text
when-it-fails/errors/<primary-category>/<title-slug>
```

For example:

```text
when-it-fails/errors/network/connection-interrupted
```

The generated key is:

- lowercase,
- slash-separated,
- kebab-case inside each segment,
- checked for canonical format,
- checked for uniqueness before the catalog is saved.

The normal success output prints the generated key so the author immediately knows where the corresponding extended documentation belongs.

## Previewing a documentation key

Use `suggest-doc-key` when you want to see the key before creating an error:

```text
suggest-doc-key <path> <category-name|alias> <title> [--plain|--json]
```

Example:

```powershell
dotnet run --project Toolroom/WhenItFails/Setter -- \
  suggest-doc-key . NETWORK "Connection interrupted"
```

The command resolves the category against the active workspace, reads existing documentation keys, and returns the first available canonical value.

If the base key already exists, Setter adds the first free numeric suffix:

```text
when-it-fails/errors/network/connection-interrupted
when-it-fails/errors/network/connection-interrupted-2
when-it-fails/errors/network/connection-interrupted-3
```

The command is read-only. It does not change `errors.en.json` and does not create a backup.

Use `--plain` when a script needs only the key:

```powershell
dotnet run --project Toolroom/WhenItFails/Setter -- \
  suggest-doc-key . NETWORK "Connection interrupted" --plain
```

Use `--json` for the standard structured command envelope. The `data` object always has the same fields:

```text
category
title
documentationKey
failureCode
failureMessage
```

On success, `category`, `title`, and `documentationKey` are populated while `failureCode` and `failureMessage` are `null`.

On failure, `documentationKey` is `null` and the failure fields describe the problem. For example, an unknown category returns:

```json
{
  "category": "DOES_NOT_EXIST",
  "title": "Connection interrupted",
  "documentationKey": null,
  "failureCode": "CategoryNotFound",
  "failureMessage": "Category 'DOES_NOT_EXIST' was not found."
}
```

The command returns exit code `0` for a successful suggestion, `1` for invalid command arguments, and `2` when the workspace lookup or suggestion fails.

## Normal success output

A successful command prints the new definition identity and its generated documentation key:

```text
Error definition added
Id: AFW_NET_0005
Code: 600005
Name: CONNECTION_INTERRUPTED
Owner: AFW
Code group: NETWORK
Primary category: NETWORK
Severity: Warning
Documentation key: when-it-fails/errors/network/connection-interrupted
```

## JSON output

Add `--json` for a stable machine-readable envelope:

```powershell
dotnet run --project Toolroom/WhenItFails/Setter -- \
  add-error . AFW NETWORK NETWORK \
  "Connection interrupted" \
  "Connection interrupted" \
  "The remote connection was interrupted." \
  Warning --json
```

On success, `data.error` contains the complete saved error definition, including `documentationKey`.

On failure, the envelope contains:

```text
added: false
error: null
failureCode
failureMessage
```

## Safety guarantees

Before writing, Setter validates the request, resolves all referenced catalogs, computes the next identity, generates the documentation key, and validates the complete edited error catalog.

A rejected operation does not save the catalog and does not create a backup.

A successful operation creates the normal catalog backup before replacing `errors.en.json`.

## Follow-up checks

After adding errors, repository or CI checks may run:

```powershell
dotnet run --project Toolroom/WhenItFails/Setter -- check-doc-keys .
```

This verifies that every error still has a non-empty, unique, canonical documentation key.
