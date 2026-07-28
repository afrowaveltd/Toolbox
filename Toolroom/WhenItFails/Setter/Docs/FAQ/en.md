# FAQ

This page answers common questions about WhenItFails Setter.

For complete command syntax, see [Commands](../Commands/en.md). For the shortest working flow, see [Getting Started](../Getting-Started/en.md).

## What is Setter?

Setter is a command-line authoring and maintenance tool for WhenItFails JSON catalogs.

It can initialize and validate workspaces, browse reference catalogs, inspect and edit error definitions, manage profile selectors and mappings, create and restore backups, and check documentation keys and local Markdown links.

Setter is not the runtime failure-handling library.

## Where is a WhenItFails workspace?

A normal workspace is stored under:

```text
Jsons/WhenItFails
```

Most commands accept either the project root or the package directory. The `init` command expects the project root.

## How do I start?

From the repository root:

```powershell
dotnet run --project Toolroom/WhenItFails/Setter -- validate .
dotnet run --project Toolroom/WhenItFails/Setter -- summary .
dotnet run --project Toolroom/WhenItFails/Setter -- reference .
```

For a new workspace:

```powershell
dotnet run --project Toolroom/WhenItFails/Setter -- init .
```

## How do I list and inspect errors?

List errors:

```powershell
dotnet run --project Toolroom/WhenItFails/Setter -- errors .
```

Inspect one error by stable ID, numeric code, or symbolic name:

```powershell
dotnet run --project Toolroom/WhenItFails/Setter -- details . AFW_NET_0001
```

Use stable identifiers for scripts and maintenance work. Titles and messages are human-facing text and may change.

## Can I filter the error list?

Yes. Common filters include:

```text
--owner <value>
--group <value>
--code-group <value>
--category <value>
--severity <value>
--profile <value>
--search <text>
```

Filters combine with logical AND.

## Can Setter add a new error?

Yes. First inspect the reference catalogs and obtain safe candidate values:

```powershell
dotnet run --project Toolroom/WhenItFails/Setter -- reference .
dotnet run --project Toolroom/WhenItFails/Setter -- next-code . NETWORK
dotnet run --project Toolroom/WhenItFails/Setter -- suggest-doc-key . NETWORK "Network unavailable"
```

Then use `add-error` with the required explicit values. Setter validates the workspace and protects the write with a backup.

After creation, inspect the result:

```powershell
dotnet run --project Toolroom/WhenItFails/Setter -- details . NEW_ERROR_ID
```

## Can Setter remove an error?

Yes, through `remove-error`.

Removal is compatibility-sensitive. Before removing a stable error, inspect references:

```powershell
dotnet run --project Toolroom/WhenItFails/Setter -- error-references . AFW_NET_0001
```

Search the repository as well, review migration impact, and prefer deprecation when external consumers may already use the error.

## Which error fields can Setter edit?

Setter has focused commands for error text, classification, ownership, identifiers, tags, and aliases. These include:

```text
set-title
set-message
set-developer-hint
set-severity
set-documentation-key
set-name
set-subcategory
set-owner
set-code-group
set-primary-category
add-error-tag
remove-error-tag
add-error-alias
remove-error-alias
```

Use the detailed command reference for exact argument shapes.

## Can Setter browse reference catalogs?

Yes. Setter can list and show owners, categories, code groups, and profiles. It can also display a combined workspace reference overview.

Examples:

```powershell
dotnet run --project Toolroom/WhenItFails/Setter -- list-owners .
dotnet run --project Toolroom/WhenItFails/Setter -- show-category . NETWORK
dotnet run --project Toolroom/WhenItFails/Setter -- show-code-group . NETWORK
dotnet run --project Toolroom/WhenItFails/Setter -- show-profile . WEB
```

## Can Setter edit profiles and mappings?

Yes. Setter supports profile metadata, include and exclude selectors, default mappings, mapping entries, and workspace metadata.

Use `explain-profile` to understand why errors are included or excluded:

```powershell
dotnet run --project Toolroom/WhenItFails/Setter -- explain-profile . WEB
```

Profile output helps with authoring and diagnostics, but the consuming application remains responsible for enforcing runtime policy.

## Does Setter create backups?

Yes. Safe write commands create timestamped local backups before replacing an active catalog file.

Backups reduce recovery risk, but they do not replace Git history, external backups, release tags, or code review.

## How do I list or restore backups?

List available backups:

```powershell
dotnet run --project Toolroom/WhenItFails/Setter -- list-backups .
```

Restore a selected backup:

```powershell
dotnet run --project Toolroom/WhenItFails/Setter -- restore-backup . <backup-file>
```

Review the selected file carefully. Restoration is a write operation and should be followed by validation and diff inspection.

## Does Setter delete old backups automatically?

No. Backup retention and cleanup remain deliberate manual responsibilities.

Before deleting backups, confirm that the active workspace validates, the desired changes are committed, and no recovery is needed.

## Does Setter support JSON output?

Yes. Commands that support machine-readable output accept `--json`; uppercase `--JSON` is also recognized where documented and tested.

Use JSON output for automation when available. Treat its schema as a public contract and avoid parsing rich terminal output.

`--plain` provides simpler human-readable text, not a universal JSON, CSV, or TSV API.

## What are the exit codes?

The general model is:

```text
0 = success
1 = missing or invalid command input
2 = validation, lookup, editing, backup, save, or operation failure
3 = unexpected top-level application failure
```

A valid query that returns no matching rows may still succeed with exit code `0`.

## Does Setter verify documentation?

Setter provides two complementary checks:

```powershell
dotnet run --project Toolroom/WhenItFails/Setter -- check-doc-keys .
dotnet run --project Toolroom/WhenItFails/Setter -- check-doc-links .
```

`check-doc-keys` checks documentation-key presence, uniqueness, and canonical format according to current catalog rules.

`check-doc-links` checks local Markdown links. It does not verify arbitrary remote URLs or prove the quality of every document.

## Does validation prove the catalog is perfect?

No. Validation checks schema and known relationships. It cannot decide whether every message is ideal, every severity is the best business choice, every profile is safe for every application, or two differently named errors are semantically duplicates.

Validation is necessary, not sufficient.

## Can I run multiple write commands at the same time?

Avoid concurrent writers against the same workspace.

Safe writes protect individual file replacement, but Setter is not a multi-process locking system or a multi-file transaction manager.

## Does Setter migrate old schemas automatically?

No. Validation does not silently rewrite catalog structures.

Any future migration workflow should be explicit, versioned, backed up, validated before and after, and documented.

## Does Setter provide a GUI or interactive TUI?

No. Setter is currently a command-line tool.

A future interface may build on the same catalog rules, but the CLI remains the authoritative authoring surface today.

## Does Setter provide full localization management?

Not yet. Current catalogs and documentation are primarily English-oriented.

A complete localization workflow would need translation completeness checks, language-neutral field rules, fallback behavior, stale-translation detection, and synchronization across localized files.

## What makes a good error message?

A good user-facing message is clear, neutral, safe, reusable, and does not claim an unproven cause. Do not expose credentials, tokens, stack traces, private paths, internal hostnames, customer identifiers, or raw SQL.

Developer hints may contain deeper troubleshooting guidance, but they must remain safe and actionable.

## Can I reuse an old numeric code?

Avoid it. Old codes may remain in logs, tickets, telemetry, dashboards, integrations, or released applications. Reusing a code for a different meaning corrupts historical interpretation.

## What should I run before committing?

At minimum:

```powershell
dotnet test Toolroom/WhenItFails/Setter.Tests
dotnet run --project Toolroom/WhenItFails/Setter -- validate .
dotnet run --project Toolroom/WhenItFails/Setter -- check-doc-keys .
dotnet run --project Toolroom/WhenItFails/Setter -- check-doc-links .
git diff --check
git status --short
git diff
```

Keep each commit focused and review generated backup files before staging.

## Where should I read next?

- [Getting Started](../Getting-Started/en.md)
- [Commands](../Commands/en.md)
- [Command Quick Reference](../Command%20Quick%20Reference/en.md)
- [Catalog Author Checklist](../Catalog%20Author%20Checklist/en.md)
- [Backups and Recovery](../Backups%20and%20Recovery/en.md)
- [Known Limitations](../Known%20Limitations/en.md)
- [Roadmap and Future Work](../Roadmap%20and%20Future%20Work/en.md)

## Central principle

> Validate first, make explicit changes, inspect the result, test it, and preserve a recoverable history.
