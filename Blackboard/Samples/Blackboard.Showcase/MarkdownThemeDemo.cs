using Afrowave.Toolbox.Blackboard.SpectreConsole.Markdown;
using Afrowave.Toolbox.ExchangeHouse.Markdown;
using Spectre.Console;

namespace Afrowave.Toolbox.Blackboard.Showcase;

/// <summary>
/// An interactive, file-free demonstration of independently themeable Markdown rendering.
/// All themes share a single parsed ExchangeHouse document.
/// </summary>
public sealed class MarkdownThemeDemo : IBlackboardShowcaseDemo
{
    private static readonly MarkdownDocument Sample = MarkdownParser.Parse(
        """
        # Markdown theme gallery

        ## Text and emphasis
        One **document** can have *many appearances*. Try `inline code` and [read the guide](../Docs/en.md).

        ### Common blocks
        - **Headings** have six individually configurable styles
        - *Lists*, links, and literal code keep their original meaning
        - Markdown source is parsed once, independent of terminal colors

        ```csharp
        var document = MarkdownParser.Parse(markdown);
        MarkdownSpectreRenderer.Render(console, document, theme);
        ```
        """);

    private static readonly IReadOnlyList<ThemeChoice> Themes =
    [
        new("classic", "Classic — default", new MarkdownSpectreTheme { ShowLinkDestinations = true }),
        new("ocean", "Ocean — blue emphasis", new MarkdownSpectreTheme
        {
            Heading1 = new Style(foreground: Color.Blue, decoration: Decoration.Bold),
            Heading2 = new Style(foreground: Color.Blue, decoration: Decoration.Underline),
            Heading3 = new Style(foreground: Color.White, decoration: Decoration.Bold),
            Strong = new Style(foreground: Color.Blue, decoration: Decoration.Bold),
            Emphasis = new Style(foreground: Color.White, decoration: Decoration.Italic),
            Link = new Style(foreground: Color.Blue, decoration: Decoration.Underline),
            Code = new Style(foreground: Color.Yellow),
            ListMarker = new Style(foreground: Color.Blue),
            ShowLinkDestinations = true
        }),
        new("forest", "Forest — green accents", new MarkdownSpectreTheme
        {
            Heading1 = new Style(foreground: Color.Green, decoration: Decoration.Bold),
            Heading2 = new Style(foreground: Color.Green, decoration: Decoration.Underline),
            Heading3 = new Style(foreground: Color.Yellow, decoration: Decoration.Bold),
            Strong = new Style(foreground: Color.Green, decoration: Decoration.Bold),
            Emphasis = new Style(foreground: Color.Yellow, decoration: Decoration.Italic),
            Link = new Style(foreground: Color.Green, decoration: Decoration.Underline),
            Code = new Style(foreground: Color.Yellow),
            ListMarker = new Style(foreground: Color.Green),
            ListBullet = "+",
            CodeIndent = "  ",
            ShowLinkDestinations = true
        }),
        new("mono", "Monochrome — no explicit colors", new MarkdownSpectreTheme
        {
            Heading1 = new Style(decoration: Decoration.Bold | Decoration.Underline),
            Heading2 = new Style(decoration: Decoration.Bold),
            Heading3 = new Style(decoration: Decoration.Underline),
            Heading4 = new Style(decoration: Decoration.Bold),
            Heading5 = new Style(decoration: Decoration.Bold),
            Heading6 = new Style(decoration: Decoration.Bold),
            Strong = new Style(decoration: Decoration.Bold),
            Emphasis = new Style(decoration: Decoration.Italic),
            Code = Style.Plain,
            Link = new Style(decoration: Decoration.Underline),
            ListMarker = Style.Plain,
            ListBullet = "-",
            CodeIndent = "  ",
            ShowLinkDestinations = true,
            ShowCodeFenceDelimiters = true
        })
    ];

    /// <inheritdoc/>
    public string Id => "markdown-themes";

    /// <inheritdoc/>
    public string Title => "Markdown — preview and compare color themes";

    /// <inheritdoc/>
    public async Task RunAsync(
        IAnsiConsole console, string startingDirectory, CancellationToken cancellationToken)
    {
        ArgumentNullException.ThrowIfNull(console);
        ArgumentException.ThrowIfNullOrWhiteSpace(startingDirectory);

        if (!console.Profile.Capabilities.Interactive)
            throw new InvalidOperationException("The Markdown gallery requires an interactive terminal.");

        var choices = Themes
            .Select(theme => new MenuChoice(theme.Label, theme.Id))
            .Append(new MenuChoice("Compare all themes", "compare"))
            .Append(new MenuChoice("Back to Showcase", null))
            .ToArray();

        while (true)
        {
            cancellationToken.ThrowIfCancellationRequested();
            console.Write(new Text("Markdown gallery — the same document, multiple themes." + Environment.NewLine));

            var selected = await new SelectionPrompt<MenuChoice>()
                .Title("Choose a Markdown theme")
                .PageSize(7)
                .UseConverter(item => Markup.Escape(item.Label))
                .AddChoices(choices)
                .AddCancelResult(choices[^1])
                .ShowAsync(console, cancellationToken);

            if (selected.Id is null)
                return;

            if (selected.Id == "compare")
            {
                foreach (var theme in Themes)
                    RenderTheme(console, theme);
            }
            else
            {
                // The menu maps stable IDs to presets; it never looks up themes by text labels.
                var theme = Themes.First(value => value.Id == selected.Id);
                RenderTheme(console, theme);
            }

            var back = "Return to theme menu";
            await new SelectionPrompt<string>()
                .Title("Preview finished")
                .AddChoices(back)
                .AddCancelResult(back)
                .ShowAsync(console, cancellationToken);
        }
    }

    private static void RenderTheme(IAnsiConsole console, ThemeChoice theme)
    {
        console.Write(new Text(Environment.NewLine + "Theme preview: " + theme.Label + Environment.NewLine));
        MarkdownSpectreRenderer.Render(console, Sample, theme.Theme);
        console.Write(new Text(Environment.NewLine));
    }

    private sealed record ThemeChoice(string Id, string Label, MarkdownSpectreTheme Theme);
    private sealed record MenuChoice(string Label, string? Id);
}
