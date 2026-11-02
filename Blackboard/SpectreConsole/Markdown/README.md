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

**Limitations:** the underlying ExchangeHouse scanner is not full CommonMark. It supports inline images (text alternatives) and simple contiguous nested/numbered lists, but **not** tables, multiline/loose list items, reference-style links or complete CommonMark syntax. Nested style decorations are not combined; most specific tokens take precedence. The UnderstandMe Configurator's offline Help now delegates to this library through a tiny application-specific theme adapter (the previously separate integration increment).

[Rendering and theming](Docs/Rendering-and-Themes/en.md) · [Implementation status](IMPLEMENTATION_STATUS.md)

```bash
dotnet test Blackboard/SpectreConsole/Markdown.Tests/Afrowave.Toolbox.Blackboard.SpectreConsole.Markdown.Tests.csproj
```

## Accessible images (2026-10-29)

Images from ExchangeHouse Markdown are represented as readable text (default: [Image: alt text]) instead of attempting terminal image transfer. The theme exposes Image, ImageDestination, ImagePrefix, ImageSuffix and ShowImageDestinations independently of link settings. No file or network I/O is performed, and alt text is rendered as safe literal Spectre Text. See [Rendering and Themes](Docs/Rendering-and-Themes/en.md).

## Nested lists and original numbering (2026-10-31)

Numbered items now display their original number and `.` / `)` delimiter rather than a generic bullet. Nested items use the ExchangeHouse tree depth. Themes can set `ListIndent` and `PreserveUnorderedMarkers` while retaining their existing `ListBullet`. See [Nested list presentation](Docs/Nested-List-Presentation/en.md).

## Multiline list text (2026-10-31)

The renderer now aligns explicitly indented continuation paragraphs under their owning list item's visible content without repeating the bullet or number. This uses source-aware ExchangeHouse metadata, respects theme indentation and leaves original Markdown untouched. See [Continuation presentation](Docs/List-Continuation-Presentation/en.md).

## Loose-list paragraph display (2026-10-31)

Blank-separated paragraphs owned by an ExchangeHouse list item remain aligned beneath its visible bullet or number. The renderer preserves a blank line without duplicating the marker or changing the document. See [Loose list presentation](Docs/Loose-List-Presentation/en.md).

## Code, blockquotes and optional syntax highlighting (2026-10-31)

Indented list-embedded quotes and fenced code now render beneath their list item, with optional code language labels. A session-based `SyntaxHighlighter` theme property accepts independent coloring providers; the base Markdown renderer retains plain-text fallback. The new optional [TextMate adapter](../Markdown.TextMate/README.md) integrates TextMateSharp grammar coloring without forcing native assets on normal Markdown consumers. See [Code and Quote Presentation](Docs/Code-Quote-Presentation/en.md). Not publicly published on NuGet.

## Quoted list rendering (2026-11-01)

Quote-contained lists render with configurable quote prefix outside the list indentation, preserving original ordered numbers and source-aware continuation alignment. [Presentation documentation](Docs/Quoted-List-Presentation/en.md). More complex quoted code containers remain future work.

## Quoted code and syntax highlighting (2026-11-01)

The renderer can display syntax-colored fenced source inside quoted lists, drawing themed quote prefixes outside the code/list indentation. An optional per-fence TextMate highlighting session is disposed on closure or interruption. Code remains safe literal text; see [quote-code rendering](Docs/Quoted-Code-Presentation/en.md).

## Loose quoted list paragraphs (2026-11-01)

The quote-prefix styling and list-content alignment now handle quoted list items with blank-separated, explicitly indented paragraphs. The parser decides ownership; the existing renderer shows preserved quote blank lines and resumes beneath the original numbered marker without duplicating it. [Presentation details](Docs/Quoted-Loose-Presentation/en.md).

## Outer and inner list indentation (2026-11-01)

Quoted lists embedded beneath ordinary list items draw the **outer list indentation before the quote prefix**, then the inner quote-list nesting. Code-fence language labels, delimiters and bodies use that same layout; plain quoted prose owned by the outer item is not double-indented. [Quoted list container presentation](Docs/Quoted-List-Containers/en.md).

## Future standalone Markdown viewer — design proposal (2026-11-02)

After this parser/renderer has matured, a separate terminal app could open local Markdown, HTTP(S) documents and GitHub repository README files by `github:owner/repo`. No-argument mode would expose an interactive source picker. The reader would reuse this library rather than embedding fetch/navigation concerns into the renderer. [Proposal and non-goals](Docs/Standalone-Markdown-Viewer-Proposal/en.md). **Not implemented.**
