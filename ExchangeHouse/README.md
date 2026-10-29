# ExchangeHouse — document formats and conversions

ExchangeHouse is the reusable, **UI-independent** Afrowave.Toolbox family for reading, writing, analyzing and converting data/document formats. It is a **folder and family of independent packages**, not a mandatory aggregate NuGet dependency.

The name communicates the purpose: content passes between formats without forcing consumers to adopt unrelated format libraries.

## First independent component

- [Markdown](Markdown/README.md) — initial .NET 10 Markdown block scanner and lossless writer foundation.
- [Architecture](Docs/Architecture/en.md) — dependency boundaries, format adapters and conversion principles.
- [Conversion roadmap](Docs/Converters/en.md) — possible Markdown, plain text, HTML, JSON, AJIS and XML conversion paths.
- [Implementation status](IMPLEMENTATION_STATUS.md) — verified state and next steps.

Each format parser retains its own format-aware model; no universal AST is imposed. Converters reference only the necessary format components. A conversion may be lossy: the API must eventually report fidelity, dropped structures and failures instead of silently implying reversibility.

The Spectre.Console Markdown renderer belongs separately under `Blackboard/SpectreConsole/Markdown` and will consume this library. ExchangeHouse must **not** reference Spectre.Console, Blackboard, Afutrans or UnderstandMe.

The source in `afrowaveltd/AfrowaveWebPublic`, particularly `Id`'s Markdown tools, is useful design inspiration but is **not blindly copied**.

## Development discipline

Commit each small, testable increment to Toolbox; keep each project's English `README.md`, topic-level `Docs/<topic>/en.md`, tests and `IMPLEMENTATION_STATUS.md` current. New packages are opt-in; do not publish a partially implemented parser as full CommonMark support.
