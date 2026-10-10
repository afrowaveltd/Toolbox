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

## Open design details

- Determine which LibreTranslate-compatible endpoints (if any) are worth replicating, and how capability negotiation avoids false HTML support claims.
- Define separation of stable engine APIs from optional HTTP host, Scalar reference, translation website and admin UI.
- Specify security for external CSS/theme loading, per-instance authorized admin controls and public API documentation.
- Benchmark recovery, memory, model cold-starts and throughput before claiming improvements.

See [working design notes](../../Notes.md), [open questions](../Open-Questions/en.md), and [implementation status](../../IMPLEMENTATION_STATUS.md).
