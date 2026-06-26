# Design philosophy

WhenItFails is built around the idea that application failures should be structured, identifiable, diagnosable, and reusable.

An error is not only a text message.

It may have:

* a stable identity,
* a numeric code,
* an owner,
* a category,
* a severity,
* a user-facing explanation,
* a developer-facing hint,
* environment-specific mappings,
* runtime occurrence details,
* documentation,
* selection rules,
* recovery metadata.

When those concerns are represented only by exception messages scattered through application code, they become difficult to validate, localize, document, search, and maintain.

WhenItFails separates those concerns into explicit catalog definitions and runtime descriptors.

## Definitions and occurrences

The most important distinction in the package is:

```text
ErrorDefinition
→ describes a reusable kind of failure

ErrorDescriptor
→ describes one concrete occurrence
```

A definition answers questions such as:

```text
What is this error?
Who owns it?
What is its stable ID?
Which categories does it belong to?
What should its default message and severity be?
```

A descriptor answers questions such as:

```text
Where did it happen?
Which operation failed?
Which component reported it?
What concrete resource was involved?
Was an exception attached?
What runtime metadata belongs to this occurrence?
```

The catalog should not be modified merely because one particular operation failed.

Occurrence-specific information belongs to the descriptor.

## Stable identity before wording

Human-readable wording may evolve.

Stable error identity should not.

For that reason, WhenItFails treats these values as contracts:

```text
error ID
numeric code
symbolic name
```

Titles, messages, hints, and presentation mappings may improve over time without changing the identity of the error.

A consumer should not parse a human-readable message to determine what happened.

It should use the structured identity.

## Separation of concerns

WhenItFails deliberately keeps several concepts separate:

```text
identity
classification
ownership
numbering
presentation
selection
runtime detail
recovery state
```

These concepts are related, but they are not interchangeable.

For example:

* an owner says who controls a definition,
* a code group says what numeric and symbolic family it belongs to,
* a category says what kind of problem it represents,
* a profile says where or how it should be selected,
* a mapping says how a platform may present it,
* a descriptor says what happened this time.

Combining all of these into one unstructured message would make the system simpler only at the beginning and much harder to maintain later.

## Catalogs are project-owned data

Bundled catalogs are package resources.

Project catalogs are project-owned data.

The package may create missing project copies during bootstrap, but it must not silently replace or rewrite existing project catalogs.

This distinction protects:

* local customization,
* version-control history,
* user ownership,
* reproducibility,
* auditability,
* recovery from mistakes.

A runtime library should not behave like an invisible catalog editor.

## Runtime is not an authoring tool

The runtime is responsible for:

* loading catalogs,
* normalizing values,
* validating documents,
* validating cross-catalog relationships,
* creating an active context,
* resolving definitions,
* creating descriptors,
* exposing diagnostics,
* recovering according to explicit policy.

The runtime is not responsible for silently:

* repairing malformed JSON,
* allocating new identifiers,
* renumbering errors,
* rewriting deprecated fields,
* merging package updates into project files,
* deleting unknown definitions,
* migrating catalogs without permission.

Those responsibilities belong to explicit authoring, validation, migration, and maintenance tools.

This is the role of tools such as the WhenItFails Setter.

## Recovery without hidden mutation

WhenItFails may recover by:

* retaining a previously valid context,
* activating bundled defaults.

Recovery may change the active in-memory context.

Recovery must not silently change the source catalogs.

This gives applications a controlled way to remain operational while keeping the failed project state available for diagnosis and repair.

## Availability and correctness

Different applications require different priorities.

A development tool may prefer to continue with bundled defaults.

A long-running service may prefer to retain its previously valid context.

A security-sensitive application may require strict initialization and refuse to start when the requested project catalog is invalid.

WhenItFails therefore does not impose one universal recovery policy.

It exposes strict and flexible initialization modes and makes the active runtime state observable.

## Degraded operation must remain visible

Successful recovery does not mean that nothing went wrong.

A runtime using a previous context or bundled fallback is operational, but it is degraded relative to the requested project configuration.

That condition must remain inspectable through:

* initialization results,
* response metadata,
* runtime status,
* logging and monitoring integrations.

An option may hide a recoverable failure from the normal result flow, but it must not erase the diagnostics describing the recovery.

## Complete contexts only

The active runtime context contains multiple related catalogs.

A context must be activated as one validated unit.

The runtime must never expose a partially updated state such as:

```text
new errors
+
old categories
+
missing profiles
```

A consumer should observe either:

```text
the previous complete valid context
```

or:

```text
the new complete valid context
```

Never an intermediate mixture.

## Atomic publication

Context and status publication must happen only after successful creation and validation.

Internally inconsistent status snapshots must be rejected before publication.

This protects concurrent consumers from observing impossible combinations.

## Explicit defaults

Bundled defaults may be used in two different ways:

```text
automatic fallback
or
explicit reset
```

These are not the same state.

Automatic fallback means that the requested project initialization failed and the runtime recovered.

Explicit reset means that the caller intentionally selected the bundled defaults.

Only the first is degraded recovery.

## User-safe and developer-safe information

An error may contain information for different audiences.

User-facing information should be:

* understandable,
* relevant,
* safe to disclose,
* free from secrets and internal implementation details.

Developer-facing information may contain:

* operation names,
* component names,
* source names,
* diagnostic details,
* remediation hints,
* attached exceptions.

The package stores these concerns separately so presentation layers can decide what is appropriate for the current environment and audience.

## Exceptions are evidence, not identity

An exception may accompany an error occurrence.

It is not the stable identity of the error.

Different low-level exceptions may represent the same catalog error.

The same exception type may also represent different logical failures depending on context.

WhenItFails therefore allows an exception to be attached to a descriptor without making exception types the primary catalog model.

Exceptions are excluded from ordinary descriptor JSON serialization because they are runtime objects and may contain sensitive information.

## Profiles select; they do not duplicate

Profiles provide reusable views over the active catalog.

A profile may select definitions by:

* owner,
* code group,
* category,
* subcategory,
* tag.

A profile should not copy full error definitions.

Duplicating definitions inside profiles would create multiple sources of truth and make updates inconsistent.

## Mappings are extensible policy

Mappings allow platform-specific or environment-specific behavior without adding a dedicated property for every possible integration.

Examples include:

```text
web.httpStatusCode
web.problemDetails
cli.includeExitCode
desktop.showDialog
service.includeRetryInformation
```

Mappings should describe presentation or integration policy.

They should not replace core identity fields that deserve explicit validation.

## Normalization is assistance, not repair

Normalization helps the runtime handle canonical forms consistently.

It may normalize values such as:

* symbolic names,
* aliases,
* tags,
* prefixes,
* optional text,
* empty collections.

Normalization must not disguise structurally invalid or contradictory catalog data.

Authors should still write clear canonical values.

Validation remains responsible for deciding whether the resulting context is acceptable.

## Validation protects contracts

Validation is not merely defensive parsing.

It protects contracts between:

* definitions,
* owners,
* code groups,
* categories,
* profiles,
* consumers,
* external systems.

A duplicate numeric code or unresolved owner is not just untidy data.

It can make diagnostics ambiguous and integrations unreliable.

For that reason, invalid complete contexts are not activated.

## Portability

Catalogs use JSON so they can be:

* stored in version control,
* reviewed through ordinary diffs,
* copied between projects,
* generated by tools,
* validated in CI,
* exported and imported,
* inspected without proprietary software.

Project-specific profiles and definitions should remain portable whenever possible.

## Local customization without package forks

Projects should be able to customize error catalogs without modifying or forking the WhenItFails package.

The package supplies:

* runtime behavior,
* validation rules,
* bundled templates,
* public contracts,
* maintenance tooling.

The project supplies:

* its own definitions,
* its own owners,
* its own profiles,
* its own mappings,
* its own presentation policy.

This separation allows package updates without discarding project-specific work.

## Predictability over cleverness

WhenItFails favors explicit behavior over hidden convenience.

Examples include:

```text
explicit initialization
explicit recovery modes
explicit active context
explicit runtime status
explicit reset to defaults
explicit authoring tools
```

A caller should be able to determine:

* what context is active,
* where it came from,
* whether recovery occurred,
* why recovery occurred,
* whether project files were changed.

Hidden magic may initially reduce code, but it makes failures harder to trust.

## Main design rules

Future development should preserve these rules:

1. Never use human-readable messages as stable identity.
2. Never publish partially initialized contexts.
3. Never overwrite project catalogs silently.
4. Never hide unrecoverable failures.
5. Never describe fallback as normal project activation.
6. Never treat a runtime occurrence as a catalog definition.
7. Never expose attached exceptions blindly to users.
8. Never duplicate definitions inside profiles.
9. Never let arbitrary mappings replace validated core fields.
10. Never publish an internally inconsistent runtime status.
11. Keep authoring and runtime responsibilities separate.
12. Keep recovery observable.
13. Prefer explicit policy over surprising automation.
14. Preserve stable IDs and numeric codes after publication.
15. Treat project-local catalogs as user-owned data.

## Central principle

The package can be summarized by one rule:

> Failures should be structured enough for machines, understandable enough for people, and explicit enough to remain trustworthy.

