# SeeMe — reusable console presentation

SeeMe is the Toolbox family for displaying structured results without moving domain logic into a UI renderer.

## Implemented component

[WhenItFails Console](WhenItFails/Console/README.md) contains the `.NET 10` Spectre.Console adapter for displaying WhenItFails issues and validation results. Its source is at `SeeMe/WhenItFails/Console`; it is not a substitute for the underlying catalog runtime.

The WhenItFails runtime lives independently in [WhenItFails](../WhenItFails/README.md). Tools such as [Setter](../Toolroom/WhenItFails/Setter/README.md) can consume the display adapter without forcing the main WhenItFails package to depend on Spectre.Console.

## Documentation and status

[Console rendering](Docs/Console-Presentation/en.md) · [Implementation status](IMPLEMENTATION_STATUS.md). SeeMe is not documented as a separately published NuGet package.
