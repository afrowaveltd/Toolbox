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

        foreach (var block in document.Blocks)
        {
            switch (block.Kind)
            {
                case MarkdownBlockKind.Blank:
                    console.Write(new Text("\n"));
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
                    var list = new Text();
                    AppendSafe(list, theme.ListBullet + " ", theme.ListMarker);
                    AppendInlines(list, block.Inlines, block.Text, theme.Body, theme);
                    list.Append("\n");
                    console.Write(list);
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
        var line = new Text();
        AppendInlines(line, block.Inlines, block.Text, inherited, theme);
        line.Append("\n");
        console.Write(line);
    }

    private static void WriteLine(IAnsiConsole console, string raw, Style style)
    {
        var line = new Text();
        AppendSafe(line, raw, style);
        line.Append("\n");
        console.Write(line);
    }

    private static void AppendInlines(Text line, IReadOnlyList<MarkdownInlineNode> nodes,
        string fallback, Style inherited, MarkdownSpectreTheme theme)
    {
        if (nodes.Count == 0)
        {
            AppendSafe(line, fallback, inherited);
            return;
        }
        foreach (var node in nodes)
        {
            switch (node.Kind)
            {
                case MarkdownInlineKind.Text:
                    AppendSafe(line, node.Text, inherited);
                    break;
                case MarkdownInlineKind.Code:
                    AppendSafe(line, node.Text, theme.Code);
                    break;
                case MarkdownInlineKind.Strong:
                    AppendInlines(line, node.Children, node.Text, theme.Strong, theme);
                    break;
                case MarkdownInlineKind.Emphasis:
                    AppendInlines(line, node.Children, node.Text, theme.Emphasis, theme);
                    break;
                case MarkdownInlineKind.Link:
                    AppendInlines(line, node.Children, node.Text, theme.Link, theme);
                    if (theme.ShowLinkDestinations && node.Destination is not null)
                        AppendSafe(line, " (" + node.Destination + ")", theme.LinkDestination);
                    break;
            }
        }
    }

    private static void AppendSafe(Text line, string content, Style style)
    {
        // Spectre Text treats brackets literally; sanitize terminal control bytes as well.
        StringBuilder? cleaned = null;
        for (var i = 0; i < content.Length; i++)
        {
            if (!char.IsControl(content[i]) || content[i] == '\t')
                continue;
            cleaned ??= new StringBuilder(content);
            cleaned[i] = '\uFFFD';
        }
        line.Append(cleaned?.ToString() ?? content, style);
    }
}
