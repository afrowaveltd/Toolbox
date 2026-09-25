# Initialization and recovery

The WhenItFails runtime loads, validates, and activates a complete error catalog context.

The active context contains the project error definitions together with supporting category, code-group, owner, and profile catalogs.

Initialization is intentionally separated from catalog repair. Runtime code may recover from invalid project catalogs, but it must never silently rewrite user-managed JSON files.

## Initialization pipeline

A normal initialization follows this sequence:

```text
Bootstrap project workspace
→ load project catalogs
→ normalize documents
→ validate catalogs
→ create runtime context
→ activate context
→ record runtime status
```

A context becomes active only after the complete initialization pipeline succeeds.

Partial or invalid contexts are never published.

A project workspace may itself be partially prepared after an earlier bootstrap
cancellation or I/O failure. Initialization is allowed to resume that workspace:
bootstrap preserves every existing project file, creates only the missing
catalogs, then the context provider loads and cross-validates the complete
five-catalog set before publication. A focused default-initializer integration
case and a public-runtime integration case cover this recovery path; verification
is pending with an expected complete suite of **1458/1458 GREEN** (last
confirmed **1456/1456 GREEN**).

## Initialization modes

The runtime supports two modes:

```csharp
ErrorCatalogInitializationMode.Strict
ErrorCatalogInitializationMode.Flexible
```

The mode is configured through:

```csharp
WhenItFailsOptions.InitializationMode
```

The default mode is `Flexible`.

## Malformed existing catalogs in a partially prepared workspace

Bootstrap only fills missing project catalog files. If a previously published
catalog exists but contains malformed JSON, re-entering bootstrap must not
replace or repair it. The remaining missing files may be created independently,
but full context loading/validation must reject the malformed catalog before
publishing a project context.

Two strict-mode integration contracts now exercise this boundary using the
real default DI graph: a first start with no prior context must leave the
runtime uninitialized, and a failed reinitialization must keep the previous
context and recorded status reference-identical. Both cases also check byte
preservation of the malformed catalog, creation of the other four files, and
absence of staged temporary files. Both tests were confirmed locally in the
complete **1460/1460 GREEN** suite.

Two subsequent strict-mode integration tests exercise explicit repair instead
of automatic repair. A caller replaces only the malformed error catalog with
valid bytes and retries initialization. The runtime must then publish a
non-degraded project context, preserve all five project catalogs byte-for-byte
and report every file as already existing/skipped. First-start recovery and
successful reinitialization following a retained healthy context are tested
separately. Both tests were confirmed locally in the complete
**1462/1462 GREEN** suite.

An additional default-DI integration contract now exercises the flexible-mode
transition after an interrupted project bootstrap: malformed existing JSON
leads to a degraded `BuiltInFallback` on first start without replacing the
project file. After the caller restores valid project JSON and retries,
runtime must publish a fresh, non-degraded `ProjectCatalog` context,
clear all recorded recovery metadata, report every existing project file
as skipped, preserve all five files byte-for-byte, and leave no staged
temporary artifacts. The new focused test is pending local verification
(expected complete suite **1463/1463 GREEN**, last confirmed **1462/1462 GREEN**).

## Strict mode

Strict mode requires the requested project-local catalog context to initialize successfully.

```text
Project catalog succeeds
→ activate project context

Project catalog fails
→ return failure
→ do not activate bundled defaults
```

If a valid context was already active before the failed initialization attempt, that context remains stored internally.

The failed strict initialization does not replace or invalidate the previously active context or runtime status.

Strict mode is appropriate when using an unexpected catalog would be more dangerous than refusing initialization.

## Flexible mode

Flexible mode prioritizes application availability while preserving diagnostics.

Its recovery order is:

```text
1. Project catalog succeeds
   → activate project context

2. Project catalog fails
   and a previous valid context exists
   → retain previous context

3. Project catalog fails
   and no previous context exists
   → activate bundled defaults

4. Bundled defaults also fail
   → return failure
```

Flexible recovery never modifies the failed project catalog.

## Project catalog activation

A successful project initialization produces:

```text
ContextSource: ProjectCatalog
IsDegraded: false
KeptPreviousContext: false
UsedFallback: false
State: ProjectCatalog
```

No recovery metadata is present.

## Previous-context recovery

When flexible initialization fails and a valid context is already active, the runtime retains that context.

```text
ContextSource: PreviousContext
IsDegraded: true
KeptPreviousContext: true
UsedFallback: false
State: PreviousContextRecovery
```

The status also contains:

* recovery reason code,
* failure status,
* recovery message.

This state indicates that the application remains operational, but the most recent initialization request failed.

## Built-in fallback

When flexible initialization fails and no previous context exists, the runtime attempts to activate bundled defaults.

```text
ContextSource: BuiltInDefaults
IsDegraded: true
KeptPreviousContext: false
UsedFallback: true
State: BuiltInFallback
```

Bundled catalogs are loaded through an isolated temporary workspace using the normal catalog loading, normalization, validation, and context creation pipeline.

They are not copied over invalid project files.

## Explicit built-in defaults

Bundled defaults may also be activated intentionally:

```csharp
await runtime.ResetToDefaultsAsync();
```

An explicit reset is not considered recovery or degradation.

```text
ContextSource: BuiltInDefaults
IsDegraded: false
KeptPreviousContext: false
UsedFallback: false
State: BuiltInDefaults
```

Explicit reset changes only the active runtime context.

Project-local files remain unchanged.

## Activation serialization

The default `ErrorCatalogRuntime` serializes `InitializeAsync` (both overloads)
and `ResetToDefaultsAsync` with one instance-local asynchronous gate. A queued
activation waits until the preceding operation completes, fails or is
cancelled; cancelling a queued request prevents that request from invoking
its initializer or built-in provider. The gate is released in `finally`,
including when cancellation propagates. The gate is not reentrant: do not
invoke an awaited activation recursively from its initializer or provider.

Two further in-memory gate contract cases cover both **opposite-operation
queued cancellations**: a cancelled reset behind an in-flight initialization
does not enter its built-in provider; a cancelled initialization behind an
in-flight reset does not enter its initializer. In the former, an already
active project context and its recorded status/sequence remain unchanged
until the leading initializer finishes. In the latter, only the leading
reset may publish. These two additional cases were confirmed locally by the maintainer in the
complete **1447/1447 GREEN** suite. They are specific to the current gated default runtime, not the
original 0.1.0 runtime, and use in-memory fakes rather than project JSON I/O.

The gate does **not** block ordinary readers, direct external store writes,
or operations on other runtime instances that share the same store. It does
not make context publication and runtime status one atomic write: the context
can still be observed after `Set` and before its status is recorded. See
[completed activation status observations](../Activation-Status/en.md) for
the distinct runtime-local status sequence, publication generation and
remaining consistency limits.

## Runtime status

The active runtime status is available through:

```csharp
Response<ErrorCatalogRuntimeStatus> response =
    runtime.GetStatus();
```

The status snapshot contains:

```text
ContextSource
State
IsConsistent
IsDegraded
KeptPreviousContext
UsedFallback
RecoveryReasonCode
RecoveryStatus
RecoveryMessage
ActivatedAtUtc
PackageDirectoryPath
```

The snapshot is stored atomically after successful context activation.

A failed initialization or reset does not overwrite the previous valid status.

## Status consistency

The runtime validates status combinations before publishing them.

Valid normal states must not contain recovery metadata.

Valid recovery states must contain complete recovery metadata:

```text
RecoveryReasonCode
RecoveryStatus
RecoveryMessage
```

Examples of invalid combinations include:

```text
ProjectCatalog + IsDegraded
ProjectCatalog + UsedFallback
BuiltInDefaults fallback without IsDegraded
Recovery state without recovery reason
Normal state containing recovery details
```

An inconsistent status is rejected before it can replace the active status snapshot.

## Hidden recoverable failures

The option:

```csharp
WhenItFailsOptions.HideRecoverableFailures
```

may suppress recoverable failures from the normal public result flow.

It may apply only when the runtime successfully recovers by:

* retaining a valid previous context, or
* activating a valid bundled fallback context.

It must never hide:

* unrecoverable failures,
* cancellation,
* invalid API usage,
* fatal runtime failures.

Recovery diagnostics remain available through response metadata and runtime status even when the recoverable failure is hidden.

## File safety

Runtime initialization follows these rules:

```text
Create missing project files when appropriate
Never overwrite existing project files automatically
Never repair invalid files silently
Never replace project catalogs during fallback
Never delete user-managed catalogs
```

Catalog repair and migration belong to explicit authoring and maintenance tools, not to runtime initialization.

## Design principle

The runtime follows one central rule:

> Recovery may change the active in-memory context, but it must never silently change the user's source catalogs.
