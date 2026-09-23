# ASP.NET Core Problem Details integration plan

## Purpose

WhenItFails should integrate cleanly with ASP.NET Core HTTP APIs without introducing ASP.NET Core dependencies into the core `Afrowave.Toolbox.WhenItFails` package.

ASP.NET Core uses `ProblemDetails` and `ValidationProblemDetails` as standard machine-readable HTTP error payloads. WhenItFails already provides stable error identity, titles, messages, severity, documentation keys, metadata, profiles and web-oriented mappings such as:

```text
web.problemDetails
web.httpStatusCode
web.includeTraceId
web.includeExceptionDetails
web.includeStackTrace
```

The missing piece is an explicit adapter between the WhenItFails runtime model and ASP.NET Core.

## Packaging

Preferred design:

```text
Afrowave.Toolbox.WhenItFails
    no ASP.NET Core dependency

Afrowave.Toolbox.WhenItFails.AspNetCore
    references WhenItFails
    references ASP.NET Core
    contains HTTP-specific adapters and registration helpers
```

This keeps the core package usable by CLI, desktop, services, workers and other non-web applications.

## First stable scope

The first implementation should provide a deterministic conversion from:

```text
ErrorDescriptor
→
ProblemDetails
```

Suggested public abstractions:

```csharp
IErrorProblemDetailsMapper
ErrorProblemDetailsMapper
WhenItFailsProblemDetailsOptions
```

The exact names may change during implementation.

## Mapping rules

The mapping must be explicit and security-safe.

Suggested defaults:

```text
ProblemDetails.Title
    ← ErrorDescriptor.Title

ProblemDetails.Detail
    ← ErrorDescriptor.Message

ProblemDetails.Status
    ← explicit web.httpStatusCode mapping when available
    ← otherwise configured fallback status

ProblemDetails.Type
    ← configured documentation/type URI resolver
    ← optionally derived from DocumentationKey

ProblemDetails.Instance
    ← request-specific value supplied by ASP.NET Core integration
    ← not derived from internal source names
```

Suggested extensions:

```text
errorId
errorCode
errorName
severity
traceId
```

Optional fields such as categories or documentation keys may be included through configuration.

## Security defaults

The adapter must not expose sensitive runtime information by default.

These fields must remain excluded unless explicitly enabled:

```text
Exception
stack trace
DeveloperHint
internal Detail
OperationName
ComponentName
SourceName
arbitrary metadata
```

The existing profile mappings already express useful policy intent:

```text
web.includeTraceId
web.includeExceptionDetails
web.includeStackTrace
```

The ASP.NET Core adapter should honor those mappings or equivalent explicit options.

## HTTP status policy

Do not infer HTTP status codes from error severity.

Severity and HTTP semantics are different concepts.

Preferred precedence:

```text
explicit error/category/code-group/profile web.httpStatusCode
→ integration-level configured mapping
→ configured fallback status, normally 500
```

The exact inheritance/precedence rules between category, code group and profile mappings must be specified and tested before implementation.

## ASP.NET Core integration levels

### Phase 1 — mapper

Provide direct conversion:

```csharp
ProblemDetails problem =
    mapper.Map(descriptor);
```

This phase should not require an active `HttpContext`.

### Phase 2 — endpoint/controller helpers

Provide convenient usage for Minimal APIs and controllers.

Possible shapes:

```csharp
return descriptor.ToProblemResult();
```

or explicit service-based helpers that return an ASP.NET Core `IResult` / `ActionResult`.

The final API should avoid hiding status-selection policy.

### Phase 3 — IProblemDetailsService integration

Integrate with the ASP.NET Core problem-details pipeline so applications may use WhenItFails consistently with:

```text
AddProblemDetails
ExceptionHandlerMiddleware
StatusCodePagesMiddleware
Minimal API validation
controller-based APIs
```

Prefer cooperation with ASP.NET Core's existing `IProblemDetailsService` / writer pipeline rather than replacing framework behavior unnecessarily.

### Phase 4 — ValidationProblemDetails

Evaluate mapping field-level validation failures to `ValidationProblemDetails`.

This should be implemented only when WhenItFails has a clear structured representation of field-level validation errors. Do not invent field names from unstructured error messages.

## OpenAPI

The integration should support normal ASP.NET Core OpenAPI metadata so endpoints can document `ProblemDetails` / `ValidationProblemDetails` responses without custom schema duplication.

## Testing requirements

Required coverage should include:

- basic descriptor-to-problem mapping,
- explicit 4xx and 5xx status mappings,
- missing/invalid status mapping fallback,
- title/message mapping,
- stable error identity extensions,
- trace ID inclusion/exclusion,
- exception/detail/stack-trace suppression by default,
- explicit diagnostic opt-in,
- metadata filtering,
- documentation/type URI handling,
- Minimal API result integration,
- controller integration where supported,
- `IProblemDetailsService` integration,
- JSON serialization shape,
- no mutation of the source `ErrorDescriptor`,
- cross-platform test execution.

## Documentation requirements

When implemented:

- add registration and usage examples to `WhenItFails/README.md`,
- add a dedicated ASP.NET Core integration document,
- document mapping precedence,
- document safe production defaults,
- document controller and Minimal API examples,
- document OpenAPI behavior.

## Release priority

Problem Details support is considered a strong candidate for the first stable WhenItFails release because HTTP APIs are a primary consumer of structured errors and the existing WEB/API profile mappings already anticipate this integration.

It should be implemented as an integration package rather than by coupling ASP.NET Core directly into the core runtime.

## Central principle

> WhenItFails should preserve one stable error identity while allowing ASP.NET Core to present that error through its native Problem Details contract.
