# Known Limitations

This page documents known limitations of WhenItFails Setter.

A limitation is not necessarily a bug.

Some limitations are intentional boundaries that keep Setter small, safe, and predictable. Others are possible future improvement areas.

## Main principle

Setter should be honest about what it does and what it does not do.

Known limitations should be documented so users do not mistake current behavior for a broader contract.

## Not a full catalog editor

Setter currently provides focused editing commands for selected error fields.

Current edit commands update:
in:
Setter is not currently a full visual or interactive catalog editor.

## No add-error command yet

New error definitions are currently added by editing JSON directly.

Recommended flow:
There is no current command such as:
or:
This may be added later, but it should be designed carefully because new errors affect IDs, numeric codes, owners, code groups, categories, profiles, documentation, and support workflows.

## No remove-error command yet

Setter does not currently provide a command to remove error definitions.

Removal can be a compatibility-sensitive operation.

Use manual JSON editing only after reviewing logs, docs, tests, runtime references, support references, and migration notes.

Prefer deprecation over immediate removal when the error may already be public.

## No owner/category/code-group/profile editor yet

Setter does not currently provide focused edit commands for:
These files are edited manually.

After manual edits, always run:
dotnet run \
  --project Toolroom/WhenItFails/Setter \
  -- validate .
Then review the summary and affected list/detail commands.

## Plain output is not a stable machine API

Some commands support:
Plain output is simpler than rich terminal output, but it is not currently a versioned machine-readable contract.

Do not treat plain output as stable JSON, TSV, or CSV.

For automation, rely primarily on:
and only use plain output for simple inspection unless the exact format is documented for the command.

## No JSON command output yet

Setter does not currently provide a stable JSON output mode for commands.

There is no current option such as:
A future JSON output mode should be treated as a public output schema and tested accordingly.

Until then, command output is primarily user-facing.

## Rich output is for humans

Rich output may use terminal formatting, tables, colors, or layout features.

It is intended for human reading.

Do not parse rich output in scripts.

Use exit codes for automation decisions.

Use `--plain` where available for simpler text.

## Profile browsing is simplified

The current `errors --profile` behavior uses a simplified profile filter.

It currently focuses on include-style matching for:
Current Setter browsing does not fully apply every possible runtime-style profile concept, such as:
Runtime consumers may interpret profiles more fully than Setter browsing currently does.

## Profile output is not production policy proof

Do not use:
dotnet run \
  --project Toolroom/WhenItFails/Setter \
  -- errors . --profile PRODUCTION
as the only proof that runtime production filtering is safe.

Review profile mappings, tags, runtime code, and documentation.

Production safety must be enforced by the consuming application, not merely assumed from catalog browsing.

## Init expects project root

Most commands accept either:
or:
The `init` command is different.

It expects a project root and creates or ensures:
Do not pass the package directory to `init` unless the intended result is a nested package.

## No automatic schema migration

Setter does not currently perform automatic schema migration.

Validation should not silently rewrite catalog files.

If schema migration is introduced later, it should be explicit and documented.

A future command might look like:
but no such command currently exists.

## No restore-backup command yet

Setter creates timestamped backups during safe writes, but it does not currently provide a command such as:
Recovery is manual.

Typical recovery flow:
See backup documentation before restoring.

## Safe write is not a multi-process transaction

Setter safe writes are designed to protect individual file replacement.

They are not a complete multi-process transaction system.

Avoid running multiple write commands against the same workspace at the same time.

Recommended rule:
Concurrent writers may still interfere with each other at the workflow level.

## Safe write is not multi-file atomic

A command that writes one file can protect that one file.

Setter does not currently provide a transaction covering multiple catalog files at once.

Manual edits across several files should be reviewed and validated carefully.

If a future command changes multiple files, it should define backup and rollback behavior explicitly.

## Backups are local files

Backups are created next to the target catalog file.

They are useful for local recovery.

They are not a replacement for:

- Git history,
- external backups,
- release tags,
- code review,
- artifact archives.

Do not rely on local `.bak.json` files as the only recovery strategy.

## Backup cleanup is manual

Setter does not currently provide automatic backup retention cleanup.

Over time, repeated edits can create many backup files.

Cleanup should be manual and careful.

Before deleting backups, confirm:

- active catalogs validate,
- changes are committed,
- no recovery is needed,
- no backup files are accidentally staged.

## Temporary files may remain after interruption

Safe writing uses temporary files.

If the process is interrupted at the wrong time, a temporary file may remain.

Temporary files are normally identifiable by their generated temporary naming pattern.

Do not delete files blindly.

First inspect:
ls -la Jsons/WhenItFails
Then validate the workspace.

## Validation is not product policy

Validation checks catalog structure and known catalog relationships.

It does not prove every product-level policy.

For example, validation may not prove:

- every documentation key resolves to a real page,
- every user-facing message is ideal,
- every production profile is safe,
- every tag is semantically perfect,
- every code choice is the best possible one.

Validation is necessary, not sufficient.

## Documentation key targets are not fully verified

Setter can store and display:
Current validation may ensure the field shape or presence depending on rules, but it does not necessarily prove that a corresponding external documentation page exists.

Review documentation links and generated docs separately.

## No full localization workflow yet

Current catalogs are primarily English-oriented.

Files such as:
show language-specific naming.

Setter does not currently provide a full workflow for:

- adding another language,
- comparing translations,
- detecting missing translated text,
- synchronizing language-neutral fields,
- validating translation completeness.

Localization should be designed as schema and workflow evolution.

## No interactive TUI or GUI editor yet

Setter is currently a command-line tool.

There is no current interactive TUI or GUI catalog editor.

Future tools may provide:

- guided error creation,
- profile editing,
- schema migration,
- review dashboards,
- localization workflows.

Those should build on the same catalog rules.

## No remote catalog sync

Setter works with local workspace files.

It does not currently synchronize catalogs with:

- remote registries,
- package feeds,
- web services,
- central catalog servers.

Use Git and normal release processes for sharing catalog changes.

## No automatic dependency discovery

Setter does not currently scan all source code to discover every runtime reference to an error ID, code, profile, category, or mapping key.

Before renaming or removing stable values, use manual search and review.

Example:
grep -R "AFW_NET_0001" .
## No semantic duplicate detection

Setter validation may catch exact duplicate IDs or numeric codes depending on current rules.

It does not fully understand whether two error definitions mean the same thing.

Catalog authors still need review judgment.

Example:
may or may not represent the same concept.

Validation cannot decide that alone.

## No severity policy enforcement beyond allowed values

Setter can validate supported severity values.

Supported values include:
But it does not fully know whether a specific severity is perfect for a specific business context.

Severity still requires author review.

## No complete security review

Setter can help avoid obvious catalog mistakes, but it is not a full security scanner.

It does not guarantee that messages or hints never expose:

- hostnames,
- private paths,
- tokens,
- credentials,
- internal service names,
- customer identifiers.

Review user-facing and production-facing entries carefully.

## No full documentation linting

Setter does not currently lint all Markdown documentation.

It does not fully verify:

- broken relative links,
- stale examples,
- heading style,
- duplicate sections,
- missing README entries.

Use manual review and `git diff --check`.

A future documentation check command may be useful.

## No package publishing automation

Setter documentation includes release guidance, but Setter itself does not currently publish packages.

Release tasks remain external to Setter.

Use the project’s normal build, test, tag, package, and publish workflow.

## No guaranteed output ordering as an API

Human-facing lists should be predictable enough to read.

However, unless a command explicitly documents stable sorting, scripts should not depend on exact row order.

A future machine-readable output mode should define ordering if ordering matters.

## No command plug-in system

Setter commands are currently built into the tool.

There is no current plugin model for third-party commands.

A plugin model would require careful design around command discovery, safety, validation, versioning, documentation, trust, and output contracts.

## No partial validation mode

Validation currently treats the workspace as a package.

There is no broad documented mode such as:
or:
Catalog files often reference each other, so package-level validation is safer.

## No automatic formatting-only command

Setter does not currently provide a dedicated command such as:
to normalize JSON formatting.

Manual edits should preserve clean formatting.

Avoid unrelated formatting churn in commits.

## No automatic catalog sorting

Setter does not currently provide a command to sort errors, categories, owners, or profiles.

Sorting can create large diffs.

If sorting is added later, it should be explicit and documented.

## No policy for every future consumer

WhenItFails catalogs are designed to be useful across applications, but Setter cannot define every runtime behavior for every future consumer.

Consuming applications must decide how to use:

- mappings,
- profiles,
- tags,
- documentation keys,
- severity,
- developer hints.

Setter provides catalog support, not all runtime policy.

## Workarounds

Current practical workarounds:
## When to turn a limitation into a feature

A limitation may deserve a feature when:

- users hit it repeatedly,
- manual work is error-prone,
- automation needs it,
- safety would improve,
- tests can define behavior clearly,
- documentation can explain it simply.

Good future feature candidates:
Each should be designed as public behavior, not a quick shortcut.

## Review checklist

When documenting or changing a limitation, confirm:
## Related documentation

- [Architecture Overview](../Architecture%20Overview/en.md)
- [Maintainer Notes](../Maintainer%20Notes/en.md)
- [Command Quick Reference](../Command%20Quick%20Reference/en.md)
- [Exit Codes and Automation](../Exit%20Codes%20and%20Automation/en.md)
- [Backups and Recovery](../Backups%20and%20Recovery/en.md)
- [Deprecation and Migration](../Deprecation%20and%20Migration/en.md)
- [Schema Evolution](../Schema%20Evolution/en.md)
- [Catalog Author Checklist](../Catalog%20Author%20Checklist/en.md)
- [Troubleshooting](../Troubleshooting/en.md)

## Central principle

> A documented limitation is safer than an undocumented assumption.