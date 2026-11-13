using Afrowave.Toolbox.Blackboard.SpectreConsole.Markdown;
using Afrowave.Toolbox.ExchangeHouse.Markdown;
using Spectre.Console;

namespace Afrowave.Toolbox.Blackboard.Tools.MdView;

/// <summary>Visual page rendered from a source-bound start, including its next resume point.</summary>
/// <param name="Start">Source-and-wrap position at the start of the page.</param>
/// <param name="Next">The first source visual row after the rendered page.</param>
/// <param name="Rows">Number of displayed visual content rows.</param>
/// <param name="ContainsClippedCode">True if any code line extends beyond the viewport.</param>
public sealed record MdViewTerminalRenderedPage(
    MdViewVisualPosition Start, MdViewVisualPosition Next,
    int Rows, bool ContainsClippedCode);

/// <summary>
/// Renders a bounded number of visual Markdown lines from the existing
/// disk-backed container cursor, using the same layout as the page index.
/// </summary>
public static class MdViewTerminalPageRenderer
{
    /// <summary>Renders at most the requested visual rows.</summary>
    /// <param name="console">Target Spectre terminal.</param>
    /// <param name="cursor">Validated snapshot's persistent source-aware cursor.</param>
    /// <param name="start">Start physical line and optional soft-wrap row.</param>
    /// <param name="columns">Available text columns.</param>
    /// <param name="contentRows">Visual row budget.</param>
    /// <param name="theme">Existing Markdown presentation theme.</param>
    /// <param name="cancellationToken">Cancels source reading and output.</param>
    /// <returns>Exact next source/row location for a following page.</returns>
    public static async Task<MdViewTerminalRenderedPage> RenderAsync(
        IAnsiConsole console, MdViewContainerPageCursor cursor,
        MdViewVisualPosition start, int columns, int contentRows,
        MarkdownSpectreTheme theme, CancellationToken cancellationToken = default)
    {
        ArgumentNullException.ThrowIfNull(console);
        ArgumentNullException.ThrowIfNull(cursor);
        ArgumentNullException.ThrowIfNull(theme);
        if (start.Line < 0 || start.WrapRow < 0)
            throw new ArgumentOutOfRangeException(nameof(start));
        if (columns is < 8 or > 4096)
            throw new ArgumentOutOfRangeException(nameof(columns));
        if (contentRows is < 1 or > 128)
            throw new ArgumentOutOfRangeException(nameof(contentRows));

        var position = start;
        var emitted = 0;
        var clipped = false;

        while (emitted < contentRows)
        {
            cancellationToken.ThrowIfCancellationRequested();
            var batch = await cursor.ReadAsync(
                position.Line, Math.Min(128, contentRows - emitted),
                cancellationToken).ConfigureAwait(false);
            if (batch.Count == 0)
                break;

            foreach (var classified in batch)
            {
                cancellationToken.ThrowIfCancellationRequested();
                if (classified.Source.LineNumber != position.Line)
                    throw new InvalidDataException("Preview source cursor lost line alignment.");

                var rows = MdViewTerminalLineLayout.Layout(classified, columns);
                if (position.WrapRow >= rows.Count)
                    throw new InvalidDataException(
                        "Visual page position exceeds the wrapped source line.");

                for (var row = position.WrapRow; row < rows.Count; row++)
                {
                    var rendered = rows[row];
                    if (rendered.IsClippedCode)
                        clipped = true;
                    foreach (var segment in rendered.Spans)
                        console.Write(new Text(segment.Text,
                            ResolveStyle(segment.Role, classified, theme)));
                    // Emit one actual terminal row per indexed row. Parsing
                    // each soft fragment again used to duplicate list markers,
                    // lose link styles and disagree with the page index.
                    console.WriteLine();

                    emitted++;
                    if (emitted == contentRows && row + 1 < rows.Count)
                        return new MdViewTerminalRenderedPage(start,
                            new MdViewVisualPosition(position.Line, row + 1),
                            emitted, clipped);
                    if (emitted == contentRows)
                        return new MdViewTerminalRenderedPage(start,
                            new MdViewVisualPosition(position.Line + 1, 0),
                            emitted, clipped);
                }
                position = new MdViewVisualPosition(position.Line + 1, 0);
            }
        }

        return new MdViewTerminalRenderedPage(start, position, emitted, clipped);
    }

    private static Style ResolveStyle(
        MdViewTerminalSpanRole role, MarkdownContainerLine source,
        MarkdownSpectreTheme theme) => role switch
    {
        MdViewTerminalSpanRole.Marker => theme.ListMarker,
        MdViewTerminalSpanRole.Quote => theme.Quote,
        MdViewTerminalSpanRole.Heading =>
            source.Source.HeadingLevel is >= 1 and <= 6
                ? theme.GetHeadingStyle(source.Source.HeadingLevel)
                : theme.Body,
        MdViewTerminalSpanRole.Strong => theme.Strong,
        MdViewTerminalSpanRole.Emphasis => theme.Emphasis,
        MdViewTerminalSpanRole.Link => theme.Link,
        MdViewTerminalSpanRole.Code => theme.Code,
        MdViewTerminalSpanRole.Image => theme.Image,
        _ => theme.Body
    };
}
