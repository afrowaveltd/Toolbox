# SeeMe console presentation

SeeMe owns only terminal presentation of structured WhenItFails results. The error catalogs, validation and recovery behavior remain in [WhenItFails](../../../WhenItFails/README.md).

The current [console adapter](../../WhenItFails/Console/README.md) supplies reusable issue/validation renderers, themes and options through Spectre.Console. Reusable domain libraries should not import UI frameworks merely for formatting output. A separate [Setter CLI](../../../Toolroom/WhenItFails/Setter/README.md) is an example consumer.

Source/project reference remains authoritative; no new NuGet publishing claim is made. See [implementation status](../../IMPLEMENTATION_STATUS.md).
