# Getting started with the Toolbox repository

## Prerequisites

Install the .NET 10 SDK, Git and an interactive terminal for the Spectre.Console demos. Clone [afrowaveltd/Toolbox](https://github.com/afrowaveltd/Toolbox) and run from its root.

```bash
git clone https://github.com/afrowaveltd/Toolbox.git
cd Toolbox
dotnet restore Toolbox.sln
dotnet build Toolbox.sln -c Release
dotnet test Toolbox.sln -c Release
```

On Windows PowerShell the `dotnet` commands are identical. A local preview .NET 11 SDK may build the `net10.0` projects but can emit an informational preview SDK advisory.

## Install a published component

Three libraries are maintainer-confirmed as public NuGet packages:

```bash
dotnet add package Afrowave.Toolbox.Essentials --version 0.2.0
dotnet add package Afrowave.Toolbox.WhenItFails --version 1.0.0
dotnet add package Afrowave.Toolbox.Blackboard.SpectreConsole.FileBrowser --version 0.1.0-preview.1
```

Run each `dotnet add package` command from the **consumer project's directory**, and only install the libraries you actually need. FileBrowser is prerelease and requires an appropriate .NET target. NuGet version publication does not automatically mean an in-repository example is built from its public package rather than local source.

## Try the UI and help

```bash
dotnet run --project Blackboard/Samples/Blackboard.Showcase
dotnet run --project UnderstandMe/Tools/Configurator
```

Showcase includes a text/JSON/directory FileBrowser demonstration and an interactive Markdown theme gallery. Configurator contains an offline Help menu rendered with Blackboard Markdown. These are independent applications, not required runtime dependencies.

## Working on unpublished Markdown libraries

The two implemented Markdown projects are available by project reference:

- [ExchangeHouse.Markdown](../../ExchangeHouse/Markdown/README.md): source-aware syntax scanner, link/image metadata, list hierarchy and unchanged Markdown writer.
- [Blackboard.SpectreConsole.Markdown](../../Blackboard/SpectreConsole/Markdown/README.md): configurable Spectre renderer.

Example targeted test commands:

```bash
dotnet test ExchangeHouse/Markdown.Tests/Afrowave.Toolbox.ExchangeHouse.Markdown.Tests.csproj -c Release
dotnet test Blackboard/SpectreConsole/Markdown.Tests/Afrowave.Toolbox.Blackboard.SpectreConsole.Markdown.Tests.csproj -c Release
dotnet test Blackboard/Samples/Blackboard.Showcase.Tests/Blackboard.Showcase.Tests.csproj -c Release
```

## Find more documentation

[Main documentation index](../Documentation/en.md) · [Essentials result examples](../Getting-Started/en.md) · [Release and package status](../Release-Status/en.md) · [Development rules](../Development-and-Verification/en.md).
