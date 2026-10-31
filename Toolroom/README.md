# Toolroom — developer and maintenance utilities

Toolroom owns runnable tools, scripts and release helpers. Unlike Essentials and other shared libraries, Toolroom applications can compose domain libraries and UI adapters to perform explicit user-requested maintenance.

## Implemented tools

- [WhenItFails Setter](WhenItFails/Setter/README.md) — catalog authoring, inspection, validation and safe writes; has its own xUnit suite and localized-topic documentation.
- [Public API comparer](WhenItFails/PublicApiComparer/README.md) — PowerShell-based source/public-package API comparison and legacy binary smoke scripts.
- [Release candidate validation](WhenItFails/Release/README.md) — candidate package validation scripts and instructions.

## Usage

Run from the Toolbox repository root:

```bash
dotnet run --project Toolroom/WhenItFails/Setter -- --help
```

If the Setter's CLI expects different arguments, see the [Setter README](WhenItFails/Setter/README.md) for the authoritative command syntax. For PowerShell release tools, follow their respective READMEs.

See [tool ownership and documentation](Docs/Utilities/en.md) and [implementation status](IMPLEMENTATION_STATUS.md).
