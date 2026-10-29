# Afrowave.Toolbox.Blackboard.SpectreConsole.Markdown

Standalone .NET 10 Spectre.Console Markdown renderer depending only on [ExchangeHouse.Markdown](../../../ExchangeHouse/Markdown/README.md) and Spectre.Console. No dependency on UnderstandMe or other Blackboard components. **Not yet published to NuGet.**

## Usage

```csharp
using Afrowave.Toolbox.Blackboard.SpectreConsole.Markdown;
using Spectre.Console;

var theme = new MarkdownSpectreTheme
{
    Heading1 = new Style(foreground: Color.Green, decoration: Decoration.Bold),
    ShowLinkDestinations = true,
    CodeIndent = "  "
};
MarkdownSpectreRenderer.Render(AnsiConsole.Console,
    "# Help\nRead **our docs**: [Guide](../Docs/en.md)\n", theme);
```

You may also parse once with `MarkdownParser.Parse` and pass the `MarkdownDocument` to `Render` repeatedly with different themes.

Themes expose six heading styles, body, bold, emphasis, inline/fenced code, link label, link destination and list marker styles, a list bullet, code indentation, and optional fence/destination display. Input is rendered as literal Spectre `Text`, with terminal control characters sanitized. No link is opened or converted. The renderer does not clear the terminal.

**Limitations:** the underlying ExchangeHouse scanner is not full CommonMark. No tables, images, complex nested lists or full link syntax yet. Nested style decorations are not combined; most specific tokens take precedence. The Configurator's existing help renderer remains in place until a separate integration increment.

[Rendering and theming](Docs/Rendering-and-Themes/en.md) · [Implementation status](IMPLEMENTATION_STATUS.md)

```bash
dotnet test Blackboard/SpectreConsole/Markdown.Tests/Afrowave.Toolbox.Blackboard.SpectreConsole.Markdown.Tests.csproj
```
