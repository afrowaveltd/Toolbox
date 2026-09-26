# WhenItFails 1.0 public API policy

Status: **candidate 1.0 policy for the maintainer-confirmed 1473/1473 GREEN source tree**.
The final cross-version freeze still requires a refreshed comparison against the exact
maintainer-held local 0.1.0 NuGet reference artifact. That artifact was never
published to a package feed.

This document defines **support level and compatibility intent**. It does not change
CLR visibility by itself and does not turn every public type into an application-level
entry point.

## Policy goals

WhenItFails 1.0 should provide a small, obvious application API while preserving the
public CLR surface already exercised by the 0.1.0 reference artifact and by current
Toolbox tooling.

The compatibility rules are:

- prefer additive evolution over renaming, removal or signature replacement;
- keep the original nine-method `IErrorCatalogRuntime` interface stable;
- add optional runtime capabilities through separate interfaces and extension methods;
- treat enum numeric values, nullable-reference annotations, generic constraints,
  `init` versus `set`, optional parameters and cancellation-token positions as
  compatibility-relevant API;
- separate CLR compatibility from persistent catalog JSON compatibility;
- do not silently reinterpret a live mutable context as an immutable snapshot;
- do not narrow an already public concrete type or constructor without a specific
  compatibility review and migration path.

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

`JsonCatalogDocumentLoader` also has a dedicated public generic loading contract and
is treated as a supported low-level loading utility.

Other public helper/normalizer classes are not automatically promoted to
application-facing API merely because they are exported. Their visibility remains
compatibility-relevant, but new application code should prefer documented interfaces
or utilities.

## 7. Public concrete implementations

Default concrete implementations such as `ErrorCatalogRuntime`,
`ErrorCatalogContextStore`, `ErrorCatalogContextProvider`, `ErrorCatalogFactory`,
`ErrorCatalog`, `JsonsBootstrapper`, default loaders/providers/validators,
normalizers and resolver/factory classes remain real public CLR APIs.

For 1.0 policy they are classified as **public implementation surface** unless a
section above explicitly promotes the type to core, advanced infrastructure or
supported utility API.

Rules for this category:

- existing public visibility and already relied-upon constructors are not narrowed
  casually;
- interfaces remain the preferred dependency boundary for new application code;
- constructor dependencies and implementation details may evolve only through a
  compatibility review;
- an implementation-surface classification is not permission to make a breaking
  change inside 1.x.

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

### Binary compatibility

The project maintains targeted binary smoke coverage against the original 0.1.0
reference consumer. A fresh complete API comparison against the exact original 0.1.0
`.nupkg` is pending until that maintainer-held Windows artifact is available again.

## 9. 1.0 freeze gate

Before declaring the 1.0 public surface frozen:

1. rerun the current exported-assembly inventory;
2. rerun the source-versus-exact-0.1.0 comparer using the original local artifact;
3. require zero unexplained package-only/removal entries;
4. compare against the freshly confirmed current-source baseline of 148 exported types / 830 API entries and review any later source-only additions;
5. confirm the complete library suite remains GREEN;
6. update this policy and the implementation status with the final artifact hashes and
   comparison counts.

Until that artifact refresh is available, development may continue as long as changes
respect the policy above and do not depend on inventing or repacking a replacement
0.1.0 baseline.
