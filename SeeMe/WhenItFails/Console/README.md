# SeeMe.WhenItFails.Console

A reusable .NET 10 console presentation project for WhenItFails error issues and catalog validation output, based on Spectre.Console. This is a library/adapter, **not** the WhenItFails catalog owner, a whole-Toolbox package or an executable.

## Public building blocks

- `ConsoleIssueShow`
- `ConsoleValidationResultShow`
- `ConsoleShowTheme` and `ConsoleShowOptions`
- `ConsoleMarkupHelper`

The project references `WhenItFails` and `Spectre.Console 0.57.2`. Its markup safety and rendering contracts must be verified against actual sources and tests before being extended.

## Usage in the repository

[Toolroom Setter](../../../Toolroom/WhenItFails/Setter/README.md) consumes this adapter. It is **not recorded as a public NuGet release**.

[Implementation notes](Docs/Presentation/en.md) · [Status](IMPLEMENTATION_STATUS.md).
