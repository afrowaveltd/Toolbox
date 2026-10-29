using System.Text;
using Afrowave.Toolbox.ExchangeHouse.Markdown;
using Spectre.Console;

namespace Afrowave.Toolbox.Blackboard.SpectreConsole.Markdown;

/// <summary>
/// Read-only presentation adapter. No filesystem or network access and no link opening.
/// Markdown input is passed through Spectre Text rather than evaluated as Spectre markup.
/// </summary>
public static class MarkdownSpectreRenderer
{
    /// <summary>Parse a Markdown string and render it with an optional theme.</summary>
    public static void Render(IAnsiConsole console, string markdown, MarkdownSpectreTheme? theme = null)
    {
        ArgumentNullException.ThrowIfNull(console);
        ArgumentNullException.ThrowIfNull(markdown);
        Render(console, MarkdownParser.Parse(markdown), theme);
    }

    /// <summary>Render an already parsed Markdown document without parsing it again.</summary>
    public static void Render(IAnsiConsole console, MarkdownDocument document, MarkdownSpectreTheme? theme = null)
    {
        ArgumentNullException.ThrowIfNull(console);
        ArgumentNullException.ThrowIfNull(document);
        theme ??= new MarkdownSpectreTheme();
        ArgumentNullException.ThrowIfNull(theme.ListBullet);
        ArgumentNullException.ThrowIfNull(theme.CodeIndent);
        ArgumentNullException.ThrowIfNull(theme.ImagePrefix);
        ArgumentNullException.ThrowIfNull(theme.ImageSuffix);

        foreach (var block in document.Blocks)
        {
            switch (block.Kind)
            {
                case MarkdownBlockKind.Blank:
                    WriteNewline(console);
                    break;
                case MarkdownBlockKind.CodeFence:
                    if (theme.ShowCodeFenceDelimiters)
                        WriteLine(console, block.RawText.TrimEnd('\r', '\n'), theme.Code);
                    break;
                case MarkdownBlockKind.CodeLine:
                    WriteLine(console, theme.CodeIndent + block.Text, theme.Code);
                    break;
                case MarkdownBlockKind.Heading:
                    WriteInlineLine(console, block, theme.GetHeadingStyle(block.HeadingLevel), theme);
                    break;
                case MarkdownBlockKind.ListItem:
                    WriteSafe(console, theme.ListBullet + " ", theme.ListMarker);
                    WriteInlines(console, block.Inlines, block.Text, theme.Body, theme);
                    WriteNewline(console);
                    break;
                case MarkdownBlockKind.Paragraph:
                    WriteInlineLine(console, block, theme.Body, theme);
                    break;
            }
        }
    }

    private static void WriteInlineLine(
        IAnsiConsole console, MarkdownBlock block, Style inherited, MarkdownSpectreTheme theme)
    {
        WriteInlines(console, block.Inlines, block.Text, inherited, theme);
        WriteNewline(console);
    }

    private static void WriteLine(IAnsiConsole console, string raw, Style style)
    {
        WriteSafe(console, raw, style);
        WriteNewline(console);
    }

    private static void WriteNewline(IAnsiConsole console) => console.Write(new Text("\n"));

    private static void WriteInlines(IAnsiConsole console, IReadOnlyList<MarkdownInlineNode> nodes,
        string fallback, Style inherited, MarkdownSpectreTheme theme)
    {
        if (nodes.Count == 0)
        {
            WriteSafe(console, fallback, inherited);
            return;
        }
        foreach (var node in nodes)
        {
            switch (node.Kind)
            {
                case MarkdownInlineKind.Text:
                    WriteSafe(console, node.Text, inherited);
                    break;
                case MarkdownInlineKind.Code:
                    WriteSafe(console, node.Text, theme.Code);
                    break;
                case MarkdownInlineKind.Strong:
                    WriteInlines(console, node.Children, node.Text, theme.Strong, theme);
                    break;
                case MarkdownInlineKind.Emphasis:
                    WriteInlines(console, node.Children, node.Text, theme.Emphasis, theme);
                    break;
                case MarkdownInlineKind.Link:
                    WriteInlines(console, node.Children, node.Text, theme.Link, theme);
                    if (theme.ShowLinkDestinations && node.Destination is not null)
                        WriteSafe(console, " (" + node.Destination + ")", theme.LinkDestination);
                    break;
                case MarkdownInlineKind.Image:
                    WriteSafe(console, theme.ImagePrefix, theme.Image);
                    WriteInlines(console, node.Children, node.Text, theme.Image, theme);
                    WriteSafe(console, theme.ImageSuffix, theme.Image);
                    if (theme.ShowImageDestinations && node.Destination is not null)
                        WriteSafe(console, " (" + node.Destination + ")", theme.ImageDestination);
                    break;
            }
        }
    }

    private static void WriteSafe(IAnsiConsole console, string content, Style style)
    {
        // Render as literal Text, never Spectre markup. Sanitize terminal control sequences.
        StringBuilder? cleaned = null;
        for (var i = 0; i < content.Length; i++)
        {
            if (!char.IsControl(content[i]) || content[i] == '\t')
                continue;
            cleaned ??= new StringBuilder(content);
            cleaned[i] = '\uFFFD';
        }
        console.Write(new Text(cleaned?.ToString() ?? content, style));
    }
}
