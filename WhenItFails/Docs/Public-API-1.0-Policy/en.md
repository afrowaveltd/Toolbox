# WhenItFails 1.0 public API policy

Status: **first-stable-release policy under final surface cleanup; first 17-type batch verified, second 5-type helper batch committed and verification pending**.

WhenItFails has not been published and has no external consumers. The maintainer-held
local 0.1.0 artifact is therefore historical test evidence, **not a compatibility
target or release gate**. The 1.0 surface may still make deliberate breaking cleanup
changes until the final 1.0 baseline is verified and published.

This document defines **support level and compatibility intent**. It does not change
CLR visibility by itself and does not turn every public type into an application-level
entry point.

## Policy goals

WhenItFails 1.0 should provide a small, obvious application API and avoid freezing
default implementation details that consumers can already replace through interfaces.

Before the first stable publication:

- breaking cleanup is allowed when it reduces accidental public surface or fixes a
  design contract;
- the nine-method `IErrorCatalogRuntime` remains the intended core application facade;
- optional runtime capabilities remain separate interfaces and extension methods;
- enum numeric values, nullable-reference annotations, generic constraints,
  `init` versus `set`, optional parameters and cancellation-token positions are
  compatibility-relevant **once the 1.0 baseline is frozen**;
- persistent catalog JSON compatibility remains separate from CLR compatibility;
- a live mutable context must never be described as an immutable snapshot;
- default orchestration implementations should be internal unless direct construction
  is an intentional supported/tooling contract.

## 1. Core application API

The following surface is the recommended entry point for normal application code and
is intended to be a stable 1.x contract.

### Registration and runtime

- `WhenItFailsServiceCollectionExtensions.AddWhenItFails(...)` — all four existing
  overloads;
- `IErrorCatalogRuntime` — the existing nine declared methods;
- `IErrorCatalog` — read-only lookup/query abstraction.

Adding a method directly to `IErrorCatalogRuntime` is treated as a breaking design
decision because third-party implementations may exist. New optional capabilities
should use a separate interface or extension method instead.

### Configuration

- `WhenItFailsOptions`;
- `JsonsOptions`;
- `ErrorCatalogInitializationMode`.

### Main application models

- `ErrorDefinition`;
- `ErrorDescriptor`;
- `ErrorDescriptor<TAttachment>`;
- `ErrorDescriptorRequest`;
- `ErrorCatalogContext`;
- `ErrorCatalogRuntimeStatus`;
- `ErrorCatalogInitializationPayload`;
- `JsonsBootstrapPayload`;
- `JsonsBootstrapFileResult`;
- `ErrorCatalogContextSource`;
- `ErrorCatalogRuntimeState`.

`ErrorCatalogContext` remains a **live shared mutable context reference** for
compatibility with the existing runtime contract. Application code should treat it as
read-only. Detached snapshot APIs exist when ownership isolation is required.

An empty manually constructed initialization payload is not equivalent to a completed
runtime initialization. Successful runtime-produced payloads require valid bootstrap
and context data according to the runtime contracts.

## 2. Supporting and transitive public contracts

Types that appear in supported public signatures are compatibility-relevant even when
applications normally reach them indirectly.

This includes:

- the five catalog document models;
- category, owner, code-group and profile definition models;
- provider payload models;
- `ErrorCatalogValidationResult`;
- `ErrorCatalogValidationIssue`;
- `ErrorCatalogValidationSeverity`;
- `JsonsTemplateFile`.

Their CLR shape, nullability and documented defaults are part of the supported surface
where already covered by focused public-API contracts.

This **does not automatically freeze every JSON serialization detail as a wire
protocol**. Persistent project catalog JSON is versioned and documented separately.
Snapshot DTO JSON is not a persistent catalog schema unless a dedicated document
explicitly says otherwise.

## 3. Supported DI extension points

The public interfaces registered through `AddWhenItFails()` and covered by
pre-registration precedence tests are supported replacement seams.

This includes bootstrap/template, initializer/context, loader/provider/validator,
normalizer, catalog factory, descriptor and profile-resolution interfaces.

Support means:

- the public interface signature is compatibility-relevant;
- default registration continues to respect a compatible earlier registration where
  the current `TryAddSingleton` contract applies;
- cancellation and nullability annotations in the interface remain meaningful;
- third-party implementations are responsible for preserving the semantic contract
  expected by the runtime.

Support does **not** mean every custom implementation automatically inherits the
default implementation's recovery, ownership, safe-write or validation guarantees.

## 4. Advanced observation and snapshot API

The additive snapshot and observation surface is a supported **advanced** API. It is
separate from the nine-method core runtime interface so custom runtime
implementations are not forced to implement every observation capability.

### Optional runtime readers

- `IErrorCatalogRuntimePublicationReader`;
- `IErrorCatalogRuntimeActivationReader`;
- `IErrorCatalogRuntimeCombinedObservationReader`;
- `IErrorCatalogRuntimeSupportingObservationReader`;
- `IErrorCatalogRuntimeFullObservationReader`.

Consumers must feature-detect optional interfaces and handle `NotSupported` or other
non-success responses.

### Detached snapshot models and extensions

The detached definition, validation, category, owner, code-group, profile,
supporting-catalog, combined, published and completed snapshot models and their public
extension methods are supported advanced CLR contracts.

These snapshots are designed for detached operational observation. They are not
promises of:

- a raw clone of all source JSON;
- a globally atomic transaction against external writers or in-place mutation;
- a versioned JSON wire format unless explicitly documented;
- implementation of optional readers by every custom `IErrorCatalogRuntime`.

## 5. Publication infrastructure API

The following surface is supported for infrastructure code rather than ordinary
application logic:

- `IErrorCatalogContextPublicationReader`;
- `IErrorCatalogContextPublisher`;
- `ErrorCatalogContextPublication`;
- `ErrorCatalogActivationStatusSnapshot`;
- the corresponding optional methods implemented by the default
  `ErrorCatalogContextStore` and `ErrorCatalogRuntime`.

Publication identity is store-scoped. `Generation` identifies successful store
publication; `ActivationSequence` identifies matching completed runtime observations.
They are not interchangeable.

`ErrorCatalogContextPublication.Context` is still a live mutable context reference.
The publication record establishes identity/ownership of a write, not deep
immutability.

## 6. Supported standalone utilities

The following existing public utilities have verified repository consumers and focused
public-shape contracts and are treated as supported tooling/library utilities:

- `JsonCatalogDocumentWriter`;
- `DocumentationKeyGenerator`;
- `DocumentationKeyFormat`;
- `ErrorCatalogCrossValidator`.

They may be used directly without DI where documented.

The shared generic `JsonCatalogDocumentLoader` is an internal implementation helper.
Consumers use the five typed loader interfaces/classes instead of depending on that
generic helper directly.

Other exported helper/normalizer classes are not automatically promoted to
application-facing API merely because they are visible. Types used directly by
Toolbox tooling remain public only when that direct construction is intentional.

## 7. Default implementation visibility

Default runtime orchestration implementations are **not** part of the intended 1.0
consumer API. Their public interfaces remain the extension boundary while the default
classes may be internal. Commit `6afd3f6f88c7d71097fd1813d8e0dc2d9d80b276`
applies the first visibility batch to 17 DI-only implementation classes and is
maintainer-confirmed in the **1474/1474 GREEN** suite. Commit
`147b96310090c6cbbb5f2a5819b440026fb6062b` applies a second cleanup to the
generic JSON document loader plus four definition normalizers and hides their
injection-only constructors from the exported surface; verification of that second
batch is pending.

The final cleanup explicitly targets DI-only implementation classes such as the
default runtime, context store/provider, initializer, catalog providers/factory,
descriptor services/factories/resolvers and profile-resolution services.

Low-level concrete types that are directly used by Toolbox tooling remain public only
when that direct construction is intentional. In particular the Setter currently
depends on selected JSON loaders/normalizers plus the standalone utilities listed
above; those are reviewed separately rather than hidden in bulk.

After 1.0 is published, changing an exported type's accessibility is a breaking change
and must follow normal major-version rules.

## 8. Compatibility dimensions

### CLR/source compatibility

For stable and supported categories, the project treats the following as relevant:

- type/member names and accessibility;
- base type and implemented public interfaces;
- parameter and return types;
- generic constraints;
- optional parameters and default values;
- nullable-reference annotations where contract tests exist;
- getter/setter/`init` shape;
- enum underlying values.

### Behavioral compatibility

Documented response statuses, cancellation boundaries, context publication ownership,
safe-write behavior and recovery semantics are behavioral contracts where dedicated
tests exist.

Exact human-readable message wording is not automatically a permanent compatibility
contract unless a focused contract explicitly requires it.

### Persistent JSON compatibility

Project-local WhenItFails catalog documents are a persisted configuration format and
must be evolved deliberately.

The stable 1.0 catalog-schema baseline is `schemaVersion: "1.0"` across all five
catalog families. Default validators reject another non-empty version with
`UnsupportedSchemaVersion`; runtime initialization does not infer compatibility or
silently migrate it. Adding support for a later schema version requires an explicit
compatibility/migration decision rather than accepting arbitrary version text.

Detached runtime snapshot DTOs are CLR projections. Their public CLR shape does not
by itself establish a versioned wire format.

### Historical 0.1.0 diagnostics

The original local 0.1.0 package may still be used for historical comparison or
binary-smoke diagnostics. Because it was never published or consumed externally, its
surface does not constrain the first stable 1.0 release. No final-release decision
depends on reproducing that artifact.

## 9. 1.0 freeze gate

Before declaring the 1.0 public surface frozen:

1. complete the intentional visibility cleanup of default implementation classes;
2. rerun the source-only comparer and exported-assembly inventory and record the new
   stable 1.0 counts;
3. confirm the complete WhenItFails suite, Essentials suite, candidate package smoke,
   external-consumer smoke and documentation validation are GREEN;
4. review every remaining exported type as core API, extension point, transitive model,
   snapshot/observation contract or intentional standalone/tooling utility;
5. finalize package version/release notes and the supported Essentials dependency
   policy;
6. pack the exact final `1.0.0` artifact, record hashes, restore it into a clean
   external consumer, and only then publish/tag that exact commit.

The historical 0.1.0 artifact is optional diagnostic evidence and is not part of this
freeze gate.
