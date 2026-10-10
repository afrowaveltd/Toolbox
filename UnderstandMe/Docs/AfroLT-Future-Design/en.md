# AfroLT — Future Lightweight .NET Translation Service (Design Notes)

Status: **future concept only** (2026-10-10). No AfroLT implementation, API or release exists in this repository.

## Intent and coexistence

AfroLT is a possible **independently implemented** .NET-native translation worker/library and optionally deployable HTTP service. It does **not** replace LibreTranslate. LibreTranslate remains a supported first-class translation backend, notably for reliable HTML-aware translation used by UnderstandMe's protected-placeholder techniques. Avoid reimplementing useful proven HTML handling merely to reach feature parity.

AfroLT may provide a comparable user-visible translation experience and optional familiar HTTP routes, while using a separate runtime design focused on bounded concurrency, efficient model management, load isolation, per-instance directional model subsets, monitoring and recovery. API compatibility would need a separate testable contract; do not assume it from a similar endpoint name. The engine should work both directly inside a .NET consumer where appropriate and as an isolated VPS worker/service.

Use the same interfaces and QueueMaster eligibility rules as any other translation provider. A Chain can select LibreTranslate for HTML or other special capabilities and AfroLT for compatible plain-text requests. Neither backend requires QueueMaster to know its concrete type. Neither a lack of a requested language pair nor a failed worker may corrupt published results.

## Minimal web interface with external themes

A lightweight website should allow ordinary interactive translation and access to authorized instance settings/status. The interface should use standard ASP.NET Core HTML/CSS and minimal scripting where sufficient.

**External CSS themes** are an intentional extension point. Keep layout/behavior distinct from presentation; select and load safe local theme assets without modifying application binaries. Validate trusted theme locations, avoid arbitrary remote CSS or uploads in privileged/admin sessions, and retain a working built-in default. Localization of AfroLT UI should use UnderstandMe's general localization components with a usable English fallback.

The website and API reference are different surfaces: the website is for translating text and administering the service; the interactive OpenAPI reference documents/test-drives the HTTP API.

## OpenAPI and Scalar

Prefer the Microsoft-provided `Microsoft.AspNetCore.OpenApi` generator and `System.Text.Json`, with **Scalar.AspNetCore** as the optional interactive API reference, instead of requiring the Swashbuckle/Swagger UI stack.

ASP.NET Core 11 generates OpenAPI **3.2** by default, while earlier client generators may still require a configurable OpenAPI **3.1** document. Verify actual OpenAPI 3.2 support across the intended deployed toolchain and client SDK generators; keep an explicit compatibility switch at build/configuration time for generated document version when needed.

Conceptual ASP.NET Core bootstrap (non-final):

```csharp
using Scalar.AspNetCore;

var builder = WebApplication.CreateBuilder(args);
builder.Services.AddOpenApi();
var app = builder.Build();
app.MapOpenApi();
app.MapScalarApiReference("/api-docs");
app.Run();
```

Protect or deliberately limit the OpenAPI and test-request UI on externally accessible VPS instances; only public API documentation should be anonymous by default, and a public docs endpoint does not imply public administrative APIs.

References:
- Microsoft OpenAPI: https://learn.microsoft.com/en-us/aspnet/core/fundamentals/openapi/aspnetcore-openapi
- Scalar ASP.NET Core: https://scalar.com/products/api-references/integrations/aspnetcore/integration
- LibreTranslate HTML `format`: https://docs.libretranslate.com/api/operations/translate/
- LibreTranslate: https://github.com/LibreTranslate/LibreTranslate


## Versioned Scalar references and compatibility

Desired documentation routes (design, not implemented):

| Route | Purpose |
| --- | --- |
| `/api-docs` | Redirect to `/api-docs/latest` |
| `/api-docs/latest` | Reference for the newest supported **OpenAPI specification version** (currently 3.2 on .NET 11) |
| `/api-docs/v3.2/` | Stable, version-specific Scalar reference to an OpenAPI 3.2 document |
| `/api-docs/v3.1/` | Stable, version-specific Scalar reference to an OpenAPI 3.1 document |
| `/openapi/...` | Persistent machine-readable specification documents, each with its own versioned URL |

As future specs appear, only the `latest` alias changes. Existing versioned URLs should remain stable; a retained/archive JSON document is acceptable when a newer generator can no longer reproduce exactly an old specification.

**The OpenAPI document format version is NOT the AfroLT HTTP API version.** The same AfroLT API v1 might be described with both OpenAPI 3.1 and 3.2. Separate the lifecycle of the HTTP contract (e.g. `/api/v1`, `/api/v2`), JSON DTO compatibility, generated spec versions and Scalar-rendered pages. Maintaining old reference pages without maintaining old HTTP behavior is not backward compatibility.

Current Scalar ASP.NET Core supports multiple OpenAPI documents, configurable paths, and multiple UI mounts. Microsoft's `Microsoft.AspNetCore.OpenApi` can emit multiple separately configured documents and select the OpenAPI specification version. Prototype exact registration and use automated schema/client-compatibility tests before locking routes or advertised versions.

Interoperability extends beyond .NET 10/11: non-.NET and older consumers should be able to use versioned plain HTTP+JSON contracts without C# native unions. Prefer `System.Text.Json`, stable error formats, explicit capability discovery, documented authentication and throttling, and an optional, tested compatibility adapter for useful LibreTranslate endpoints (rather than claiming full API equivalence by default). Keep LibreTranslate itself as an eligible worker, notably for HTML translations.

## Suggest a better translation

Keep a user-visible `Suggest a better translation` action inspired by LibreTranslate's `POST /suggest`, which accepts source text (`q`), a proposed translation (`s`), and `source`/`target` language identifiers. The LibreTranslate maintainers explicitly explain that submitted suggestions are **saved for future use, not automatically applied to the deployed model**.

AfroLT should keep suggestions in a separate, reviewable store (source text, translated candidate when available, new proposal, language pair, relevant context/target, timestamp, origin and optional actor with appropriate permissions). Submitting a suggestion never overwrites a published translation or neural-model weights. Configurable review may compare the proposal against the *original* text using the optional AI semantic auditor, request human confirmation, or reject content violating meaning, placeholder, formatting, privacy or source-context policy.

Approved suggestions have two distinct possible follow-ups:
1. An opt-in, auditable **translation-memory override** or localized dictionary edit can improve identical-context future lookups immediately without retraining a model.
2. A separately curated, rights-verified, versioned **training dataset** can be used for later offline model fine-tuning/retraining and explicit model deployment. Model training is NOT a per-suggestion side effect.

Provide abuse controls, duplicate handling, tenant/target isolation, contributor rights/consent and data-retention policy; automatic feedback cannot turn into uncontrolled data poisoning. If useful, a legacy `/suggest` payload adapter can translate to the richer internal suggestion contract.

Sources:
- https://docs.libretranslate.com/api/operations/suggest/
- https://community.libretranslate.com/t/feedback-suggest-api-not-working-in-offline-mode/649
- https://scalar.com/products/api-references/integrations/aspnetcore/integration
- https://learn.microsoft.com/en-us/aspnet/core/fundamentals/minimal-apis/aspnetcore-openapi

## Standalone-first core and optional SignalR Hub

**Architectural requirement:** AfroLT MUST be usable **on its own**, without either UnderstandMe or Afutrans at build time or runtime. It may be deployed in-process as a reusable .NET library, or as a separately hosted service. Its engine owns only the resource/capacity controls it needs to function independently; it must not be forced to adopt the global five-queue UnderstandMe scheduler, Messenger, Translation Chains, or Afutrans UI in standalone installations.

AfroLT uses **one internal translation service** with three interchangeable adapters:

| Surface | Consumer | Notes |
| --- | --- | --- |
| Direct .NET API | .NET library consumer | In-process invocation, no web server or SignalR required |
| REST HTTP + JSON | Legacy, browser, PHP, Python, .NET and other HTTP clients | Versioned contracts, Microsoft OpenAPI + Scalar reference |
| Optional ASP.NET Core SignalR Hub | Persistent .NET and other SignalR clients | Bidirectional requests, progress, notifications and result delivery |

All three call the same translation core, capability discovery, bounded worker/model manager and fault-handling services. Neither HTTP nor SignalR Hub may implement its own translation logic or introduce a second authoritative state. The web host should permit SignalR to be disabled independently without disabling the REST API or the direct library.

The **SignalR Hub belongs to AfroLT's standalone host**, not to UnderstandMe's passive `Messenger`. These can later be connected by a separate adapter, while keeping UnderstandMe's Messenger passive and QueueMaster solely responsible for global work assignment. AfroLT Core must have no compile-time dependency on UnderstandMe or Afutrans. Optional integration packages may depend inward on AfroLT Core and UnderstandMe contracts, not the reverse.

### Transport-neutral command API (agreed design principle)

**REST and SignalR are adapters over EXACTLY the same application commands and typed request/response models.** A command has one handler, one validation path, one policy/security decision and one outcome/error interpretation. An application command never asks which transport delivered it. A Hub is not a second implementation of the REST endpoint, and the REST endpoint is not an HTTP wrapper that invokes a Hub.

Illustrative command names (not final C# signatures): `Translate`, `TranslateBatch`, `SuggestTranslation`, `GetLanguages`, `GetStatus` and `GetJob`. Each command is executed by the same in-process application service, whether the caller used the library API, REST, or SignalR. Transport adapters must remain thin.

- **Same request/response DTO contracts:** field names, validation, culture identifiers, correlation/job IDs, result objects and documented failure codes are shared. Network DTOs are plain, versioned, language-neutral objects; C# 15 native unions are not imposed on non-.NET clients.
- **Same execution and security:** cancellation, per-client limits, authorizations, idempotency, provider selection, model capabilities and job state reside in the core/application layer. Hub and HTTP request identities are normalized into a transport-neutral caller context.
- **Transport-specific delivery only:** HTTP status codes and headers exist only on REST, while Hub method replies, streaming/progress events and connection lifecycle exist only on SignalR. Both convey the same logical outcome; a queued-job acceptance is not mistaken for completed translation.
- **One source of truth for results:** asynchronous job progress is tracked by the core and made observable through polling endpoints or SignalR notifications. A lost Hub connection cannot lose a persisted job; reconnect and query by job ID.
- **Contract verification:** run shared command-handler tests plus parity tests invoking both transport adapters with equivalent requests and asserting identical logical responses, failure codes, authorization decisions and idempotency behavior.

This principle applies to new commands and legacy compatibility adapters alike: an old `/suggest` or `/translate` shape maps to the same internal command rather than branching into a separate processing path.

### SignalR behavior

Suggested hub features (nonfinal API names): submit a short translation, submit a bounded batch/longer job, query available language pairs/models and subscribe to the caller's job status; the server can emit progress and result/failed/completed notifications. For long-running operations return a correlation/job ID rather than holding one hub method invocation open indefinitely. Keep typed DTOs compatible with both .NET 10 and 11 clients and language-neutral HTTP JSON consumers. The REST surface remains a complete fallback for clients without SignalR.

SignalR uses WebSockets when possible and can fall back to other transports. Persistent bidirectional connections can reduce the overhead of repeated exchanges and enable server-initiated events; this is **not a blanket throughput advantage over REST** for all request patterns. Default wire format: SignalR JSON via `System.Text.Json`, compatible with our other JSON conventions. Optional MessagePack can reduce wire sizes, but is a separate dependency/protocol with interoperability tradeoffs; notably .NET 11's native union types are currently supported by SignalR's JSON protocol, not by the MessagePack protocol. Expose wire DTOs that don't require native unions.

Ensure authentication/authorization for connected users, per-client job isolation, quotas and backpressure, message size/streaming limits, cancellation, and reconnection/resume by job ID. SignalR reconnection alone does not guarantee durable delivery; acknowledge persistent jobs only after durable acceptance and avoid duplicate publication through idempotent IDs. Ephemeral short translations may use simpler nonpersistent semantics.

Proposed module boundaries (names provisional):

`AfroLT.Core` -> independent translation/model/capacity/fault handling contracts and implementation
`AfroLT.Hosting` -> optional ASP.NET Core HTTP host and admin/translation website
`AfroLT.SignalR` -> optional Hub transport adapter
`AfroLT.UnderstandMe` -> optional plugin adapter exposing AfroLT as an UnderstandMe translation worker

These are **conceptual boundaries, not committed package names**. A tiny standalone embedded client should not be required to load ASP.NET Core, SignalR, QueueMaster, or other host-only infrastructure.

References:
- Microsoft SignalR overview: https://learn.microsoft.com/en-us/aspnet/core/signalr/introduction
- Microsoft SignalR hubs: https://learn.microsoft.com/en-us/aspnet/core/signalr/hubs
- Microsoft SignalR .NET client: https://learn.microsoft.com/en-us/aspnet/core/signalr/dotnet-client

## Open design details

- Determine which LibreTranslate-compatible endpoints (if any) are worth replicating, and how capability negotiation avoids false HTML support claims.
- Define separation of stable engine APIs from optional HTTP host, Scalar reference, translation website and admin UI.
- Specify security for external CSS/theme loading, per-instance authorized admin controls and public API documentation.
- Benchmark recovery, memory, model cold-starts and throughput before claiming improvements.

See [working design notes](../../Notes.md), [open questions](../Open-Questions/en.md), and [implementation status](../../IMPLEMENTATION_STATUS.md).
