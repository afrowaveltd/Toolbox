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
        ArgumentNullException.ThrowIfNull(theme.ListIndent);
        ArgumentNullException.ThrowIfNull(theme.CodeIndent);
        ArgumentNullException.ThrowIfNull(theme.QuotePrefix);
        ArgumentNullException.ThrowIfNull(theme.ImagePrefix);
        ArgumentNullException.ThrowIfNull(theme.ImageSuffix);

        IMarkdownHighlightSession? highlightSession = null;
        try
        {
        foreach (var block in document.Blocks)
        {
            // An interrupted quote-owned fence has no physical closing delimiter.
            // Never carry its highlighter state into the following prose or list.
            if (highlightSession is not null &&
                block.Kind is not (MarkdownBlockKind.CodeFence or MarkdownBlockKind.CodeLine))
            {
                highlightSession.Dispose();
                highlightSession = null;
            }

            switch (block.Kind)
            {
                case MarkdownBlockKind.Blank:
                    WriteNewline(console);
                    break;
                case MarkdownBlockKind.CodeFence:
                    if (block.IsClosingCodeFence)
                    {
                        highlightSession?.Dispose();
                        highlightSession = null;
                    }
                    else
                    {
                        highlightSession?.Dispose();
                        highlightSession = null;
                        if (block.CodeLanguage is { Length: > 0 } language &&
                            theme.SyntaxHighlighter is not null)
                        {
                            try
                            {
                                highlightSession = theme.SyntaxHighlighter.CreateSession(language);
                            }
                            catch (Exception ex) when (ex is not OutOfMemoryException)
                            {
                                // An optional highlighting provider must not make help unreadable.
                            }
                        }
                    }
                    if (theme.ShowCodeFenceDelimiters)
                        WriteLine(console, QuoteContainerPrefix(block, theme) +
                            ListContainerIndent(block, theme) +
                            (block.QuoteDepth > 0 || block.ListContinuationMarker is not null
                                ? block.Text : block.RawText.TrimEnd('\r', '\n')), theme.Code);
                    else if (!block.IsClosingCodeFence && theme.ShowCodeLanguage &&
                             block.CodeLanguage is { Length: > 0 } codeLanguage)
                        WriteLine(console, QuoteContainerPrefix(block, theme) +
                            ListContainerIndent(block, theme) +
                            theme.CodeIndent + "[" + codeLanguage + "]", theme.CodeLanguageStyle);
                    break;
                case MarkdownBlockKind.CodeLine:
                    if (block.QuoteDepth > 0)
                        WriteSafe(console, QuoteContainerPrefix(block, theme), theme.QuoteMarker);
                    WriteSafe(console, ListContainerIndent(block, theme) + theme.CodeIndent, theme.Code);
                    WriteHighlightedCode(console, block.Text, block.CodeLanguage, theme, ref highlightSession);
                    WriteNewline(console);
                    break;
                case MarkdownBlockKind.BlockQuote:
                    WriteSafe(console, ListContainerIndent(block, theme) +
                        string.Concat(Enumerable.Repeat(theme.QuotePrefix,
                            Math.Clamp(block.QuoteDepth, 0, 32))), theme.QuoteMarker);
                    WriteInlineLine(console, block, theme.Quote, theme);
                    break;
                case MarkdownBlockKind.Heading:
                    WriteInlineLine(console, block, theme.GetHeadingStyle(block.HeadingLevel), theme);
                    break;
                case MarkdownBlockKind.ListItem:
                    if (block.QuoteDepth > 0)
                        WriteSafe(console, QuoteContainerPrefix(block, theme), theme.QuoteMarker);
                    var marker = block.ListMarker;
                    var depth = Math.Clamp(marker?.Depth ?? 0, 0, 32);
                    var prefix = ListPrefix(marker, theme);
                    WriteSafe(console, string.Concat(Enumerable.Repeat(theme.ListIndent, depth)) +
                        prefix + " ", theme.ListMarker);
                    WriteInlines(console, block.Inlines, block.Text, theme.Body, theme);
                    WriteNewline(console);
                    break;
                case MarkdownBlockKind.Paragraph:
                    if (block.QuoteDepth > 0)
                        WriteSafe(console, QuoteContainerPrefix(block, theme), theme.QuoteMarker);
                    if (block.ListContinuationMarker is not null)
                        WriteSafe(console, ListContainerIndent(block, theme), theme.Body);
                    WriteInlineLine(console, block, theme.Body, theme);
                    break;
            }
        }
        }
        finally
        {
            highlightSession?.Dispose();
        }
    }

    // Quote prefixes are rendered separately from list indentation so callers
    // can style nesting markers without changing source-relative list depths.
    private static string QuoteContainerPrefix(MarkdownBlock block, MarkdownSpectreTheme theme) =>
        string.Concat(Enumerable.Repeat(theme.QuotePrefix,
            Math.Clamp(block.QuoteDepth, 0, 32)));

    private static string ListContainerIndent(MarkdownBlock block, MarkdownSpectreTheme theme)
    {
        if (block.ListContinuationMarker is not { } marker)
            return string.Empty;
        var markerPrefix = ListPrefix(marker, theme);
        var level = string.Concat(Enumerable.Repeat(theme.ListIndent,
            Math.Clamp(marker.Depth, 0, 32)));
        return level + new string(' ', markerPrefix.Length + 1);
    }

    private static void WriteHighlightedCode(
        IAnsiConsole console, string line, string? language, MarkdownSpectreTheme theme,
        ref IMarkdownHighlightSession? session)
    {
        if (session is null || language is null || line.Length > 16384)
        {
            WriteSafe(console, line, theme.Code);
            return;
        }

        IReadOnlyList<MarkdownHighlightSpan>? spans;
        try
        {
            spans = session.HighlightLine(line);
        }
        catch (Exception ex) when (ex is not OutOfMemoryException)
        {
            session.Dispose();
            session = null;
            WriteSafe(console, line, theme.Code);
            return;
        }

        if (spans is null || spans.Count > line.Length ||
            !AreSafeSpans(spans, line.Length))
        {
            WriteSafe(console, line, theme.Code);
            return;
        }

        var previous = 0;
        foreach (var span in spans)
        {
            if (span.Start > previous)
                WriteSafe(console, line[previous..span.Start], theme.Code);
            WriteSafe(console, line.Substring(span.Start, span.Length), span.Style);
            previous = span.Start + span.Length;
        }
        if (previous < line.Length)
            WriteSafe(console, line[previous..], theme.Code);
    }

    private static bool AreSafeSpans(IReadOnlyList<MarkdownHighlightSpan> spans, int length)
    {
        var previous = 0;
        foreach (var span in spans)
        {
            if (span.Length <= 0 || span.Start < previous ||
                span.Start > length || span.Length > length - span.Start)
                return false;
            previous = span.Start + span.Length;
        }
        return true;
    }

    private static string ListPrefix(MarkdownListMarker? marker, MarkdownSpectreTheme theme) =>
        marker is { IsOrdered: true } ? marker.OriginalMarker :
        theme.PreserveUnorderedMarkers && marker is not null ? marker.OriginalMarker :
        theme.ListBullet;

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
